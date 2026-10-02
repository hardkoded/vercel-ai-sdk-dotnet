// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Luma;

/// <summary>Reference image passed to Luma. Only URL images are accepted.</summary>
public sealed class LumaReferenceImage
{
    private LumaReferenceImage(string? url, byte[]? data)
    {
        Url = url;
        Data = data;
    }

    /// <summary>Creates a URL reference.</summary>
    public static LumaReferenceImage FromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentNullException(nameof(url));
        }

        return new LumaReferenceImage(url, null);
    }

    /// <summary>Creates an inline image. Luma rejects these.</summary>
    public static LumaReferenceImage FromBytes(byte[] data)
    {
        return new LumaReferenceImage(null, data ?? throw new ArgumentNullException(nameof(data)));
    }

    /// <summary>Public image URL.</summary>
    public string? Url { get; }

    /// <summary>Inline bytes. Luma does not accept these.</summary>
    public byte[]? Data { get; }
}

/// <summary>Settings ignored or adapted by Luma image generation.</summary>
public sealed class LumaImageWarning
{
    /// <summary>Creates a warning.</summary>
    public LumaImageWarning(string type, string feature, string details)
    {
        Type = type ?? string.Empty;
        Feature = feature ?? string.Empty;
        Details = details ?? string.Empty;
    }

    /// <summary>Warning category.</summary>
    public string Type { get; }

    /// <summary>Feature that was ignored.</summary>
    public string Feature { get; }

    /// <summary>Explanation.</summary>
    public string Details { get; }
}

/// <summary>Luma image generation request.</summary>
public sealed class LumaImageRequest
{
    /// <summary>Creates a request.</summary>
    public LumaImageRequest(string prompt)
    {
        Prompt = prompt ?? throw new ArgumentNullException(nameof(prompt));
    }

    /// <summary>Image prompt.</summary>
    public string Prompt { get; }

    /// <summary>Aspect ratio such as <c>16:9</c>.</summary>
    public string? AspectRatio { get; set; }

    /// <summary>Ignored. Luma wants <see cref="AspectRatio"/>.</summary>
    public string? Size { get; set; }

    /// <summary>Ignored.</summary>
    public int? Seed { get; set; }

    /// <summary>Reference images.</summary>
    public IReadOnlyList<LumaReferenceImage>? Files { get; set; }

    /// <summary>Mask. Luma rejects mask editing.</summary>
    public LumaReferenceImage? Mask { get; set; }

    /// <summary>Luma provider options object. Poll settings are not sent.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Headers merged over the provider headers for this call.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>Luma image generation result.</summary>
public sealed class LumaImageGeneration
{
    /// <summary>Creates a result.</summary>
    public LumaImageGeneration(byte[] image, string modelId, DateTimeOffset timestamp, IReadOnlyDictionary<string, string> responseHeaders, IReadOnlyList<LumaImageWarning> warnings)
    {
        Image = image ?? Array.Empty<byte>();
        ModelId = modelId ?? string.Empty;
        Timestamp = timestamp;
        ResponseHeaders = responseHeaders ?? new Dictionary<string, string>();
        Warnings = warnings ?? Array.Empty<LumaImageWarning>();
    }

    /// <summary>Downloaded image bytes.</summary>
    public byte[] Image { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Clock value captured when the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Headers from the generation POST.</summary>
    public IReadOnlyDictionary<string, string> ResponseHeaders { get; }

    /// <summary>Warnings for unsupported settings.</summary>
    public IReadOnlyList<LumaImageWarning> Warnings { get; }
}

/// <summary>HTTP failure from a Luma image call.</summary>
public sealed class LumaRequestException : AiSdkException
{
    /// <summary>Creates an exception.</summary>
    public LumaRequestException(string message, int statusCode, string url, string? responseBody, string? requestBody)
        : base(message)
    {
        StatusCode = statusCode;
        Url = url ?? string.Empty;
        ResponseBody = responseBody;
        RequestBody = requestBody;
    }

    /// <summary>HTTP status code.</summary>
    public int StatusCode { get; }

    /// <summary>Request URL.</summary>
    public string Url { get; }

    /// <summary>Response body.</summary>
    public string? ResponseBody { get; }

