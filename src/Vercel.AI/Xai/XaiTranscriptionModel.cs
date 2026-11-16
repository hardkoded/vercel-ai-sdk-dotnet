// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Xai;

/// <summary>xAI speech-to-text model, over <c>/stt</c> and its WebSocket. Maps to <c>XaiTranscriptionModel</c>.</summary>
public sealed class XaiTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
{
    private static readonly (string Type, string Encoding)[] InputAudioFormatEncodings =
    {
        ("audio/pcm", "pcm"),
        ("audio/pcmu", "mulaw"),
        ("audio/pcma", "alaw"),
        ("audio/opus", "opus"),
    };

    private readonly XaiProvider _provider;
    private readonly Func<DateTimeOffset>? _clock;

    /// <summary>
    /// Creates a transcription model. <paramref name="modelId"/> is empty for xAI's default model.
    /// <paramref name="clock"/> sets the response timestamp (upstream <c>_internal.currentDate</c>) and defaults to UTC now.
    /// </summary>
    public XaiTranscriptionModel(
        XaiProvider provider,
        string modelId = "",
        Func<DateTimeOffset>? clock = null
    )
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock;
    }

    /// <inheritdoc />
    public string Provider => "xai.transcription";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public bool CanStream => true;

    /// <summary>
    /// Serializes the model id and config for a workflow boundary. Maps to <c>[WORKFLOW_SERIALIZE]</c>. The API key and
    /// headers are not part of the serialized config. Pass the key to <see cref="Deserialize"/>.
    /// </summary>
    public SerializedModelOptions Serialize()
    {
        var config = new Dictionary<string, object?>
        {
            ["provider"] = Provider,
            ["baseURL"] = _provider.Options.BaseUrl,
        };
        return ModelOptionsSerialization.SerializeModelOptions(ModelId, config);
    }

    /// <summary>Restores a model from <see cref="Serialize"/>. Maps to <c>[WORKFLOW_DESERIALIZE]</c>.</summary>
    /// <param name="serialized">The serialized model id and config.</param>
    /// <param name="apiKey">API key. Defaults to the <c>XAI_API_KEY</c> environment variable.</param>
    /// <param name="handler">HTTP handler. Pass one from tests.</param>
    public static XaiTranscriptionModel Deserialize(
        SerializedModelOptions serialized,
        string? apiKey = null,
        HttpMessageHandler? handler = null
    )
    {
        serialized = serialized ?? throw new ArgumentNullException(nameof(serialized));
        var options = new XaiOptions { ApiKey = apiKey };
        if (
            serialized.Config.TryGetValue("baseURL", out var baseUrl)
            && baseUrl is string text
            && text.Length > 0
        )
        {
            options.BaseUrl = text;
        }
        else if (
            serialized.Config.TryGetValue("baseURL", out var element)
            && element is JsonElement { ValueKind: JsonValueKind.String } json
        )
        {
            options.BaseUrl = json.GetString()!;
        }

        return new XaiTranscriptionModel(XaiProvider.Create(options, handler), serialized.ModelId);
    }

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(
        AudioInput audio,
        CancellationToken cancellationToken
    )
    {
        audio = audio ?? throw new ArgumentNullException(nameof(audio));
        var result = await DoGenerateAsync(
                new TranscriptionModelCall(
                    audio.Data,
                    string.IsNullOrEmpty(audio.MediaType) ? "audio/wav" : audio.MediaType,
                    OperationJson.Parse("{}"),
                    new Dictionary<string, string>(),
                    cancellationToken
                ),
                cancellationToken
            )
            .ConfigureAwait(false);
        return new TranscriptionResult(result.Text ?? string.Empty);
    }

    /// <inheritdoc />
    public async Task<TranscriptionModelResult> DoGenerateAsync(
        TranscriptionModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var timestamp = _clock?.Invoke() ?? DateTimeOffset.UtcNow;
        var warnings = new List<OperationWarning>();
        var xaiOptions = XaiTranscriptionModelOptions.Parse(call.ProviderOptions);

        using var form = new MultipartFormDataContent();
        var fields = GetXaiSttParams(ModelId, xaiOptions);
        fields.Add(
            (
                "audio_format",
                xaiOptions?.AudioFormat is { } audioFormat ? new[] { audioFormat } : null
            )
        );
        fields.Add(("format", xaiOptions?.Format is { } format ? new[] { Bool(format) } : null));
        AppendXaiSttParams(fields, (name, value) => form.Add(new StringContent(value), name));

        // xAI requires `file` to be the final multipart field.
        var mediaType = string.IsNullOrEmpty(call.MediaType) ? "audio/wav" : call.MediaType;
        var file = new ByteArrayContent(call.Audio);
        file.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
        form.Add(file, "file", "audio." + ProviderValues.MediaTypeToExtension(mediaType));

        var response = await _provider
            .Http.SendBinaryAsync(
                HttpMethod.Post,
                ApiKeys.Combine(_provider.Options.BaseUrl, "stt"),
                form,
                _provider.CreateHeaders(
                    call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value)
                ),
                cancellationToken
            )
            .ConfigureAwait(false);

        var body = Encoding.UTF8.GetString(response.Body);
        JsonElement root;
        try
        {
            using var document = JsonDocument.Parse(body);
            root = document.RootElement.Clone();
        }
        catch (JsonException)
        {
            throw new InvalidResponseDataException(
                body,
                "Invalid xAI transcription response: the body is not valid JSON."
            );
        }

        if (
            root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("text", out var textElement)
            || textElement.ValueKind != JsonValueKind.String
        )
        {
            throw new InvalidResponseDataException(
                body,
                "Invalid xAI transcription response: expected text."
            );
        }

        var text = textElement.GetString()!;
        var responseWords = ReadWords(root, body);
        var language =
            root.TryGetProperty("language", out var languageElement)
            && languageElement.ValueKind == JsonValueKind.String
                ? languageElement.GetString()
                : null;
        double? duration =
            root.TryGetProperty("duration", out var durationElement)
            && durationElement.ValueKind == JsonValueKind.Number
                ? durationElement.GetDouble()
                : null;

        if (call.TimestampGranularities?.Contains("segment") == true)
        {
            warnings.Add(
                new OperationWarning(
                    "unsupported",
                    feature: "timestampGranularities",
                    details: "xAI transcription returns word timings and does not provide separate caption segments."
                )
            );
        }

        var words = responseWords
            ?.Select(word => new TranscriptSegment(word.Text, word.Start, word.End))
            .ToList();
        if (
            call.TimestampGranularities?.Contains("word") == true
            && (words is null || words.Count == 0)
            && text.Trim() != string.Empty
        )
        {
            warnings.Add(
                new OperationWarning(
                    "other",
                    message: "The xAI transcription response omitted the requested word timings."
                )
            );
        }

        JsonElement? providerMetadata = null;
        if (responseWords?.Any(word => word.Speaker is not null) == true)
        {
            providerMetadata = JsonSerializer.SerializeToElement(
                new JsonObject
                {
                    ["xai"] = new JsonObject
                    {
                        ["words"] = new JsonArray(responseWords.Select(word => word.Raw).ToArray()),
                    },
                }
            );
        }

        return new TranscriptionModelResult(
            text,
            words ?? new List<TranscriptSegment>(),
            string.IsNullOrEmpty(language) ? null : language,
            duration,
            warnings,
            providerMetadata,
            new ProviderResponse(
                response.Headers,
                root,
                timestamp: timestamp.UtcDateTime,
                modelId: ModelId
            ),
            words is { Count: > 0 } ? words : null
        );
    }

    /// <inheritdoc />
    public Task<TranscriptionStreamStart?> DoStreamAsync(
        TranscriptionStreamCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var timestamp = _clock?.Invoke() ?? DateTimeOffset.UtcNow;
        var warnings = new List<OperationWarning>();
        var xaiOptions = XaiTranscriptionModelOptions.Parse(call.ProviderOptions);

        if (xaiOptions?.Multichannel == true && xaiOptions.Channels is null)
        {
            throw new InvalidArgumentException(
                "providerOptions",
                null,
                "providerOptions.xai.channels is required when providerOptions.xai.multichannel is true"
            );
        }

        if (xaiOptions?.Format is not null)
        {
            warnings.Add(
                new OperationWarning(
                    "unsupported",
                    feature: "providerOptions.xai.format",
                    details: "xAI streaming transcription does not support format."
                )
            );
        }

        if (
            xaiOptions?.AudioFormat is null
            && !InputAudioFormatEncodings.Any(entry => entry.Type == call.InputAudioFormat.Type)
        )
        {
            warnings.Add(
                new OperationWarning(
                    "other",
                    message: "Unrecognized inputAudioFormat.type \""
                        + call.InputAudioFormat.Type
                        + "\"; falling back to raw PCM encoding. Use "
                        + string.Join(", ", InputAudioFormatEncodings.Select(entry => entry.Type))
                        + ", or set providerOptions.xai.audioFormat explicitly."
                )
            );
        }

        var url = BuildXaiStreamingTranscriptionUrl(call.InputAudioFormat, xaiOptions);
        var headers = _provider.CreateHeaders(call.Headers);
        var session = new Session(
            call,
            warnings,
            xaiOptions?.Language,
            xaiOptions?.Multichannel == true ? xaiOptions.Channels!.Value : 1
        );
        session.Start(url, headers, (_provider.Options as XaiOptions)?.WebSocket);
        return Task.FromResult<TranscriptionStreamStart?>(
            new TranscriptionStreamStart(
                new ModelPartStream(session.Queue, session.Cancel),
                new ProviderResponse(timestamp: timestamp.UtcDateTime, modelId: ModelId),
                new ProviderRequest(url.ToString())
            )
        );
    }

    private static string Bool(bool value) => value ? "true" : "false";

    private static string Number(double value) => value.ToString("R", CultureInfo.InvariantCulture);

    // Maps to `getXaiSttParams`. The model comes from `getXaiTranscriptionModelEntry(modelId).requestConfig.transcription.model`.
    private List<(string Name, string[]? Values)> GetXaiSttParams(
        string modelId,
        XaiTranscriptionModelOptions? options
    )
    {
        return new List<(string, string[]?)>
        {
            (
                "model",
                XaiModelCatalog.GetXaiTranscriptionModelEntry(modelId).Model is { } model
                    ? new[] { model }
                    : null
            ),
            (
                "sample_rate",
                options?.SampleRate is { } rate
                    ? new[] { rate.ToString(CultureInfo.InvariantCulture) }
                    : null
            ),
            ("language", options?.Language is { } language ? new[] { language } : null),
            (
                "multichannel",
                options?.Multichannel is { } multichannel ? new[] { Bool(multichannel) } : null
            ),
            (
                "channels",
                options?.Channels is { } channels
                    ? new[] { channels.ToString(CultureInfo.InvariantCulture) }
                    : null
            ),
            ("diarize", options?.Diarize is { } diarize ? new[] { Bool(diarize) } : null),
            (
                "filler_words",
                options?.FillerWords is { } fillerWords ? new[] { Bool(fillerWords) } : null
            ),
            ("vad_threshold", options?.VadThreshold is { } vad ? new[] { Number(vad) } : null),
            ("keyterm", options?.Keyterm?.ToArray()),
        };
    }

    // Maps to `appendXaiSttParams`.
    private static void AppendXaiSttParams(
        List<(string Name, string[]? Values)> parameters,
        Action<string, string> append
    )
    {
        foreach (var (name, values) in parameters)
        {
            if (values is null)
            {
                continue;
            }

            foreach (var value in values)
            {
                append(name, value);
            }
        }
    }

    // Maps to `buildXaiStreamingTranscriptionUrl`.
    private Uri BuildXaiStreamingTranscriptionUrl(
        AudioFormat inputAudioFormat,
        XaiTranscriptionModelOptions? options
    )
    {
        var url = WebSocketUtils.ToWebSocketUrl(_provider.Options.BaseUrl.TrimEnd('/') + "/stt");
        var parameters = GetXaiSttParams(ModelId, options);
        var sampleRate = parameters.FindIndex(entry => entry.Name == "sample_rate");
        if (parameters[sampleRate].Values is null && inputAudioFormat.Rate is { } rate)
        {
            parameters[sampleRate] = ("sample_rate", new[] { Number(rate) });
        }

        var encoding = options?.AudioFormat
            ?? InputAudioFormatEncodings
                .Where(entry => entry.Type == inputAudioFormat.Type)
                .Select(entry => entry.Encoding)
                .FirstOrDefault()
            ?? "pcm";
        parameters.Add(("encoding", new[] { encoding }));
        parameters.Add(
            (
                "interim_results",
                options?.Streaming?.InterimResults is { } interim ? new[] { Bool(interim) } : null
            )
        );
        parameters.Add(
            (
                "endpointing",
                options?.Streaming?.Endpointing is { } endpointing
                    ? new[] { endpointing.ToString(CultureInfo.InvariantCulture) }
                    : null
            )
        );
        parameters.Add(
            (
                "smart_turn",
                options?.Streaming?.SmartTurn is { } smartTurn ? new[] { Number(smartTurn) } : null
            )
        );
        parameters.Add(
            (
                "smart_turn_timeout",
                options?.Streaming?.SmartTurnTimeout is { } timeout
                    ? new[] { timeout.ToString(CultureInfo.InvariantCulture) }
                    : null
            )
        );

        var query = new List<string>();
        AppendXaiSttParams(parameters, (name, value) =>
            query.Add(FormEncode(name) + "=" + FormEncode(value))
        );
        return new UriBuilder(url) { Query = string.Join("&", query) }.Uri;
    }

    // Matches `URLSearchParams` serialization (application/x-www-form-urlencoded).
    private static string FormEncode(string value)
    {
        var builder = new StringBuilder();
        foreach (var b in Encoding.UTF8.GetBytes(value))
        {
            var c = (char)b;
            if (b < 0x80 && (char.IsLetterOrDigit(c) || c is '*' or '-' or '.' or '_'))
            {
                builder.Append(c);
            }
            else if (c == ' ')
            {
                builder.Append('+');
            }
            else
            {
                builder.Append('%').Append(b.ToString("X2", CultureInfo.InvariantCulture));
            }
        }

        return builder.ToString();
    }

    private sealed class XaiWord
    {
        public XaiWord(string text, double start, double end, JsonNode? speaker, JsonObject raw)
        {
            Text = text;
            Start = start;
            End = end;
            Speaker = speaker;
            Raw = raw;
        }

        public string Text { get; }

        public double Start { get; }

        public double End { get; }

        public JsonNode? Speaker { get; }

        public JsonObject Raw { get; }
    }

    // Maps to `xaiTranscriptionResponseSchema` words: text, start, end, and an optional speaker. Other keys are dropped.
    private static List<XaiWord>? ReadWords(JsonElement root, string body)
    {
        if (
            !root.TryGetProperty("words", out var wordItems)
            || wordItems.ValueKind == JsonValueKind.Null
        )
        {
            return null;
        }

        if (wordItems.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidResponseDataException(
                body,
                "Invalid xAI transcription response: words must be an array."
            );
        }

        var words = new List<XaiWord>();
        foreach (var item in wordItems.EnumerateArray())
        {
            if (
                item.ValueKind != JsonValueKind.Object
                || !item.TryGetProperty("text", out var text)
                || text.ValueKind != JsonValueKind.String
                || !item.TryGetProperty("start", out var start)
                || start.ValueKind != JsonValueKind.Number
                || !item.TryGetProperty("end", out var end)
                || end.ValueKind != JsonValueKind.Number
            )
            {
                throw new InvalidResponseDataException(
                    body,
                    "Invalid xAI transcription response: a word is malformed."
                );
            }

            var raw = new JsonObject
            {
                ["text"] = JsonNode.Parse(text.GetRawText()),
                ["start"] = JsonNode.Parse(start.GetRawText()),
                ["end"] = JsonNode.Parse(end.GetRawText()),
            };
            JsonNode? speaker = null;
            if (item.TryGetProperty("speaker", out var speakerElement))
            {
                if (speakerElement.ValueKind is not (JsonValueKind.Number or JsonValueKind.Null))
                {
                    throw new InvalidResponseDataException(
                        body,
                        "Invalid xAI transcription response: a word speaker must be a number."
                    );
                }

                speaker =
                    speakerElement.ValueKind == JsonValueKind.Null
                        ? null
                        : JsonNode.Parse(speakerElement.GetRawText());
                raw["speaker"] =
                    speakerElement.ValueKind == JsonValueKind.Null
                        ? null
                        : JsonNode.Parse(speakerElement.GetRawText());
            }

            words.Add(
                new XaiWord(text.GetString()!, start.GetDouble(), end.GetDouble(), speaker, raw)
            );
        }

        return words;
    }

    private static string? ChannelId(int? channelIndex) =>
        channelIndex is null
            ? null
            : "channel-" + channelIndex.Value.ToString(CultureInfo.InvariantCulture);

    // Maps to `createXaiStreamingTranscriptionStream`.
    private sealed class Session
    {
        private readonly TranscriptionStreamCall _call;
        private readonly IReadOnlyList<OperationWarning> _warnings;
        private readonly string? _language;
        private readonly int _expectedDoneCount;
        private readonly object _gate = new object();
        private readonly SortedDictionary<int, string> _doneTexts =
            new SortedDictionary<int, string>();

        // Per-channel finalized utterances and latest revisable text, used to reconstruct the finish text (xAI's
        // `transcript.done` has empty text).
        private readonly Dictionary<int, List<string>> _finalizedTexts =
            new Dictionary<int, List<string>>();
        private readonly Dictionary<int, string> _pendingTexts = new Dictionary<int, string>();
        private WebSocketConnection? _connection;
        private AudioChunkReader? _reader;
        private bool _finished;
        private double? _doneDuration;

        public Session(
            TranscriptionStreamCall call,
            IReadOnlyList<OperationWarning> warnings,
            string? language,
            int expectedDoneCount
        )
        {
            _call = call;
            _warnings = warnings;
            _language = language;
            _expectedDoneCount = expectedDoneCount;
        }

        public PartQueue<ModelStreamPart> Queue { get; } = new PartQueue<ModelStreamPart>();

        public void Start(
            Uri url,
            IReadOnlyDictionary<string, string?> headers,
            WebSocketFactory? webSocket
        )
        {
            _connection = ConnectToWebSocket.Connect(
                new ConnectToWebSocketOptions
                {
                    Url = url,
                    Headers = headers,
                    WebSocket = webSocket,
                    AbortSignal = _call.CancellationToken,
                    AbortReason = _call.AbortReason,
                    OnAbort = FinishWithError,
                    OnProcessingError = FinishWithError,
                    OnMessageText = OnMessageTextAsync,
                    OnSocketError = () =>
                        FinishWithError(
                            new AiSdkException(
                                "xAI streaming transcription error."
                                    + (
                                        webSocket is null
                                            ? " Note: ClientWebSocket may not be able to send the Authorization header required by xAI on every runtime."
                                                + " Pass a header-capable WebSocket implementation via XaiOptions.WebSocket."
                                            : string.Empty
                                    )
                            )
                        ),
                    OnClose = (_, _) =>
                    {
                        if (!TryFinish())
                        {
                            return;
                        }

                        Cleanup(null);
                        Queue.Complete(null);
                    },
                }
            );
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
            lock (_gate)
            {
                reader = _reader;
            }

            try
            {
                if (reader is not null)
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
                // A model-owned stream stays with its owner.
            }

            _connection?.Close(closeCode);
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

        private void MaybeFinish()
        {
            string text;
            double? duration;
            lock (_gate)
            {
                if (_finished || _doneTexts.Count < _expectedDoneCount)
                {
                    return;
                }

                _finished = true;
                text = string.Join("\n", _doneTexts.Values);
                duration = _doneDuration;
            }

            Queue.Enqueue(
                new ModelStreamPart
                {
                    Type = "finish",
                    Text = text,
                    Segments = Array.Empty<TranscriptSegment>(),
                    Language = _language,
                    DurationInSeconds = duration,
                }
            );
            Queue.Complete(null);
            Cleanup(1000);
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
                    var bytes = await reader
                        .ReadAsync(_call.CancellationToken)
                        .ConfigureAwait(false);
                    if (bytes is null || IsFinished)
                    {
                        break;
                    }

                    socket.Send(bytes);

                    // Backpressure: pause reads while the socket buffer is full.
                    await WebSocketUtils
                        .WaitForWebSocketBufferDrainAsync(
                            socket,
                            cancellationToken: _call.CancellationToken
                        )
                        .ConfigureAwait(false);
                }
            }
            finally
            {
                reader.ReleaseLock();
                lock (_gate)
                {
                    // Unlocked again: cleanup must cancel the audio stream, not the reader.
                    _reader = null;
                }
            }

            if (!IsFinished)
            {
                socket.Send(new JsonObject { ["type"] = "audio.done" }.ToJsonString());
            }
        }

        private Task OnMessageTextAsync(string text)
        {
            JsonElement raw;
            try
            {
                using var document = JsonDocument.Parse(text);
                raw = document.RootElement.Clone();
            }
            catch (JsonException)
            {
                return Task.CompletedTask;
            }

            if (raw.ValueKind != JsonValueKind.Object)
            {
                return Task.CompletedTask;
            }

            if (_call.IncludeRawChunks == true)
            {
                Queue.Enqueue(new ModelStreamPart { Type = "raw", Raw = raw });
            }

            switch (ReadString(raw, "type"))
            {
                case "transcript.created":
                    Queue.Enqueue(
                        new ModelStreamPart { Type = "stream-start", Warnings = _warnings }
                    );
                    var socket = _connection?.Socket;
                    if (socket is null)
                    {
                        FinishWithError(new AiSdkException("WebSocket is not connected."));
                        break;
                    }

                    _ = SendAudioAsync(socket)
                        .ContinueWith(
                            task => FinishWithError(task.Exception!.GetBaseException()),
                            TaskContinuationOptions.OnlyOnFaulted
                        );
                    break;

                case "transcript.partial":
                    OnPartial(raw);
                    break;

                case "transcript.done":
                    var doneChannel = ReadInt(raw, "channel_index") ?? 0;
                    lock (_gate)
                    {
                        // `transcript.done` text is empty; fall back to the accumulated utterances plus any trailing unfinalized text.
                        var accumulated = new List<string>();
                        if (_finalizedTexts.TryGetValue(doneChannel, out var finalized))
                        {
                            accumulated.AddRange(finalized);
                        }

                        if (
                            _pendingTexts.TryGetValue(doneChannel, out var pending)
                            && pending.Length > 0
                        )
                        {
                            accumulated.Add(pending);
                        }

                        var doneText = ReadString(raw, "text");
                        _doneTexts[doneChannel] = string.IsNullOrEmpty(doneText)
                            ? string.Join(" ", accumulated)
                            : doneText!;
                        _doneDuration = ReadDouble(raw, "duration") ?? _doneDuration;
                    }

                    MaybeFinish();
                    break;

                case "error":
                    // xAI STT errors are terminal: surface the server message instead of letting the socket close mask it.
                    FinishWithError(
                        new AiSdkException(ReadString(raw, "message") ?? "xAI STT error")
                    );
                    break;
            }

            return Task.CompletedTask;
        }

        private void OnPartial(JsonElement raw)
        {
            var channel = ReadInt(raw, "channel_index");
            var channelIndex = channel ?? 0;
            var text = ReadString(raw, "text");
            var start = ReadDouble(raw, "start");
            var duration = ReadDouble(raw, "duration");

            // Only `speech_final` completes an utterance; `is_final` fragments are re-sent or revised later, so they stay partials.
            if (ReadBool(raw, "is_final") && ReadBool(raw, "speech_final"))
            {
                lock (_gate)
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        if (!_finalizedTexts.TryGetValue(channelIndex, out var finalized))
                        {
                            finalized = new List<string>();
                            _finalizedTexts[channelIndex] = finalized;
                        }

                        finalized.Add(text!);
                    }

                    _pendingTexts.Remove(channelIndex);
                }

                Queue.Enqueue(
                    new ModelStreamPart
                    {
                        Type = "transcript-final",
                        Id = ChannelId(channel),
                        Text = text ?? string.Empty,
                        StartSecond = start,
                        EndSecond =
                            start is not null && duration is not null ? start + duration : null,
                        ChannelIndex = channel,
                    }
                );
            }
            else
            {
                lock (_gate)
                {
                    _pendingTexts[channelIndex] = text ?? string.Empty;
                }

                Queue.Enqueue(
                    new ModelStreamPart
                    {
                        Type = "transcript-partial",
                        Id = ChannelId(channel),
                        Text = text ?? string.Empty,
                        StartSecond = start,
                        DurationInSeconds = duration,
                        ChannelIndex = channel,
                    }
                );
            }
        }

        private static string? ReadString(JsonElement element, string name)
        {
            return
                element.TryGetProperty(name, out var value)
                && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        private static double? ReadDouble(JsonElement element, string name)
        {
            return
                element.TryGetProperty(name, out var value)
                && value.ValueKind == JsonValueKind.Number
                ? value.GetDouble()
                : null;
        }

        private static int? ReadInt(JsonElement element, string name)
        {
            return
                element.TryGetProperty(name, out var value)
                && value.ValueKind == JsonValueKind.Number
                && value.TryGetInt32(out var number)
                ? number
                : null;
        }

        private static bool ReadBool(JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var value)
                && value.ValueKind == JsonValueKind.True;
        }
    }
}
