// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.AssemblyAI;

/// <summary>One custom spelling rule. Maps to an entry of <c>customSpelling</c>.</summary>
public sealed class AssemblyAICustomSpelling
{
    /// <summary>The spellings to replace. Maps to <c>from</c>.</summary>
    public IReadOnlyList<string> From { get; init; } = [];

    /// <summary>The replacement. Maps to <c>to</c>.</summary>
    public string To { get; init; } = string.Empty;
}

/// <summary>Options for automatic language detection. Maps to <c>languageDetectionOptions</c>.</summary>
public sealed class AssemblyAILanguageDetectionOptions
{
    /// <summary>List of languages expected in the audio file. Maps to <c>expectedLanguages</c>.</summary>
    public IReadOnlyList<string>? ExpectedLanguages { get; init; }

    /// <summary>Fallback language if the detected language is not expected. Maps to <c>fallbackLanguage</c>.</summary>
    public string? FallbackLanguage { get; init; }

    /// <summary>Whether code switching should be detected. Maps to <c>codeSwitching</c>.</summary>
    public bool? CodeSwitching { get; init; }

    /// <summary>Confidence threshold for code switching detection (0-1). Maps to <c>codeSwitchingConfidenceThreshold</c>.</summary>
    public double? CodeSwitchingConfidenceThreshold { get; init; }
}

/// <summary>Options for PII-redacted audio files. Maps to <c>redactPiiAudioOptions</c>.</summary>
public sealed class AssemblyAIRedactPiiAudioOptions
{
    /// <summary>Return redacted audio even for files without detected speech. Maps to <c>returnRedactedNoSpeechAudio</c>.</summary>
    public bool? ReturnRedactedNoSpeechAudio { get; init; }

    /// <summary>Redaction method; set to <c>silence</c> to replace PII with silence. Maps to <c>overrideAudioRedactionMethod</c>.</summary>
    public string? OverrideAudioRedactionMethod { get; init; }
}

/// <summary>Options for speaker diarization. Maps to <c>speakerOptions</c>.</summary>
public sealed class AssemblyAISpeakerOptions
{
    /// <summary>Minimum number of speakers expected in the audio file. Maps to <c>minSpeakersExpected</c>.</summary>
    public int? MinSpeakersExpected { get; init; }

    /// <summary>Maximum number of speakers expected in the audio file. Maps to <c>maxSpeakersExpected</c>.</summary>
    public int? MaxSpeakersExpected { get; init; }
}

/// <summary>
/// Provider options for AssemblyAI transcription, read from <c>providerOptions.assemblyai</c>.
/// Maps to <c>AssemblyAITranscriptionModelOptions</c> of <c>assemblyai-transcription-model-options.ts</c>.
/// </summary>
public sealed class AssemblyAITranscriptionModelOptions
{
    /// <summary>End time of the audio in milliseconds. Maps to <c>audioEndAt</c>.</summary>
    public int? AudioEndAt { get; init; }

    /// <summary>Start time of the audio in milliseconds. Maps to <c>audioStartFrom</c>.</summary>
    public int? AudioStartFrom { get; init; }

    /// <summary>Whether to automatically generate chapters. Maps to <c>autoChapters</c>.</summary>
    public bool? AutoChapters { get; init; }

    /// <summary>Whether to automatically generate highlights. Maps to <c>autoHighlights</c>.</summary>
    public bool? AutoHighlights { get; init; }

    /// <summary>Boost parameter for <see cref="WordBoost"/>: <c>low</c>, <c>default</c>, or <c>high</c>. Deprecated, use <see cref="KeytermsPrompt"/>. Maps to <c>boostParam</c>.</summary>
    public string? BoostParam { get; init; }

    /// <summary>Whether to enable content safety filtering. Maps to <c>contentSafety</c>.</summary>
    public bool? ContentSafety { get; init; }

    /// <summary>Confidence threshold for content safety filtering (25-100). Maps to <c>contentSafetyConfidence</c>.</summary>
    public int? ContentSafetyConfidence { get; init; }

