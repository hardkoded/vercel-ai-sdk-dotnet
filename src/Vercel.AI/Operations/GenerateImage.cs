// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Operations;

/// <summary>An image sent to <c>doGenerate</c> as a file or URL.</summary>
public sealed class ImageModelFile
{
    /// <summary>Creates a URL image.</summary>
    public static ImageModelFile FromUrl(string url)
    {
        return new ImageModelFile("url", url, null, null);
    }

    /// <summary>Creates a binary image.</summary>
    public static ImageModelFile FromFile(byte[] data, string mediaType)
    {
        return new ImageModelFile("file", null, data ?? Array.Empty<byte>(), mediaType);
    }

    private ImageModelFile(string type, string? url, byte[]? data, string? mediaType)
    {
        Type = type;
        Url = url;
        Data = data;
        MediaType = mediaType;
    }

    /// <summary><c>url</c> or <c>file</c>.</summary>
    public string Type { get; }

    /// <summary>URL when <see cref="Type"/> is <c>url</c>.</summary>
    public string? Url { get; }

    /// <summary>Bytes when <see cref="Type"/> is <c>file</c>.</summary>
    public byte[]? Data { get; }

    /// <summary>Media type of a file image.</summary>
    public string? MediaType { get; }
}

/// <summary>Arguments for one image model call.</summary>
public sealed class ImageModelCall
{
    /// <summary>Creates a call.</summary>
    public ImageModelCall(string? prompt, IReadOnlyList<ImageModelFile>? files, ImageModelFile? mask, int n, string? size, string? aspectRatio, int? seed, JsonElement? providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
    {
        Prompt = prompt;
        Files = files;
        Mask = mask;
        N = n;
        Size = size;
        AspectRatio = aspectRatio;
        Seed = seed;
        ProviderOptions = providerOptions;
        Headers = headers;
        CancellationToken = cancellationToken;
    }

    /// <summary>Prompt text.</summary>
    public string? Prompt { get; }

    /// <summary>Reference images.</summary>
    public IReadOnlyList<ImageModelFile>? Files { get; }

    /// <summary>Mask image.</summary>
    public ImageModelFile? Mask { get; }

    /// <summary>Images requested from this call.</summary>
    public int N { get; }

    /// <summary>Size such as <c>1024x1024</c>.</summary>
    public string? Size { get; }

    /// <summary>Aspect ratio such as <c>16:9</c>.</summary>
    public string? AspectRatio { get; }

    /// <summary>Seed.</summary>
    public int? Seed { get; }

    /// <summary>Provider options. Empty object when the caller omitted them.</summary>
    public JsonElement? ProviderOptions { get; }

    /// <summary>Headers including the user-agent suffix.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Cancellation token.</summary>
    public CancellationToken CancellationToken { get; }
}

/// <summary>One <c>doGenerate</c> result.</summary>
public sealed class ImageModelResult
{
    /// <summary>Creates a result.</summary>
    public ImageModelResult(IReadOnlyList<object?> images, IReadOnlyList<OperationWarning>? warnings = null, OperationUsage? usage = null, JsonElement? providerMetadata = null, ProviderResponse? response = null, bool? isRetryable = null)
    {
        Images = images ?? Array.Empty<object?>();
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        Usage = usage;
        ProviderMetadata = providerMetadata;
        Response = response ?? new ProviderResponse();
        IsRetryable = isRetryable;
    }

    /// <summary>Image payloads: base64 strings or byte arrays.</summary>
    public IReadOnlyList<object?> Images { get; }

    /// <summary>Warnings from this call.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Token usage. Null when the provider omitted it.</summary>
    public OperationUsage? Usage { get; }

    /// <summary>Provider metadata, including per-image arrays.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata.</summary>
    public ProviderResponse Response { get; }

    /// <summary>When false, an empty image list is terminal.</summary>
    public bool? IsRetryable { get; }
}

/// <summary>Image model used by <see cref="GenerateImage"/>.</summary>
public interface IImageCaller
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Maximum images in one call. Null uses 1.</summary>
    int? MaxImagesPerCall { get; }

    /// <summary>Resolves a functional per-call limit. The default returns <see cref="MaxImagesPerCall"/>.</summary>
    Task<int?> ResolveMaxImagesPerCallAsync(CancellationToken cancellationToken);

