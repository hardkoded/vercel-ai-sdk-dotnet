// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Alibaba;

/// <summary>Configuration of <see cref="AlibabaVideoModel"/>. Maps to <c>AlibabaVideoModelConfig</c>.</summary>
public sealed class AlibabaVideoModelConfig
{
    /// <summary>Creates a config.</summary>
    public AlibabaVideoModelConfig(
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

    /// <summary>Provider id, for example <c>alibaba.video</c>.</summary>
    public string Provider { get; }

    /// <summary>DashScope native API origin, for example <c>https://dashscope-intl.aliyuncs.com</c>.</summary>
    public string BaseUrl { get; }

    /// <summary>Resolves the request headers, including the credential.</summary>
    public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> Headers { get; }

    /// <summary>Maps <c>fetch</c>: the HTTP client for API calls.</summary>
    public HttpClient HttpClient { get; }

    /// <summary>Sets the response timestamp and defaults to UTC now. Maps to <c>_internal.currentDate</c>.</summary>
    public Func<DateTime>? CurrentDate { get; set; }
}

/// <summary>
/// Alibaba (DashScope) Wan video model. <see cref="DoStartAsync"/> creates a task and <see cref="DoStatusAsync"/> reads it.
/// The operation is a JSON object with the <c>taskId</c>. Maps to <c>AlibabaVideoModel</c>.
/// </summary>
public sealed class AlibabaVideoModel : IVideoModel, IVideoCaller
{
    // Maps SDK "WIDTHxHEIGHT" resolutions to Alibaba resolution tiers.
    private static readonly Dictionary<string, string> ResolutionTierMap = new(
        StringComparer.Ordinal
    )
    {
        ["1280x720"] = "720P",
        ["720x1280"] = "720P",
        ["960x960"] = "720P",
        ["1088x832"] = "720P",
        ["832x1088"] = "720P",
        ["1920x1080"] = "1080P",
        ["1080x1920"] = "1080P",
        ["1440x1440"] = "1080P",
        ["1632x1248"] = "1080P",
        ["1248x1632"] = "1080P",
        ["832x480"] = "480P",
        ["480x832"] = "480P",
        ["624x624"] = "480P",
    };

    private static readonly HashSet<string> SupportedRatios = new(StringComparer.Ordinal)
    {
        "16:9",
        "9:16",
        "1:1",
        "4:3",
        "3:4",
    };

    private static readonly Regex VideoUrlPattern = new(
        @"\.(mp4|mov)([?#]|\z)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1)
    );

    private static readonly string[] CreateTaskFields = ["output", "request_id"];

    private readonly AlibabaVideoModelConfig _config;

    /// <summary>Creates a model.</summary>
    public AlibabaVideoModel(string modelId, AlibabaVideoModelConfig config)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider => _config.Provider;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Alibaba creates one video per task.</summary>
    public int? MaxVideosPerCall => 1;

    /// <inheritdoc />
    public bool CanGenerate => false;

    /// <inheritdoc />
    public bool CanStart => true;

    /// <inheritdoc />
    public bool CanStatus => true;

    /// <summary>False. Alibaba does not send webhooks to this model.</summary>
    public bool CanWebhook => false;

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
    public Task<VideoModelResult> DoGenerateAsync(
        VideoModelCall call,
        CancellationToken cancellationToken
    )
    {
        throw new NotSupportedException("Alibaba generates videos with doStart and doStatus.");
    }

    /// <inheritdoc />
    public Task<VideoWebhook> HandleWebhookAsync(
        Func<Task<VideoWebhook>> webhook,
        CancellationToken cancellationToken
    )
    {
        throw new NotSupportedException("Alibaba does not handle webhooks.");
    }

