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

    /// <summary>
    /// Entries of every known model id. Upstream <c>tencentModelCatalog</c>; renamed because C# does not allow a member named like its class.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, TencentModelEntry> Entries = BuildEntries();

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

    private static Dictionary<string, TencentModelEntry> BuildEntries()
    {
        var entries = new Dictionary<string, TencentModelEntry>();
        foreach (var id in TencentLanguageModelIds)
        {
            entries[id] = new TencentModelEntry(GetTencentLanguageModelEntry(id), entries.TryGetValue(id, out var previous) ? previous.Video : null);
        }

        foreach (var id in TencentVideoModelIds)
        {
            entries[id] = new TencentModelEntry(entries.TryGetValue(id, out var previous) ? previous.Language : null, GetTencentVideoModelEntry(id));
        }

        return entries;
    }

    /// <summary>Returns the catalog entry of a language model. Upstream <c>getTencentLanguageModelEntry</c>.</summary>
    public static TencentLanguageModelEntry GetTencentLanguageModelEntry(string modelId)
    {
        var options = new Dictionary<string, ModelCapability>();
        foreach (var key in TencentProviderOptions.Language.Keys)
        {
            options[key] = ModelCapability.Unknown;
        }

        return new TencentLanguageModelEntry(options);
    }

    /// <summary>Returns the catalog entry of a video model. Upstream <c>getTencentVideoModelEntry</c>.</summary>
    public static TencentVideoModelEntry GetTencentVideoModelEntry(string modelId)
    {
        var parsed = ParseTencentVideoModelId(modelId);
        var vod = parsed is { IsVod: true };
        var motion = parsed is { IsVod: false, Kind: TencentKlingVideoKind.MotionControl };
        var audioAndMultiShot = modelId is not ("kling-video-v4" or "kling-video-v3-turbo");
        var omni = parsed is { IsVod: false } && (parsed.Kind == TencentKlingVideoKind.Omni || modelId == "kling-video-v4");
        var backendSupport = ModelCapability.When("Depends on the selected VOD model and scenario.");

        var video = new TencentVideoCapabilities
        {
            Text = vod ? backendSupport : ModelCapability.Supported,
            Image = vod ? backendSupport : ModelCapability.Supported,
            FirstFrame = vod ? backendSupport : ModelCapability.Supported,
            LastFrame = vod ? backendSupport : ModelCapability.From(!motion),
            ReferenceImage = vod ? backendSupport : ModelCapability.From(omni || motion),
            ReferenceVideo = vod ? backendSupport : ModelCapability.From(omni || motion),
            GenerateAudio = vod
                ? backendSupport
                : motion
                    ? ModelCapability.When("Keeps or removes the original audio from the motion reference video.")
                    : ModelCapability.From(audioAndMultiShot),
            AspectRatio = vod
                ? backendSupport
                : motion
                    ? ModelCapability.Unsupported
                    : ModelCapability.When("Text-to-video and omni generation only."),
            Resolution = ModelCapability.Supported,
            Duration = vod ? backendSupport : ModelCapability.From(!motion),
            Fps = ModelCapability.Unsupported,
            Seed = ModelCapability.From(vod),
            MaxVideosPerCall = 1,
        };

        var providerOptions = new Dictionary<string, ModelCapability>();
        foreach (var key in TencentProviderOptions.Video.Keys)
        {
            providerOptions[key] = parsed is not null ? ModelCapability.Supported : ModelCapability.Unknown;
        }

        foreach (var key in TokenHubOnlyOptions)
        {
            providerOptions[key] = ModelCapability.From(!vod);
        }

        foreach (var key in VodOnlyOptions)
        {
            providerOptions[key] = ModelCapability.From(vod);
        }

        providerOptions["audio"] = ModelCapability.From(!vod && (motion || audioAndMultiShot));
        providerOptions["multiShot"] = ModelCapability.From(!vod && !motion && audioAndMultiShot);
        providerOptions["characterOrientation"] = ModelCapability.From(motion);
        providerOptions["voices"] = ModelCapability.From(!vod && omni);
        providerOptions["elements"] = vod ? ModelCapability.Unsupported : ModelCapability.When("Image-to-video, omni, or motion control only.");
        providerOptions["referenceVideoType"] = ModelCapability.From(vod || omni);

        return new TencentVideoModelEntry(video, providerOptions, new TencentVideoRequestConfig(parsed, audioAndMultiShot, omni));
    }

    /// <summary>Request-shaping policy for a video model. Upstream <c>getTencentVideoModelEntry(modelId).requestConfig.video</c>.</summary>
    public static TencentVideoRequestConfig GetTencentVideoRequestConfig(string modelId) => GetTencentVideoModelEntry(modelId).RequestConfig;
}

