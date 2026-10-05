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
using TranscriptSegment = Vercel.AI.Operations.TranscriptSegment;

namespace Vercel.AI.ElevenLabs;

/// <summary>ElevenLabs settings.</summary>
public class ElevenLabsOptions : OpenAICompatibleOptions
{
    /// <summary>Clock used for response timestamps. Defaults to UTC now.</summary>
    public Func<DateTimeOffset>? Clock { get; set; }
}

/// <summary>Parsed ElevenLabs <c>error</c> object.</summary>
public sealed class ElevenLabsErrorData
{
    /// <summary>Creates parsed error data.</summary>
    public ElevenLabsErrorData(string message, int code)
    {
        Message = message ?? string.Empty;
        Code = code;
    }

    /// <summary>Provider message. Nested JSON is left as text.</summary>
    public string Message { get; }

    /// <summary>Provider error code.</summary>
    public int Code { get; }
}

/// <summary>Result of parsing an ElevenLabs error body.</summary>
public sealed class ElevenLabsErrorParseResult
{
    /// <summary>Creates a parse result.</summary>
    public ElevenLabsErrorParseResult(bool success, ElevenLabsErrorData? value)
    {
        Success = success;
        Value = value;
        RawValue = value;
    }

    /// <summary>True when <c>error.message</c> and <c>error.code</c> were present.</summary>
    public bool Success { get; }

    /// <summary>Parsed error.</summary>
    public ElevenLabsErrorData? Value { get; }

    /// <summary>Same payload as <see cref="Value"/>.</summary>
    public ElevenLabsErrorData? RawValue { get; }
}

/// <summary>Parses ElevenLabs error JSON.</summary>
public static class ElevenLabsError
{
    /// <summary>Parses an error body.</summary>
    public static ElevenLabsErrorParseResult Parse(string json)
    {
        if (ProviderExchange.TryParseError(json, out var message, out var code))
        {
            return new ElevenLabsErrorParseResult(true, new ElevenLabsErrorData(message, code));
        }

        return new ElevenLabsErrorParseResult(false, null);
    }
}

/// <summary>One ElevenLabs text-to-speech call.</summary>
public sealed class ElevenLabsSpeechRequest
{
    /// <summary>Default voice used when the caller does not set one.</summary>
    public const string DefaultVoiceId = "21m00Tcm4TlvDq8ikWAM";

    /// <summary>Creates a speech request.</summary>
    public ElevenLabsSpeechRequest(string text)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>Text to speak.</summary>
    public string Text { get; }

    /// <summary>Voice id. Null selects <see cref="DefaultVoiceId"/>.</summary>
    public string? Voice { get; set; }

    /// <summary>Output format, such as <c>mp3</c> or <c>pcm_44100</c>. Sent as the <c>output_format</c> query parameter.</summary>
    public string OutputFormat { get; set; } = "mp3_44100_128";

    /// <summary>Ignored. ElevenLabs does not accept instructions; a warning is returned.</summary>
    public string? Instructions { get; set; }

    /// <summary>Language code, sent as <c>language_code</c>.</summary>
    public string? Language { get; set; }

    /// <summary>Speaking rate, sent in <c>voice_settings</c>.</summary>
    public double? Speed { get; set; }

    /// <summary>Provider options. The <c>elevenlabs</c> object is read.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Headers merged over the provider headers.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>Response metadata for one ElevenLabs call.</summary>
public sealed class ElevenLabsResponse
{
    /// <summary>Creates response metadata.</summary>
    public ElevenLabsResponse(DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string> headers, JsonElement? body)
    {
        Timestamp = timestamp;
        ModelId = modelId ?? string.Empty;
        Headers = headers ?? new Dictionary<string, string>();
        Body = body;
    }