    /// <inheritdoc />
    public async Task<VideoStartResult> DoStartAsync(
        VideoModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var currentDate = CurrentDate();
        var (input, parameters, warnings, _) = BuildRequest(call);

        var headers = ProviderExchange.Merge(
            await _config.Headers(cancellationToken).ConfigureAwait(false),
            call.Headers
        );
        headers["X-DashScope-Async"] = "enable";

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["input"] = input,
            ["parameters"] = parameters,
        };
        var response = await ProviderExchange
            .SendAsync(
                _config.HttpClient,
                HttpMethod.Post,
                ApiKeys.Combine(
                    _config.BaseUrl,
                    "/api/v1/services/aigc/video-generation/video-synthesis"
                ),
                ProviderExchange.Json(body.ToJsonString()),
                headers,
                cancellationToken,
                AlibabaVideoFailedResponseHandler
            )
            .ConfigureAwait(false);

        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        var taskId = String(Property(root, "output"), "task_id");
        if (string.IsNullOrEmpty(taskId))
        {
            throw new AiSdkException(
                "No task_id returned from Alibaba API. Response: " + KnownFields(root)
            );
        }

        return new VideoStartResult(
            OperationJson.Parse(new JsonObject { ["taskId"] = taskId }.ToJsonString()),
            warnings,
            response: new ProviderResponse(
                response.Headers,
                timestamp: currentDate,
                modelId: ModelId
            )
        );
    }

    /// <inheritdoc />
    public async Task<VideoStatusResult> DoStatusAsync(
        VideoModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var currentDate = CurrentDate();
        var taskId = call.Operation is JsonElement { ValueKind: JsonValueKind.Object } operation
            ? String(operation, "taskId")
            : null;
        if (taskId == null)
        {
            throw new InvalidArgumentException(
                "operation",
                call.Operation,
                "Alibaba operations are objects with a taskId."
            );
        }

        var response = await ProviderExchange
            .SendAsync(
                _config.HttpClient,
                HttpMethod.Get,
                ApiKeys.Combine(_config.BaseUrl, "/api/v1/tasks/" + PathSegments.Encode(taskId)),
                null,
                ProviderExchange.Merge(
                    await _config.Headers(cancellationToken).ConfigureAwait(false),
                    call.Headers
                ),
                cancellationToken,
                AlibabaVideoFailedResponseHandler
            )
            .ConfigureAwait(false);
        var providerResponse = new ProviderResponse(
            response.Headers,
            timestamp: currentDate,
            modelId: ModelId
        );

        using var document = JsonDocument.Parse(response.Body);
        var statusResponse = document.RootElement;
        var taskStatus = String(Property(statusResponse, "output"), "task_status");

        if (taskStatus == "SUCCEEDED")
        {
            return BuildCompletedResult(statusResponse, providerResponse, []);
        }

        if (taskStatus is "FAILED" or "CANCELED")
        {
            var message = String(Property(statusResponse, "output"), "message") ?? string.Empty;
            return new VideoStatusResult(
                "error",
                error: (
                    "Video generation "
                    + taskStatus.ToLowerInvariant()
                    + ". Task ID: "
                    + taskId
                    + ". "
                    + message
                ).Trim(),
                response: providerResponse
            );
        }

        return new VideoStatusResult("pending", response: providerResponse);
    }

    private static ApiException AlibabaVideoFailedResponseHandler(int statusCode, string body)
    {
        // DashScope native API error format (different from OpenAI-compatible endpoint).
        string? message = null;
        try
        {
            using var document = JsonDocument.Parse(body);
            message = String(document.RootElement, "message");
        }
        catch (JsonException)
        {
            // Not the documented error shape: fall back to the shared extraction below.
        }

        return ProviderHttp.MapStatus(
            statusCode,
            message ?? JsonValues.ExtractErrorMessage(body, statusCode),
            body
        );
    }

