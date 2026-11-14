// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using GeneratedImage = Vercel.AI.Provider.GeneratedImage;

namespace Vercel.AI.Azure;

/// <summary>
/// MAI image generations and edits (<c>/mai/v1/images/*</c>) on Azure AI Foundry.
/// The API returns one PNG per request and takes <c>width</c>/<c>height</c>, not <c>size</c>.
/// Maps to <c>AzureMaiImageModel</c> in <c>azure-mai-image-model.ts</c>.
/// </summary>
internal sealed class AzureMaiImageModel : IImageModel
{
    // MAI rejects either side below 768px; 1024x1024 is its default output.
    private const int MinSide = 768;
    private const int DefaultPixels = 1024 * 1024;

    private readonly AzureOpenAIProvider _provider;
    private readonly ProviderHttp _http;
    private readonly Func<DateTimeOffset>? _clock;

    /// <summary>Creates a MAI image model. <paramref name="clock"/> sets the response timestamp.</summary>
    public AzureMaiImageModel(AzureOpenAIProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock;

        // The API ignores retries upstream: a failed request is reported, not repeated, so `n` requests stay `n` requests.
        _http = new ProviderHttp(provider.SharedHttpClient, new RetryPolicy { MaxRetries = 0 });
    }

    /// <inheritdoc />
    public string Provider => "azure.image";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>One image per request.</summary>
    public int MaxImagesPerCall => 1;

    /// <summary>MAI edits take reference images.</summary>
    public bool SupportsFileInputs => true;

    /// <summary>MAI edits do not take masks.</summary>
    public bool SupportsMaskInputs => false;

    /// <summary>
    /// Width and height for an aspect ratio such as <c>16:9</c>, keeping the default 1024x1024 pixel budget and raising the
    /// short side to 768. Null when the ratio is not valid. Maps to <c>dimensionsForAspectRatio</c>.
    /// </summary>
    public static (int Width, int Height)? DimensionsForAspectRatio(string aspectRatio)
    {
        var parts = aspectRatio.Split(':');
        var w = ParseNumber(parts[0]);
        var h = parts.Length > 1 ? ParseNumber(parts[1]) : double.NaN;
        if (!(w > 0 && h > 0))
        {
            return null;
        }

        var ratio = w / h;
        var width = Math.Sqrt(DefaultPixels * ratio);
        var height = width / ratio;
        if (Math.Min(width, height) < MinSide)
        {
            if (width < height)
            {
                width = MinSide;
                height = MinSide / ratio;
            }
            else
            {
                width = MinSide * ratio;
                height = MinSide;
            }
        }

        return (Math.Max(MinSide, Floor16(width)), Math.Max(MinSide, Floor16(height)));
    }

