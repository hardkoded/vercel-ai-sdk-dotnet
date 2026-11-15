// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tencent.TencentVideoInputs;

namespace Vercel.AI.Tencent;

/// <summary>A TokenHub Kling request. Upstream returns <c>{ endpoint, body }</c>.</summary>
internal sealed class TokenHubKlingRequest
{
    internal TokenHubKlingRequest(string endpoint, JsonObject body)
    {
        Endpoint = endpoint;
        Body = body;
    }

    /// <summary><c>text-to-video</c>, <c>image-to-video</c>, <c>omni-video</c> or <c>motion-control</c>.</summary>
    internal string Endpoint { get; }

    internal JsonObject Body { get; }
}

/// <summary>TokenHub Kling request building and status handling. Upstream <c>tencent-tokenhub-video.ts</c>.</summary>
internal static class TencentTokenHubVideo
{
    private static readonly Dictionary<int, string> KlingResolutionTiers = new()
    {
        [720] = "720p",
        [1080] = "1080p",
        [2160] = "4k",
    };

    internal static TokenHubKlingRequest BuildTokenHubKlingRequest(
        string modelId,
        TencentKlingVideoKind kind,
        VideoModelCall options,
        TencentVideoModelOptions? tencentOptions,
        List<OperationWarning> warnings)
    {
        var firstFrame = ResolveFirstFrameImage(options, warnings);
        var lastFrame = ResolveLastFrameImage(options, warnings);
        var references = SplitInputReferences(options);
        var resolution = ResolveResolution(options, tencentOptions, KlingResolutionTiers, warnings);
        if (references.Audios.Count > 0)
        {
            warnings.Add(OperationWarning.Unsupported(
                "inputReferences",
                "TokenHub Kling models do not accept audio references; use `providerOptions.tencent.voices` instead. The audio was ignored."));
        }

        if (kind == TencentKlingVideoKind.MotionControl)
        {
            // The character image comes from the first frame, else the first image reference;
            // the motion comes from the first video reference.
            var firstImageReference = references.Images.FirstOrDefault();
            var otherImageReferences = references.Images.Skip(1).ToList();
            var motionVideo = references.Videos.FirstOrDefault();
            var otherVideoReferences = references.Videos.Skip(1).ToList();
            var unusedImageReferences = firstFrame != null ? references.Images : otherImageReferences;
            return new TokenHubKlingRequest(
                "motion-control",
                BuildMotionControlBody(
                    modelId,
                    options,
                    tencentOptions,
                    firstFrame ?? firstImageReference,
                    motionVideo,
                    lastFrame,
                    unusedImageReferences.Count + otherVideoReferences.Count,
                    resolution,
                    warnings));
        }

        var config = TencentModelCatalog.GetTencentVideoRequestConfig(modelId);
        var supportsAudioAndMultiShot = config.AudioAndMultiShot;
        var audio = ResolveAudio(options, tencentOptions, supportsAudioAndMultiShot, warnings);
        var multiShot = ResolveMultiShot(tencentOptions, supportsAudioAndMultiShot, warnings);
        var hasReferences = references.Images.Count > 0 || references.Videos.Count > 0 || (tencentOptions?.Voices?.Count ?? 0) > 0;
        // Elements work on image-to-video and omni but not text-to-video, so an
        // omni-capable model sends elements without a first frame to omni.
        var hasElementsWithoutFirstFrame = firstFrame == null && (tencentOptions?.Elements?.Count ?? 0) > 0;
        if (kind == TencentKlingVideoKind.Omni || (config.Omni && (hasReferences || hasElementsWithoutFirstFrame)))
        {
            var videoType = tencentOptions?.ReferenceVideoType == "base" ? "base_video" : "feature_video";
            var contents = new JsonArray();
            AddPrompt(contents, options.Prompt);
            if (firstFrame != null)
            {
                contents.Add(Content("first_frame", "url", ToUrlOrDataUri(firstFrame)));
            }

            if (lastFrame != null)
            {
                contents.Add(Content("last_frame", "url", ToUrlOrDataUri(lastFrame)));
            }

            foreach (var image in references.Images)
            {
                contents.Add(Content("refer_image", "url", ToUrlOrDataUri(image)));
            }

            foreach (var video in references.Videos)
            {
                contents.Add(Content(videoType, "url", ToUrlOrDataUri(video)));
            }

            AddElements(contents, tencentOptions);
            foreach (var voice in tencentOptions?.Voices ?? [])
            {
                contents.Add(new JsonObject { ["type"] = "voice", ["voice_id"] = voice.VoiceId, ["id"] = voice.Id });
            }

            return new TokenHubKlingRequest(
                "omni-video",
                WithSettingsAndOptions(
                    new JsonObject { ["model"] = modelId, ["contents"] = contents },
                    new JsonObject
                    {
                        ["resolution"] = Str(resolution),
                        ["aspect_ratio"] = Str(options.AspectRatio),
                        ["duration"] = Num(options.Duration),
                        ["multi_shot"] = Bool(multiShot),
                        ["audio"] = Str(audio),
                    },
                    tencentOptions));
        }

        if (hasReferences)
        {
            warnings.Add(OperationWarning.Unsupported(
                "inputReferences",
                modelId + " does not accept reference inputs; use an omni model such as kling-video-v3-omni or kling-video-v4. The references were ignored."));
        }

        if (firstFrame != null)
        {
            if (options.AspectRatio != null)
            {
                warnings.Add(OperationWarning.Unsupported("aspectRatio", "TokenHub Kling image-to-video follows the aspect ratio of the input image."));
            }

            var contents = new JsonArray();
            AddPrompt(contents, options.Prompt);
            contents.Add(Content("first_frame", "url", ToUrlOrDataUri(firstFrame)));
            if (lastFrame != null)
            {
                contents.Add(Content("last_frame", "url", ToUrlOrDataUri(lastFrame)));
            }

            AddElements(contents, tencentOptions);
            return new TokenHubKlingRequest(
                "image-to-video",
                WithSettingsAndOptions(
                    new JsonObject { ["model"] = modelId, ["contents"] = contents },
                    new JsonObject
                    {
                        ["resolution"] = Str(resolution),
                        ["duration"] = Num(options.Duration),
                        ["multi_shot"] = Bool(multiShot),
                        ["audio"] = Str(audio),
                    },
                    tencentOptions));
        }

        if (lastFrame != null)
        {
            warnings.Add(OperationWarning.Unsupported("frameImages", "TokenHub Kling does not support a last frame without a first frame; the last frame was ignored."));
        }

        if ((tencentOptions?.Elements?.Count ?? 0) > 0)
        {
            warnings.Add(OperationWarning.Unsupported("providerOptions.tencent.elements", "TokenHub Kling text-to-video does not accept elements; the elements were ignored."));
        }

        var body = new JsonObject { ["model"] = modelId };
        if (options.Prompt != null)
        {
            body["prompt"] = options.Prompt;
        }

        return new TokenHubKlingRequest(
            "text-to-video",
            WithSettingsAndOptions(
                body,
                new JsonObject
                {
                    ["resolution"] = Str(resolution),
                    ["aspect_ratio"] = Str(options.AspectRatio),
                    ["duration"] = Num(options.Duration),
                    ["multi_shot"] = Bool(multiShot),
                    ["audio"] = Str(audio),
                },
                tencentOptions));
    }

