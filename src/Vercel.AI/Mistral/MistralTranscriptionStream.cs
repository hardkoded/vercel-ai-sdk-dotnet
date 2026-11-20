// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Mistral;

/// <summary>Mistral realtime transcription over WebSocket. Maps to <c>mistral-transcription-stream.ts</c>.</summary>
internal static class MistralTranscriptionStream
{
    private const int ChunkSize = 64 * 1024;

    private static readonly JsonSerializerOptions SendOptions = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static readonly HashSet<string> HandledEvents = new HashSet<string>(
        StringComparer.Ordinal
    )
    {
        "session.created",
        "session.updated",
        "transcription.text.delta",
        "transcription.segment",
        "transcription.done",
        "error",
    };

    /// <summary>Maps to <c>streamMistralTranscription</c>.</summary>
    public static TranscriptionStreamStart StreamMistralTranscription(
        TranscriptionStreamCall call,
        string modelId,
        string baseUrl,
        IReadOnlyDictionary<string, string?> headers,
        WebSocketFactory? webSocket,
        DateTime currentDate
    )
    {
        var mistralOptions = MistralTranscriptionModelOptions.Parse(call.ProviderOptions);
        var type = call.InputAudioFormat.Type;
        var rate = call.InputAudioFormat.Rate ?? 16000;
        if (type != "audio/pcm" || rate != Math.Floor(rate) || rate <= 0)
        {
            throw new InvalidArgumentException(
                "inputAudioFormat",
                null,
                "Mistral realtime transcription requires audio/pcm (mono, signed 16-bit little-endian) with a positive integer sample rate."
            );
        }

        var warnings = new List<OperationWarning>();
        foreach (
            var (option, present) in new (string, bool)[]
            {
                ("returnLanguage", mistralOptions?.ReturnLanguage is not null),
                ("temperature", mistralOptions?.Temperature is not null),
                ("timestampGranularities", mistralOptions?.TimestampGranularities is not null),
            }
        )
        {
            if (present)
            {
                warnings.Add(
                    new OperationWarning(
                        "unsupported",
                        feature: "providerOptions.mistral." + option,
                        details: "This option is not supported for realtime transcription."
                    )
                );
            }
        }

        var session = new JsonObject
        {
            ["audio_format"] = new JsonObject
            {
                ["encoding"] = "pcm_s16le",
                ["sample_rate"] = (long)rate,
            },
        };
        if (mistralOptions?.TargetStreamingDelayMs is { } delay)
        {
            session["target_streaming_delay_ms"] = delay;
        }

        if (mistralOptions?.Language is { } language)
        {
            session["language"] = language;
        }

        if (mistralOptions?.ContextBias is { } contextBias)
        {
            session["context_bias"] = new JsonArray(
                contextBias.Select(entry => (JsonNode?)JsonValue.Create(entry)).ToArray()
            );
        }

        if (mistralOptions?.Diarize is { } diarize)
        {
            session["diarize"] = diarize;
        }

        if (mistralOptions?.SpeakerReferences is { } speakerReferences)
        {
            var references = new JsonObject();
            foreach (var pair in speakerReferences)
            {
                references[pair.Key] = pair.Value;
            }

            session["speaker_references"] = references;
        }

        if (mistralOptions?.TurnDetection is { } turnDetection)
        {
            var detection = new JsonObject();
            if (turnDetection.StartThreshold is { } start)
            {
                detection["start_threshold"] = start;
            }

            if (turnDetection.EagerEndThreshold is { } eager)
            {
                detection["eager_end_threshold"] = eager;
            }

            if (turnDetection.EndThreshold is { } end)
            {
                detection["end_threshold"] = end;
            }

            if (turnDetection.MinSilenceMs is { } minSilence)
            {
                detection["min_silence_ms"] = minSilence;
            }

            if (turnDetection.EndTimeoutMs is { } endTimeout)
            {
                detection["end_timeout_ms"] = endTimeout;
            }

            session["turn_detection"] = detection;
        }

        var url = WebSocketUtils.ToWebSocketUrl(
            ApiKeys.Combine(baseUrl, "audio/transcriptions/realtime")
        );
        url = new UriBuilder(url) { Query = "model=" + Uri.EscapeDataString(modelId) }.Uri;

        var stream = new Session(call, modelId, session, warnings);
        stream.Start(url, headers, webSocket);
        return new TranscriptionStreamStart(
            new ModelPartStream(stream.Queue, stream.Cancel),
            new ProviderResponse(timestamp: currentDate, modelId: modelId),
            new ProviderRequest(url.AbsoluteUri)
        );
    }