    /// <summary>Timestamp captured when the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>JSON response body. Speech calls return audio, so this is null for them.</summary>
    public JsonElement? Body { get; }
}

/// <summary>Synthesized ElevenLabs audio.</summary>
public sealed class ElevenLabsSpeechResult
{
    /// <summary>Creates a speech result.</summary>
    public ElevenLabsSpeechResult(byte[] audio, string mediaType, IReadOnlyList<ModelWarning> warnings, string requestBody, ElevenLabsResponse response)
    {
        Audio = audio ?? Array.Empty<byte>();
        MediaType = mediaType ?? "audio/mpeg";
        Warnings = warnings ?? Array.Empty<ModelWarning>();
        RequestBody = requestBody ?? string.Empty;
        Response = response;
    }

    /// <summary>Audio bytes.</summary>
    public byte[] Audio { get; }

    /// <summary>Response media type.</summary>
    public string MediaType { get; }

    /// <summary>Warnings for ignored settings.</summary>
    public IReadOnlyList<ModelWarning> Warnings { get; }

    /// <summary>JSON request body.</summary>
    public string RequestBody { get; }

    /// <summary>Response metadata.</summary>
    public ElevenLabsResponse Response { get; }
}

/// <summary>Provider options and headers for one ElevenLabs transcription.</summary>
public sealed class ElevenLabsTranscriptionRequest
{
    /// <summary>Provider options. The <c>elevenlabs</c> object is read.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Headers merged over the provider headers.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>ElevenLabs transcription with word timings and response metadata.</summary>
public sealed class ElevenLabsTranscriptionResult
{
    /// <summary>Creates a transcription result.</summary>
    public ElevenLabsTranscriptionResult(
        string text,
        IReadOnlyList<TranscriptSegment> segments,
        string? language,
        double? durationInSeconds,
        IReadOnlyList<ModelWarning> warnings,
        ElevenLabsResponse response)
    {
        Text = text ?? string.Empty;
        Segments = segments ?? Array.Empty<TranscriptSegment>();
        Language = language;
        DurationInSeconds = durationInSeconds;
        Warnings = warnings ?? Array.Empty<ModelWarning>();
        Response = response;
    }

    /// <summary>Full transcript.</summary>
    public string Text { get; }

    /// <summary>One segment per word or spacing item.</summary>
    public IReadOnlyList<TranscriptSegment> Segments { get; }

    /// <summary>Detected language code.</summary>
    public string? Language { get; }

    /// <summary>End time of the last word, in seconds.</summary>
    public double? DurationInSeconds { get; }

    /// <summary>Warnings for ignored settings.</summary>
    public IReadOnlyList<ModelWarning> Warnings { get; }

    /// <summary>Response metadata.</summary>
    public ElevenLabsResponse Response { get; }
}

/// <summary>ElevenLabs provider.</summary>
public sealed class ElevenLabsProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "elevenlabs";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.elevenlabs.io";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public ElevenLabsProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("ElevenLabs does not provide language models.");
    }

