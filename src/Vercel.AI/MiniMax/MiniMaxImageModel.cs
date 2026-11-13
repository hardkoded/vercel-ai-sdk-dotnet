// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.GenerateText;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using ProviderImage = Vercel.AI.Provider.GeneratedImage;

namespace Vercel.AI.MiniMax;

/// <summary>Configuration of a <see cref="MiniMaxImageModel"/>.</summary>
public sealed class MiniMaxImageModelConfig
{
    /// <summary>Creates a configuration.</summary>
    public MiniMaxImageModelConfig(string provider, string baseUrl, Func<Dictionary<string, string>> headers, HttpClient httpClient, Func<DateTime>? currentDate = null)
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        Headers = headers ?? throw new ArgumentNullException(nameof(headers));
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        CurrentDate = currentDate;
    }

    /// <summary>Provider id, <c>minimax.image</c>.</summary>
    public string Provider { get; }

    /// <summary>Image API origin including the version suffix.</summary>
    public string BaseUrl { get; }

    /// <summary>Resolves the request headers on every call (bearer token and user agent).</summary>
    public Func<Dictionary<string, string>> Headers { get; }

    /// <summary>The client every request goes through (upstream <c>fetch</c>).</summary>
    public HttpClient HttpClient { get; }

    /// <summary>Sets the response timestamp (upstream <c>_internal.currentDate</c>).</summary>
    public Func<DateTime>? CurrentDate { get; }
}

/// <summary>
/// MiniMax image model. It posts to <c>{baseURL}/images/generations</c>, or <c>/images/edits</c> when input images are given,
/// asks for URL output, and downloads each image.
/// </summary>
public sealed class MiniMaxImageModel : IImageModel, IImageCaller
{
    private const int ImagesPerCall = 10;

    /// <summary>Creates a model.</summary>
    public MiniMaxImageModel(string modelId, MiniMaxImageModelConfig config)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        Config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion => "v4";

    /// <inheritdoc cref="IImageModel.Provider" />
    public string Provider => Config.Provider;

    /// <inheritdoc cref="IImageModel.ModelId" />
    public string ModelId { get; }

    /// <summary>MiniMax creates up to 10 images per call.</summary>
    public int? MaxImagesPerCall => ImagesPerCall;

    /// <summary>MiniMax image models accept input images. Maps to <c>supportsFileInputs</c>.</summary>
    public bool SupportsFileInputs => true;

    /// <summary>MiniMax image models do not accept masks. Maps to <c>supportsMaskInputs</c>.</summary>
    public bool SupportsMaskInputs => false;

    /// <summary>The configuration the provider passed in.</summary>
    public MiniMaxImageModelConfig Config { get; }

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

        var result = await DoGenerateAsync(options.ToModelCall(cancellationToken), cancellationToken).ConfigureAwait(false);
        var images = new List<ProviderImage>();
        foreach (var image in result.Images)
        {
            var bytes = (byte[])image!;
            images.Add(new ProviderImage(MediaTypes.DetectMediaType(bytes, "image") ?? "image/png", bytes, null));
        }

