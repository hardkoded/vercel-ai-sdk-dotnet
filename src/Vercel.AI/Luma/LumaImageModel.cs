// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.GenerateText;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Luma;

/// <summary>Luma image generation settings beyond the shared prompt and size.</summary>
public sealed class LumaImageRequest
{
    /// <summary>Creates a request for <paramref name="prompt"/>.</summary>
    public LumaImageRequest(string prompt)
    {
        Prompt = prompt ?? throw new ArgumentNullException(nameof(prompt));
    }

    /// <summary>Image prompt.</summary>
    public string Prompt { get; }

    /// <summary>Aspect ratio such as <c>16:9</c>, sent as <c>aspect_ratio</c>.</summary>
    public string? AspectRatio { get; set; }

    /// <summary>Size. Luma does not accept it; a warning is returned.</summary>
    public string? Size { get; set; }

    /// <summary>Seed. Luma does not accept it; a warning is returned.</summary>
    public int? Seed { get; set; }

    /// <summary>Reference images. Each one must be a URL.</summary>
    public IReadOnlyList<OpenAICompatibleImageFile>? Files { get; set; }

    /// <summary>Edit mask. Luma does not accept one; setting it throws.</summary>
    public OpenAICompatibleImageFile? Mask { get; set; }

    /// <summary>
    /// Provider options. The <c>luma</c> object is read: <c>referenceType</c>, <c>images</c>,
    /// <c>pollIntervalMillis</c>, and <c>maxPollAttempts</c> configure the call, and other fields are sent in the body.
    /// </summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Headers merged over the provider headers.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>Luma image result with warnings and response metadata.</summary>
public sealed class LumaImageGeneration : ImageGenerationResult
{
    internal LumaImageGeneration(IReadOnlyList<GeneratedImage> images, IReadOnlyList<ModelWarning> warnings, DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string> headers)
        : base(images)
    {
        Warnings = warnings;
        Timestamp = timestamp;
        ModelId = modelId;
        Headers = headers;
    }

    /// <summary>Warnings for settings Luma does not support.</summary>
    public IReadOnlyList<ModelWarning> Warnings { get; }

    /// <summary>Time the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Headers of the generation response.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }
}

/// <summary>Luma Dream Machine image model. It submits a generation, polls it, and downloads the image.</summary>
public sealed class LumaImageModel : IImageModel
{
    private const int DefaultPollIntervalMillis = 500;
    private const int DefaultMaxPollAttempts = 60000 / DefaultPollIntervalMillis;

    private readonly LumaProvider _provider;
    private readonly Func<DateTimeOffset> _clock;