/// <summary>Provider option descriptions. Upstream <c>tencentProviderOptions</c>.</summary>
public static class TencentProviderOptions
{
    /// <summary>Language model options (upstream <c>tencentProviderOptions.language</c>), keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Language = new Dictionary<string, ProviderOptionDefinition>
    {
        ["user"] = new("End-user identifier forwarded by the OpenAI-compatible chat adapter."),
        ["reasoningEffort"] = new("Reasoning effort; support depends on the hosted model."),
        ["textVerbosity"] = new("Text verbosity; support depends on the hosted model."),
        ["strictJsonSchema"] = new("Strict response schema validation when supported by the hosted model."),
    };

    /// <summary>Video model options (upstream <c>tencentProviderOptions.video</c>), keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Video = new Dictionary<string, ProviderOptionDefinition>
    {
        ["resolution"] = new("Output resolution tier, sent as-is. Takes precedence over the top-level `resolution`. TokenHub Kling: `720p`, `1080p`, `4k`. VOD: e.g. `720P`, `1080P`, `2K`, `4K` depending on the model. Both backends."),
        ["audio"] = new("TokenHub Kling: audio track mode. `native` generates audio, `original` keeps the audio of a reference video (omni and motion control), `off` disables audio. Takes precedence over the top-level `generateAudio`."),
        ["multiShot"] = new("TokenHub Kling: whether a multi-shot prompt produces multi-shot output. Supported by `kling-video-v3` and `kling-video-v3-omni`."),
        ["characterOrientation"] = new("TokenHub Kling motion control: whether the character orientation follows the image reference or the motion reference video. Required by the API for motion control models."),
        ["elements"] = new("TokenHub Kling: custom elements (subjects) created through the TokenHub element API. `id` is required by motion control models and lets the prompt reference the element."),
        ["voices"] = new("TokenHub Kling omni: voices to use, referenced in the prompt as `@id`."),
        ["referenceVideoType"] = new("How reference videos from `inputReferences` are used. `feature` uses them as a feature reference, `base` as a video to edit. Defaults to `feature`. TokenHub Kling omni and VOD Kling."),
        ["watermark"] = new("Whether to add a watermark (TokenHub `watermark_info`, VOD `LogoAdd`). Both backends."),
        ["externalTaskId"] = new("TokenHub Kling: custom task ID, unique within the account."),
        ["negativePrompt"] = new("VOD: prompt describing what the model should avoid."),
        ["enhancePrompt"] = new("VOD: whether to optimize the prompt automatically."),
        ["inputRegion"] = new("VOD: where input files are stored. Tencent defaults to `Mainland`."),
        ["sceneType"] = new("VOD: scenario type, e.g. `motion_control`, `avatar_i2v` or `lip_sync` for Kling."),
        ["personGeneration"] = new("VOD: whether people and faces may be generated."),
        ["inputComplianceCheck"] = new("VOD: whether to run a compliance check on the input."),
        ["outputComplianceCheck"] = new("VOD: whether to run a compliance check on the output."),
        ["enhanceSwitch"] = new("VOD: whether to upscale output with video enhancement. Tencent enables enhancement by default when the resolution exceeds what the model renders natively."),
        ["offPeak"] = new("VOD: whether to run the task in off-peak mode."),
        ["sessionId"] = new("VOD: deduplication identifier. A request that repeats an identifier used within the last three days fails."),
        ["sessionContext"] = new("VOD: context string passed through to callbacks."),
        ["tasksPriority"] = new("VOD: task priority from -10 to 10."),
        ["extInfo"] = new("VOD: reserved field for model-specific parameters, sent as-is."),
        ["assets"] = new("VOD: materials registered with `CreateAigcMaterial`, referenced by their `asset://` URL. VOD requires assets for images that show real people. Sent after the other file inputs."),
    };
}

/// <summary>One catalog record of a model id, holding the operations it declares. Upstream <c>ModelEntry</c> in <c>tencentModelCatalog</c>.</summary>
public sealed class TencentModelEntry
{
    internal TencentModelEntry(TencentLanguageModelEntry? language, TencentVideoModelEntry? video)
    {
        Language = language;
        Video = video;
    }

    /// <summary>The language entry, or null when the id is not a language model.</summary>
    public TencentLanguageModelEntry? Language { get; }

    /// <summary>The video entry, or null when the id is not a video model.</summary>
    public TencentVideoModelEntry? Video { get; }
}

