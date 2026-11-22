// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.GenerateText;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using ProviderGeneratedImage = Vercel.AI.Provider.GeneratedImage;

namespace Vercel.AI.Xai;

/// <summary>Settings of <see cref="XaiImageModel"/>. Maps to <c>XaiImageModelConfig</c>.</summary>
public sealed class XaiImageModelConfig
{
    /// <summary>Creates a config. <paramref name="headers"/> maps <c>headers</c>: the model adds no headers of its own.</summary>
    public XaiImageModelConfig(
        string provider,
        string baseUrl,
        Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> headers,
        HttpClient httpClient
    )
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        Headers = headers ?? throw new ArgumentNullException(nameof(headers));
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Provider id, for example <c>xai.image</c>.</summary>
    public string Provider { get; }

    /// <summary>API base URL.</summary>
    public string BaseUrl { get; }

    /// <summary>Resolves the request headers, including the credential.</summary>
    public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> Headers { get; }

    /// <summary>Maps <c>fetch</c>: the HTTP client for API calls.</summary>
    public HttpClient HttpClient { get; }

    /// <summary>HTTP client that downloads URL-only images. Defaults to a client that does not follow redirects.</summary>
    public HttpClient? DownloadHttpClient { get; set; }

    /// <summary>Sets the response timestamp and defaults to UTC now. Maps to <c>_internal.currentDate</c>.</summary>
    public Func<DateTime>? CurrentDate { get; set; }
}

/// <summary>xAI image model, over <c>/images/generations</c> and <c>/images/edits</c>. Maps to <c>XaiImageModel</c>.</summary>
public sealed class XaiImageModel : IImageModel, IImageCaller
{
    private readonly XaiImageModelConfig _config;

    /// <summary>Creates a model.</summary>
    public XaiImageModel(string modelId, XaiImageModelConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        MaxImagesPerCall = XaiModelCatalog
            .GetXaiImageModelEntry(modelId)
            .Capabilities.MaxImagesPerCall;
    }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion => "v4";

    /// <inheritdoc cref="IImageModel.Provider" />
    public string Provider => _config.Provider;

    /// <inheritdoc cref="IImageModel.ModelId" />
    public string ModelId { get; }

    /// <summary>Maximum images in one call. Maps to <c>maxImagesPerCall</c>.</summary>
    public int? MaxImagesPerCall { get; }

    /// <summary>Whether the model takes input images. Null when the catalog does not say. Maps to <c>supportsFileInputs</c>.</summary>
    public bool? SupportsFileInputs =>
        Known(XaiModelCatalog.GetXaiImageModelEntry(ModelId).Capabilities.FileInput);

    /// <summary>Whether the model takes a mask. Null when the catalog does not say. Maps to <c>supportsMaskInputs</c>.</summary>
    public bool? SupportsMaskInputs =>
        Known(XaiModelCatalog.GetXaiImageModelEntry(ModelId).Capabilities.MaskInput);

