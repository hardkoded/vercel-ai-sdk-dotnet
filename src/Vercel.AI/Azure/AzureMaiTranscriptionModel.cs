// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Azure;

/// <summary>
/// MAI streaming transcription over the OpenAI Realtime-compatible <c>/mai/v1/realtime</c> WebSocket. The model ID is
/// the Foundry deployment name. Maps to <c>AzureMaiTranscriptionModel</c>.
/// </summary>
internal sealed class AzureMaiTranscriptionModel
{
    private static readonly int[] SupportedSampleRates = { 16000, 24000 };
    private const int DefaultSampleRate = 24000;

    private readonly Func<Uri> _url;
    private readonly Func<Dictionary<string, string?>> _headers;
    private readonly WebSocketFactory? _webSocket;
    private readonly WebSocketFactory? _nativeWebSocket;

    /// <summary>Creates a MAI transcription model.</summary>
    /// <param name="modelId">The Foundry deployment name.</param>
    /// <param name="url">The realtime endpoint.</param>
    /// <param name="headers">Provider headers, resolved for each stream.</param>
    /// <param name="webSocket">Custom WebSocket implementation. It can send headers. Maps to <c>webSocket</c>.</param>
    /// <param name="nativeWebSocket">
    /// The WebSocket used without a custom implementation. It cannot send headers, so the key moves to the query string.
    /// Defaults to <c>ClientWebSocket</c>. Maps to <c>globalThis.WebSocket</c>, which upstream tests stub.
    /// </param>
    public AzureMaiTranscriptionModel(
        string modelId,
        Func<Uri> url,
        Func<Dictionary<string, string?>> headers,
        WebSocketFactory? webSocket = null,
        WebSocketFactory? nativeWebSocket = null)
    {
        ModelId = modelId;
        _url = url;
        _headers = headers;
        _webSocket = webSocket;
        _nativeWebSocket = nativeWebSocket;
    }

    /// <summary>Provider id.</summary>
    public string Provider => "azure.transcription";

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>MAI models only stream. Maps to <c>doGenerate</c>.</summary>
    public Task<TranscriptionModelResult> DoGenerateAsync()
    {
        throw new UnsupportedFunctionalityException(
            "file transcription with " + ModelId,
            "'file transcription with " + ModelId + " (use streaming transcription)' functionality not supported.");
    }

    /// <summary>Starts a streaming transcription. Maps to <c>doStream</c>.</summary>
    public Task<TranscriptionStreamStart?> DoStreamAsync(
        TranscriptionStreamCall call,
        AzureTranscriptionModelMaiOptions maiOptions,
        IReadOnlyList<OperationWarning> warnings)
    {
        var timestamp = DateTime.UtcNow;
        var type = call.InputAudioFormat.Type;
        var rate = call.InputAudioFormat.Rate ?? DefaultSampleRate;
        if (type != "audio/pcm" || Array.IndexOf(SupportedSampleRates, rate) < 0)
        {
            throw new InvalidArgumentException(
                "inputAudioFormat",
                call.InputAudioFormat,
                "MAI streaming transcription requires 16-bit mono PCM (audio/pcm) at 16000 or 24000 Hz, got " + type
                + (call.InputAudioFormat.Rate != null ? " at " + rate.ToString(CultureInfo.InvariantCulture) + " Hz" : string.Empty) + ".");
        }

        var transcription = new JsonObject { ["model"] = ModelId };
        if (maiOptions.Language != null)
        {
            transcription["language"] = maiOptions.Language;
        }

        var sessionUpdate = new JsonObject
        {
            ["type"] = "session.update",
            ["session"] = new JsonObject
            {
                ["type"] = "transcription",
                ["audio"] = new JsonObject
                {
                    ["input"] = new JsonObject
                    {
                        ["format"] = new JsonObject { ["type"] = "audio/pcm", ["rate"] = rate },
                        ["transcription"] = transcription,
                        // MAI supports neither server VAD nor noise reduction; the stream commits once when the input audio ends.
                        ["turn_detection"] = null,
                        ["noise_reduction"] = null,
                    },
                },
            },
        };

        var headers = new Dictionary<string, string?>(_headers(), StringComparer.OrdinalIgnoreCase);
        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        // Native WebSocket constructors drop headers, so bearer auth would be lost.
        if (_webSocket == null && headers.Any(pair => string.Equals(pair.Key, "authorization", StringComparison.OrdinalIgnoreCase) && pair.Value != null))
        {
            throw new InvalidArgumentException(
                "webSocket",
                null,
                "MAI streaming transcription with Microsoft Entra ID (tokenProvider) or an Authorization header requires a custom webSocket implementation, because the native WebSocket cannot send headers.");
        }

        var connection = GetConnection(WebSocketUtils.ToWebSocketUrl(_url()), headers, _webSocket != null);
        var session = new Session(call, sessionUpdate, rate * 2, maiOptions.Language, warnings);
        session.Start(connection.Url, connection.Headers, _webSocket ?? _nativeWebSocket);
        return Task.FromResult<TranscriptionStreamStart?>(new TranscriptionStreamStart(
            new ModelPartStream(session.Queue, session.Cancel),
            new ProviderResponse(timestamp: timestamp, modelId: ModelId),
            new ProviderRequest(sessionUpdate.ToJsonString())));
    }