    /// <inheritdoc />
    public Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        return DoGenerateAsync(options, AzureImageModelOptions.Parse(options.ProviderOptionsElement), cancellationToken);
    }

    /// <summary>Generates or edits images with the MAI image API. Maps to <c>doGenerate</c>.</summary>
    public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, AzureImageModelMaiOptions? maiOptions, CancellationToken cancellationToken)
    {
        var timestamp = _clock?.Invoke() ?? DateTimeOffset.UtcNow;
        var warnings = new List<CallWarning>();

        if (options.Seed != null)
        {
            warnings.Add(new CallWarning("unsupported", "seed", null));
        }

        if (options.Mask != null)
        {
            warnings.Add(new CallWarning("unsupported", "mask", "MAI image edits do not support masks."));
        }

        var dimensions = ParseSize(options.Size);
        if (options.Size != null && options.AspectRatio != null)
        {
            warnings.Add(new CallWarning("unsupported", "aspectRatio", "aspectRatio is ignored when size is set."));
        }
        else if (options.AspectRatio != null)
        {
            dimensions = DimensionsForAspectRatio(options.AspectRatio);
            if (dimensions == null)
            {
                warnings.Add(new CallWarning("unsupported", "aspectRatio", "Invalid aspect ratio: " + options.AspectRatio + "."));
            }
        }

        var fields = new JsonObject
        {
            ["model"] = ModelId,
            ["prompt"] = options.Prompt,
        };
        if (dimensions is { } size)
        {
            fields["width"] = size.Width;
            fields["height"] = size.Height;
        }

        if (maiOptions?.AutoAspectRatio is { } autoAspectRatio)
        {
            fields["auto_aspect_ratio"] = autoAspectRatio;
        }

        if (maiOptions?.WebGrounding is { } webGrounding)
        {
            fields["web_grounding"] = webGrounding;
        }

        var headers = _provider.CreateHeaders(AzureApi.Mai, options.Headers);
        var images = options.Files is { Count: > 0 }
            ? await Task.WhenAll(options.Files.Select(file => FileToBlobAsync(file, cancellationToken))).ConfigureAwait(false)
            : null;

        // The API ignores `n`, so a call reached with n > 1 (e.g. through an `api: 'mai'` override) sends one request per image.
        var requests = Enumerable.Range(0, Math.Max(1, options.Count)).Select(_ => images != null
            ? SendEditAsync(fields, images, headers, cancellationToken)
            : SendGenerationAsync(fields, headers, cancellationToken)).ToArray();
        var responses = await Task.WhenAll(requests).ConfigureAwait(false);

        var generated = new List<GeneratedImage>();
        var metadata = new JsonArray();
        int? inputTokens = null;
        int? outputTokens = null;
        int? totalTokens = null;
        foreach (var response in responses)
        {
            using var document = ParseResponse(response.Body);
            var root = document.RootElement;
            var usage = Child(root, "usage", JsonValueKind.Object);
            var textTokens = Int(usage, "num_input_text_tokens");
            var imageTokens = Int(usage, "num_input_image_tokens");
            if (usage != null)
            {
                var input = AddTokens(textTokens, imageTokens);
                var output = Int(usage, "num_output_tokens");
                inputTokens = AddTokens(inputTokens, input);
                outputTokens = AddTokens(outputTokens, output);
                totalTokens = AddTokens(totalTokens, AddTokens(input, output));
            }

            var created = root.TryGetProperty("created", out var createdValue) && createdValue.ValueKind == JsonValueKind.Number ? createdValue : (JsonElement?)null;
            var responseSize = root.TryGetProperty("size", out var sizeValue) && sizeValue.ValueKind == JsonValueKind.String ? sizeValue.GetString() : null;
            foreach (var item in root.GetProperty("data").EnumerateArray())
            {
                generated.Add(new GeneratedImage("image/png", Convert.FromBase64String(item.GetProperty("b64_json").GetString() ?? string.Empty), null));
                var entry = new JsonObject();
                if (created != null)
                {
                    entry["created"] = JsonNode.Parse(created.Value.GetRawText());
                }

                if (responseSize != null)
                {
                    entry["size"] = responseSize;
                }

                if (textTokens != null)
                {
                    entry["textTokens"] = textTokens;
                }

                if (imageTokens != null)
                {
                    entry["imageTokens"] = imageTokens;
                }

                metadata.Add(entry);
            }
        }

        var providerMetadata = OpenAIJson.Element(new JsonObject { ["azure"] = new JsonObject { ["images"] = metadata } });
        return new ImageGenerationResult(
            generated,
            warnings,
            new ImageResponseInfo(timestamp, ModelId, responses.Length > 0 ? responses[0].Headers : null),
            inputTokens == null && outputTokens == null && totalTokens == null ? null : new LanguageModelUsage(inputTokens, outputTokens, totalTokens),
            providerMetadata);
    }

    private static double ParseNumber(string value)
    {
        if (value.Length == 0)
        {
            return 0;
        }

        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) ? number : double.NaN;
    }

    private static int Floor16(double value)
    {
        return (int)(Math.Floor((value / 16) + 1e-9) * 16);
    }

    private static (int Width, int Height)? ParseSize(string? size)
    {
        if (size == null)
        {
            return null;
        }

        var parts = size.Split('x');
        return parts.Length == 2
            && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var width)
            && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var height)
            ? (width, height)
            : null;
    }

    private static int? AddTokens(int? a, int? b)
    {
        return a == null && b == null ? null : (a ?? 0) + (b ?? 0);
    }

    private static JsonElement? Child(JsonElement element, string name, JsonValueKind kind)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == kind ? value : null;
    }

    private static int? Int(JsonElement? element, string name)
    {
        return element is { } value && value.TryGetProperty(name, out var number) && number.ValueKind == JsonValueKind.Number && number.TryGetInt32(out var result)
            ? result
            : null;
    }

    private static JsonDocument ParseResponse(string body)
    {
        try
        {
            var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("data", out var data)
                && data.ValueKind == JsonValueKind.Array)
            {
                return document;
            }

            document.Dispose();
        }
        catch (JsonException)
        {
        }

        throw new InvalidResponseDataException(body, "Invalid MAI image response.");
    }

    private Task<ProviderTextResponse> SendGenerationAsync(JsonObject fields, IReadOnlyDictionary<string, string?> headers, CancellationToken cancellationToken)
    {
        return _http.SendJsonStringAsync(HttpMethod.Post, MaiUri("/images/generations"), fields.ToJsonString(), headers, cancellationToken);
    }

    private async Task<ProviderTextResponse> SendEditAsync(
        JsonObject fields,
        IReadOnlyList<ImageBlob> images,
        IReadOnlyDictionary<string, string?> headers,
        CancellationToken cancellationToken)
    {
        using var form = new MultipartFormDataContent();
        foreach (var field in fields)
        {
            var text = field.Value is JsonValue value && value.TryGetValue<string>(out var str) ? str : field.Value!.ToJsonString();
            form.Add(new StringContent(text), field.Key);
        }

        for (var index = 0; index < images.Count; index++)
        {
            var content = new ByteArrayContent(images[index].Data);
            if (System.Net.Http.Headers.MediaTypeHeaderValue.TryParse(images[index].MediaType, out var type))
            {
                content.Headers.ContentType = type;
            }

            var extension = images[index].MediaType == "image/jpeg" ? "jpg" : "png";
            form.Add(content, "image", "image-" + (index + 1).ToString(CultureInfo.InvariantCulture) + "." + extension);
        }

        var response = await _http.SendBinaryAsync(HttpMethod.Post, MaiUri("/images/edits"), form, headers, cancellationToken).ConfigureAwait(false);
        return new ProviderTextResponse(System.Text.Encoding.UTF8.GetString(response.Body), response.Headers);
    }

    private Uri MaiUri(string path)
    {
        return new Uri(_provider.MaiBaseUrl() + path);
    }

    private async Task<ImageBlob> FileToBlobAsync(ImageModelFile file, CancellationToken cancellationToken)
    {
        if (file.Type == "url")
        {
            var (data, mediaType) = await ImageFileDownload.DownloadBlobAsync(_provider, _provider.CreateHeaders(), file.Url!, cancellationToken).ConfigureAwait(false);
            return new ImageBlob(data, mediaType);
        }

        return new ImageBlob(file.Data ?? Array.Empty<byte>(), file.MediaType ?? "application/octet-stream");
    }

    private sealed record ImageBlob(byte[] Data, string MediaType);
}
