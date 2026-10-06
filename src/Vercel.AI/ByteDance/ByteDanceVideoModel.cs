// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.ByteDance;

/// <summary>
/// ByteDance Seedance video model. <see cref="DoStartAsync"/> creates a task and <see cref="DoStatusAsync"/> reads it.
/// The operation is a JSON object with the <c>taskId</c>.
/// </summary>
public sealed class ByteDanceVideoModel : IVideoModel, IVideoCaller
{
    private static readonly HashSet<string> HandledOptions = new(StringComparer.Ordinal)
    {
        "watermark", "generateAudio", "cameraFixed", "returnLastFrame", "serviceTier", "draft",
        "lastFrameImage", "referenceImages", "referenceVideos", "referenceAudio", "pollIntervalMs", "pollTimeoutMs",
    };

    private static readonly Dictionary<string, string> Resolutions = new(StringComparer.Ordinal)
    {
        ["864x496"] = "480p",
        ["496x864"] = "480p",
        ["752x560"] = "480p",
        ["560x752"] = "480p",
        ["640x640"] = "480p",
        ["992x432"] = "480p",
        ["432x992"] = "480p",
        ["864x480"] = "480p",
        ["480x864"] = "480p",
        ["736x544"] = "480p",
        ["544x736"] = "480p",
        ["960x416"] = "480p",
        ["416x960"] = "480p",
        ["832x480"] = "480p",
        ["480x832"] = "480p",
        ["624x624"] = "480p",
        ["1280x720"] = "720p",
        ["720x1280"] = "720p",
        ["1112x834"] = "720p",
        ["834x1112"] = "720p",
        ["960x960"] = "720p",
        ["1470x630"] = "720p",
        ["630x1470"] = "720p",
        ["1248x704"] = "720p",
        ["704x1248"] = "720p",
        ["1120x832"] = "720p",
        ["832x1120"] = "720p",
        ["1504x640"] = "720p",
        ["640x1504"] = "720p",
        ["1920x1080"] = "1080p",
        ["1080x1920"] = "1080p",
        ["1664x1248"] = "1080p",
        ["1248x1664"] = "1080p",
        ["1440x1440"] = "1080p",
        ["2206x946"] = "1080p",
        ["946x2206"] = "1080p",
        ["1920x1088"] = "1080p",
        ["1088x1920"] = "1080p",
        ["2176x928"] = "1080p",
        ["928x2176"] = "1080p",
    };

    private static readonly string[] StatusFields = { "id", "model", "status", "content", "usage", "error" };

    private readonly ByteDanceProvider _provider;
    private readonly Func<DateTimeOffset> _clock;

    /// <summary>Creates a model. <paramref name="clock"/> sets the response timestamp.</summary>
    public ByteDanceVideoModel(ByteDanceProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public string Provider => "bytedance.video";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>ByteDance creates one video per task.</summary>
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
        throw new NotSupportedException("ByteDance generates videos with doStart and doStatus.");
    }

    /// <inheritdoc />
    public async Task<VideoStartResult> DoStartAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var timestamp = _clock();
        var warnings = new List<OperationWarning>();
        var body = Build(call, warnings);
        if (call.WebhookUrl != null)
        {
            body["callback_url"] = call.WebhookUrl;
        }

        var response = await ProviderExchange.SendAsync(
            _provider._httpClient,
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "contents/generations/tasks"),
            ProviderExchange.Json(body.ToJsonString()),
            ProviderExchange.Merge(_provider.CreateHeaders(), call.Headers),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        var taskId = String(document.RootElement, "id");
        if (string.IsNullOrEmpty(taskId))
        {
            throw new AiSdkException("No task ID returned from API");
        }

