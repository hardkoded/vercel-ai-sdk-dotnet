// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.AssemblyAI;

/// <summary>One word from an AssemblyAI transcript, with timings converted to seconds.</summary>
public sealed class AssemblyAISegment
{
    /// <summary>Creates a segment.</summary>
    public AssemblyAISegment(string text, double startSecond, double endSecond)
    {
        Text = text ?? string.Empty;
        StartSecond = startSecond;
        EndSecond = endSecond;
    }

    /// <summary>Word text.</summary>
    public string Text { get; }

    /// <summary>Start time in seconds.</summary>
    public double StartSecond { get; }

    /// <summary>End time in seconds.</summary>
    public double EndSecond { get; }
}

/// <summary>A setting AssemblyAI ignored or will stop accepting.</summary>
public sealed class AssemblyAITranscriptionWarning
{
    /// <summary>Creates a warning.</summary>
    public AssemblyAITranscriptionWarning(string type, string message, string? setting)
    {
        Type = type ?? string.Empty;
        Message = message ?? string.Empty;
        Setting = setting;
    }

    /// <summary>Warning category, such as <c>deprecated</c> or <c>other</c>.</summary>
    public string Type { get; }

    /// <summary>Explanation.</summary>
    public string Message { get; }

    /// <summary>Setting the warning is about, when there is one.</summary>
    public string? Setting { get; }
}

/// <summary>Audio plus AssemblyAI provider options for one transcription.</summary>
public sealed class AssemblyAITranscriptionRequest
{
    /// <summary>Creates a request.</summary>
    public AssemblyAITranscriptionRequest(byte[] audio)
    {
        Audio = audio ?? throw new ArgumentNullException(nameof(audio));
    }

    /// <summary>Audio bytes uploaded to <c>/v2/upload</c>.</summary>
    public byte[] Audio { get; }

    /// <summary>Declared media type. AssemblyAI uploads the raw bytes.</summary>
    public string? MediaType { get; set; }

    /// <summary>AssemblyAI provider options. A wrapping <c>assemblyai</c> object is accepted.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Headers merged over the provider headers for this call.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>Completed AssemblyAI transcription.</summary>
public sealed class AssemblyAITranscriptionResult
{
    /// <summary>Creates a result.</summary>
    public AssemblyAITranscriptionResult(
        string text,
        IReadOnlyList<AssemblyAISegment> segments,
        string? language,
        double? durationInSeconds,
        IReadOnlyList<AssemblyAITranscriptionWarning> warnings,
        JsonObject? providerMetadata,
        DateTimeOffset timestamp,
        string modelId,
        IReadOnlyDictionary<string, string> responseHeaders,
        JsonNode? responseBody)
    {
        Text = text ?? string.Empty;
        Segments = segments ?? Array.Empty<AssemblyAISegment>();
        Language = language;
        DurationInSeconds = durationInSeconds;
        Warnings = warnings ?? Array.Empty<AssemblyAITranscriptionWarning>();
        ProviderMetadata = providerMetadata;
        Timestamp = timestamp;
        ModelId = modelId ?? string.Empty;
        ResponseHeaders = responseHeaders ?? new Dictionary<string, string>();
        ResponseBody = responseBody;
    }

    /// <summary>Transcript text.</summary>
    public string Text { get; }

    /// <summary>Words with timings in seconds.</summary>
    public IReadOnlyList<AssemblyAISegment> Segments { get; }

    /// <summary>Detected language code.</summary>
    public string? Language { get; }

    /// <summary>Audio duration in seconds.</summary>
    public double? DurationInSeconds { get; }

    /// <summary>Warnings produced while building the request.</summary>
    public IReadOnlyList<AssemblyAITranscriptionWarning> Warnings { get; }

    /// <summary>Diarization and audio-intelligence fields under <c>assemblyai</c>.</summary>
    public JsonObject? ProviderMetadata { get; }

    /// <summary>Clock value captured when the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Model id sent to AssemblyAI.</summary>
    public string ModelId { get; }

