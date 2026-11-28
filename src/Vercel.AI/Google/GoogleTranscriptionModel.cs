// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>Gemini transcription. Unary models post to the Interactions API.</summary>
public sealed class GoogleTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
{
    /// <summary>After the input audio ended, finish when no terminal signal arrives within this window. Maps to <c>defaultFinishGraceMs</c>.</summary>
    private const int DefaultFinishGraceMs = 3000;

    /// <summary>Speech that server VAD ended within this distance of the end of the sent audio ends at the end of the audio. Maps to <c>audioEndToleranceMs</c>.</summary>
    private const double AudioEndToleranceMs = 250;

    private readonly GoogleProvider _provider;

    /// <summary>Creates a transcription model.</summary>
    public GoogleTranscriptionModel(GoogleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string Provider
    {
        get { return _provider.ModelProvider; }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>True when the model id is a Live API transcription model.</summary>
    public static bool IsLive(string modelId)
    {
        return modelId != null && modelId.IndexOf("-live", StringComparison.Ordinal) >= 0;
    }

    /// <inheritdoc />
    public string SpecificationVersion
    {
        get { return "v4"; }
    }

    /// <inheritdoc />
    public bool CanStream
    {
        get { return true; }
    }

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(
        AudioInput audio,
        CancellationToken cancellationToken
    )
    {
        var call = new TranscriptionModelCall(
            audio.Data,
            audio.MediaType,
            default,
            new Dictionary<string, string>(),
            cancellationToken
        );
        var result = await DoGenerateAsync(call, cancellationToken).ConfigureAwait(false);
        return new TranscriptionResult(result.Text ?? string.Empty, null);
    }

    /// <summary>
    /// Transcribes audio through the Interactions API. <c>google</c> provider options become the
    /// <c>transcription_config</c>, and <c>word_info</c> annotations become word segments.
    /// </summary>
    public async Task<TranscriptionModelResult> DoGenerateAsync(
        TranscriptionModelCall call,
        CancellationToken cancellationToken
    )
    {
        if (IsLive(ModelId))
        {
            throw new ArgumentException(
                "Model '"
                    + ModelId
                    + "' only supports streaming transcription. Use a unary model such as 'gemini-3.5-transcribe'.",
                nameof(ModelId)
            );
        }

        var timestamp = _provider.Clock();
        var body = BuildRequest(
            ModelId,
            new AudioInput(call.Audio, call.MediaType, null),
            TranscriptionConfig(call.ProviderOptions)
        );
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        foreach (var header in call.Headers)
        {
            headers[header.Key] = header.Value;
        }

        var response = await _provider
            .Http.SendJsonStringAsync(
                HttpMethod.Post,
                ApiKeys.Combine(_provider.Options.BaseUrl, "interactions"),
                GoogleJson.Write(body),
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        var text = string.Empty;
        var segments = new List<TranscriptSegment>();
        if (root.TryGetProperty("steps", out var steps) && steps.ValueKind == JsonValueKind.Array)
        {
            foreach (var step in steps.EnumerateArray())
            {
                if (
                    !step.TryGetProperty("content", out var content)
                    || content.ValueKind != JsonValueKind.Array
                )
                {
                    continue;
                }

                foreach (var part in content.EnumerateArray())
                {
                    if (
                        GoogleJson.String(part, "type") != "text"
                        || GoogleJson.String(part, "text") is not { } partText
                    )
                    {
                        continue;
                    }

                    text += partText;
                    if (
                        !part.TryGetProperty("annotations", out var annotations)
                        || annotations.ValueKind != JsonValueKind.Array
                    )
                    {
                        continue;
                    }

                    foreach (var annotation in annotations.EnumerateArray())
                    {
                        var word = GoogleJson.String(annotation, "text");
                        var start = GoogleVertexSpeechTranscriptionModel.ParseDuration(
                            GoogleJson.String(annotation, "start_offset")
                        );
                        var end = GoogleVertexSpeechTranscriptionModel.ParseDuration(
                            GoogleJson.String(annotation, "end_offset")
                        );
                        if (
                            GoogleJson.String(annotation, "type") == "word_info"
                            && word != null
                            && start != null
                            && end != null
                        )
                        {
                            segments.Add(new TranscriptSegment(word, start.Value, end.Value));
                        }
                    }
                }
            }
        }

        JsonElement? metadata = null;
        if (GoogleJson.TryObject(root, "usage", out var usage))
        {
            metadata = JsonSerializer.SerializeToElement(
                new JsonObject
                {
                    ["google"] = new JsonObject { ["usage"] = JsonNode.Parse(usage.GetRawText()) },
                }
            );
        }

        return new TranscriptionModelResult(
            text,
            segments,
            providerMetadata: metadata,
            response: new ProviderResponse(
                response.Headers,
                root.Clone(),
                timestamp: timestamp.UtcDateTime,
                modelId: ModelId
            )
        );
    }

    /// <summary>Streams transcription over the Gemini Live API WebSocket. Only <c>*-live</c> models support it.</summary>
    public async Task<TranscriptionStreamStart?> DoStreamAsync(
        TranscriptionStreamCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        if (!IsLive(ModelId))
        {
            throw new InvalidArgumentException(
                "modelId",
                ModelId,
                "Model '"
                    + ModelId
                    + "' does not support streaming transcription. Use a live model such as 'gemini-3.5-transcribe-live'."
            );
        }

        var timestamp = _provider.Clock();
        var warnings = new List<OperationWarning>();

        ValidateLiveInputAudioFormat(call.InputAudioFormat);

        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        foreach (var header in call.Headers)
        {
            headers[header.Key] = header.Value;
        }

        string? apiKey = null;
        foreach (var header in headers)
        {
            if (
                string.Equals(header.Key, "x-goog-api-key", StringComparison.OrdinalIgnoreCase)
                && header.Value is not null
            )
            {
                apiKey = header.Value;
            }
        }

        if (apiKey is null)
        {
            throw new AiSdkException(
                "Google Generative AI API key is required for streaming transcription."
            );
        }

        var webSocketHeaders = headers
            .Where(pair =>
                !string.Equals(pair.Key, "x-goog-api-key", StringComparison.OrdinalIgnoreCase)
            )
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        var options = call.ProviderOptions;
        var manualActivityDetection =
            GoogleJson.TryObject(options, "google", out var google)
            && GoogleJson.TryObject(google, "streaming", out var streaming)
            && GoogleJson.TryObject(streaming, "automaticActivityDetection", out var detection)
            && detection.TryGetProperty("disabled", out var disabled)
            && disabled.ValueKind == JsonValueKind.True;

        // Google's GA announcement shows the setup with generationConfig responseModalities TEXT, but sending it
        // suppresses the final inputTranscription segments on the current endpoint. It is omitted on purpose.
        var setup = new JsonObject
        {
            ["model"] = GoogleModelPath.Get(ModelId),
            ["inputAudioTranscription"] = BuildAudioTranscriptionConfig(options)
                ?? new JsonObject(),
        };
        if (manualActivityDetection)
        {
            setup["realtimeInputConfig"] = new JsonObject
            {
                ["automaticActivityDetection"] = new JsonObject { ["disabled"] = true },
            };
        }

        var session = new Session(
            call,
            warnings,
            setup,
            call.InputAudioFormat.Rate ?? 16000,
            manualActivityDetection,
            _provider.Options.FinishGraceMs ?? DefaultFinishGraceMs
        );
        session.Start(
            GoogleSpeechTranslation.WebSocketUrl(_provider.Options.BaseUrl, apiKey),
            webSocketHeaders,
            _provider.Options.WebSocket
        );
        return new TranscriptionStreamStart(
            new ModelPartStream(session.Queue, session.Cancel),
            new ProviderResponse(timestamp: timestamp.UtcDateTime, modelId: ModelId),
            new ProviderRequest(setup.ToJsonString())
        );
    }

    /// <summary>Builds the Interactions transcription body.</summary>
    public static JsonObject BuildRequest(
        string modelId,
        AudioInput audio,
        JsonObject? transcriptionConfig
    )
    {
        var body = new JsonObject
        {
            ["model"] = modelId,
            ["input"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "audio",
                    ["data"] = Convert.ToBase64String(audio.Data),
                    ["mime_type"] = audio.MediaType,
                },
            },
        };
        if (transcriptionConfig != null)
        {
            body["generation_config"] = new JsonObject
            {
                ["transcription_config"] = transcriptionConfig,
            };
        }

        return body;
    }

    /// <summary>Builds Google's <c>AudioTranscriptionConfig</c> from provider options, or null when none are set.</summary>
    private static JsonObject? BuildAudioTranscriptionConfig(JsonElement providerOptions)
    {
        if (!GoogleJson.TryObject(providerOptions, "google", out var options))
        {
            return null;
        }

        var config = new JsonObject();
        foreach (
            var name in new[]
            {
                "languageCodes",
                "customVocabulary",
                "wordTimestamp",
                "diarization",
                "mode",
            }
        )
        {
            if (
                options.TryGetProperty(name, out var value)
                && value.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined)
            )
            {
                config[name] = GoogleJson.Clone(value);
            }
        }

        return config.Count > 0 ? config : null;
    }

    private static void ValidateLiveInputAudioFormat(AudioFormat inputAudioFormat)
    {
        if (
            inputAudioFormat.Type != "audio/pcm"
            || (inputAudioFormat.Rate is { } rate && rate != 16000)
        )
        {
            throw new InvalidArgumentException(
                "inputAudioFormat",
                inputAudioFormat.Type,
                "The Gemini Live transcription API only supports 16kHz 16-bit PCM input audio."
            );
        }
    }

    /// <summary>Maps <c>google</c> options onto <c>transcription_config</c>, or null when none are set.</summary>
    private static JsonObject? TranscriptionConfig(JsonElement providerOptions)
    {
        if (!GoogleJson.TryObject(providerOptions, "google", out var options))
        {
            return null;
        }

        var config = new JsonObject();
        if (options.TryGetProperty("languageCodes", out var languages))
        {
            config["language_codes"] = GoogleJson.Clone(languages);
        }

        if (options.TryGetProperty("customVocabulary", out var vocabulary))
        {
            config["custom_vocabulary"] = GoogleJson.Clone(vocabulary);
        }

        var mode = GoogleJson.String(options, "mode");
        var diarization =
            options.TryGetProperty("diarization", out var diarizationValue)
            && diarizationValue.ValueKind == JsonValueKind.True;
        var wordTimestamp =
            options.TryGetProperty("wordTimestamp", out var wordValue)
            && wordValue.ValueKind == JsonValueKind.True;
        if (mode != null || diarization || wordTimestamp)
        {
            var modeObject = new JsonObject { ["type"] = (mode ?? "VERBATIM").ToLowerInvariant() };
            if (diarization)
            {
                modeObject["diarization_mode"] = "speaker";
            }

            if (wordTimestamp)
            {
                modeObject["timestamp_granularities"] = new JsonArray("word");
            }

            config["mode"] = modeObject;
        }

        return config.Count > 0 ? config : null;
    }

    // Maps to `createGoogleLiveTranscriptionStream`.
    private sealed class Session
    {
        private readonly TranscriptionStreamCall _call;
        private readonly IReadOnlyList<OperationWarning> _warnings;
        private readonly JsonObject _setup;
        private readonly double _inputAudioRate;
        private readonly bool _manualActivityDetection;
        private readonly int _finishGraceMs;

        // Handlers, the audio loop and the grace timer all touch this state.
        private readonly object _gate = new object();
        private readonly TaskCompletionSource<bool> _setupComplete = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        private WebSocketConnection? _connection;
        private AudioChunkReader? _reader;
        private Timer? _finishTimer;
        private bool _finished;

        // Google Live messages carry no response or item ids, so a segment counter makes synthetic ones.
        private int _segmentCounter;
        private string _segmentBuffer = string.Empty;
        private string _fullText = string.Empty;

        // Latest revisable interim text: the fallback final when no finished segment arrives.
        private string _latestInterim = string.Empty;
        private string? _language;
        private bool _audioEnded;
        private JsonElement? _usageMetadata;
        private string? _lastVoiceActivityType;
        private double? _lastVoiceActivityOffsetMs;
        private long _sentAudioBytes;

        public Session(
            TranscriptionStreamCall call,
            IReadOnlyList<OperationWarning> warnings,
            JsonObject setup,
            double inputAudioRate,
            bool manualActivityDetection,
            int finishGraceMs
        )
        {
            _call = call;
            _warnings = warnings;
            _setup = setup;
            _inputAudioRate = inputAudioRate;
            _manualActivityDetection = manualActivityDetection;
            _finishGraceMs = finishGraceMs;
        }

        public PartQueue<ModelStreamPart> Queue { get; } = new PartQueue<ModelStreamPart>();

        public void Start(
            Uri url,
            IReadOnlyDictionary<string, string?> headers,
            WebSocketFactory? webSocket
        )
        {
            var connection = ConnectToWebSocket.Connect(
                new ConnectToWebSocketOptions
                {
                    Url = url,
                    Headers = headers,
                    WebSocket = webSocket,
                    AbortSignal = _call.CancellationToken,
                    AbortReason = _call.AbortReason,
                    OnAbort = FinishWithError,
                    OnProcessingError = FinishWithError,
                    OnOpen = OnOpen,
                    OnMessageText = OnMessageTextAsync,
                    OnSocketError = () =>
                        FinishWithError(new AiSdkException("Google Live transcription error")),
                    OnClose = OnClose,
                }
            );
            lock (_gate)
            {
                _connection = connection;
            }
        }

        public void Cancel()
        {
            lock (_gate)
            {
                if (_finished)
                {
                    return;
                }

                _finished = true;
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

        private string SegmentId =>
            "google-segment-" + _segmentCounter.ToString(CultureInfo.InvariantCulture);

        private double SentAudioMs => _sentAudioBytes / (_inputAudioRate * 2) * 1000;

        // Once the audio has ended, the server has transcribed all of it when server VAD ended the last speech at the
        // end of the sent audio and no transcript is pending.
        private bool IsTranscriptionComplete =>
            _lastVoiceActivityType == "ACTIVITY_END"
            && _segmentBuffer.Length == 0
            && _latestInterim.Length == 0
            && (_lastVoiceActivityOffsetMs ?? 0) >= SentAudioMs - AudioEndToleranceMs;

        private void CancelPendingFinish()
        {
            _finishTimer?.Dispose();
            _finishTimer = null;
        }

        // Trailing transcripts can arrive after audioStreamEnd; without a terminal signal, finish after a quiet grace
        // window. Transcript activity reschedules the timer.
        private void SchedulePendingFinish()
        {
            if (_finished || !_audioEnded)
            {
                return;
            }

            CancelPendingFinish();
            _finishTimer = new Timer(
                _ =>
                {
                    lock (_gate)
                    {
                        _finishTimer = null;
                        Finish();
                    }
                },
                null,
                _finishGraceMs,
                Timeout.Infinite
            );
        }

        private void Cleanup(int? closeCode)
        {
            AudioChunkReader? reader;
            WebSocketConnection? connection;
            lock (_gate)
            {
                CancelPendingFinish();
                reader = _reader;
                connection = _connection;
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

            connection?.Close(closeCode);
        }

        private void FinishWithError(Exception error)
        {
            lock (_gate)
            {
                if (_finished)
                {
                    return;
                }

                _finished = true;
            }

            Cleanup(null);
            Queue.Complete(error);
        }

        private void CompleteSegment()
        {
            // A finished segment supersedes any interim text it revises.
            if (_segmentBuffer.Length == 0)
            {
                if (_latestInterim.Length == 0)
                {
                    return;
                }

                _segmentBuffer = _latestInterim;
            }

            _latestInterim = string.Empty;
            Queue.Enqueue(
                new ModelStreamPart
                {
                    Type = "transcript-final",
                    Id = SegmentId,
                    Text = _segmentBuffer,
                }
            );
            _fullText += _fullText.Length == 0 ? _segmentBuffer : " " + _segmentBuffer;
            _segmentBuffer = string.Empty;
            _segmentCounter++;
        }

        // Callers hold `_gate`.
        private void Finish()
        {
            if (_finished)
            {
                return;
            }

            CompleteSegment();
            _finished = true;
            var part = new ModelStreamPart
            {
                Type = "finish",
                Text = _fullText,
                Segments = Array.Empty<TranscriptSegment>(),
                Language = _language,
            };
            if (_usageMetadata is { } usage)
            {
                part.Usage = ReadUsage(usage);
                part.UsageValue = usage;
                part.ProviderMetadata = JsonSerializer.SerializeToElement(
                    new JsonObject
                    {
                        ["google"] = new JsonObject
                        {
                            ["usageMetadata"] = JsonNode.Parse(usage.GetRawText()),
                        },
                    }
                );
            }

            Queue.Enqueue(part);
            Queue.Complete(null);
            Cleanup(1000);
        }

        private static OperationUsage ReadUsage(JsonElement usage)
        {
            static int? Count(JsonElement element, string name) =>
                element.TryGetProperty(name, out var value)
                && value.ValueKind == JsonValueKind.Number
                && value.TryGetInt32(out var number)
                    ? number
                    : null;

            return new OperationUsage(
                Count(usage, "promptTokenCount"),
                Count(usage, "candidatesTokenCount"),
                Count(usage, "totalTokenCount")
            );
        }

        private void OnOpen(IWebSocketLike socket)
        {
            Queue.Enqueue(new ModelStreamPart { Type = "stream-start", Warnings = _warnings });
            socket.Send(new JsonObject { ["setup"] = _setup.DeepClone() }.ToJsonString());

            // Audio may only be sent after the server acknowledged the setup.
            _ = Task.Run(async () =>
            {
                try
                {
                    await _setupComplete.Task.ConfigureAwait(false);
                    if (!IsFinished)
                    {
                        await SendAudioAsync(socket).ConfigureAwait(false);
                    }
                }
                catch (Exception error)
                {
                    FinishWithError(error);
                }
            });
        }

        private async Task SendAudioAsync(IWebSocketLike socket)
        {
            // With manual activity detection, the audio stream is one user turn.
            if (_manualActivityDetection)
            {
                socket.Send(
                    new JsonObject
                    {
                        ["realtimeInput"] = new JsonObject { ["activityStart"] = new JsonObject() },
                    }.ToJsonString()
                );
            }

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

                    lock (_gate)
                    {
                        _sentAudioBytes += bytes.Length;
                    }

                    socket.Send(
                        new JsonObject
                        {
                            ["realtimeInput"] = new JsonObject
                            {
                                ["audio"] = new JsonObject
                                {
                                    ["data"] = Convert.ToBase64String(bytes),
                                    ["mimeType"] =
                                        "audio/pcm;rate="
                                        + _inputAudioRate.ToString(
                                            "R",
                                            CultureInfo.InvariantCulture
                                        ),
                                },
                            },
                        }.ToJsonString()
                    );

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

            lock (_gate)
            {
                if (_finished)
                {
                    return;
                }

                socket.Send(
                    new JsonObject
                    {
                        ["realtimeInput"] = _manualActivityDetection
                            ? new JsonObject { ["activityEnd"] = new JsonObject() }
                            : new JsonObject { ["audioStreamEnd"] = true },
                    }.ToJsonString()
                );
                _audioEnded = true;
                if (IsTranscriptionComplete)
                {
                    Finish();
                }
                else
                {
                    SchedulePendingFinish();
                }
            }
        }

        private void OnClose(int? code, string? reason)
        {
            lock (_gate)
            {
                if (_finished)
                {
                    return;
                }

                // A close after the input audio ended means the server delivered everything it will deliver.
                if (_audioEnded)
                {
                    Finish();
                    return;
                }
            }

            FinishWithError(
                new AiSdkException(
                    "Google Live transcription WebSocket closed unexpectedly before finishing (code "
                        + (code?.ToString(CultureInfo.InvariantCulture) ?? "unknown")
                        + (string.IsNullOrEmpty(reason) ? string.Empty : ", reason: " + reason)
                        + ")."
                )
            );
        }

        private Task OnMessageTextAsync(string text)
        {
            JsonElement message;
            try
            {
                using var document = JsonDocument.Parse(text);
                message = document.RootElement.Clone();
            }
            catch (JsonException)
            {
                return Task.CompletedTask;
            }

            if (message.ValueKind != JsonValueKind.Object)
            {
                return Task.CompletedTask;
            }

            lock (_gate)
            {
                if (_finished)
                {
                    return Task.CompletedTask;
                }

                if (_call.IncludeRawChunks == true)
                {
                    Queue.Enqueue(new ModelStreamPart { Type = "raw", Raw = message });
                }

                if (HasValue(message, "setupComplete"))
                {
                    _setupComplete.TrySetResult(true);
                }

                if (GoogleJson.TryObject(message, "usageMetadata", out var usage))
                {
                    _usageMetadata = usage;
                }

                if (
                    GoogleJson.TryObject(message, "voiceActivity", out var activity)
                    && GoogleJson.String(activity, "type") is { } activityType
                )
                {
                    _lastVoiceActivityType = activityType;
                    _lastVoiceActivityOffsetMs =
                        GoogleVertexSpeechTranscriptionModel.ParseDuration(
                            GoogleJson.String(activity, "audioOffset")
                        ) * 1000;
                }

                if (GoogleJson.TryObject(message, "error", out var error))
                {
                    var failure = new AiSdkException(
                        GoogleJson.String(error, "message") ?? "Google Live API error"
                    );
                    // Re-entrant: FinishWithError locks the same gate.
                    FinishWithError(failure);
                    return Task.CompletedTask;
                }

                GoogleJson.TryObject(message, "serverContent", out var serverContent);
                var hasContent = serverContent.ValueKind == JsonValueKind.Object;

                // Low-latency revisable transcription while the user is speaking.
                if (
                    hasContent
                    && GoogleJson.TryObject(
                        serverContent,
                        "interimInputTranscription",
                        out var interim
                    )
                    && GoogleJson.String(interim, "text") is { Length: > 0 } interimText
                )
                {
                    SchedulePendingFinish();
                    _latestInterim = interimText;
                    Queue.Enqueue(
                        new ModelStreamPart
                        {
                            Type = "transcript-partial",
                            Id = SegmentId,
                            Text = interimText,
                        }
                    );
                }

                JsonElement transcription = default;
                var hasTranscription =
                    (
                        hasContent
                        && GoogleJson.TryObject(
                            serverContent,
                            "inputTranscription",
                            out transcription
                        )
                    ) || GoogleJson.TryObject(message, "inputTranscription", out transcription);
                if (hasTranscription)
                {
                    if (GoogleJson.String(transcription, "languageCode") is { } languageCode)
                    {
                        _language = languageCode;
                    }

                    if (GoogleJson.String(transcription, "text") is { Length: > 0 } delta)
                    {
                        SchedulePendingFinish();

                        // A real transcription delta supersedes interim fallback text.
                        _latestInterim = string.Empty;
                        _segmentBuffer += delta;
                        Queue.Enqueue(
                            new ModelStreamPart
                            {
                                Type = "transcript-delta",
                                Id = SegmentId,
                                Delta = delta,
                            }
                        );
                    }

                    if (
                        transcription.TryGetProperty("finished", out var finished)
                        && finished.ValueKind == JsonValueKind.True
                    )
                    {
                        CompleteSegment();
                    }
                }

                // gemini-3.5-transcribe-live ends each segment with generationComplete and never sends turnComplete.
                var turnComplete = hasContent && IsTrue(serverContent, "turnComplete");
                if ((hasContent && IsTrue(serverContent, "generationComplete")) || turnComplete)
                {
                    CompleteSegment();
                }

                // interactionStatus idle (REQUIRES_ACTION in the EAP builds) is the definitive
                // all-processing-complete signal: finish as soon as the input audio has ended.
                var status = hasContent
                    ? GoogleJson.String(serverContent, "interactionStatus")
                    : null;
                if (
                    _audioEnded
                    && (
                        status is "IDLE" or "REQUIRES_ACTION"
                        || (turnComplete && status is null)
                        || IsTranscriptionComplete
                    )
                )
                {
                    Finish();
                }
            }

            return Task.CompletedTask;
        }

        private static bool HasValue(JsonElement element, string name) =>
            element.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null;

        private static bool IsTrue(JsonElement element, string name) =>
            element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;
    }
}

/// <summary>Speech translation configuration for the Gemini Live API. The socket itself is not opened here.</summary>
public static class GoogleSpeechTranslation
{
    /// <summary>Live translation WebSocket path.</summary>
    public const string LivePath =
        "google.ai.generativelanguage.v1beta.GenerativeService.BidiGenerateContent";

    /// <summary>Builds a <c>wss</c> URL and strips a trailing <c>/v1beta</c> or <c>/v1alpha</c> segment.</summary>
    public static Uri WebSocketUrl(string baseUrl, string? apiKey)
    {
        var builder = LiveWebSocketUrl(baseUrl, LivePath);
        if (!string.IsNullOrEmpty(apiKey))
        {
            builder.Query = "key=" + Uri.EscapeDataString(apiKey!);
        }

        return builder.Uri;
    }

    /// <summary>The Live API base URL: <paramref name="baseUrl"/> without a trailing <c>/v1beta</c> or <c>/v1alpha</c>.</summary>
    internal static UriBuilder LiveBaseUrl(string baseUrl)
    {
        var url = new Uri(baseUrl, UriKind.Absolute);
        var path = url.AbsolutePath.TrimEnd('/');
        if (
            path.EndsWith("/v1beta", StringComparison.Ordinal)
            || path.EndsWith("/v1alpha", StringComparison.Ordinal)
        )
        {
            path = path.Substring(0, path.LastIndexOf('/'));
        }

        return new UriBuilder(url) { Path = path };
    }

    /// <summary>A <c>wss</c> (or <c>ws</c> for http) Live API URL for the given bidi service path.</summary>
    internal static UriBuilder LiveWebSocketUrl(string baseUrl, string servicePath)
    {
        var builder = LiveBaseUrl(baseUrl);
        builder.Scheme = builder.Scheme == "https" ? "wss" : "ws";
        builder.Path = builder.Path.TrimEnd('/') + "/ws/" + servicePath;
        return builder;
    }

    /// <summary>Warnings for options the Live translation API does not accept.</summary>
    public static IReadOnlyList<GoogleWarning> WarningsFor(
        string? sourceLanguage,
        string? outputAudioFormat
    )
    {
        var warnings = new List<GoogleWarning>();
        if (!string.IsNullOrEmpty(sourceLanguage))
        {
            warnings.Add(GoogleWarning.Unsupported("sourceLanguage"));
        }

        if (!string.IsNullOrEmpty(outputAudioFormat))
        {
            warnings.Add(GoogleWarning.Unsupported("outputAudioFormat"));
        }

        return warnings;
    }
}