        var operation = OperationJson.Parse(new JsonObject { ["taskId"] = taskId }.ToJsonString());
        return new VideoStartResult(operation, warnings, response: Response(response, timestamp));
    }

    /// <inheritdoc />
    public async Task<VideoStatusResult> DoStatusAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var timestamp = _clock();
        var taskId = call.Operation is JsonElement { ValueKind: JsonValueKind.Object } operation ? String(operation, "taskId") : null;
        if (taskId == null)
        {
            throw new InvalidArgumentException("operation", call.Operation, "ByteDance operations are objects with a taskId.");
        }

        var response = await ProviderExchange.SendAsync(
            _provider._httpClient,
            HttpMethod.Get,
            ApiKeys.Combine(_provider.Options.BaseUrl, "contents/generations/tasks/" + Uri.EscapeDataString(taskId)),
            null,
            ProviderExchange.Merge(_provider.CreateHeaders(), call.Headers),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        var status = String(root, "status");
        if (status == "succeeded")
        {
            var content = root.TryGetProperty("content", out var contentElement) ? contentElement : default;
            var videoUrl = String(content, "video_url");
            if (string.IsNullOrEmpty(videoUrl))
            {
                throw new AiSdkException("No video URL in response. Task ID: " + taskId);
            }

            var metadata = new JsonObject { ["taskId"] = taskId };
            if (root.TryGetProperty("usage", out var usage) && usage.ValueKind != JsonValueKind.Null)
            {
                metadata["usage"] = JsonNode.Parse(usage.GetRawText());
            }

            if (String(content, "last_frame_url") is { } lastFrameUrl)
            {
                metadata["lastFrameUrl"] = lastFrameUrl;
            }

            return new VideoStatusResult(
                "completed",
                new[] { new VideoOutput("url", videoUrl, mediaType: "video/mp4") },
                providerMetadata: OperationJson.Parse(new JsonObject { ["bytedance"] = metadata }.ToJsonString()),
                response: Response(response, timestamp));
        }

        // ModelArk documents "cancelled"; "canceled" is accepted too.
        if (status is "failed" or "expired" or "cancelled" or "canceled")
        {
            var error = root.TryGetProperty("error", out var errorElement) ? errorElement : default;
            var details = String(error, "message") ?? String(error, "code") ?? KnownStatusFields(root);
            return new VideoStatusResult("error", error: "Video generation " + status + ". Task ID: " + taskId + ". " + details, response: Response(response, timestamp));
        }

        return new VideoStatusResult("pending", response: Response(response, timestamp));
    }

    /// <inheritdoc />
    public Task<VideoWebhook> HandleWebhookAsync(Func<Task<VideoWebhook>> webhook, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("ByteDance forwards a webhook URL as callback_url but does not handle webhooks.");
    }

    private JsonObject Build(VideoModelCall call, List<OperationWarning> warnings)
    {
        var options = call.ProviderOptions.ValueKind == JsonValueKind.Object
            && call.ProviderOptions.TryGetProperty(ByteDanceProvider.ProviderId, out var bytedance)
            && bytedance.ValueKind == JsonValueKind.Object
                ? bytedance
                : default;

        // Polling runs through doStart and doStatus, so the old provider poll options do nothing.
        foreach (var setting in new[] { "pollIntervalMs", "pollTimeoutMs" })
        {
            if (Option(options, setting) != null)
            {
                warnings.Add(new OperationWarning("deprecated", message: "`" + setting + "` is ignored. Polling is orchestrated by the AI SDK: pass `poll: { intervalMs, timeoutMs }` to `generateVideo` instead.", setting: setting));
            }
        }

        if (call.Fps != null)
        {
            warnings.Add(OperationWarning.Unsupported("fps", "ByteDance video models do not support custom FPS. Frame rate is fixed at 24 fps."));
        }

        if (call.N > 1)
        {
            warnings.Add(OperationWarning.Unsupported("n", "ByteDance video models do not support generating multiple videos per call. Only 1 video will be generated."));
        }

        var content = new JsonArray();
        if (call.Prompt != null)
        {
            content.Add(new JsonObject { ["type"] = "text", ["text"] = call.Prompt });
        }

        var startImage = Frame(call, "first_frame") ?? call.Image;
        var lastFrame = Frame(call, "last_frame") is { } frame ? DataUri(frame) : Option(options, "lastFrameImage")?.GetString();
        var references = References(call, options, warnings);
        if (startImage != null)
        {
            var image = Part("image_url", DataUri(startImage), null);
            if (lastFrame != null)
            {
                image["role"] = "first_frame";
            }
            else if (references.Count > 0)
            {
                image["role"] = "reference_image";
            }

            content.Add(image);
        }

        if (lastFrame != null)
        {
            content.Add(Part("image_url", lastFrame, "last_frame"));
        }

        foreach (var reference in references)
        {
            content.Add(reference);
        }

        foreach (var audio in Strings(options, "referenceAudio"))
        {
            content.Add(Part("audio_url", audio, "reference_audio"));
        }

        var body = new JsonObject { ["model"] = ModelId, ["content"] = content };
        if (call.AspectRatio != null)
        {
            body["ratio"] = call.AspectRatio;
        }

        if (call.Duration != null)
        {
            body["duration"] = call.Duration;
        }

        if (call.Seed != null)
        {
            body["seed"] = call.Seed;
        }

        if (call.Resolution != null)
        {
            body["resolution"] = Resolutions.TryGetValue(call.Resolution, out var mapped) ? mapped : call.Resolution;
        }

        // The top-level setting wins over the older provider option.
        if (call.GenerateAudio is { } generateAudio)
        {
            body["generate_audio"] = generateAudio;
        }
        else if (Option(options, "generateAudio") is { } legacyAudio)
        {
            body["generate_audio"] = JsonNode.Parse(legacyAudio.GetRawText());
        }

        if (options.ValueKind == JsonValueKind.Object)
        {
            Copy(options, "watermark", body, "watermark");
            Copy(options, "cameraFixed", body, "camera_fixed");
            Copy(options, "returnLastFrame", body, "return_last_frame");
            Copy(options, "serviceTier", body, "service_tier");
            Copy(options, "draft", body, "draft");
            foreach (var option in options.EnumerateObject().Where(option => !HandledOptions.Contains(option.Name)))
            {
                body[option.Name] = JsonNode.Parse(option.Value.GetRawText());
            }
        }

        return body;
    }

    // Frames replace every reference. Input references replace the provider reference lists.
    private static List<JsonObject> References(VideoModelCall call, JsonElement options, List<OperationWarning> warnings)
    {
        var references = new List<JsonObject>();
        if (call.FrameImages is { Count: > 0 })
        {
            return references;
        }

        if (call.InputReferences is { Count: > 0 } inputs)
        {
            foreach (var input in inputs)
            {
                if (input.Type == "url" && input.MediaType == null)
                {
                    warnings.Add(OperationWarning.Unsupported("inputReferences", "ByteDance requires an explicit mediaType to route URL references as video or image. Pass { data: url, mediaType: \"video/mp4\" } for video references. The reference was treated as an image."));
                }

                references.Add(input.MediaType != null && MediaTypes.GetTopLevelMediaType(input.MediaType) == "video"
                    ? Part("video_url", DataUri(input), "reference_video")
                    : Part("image_url", DataUri(input), "reference_image"));
            }

            return references;
        }

        references.AddRange(Strings(options, "referenceImages").Select(url => Part("image_url", url, "reference_image")));
        references.AddRange(Strings(options, "referenceVideos").Select(url => Part("video_url", url, "reference_video")));
        return references;
    }

    private static VideoModelFile? Frame(VideoModelCall call, string frameType)
    {
        return call.FrameImages?.FirstOrDefault(frame => frame.FrameType == frameType)?.Image;
    }

    private static JsonObject Part(string type, string url, string? role)
    {
        var part = new JsonObject { ["type"] = type, [type] = new JsonObject { ["url"] = url } };
        if (role != null)
        {
            part["role"] = role;
        }

        return part;
    }

    private static string DataUri(VideoModelFile file)
    {
        return file.Url ?? "data:" + file.MediaType + ";base64," + Convert.ToBase64String(file.Data ?? Array.Empty<byte>());
    }

    // The status body as the documented fields only, for failures that carry no reason.
    private static string KnownStatusFields(JsonElement root)
    {
        var known = new JsonObject();
        foreach (var name in StatusFields)
        {
            if (root.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null)
            {
                known[name] = JsonNode.Parse(value.GetRawText());
            }
        }

        return known.ToJsonString();
    }

    private ProviderResponse Response(ProviderExchangeResult response, DateTimeOffset timestamp)
    {
        return new ProviderResponse(response.Headers, timestamp: timestamp.UtcDateTime, modelId: ModelId);
    }

    private static void Copy(JsonElement options, string name, JsonObject body, string field)
    {
        if (Option(options, name) is { } value)
        {
            body[field] = JsonNode.Parse(value.GetRawText());
        }
    }

    private static JsonElement? Option(JsonElement options, string name)
    {
        return options.ValueKind == JsonValueKind.Object && options.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null ? value : null;
    }

    private static IEnumerable<string> Strings(JsonElement options, string name)
    {
        return Option(options, name) is { ValueKind: JsonValueKind.Array } values
            ? values.EnumerateArray().Select(value => value.GetString() ?? string.Empty).ToList()
            : Enumerable.Empty<string>();
    }

    private static string? String(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }
}
