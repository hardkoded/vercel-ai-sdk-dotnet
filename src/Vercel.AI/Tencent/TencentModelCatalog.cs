// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tencent;

/// <summary>The kind of a TokenHub Kling video model. Upstream <c>TencentKlingVideoKind</c>.</summary>
public enum TencentKlingVideoKind
{
    /// <summary>Text-to-video and image-to-video.</summary>
    Standard,

    /// <summary>Omni (reference) generation.</summary>
    Omni,

    /// <summary>Motion control.</summary>
    MotionControl,
}

/// <summary>A parsed video model id. Upstream <c>ParsedTencentVideoModelId</c>.</summary>
public sealed class ParsedTencentVideoModelId
{
    private ParsedTencentVideoModelId(bool isVod, TencentKlingVideoKind kind, string? modelName, string? modelVersion)
    {
        IsVod = isVod;
        Kind = kind;
        ModelName = modelName;
        ModelVersion = modelVersion;
    }

    /// <summary>True for a VOD AIGC model (<c>vod/&lt;ModelName&gt;/&lt;ModelVersion&gt;</c>), false for a TokenHub Kling model.</summary>
    public bool IsVod { get; }

    /// <summary>The Kling kind. Only meaningful when <see cref="IsVod"/> is false.</summary>
    public TencentKlingVideoKind Kind { get; }

    /// <summary>The VOD model name. Null for TokenHub.</summary>
    public string? ModelName { get; }

    /// <summary>The VOD model version. Null for TokenHub.</summary>
    public string? ModelVersion { get; }

    internal static ParsedTencentVideoModelId TokenHub(TencentKlingVideoKind kind) => new(false, kind, null, null);

    internal static ParsedTencentVideoModelId Vod(string modelName, string modelVersion) => new(true, TencentKlingVideoKind.Standard, modelName, modelVersion);
}

/// <summary>Known Tencent model ids and the per-model policy. Upstream <c>tencent-model-catalog.ts</c>.</summary>
public static class TencentModelCatalog
{
    /// <summary>TokenHub language model ids.</summary>
    public static readonly IReadOnlyList<string> TencentLanguageModelIds =
    [
        "hy4", "hy4-preview", "hy3", "hy-mt2-pro", "hy-mt2-plus", "hy-mt2-lite", "deepseek-v4-pro-0813", "deepseek-v4.1-flash",
        "glm-5.3", "glm-5.3-flash", "glm-5.2", "mimo-v2.6-pro", "mimo-v2.6-flash", "step-5-preview",
    ];

    /// <summary>Video model ids.</summary>
    public static readonly IReadOnlyList<string> TencentVideoModelIds =
    [
        "kling-video-v4", "kling-video-v3", "kling-video-v3-turbo", "kling-video-v3-omni", "kling-video-motion-control-v3.0",
        "kling-video-motion-control-v2.6", "vod/VS/2.5", "vod/VS/2.0", "vod/VS/2.0-fast", "vod/VS/2.0-mini", "vod/Kling/3.0",
        "vod/Kling/3.0-Omni", "vod/Kling/3.0-turbo", "vod/Kling/2.6", "vod/Hailuo/2.3", "vod/Vidu/q3-pro",
    ];

    /// <summary>Options that only TokenHub Kling models read.</summary>
    public static readonly IReadOnlyList<string> TokenHubOnlyOptions = ["audio", "multiShot", "characterOrientation", "elements", "voices", "externalTaskId"];

    /// <summary>Options that only VOD models read.</summary>
    public static readonly IReadOnlyList<string> VodOnlyOptions =
    [
        "negativePrompt", "enhancePrompt", "inputRegion", "sceneType", "personGeneration", "inputComplianceCheck", "outputComplianceCheck",
        "enhanceSwitch", "offPeak", "sessionId", "sessionContext", "tasksPriority", "extInfo", "assets",
    ];

    private const string VodModelIdPrefix = "vod/";

    /// <summary>Parses a video model id. Returns null for an id that is neither a TokenHub Kling nor a VOD id.</summary>
    public static ParsedTencentVideoModelId? ParseTencentVideoModelId(string modelId)
    {
        if (modelId.StartsWith(VodModelIdPrefix, StringComparison.Ordinal))
        {
            var rest = modelId.Substring(VodModelIdPrefix.Length);
            var separator = rest.IndexOf('/');
            if (separator <= 0 || separator == rest.Length - 1)
            {
                return null;
            }

            return ParsedTencentVideoModelId.Vod(rest.Substring(0, separator), rest.Substring(separator + 1));
        }

        if (modelId.StartsWith("kling-video-motion-control-", StringComparison.Ordinal))
        {
            return ParsedTencentVideoModelId.TokenHub(TencentKlingVideoKind.MotionControl);
        }

        if (modelId.StartsWith("kling-video-", StringComparison.Ordinal))
        {
            return ParsedTencentVideoModelId.TokenHub(modelId.EndsWith("-omni", StringComparison.Ordinal) ? TencentKlingVideoKind.Omni : TencentKlingVideoKind.Standard);
        }

        return null;
    }

    /// <summary>Request-shaping policy for a video model. Upstream <c>getTencentVideoModelEntry(modelId).requestConfig.video</c>.</summary>
    public static TencentVideoRequestConfig GetTencentVideoRequestConfig(string modelId)
    {
        var parsed = ParseTencentVideoModelId(modelId);
        var audioAndMultiShot = modelId is not ("kling-video-v4" or "kling-video-v3-turbo");
        var omni = parsed is { IsVod: false } && (parsed.Kind == TencentKlingVideoKind.Omni || modelId == "kling-video-v4");
        return new TencentVideoRequestConfig(parsed, audioAndMultiShot, omni);
    }
}

/// <summary>What a video model id implies for request building.</summary>
public sealed class TencentVideoRequestConfig
{
    internal TencentVideoRequestConfig(ParsedTencentVideoModelId? parsed, bool audioAndMultiShot, bool omni)
    {
        Parsed = parsed;
        AudioAndMultiShot = audioAndMultiShot;
        Omni = omni;
    }

    /// <summary>The parsed id, or null when the id is not recognised.</summary>
    public ParsedTencentVideoModelId? Parsed { get; }

    /// <summary>Whether the model accepts the audio and multi-shot settings.</summary>
    public bool AudioAndMultiShot { get; }

    /// <summary>Whether the model can route reference inputs to the omni endpoint.</summary>
    public bool Omni { get; }
}