    /// <summary>Generates images.</summary>
    Task<ImageModelResult> DoGenerateAsync(ImageModelCall call, CancellationToken cancellationToken);
}

/// <summary>A generated image file.</summary>
public sealed class GeneratedImage
{
    /// <summary>Creates a generated image.</summary>
    public GeneratedImage(byte[] data, string mediaType, JsonElement? providerMetadata = null)
    {
        Data = data ?? Array.Empty<byte>();
        MediaType = mediaType ?? "image/png";
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Image bytes.</summary>
    public byte[] Data { get; }

    /// <summary>Base64 form of <see cref="Data"/>.</summary>
    public string Base64
    {
        get { return Convert.ToBase64String(Data); }
    }

    /// <summary>Detected or fallback media type.</summary>
    public string MediaType { get; }

    /// <summary>Per-image provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>One provider call preserved on the result.</summary>
public sealed class ImageCallResult
{
    /// <summary>Creates a call record.</summary>
    public ImageCallResult(IReadOnlyList<GeneratedImage> images, JsonElement? providerMetadata, ProviderResponse response, IReadOnlyList<OperationWarning> warnings, OperationUsage? usage)
    {
        Images = images ?? Array.Empty<GeneratedImage>();
        ProviderMetadata = providerMetadata;
        Response = response;
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        Usage = usage;
    }

    /// <summary>Images from this call.</summary>
    public IReadOnlyList<GeneratedImage> Images { get; }

    /// <summary>Provider metadata from this call.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata.</summary>
    public ProviderResponse Response { get; }

    /// <summary>Warnings from this call.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Usage from this call.</summary>
    public OperationUsage? Usage { get; }
}

/// <summary>No image was produced after retries.</summary>
public sealed class NoImageGeneratedException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public NoImageGeneratedException(IReadOnlyList<ImageCallResult> calls, IReadOnlyList<ProviderResponse> responses)
        : base("No image generated.")
    {
        Calls = calls ?? Array.Empty<ImageCallResult>();
        Responses = responses ?? Array.Empty<ProviderResponse>();
    }

    /// <summary>Calls that returned no usable image.</summary>
    public IReadOnlyList<ImageCallResult> Calls { get; }

    /// <summary>Provider responses included with the failure.</summary>
    public IReadOnlyList<ProviderResponse> Responses { get; }
}

/// <summary>Result of <see cref="GenerateImage.GenerateImageAsync"/>.</summary>
public sealed class GenerateImageResult
{
    /// <summary>Creates a result.</summary>
    public GenerateImageResult(IReadOnlyList<GeneratedImage> images, IReadOnlyList<ImageCallResult> calls, IReadOnlyList<OperationWarning> warnings, IReadOnlyList<ProviderResponse> responses, JsonElement providerMetadata, OperationUsage usage)
    {
        Images = images ?? Array.Empty<GeneratedImage>();
        Calls = calls ?? Array.Empty<ImageCallResult>();
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        Responses = responses ?? Array.Empty<ProviderResponse>();
        ProviderMetadata = providerMetadata;
        Usage = usage ?? new OperationUsage();
    }

    /// <summary>First image.</summary>
    public GeneratedImage Image
    {
        get { return Images[0]; }
    }

    /// <summary>All images.</summary>
    public IReadOnlyList<GeneratedImage> Images { get; }

    /// <summary>Underlying calls, including empty retries that were later replaced.</summary>
    public IReadOnlyList<ImageCallResult> Calls { get; }

    /// <summary>Aggregated warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>One response per collected call.</summary>
    public IReadOnlyList<ProviderResponse> Responses { get; }

    /// <summary>Merged provider metadata.</summary>
    public JsonElement ProviderMetadata { get; }

    /// <summary>Summed usage. Counts stay null until a provider reports them.</summary>
    public OperationUsage Usage { get; }
}

/// <summary>A string prompt or an image edit prompt.</summary>
public sealed class ImagePrompt
{
    /// <summary>Creates a text prompt.</summary>
    public ImagePrompt(string text)
    {
        Text = text;
    }

    /// <summary>Creates an edit prompt.</summary>
    public ImagePrompt(string? text, IReadOnlyList<object?> images, object? mask = null)
    {
        Text = text;
        Images = images ?? Array.Empty<object?>();
        Mask = mask;
        HasImages = true;
    }