    /// <summary>Interprets a status response. Upstream <c>interpretTokenHubKlingStatus</c>.</summary>
    internal static VideoStatusResult InterpretTokenHubKlingStatus(JsonElement statusResponse, string taskId, ProviderResponse response)
    {
        var task = FindTask(statusResponse, taskId);
        var status = task is { } t ? String(t, "status") : null;
        if (status == "succeeded")
        {
            var outputs = new List<JsonElement>();
            if (task!.Value.TryGetProperty("outputs", out var rawOutputs) && rawOutputs.ValueKind == JsonValueKind.Array)
            {
                foreach (var output in rawOutputs.EnumerateArray())
                {
                    if (String(output, "url") != null && (String(output, "type") ?? "video") == "video")
                    {
                        outputs.Add(output);
                    }
                }
            }

            if (outputs.Count == 0)
            {
                return new VideoStatusResult("error", error: "TokenHub Kling task " + taskId + " succeeded without a video URL.", response: response);
            }

            var videos = new JsonArray();
            foreach (var output in outputs)
            {
                var entry = new JsonObject();
                if (String(output, "id") is { } id)
                {
                    entry["id"] = id;
                }

                entry["url"] = String(output, "url");
                // TokenHub reports the duration as a string; VOD reports a number.
                if (TryDuration(output, out var duration))
                {
                    entry["duration"] = duration;
                }

                videos.Add(entry);
            }

            var metadata = new JsonObject { ["taskId"] = taskId, ["videos"] = videos };
            if (String(statusResponse, "request_id") is { } requestId)
            {
                metadata["requestId"] = requestId;
            }

            if (statusResponse.TryGetProperty("tokenhub_usage", out var usage) && usage.ValueKind == JsonValueKind.Object
                && usage.TryGetProperty("total_tokens", out var totalTokens) && totalTokens.ValueKind == JsonValueKind.Number)
            {
                metadata["usage"] = new JsonObject { ["totalTokens"] = JsonNode.Parse(totalTokens.GetRawText()) };
            }

            return new VideoStatusResult(
                "completed",
                outputs.Select(output => new VideoOutput("url", url: String(output, "url"), mediaType: "video/mp4")).ToList(),
                warnings: [],
                providerMetadata: OperationJson.Parse(new JsonObject { ["tencent"] = metadata }.ToJsonString()),
                response: response);
        }

        if (status == "failed")
        {
            var message = task is { } failed ? String(failed, "message") : null;
            return new VideoStatusResult("error", error: "Video generation failed: " + (string.IsNullOrEmpty(message) ? "unknown error" : message) + " (task " + taskId + ")", response: response);
        }

        return new VideoStatusResult("pending", response: response);
    }