    /// <summary>Headers from the final transcript GET.</summary>
    public IReadOnlyDictionary<string, string> ResponseHeaders { get; }

    /// <summary>Raw JSON from the final transcript GET.</summary>
    public JsonNode? ResponseBody { get; }
}

/// <summary>AssemblyAI pre-recorded transcription. Uploads audio, submits <c>/v2/transcript</c>, and polls.</summary>
public sealed class AssemblyAITranscriptionModel : ITranscriptionModel
{
    private const string SpeechModelDocs = "https://www.assemblyai.com/docs/pre-recorded-audio/select-the-speech-model";

    private static readonly string[][] Scalars =
    {
        new[] { "audioEndAt", "audio_end_at" },
        new[] { "audioStartFrom", "audio_start_from" },
        new[] { "autoChapters", "auto_chapters" },
        new[] { "autoHighlights", "auto_highlights" },
        new[] { "boostParam", "boost_param" },
        new[] { "contentSafety", "content_safety" },
        new[] { "contentSafetyConfidence", "content_safety_confidence" },
        new[] { "customSpelling", "custom_spelling" },
        new[] { "disfluencies", "disfluencies" },
        new[] { "entityDetection", "entity_detection" },
        new[] { "filterProfanity", "filter_profanity" },
        new[] { "formatText", "format_text" },
        new[] { "iabCategories", "iab_categories" },
        new[] { "languageCode", "language_code" },
        new[] { "languageConfidenceThreshold", "language_confidence_threshold" },
        new[] { "languageDetection", "language_detection" },
        new[] { "multichannel", "multichannel" },
        new[] { "punctuate", "punctuate" },
        new[] { "redactPii", "redact_pii" },
        new[] { "redactPiiAudio", "redact_pii_audio" },
        new[] { "redactPiiAudioQuality", "redact_pii_audio_quality" },
        new[] { "redactPiiPolicies", "redact_pii_policies" },
        new[] { "redactPiiSub", "redact_pii_sub" },
        new[] { "sentimentAnalysis", "sentiment_analysis" },
        new[] { "speakerLabels", "speaker_labels" },
        new[] { "speakersExpected", "speakers_expected" },
        new[] { "speechThreshold", "speech_threshold" },
        new[] { "summarization", "summarization" },
        new[] { "summaryModel", "summary_model" },
        new[] { "summaryType", "summary_type" },
        new[] { "webhookAuthHeaderName", "webhook_auth_header_name" },
        new[] { "webhookAuthHeaderValue", "webhook_auth_header_value" },
        new[] { "webhookUrl", "webhook_url" },
        new[] { "wordBoost", "word_boost" },
        new[] { "keytermsPrompt", "keyterms_prompt" },
        new[] { "prompt", "prompt" },
        new[] { "temperature", "temperature" },
        new[] { "removeAudioTags", "remove_audio_tags" },
        new[] { "domain", "domain" },
        new[] { "redactPiiReturnUnredacted", "redact_pii_return_unredacted" },
        new[] { "redactStaticEntities", "redact_static_entities" },
    };

    private readonly HttpClient _http;
    private readonly Func<IReadOnlyDictionary<string, string?>> _headers;

    /// <summary>Creates a transcription model.</summary>
    public AssemblyAITranscriptionModel(HttpClient http, string modelId, string provider, string baseUrl, Func<IReadOnlyDictionary<string, string?>> headers)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        Provider = string.IsNullOrEmpty(provider) ? "assemblyai.transcription" : provider;
        BaseUrl = string.IsNullOrEmpty(baseUrl) ? AssemblyAIProvider.DefaultBaseUrl : baseUrl.TrimEnd('/');
        _headers = headers ?? throw new ArgumentNullException(nameof(headers));
    }

    /// <inheritdoc />
    public string Provider { get; }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>API origin without a trailing slash.</summary>
    public string BaseUrl { get; }

    /// <summary>Delay between transcript polls. Tests set this to zero.</summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(3000);

