// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tencent.TencentVideoInputs;

namespace Vercel.AI.Tencent;

/// <summary>VOD AIGC video request building and status handling. Upstream <c>tencent-vod-video.ts</c>.</summary>
internal static class TencentVodVideo
{
    internal const string TencentVodService = "vod";
    internal const string TencentVodVersion = "2018-07-17";

    private static readonly Dictionary<int, string> VodResolutionTiers = new()
    {
        [480] = "480P",
        [720] = "720P",
        [1080] = "1080P",
        [1440] = "2K",
        [2160] = "4K",
    };

    private static readonly Dictionary<string, string> VodFileTypeMediaTypes = new(StringComparer.Ordinal)
    {
        ["mov"] = "video/quicktime",
        ["mp4"] = "video/mp4",
        ["webm"] = "video/webm",
    };

    /// <summary>Builds the <c>CreateAigcVideoTask</c> request body.</summary>
    internal static JsonObject BuildVodCreateTaskBody(
        string modelName,
        string modelVersion,
        long subAppId,
        VideoModelCall options,
        TencentVideoModelOptions? tencentOptions,
        List<OperationWarning> warnings)
    {
        var fileInfos = new List<JsonObject>();
        var firstFrame = ResolveFirstFrameImage(options, warnings);
        if (firstFrame != null)
        {
            fileInfos.Add(FileInfo(firstFrame, "Image", "FirstFrame"));
        }

        var lastFrame = ResolveLastFrameImage(options, warnings);
        if (lastFrame != null)
        {
            fileInfos.Add(FileInfo(lastFrame, "Image", "LastFrame"));
        }

        foreach (var reference in options.InputReferences ?? [])
        {
            var category = IsVideoFile(reference) ? "Video" : IsAudioFile(reference) ? "Audio" : "Image";
            var info = FileInfo(reference, category, "Reference");
            if (category == "Video" && tencentOptions?.ReferenceVideoType != null)
            {
                info["ReferenceType"] = tencentOptions.ReferenceVideoType;
            }

            fileInfos.Add(info);
        }

        // `asset://` URLs cannot come through `inputReferences`, which only passes http(s) strings through as URLs.
        foreach (var asset in tencentOptions?.Assets ?? [])
        {
            var usage = asset.Usage ?? "Reference";
            // VOD takes one first frame and one last frame; keep the first one given.
            if (usage != "Reference" && fileInfos.Any(file => (string?)file["Usage"] == usage))
            {
                warnings.Add(OperationWarning.Unsupported(
                    "providerOptions.tencent.assets",
                    "VOD accepts one " + (usage == "FirstFrame" ? "first" : "last") + " frame; the asset " + asset.Url + " was ignored."));
                continue;
            }

            fileInfos.Add(new JsonObject { ["Type"] = "Url", ["Url"] = asset.Url, ["Category"] = asset.Category ?? "Image", ["Usage"] = usage });
        }

        var outputConfig = new JsonObject { ["StorageMode"] = "Temporary" };
        Set(outputConfig, "Resolution", ResolveResolution(options, tencentOptions, VodResolutionTiers, warnings));
        Set(outputConfig, "AspectRatio", options.AspectRatio);
        if (options.Duration != null)
        {
            outputConfig["Duration"] = options.Duration.Value;
        }

        Set(outputConfig, "AudioGeneration", EnabledDisabled(options.GenerateAudio));
        Set(outputConfig, "PersonGeneration", tencentOptions?.PersonGeneration);
        Set(outputConfig, "InputComplianceCheck", EnabledDisabled(tencentOptions?.InputComplianceCheck));
        Set(outputConfig, "OutputComplianceCheck", EnabledDisabled(tencentOptions?.OutputComplianceCheck));
        Set(outputConfig, "EnhanceSwitch", EnabledDisabled(tencentOptions?.EnhanceSwitch));
        Set(outputConfig, "OffPeak", EnabledDisabled(tencentOptions?.OffPeak));
        Set(outputConfig, "LogoAdd", EnabledDisabled(tencentOptions?.Watermark));

        var body = new JsonObject
        {
            ["SubAppId"] = subAppId,
            ["ModelName"] = modelName,
            ["ModelVersion"] = modelVersion,
        };
        Set(body, "Prompt", options.Prompt);
        Set(body, "NegativePrompt", tencentOptions?.NegativePrompt);
        Set(body, "EnhancePrompt", EnabledDisabled(tencentOptions?.EnhancePrompt));
        if (fileInfos.Count > 0)
        {
            body["FileInfos"] = new JsonArray(fileInfos.Cast<JsonNode?>().ToArray());
        }

        body["OutputConfig"] = outputConfig;
        Set(body, "InputRegion", tencentOptions?.InputRegion);
        Set(body, "SceneType", tencentOptions?.SceneType);
        if (options.Seed != null)
        {
            body["Seed"] = options.Seed.Value;
        }

        Set(body, "SessionId", tencentOptions?.SessionId);
        Set(body, "SessionContext", tencentOptions?.SessionContext);
        if (tencentOptions?.TasksPriority != null)
        {
            body["TasksPriority"] = tencentOptions.TasksPriority.Value;
        }

        Set(body, "ExtInfo", tencentOptions?.ExtInfo);
        return body;
    }