    private static string Serialize(JsonObject value) => value.ToJsonString(SendOptions);

    private sealed class Session
    {
        private readonly TranscriptionStreamCall _call;
        private readonly string _modelId;
        private readonly JsonObject _session;
        private readonly IReadOnlyList<OperationWarning> _warnings;
        private readonly object _gate = new object();
        private readonly CancellationTokenSource _stop = new CancellationTokenSource();
        private WebSocketConnection? _connection;
        private AudioChunkReader? _reader;
        private bool _finished;
        private bool _configured;
        private bool _sending;

        public Session(
            TranscriptionStreamCall call,
            string modelId,
            JsonObject session,
            IReadOnlyList<OperationWarning> warnings
        )
        {
            _call = call;
            _modelId = modelId;
            _session = session;
            _warnings = warnings;
        }

        public PartQueue<ModelStreamPart> Queue { get; } = new PartQueue<ModelStreamPart>();

        public void Start(
            Uri url,
            IReadOnlyDictionary<string, string?> headers,
            WebSocketFactory? webSocket
        )
        {
            Queue.Enqueue(new ModelStreamPart { Type = "stream-start", Warnings = _warnings });
            _connection = ConnectToWebSocket.Connect(
                new ConnectToWebSocketOptions
                {
                    Url = url,
                    Headers = headers,
                    WebSocket = webSocket,
                    AbortSignal = _call.CancellationToken,
                    AbortReason = _call.AbortReason,
                    OnAbort = Fail,
                    OnProcessingError = Fail,
                    OnMessageText = OnMessageTextAsync,
                    OnSocketError = () =>
                        Fail(
                            new AiSdkException(
                                "Mistral realtime transcription WebSocket error."
                                    + (
                                        webSocket is null
                                            ? " Pass a header-capable WebSocket implementation via MistralOptions.WebSocket to send the Authorization header."
                                            : string.Empty
                                    )
                            )
                        ),
                    OnClose = (code, reason) =>
                        Fail(
                            new AiSdkException(
                                "Mistral realtime transcription connection closed before transcription.done (code: "
                                    + (code?.ToString() ?? "unknown")
                                    + (
                                        string.IsNullOrEmpty(reason)
                                            ? string.Empty
                                            : ", reason: " + reason
                                    )
                                    + ")."
                            )
                        ),
                }
            );

            // A constructor failure or an already-aborted signal can finish synchronously.
            if (IsFinished)
            {
                _connection.Close();
            }
        }

