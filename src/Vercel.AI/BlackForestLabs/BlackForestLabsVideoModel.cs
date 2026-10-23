// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.GenerateText;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.BlackForestLabs.BlackForestLabsApi;

namespace Vercel.AI.BlackForestLabs;

/// <summary>
/// Black Forest Labs FLUX 3 video model. <see cref="DoStartAsync"/> submits a job and <see cref="DoStatusAsync"/> reads it.
/// The video is returned as a signed URL and is never downloaded. The operation is a JSON object with the
/// <c>requestId</c>, the <c>pollingUrl</c>, and the estimates of the submit response.
/// </summary>
public sealed class BlackForestLabsVideoModel : IVideoModel, IVideoCaller
{
    private const int DefaultPollIntervalMillis = 2000;
    private const int DefaultPollTimeoutMillis = 600000;
    private const int MinDurationSeconds = 5;
    private const int MaxDurationSeconds = 20;

    /// <summary>Keyframe counts at or above this need an explicit duration when untimed.</summary>
    private const int UntimedKeyframesNeedingDuration = 3;

    private static readonly string[] AspectRatios = { "21:9", "2:1", "16:9", "4:3", "1:1", "3:4", "9:16", "auto" };

    private static readonly string[] Resolutions = { "hd", "fhd" };

    private static readonly Regex Dimensions = new Regex("^([0-9]+)x([0-9]+)\\z", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    private readonly BlackForestLabsProvider _provider;
    private readonly Func<DateTimeOffset> _clock;

    /// <summary>Creates a model. <paramref name="clock"/> sets the response timestamp.</summary>
    public BlackForestLabsVideoModel(BlackForestLabsProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider => "black-forest-labs.video";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Black Forest Labs returns one video per call.</summary>
    public int? MaxVideosPerCall => 1;

    /// <inheritdoc />
    public bool CanGenerate => true;

    /// <inheritdoc />
    public bool CanStart => true;

    /// <inheritdoc />
    public bool CanStatus => true;

    /// <inheritdoc />
    public bool CanWebhook => false;

    /// <inheritdoc />
    public Task<int?> ResolveMaxVideosPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxVideosPerCall);
    }