    /// <summary>Clock used for <see cref="AssemblyAITranscriptionResult.Timestamp"/>.</summary>
    public Func<DateTimeOffset> Clock { get; set; } = () => DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        var result = await TranscribeAsync(new AssemblyAITranscriptionRequest(audio.Data) { MediaType = audio.MediaType }, cancellationToken).ConfigureAwait(false);
        return new TranscriptionResult(result.Text);
    }

    /// <summary>Uploads audio, submits a transcript, and polls until it completes.</summary>
    public async Task<AssemblyAITranscriptionResult> TranscribeAsync(AssemblyAITranscriptionRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var timestamp = Clock();
        var headers = Merge(request.Headers);
        var uploaded = await MediaExchange.SendAsync(_http, HttpMethod.Post, Url("/v2/upload"), MediaExchange.Octet(request.Audio), headers, cancellationToken).ConfigureAwait(false);
        EnsureSuccess(uploaded);
        var audioUrl = UploadUrl(uploaded.Text);

        var warnings = new List<AssemblyAITranscriptionWarning>();
        var body = BuildBody(audioUrl, request.ProviderOptions, warnings);
        var submitted = await MediaExchange.SendAsync(_http, HttpMethod.Post, Url("/v2/transcript"), MediaExchange.Json(body.ToJsonString()), headers, cancellationToken).ConfigureAwait(false);
        EnsureSuccess(submitted);
        var id = StringOf(Parse(submitted.Text), "id");
        if (string.IsNullOrEmpty(id))
        {
            id = "id_1";
        }

        var pollUrl = Url("/v2/transcript/" + id);
        JsonNode? raw = null;
        MediaResponse final = submitted;
        for (var attempt = 0; attempt < 60; attempt++)
        {
            final = await MediaExchange.SendAsync(_http, HttpMethod.Get, pollUrl, null, headers, cancellationToken).ConfigureAwait(false);
            EnsureSuccess(final);
            raw = ParseNode(final.Text);
            var status = raw is JsonObject obj ? obj["status"]?.GetValue<string>() : null;
            if (string.Equals(status, "completed", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(status))
            {
                break;
            }

            if (string.Equals(status, "error", StringComparison.OrdinalIgnoreCase))
            {
                var reason = raw is JsonObject failed && failed["error"] is JsonValue ? failed["error"]!.GetValue<string>() : null;
                throw new AiSdkException("Transcription failed: " + (string.IsNullOrEmpty(reason) ? "Unknown error" : reason));
            }

            if (PollInterval > TimeSpan.Zero)
            {
                await Task.Delay(PollInterval, cancellationToken).ConfigureAwait(false);
            }
        }

        return ParseResult(raw, warnings, timestamp, final.Headers);
    }

    private AssemblyAITranscriptionResult ParseResult(JsonNode? raw, List<AssemblyAITranscriptionWarning> warnings, DateTimeOffset timestamp, IReadOnlyDictionary<string, string> headers)
    {
        var root = raw as JsonObject ?? new JsonObject();
        var text = root["text"]?.GetValue<string>() ?? string.Empty;
        var segments = new List<AssemblyAISegment>();
        double? lastEnd = null;
        if (root["words"] is JsonArray words)
        {
            foreach (var word in words)
            {
                if (word is not JsonObject item)
                {
                    continue;
                }

                var start = Number(item, "start");
                var end = Number(item, "end");
                lastEnd = end;
                segments.Add(new AssemblyAISegment(item["text"]?.GetValue<string>() ?? string.Empty, start / 1000d, end / 1000d));
            }
        }

        double? duration = NumberOrNull(root, "audio_duration");
        if (duration == null && lastEnd is double endMs)
        {
            duration = endMs / 1000d;
        }

        return new AssemblyAITranscriptionResult(
            text,
            segments,
            root["language_code"]?.GetValue<string>(),
            duration,
            warnings,
            Metadata(root),
            timestamp,
            ModelId,
            headers,
            raw);
    }

    private static JsonObject? Metadata(JsonObject root)
    {
        var assembly = new JsonObject();
        CopyMeta(root, assembly, "utterances", "utterances");
        CopyMeta(root, assembly, "sentiment_analysis_results", "sentimentAnalysisResults");
        CopyMeta(root, assembly, "entities", "entities");
        CopyMeta(root, assembly, "content_safety_labels", "contentSafetyLabels");
        CopyMeta(root, assembly, "iab_categories_result", "iabCategoriesResult");
        CopyMeta(root, assembly, "auto_highlights_result", "autoHighlightsResult");
        if (assembly.Count == 0)
        {
            return null;
        }

        return new JsonObject { ["assemblyai"] = assembly };
    }

    private static void CopyMeta(JsonObject root, JsonObject target, string source, string name)
    {
        if (root[source] != null)
        {
            target[name] = JsonNode.Parse(root[source]!.ToJsonString());
        }
    }

    private JsonObject BuildBody(string audioUrl, JsonElement? providerOptions, List<AssemblyAITranscriptionWarning> warnings)
    {
        var body = new JsonObject();
        if (string.Equals(ModelId, "best", StringComparison.Ordinal))
        {
            body["speech_model"] = ModelId;
            warnings.Add(new AssemblyAITranscriptionWarning(
                "deprecated",
                "The 'best' model is a legacy AssemblyAI model. Use 'universal-3-5-pro' instead. See documentation: " + SpeechModelDocs,
                "model 'best'"));
        }
        else
        {
            body["speech_models"] = new JsonArray { ModelId };
            if (string.Equals(ModelId, "universal-3-pro", StringComparison.Ordinal))
            {
                warnings.Add(new AssemblyAITranscriptionWarning(
                    "other",
                    "'universal-3-5-pro' is AssemblyAI's latest flagship model and is set to replace 'universal-3-pro'. See " + SpeechModelDocs,
                    null));
            }
            else if (string.Equals(ModelId, "universal-2", StringComparison.Ordinal))
            {
                warnings.Add(new AssemblyAITranscriptionWarning(
                    "other",
                    "'universal-3-5-pro' is AssemblyAI's latest flagship model. See " + SpeechModelDocs,
                    null));
            }
        }

        var options = Unwrap(providerOptions);
        if (options is { } element)
        {
            foreach (var pair in Scalars)
            {
                if (TryRaw(element, pair[0], out var raw))
                {
                    body[pair[1]] = JsonNode.Parse(raw);
                }
            }

            CopyGroup(element, body, "speakerOptions", "speaker_options", new[] { "minSpeakersExpected", "min_speakers_expected" }, new[] { "maxSpeakersExpected", "max_speakers_expected" });
            CopyGroup(
                element,
                body,
                "languageDetectionOptions",
                "language_detection_options",
                new[] { "expectedLanguages", "expected_languages" },
                new[] { "fallbackLanguage", "fallback_language" },
                new[] { "codeSwitching", "code_switching" },
                new[] { "codeSwitchingConfidenceThreshold", "code_switching_confidence_threshold" });
            CopyGroup(
                element,
                body,
                "redactPiiAudioOptions",
                "redact_pii_audio_options",
                new[] { "returnRedactedNoSpeechAudio", "return_redacted_no_speech_audio" },
                new[] { "overrideAudioRedactionMethod", "override_audio_redaction_method" });
            AddOptionWarnings(element, warnings);
        }

        body["audio_url"] = audioUrl;
        return body;
    }

    private static void AddOptionWarnings(JsonElement options, List<AssemblyAITranscriptionWarning> warnings)
    {
        var deprecated = new List<string>();
        if (Present(options, "wordBoost"))
        {
            deprecated.Add("wordBoost");
        }

        if (Present(options, "boostParam"))
        {
            deprecated.Add("boostParam");
        }

        if (deprecated.Count > 0)
        {
            warnings.Add(new AssemblyAITranscriptionWarning(
                "deprecated",
                "'wordBoost' and 'boostParam' are deprecated and are rejected by 'universal-3-pro' / 'universal-3-5-pro' and 'slam-1'. Use 'keytermsPrompt' instead.",
                string.Join(", ", deprecated)));
        }

        if ((Present(options, "redactPiiReturnUnredacted") || Present(options, "redactStaticEntities")) && !IsTrue(options, "redactPii"))
        {
            warnings.Add(new AssemblyAITranscriptionWarning(
                "other",
                "'redactPiiReturnUnredacted' and 'redactStaticEntities' require 'redactPii' to be enabled; AssemblyAI rejects the request otherwise.",
                null));
        }

        if (Present(options, "redactPiiAudioOptions") && !IsTrue(options, "redactPiiAudio"))
        {
            warnings.Add(new AssemblyAITranscriptionWarning(
                "other",
                "'redactPiiAudioOptions' only applies when 'redactPiiAudio' is enabled; it is otherwise ignored.",
                null));
        }

        if (Present(options, "languageCode") && IsTrue(options, "languageDetection"))
        {
            warnings.Add(new AssemblyAITranscriptionWarning(
                "other",
                "'languageDetection' cannot be combined with an explicit 'languageCode'; AssemblyAI rejects requests that set both.",
                null));
        }
    }

    private static void CopyGroup(JsonElement options, JsonObject body, string source, string target, params string[][] fields)
    {
        if (!options.TryGetProperty(source, out var group) || group.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        var nested = new JsonObject();
        foreach (var field in fields)
        {
            if (TryRaw(group, field[0], out var raw))
            {
                nested[field[1]] = JsonNode.Parse(raw);
            }
        }

        body[target] = nested;
    }

    private static JsonElement? Unwrap(JsonElement? options)
    {
        if (options is not { } element || element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (element.TryGetProperty("assemblyai", out var nested) && nested.ValueKind == JsonValueKind.Object)
        {
            return nested;
        }

        return element;
    }

    private static bool Present(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null && value.ValueKind != JsonValueKind.Undefined;
    }

    private static bool IsTrue(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;
    }

    private static bool TryRaw(JsonElement element, string name, out string raw)
    {
        raw = string.Empty;
        if (!element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
        {
            return false;
        }

        raw = value.GetRawText();
        return true;
    }

    private static string UploadUrl(string json)
    {
        var root = Parse(json);
        var upload = StringOf(root, "upload_url");
        if (!string.IsNullOrEmpty(upload))
        {
            return upload!;
        }

        var url = StringOf(root, "url");
        return string.IsNullOrEmpty(url) ? string.Empty : url!;
    }

    private static JsonElement Parse(string json)
    {
        using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json))
        {
            return document.RootElement.Clone();
        }
    }

    private static JsonNode? ParseNode(string json)
    {
        return JsonNode.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
    }

    private static string? StringOf(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return value.GetString();
    }

    private static double Number(JsonObject item, string name)
    {
        var node = item[name];
        if (node == null)
        {
            return 0;
        }

        return node.GetValue<double>();
    }

    private static double? NumberOrNull(JsonObject item, string name)
    {
        var node = item[name];
        if (node == null)
        {
            return null;
        }

        return node.GetValue<double>();
    }

    private static void EnsureSuccess(MediaResponse response)
    {
        if (response.StatusCode >= 200 && response.StatusCode <= 299)
        {
            return;
        }

        if (AssemblyAIError.TryParse(response.Text, out var error) && error != null)
        {
            throw new AiSdkException(error.Message);
        }

        throw new AiSdkException(JsonValues.ExtractErrorMessage(response.Text, response.StatusCode));
    }

    private Uri Url(string path)
    {
        return ApiKeys.Combine(BaseUrl, path);
    }

    private Dictionary<string, string?> Merge(IDictionary<string, string>? requestHeaders)
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in _headers())
        {
            headers[pair.Key] = pair.Value;
        }

        if (requestHeaders != null)
        {
            foreach (var pair in requestHeaders)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }
}
