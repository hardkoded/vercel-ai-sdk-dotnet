// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using ProviderImage = Vercel.AI.Provider.GeneratedImage;

namespace Vercel.AI.Replicate;

/// <summary>
/// Replicate image model. It creates a prediction, polls it when the sync wait expires, and downloads each output image.
/// A <c>model:version</c> id posts to <c>/predictions</c>; other ids post to <c>/models/{model}/predictions</c>.
/// </summary>
public sealed class ReplicateImageModel : IImageModel, IImageCaller
{
    private const int DefaultPollIntervalMillis = 500;
    private const int DefaultMaxPollAttempts = 240;
    private const int MaxFlux2InputImages = 8;
    private static readonly Regex Flux2Model = new Regex("^black-forest-labs/flux-2-", RegexOptions.CultureInvariant);
    private static readonly string[] CallSettings = { "maxWaitTimeInSeconds", "pollIntervalMillis", "maxPollAttempts" };

    private readonly ReplicateProvider _provider;
    private readonly Func<DateTime>? _clock;

    /// <summary>Creates an image model. <paramref name="clock"/> sets the response timestamp.</summary>
    public ReplicateImageModel(ReplicateProvider provider, string modelId, Func<DateTime>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock;
    }

    /// <inheritdoc cref="IImageModel.Provider" />
    public string Provider => "replicate.image";

    /// <inheritdoc cref="IImageModel.ModelId" />
    public string ModelId { get; }

    /// <summary>Flux-2 models accept up to 8 input images; other models return one image per call.</summary>
    public int? MaxImagesPerCall => IsFlux2 ? MaxFlux2InputImages : 1;

    private bool IsFlux2 => Flux2Model.IsMatch(ModelId);