    /// <summary>Interprets a <c>DescribeTaskDetail</c> response for an AIGC video task.</summary>
    internal static VideoStatusResult InterpretVodTaskDetail(JsonElement detail, string taskId, ProviderResponse response)
    {
        var task = Object(detail, "AigcVideoTask");
        var detailStatus = String(detail, "Status");
        if (detailStatus == "ABORTED")
        {
            return new VideoStatusResult("error", error: "Video generation aborted (task " + taskId + ")", response: response);
        }

        if (detailStatus != "FINISH")
        {
            return new VideoStatusResult("pending", response: response);
        }

        if (task is { } t && t.TryGetProperty("ErrCode", out var errCode) && errCode.ValueKind == JsonValueKind.Number && errCode.GetDouble() != 0)
        {
            var ext = String(t, "ErrCodeExt");
            var code = !string.IsNullOrEmpty(ext) ? ext : errCode.GetRawText();
            var message = String(t, "Message");
            return new VideoStatusResult(
                "error",
                error: "Video generation failed (" + code + "): " + (string.IsNullOrEmpty(message) ? "unknown error" : message) + " (task " + taskId + ")",
                response: response);
        }

        // VOD reports unset strings as "". `return_last_frame` (via `extInfo`) adds the last frame as a PNG entry
        // with UsageType `last_frame_url`; it is not a video.
        var output = task is { } owner ? Object(owner, "Output") : null;
        var fileInfos = new List<JsonElement>();
        if (output is { } outputElement && outputElement.TryGetProperty("FileInfos", out var rawInfos) && rawInfos.ValueKind == JsonValueKind.Array)
        {
            fileInfos.AddRange(rawInfos.EnumerateArray().Where(file => !string.IsNullOrEmpty(String(file, "FileUrl"))));
        }

        var files = fileInfos.Where(file => String(file, "UsageType") != "last_frame_url").ToList();
        var lastFrameUrl = fileInfos.Where(file => String(file, "UsageType") == "last_frame_url").Select(file => String(file, "FileUrl")).FirstOrDefault();
        if (files.Count == 0)
        {
            return new VideoStatusResult("error", error: "VOD task " + taskId + " finished without a video URL.", response: response);
        }

        var usage = output is { } usageOwner ? Object(usageOwner, "Usage") : null;
        // VOD reports unset strings as "". The billing SKU (e.g. `SV2.0_720P`) and the resolved resolution identify the price tier.
        var specificationUsages = new JsonArray();
        if (usage is { } usageElement && usageElement.TryGetProperty("SpecificationUsages", out var rawSpecs) && rawSpecs.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in rawSpecs.EnumerateArray())
            {
                var compact = new JsonObject();
                Set(compact, "specification", NonEmpty(String(entry, "Specification")));
                Set(compact, "category", NonEmpty(String(entry, "Category")));
                if (compact.Count > 0)
                {
                    specificationUsages.Add(compact);
                }
            }
        }

