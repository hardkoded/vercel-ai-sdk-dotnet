// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
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
        if (options.Mask != null)
        {
            throw new AiSdkException("Gemini image models do not support mask-based image editing.");
        }

        var warnings = new List<GoogleWarning>();
        if (!string.IsNullOrEmpty(options.Size))
        {
            warnings.Add(GoogleWarning.Unsupported("size", "Gemini image models use aspectRatio instead of size."));
        }

        var modalities = new JsonArray();
        modalities.Add(JsonValue.Create("IMAGE"));
        var config = new JsonObject { ["responseModalities"] = modalities };
        if (options.Seed != null)
        {
            config["seed"] = options.Seed;
        }

        var google = options.ProviderOptions != null && options.ProviderOptions.TryGetValue("google", out var googleOptions) && googleOptions.ValueKind == JsonValueKind.Object
            ? googleOptions
            : (JsonElement?)null;
        JsonObject? imageConfig = null;
        if (google is { } googleElement && googleElement.TryGetProperty("imageConfig", out var userImageConfig) && userImageConfig.ValueKind == JsonValueKind.Object)
        {
            imageConfig = JsonNode.Parse(userImageConfig.GetRawText())!.AsObject();
        }

        if (!string.IsNullOrEmpty(options.AspectRatio))
        {
            imageConfig ??= new JsonObject();
            imageConfig["aspectRatio"] = options.AspectRatio;
        }

        if (imageConfig != null)
        {
            config["imageConfig"] = imageConfig;
        }

        var userParts = new JsonArray { new JsonObject { ["text"] = options.Prompt } };
        if (options.Files != null)
        {
            foreach (var file in options.Files)
            {
                if (file.Type == "url")
                {
                    throw new AiSdkException("File of media type \"image/*\" must specify subtype since it is not passed as inline bytes.");
                }

                userParts.Add(new JsonObject
                {
                    ["inlineData"] = new JsonObject
                    {
                        ["mimeType"] = file.MediaType,
                        ["data"] = Convert.ToBase64String(file.Data ?? Array.Empty<byte>()),
                    },
                });
            }
        }

        var body = new JsonObject
        {
            ["contents"] = new JsonArray
            {
                new JsonObject
                {
                    ["role"] = "user",
                    ["parts"] = userParts,
                },
            },
            ["generationConfig"] = config,
        };
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, GoogleModelPath.Get(ModelId) + ":generateContent"),
            GoogleJson.Write(body),
            await HeadersAsync(options, cancellationToken).ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);
        var images = new List<GeneratedImage>();
        if (document.RootElement.TryGetProperty("candidates", out var candidates))
        {
            foreach (var candidate in candidates.EnumerateArray())
            {
                if (!candidate.TryGetProperty("content", out var content) || !content.TryGetProperty("parts", out var parts))
                {
                    continue;
                }

                foreach (var part in parts.EnumerateArray())
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

    private async Task<Dictionary<string, string?>> HeadersAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        if (options.Headers != null)
        {
            foreach (var pair in options.Headers)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
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