    private static string? DeriveRatioFromResolution(string resolution)
    {
        var parts = resolution.Split('x');
        if (
            parts.Length < 2
            || !TryParseInteger(parts[0], out var width)
            || !TryParseInteger(parts[1], out var height)
            || width <= 0
            || height <= 0
        )
        {
            return null;
        }

        var a = width;
        var b = height;
        while (b != 0)
        {
            (a, b) = (b, a % b);
        }

        var ratio =
            (width / a).ToString(CultureInfo.InvariantCulture)
            + ":"
            + (height / a).ToString(CultureInfo.InvariantCulture);
        return SupportedRatios.Contains(ratio) ? ratio : null;
    }

    private static bool TryParseInteger(string text, out long value)
    {
        value = 0;
        if (
            !double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
            || Math.Floor(number) != number
            || Math.Abs(number) > long.MaxValue
        )
        {
            return false;
        }

        value = (long)number;
        return true;
    }

    private static string FileToImageString(VideoModelFile file)
    {
        if (file.Type == "url")
        {
            return file.Url ?? string.Empty;
        }

        return file.Base64 ?? ByteEncoding.ToBase64(file.Data ?? Array.Empty<byte>());
    }

    private static VideoModelFile? GetFirstFrameImage(VideoModelCall options)
    {
        return options
            .FrameImages?.FirstOrDefault(frame => frame.FrameType == "first_frame")
            ?.Image;
    }

    private static VideoModelFile? GetLastFrameImage(VideoModelCall options)
    {
        return options.FrameImages?.FirstOrDefault(frame => frame.FrameType == "last_frame")?.Image;
    }

    private static VideoModelFile? ResolveStartImage(VideoModelCall options)
    {
        return GetFirstFrameImage(options) ?? options.Image;
    }

    private static bool IsVideoUrl(string url)
    {
        return VideoUrlPattern.IsMatch(url);
    }

    // Builds the input.media array (wan2.7 and wan3) from inputReferences plus the frame images the
    // caller resolved for this protocol.
    private static JsonArray? ResolveMedia(
        VideoModelCall options,
        AlibabaVideoModelOptions? alibabaOptions,
        List<OperationWarning> warnings,
        VideoModelFile? first,
        VideoModelFile? last
    )
    {
        if (alibabaOptions?.Media is { Count: > 0 } explicitMedia)
        {
            var items = new JsonArray();
            foreach (var item in explicitMedia)
            {
                var node = new JsonObject { ["type"] = item.Type, ["url"] = item.Url };
                if (item.ReferenceVoice != null)
                {
                    node["reference_voice"] = item.ReferenceVoice;
                }

                items.Add(node);
            }

            return items;
        }

        var media = new JsonArray();

        foreach (var reference in options.InputReferences ?? [])
        {
            if (reference.Type == "url")
            {
                var url = reference.Url ?? string.Empty;
                media.Add(
                    new JsonObject
                    {
                        ["type"] = IsVideoUrl(url) ? "reference_video" : "reference_image",
                        ["url"] = url,
                    }
                );
            }
            else if (reference.MediaType?.StartsWith("image/", StringComparison.Ordinal) == true)
            {
                media.Add(
                    new JsonObject
                    {
                        ["type"] = "reference_image",
                        ["url"] = FileDataConversions.ConvertImageModelFileToDataUri(reference),
                    }
                );
            }
            else
            {
                warnings.Add(
                    OperationWarning.Unsupported(
                        "inputReferences",
                        "Alibaba reference-to-video requires URL references for videos. "
                            + "Non-URL video reference was skipped."
                    )
                );
            }
        }

        if (first != null)
        {
            media.Add(
                new JsonObject
                {
                    ["type"] = "first_frame",
                    ["url"] = FileDataConversions.ConvertImageModelFileToDataUri(first),
                }
            );
        }

        if (last != null)
        {
            media.Add(
                new JsonObject
                {
                    ["type"] = "last_frame",
                    ["url"] = FileDataConversions.ConvertImageModelFileToDataUri(last),
                }
            );
        }

        return media.Count > 0 ? media : null;
    }

