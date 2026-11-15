// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>Transcription settings beyond the audio payload.</summary>
public sealed class OpenAITranscriptionCall
{
    /// <summary>Creates a transcription call.</summary>
    public OpenAITranscriptionCall(AudioInput audio)
    {
        Audio = audio ?? throw new ArgumentNullException(nameof(audio));
    }

    /// <summary>Audio to transcribe.</summary>
    public AudioInput Audio { get; }

    /// <summary>Provider options. The <c>openai</c> object is read.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Extra HTTP headers.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }
}

/// <summary>Response metadata recorded for a transcription.</summary>
public sealed class OpenAITranscriptionResponse
{
    internal OpenAITranscriptionResponse(DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string> headers, string body)
    {
        Timestamp = timestamp;
        ModelId = modelId;
        Headers = headers;
        Body = body;
    }

    /// <summary>Timestamp captured when the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Model id used for the call.</summary>
    public string ModelId { get; }

    /// <summary>HTTP response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Raw response JSON.</summary>
    public string Body { get; }
}

/// <summary>Transcript with timed segments and response metadata.</summary>
public sealed class OpenAITranscriptionResult
{
    internal OpenAITranscriptionResult(
        string text,
        IReadOnlyList<TranscriptSegment> segments,
        string? language,
        double? durationInSeconds,
        JsonElement? providerMetadata,
        IReadOnlyList<OpenAICallWarning> warnings,
        OpenAITranscriptionResponse response)
    {
        Text = text;
        Segments = segments;
        Language = language;
        DurationInSeconds = durationInSeconds;
        ProviderMetadata = providerMetadata;
        Warnings = warnings;
        Response = response;
    }

    /// <summary>Transcript text.</summary>
    public string Text { get; }

    /// <summary>Segments, or words when the response has no segments.</summary>
    public IReadOnlyList<TranscriptSegment> Segments { get; }

    /// <summary>ISO-639-1 code, when OpenAI reports a two-letter code, a known language name, or a single detected language.</summary>
    public string? Language { get; }

    /// <summary>Audio duration in seconds.</summary>
    public double? DurationInSeconds { get; }

    /// <summary>Speaker segments under <c>openai.segments</c> for diarized responses and detected languages under <c>openai.languages</c>.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Warnings produced while preparing the request. OpenAI transcription reports none today.</summary>
    public IReadOnlyList<OpenAICallWarning> Warnings { get; }

    /// <summary>Response metadata.</summary>
    public OpenAITranscriptionResponse Response { get; }
}

/// <summary>OpenAI audio transcription model.</summary>
public sealed class OpenAITranscriptionModel : ITranscriptionModel, ITranscriptionCaller
{
    // OpenAI reports the detected language by name. Only these names map to ISO-639-1 codes.
    private static readonly Dictionary<string, string> LanguageCodes = new(StringComparer.Ordinal)
    {
        ["afrikaans"] = "af",
        ["arabic"] = "ar",
        ["armenian"] = "hy",
        ["azerbaijani"] = "az",
        ["belarusian"] = "be",
        ["bosnian"] = "bs",
        ["bulgarian"] = "bg",
        ["catalan"] = "ca",
        ["chinese"] = "zh",
        ["croatian"] = "hr",
        ["czech"] = "cs",
        ["danish"] = "da",
        ["dutch"] = "nl",
        ["english"] = "en",
        ["estonian"] = "et",
        ["finnish"] = "fi",
        ["french"] = "fr",
        ["galician"] = "gl",
        ["german"] = "de",
        ["greek"] = "el",
        ["hebrew"] = "he",
        ["hindi"] = "hi",
        ["hungarian"] = "hu",
        ["icelandic"] = "is",
        ["indonesian"] = "id",
        ["italian"] = "it",
        ["japanese"] = "ja",
        ["kannada"] = "kn",
        ["kazakh"] = "kk",
        ["korean"] = "ko",
        ["latvian"] = "lv",
        ["lithuanian"] = "lt",
        ["macedonian"] = "mk",
        ["malay"] = "ms",
        ["marathi"] = "mr",
        ["maori"] = "mi",
        ["nepali"] = "ne",
        ["norwegian"] = "no",
        ["persian"] = "fa",
        ["polish"] = "pl",
        ["portuguese"] = "pt",
        ["romanian"] = "ro",
        ["russian"] = "ru",
        ["serbian"] = "sr",
        ["slovak"] = "sk",
        ["slovenian"] = "sl",
        ["spanish"] = "es",
        ["swahili"] = "sw",
        ["swedish"] = "sv",
        ["tagalog"] = "tl",
        ["tamil"] = "ta",
        ["thai"] = "th",
        ["turkish"] = "tr",
        ["ukrainian"] = "uk",
        ["urdu"] = "ur",
        ["vietnamese"] = "vi",
        ["welsh"] = "cy",
    };