    /// <inheritdoc />
    public async Task<VideoResult> DoGenerateAsync(VideoCallOptions options, CancellationToken cancellationToken)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var call = new VideoModelCall(options.Prompt, 1, null, null, options.DurationSeconds, null, null, null, null, null, null, OperationJson.Parse("{}"), new Dictionary<string, string>(), cancellationToken);
        var video = (await DoGenerateAsync(call, cancellationToken).ConfigureAwait(false)).Videos[0];
        return new VideoResult(video.Url, null, video.MediaType!);
    }

    /// <summary>Starts a job and polls it until the video is ready.</summary>
    public async Task<VideoModelResult> DoGenerateAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var started = await DoStartAsync(call, cancellationToken).ConfigureAwait(false);
        var operation = (JsonElement)started.Operation!;
        var requestId = operation.GetProperty("requestId").GetString();
        var pollIntervalMillis = _provider.PollIntervalMillis ?? DefaultPollIntervalMillis;
        var pollTimeoutMillis = _provider.PollTimeoutMillis ?? DefaultPollTimeoutMillis;
        var statusCall = new VideoModelCall(null, 1, null, null, null, null, null, null, null, null, null, OperationJson.Parse("{}"), call.Headers, cancellationToken, operation: operation);
        var stopwatch = Stopwatch.StartNew();

        while (true)
        {
            await Task.Delay(pollIntervalMillis, cancellationToken).ConfigureAwait(false);

            if (stopwatch.ElapsedMilliseconds > pollTimeoutMillis)
            {
                throw new AiSdkException(
                    "Black Forest Labs video generation timed out after " + pollTimeoutMillis.ToString(CultureInfo.InvariantCulture) + "ms. Request id: " + requestId);
            }

            var status = await DoStatusAsync(statusCall, cancellationToken).ConfigureAwait(false);
            if (status.Status == "pending")
            {
                continue;
            }

            if (status.Status == "error")
            {
                throw new AiSdkException(status.Error!);
            }

            return new VideoModelResult(status.Videos, started.Warnings.Concat(status.Warnings).ToList(), status.ProviderMetadata, status.Response);
        }
    }

    /// <inheritdoc />
    public async Task<VideoStartResult> DoStartAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var (body, warnings) = GetArgs(call);
        var timestamp = _clock();
        var headers = _provider.CreateHeaders(call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value));
        var submitUrl = _provider.Options.BaseUrl.TrimEnd('/') + "/" + ModelId;

        ProviderExchangeResult submitted;
        try
        {
            submitted = await ProviderExchange.SendAsync(_provider.HttpClient, HttpMethod.Post, new Uri(submitUrl), ProviderExchange.Json(body.ToJsonString()), headers, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException exception)
        {
            throw Failed(exception, submitUrl);
        }

        using var submit = ParseSubmit(submitted.Body);
        var root = submit.RootElement;
        var operation = new JsonObject
        {
            ["requestId"] = root.GetProperty("id").GetString(),
            ["pollingUrl"] = root.GetProperty("polling_url").GetString(),
        };
        AddNumber(operation, "cost", Number(root, "cost"));
        AddNumber(operation, "inputMegapixels", Number(root, "input_mp"));
        AddNumber(operation, "outputMegapixels", Number(root, "output_mp"));
        return new VideoStartResult(
            OperationJson.Parse(operation.ToJsonString()),
            warnings,
            response: new ProviderResponse(submitted.Headers, timestamp: timestamp.UtcDateTime, modelId: ModelId));
    }

    /// <inheritdoc />
    public async Task<VideoStatusResult> DoStatusAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var timestamp = _clock();
        var operation = ReadOperation(call.Operation);
        var baseUrl = _provider.Options.BaseUrl.TrimEnd('/');
        var url = WithRequestId(operation.PollingUrl, operation.RequestId);
        var headers = _provider.CreateHeaders(call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value));

        // The polling URL comes from the provider response. Credentials only go to a trusted provider host.
        var trusted = IsTrustedUrl(url.AbsoluteUri, baseUrl);
        ProviderExchangeResult response;
        try
        {
            response = await ProviderExchange.GetResponseUrlAsync(
                _provider.HttpClient,
                url.AbsoluteUri,
                trusted ? url.AbsoluteUri : baseUrl,
                DownloadUrls.ValidateDownloadUrl,
                trusted ? headers : null,
                cancellationToken,
                ProviderExchange.DownloadClient(_provider.Options),
                UserAgentOnly(headers)).ConfigureAwait(false);
        }
        catch (ApiException exception)
        {
            throw Failed(exception, url.AbsoluteUri);
        }

        using var document = ParsePoll(response.Body, out var status);
        var root = document.RootElement;
        var providerResponse = new ProviderResponse(response.Headers, timestamp: timestamp.UtcDateTime, modelId: ModelId);
        var cost = Number(root, "cost") ?? operation.Cost;

        if (status == "Ready")
        {
            var result = root.TryGetProperty("result", out var found) ? ReadResult(found) : null;
            if (result == null)
            {
                throw new AiSdkException(
                    "Black Forest Labs reported the video as Ready but returned no result.sample URL. Request id: " + operation.RequestId);
            }

            var sample = result.Value.GetProperty("sample").GetString()!;
            var metadata = new JsonObject { ["id"] = operation.RequestId, ["videoUrl"] = sample };
            AddNumber(metadata, "seed", Number(result.Value, "seed"));
            AddNumber(metadata, "start_time", Number(result.Value, "start_time"));
            AddNumber(metadata, "end_time", Number(result.Value, "end_time"));
            AddNumber(metadata, "duration", Number(result.Value, "duration"));
            if (StringProperty(result.Value, "draft_cache") is { } draftCache)
            {
                metadata["draftCache"] = draftCache;
            }

            AddNumber(metadata, "cost", cost);
            AddNumber(metadata, "inputMegapixels", operation.InputMegapixels);
            AddNumber(metadata, "outputMegapixels", operation.OutputMegapixels);
            var providerMetadata = new JsonObject { ["blackForestLabs"] = new JsonObject { ["videos"] = new JsonArray(metadata) } };
            return new VideoStatusResult(
                "completed",
                new[] { new VideoOutput("url", sample, mediaType: "video/mp4") },
                providerMetadata: OperationJson.Parse(providerMetadata.ToJsonString()),
                response: providerResponse);
        }

        if (Array.IndexOf(TerminalStatuses, status) >= 0)
        {
            var detail = root.TryGetProperty("details", out var details) ? DescribeDetails(details) : null;
            return new VideoStatusResult(
                "error",
                error: "Black Forest Labs video generation failed with status \"" + status + "\""
                    + (detail != null ? ": " + detail : string.Empty)
                    + ". Request id: " + operation.RequestId,
                response: providerResponse);
        }

        return new VideoStatusResult("pending", response: providerResponse);
    }

    /// <inheritdoc />
    public Task<VideoWebhook> HandleWebhookAsync(Func<Task<VideoWebhook>> webhook, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Black Forest Labs does not support video webhooks.");
    }

    private static VideoOperation ReadOperation(object? operation)
    {
        if (operation is JsonElement { ValueKind: JsonValueKind.Object } element
            && StringProperty(element, "requestId") is { } requestId
            && StringProperty(element, "pollingUrl") is { } pollingUrl
            && Uri.TryCreate(pollingUrl, UriKind.Absolute, out _))
        {
            return new VideoOperation(requestId, pollingUrl, Number(element, "cost"), Number(element, "inputMegapixels"), Number(element, "outputMegapixels"));
        }

        throw new InvalidArgumentException("operation", operation, "Black Forest Labs operations are objects with a requestId and a pollingUrl.");
    }

    /// <summary>The poll response. Unlike the image poll, the status is any string and the result is checked once the video is ready.</summary>
    private static JsonDocument ParsePoll(string text, out string status)
    {
        var document = Parse(text);
        var root = document.RootElement;
        string? found = null;
        var valid = root.ValueKind == JsonValueKind.Object;
        if (valid)
        {
            foreach (var name in new[] { "status", "state" })
            {
                if (root.TryGetProperty(name, out var value))
                {
                    valid &= value.ValueKind == JsonValueKind.String;
                    found ??= value.GetString();
                }
            }

            valid &= found != null && NullableNumber(root, "cost");
        }

        if (!valid)
        {
            document.Dispose();
            throw new InvalidResponseDataException(text, found == null ? "Missing status in Black Forest Labs poll response" : "Invalid Black Forest Labs poll response");
        }

        status = found!;
        return document;
    }

    /// <summary>The ready result, or null when it does not carry a valid <c>sample</c> URL.</summary>
    private static JsonElement? ReadResult(JsonElement result)
    {
        var valid = result.ValueKind == JsonValueKind.Object
            && result.TryGetProperty("sample", out var sample) && IsUrl(sample)
            && NullableNumber(result, "seed") && NullableNumber(result, "start_time")
            && NullableNumber(result, "end_time") && NullableNumber(result, "duration")
            && (!result.TryGetProperty("draft_cache", out var draftCache) || draftCache.ValueKind is JsonValueKind.Null or JsonValueKind.String);
        return valid ? result : null;
    }

    /// <summary>A failed poll carries a <c>details</c> payload that is a plain string for moderation refusals and an object for everything else.</summary>
    private static string? DescribeDetails(JsonElement details)
    {
        return details.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.String => details.GetString(),
            _ => JsonNode.Parse(details.GetRawText())!.ToJsonString(),
        };
    }

    private (JsonObject Body, List<OperationWarning> Warnings) GetArgs(VideoModelCall call)
    {
        var warnings = new List<OperationWarning>();
        var options = VideoOptions.Parse(call.ProviderOptions);

        if (options.DraftCache != null)
        {
            return GetDraftEnhanceArgs(call, options);
        }

        if (call.Fps != null)
        {
            warnings.Add(OperationWarning.Unsupported("fps", "FLUX 3 video does not support a custom frame rate."));
        }

        if (call.Seed != null)
        {
            warnings.Add(OperationWarning.Unsupported("seed", "FLUX 3 video does not accept a seed."));
        }

        if (call.N > 1)
        {
            warnings.Add(OperationWarning.Unsupported("n", "FLUX 3 video generates a single video per call. Only 1 video will be generated."));
        }

        // Resolution: an explicit provider option is already a tier, so it wins and
        // a top-level value only has to be resolved when it is the sole source.
        var resolution = options.Resolution;
        if (call.Resolution != null)
        {
            var resolved = ResolveTopLevelResolution(call.Resolution);
            if (resolution != null)
            {
                if (resolved == null)
                {
                    warnings.Add(OperationWarning.Unsupported(
                        "resolution",
                        "Unrecognized resolution \"" + call.Resolution + "\". FLUX 3 video supports \"hd\" and \"fhd\", "
                            + "so providerOptions.blackForestLabs.resolution (\"" + resolution + "\") was used instead."));
                }
            }
            else if (resolved == null)
            {
                warnings.Add(OperationWarning.Unsupported(
                    "resolution",
                    "Unrecognized resolution \"" + call.Resolution + "\". FLUX 3 video supports "
                        + "\"hd\" and \"fhd\", or a {width}x{height} value to map onto one."));
            }
            else
            {
                resolution = resolved.Value.Tier;
                if (resolved.Value.Derived)
                {
                    warnings.Add(new OperationWarning(
                        "compatibility",
                        feature: "resolution",
                        details: "FLUX 3 video renders at \"hd\" or \"fhd\"; the requested resolution "
                            + "\"" + call.Resolution + "\" was mapped to \"" + resolved.Value.Tier + "\"."));
                }
            }
        }

        // Aspect ratio: the provider option can also express `auto`, so it wins.
        var aspectRatio = options.AspectRatio;
        if (aspectRatio == null && call.AspectRatio != null)
        {
            if (Array.IndexOf(AspectRatios, call.AspectRatio) >= 0)
            {
                aspectRatio = call.AspectRatio;
            }
            else
            {
                warnings.Add(OperationWarning.Unsupported(
                    "aspectRatio",
                    "FLUX 3 video does not support the aspect ratio \"" + call.AspectRatio + "\". "
                        + "Using the provider default (auto)."));
            }
        }

        // Resolve first/last frame inputs. A standalone `image` is treated as the first frame (image-to-video).
        // The core sets `image` to the `first_frame` frame image when there is one, so the frame-image entry is
        // tracked separately to report which input a warning came from.
        var firstFrameImage = call.FrameImages?.FirstOrDefault(frame => frame.FrameType == "first_frame")?.Image;
        var firstFrame = firstFrameImage ?? call.Image;
        var lastFrame = call.FrameImages?.FirstOrDefault(frame => frame.FrameType == "last_frame")?.Image;

        var firstFrameMediaType = firstFrame != null ? NonImageFrameMediaType(firstFrame) : null;
        if (firstFrame != null && firstFrameMediaType != null)
        {
            warnings.Add(OperationWarning.Unsupported(
                firstFrameImage != null ? "frameImages" : "image",
                firstFrameMediaType == "video"
                    ? "FLUX 3 video does not accept a video as a keyframe. Pass it as an inputReference to continue from it instead."
                    : "FLUX 3 video only accepts an image as a keyframe; the \"" + firstFrame.MediaType + "\" file was ignored."));
            firstFrame = null;
        }

        if (lastFrame != null)
        {
            if (firstFrame == null)
            {
                // Keyframes are positional: the first entry opens the clip, so there is no way to send a closing frame on its own.
                warnings.Add(OperationWarning.Unsupported(
                    "frameImages",
                    "FLUX 3 video requires a first_frame when a last_frame is provided. The last_frame was ignored."));
                lastFrame = null;
            }
            else
            {
                var lastFrameMediaType = NonImageFrameMediaType(lastFrame);
                if (lastFrameMediaType != null)
                {
                    warnings.Add(OperationWarning.Unsupported(
                        "frameImages",
                        lastFrameMediaType == "video"
                            ? "FLUX 3 video does not accept a video as a keyframe. The last_frame video was ignored."
                            : "FLUX 3 video only accepts an image as a keyframe; the \"" + lastFrame.MediaType + "\" last_frame was ignored."));
                    lastFrame = null;
                }
            }
        }

        // Keyframes: the provider option covers the shapes the top-level fields cannot express
        // (3+ images, images pinned to a second), so it wins.
        var keyframes = options.Keyframes is { Count: > 0 } provided ? provided.ToList() : null;
        if (keyframes != null)
        {
            if (firstFrame != null || lastFrame != null)
            {
                warnings.Add(OperationWarning.Unsupported(
                    call.FrameImages != null ? "frameImages" : "image",
                    "FLUX 3 video takes a single keyframe list. providerOptions.blackForestLabs.keyframes "
                        + "was used and the top-level frame images were ignored."));
            }
        }
        else if (firstFrame != null)
        {
            keyframes = new List<JsonNode?> { JsonValue.Create(ToBlackForestLabsFile(firstFrame)) };
            if (lastFrame != null)
            {
                keyframes.Add(JsonValue.Create(ToBlackForestLabsFile(lastFrame)));
            }
        }

        // Video continuation. FLUX 3 takes a single `start_video` and has no
        // reference-image concept, so image references cannot be honored.
        string? startVideo = null;
        var referenceVideos = new List<VideoModelFile>();
        foreach (var file in call.InputReferences ?? Array.Empty<VideoModelFile>())
        {
            var topLevelMediaType = file.MediaType != null ? MediaTypes.GetTopLevelMediaType(file.MediaType) : null;
            if (topLevelMediaType == "video")
            {
                referenceVideos.Add(file);
            }
            else if (topLevelMediaType == null)
            {
                warnings.Add(new OperationWarning(
                    "compatibility",
                    feature: "inputReferences",
                    details: "FLUX 3 video only accepts a video reference, so the reference with no mediaType "
                        + "was treated as the video to continue from. Pass { url, mediaType: \"video/mp4\" } to be explicit."));
                referenceVideos.Add(file);
            }
            else if (topLevelMediaType == "image")
            {
                warnings.Add(OperationWarning.Unsupported(
                    "inputReferences",
                    "FLUX 3 video has no reference-image input. Pass images as `image`, `frameImages`, or "
                        + "providerOptions.blackForestLabs.keyframes instead. The reference was ignored."));
            }
            else
            {
                warnings.Add(OperationWarning.Unsupported(
                    "inputReferences",
                    "FLUX 3 video only accepts a video reference; the \"" + file.MediaType + "\" reference was ignored."));
            }
        }

        if (referenceVideos.Count > 0)
        {
            if (keyframes != null)
            {
                // Continuation and keyframes are mutually exclusive modes.
                warnings.Add(OperationWarning.Unsupported(
                    "inputReferences",
                    "FLUX 3 video cannot combine keyframes with a video to continue from. The video reference was ignored."));
            }
            else
            {
                startVideo = ToBlackForestLabsFile(referenceVideos[0]);
                if (referenceVideos.Count > 1)
                {
                    warnings.Add(OperationWarning.Unsupported(
                        "inputReferences",
                        "FLUX 3 video continues from a single video. Only the first video reference was used."));
                }
            }
        }

        // Duration: whole seconds between 5 and 20, or omitted to let the API pick one that matches the content.
        var duration = call.Duration;
        if (duration != null)
        {
            var requested = FormatNumber(call.Duration!.Value);
            if (Math.Floor(duration.Value) != duration.Value)
            {
                // Math.Round would round half to even. JavaScript rounds half up.
                var rounded = Math.Floor(duration.Value + 0.5);
                warnings.Add(OperationWarning.Unsupported(
                    "duration",
                    "FLUX 3 video requires a whole number of seconds. The requested duration of "
                        + FormatNumber(duration.Value) + " was rounded to " + FormatNumber(rounded) + "."));
                duration = rounded;
            }

            if (duration > MaxDurationSeconds)
            {
                warnings.Add(OperationWarning.Unsupported(
                    "duration",
                    "FLUX 3 video supports at most " + MaxDurationSeconds.ToString(CultureInfo.InvariantCulture)
                        + " seconds. The requested duration of " + requested + " was clamped to " + MaxDurationSeconds.ToString(CultureInfo.InvariantCulture) + "."));
                duration = MaxDurationSeconds;
            }
            else if (duration < MinDurationSeconds)
            {
                warnings.Add(OperationWarning.Unsupported(
                    "duration",
                    "FLUX 3 video requires at least " + MinDurationSeconds.ToString(CultureInfo.InvariantCulture)
                        + " seconds. The requested duration of " + requested + " was clamped to " + MinDurationSeconds.ToString(CultureInfo.InvariantCulture) + "."));
                duration = MinDurationSeconds;
            }
        }

        var untimedKeyframeCount = keyframes?.Count(keyframe => keyframe?.GetValueKind() == JsonValueKind.String) ?? 0;
        if (duration == null && untimedKeyframeCount >= UntimedKeyframesNeedingDuration)
        {
            throw new InvalidArgumentException(
                "duration",
                null,
                "FLUX 3 video requires an explicit duration when " + UntimedKeyframesNeedingDuration.ToString(CultureInfo.InvariantCulture)
                    + " or more keyframes are sent without a timestamp.");
        }

        var mode = keyframes != null ? "i2v" : startVideo != null ? "v2v" : "t2v";
        var body = new JsonObject { ["mode"] = mode, ["prompt"] = call.Prompt ?? string.Empty };
        Set(body, "aspect_ratio", aspectRatio);
        if (duration != null)
        {
            body["duration"] = (long)duration.Value;
        }

        Set(body, "resolution", resolution);
        Set(body, "version", options.Version);
        if (call.GenerateAudio != null)
        {
            body["generate_audio"] = call.GenerateAudio.Value;
        }

        if (options.SafetyTolerance != null)
        {
            body["safety_tolerance"] = options.SafetyTolerance.Value;
        }

        if (options.Draft != null)
        {
            body["draft"] = options.Draft.Value;
        }

        if (mode == "i2v")
        {
            body["keyframes"] = new JsonArray(keyframes!.ToArray());
        }
        else if (mode == "v2v")
        {
            body["start_video"] = startVideo;
        }

        return (body, warnings);
    }

    /// <summary>
    /// Draft-enhance replays an encrypted bundle from a prior <c>draft</c> generation at full quality. The bundle
    /// pins the original mode, prompt, seed, and conditioning media, and the API accepts nothing alongside it but
    /// <c>safety_tolerance</c>. So this is a separate body rather than one more field, and everything else the
    /// caller set is reported as dropped.
    /// </summary>
    private static (JsonObject Body, List<OperationWarning> Warnings) GetDraftEnhanceArgs(VideoModelCall call, VideoOptions options)
    {
        var warnings = new List<OperationWarning>();
        var pinnedByBundle = new (string Feature, bool IsSet)[]
        {
            ("prompt", (call.Prompt?.Trim().Length ?? 0) > 0),
            ("aspectRatio", call.AspectRatio != null || options.AspectRatio != null),
            ("resolution", call.Resolution != null || options.Resolution != null),
            ("duration", call.Duration != null),
            ("fps", call.Fps != null),
            ("seed", call.Seed != null),
            ("generateAudio", call.GenerateAudio != null),
            ("image", call.Image != null),
            ("frameImages", (call.FrameImages?.Count ?? 0) > 0),
            ("inputReferences", (call.InputReferences?.Count ?? 0) > 0),
            ("keyframes", (options.Keyframes?.Count ?? 0) > 0),
            ("version", options.Version != null),
        };

        foreach (var (feature, isSet) in pinnedByBundle)
        {
            if (isSet)
            {
                warnings.Add(OperationWarning.Unsupported(
                    feature,
                    "FLUX 3 draft enhance replays the draft bundle as it was generated, so \"" + feature + "\" "
                        + "was ignored. Set it on the original draft request instead."));
            }
        }

        if (options.Draft != null)
        {
            warnings.Add(OperationWarning.Unsupported("draft", "FLUX 3 draft enhance always renders at full quality. The draft option was ignored."));
        }

        if (call.N > 1)
        {
            warnings.Add(OperationWarning.Unsupported("n", "FLUX 3 video generates a single video per call. Only 1 video will be generated."));
        }

        var body = new JsonObject { ["mode"] = "draft_enhance", ["draft_cache"] = options.DraftCache };
        if (options.SafetyTolerance != null)
        {
            body["safety_tolerance"] = options.SafetyTolerance.Value;
        }

        return (body, warnings);
    }

    /// <summary>
    /// The API takes a named tier (<c>hd</c>/<c>fhd</c>) while the top-level <c>resolution</c> is <c>{width}x{height}</c>,
    /// so a caller may reasonably pass either.
    /// </summary>
    private static (string Tier, bool Derived)? ResolveTopLevelResolution(string resolution)
    {
        var named = resolution.ToLowerInvariant();
        if (Array.IndexOf(Resolutions, named) >= 0)
        {
            return (named, false);
        }

        var match = Dimensions.Match(resolution);
        if (!match.Success)
        {
            return null;
        }

        var shorterSide = Math.Min(
            double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
            double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture));
        return (shorterSide <= 720 ? "hd" : "fhd", shorterSide != 720 && shorterSide != 1080);
    }

    private static string? NonImageFrameMediaType(VideoModelFile file)
    {
        if (file.MediaType == null)
        {
            return null;
        }

        var topLevelMediaType = MediaTypes.GetTopLevelMediaType(file.MediaType);
        return topLevelMediaType == "image" ? null : topLevelMediaType;
    }

    /// <summary>The API takes conditioning media as an http(s) URL or a bare base64 string, not a data URI.</summary>
    private static string ToBlackForestLabsFile(VideoModelFile file)
    {
        return file.Type == "url" ? file.Url! : Convert.ToBase64String(file.Data ?? Array.Empty<byte>());
    }

    private static string FormatNumber(double value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    private static void AddNumber(JsonObject target, string name, JsonElement? value)
    {
        if (value != null)
        {
            target[name] = JsonNode.Parse(value.Value.GetRawText());
        }
    }

    private static void Set(JsonObject target, string name, string? value)
    {
        if (value != null)
        {
            target[name] = value;
        }
    }

    private sealed class VideoOperation
    {
        public VideoOperation(string requestId, string pollingUrl, JsonElement? cost, JsonElement? inputMegapixels, JsonElement? outputMegapixels)
        {
            RequestId = requestId;
            PollingUrl = pollingUrl;
            Cost = cost;
            InputMegapixels = inputMegapixels;
            OutputMegapixels = outputMegapixels;
        }

        public string RequestId { get; }

        public string PollingUrl { get; }

        public JsonElement? Cost { get; }

        public JsonElement? InputMegapixels { get; }

        public JsonElement? OutputMegapixels { get; }
    }

    /// <summary>The <c>blackForestLabs</c> provider options after validation. Unknown keys are dropped.</summary>
    private sealed class VideoOptions
    {
        public string? Resolution { get; private set; }

        public string? AspectRatio { get; private set; }

        /// <summary>Plain strings, or <c>[seconds, image]</c> pairs.</summary>
        public IReadOnlyList<JsonNode?>? Keyframes { get; private set; }

        public int? SafetyTolerance { get; private set; }

        public bool? Draft { get; private set; }

        public string? DraftCache { get; private set; }

        public string? Version { get; private set; }

        public static VideoOptions Parse(JsonElement providerOptions)
        {
            var options = new VideoOptions();
            if (providerOptions.ValueKind != JsonValueKind.Object
                || !providerOptions.TryGetProperty("blackForestLabs", out var raw)
                || raw.ValueKind == JsonValueKind.Null)
            {
                return options;
            }

            if (raw.ValueKind != JsonValueKind.Object)
            {
                throw Invalid("expected an object.");
            }

            foreach (var property in raw.EnumerateObject())
            {
                var value = property.Value;
                switch (property.Name)
                {
                    case "resolution":
                        options.Resolution = OneOf(value, "resolution", Resolutions);
                        break;
                    case "aspectRatio":
                        options.AspectRatio = OneOf(value, "aspectRatio", AspectRatios);
                        break;
                    case "keyframes":
                        options.Keyframes = ReadKeyframes(value);
                        break;
                    case "safetyTolerance":
                        options.SafetyTolerance = value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var tolerance) && tolerance >= 0 && tolerance <= 4
                            ? tolerance
                            : throw Invalid("safetyTolerance must be an integer from 0 to 4.");
                        break;
                    case "draft":
                        options.Draft = value.ValueKind is JsonValueKind.True or JsonValueKind.False
                            ? value.GetBoolean()
                            : throw Invalid("draft must be a boolean.");
                        break;
                    case "draftCache":
                        options.DraftCache = value.ValueKind == JsonValueKind.String ? value.GetString() : throw Invalid("draftCache must be a string.");
                        break;
                    case "version":
                        options.Version = OneOf(value, "version", "latest");
                        break;
                }
            }

            return options;
        }

        private static string OneOf(JsonElement value, string name, params string[] allowed)
        {
            return value.ValueKind == JsonValueKind.String && Array.IndexOf(allowed, value.GetString()) >= 0
                ? value.GetString()!
                : throw Invalid(name + " must be one of " + string.Join(", ", allowed) + ".");
        }

        private static List<JsonNode?> ReadKeyframes(JsonElement value)
        {
            if (value.ValueKind != JsonValueKind.Array || value.GetArrayLength() < 1 || value.GetArrayLength() > 10)
            {
                throw Invalid("keyframes must be an array of 1 to 10 entries.");
            }

            var items = value.EnumerateArray().ToList();
            if (items.All(item => item.ValueKind == JsonValueKind.String))
            {
                return items.Select(item => (JsonNode?)JsonValue.Create(item.GetString())).ToList();
            }

            var timed = new List<JsonNode?>();
            double? previous = null;
            foreach (var item in items)
            {
                if (item.ValueKind != JsonValueKind.Array || item.GetArrayLength() != 2)
                {
                    throw Invalid("keyframes must be all images or all [seconds, image] pairs.");
                }

                var seconds = item[0];
                var image = item[1];
                if (seconds.ValueKind != JsonValueKind.Number || image.ValueKind != JsonValueKind.String)
                {
                    throw Invalid("keyframes must be all images or all [seconds, image] pairs.");
                }

                var time = seconds.GetDouble();
                if (time < 0 || time > 20)
                {
                    throw Invalid("keyframe timestamps must be between 0 and 20 seconds.");
                }

                if (previous != null && time <= previous)
                {
                    throw Invalid("Timed keyframes must be in chronological order.");
                }

                previous = time;
                timed.Add(JsonNode.Parse(item.GetRawText()));
            }

            return timed;
        }

        private static InvalidArgumentException Invalid(string detail)
        {
            return new InvalidArgumentException("providerOptions", null, "invalid blackForestLabs provider options: " + detail);
        }
    }
}