    // Native WebSocket constructors cannot send headers, so the key moves to the `api-key` query parameter that MAI also accepts.
    internal static (Uri Url, Dictionary<string, string?> Headers) GetConnection(Uri url, IReadOnlyDictionary<string, string?> headers, bool supportsHeaders)
    {
        if (supportsHeaders)
        {
            var all = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in headers)
            {
                all[pair.Key] = pair.Value;
            }

            return (url, all);
        }

        var remaining = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var builder = new UriBuilder(url);
        foreach (var pair in headers)
        {
            if (string.Equals(pair.Key, "api-key", StringComparison.OrdinalIgnoreCase))
            {
                if (pair.Value != null)
                {
                    var query = builder.Query.TrimStart('?')
                        .Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(part => !part.StartsWith("api-key=", StringComparison.Ordinal))
                        .ToList();
                    query.Add("api-key=" + Uri.EscapeDataString(pair.Value));
                    builder.Query = string.Join("&", query);
                }
            }
            else
            {
                remaining[pair.Key] = pair.Value;
            }
        }

        return (builder.Uri, remaining);
    }

    // Maps to `createMaiTranscriptionStream`.
    private sealed class Session
    {
        private readonly TranscriptionStreamCall _call;
        private readonly JsonObject _sessionUpdate;
        private readonly int _bytesPerSecond;
        private readonly string? _language;
        private readonly IReadOnlyList<OperationWarning> _warnings;
        private readonly object _gate = new object();
        private readonly System.Text.StringBuilder _deltas = new System.Text.StringBuilder();
        private WebSocketConnection? _connection;
        private AudioChunkReader? _reader;
        private bool _finished;
        private bool _audioStarted;
        private bool _committed;
        private long _audioBytes;

        public Session(TranscriptionStreamCall call, JsonObject sessionUpdate, int bytesPerSecond, string? language, IReadOnlyList<OperationWarning> warnings)
        {
            _call = call;
            _sessionUpdate = sessionUpdate;
            _bytesPerSecond = bytesPerSecond;
            _language = language;
            _warnings = warnings;
        }

        public PartQueue<ModelStreamPart> Queue { get; } = new PartQueue<ModelStreamPart>();

        public void Start(Uri url, IReadOnlyDictionary<string, string?> headers, WebSocketFactory? webSocket)
        {
            _connection = ConnectToWebSocket.Connect(new ConnectToWebSocketOptions
            {
                Url = url,
                Headers = headers,
                WebSocket = webSocket,
                AbortSignal = _call.CancellationToken,
                OnAbort = FinishWithError,
                OnProcessingError = FinishWithError,
                OnOpen = _ => Queue.Enqueue(new ModelStreamPart { Type = "stream-start", Warnings = _warnings }),
                OnMessageText = OnMessageTextAsync,
                OnSocketError = () => FinishWithError(new AiSdkException("Azure MAI transcription WebSocket error")),
                OnClose = (code, reason) => FinishWithError(new AiSdkException(
                    "Azure MAI transcription connection closed before the transcript completed"
                    + (code != null ? " (code " + code.Value.ToString(CultureInfo.InvariantCulture) + (string.IsNullOrEmpty(reason) ? string.Empty : ": " + reason) + ")" : string.Empty))),
            });
        }

        public void Cancel()
        {
            if (!TryFinish())
            {
                return;
            }

            Queue.Complete(null);
            Cleanup(null);
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
                if (reader != null)
                {
                    reader.Cancel();
                }
                else
                {
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

        private void Finish(string text)
        {
            if (!TryFinish())
            {
                return;
            }

            Queue.Enqueue(new ModelStreamPart
            {
                Type = "finish",
                Text = text,
                Segments = Array.Empty<TranscriptSegment>(),
                Language = _language,
                DurationInSeconds = Interlocked.Read(ref _audioBytes) > 0 ? (double)Interlocked.Read(ref _audioBytes) / _bytesPerSecond : null,
            });
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
                    var bytes = await reader.ReadAsync(_call.CancellationToken).ConfigureAwait(false);
                    if (bytes == null || IsFinished)
                    {
                        break;
                    }

                    if (bytes.Length == 0)
                    {
                        continue;
                    }

                    Interlocked.Add(ref _audioBytes, bytes.Length);
                    socket.Send(new JsonObject { ["type"] = "input_audio_buffer.append", ["audio"] = Convert.ToBase64String(bytes) }.ToJsonString());
                    await WebSocketUtils.WaitForWebSocketBufferDrainAsync(socket, cancellationToken: _call.CancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                reader.ReleaseLock();
                lock (_gate)
                {
                    _reader = null;
                }
            }

            if (IsFinished)
            {
                return;
            }

            if (Interlocked.Read(ref _audioBytes) == 0)
            {
                Finish(string.Empty);
                return;
            }

            lock (_gate)
            {
                _committed = true;
            }

            socket.Send(new JsonObject { ["type"] = "input_audio_buffer.commit" }.ToJsonString());
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

            var socket = _connection?.Socket;
            if (_call.IncludeRawChunks == true)
            {
                Queue.Enqueue(new ModelStreamPart { Type = "raw", Raw = root });
            }

            var id = ReadString(root, "item_id");
            switch (ReadString(root, "type"))
            {
                case "session.created":
                    socket?.Send(_sessionUpdate.ToJsonString());
                    break;

                // Settings are locked after the first append, so audio waits for the server to acknowledge the session configuration.
                case "session.updated":
                    lock (_gate)
                    {
                        if (_audioStarted || socket == null)
                        {
                            break;
                        }

                        _audioStarted = true;
                    }

                    _ = SendAudioAsync(socket).ContinueWith(task => FinishWithError(task.Exception!.GetBaseException()), TaskContinuationOptions.OnlyOnFaulted);
                    break;

                case "conversation.item.input_audio_transcription.delta":
                    var delta = ReadString(root, "delta") ?? string.Empty;
                    lock (_gate)
                    {
                        _deltas.Append(delta);
                    }

                    Queue.Enqueue(new ModelStreamPart { Type = "transcript-delta", Id = id, Delta = delta });
                    break;

                // MAI-specific: the provisional text after the latest delta.
                case "conversation.item.input_audio_transcription.intermediate":
                    Queue.Enqueue(new ModelStreamPart { Type = "transcript-partial", Id = id, Text = ReadString(root, "intermediate") ?? string.Empty });
                    break;

                case "conversation.item.input_audio_transcription.completed":
                    string accumulated;
                    bool committed;
                    lock (_gate)
                    {
                        accumulated = _deltas.ToString();
                        committed = _committed;
                    }

                    var transcript = ReadString(root, "transcript") ?? accumulated;
                    Queue.Enqueue(new ModelStreamPart { Type = "transcript-final", Id = id, Text = transcript });
                    if (committed)
                    {
                        Finish(transcript);
                    }

                    break;

                case "conversation.item.input_audio_transcription.failed":
                case "error":
                    root.TryGetProperty("error", out var error);
                    var code = ReadString(error, "code");
                    FinishWithError(new AiSdkException(
                        "Azure MAI transcription error" + (code != null ? " (" + code + ")" : string.Empty) + ": " + (ReadString(error, "message") ?? "unknown error")));
                    break;
            }

            return Task.CompletedTask;
        }

        private static string? ReadString(JsonElement element, string name)
        {
            return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }
    }
}