    /// <summary>Custom spelling rules. Maps to <c>customSpelling</c>.</summary>
    public IReadOnlyList<AssemblyAICustomSpelling>? CustomSpelling { get; init; }

    /// <summary>Whether to include filler words (um, uh, etc.). Maps to <c>disfluencies</c>.</summary>
    public bool? Disfluencies { get; init; }

    /// <summary>Domain-specific model, currently <c>medical-v1</c>. Maps to <c>domain</c>.</summary>
    public string? Domain { get; init; }

    /// <summary>Whether to enable entity detection. Maps to <c>entityDetection</c>.</summary>
    public bool? EntityDetection { get; init; }

    /// <summary>Whether to filter profanity. Maps to <c>filterProfanity</c>.</summary>
    public bool? FilterProfanity { get; init; }

    /// <summary>Whether to format text with punctuation and capitalization. Maps to <c>formatText</c>.</summary>
    public bool? FormatText { get; init; }

    /// <summary>Whether to enable IAB categories detection. Maps to <c>iabCategories</c>.</summary>
    public bool? IabCategories { get; init; }

    /// <summary>Domain-specific keyterms to boost recognition for (max 6 words per phrase). Maps to <c>keytermsPrompt</c>.</summary>
    public IReadOnlyList<string>? KeytermsPrompt { get; init; }

    /// <summary>Language code for the transcription. Maps to <c>languageCode</c>.</summary>
    public string? LanguageCode { get; init; }

    /// <summary>Confidence threshold for language detection. Maps to <c>languageConfidenceThreshold</c>.</summary>
    public double? LanguageConfidenceThreshold { get; init; }

    /// <summary>Whether to enable language detection. Maps to <c>languageDetection</c>.</summary>
    public bool? LanguageDetection { get; init; }

    /// <summary>Options for automatic language detection. Maps to <c>languageDetectionOptions</c>.</summary>
    public AssemblyAILanguageDetectionOptions? LanguageDetectionOptions { get; init; }

    /// <summary>Whether to process audio as multichannel. Maps to <c>multichannel</c>.</summary>
    public bool? Multichannel { get; init; }

    /// <summary>Natural-language context (up to 1,500 words) to steer the model. Maps to <c>prompt</c>.</summary>
    public string? Prompt { get; init; }

    /// <summary>Whether to add punctuation. Maps to <c>punctuate</c>.</summary>
    public bool? Punctuate { get; init; }

    /// <summary>Whether to redact personally identifiable information (PII). Maps to <c>redactPii</c>.</summary>
    public bool? RedactPii { get; init; }

    /// <summary>Whether to redact PII in the audio file. Maps to <c>redactPiiAudio</c>.</summary>
    public bool? RedactPiiAudio { get; init; }

    /// <summary>Options for PII-redacted audio files. Requires <see cref="RedactPiiAudio"/>. Maps to <c>redactPiiAudioOptions</c>.</summary>
    public AssemblyAIRedactPiiAudioOptions? RedactPiiAudioOptions { get; init; }

    /// <summary>Audio format for PII redaction. Maps to <c>redactPiiAudioQuality</c>.</summary>
    public string? RedactPiiAudioQuality { get; init; }

    /// <summary>List of PII types to redact. Maps to <c>redactPiiPolicies</c>.</summary>
    public IReadOnlyList<string>? RedactPiiPolicies { get; init; }

    /// <summary>Return the original unredacted transcript alongside the redacted one. Requires <see cref="RedactPii"/>. Maps to <c>redactPiiReturnUnredacted</c>.</summary>
    public bool? RedactPiiReturnUnredacted { get; init; }

    /// <summary>Substitution method for redacted PII. Maps to <c>redactPiiSub</c>.</summary>
    public string? RedactPiiSub { get; init; }

    /// <summary>Map of user-defined labels to exact terms to redact. Requires <see cref="RedactPii"/>. Maps to <c>redactStaticEntities</c>.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>>? RedactStaticEntities { get; init; }

    /// <summary>Remove inline annotations from rich transcripts: <c>all</c> or <c>speaker</c>. Maps to <c>removeAudioTags</c>.</summary>
    public string? RemoveAudioTags { get; init; }

