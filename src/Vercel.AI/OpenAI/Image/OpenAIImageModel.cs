// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>Image generation settings beyond the shared prompt and size.</summary>
public sealed class OpenAIImageCall
{
    /// <summary>Creates an image call.</summary>
    public OpenAIImageCall(string prompt)
    {
        Prompt = prompt ?? throw new ArgumentNullException(nameof(prompt));
    }

    /// <summary>Image prompt.</summary>
    public string Prompt { get; }

    /// <summary>How many images to generate.</summary>
    public int Count { get; set; } = 1;

    /// <summary>Size such as <c>1024x1024</c>.</summary>
    public string? Size { get; set; }

    /// <summary>Aspect ratio. OpenAI image models do not accept it.</summary>
    public string? AspectRatio { get; set; }

    /// <summary>Seed. OpenAI image models do not accept it.</summary>
    public int? Seed { get; set; }

    /// <summary>Provider options. The <c>openai</c> object is read.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Source images for an edit.</summary>
    public IReadOnlyList<OpenAIImageFile>? Files { get; set; }

    /// <summary>Optional edit mask.</summary>
    public OpenAIImageFile? Mask { get; set; }

    /// <summary>Extra HTTP headers.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }
}

/// <summary>An image file sent to the edits endpoint.</summary>
public sealed class OpenAIImageFile
{
    /// <summary>Creates an inline image file.</summary>
    public OpenAIImageFile(byte[] data, string mediaType, string? fileName)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        MediaType = mediaType;
        FileName = fileName;
    }

    /// <summary>Image bytes.</summary>
    public byte[] Data { get; }

    /// <summary>IANA media type.</summary>
    public string MediaType { get; }

    /// <summary>File name used in the multipart body.</summary>
    public string? FileName { get; }
}

/// <summary>Image generation result, including usage and warnings.</summary>
public sealed class OpenAIImageGeneration
{
    internal OpenAIImageGeneration(
        IReadOnlyList<GeneratedImage> images,
        IReadOnlyList<OpenAICallWarning> warnings,
        int? inputTokens,
        int? outputTokens,
        int? totalTokens,
        JsonElement providerMetadata,
        DateTimeOffset timestamp)
    {
        Images = images;
        Warnings = warnings;
        InputTokens = inputTokens;
        OutputTokens = outputTokens;
        TotalTokens = totalTokens;
        ProviderMetadata = providerMetadata;
        Timestamp = timestamp;
    }

    /// <summary>Generated images.</summary>
    public IReadOnlyList<GeneratedImage> Images { get; }

    /// <summary>Warnings produced while preparing the request.</summary>
    public IReadOnlyList<OpenAICallWarning> Warnings { get; }

    /// <summary>Input tokens, when the provider reports them.</summary>
    public int? InputTokens { get; }

    /// <summary>Output tokens, when the provider reports them.</summary>
    public int? OutputTokens { get; }

    /// <summary>Total tokens, when the provider reports them.</summary>
    public int? TotalTokens { get; }

    /// <summary>Provider metadata, including per-image token details.</summary>
    public JsonElement ProviderMetadata { get; }

    /// <summary>Timestamp recorded for the response.</summary>
    public DateTimeOffset Timestamp { get; }
}

/// <summary>OpenAI image generation and edit model.</summary>
public sealed class OpenAIImageModel : IImageModel
{
    private readonly OpenAIProvider _provider;
    private readonly Func<DateTimeOffset>? _clock;

    /// <summary>Creates an image model.</summary>
    public OpenAIImageModel(OpenAIProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock;
    }

    /// <inheritdoc />
    public string Provider => _provider.Name + ".image";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Maximum images accepted by one call for this model id.</summary>
    public int MaxImagesPerCall => MaxImages(ModelId);

    /// <summary>Maximum images for <paramref name="modelId"/>.</summary>
    public static int MaxImages(string modelId)
    {
        if (modelId == "dall-e-2")
        {
            return 10;
        }

        if (modelId.StartsWith("gpt-image-", StringComparison.Ordinal) || modelId.StartsWith("chatgpt-image-", StringComparison.Ordinal))
        {
            return 10;
        }

        if (modelId == "dall-e-3")
        {
            return 1;
        }

        return 1;
    }