    /// <summary>Creates a provider.</summary>
    public static new ElevenLabsProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new ElevenLabsProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "ELEVENLABS_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.CustomHeader;
        options.ApiKeyHeaderName = "xi-api-key";
        options.UserAgent = ProviderExchange.UserAgent("elevenlabs");
        return options;
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId) => new ElevenLabsSpeechModel(this, modelId);

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId) => new ElevenLabsTranscriptionModel(this, modelId);

    private DateTimeOffset Now()
    {
        var clock = (Options as ElevenLabsOptions)?.Clock;
        return clock != null ? clock() : DateTimeOffset.UtcNow;
    }

    private static JsonElement? ElevenLabsObject(JsonElement? providerOptions)
    {
        return providerOptions is { ValueKind: JsonValueKind.Object } bag && bag.TryGetProperty("elevenlabs", out var value) && value.ValueKind == JsonValueKind.Object
            ? value
            : null;
    }

    private static JsonElement? Child(JsonElement? parent, string name)
    {
        return parent != null && parent.Value.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null ? value : null;
    }

    /// <summary>ElevenLabs text-to-speech model.</summary>
    public sealed class ElevenLabsSpeechModel : ISpeechModel
    {
        private static readonly Dictionary<string, string> OutputFormats = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["mp3"] = "mp3_44100_128",
            ["mp3_32"] = "mp3_44100_32",
            ["mp3_64"] = "mp3_44100_64",
            ["mp3_96"] = "mp3_44100_96",
            ["mp3_128"] = "mp3_44100_128",
            ["mp3_192"] = "mp3_44100_192",
            ["pcm"] = "pcm_44100",
            ["pcm_16000"] = "pcm_16000",
            ["pcm_22050"] = "pcm_22050",
            ["pcm_24000"] = "pcm_24000",
            ["pcm_44100"] = "pcm_44100",
            ["ulaw"] = "ulaw_8000",
        };

        private readonly ElevenLabsProvider _provider;

        /// <summary>Creates a speech model.</summary>
        public ElevenLabsSpeechModel(ElevenLabsProvider provider, string modelId)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        }

        /// <inheritdoc />
        public string Provider => "elevenlabs.speech";

        /// <inheritdoc />
        public string ModelId { get; }

        /// <inheritdoc />
        public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
        {
            options = options ?? throw new ArgumentNullException(nameof(options));
            var result = await GenerateAsync(new ElevenLabsSpeechRequest(options.Text) { Voice = options.Voice }, cancellationToken).ConfigureAwait(false);
            return new SpeechResult(result.Audio, result.MediaType);
        }

        /// <summary>Synthesizes speech and returns the audio with warnings and response metadata.</summary>
        public async Task<ElevenLabsSpeechResult> GenerateAsync(ElevenLabsSpeechRequest request, CancellationToken cancellationToken)
        {
            request = request ?? throw new ArgumentNullException(nameof(request));
            var timestamp = _provider.Now();
            var warnings = new List<ModelWarning>();
            var elevenlabs = ElevenLabsObject(request.ProviderOptions);
            var body = new JsonObject { ["text"] = request.Text, ["model_id"] = ModelId };
            var query = new List<string>();
            if (!string.IsNullOrEmpty(request.OutputFormat))
            {
                query.Add("output_format=" + Uri.EscapeDataString(OutputFormats.TryGetValue(request.OutputFormat, out var mapped) ? mapped : request.OutputFormat));
            }

            if (!string.IsNullOrEmpty(request.Language))
            {
                body["language_code"] = request.Language;
            }

            var voiceSettings = new JsonObject();
            if (request.Speed is { } speed)
            {
                voiceSettings["speed"] = speed;
            }

            var settings = Child(elevenlabs, "voiceSettings");
            CopyNumber(settings, "stability", voiceSettings, "stability");
            CopyNumber(settings, "similarityBoost", voiceSettings, "similarity_boost");
            CopyNumber(settings, "style", voiceSettings, "style");
            if (Child(settings, "useSpeakerBoost") is { } speakerBoost)
            {
                voiceSettings["use_speaker_boost"] = speakerBoost.GetBoolean();
            }

            if (body["language_code"] == null && Child(elevenlabs, "languageCode") is { ValueKind: JsonValueKind.String } languageCode && languageCode.GetString()!.Length > 0)
            {
                body["language_code"] = languageCode.GetString();
            }

            if (Child(elevenlabs, "pronunciationDictionaryLocators") is { ValueKind: JsonValueKind.Array } locators)
            {
                var array = new JsonArray();
                foreach (var locator in locators.EnumerateArray())
                {
                    var item = new JsonObject { ["pronunciation_dictionary_id"] = Child(locator, "pronunciationDictionaryId")?.GetString() };
                    if (Child(locator, "versionId") is { ValueKind: JsonValueKind.String } version && version.GetString()!.Length > 0)
                    {
                        item["version_id"] = version.GetString();
                    }

                    array.Add(item);
                }

                body["pronunciation_dictionary_locators"] = array;
            }

            CopyNumber(elevenlabs, "seed", body, "seed");
            CopyText(elevenlabs, "previousText", body, "previous_text");
            CopyText(elevenlabs, "nextText", body, "next_text");
            CopyRaw(elevenlabs, "previousRequestIds", body, "previous_request_ids");
            CopyRaw(elevenlabs, "nextRequestIds", body, "next_request_ids");
            CopyText(elevenlabs, "applyTextNormalization", body, "apply_text_normalization");
            CopyRaw(elevenlabs, "applyLanguageTextNormalization", body, "apply_language_text_normalization");
            if (Child(elevenlabs, "enableLogging") is { } enableLogging)
            {
                query.Add("enable_logging=" + (enableLogging.GetBoolean() ? "true" : "false"));
            }

            if (voiceSettings.Count > 0)
            {
                body["voice_settings"] = voiceSettings;
            }

            if (!string.IsNullOrEmpty(request.Instructions))
            {
                warnings.Add(new UnsupportedWarning("instructions", "ElevenLabs speech models do not support instructions. Instructions parameter was ignored."));
            }

            var voice = string.IsNullOrEmpty(request.Voice) ? ElevenLabsSpeechRequest.DefaultVoiceId : request.Voice!;
            var path = "/v1/text-to-speech/" + Uri.EscapeDataString(voice) + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
            var json = body.ToJsonString();
            var response = await ProviderExchange.SendAsync(
                _provider._httpClient,
                HttpMethod.Post,
                ApiKeys.Combine(_provider.Options.BaseUrl, path),
                ProviderExchange.Json(json),
                ProviderExchange.Merge(_provider.CreateHeaders(), request.Headers),
                cancellationToken).ConfigureAwait(false);
            var mediaType = "audio/mpeg";
            if (response.Headers.TryGetValue("Content-Type", out var contentType) && contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            {
                var separator = contentType.IndexOf(';');
                mediaType = (separator >= 0 ? contentType.Substring(0, separator) : contentType).Trim();
            }

            return new ElevenLabsSpeechResult(response.Bytes, mediaType, warnings, json, new ElevenLabsResponse(timestamp, ModelId, response.Headers, null));
        }

        private static void CopyNumber(JsonElement? source, string name, JsonObject target, string targetName)
        {
            if (Child(source, name) is { } value)
            {
                target[targetName] = JsonNode.Parse(value.GetRawText());
            }
        }

        private static void CopyText(JsonElement? source, string name, JsonObject target, string targetName)
        {
            if (Child(source, name) is { ValueKind: JsonValueKind.String } value && value.GetString()!.Length > 0)
            {
                target[targetName] = value.GetString();
            }
        }

        private static void CopyRaw(JsonElement? source, string name, JsonObject target, string targetName)
        {
            if (Child(source, name) is { } value)
            {
                target[targetName] = JsonNode.Parse(value.GetRawText());
            }
        }
    }

    /// <summary>ElevenLabs batch speech-to-text model.</summary>
    public sealed class ElevenLabsTranscriptionModel : ITranscriptionModel
    {
        private readonly ElevenLabsProvider _provider;

        /// <summary>Creates a transcription model.</summary>
        public ElevenLabsTranscriptionModel(ElevenLabsProvider provider, string modelId)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        }

        /// <inheritdoc />
        public string Provider => "elevenlabs.transcription";

        /// <inheritdoc />
        public string ModelId { get; }

        /// <inheritdoc />
        public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
        {
            var result = await TranscribeAsync(audio, null, cancellationToken).ConfigureAwait(false);
            return new TranscriptionResult(result.Text);
        }

        /// <summary>Uploads audio to <c>/v1/speech-to-text</c> and reads the transcript.</summary>
        public async Task<ElevenLabsTranscriptionResult> TranscribeAsync(AudioInput audio, ElevenLabsTranscriptionRequest? request, CancellationToken cancellationToken)
        {
            audio = audio ?? throw new ArgumentNullException(nameof(audio));
            if (ModelId == "scribe_v2_realtime")
            {
                throw new Operations.UnsupportedFunctionalityException("non-streaming transcription with " + ModelId, "'non-streaming transcription with " + ModelId + "' functionality not supported.");
            }

            var timestamp = _provider.Now();
            var warnings = new List<ModelWarning>();
            var elevenlabs = ElevenLabsObject(request?.ProviderOptions);
            if (Child(elevenlabs, "streaming") != null)
            {
                warnings.Add(new UnsupportedWarning("providerOptions.elevenlabs.streaming", "ElevenLabs batch transcription does not support streaming options."));
            }

            var form = new MultipartFormDataContent();
            form.Add(new StringContent(ModelId), "model_id");
            var file = new ByteArrayContent(audio.Data);
            if (!string.IsNullOrEmpty(audio.MediaType))
            {
                file.Headers.ContentType = MediaTypeHeaderValue.Parse(audio.MediaType);
            }

            form.Add(file, "file", "audio." + ProviderValues.MediaTypeToExtension(audio.MediaType));
            form.Add(new StringContent("true"), "diarize");
            if (elevenlabs != null)
            {
                // Option defaults apply once the elevenlabs object is present, as in the upstream schema.
                form.Add(new StringContent(FormValue(Child(elevenlabs, "diarize")) ?? "false"), "diarize");
                AddField(form, "language_code", FormValue(Child(elevenlabs, "languageCode")));
                AddField(form, "tag_audio_events", FormValue(Child(elevenlabs, "tagAudioEvents")) ?? "true");
                AddField(form, "num_speakers", FormValue(Child(elevenlabs, "numSpeakers")));
                AddField(form, "timestamps_granularity", FormValue(Child(elevenlabs, "timestampsGranularity")) ?? "word");
                AddField(form, "file_format", FormValue(Child(elevenlabs, "fileFormat")) ?? "other");
            }

            var response = await ProviderExchange.SendAsync(
                _provider._httpClient,
                HttpMethod.Post,
                ApiKeys.Combine(_provider.Options.BaseUrl, "/v1/speech-to-text"),
                form,
                ProviderExchange.Merge(_provider.CreateHeaders(), request?.Headers),
                cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(response.Body);
            var root = document.RootElement;
            var segments = new List<TranscriptSegment>();
            double? duration = null;
            if (root.TryGetProperty("words", out var words) && words.ValueKind == JsonValueKind.Array)
            {
                foreach (var word in words.EnumerateArray())
                {
                    var end = Child(word, "end")?.GetDouble();
                    segments.Add(new TranscriptSegment(Child(word, "text")?.GetString() ?? string.Empty, Child(word, "start")?.GetDouble() ?? 0, end ?? 0));
                    duration = end;
                }
            }

            return new ElevenLabsTranscriptionResult(
                Child(root, "text")?.GetString() ?? string.Empty,
                segments,
                Child(root, "language_code")?.GetString(),
                duration,
                warnings,
                new ElevenLabsResponse(timestamp, ModelId, response.Headers, root.Clone()));
        }

        private static void AddField(MultipartFormDataContent form, string name, string? value)
        {
            if (value != null)
            {
                form.Add(new StringContent(value), name);
            }
        }

        private static string? FormValue(JsonElement? value)
        {
            if (value == null)
            {
                return null;
            }

            return value.Value.ValueKind switch
            {
                JsonValueKind.String => value.Value.GetString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Number => value.Value.GetDouble().ToString(CultureInfo.InvariantCulture),
                _ => value.Value.GetRawText(),
            };
        }
    }
}

/// <summary>Registers ElevenLabs.</summary>
public static class ElevenLabsServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddElevenLabs(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(ElevenLabsProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new ElevenLabsProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(ElevenLabsProvider.ProviderId), options);
        });
        return services;
    }
}
