// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Tencent;

/// <summary>A custom TokenHub Kling element. Upstream <c>elements</c> entries.</summary>
public sealed class TencentVideoElement
{
    /// <summary>Element id created through the TokenHub element API.</summary>
    public string ElementId { get; set; } = string.Empty;

    /// <summary>Id that lets the prompt reference the element. Required by motion control models.</summary>
    public string? Id { get; set; }
}

/// <summary>A TokenHub Kling omni voice. Upstream <c>voices</c> entries.</summary>
public sealed class TencentVideoVoice
{
    /// <summary>Voice id.</summary>
    public string VoiceId { get; set; } = string.Empty;

    /// <summary>Id the prompt uses as <c>@id</c>.</summary>
    public string Id { get; set; } = string.Empty;
}

/// <summary>A VOD material registered with <c>CreateAigcMaterial</c>. Upstream <c>assets</c> entries.</summary>
public sealed class TencentVideoAsset
{
    /// <summary>Asset URL, for example <c>asset://asset-…</c>.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary><c>FirstFrame</c>, <c>LastFrame</c> or <c>Reference</c>. Defaults to <c>Reference</c>.</summary>
    public string? Usage { get; set; }

    /// <summary><c>Image</c>, <c>Video</c> or <c>Audio</c>. Defaults to <c>Image</c>.</summary>
    public string? Category { get; set; }
}

/// <summary>
/// Provider-specific options for Tencent video generation, passed as <c>providerOptions.tencent</c>.
/// Options apply to TokenHub Kling models (<c>kling-video-*</c>), VOD AIGC models (<c>vod/&lt;ModelName&gt;/&lt;ModelVersion&gt;</c>), or both.
/// An option that does not apply to the selected model is ignored with a warning.
/// </summary>
public sealed class TencentVideoModelOptions
{
    /// <summary>Output resolution tier, sent as is. Takes precedence over the top-level resolution. Both backends.</summary>
    public string? Resolution { get; set; }

    /// <summary>TokenHub Kling: <c>native</c>, <c>original</c> or <c>off</c>. Takes precedence over the top-level generateAudio.</summary>
    public string? Audio { get; set; }

    /// <summary>TokenHub Kling: whether a multi-shot prompt produces multi-shot output.</summary>
    public bool? MultiShot { get; set; }

    /// <summary>TokenHub Kling motion control: <c>image</c> or <c>video</c>.</summary>
    public string? CharacterOrientation { get; set; }

    /// <summary>TokenHub Kling: custom elements.</summary>
    public IReadOnlyList<TencentVideoElement>? Elements { get; set; }

    /// <summary>TokenHub Kling omni: voices, referenced in the prompt as <c>@id</c>.</summary>
    public IReadOnlyList<TencentVideoVoice>? Voices { get; set; }

    /// <summary>How reference videos are used: <c>feature</c> (default) or <c>base</c>. TokenHub Kling omni and VOD Kling.</summary>
    public string? ReferenceVideoType { get; set; }

    /// <summary>Whether to add a watermark (TokenHub <c>watermark_info</c>, VOD <c>LogoAdd</c>). Both backends.</summary>
    public bool? Watermark { get; set; }

    /// <summary>TokenHub Kling: custom task id.</summary>
    public string? ExternalTaskId { get; set; }

    /// <summary>VOD: prompt describing what the model should avoid.</summary>
    public string? NegativePrompt { get; set; }

    /// <summary>VOD: whether to optimize the prompt automatically.</summary>
    public bool? EnhancePrompt { get; set; }

    /// <summary>VOD: where input files are stored: <c>Oversea</c>, <c>OverseaUSWest</c> or <c>Mainland</c>.</summary>
    public string? InputRegion { get; set; }

    /// <summary>VOD: scenario type, for example <c>motion_control</c>.</summary>
    public string? SceneType { get; set; }

    /// <summary>VOD: <c>AllowAdult</c> or <c>Disallowed</c>.</summary>
    public string? PersonGeneration { get; set; }

    /// <summary>VOD: whether to run a compliance check on the input.</summary>
    public bool? InputComplianceCheck { get; set; }

    /// <summary>VOD: whether to run a compliance check on the output.</summary>
    public bool? OutputComplianceCheck { get; set; }

    /// <summary>VOD: whether to upscale output with video enhancement.</summary>
    public bool? EnhanceSwitch { get; set; }

    /// <summary>VOD: whether to run the task in off-peak mode.</summary>
    public bool? OffPeak { get; set; }

    /// <summary>VOD: deduplication identifier.</summary>
    public string? SessionId { get; set; }

    /// <summary>VOD: context string passed through to callbacks.</summary>
    public string? SessionContext { get; set; }

    /// <summary>VOD: task priority from -10 to 10.</summary>
    public int? TasksPriority { get; set; }

    /// <summary>VOD: reserved field for model-specific parameters, sent as is.</summary>
    public string? ExtInfo { get; set; }

    /// <summary>VOD: registered materials, referenced by their <c>asset://</c> URL. Sent after the other file inputs.</summary>
    public IReadOnlyList<TencentVideoAsset>? Assets { get; set; }