    /// <summary>True when the model rejects an explicit <c>response_format</c>.</summary>
    public static bool OmitsResponseFormat(string modelId)
    {
        return modelId.StartsWith("gpt-image-", StringComparison.Ordinal)
            || modelId.StartsWith("chatgpt-image-", StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        var result = await GenerateAsync(
            new OpenAIImageCall(options.Prompt)
            {
                Count = options.Count,
                Size = options.Size,
                AspectRatio = options.AspectRatio,
            },
            cancellationToken).ConfigureAwait(false);
        return new ImageGenerationResult(result.Images);
    }

    /// <summary>Generates or edits images.</summary>
    public async Task<OpenAIImageGeneration> GenerateAsync(OpenAIImageCall call, CancellationToken cancellationToken)
    {
        var warnings = new List<OpenAICallWarning>();
        if (call.AspectRatio != null)
        {
            warnings.Add(new OpenAICallWarning("unsupported", "aspectRatio", "This model does not support aspect ratio. Use `size` instead."));
        }

        if (call.Seed != null)
        {
            warnings.Add(new OpenAICallWarning("unsupported", "seed", null));
        }

        var timestamp = OpenAIClock.Now(_clock);
        if (call.Files is { Count: > 0 })
        {
            return await EditAsync(call, warnings, timestamp, cancellationToken).ConfigureAwait(false);
        }

        var openai = OpenAIJson.OpenAIObject(call.ProviderOptions);
        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["prompt"] = call.Prompt,
            ["n"] = call.Count,
        };
        if (call.Size != null)
        {
            body["size"] = call.Size;
        }

        Copy(body, "quality", OpenAIJson.String(openai, "quality"));
        Copy(body, "style", OpenAIJson.String(openai, "style"));
        Copy(body, "background", OpenAIJson.String(openai, "background"));
        Copy(body, "moderation", OpenAIJson.String(openai, "moderation"));
        Copy(body, "output_format", OpenAIJson.String(openai, "outputFormat"));
        if (OpenAIJson.Int(openai, "outputCompression") is { } compression)
        {
            body["output_compression"] = compression;
        }

        Copy(body, "user", OpenAIJson.String(openai, "user"));
        if (!OmitsResponseFormat(ModelId))
        {
            body["response_format"] = "b64_json";
        }

        var response = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "images/generations"),
            body.ToJsonString(),
            _provider.CreateOpenAIHeaders(call.Headers),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        return Read(document.RootElement, warnings, timestamp);
    }

    private async Task<OpenAIImageGeneration> EditAsync(
        OpenAIImageCall call,
        List<OpenAICallWarning> warnings,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(ModelId), "model");
        content.Add(new StringContent(call.Prompt), "prompt");
        content.Add(new StringContent(call.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)), "n");
        if (call.Size != null)
        {
            content.Add(new StringContent(call.Size), "size");
        }

        var openai = OpenAIJson.OpenAIObject(call.ProviderOptions);
        Add(content, "quality", OpenAIJson.String(openai, "quality"));
        Add(content, "background", OpenAIJson.String(openai, "background"));
        Add(content, "output_format", OpenAIJson.String(openai, "outputFormat"));
        Add(content, "user", OpenAIJson.String(openai, "user"));
        foreach (var file in call.Files!)
        {
            var bytes = new ByteArrayContent(file.Data);
            bytes.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.MediaType);
            content.Add(bytes, "image", file.FileName ?? "image.png");
        }

        if (call.Mask != null)
        {
            var mask = new ByteArrayContent(call.Mask.Data);
            mask.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(call.Mask.MediaType);
            content.Add(mask, "mask", call.Mask.FileName ?? "mask.png");
        }

        var raw = await _provider.Http.SendBytesAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "images/edits"),
            content,
            _provider.CreateOpenAIHeaders(call.Headers),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(System.Text.Encoding.UTF8.GetString(raw));
        return Read(document.RootElement, warnings, timestamp);
    }

    private static OpenAIImageGeneration Read(JsonElement root, IReadOnlyList<OpenAICallWarning> warnings, DateTimeOffset timestamp)
    {
        var images = new List<GeneratedImage>();
        var metadataImages = new JsonArray();
        var count = root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array ? data.GetArrayLength() : 0;
        int? imageTokens = Nested(root, "image_tokens");
        int? textTokens = Nested(root, "text_tokens");
        if (data.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var item in data.EnumerateArray())
            {
                if (item.TryGetProperty("b64_json", out var b64) && b64.ValueKind == JsonValueKind.String)
                {
                    images.Add(new GeneratedImage("image/png", Convert.FromBase64String(b64.GetString() ?? string.Empty), null));
                }
                else if (item.TryGetProperty("url", out var url) && url.ValueKind == JsonValueKind.String)
                {
                    images.Add(new GeneratedImage("image/png", null, url.GetString()));
                }

                var entry = new JsonObject();
                if (item.TryGetProperty("revised_prompt", out var revised) && revised.ValueKind == JsonValueKind.String)
                {
                    entry["revisedPrompt"] = revised.GetString();
                }

                if (imageTokens != null)
                {
                    entry["imageTokens"] = Share(imageTokens.Value, index, count);
                }

                if (textTokens != null)
                {
                    entry["textTokens"] = Share(textTokens.Value, index, count);
                }

                metadataImages.Add(entry);
                index++;
            }
        }

        int? input = Token(root, "input_tokens");
        int? output = Token(root, "output_tokens");
        int? total = Token(root, "total_tokens");
        return new OpenAIImageGeneration(
            images,
            warnings,
            input,
            output,
            total,
            OpenAIJson.ProviderMetadata(new JsonObject { ["images"] = metadataImages }),
            timestamp);
    }

    private static int Share(int total, int index, int count)
    {
        if (count <= 0)
        {
            return total;
        }

        var baseCount = total / count;
        var remainder = total - (baseCount * (count - 1));
        return index == count - 1 ? remainder : baseCount;
    }

    private static int? Token(JsonElement root, string name)
    {
        if (!root.TryGetProperty("usage", out var usage) || usage.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return usage.TryGetProperty(name, out var value) && value.TryGetInt32(out var number) ? number : null;
    }

    private static int? Nested(JsonElement root, string name)
    {
        if (!root.TryGetProperty("usage", out var usage) || !usage.TryGetProperty("input_tokens_details", out var details))
        {
            return null;
        }

        return details.ValueKind == JsonValueKind.Object && details.TryGetProperty(name, out var value) && value.TryGetInt32(out var number)
            ? number
            : null;
    }

    private static void Copy(JsonObject body, string name, string? value)
    {
        if (value != null)
        {
            body[name] = value;
        }
    }

    private static void Add(MultipartFormDataContent content, string name, string? value)
    {
        if (value != null)
        {
            content.Add(new StringContent(value), name);
        }
    }
}