    /// <summary>Whether to enable sentiment analysis. Maps to <c>sentimentAnalysis</c>.</summary>
    public bool? SentimentAnalysis { get; init; }

    /// <summary>Whether to identify different speakers. Maps to <c>speakerLabels</c>.</summary>
    public bool? SpeakerLabels { get; init; }

    /// <summary>Options for speaker diarization. Maps to <c>speakerOptions</c>.</summary>
    public AssemblyAISpeakerOptions? SpeakerOptions { get; init; }

    /// <summary>Number of speakers expected in the audio. Maps to <c>speakersExpected</c>.</summary>
    public int? SpeakersExpected { get; init; }

    /// <summary>Threshold for speech detection (0-1). Maps to <c>speechThreshold</c>.</summary>
    public double? SpeechThreshold { get; init; }

    /// <summary>Whether to generate a summary. Maps to <c>summarization</c>.</summary>
    public bool? Summarization { get; init; }

    /// <summary>Model to use for summarization. Maps to <c>summaryModel</c>.</summary>
    public string? SummaryModel { get; init; }

    /// <summary>Type of summary to generate. Maps to <c>summaryType</c>.</summary>
    public string? SummaryType { get; init; }

    /// <summary>Sampling temperature (0-1). Maps to <c>temperature</c>.</summary>
    public double? Temperature { get; init; }

    /// <summary>Name of the authentication header for webhook requests. Maps to <c>webhookAuthHeaderName</c>.</summary>
    public string? WebhookAuthHeaderName { get; init; }

    /// <summary>Value of the authentication header for webhook requests. Maps to <c>webhookAuthHeaderValue</c>.</summary>
    public string? WebhookAuthHeaderValue { get; init; }

    /// <summary>URL to send webhook notifications to. Maps to <c>webhookUrl</c>.</summary>
    public string? WebhookUrl { get; init; }

    /// <summary>List of words to boost recognition for. Deprecated, use <see cref="KeytermsPrompt"/>. Maps to <c>wordBoost</c>.</summary>
    public IReadOnlyList<string>? WordBoost { get; init; }

