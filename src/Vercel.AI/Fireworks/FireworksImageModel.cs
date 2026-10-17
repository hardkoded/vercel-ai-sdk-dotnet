// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using ImageModelFile = Vercel.AI.Operations.ImageModelFile;
using Vercel.AI.GenerateText;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Fireworks;

/// <summary>Fireworks image model. Sync models return bytes. Kontext models poll <c>get_result</c>.</summary>
public sealed class FireworksImageModel : IImageModel
{
    private readonly FireworksProvider _provider;

    /// <summary>Creates an image model.</summary>
    public FireworksImageModel(FireworksProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string Provider => "fireworks.image";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Fireworks returns one image per call.</summary>
    public int MaxImagesPerCall => 1;

    /// <summary>Delay between async polls.</summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>How long async polling may run.</summary>
    public TimeSpan PollTimeout { get; set; } = TimeSpan.FromMilliseconds(120000);

    /// <summary>Clock used for the response timestamp. Defaults to UTC now.</summary>
    public Func<DateTimeOffset>? Clock { get; set; }

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var warnings = new List<CallWarning>();
        var supportsSize = SupportsSize(ModelId);
        if (!supportsSize && !string.IsNullOrEmpty(options.Size))
        {
            warnings.Add(new CallWarning("unsupported", "This model does not support the `size` option. Use `aspectRatio` instead."));
        }

        if (supportsSize && !string.IsNullOrEmpty(options.AspectRatio))
        {
            warnings.Add(new CallWarning("unsupported", "This model does not support the `aspectRatio` option."));
        }

        if (options.Files != null && options.Files.Count > 1)
        {
            warnings.Add(new CallWarning("other", "Fireworks only supports a single input image. Additional images are ignored."));
        }

        if (options.Mask != null)
        {
            warnings.Add(new CallWarning("unsupported", "Fireworks Kontext models do not support explicit masks. Use the prompt to describe the areas to edit."));
        }

        var body = new JsonObject
        {
            ["prompt"] = options.Prompt,
            ["samples"] = options.Count,
        };
        if (!string.IsNullOrEmpty(options.AspectRatio))
        {
            body["aspect_ratio"] = options.AspectRatio;
        }

        if (options.Seed != null)
        {
            body["seed"] = options.Seed.Value;
        }

        var size = SplitSize(options.Size);
        if (size != null)
        {
            body["width"] = size.Value.Width;
            body["height"] = size.Value.Height;
        }

        if (options.Files != null && options.Files.Count > 0)
        {
            body["input_image"] = DataUri(options.Files[0]);
        }

        ValidateOptions(options.ProviderOptions);
        OpenAICompatibleImages.MergeOptions(body, Provider, options.ProviderOptions, warnings);
        var timestamp = Clock != null ? Clock() : DateTimeOffset.UtcNow;
        var headers = _provider.CreateHeaders(options.Headers);
        if (IsAsync(ModelId))
        {
            return await GenerateAsync(body, headers, warnings, timestamp, cancellationToken).ConfigureAwait(false);
        }

        var binary = await _provider.PostBinaryAsync(RequestUri(), body.ToJsonString(), headers, cancellationToken).ConfigureAwait(false);
        if (binary.Body.Length == 0)
        {
            throw new ApiException("Response body is empty", binary.StatusCode, string.Empty);
        }

        return new ImageGenerationResult(new[] { new GeneratedImage("image/png", binary.Body, null) }, warnings, new ImageResponseInfo(timestamp, ModelId, binary.Headers));
    }

