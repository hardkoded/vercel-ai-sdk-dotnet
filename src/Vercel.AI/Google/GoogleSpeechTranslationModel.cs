// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>Settings of <see cref="GoogleSpeechTranslationModel"/>. Maps to <c>GoogleSpeechTranslationModelConfig</c>.</summary>
public sealed class GoogleSpeechTranslationModelConfig
{
    /// <summary>Creates a config.</summary>
    public GoogleSpeechTranslationModelConfig(
        string provider,
        string baseUrl,
        Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> headers
    )
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        Headers = headers ?? throw new ArgumentNullException(nameof(headers));
    }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>API base URL.</summary>
    public string BaseUrl { get; }

    /// <summary>Resolves the request headers, including the credential.</summary>
    public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> Headers { get; }

    /// <summary>Custom WebSocket implementation. Maps to <c>webSocket</c>.</summary>
    public WebSocketFactory? WebSocket { get; set; }

    /// <summary>Clock used for the response timestamp. Maps to <c>_internal.currentDate</c>.</summary>
    public Func<DateTimeOffset>? CurrentDate { get; set; }

    /// <summary>Trailing output silence, in milliseconds, after which the stream finishes. Maps to <c>_internal.finishGraceMs</c>.</summary>
    public int? FinishGraceMs { get; set; }
}

/// <summary>
/// Google speech translation model over the Gemini Live API WebSocket. Maps to <c>GoogleSpeechTranslationModel</c>.
/// </summary>
public sealed class GoogleSpeechTranslationModel : ISpeechTranslationModel, ISpeechTranslationCaller
{
    /// <summary>After the input audio has ended, finish after this much trailing output silence.</summary>
    private const int DefaultFinishGraceMs = 1000;

    private const int MinFinishDelayMs = 50;

    private const int GoogleLiveOutputAudioRate = 24000;
    private const int Pcm16SilenceAmplitudeThreshold = 128;

    private readonly GoogleSpeechTranslationModelConfig _config;

    /// <summary>Creates a speech translation model.</summary>
    public GoogleSpeechTranslationModel(string modelId, GoogleSpeechTranslationModelConfig config)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>Creates a speech translation model that takes its base URL, headers and WebSocket from <paramref name="provider"/>.</summary>
    public GoogleSpeechTranslationModel(GoogleProvider provider, string modelId)
        : this(
            modelId,
            new GoogleSpeechTranslationModelConfig(
                (provider ?? throw new ArgumentNullException(nameof(provider))).ModelProvider
                    + ".speech-translation",
                provider.Options.BaseUrl,
                async cancellationToken =>
                    await provider.HeadersAsync(cancellationToken).ConfigureAwait(false)
            )
            {
                WebSocket = provider.Options.WebSocket,
                CurrentDate = () => provider.Clock(),
            }
        ) { }

    /// <inheritdoc />
    public string Provider => _config.Provider;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <summary>The Gemini Live API only supports streaming translation.</summary>
    public Task<TranscriptionResult> DoTranslateAsync(
        AudioInput audio,
        CancellationToken cancellationToken
    )
    {
        throw new UnsupportedFunctionalityException(
            "translate",
            "The Gemini Live translation API only supports streaming translation."
        );
    }

    /// <summary>Starts a streaming translation over the Gemini Live API WebSocket. Maps to <c>doStream</c>.</summary>
    public async Task<TranslationStreamStart> DoStreamAsync(
        TranslationStreamCall call,
        CancellationToken cancellationToken
    )
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        if (string.IsNullOrEmpty(call.TargetLanguage))
        {
            throw new InvalidArgumentException(
                "targetLanguage",
                null,
                "targetLanguage is required for translation model '" + ModelId + "'."
            );
        }