    /// <summary>Reads <c>providerOptions.assemblyai</c>. Maps to <c>parseProviderOptions</c> with <c>assemblyaiTranscriptionModelOptionsSchema</c>.</summary>
    /// <exception cref="InvalidArgumentException">An option has the wrong shape or is out of range.</exception>
    public static AssemblyAITranscriptionModelOptions? Parse(
        IReadOnlyDictionary<string, JsonElement>? providerOptions
    )
    {
        if (
            providerOptions is null
            || !providerOptions.TryGetValue(AssemblyAIProvider.ProviderId, out var raw)
            || raw.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
        )
        {
            return null;
        }

        if (raw.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidArgumentException(
                "providerOptions.assemblyai",
                raw.ToString(),
                "Expected an object."
            );
        }

        return new AssemblyAITranscriptionModelOptions
        {
            AudioEndAt = Int(raw, "audioEndAt"),
            AudioStartFrom = Int(raw, "audioStartFrom"),
            AutoChapters = Bool(raw, "autoChapters"),
            AutoHighlights = Bool(raw, "autoHighlights"),
            BoostParam = Str(raw, "boostParam"),
            ContentSafety = Bool(raw, "contentSafety"),
            ContentSafetyConfidence = Int(raw, "contentSafetyConfidence", 25, 100),
            CustomSpelling = CustomSpellingList(raw),
            Disfluencies = Bool(raw, "disfluencies"),
            Domain = Str(raw, "domain"),
            EntityDetection = Bool(raw, "entityDetection"),
            FilterProfanity = Bool(raw, "filterProfanity"),
            FormatText = Bool(raw, "formatText"),
            IabCategories = Bool(raw, "iabCategories"),
            KeytermsPrompt = StrList(raw, "keytermsPrompt"),
            LanguageCode = Str(raw, "languageCode"),
            LanguageConfidenceThreshold = Num(raw, "languageConfidenceThreshold"),
            LanguageDetection = Bool(raw, "languageDetection"),
            LanguageDetectionOptions = Object(raw, "languageDetectionOptions") is { } detection
                ? new AssemblyAILanguageDetectionOptions
                {
                    ExpectedLanguages = StrList(
                        detection,
                        "expectedLanguages",
                        "languageDetectionOptions."
                    ),
                    FallbackLanguage = Str(
                        detection,
                        "fallbackLanguage",
                        "languageDetectionOptions."
                    ),
                    CodeSwitching = Bool(detection, "codeSwitching", "languageDetectionOptions."),
                    CodeSwitchingConfidenceThreshold = Num(
                        detection,
                        "codeSwitchingConfidenceThreshold",
                        "languageDetectionOptions.",
                        0,
                        1
                    ),
                }
                : null,
            Multichannel = Bool(raw, "multichannel"),
            Prompt = Str(raw, "prompt"),
            Punctuate = Bool(raw, "punctuate"),
            RedactPii = Bool(raw, "redactPii"),
            RedactPiiAudio = Bool(raw, "redactPiiAudio"),
            RedactPiiAudioOptions = Object(raw, "redactPiiAudioOptions") is { } audio
                ? new AssemblyAIRedactPiiAudioOptions
                {
                    ReturnRedactedNoSpeechAudio = Bool(
                        audio,
                        "returnRedactedNoSpeechAudio",
                        "redactPiiAudioOptions."
                    ),
                    OverrideAudioRedactionMethod = Str(
                        audio,
                        "overrideAudioRedactionMethod",
                        "redactPiiAudioOptions.",
                        ["silence"]
                    ),
                }
                : null,
            RedactPiiAudioQuality = Str(raw, "redactPiiAudioQuality"),
            RedactPiiPolicies = StrList(raw, "redactPiiPolicies"),
            RedactPiiReturnUnredacted = Bool(raw, "redactPiiReturnUnredacted"),
            RedactPiiSub = Str(raw, "redactPiiSub"),
            RedactStaticEntities = StaticEntities(raw),
            RemoveAudioTags = Str(raw, "removeAudioTags", allowed: ["all", "speaker"]),
            SentimentAnalysis = Bool(raw, "sentimentAnalysis"),
            SpeakerLabels = Bool(raw, "speakerLabels"),
            SpeakerOptions = Object(raw, "speakerOptions") is { } speakers
                ? new AssemblyAISpeakerOptions
                {
                    MinSpeakersExpected = Int(
                        speakers,
                        "minSpeakersExpected",
                        prefix: "speakerOptions."
                    ),
                    MaxSpeakersExpected = Int(
                        speakers,
                        "maxSpeakersExpected",
                        prefix: "speakerOptions."
                    ),
                }
                : null,
            SpeakersExpected = Int(raw, "speakersExpected"),
            SpeechThreshold = Num(raw, "speechThreshold", min: 0, max: 1),
            Summarization = Bool(raw, "summarization"),
            SummaryModel = Str(raw, "summaryModel"),
            SummaryType = Str(raw, "summaryType"),
            Temperature = Num(raw, "temperature", min: 0, max: 1),
            WebhookAuthHeaderName = Str(raw, "webhookAuthHeaderName"),
            WebhookAuthHeaderValue = Str(raw, "webhookAuthHeaderValue"),
            WebhookUrl = Str(raw, "webhookUrl"),
            WordBoost = StrList(raw, "wordBoost"),
        };
    }

