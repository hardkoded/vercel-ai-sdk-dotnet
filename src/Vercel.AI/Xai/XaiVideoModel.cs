// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Xai;

/// <summary>
/// xAI video model. <see cref="DoStartAsync"/> creates a request and <see cref="DoStatusAsync"/> reads it.
/// It supports generation, editing, extension, and reference-to-video, and can deliver the video to a signed upload URL.
/// The operation is a JSON object with the <c>requestId</c> and, for upload delivery, <c>hasUploadUrl</c>.
/// </summary>
public sealed class XaiVideoModel : IVideoModel, IVideoCaller
{
    private const string Grok15 = "grok-imagine-video-1.5";

    // Generous bound for a `{status, progress}` payload of ~50 bytes.
    private const long MaxPendingBodyBytes = 1024 * 1024;

    private static readonly Dictionary<string, string> ResolutionMap = new(StringComparer.Ordinal)
    {
        ["1920x1080"] = "1080p",
        ["1280x720"] = "720p",
        ["854x480"] = "480p",
        ["640x480"] = "480p",
    };

    private readonly XaiProvider _provider;
    private readonly Func<DateTimeOffset> _clock;

    /// <summary>Creates a model. <paramref name="clock"/> sets the response timestamp.</summary>
    public XaiVideoModel(XaiProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider => "xai.video";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>xAI creates one video per request.</summary>
    public int? MaxVideosPerCall => 1;

    /// <inheritdoc />
    public bool CanGenerate => false;

    /// <inheritdoc />
    public bool CanStart => true;

    /// <inheritdoc />
    public bool CanStatus => true;

    /// <summary>False. xAI does not send webhooks to this model.</summary>
    public bool CanWebhook => false;

    /// <inheritdoc />
    public Task<int?> ResolveMaxVideosPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxVideosPerCall);
    }

    /// <summary>Starts a request and polls it until the video is ready.</summary>
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
        throw new NotSupportedException("xAI generates videos with doStart and doStatus.");
    }

    /// <inheritdoc />
    public Task<VideoWebhook> HandleWebhookAsync(
        Func<Task<VideoWebhook>> webhook,
        CancellationToken cancellationToken
    )
    {
        throw new NotSupportedException("xAI does not handle webhooks.");
    }

    /// <inheritdoc />
    public async Task<VideoStartResult> DoStartAsync(
        VideoModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var timestamp = _clock();
        var (body, warnings, xaiOptions, isEdit, isExtension) = BuildRequestBody(call);

        var baseUrl = _provider.Options.BaseUrl;

        // Determine the endpoint from the mode.
        var endpoint = ApiKeys.Combine(
            baseUrl,
            isEdit ? "/videos/edits"
                : isExtension ? "/videos/extensions"
                : "/videos/generations"
        );

        ProviderExchangeResult response;
        try
        {
            response = await ProviderExchange
                .SendAsync(
                    _provider.SharedHttpClient,
                    HttpMethod.Post,
                    endpoint,
                    ProviderExchange.Json(body.ToJsonString()),
                    ProviderExchange.Merge(_provider.CreateHeaders(), call.Headers),
                    cancellationToken
                )
                .ConfigureAwait(false);
        }
        catch (ApiException exception) when (xaiOptions?.OutputUploadUrl != null)
        {
            // The API error contains the full request and may echo it in its response. Keep retry semantics,
            // but discard credential-bearing details.
            var redacted = (JsonObject)body.DeepClone();
            redacted["output"] = new JsonObject { ["upload_url"] = "[REDACTED]" };
            throw new ApiException("xAI video upload request failed.", exception.StatusCode, null)
            {
                Url = endpoint.AbsoluteUri,
                RequestBodyValues = OperationJson.Parse(redacted.ToJsonString()),
                IsRetryable = exception.IsRetryable,
            };
        }

        var requestId = ReadRequestId(response.Body);
        if (string.IsNullOrEmpty(requestId))
        {
            throw new AiSdkException("No request_id returned from xAI API.");
        }

        var operation = new JsonObject { ["requestId"] = requestId };
        if (xaiOptions?.OutputUploadUrl != null)
        {
            operation["hasUploadUrl"] = true;
        }

        return new VideoStartResult(
            OperationJson.Parse(operation.ToJsonString()),
            warnings,
            response: Response(response, timestamp)
        );
    }

    /// <inheritdoc />
    public async Task<VideoStatusResult> DoStatusAsync(
        VideoModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var timestamp = _clock();
        var requestId = call.Operation is JsonElement { ValueKind: JsonValueKind.Object } operation
            ? String(operation, "requestId")
            : null;
        if (string.IsNullOrEmpty(requestId))
        {
            throw new InvalidArgumentException(
                "operation",
                call.Operation,
                "xAI operations are objects with a requestId."
            );
        }

        var hasUploadUrl =
            call.Operation is JsonElement { ValueKind: JsonValueKind.Object } source
            && source.TryGetProperty("hasUploadUrl", out var upload)
            && upload.ValueKind == JsonValueKind.True;

        var baseUrl = _provider.Options.BaseUrl;
        var url = ApiKeys.Combine(baseUrl, "/videos/" + PathSegments.Encode(requestId!));
        ProviderExchangeResult response;
        try
        {
            var headers = ProviderExchange.Merge(_provider.CreateHeaders(), call.Headers);
            response = await ProviderExchange
                .GetResponseUrlAsync(
                    _provider.SharedHttpClient,
                    url.AbsoluteUri,
                    baseUrl,
                    _ => { },
                    headers,
                    cancellationToken,
                    ProviderExchange.DownloadClient(_provider.Options),
                    ProviderExchange.UserAgentOnly(headers),
                    failedResponseHandler: XaiError.FailedResponseHandler,
                    maxPendingBodyBytes: MaxPendingBodyBytes
                )
                .ConfigureAwait(false);
        }
        catch (ApiException exception) when (hasUploadUrl)
        {
            throw new ApiException(
                "xAI video upload status request failed.",
                exception.StatusCode,
                null
            )
            {
                Url = url.AbsoluteUri,
                IsRetryable = exception.IsRetryable,
            };
        }

        var status = ReadStatus(response);
        if (status.Status == "expired")
        {
            return new VideoStatusResult(
                "error",
                error: "Video generation request expired.",
                response: Response(response, timestamp)
            );
        }

        if (status.Status == "failed")
        {
            var details = status.ErrorMessage ?? status.ErrorCode;
            return new VideoStatusResult(
                "error",
                error: !hasUploadUrl && details != null
                    ? "Video generation failed: " + details
                    : "Video generation failed.",
                response: Response(response, timestamp)
            );
        }

        if (
            status.Status == "done"
            || (status.Status == null && !string.IsNullOrEmpty(status.Video?.Url))
        )
        {
            var video = status.Video;

            // Terminal outcomes, so they are reported the same way as an upstream failed.
            if (video?.RespectModeration == false)
            {
                return new VideoStatusResult(
                    "error",
                    error: "Video generation was blocked due to a content policy violation.",
                    response: Response(response, timestamp)
                );
            }

            var videoUrl = video?.Url ?? video?.FileOutput?.PublicUrl;
            if (!hasUploadUrl && string.IsNullOrEmpty(videoUrl))
            {
                return new VideoStatusResult(
                    "error",
                    error: "Video generation completed but no video URL was returned.",
                    response: Response(response, timestamp)
                );
            }

            var videos = hasUploadUrl
                ? new List<VideoOutput> { new("uploaded", mediaType: "video/mp4") }
                : new List<VideoOutput> { new("url", videoUrl, mediaType: "video/mp4") };

            var metadata = new JsonObject { ["requestId"] = requestId };
            if (!hasUploadUrl && !string.IsNullOrEmpty(videoUrl))
            {
                metadata["videoUrl"] = videoUrl;
            }

            if (video?.Duration != null)
            {
                metadata["duration"] = video.Duration.DeepClone();
            }

            if (status.CostInUsdTicks != null)
            {
                metadata["costInUsdTicks"] = status.CostInUsdTicks.DeepClone();
            }

            if (status.Progress != null)
            {
                metadata["progress"] = status.Progress.DeepClone();
            }

            if (video?.FileOutput is { } fileOutput)
            {
                var output = new JsonObject
                {
                    ["fileId"] = fileOutput.FileId,
                    ["filename"] = fileOutput.Filename,
                };
                if (fileOutput.ExpiresAt != null)
                {
                    output["expiresAt"] = fileOutput.ExpiresAt.DeepClone();
                }

                if (fileOutput.PublicUrl != null)
                {
                    output["publicUrl"] = fileOutput.PublicUrl;
                }

                if (fileOutput.PublicUrlError != null)
                {
                    output["publicUrlError"] = fileOutput.PublicUrlError;
                }

                if (fileOutput.PublicUrlExpiresAt != null)
                {
                    output["publicUrlExpiresAt"] = fileOutput.PublicUrlExpiresAt.DeepClone();
                }

                metadata["fileOutput"] = output;
            }

            if (video?.StorageError != null)
            {
                metadata["storageError"] = video.StorageError;
            }

            return new VideoStatusResult(
                "completed",
                videos,
                warnings: [],
                providerMetadata: OperationJson.Parse(
                    new JsonObject { ["xai"] = metadata }.ToJsonString()
                ),
                response: Response(response, timestamp)
            );
        }

        // Pending status.
        return new VideoStatusResult("pending", response: Response(response, timestamp));
    }

    private static bool IsVideoFile(VideoModelFile file)
    {
        return file.MediaType != null && MediaTypes.GetTopLevelMediaType(file.MediaType) == "video";
    }

    // References without a media type (only possible for URLs) are treated as images, matching the legacy referenceImageUrls behavior.
    private static bool IsImageReference(VideoModelFile file)
    {
        return file.MediaType == null || MediaTypes.GetTopLevelMediaType(file.MediaType) == "image";
    }

    private static bool IsAudioReference(VideoModelFile file)
    {
        return file.MediaType != null && MediaTypes.GetTopLevelMediaType(file.MediaType) == "audio";
    }

    private static VideoModelFile? GetFirstFrameImage(VideoModelCall call)
    {
        return call.FrameImages?.FirstOrDefault(frame => frame.FrameType == "first_frame")?.Image;
    }

    private static VideoModelFile? GetLastFrameImage(VideoModelCall call)
    {
        return call.FrameImages?.FirstOrDefault(frame => frame.FrameType == "last_frame")?.Image;
    }

    private static VideoModelFile? ResolveStartImage(VideoModelCall call)
    {
        return GetFirstFrameImage(call) ?? call.Image;
    }

    private static string FileToXaiUrl(VideoModelFile file)
    {
        return file.Type == "url"
            ? file.Url!
            : "data:"
                + file.MediaType
                + ";base64,"
                + Convert.ToBase64String(file.Data ?? Array.Empty<byte>());
    }

    // Resolves the reference images for R2V generation. First-class inputReferences win over the legacy
    // referenceImageUrls provider option. Video references are not supported for reference-to-video and
    // are skipped with a warning. Audio references are handled separately.
    private static JsonArray? ResolveReferences(
        VideoModelCall call,
        XaiVideoModelOptions? xaiOptions,
        List<OperationWarning> warnings
    )
    {
        if (call.InputReferences is { Count: > 0 } references)
        {
            var images = new JsonArray();
            foreach (var reference in references)
            {
                if (IsAudioReference(reference))
                {
                    continue;
                }

                if (!IsImageReference(reference))
                {
                    warnings.Add(
                        OperationWarning.Unsupported(
                            "inputReferences",
                            "xAI reference-to-video does not accept video references. The video reference was ignored. Use providerOptions.xai.mode \"extend-video\" to continue from a video."
                        )
                    );
                    continue;
                }

                images.Add(new JsonObject { ["url"] = FileToXaiUrl(reference) });
            }

            return images.Count > 0 ? images : null;
        }

        if (xaiOptions?.ReferenceImageUrls is { Count: > 0 } urls)
        {
            var images = new JsonArray();
            foreach (var url in urls)
            {
                images.Add(new JsonObject { ["url"] = url });
            }

            return images;
        }

        return null;
    }

    private static List<JsonObject> ResolveReferenceAudios(VideoModelCall call)
    {
        return call.InputReferences?.Where(IsAudioReference)
            .Select(reference => new JsonObject { ["url"] = FileToXaiUrl(reference) })
            .ToList()
            ?? new List<JsonObject>();
    }

    // True when at least one reference would survive as an image.
    private static bool HasImageInputReference(VideoModelCall call)
    {
        return call.InputReferences?.Any(IsImageReference) ?? false;
    }

    private static bool HasAudioInputReference(VideoModelCall call)
    {
        return call.InputReferences?.Any(IsAudioReference) ?? false;
    }

    private static string? ResolveVideoMode(VideoModelCall call, XaiVideoModelOptions? xaiOptions)
    {
        if (xaiOptions?.Mode != null)
        {
            return xaiOptions.Mode;
        }

        if (xaiOptions?.VideoUrl != null)
        {
            return XaiVideoModelOptions.EditVideo;
        }

        var hasLegacyReferenceUrls = xaiOptions?.ReferenceImageUrls is { Count: > 0 };

        // xAI supports image references, audio references, or both. Video-only references must not
        // flip a standard generation request into R2V.
        if (HasImageInputReference(call) || HasAudioInputReference(call) || hasLegacyReferenceUrls)
        {
            return XaiVideoModelOptions.ReferenceToVideo;
        }

        return null;
    }

    private static string? ReadRequestId(string text)
    {
        try
        {
            using var document = JsonDocument.Parse(text);
            return String(document.RootElement, "request_id");
        }
        catch (JsonException)
        {
            throw new ApiException("Invalid JSON response", 200, text);
        }
    }

    // xAI answers 202 while a generation is still running, sometimes with an empty body or one that does not parse.
    private static StatusResponse ReadStatus(ProviderExchangeResult response)
    {
        var pending = response.StatusCode == 202;
        if (pending && string.IsNullOrWhiteSpace(response.Body))
        {
            return new StatusResponse { Status = "pending" };
        }

        try
        {
            using var document = JsonDocument.Parse(response.Body);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException("The status response is not an object.");
            }

            return StatusResponse.From(document.RootElement);
        }
        catch (JsonException) when (pending)
        {
            return new StatusResponse { Status = "pending" };
        }
        catch (JsonException)
        {
            throw new ApiException("Invalid JSON response", response.StatusCode, response.Body);
        }
    }

    private static string? String(JsonElement element, string name)
    {
        return
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private (
        JsonObject Body,
        List<OperationWarning> Warnings,
        XaiVideoModelOptions? XaiOptions,
        bool IsEdit,
        bool IsExtension
    ) BuildRequestBody(VideoModelCall call)
    {
        var warnings = new List<OperationWarning>();
        var xaiOptions = XaiVideoModelOptions.Parse(call.ProviderOptions);
        var effectiveMode = ResolveVideoMode(call, xaiOptions);

        var isEdit = effectiveMode == XaiVideoModelOptions.EditVideo;
        var isExtension = effectiveMode == XaiVideoModelOptions.ExtendVideo;
        var hasReferenceImages = effectiveMode == XaiVideoModelOptions.ReferenceToVideo;

        if (call.Fps != null)
        {
            warnings.Add(
                OperationWarning.Unsupported("fps", "xAI video models do not support custom FPS.")
            );
        }

        if (call.Seed != null)
        {
            warnings.Add(
                OperationWarning.Unsupported("seed", "xAI video models do not support seed.")
            );
        }

        if (call.N > 1)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "n",
                    "xAI video models do not support generating multiple videos per call. Only 1 video will be generated."
                )
            );
        }

        // Edit mode: duration, aspectRatio, resolution not supported.
        if (isEdit && call.Duration != null)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "duration",
                    "xAI video editing does not support custom duration."
                )
            );
        }

        if (isEdit && call.AspectRatio != null)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "aspectRatio",
                    "xAI video editing does not support custom aspect ratio."
                )
            );
        }

        if (isEdit && (xaiOptions?.Resolution != null || call.Resolution != null))
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "resolution",
                    "xAI video editing does not support custom resolution."
                )
            );
        }

        // Extension mode: aspectRatio and resolution not supported.
        if (isExtension && call.AspectRatio != null)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "aspectRatio",
                    "xAI video extension does not support custom aspect ratio."
                )
            );
        }

        if (isExtension && (xaiOptions?.Resolution != null || call.Resolution != null))
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "resolution",
                    "xAI video extension does not support custom resolution."
                )
            );
        }

        var body = new JsonObject { ["model"] = ModelId, ["prompt"] = call.Prompt };

        if (xaiOptions?.OutputUploadUrl != null)
        {
            body["output"] = new JsonObject { ["upload_url"] = xaiOptions.OutputUploadUrl };
        }

        var allowDuration = !isEdit;
        var allowAspectRatio = !isEdit && !isExtension;
        var allowResolution = !isEdit && !isExtension;

        if (allowDuration && call.Duration is { } duration)
        {
            body["duration"] = duration;
        }

        if (allowAspectRatio && call.AspectRatio != null)
        {
            body["aspect_ratio"] = call.AspectRatio;
        }

        if (allowResolution && xaiOptions?.Resolution != null)
        {
            body["resolution"] = xaiOptions.Resolution;
        }
        else if (allowResolution && call.Resolution != null)
        {
            if (ResolutionMap.TryGetValue(call.Resolution, out var mapped))
            {
                body["resolution"] = mapped;
            }
            else
            {
                warnings.Add(
                    OperationWarning.Unsupported(
                        "resolution",
                        "Unrecognized resolution \""
                            + call.Resolution
                            + "\". Use providerOptions.xai.resolution with \"480p\", \"720p\", or \"1080p\" instead."
                    )
                );
            }
        }

        if (call.GenerateAudio is { } generateAudio && !isEdit && !isExtension)
        {
            body["generate_audio"] = generateAudio;
        }
        else if (call.GenerateAudio != null)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "generateAudio",
                    "xAI "
                        + (isEdit ? "video editing" : "video extension")
                        + " does not support generateAudio."
                )
            );
        }

        if (xaiOptions?.StorageOptions is { } storage)
        {
            var storageOptions = new JsonObject { ["filename"] = storage.Filename };
            if (storage.ExpiresAfter != null)
            {
                storageOptions["expires_after"] = storage.ExpiresAfter;
            }

            if (storage.PublicUrlIsObject)
            {
                var publicUrl = new JsonObject();
                if (storage.PublicUrlExpiresAfter != null)
                {
                    publicUrl["expires_after"] = storage.PublicUrlExpiresAfter;
                }

                storageOptions["public_url"] = publicUrl;
            }
            else if (storage.PublicUrl != null)
            {
                storageOptions["public_url"] = storage.PublicUrl;
            }

            body["storage_options"] = storageOptions;
        }

        if (xaiOptions?.Keyframes is { Count: > 0 } keyframes)
        {
            if (ModelId != Grok15 || isEdit || isExtension)
            {
                warnings.Add(
                    OperationWarning.Unsupported(
                        "keyframes",
                        ModelId != Grok15
                            ? "xAI only supports keyframes with \"grok-imagine-video-1.5\"."
                            : "xAI "
                                + (isEdit ? "video editing" : "video extension")
                                + " does not support keyframes."
                    )
                );
            }
            else
            {
                var entries = new JsonArray();
                foreach (var keyframe in keyframes)
                {
                    entries.Add(
                        new JsonObject
                        {
                            ["image"] = new JsonObject { ["url"] = keyframe.ImageUrl },
                            ["timestamp_s"] = keyframe.TimestampSeconds,
                        }
                    );
                }

                body["keyframes"] = entries;
            }
        }

        // Video editing and extension: pass the source video URL (nested object).
        if (isEdit || isExtension)
        {
            body["video"] = new JsonObject { ["url"] = xaiOptions!.VideoUrl };
        }

        // Convert the start image (first_frame or image-to-video input) to the nested xAI request image object.
        if (ResolveStartImage(call) is { } startImage)
        {
            if (IsVideoFile(startImage))
            {
                var fromFrameImages = GetFirstFrameImage(call) != null;
                warnings.Add(
                    OperationWarning.Unsupported(
                        fromFrameImages ? "frameImages" : "image",
                        "xAI does not accept a video as a start/frame image. The video was ignored. Use providerOptions.xai.mode \"extend-video\" to continue from a video instead."
                    )
                );
            }
            else
            {
                body["image"] = new JsonObject { ["url"] = FileToXaiUrl(startImage) };
            }
        }

        // Only grok-imagine-video-1.5 supports a pinned last frame.
        if (GetLastFrameImage(call) is { } lastFrameImage)
        {
            if (ModelId != Grok15 || isEdit || isExtension || IsVideoFile(lastFrameImage))
            {
                warnings.Add(
                    OperationWarning.Unsupported(
                        "frameImages",
                        ModelId != Grok15
                            ? "xAI only supports last_frame with \"grok-imagine-video-1.5\". The last frame was ignored."
                            : "xAI only accepts an image last_frame for video generation. The last frame was ignored."
                    )
                );
            }
            else
            {
                body["last_frame"] = new JsonObject { ["url"] = FileToXaiUrl(lastFrameImage) };
            }
        }

        // Reference images for R2V (reference-to-video) generation.
        if (hasReferenceImages)
        {
            var referenceImages = ResolveReferences(call, xaiOptions, warnings);
            var referenceAudios = ResolveReferenceAudios(call);

            if (referenceImages != null)
            {
                body["reference_images"] = referenceImages;
            }
            else if (referenceAudios.Count == 0)
            {
                // Explicit R2V with no usable image references would silently send a plain generations
                // request; tell the user it is no longer R2V.
                warnings.Add(
                    OperationWarning.Unsupported(
                        "referenceImages",
                        "xAI reference-to-video requires at least one image reference. The video will be generated without reference images."
                    )
                );
            }

            var referenceAudioInputs = new List<JsonObject>(referenceAudios);
            if (xaiOptions?.ReferenceVoiceIds is { } voiceIds)
            {
                referenceAudioInputs.AddRange(
                    voiceIds.Select(voiceId => new JsonObject { ["voice_id"] = voiceId })
                );
            }

            if (referenceAudioInputs.Count > 0)
            {
                if (referenceAudioInputs.Count > 3)
                {
                    warnings.Add(
                        OperationWarning.Unsupported(
                            "inputReferences",
                            "xAI reference-to-video supports at most 3 audio references. Only the first 3 were used."
                        )
                    );
                }

                body["reference_audios"] = new JsonArray(
                    referenceAudioInputs.Take(3).Select(entry => (JsonNode)entry).ToArray()
                );
            }

            // Reference-to-video is capped at 720p; downgrade a 1080p request.
            if (body["resolution"]?.GetValue<string>() == "1080p")
            {
                warnings.Add(
                    OperationWarning.Unsupported(
                        "resolution",
                        "xAI reference-to-video is limited to 720p. The request was downgraded from 1080p to 720p."
                    )
                );
                body["resolution"] = "720p";
            }
        }

        // 1080p requires grok-imagine-video-1.5; the original grok-imagine-video rejects it. Warn, but send the request as the user asked.
        if (body["resolution"]?.GetValue<string>() == "1080p" && ModelId == "grok-imagine-video")
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "resolution",
                    "xAI model \"grok-imagine-video\" does not support 1080p. Use \"grok-imagine-video-1.5\" for 1080p, or a lower resolution. The request was sent with 1080p."
                )
            );
        }

        // Warn when references were provided but cannot be used in the resolved mode (for example alongside
        // frameImages, in edit/extend modes, or when the references carried no usable image to drive reference-to-video).
        if (call.InputReferences is { Count: > 0 } && !hasReferenceImages)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "inputReferences",
                    HasImageInputReference(call) || HasAudioInputReference(call)
                        ? "xAI only supports inputReferences for reference-to-video generation. The references were ignored."
                        : "xAI reference-to-video requires at least one image or audio reference. The references were ignored."
                )
            );
        }

        // Preset reference voices only apply to reference-to-video generation.
        if (xaiOptions?.ReferenceVoiceIds is { Count: > 0 } && !hasReferenceImages)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "referenceVoiceIds",
                    "xAI only supports reference voices for reference-to-video generation. The reference voices were ignored."
                )
            );
        }

        if (!isExtension && xaiOptions?.User != null)
        {
            body["user"] = xaiOptions.User;
        }

        if (xaiOptions != null)
        {
            foreach (var pair in xaiOptions.AdditionalProperties)
            {
                body[pair.Key] = JsonNode.Parse(pair.Value.GetRawText());
            }
        }

        return (body, warnings, xaiOptions, isEdit, isExtension);
    }

    private ProviderResponse Response(ProviderExchangeResult response, DateTimeOffset timestamp)
    {
        return new ProviderResponse(
            response.Headers,
            timestamp: timestamp.UtcDateTime,
            modelId: ModelId
        );
    }

    // The xAI video status response. Every field is nullish, as in upstream.
    private sealed class StatusResponse
    {
        public string? Status { get; set; }

        public StatusVideo? Video { get; set; }

        public JsonNode? CostInUsdTicks { get; set; }

        public JsonNode? Progress { get; set; }

        public string? ErrorCode { get; set; }

        public string? ErrorMessage { get; set; }

        public static StatusResponse From(JsonElement root)
        {
            var response = new StatusResponse
            {
                Status = String(root, "status"),
                Progress = Number(root, "progress"),
            };
            if (
                root.TryGetProperty("video", out var video)
                && video.ValueKind == JsonValueKind.Object
            )
            {
                response.Video = StatusVideo.From(video);
            }

            if (root.TryGetProperty("usage", out var usage))
            {
                response.CostInUsdTicks = Number(usage, "cost_in_usd_ticks");
            }

            if (
                root.TryGetProperty("error", out var error)
                && error.ValueKind == JsonValueKind.Object
            )
            {
                response.ErrorCode = String(error, "code");
                response.ErrorMessage = String(error, "message");
            }

            return response;
        }
    }

    private sealed class StatusVideo
    {
        public string? Url { get; set; }

        public JsonNode? Duration { get; set; }

        public bool? RespectModeration { get; set; }

        public StatusFileOutput? FileOutput { get; set; }

        public string? StorageError { get; set; }

        public static StatusVideo From(JsonElement video)
        {
            var result = new StatusVideo
            {
                Url = String(video, "url"),
                Duration = Number(video, "duration"),
                StorageError = String(video, "storage_error"),
            };
            if (
                video.TryGetProperty("respect_moderation", out var moderation)
                && moderation.ValueKind is JsonValueKind.True or JsonValueKind.False
            )
            {
                result.RespectModeration = moderation.GetBoolean();
            }

            if (
                video.TryGetProperty("file_output", out var fileOutput)
                && fileOutput.ValueKind == JsonValueKind.Object
            )
            {
                result.FileOutput = new StatusFileOutput
                {
                    FileId = String(fileOutput, "file_id") ?? string.Empty,
                    Filename = String(fileOutput, "filename") ?? string.Empty,
                    ExpiresAt = Number(fileOutput, "expires_at"),
                    PublicUrl = String(fileOutput, "public_url"),
                    PublicUrlError = String(fileOutput, "public_url_error"),
                    PublicUrlExpiresAt = Number(fileOutput, "public_url_expires_at"),
                };
            }

            return result;
        }
    }

    private sealed class StatusFileOutput
    {
        public string FileId { get; set; } = string.Empty;

        public string Filename { get; set; } = string.Empty;

        public JsonNode? ExpiresAt { get; set; }

        public string? PublicUrl { get; set; }

        public string? PublicUrlError { get; set; }

        public JsonNode? PublicUrlExpiresAt { get; set; }
    }

    private static JsonNode? Number(JsonElement element, string name)
    {
        return
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
            ? JsonNode.Parse(value.GetRawText())
            : null;
    }
}