    /// <summary>Creates an image model. <paramref name="clock"/> sets the response timestamp.</summary>
    public LumaImageModel(LumaProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public string Provider => "luma.image";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Luma returns one image per call.</summary>
    public int MaxImagesPerCall => 1;

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        return await GenerateAsync(new LumaImageRequest(options.Prompt) { Size = options.Size, AspectRatio = options.AspectRatio }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Generates one image.</summary>
    public async Task<LumaImageGeneration> GenerateAsync(LumaImageRequest request, CancellationToken cancellationToken)
    {
        request = request ?? throw new ArgumentNullException(nameof(request));
        var warnings = new List<ModelWarning>();
        if (request.Seed != null)
        {
            warnings.Add(new UnsupportedWarning("seed", "This model does not support the `seed` option."));
        }

        if (request.Size != null)
        {
            warnings.Add(new UnsupportedWarning("size", "This model does not support the `size` option. Use `aspectRatio` instead."));
        }

        var luma = request.ProviderOptions is { ValueKind: JsonValueKind.Object } bag && bag.TryGetProperty("luma", out var value) && value.ValueKind == JsonValueKind.Object
            ? value
            : (JsonElement?)null;
        var body = new JsonObject { ["prompt"] = request.Prompt };
        if (!string.IsNullOrEmpty(request.AspectRatio))
        {
            body["aspect_ratio"] = request.AspectRatio;
        }

        body["model"] = ModelId;
        AddReferences(body, request.Files, request.Mask, luma);
        int? pollIntervalMillis = null;
        int? maxPollAttempts = null;
        if (luma != null)
        {
            foreach (var property in luma.Value.EnumerateObject())
            {
                switch (property.Name)
                {
                    case "pollIntervalMillis":
                        pollIntervalMillis = ReadInt(property.Value, property.Name);
                        break;
                    case "maxPollAttempts":
                        maxPollAttempts = ReadInt(property.Value, property.Name);
                        break;
                    case "referenceType":
                    case "images":
                        break;
                    default:
                        body[property.Name] = JsonNode.Parse(property.Value.GetRawText());
                        break;
                }
            }
        }

        var timestamp = _clock();
        var headers = ProviderExchange.Merge(_provider.CreateHeaders(), request.Headers);
        var submitted = await ProviderExchange.SendAsync(_provider.HttpClient, HttpMethod.Post, GenerationsUri("image"), ProviderExchange.Json(body.ToJsonString()), headers, cancellationToken).ConfigureAwait(false);
        string generationId;
        using (var document = JsonDocument.Parse(submitted.Body))
        {
            generationId = document.RootElement.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String
                ? id.GetString()!
                : throw new Operations.InvalidResponseDataException(submitted.Body, "Luma generation response has no id.");
        }

        var imageUrl = await PollForImageUrlAsync(generationId, headers, pollIntervalMillis ?? DefaultPollIntervalMillis, maxPollAttempts ?? DefaultMaxPollAttempts, cancellationToken).ConfigureAwait(false);
        if (!ProviderValues.IsSameOrigin(imageUrl, _provider.Options.BaseUrl))
        {
            DownloadUrls.ValidateDownloadUrl(imageUrl);
        }

        var image = await ProviderExchange.SendAsync(_provider.HttpClient, HttpMethod.Get, new Uri(imageUrl), null, null, cancellationToken).ConfigureAwait(false);
        var mediaType = MediaTypes.DetectMediaType(image.Bytes, "image") ?? "image/png";
        return new LumaImageGeneration(new[] { new GeneratedImage(mediaType, image.Bytes, null) }, warnings, timestamp, ModelId, submitted.Headers);
    }

    private async Task<string> PollForImageUrlAsync(string generationId, IReadOnlyDictionary<string, string?> headers, int pollIntervalMillis, int maxPollAttempts, CancellationToken cancellationToken)
    {
        var uri = GenerationsUri(generationId);
        for (var attempt = 0; attempt < maxPollAttempts; attempt++)
        {
            var status = await ProviderExchange.SendAsync(_provider.HttpClient, HttpMethod.Get, uri, null, headers, cancellationToken).ConfigureAwait(false);
            using (var document = JsonDocument.Parse(status.Body))
            {
                var root = document.RootElement;
                var state = root.TryGetProperty("state", out var stateElement) && stateElement.ValueKind == JsonValueKind.String ? stateElement.GetString() : null;
                switch (state)
                {
                    case "completed":
                        if (root.TryGetProperty("assets", out var assets)
                            && assets.ValueKind == JsonValueKind.Object
                            && assets.TryGetProperty("image", out var image)
                            && image.ValueKind == JsonValueKind.String
                            && !string.IsNullOrEmpty(image.GetString()))
                        {
                            return image.GetString()!;
                        }

                        throw new Operations.InvalidResponseDataException(status.Body, "Image generation completed but no image was found.");
                    case "failed":
                        throw new Operations.InvalidResponseDataException(status.Body, "Image generation failed.");
                    case "queued":
                    case "dreaming":
                        break;
                    default:
                        throw new Operations.InvalidResponseDataException(status.Body, "Unknown Luma generation state: " + (state ?? "null") + ".");
                }
            }

            await Task.Delay(pollIntervalMillis, cancellationToken).ConfigureAwait(false);
        }

        throw new AiSdkException("Image generation timed out after " + maxPollAttempts + " attempts.");
    }

    private static void AddReferences(JsonObject body, IReadOnlyList<OpenAICompatibleImageFile>? files, OpenAICompatibleImageFile? mask, JsonElement? luma)
    {
        if (mask != null)
        {
            throw new AiSdkException(
                "Luma AI does not support mask-based image editing. "
                + "Use the prompt to describe the changes you want to make, along with "
                + "`prompt.images` containing the source image URL.");
        }

        if (files == null || files.Count == 0)
        {
            return;
        }

        var urls = new List<string>();
        foreach (var file in files)
        {
            if (string.IsNullOrEmpty(file.Url))
            {
                throw new AiSdkException(
                    "Luma AI only supports URL-based images. "
                    + "Please provide image URLs using `prompt.images` with publicly accessible URLs. "
                    + "Base64 and Uint8Array data are not supported.");
            }

            urls.Add(file.Url!);
        }

        var referenceType = "image";
        var configs = new List<JsonElement>();
        if (luma != null)
        {
            if (luma.Value.TryGetProperty("referenceType", out var type) && type.ValueKind != JsonValueKind.Null)
            {
                referenceType = type.ValueKind == JsonValueKind.String ? type.GetString()! : string.Empty;
            }

            if (luma.Value.TryGetProperty("images", out var images) && images.ValueKind == JsonValueKind.Array)
            {
                configs.AddRange(images.EnumerateArray());
            }
        }

        switch (referenceType)
        {
            case "image":
                if (urls.Count > 4)
                {
                    throw new AiSdkException("Luma AI image supports up to 4 reference images. You provided " + urls.Count + " images.");
                }

                body["image"] = WeightedUrls(urls, configs, 0.85);
                break;
            case "style":
                body["style"] = WeightedUrls(urls, configs, 0.8);
                break;
            case "character":
                var identities = new JsonObject();
                for (var i = 0; i < urls.Count; i++)
                {
                    var identity = ConfigString(configs, i, "id") ?? "identity0";
                    if (identities[identity] is not JsonObject group)
                    {
                        group = new JsonObject { ["images"] = new JsonArray() };
                        identities[identity] = group;
                    }

                    ((JsonArray)group["images"]!).Add(urls[i]);
                }

                foreach (var pair in identities)
                {
                    var count = ((JsonArray)pair.Value!["images"]!).Count;
                    if (count > 4)
                    {
                        throw new AiSdkException("Luma AI character supports up to 4 images per identity. Identity '" + pair.Key + "' has " + count + " images.");
                    }
                }

                body["character"] = identities;
                break;
            case "modify_image":
                if (urls.Count > 1)
                {
                    throw new AiSdkException("Luma AI modify_image only supports a single input image. You provided " + urls.Count + " images.");
                }

                body["modify_image"] = new JsonObject { ["url"] = urls[0], ["weight"] = ConfigWeight(configs, 0) ?? 1.0 };
                break;
            default:
                throw new Operations.InvalidArgumentException("providerOptions.luma.referenceType", referenceType, "must be image, style, character, or modify_image.");
        }
    }

    private static JsonArray WeightedUrls(List<string> urls, List<JsonElement> configs, double defaultWeight)
    {
        var array = new JsonArray();
        for (var i = 0; i < urls.Count; i++)
        {
            array.Add(new JsonObject { ["url"] = urls[i], ["weight"] = ConfigWeight(configs, i) ?? defaultWeight });
        }

        return array;
    }

    private static double? ConfigWeight(List<JsonElement> configs, int index)
    {
        if (index >= configs.Count || configs[index].ValueKind != JsonValueKind.Object || !configs[index].TryGetProperty("weight", out var weight) || weight.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (weight.ValueKind != JsonValueKind.Number || weight.GetDouble() < 0 || weight.GetDouble() > 1)
        {
            throw new Operations.InvalidArgumentException("providerOptions.luma.images", weight.GetRawText(), "weight must be a number from 0 to 1.");
        }

        return weight.GetDouble();
    }

    private static string? ConfigString(List<JsonElement> configs, int index, string name)
    {
        return index < configs.Count && configs[index].ValueKind == JsonValueKind.Object && configs[index].TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static int? ReadInt(JsonElement value, string name)
    {
        if (value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var number))
        {
            throw new Operations.InvalidArgumentException("providerOptions.luma." + name, value.GetRawText(), "must be an integer.");
        }

        return number;
    }

    private Uri GenerationsUri(string path)
    {
        return ApiKeys.Combine(_provider.Options.BaseUrl, "/dream-machine/v1/generations/" + path);
    }
}
