// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Gladia;

/// <summary>
/// Cause of the <c>TranscriptionJobFailed</c> error. Carries the Gladia result that upstream passes as <c>cause</c>.
/// .NET-only: a JavaScript <c>cause</c> can be any value, but an <see cref="Exception"/> cause must be an exception.
/// </summary>
public sealed class GladiaTranscriptionJobResultException : Exception
{
    /// <summary>Creates the cause for a failed job.</summary>
    public GladiaTranscriptionJobResultException(string message, JsonObject result, int? errorCode)
        : base(message)
    {
        Result = result;
        ErrorCode = errorCode;
    }

    /// <summary>The parsed Gladia result of the failed job.</summary>
    public JsonObject Result { get; }

    /// <summary>The <c>error_code</c> of the result, when Gladia sent one.</summary>
    public int? ErrorCode { get; }
}

/// <summary>Gladia settings.</summary>
public class GladiaOptions : OpenAICompatibleOptions
{
    /// <summary>Clock used for response timestamps and to measure the polling budget.</summary>
    public Func<DateTimeOffset>? Clock { get; set; }

    /// <summary>Wait between result polls. Defaults to <see cref="PollIntervalMs"/>.</summary>
    public Func<CancellationToken, Task>? PollDelay { get; set; }

    /// <summary>
    /// Interval in milliseconds between transcription job status checks. Defaults to 1000 (1 second).
    /// Must be a positive integer. A per-call <see cref="GladiaTranscriptionRequest.PollIntervalMs"/> overrides it.
    /// </summary>
    public int? PollIntervalMs { get; set; }

    /// <summary>
    /// Maximum time in milliseconds to wait for an asynchronous transcription job to finish before throwing
    /// a <c>TranscriptionJobPollingTimedOut</c> error. Defaults to 600000 (10 minutes). Must be a positive integer.
    /// A per-call <see cref="GladiaTranscriptionRequest.PollTimeoutMs"/> overrides it.
    /// </summary>
    public int? PollTimeoutMs { get; set; }
}

/// <summary>Parsed Gladia error payload.</summary>
public sealed class GladiaErrorData
{
    /// <summary>Creates parsed error data.</summary>
    public GladiaErrorData(string message, int code)
    {
        Message = message ?? string.Empty;
        Code = code;
    }

    /// <summary>Provider message.</summary>
    public string Message { get; }

    /// <summary>Provider code.</summary>
    public int Code { get; }
}

/// <summary>Result of parsing a Gladia error body.</summary>
public sealed class GladiaErrorParseResult
{
    /// <summary>Creates a parse result.</summary>
    public GladiaErrorParseResult(bool success, GladiaErrorData? value)
    {
        Success = success;
        Value = value;
        RawValue = value;
    }

    /// <summary>True when the error object was present.</summary>
    public bool Success { get; }

    /// <summary>Parsed error.</summary>
    public GladiaErrorData? Value { get; }

    /// <summary>Same payload as <see cref="Value"/>.</summary>
    public GladiaErrorData? RawValue { get; }
}

/// <summary>Parses Gladia error JSON.</summary>
public static class GladiaError
{
    /// <summary>Parses a resource error body.</summary>
    public static GladiaErrorParseResult Parse(string json)
    {
        if (ProviderExchange.TryParseError(json, out var message, out var code))
        {
            return new GladiaErrorParseResult(true, new GladiaErrorData(message, code));
        }

        return new GladiaErrorParseResult(false, null);
    }
}

/// <summary>Headers and extra pre-recorded fields for one Gladia transcription.</summary>
public sealed class GladiaTranscriptionRequest
{
    /// <summary>Headers merged over the provider headers.</summary>
    public IDictionary<string, string>? Headers { get; set; }

    /// <summary>Provider options. The <c>gladia</c> object is read, with camelCase keys.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }

