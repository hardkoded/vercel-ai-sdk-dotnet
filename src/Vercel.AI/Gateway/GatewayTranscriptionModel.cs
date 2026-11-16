// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Gateway;

/// <summary>
/// Gateway transcription model, routed through <c>/transcription-model</c>. Maps to <c>GatewayTranscriptionModel</c>.
/// Streaming transcription runs over the <c>/transcription-model</c> WebSocket.
/// </summary>
public sealed class GatewayTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
{
    private readonly GatewayProvider _provider;

    /// <summary>Creates a Gateway transcription model.</summary>
    /// <param name="provider">Gateway provider that sends the requests.</param>
    /// <param name="modelId">Model id.</param>
    /// <param name="providerName">Value of <see cref="Provider"/> (upstream <c>GatewayConfig.provider</c>). Defaults to <c>gateway</c>.</param>
    public GatewayTranscriptionModel(GatewayProvider provider, string modelId, string? providerName = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        Provider = providerName ?? GatewayProvider.ProviderId;
    }

    /// <inheritdoc />
    public string Provider { get; }

    /// <summary>Resolves the config headers of a streaming call. Maps to <c>config.headers</c>. Defaults to the provider's auth headers.</summary>
    internal Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>>? HeadersResolver { get; set; }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public bool CanStream => true;

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        audio = audio ?? throw new ArgumentNullException(nameof(audio));
        var result = await DoGenerateAsync(new TranscriptionModelCall(audio.Data, string.IsNullOrEmpty(audio.MediaType) ? "audio/wav" : audio.MediaType, OperationJson.Parse("{}"), new Dictionary<string, string>(), cancellationToken), cancellationToken).ConfigureAwait(false);
        return new TranscriptionResult(result.Text ?? string.Empty);
    }

    /// <inheritdoc />
    public async Task<TranscriptionModelResult> DoGenerateAsync(TranscriptionModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var body = new JsonObject
        {
            ["audio"] = ByteEncoding.ToBase64(call.Audio),
            ["mediaType"] = call.MediaType,
        };
        if (call.ProviderOptions.ValueKind == JsonValueKind.Object)
        {
            body["providerOptions"] = JsonNode.Parse(call.ProviderOptions.GetRawText());
        }

        var headers = new Dictionary<string, string?>();
        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        ProviderTextResponse response;
        try
        {
            response = await _provider.Http.SendJsonStringAsync(
                HttpMethod.Post,
                _provider.Route("transcription-model"),
                body.ToJsonString(),
                _provider.Headers("ai-transcription-model-specification-version", "4", "ai-model-id", ModelId, null, headers),
                cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException exception)
        {
            throw GatewayProvider.MapFailure(exception);
        }
        catch (ApiTimeoutException exception)
        {
            throw GatewayProvider.MapFailure(exception);
        }

        JsonElement root;
        try
        {
            using var document = JsonDocument.Parse(response.Body);
            root = document.RootElement.Clone();
        }
        catch (JsonException)
        {
            throw new InvalidResponseDataException(response.Body, "Invalid Gateway transcription response: the body is not valid JSON.");
        }

        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("text", out var textElement) || textElement.ValueKind != JsonValueKind.String)
        {
            throw new InvalidResponseDataException(response.Body, "Invalid Gateway transcription response: expected text.");
        }

        var segments = ReadSegments(root, "segments", response.Body) ?? new List<TranscriptSegment>();
        var words = ReadSegments(root, "words", response.Body);
        var warnings = new List<OperationWarning>();
        if (root.TryGetProperty("warnings", out var warningArray) && warningArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var warning in warningArray.EnumerateArray())
            {
                warnings.Add(GatewayImageModel.ReadWarning(warning));
            }
        }

        if (call.TimestampGranularities is { Count: > 0 })
        {
            warnings.Add(new OperationWarning(
                "unsupported",
                feature: "timestampGranularities",
                details: "The AI Gateway adapter does not forward the shared timestampGranularities option."));
        }

        if (IsXaiTranscriptionModel(ModelId) && RequestsXaiDiarization(call.ProviderOptions) && !ContainsSpeaker(root))
        {
            warnings.Add(new OperationWarning(
                "unsupported",
                feature: "providerOptions.xai.diarize",
                details: "AI Gateway does not currently expose xAI speaker diarization metadata."));
        }

        JsonElement? usage = root.TryGetProperty("usage", out var usageElement) && usageElement.ValueKind == JsonValueKind.Object ? usageElement.Clone() : null;
        JsonElement? providerMetadata = root.TryGetProperty("providerMetadata", out var metadata) && metadata.ValueKind == JsonValueKind.Object ? metadata.Clone() : null;
        return new TranscriptionModelResult(
            textElement.GetString(),
            segments,
            root.TryGetProperty("language", out var language) && language.ValueKind == JsonValueKind.String ? language.GetString() : null,
            root.TryGetProperty("durationInSeconds", out var duration) && duration.ValueKind == JsonValueKind.Number ? duration.GetDouble() : null,
            warnings,
            providerMetadata,
            new ProviderResponse(response.Headers, root, timestamp: DateTime.UtcNow, modelId: ModelId),
            words,
            usage);
    }

    /// <inheritdoc />
    public async Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var currentDate = _provider.Options.CurrentDate?.Invoke().UtcDateTime ?? DateTime.UtcNow;

        var headers = new Dictionary<string, string?>(StringComparer.Ordinal);
        var configHeaders = HeadersResolver is not null
            ? await HeadersResolver(cancellationToken).ConfigureAwait(false)
            : await _provider.GetHeadersAsync(cancellationToken).ConfigureAwait(false);
        foreach (var pair in configHeaders)
        {
            headers[pair.Key] = pair.Value;
        }

        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        headers["ai-transcription-model-specification-version"] = "4";
        headers["ai-model-id"] = ModelId;
        foreach (var pair in _provider.O11yHeaders())
        {
            headers[pair.Key] = pair.Value;
        }

        var authMethod = GatewayErrors.ParseAuthMethod(headers);

        // First frame after the WebSocket opens, per the transcription-stream envelope. Optional keys are omitted so
        // the frame stays minimal.
        var startFrame = new TranscriptionStreamStartFrame(
            call.InputAudioFormat,
            call.ProviderOptions.ValueKind == JsonValueKind.Object ? call.ProviderOptions : null,
            call.IncludeRawChunks);

        var startFrameJson = startFrame.ToJson();
        var session = new Session(call, authMethod);
        session.Start(
            new Uri(ToGatewayTranscriptionUrl(_provider.Options.BaseUrl, ModelId)),
            GetProtocolsFromHeaders(headers),
            headers,
            startFrameJson,
            _provider.Options.WebSocket);
        return new TranscriptionStreamStart(
            new ModelPartStream(session.Queue, session.Cancel),
            new ProviderResponse(timestamp: currentDate, modelId: ModelId),
            new ProviderRequest(startFrameJson));
    }

    /// <summary>
    /// Gateway streaming transcription WebSocket URL: the HTTP(S) base upgraded to WS(S), with the model id in
    /// <c>?ai-model-id=</c> (slash-safe for qualified ids like <c>openai/gpt-realtime-whisper</c>). Maps to <c>toGatewayTranscriptionUrl</c>.
    /// </summary>
    public static string ToGatewayTranscriptionUrl(string baseUrl, string modelId)
    {
        var trimmed = baseUrl.TrimEnd('/');
        var wsBase = trimmed.StartsWith("http", StringComparison.Ordinal) ? "ws" + trimmed.Substring(4) : trimmed;
        return new Uri(wsBase + "/transcription-model").AbsoluteUri + "?ai-model-id=" + GatewayRealtimeModel.FormEncode(modelId);
    }

    /// <summary>
    /// Auth-carrying subprotocols from the resolved request headers (bearer token plus optional team scope). A
    /// browser <c>WebSocket</c> cannot send headers, so auth rides the <c>Sec-WebSocket-Protocol</c> handshake. Header
    /// lookups are case-insensitive. Maps to <c>getProtocolsFromHeaders</c>.
    /// </summary>
    public static IReadOnlyList<string> GetProtocolsFromHeaders(
        IReadOnlyDictionary<string, string?> headers,
        string marker = GatewayRealtimeAuth.GatewayTranscriptionSubprotocol,
        Func<string, string?, IReadOnlyList<string>>? getProtocols = null)
    {
        var normalized = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in headers)
        {
            normalized[pair.Key] = pair.Value;
        }

        normalized.TryGetValue("authorization", out var authorization);
        var token = authorization is not null && authorization.StartsWith("Bearer ", StringComparison.Ordinal)
            ? authorization.Substring("Bearer ".Length)
            : null;
        if (token is null)
        {
            return new[] { marker };
        }

        normalized.TryGetValue(GatewayErrors.TeamHeader, out var team);
        return (getProtocols ?? GatewayRealtimeAuth.GetGatewayTranscriptionProtocols)(token, team);
    }

    private static List<TranscriptSegment>? ReadSegments(JsonElement root, string name, string body)
    {
        if (!root.TryGetProperty(name, out var items) || items.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (items.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidResponseDataException(body, "Invalid Gateway transcription response: " + name + " must be an array.");
        }

        var result = new List<TranscriptSegment>();
        foreach (var item in items.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object
                || !item.TryGetProperty("text", out var text) || text.ValueKind != JsonValueKind.String
                || !item.TryGetProperty("startSecond", out var start) || start.ValueKind != JsonValueKind.Number
                || !item.TryGetProperty("endSecond", out var end) || end.ValueKind != JsonValueKind.Number)
            {
                throw new InvalidResponseDataException(body, "Invalid Gateway transcription response: an entry of " + name + " is malformed.");
            }

            result.Add(new TranscriptSegment(text.GetString()!, start.GetDouble(), end.GetDouble()));
        }

        return result;
    }

    private static bool IsXaiTranscriptionModel(string modelId)
    {
        return modelId.StartsWith("spacexai/", StringComparison.Ordinal) || modelId.StartsWith("xai/", StringComparison.Ordinal);
    }

    private static bool RequestsXaiDiarization(JsonElement providerOptions)
    {
        if (providerOptions.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        foreach (var name in new[] { "xai", "spacexai" })
        {
            if (providerOptions.TryGetProperty(name, out var options)
                && options.ValueKind == JsonValueKind.Object
                && options.TryGetProperty("diarize", out var diarize)
                && diarize.ValueKind == JsonValueKind.True)
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsSpeaker(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Array:
                return value.EnumerateArray().Any(ContainsSpeaker);
            case JsonValueKind.Object:
                return value.EnumerateObject().Any(property =>
                    (property.Name == "speaker" && property.Value.ValueKind != JsonValueKind.Null)
                    || ContainsSpeaker(property.Value));
            default:
                return false;
        }
    }

    /// <summary>Audio frames stay under server frame-size limits (envelope rule 7).</summary>
    private const int MaxAudioFrameBytes = 64 * 1024;

    // Maps to `createGatewayTranscriptionStream`.
    private sealed class Session
    {
        private readonly TranscriptionStreamCall _call;
        private readonly string? _authMethod;
        private readonly object _gate = new object();
        private WebSocketConnection? _connection;
        private AudioChunkReader? _reader;
        private bool _finished;
        private bool _hasServerErrorPart;
        private JsonElement? _lastServerError;
        private bool _audioStopped;

        public Session(TranscriptionStreamCall call, string? authMethod)
        {
            _call = call;
            _authMethod = authMethod;
        }

        public PartQueue<ModelStreamPart> Queue { get; } = new PartQueue<ModelStreamPart>();

        public void Start(Uri url, IReadOnlyList<string> protocols, IReadOnlyDictionary<string, string?> headers, string startFrame, WebSocketFactory? webSocket)
        {
            _connection = ConnectToWebSocket.Connect(new ConnectToWebSocketOptions
            {
                Url = url,
                Protocols = protocols,
                Headers = headers,
                WebSocket = webSocket,
                AbortSignal = _call.CancellationToken,
                AbortReason = _call.AbortReason,
                OnAbort = FinishWithAbort,
                OnProcessingError = FinishWithError,
                OnOpen = socket =>
                {
                    socket.Send(startFrame);
                    _ = SendAudioOrFailAsync(socket);
                },
                // Server frames are envelope-serialized stream parts; the codec handles parsing, unknown-part skipping, and timestamp revival.
                OnMessageText = OnMessageTextAsync,
                OnSocketError = () => FinishWithError(new AiSdkException("Connection error on AI Gateway transcription stream")),
                OnClose = (_, _) =>
                {
                    bool hasServerErrorPart;
                    JsonElement? lastServerError;
                    lock (_gate)
                    {
                        hasServerErrorPart = _hasServerErrorPart;
                        lastServerError = _lastServerError;
                    }

                    if (hasServerErrorPart)
                    {
                        if (IsFinished)
                        {
                            return;
                        }

                        FinishWithError(GatewayStreamErrors.CreateErrorFromServerErrorPart(lastServerError, _authMethod));
                        return;
                    }

                    FinishWithError(new AiSdkException("AI Gateway transcription stream closed before a finish part was received"));
                },
            });
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
            CancelAudio();
            _connection?.Close(closeCode);
        }

        // Cancels the audio reader, or the caller's audio stream when `SendAudioAsync` never took a reader (pre-open
        // failure or abort), so an upstream producer piping into it does not hang.
        private void CancelAudio()
        {
            AudioChunkReader? reader;
            lock (_gate)
            {
                reader = _reader;
                _reader = null;
            }

            try
            {
                if (reader is not null)
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
        }

        private void StopAudio()
        {
            lock (_gate)
            {
                _audioStopped = true;
            }

            CancelAudio();
        }

        private void FinishWithAbort(Exception reason)
        {
            if (!TryFinish())
            {
                return;
            }

            Cleanup(null);
            Queue.Complete(reason);
        }

        private void FinishWithError(Exception error)
        {
            if (!TryFinish())
            {
                return;
            }

            Cleanup(null);
            GatewayStreamErrors.ErrorControllerWithGatewayError(Queue, error, _authMethod);
        }

        // A cancelled audio stream surfaces as a canceled task, so the failure is caught here, not in a continuation.
        private async Task SendAudioOrFailAsync(IWebSocketLike socket)
        {
            try
            {
                await SendAudioAsync(socket).ConfigureAwait(false);
            }
            catch (Exception error)
            {
                // The abort signal also cancels the pending audio read; the stream fails with the abort reason, as for `onAbort`.
                if (error is OperationCanceledException && _call.CancellationToken.IsCancellationRequested)
                {
                    FinishWithAbort(_call.AbortReason ?? error);
                    return;
                }

                FinishWithError(error);
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
                    var bytes = await reader.ReadAsync(_call.CancellationToken).ConfigureAwait(false);
                    if (bytes is null || IsFinished)
                    {
                        break;
                    }

                    // Binary frames. Caller chunks are split to stay under server frame-size limits (envelope rule 7),
                    // with backpressure between frames.
                    for (var offset = 0; offset < bytes.Length; offset += MaxAudioFrameBytes)
                    {
                        if (IsFinished)
                        {
                            break;
                        }

                        var length = Math.Min(MaxAudioFrameBytes, bytes.Length - offset);
                        var frame = new byte[length];
                        Buffer.BlockCopy(bytes, offset, frame, 0, length);
                        socket.Send(frame);
                        await WebSocketUtils.WaitForWebSocketBufferDrainAsync(socket, cancellationToken: _call.CancellationToken).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                reader.ReleaseLock();
                lock (_gate)
                {
                    // Unlocked again: cleanup must cancel the audio stream, not the reader.
                    if (ReferenceEquals(_reader, reader))
                    {
                        _reader = null;
                    }
                }
            }

            bool audioStopped;
            lock (_gate)
            {
                audioStopped = _audioStopped;
            }

            if (!IsFinished && !audioStopped)
            {
                socket.Send(new JsonObject { ["type"] = TranscriptionStreamEnvelope.TranscriptionStreamAudioDoneFrameType }.ToJsonString());
            }
        }

        private Task OnMessageTextAsync(string text)
        {
            if (IsFinished)
            {
                return Task.CompletedTask;
            }

            var part = TranscriptionStreamEnvelope.ParseTranscriptionStreamPart(text);
            if (part is null)
            {
                return Task.CompletedTask;
            }

            if (part.Type == "finish")
            {
                if (!TryFinish())
                {
                    return Task.CompletedTask;
                }

                Queue.Enqueue(part);
                Queue.Complete(null);
                Cleanup(1000);
                return Task.CompletedTask;
            }

            if (part.Type == "error")
            {
                // Remembered so the terminal close error (the server closes non-1000 after an error part) surfaces the
                // server's message to promise-based consumers.
                lock (_gate)
                {
                    _hasServerErrorPart = true;
                    _lastServerError = part.ErrorValue;
                }

                // Envelope rule 5: error parts are terminal. Stop sending audio while the server holds the connection
                // open (for example for its final billing flush).
                StopAudio();
            }

            Queue.Enqueue(part);
            return Task.CompletedTask;
        }
    }
}