    private static JsonElement? Get(JsonElement options, string name)
    {
        return
            options.TryGetProperty(name, out var value)
            && value.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined)
            ? value
            : null;
    }

    private static JsonElement? Object(JsonElement options, string name)
    {
        if (Get(options, name) is not { } value)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.Object
            ? value
            : throw Invalid(name, value, "Expected an object.");
    }

    private static bool? Bool(JsonElement options, string name, string prefix = "")
    {
        if (Get(options, name) is not { } value)
        {
            return null;
        }

        return value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : throw Invalid(prefix + name, value, "Expected a boolean.");
    }

    private static string? Str(
        JsonElement options,
        string name,
        string prefix = "",
        string[]? allowed = null
    )
    {
        if (Get(options, name) is not { } value)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            throw Invalid(prefix + name, value, "Expected a string.");
        }

        var text = value.GetString()!;
        if (allowed is not null && Array.IndexOf(allowed, text) < 0)
        {
            throw Invalid(
                prefix + name,
                value,
                "Expected one of: " + string.Join(", ", allowed) + "."
            );
        }

        return text;
    }

    private static int? Int(
        JsonElement options,
        string name,
        int min = int.MinValue,
        int max = int.MaxValue,
        string prefix = ""
    )
    {
        if (Get(options, name) is not { } value)
        {
            return null;
        }

        if (
            value.ValueKind != JsonValueKind.Number
            || !value.TryGetDouble(out var whole)
            || whole != Math.Floor(whole)
            || whole < min
            || whole > max
        )
        {
            throw Invalid(
                prefix + name,
                value,
                min == int.MinValue && max == int.MaxValue
                    ? "Expected an integer."
                    : "Expected an integer between "
                        + min.ToString(CultureInfo.InvariantCulture)
                        + " and "
                        + max.ToString(CultureInfo.InvariantCulture)
                        + "."
            );
        }

        return (int)whole;
    }

    private static double? Num(
        JsonElement options,
        string name,
        string prefix = "",
        double min = double.NegativeInfinity,
        double max = double.PositiveInfinity
    )
    {
        if (Get(options, name) is not { } value)
        {
            return null;
        }

        if (
            value.ValueKind != JsonValueKind.Number
            || value.GetDouble() < min
            || value.GetDouble() > max
        )
        {
            throw Invalid(
                prefix + name,
                value,
                double.IsInfinity(min) && double.IsInfinity(max)
                    ? "Expected a number."
                    : "Expected a number between "
                        + min.ToString(CultureInfo.InvariantCulture)
                        + " and "
                        + max.ToString(CultureInfo.InvariantCulture)
                        + "."
            );
        }

        return value.GetDouble();
    }

    private static List<string>? StrList(JsonElement options, string name, string prefix = "")
    {
        if (Get(options, name) is not { } value)
        {
            return null;
        }

        return StrArray(value, prefix + name);
    }

    private static List<string> StrArray(JsonElement value, string path)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw Invalid(path, value, "Expected an array of strings.");
        }

        var list = new List<string>();
        foreach (var item in value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                throw Invalid(path, item, "Expected an array of strings.");
            }

            list.Add(item.GetString()!);
        }

        return list;
    }

    private static List<AssemblyAICustomSpelling>? CustomSpellingList(JsonElement options)
    {
        if (Get(options, "customSpelling") is not { } value)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.Array)
        {
            throw Invalid("customSpelling", value, "Expected an array.");
        }

        var list = new List<AssemblyAICustomSpelling>();
        foreach (var item in value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                throw Invalid("customSpelling", item, "Expected an object with from and to.");
            }

            if (!item.TryGetProperty("from", out var from))
            {
                throw Invalid("customSpelling.from", item, "Required.");
            }

            if (!item.TryGetProperty("to", out var to) || to.ValueKind != JsonValueKind.String)
            {
                throw Invalid("customSpelling.to", item, "Expected a string.");
            }

            list.Add(
                new AssemblyAICustomSpelling
                {
                    From = StrArray(from, "customSpelling.from"),
                    To = to.GetString()!,
                }
            );
        }

        return list;
    }

    private static Dictionary<string, IReadOnlyList<string>>? StaticEntities(JsonElement options)
    {
        if (Object(options, "redactStaticEntities") is not { } value)
        {
            return null;
        }

        var map = new Dictionary<string, IReadOnlyList<string>>();
        foreach (var property in value.EnumerateObject())
        {
            map[property.Name] = StrArray(property.Value, "redactStaticEntities." + property.Name);
        }

        return map;
    }

    private static InvalidArgumentException Invalid(string name, JsonElement value, string message)
    {
        return new InvalidArgumentException(
            "providerOptions.assemblyai." + name,
            value.ToString(),
            message
        );
    }
}