    private static JsonElement? FindTask(JsonElement statusResponse, string taskId)
    {
        if (!statusResponse.TryGetProperty("data", out var data))
        {
            return null;
        }

        if (data.ValueKind == JsonValueKind.Array)
        {
            JsonElement? first = null;
            foreach (var entry in data.EnumerateArray())
            {
                first ??= entry;
                if (String(entry, "id") == taskId)
                {
                    return entry;
                }
            }

            return first;
        }

        return data.ValueKind == JsonValueKind.Object ? data : null;
    }

    private static bool TryDuration(JsonElement output, out double duration)
    {
        duration = 0;
        if (!output.TryGetProperty("duration", out var value))
        {
            return false;
        }

        if (value.ValueKind == JsonValueKind.Number)
        {
            duration = value.GetDouble();
            return true;
        }

        return value.ValueKind == JsonValueKind.String
            && !string.IsNullOrEmpty(value.GetString())
            && double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out duration)
            && !double.IsInfinity(duration);
    }

    private static string? String(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static JsonNode? Str(string? value) => value == null ? null : JsonValue.Create(value);

    private static JsonNode? Num(double? value) => value == null ? null : JsonValue.Create(value.Value);

    private static JsonNode? Bool(bool? value) => value == null ? null : JsonValue.Create(value.Value);

    private static JsonObject Content(string type, string key, string value) => new() { ["type"] = type, [key] = value };

    private static void AddPrompt(JsonArray contents, string? prompt)
    {
        if (prompt != null)
        {
            contents.Add(new JsonObject { ["type"] = "prompt", ["text"] = prompt });
        }
    }

    private static void AddElements(JsonArray contents, TencentVideoModelOptions? tencentOptions)
    {
        foreach (var element in tencentOptions?.Elements ?? [])
        {
            var entry = new JsonObject { ["type"] = "element", ["element_id"] = element.ElementId };
            if (element.Id != null)
            {
                entry["id"] = element.Id;
            }

            contents.Add(entry);
        }
    }

    // Drops null entries and returns null when nothing is left.
    private static JsonObject? Compact(JsonObject value)
    {
        var result = new JsonObject();
        foreach (var pair in value.ToList())
        {
            if (pair.Value != null)
            {
                value.Remove(pair.Key);
                result[pair.Key] = pair.Value;
            }
        }

        return result.Count > 0 ? result : null;
    }

    private static JsonObject WithSettingsAndOptions(JsonObject body, JsonObject settings, TencentVideoModelOptions? tencentOptions)
    {
        var compactSettings = Compact(settings);
        var watermark = tencentOptions?.Watermark != null ? new JsonObject { ["enabled"] = tencentOptions.Watermark.Value } : null;
        var requestOptions = Compact(new JsonObject { ["external_task_id"] = Str(tencentOptions?.ExternalTaskId), ["watermark_info"] = watermark });
        if (compactSettings != null)
        {
            body["settings"] = compactSettings;
        }

        if (requestOptions != null)
        {
            body["options"] = requestOptions;
        }

        return body;
    }

    private static string? ResolveAudio(VideoModelCall options, TencentVideoModelOptions? tencentOptions, bool supported, List<OperationWarning> warnings)
    {
        var audio = tencentOptions?.Audio ?? (options.GenerateAudio != null ? (options.GenerateAudio.Value ? "native" : "off") : null);
        if (audio != null && !supported)
        {
            warnings.Add(OperationWarning.Unsupported("generateAudio", "This TokenHub Kling model does not support the audio setting."));
            return null;
        }

        return audio;
    }

    private static bool? ResolveMultiShot(TencentVideoModelOptions? tencentOptions, bool supported, List<OperationWarning> warnings)
    {
        if (tencentOptions?.MultiShot != null && !supported)
        {
            warnings.Add(OperationWarning.Unsupported("providerOptions.tencent.multiShot", "This TokenHub Kling model does not support multi-shot output."));
            return null;
        }

        return tencentOptions?.MultiShot;
    }

    private static JsonObject BuildMotionControlBody(
        string modelId,
        VideoModelCall options,
        TencentVideoModelOptions? tencentOptions,
        VideoModelFile? characterImage,
        VideoModelFile? motionVideo,
        VideoModelFile? lastFrame,
        int extraReferenceCount,
        string? resolution,
        List<OperationWarning> warnings)
    {
        if (options.Duration != null)
        {
            warnings.Add(OperationWarning.Unsupported("duration", "Motion control output follows the length of the motion reference video."));
        }

        if (options.AspectRatio != null)
        {
            warnings.Add(OperationWarning.Unsupported("aspectRatio", "Motion control output follows the aspect ratio of the input media."));
        }

        if (lastFrame != null)
        {
            warnings.Add(OperationWarning.Unsupported("frameImages", "Motion control does not support a last frame; it was ignored."));
        }

        if (extraReferenceCount > 0)
        {
            warnings.Add(OperationWarning.Unsupported(
                "inputReferences",
                "Motion control uses one character image and one motion reference video; additional references were ignored."));
        }

        var audio = tencentOptions?.Audio ?? (options.GenerateAudio == false ? "off" : null);
        var contents = new JsonArray();
        AddPrompt(contents, options.Prompt);
        if (characterImage != null)
        {
            contents.Add(Content("image", "url", ToUrlOrDataUri(characterImage)));
        }

        if (motionVideo != null)
        {
            contents.Add(Content("video", "url", ToUrlOrDataUri(motionVideo)));
        }

        AddElements(contents, tencentOptions);
        return WithSettingsAndOptions(
            new JsonObject { ["model"] = modelId, ["contents"] = contents },
            new JsonObject
            {
                ["character_orientation"] = Str(tencentOptions?.CharacterOrientation),
                ["audio"] = Str(audio),
                ["resolution"] = Str(resolution),
            },
            tencentOptions);
    }
}
