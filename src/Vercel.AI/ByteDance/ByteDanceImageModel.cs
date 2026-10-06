// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using ProviderGeneratedImage = Vercel.AI.Provider.GeneratedImage;

namespace Vercel.AI.ByteDance;

/// <summary>ByteDance Seedream image model. Input files turn a generation into an edit.</summary>
public sealed class ByteDanceImageModel : IImageModel, IImageCaller
{
    private static readonly HashSet<string> HandledOptions = new(StringComparer.Ordinal)
    {
        "watermark", "outputFormat", "size", "sequentialImageGeneration", "maxImages", "optimizePromptMode",
    };

    private readonly ByteDanceProvider _provider;
    private readonly Func<DateTimeOffset> _clock;

    /// <summary>Creates a model. <paramref name="clock"/> sets the response timestamp.</summary>
    public ByteDanceImageModel(ByteDanceProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public string Provider => "bytedance.image";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>The API has no output count, so each call returns one image.</summary>
    public int? MaxImagesPerCall => 1;

    /// <inheritdoc />
    public Task<int?> ResolveMaxImagesPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxImagesPerCall);
    }

    /// <inheritdoc />
    public Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        return GenerateAsync(options, null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ImageModelResult> DoGenerateAsync(ImageModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var warnings = new List<OperationWarning>();
        if (call.AspectRatio != null)
        {
            warnings.Add(OperationWarning.Unsupported("aspectRatio", "ByteDance does not support aspectRatio. Use `size` (e.g. \"2048x2048\" or a resolution level like \"2K\" via providerOptions) instead."));
        }

        if (call.Seed != null)
        {
            warnings.Add(OperationWarning.Unsupported("seed"));
        }

        if (call.Mask != null)
        {
            warnings.Add(OperationWarning.Unsupported("mask", "ByteDance Seedream does not support a separate mask. Provide edit instructions in the prompt, optionally with markings on the input image."));
        }

        var timestamp = _clock();
        var body = new JsonObject { ["model"] = ModelId, ["prompt"] = call.Prompt };
        if (call.Files is { Count: > 0 } files)
        {
            body["image"] = files.Count == 1
                ? FileDataConversions.ConvertImageModelFileToDataUri(files[0])
                : new JsonArray(files.Select(file => (JsonNode?)FileDataConversions.ConvertImageModelFileToDataUri(file)).ToArray());
        }

        if (call.Size != null)
        {
            body["size"] = call.Size;
        }

        if (call.ProviderOptions is { ValueKind: JsonValueKind.Object } bag && bag.TryGetProperty(ByteDanceProvider.ProviderId, out var options) && options.ValueKind == JsonValueKind.Object)
        {
            Copy(options, "watermark", body, "watermark");
            Copy(options, "outputFormat", body, "output_format");

            // A resolution level such as "2K" overrides the top-level pixel size.
            Copy(options, "size", body, "size");
            Copy(options, "sequentialImageGeneration", body, "sequential_image_generation");
            if (Value(options, "maxImages") is { } maxImages)
            {
                body["sequential_image_generation_options"] = new JsonObject { ["max_images"] = maxImages };
            }

            if (Value(options, "optimizePromptMode") is { } mode)
            {
                body["optimize_prompt_options"] = new JsonObject { ["mode"] = mode };
            }

            foreach (var option in options.EnumerateObject().Where(option => !HandledOptions.Contains(option.Name)))
            {
                body[option.Name] = JsonNode.Parse(option.Value.GetRawText());
            }
        }

        // Always ask for base64 so the SDK receives the image bytes.
        body["response_format"] = "b64_json";
        var response = await ProviderExchange.SendAsync(
            _provider._httpClient,
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "/images/generations"),
            ProviderExchange.Json(body.ToJsonString()),
            ProviderExchange.Merge(_provider.CreateHeaders(), call.Headers),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        var images = root.GetProperty("data").EnumerateArray().Select(item => (object?)item.GetProperty("b64_json").GetString()).ToList();
        OperationUsage? usage = null;
        if (root.TryGetProperty("usage", out var usageElement) && usageElement.ValueKind == JsonValueKind.Object)
        {
            // Ark reports no input tokens, and generated_images counts images, not tokens.
            usage = new OperationUsage(null, Count(usageElement, "output_tokens"), Count(usageElement, "total_tokens"));
        }

        return new ImageModelResult(images, warnings, usage, response: new ProviderResponse(response.Headers, timestamp: timestamp.UtcDateTime, modelId: ModelId));
    }

    internal async Task<ImageGenerationResult> GenerateAsync(ImageCallOptions options, IDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var call = new ImageModelCall(options.Prompt, null, null, options.Count, options.Size, options.AspectRatio, null, null, new Dictionary<string, string>(headers ?? new Dictionary<string, string>()), cancellationToken);
        var result = await DoGenerateAsync(call, cancellationToken).ConfigureAwait(false);
        return new ImageGenerationResult(result.Images.Select(image =>
        {
            var data = Convert.FromBase64String((string)image!);
            return new ProviderGeneratedImage(MediaTypes.DetectMediaType(data, "image") ?? "image/jpeg", data, null);
        }).ToList());
    }

    private static void Copy(JsonElement options, string name, JsonObject body, string field)
    {
        if (Value(options, name) is { } value)
        {
            body[field] = value;
        }
    }

    private static JsonNode? Value(JsonElement options, string name)
    {
        return options.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null ? JsonNode.Parse(value.GetRawText()) : null;
    }

    private static int? Count(JsonElement usage, string name)
    {
        return usage.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number ? value.GetInt32() : null;
    }
}