    /// <inheritdoc />
    public Task<int?> ResolveMaxImagesPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxImagesPerCall);
    }

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var call = new ImageModelCall(options.Prompt, null, null, options.Count, options.Size, options.AspectRatio, null, null, new Dictionary<string, string>(), cancellationToken);
        var result = await DoGenerateAsync(call, cancellationToken).ConfigureAwait(false);
        var images = new List<ProviderImage>();
        foreach (var image in result.Images)
        {
            var bytes = (byte[])image!;
            images.Add(new ProviderImage(MediaTypes.DetectMediaType(bytes, "image") ?? "image/png", bytes, null));
        }

        return new ImageGenerationResult(images);
    }

    /// <inheritdoc />
    public async Task<ImageModelResult> DoGenerateAsync(ImageModelCall call, CancellationToken cancellationToken)
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var warnings = new List<OperationWarning>();
        var separator = ModelId.IndexOf(':');
        var model = separator < 0 ? ModelId : ModelId.Substring(0, separator);
        var version = separator < 0 ? null : ModelId.Substring(separator + 1);
        var timestamp = _clock?.Invoke() ?? DateTime.UtcNow;
        var replicate = ReplicateOptions(call.ProviderOptions);
        var input = new JsonObject();
        if (call.Prompt != null)
        {
            input["prompt"] = call.Prompt;
        }

        if (call.AspectRatio != null)
        {
            input["aspect_ratio"] = call.AspectRatio;
        }

        if (call.Size != null)
        {
            input["size"] = call.Size;
        }

        if (call.Seed is { } seed)
        {
            input["seed"] = seed;
        }

        input["num_outputs"] = call.N;
        AddImageInputs(input, call.Files, call.Mask, warnings);
        if (replicate is { } inputOptions)
        {
            foreach (var property in inputOptions.EnumerateObject())
            {
                if (Array.IndexOf(CallSettings, property.Name) < 0)
                {
                    input[property.Name] = JsonNode.Parse(property.Value.GetRawText());
                }
            }
        }

        var body = new JsonObject { ["input"] = input };
        if (version != null)
        {
            body["version"] = version;
        }

        var maxWait = ReadSetting(replicate, "maxWaitTimeInSeconds");
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        headers["Prefer"] = maxWait == null ? "wait" : "wait=" + maxWait.Value.GetRawText();
        var path = version != null ? "/predictions" : "/models/" + model + "/predictions";
        var created = await _provider.SendAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, path), ProviderExchange.Json(body.ToJsonString()), headers, cancellationToken).ConfigureAwait(false);
        var prediction = await PollAsync(
            ParsePrediction(created.Body),
            call.Headers,
            ReadSetting(replicate, "pollIntervalMillis")?.GetInt32() ?? DefaultPollIntervalMillis,
            ReadSetting(replicate, "maxPollAttempts")?.GetInt32() ?? DefaultMaxPollAttempts,
            cancellationToken).ConfigureAwait(false);
        if (!prediction.TryGetProperty("output", out var output) || output.ValueKind == JsonValueKind.Null)
        {
            throw new InvalidResponseDataException(prediction, "Replicate image generation completed without output.");
        }

        var urls = output.ValueKind == JsonValueKind.Array ? output.EnumerateArray().Select(url => url.GetString()!).ToList() : new List<string> { output.GetString()! };
        var images = new List<object?>();
        foreach (var url in urls)
        {
            var image = await _provider.GetResponseUrlAsync(url, null, cancellationToken).ConfigureAwait(false);
            images.Add(image.Bytes);
        }

        return new ImageModelResult(images, warnings, response: new ProviderResponse(created.Headers, timestamp: timestamp, modelId: ModelId));
    }

    private void AddImageInputs(JsonObject input, IReadOnlyList<ImageModelFile>? files, ImageModelFile? mask, List<OperationWarning> warnings)
    {
        if (files is { Count: > 0 })
        {
            if (IsFlux2)
            {
                for (var i = 0; i < Math.Min(files.Count, MaxFlux2InputImages); i++)
                {
                    input[i == 0 ? "input_image" : "input_image_" + (i + 1)] = FileDataConversions.ConvertImageModelFileToDataUri(files[i]);
                }

                if (files.Count > MaxFlux2InputImages)
                {
                    warnings.Add(OperationWarning.Other("Flux-2 models support up to " + MaxFlux2InputImages + " input images. Additional images are ignored."));
                }
            }
            else
            {
                input["image"] = FileDataConversions.ConvertImageModelFileToDataUri(files[0]);
                if (files.Count > 1)
                {
                    warnings.Add(OperationWarning.Other("This Replicate model only supports a single input image. Additional images are ignored."));
                }
            }
        }

        if (mask != null)
        {
            if (IsFlux2)
            {
                warnings.Add(OperationWarning.Other("Flux-2 models do not support mask input. The mask will be ignored."));
            }
            else
            {
                input["mask"] = FileDataConversions.ConvertImageModelFileToDataUri(mask);
            }
        }
    }

    // Polls the prediction's get URL until it has output, succeeds, or fails.
    private async Task<JsonElement> PollAsync(JsonElement prediction, IReadOnlyDictionary<string, string> headers, int pollIntervalMillis, int maxPollAttempts, CancellationToken cancellationToken)
    {
        var current = prediction;
        for (var attempt = 0; attempt < maxPollAttempts; attempt++)
        {
            if (IsComplete(current))
            {
                return current;
            }

            var url = current.GetProperty("urls").GetProperty("get").GetString()!;
            var response = await _provider.GetResponseUrlAsync(url, headers, cancellationToken).ConfigureAwait(false);
            current = ParsePrediction(response.Body);
            if (attempt < maxPollAttempts - 1)
            {
                await Task.Delay(pollIntervalMillis, cancellationToken).ConfigureAwait(false);
            }
        }

        if (IsComplete(current))
        {
            return current;
        }

        throw new AiSdkException("Replicate image generation did not complete after " + maxPollAttempts + " polling attempts.");
    }

    private static bool IsComplete(JsonElement prediction)
    {
        var status = prediction.GetProperty("status").GetString();
        if (status == "failed" || status == "canceled")
        {
            var error = prediction.TryGetProperty("error", out var message) && message.ValueKind == JsonValueKind.String ? message.GetString() : null;
            throw new InvalidResponseDataException(prediction, "Replicate image generation " + status + ": " + (error ?? "Unknown error"));
        }

        return (prediction.TryGetProperty("output", out var output) && output.ValueKind != JsonValueKind.Null) || status == "succeeded";
    }

    private static JsonElement ParsePrediction(string body)
    {
        using var document = JsonDocument.Parse(body);
        return document.RootElement.Clone();
    }

    private static JsonElement? ReplicateOptions(JsonElement? providerOptions)
    {
        return providerOptions is { ValueKind: JsonValueKind.Object } options && options.TryGetProperty(ReplicateProvider.ProviderId, out var replicate) && replicate.ValueKind == JsonValueKind.Object
            ? replicate
            : null;
    }

    private static JsonElement? ReadSetting(JsonElement? replicate, string name)
    {
        return replicate is { } options && options.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number ? value : null;
    }
}