    private static readonly Regex BearerToken = new Regex(@"^bearer\s+(.+)\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline, TimeSpan.FromSeconds(1));

    private readonly ProviderHttp _http;
    private readonly string _providerName;
    private readonly Func<Uri> _url;
    private readonly Func<Uri> _realtimeUrl;
    private readonly Func<IReadOnlyDictionary<string, string?>?, Dictionary<string, string?>> _headers;
    private readonly WebSocketFactory? _webSocket;
    private readonly Func<DateTimeOffset>? _clock;

    /// <summary>Creates a transcription model. <paramref name="clock"/> sets the response timestamp and defaults to UTC now.</summary>
    public OpenAITranscriptionModel(OpenAIProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
        : this(provider, modelId, "audio/transcriptions", clock)
    {
    }

    /// <summary>Creates a transcription or translation model for <paramref name="path"/>.</summary>
    public OpenAITranscriptionModel(OpenAIProvider provider, string modelId, string path, Func<DateTimeOffset>? clock = null)
    {
        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _http = provider.Http;
        _providerName = provider.Name;
        _url = () => ApiKeys.Combine(provider.Options.BaseUrl, path);
        _realtimeUrl = () => ApiKeys.Combine(provider.Options.BaseUrl, "realtime?intent=transcription");
        _headers = provider.CreateOpenAIHeaders;
        _webSocket = provider.WebSocket;
        _clock = clock;
    }

    /// <summary>
    /// Creates an Azure OpenAI transcription model for a deployment. Requests go to the deployment's
    /// <c>audio/transcriptions</c> route and the provider id is <c>azure.transcription</c>. <paramref name="webSocket"/> is the WebSocket for realtime streaming. It defaults to <c>ClientWebSocket</c>.
    /// </summary>
    public OpenAITranscriptionModel(Azure.AzureOpenAIProvider provider, string deploymentId, Func<DateTimeOffset>? clock = null, WebSocketFactory? webSocket = null)
    {
        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        ModelId = deploymentId ?? throw new ArgumentNullException(nameof(deploymentId));
        _http = provider.Http;
        _providerName = provider.Name;
        _url = () => provider.AzureUri("audio/transcriptions", deploymentId);
        _realtimeUrl = () =>
        {
            // The deployment route keeps `api-version`; `intent` goes first, as in the upstream URL builder.
            var azure = provider.AzureUri("realtime", deploymentId);
            var query = azure.Query.TrimStart('?');
            return new UriBuilder(azure) { Query = query.Length == 0 ? "intent=transcription" : "intent=transcription&" + query }.Uri;
        };
        _headers = provider.CreateHeaders;
        _webSocket = webSocket;
        _clock = clock;
    }

    /// <summary>
    /// Creates a transcription model from explicit configuration, as upstream's <c>OpenAITranscriptionModelConfig</c> does.
    /// <paramref name="headers"/> are the configuration headers. Per-call headers are merged over them.
    /// </summary>
    internal OpenAITranscriptionModel(
        string modelId,
        string provider,
        ProviderHttp http,
        Func<Uri> url,
        Func<Uri> realtimeUrl,
        Func<Dictionary<string, string?>> headers,
        WebSocketFactory? webSocket = null,
        Func<DateTimeOffset>? clock = null)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _providerName = provider;
        _http = http;
        _url = url;
        _realtimeUrl = realtimeUrl;
        _headers = callHeaders =>
        {
            var combined = new Dictionary<string, string?>(headers(), StringComparer.OrdinalIgnoreCase);
            if (callHeaders != null)
            {
                foreach (var pair in callHeaders)
                {
                    combined[pair.Key] = pair.Value;
                }
            }

            return combined;
        };
        _webSocket = webSocket;
        _clock = clock;
    }

    /// <inheritdoc />
    public string Provider => _providerName.EndsWith(".transcription", StringComparison.Ordinal) ? _providerName : _providerName + ".transcription";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public bool CanStream => true;

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        var result = await TranscribeAsync(new OpenAITranscriptionCall(audio), cancellationToken).ConfigureAwait(false);
        return new TranscriptionResult(result.Text);
    }