    /// <summary>Prompt text.</summary>
    public string? Text { get; }

    /// <summary>Reference images when this is an edit prompt.</summary>
    public IReadOnlyList<object?> Images { get; } = Array.Empty<object?>();

    /// <summary>Optional mask.</summary>
    public object? Mask { get; }

    /// <summary>True when the caller passed the object form.</summary>
    public bool HasImages { get; }
}

/// <summary>Options for <see cref="GenerateImage.GenerateImageAsync"/>.</summary>
public sealed class GenerateImageRequest : OperationRequest
{
    /// <summary>Image model.</summary>
    public IImageCaller? Model { get; set; }

    /// <summary>Text prompt. Ignored when <see cref="Prompt"/> is set.</summary>
    public string? Text { get; set; }

    /// <summary>Text or edit prompt.</summary>
    public ImagePrompt? Prompt { get; set; }

    /// <summary>Number of images. Default 1.</summary>
    public int N { get; set; } = 1;

    /// <summary>Overrides the model's maximum images per call.</summary>
    public int? MaxImagesPerCall { get; set; }

    /// <summary>Size such as <c>1024x1024</c>.</summary>
    public string? Size { get; set; }

    /// <summary>Aspect ratio such as <c>16:9</c>.</summary>
    public string? AspectRatio { get; set; }

    /// <summary>Seed.</summary>
    public int? Seed { get; set; }
}

/// <summary>Generates images. Maps to <c>generateImage</c>.</summary>
public static class GenerateImage
{
    private static readonly string[] GatewayCostKeys =
    {
        "cost", "gatewayCost", "inferenceCost", "inputInferenceCost", "marketCost", "outputInferenceCost", "surchargeCost",
    };

    /// <summary>Generates images and merges parallel calls.</summary>
    public static async Task<GenerateImageResult> GenerateImageAsync(GenerateImageRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var model = request.Model ?? throw new InvalidArgumentException("model", null, "model is required");
        var token = request.CancellationToken.CanBeCanceled ? request.CancellationToken : cancellationToken;
        var headers = OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent);
        var providerOptions = request.ProviderOptions ?? OperationJson.Parse("{}");
        var maxPerCall = request.MaxImagesPerCall ?? await model.ResolveMaxImagesPerCallAsync(token).ConfigureAwait(false) ?? 1;
        if (maxPerCall < 1)
        {
            maxPerCall = 1;
        }

        var counts = CallCounts(request.N, maxPerCall);
        var groups = await Task.WhenAll(counts.Select(count => GenerateCallAsync(model, request, count, headers, providerOptions, token)).ToArray()).ConfigureAwait(false);
        var results = new List<ImageModelResult>();
        foreach (var group in groups)
        {
            results.AddRange(group);
        }

        var images = new List<GeneratedImage>();
        var calls = new List<ImageCallResult>();
        var warnings = new List<OperationWarning>();
        var responses = new List<ProviderResponse>();
        var metadata = new JsonObject();
        var usage = new OperationUsage();
        foreach (var result in results)
        {
            var callImages = new List<GeneratedImage>();
            for (var index = 0; index < result.Images.Count; index++)
            {
                var bytes = MediaTypeDetector.ToBytes(result.Images[index]);
                callImages.Add(new GeneratedImage(bytes, MediaTypeDetector.Detect(bytes, "image") ?? "image/png", ImageMetadata(result.ProviderMetadata, index)));
            }

            images.AddRange(callImages);
            calls.Add(new ImageCallResult(callImages, result.ProviderMetadata, result.Response, result.Warnings, result.Usage));
            warnings.AddRange(result.Warnings);
            if (result.Usage != null)
            {
                usage = OperationUsage.AddImage(usage, result.Usage);
            }

            MergeMetadata(metadata, result.ProviderMetadata);
            responses.Add(result.Response);
        }

        WarningLog.Write(warnings, model.Provider, model.ModelId);
        if (images.Count == 0)
        {
            throw new NoImageGeneratedException(calls, responses);
        }

        return new GenerateImageResult(images, calls, warnings, responses, OperationJson.Parse(metadata.ToJsonString()), usage);
    }