    /// <summary>JSON request body.</summary>
    public string? RequestBody { get; }
}

/// <summary>Luma image model. Posts to <c>/dream-machine/v1/generations/image</c> and polls the generation.</summary>
public sealed class LumaImageModel : IImageModel
{
    private const int DefaultPollIntervalMillis = 500;

    private readonly HttpClient _http;
    private readonly Func<IReadOnlyDictionary<string, string?>> _headers;

    /// <summary>Creates an image model.</summary>
    public LumaImageModel(HttpClient http, string modelId, string provider, string baseUrl, Func<IReadOnlyDictionary<string, string?>> headers)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        Provider = provider ?? "luma.image";
        BaseUrl = string.IsNullOrEmpty(baseUrl) ? LumaProvider.DefaultBaseUrl : baseUrl.TrimEnd('/');
        _headers = headers ?? throw new ArgumentNullException(nameof(headers));
    }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion
    {
        get { return "v4"; }
    }

    /// <summary>Luma accepts one image per call.</summary>
    public int MaxImagesPerCall
    {
        get { return 1; }
    }

    /// <inheritdoc />
    public string Provider { get; }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>API origin without a trailing slash.</summary>
    public string BaseUrl { get; }

    /// <summary>HTTP client used for generation and image download.</summary>
    internal HttpClient HttpClient
    {
        get { return _http; }
    }

    /// <summary>Clock used for <see cref="LumaImageGeneration.Timestamp"/>.</summary>
    public Func<DateTimeOffset> Clock { get; set; } = () => DateTimeOffset.UtcNow;

    /// <summary>Headers the provider sends before per-request headers are merged.</summary>
    public IReadOnlyDictionary<string, string?> Headers()
    {
        return _headers();
    }

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        var request = new LumaImageRequest(options.Prompt)
        {
            AspectRatio = options.AspectRatio,
            Size = options.Size,
        };
        var result = await GenerateAsync(request, cancellationToken).ConfigureAwait(false);
        return new ImageGenerationResult(new[] { new GeneratedImage("image/png", result.Image, null) });
    }