    /// <summary>Reads <c>providerOptions.tencent</c>. Returns null when absent. An invalid value throws <see cref="InvalidArgumentException"/>.</summary>
    public static TencentVideoModelOptions? Parse(JsonElement providerOptions)
    {
        if (providerOptions.ValueKind != JsonValueKind.Object
            || !providerOptions.TryGetProperty("tencent", out var raw)
            || raw.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (raw.ValueKind != JsonValueKind.Object)
        {
            throw Invalid("expected an object.");
        }

        var reader = new Reader(raw);
        var options = new TencentVideoModelOptions
        {
            Resolution = reader.String("resolution"),
            Audio = reader.Enum("audio", "native", "original", "off"),
            MultiShot = reader.Bool("multiShot"),
            CharacterOrientation = reader.Enum("characterOrientation", "image", "video"),
            Elements = reader.List("elements", item =>
            {
                var entry = new Reader(item);
                return new TencentVideoElement { ElementId = entry.Required("elementId"), Id = entry.String("id") };
            }),
            Voices = reader.List("voices", item =>
            {
                var entry = new Reader(item);
                return new TencentVideoVoice { VoiceId = entry.Required("voiceId"), Id = entry.Required("id") };
            }),
            ReferenceVideoType = reader.Enum("referenceVideoType", "feature", "base"),
            Watermark = reader.Bool("watermark"),
            ExternalTaskId = reader.String("externalTaskId"),
            NegativePrompt = reader.String("negativePrompt"),
            EnhancePrompt = reader.Bool("enhancePrompt"),
            InputRegion = reader.Enum("inputRegion", "Oversea", "OverseaUSWest", "Mainland"),
            SceneType = reader.String("sceneType"),
            PersonGeneration = reader.Enum("personGeneration", "AllowAdult", "Disallowed"),
            InputComplianceCheck = reader.Bool("inputComplianceCheck"),
            OutputComplianceCheck = reader.Bool("outputComplianceCheck"),
            EnhanceSwitch = reader.Bool("enhanceSwitch"),
            OffPeak = reader.Bool("offPeak"),
            SessionId = reader.String("sessionId"),
            SessionContext = reader.String("sessionContext"),
            TasksPriority = reader.Int("tasksPriority", -10, 10),
            ExtInfo = reader.String("extInfo"),
            Assets = reader.List("assets", item =>
            {
                var entry = new Reader(item);
                return new TencentVideoAsset
                {
                    Url = entry.Required("url"),
                    Usage = entry.Enum("usage", "FirstFrame", "LastFrame", "Reference"),
                    Category = entry.Enum("category", "Image", "Video", "Audio"),
                };
            }),
        };
        return options;
    }

    /// <summary>Whether the option named <paramref name="key"/> (a camelCase option name) is set.</summary>
    internal bool IsSet(string key) => key switch
    {
        "resolution" => Resolution != null,
        "audio" => Audio != null,
        "multiShot" => MultiShot != null,
        "characterOrientation" => CharacterOrientation != null,
        "elements" => Elements != null,
        "voices" => Voices != null,
        "referenceVideoType" => ReferenceVideoType != null,
        "watermark" => Watermark != null,
        "externalTaskId" => ExternalTaskId != null,
        "negativePrompt" => NegativePrompt != null,
        "enhancePrompt" => EnhancePrompt != null,
        "inputRegion" => InputRegion != null,
        "sceneType" => SceneType != null,
        "personGeneration" => PersonGeneration != null,
        "inputComplianceCheck" => InputComplianceCheck != null,
        "outputComplianceCheck" => OutputComplianceCheck != null,
        "enhanceSwitch" => EnhanceSwitch != null,
        "offPeak" => OffPeak != null,
        "sessionId" => SessionId != null,
        "sessionContext" => SessionContext != null,
        "tasksPriority" => TasksPriority != null,
        "extInfo" => ExtInfo != null,
        "assets" => Assets != null,
        _ => false,
    };

    private static InvalidArgumentException Invalid(string message)
    {
        return new InvalidArgumentException("providerOptions.tencent", null, "Invalid Tencent provider options: " + message);
    }

    // Reads one object. A null value counts as not set, like zod `.optional()` after provider-option parsing.
    private readonly struct Reader
    {
        private readonly JsonElement _raw;

        public Reader(JsonElement raw)
        {
            if (raw.ValueKind != JsonValueKind.Object)
            {
                throw Invalid("expected an object.");
            }

            _raw = raw;
        }

        public string? String(string name)
        {
            return Get(name) is { } value ? value.ValueKind == JsonValueKind.String ? value.GetString() : throw Invalid(name + " must be a string.") : null;
        }

        public string Required(string name)
        {
            return String(name) ?? throw Invalid(name + " is required.");
        }

        public bool? Bool(string name)
        {
            return Get(name) is { } value
                ? value.ValueKind is JsonValueKind.True or JsonValueKind.False ? value.GetBoolean() : throw Invalid(name + " must be a boolean.")
                : null;
        }

        public int? Int(string name, int min, int max)
        {
            if (Get(name) is not { } value)
            {
                return null;
            }

            if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var number) || number < min || number > max)
            {
                throw Invalid(name + " must be an integer from " + min + " to " + max + ".");
            }

            return number;
        }

        public string? Enum(string name, params string[] allowed)
        {
            var value = String(name);
            if (value != null && Array.IndexOf(allowed, value) < 0)
            {
                throw Invalid(name + " must be one of " + string.Join(", ", allowed) + ".");
            }

            return value;
        }

        public IReadOnlyList<T>? List<T>(string name, Func<JsonElement, T> read)
        {
            if (Get(name) is not { } value)
            {
                return null;
            }

            if (value.ValueKind != JsonValueKind.Array)
            {
                throw Invalid(name + " must be an array.");
            }

            var items = new List<T>();
            foreach (var item in value.EnumerateArray())
            {
                items.Add(read(item));
            }

            return items;
        }

        private JsonElement? Get(string name)
        {
            return _raw.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null ? value : null;
        }
    }
}