    /// <summary>Transcribes audio and returns segments, language, duration, and response metadata.</summary>
    public async Task<OpenAITranscriptionResult> TranscribeAsync(OpenAITranscriptionCall call, CancellationToken cancellationToken)
    {
        if (OpenAIModelCatalog.GetOpenAITranscriptionModelEntry(ModelId).Capabilities.Transcription == false)
        {
            throw new UnsupportedFunctionalityException(
                "non-streaming transcription with " + ModelId,
                "'non-streaming transcription with " + ModelId + "' functionality not supported.");
        }

        var timestamp = OpenAIClock.Now(_clock);
        var response = await _http.SendBinaryAsync(
            HttpMethod.Post,
            _url(),
            Build(call),
            _headers(call.Headers),
            cancellationToken).ConfigureAwait(false);
        var body = Encoding.UTF8.GetString(response.Body);
        using var document = OpenAIJson.ParseResponse(body);
        var root = document.RootElement;
        var segments = new List<TranscriptSegment>();
        var speakers = new JsonArray();
        if (root.TryGetProperty("segments", out var segmentItems) && segmentItems.ValueKind == JsonValueKind.Array)
        {
            foreach (var segment in segmentItems.EnumerateArray())
            {
                var text = OpenAIJson.String(segment, "text") ?? string.Empty;
                var start = segment.GetProperty("start").GetDouble();
                var end = segment.GetProperty("end").GetDouble();
                segments.Add(new TranscriptSegment(text, start, end));
                if (segment.TryGetProperty("speaker", out var speaker))
                {
                    speakers.Add(new JsonObject { ["text"] = text, ["startSecond"] = start, ["endSecond"] = end, ["speaker"] = JsonNode.Parse(speaker.GetRawText()) });
                }
            }
        }
        else if (root.TryGetProperty("words", out var words) && words.ValueKind == JsonValueKind.Array)
        {
            foreach (var word in words.EnumerateArray())
            {
                segments.Add(new TranscriptSegment(OpenAIJson.String(word, "word") ?? string.Empty, word.GetProperty("start").GetDouble(), word.GetProperty("end").GetDouble()));
            }
        }

        var languages = DetectedLanguages(root);
        var languageName = OpenAIJson.String(root, "language");
        var language = IsLanguageCode(languageName)
            ? languageName
            : languageName != null && LanguageCodes.TryGetValue(languageName, out var code) ? code : null;
        if (language == null && languages?.Count == 1)
        {
            var detected = languages[0]!["code"]!.GetValue<string>();
            language = IsLanguageCode(detected) ? detected : null;
        }

        return new OpenAITranscriptionResult(
            OpenAIJson.String(root, "text") ?? string.Empty,
            segments,
            language,
            root.TryGetProperty("duration", out var duration) && duration.ValueKind == JsonValueKind.Number ? duration.GetDouble() : null,
            speakers.Count > 0 || languages != null ? ProviderMetadata(speakers, languages) : null,
            Array.Empty<OpenAICallWarning>(),
            new OpenAITranscriptionResponse(timestamp, ModelId, response.Headers, body));
    }