    private static List<string>? ResolveReferenceUrls(
        VideoModelCall options,
        AlibabaVideoModelOptions? alibabaOptions,
        List<OperationWarning> warnings
    )
    {
        if (options.FrameImages is { Count: > 0 })
        {
            return null;
        }

        if (options.InputReferences is { Count: > 0 } references)
        {
            var urls = new List<string>();

            foreach (var reference in references)
            {
                if (reference.Type == "url")
                {
                    urls.Add(reference.Url ?? string.Empty);
                }
                else
                {
                    warnings.Add(
                        OperationWarning.Unsupported(
                            "inputReferences",
                            "Alibaba reference-to-video requires URL references. "
                                + "Non-URL reference was skipped."
                        )
                    );
                }
            }

            return urls.Count > 0 ? urls : null;
        }

        return alibabaOptions?.ReferenceUrls?.ToList();
    }

    private static JsonElement Property(JsonElement element, string name)
    {
        return
            element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value)
            ? value
            : default;
    }

    private static string? String(JsonElement element, string name)
    {
        return Property(element, name) is { ValueKind: JsonValueKind.String } value
            ? value.GetString()
            : null;
    }

    // JSON.stringify of the parsed create-task response: only the fields of its schema.
    private static string KnownFields(JsonElement root)
    {
        var known = new JsonObject();
        foreach (var name in CreateTaskFields)
        {
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out var value))
            {
                known[name] = JsonNode.Parse(value.GetRawText());
            }
        }

        return known.ToJsonString();
    }

    private static void CopyUsage(JsonElement usage, string source, JsonObject target, string name)
    {
        if (usage.TryGetProperty(source, out var value))
        {
            target[name] = JsonNode.Parse(value.GetRawText());
        }
    }

    private static void CopyUsageWhenSet(
        JsonElement usage,
        string source,
        JsonObject target,
        string name
    )
    {
        if (usage.TryGetProperty(source, out var value) && value.ValueKind != JsonValueKind.Null)
        {
            target[name] = JsonNode.Parse(value.GetRawText());
        }
    }

    // String.prototype.replace with a string pattern replaces the first match only.
    private static string ReplaceFirst(string value, char oldChar, char newChar)
    {
        var index = value.IndexOf(oldChar);
        return index < 0 ? value : value.Substring(0, index) + newChar + value.Substring(index + 1);
    }

    private DateTime CurrentDate()
    {
        return _config.CurrentDate?.Invoke() ?? DateTime.UtcNow;
    }

    private (
        JsonObject Input,
        JsonObject Parameters,
        List<OperationWarning> Warnings,
        AlibabaVideoModelOptions? AlibabaOptions
    ) BuildRequest(VideoModelCall options)
    {
        var warnings = new List<OperationWarning>();
        var requestConfig = AlibabaModelCatalog.GetAlibabaVideoModelEntry(ModelId).RequestConfig;
        var mode = requestConfig.Mode;

        var alibabaOptions = AlibabaVideoModelOptions.Parse(options.ProviderOptions);

        // Build input object
        var input = new JsonObject();

        if (options.Prompt != null)
        {
            input["prompt"] = options.Prompt;
        }

        if (alibabaOptions?.NegativePrompt != null)
        {
            input["negative_prompt"] = alibabaOptions.NegativePrompt;
        }

        if (alibabaOptions?.AudioUrl != null)
        {
            input["audio_url"] = alibabaOptions.AudioUrl;
        }

        var startImage = ResolveStartImage(options);
        var protocol = requestConfig.Protocol;
        var wan27 = protocol == "wan27";
        var wan3 = protocol == "wan3";
        // Resolution tiers and input.media replaced size/img_url from wan2.7 on.
        var tieredProtocol = wan27 || wan3;
        // wan2.7 T2V and R2V take an explicit aspect ratio (I2V follows the input image); wan3 serves
        // every mode from one id, so it always takes one.
        var supportsRatio = wan3 || (wan27 && mode != "i2v");

        // Handle image input for I2V mode (wan3 carries frames in input.media)
        if (!wan3 && mode == "i2v" && startImage != null)
        {
            input["img_url"] = FileToImageString(startImage);
        }

        if (wan3)
        {
            // The media the request carries decides whether this is text-, image-, or
            // reference-to-video, so there is no mode to read off the id. A bare prompt stays
            // text-to-video.
            var media = ResolveMedia(
                options,
                alibabaOptions,
                warnings,
                startImage,
                GetLastFrameImage(options)
            );
            if (media != null)
            {
                input["media"] = media;
            }
        }
        else if (mode == "r2v")
        {
            if (wan27)
            {
                // wan2.7: input.media
                var media = ResolveMedia(
                    options,
                    alibabaOptions,
                    warnings,
                    GetFirstFrameImage(options),
                    null
                );
                if (media != null)
                {
                    input["media"] = media;
                }
            }
            else
            {
                // wan2.6: legacy protocol with input.reference_urls
                var referenceUrls = ResolveReferenceUrls(options, alibabaOptions, warnings);
                if (referenceUrls is { Count: > 0 })
                {
                    var urls = new JsonArray();
                    foreach (var url in referenceUrls)
                    {
                        urls.Add(url);
                    }

                    input["reference_urls"] = urls;
                }
            }
        }

        var lastFrame = GetLastFrameImage(options);

        // wan3 has a real closing-frame slot, filled in input.media above.
        if (lastFrame != null && !wan3)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "frameImages",
                    "This model does not support last_frame. The last frame image was ignored."
                )
            );
        }

        if (options.InputReferences is { Count: > 0 } && mode != "r2v" && !wan3)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "inputReferences",
                    "Alibaba only supports inputReferences (reference-to-video) on "
                        + "reference-to-video models. The reference images were ignored."
                )
            );
        }

        // Build parameters object
        var parameters = new JsonObject();

        if (options.Duration != null)
        {
            parameters["duration"] = options.Duration;
        }

        if (options.Seed != null)
        {
            parameters["seed"] = options.Seed;
        }

        // Resolution / Size mapping
        if (options.Resolution != null)
        {
            if (mode == "i2v" || tieredProtocol)
            {
                // I2V, wan2.7, and wan3 use the "720P" / "1080P" tier format
                var resolutionTier = ResolutionTierMap.TryGetValue(
                    options.Resolution,
                    out var mapped
                )
                    ? mapped
                    : options.Resolution;
                // wan3 adds a 480P tier to wan2.7's two.
                string[] supportedTiers = wan3 ? ["480P", "720P", "1080P"] : ["720P", "1080P"];
                if (tieredProtocol && Array.IndexOf(supportedTiers, resolutionTier) < 0)
                {
                    warnings.Add(
                        OperationWarning.Unsupported(
                            "resolution",
                            (wan3 ? "wan3" : "wan2.7")
                                + " models only support the "
                                + string.Join(", ", supportedTiers)
                                + " resolution tiers. The resolution \""
                                + options.Resolution
                                + "\" was ignored."
                        )
                    );
                }
                else
                {
                    parameters["resolution"] = resolutionTier;
                }
            }
            else
            {
                // wan2.6 T2V and R2V use "WIDTH*HEIGHT" format for the size parameter.
                // Convert "WIDTHxHEIGHT" (SDK standard) to "WIDTH*HEIGHT" (Alibaba API).
                parameters["size"] = ReplaceFirst(options.Resolution, 'x', '*');
            }
        }

        // wan2.7 T2V and R2V support an explicit aspect ratio parameter
        if (supportsRatio)
        {
            var ratio = alibabaOptions?.Ratio
                ?? options.AspectRatio
                ?? (
                    options.Resolution != null
                        ? DeriveRatioFromResolution(options.Resolution)
                        : null
                );
            if (ratio != null)
            {
                parameters["ratio"] = ratio;
            }
        }

        // Provider-specific parameters
        if (alibabaOptions?.PromptExtend != null)
        {
            parameters["prompt_extend"] = alibabaOptions.PromptExtend;
        }

        if (alibabaOptions?.ShotType != null)
        {
            if (tieredProtocol)
            {
                // wan2.7 removed shot_type; shot structure is described in the prompt
                warnings.Add(
                    OperationWarning.Unsupported(
                        "shotType",
                        (wan3 ? "wan3" : "wan2.7")
                            + " models do not support the shotType option. "
                            + "Describe the shot structure in the prompt instead."
                    )
                );
            }
            else
            {
                parameters["shot_type"] = alibabaOptions.ShotType;
            }
        }

        if (alibabaOptions?.Watermark != null)
        {
            parameters["watermark"] = alibabaOptions.Watermark;
        }

        var audio = options.GenerateAudio ?? alibabaOptions?.Audio;
        if (audio != null)
        {
            if (wan27)
            {
                // wan2.7 does not have an audio parameter (audio is always generated)
                warnings.Add(
                    OperationWarning.Unsupported(
                        "generateAudio",
                        "wan2.7 models always generate audio. The audio option was ignored."
                    )
                );
            }
            else
            {
                parameters["audio"] = audio;
            }
        }

        // Warn about unsupported standard options
        if (!string.IsNullOrEmpty(options.AspectRatio) && !supportsRatio)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "aspectRatio",
                    "Alibaba video models use explicit size/resolution dimensions. Use the resolution option or providerOptions.alibaba for size control."
                )
            );
        }

        if (options.Fps is { } fps && fps != 0)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "fps",
                    "Alibaba video models do not support custom FPS."
                )
            );
        }

        if (options.N > 1)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "n",
                    "Alibaba video models only support generating 1 video per call."
                )
            );
        }

        return (input, parameters, warnings, alibabaOptions);
    }

    private VideoStatusResult BuildCompletedResult(
        JsonElement statusResponse,
        ProviderResponse response,
        IReadOnlyList<OperationWarning> warnings
    )
    {
        var output = Property(statusResponse, "output");
        var taskId = String(output, "task_id");
        var videoUrl = String(output, "video_url");

        if (string.IsNullOrEmpty(videoUrl))
        {
            throw new AiSdkException("No video URL in response. Task ID: " + taskId);
        }

        var alibaba = new JsonObject { ["taskId"] = taskId, ["videoUrl"] = videoUrl };
        if (String(output, "actual_prompt") is { Length: > 0 } actualPrompt)
        {
            alibaba["actualPrompt"] = actualPrompt;
        }

        var usage = Property(statusResponse, "usage");
        if (usage.ValueKind == JsonValueKind.Object)
        {
            var usageMetadata = new JsonObject();
            CopyUsage(usage, "duration", usageMetadata, "duration");
            CopyUsage(usage, "output_video_duration", usageMetadata, "outputVideoDuration");
            CopyUsage(usage, "SR", usageMetadata, "resolution");
            CopyUsage(usage, "size", usageMetadata, "size");
            // wan3-only. Left out rather than set to null so the metadata shape for older wan
            // models is unchanged.
            CopyUsageWhenSet(usage, "input_video_duration", usageMetadata, "inputVideoDuration");
            CopyUsageWhenSet(usage, "fps", usageMetadata, "fps");
            CopyUsageWhenSet(usage, "ratio", usageMetadata, "ratio");
            alibaba["usage"] = usageMetadata;
        }

        return new VideoStatusResult(
            "completed",
            [new VideoOutput("url", videoUrl, mediaType: "video/mp4")],
            warnings: warnings,
            providerMetadata: OperationJson.Parse(
                new JsonObject { ["alibaba"] = alibaba }.ToJsonString()
            ),
            response: response
        );
    }
}
