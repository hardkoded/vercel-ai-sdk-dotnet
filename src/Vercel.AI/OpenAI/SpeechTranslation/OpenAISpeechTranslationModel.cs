// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>
/// OpenAI speech translation model. REST calls use <c>/audio/translations</c>. Realtime streaming uses the
/// <c>/realtime/translations</c> WebSocket.
/// </summary>
public sealed class OpenAISpeechTranslationModel : ISpeechTranslationModel, ISpeechTranslationCaller
{
    private readonly string _providerName;
    private readonly OpenAITranscriptionModel _inner;
    private readonly Func<string, Uri> _url;
    private readonly Func<
        IReadOnlyDictionary<string, string?>?,
        CancellationToken,
        Task<Dictionary<string, string?>>
    > _headers;
    private readonly WebSocketFactory? _webSocket;
    private readonly Func<DateTimeOffset>? _clock;

    /// <summary>Creates a speech translation model.</summary>
    public OpenAISpeechTranslationModel(OpenAIProvider provider, string modelId)
    {
        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        _providerName = provider.Name;
        _inner = new OpenAITranscriptionModel(provider, modelId, "audio/translations");
        _url = path => ApiKeys.Combine(provider.Options.BaseUrl, path);
        _headers = provider.CreateOpenAIHeadersAsync;
        _webSocket = provider.WebSocket;
        ModelId = modelId;
    }

    /// <summary>
    /// Creates a speech translation model from explicit configuration, as upstream's
    /// <c>OpenAISpeechTranslationModelConfig</c> does. <paramref name="url"/> maps a path to the request URL.
    /// </summary>
    internal OpenAISpeechTranslationModel(
        string modelId,
        string provider,
        ProviderHttp http,
        Func<string, Uri> url,
        Func<CancellationToken, Task<Dictionary<string, string?>>> headers,
        WebSocketFactory? webSocket = null,
        Func<DateTimeOffset>? clock = null
    )
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _providerName = provider;
        _url = url;
        _inner = new OpenAITranscriptionModel(
            modelId,
            provider,
            http,
            () => url("audio/translations"),
            () => url("realtime/translations"),
            headers
        );
        _headers = async (callHeaders, cancellationToken) =>
        {
            var combined = new Dictionary<string, string?>(
                await headers(cancellationToken).ConfigureAwait(false),
                StringComparer.OrdinalIgnoreCase
            );
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
    public string Provider => _providerName + ".speech-translation";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public Task<TranscriptionResult> DoTranslateAsync(
        AudioInput audio,
        CancellationToken cancellationToken
    )
    {
        return _inner.DoTranscribeAsync(audio, cancellationToken);
    }

    /// <summary>Starts a realtime speech translation over the OpenAI Realtime translations WebSocket. Maps to <c>doStream</c>.</summary>
    public async Task<TranslationStreamStart> DoStreamAsync(
        TranslationStreamCall call,
        CancellationToken cancellationToken
    )
    {
        return await DoStream(
                call ?? throw new ArgumentNullException(nameof(call)),
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    private static JsonObject BuildOpenAIRealtimeSpeechTranslationSession(string targetLanguage)
    {
        return new JsonObject
        {
            ["type"] = "session.update",
            ["session"] = new JsonObject
            {
                ["audio"] = new JsonObject
                {
                    ["input"] = new JsonObject
                    {
                        ["transcription"] = new JsonObject { ["model"] = "gpt-realtime-whisper" },
                        ["noise_reduction"] = null,
                    },
                    ["output"] = new JsonObject { ["language"] = targetLanguage },
                },
            },
        };
    }

    private static void ValidateOpenAISpeechTranslationInputAudioFormat(AudioFormat format)
    {
        if (format.Type != "audio/pcm" || (format.Rate != null && format.Rate != 24000))
        {
            throw new InvalidArgumentException(
                "inputAudioFormat",
                format.Type,
                "The OpenAI Realtime translation API only supports 24kHz 16-bit PCM input audio."
            );
        }
    }

    private async Task<TranslationStreamStart> DoStream(
        TranslationStreamCall call,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrEmpty(call.TargetLanguage))
        {
            throw new InvalidArgumentException(
                "targetLanguage",
                null,
                "targetLanguage is required for translation model '" + ModelId + "'."
            );
        }

        var timestamp = OpenAIClock.Now(_clock).UtcDateTime;
        var openai = OpenAIJson.Child(call.ProviderOptions, "openai");
        if (openai is { } provided && provided.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidArgumentException(
                "providerOptions",
                provided.GetRawText(),
                "invalid openai provider options: expected an object"
            );
        }

        var warnings = new List<OperationWarning>();
        ValidateOpenAISpeechTranslationInputAudioFormat(call.InputAudioFormat);

        if (call.SourceLanguage != null)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "sourceLanguage",
                    "The OpenAI Realtime translation API auto-detects the source language and does not accept a source language."
                )
            );
        }