    /// <summary>Builds the multipart body so tests can inspect field names.</summary>
    public MultipartFormDataContent Build(OpenAITranscriptionCall call)
    {
        var content = new MultipartFormDataContent();
        var mediaType = string.IsNullOrEmpty(call.Audio.MediaType) ? "audio/mpeg" : call.Audio.MediaType;
        var file = new ByteArrayContent(call.Audio.Data);
        file.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        content.Add(file, "file", "audio." + OpenAIJson.Extension(mediaType));
        content.Add(new StringContent(ModelId), "model");
        var openai = OpenAIJson.OpenAIObject(call.ProviderOptions);
        var diarize = ModelId == "gpt-4o-transcribe-diarize";
        if (ModelId == "whisper-1")
        {
            content.Add(new StringContent("verbose_json"), "response_format");
        }

        var chunking = Chunking(openai, diarize);
        if (openai != null && ModelId != "whisper-1")
        {
            var format = OpenAIJson.String(openai, "responseFormat");
            if (format == null)
            {
                format = diarize
                    ? "diarized_json"
                    : ModelId == "gpt-4o-transcribe" || ModelId == "gpt-4o-mini-transcribe" ? "json" : "verbose_json";
            }

            content.Add(new StringContent(format), "response_format");
        }
        else if (openai == null && diarize)
        {
            content.Add(new StringContent("diarized_json"), "response_format");
        }

        if (openai != null)
        {
            AddAll(content, "include[]", OpenAIJson.Child(openai, "include"));
            Add(content, "language", OpenAIJson.String(openai, "language"));
            AddAll(content, "languages[]", OpenAIJson.Child(openai, "languages"));
            AddAll(content, "keywords[]", OpenAIJson.Child(openai, "keywords"));
            Add(content, "prompt", OpenAIJson.String(openai, "prompt"));
            var temperature = OpenAIJson.Double(openai, "temperature") ?? 0d;
            content.Add(new StringContent(temperature.ToString(CultureInfo.InvariantCulture)), "temperature");
            foreach (var granularity in Granularities(openai.Value))
            {
                content.Add(new StringContent(granularity), "timestamp_granularities[]");
            }
        }

        if (chunking != null)
        {
            content.Add(new StringContent(chunking), "chunking_strategy");
        }

        return content;
    }

