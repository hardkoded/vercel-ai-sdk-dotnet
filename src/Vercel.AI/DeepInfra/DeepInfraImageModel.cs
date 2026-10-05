// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.DeepInfra;

/// <summary>
/// DeepInfra image model. Generation posts to <c>/inference/{model}</c>.
/// Edits post multipart to the OpenAI-compatible <c>/openai/images/edits</c> route.
/// </summary>
public sealed class DeepInfraImageModel : IImageModel
{
    private readonly DeepInfraProvider _provider;
    private readonly Func<DateTimeOffset>? _clock;

    /// <summary>Creates an image model. <paramref name="clock"/> sets <see cref="LastResponseTimestamp"/> and defaults to UTC now.</summary>
    public DeepInfraImageModel(DeepInfraProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock;
    }

    /// <inheritdoc />
    public string Provider => "deepinfra.image";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>DeepInfra accepts one image per call.</summary>
    public int MaxImagesPerCall => 1;

    /// <summary>Random seed sent as <c>seed</c>.</summary>
    public int? Seed { get; set; }

    /// <summary>Provider-specific fields merged into the body.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }

    /// <summary>Headers for this call.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Input images. When set, the call edits them instead of generating.</summary>
    public IReadOnlyList<OpenAICompatibleImageFile>? Files { get; set; }

    /// <summary>Optional edit mask.</summary>
    public OpenAICompatibleImageFile? Mask { get; set; }

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
        var timestamp = _clock?.Invoke() ?? DateTimeOffset.UtcNow;
        var warnings = new List<CallWarning>();
        if (Files != null && Files.Count > 0)
        {
            var edited = await _provider.PostMultipartAsync(EditUri(), BuildEditForm(options, warnings), _provider.CreateHeaders(Headers), cancellationToken).ConfigureAwait(false);
            LastWarnings = warnings;
            LastResponseHeaders = edited.Headers;
            LastResponseTimestamp = timestamp;
            return ReadEditedImages(edited.Body);
        }

        var body = new JsonObject
        {
            ["prompt"] = options.Prompt,
            ["num_images"] = options.Count,
        };
        if (!string.IsNullOrEmpty(options.AspectRatio))
        {
            body["aspect_ratio"] = options.AspectRatio;
        }

        var size = SplitSize(options.Size);
        if (size != null)
        {
            body["width"] = size.Value.Width;
            body["height"] = size.Value.Height;
        }

        if (Seed != null)
        {
            body["seed"] = Seed.Value;
        }

        OpenAICompatibleImages.MergeOptions(body, Provider, ProviderOptions, warnings);
        var origin = string.IsNullOrEmpty(_provider.Options.ImageBaseUrl)
            ? "https://api.deepinfra.com/v1/inference"
            : _provider.Options.ImageBaseUrl!.TrimEnd('/');
        var response = await _provider.PostJsonAsync(new Uri(origin + "/" + ModelId), body.ToJsonString(), _provider.CreateHeaders(Headers), cancellationToken).ConfigureAwait(false);
        LastWarnings = warnings;
        LastResponseHeaders = response.Headers;
        LastResponseTimestamp = timestamp;
        return ReadImages(response.Body);
    }

    /// <summary>Reads <c>detail.error</c>, then the shared OpenAI-compatible error message.</summary>
    public static string? ReadError(string? body)
    {
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                var root = JsonNode.Parse(body!) as JsonObject;
                if (root?["detail"] is JsonObject detail && detail["error"] is JsonValue value && value.TryGetValue<string>(out var text) && !string.IsNullOrEmpty(text))
                {
                    return text;
                }
            }
            catch (JsonException)
            {
            }
        }

        return OpenAICompatibleChat.ReadErrorMessage(body);
    }

    private Uri EditUri()
    {
        var origin = string.IsNullOrEmpty(_provider.Options.ImageBaseUrl)
            ? "https://api.deepinfra.com/v1/inference"
            : _provider.Options.ImageBaseUrl!.TrimEnd('/');
        return new Uri(origin.Replace("/inference", "/openai") + "/images/edits");
    }

    private MultipartFormDataContent BuildEditForm(ImageCallOptions options, List<CallWarning> warnings)
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(ModelId), "model");
        form.Add(new StringContent(options.Prompt), "prompt");
        var index = 0;
        foreach (var file in Files!)
        {
            form.Add(OpenAICompatibleImages.FileContent(file), "image", file.FileName ?? ("image-" + index.ToString(CultureInfo.InvariantCulture) + ".png"));
            index++;
        }

        if (Mask != null)
        {
            form.Add(OpenAICompatibleImages.FileContent(Mask), "mask", Mask.FileName ?? "mask.png");
        }

        form.Add(new StringContent(options.Count.ToString(CultureInfo.InvariantCulture)), "n");
        if (!string.IsNullOrEmpty(options.Size))
        {
            form.Add(new StringContent(options.Size!), "size");
        }

        var extra = new JsonObject();
        OpenAICompatibleImages.MergeOptions(extra, Provider, ProviderOptions, warnings);
        foreach (var pair in extra)
        {
            if (pair.Value != null)
            {
                form.Add(new StringContent(pair.Value is JsonValue value && value.TryGetValue<string>(out var text) ? text : pair.Value.ToJsonString()), pair.Key);
            }
        }

        return form;
    }

    private static ImageGenerationResult ReadEditedImages(string? json)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json!);
        var images = new List<GeneratedImage>();
        if (document.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in data.EnumerateArray())
            {
                if (item.TryGetProperty("b64_json", out var b64) && b64.ValueKind == JsonValueKind.String)
                {
                    images.Add(new GeneratedImage("image/png", Convert.FromBase64String(b64.GetString() ?? string.Empty), null));
                }
            }
        }

        return new ImageGenerationResult(images);
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

    private static ImageGenerationResult ReadImages(string? json)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json!);
        var images = new List<GeneratedImage>();
        if (!document.RootElement.TryGetProperty("images", out var items) || items.ValueKind != JsonValueKind.Array)
        {
            return new ImageGenerationResult(images);
        }

        foreach (var item in items.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var value = item.GetString() ?? string.Empty;
            var comma = value.IndexOf(",", StringComparison.Ordinal);
            if (value.StartsWith("data:", StringComparison.Ordinal) && comma >= 0)
            {
                value = value.Substring(comma + 1);
            }

            images.Add(new GeneratedImage("image/png", Convert.FromBase64String(value), null));
        }

        return new ImageGenerationResult(images);
    }
}