    /// <summary>
    /// Extra JSON fields sent with <c>audio_url</c>, in the snake_case names of the Gladia API. .NET-only: they are
    /// merged over the fields built from <see cref="ProviderOptions"/>.
    /// </summary>
    public JsonObject? Body { get; set; }

    /// <summary>
    /// Interval in milliseconds between job status checks for this call. Overrides
    /// <see cref="GladiaOptions.PollIntervalMs"/>. Must be a positive integer. Not sent to the Gladia API.
    /// </summary>
    public int? PollIntervalMs { get; set; }

    /// <summary>
    /// Maximum time in milliseconds to wait for the job to finish for this call. Overrides
    /// <see cref="GladiaOptions.PollTimeoutMs"/>. Must be a positive integer. Not sent to the Gladia API.
    /// </summary>
    public int? PollTimeoutMs { get; set; }
}

/// <summary>One Gladia utterance mapped to a segment.</summary>
public sealed class GladiaTranscriptionSegment
{
    /// <summary>Creates a segment.</summary>
    public GladiaTranscriptionSegment(string text, double startSecond, double endSecond)
    {
        Text = text ?? string.Empty;
        StartSecond = startSecond;
        EndSecond = endSecond;
    }

    /// <summary>Utterance text.</summary>
    public string Text { get; }

    /// <summary>Start time in seconds.</summary>
    public double StartSecond { get; }

    /// <summary>End time in seconds.</summary>
    public double EndSecond { get; }
}

/// <summary>Gladia response metadata.</summary>
public sealed class GladiaTranscriptionResponse
{
    /// <summary>Creates response metadata.</summary>
    public GladiaTranscriptionResponse(DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string> headers)
    {
        Timestamp = timestamp;
        ModelId = modelId ?? string.Empty;
        Headers = headers ?? new Dictionary<string, string>();
    }

    /// <summary>Timestamp captured when the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Model id. Gladia pre-recorded calls use <c>default</c>.</summary>
    public string ModelId { get; }

    /// <summary>Headers from the result response.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }
}

/// <summary>Gladia transcription, including utterance metadata.</summary>
public sealed class GladiaTranscriptionResult
{
    /// <summary>Creates a transcription result.</summary>
    public GladiaTranscriptionResult(
        string text,
        IReadOnlyList<GladiaTranscriptionSegment> segments,
        string? language,
        double? durationInSeconds,
        JsonElement providerMetadata,
        IReadOnlyList<ModelWarning> warnings,
        GladiaTranscriptionResponse response)
    {
        Text = text ?? string.Empty;
        Segments = segments ?? Array.Empty<GladiaTranscriptionSegment>();
        Language = language;
        DurationInSeconds = durationInSeconds;
        ProviderMetadata = providerMetadata;
        Warnings = warnings ?? Array.Empty<ModelWarning>();
        Response = response;
    }

    /// <summary>Full transcript.</summary>
    public string Text { get; }

    /// <summary>Utterances.</summary>
    public IReadOnlyList<GladiaTranscriptionSegment> Segments { get; }

    /// <summary>First detected language.</summary>
    public string? Language { get; }

    /// <summary>Audio duration in seconds.</summary>
    public double? DurationInSeconds { get; }

    /// <summary>Raw Gladia result object.</summary>
    public JsonElement ProviderMetadata { get; }

    /// <summary>Warnings produced while building the request.</summary>
    public IReadOnlyList<ModelWarning> Warnings { get; }

    /// <summary>Response metadata.</summary>
    public GladiaTranscriptionResponse Response { get; }
}

