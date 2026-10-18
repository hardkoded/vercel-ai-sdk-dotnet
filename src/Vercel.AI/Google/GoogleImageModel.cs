// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>Gemini image generation through <c>generateContent</c>.</summary>
public sealed class GoogleImageModel : IImageModel
{
    private readonly GoogleProvider _provider;

    /// <summary>Creates an image model.</summary>
    public GoogleImageModel(GoogleProvider provider, string modelId, int? maxImagesPerCall = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        MaxImagesPerCall = maxImagesPerCall ?? 1;
    }

    /// <inheritdoc />
    public string Provider
    {
        get
        {
            return _provider.ModelProvider.IndexOf("vertex", StringComparison.OrdinalIgnoreCase) >= 0
                ? "google.vertex.image"
                : _provider.ModelProvider;
        }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Images accepted in one call.</summary>
    public int MaxImagesPerCall { get; }

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        if (ModelId == null || !ModelId.StartsWith("gemini-", StringComparison.Ordinal))
        {
            throw new AiSdkException("Google image models other than Gemini are no longer supported. Use a model ID that starts with `gemini-`.");
        }

        options ??= new ImageCallOptions(string.Empty);
        var warnings = new List<GoogleWarning>();
        if (!string.IsNullOrEmpty(options.Size))
        {
            warnings.Add(GoogleWarning.Unsupported("size", "Gemini image models use aspectRatio instead of size."));
        }

        if (options.Mask != null)
        {
            warnings.Add(GoogleWarning.Unsupported("mask", "Gemini image models do not support masks."));
        }

        var modalities = new JsonArray();
        modalities.Add(JsonValue.Create("TEXT"));
        modalities.Add(JsonValue.Create("IMAGE"));
        var config = new JsonObject { ["responseModalities"] = modalities };
        if (!string.IsNullOrEmpty(options.AspectRatio))
        {
            config["imageConfig"] = new JsonObject { ["aspectRatio"] = options.AspectRatio };
        }

        if (options.Seed is { } seed)
        {
            config["seed"] = seed;
        }

        if (options.ProviderOptions != null
            && options.ProviderOptions.TryGetValue("google", out var google)
            && google.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            foreach (var property in google.EnumerateObject())
            {
                config[property.Name] = JsonNode.Parse(property.Value.GetRawText());
            }
        }

        var parts = new JsonArray { new JsonObject { ["text"] = options.Prompt } };
        if (options.Files != null)
        {
            foreach (var file in options.Files)
            {
                parts.Add(file.Url != null
                    ? new JsonObject { ["fileData"] = new JsonObject { ["mimeType"] = file.MediaType ?? "image/png", ["fileUri"] = file.Url } }
                    : new JsonObject { ["inlineData"] = new JsonObject { ["mimeType"] = file.MediaType, ["data"] = Convert.ToBase64String(file.Data ?? Array.Empty<byte>()) } });
            }
        }

        var body = new JsonObject
        {
            ["contents"] = new JsonArray
            {
                new JsonObject
                {
                    ["role"] = "user",
                    ["parts"] = parts,
                },
            },
            ["generationConfig"] = config,
        };
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        if (options.Headers != null)
        {
            foreach (var pair in options.Headers)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, GoogleModelPath.Get(ModelId) + ":generateContent"),
            GoogleJson.Write(body),
            headers,
            cancellationToken).ConfigureAwait(false);
        var images = new List<GeneratedImage>();
        if (document.RootElement.TryGetProperty("candidates", out var candidates))
        {
            foreach (var candidate in candidates.EnumerateArray())
            {
                if (!candidate.TryGetProperty("content", out var content) || !content.TryGetProperty("parts", out var parts2))
                {
                    continue;
                }

                foreach (var part in parts2.EnumerateArray())
                {
                    if (!part.TryGetProperty("inlineData", out var inline))
                    {
                        continue;
                    }

                    var media = GoogleJson.String(inline, "mimeType") ?? "image/png";
                    var data = GoogleJson.String(inline, "data");
                    images.Add(new GeneratedImage(media, string.IsNullOrEmpty(data) ? null : Convert.FromBase64String(data!), null));
                }
            }
        }

        return new GoogleImageResult(images, warnings);
    }
}

/// <summary>Image result that also carries preparation warnings.</summary>
public sealed class GoogleImageResult : ImageGenerationResult
{
    /// <summary>Creates an image result.</summary>
    public GoogleImageResult(IReadOnlyList<GeneratedImage> images, IReadOnlyList<GoogleWarning> warnings)
        : base(images)
    {
        Warnings = warnings ?? Array.Empty<GoogleWarning>();
    }

    /// <summary>Warnings such as an unsupported size.</summary>
    public IReadOnlyList<GoogleWarning> Warnings { get; }
}