        var outputSeconds = usage is { } seconds ? Number(seconds, "OutputSeconds") : null;
        var videos = new JsonArray();
        foreach (var file in files)
        {
            var entry = new JsonObject();
            Set(entry, "url", String(file, "FileUrl"));
            // Temporary storage reports an empty FileId.
            Set(entry, "fileId", NonEmpty(String(file, "FileId")));
            var metaData = Object(file, "MetaData");
            var duration = (metaData is { } meta ? Number(meta, "Duration") : null) ?? outputSeconds;
            if (duration != null)
            {
                entry["duration"] = duration.Value;
            }

            videos.Add(entry);
        }

        var metadata = new JsonObject { ["taskId"] = taskId };
        Set(metadata, "requestId", String(detail, "RequestId"));
        var input = task is { } inputOwner ? Object(inputOwner, "Input") : null;
        var outputConfig = input is { } inputElement ? Object(inputElement, "OutputConfig") : null;
        Set(metadata, "resolution", NonEmpty(outputConfig is { } config ? String(config, "Resolution") : null));
        Set(metadata, "lastFrameUrl", lastFrameUrl);
        metadata["videos"] = videos;
        if (usage is { } usageValue)
        {
            var usageJson = new JsonObject();
            CopyNumber(usageJson, "inputImageCount", usageValue, "InputImageCount");
            CopyNumber(usageJson, "inputSeconds", usageValue, "InputSeconds");
            CopyNumber(usageJson, "inputTokens", usageValue, "InputTokens");
            CopyNumber(usageJson, "outputSeconds", usageValue, "OutputSeconds");
            if (specificationUsages.Count > 0)
            {
                usageJson["specificationUsages"] = specificationUsages;
            }

            CopyNumber(usageJson, "thoughtTokens", usageValue, "ThoughtTokens");
            CopyNumber(usageJson, "totalSeconds", usageValue, "TotalSeconds");
            metadata["usage"] = usageJson;
        }

        return new VideoStatusResult(
            "completed",
            files.Select(file =>
            {
                var fileType = (String(file, "FileType") ?? string.Empty).ToLowerInvariant();
                return new VideoOutput("url", url: String(file, "FileUrl"), mediaType: VodFileTypeMediaTypes.TryGetValue(fileType, out var mediaType) ? mediaType : "video/mp4");
            }).ToList(),
            warnings: [],
            providerMetadata: OperationJson.Parse(new JsonObject { ["tencent"] = metadata }.ToJsonString()),
            response: response);
    }

    private static string? EnabledDisabled(bool? value) => value == null ? null : value.Value ? "Enabled" : "Disabled";

    private static string? NonEmpty(string? value) => string.IsNullOrEmpty(value) ? null : value;

    private static void Set(JsonObject target, string name, string? value)
    {
        if (value != null)
        {
            target[name] = value;
        }
    }

    private static void CopyNumber(JsonObject target, string name, JsonElement source, string sourceName)
    {
        if (source.TryGetProperty(sourceName, out var value) && value.ValueKind == JsonValueKind.Number)
        {
            target[name] = JsonNode.Parse(value.GetRawText());
        }
    }

    private static JsonObject FileInfo(VideoModelFile file, string category, string usage)
    {
        var info = file.Type == "url"
            ? new JsonObject { ["Type"] = "Url", ["Url"] = file.Url }
            : new JsonObject { ["Type"] = "Base64", ["Base64"] = GetFileBase64(file) };
        info["Category"] = category;
        info["Usage"] = usage;
        return info;
    }

    private static JsonElement? Object(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object ? value : null;
    }

    private static string? String(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static double? Number(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number ? value.GetDouble() : null;
    }
}
