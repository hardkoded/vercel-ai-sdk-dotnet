// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.MiniMax;

/// <summary>Configuration of a <see cref="MiniMaxVideoModel"/>.</summary>
public sealed class MiniMaxVideoModelConfig
{
    /// <summary>Creates a configuration.</summary>
    public MiniMaxVideoModelConfig(
        string provider,
        string baseUrl,
        Func<Dictionary<string, string>> headers,
        HttpClient httpClient,
        Func<DateTime>? currentDate = null
    )
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        Headers = headers ?? throw new ArgumentNullException(nameof(headers));
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        CurrentDate = currentDate;
    }

    /// <summary>Provider id, <c>minimax.video</c>.</summary>
    public string Provider { get; }

    /// <summary>API origin with no version suffix.</summary>
    public string BaseUrl { get; }

    /// <summary>Resolves the request headers on every call (bearer token and user agent).</summary>
    public Func<Dictionary<string, string>> Headers { get; }

    /// <summary>The client every request goes through (upstream <c>fetch</c>).</summary>
    public HttpClient HttpClient { get; }

    /// <summary>Sets the response timestamp (upstream <c>_internal.currentDate</c>).</summary>
    public Func<DateTime>? CurrentDate { get; }
}

/// <summary>
/// MiniMax video model. <c>DoStartAsync</c> posts to <c>/v2/video_generation</c> and <c>DoStatusAsync</c> reads the task.
/// The operation is a JSON object with the <c>taskId</c> and the <c>resolvedInputs</c> that were sent.
/// </summary>
public sealed class MiniMaxVideoModel : IVideoModel, IVideoCaller
{
    private const string DefaultAspectRatio = "16:9";
    private const int DefaultPollIntervalMs = 10_000;
    private const int DefaultPollTimeoutMs = 600_000;
    private const int DefaultDurationSeconds = 5;
    private const int MaxDurationSeconds = 15;
    private const int MaxReferenceImages = 9;
    private const int MaxReferenceVideos = 3;
    private const int MaxReferenceAudios = 3;

    // The top-level resolution is {width}x{height}, but the API takes a named tier. MiniMax does not publish
    // tier dimensions, so this is a closed table: one frame size per ratio per tier.
    private static readonly Dictionary<string, string> ResolutionMap = new(StringComparer.Ordinal)
    {
        ["480x480"] = "480P",
        ["1120x480"] = "480P",
        ["854x480"] = "480P",
        ["640x480"] = "480P",
        ["480x854"] = "480P",
        ["480x640"] = "480P",
        ["768x768"] = "768P",
        ["1792x768"] = "768P",
        ["1366x768"] = "768P",
        ["1024x768"] = "768P",
        ["768x1366"] = "768P",
        ["768x1024"] = "768P",
        ["2048x2048"] = "2K",
        ["2560x1080"] = "2K",
        ["2560x1440"] = "2K",
        ["2048x1536"] = "2K",
        ["1440x2560"] = "2K",
        ["1536x2048"] = "2K",
    };

    /// <summary>Creates a model.</summary>
    public MiniMaxVideoModel(string modelId, MiniMaxVideoModelConfig config)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        Config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider => Config.Provider;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>MiniMax creates one video per task.</summary>
    public int? MaxVideosPerCall => 1;

    /// <inheritdoc />
    public bool CanGenerate => true;

    /// <inheritdoc />
    public bool CanStart => true;

    /// <inheritdoc />
    public bool CanStatus => true;

    /// <summary>False. The challenge handshake and progress notifications need a protocol-aware receiver, so a webhook URL is only forwarded as <c>callback_url</c>.</summary>
    public bool CanWebhook => false;

    /// <summary>The configuration the provider passed in.</summary>
    public MiniMaxVideoModelConfig Config { get; }