    /// <summary>Normalizes a prompt the way <c>normalizePrompt</c> does.</summary>
    public static ImagePromptParts NormalizePrompt(ImagePrompt? prompt, string? text)
    {
        if (prompt == null || !prompt.HasImages)
        {
            return new ImagePromptParts(prompt?.Text ?? text, null, null);
        }

        return new ImagePromptParts(prompt.Text, prompt.Images.Select(ToFile).ToArray(), prompt.Mask == null ? null : ToFile(prompt.Mask));
    }

    private static async Task<IReadOnlyList<ImageModelResult>> GenerateCallAsync(IImageCaller model, GenerateImageRequest request, int count, IReadOnlyDictionary<string, string> headers, JsonElement providerOptions, CancellationToken cancellationToken)
    {
        var collected = new List<ImageModelResult>();
        try
        {
            await OperationRetry.ExecuteAsync(request.MaxRetries, cancellationToken, request.AbortReason, async ct =>
            {
                var parts = NormalizePrompt(request.Prompt, request.Text);
                var result = await model.DoGenerateAsync(new ImageModelCall(parts.Prompt, parts.Files, parts.Mask, count, request.Size, request.AspectRatio, request.Seed, providerOptions, headers, ct), ct).ConfigureAwait(false);
                collected.Add(result);
                if (result.Images.Count == 0 && result.IsRetryable != false)
                {
                    throw new RetryableNoImageException();
                }

                return result;
            }, error => error is RetryableNoImageException).ConfigureAwait(false);
            return collected;
        }
        catch (Exception error)
        {
            if (error is RetryableNoImageException || (error is OperationRetryException retry && retry.LastError is RetryableNoImageException))
            {
                return collected;
            }

            throw;
        }
    }

    private static List<int> CallCounts(int n, int maxPerCall)
    {
        var count = (int)Math.Ceiling(n / (double)maxPerCall);
        var counts = new List<int>(count);
        for (var i = 0; i < count; i++)
        {
            if (i < count - 1)
            {
                counts.Add(maxPerCall);
            }
            else
            {
                var remainder = n % maxPerCall;
                counts.Add(remainder == 0 ? maxPerCall : remainder);
            }
        }

        return counts;
    }

    private static ImageModelFile ToFile(object? data)
    {
        if (data is string text && text.StartsWith("http", StringComparison.Ordinal))
        {
            return ImageModelFile.FromUrl(text);
        }

        if (data is string dataUrl && dataUrl.StartsWith("data:", StringComparison.Ordinal))
        {
            var split = SplitDataUrl(dataUrl);
            var bytes = split.Base64 == null ? Array.Empty<byte>() : Convert.FromBase64String(split.Base64);
            return ImageModelFile.FromFile(bytes, string.IsNullOrEmpty(split.MediaType) ? MediaTypeDetector.Detect(bytes, "image") ?? "image/png" : split.MediaType!);
        }

        var payload = MediaTypeDetector.ToBytes(data);
        return ImageModelFile.FromFile(payload, MediaTypeDetector.Detect(payload, "image") ?? "image/png");
    }

    private static (string? MediaType, string? Base64) SplitDataUrl(string dataUrl)
    {
        var comma = dataUrl.IndexOf(',');
        var header = comma < 0 ? dataUrl : dataUrl.Substring(0, comma);
        var payload = comma < 0 ? null : dataUrl.Substring(comma + 1);
        var media = header.Substring("data:".Length);
        var separator = media.IndexOf(';');
        if (separator >= 0)
        {
            media = media.Substring(0, separator);
        }

        return (media.Length == 0 ? null : media, payload);
    }