/// <summary>Gladia pre-recorded transcription provider.</summary>
public sealed class GladiaProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "gladia";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.gladia.io";

    private const int DefaultPollIntervalMs = 1000;
    private const int DefaultPollTimeoutMs = 10 * 60 * 1000;

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public GladiaProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Creates a provider.</summary>
    public static new GladiaProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new GladiaProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("Gladia does not provide language models.");
    }

    /// <summary>Creates the pre-recorded transcription model.</summary>
    public GladiaTranscriptionModel Transcription()
    {
        return new GladiaTranscriptionModel(this, "default");
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId)
    {
        return new GladiaTranscriptionModel(this, string.IsNullOrEmpty(modelId) ? "default" : modelId);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options = options?.Copy() ?? new OpenAICompatibleOptions();
        if (options is GladiaOptions gladia)
        {
            ValidatePollingSettings(gladia.PollIntervalMs, gladia.PollTimeoutMs);
        }

        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "GLADIA_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.CustomHeader;
        options.ApiKeyHeaderName = "x-gladia-key";
        options.UserAgent = ProviderExchange.UserAgent("gladia");
        if (!options.Headers.ContainsKey("x-gladia-version"))
        {
            options.Headers["x-gladia-version"] = "ai-sdk/" + Operations.AiSdkVersion.Version;
        }

        return options;
    }

    private static void ValidatePollingSettings(int? pollIntervalMs, int? pollTimeoutMs)
    {
        ValidatePollSetting("PollIntervalMs", pollIntervalMs);
        ValidatePollSetting("PollTimeoutMs", pollTimeoutMs);
    }

    private static void ValidatePollSetting(string parameter, int? value)
    {
        if (value is int milliseconds && milliseconds <= 0)
        {
            throw new InvalidArgumentError(parameter, value, parameter + " must be a positive integer number of milliseconds");
        }
    }

    /// <summary>Uploads audio, starts a pre-recorded job, and reads the result.</summary>
    public sealed class GladiaTranscriptionModel : ITranscriptionModel
    {
        private readonly GladiaProvider _provider;

        /// <summary>Creates a transcription model.</summary>
        public GladiaTranscriptionModel(GladiaProvider provider, string modelId)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            ModelId = string.IsNullOrEmpty(modelId) ? "default" : modelId;
        }

        /// <inheritdoc />
        public string Provider
        {
            get { return "gladia.transcription"; }
        }

        /// <inheritdoc />
        public string ModelId { get; }

        /// <summary>
        /// Serializes the model id and config for a workflow boundary. Maps to <c>[WORKFLOW_SERIALIZE]</c>.
        /// The API key is not part of the serialized config. Pass it to <see cref="Deserialize"/>.
        /// </summary>
        public SerializedModelOptions Serialize()
        {
            var headers = new Dictionary<string, object?>();
            foreach (var pair in _provider.Options.Headers)
            {
                if (!IsSensitiveHeader(pair.Key))
                {
                    headers[pair.Key] = pair.Value;
                }
            }

            var options = _provider.Options as GladiaOptions;
            var config = new Dictionary<string, object?>
            {
                ["provider"] = Provider,
                ["url"] = _provider.Options.BaseUrl,
                ["headers"] = headers,
                ["pollIntervalMs"] = options?.PollIntervalMs,
                ["pollTimeoutMs"] = options?.PollTimeoutMs,
            };
            return ModelOptionsSerialization.SerializeModelOptions(ModelId, config);
        }

        /// <summary>Restores a model from <see cref="Serialize"/>. Maps to <c>[WORKFLOW_DESERIALIZE]</c>.</summary>
        /// <param name="serialized">The serialized model id and config.</param>
        /// <param name="apiKey">API key. Defaults to the <c>GLADIA_API_KEY</c> environment variable.</param>
        /// <param name="handler">HTTP handler. Pass one from tests.</param>
        public static GladiaTranscriptionModel Deserialize(SerializedModelOptions serialized, string? apiKey = null, HttpMessageHandler? handler = null)
        {
            if (serialized is null)
            {
                throw new ArgumentNullException(nameof(serialized));
            }

            var options = new GladiaOptions { ApiKey = apiKey };
            // The provider's fixed endpoint is restored when the config has no url.
            if (ReadString(serialized.Config, "url") is { } baseUrl)
            {
                options.BaseUrl = baseUrl;
            }

            if (serialized.Config.TryGetValue("headers", out var headers))
            {
                foreach (var pair in ReadStringMap(headers))
                {
                    options.Headers[pair.Key] = pair.Value;
                }
            }

            options.PollIntervalMs = ReadInt(serialized.Config, "pollIntervalMs");
            options.PollTimeoutMs = ReadInt(serialized.Config, "pollTimeoutMs");
            return (GladiaTranscriptionModel)GladiaProvider.Create(options, handler).TranscriptionModel(serialized.ModelId);
        }

        /// <inheritdoc />
        public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
        {
            var result = await TranscribeAsync(audio, null, cancellationToken).ConfigureAwait(false);
            return new TranscriptionResult(result.Text);
        }

        /// <summary>Transcribes audio through upload, pre-recorded init, and result polling.</summary>
        public async Task<GladiaTranscriptionResult> TranscribeAsync(AudioInput audio, GladiaTranscriptionRequest? request, CancellationToken cancellationToken)
        {
            if (audio == null)
            {
                throw new ArgumentNullException(nameof(audio));
            }

            var options = _provider.Options as GladiaOptions;
            ValidatePollingSettings(request?.PollIntervalMs, request?.PollTimeoutMs);

            // Options are validated before the audio is uploaded.
            var (args, warnings) = GetArgs(request);
            var started = options?.Clock != null ? options.Clock() : DateTimeOffset.UtcNow;
            var headers = Headers(request, includeKey: true);
            var form = new MultipartFormDataContent();
            var media = new ByteArrayContent(audio.Data);
            if (!string.IsNullOrEmpty(audio.MediaType))
            {
                media.Headers.ContentType = MediaTypeHeaderValue.Parse(audio.MediaType);
            }

            form.Add(media, "audio", "audio." + Extension(audio.MediaType));
            var upload = await ProviderExchange.SendAsync(
                _provider._httpClient,
                HttpMethod.Post,
                ApiKeys.Combine(_provider.Options.BaseUrl, "/v2/upload"),
                form,
                headers,
                cancellationToken).ConfigureAwait(false);
            using (var uploaded = JsonDocument.Parse(string.IsNullOrWhiteSpace(upload.Body) ? "{}" : upload.Body))
            {
                var audioUrl = ReadString(uploaded.RootElement, "audio_url");
                if (string.IsNullOrEmpty(audioUrl))
                {
                    var text = ReadString(uploaded.RootElement, "text") ?? string.Empty;
                    return new GladiaTranscriptionResult(
                        text,
                        Array.Empty<GladiaTranscriptionSegment>(),
                        null,
                        null,
                        uploaded.RootElement.Clone(),
                        warnings,
                        new GladiaTranscriptionResponse(started, ModelId, upload.Headers));
                }

                var body = args;
                if (request?.Body != null)
                {
                    foreach (var pair in request.Body)
                    {
                        body[pair.Key] = pair.Value?.DeepClone();
                    }
                }

                body["audio_url"] = audioUrl;

                var initiated = await ProviderExchange.SendAsync(
                    _provider._httpClient,
                    HttpMethod.Post,
                    ApiKeys.Combine(_provider.Options.BaseUrl, "/v2/pre-recorded"),
                    ProviderExchange.Json(body.ToJsonString()),
                    headers,
                    cancellationToken).ConfigureAwait(false);
                using (var initDocument = JsonDocument.Parse(string.IsNullOrWhiteSpace(initiated.Body) ? "{}" : initiated.Body))
                {
                    var resultUrl = ReadString(initDocument.RootElement, "result_url");
                    if (string.IsNullOrEmpty(resultUrl))
                    {
                        throw new AiSdkException("Gladia did not return a result URL.");
                    }

                    var result = await PollAsync(resultUrl!, headers, request, cancellationToken).ConfigureAwait(false);
                    return Parse(result.Body, result.Headers, started, warnings);
                }
            }
        }

        private (JsonObject Body, List<ModelWarning> Warnings) GetArgs(GladiaTranscriptionRequest? request)
        {
            var warnings = new List<ModelWarning>();
            var body = new JsonObject();

            // `default` is the placeholder used when no model id is passed: omit the field
            // and let the Gladia API fall back to its own default (`solaria-1`).
            if (ModelId != "default")
            {
                body["model"] = ModelId;
            }

            JsonElement? gladia = request?.ProviderOptions != null
                && request.ProviderOptions.TryGetValue(ProviderId, out var provided)
                && provided.ValueKind != JsonValueKind.Null
                    ? provided
                    : null;
            if (gladia is { } options)
            {
                var error = ProviderOptionShape.Validate(options, GladiaTranscriptionModelOptions.GladiaTranscriptionModelOptionsShape(), false);
                if (error != null)
                {
                    throw new InvalidArgumentError("providerOptions", options.GetRawText(), "invalid gladia provider options: " + error);
                }

                AddOptions(body, options, warnings);
            }

            if (ModelId == "solaria-3")
            {
                var languageConfig = body["language_config"] as JsonObject;
                if ((languageConfig?["languages"] as JsonArray)?.Count != 1)
                {
                    warnings.Add(new OtherWarning(
                        "The \"solaria-3\" model supports a single language only. Pass exactly one language in languageConfig.languages; Gladia may reject the request."));
                }

                if (languageConfig?["code_switching"]?.GetValue<bool>() == true)
                {
                    warnings.Add(new OtherWarning(
                        "The \"solaria-3\" model does not support code switching; Gladia may reject the request."));
                }
            }

            return (body, warnings);
        }

        private static void AddOptions(JsonObject body, JsonElement options, List<ModelWarning> warnings)
        {
            foreach (var feature in new[] { "contextPrompt", "moderation", "chapterization", "nameConsistency", "structuredDataExtraction", "structuredDataExtractionConfig", "displayMode" })
            {
                if (Has(options, feature))
                {
                    warnings.Add(new UnsupportedWarning(feature, "No longer supported by the Gladia v2 pre-recorded API; ignored."));
                }
            }

            if (Has(options, "diarizationConfig") && Has(options.GetProperty("diarizationConfig"), "enhanced"))
            {
                warnings.Add(new UnsupportedWarning("diarizationConfig.enhanced", "No longer supported by the Gladia v2 pre-recorded API; ignored."));
            }

            foreach (var setting in new[] { "language", "detectLanguage", "enableCodeSwitching", "codeSwitchingConfig" })
            {
                if (Has(options, setting))
                {
                    warnings.Add(new DeprecatedWarning(
                        setting,
                        Has(options, "languageConfig")
                            ? "Ignored because languageConfig is provided; remove this deprecated option."
                            : "Use languageConfig instead."));
                }
            }

            var resolved = GladiaTranscriptionOptionsResolution.ResolveCustomVocabulary(options, warnings);
            if (resolved.CustomVocabulary is { } customVocabulary)
            {
                body["custom_vocabulary"] = customVocabulary;
            }

            Copy(body, options, "callback", "callback");
            Copy(body, options, "subtitles", "subtitles");
            Copy(body, options, "diarization", "diarization");
            Copy(body, options, "translation", "translation");
            Copy(body, options, "summarization", "summarization");
            Copy(body, options, "namedEntityRecognition", "named_entity_recognition");
            Copy(body, options, "customSpelling", "custom_spelling");
            Copy(body, options, "sentimentAnalysis", "sentiment_analysis");
            Copy(body, options, "audioToLlm", "audio_to_llm");
            Copy(body, options, "piiRedaction", "pii_redaction");
            Copy(body, options, "customMetadata", "custom_metadata");
            Copy(body, options, "sentences", "sentences");
            Copy(body, options, "punctuationEnhanced", "punctuation_enhanced");

            JsonNode? vocabulary = null;
            JsonElement? vocabularyConfig = Has(options, "customVocabularyConfig") ? options.GetProperty("customVocabularyConfig") : null;
            if (vocabularyConfig is { } configured && Has(configured, "vocabulary"))
            {
                vocabulary = JsonNode.Parse(configured.GetProperty("vocabulary").GetRawText());
            }
            else if (resolved.Vocabulary != null)
            {
                vocabulary = resolved.Vocabulary;
            }

            if (vocabulary != null)
            {
                var config = new JsonObject { ["vocabulary"] = vocabulary };
                if (vocabularyConfig is { } source)
                {
                    Copy(config, source, "defaultIntensity", "default_intensity");
                }

                body["custom_vocabulary_config"] = config;
            }

            var languageConfig = GladiaTranscriptionOptionsResolution.ResolveLanguageConfig(options);
            if (languageConfig != null)
            {
                var language = new JsonObject();
                if (languageConfig.Languages != null)
                {
                    language["languages"] = new JsonArray(languageConfig.Languages.Select(item => (JsonNode?)JsonValue.Create(item)).ToArray());
                }

                if (languageConfig.CodeSwitching is { } codeSwitching)
                {
                    language["code_switching"] = codeSwitching;
                }

                body["language_config"] = language;
            }

            Nested(body, options, "callbackConfig", "callback_config", ["url", "url"], ["method", "method"]);
            Nested(
                body,
                options,
                "subtitlesConfig",
                "subtitles_config",
                ["formats", "formats"],
                ["minimumDuration", "minimum_duration"],
                ["maximumDuration", "maximum_duration"],
                ["maximumCharactersPerRow", "maximum_characters_per_row"],
                ["maximumRowsPerCaption", "maximum_rows_per_caption"],
                ["style", "style"]);
            Nested(body, options, "diarizationConfig", "diarization_config", ["numberOfSpeakers", "number_of_speakers"], ["minSpeakers", "min_speakers"], ["maxSpeakers", "max_speakers"]);
            Nested(
                body,
                options,
                "translationConfig",
                "translation_config",
                ["targetLanguages", "target_languages"],
                ["model", "model"],
                ["matchOriginalUtterances", "match_original_utterances"],
                ["lipsync", "lipsync"],
                ["contextAdaptation", "context_adaptation"],
                ["context", "context"],
                ["informal", "informal"]);
            Nested(body, options, "summarizationConfig", "summarization_config", ["type", "type"]);
            Nested(body, options, "customSpellingConfig", "custom_spelling_config", ["spellingDictionary", "spelling_dictionary"]);
            Nested(body, options, "audioToLlmConfig", "audio_to_llm_config", ["prompts", "prompts"], ["model", "model"]);

            if (Has(options, "piiRedactionConfig"))
            {
                if (!(Has(options, "piiRedaction") && options.GetProperty("piiRedaction").GetBoolean()))
                {
                    warnings.Add(new OtherWarning("piiRedactionConfig has no effect unless piiRedaction is true."));
                }

                Nested(body, options, "piiRedactionConfig", "pii_redaction_config", ["entityTypes", "entity_types"], ["processedTextType", "processed_text_type"]);
            }
        }

        private static bool Has(JsonElement element, string name)
        {
            return element.ValueKind == JsonValueKind.Object
                && element.TryGetProperty(name, out var value)
                && value.ValueKind != JsonValueKind.Null;
        }

        private static void Copy(JsonObject target, JsonElement source, string name, string key)
        {
            if (Has(source, name))
            {
                target[key] = JsonNode.Parse(source.GetProperty(name).GetRawText());
            }
        }

        private static void Nested(JsonObject body, JsonElement options, string name, string key, params string[][] fields)
        {
            if (!Has(options, name))
            {
                return;
            }

            var source = options.GetProperty(name);
            var nested = new JsonObject();
            foreach (var field in fields)
            {
                Copy(nested, source, field[0], field[1]);
            }

            body[key] = nested;
        }

        private GladiaTranscriptionResult Parse(string body, IReadOnlyDictionary<string, string> headers, DateTimeOffset started, IReadOnlyList<ModelWarning> warnings)
        {
            using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body))
            {
                var root = document.RootElement;
                var text = string.Empty;
                string? language = null;
                double? duration = null;
                var segments = new List<GladiaTranscriptionSegment>();
                if (root.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.Object)
                {
                    if (result.TryGetProperty("metadata", out var metadata)
                        && metadata.TryGetProperty("audio_duration", out var audioDuration)
                        && audioDuration.TryGetDouble(out var seconds))
                    {
                        duration = seconds;
                    }

                    if (result.TryGetProperty("transcription", out var transcription) && transcription.ValueKind == JsonValueKind.Object)
                    {
                        text = ReadString(transcription, "full_transcript") ?? string.Empty;
                        if (transcription.TryGetProperty("languages", out var languages)
                            && languages.ValueKind == JsonValueKind.Array
                            && languages.GetArrayLength() > 0
                            && languages[0].ValueKind == JsonValueKind.String)
                        {
                            language = languages[0].GetString();
                        }

                        if (transcription.TryGetProperty("utterances", out var utterances) && utterances.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var utterance in utterances.EnumerateArray())
                            {
                                var start = utterance.TryGetProperty("start", out var startValue) && startValue.TryGetDouble(out var startSecond) ? startSecond : 0;
                                var end = utterance.TryGetProperty("end", out var endValue) && endValue.TryGetDouble(out var endSecond) ? endSecond : start;
                                segments.Add(new GladiaTranscriptionSegment(ReadString(utterance, "text") ?? string.Empty, start, end));
                            }
                        }
                    }
                }

                using (var wrapped = JsonDocument.Parse("{\"gladia\":" + root.GetRawText() + "}"))
                {
                    return new GladiaTranscriptionResult(
                        text,
                        segments,
                        language,
                        duration,
                        wrapped.RootElement.Clone(),
                        warnings,
                        new GladiaTranscriptionResponse(started, ModelId, headers));
                }
            }
        }

        private async Task<ProviderExchangeResult> PollAsync(string resultUrl, IReadOnlyDictionary<string, string?> headers, GladiaTranscriptionRequest? request, CancellationToken cancellationToken)
        {
            var options = _provider.Options as GladiaOptions;
            var origin = new Uri(_provider.Options.BaseUrl, UriKind.Absolute);
            var target = new Uri(resultUrl, UriKind.Absolute);
            IReadOnlyDictionary<string, string?> callHeaders = headers;
            if (!string.Equals(origin.Host, target.Host, StringComparison.OrdinalIgnoreCase))
            {
                var stripped = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                foreach (var pair in headers)
                {
                    if (pair.Key.Equals("x-gladia-key", StringComparison.OrdinalIgnoreCase)
                        || pair.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    stripped[pair.Key] = pair.Value;
                }

                callHeaders = stripped;
            }

            var timeoutMs = request?.PollTimeoutMs ?? options?.PollTimeoutMs ?? DefaultPollTimeoutMs;
            var intervalMs = request?.PollIntervalMs ?? options?.PollIntervalMs ?? DefaultPollIntervalMs;
            var started = Now(options);
            while (true)
            {
                // Stop before polling again when the caller cancelled the request.
                cancellationToken.ThrowIfCancellationRequested();

                if ((Now(options) - started).TotalMilliseconds > timeoutMs)
                {
                    throw new AiSdkError(
                        "TranscriptionJobPollingTimedOut",
                        "Transcription job polling timed out after " + timeoutMs.ToString(CultureInfo.InvariantCulture) + " ms");
                }

                var response = await ProviderExchange.SendAsync(
                    _provider._httpClient,
                    HttpMethod.Get,
                    target,
                    null,
                    callHeaders,
                    cancellationToken).ConfigureAwait(false);
                using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body))
                {
                    var status = ReadString(document.RootElement, "status");
                    if (string.Equals(status, "error", StringComparison.Ordinal))
                    {
                        var errorCode = document.RootElement.TryGetProperty("error_code", out var code) && code.ValueKind == JsonValueKind.Number && code.TryGetInt32(out var number)
                            ? number
                            : (int?)null;
                        var message = errorCode is null
                            ? "Transcription job failed"
                            : "Transcription job failed with error code " + errorCode.Value.ToString(CultureInfo.InvariantCulture);
                        throw new AiSdkError(
                            "TranscriptionJobFailed",
                            message,
                            new GladiaTranscriptionJobResultException(
                                message,
                                JsonNode.Parse(document.RootElement.GetRawText()) as JsonObject ?? [],
                                errorCode));
                    }

                    if (string.Equals(status, "done", StringComparison.Ordinal) || string.IsNullOrEmpty(status))
                    {
                        return response;
                    }
                }

                if (options?.PollDelay != null)
                {
                    await options.PollDelay(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await Task.Delay(intervalMs, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private static DateTimeOffset Now(GladiaOptions? options)
        {
            return options?.Clock != null ? options.Clock() : DateTimeOffset.UtcNow;
        }

        private Dictionary<string, string?> Headers(GladiaTranscriptionRequest? request, bool includeKey)
        {
            var headers = ProviderExchange.Merge(_provider.CreateHeaders(), request?.Headers);
            if (!includeKey)
            {
                headers.Remove("x-gladia-key");
            }

            ProviderExchange.AddUserAgent(headers, "gladia");
            return headers;
        }
    }

    private static string? ReadString(IReadOnlyDictionary<string, object?> config, string name)
    {
        return config.TryGetValue(name, out var value)
            ? value switch
            {
                string text => text,
                JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
                _ => null,
            }
            : null;
    }

    private static Dictionary<string, string> ReadStringMap(object? value)
    {
        var map = new Dictionary<string, string>();
        if (value is JsonElement { ValueKind: JsonValueKind.Object } element)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    map[property.Name] = property.Value.GetString()!;
                }
            }
        }
        else if (value is System.Collections.IDictionary dictionary)
        {
            foreach (System.Collections.DictionaryEntry entry in dictionary)
            {
                if (entry.Key is string key && entry.Value is string text)
                {
                    map[key] = text;
                }
            }
        }

        return map;
    }

    private static int? ReadInt(IReadOnlyDictionary<string, object?> config, string name)
    {
        if (!config.TryGetValue(name, out var value) || value is null)
        {
            return null;
        }

        return value is JsonElement { ValueKind: JsonValueKind.Number } element
            ? element.GetInt32()
            : Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    private static bool IsSensitiveHeader(string name)
    {
        foreach (var marker in new[] { "authorization", "key", "token", "secret", "cookie" })
        {
            if (name.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static string? ReadString(JsonElement element, string name)
    {
        if (element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        return null;
    }

    private static string Extension(string? mediaType)
    {
        if (string.IsNullOrEmpty(mediaType))
        {
            return "bin";
        }

        var value = mediaType!;
        if (value.IndexOf("wav", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "wav";
        }

        if (value.IndexOf("mpeg", StringComparison.OrdinalIgnoreCase) >= 0 || value.IndexOf("mp3", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "mp3";
        }

        return "bin";
    }
}

/// <summary>Registers Gladia.</summary>
public static class GladiaServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddGladia(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(GladiaProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new GladiaProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(GladiaProvider.ProviderId), options);
        });
        return services;
    }
}