        if (call.OutputAudioFormat != null)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "outputAudioFormat",
                    "The OpenAI Realtime translation API always outputs 24kHz 16-bit PCM audio and does not accept an output audio format."
                )
            );
        }

        var headers = await _headers(
                call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value),
                cancellationToken
            )
            .ConfigureAwait(false);
        var sessionUpdate = BuildOpenAIRealtimeSpeechTranslationSession(call.TargetLanguage);
        var connection = OpenAITranscriptionModel.GetOpenAIRealtimeConnection(headers);
        var session = new RealtimeSpeechTranslationSession(call, sessionUpdate, warnings);
        session.Start(
            WebSocketUtils.ToWebSocketUrl(
                _url("realtime/translations?model=" + Uri.EscapeDataString(ModelId))
            ),
            connection.Protocols,
            connection.Headers,
            _webSocket
        );
        return new TranslationStreamStart(
            new ModelPartStream(session.Queue, session.Cancel),
            new ProviderResponse(timestamp: timestamp, modelId: ModelId),
            new ProviderRequest(sessionUpdate.ToJsonString())
        );
    }

    // Maps to `createOpenAIRealtimeSpeechTranslationStream`.
    private sealed class RealtimeSpeechTranslationSession
    {
        private readonly TranslationStreamCall _call;
        private readonly JsonObject _sessionUpdate;
        private readonly IReadOnlyList<OperationWarning> _warnings;
        private readonly object _gate = new object();
        private WebSocketConnection? _connection;
        private AudioChunkReader? _reader;
        private bool _finished;
        private string _sourceText = string.Empty;
        private string _translationText = string.Empty;

        public RealtimeSpeechTranslationSession(
            TranslationStreamCall call,
            JsonObject sessionUpdate,
            IReadOnlyList<OperationWarning> warnings
        )
        {
            _call = call;
            _sessionUpdate = sessionUpdate;
            _warnings = warnings;
        }

        public PartQueue<ModelStreamPart> Queue { get; } = new PartQueue<ModelStreamPart>();

        public void Start(
            Uri url,
            IReadOnlyList<string> protocols,
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
                        Protocols = protocols,
                        Headers = headers,
                        WebSocket = webSocket,
                        AbortSignal = _call.CancellationToken,
                        AbortReason = _call.AbortReason,
                        OnAbort = FinishWithError,
                        OnProcessingError = FinishWithError,
                        OnOpen = OnOpen,
                        OnMessageText = OnMessageTextAsync,
                        OnSocketError = () =>
                            FinishWithError(
                                new AiSdkException("OpenAI realtime translation error")
                            ),
                        OnClose = (code, reason) =>
                        {
                            if (IsFinished)
                            {
                                return;
                            }

                            // A close before the finish event is an abnormal termination:
                            // surface the close diagnostics instead of silently closing.
                            FinishWithError(
                                new AiSdkException(
                                    "OpenAI realtime translation WebSocket closed unexpectedly before finishing"
                                        + " (code "
                                        + (code?.ToString() ?? "unknown")
                                        + (
                                            string.IsNullOrEmpty(reason)
                                                ? string.Empty
                                                : ", reason: " + reason
                                        )
                                        + ")."
                                )
                            );
                        },
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

        private void Finish()
        {
            if (!TryFinish())
            {
                return;
            }

            if (_sourceText != string.Empty)
            {
                Queue.Enqueue(
                    new ModelStreamPart { Type = "source-transcript-final", Text = _sourceText }
                );
            }

            if (_translationText != string.Empty)
            {
                Queue.Enqueue(
                    new ModelStreamPart { Type = "output-text-final", Text = _translationText }
                );
            }

            Queue.Enqueue(
                new ModelStreamPart
                {
                    Type = "finish",
                    SourceText = _sourceText,
                    OutputText = _translationText,
                }
            );
            Queue.Complete(null);
            Cleanup(1000);
        }

        private void OnOpen(IWebSocketLike socket)
        {
            Queue.Enqueue(new ModelStreamPart { Type = "stream-start", Warnings = _warnings });
            socket.Send(_sessionUpdate.ToJsonString());
            _ = SendAudioAsync(socket)
                .ContinueWith(
                    task => FinishWithError(task.Exception!.GetBaseException()),
                    TaskContinuationOptions.OnlyOnFaulted
                );
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
                            ["type"] = "session.input_audio_buffer.append",
                            ["audio"] = Convert.ToBase64String(bytes),
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

            if (!IsFinished)
            {
                socket.Send(new JsonObject { ["type"] = "session.close" }.ToJsonString());
            }
        }

        private Task OnMessageTextAsync(string text)
        {
            if (IsFinished)
            {
                return Task.CompletedTask;
            }

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

            var delta = OpenAIJson.String(root, "delta");
            switch (OpenAIJson.String(root, "type"))
            {
                case "session.output_audio.delta":
                    // Skip empty deltas: an empty `audio` part carries no data.
                    if (!string.IsNullOrEmpty(delta))
                    {
                        Queue.Enqueue(
                            new ModelStreamPart
                            {
                                Type = "audio",
                                Audio = Convert.FromBase64String(delta),
                            }
                        );
                    }

                    break;

                case "session.output_transcript.delta":
                    _translationText += delta ?? string.Empty;
                    Queue.Enqueue(
                        new ModelStreamPart
                        {
                            Type = "output-text-delta",
                            Delta = delta ?? string.Empty,
                        }
                    );
                    break;

                case "session.input_transcript.delta":
                    _sourceText += delta ?? string.Empty;
                    Queue.Enqueue(
                        new ModelStreamPart
                        {
                            Type = "source-transcript-delta",
                            Delta = delta ?? string.Empty,
                        }
                    );
                    break;

                case "session.closed":
                    Finish();
                    break;

                case "error":
                    Queue.Enqueue(
                        new ModelStreamPart
                        {
                            Type = "error",
                            Error = new AiSdkException(
                                OpenAIJson.String(OpenAIJson.Child(root, "error"), "message")
                                    ?? "OpenAI realtime error"
                            ),
                        }
                    );
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
