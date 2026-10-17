// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.KlingAI;

/// <summary>
/// KlingAI video model. <see cref="DoStartAsync"/> creates a task and <see cref="DoStatusAsync"/> reads it.
/// The mode comes from the model id suffix: <c>-t2v</c>, <c>-i2v</c> or <c>-motion-control</c>.
/// The operation is a JSON object with the <c>taskId</c> and the <c>endpointPath</c>.
/// </summary>
public sealed class KlingAIVideoModel : IVideoModel, IVideoCaller
{
    // Option keys the model maps itself. Every other key is sent to the API as is.
    private static readonly HashSet<string> HandledOptions = new(StringComparer.Ordinal)
    {
        "mode", "pollIntervalMs", "pollTimeoutMs", "negativePrompt", "sound", "cfgScale", "cameraControl", "multiShot", "shotType",
        "multiPrompt", "elementList", "voiceList", "imageTail", "staticMask", "dynamicMasks", "videoUrl", "characterOrientation",
        "keepOriginalSound", "watermarkEnabled",
    };

    private static readonly Dictionary<string, string> Endpoints = new(StringComparer.Ordinal)
    {
        ["t2v"] = "/v1/videos/text2video",
        ["i2v"] = "/v1/videos/image2video",
        ["mi2v"] = "/v1/videos/multi-image2video",
        ["motion-control"] = "/v1/videos/motion-control",
    };

    private readonly KlingAIProvider _provider;
    private readonly Func<DateTimeOffset> _clock;