/// <summary>The catalog entry of a language model. Upstream <c>getTencentLanguageModelEntry</c> result.</summary>
public sealed class TencentLanguageModelEntry
{
    internal TencentLanguageModelEntry(IReadOnlyDictionary<string, ModelCapability> providerOptions)
    {
        ProviderOptions = providerOptions;
    }

    /// <summary>Image input. Unknown: the hosted model decides.</summary>
    public ModelCapability ImageInput { get; } = ModelCapability.Unknown;

    /// <summary>Audio input.</summary>
    public ModelCapability AudioInput { get; } = ModelCapability.Unknown;

    /// <summary>Object generation.</summary>
    public ModelCapability ObjectGeneration { get; } = ModelCapability.Unknown;

    /// <summary>Tool usage.</summary>
    public ModelCapability ToolUsage { get; } = ModelCapability.Unknown;

    /// <summary>Tool streaming.</summary>
    public ModelCapability ToolStreaming { get; } = ModelCapability.Unknown;

    /// <summary>Reasoning support.</summary>
    public ModelCapability ReasoningSupport { get; } = ModelCapability.Unknown;

    /// <summary>Temperature sampling.</summary>
    public ModelCapability Temperature { get; } = ModelCapability.Unknown;

    /// <summary>Top-p sampling.</summary>
    public ModelCapability TopP { get; } = ModelCapability.Unknown;

    /// <summary>Logprobs.</summary>
    public ModelCapability Logprobs { get; } = ModelCapability.Unknown;

    /// <summary>The model uses the chat completions API.</summary>
    public bool ChatCompletions { get; } = true;

    /// <summary>Support per language option in <see cref="TencentProviderOptions.Language"/>.</summary>
    public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; }
}

/// <summary>The catalog entry of a video model. Upstream <c>getTencentVideoModelEntry</c> result.</summary>
public sealed class TencentVideoModelEntry
{
    internal TencentVideoModelEntry(TencentVideoCapabilities capabilities, IReadOnlyDictionary<string, ModelCapability> providerOptions, TencentVideoRequestConfig requestConfig)
    {
        Capabilities = capabilities;
        ProviderOptions = providerOptions;
        RequestConfig = requestConfig;
    }

    /// <summary>Upstream <c>capabilities.video</c>.</summary>
    public TencentVideoCapabilities Capabilities { get; }

    /// <summary>Support per video option in <see cref="TencentProviderOptions.Video"/>.</summary>
    public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; }

    /// <summary>Upstream <c>requestConfig.video</c>.</summary>
    public TencentVideoRequestConfig RequestConfig { get; }
}

/// <summary>Video capabilities of a model. Upstream <c>ModelCapabilities.video</c>.</summary>
public sealed class TencentVideoCapabilities
{
    /// <summary>Text input.</summary>
    public ModelCapability Text { get; init; } = ModelCapability.Unknown;

    /// <summary>The single starting image passed through <c>image</c>.</summary>
    public ModelCapability Image { get; init; } = ModelCapability.Unknown;

    /// <summary>Role-tagged first frame passed through <c>frameImages</c>.</summary>
    public ModelCapability FirstFrame { get; init; } = ModelCapability.Unknown;

    /// <summary>Role-tagged last frame passed through <c>frameImages</c>.</summary>
    public ModelCapability LastFrame { get; init; } = ModelCapability.Unknown;

    /// <summary>Reference images passed through <c>inputReferences</c>.</summary>
    public ModelCapability ReferenceImage { get; init; } = ModelCapability.Unknown;

    /// <summary>Reference videos passed through <c>inputReferences</c>.</summary>
    public ModelCapability ReferenceVideo { get; init; } = ModelCapability.Unknown;

    /// <summary>Audio generation.</summary>
    public ModelCapability GenerateAudio { get; init; } = ModelCapability.Unknown;

    /// <summary>Aspect ratio.</summary>
    public ModelCapability AspectRatio { get; init; } = ModelCapability.Unknown;

    /// <summary>Resolution.</summary>
    public ModelCapability Resolution { get; init; } = ModelCapability.Unknown;

    /// <summary>Duration.</summary>
    public ModelCapability Duration { get; init; } = ModelCapability.Unknown;

    /// <summary>Frames per second.</summary>
    public ModelCapability Fps { get; init; } = ModelCapability.Unknown;

    /// <summary>Seed.</summary>
    public ModelCapability Seed { get; init; } = ModelCapability.Unknown;

    /// <summary>Fixed limit of videos per call. Null when the catalog declares none.</summary>
    public int? MaxVideosPerCall { get; init; }
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