    private async Task<ImageGenerationResult> GenerateAsync(JsonObject body, Dictionary<string, string?> headers, IReadOnlyList<CallWarning> warnings, DateTimeOffset timestamp, CancellationToken cancellationToken)
    {
        var submitted = await _provider.PostJsonAsync(RequestUri(), body.ToJsonString(), headers, cancellationToken).ConfigureAwait(false);
        using var submit = JsonDocument.Parse(string.IsNullOrWhiteSpace(submitted.Body) ? "{}" : submitted.Body);
        var requestId = submit.RootElement.TryGetProperty("request_id", out var idElement) ? idElement.GetString() : null;
        var pollBody = new JsonObject { ["id"] = requestId ?? string.Empty };
        var started = DateTime.UtcNow;
        while (true)
        {
            if (DateTime.UtcNow - started > PollTimeout)
            {
                throw new AiSdkException("Fireworks image generation timed out after " + PollTimeout.TotalMilliseconds.ToString(CultureInfo.InvariantCulture) + "ms");
            }

            var poll = await _provider.PostJsonAsync(PollUri(), pollBody.ToJsonString(), headers, cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(poll.Body) ? "{}" : poll.Body);
            var status = document.RootElement.TryGetProperty("status", out var statusElement) ? statusElement.GetString() : null;
            if (status == "Ready")
            {
                var sample = document.RootElement.TryGetProperty("result", out var result)
                    && result.ValueKind == JsonValueKind.Object
                    && result.TryGetProperty("sample", out var sampleElement)
                    && sampleElement.ValueKind == JsonValueKind.String
                    ? sampleElement.GetString()
                    : null;
                if (string.IsNullOrEmpty(sample))
                {
                    throw new AiSdkException("Fireworks poll response is Ready but missing result.sample");
                }

                // The sample URL comes from the response body. Only send credentials to the provider's own origin.
                var sameOrigin = ProviderValues.IsSameOrigin(sample!, _provider.Options.BaseUrl);
                if (!sameOrigin)
                {
                    DownloadUrls.ValidateDownloadUrl(sample!);
                }

                var image = await _provider.GetBinaryAsync(new Uri(sample!), sameOrigin ? headers : null, cancellationToken).ConfigureAwait(false);
                return new ImageGenerationResult(new[] { new GeneratedImage("image/png", image.Body, sample) }, warnings, new ImageResponseInfo(timestamp, ModelId, image.Headers));
            }

            if (status == "Error" || status == "Failed")
            {
                throw new AiSdkException("Fireworks image generation failed with status: " + status);
            }

            await Task.Delay(PollInterval, cancellationToken).ConfigureAwait(false);
        }
    }

    private static void ValidateOptions(IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        if (providerOptions == null || !providerOptions.TryGetValue("fireworks", out var options) || options.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var property in options.EnumerateObject())
        {
            var value = property.Value;
            var valid = property.Name switch
            {
                "guidance_scale" or "cfg_scale" => value.ValueKind == JsonValueKind.Number,
                "num_inference_steps" or "steps" => value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out _),
                "output_format" => value.ValueKind == JsonValueKind.String && (value.GetString() == "jpeg" || value.GetString() == "png"),
                "webhook_url" or "webhook_secret" => value.ValueKind == JsonValueKind.String,
                "prompt_upsampling" => value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False,
                "safety_tolerance" => value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var tolerance) && tolerance >= 0 && tolerance <= 6,
                _ => true,
            };
            if (!valid)
            {
                throw new Operations.InvalidArgumentException("providerOptions", options.GetRawText(), "invalid fireworks provider options");
            }
        }
    }

    private Uri RequestUri()
    {
        var root = _provider.Options.BaseUrl.TrimEnd('/');
        if (IsImageGeneration(ModelId))
        {
            return new Uri(root + "/image_generation/" + ModelId);
        }

        if (IsAsync(ModelId))
        {
            return new Uri(root + "/workflows/" + ModelId);
        }

        return new Uri(root + "/workflows/" + ModelId + "/text_to_image");
    }

    private Uri PollUri()
    {
        return new Uri(_provider.Options.BaseUrl.TrimEnd('/') + "/workflows/" + ModelId + "/get_result");
    }

    private static bool IsAsync(string modelId)
    {
        return modelId == "accounts/fireworks/models/flux-kontext-pro"
            || modelId == "accounts/fireworks/models/flux-kontext-max";
    }

    private static bool IsImageGeneration(string modelId)
    {
        return modelId == "accounts/fireworks/models/playground-v2-5-1024px-aesthetic"
            || modelId == "accounts/fireworks/models/japanese-stable-diffusion-xl"
            || modelId == "accounts/fireworks/models/playground-v2-1024px-aesthetic"
            || modelId == "accounts/fireworks/models/stable-diffusion-xl-1024-v1-0"
            || modelId == "accounts/fireworks/models/SSD-1B";
    }

    private static bool SupportsSize(string modelId)
    {
        return IsImageGeneration(modelId);
    }

    private static (string Width, string Height)? SplitSize(string? size)
    {
        if (string.IsNullOrEmpty(size))
        {
            return null;
        }

        var parts = size!.Split('x');
        if (parts.Length != 2)
        {
            return null;
        }

        return (parts[0], parts[1]);
    }

    private static string DataUri(ImageModelFile file)
    {
        if (!string.IsNullOrEmpty(file.Url) && (file.Data == null || file.Data.Length == 0))
        {
            return file.Url!;
        }

        return "data:" + file.MediaType + ";base64," + Convert.ToBase64String(file.Data ?? Array.Empty<byte>());
    }
}