    /// <inheritdoc />
    public Task<int?> ResolveMaxVideosPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxVideosPerCall);
    }

    /// <summary>Starts a task and polls it until the video is ready.</summary>
    public async Task<VideoResult> DoGenerateAsync(
        VideoCallOptions options,
        CancellationToken cancellationToken
    )
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var result = await GenerateVideo
            .GenerateVideoAsync(
                new GenerateVideoRequest
                {
                    Model = this,
                    Prompt = new VideoPrompt(options.Prompt),
                    Duration = options.DurationSeconds,
                },
                cancellationToken
            )
            .ConfigureAwait(false);
        return new VideoResult(null, result.Video.Data, result.Video.MediaType);
    }

    /// <inheritdoc />
    public async Task<VideoModelResult> DoGenerateAsync(
        VideoModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var startResult = await DoStartAsync(call, cancellationToken).ConfigureAwait(false);
        var operation = (JsonElement)startResult.Operation!;
        var taskId = operation.GetProperty("taskId").GetString()!;
        var minimax = ReadOptions(call.ProviderOptions);
        var pollIntervalMs = minimax.PollIntervalMs ?? DefaultPollIntervalMs;
        var pollTimeoutMs = minimax.PollTimeoutMs ?? DefaultPollTimeoutMs;
        var stopwatch = Stopwatch.StartNew();

        while (true)
        {
            await Task.Delay(pollIntervalMs, cancellationToken).ConfigureAwait(false);

            if (stopwatch.ElapsedMilliseconds > pollTimeoutMs)
            {
                throw new MiniMaxVideoGenerationException(
                    "MINIMAX_VIDEO_GENERATION_TIMEOUT",
                    "MiniMax video generation timed out after "
                        + pollTimeoutMs.ToString(CultureInfo.InvariantCulture)
                        + "ms. Task ID: "
                        + taskId
                );
            }

            var (result, statusError) = await GetStatusAsync(
                    operation,
                    call.Headers,
                    cancellationToken
                )
                .ConfigureAwait(false);
            if (result.Status == "pending")
            {
                continue;
            }

            if (result.Status == "error")
            {
                throw statusError!;
            }

            var references = new JsonArray();
            foreach (
                var index in operation
                    .GetProperty("resolvedInputs")
                    .GetProperty("referenceVideoIndices")
                    .EnumerateArray()
            )
            {
                if (
                    call.InputReferences is { } inputs
                    && index.GetInt32() < inputs.Count
                    && inputs[index.GetInt32()] is { Type: "url" } file
                )
                {
                    references.Add(file.Url);
                }
            }

            var metadata = JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!.AsObject();
            var minimaxMetadata = metadata["minimax"]!.AsObject();
            minimaxMetadata["resolvedInputs"] = new JsonObject
            {
                ["imageCount"] = operation
                    .GetProperty("resolvedInputs")
                    .GetProperty("imageCount")
                    .GetInt32(),
                ["referenceVideoUrls"] = references,
            };
            return new VideoModelResult(
                result.Videos,
                startResult.Warnings.Concat(result.Warnings).ToList(),
                OperationJson.Parse(metadata.ToJsonString()),
                new ProviderResponse(
                    result.Response.Headers,
                    timestamp: startResult.Response.Timestamp,
                    modelId: result.Response.ModelId
                )
            );
        }
    }

    /// <inheritdoc />
    public async Task<VideoStartResult> DoStartAsync(
        VideoModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var timestamp = Config.CurrentDate?.Invoke() ?? DateTime.UtcNow;
        var (body, warnings, resolvedInputs) = GetArgs(call);
        // The challenge handshake and progress notifications rule out a generic webhook hook.
        // Forward the URL for caller-owned receivers that echo the challenge within 3s.
        if (call.WebhookUrl != null)
        {
            body["callback_url"] = call.WebhookUrl;
        }

        var createUrl = ApiKeys.Combine(Config.BaseUrl, "/v2/video_generation");
        var exchange = await ProviderExchange
            .SendAsync(
                Config.HttpClient,
                HttpMethod.Post,
                createUrl,
                ProviderExchange.Json(body.ToJsonString()),
                RequestHeaders(call.Headers),
                cancellationToken,
                (status, body) =>
                    MiniMaxVideoError.FailedResponseHandler(status, body, createUrl.AbsoluteUri)
            )
            .ConfigureAwait(false);
        using var document = JsonDocument.Parse(exchange.Body);
        var taskId = ReadString(document.RootElement, "task_id");
        if (string.IsNullOrEmpty(taskId))
        {
            throw new MiniMaxVideoGenerationException(
                "MINIMAX_VIDEO_GENERATION_ERROR",
                "No task_id returned from the MiniMax API. Response: " + exchange.Body
            );
        }

        var operation = new JsonObject { ["taskId"] = taskId, ["resolvedInputs"] = resolvedInputs };
        return new VideoStartResult(
            OperationJson.Parse(operation.ToJsonString()),
            warnings,
            response: new ProviderResponse(exchange.Headers, timestamp: timestamp, modelId: ModelId)
        );
    }

    /// <inheritdoc />
    public Task<VideoStatusResult> DoStatusAsync(
        VideoModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        if (call.Operation is not JsonElement { ValueKind: JsonValueKind.Object } operation)
        {
            throw new InvalidArgumentException(
                "operation",
                call.Operation,
                "MiniMax operations are objects with a taskId."
            );
        }

        return DoStatusCoreAsync(operation, call.Headers, cancellationToken);
    }

    /// <inheritdoc />
    public Task<VideoWebhook> HandleWebhookAsync(
        Func<Task<VideoWebhook>> webhook,
        CancellationToken cancellationToken
    )
    {
        throw new NotSupportedException(
            "MiniMax forwards a webhook URL as callback_url but does not handle webhooks."
        );
    }

    private static string? ReadString(JsonElement element, string name)
    {
        return
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    // Maps to resolveTopLevelResolution: a caller may pass a named tier instead of a frame size.
    private static string? ResolveTopLevelResolution(string resolution)
    {
        var named = resolution.ToUpperInvariant();
        return MiniMaxVideoModelOptions.MiniMaxVideoResolutions.Contains(named) ? named
            : ResolutionMap.TryGetValue(resolution, out var mapped) ? mapped
            : null;
    }

    // Maps to nonImageFrameMediaType. A frame with no media type is left alone.
    private static string? NonImageFrameMediaType(VideoModelFile file)
    {
        if (file.MediaType == null)
        {
            return null;
        }

        var topLevel = MediaTypes.GetTopLevelMediaType(file.MediaType);
        return topLevel == "image" ? null : topLevel;
    }

    private static string DataUri(VideoModelFile file)
    {
        return file.Url
            ?? "data:"
                + file.MediaType
                + ";base64,"
                + (file.Base64 ?? Convert.ToBase64String(file.Data ?? Array.Empty<byte>()));
    }

    private static JsonObject Part(string type, string url, string role)
    {
        return new JsonObject
        {
            ["type"] = type,
            [type] = new JsonObject { ["url"] = url },
            ["role"] = role,
        };
    }

    private static string Number(double value)
    {
        return value.ToString("R", CultureInfo.InvariantCulture);
    }

    private static MiniMaxVideoOptionValues ReadOptions(JsonElement providerOptions)
    {
        var values = new MiniMaxVideoOptionValues();
        if (
            providerOptions.ValueKind != JsonValueKind.Object
            || !providerOptions.TryGetProperty(MiniMaxProvider.ProviderId, out var minimax)
            || minimax.ValueKind != JsonValueKind.Object
        )
        {
            return values;
        }

        values.Resolution = ReadEnum(
            minimax,
            "resolution",
            MiniMaxVideoModelOptions.MiniMaxVideoResolutions
        );
        values.Ratio = ReadEnum(minimax, "ratio", MiniMaxVideoModelOptions.MiniMaxVideoRatios);
        if (Present(minimax, "aigcWatermark", out var watermark))
        {
            values.AigcWatermark = watermark.ValueKind is JsonValueKind.True or JsonValueKind.False
                ? watermark.GetBoolean()
                : throw Invalid("aigcWatermark", watermark, "must be a boolean.");
        }

        if (Present(minimax, "referenceAudioUrls", out var audio))
        {
            if (audio.ValueKind != JsonValueKind.Array)
            {
                throw Invalid("referenceAudioUrls", audio, "must be an array of strings.");
            }

            foreach (var url in audio.EnumerateArray())
            {
                values.ReferenceAudioUrls.Add(
                    url.ValueKind == JsonValueKind.String
                        ? url.GetString()!
                        : throw Invalid("referenceAudioUrls", audio, "must be an array of strings.")
                );
            }
        }

        values.PollIntervalMs = ReadInt(minimax, "pollIntervalMs");
        values.PollTimeoutMs = ReadInt(minimax, "pollTimeoutMs");
        return values;
    }

    private static bool Present(JsonElement options, string name, out JsonElement value)
    {
        return options.TryGetProperty(name, out value) && value.ValueKind != JsonValueKind.Null;
    }

    private static string? ReadEnum(JsonElement options, string name, IReadOnlyList<string> allowed)
    {
        if (!Present(options, name, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String && allowed.Contains(value.GetString()!)
            ? value.GetString()
            : throw Invalid(name, value, "must be one of " + string.Join(", ", allowed) + ".");
    }

    private static int? ReadInt(JsonElement options, string name)
    {
        if (!Present(options, name, out var value))
        {
            return null;
        }

        return
            value.ValueKind == JsonValueKind.Number
            && value.TryGetInt32(out var number)
            && number > 0
            ? number
            : throw Invalid(name, value, "must be a positive integer.");
    }

    private static InvalidArgumentException Invalid(string name, JsonElement value, string message)
    {
        return new InvalidArgumentException(
            "providerOptions.minimax." + name,
            value.GetRawText(),
            message
        );
    }

    private Dictionary<string, string?> RequestHeaders(IReadOnlyDictionary<string, string> headers)
    {
        return ProviderExchange.Merge(
            Config.Headers().ToDictionary(pair => pair.Key, pair => (string?)pair.Value),
            headers
        );
    }

    private (JsonObject Body, List<OperationWarning> Warnings, JsonObject ResolvedInputs) GetArgs(
        VideoModelCall call
    )
    {
        var warnings = new List<OperationWarning>();
        var minimaxOptions = ReadOptions(call.ProviderOptions);

        if (call.Fps != null)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "fps",
                    ModelId + " does not support a custom frame rate."
                )
            );
        }

        if (call.Seed != null)
        {
            warnings.Add(
                OperationWarning.Unsupported("seed", ModelId + " does not support a seed.")
            );
        }

        if (call.N > 1)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "n",
                    ModelId + " generates a single video per call. Only 1 video will be generated."
                )
            );
        }

        if (call.GenerateAudio != null)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "generateAudio",
                    "The "
                        + ModelId
                        + " API does not expose an audio parameter. The generateAudio option was ignored."
                )
            );
        }

        var videoEntry = MiniMaxModelCatalog.GetMiniMaxVideoModelEntry(ModelId);
        var supportedResolutionNames = string.Join(
            " or ",
            videoEntry.SupportedResolutions.Select(resolution => "\"" + resolution + "\"")
        );

        // The API takes a named tier, so an explicit provider option wins and a top-level value is resolved to a tier.
        var resolution = minimaxOptions.Resolution;
        if (resolution != null && !videoEntry.SupportedResolutions.Contains(resolution))
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "resolution",
                    ModelId
                        + " supports "
                        + supportedResolutionNames
                        + ". The provider resolution \""
                        + resolution
                        + "\" was ignored."
                )
            );
            resolution = null;
        }

        if (call.Resolution != null)
        {
            var mapped = ResolveTopLevelResolution(call.Resolution);
            if (resolution != null)
            {
                if (mapped == null)
                {
                    warnings.Add(
                        OperationWarning.Unsupported(
                            "resolution",
                            "Unrecognized resolution \""
                                + call.Resolution
                                + "\". "
                                + ModelId
                                + " supports "
                                + supportedResolutionNames
                                + ", so providerOptions.minimax.resolution (\""
                                + resolution
                                + "\") was used instead."
                        )
                    );
                }
                else if (mapped != resolution)
                {
                    warnings.Add(
                        OperationWarning.Unsupported(
                            "resolution",
                            "The resolution \""
                                + call.Resolution
                                + "\" selects "
                                + mapped
                                + ", but providerOptions.minimax.resolution (\""
                                + resolution
                                + "\") was used instead."
                        )
                    );
                }
            }
            else if (mapped != null && videoEntry.SupportedResolutions.Contains(mapped))
            {
                resolution = mapped;
            }
            else
            {
                warnings.Add(
                    OperationWarning.Unsupported(
                        "resolution",
                        mapped == null
                            ? "Unrecognized resolution \""
                                + call.Resolution
                                + "\". "
                                + ModelId
                                + " supports "
                                + supportedResolutionNames
                                + "."
                            : ModelId
                                + " does not support the resolution \""
                                + mapped
                                + "\". It supports "
                                + supportedResolutionNames
                                + "."
                    )
                );
            }
        }

        resolution ??= videoEntry.DefaultResolution;

        var content = new JsonArray
        {
            new JsonObject { ["type"] = "text", ["text"] = call.Prompt ?? string.Empty },
        };

        // The inputs that actually make it into content, after the caps and rejections. Accumulated at the push sites
        // rather than re-derived, so the two cannot disagree.
        var sentImageCount = 0;
        var sentReferenceVideoIndices = new JsonArray();

        // A standalone image is the first frame (image-to-video). The frame-image entry is tracked separately to report
        // which input a warning came from.
        var firstFrameImage = call
            .FrameImages?.FirstOrDefault(frame => frame.FrameType == "first_frame")
            ?.Image;
        var firstFrame = firstFrameImage ?? call.Image;
        var lastFrame = call
            .FrameImages?.FirstOrDefault(frame => frame.FrameType == "last_frame")
            ?.Image;

        var firstFrameMediaType = firstFrame != null ? NonImageFrameMediaType(firstFrame) : null;
        if (firstFrame != null && firstFrameMediaType != null)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    firstFrameImage != null ? "frameImages" : "image",
                    firstFrameMediaType == "video"
                        ? ModelId
                            + " does not accept a video as a frame image. The video was ignored."
                        : ModelId
                            + " only accepts an image as a frame image; the \""
                            + firstFrame.MediaType
                            + "\" file was ignored."
                )
            );
            firstFrame = null;
        }

        if (lastFrame != null)
        {
            if (firstFrame == null)
            {
                warnings.Add(
                    OperationWarning.Unsupported(
                        "frameImages",
                        ModelId
                            + " requires a first_frame when a last_frame is provided. The last_frame was ignored."
                    )
                );
                lastFrame = null;
            }
            else
            {
                var lastFrameMediaType = NonImageFrameMediaType(lastFrame);
                if (lastFrameMediaType != null)
                {
                    warnings.Add(
                        OperationWarning.Unsupported(
                            "frameImages",
                            lastFrameMediaType == "video"
                                ? ModelId
                                    + " does not accept a video as a frame image. The last_frame video was ignored."
                                : ModelId
                                    + " only accepts an image as a frame image; the \""
                                    + lastFrame.MediaType
                                    + "\" last_frame was ignored."
                        )
                    );
                    lastFrame = null;
                }
            }
        }

        var usesFrameImages = firstFrame != null || lastFrame != null;

        var referenceFiles = call.InputReferences ?? Array.Empty<VideoModelFile>();
        var referenceAudioUrls = minimaxOptions.ReferenceAudioUrls;
        var supportsReferences = videoEntry.ReferenceImage;

        if (!supportsReferences && referenceFiles.Count > 0)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "inputReferences",
                    "MiniMax-H3-Max does not support reference-to-video inputs. The references were ignored."
                )
            );
        }

        if (!supportsReferences && referenceAudioUrls.Count > 0)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "referenceAudioUrls",
                    "MiniMax-H3-Max does not support reference audio. The audio was ignored."
                )
            );
        }

        var usesReferences =
            supportsReferences && (referenceFiles.Count > 0 || referenceAudioUrls.Count > 0);

        if (usesFrameImages)
        {
            if (firstFrame != null)
            {
                content.Add(Part("image_url", DataUri(firstFrame), "first_frame"));
                sentImageCount++;
            }

            if (lastFrame != null)
            {
                content.Add(Part("image_url", DataUri(lastFrame), "last_frame"));
                sentImageCount++;
            }

            // Frame images and reference inputs are mutually exclusive.
            if (usesReferences)
            {
                warnings.Add(
                    OperationWarning.Unsupported(
                        "inputReferences",
                        "MiniMax-H3 cannot combine frame images with reference inputs. The references were ignored."
                    )
                );
            }
        }
        else if (usesReferences)
        {
            var referenceImages = new List<VideoModelFile>();
            var referenceVideos = new List<(VideoModelFile File, int Index)>();

            for (var index = 0; index < referenceFiles.Count; index++)
            {
                var file = referenceFiles[index];
                var topLevel =
                    file.MediaType != null ? MediaTypes.GetTopLevelMediaType(file.MediaType) : null;
                if (topLevel == "video")
                {
                    referenceVideos.Add((file, index));
                }
                else if (topLevel == "image")
                {
                    referenceImages.Add(file);
                }
                else if (topLevel == null)
                {
                    // Only URL references can omit a media type. Fall back to an image and warn.
                    warnings.Add(
                        OperationWarning.Unsupported(
                            "inputReferences",
                            "MiniMax-H3 requires an explicit mediaType to route URL references as video or image. Pass { data: url, mediaType: \"video/mp4\" } for video references. The reference was treated as an image."
                        )
                    );
                    referenceImages.Add(file);
                }
                else
                {
                    warnings.Add(
                        OperationWarning.Unsupported(
                            "inputReferences",
                            "MiniMax-H3 only accepts image and video references; the \""
                                + file.MediaType
                                + "\" reference was ignored. Pass reference audio via providerOptions.minimax.referenceAudioUrls."
                        )
                    );
                }
            }

            foreach (var image in referenceImages.Take(MaxReferenceImages))
            {
                content.Add(Part("image_url", DataUri(image), "reference_image"));
                sentImageCount++;
            }

            if (referenceImages.Count > MaxReferenceImages)
            {
                warnings.Add(
                    OperationWarning.Unsupported(
                        "inputReferences",
                        "MiniMax-H3 accepts at most 9 reference images. Extra images were ignored."
                    )
                );
            }

            foreach (var (video, index) in referenceVideos.Take(MaxReferenceVideos))
            {
                content.Add(Part("video_url", DataUri(video), "reference_video"));
                // Retain input positions, not request URLs or inline data, in the operation.
                if (video.Type == "url")
                {
                    sentReferenceVideoIndices.Add(index);
                }
            }

            if (referenceVideos.Count > MaxReferenceVideos)
            {
                warnings.Add(
                    OperationWarning.Unsupported(
                        "inputReferences",
                        "MiniMax-H3 accepts at most 3 reference videos. Extra videos were ignored."
                    )
                );
            }

            // Reference audio must accompany at least one reference image or video.
            if (referenceAudioUrls.Count > 0)
            {
                if (referenceImages.Count == 0 && referenceVideos.Count == 0)
                {
                    warnings.Add(
                        OperationWarning.Unsupported(
                            "referenceAudioUrls",
                            "MiniMax-H3 reference audio must be paired with at least one reference image or video. The audio was ignored."
                        )
                    );
                }
                else
                {
                    foreach (var url in referenceAudioUrls.Take(MaxReferenceAudios))
                    {
                        content.Add(Part("audio_url", url, "reference_audio"));
                    }

                    if (referenceAudioUrls.Count > MaxReferenceAudios)
                    {
                        warnings.Add(
                            OperationWarning.Unsupported(
                                "referenceAudioUrls",
                                "MiniMax-H3 accepts at most 3 reference audios. Extra audios were ignored."
                            )
                        );
                    }
                }
            }
        }

        // In frame-image mode the ratio follows the supplied image, so an explicit ratio is ignored.
        var isTextToVideo = content.Count == 1;
        var ratio = minimaxOptions.Ratio;
        if (ratio == null && call.AspectRatio != null)
        {
            if (MiniMaxVideoModelOptions.MiniMaxVideoRatios.Contains(call.AspectRatio))
            {
                ratio = call.AspectRatio;
            }
            else
            {
                warnings.Add(
                    OperationWarning.Unsupported(
                        "aspectRatio",
                        isTextToVideo
                            ? ModelId
                                + " does not support the aspect ratio \""
                                + call.AspectRatio
                                + "\". Using the default ("
                                + DefaultAspectRatio
                                + ")."
                            : ModelId
                                + " does not support the aspect ratio \""
                                + call.AspectRatio
                                + "\". Using the provider default (adaptive)."
                    )
                );
                if (isTextToVideo)
                {
                    ratio = DefaultAspectRatio;
                }
            }
        }

        if (ratio == "adaptive" && isTextToVideo)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "aspectRatio",
                    ModelId
                        + " text-to-video does not support the adaptive aspect ratio. Using the default ("
                        + DefaultAspectRatio
                        + ")."
                )
            );
            ratio = DefaultAspectRatio;
        }

        if (usesFrameImages && ratio != null)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "aspectRatio",
                    ModelId
                        + " derives the aspect ratio from the frame image; the requested ratio was ignored."
                )
            );
            ratio = null;
        }

        if (ratio == null && isTextToVideo)
        {
            ratio = DefaultAspectRatio;
        }

        // Both H3 models take an integer duration, but H3 starts at 4 seconds while H3-Max starts at 5.
        // Round before clamping because the API rejects both fractional and out-of-range values.
        var minDurationSeconds = videoEntry.MinDurationSeconds;
        double duration = DefaultDurationSeconds;
        if (call.Duration is { } requested)
        {
            if (double.IsNaN(requested))
            {
                throw new InvalidArgumentException(
                    "duration",
                    requested,
                    "must be a number of seconds."
                );
            }

            duration = requested;
            if (Math.Floor(duration) != duration)
            {
                // JavaScript Math.round rounds halves up.
                duration = Math.Floor(duration + 0.5);
                warnings.Add(
                    OperationWarning.Unsupported(
                        "duration",
                        ModelId
                            + " requires a whole number of seconds. The requested duration of "
                            + Number(requested)
                            + " was rounded to "
                            + Number(duration)
                            + "."
                    )
                );
            }

            if (duration > MaxDurationSeconds)
            {
                warnings.Add(
                    OperationWarning.Unsupported(
                        "duration",
                        ModelId
                            + " supports at most 15 seconds. The requested duration of "
                            + Number(requested)
                            + " was clamped to 15."
                    )
                );
                duration = MaxDurationSeconds;
            }
            else if (duration < minDurationSeconds)
            {
                warnings.Add(
                    OperationWarning.Unsupported(
                        "duration",
                        ModelId
                            + " requires at least "
                            + minDurationSeconds.ToString(CultureInfo.InvariantCulture)
                            + " seconds. The requested duration of "
                            + Number(requested)
                            + " was clamped to "
                            + minDurationSeconds.ToString(CultureInfo.InvariantCulture)
                            + "."
                    )
                );
                duration = minDurationSeconds;
            }
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["content"] = content,
            ["resolution"] = resolution,
            ["duration"] = (int)duration,
        };
        if (ratio != null)
        {
            body["ratio"] = ratio;
        }

        if (minimaxOptions.AigcWatermark != null)
        {
            body["aigc_watermark"] = minimaxOptions.AigcWatermark;
        }

        return (
            body,
            warnings,
            new JsonObject
            {
                ["imageCount"] = sentImageCount,
                ["referenceVideoIndices"] = sentReferenceVideoIndices,
            }
        );
    }

    private async Task<VideoStatusResult> DoStatusCoreAsync(
        JsonElement operation,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken cancellationToken
    )
    {
        var (result, _) = await GetStatusAsync(operation, headers, cancellationToken)
            .ConfigureAwait(false);
        return result;
    }

    private async Task<(
        VideoStatusResult Result,
        MiniMaxVideoGenerationException? Error
    )> GetStatusAsync(
        JsonElement operation,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken cancellationToken
    )
    {
        var timestamp = Config.CurrentDate?.Invoke() ?? DateTime.UtcNow;
        var taskId = ReadString(operation, "taskId")
            ?? throw new InvalidArgumentException(
                "operation",
                operation.GetRawText(),
                "MiniMax operations are objects with a taskId."
            );
        var resolvedInputs = operation.TryGetProperty("resolvedInputs", out var inputs)
            ? JsonNode.Parse(inputs.GetRawText())
            : null;
        var statusUrl = ApiKeys.Combine(
            Config.BaseUrl,
            "/v2/query/video_generation/" + PathSegments.Encode(taskId)
        );
        var requestHeaders = RequestHeaders(headers);
        var exchange = await ProviderExchange
            .GetResponseUrlAsync(
                Config.HttpClient,
                statusUrl.AbsoluteUri,
                Config.BaseUrl,
                Download.ValidateDownloadUrl,
                requestHeaders,
                cancellationToken,
                untrustedHeaders: ProviderExchange.UserAgentOnly(requestHeaders),
                failedResponseHandler: (status, body) =>
                    MiniMaxVideoError.FailedResponseHandler(status, body, statusUrl.AbsoluteUri)
            )
            .ConfigureAwait(false);
        var response = new ProviderResponse(
            exchange.Headers,
            timestamp: timestamp,
            modelId: ModelId
        );
        using var document = JsonDocument.Parse(exchange.Body);
        var task =
            document.RootElement.ValueKind == JsonValueKind.Object
            && document.RootElement.TryGetProperty("task", out var taskElement)
                ? taskElement
                : default;

        switch (ReadString(task, "status"))
        {
            case "succeeded":
            {
                var content = task.TryGetProperty("content", out var contentElement)
                    ? contentElement
                    : default;
                var url = ReadString(content, "url");
                if (string.IsNullOrEmpty(url))
                {
                    throw new MiniMaxVideoGenerationException(
                        "MINIMAX_VIDEO_GENERATION_ERROR",
                        "MiniMax video generation completed but no video URL was returned. Task ID: "
                            + taskId
                    );
                }

                var metadata = new JsonObject
                {
                    ["taskId"] = taskId,
                    ["videoUrl"] = url,
                    ["resolvedInputs"] = resolvedInputs,
                };
                CopyNumber(task, "duration", metadata, "duration");
                if (ReadString(task, "ratio") is { } ratio)
                {
                    metadata["ratio"] = ratio;
                }

                if (ReadString(task, "resolution") is { } resolution)
                {
                    metadata["resolution"] = resolution;
                }

                if (
                    task.TryGetProperty("usage", out var usage)
                    && usage.ValueKind == JsonValueKind.Object
                )
                {
                    var usageMetadata = new JsonObject();
                    CopyNumber(usage, "total_seconds", usageMetadata, "totalSeconds");
                    CopyNumber(usage, "input_seconds", usageMetadata, "inputSeconds");
                    CopyNumber(usage, "output_seconds", usageMetadata, "outputSeconds");
                    metadata["usage"] = usageMetadata;
                }

                return (
                    new VideoStatusResult(
                        "completed",
                        new[] { new VideoOutput("url", url, mediaType: "video/mp4") },
                        providerMetadata: OperationJson.Parse(
                            new JsonObject { ["minimax"] = metadata }.ToJsonString()
                        ),
                        response: response
                    ),
                    null
                );
            }

            case "failed":
            {
                var error = task.TryGetProperty("error", out var errorElement)
                    ? errorElement
                    : default;
                var message = ReadString(error, "message");
                var code =
                    error.ValueKind == JsonValueKind.Object
                    && error.TryGetProperty("code", out var codeElement)
                    && codeElement.ValueKind != JsonValueKind.Null
                        ? codeElement.ValueKind == JsonValueKind.String
                            ? codeElement.GetString()
                            : codeElement.GetRawText()
                        : null;
                return Failure(
                    "MINIMAX_VIDEO_GENERATION_FAILED",
                    "MiniMax video generation failed"
                        + (string.IsNullOrEmpty(message) ? string.Empty : ": " + message)
                        + (code != null ? " (" + code + ")" : string.Empty)
                        + ". Task ID: "
                        + taskId,
                    response
                );
            }

            case "cancelled":
                return Failure(
                    "MINIMAX_VIDEO_GENERATION_CANCELLED",
                    "MiniMax video generation was cancelled. Task ID: " + taskId,
                    response
                );

            case "expired":
                return Failure(
                    "MINIMAX_VIDEO_GENERATION_EXPIRED",
                    "MiniMax video generation request expired. Task ID: " + taskId,
                    response
                );

            // queued, running, and unknown statuses keep polling.
            default:
                return (new VideoStatusResult("pending", response: response), null);
        }
    }

    private static (VideoStatusResult Result, MiniMaxVideoGenerationException? Error) Failure(
        string name,
        string message,
        ProviderResponse response
    )
    {
        return (
            new VideoStatusResult("error", error: message, response: response),
            new MiniMaxVideoGenerationException(name, message)
        );
    }

    private static void CopyNumber(JsonElement source, string name, JsonObject target, string field)
    {
        if (
            source.ValueKind == JsonValueKind.Object
            && source.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
        )
        {
            target[field] = JsonNode.Parse(value.GetRawText());
        }
    }

    private sealed class MiniMaxVideoOptionValues
    {
        public string? Resolution { get; set; }

        public string? Ratio { get; set; }

        public bool? AigcWatermark { get; set; }

        public List<string> ReferenceAudioUrls { get; } = new();

        public int? PollIntervalMs { get; set; }

        public int? PollTimeoutMs { get; set; }
    }
}