        public void Cancel()
        {
            if (!TryFinish())
            {
                return;
            }

            Cleanup();
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

        private void Cleanup()
        {
            AudioChunkReader? reader;
            lock (_gate)
            {
                reader = _reader;
            }

            _stop.Cancel();
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

            _connection?.Close(1000);
        }

        private void Fail(Exception error)
        {
            if (!TryFinish())
            {
                return;
            }

            Cleanup();
            Queue.Complete(error);
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
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(
                    _call.CancellationToken,
                    _stop.Token
                );
                while (!IsFinished)
                {
                    var bytes = await reader.ReadAsync(linked.Token).ConfigureAwait(false);
                    if (bytes is null || IsFinished)
                    {
                        break;
                    }

                    // Keep each message below Mistral's 256 KiB limit, including base64 expansion.
                    for (var offset = 0; offset < bytes.Length; offset += ChunkSize)
                    {
                        if (IsFinished)
                        {
                            break;
                        }

                        socket.Send(
                            Serialize(
                                new JsonObject
                                {
                                    ["type"] = "input_audio.append",
                                    ["audio"] = Convert.ToBase64String(
                                        bytes,
                                        offset,
                                        Math.Min(ChunkSize, bytes.Length - offset)
                                    ),
                                }
                            )
                        );
                        await WebSocketUtils
                            .WaitForWebSocketBufferDrainAsync(
                                socket,
                                cancellationToken: linked.Token
                            )
                            .ConfigureAwait(false);
                    }
                }

                if (!IsFinished)
                {
                    socket.Send(Serialize(new JsonObject { ["type"] = "input_audio.end" }));
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
        }

        private Task OnMessageTextAsync(string text)
        {
            if (IsFinished)
            {
                return Task.CompletedTask;
            }

            using var document = JsonDocument.Parse(text);
            var raw = document.RootElement.Clone();
            if (
                raw.ValueKind != JsonValueKind.Object
                || !raw.TryGetProperty("type", out var typeElement)
                || typeElement.ValueKind != JsonValueKind.String
            )
            {
                throw new AiSdkException("Invalid Mistral realtime transcription event.");
            }

            if (_call.IncludeRawChunks == true)
            {
                Queue.Enqueue(new ModelStreamPart { Type = "raw", Raw = raw });
            }

            // Unknown events (including turn events) remain available as raw chunks.
            var type = typeElement.GetString()!;
            if (!HandledEvents.Contains(type))
            {
                return Task.CompletedTask;
            }

            switch (type)
            {
                case "session.created":
                    lock (_gate)
                    {
                        if (_configured)
                        {
                            break;
                        }

                        _configured = true;
                    }

                    _connection!.Socket!.Send(
                        Serialize(
                            new JsonObject
                            {
                                ["type"] = "session.update",
                                ["session"] = _session.DeepClone(),
                            }
                        )
                    );
                    break;

                case "session.updated":
                    lock (_gate)
                    {
                        if (!_configured || _sending)
                        {
                            break;
                        }

                        _sending = true;
                    }

                    var socket = _connection!.Socket!;
                    _ = SendAudioAsync(socket)
                        .ContinueWith(
                            task => Fail(task.Exception!.GetBaseException()),
                            TaskContinuationOptions.OnlyOnFaulted
                        );
                    break;

                case "transcription.text.delta":
                    Queue.Enqueue(
                        new ModelStreamPart
                        {
                            Type = "transcript-delta",
                            Delta = RequireString(raw, "text"),
                        }
                    );
                    break;

                case "transcription.segment":
                    OnSegment(raw);
                    break;

                case "transcription.done":
                    OnDone(raw);
                    break;

                case "error":
                    if (
                        !raw.TryGetProperty("error", out var error)
                        || error.ValueKind != JsonValueKind.Object
                    )
                    {
                        throw new AiSdkException("Invalid Mistral realtime transcription event.");
                    }

                    Fail(new AiSdkException(RequireString(error, "message")));
                    break;
            }

            return Task.CompletedTask;
        }

        private void OnSegment(JsonElement raw)
        {
            var segmentText = RequireString(raw, "text");
            var start = OptionalNumber(raw, "start");
            var end = OptionalNumber(raw, "end");
            var speakerId = OptionalString(raw, "speaker_id");
            Queue.Enqueue(
                new ModelStreamPart
                {
                    Type = "transcript-final",
                    Text = segmentText,
                    StartSecond = start,
                    EndSecond = end,
                    ProviderMetadata = speakerId is null
                        ? null
                        : JsonSerializer.SerializeToElement(
                            new JsonObject
                            {
                                ["mistral"] = new JsonObject { ["speakerId"] = speakerId },
                            }
                        ),
                }
            );
        }

        private void OnDone(JsonElement raw)
        {
            var doneText = RequireString(raw, "text");
            var model = OptionalString(raw, "model");
            var language = OptionalString(raw, "language");
            var segments = new List<(string Text, double? Start, double? End, string? Speaker)>();
            if (
                raw.TryGetProperty("segments", out var segmentItems)
                && segmentItems.ValueKind != JsonValueKind.Null
            )
            {
                if (segmentItems.ValueKind != JsonValueKind.Array)
                {
                    throw new AiSdkException("Invalid Mistral realtime transcription event.");
                }

                foreach (var item in segmentItems.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object)
                    {
                        throw new AiSdkException("Invalid Mistral realtime transcription event.");
                    }

                    segments.Add(
                        (
                            RequireString(item, "text"),
                            OptionalNumber(item, "start"),
                            OptionalNumber(item, "end"),
                            OptionalString(item, "speaker_id")
                        )
                    );
                }
            }

            JsonElement? usage = null;
            if (
                raw.TryGetProperty("usage", out var usageElement)
                && usageElement.ValueKind != JsonValueKind.Null
            )
            {
                if (usageElement.ValueKind != JsonValueKind.Object)
                {
                    throw new AiSdkException("Invalid Mistral realtime transcription event.");
                }

                usage = usageElement;
            }

            double? duration =
                usage is { } usageValue
                && usageValue.TryGetProperty("prompt_audio_seconds", out var seconds)
                && seconds.ValueKind == JsonValueKind.Number
                    ? seconds.GetDouble()
                    : null;

            var metadata = new JsonObject();
            if (usage is { } rawUsage)
            {
                metadata["usage"] = JsonNode.Parse(rawUsage.GetRawText());
            }

            var metadataSegments = new JsonArray();
            foreach (var segment in segments)
            {
                var entry = new JsonObject { ["text"] = segment.Text };
                if (segment.Start is { } startSecond)
                {
                    entry["startSecond"] = startSecond;
                }

                if (segment.End is { } endSecond)
                {
                    entry["endSecond"] = endSecond;
                }

                if (segment.Speaker is { } speaker)
                {
                    entry["speakerId"] = speaker;
                }

                metadataSegments.Add(entry);
            }

            metadata["segments"] = metadataSegments;

            if (!TryFinish())
            {
                return;
            }

            Queue.Enqueue(
                new ModelStreamPart
                {
                    Type = "response-metadata",
                    ModelId = model ?? _modelId,
                    Body = raw,
                }
            );
            Queue.Enqueue(
                new ModelStreamPart
                {
                    Type = "finish",
                    Text = doneText,
                    Segments = segments
                        .Where(segment => segment.Start is not null && segment.End is not null)
                        .Select(segment => new TranscriptSegment(
                            segment.Text,
                            segment.Start!.Value,
                            segment.End!.Value
                        ))
                        .ToList(),
                    Language = language,
                    DurationInSeconds = duration,
                    UsageValue = usage,
                    ProviderMetadata = JsonSerializer.SerializeToElement(
                        new JsonObject { ["mistral"] = metadata }
                    ),
                }
            );
            Queue.Complete(null);
            Cleanup();
        }

        private static string RequireString(JsonElement element, string name)
        {
            if (
                !element.TryGetProperty(name, out var value)
                || value.ValueKind != JsonValueKind.String
            )
            {
                throw new AiSdkException("Invalid Mistral realtime transcription event.");
            }

            return value.GetString()!;
        }

        private static string? OptionalString(JsonElement element, string name)
        {
            if (
                !element.TryGetProperty(name, out var value)
                || value.ValueKind == JsonValueKind.Null
            )
            {
                return null;
            }

            if (value.ValueKind != JsonValueKind.String)
            {
                throw new AiSdkException("Invalid Mistral realtime transcription event.");
            }

            return value.GetString();
        }

        private static double? OptionalNumber(JsonElement element, string name)
        {
            if (
                !element.TryGetProperty(name, out var value)
                || value.ValueKind == JsonValueKind.Null
            )
            {
                return null;
            }

            if (value.ValueKind != JsonValueKind.Number)
            {
                throw new AiSdkException("Invalid Mistral realtime transcription event.");
            }

            return value.GetDouble();
        }
    }
}