        var usage = result.Usage == null ? null : new LanguageModelUsage(result.Usage.InputTokens, result.Usage.OutputTokens, result.Usage.TotalTokens);
        var response = new ImageResponseInfo(
            result.Response.Timestamp is { } timestamp ? new DateTimeOffset(timestamp) : DateTimeOffset.UtcNow,
            result.Response.ModelId,
            result.Response.Headers);
        return new ImageGenerationResult(images, result.Warnings.Select(warning => warning.ToCallWarning()).ToList(), response, usage, result.ProviderMetadata);
    }

    /// <inheritdoc />
    public async Task<ImageModelResult> DoGenerateAsync(ImageModelCall call, CancellationToken cancellationToken)
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var warnings = new List<OperationWarning>();
        if (call.Seed != null)
        {
            warnings.Add(new OperationWarning("unsupported", feature: "seed"));
        }

        if (call.Mask != null)
        {
            throw new UnsupportedFunctionalityException("MiniMax image masks", "'MiniMax image masks' functionality not supported.");
        }

        var minimax = MiniMaxOptions(call.ProviderOptions);
        var area = ReadString(minimax, "area", MiniMaxImageModelOptions.MiniMaxImageAreas);
        var outputFormat = ReadString(minimax, "outputFormat", MiniMaxImageModelOptions.MiniMaxImageOutputFormats);
        var outputCompression = ReadCompression(minimax);
        var disablePromptEnhanced = ReadBool(minimax, "disablePromptEnhanced");
        var hasFiles = call.Files is { Count: > 0 };
        if (hasFiles && disablePromptEnhanced != null)
        {
            warnings.Add(new OperationWarning("unsupported", feature: "disablePromptEnhanced", details: "MiniMax image edits always apply prompt enhancement."));
        }

        var timestamp = Config.CurrentDate?.Invoke() ?? DateTime.UtcNow;
        var body = new JsonObject { ["model"] = ModelId, ["prompt"] = call.Prompt, ["n"] = call.N };
        AddIfNotNull(body, "size", call.Size);
        AddIfNotNull(body, "aspect_ratio", call.AspectRatio);
        AddIfNotNull(body, "area", area);
        AddIfNotNull(body, "output_format", outputFormat);
        if (outputCompression != null)
        {
            body["output_compression"] = outputCompression;
        }

        if (!hasFiles && disablePromptEnhanced != null)
        {
            body["disable_prompt_enhanced"] = disablePromptEnhanced;
        }

        if (hasFiles)
        {
            var images = new JsonArray();
            foreach (var file in call.Files!)
            {
                images.Add(new JsonObject { ["image_url"] = FileDataConversions.ConvertImageModelFileToDataUri(file) });
            }

            body["images"] = images;
        }

        // URL output supports the full batch size without the base64 area limit.
        body["response_format"] = "url";
        var headers = ProviderExchange.Merge(Config.Headers().ToDictionary(pair => pair.Key, pair => (string?)pair.Value), call.Headers);
        var uri = ApiKeys.Combine(Config.BaseUrl, "/images/" + (hasFiles ? "edits" : "generations"));
        var exchange = await ProviderExchange.SendAsync(Config.HttpClient, HttpMethod.Post, uri, ProviderExchange.Json(body.ToJsonString()), headers, cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(exchange.Body);
        var root = document.RootElement;
        if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidResponseDataException(exchange.Body, "MiniMax image response has no data array.");
        }

        var successful = new List<JsonElement>();
        var position = 0;
        foreach (var image in data.EnumerateArray())
        {
            position++;
            if (image.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
            {
                var message = error.TryGetProperty("message", out var text) && text.ValueKind == JsonValueKind.String ? text.GetString() : null;
                warnings.Add(new OperationWarning("other", message: "Image " + position + " failed: " + message));
                continue;
            }

            successful.Add(image);
        }

        var downloads = new List<object?>();
        foreach (var image in successful)
        {
            var url = ReadOptionalString(image, "url") ?? throw new AiSdkException("MiniMax returned an image without a URL.");
            var response = await ProviderExchange.GetResponseUrlAsync(Config.HttpClient, url, Config.BaseUrl, DownloadUrls.ValidateDownloadUrl, null, cancellationToken, null, ProviderExchange.UserAgentOnly(headers)).ConfigureAwait(false);
            downloads.Add(response.Bytes);
        }

        OperationUsage? usage = null;
        JsonElement? rawUsage = root.TryGetProperty("usage", out var usageElement) && usageElement.ValueKind == JsonValueKind.Object ? usageElement : null;
        if (rawUsage is { } counts)
        {
            usage = new OperationUsage(ReadOptionalInt(counts, "input_tokens"), ReadOptionalInt(counts, "output_tokens"), ReadOptionalInt(counts, "total_tokens"));
        }

        var outputFormatReported = ReadOptionalString(root, "output_format");
        var requestId = ReadOptionalString(root, "request_id");
        var imageMetadata = new JsonArray();
        for (var index = 0; index < successful.Count; index++)
        {
            var entry = new JsonObject
            {
                ["size"] = ReadOptionalString(successful[index], "size"),
                ["revisedPrompt"] = ReadOptionalString(successful[index], "revised_prompt"),
            };
            if (outputFormatReported != null)
            {
                entry["outputFormat"] = outputFormatReported;
            }

            if (requestId != null)
            {
                entry["requestId"] = requestId;
            }

            // Core merges only per-image metadata; keep request usage once per call.
            if (index == 0 && rawUsage is { } reported)
            {
                entry["usage"] = JsonNode.Parse(reported.GetRawText());
            }

            imageMetadata.Add(entry);
        }

        var providerMetadata = JsonSerializer.SerializeToElement(new JsonObject { [MiniMaxProvider.ProviderId] = new JsonObject { ["images"] = imageMetadata } });
        return new ImageModelResult(downloads, warnings, usage, providerMetadata, new ProviderResponse(exchange.Headers, timestamp: timestamp, modelId: ModelId));
    }

    private static JsonElement? MiniMaxOptions(JsonElement? providerOptions)
    {
        return providerOptions is { ValueKind: JsonValueKind.Object } options
            && options.TryGetProperty(MiniMaxProvider.ProviderId, out var minimax)
            && minimax.ValueKind == JsonValueKind.Object
                ? minimax
                : null;
    }

    private static void AddIfNotNull(JsonObject body, string name, string? value)
    {
        if (value != null)
        {
            body[name] = value;
        }
    }

    private static string? ReadString(JsonElement? options, string name, IReadOnlyList<string> allowed)
    {
        if (options is not { } bag || !bag.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.String || !allowed.Contains(value.GetString()!))
        {
            throw new InvalidArgumentException("providerOptions.minimax." + name, value.GetRawText(), "must be one of " + string.Join(", ", allowed) + ".");
        }

        return value.GetString();
    }

    private static bool? ReadBool(JsonElement? options, string name)
    {
        if (options is not { } bag || !bag.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.True && value.ValueKind != JsonValueKind.False)
        {
            throw new InvalidArgumentException("providerOptions.minimax." + name, value.GetRawText(), "must be a boolean.");
        }

        return value.GetBoolean();
    }

    private static int? ReadCompression(JsonElement? options)
    {
        if (options is not { } bag || !bag.TryGetProperty("outputCompression", out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var number) || number < 0 || number > 100)
        {
            throw new InvalidArgumentException("providerOptions.minimax.outputCompression", value.GetRawText(), "must be an integer from 0 to 100.");
        }

        return number;
    }

    private static string? ReadOptionalString(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static int? ReadOptionalInt(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) ? number : null;
    }
}