    /// <inheritdoc />
    public Task<int?> ResolveMaxImagesPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxImagesPerCall);
    }

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(
        ImageCallOptions options,
        CancellationToken cancellationToken
    )
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var result = await DoGenerateAsync(
                options.ToModelCall(cancellationToken),
                cancellationToken
            )
            .ConfigureAwait(false);
        var images = new List<ProviderGeneratedImage>();
        foreach (var image in result.Images)
        {
            var data = image is string base64 ? Convert.FromBase64String(base64) : (byte[])image!;
            images.Add(
                new ProviderGeneratedImage(
                    MediaTypes.DetectMediaType(data, "image") ?? "image/png",
                    data,
                    null
                )
            );
        }

        return new ImageGenerationResult(
            images,
            result.Warnings.Select(warning => warning.ToCallWarning()).ToList()
        );
    }

    /// <inheritdoc />
    public async Task<ImageModelResult> DoGenerateAsync(
        ImageModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var warnings = new List<OperationWarning>();

        if (call.Size != null)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "size",
                    "This model does not support the `size` option. Use `aspectRatio` instead."
                )
            );
        }

        if (call.Seed != null)
        {
            warnings.Add(OperationWarning.Unsupported("seed"));
        }

        if (call.Mask != null)
        {
            warnings.Add(OperationWarning.Unsupported("mask"));
        }

        var xaiOptions = XaiImageModelOptions.Parse(call.ProviderOptions);

        var imageUrls = (call.Files ?? [])
            .Select(FileDataConversions.ConvertImageModelFileToDataUri)
            .ToList();
        var endpoint = imageUrls.Count > 0 ? "images/edits" : "images/generations";

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["prompt"] = call.Prompt,
            ["n"] = call.N,
            ["response_format"] = "b64_json",
        };

        if (call.AspectRatio != null)
        {
            body["aspect_ratio"] = call.AspectRatio;
        }

        if (xaiOptions?.OutputFormat != null)
        {
            body["output_format"] = xaiOptions.OutputFormat;
        }

        if (xaiOptions?.SyncMode != null)
        {
            body["sync_mode"] = xaiOptions.SyncMode;
        }

        if (xaiOptions?.AspectRatio != null && call.AspectRatio == null)
        {
            body["aspect_ratio"] = xaiOptions.AspectRatio;
        }

        if (xaiOptions?.Resolution != null)
        {
            body["resolution"] = xaiOptions.Resolution;
        }

        if (xaiOptions?.Quality != null)
        {
            body["quality"] = xaiOptions.Quality;
        }

        if (xaiOptions?.User != null)
        {
            body["user"] = xaiOptions.User;
        }

        if (imageUrls.Count == 1)
        {
            body["image"] = ImageUrl(imageUrls[0]);
        }
        else if (imageUrls.Count > 1)
        {
            body["images"] = new JsonArray(imageUrls.Select(ImageUrl).ToArray<JsonNode?>());
        }

        var timestamp = _config.CurrentDate?.Invoke() ?? DateTime.UtcNow;
        var headers = ProviderExchange.Merge(
            await _config.Headers(cancellationToken).ConfigureAwait(false),
            call.Headers
        );
        var response = await ProviderExchange
            .SendAsync(
                _config.HttpClient,
                HttpMethod.Post,
                ApiKeys.Combine(_config.BaseUrl, endpoint),
                ProviderExchange.Json(body.ToJsonString()),
                headers,
                cancellationToken,
                XaiError.FailedResponseHandler
            )
            .ConfigureAwait(false);

        var data = ReadImages(response.Body);
        if (data.Any(image => image.RespectModeration == false))
        {
            throw new AiSdkException(
                "Image generation was blocked due to a content policy violation."
            );
        }

        var hasAllBase64 = data.All(image => image.B64Json != null);
        // Upstream downloads URL-only images with Promise.all.
        object?[] images = hasAllBase64
            ? data.Select(image => (object?)image.B64Json).ToArray()
            : await Task.WhenAll(
                    data.Select(async image =>
                        (object?)
                            await DownloadImageAsync(image.Url!, headers, cancellationToken)
                                .ConfigureAwait(false)
                    )
                )
                .ConfigureAwait(false);

        var metadata = new JsonObject
        {
            ["images"] = new JsonArray(
                data.Select(image =>
                        (JsonNode?)(
                            string.IsNullOrEmpty(image.RevisedPrompt)
                                ? new JsonObject()
                                : new JsonObject { ["revisedPrompt"] = image.RevisedPrompt }
                        )
                    )
                    .ToArray()
            ),
        };
        if (ReadCost(response.Body) is { } cost)
        {
            metadata["costInUsdTicks"] = cost;
        }

        return new ImageModelResult(
            images,
            warnings,
            providerMetadata: OperationJson.Parse(
                new JsonObject { [XaiProvider.ProviderId] = metadata }.ToJsonString()
            ),
            response: new ProviderResponse(response.Headers, timestamp: timestamp, modelId: ModelId)
        );
    }

    private static bool? Known(ModelCapability capability)
    {
        return capability.IsUnknown || capability.Condition != null ? null : capability.IsSupported;
    }

    private static JsonObject ImageUrl(string url)
    {
        return new JsonObject { ["url"] = url, ["type"] = "image_url" };
    }

    private static List<XaiImageData> ReadImages(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var items = new List<XaiImageData>();
            foreach (var item in document.RootElement.GetProperty("data").EnumerateArray())
            {
                items.Add(
                    new XaiImageData(
                        Text(item, "url"),
                        Text(item, "b64_json"),
                        Text(item, "revised_prompt"),
                        item.TryGetProperty("respect_moderation", out var moderation)
                        && moderation.ValueKind is JsonValueKind.True or JsonValueKind.False
                            ? moderation.GetBoolean()
                            : null
                    )
                );
            }

            return items;
        }
        catch (Exception error)
            when (error is JsonException or InvalidOperationException or KeyNotFoundException)
        {
            throw new InvalidResponseDataException(body, "Invalid xAI image response.");
        }
    }

    private static double? ReadCost(string body)
    {
        using var document = JsonDocument.Parse(body);
        return
            document.RootElement.TryGetProperty("usage", out var usage)
            && usage.ValueKind == JsonValueKind.Object
            && usage.TryGetProperty("cost_in_usd_ticks", out var cost)
            && cost.ValueKind == JsonValueKind.Number
            ? cost.GetDouble()
            : null;
    }

    private static string? Text(JsonElement item, string name)
    {
        return item.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private async Task<byte[]> DownloadImageAsync(
        string url,
        IReadOnlyDictionary<string, string?> headers,
        CancellationToken cancellationToken
    )
    {
        // The URL is a generated-image URL from the provider response; validate it.
        var image = await ProviderExchange
            .GetResponseUrlAsync(
                _config.HttpClient,
                url,
                _config.BaseUrl,
                DownloadUrls.ValidateDownloadUrl,
                null,
                cancellationToken,
                ProviderExchange.DownloadClient(_config.DownloadHttpClient),
                ProviderExchange.UserAgentOnly(headers)
            )
            .ConfigureAwait(false);
        return image.Bytes;
    }

    private sealed record XaiImageData(
        string? Url,
        string? B64Json,
        string? RevisedPrompt,
        bool? RespectModeration
    );
}