        var timestamp = (_config.CurrentDate?.Invoke() ?? DateTimeOffset.UtcNow).UtcDateTime;
        bool? echoTargetLanguage = null;
        if (
            call.ProviderOptions.ValueKind == JsonValueKind.Object
            && call.ProviderOptions.TryGetProperty("google", out var google)
            && google.ValueKind == JsonValueKind.Object
            && google.TryGetProperty("echoTargetLanguage", out var echo)
            && echo.ValueKind != JsonValueKind.Null
        )
        {
            if (echo.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            {
                throw new InvalidArgumentException(
                    "providerOptions",
                    google.GetRawText(),
                    "invalid google provider options: echoTargetLanguage must be a boolean"
                );
            }

            echoTargetLanguage = echo.GetBoolean();
        }

        var warnings = new List<OperationWarning>();
        ValidateGoogleSpeechTranslationInputAudioFormat(call.InputAudioFormat);

        if (call.SourceLanguage != null)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "sourceLanguage",
                    "The Gemini Live translation API auto-detects the source language and does not accept a source language."
                )
            );
        }

        if (call.OutputAudioFormat != null)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "outputAudioFormat",
                    "The Gemini Live API always outputs 24kHz 16-bit PCM audio and does not accept an output audio format."
                )
            );
        }

        var headers = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var pair in await _config.Headers(cancellationToken).ConfigureAwait(false))
        {
            headers[pair.Key] = pair.Value;
        }

        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        // The last case-variant wins: per-call headers are spread after configuration headers.
        string? apiKey = null;
        foreach (var pair in headers)
        {
            if (
                string.Equals(pair.Key, "x-goog-api-key", StringComparison.OrdinalIgnoreCase)
                && pair.Value != null
            )
            {
                apiKey = pair.Value;
            }
        }

        if (apiKey == null)
        {
            throw new AiSdkException(
                "Google Generative AI API key is required for streaming translation."
            );
        }

        var webSocketHeaders = headers
            .Where(pair =>
                !string.Equals(pair.Key, "x-goog-api-key", StringComparison.OrdinalIgnoreCase)
            )
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

        var setup = BuildGoogleLiveSpeechTranslationSetup(
            ModelId,
            call.TargetLanguage,
            echoTargetLanguage
        );
        var session = new LiveSpeechTranslationSession(
            call,
            new JsonObject { ["setup"] = setup.DeepClone() },
            (int)(call.InputAudioFormat.Rate ?? 16000),
            _config.FinishGraceMs ?? DefaultFinishGraceMs,
            warnings
        );
        session.Start(
            GoogleSpeechTranslation.WebSocketUrl(_config.BaseUrl, apiKey),
            webSocketHeaders,
            _config.WebSocket
        );
        return new TranslationStreamStart(
            new ModelPartStream(session.Queue, session.Cancel),
            new ProviderResponse(timestamp: timestamp, modelId: ModelId),
            new ProviderRequest(setup.ToJsonString())
        );
    }

    private static JsonObject BuildGoogleLiveSpeechTranslationSetup(
        string modelId,
        string targetLanguage,
        bool? echoTargetLanguage
    )
    {
        var translationConfig = new JsonObject { ["targetLanguageCode"] = targetLanguage };
        if (echoTargetLanguage != null)
        {
            translationConfig["echoTargetLanguage"] = echoTargetLanguage;
        }

        return new JsonObject
        {
            ["model"] = GoogleModelPath.Get(modelId),
            ["generationConfig"] = new JsonObject
            {
                ["responseModalities"] = new JsonArray("AUDIO"),
                ["translationConfig"] = translationConfig,
            },
            ["inputAudioTranscription"] = new JsonObject(),
            ["outputAudioTranscription"] = new JsonObject(),
        };
    }

    private static void ValidateGoogleSpeechTranslationInputAudioFormat(AudioFormat format)
    {
        if (format.Type != "audio/pcm" || (format.Rate != null && format.Rate != 16000))
        {
            throw new InvalidArgumentException(
                "inputAudioFormat",
                format.Type,
                "The Gemini Live translation API only supports 16kHz 16-bit PCM input audio."
            );
        }
    }

    private static double? GetPcm16SilenceDurationMs(string audio)
    {
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(audio);
        }
        catch (FormatException)
        {
            return null;
        }

        if (bytes.Length < 2)
        {
            return null;
        }

        var sampleCount = bytes.Length / 2;
        for (var i = 0; i < sampleCount; i++)
        {
            var sample = (short)(bytes[i * 2] | (bytes[(i * 2) + 1] << 8));
            if (Math.Abs((int)sample) > Pcm16SilenceAmplitudeThreshold)
            {
                return null;
            }
        }

        return (double)sampleCount / GoogleLiveOutputAudioRate * 1000;
    }

    // Live Translation emits periodic usage deltas. Its TEXT prompt detail is internal translation context
    // (the public input is audio-only), so only aggregate the billable input/output audio modalities.
    private static OperationUsage? AccumulateGoogleLiveUsage(
        OperationUsage? usage,
        JsonElement usageMetadata
    )
    {
        var inputAudioTokens = usage?.InputAudioTokens;
        var outputAudioTokens = usage?.OutputAudioTokens;

        inputAudioTokens = SumAudioTokens(usageMetadata, "promptTokensDetails", inputAudioTokens);
        outputAudioTokens = SumAudioTokens(
            usageMetadata,
            "responseTokensDetails",
            outputAudioTokens
        );

        if (inputAudioTokens == null && outputAudioTokens == null)
        {
            return usage;
        }

        return new OperationUsage(
            inputAudioTokens: inputAudioTokens,
            outputAudioTokens: outputAudioTokens
        );
    }

    private static int? SumAudioTokens(JsonElement usageMetadata, string name, int? total)
    {
        if (
            usageMetadata.ValueKind != JsonValueKind.Object
            || !usageMetadata.TryGetProperty(name, out var details)
            || details.ValueKind != JsonValueKind.Array
        )
        {
            return total;
        }

        foreach (var detail in details.EnumerateArray())
        {
            if (
                detail.ValueKind == JsonValueKind.Object
                && detail.TryGetProperty("modality", out var modality)
                && modality.ValueKind == JsonValueKind.String
                && modality.GetString() == "AUDIO"
                && detail.TryGetProperty("tokenCount", out var count)
                && count.ValueKind == JsonValueKind.Number
                && count.TryGetInt32(out var tokens)
            )
            {
                total = (total ?? 0) + tokens;
            }
        }

        return total;
    }

    private static string? Text(JsonElement element, params string[] path)
    {
        var current = element;
        foreach (var name in path)
        {
            if (
                current.ValueKind != JsonValueKind.Object
                || !current.TryGetProperty(name, out current)
            )
            {
                return null;
            }
        }

        return current.ValueKind == JsonValueKind.String ? current.GetString() : null;
    }

    // Maps to `createGoogleLiveSpeechTranslationStream`.
    private sealed class LiveSpeechTranslationSession
    {
        private readonly TranslationStreamCall _call;
        private readonly JsonObject _setup;
        private readonly int _inputAudioRate;
        private readonly int _finishGraceMs;
        private readonly IReadOnlyList<OperationWarning> _warnings;
        private readonly object _gate = new object();

        // The Live API contract requires waiting for the `setupComplete` server message before sending realtime
        // input: the audio send loop is gated on this task.
        private readonly TaskCompletionSource<bool> _setupComplete = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        private WebSocketConnection? _connection;
        private AudioChunkReader? _reader;
        private bool _finished;

        // Google Live messages carry no response/item IDs; a turn counter generates consistent synthetic IDs.
        private int _turnCounter;
        private string _sourceText = string.Empty;
        private string _sourceTurnBuffer = string.Empty;
        private string _translationText = string.Empty;
        private string _translationTurnBuffer = string.Empty;
        private bool _audioEnded;
        private OperationUsage? _usage;

        // Live Translation is a continuous pipeline rather than a turn-based model. After audioStreamEnd it keeps
        // sending PCM silence indefinitely and does not emit turnComplete. Drain translated speech, then finish after
        // enough trailing silence. Keep turnComplete handling as a fallback for compatible implementations.
        private bool _openTurn;
        private bool _sawTurnComplete;
        private double _trailingSilenceMs;
        private Timer? _finishTimer;

        public LiveSpeechTranslationSession(
            TranslationStreamCall call,
            JsonObject setup,
            int inputAudioRate,
            int finishGraceMs,
            IReadOnlyList<OperationWarning> warnings
        )
        {
            _call = call;
            _setup = setup;
            _inputAudioRate = inputAudioRate;
            _finishGraceMs = finishGraceMs;
            _warnings = warnings;
        }

        public PartQueue<ModelStreamPart> Queue { get; } = new PartQueue<ModelStreamPart>();

        private string ItemId => "google-item-" + _turnCounter;

        public void Start(
            Uri url,
            IReadOnlyDictionary<string, string?> headers,
            WebSocketFactory? webSocket
        )
        {
            WebSocketConnection connection;
            try
            {
                connection = ConnectToWebSocket.Connect(
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
                            FinishWithError(new AiSdkException("Google Live translation error")),
                        OnClose = OnClose,
                    }
                );
            }
            catch (Exception error)
            {
                // The constructor threw: cancel the caller's audio stream so an upstream producer does not hang.
                FinishWithError(error);
                return;
            }

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

        private void CancelPendingFinish()
        {
            Timer? timer;
            lock (_gate)
            {
                timer = _finishTimer;
                _finishTimer = null;
            }

            timer?.Dispose();
        }

        private void SchedulePendingFinish()
        {
            lock (_gate)
            {
                if (_finished || _finishTimer != null)
                {
                    return;
                }

                // A JavaScript timer fires on a later macrotask, after queued server messages. A .NET timer has no such
                // ordering, so keep a small settle time to let messages already received cancel a pending finish.
                _finishTimer = new Timer(
                    _ =>
                    {
                        lock (_gate)
                        {
                            _finishTimer?.Dispose();
                            _finishTimer = null;
                        }

                        Finish();
                    },
                    null,
                    Math.Max(_finishGraceMs, MinFinishDelayMs),
                    Timeout.Infinite
                );
            }
        }

        private void OnTurnActivity()
        {
            lock (_gate)
            {
                _openTurn = true;
                _trailingSilenceMs = 0;
            }

            CancelPendingFinish();
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

            CancelPendingFinish();
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

        private void Finish()
        {
            lock (_gate)
            {
                if (_finished)
                {
                    return;
                }

                if (_sourceTurnBuffer != string.Empty || _translationTurnBuffer != string.Empty)
                {
                    CompleteTurn();
                }

                _finished = true;
                Queue.Enqueue(
                    new ModelStreamPart
                    {
                        Type = "finish",
                        SourceText = _sourceText,
                        OutputText = _translationText,
                        Usage = _usage,
                    }
                );
                Queue.Complete(null);
            }

            Cleanup(1000);
        }

        // Callers hold `_gate`.
        private void CompleteTurn()
        {
            if (_sourceTurnBuffer != string.Empty)
            {
                Queue.Enqueue(
                    new ModelStreamPart
                    {
                        Type = "source-transcript-final",
                        Id = ItemId,
                        Text = _sourceTurnBuffer,
                    }
                );
                _sourceText += _sourceTurnBuffer;
                _sourceTurnBuffer = string.Empty;
            }

            if (_translationTurnBuffer != string.Empty)
            {
                Queue.Enqueue(
                    new ModelStreamPart
                    {
                        Type = "output-text-final",
                        Id = ItemId,
                        Text = _translationTurnBuffer,
                    }
                );
                _translationText += _translationTurnBuffer;
                _translationTurnBuffer = string.Empty;
            }

            _turnCounter++;
        }

        private void OnClose(int? code, string? reason)
        {
            bool pending;
            lock (_gate)
            {
                if (_finished)
                {
                    return;
                }

                pending = _finishTimer != null;
            }

            // A close while a finish is pending confirms that no further turn activity follows.
            if (pending)
            {
                Finish();
                return;
            }

            // A close before the finish condition was reached is an abnormal termination:
            // surface the close diagnostics.
            FinishWithError(
                new AiSdkException(
                    "Google Live translation WebSocket closed unexpectedly before finishing"
                        + " (code "
                        + (code?.ToString() ?? "unknown")
                        + (string.IsNullOrEmpty(reason) ? string.Empty : ", reason: " + reason)
                        + ")."
                )
            );
        }

        private void OnOpen(IWebSocketLike socket)
        {
            Queue.Enqueue(new ModelStreamPart { Type = "stream-start", Warnings = _warnings });
            socket.Send(_setup.ToJsonString());

            // Audio may only be sent after the server acknowledged the setup.
            _ = AfterSetupAsync(socket)
                .ContinueWith(
                    task => FinishWithError(task.Exception!.GetBaseException()),
                    TaskContinuationOptions.OnlyOnFaulted
                );
        }

        private async Task AfterSetupAsync(IWebSocketLike socket)
        {
            await _setupComplete.Task.ConfigureAwait(false);
            if (!IsFinished)
            {
                await SendAudioAsync(socket).ConfigureAwait(false);
            }
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
                    if (bytes == null || IsFinished)
                    {
                        break;
                    }

                    socket.Send(
                        new JsonObject
                        {
                            ["realtimeInput"] = new JsonObject
                            {
                                ["audio"] = new JsonObject
                                {
                                    ["data"] = Convert.ToBase64String(bytes),
                                    ["mimeType"] = "audio/pcm;rate=" + _inputAudioRate,
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

                // Unlocked again: cleanup must cancel the audio stream, not the reader.
                lock (_gate)
                {
                    _reader = null;
                }
            }

            if (IsFinished)
            {
                return;
            }

            socket.Send(
                new JsonObject
                {
                    ["realtimeInput"] = new JsonObject { ["audioStreamEnd"] = true },
                }.ToJsonString()
            );

            bool schedule;
            lock (_gate)
            {
                _audioEnded = true;

                // A turnComplete already received after the final audio chunk satisfies the finish condition.
                schedule = _sawTurnComplete && !_openTurn;
            }

            if (schedule)
            {
                SchedulePendingFinish();
            }
        }

        private Task OnMessageTextAsync(string text)
        {
            if (IsFinished)
            {
                return Task.CompletedTask;
            }

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

            if (_call.IncludeRawChunks == true)
            {
                Queue.Enqueue(new ModelStreamPart { Type = "raw", Raw = message });
            }

            if (message.TryGetProperty("setupComplete", out var setupComplete))
            {
                if (setupComplete.ValueKind != JsonValueKind.Null)
                {
                    _setupComplete.TrySetResult(true);
                }
            }

            if (
                message.TryGetProperty("usageMetadata", out var usageMetadata)
                && usageMetadata.ValueKind == JsonValueKind.Object
            )
            {
                lock (_gate)
                {
                    _usage = AccumulateGoogleLiveUsage(_usage, usageMetadata);
                }
            }

            if (
                message.TryGetProperty("error", out var error)
                && error.ValueKind != JsonValueKind.Null
            )
            {
                FinishWithError(
                    new AiSdkException(Text(error, "message") ?? "Google Live API error")
                );
                return Task.CompletedTask;
            }

            var inputTranscriptionText = Text(
                message,
                "serverContent",
                "inputTranscription",
                "text"
            ) ?? Text(message, "inputTranscription", "text");
            if (!string.IsNullOrEmpty(inputTranscriptionText))
            {
                OnTurnActivity();
                lock (_gate)
                {
                    _sourceTurnBuffer += inputTranscriptionText;
                    Queue.Enqueue(
                        new ModelStreamPart
                        {
                            Type = "source-transcript-delta",
                            Id = ItemId,
                            Delta = inputTranscriptionText,
                        }
                    );
                }
            }

            if (
                !message.TryGetProperty("serverContent", out var serverContent)
                || serverContent.ValueKind != JsonValueKind.Object
            )
            {
                return Task.CompletedTask;
            }

            if (
                serverContent.TryGetProperty("modelTurn", out var modelTurn)
                && modelTurn.ValueKind == JsonValueKind.Object
                && modelTurn.TryGetProperty("parts", out var parts)
                && parts.ValueKind == JsonValueKind.Array
            )
            {
                foreach (var part in parts.EnumerateArray())
                {
                    var data = Text(part, "inlineData", "data");
                    if (string.IsNullOrEmpty(data))
                    {
                        continue;
                    }

                    lock (_gate)
                    {
                        Queue.Enqueue(
                            new ModelStreamPart
                            {
                                Type = "audio",
                                Id = ItemId,
                                Audio = Convert.FromBase64String(data),
                            }
                        );
                    }

                    var silenceDurationMs = GetPcm16SilenceDurationMs(data!);
                    bool audioEnded;
                    lock (_gate)
                    {
                        audioEnded = _audioEnded;
                    }

                    if (audioEnded && silenceDurationMs != null)
                    {
                        bool done;
                        lock (_gate)
                        {
                            _trailingSilenceMs += silenceDurationMs.Value;
                            done = _trailingSilenceMs >= _finishGraceMs;
                        }

                        if (done)
                        {
                            Finish();
                            return Task.CompletedTask;
                        }
                    }
                    else
                    {
                        OnTurnActivity();
                    }
                }
            }

            var outputText = Text(serverContent, "outputTranscription", "text");
            if (!string.IsNullOrEmpty(outputText))
            {
                OnTurnActivity();
                lock (_gate)
                {
                    _translationTurnBuffer += outputText;
                    Queue.Enqueue(
                        new ModelStreamPart
                        {
                            Type = "output-text-delta",
                            Id = ItemId,
                            Delta = outputText,
                        }
                    );
                }
            }

            if (
                serverContent.TryGetProperty("turnComplete", out var turnComplete)
                && turnComplete.ValueKind == JsonValueKind.True
            )
            {
                bool audioEnded;
                lock (_gate)
                {
                    CompleteTurn();
                    _openTurn = false;
                    _sawTurnComplete = true;
                    audioEnded = _audioEnded;
                }

                if (audioEnded)
                {
                    SchedulePendingFinish();
                }
            }

            return Task.CompletedTask;
        }
    }
}