    private static JsonElement? ImageMetadata(JsonElement? providerMetadata, int index)
    {
        if (providerMetadata is not JsonElement root || root.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var collected = new JsonObject();
        foreach (var provider in root.EnumerateObject())
        {
            if (!provider.Value.TryGetProperty("images", out var images) || images.ValueKind != JsonValueKind.Array || index >= images.GetArrayLength())
            {
                continue;
            }

            var value = images[index];
            if (value.ValueKind == JsonValueKind.Object)
            {
                collected[provider.Name] = JsonNode.Parse(value.GetRawText());
            }
        }

        return collected.Count == 0 ? null : OperationJson.Parse(collected.ToJsonString());
    }

    private static void MergeMetadata(JsonObject target, JsonElement? source)
    {
        if (source is not JsonElement root || root.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var provider in root.EnumerateObject())
        {
            var incoming = JsonNode.Parse(provider.Value.GetRawText()) as JsonObject ?? new JsonObject();
            if (provider.Name == "gateway")
            {
                JsonObject merged;
                if (target["gateway"] is JsonObject current)
                {
                    merged = new JsonObject();
                    CopyInto(merged, current);
                    CopyInto(merged, incoming);
                    foreach (var key in GatewayCostKeys)
                    {
                        var sum = AddDecimalStrings(ReadString(current[key]), ReadString(incoming[key]));
                        if (sum != null)
                        {
                            merged[key] = sum;
                        }
                    }
                }
                else
                {
                    merged = new JsonObject();
                    CopyInto(merged, incoming);
                }

                if (merged["images"] is JsonArray images && images.Count == 0)
                {
                    merged.Remove("images");
                }

                target["gateway"] = merged;
            }
            else
            {
                if (target[provider.Name] is not JsonObject existing)
                {
                    existing = new JsonObject { ["images"] = new JsonArray() };
                    target[provider.Name] = existing;
                }

                var images = existing["images"] as JsonArray ?? new JsonArray();
                existing["images"] = images;
                if (incoming["images"] is JsonArray more)
                {
                    foreach (var item in more)
                    {
                        images.Add(item?.DeepClone());
                    }
                }
            }
        }
    }

    private static void CopyInto(JsonObject target, JsonObject source)
    {
        foreach (var pair in source)
        {
            target[pair.Key] = pair.Value?.DeepClone();
        }
    }

    private static string? ReadString(JsonNode? node)
    {
        return node is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;
    }

    private static string? AddDecimalStrings(string? left, string? right)
    {
        if (left == null || right == null || !IsDecimal(left) || !IsDecimal(right))
        {
            return null;
        }

        var leftParts = left.Split('.');
        var rightParts = right.Split('.');
        var fraction1 = leftParts.Length > 1 ? leftParts[1] : string.Empty;
        var fraction2 = rightParts.Length > 1 ? rightParts[1] : string.Empty;
        var precision = Math.Max(fraction1.Length, fraction2.Length);
        var sum = BigInteger.Parse(leftParts[0] + fraction1.PadRight(precision, '0'), CultureInfo.InvariantCulture)
            + BigInteger.Parse(rightParts[0] + fraction2.PadRight(precision, '0'), CultureInfo.InvariantCulture);
        var text = sum.ToString(CultureInfo.InvariantCulture).PadLeft(precision + 1, '0');
        if (precision == 0)
        {
            return text;
        }

        return TrimDecimal(text.Substring(0, text.Length - precision) + "." + text.Substring(text.Length - precision));
    }

    private static bool IsDecimal(string value)
    {
        var dot = false;
        if (value.Length == 0)
        {
            return false;
        }

        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];
            if (character == '.' && !dot && i > 0 && i < value.Length - 1)
            {
                dot = true;
                continue;
            }

            if (character < '0' || character > '9')
            {
                return false;
            }
        }

        return true;
    }

    private static string TrimDecimal(string value)
    {
        var end = value.Length;
        while (end > 0 && value[end - 1] == '0')
        {
            end--;
        }

        if (end > 0 && value[end - 1] == '.')
        {
            end--;
        }

        return end == 0 ? "0" : value.Substring(0, end);
    }

    private sealed class RetryableNoImageException : Exception
    {
        public RetryableNoImageException()
            : base("No image generated.")
        {
        }
    }
}

/// <summary>Normalized image prompt fields.</summary>
public sealed class ImagePromptParts
{
    /// <summary>Creates normalized prompt fields.</summary>
    public ImagePromptParts(string? prompt, IReadOnlyList<ImageModelFile>? files, ImageModelFile? mask)
    {
        Prompt = prompt;
        Files = files;
        Mask = mask;
    }

    /// <summary>Prompt text.</summary>
    public string? Prompt { get; }

    /// <summary>Reference files.</summary>
    public IReadOnlyList<ImageModelFile>? Files { get; }

    /// <summary>Mask file.</summary>
    public ImageModelFile? Mask { get; }
}
