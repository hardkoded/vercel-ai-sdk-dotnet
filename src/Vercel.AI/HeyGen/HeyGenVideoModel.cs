// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.HeyGen;

/// <summary>
/// HeyGen video model. <see cref="DoStartAsync"/> submits a job and <see cref="DoStatusAsync"/> reads it.
/// The operation is a JSON object with <c>videoId</c>, <c>mode</c> and <c>resolution</c>.
/// The result is the signed video URL. The bytes are not downloaded.
/// </summary>
public sealed class HeyGenVideoModel : IVideoModel, IVideoCaller
{
    private static readonly string[] Modes = { "text_to_video", "image_to_video", "reference_to_video" };
    private static readonly string[] Resolutions = { "480p", "768p", "1080p", "2k" };
    private static readonly string[] PromptEnhancements = { "turbo", "quality", "disabled" };
    private static readonly string[] AspectRatios = { "21:9", "16:9", "4:3", "1:1", "3:4", "9:16" };

    // HeyGen's named size classes do not consistently equal the short edge.
    private static readonly Dictionary<string, (string Resolution, string AspectRatio)> FrameSizes = new(StringComparer.Ordinal)
    {
        ["960x416"] = ("480p", "21:9"),
        ["832x480"] = ("480p", "16:9"),
        ["640x480"] = ("480p", "4:3"),
        ["480x480"] = ("480p", "1:1"),
        ["480x640"] = ("480p", "3:4"),
        ["480x832"] = ("480p", "9:16"),
        ["1536x672"] = ("768p", "21:9"),
        ["1344x768"] = ("768p", "16:9"),
        ["1024x768"] = ("768p", "4:3"),
        ["768x768"] = ("768p", "1:1"),
        ["768x1024"] = ("768p", "3:4"),
        ["768x1344"] = ("768p", "9:16"),
        ["1890x1080"] = ("1080p", "16:9"),
        ["1080x1890"] = ("1080p", "9:16"),
        ["2688x1536"] = ("2k", "16:9"),
        ["1536x2688"] = ("2k", "9:16"),
    };

    private readonly HeyGenProvider _provider;
    private readonly Func<DateTimeOffset> _clock;