    /// <summary>Generates an image and downloads the completed asset.</summary>
    public async Task<LumaImageGeneration> GenerateAsync(LumaImageRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var warnings = new List<LumaImageWarning>();
        if (request.Seed != null)
        {
            warnings.Add(new LumaImageWarning("unsupported", "seed", "This model does not support the `seed` option."));
        }

        if (request.Size != null)
        {
            warnings.Add(new LumaImageWarning("unsupported", "size", "This model does not support the `size` option. Use `aspectRatio` instead."));
        }

        var timestamp = Clock();
        var pollInterval = TimeSpan.FromMilliseconds(DefaultPollIntervalMillis);
        var maxAttempts = 60000 / DefaultPollIntervalMillis;
        string referenceType = "image";
        var imageHints = new List<ImageHint>();
        JsonElement? passthrough = null;
        if (request.ProviderOptions is { } options && options.ValueKind == JsonValueKind.Object)
        {
            passthrough = options;
            if (options.TryGetProperty("pollIntervalMillis", out var interval) && interval.TryGetInt32(out var millis))
            {
                pollInterval = TimeSpan.FromMilliseconds(millis);
            }

            if (options.TryGetProperty("maxPollAttempts", out var attempts) && attempts.TryGetInt32(out var parsedAttempts))
            {
                maxAttempts = parsedAttempts;
            }

            if (options.TryGetProperty("referenceType", out var reference) && reference.ValueKind == JsonValueKind.String)
            {
                referenceType = reference.GetString() ?? "image";
            }

            if (options.TryGetProperty("images", out var images) && images.ValueKind == JsonValueKind.Array)
            {
                foreach (var image in images.EnumerateArray())
                {
                    var hint = new ImageHint();
                    if (image.TryGetProperty("weight", out var weight) && weight.ValueKind == JsonValueKind.Number)
                    {
                        hint.Weight = weight.GetDouble();
                    }

                    if (image.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String)
                    {
                        hint.Id = id.GetString();
                    }

                    imageHints.Add(hint);
                }
            }
        }

        var body = new JsonObject { ["prompt"] = request.Prompt };
        if (!string.IsNullOrEmpty(request.AspectRatio))
        {
            body["aspect_ratio"] = request.AspectRatio;
        }

        body["model"] = ModelId;
        foreach (var pair in EditingOptions(request.Files, request.Mask, referenceType, imageHints))
        {
            body[pair.Key] = pair.Value;
        }

        if (passthrough is { } extra)
        {
            foreach (var property in extra.EnumerateObject())
            {
                if (property.NameEquals("pollIntervalMillis") || property.NameEquals("maxPollAttempts") || property.NameEquals("referenceType") || property.NameEquals("images"))
                {
                    continue;
                }

                body[property.Name] = JsonNode.Parse(property.Value.GetRawText());
            }
        }

        var json = body.ToJsonString();
        var createUrl = GenerationsUrl(null);
        var created = await MediaExchange.SendAsync(_http, HttpMethod.Post, createUrl, MediaExchange.Json(json), Merge(request.Headers), cancellationToken).ConfigureAwait(false);
        if (created.StatusCode < 200 || created.StatusCode > 299)
        {
            throw new LumaRequestException(JsonValues.ExtractErrorMessage(created.Text, created.StatusCode), created.StatusCode, createUrl.AbsoluteUri, created.Text, json);
        }

        string? imageUrl;
        using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(created.Text) ? "{}" : created.Text))
        {
            imageUrl = await ResolveImageUrlAsync(document.RootElement, Merge(request.Headers), pollInterval, maxAttempts, cancellationToken).ConfigureAwait(false);
        }

        var downloaded = await MediaExchange.SendAsync(_http, HttpMethod.Get, new Uri(imageUrl, UriKind.Absolute), null, null, cancellationToken).ConfigureAwait(false);
        if (downloaded.StatusCode < 200 || downloaded.StatusCode > 299)
        {
            throw new LumaRequestException(JsonValues.ExtractErrorMessage(downloaded.Text, downloaded.StatusCode), downloaded.StatusCode, imageUrl, downloaded.Text, null);
        }

        return new LumaImageGeneration(downloaded.Body, ModelId, timestamp, created.Headers, warnings);
    }

    private async Task<string> ResolveImageUrlAsync(JsonElement created, Dictionary<string, string?> headers, TimeSpan pollInterval, int maxAttempts, CancellationToken cancellationToken)
    {
        var state = StringOf(created, "state");
        var direct = AssetImage(created) ?? StringOf(created, "url");
        if (string.IsNullOrEmpty(state))
        {
            if (!string.IsNullOrEmpty(direct))
            {
                return direct!;
            }

            throw new AiSdkException("Image generation completed but no image was found.");
        }

        if (string.Equals(state, "completed", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(direct))
            {
                throw new AiSdkException("Image generation completed but no image was found.");
            }

            return direct!;
        }

        if (string.Equals(state, "failed", StringComparison.OrdinalIgnoreCase))
        {
            throw new AiSdkException("Image generation failed.");
        }

        var id = StringOf(created, "id");
        if (string.IsNullOrEmpty(id))
        {
            throw new AiSdkException("Image generation did not return an id.");
        }

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var polled = await MediaExchange.SendAsync(_http, HttpMethod.Get, GenerationsUrl(id), null, headers, cancellationToken).ConfigureAwait(false);
            if (polled.StatusCode < 200 || polled.StatusCode > 299)
            {
                throw new LumaRequestException(JsonValues.ExtractErrorMessage(polled.Text, polled.StatusCode), polled.StatusCode, GenerationsUrl(id).AbsoluteUri, polled.Text, null);
            }

            using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(polled.Text) ? "{}" : polled.Text))
            {
                var polledState = StringOf(document.RootElement, "state");
                if (string.Equals(polledState, "completed", StringComparison.OrdinalIgnoreCase))
                {
                    var image = AssetImage(document.RootElement);
                    if (string.IsNullOrEmpty(image))
                    {
                        throw new AiSdkException("Image generation completed but no image was found.");
                    }

                    return image!;
                }

                if (string.Equals(polledState, "failed", StringComparison.OrdinalIgnoreCase))
                {
                    throw new AiSdkException("Image generation failed.");
                }
            }

            if (pollInterval > TimeSpan.Zero)
            {
                await Task.Delay(pollInterval, cancellationToken).ConfigureAwait(false);
            }
        }

        throw new AiSdkException("Image generation timed out after " + maxAttempts + " attempts.");
    }

    private static Dictionary<string, JsonNode?> EditingOptions(IReadOnlyList<LumaReferenceImage>? files, LumaReferenceImage? mask, string referenceType, List<ImageHint> hints)
    {
        var options = new Dictionary<string, JsonNode?>();
        if (mask != null)
        {
            throw new AiSdkException(
                "Luma AI does not support mask-based image editing. " +
                "Use the prompt to describe the changes you want to make, along with " +
                "`prompt.images` containing the source image URL.");
        }

        if (files == null || files.Count == 0)
        {
            return options;
        }

        foreach (var file in files)
        {
            if (string.IsNullOrEmpty(file.Url))
            {
                throw new AiSdkException(
                    "Luma AI only supports URL-based images. " +
                    "Please provide image URLs using `prompt.images` with publicly accessible URLs. " +
                    "Base64 and Uint8Array data are not supported.");
            }
        }

        switch (referenceType)
        {
            case "style":
                options["style"] = Weighted(files, hints, 0.8);
                break;
            case "character":
                options["character"] = Characters(files, hints);
                break;
            case "modify_image":
            {
                if (files.Count > 1)
                {
                    throw new AiSdkException("Luma AI modify_image only supports a single input image. You provided " + files.Count + " images.");
                }

                var modifyWeight = 1.0;
                if (hints.Count > 0 && hints[0].Weight is double customWeight)
                {
                    modifyWeight = customWeight;
                }

                options["modify_image"] = new JsonObject
                {
                    ["url"] = files[0].Url,
                    ["weight"] = modifyWeight,
                };
                break;
            }
            default:
                if (files.Count > 4)
                {
                    throw new AiSdkException("Luma AI image supports up to 4 reference images. You provided " + files.Count + " images.");
                }

                options["image"] = Weighted(files, hints, 0.85);
                break;
        }

        return options;
    }

    private static JsonArray Weighted(IReadOnlyList<LumaReferenceImage> files, List<ImageHint> hints, double fallback)
    {
        var array = new JsonArray();
        for (var index = 0; index < files.Count; index++)
        {
            var weight = index < hints.Count && hints[index].Weight is double custom ? custom : fallback;
            array.Add(new JsonObject
            {
                ["url"] = files[index].Url,
                ["weight"] = weight,
            });
        }

        return array;
    }

    private static JsonObject Characters(IReadOnlyList<LumaReferenceImage> files, List<ImageHint> hints)
    {
        var order = new List<string>();
        var groups = new Dictionary<string, JsonArray>(StringComparer.Ordinal);
        for (var index = 0; index < files.Count; index++)
        {
            var id = index < hints.Count && !string.IsNullOrEmpty(hints[index].Id) ? hints[index].Id! : "identity0";
            if (!groups.TryGetValue(id, out var images))
            {
                images = new JsonArray();
                groups[id] = images;
                order.Add(id);
            }

            images.Add(files[index].Url);
        }

        foreach (var id in order)
        {
            if (groups[id].Count > 4)
            {
                throw new AiSdkException("Luma AI character supports up to 4 images per identity. Identity '" + id + "' has " + groups[id].Count + " images.");
            }
        }

        var character = new JsonObject();
        foreach (var id in order)
        {
            character[id] = new JsonObject { ["images"] = groups[id] };
        }

        return character;
    }

    private Uri GenerationsUrl(string? generationId)
    {
        return ApiKeys.Combine(BaseUrl, "/dream-machine/v1/generations/" + (generationId ?? "image"));
    }

    private Dictionary<string, string?> Merge(IDictionary<string, string>? requestHeaders)
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in _headers())
        {
            headers[pair.Key] = pair.Value;
        }

        if (requestHeaders != null)
        {
            foreach (var pair in requestHeaders)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }

    private static string? AssetImage(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("assets", out var assets) && assets.ValueKind == JsonValueKind.Object)
        {
            return StringOf(assets, "image");
        }

        return null;
    }

    private static string? StringOf(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return value.GetString();
    }

    private sealed class ImageHint
    {
        public double? Weight { get; set; }

        public string? Id { get; set; }
    }
}
