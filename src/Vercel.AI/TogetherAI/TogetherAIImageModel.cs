// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.TogetherAI;

/// <summary>Together image generation. Posts <c>/images/generations</c> with width, height, and base64 output.</summary>
public sealed class TogetherAIImageModel : IImageModel
{
    private const string NonDiffusionModelId = "google/gemini-3-pro-image";

    // Diffusion-only provider options that non-diffusion models reject.
    private static readonly string[] DiffusionOptions = { "steps", "guidance", "negative_prompt", "disable_safety_checker" };

    private readonly TogetherAIProvider _provider;
    private readonly Func<DateTimeOffset>? _clock;

    /// <summary>Creates an image model. <paramref name="clock"/> sets <see cref="LastResponseTimestamp"/> and defaults to UTC now.</summary>
    public TogetherAIImageModel(TogetherAIProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock;
    }

    /// <inheritdoc />
    public string Provider => "togetherai.image";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Together accepts one image per call.</summary>
    public int MaxImagesPerCall => 1;

    /// <summary>Random seed. Omitted for non-diffusion models.</summary>
    public int? Seed { get; set; }

    /// <summary>Provider-specific fields merged into the body.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }

    /// <summary>Headers for this call.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Reference images. Only the first is sent, as <c>image_url</c>.</summary>
    public IReadOnlyList<OpenAICompatibleImageFile>? Files { get; set; }

    /// <summary>When set, the call fails. Together does not accept masks.</summary>
    public bool Mask { get; set; }

    /// <summary>Warnings from the most recent call.</summary>
    public IReadOnlyList<CallWarning> LastWarnings { get; private set; } = Array.Empty<CallWarning>();

    /// <summary>HTTP response headers from the most recent call.</summary>
    public IReadOnlyDictionary<string, string> LastResponseHeaders { get; private set; } = new Dictionary<string, string>();

    /// <summary>Time the most recent call started.</summary>
    public DateTimeOffset? LastResponseTimestamp { get; private set; }

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        if (Mask)
        {
            throw new AiSdkException(
                "Together AI does not support mask-based image editing. Use FLUX Kontext models (e.g., black-forest-labs/FLUX.1-kontext-pro) with a reference image and descriptive prompt instead.");
        }

        var timestamp = _clock?.Invoke() ?? DateTimeOffset.UtcNow;
        var warnings = new List<CallWarning>();
        if (!string.IsNullOrEmpty(options.AspectRatio))
        {
            warnings.Add(new CallWarning("unsupported", "This model does not support the `aspectRatio` option. Use `size` instead."));
        }

        var nonDiffusion = ModelId == NonDiffusionModelId;
        if (nonDiffusion && Seed != null)
        {
            warnings.Add(new CallWarning("unsupported", "The " + ModelId + " model does not support the `seed` option."));
        }

        if (Files != null && Files.Count > 1)
        {
            warnings.Add(new CallWarning("other", "Together AI only supports a single input image. Additional images are ignored."));
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["prompt"] = options.Prompt,
            ["response_format"] = "base64",
        };
        if (Seed != null && !nonDiffusion)
        {
            body["seed"] = Seed.Value;
        }

        if (options.Count > 1)
        {
            body["n"] = options.Count;
        }

        var size = SplitSize(options.Size);
        if (size != null)
        {
            body["width"] = size.Value.Width;
            body["height"] = size.Value.Height;
        }

        if (Files != null && Files.Count > 0)
        {
            body["image_url"] = ImageUrl(Files[0]);
        }

        OpenAICompatibleImages.MergeOptions(body, Provider, ProviderOptions, warnings);
        if (nonDiffusion)
        {
            foreach (var name in DiffusionOptions)
            {
                body.Remove(name);
            }
        }

        var response = await _provider.PostJsonAsync(_provider.ImagesUri(), body.ToJsonString(), _provider.CreateHeaders(Headers), cancellationToken).ConfigureAwait(false);
        LastWarnings = warnings;
        LastResponseHeaders = response.Headers;
        LastResponseTimestamp = timestamp;
        return ReadImages(response.Body);
    }

    private static string ImageUrl(OpenAICompatibleImageFile file)
    {
        if (!string.IsNullOrEmpty(file.Url) && (file.Data == null || file.Data.Length == 0))
        {
            return file.Url!;
        }

        return "data:" + file.MediaType + ";base64," + Convert.ToBase64String(file.Data ?? Array.Empty<byte>());
    }

    private static (int Width, int Height)? SplitSize(string? size)
    {
        if (string.IsNullOrEmpty(size))
        {
            return null;
        }

        var parts = size!.Split('x');
        if (parts.Length != 2 || !int.TryParse(parts[0], out var width) || !int.TryParse(parts[1], out var height))
        {
            return null;
        }

        return (width, height);
    }

    private static ImageGenerationResult ReadImages(string? json)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json!);
        var images = new List<GeneratedImage>();
        if (document.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in data.EnumerateArray())
            {
                byte[]? bytes = null;
                if (item.TryGetProperty("b64_json", out var b64) && b64.ValueKind == JsonValueKind.String)
                {
                    bytes = Convert.FromBase64String(b64.GetString() ?? string.Empty);
                }

                images.Add(new GeneratedImage("image/png", bytes, null));
            }
        }

        return new ImageGenerationResult(images);
    }
}