    /// <summary>Creates a model. <paramref name="clock"/> sets the response timestamp.</summary>
    public KlingAIVideoModel(KlingAIProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider => "klingai.video";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>KlingAI creates one video per task.</summary>
    public int? MaxVideosPerCall => 1;

    /// <inheritdoc />
    public bool CanGenerate => false;

    /// <inheritdoc />
    public bool CanStart => true;

    /// <inheritdoc />
    public bool CanStatus => true;

    /// <summary>False. Progress notifications need a protocol-aware receiver, so a webhook URL is only forwarded as <c>callback_url</c>.</summary>
    public bool CanWebhook => false;

    /// <inheritdoc />
    public Task<int?> ResolveMaxVideosPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxVideosPerCall);
    }

    /// <summary>Starts a task and polls it until the video is ready.</summary>
    public async Task<VideoResult> DoGenerateAsync(VideoCallOptions options, CancellationToken cancellationToken)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = this, Prompt = new VideoPrompt(options.Prompt), Duration = options.DurationSeconds }, cancellationToken).ConfigureAwait(false);
        return new VideoResult(null, result.Video.Data, result.Video.MediaType);
    }

    /// <inheritdoc />
    public Task<VideoModelResult> DoGenerateAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("KlingAI generates videos with doStart and doStatus.");
    }

    /// <inheritdoc />
    public Task<VideoWebhook> HandleWebhookAsync(Func<Task<VideoWebhook>> webhook, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("KlingAI forwards a webhook URL as callback_url but does not handle webhooks.");
    }

    /// <inheritdoc />
    public async Task<VideoStartResult> DoStartAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var timestamp = _clock();
        var warnings = new List<OperationWarning>();
        var mode = DetectMode(ModelId);
        var options = ReadOptions(call.ProviderOptions);

        var referenceImages = GetReferenceImages(call, warnings);
        var effectiveMode = mode == "i2v" && referenceImages != null ? "mi2v" : mode;
        JsonObject body = effectiveMode switch
        {
            "motion-control" => BuildMotionControl(call, options, warnings),
            "t2v" => BuildTextToVideo(call, options, warnings),
            "mi2v" => BuildMultiImage(call, options, referenceImages!, warnings),
            _ => BuildImageToVideo(call, options, warnings),
        };

        if (referenceImages != null && effectiveMode != "mi2v")
        {
            warnings.Add(OperationWarning.Unsupported("inputReferences", "KlingAI only supports inputReferences (reference-to-video) on image-to-video models. The reference images were ignored."));
        }

        // Progress notifications need a protocol-aware receiver, so the generic webhook hook stays off.
        // The URL is forwarded for a caller-owned receiver that filters progress notifications.
        if (call.WebhookUrl != null)
        {
            body["callback_url"] = call.WebhookUrl;
        }

        AddUniversalWarnings(call, warnings);

        var endpointPath = Endpoints[effectiveMode];
        var response = await SendAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, endpointPath),
            ProviderExchange.Json(body.ToJsonString()),
            call,
            cancellationToken).ConfigureAwait(false);

        string? taskId;
        try
        {
            using var document = JsonDocument.Parse(response.Body);
            var data = ReadEnvelope(document.RootElement, response.Body);
            taskId = data.HasValue ? String(data.Value, "task_id") : null;
            if (data.HasValue && taskId == null)
            {
                throw new ApiException("Invalid JSON response", 200, response.Body);
            }
        }
        catch (JsonException)
        {
            throw new ApiException("Invalid JSON response", 200, response.Body);
        }

        if (string.IsNullOrEmpty(taskId))
        {
            throw new AiSdkException("No task_id returned from KlingAI API. Response: " + response.Body);
        }

        var operation = new JsonObject { ["taskId"] = taskId, ["endpointPath"] = endpointPath };
        return new VideoStartResult(OperationJson.Parse(operation.ToJsonString()), warnings, response: Response(response, timestamp));
    }

    /// <inheritdoc />
    public async Task<VideoStatusResult> DoStatusAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var timestamp = _clock();
        var taskId = call.Operation is JsonElement { ValueKind: JsonValueKind.Object } operation ? String(operation, "taskId") : null;
        var endpointPath = call.Operation is JsonElement { ValueKind: JsonValueKind.Object } source ? String(source, "endpointPath") : null;
        if (string.IsNullOrEmpty(taskId) || string.IsNullOrEmpty(endpointPath) || !Endpoints.ContainsValue(endpointPath!))
        {
            throw new InvalidArgumentException("operation", call.Operation, "KlingAI operations are objects with a taskId and an endpointPath.");
        }

        var baseUrl = _provider.Options.BaseUrl;
        var response = await GetAsync(
            ApiKeys.Combine(baseUrl, endpointPath + "/" + Uri.EscapeDataString(taskId!)),
            call,
            baseUrl,
            cancellationToken).ConfigureAwait(false);

        JsonElement? data;
        try
        {
            using var document = JsonDocument.Parse(response.Body);
            data = ReadEnvelope(document.RootElement, response.Body)?.Clone();
        }
        catch (JsonException)
        {
            throw new ApiException("Invalid JSON response", 200, response.Body);
        }

        var status = data.HasValue ? String(data.Value, "task_status") : null;
        if (data.HasValue && (String(data.Value, "task_id") == null || status == null))
        {
            throw new ApiException("Invalid JSON response", 200, response.Body);
        }

        if (status == "succeed")
        {
            return Completed(data!.Value, response, taskId!, timestamp);
        }

        if (status == "failed")
        {
            return new VideoStatusResult(
                "error",
                error: "Video generation failed: " + (String(data!.Value, "task_status_msg") ?? "Unknown error"),
                response: Response(response, timestamp));
        }

        return new VideoStatusResult("pending", response: Response(response, timestamp));
    }

    private static string DetectMode(string modelId)
    {
        if (modelId.EndsWith("-t2v", StringComparison.Ordinal))
        {
            return "t2v";
        }

        if (modelId.EndsWith("-i2v", StringComparison.Ordinal))
        {
            return "i2v";
        }

        if (modelId.EndsWith("-motion-control", StringComparison.Ordinal))
        {
            return "motion-control";
        }

        throw new NoSuchModelError(modelId, "videoModel");
    }

    // 'kling-v2.6-t2v' -> 'kling-v2-6', 'kling-v3.0-t2v' -> 'kling-v3'.
    private string ApiModelName(string mode)
    {
        var suffix = mode == "motion-control" ? "-motion-control" : mode == "mi2v" ? "-i2v" : "-" + mode;
        var baseName = ModelId.Substring(0, ModelId.Length - suffix.Length);
        if (baseName.EndsWith(".0", StringComparison.Ordinal))
        {
            baseName = baseName.Substring(0, baseName.Length - 2);
        }

        return baseName.Replace('.', '-');
    }

    private JsonObject BuildTextToVideo(VideoModelCall call, Options options, List<OperationWarning> warnings)
    {
        var body = new JsonObject { ["model_name"] = ApiModelName("t2v") };
        if (call.Prompt != null)
        {
            body["prompt"] = call.Prompt;
        }

        Copy(options, "negativePrompt", body, "negative_prompt");
        SetSound(call, options, body);
        Copy(options, "cfgScale", body, "cfg_scale");
        Copy(options, "mode", body, "mode");
        Copy(options, "cameraControl", body, "camera_control");
        if (call.AspectRatio != null)
        {
            body["aspect_ratio"] = call.AspectRatio;
        }

        SetDuration(call, body);
        Copy(options, "multiShot", body, "multi_shot");
        Copy(options, "shotType", body, "shot_type");
        Copy(options, "multiPrompt", body, "multi_prompt");
        Copy(options, "voiceList", body, "voice_list");
        SetWatermark(options, body);
        if (ResolveStartImage(call, warnings) != null)
        {
            warnings.Add(OperationWarning.Unsupported("image", "KlingAI text-to-video does not support image input. Use an image-to-video model instead."));
        }

        AddPassthrough(body, options);
        return body;
    }

    private JsonObject BuildImageToVideo(VideoModelCall call, Options options, List<OperationWarning> warnings)
    {
        var body = new JsonObject { ["model_name"] = ApiModelName("i2v") };
        if (call.Prompt != null)
        {
            body["prompt"] = call.Prompt;
        }

        if (ResolveStartImage(call, warnings) is { } startImage)
        {
            body["image"] = ImageString(startImage);
        }

        // The last frame comes from frameImages first, then providerOptions.klingai.imageTail.
        if (ResolveImageTail(call, options, warnings) is { } imageTail)
        {
            body["image_tail"] = imageTail;
        }

        Copy(options, "negativePrompt", body, "negative_prompt");
        SetSound(call, options, body);
        Copy(options, "cfgScale", body, "cfg_scale");
        Copy(options, "mode", body, "mode");
        Copy(options, "cameraControl", body, "camera_control");
        Copy(options, "staticMask", body, "static_mask");
        Copy(options, "dynamicMasks", body, "dynamic_masks");
        Copy(options, "multiShot", body, "multi_shot");
        Copy(options, "shotType", body, "shot_type");
        Copy(options, "multiPrompt", body, "multi_prompt");
        Copy(options, "elementList", body, "element_list");
        Copy(options, "voiceList", body, "voice_list");
        SetWatermark(options, body);
        SetDuration(call, body);
        if (call.AspectRatio != null)
        {
            warnings.Add(OperationWarning.Unsupported("aspectRatio", "KlingAI image-to-video does not support aspectRatio. The output dimensions are determined by the input image."));
        }

        AddPassthrough(body, options);
        return body;
    }

    private JsonObject BuildMultiImage(VideoModelCall call, Options options, IReadOnlyList<VideoModelFile> referenceImages, List<OperationWarning> warnings)
    {
        var imageList = new JsonArray();
        foreach (var image in referenceImages)
        {
            imageList.Add(new JsonObject { ["image"] = ImageString(image) });
        }

        var body = new JsonObject { ["model_name"] = ApiModelName("mi2v"), ["image_list"] = imageList };
        if (call.Prompt != null)
        {
            body["prompt"] = call.Prompt;
        }

        Copy(options, "negativePrompt", body, "negative_prompt");
        Copy(options, "cfgScale", body, "cfg_scale");
        Copy(options, "mode", body, "mode");
        if (call.AspectRatio != null)
        {
            body["aspect_ratio"] = call.AspectRatio;
        }

        SetDuration(call, body);
        SetWatermark(options, body);
        if (ResolveStartImage(call, warnings) != null)
        {
            warnings.Add(OperationWarning.Unsupported("image", "KlingAI reference-to-video does not support a separate start frame. Provide all guidance images via inputReferences instead."));
        }

        if (ResolveImageTail(call, options, warnings) != null)
        {
            warnings.Add(OperationWarning.Unsupported("frameImages", "KlingAI reference-to-video does not support a last frame (image_tail). Provide all guidance images via inputReferences instead."));
        }

        AddPassthrough(body, options);
        return body;
    }

    private JsonObject BuildMotionControl(VideoModelCall call, Options options, List<OperationWarning> warnings)
    {
        if (string.IsNullOrEmpty(options.String("videoUrl")) || string.IsNullOrEmpty(options.String("characterOrientation")) || string.IsNullOrEmpty(options.String("mode")))
        {
            throw new AiSdkException("KlingAI Motion Control requires providerOptions.klingai with videoUrl, characterOrientation, and mode.");
        }

        var body = new JsonObject
        {
            ["model_name"] = ApiModelName("motion-control"),
            ["video_url"] = options.String("videoUrl"),
            ["character_orientation"] = options.String("characterOrientation"),
            ["mode"] = options.String("mode"),
        };
        if (call.Prompt != null)
        {
            body["prompt"] = call.Prompt;
        }

        if (ResolveStartImage(call, warnings) is { } startImage)
        {
            body["image_url"] = ImageString(startImage);
        }

        Copy(options, "keepOriginalSound", body, "keep_original_sound");
        SetWatermark(options, body);
        Copy(options, "elementList", body, "element_list");
        if (!string.IsNullOrEmpty(call.AspectRatio))
        {
            warnings.Add(OperationWarning.Unsupported("aspectRatio", "KlingAI Motion Control does not support aspectRatio. The output dimensions are determined by the reference image/video."));
        }

        if (call.Duration is { } duration && duration != 0)
        {
            warnings.Add(OperationWarning.Unsupported("duration", "KlingAI Motion Control does not support custom duration. The output duration matches the reference video duration."));
        }

        AddPassthrough(body, options);
        return body;
    }

    private static void AddUniversalWarnings(VideoModelCall call, List<OperationWarning> warnings)
    {
        if (!string.IsNullOrEmpty(call.Resolution))
        {
            warnings.Add(OperationWarning.Unsupported("resolution", "KlingAI video models do not support the resolution option."));
        }

        if (call.Seed is { } seed && seed != 0)
        {
            warnings.Add(OperationWarning.Unsupported("seed", "KlingAI video models do not support seed for deterministic generation."));
        }

        if (call.Fps is { } fps && fps != 0)
        {
            warnings.Add(OperationWarning.Unsupported("fps", "KlingAI video models do not support custom FPS."));
        }

        if (call.N > 1)
        {
            warnings.Add(OperationWarning.Unsupported("n", "KlingAI video models do not support generating multiple videos per call. Only 1 video will be generated."));
        }
    }

    private static void SetSound(VideoModelCall call, Options options, JsonObject body)
    {
        if (call.GenerateAudio is { } generateAudio)
        {
            body["sound"] = generateAudio ? "on" : "off";
        }
        else
        {
            Copy(options, "sound", body, "sound");
        }
    }

    private static void SetDuration(VideoModelCall call, JsonObject body)
    {
        if (call.Duration is { } duration)
        {
            body["duration"] = duration.ToString(CultureInfo.InvariantCulture);
        }
    }

    private static void SetWatermark(Options options, JsonObject body)
    {
        if (options.Get("watermarkEnabled") is { } enabled)
        {
            body["watermark_info"] = new JsonObject { ["enabled"] = JsonNode.Parse(enabled.GetRawText()) };
        }
    }

    private static void AddPassthrough(JsonObject body, Options options)
    {
        foreach (var property in options.Raw.EnumerateObject().Where(property => !HandledOptions.Contains(property.Name)))
        {
            body[property.Name] = JsonNode.Parse(property.Value.GetRawText());
        }
    }

    private static void Copy(Options options, string name, JsonObject body, string field)
    {
        if (options.Get(name) is { } value)
        {
            body[field] = JsonNode.Parse(value.GetRawText());
        }
    }

    // KlingAI takes a URL or raw base64 (no data: prefix).
    private static string ImageString(VideoModelFile file)
    {
        return file.Type == "url" ? file.Url! : Convert.ToBase64String(file.Data ?? Array.Empty<byte>());
    }

    private static bool IsVideo(VideoModelFile file)
    {
        return file.MediaType != null && MediaTypes.GetTopLevelMediaType(file.MediaType) == "video";
    }

    private static IReadOnlyList<VideoModelFile>? GetReferenceImages(VideoModelCall call, List<OperationWarning> warnings)
    {
        if (call.FrameImages is { Count: > 0 } || call.InputReferences is not { Count: > 0 } references)
        {
            return null;
        }

        var images = new List<VideoModelFile>();
        foreach (var reference in references)
        {
            if (IsVideo(reference))
            {
                warnings.Add(OperationWarning.Unsupported("inputReferences", "KlingAI does not support video reference inputs; the video reference was ignored."));
            }
            else
            {
                images.Add(reference);
            }
        }

        return images.Count > 0 ? images : null;
    }

    private static VideoModelFile? ResolveStartImage(VideoModelCall call, List<OperationWarning> warnings)
    {
        var startImage = call.FrameImages?.FirstOrDefault(frame => frame.FrameType == "first_frame")?.Image ?? call.Image;
        if (startImage != null && IsVideo(startImage))
        {
            warnings.Add(OperationWarning.Unsupported("frameImages", "KlingAI does not accept video as a frame image; it was ignored."));
            return null;
        }

        return startImage;
    }

    private static string? ResolveImageTail(VideoModelCall call, Options options, List<OperationWarning> warnings)
    {
        var lastFrame = call.FrameImages?.FirstOrDefault(frame => frame.FrameType == "last_frame")?.Image;
        if (lastFrame == null)
        {
            return options.String("imageTail");
        }

        if (IsVideo(lastFrame))
        {
            warnings.Add(OperationWarning.Unsupported("frameImages", "KlingAI does not accept video as a frame image; it was ignored."));
            return null;
        }

        return ImageString(lastFrame);
    }

    private VideoStatusResult Completed(JsonElement data, ProviderExchangeResult response, string taskId, DateTimeOffset timestamp)
    {
        var videos = data.TryGetProperty("task_result", out var result) && result.ValueKind == JsonValueKind.Object
            && result.TryGetProperty("videos", out var list) && list.ValueKind == JsonValueKind.Array
                ? list.EnumerateArray().ToList()
                : new List<JsonElement>();
        if (videos.Count == 0)
        {
            throw new AiSdkException("No videos in response. Response: " + response.Body);
        }

        var outputs = new List<VideoOutput>();
        var metadata = new JsonArray();
        foreach (var video in videos)
        {
            if (String(video, "url") is not { Length: > 0 } url)
            {
                continue;
            }

            outputs.Add(new VideoOutput("url", url, mediaType: "video/mp4"));
            var entry = new JsonObject { ["id"] = String(video, "id") ?? string.Empty, ["url"] = url };
            if (String(video, "watermark_url") is { Length: > 0 } watermarkUrl)
            {
                entry["watermarkUrl"] = watermarkUrl;
            }

            if (String(video, "duration") is { Length: > 0 } duration)
            {
                entry["duration"] = duration;
            }

            metadata.Add(entry);
        }

        if (outputs.Count == 0)
        {
            throw new AiSdkException("No valid video URLs in response");
        }

        var providerMetadata = new JsonObject { ["klingai"] = new JsonObject { ["taskId"] = taskId, ["videos"] = metadata } };
        return new VideoStatusResult(
            "completed",
            outputs,
            providerMetadata: OperationJson.Parse(providerMetadata.ToJsonString()),
            response: Response(response, timestamp));
    }

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

    private async Task<ProviderExchangeResult> GetAsync(Uri uri, VideoModelCall call, string trustedOrigin, CancellationToken cancellationToken)
    {
        try
        {
            return await ProviderExchange.GetResponseUrlAsync(_provider._httpClient, uri.AbsoluteUri, trustedOrigin, Download.ValidateDownloadUrl, ProviderExchange.Merge(_provider.CreateHeaders(), call.Headers), cancellationToken, ProviderExchange.DownloadClient(_provider.Options)).ConfigureAwait(false);
        }
        catch (ApiException exception) when (ErrorMessage(exception.ResponseBody) is { } message)
        {
            throw ProviderHttp.MapStatus(exception.StatusCode, message, exception.ResponseBody);
        }
    }

    // KlingAI errors are { code, message }.
    private static string? ErrorMessage(string? body)
    {
        try
        {
            using var document = JsonDocument.Parse(body ?? string.Empty);
            return document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("code", out var code) && code.ValueKind == JsonValueKind.Number
                    ? String(document.RootElement, "message")
                    : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // Checks the { code, message, data } envelope and returns data, or null when data is absent.
    private static JsonElement? ReadEnvelope(JsonElement root, string body)
    {
        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("code", out var code) || code.ValueKind != JsonValueKind.Number
            || String(root, "message") == null)
        {
            throw new ApiException("Invalid JSON response", 200, body);
        }

        if (!root.TryGetProperty("data", out var data) || data.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (data.ValueKind != JsonValueKind.Object)
        {
            throw new ApiException("Invalid JSON response", 200, body);
        }

        return data;
    }

    private ProviderResponse Response(ProviderExchangeResult response, DateTimeOffset timestamp)
    {
        return new ProviderResponse(response.Headers, timestamp: timestamp.UtcDateTime, modelId: ModelId);
    }

    private static string? String(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static Options ReadOptions(JsonElement providerOptions)
    {
        if (providerOptions.ValueKind != JsonValueKind.Object
            || !providerOptions.TryGetProperty(KlingAIProvider.ProviderId, out var klingai)
            || klingai.ValueKind == JsonValueKind.Null)
        {
            return new Options(default);
        }

        if (klingai.ValueKind != JsonValueKind.Object)
        {
            throw Invalid("klingai", "expected an object.");
        }

        var options = new Options(klingai);
        options.Enum("mode", "std", "pro");
        options.Enum("sound", "on", "off");
        options.Enum("shotType", "customize", "intelligence");
        options.Enum("characterOrientation", "image", "video");
        options.Enum("keepOriginalSound", "yes", "no");
        options.Kind("pollIntervalMs", JsonValueKind.Number);
        options.Kind("pollTimeoutMs", JsonValueKind.Number);
        options.Kind("negativePrompt", JsonValueKind.String);
        options.Kind("cfgScale", JsonValueKind.Number);
        options.Kind("cameraControl", JsonValueKind.Object);
        options.Kind("imageTail", JsonValueKind.String);
        options.Kind("staticMask", JsonValueKind.String);
        options.Kind("dynamicMasks", JsonValueKind.Array);
        options.Kind("multiShot", JsonValueKind.True, JsonValueKind.False);
        options.Kind("multiPrompt", JsonValueKind.Array);
        options.Kind("elementList", JsonValueKind.Array);
        options.Kind("voiceList", JsonValueKind.Array);
        options.Kind("videoUrl", JsonValueKind.String);
        options.Kind("watermarkEnabled", JsonValueKind.True, JsonValueKind.False);
        return options;
    }

    private static InvalidArgumentException Invalid(string name, string message)
    {
        return new InvalidArgumentException("providerOptions." + name, null, "Invalid KlingAI provider options: " + message);
    }

    // The klingai provider options. A null value counts as not set.
    private sealed class Options
    {
        public Options(JsonElement raw)
        {
            Raw = raw;
        }

        public JsonElement Raw { get; }

        public JsonElement? Get(string name)
        {
            return Raw.ValueKind == JsonValueKind.Object && Raw.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null ? value : null;
        }

        public string? String(string name)
        {
            return Get(name) is { ValueKind: JsonValueKind.String } value ? value.GetString() : null;
        }

        public void Enum(string name, params string[] allowed)
        {
            if (Get(name) is { } value && (value.ValueKind != JsonValueKind.String || !allowed.Contains(value.GetString())))
            {
                throw Invalid(name, "expected one of " + string.Join(", ", allowed) + ".");
            }
        }

        public void Kind(string name, params JsonValueKind[] kinds)
        {
            if (Get(name) is { } value && !kinds.Contains(value.ValueKind))
            {
                throw Invalid(name, "has the wrong type.");
            }
        }
    }
}