    /// <inheritdoc />
    public async Task<TranscriptionModelResult> DoGenerateAsync(TranscriptionModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var result = await TranscribeAsync(
            new OpenAITranscriptionCall(new AudioInput(call.Audio, call.MediaType, null))
            {
                ProviderOptions = call.ProviderOptions,
                Headers = call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value),
            },
            cancellationToken).ConfigureAwait(false);
        return new TranscriptionModelResult(
            result.Text,
            result.Segments,
            result.Language,
            result.DurationInSeconds,
            result.Warnings.Select(warning => new OperationWarning(warning.Type, warning.Message, warning.Feature, warning.Details)).ToList(),
            result.ProviderMetadata,
            new ProviderResponse(result.Response.Headers, OpenAIJson.Parse(result.Response.Body), timestamp: result.Response.Timestamp.UtcDateTime, modelId: result.Response.ModelId));
    }

    /// <summary>Starts a realtime streaming transcription over the OpenAI Realtime WebSocket. Maps to <c>doStream</c>.</summary>
    public Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult<TranscriptionStreamStart?>(DoStream(call ?? throw new ArgumentNullException(nameof(call))));
        }
        catch (Exception error)
        {
            return Task.FromException<TranscriptionStreamStart?>(error);
        }
    }

    private static JsonObject BuildRealtimeSession(string modelId, OpenAITranscriptionModelEntry entry, AudioFormat format, string? language, string? delay, JsonElement? include)
    {
        var audioFormat = new JsonObject { ["type"] = format.Type };
        if (format.Rate != null)
        {
            audioFormat["rate"] = format.Rate.Value;
        }

        var transcription = new JsonObject { ["model"] = modelId };
        if (language != null)
        {
            if (entry.LanguageField == "languages")
            {
                transcription["languages"] = new JsonArray(language);
            }
            else
            {
                transcription["language"] = language;
            }
        }

        if (delay != null)
        {
            transcription["delay"] = delay;
        }

        var session = new JsonObject
        {
            ["type"] = "transcription",
            ["audio"] = new JsonObject
            {
                ["input"] = new JsonObject
                {
                    ["format"] = audioFormat,
                    ["transcription"] = transcription,
                    ["turn_detection"] = null,
                },
            },
        };
        if (include is { ValueKind: JsonValueKind.Array })
        {
            session["include"] = JsonNode.Parse(include.Value.GetRawText());
        }

        return new JsonObject { ["type"] = "session.update", ["session"] = session };
    }

    // The bearer token rides the `openai-insecure-api-key` subprotocol (a native WebSocket cannot send headers) and the
    // Authorization header is stripped: OpenAI rejects handshakes that send both auth channels.
    private static (string[] Protocols, Dictionary<string, string?> Headers) GetOpenAIRealtimeConnection(IReadOnlyDictionary<string, string?> headers)
    {
        string? authorization = null;
        foreach (var pair in headers)
        {
            if (string.Equals(pair.Key, "authorization", StringComparison.OrdinalIgnoreCase) && pair.Value != null)
            {
                authorization = pair.Value;
            }
        }

        // The HTTP auth scheme is case-insensitive.
        string? token = null;
        if (authorization != null)
        {
            var match = BearerToken.Match(authorization);
            token = match.Success ? match.Groups[1].Value : null;
        }

        if (token == null)
        {
            return (new[] { "realtime" }, headers.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase));
        }

        return (
            new[] { "realtime", "openai-insecure-api-key." + token },
            headers.Where(pair => !string.Equals(pair.Key, "authorization", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase));
    }

    private TranscriptionStreamStart DoStream(TranscriptionStreamCall call)
    {
        var entry = OpenAIModelCatalog.GetOpenAITranscriptionModelEntry(ModelId);
        if (entry.Capabilities.Streaming != true)
        {
            throw new UnsupportedFunctionalityException(
                "streaming transcription with " + ModelId,
                "'streaming transcription with " + ModelId + "' functionality not supported.");
        }

        var timestamp = OpenAIClock.Now(_clock).UtcDateTime;
        var openai = OpenAIJson.Child(call.ProviderOptions, "openai");
        if (openai is { } provided)
        {
            var error = ProviderOptionShape.Validate(provided, OpenAITranscriptionModelOptions.OpenAITranscriptionModelOptionsShape(), false);
            if (error != null)
            {
                throw new InvalidArgumentException("providerOptions", provided.GetRawText(), "invalid openai provider options: " + error);
            }
        }

        var warnings = new List<OperationWarning>();

        // Options that only apply to the REST transcription endpoint.
        foreach (var option in new[] { "include", "languages", "keywords", "prompt", "temperature", "timestampGranularities" })
        {
            if (OpenAIJson.Child(openai, option) != null)
            {
                warnings.Add(OperationWarning.Unsupported("providerOptions.openai." + option, "OpenAI streaming transcription does not support " + option + "."));
            }
        }

        // gpt-transcribe transcribes each committed turn, so it has no delay setting.
        var supportsDelay = entry.Capabilities.StreamingDelay == true;
        var streaming = OpenAIJson.Child(openai, "streaming");
        var delay = OpenAIJson.String(streaming, "delay");
        if (!supportsDelay && delay != null)
        {
            warnings.Add(OperationWarning.Unsupported("providerOptions.openai.streaming.delay", "OpenAI streaming transcription with " + ModelId + " does not support delay."));
        }

        var headers = _headers(call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value));
        var language = OpenAIJson.String(openai, "language");
        var sessionUpdate = BuildRealtimeSession(ModelId, entry, call.InputAudioFormat, language, supportsDelay ? delay : null, OpenAIJson.Child(streaming, "include"));
        var connection = GetOpenAIRealtimeConnection(headers);
        var session = new RealtimeTranscriptionSession(call, sessionUpdate, language, warnings);
        session.Start(WebSocketUtils.ToWebSocketUrl(_realtimeUrl()), connection.Protocols, connection.Headers, _webSocket);
        return new TranscriptionStreamStart(
            new ModelPartStream(session.Queue, session.Cancel),
            new ProviderResponse(timestamp: timestamp, modelId: ModelId),
            new ProviderRequest(sessionUpdate.ToJsonString()));
    }

    // Two lowercase ASCII letters, as in /^[a-z]{2}$/ upstream.
    private static bool IsLanguageCode(string? language)
    {
        return language is { Length: 2 } && language[0] is >= 'a' and <= 'z' && language[1] is >= 'a' and <= 'z';
    }

    private static JsonArray? DetectedLanguages(JsonElement root)
    {
        if (!root.TryGetProperty("languages", out var items) || items.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var languages = new JsonArray();
        foreach (var item in items.EnumerateArray())
        {
            languages.Add(new JsonObject { ["code"] = OpenAIJson.String(item, "code") ?? throw new AiSdkException("Invalid transcription response: language code is missing.") });
        }

        return languages;
    }

    private static JsonElement ProviderMetadata(JsonArray segments, JsonArray? languages)
    {
        var openai = new JsonObject();
        if (segments.Count > 0)
        {
            openai["segments"] = segments;
        }

        if (languages != null)
        {
            openai["languages"] = languages;
        }

        var metadata = new JsonObject { ["openai"] = openai };
        using var document = JsonDocument.Parse(metadata.ToJsonString());
        return document.RootElement.Clone();
    }

    private static void Add(MultipartFormDataContent content, string name, string? value)
    {
        if (value != null)
        {
            content.Add(new StringContent(value), name);
        }
    }

    private static void AddAll(MultipartFormDataContent content, string name, JsonElement? values)
    {
        if (values is not { ValueKind: JsonValueKind.Array })
        {
            return;
        }

        foreach (var item in values.Value.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                content.Add(new StringContent(item.GetString() ?? string.Empty), name);
            }
        }
    }

    private static string? Chunking(JsonElement? openai, bool diarize)
    {
        var chunking = OpenAIJson.Child(openai, "chunkingStrategy");
        if (chunking == null)
        {
            return diarize ? "auto" : null;
        }

        if (chunking.Value.ValueKind == JsonValueKind.String)
        {
            return chunking.Value.GetString();
        }

        if (chunking.Value.ValueKind != JsonValueKind.Object)
        {
            return diarize ? "auto" : null;
        }

        var json = new System.Text.Json.Nodes.JsonObject();
        if (chunking.Value.TryGetProperty("type", out var type) && type.ValueKind == JsonValueKind.String)
        {
            json["type"] = type.GetString();
        }

        if (chunking.Value.TryGetProperty("threshold", out var threshold) && threshold.ValueKind == JsonValueKind.Number)
        {
            json["threshold"] = threshold.GetDouble();
        }

        if (chunking.Value.TryGetProperty("prefixPaddingMs", out var prefix) && prefix.TryGetInt32(out var prefixMs))
        {
            json["prefix_padding_ms"] = prefixMs;
        }

        if (chunking.Value.TryGetProperty("silenceDurationMs", out var silence) && silence.TryGetInt32(out var silenceMs))
        {
            json["silence_duration_ms"] = silenceMs;
        }

        return json.ToJsonString();
    }

    private static IReadOnlyList<string> Granularities(JsonElement openai)
    {
        var granularities = OpenAIJson.Child(openai, "timestampGranularities");
        if (granularities == null || granularities.Value.ValueKind != JsonValueKind.Array)
        {
            return new[] { "segment" };
        }

        var values = new List<string>();
        foreach (var item in granularities.Value.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                values.Add(item.GetString() ?? string.Empty);
            }
        }

        return values;
    }

    // Maps to `createOpenAIRealtimeTranscriptionStream`.
    private sealed class RealtimeTranscriptionSession
    {
        private readonly TranscriptionStreamCall _call;
        private readonly JsonObject _sessionUpdate;
        private readonly string? _language;
        private readonly IReadOnlyList<OperationWarning> _warnings;
        private readonly object _gate = new object();
        private WebSocketConnection? _connection;
        private AudioChunkReader? _reader;
        private bool _finished;

        public RealtimeTranscriptionSession(TranscriptionStreamCall call, JsonObject sessionUpdate, string? language, IReadOnlyList<OperationWarning> warnings)
        {
            _call = call;
            _sessionUpdate = sessionUpdate;
            _language = language;
            _warnings = warnings;
        }

        public PartQueue<ModelStreamPart> Queue { get; } = new PartQueue<ModelStreamPart>();

        public void Start(Uri url, IReadOnlyList<string> protocols, IReadOnlyDictionary<string, string?> headers, WebSocketFactory? webSocket)
        {
            var connection = ConnectToWebSocket.Connect(new ConnectToWebSocketOptions
            {
                Url = url,
                Protocols = protocols,
                Headers = headers,
                WebSocket = webSocket,
                AbortSignal = _call.CancellationToken,
                OnAbort = FinishWithError,
                OnProcessingError = FinishWithError,
                OnOpen = OnOpen,
                OnMessageText = OnMessageTextAsync,
                OnSocketError = () => FinishWithError(new AiSdkException("OpenAI realtime transcription error")),
                OnClose = (_, _) =>
                {
                    if (!TryFinish())
                    {
                        return;
                    }

                    Cleanup(null);
                    Queue.Complete(null);
                },
            });
            bool finished;
            lock (_gate)
            {
                _connection = connection;
                finished = _finished;
            }

            // A callback that fired before the connection was stored could not close it.
            if (finished)
            {
                connection.Close(null);
            }
        }

        public void Cancel()
        {
            if (!TryFinish())
            {
                return;
            }

            Cleanup(null);
            Queue.Complete(null);
        }

        private bool IsFinished
        {
            get
            {
                lock (_gate)
                {
                    return _finished;
                }
            }
        }

        private bool TryFinish()
        {
            lock (_gate)
            {
                if (_finished)
                {
                    return false;
                }

                _finished = true;
                return true;
            }
        }

        private void Cleanup(int? closeCode)
        {
            AudioChunkReader? reader;
            WebSocketConnection? connection;
            lock (_gate)
            {
                reader = _reader;
                connection = _connection;
            }

            try
            {
                if (reader != null)
                {
                    reader.Cancel();
                }
                else
                {
                    // Pre-open failure or abort: cancel the caller's audio stream so an upstream producer piping into it does not hang.
                    _call.Audio.Cancel();
                }
            }
            catch (Exception)
            {
                // A stream owned by the caller stays with its owner.
            }

            connection?.Close(closeCode);
        }

        private void FinishWithError(Exception error)
        {
            if (!TryFinish())
            {
                return;
            }

            Cleanup(null);
            Queue.Complete(error);
        }

        private void Finish(string text, string? id)
        {
            if (!TryFinish())
            {
                return;
            }

            if (id != null)
            {
                Queue.Enqueue(new ModelStreamPart { Type = "transcript-final", Id = id, Text = text });
            }

            Queue.Enqueue(new ModelStreamPart { Type = "finish", Text = text, Segments = Array.Empty<TranscriptSegment>(), Language = _language });
            Queue.Complete(null);
            Cleanup(1000);
        }

        private void OnOpen(IWebSocketLike socket)
        {
            Queue.Enqueue(new ModelStreamPart { Type = "stream-start", Warnings = _warnings });
            socket.Send(_sessionUpdate.ToJsonString());
            _ = SendAudioAsync(socket).ContinueWith(task => FinishWithError(task.Exception!.GetBaseException()), TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task SendAudioAsync(IWebSocketLike socket)
        {
            var reader = _call.Audio.GetReader();
            lock (_gate)
            {
                _reader = reader;
            }

            try
            {
                while (true)
                {
                    var bytes = await reader.ReadAsync(_call.CancellationToken).ConfigureAwait(false);
                    if (bytes == null || IsFinished)
                    {
                        break;
                    }

                    socket.Send(new JsonObject { ["type"] = "input_audio_buffer.append", ["audio"] = Convert.ToBase64String(bytes) }.ToJsonString());

                    // Backpressure: pause reads while the socket buffer is full.
                    await WebSocketUtils.WaitForWebSocketBufferDrainAsync(socket, cancellationToken: _call.CancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                reader.ReleaseLock();

                // Unlocked again: cleanup must cancel the audio stream, not the reader.
                lock (_gate)
                {
                    _reader = null;
                }
            }

            if (!IsFinished)
            {
                socket.Send(new JsonObject { ["type"] = "input_audio_buffer.commit" }.ToJsonString());
            }
        }

        private Task OnMessageTextAsync(string text)
        {
            JsonElement root;
            try
            {
                using var document = JsonDocument.Parse(text);
                root = document.RootElement.Clone();
            }
            catch (JsonException)
            {
                return Task.CompletedTask;
            }

            if (root.ValueKind != JsonValueKind.Object)
            {
                return Task.CompletedTask;
            }

            if (_call.IncludeRawChunks == true)
            {
                Queue.Enqueue(new ModelStreamPart { Type = "raw", Raw = root });
            }

            switch (OpenAIJson.String(root, "type"))
            {
                case "conversation.item.input_audio_transcription.delta":
                    Queue.Enqueue(new ModelStreamPart { Type = "transcript-delta", Id = OpenAIJson.String(root, "item_id"), Delta = OpenAIJson.String(root, "delta") ?? string.Empty });
                    break;

                case "conversation.item.input_audio_transcription.completed":
                    Finish(OpenAIJson.String(root, "transcript") ?? string.Empty, OpenAIJson.String(root, "item_id"));
                    break;

                case "error":
                    FinishWithError(new AiSdkException(OpenAIJson.String(OpenAIJson.Child(root, "error"), "message") ?? "OpenAI realtime error"));
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