    /// <summary>Creates a model. <paramref name="clock"/> sets the response timestamp.</summary>
    public HeyGenVideoModel(HeyGenProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public string Provider => "heygen.video";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>HeyGen creates one video per call.</summary>
    public int? MaxVideosPerCall => 1;

    /// <inheritdoc />
    public bool CanGenerate => false;

    /// <inheritdoc />
    public bool CanStart => true;

    /// <inheritdoc />
    public bool CanStatus => true;

    /// <summary>False. HeyGen is polled.</summary>
    public bool CanWebhook => false;

    /// <inheritdoc />
    public Task<int?> ResolveMaxVideosPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxVideosPerCall);
    }

    /// <summary>Starts a job and polls it until the video is ready.</summary>
    public async Task<VideoResult> DoGenerateAsync(VideoCallOptions options, CancellationToken cancellationToken)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = this, Prompt = new VideoPrompt(options.Prompt), Duration = options.DurationSeconds }, cancellationToken).ConfigureAwait(false);
        return new VideoResult(null, result.Video.Data, result.Video.MediaType);
    }

    /// <inheritdoc />
    public Task<VideoModelResult> DoGenerateAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("HeyGen generates videos with doStart and doStatus.");
    }

    /// <inheritdoc />
    public Task<VideoWebhook> HandleWebhookAsync(Func<Task<VideoWebhook>> webhook, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("HeyGen does not handle webhooks.");
    }

    /// <inheritdoc />
    public async Task<VideoStartResult> DoStartAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var timestamp = _clock();
        var warnings = new List<OperationWarning>();
        var options = ReadOptions(call.ProviderOptions);
        var (body, operation) = Build(call, options, warnings);

        var response = await SendAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "v3/models/videos"),
            ProviderExchange.Json(body.ToJsonString()),
            call,
            cancellationToken).ConfigureAwait(false);
        string? videoId = null;
        try
        {
            using var document = JsonDocument.Parse(response.Body);
            videoId = Object(document.RootElement, "data") is { } data ? String(data, "video_id") : null;
        }
        catch (JsonException)
        {
        }

        if (string.IsNullOrEmpty(videoId))
        {
            throw new ApiException("Invalid JSON response", 200, response.Body);
        }

        operation["videoId"] = videoId;
        var ordered = new JsonObject { ["videoId"] = videoId, ["mode"] = operation["mode"]!.DeepClone(), ["resolution"] = operation["resolution"]!.DeepClone() };
        return new VideoStartResult(
            OperationJson.Parse(ordered.ToJsonString()),
            warnings,
            OperationJson.Parse(new JsonObject { ["heygen"] = ordered.DeepClone() }.ToJsonString()),
            Response(response, timestamp));
    }

    /// <inheritdoc />
    public async Task<VideoStatusResult> DoStatusAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var operation = ReadOperation(call.Operation);
        var timestamp = _clock();
        var response = await SendAsync(
            HttpMethod.Get,
            ApiKeys.Combine(_provider.Options.BaseUrl, "v3/models/videos/" + Uri.EscapeDataString(operation["videoId"]!.GetValue<string>())),
            null,
            call,
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        var data = Object(document.RootElement, "data")
            ?? throw new InvalidResponseDataException(response.Body, "HeyGen returned an unexpected status response.");
        var status = String(data, "status")
            ?? throw new InvalidResponseDataException(response.Body, "HeyGen returned an unexpected status response.");

        var metadata = (JsonObject)operation.DeepClone();
        CopyNumber(data, "duration", metadata, "duration");
        CopyNumber(data, "width", metadata, "width");
        CopyNumber(data, "height", metadata, "height");
        if (String(data, "aspect_ratio") is { } aspectRatio)
        {
            metadata["aspectRatio"] = aspectRatio;
        }

        CopyNumber(data, "seed", metadata, "seed");
        if (Object(data, "timings") is { } timings && timings.TryGetProperty("inference", out var inference) && inference.ValueKind == JsonValueKind.Number)
        {
            metadata["timings"] = new JsonObject { ["inference"] = JsonNode.Parse(inference.GetRawText()) };
        }

        var failureCode = String(data, "failure_code");
        if (failureCode != null)
        {
            metadata["failureCode"] = failureCode;
        }

        var providerResponse = Response(response, timestamp);
        var providerMetadata = OperationJson.Parse(new JsonObject { ["heygen"] = metadata }.ToJsonString());
        if (status is "pending" or "processing")
        {
            return new VideoStatusResult("pending", providerMetadata: providerMetadata, response: providerResponse);
        }

        if (status is "failed" or "cancelled")
        {
            var message = String(data, "failure_message") ?? "HeyGen video generation " + status;
            return new VideoStatusResult("error", error: message + (failureCode != null ? " (" + failureCode + ")" : string.Empty), providerMetadata: providerMetadata, response: providerResponse);
        }

        var videoUrl = String(data, "video_url");
        if (status != "completed" || string.IsNullOrEmpty(videoUrl))
        {
            throw new InvalidResponseDataException(response.Body, "HeyGen returned an unexpected video status or a completed video without a URL.");
        }

        return new VideoStatusResult(
            "completed",
            new[] { new VideoOutput("url", videoUrl, mediaType: "video/mp4") },
            warnings: Array.Empty<OperationWarning>(),
            providerMetadata: OperationJson.Parse(new JsonObject { ["heygen"] = new JsonObject { ["videos"] = new JsonArray(metadata.DeepClone()) } }.ToJsonString()),
            response: providerResponse);
    }

    // HeyGen reports errors as { error: { code, message, param } }.
    private async Task<ProviderExchangeResult> SendAsync(HttpMethod method, Uri uri, HttpContent? content, VideoModelCall call, CancellationToken cancellationToken)
    {
        try
        {
            return await ProviderExchange.SendAsync(_provider._httpClient, method, uri, content, ProviderExchange.Merge(_provider.CreateHeaders(), call.Headers), cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException exception) when (ErrorMessage(exception.ResponseBody) is { } message)
        {
            throw ProviderHttp.MapStatus(exception.StatusCode, message, exception.ResponseBody);
        }
    }

    private static string? ErrorMessage(string? body)
    {
        try
        {
            using var document = JsonDocument.Parse(body ?? string.Empty);
            if (Object(document.RootElement, "error") is not { } error || String(error, "message") is not { } message)
            {
                return null;
            }

            return message
                + (String(error, "code") is { } code ? " (" + code + ")" : string.Empty)
                + (String(error, "param") is { } param ? " [" + param + "]" : string.Empty);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private (JsonObject Body, JsonObject Operation) Build(VideoModelCall call, Options options, List<OperationWarning> warnings)
    {
        if (string.IsNullOrEmpty(call.Prompt) || call.Prompt.Length > 32000)
        {
            throw new InvalidArgumentException("prompt", call.Prompt, "HeyGen requires a prompt between 1 and 32,000 characters.");
        }

        if (call.Duration is { } duration && (duration != Math.Floor(duration) || duration < 5 || duration > 15))
        {
            throw new InvalidArgumentException("duration", duration, "HeyGen duration must be an integer between 5 and 15 seconds.");
        }

        if (call.Seed is < 0)
        {
            throw new InvalidArgumentException("seed", call.Seed, "HeyGen seed must be an unsigned 32-bit integer.");
        }

        if (call.N > 1)
        {
            warnings.Add(OperationWarning.Unsupported("n", "HeyGen generates one video per call."));
        }

        if (call.Fps != null && call.Fps != 24)
        {
            warnings.Add(OperationWarning.Unsupported("fps", "HeyGen generates video at 24 fps."));
        }

        if (call.GenerateAudio == false)
        {
            warnings.Add(OperationWarning.Unsupported("generateAudio", "HeyGen always generates audio alongside the video."));
        }

        var frames = call.FrameImages ?? Array.Empty<VideoFrameFile>();
        if (frames.Any(frame => frame.FrameType != "first_frame") || frames.Count > 1)
        {
            throw new InvalidArgumentException("frameImages", null, "HeyGen supports a single first frame, not last-frame conditioning.");
        }

        if (frames.Count > 0 && call.Image != null)
        {
            throw new InvalidArgumentException("image", null, "Supply either image or a first frame, not both.");
        }

        var firstFrame = frames.Count > 0 ? frames[0].Image : call.Image;
        if (firstFrame != null && options.Image != null)
        {
            throw new InvalidArgumentException("image", null, "Supply either a standard image or providerOptions.heygen.image, not both.");
        }

        var image = firstFrame != null ? ToAsset(firstFrame, "image") : options.Image;
        var referenceImages = new List<Asset>();
        var referenceVideos = new List<Asset>();
        var referenceAudio = new List<Asset>();
        foreach (var reference in call.InputReferences ?? Array.Empty<VideoModelFile>())
        {
            var mediaType = reference.MediaType;
            if (mediaType != null && mediaType.StartsWith("video/", StringComparison.Ordinal))
            {
                referenceVideos.Add(ToAsset(reference, "video"));
            }
            else if (mediaType != null && mediaType.StartsWith("audio/", StringComparison.Ordinal))
            {
                referenceAudio.Add(ToAsset(reference, "audio"));
            }
            else if (mediaType != null && mediaType.StartsWith("image/", StringComparison.Ordinal))
            {
                referenceImages.Add(ToAsset(reference, "image"));
            }
            else
            {
                throw new InvalidArgumentException("inputReferences", null, "HeyGen references require an image, video, or audio mediaType.");
            }
        }

        referenceImages.AddRange(options.ReferenceImages);
        referenceVideos.AddRange(options.ReferenceVideos);
        referenceAudio.AddRange(options.ReferenceAudio);
        var referenceCount = referenceImages.Count + referenceVideos.Count + referenceAudio.Count;
        if (referenceImages.Count > 9 || referenceVideos.Count > 3 || referenceAudio.Count > 3 || referenceCount > 12)
        {
            throw new InvalidArgumentException("inputReferences", null, "HeyGen accepts at most 9 images, 3 videos, 3 audio references, and 12 references in total.");
        }

        var mode = options.Mode ?? (image != null ? "image_to_video" : referenceCount > 0 ? "reference_to_video" : "text_to_video");
        if ((mode == "image_to_video" && image == null) || (mode != "image_to_video" && image != null))
        {
            throw new InvalidArgumentException("image", null, "A first-frame image is required for image_to_video and cannot be used in other modes.");
        }

        if ((mode != "reference_to_video" && referenceCount > 0) || (mode == "reference_to_video" && referenceImages.Count + referenceVideos.Count == 0))
        {
            throw new InvalidArgumentException("inputReferences", null, "References are only supported in reference_to_video, which requires at least one image or video reference.");
        }

        (string Resolution, string AspectRatio)? frameSize = null;
        if (call.Resolution != null && options.Resolution == null)
        {
            if (!FrameSizes.TryGetValue(call.Resolution, out var size))
            {
                throw new InvalidArgumentException("resolution", call.Resolution, "Unsupported HeyGen frame size. Use a documented frame size or providerOptions.heygen.resolution to select 480p, 768p, 1080p, or 2k.");
            }

            frameSize = size;
        }

        var resolution = options.Resolution ?? frameSize?.Resolution ?? "768p";
        var aspectRatio = call.AspectRatio ?? frameSize?.AspectRatio ?? (mode == "text_to_video" ? "16:9" : "adaptive");
        if (mode == "image_to_video")
        {
            if (call.AspectRatio != null || frameSize != null)
            {
                warnings.Add(OperationWarning.Unsupported("aspectRatio", "HeyGen image-to-video follows the first frame aspect ratio; only the resolution size class is applied."));
            }
        }
        else
        {
            if (!AspectRatios.Contains(aspectRatio) && !(mode == "reference_to_video" && aspectRatio == "adaptive"))
            {
                throw new InvalidArgumentException("aspectRatio", aspectRatio, "Unsupported HeyGen aspect ratio for " + mode + ": " + aspectRatio + ".");
            }

            if ((resolution == "1080p" || resolution == "2k") && aspectRatio != "16:9" && aspectRatio != "9:16")
            {
                throw new InvalidArgumentException("aspectRatio", aspectRatio, "HeyGen 1080p and 2k require an explicit 16:9 or 9:16 aspect ratio for text-to-video and reference-to-video.");
            }
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["mode"] = mode,
            ["prompt"] = call.Prompt,
            ["duration"] = call.Duration != null ? (int)call.Duration.Value : 5,
            ["resolution"] = resolution,
        };
        if (mode != "image_to_video")
        {
            body["aspect_ratio"] = aspectRatio;
        }

        if (call.Seed != null)
        {
            body["seed"] = call.Seed;
        }

        if (options.PromptEnhancement != null)
        {
            body["prompt_enhancement"] = options.PromptEnhancement;
        }

        if (image != null)
        {
            body["image"] = AssetBody(image, "image");
        }

        AddAssets(body, "reference_images", referenceImages, "image");
        AddAssets(body, "reference_videos", referenceVideos, "video");
        AddAssets(body, "reference_audio", referenceAudio, "audio");
        return (body, new JsonObject { ["mode"] = mode, ["resolution"] = resolution });
    }

    private static void AddAssets(JsonObject body, string field, List<Asset> assets, string kind)
    {
        if (assets.Count > 0)
        {
            body[field] = new JsonArray(assets.Select(asset => (JsonNode?)AssetBody(asset, kind)).ToArray());
        }
    }

    private static Asset ToAsset(VideoModelFile file, string kind)
    {
        if (file.MediaType != null && !file.MediaType.StartsWith(kind + "/", StringComparison.Ordinal))
        {
            throw new InvalidArgumentException(kind, null, "Expected a " + kind + " mediaType.");
        }

        return file.Type == "url"
            ? new Asset("url") { Url = file.Url }
            : new Asset("base64") { MediaType = file.MediaType, Data = Convert.ToBase64String(file.Data ?? Array.Empty<byte>()) };
    }

    private static JsonObject AssetBody(Asset asset, string kind)
    {
        switch (asset.Type)
        {
            case "url":
                if (!Uri.TryCreate(asset.Url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                {
                    throw new InvalidArgumentException(kind, asset.Url, "HeyGen input URLs must be absolute HTTPS URLs.");
                }

                return new JsonObject { ["type"] = "url", ["url"] = asset.Url };
            case "asset_id":
                return new JsonObject { ["type"] = "asset_id", ["asset_id"] = asset.AssetId };
            default:
                if (asset.MediaType == null || !asset.MediaType.StartsWith(kind + "/", StringComparison.Ordinal))
                {
                    throw new InvalidArgumentException(kind, null, "Expected a " + kind + " mediaType.");
                }

                return new JsonObject { ["type"] = "base64", ["media_type"] = asset.MediaType, ["data"] = asset.Data };
        }
    }

    private static Options ReadOptions(JsonElement providerOptions)
    {
        var options = new Options();
        if (providerOptions.ValueKind != JsonValueKind.Object
            || !providerOptions.TryGetProperty(HeyGenProvider.ProviderId, out var heygen)
            || heygen.ValueKind == JsonValueKind.Null)
        {
            return options;
        }

        if (heygen.ValueKind != JsonValueKind.Object)
        {
            throw Invalid("heygen", "expected an object.");
        }

        options.Mode = Enum(heygen, "mode", Modes);
        options.Resolution = Enum(heygen, "resolution", Resolutions);
        options.PromptEnhancement = Enum(heygen, "promptEnhancement", PromptEnhancements);
        if (heygen.TryGetProperty("image", out var image) && image.ValueKind != JsonValueKind.Null)
        {
            options.Image = ReadAsset(image, "image");
        }

        options.ReferenceImages = Assets(heygen, "referenceImages", 9);
        options.ReferenceVideos = Assets(heygen, "referenceVideos", 3);
        options.ReferenceAudio = Assets(heygen, "referenceAudio", 3);
        return options;
    }

    private static List<Asset> Assets(JsonElement heygen, string name, int max)
    {
        if (!heygen.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return new List<Asset>();
        }

        if (value.ValueKind != JsonValueKind.Array || value.GetArrayLength() > max)
        {
            throw Invalid(name, "expected an array of at most " + max + " assets.");
        }

        return value.EnumerateArray().Select(item => ReadAsset(item, name)).ToList();
    }

    private static Asset ReadAsset(JsonElement element, string name)
    {
        var type = String(element, "type");
        switch (type)
        {
            case "url" when String(element, "url") is { } url && url.StartsWith("https://", StringComparison.Ordinal):
                return new Asset("url") { Url = url };
            case "asset_id" when !string.IsNullOrEmpty(String(element, "assetId")):
                return new Asset("asset_id") { AssetId = String(element, "assetId") };
            case "base64" when !string.IsNullOrEmpty(String(element, "mediaType")) && !string.IsNullOrEmpty(String(element, "data")):
                return new Asset("base64") { MediaType = String(element, "mediaType"), Data = String(element, "data") };
            default:
                throw Invalid(name, "expected a url, asset_id, or base64 asset.");
        }
    }

    private static string? Enum(JsonElement heygen, string name, string[] allowed)
    {
        if (!heygen.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var text = value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        if (text == null || !allowed.Contains(text))
        {
            throw Invalid(name, "expected one of " + string.Join(", ", allowed) + ".");
        }

        return text;
    }

    private static InvalidArgumentException Invalid(string name, string message)
    {
        return new InvalidArgumentException("providerOptions.heygen." + name, null, "Invalid HeyGen provider options: " + message);
    }

    private static JsonObject ReadOperation(object? value)
    {
        if (value is JsonElement { ValueKind: JsonValueKind.Object } element
            && String(element, "videoId") is { Length: > 0 } videoId
            && String(element, "mode") is { } mode && Modes.Contains(mode)
            && String(element, "resolution") is { } resolution && Resolutions.Contains(resolution))
        {
            return new JsonObject { ["videoId"] = videoId, ["mode"] = mode, ["resolution"] = resolution };
        }

        throw new InvalidArgumentException("operation", value, "HeyGen operations are objects with a videoId, mode, and resolution.");
    }

    private ProviderResponse Response(ProviderExchangeResult response, DateTimeOffset timestamp)
    {
        return new ProviderResponse(response.Headers, timestamp: timestamp.UtcDateTime, modelId: ModelId);
    }

    private static void CopyNumber(JsonElement data, string name, JsonObject target, string field)
    {
        if (data.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number)
        {
            target[field] = JsonNode.Parse(value.GetRawText());
        }
    }

    private static JsonElement? Object(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object ? value : null;
    }

    private static string? String(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private sealed class Asset
    {
        public Asset(string type)
        {
            Type = type;
        }

        public string Type { get; }
        public string? Url { get; set; }
        public string? AssetId { get; set; }
        public string? MediaType { get; set; }
        public string? Data { get; set; }
    }

    private sealed class Options
    {
        public string? Mode { get; set; }
        public string? Resolution { get; set; }
        public string? PromptEnhancement { get; set; }
        public Asset? Image { get; set; }
        public List<Asset> ReferenceImages { get; set; } = new();
        public List<Asset> ReferenceVideos { get; set; } = new();
        public List<Asset> ReferenceAudio { get; set; } = new();
    }
}
