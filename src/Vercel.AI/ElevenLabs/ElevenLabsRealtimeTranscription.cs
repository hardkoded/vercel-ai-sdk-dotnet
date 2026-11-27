// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.ElevenLabs;

/// <summary>
/// Scribe v2 Realtime over WebSocket. Maps to the module-level functions <c>getElevenLabsRealtimeAudioFormat</c>,
/// <c>buildElevenLabsRealtimeTranscriptionUrl</c> and <c>createElevenLabsRealtimeTranscriptionStream</c> of
/// <c>elevenlabs-transcription-model.ts</c>. They live in their own file because the port's transcription model is nested
/// in <c>ElevenLabsProvider.cs</c>.
/// </summary>
internal static class ElevenLabsRealtimeTranscription
{
    private static readonly HashSet<string> RealtimeErrorTypes = new HashSet<string>
    {
        "auth_error",
        "chunk_size_exceeded",
        "commit_throttled",
        "error",
        "input_error",
        "insufficient_audio_activity",
        "queue_overflow",
        "quota_exceeded",
        "rate_limited",
        "resource_exhausted",
        "session_time_limit_exceeded",
        "transcriber_error",
        "unaccepted_terms",
    };

    private static readonly HashSet<string> LateFinalizationErrorTypes = new HashSet<string>
    {
        "commit_throttled",
        "input_error",
        "insufficient_audio_activity",
    };

    private static readonly double[] SupportedPcmRates =
    {
        8000,
        16000,
        22050,
        24000,
        44100,
        48000,
    };

    private const int FinalCommitGracePeriodMs = 250;

    /// <summary>Maps to <c>getElevenLabsRealtimeAudioFormat</c>.</summary>
    internal static (string AudioFormat, double SampleRate) GetElevenLabsRealtimeAudioFormat(
        AudioFormat inputAudioFormat
    )
    {
        var type = inputAudioFormat.Type.ToLowerInvariant();
        var rate = inputAudioFormat.Rate;

        if (type == "audio/pcmu")
        {
            if (rate is not null && rate != 8000)
            {
                throw new InvalidArgumentException(
                    "inputAudioFormat",
                    null,
                    "ElevenLabs only supports audio/pcmu at 8000 Hz"
                );
            }

            return ("ulaw_8000", 8000);
        }

        var pcmRate = rate ?? 16000;
        if (type != "audio/pcm" || System.Array.IndexOf(SupportedPcmRates, pcmRate) < 0)
        {
            throw new InvalidArgumentException(
                "inputAudioFormat",
                null,
                "ElevenLabs realtime transcription supports audio/pcm at 8000, 16000, 22050, 24000, 44100, or 48000 Hz, and audio/pcmu at 8000 Hz"
            );
        }

        return ("pcm_" + Number(pcmRate), pcmRate);
    }

    /// <summary>Maps to <c>buildElevenLabsRealtimeTranscriptionUrl</c>.</summary>
    internal static Uri BuildElevenLabsRealtimeTranscriptionUrl(
        Uri baseUrl,
        string inputFormat,
        string? languageCode,
        string modelId,
        JsonElement? streamingOptions
    )
    {
        var includeTimestamps = Bool(streamingOptions, "includeTimestamps");
        var includeDetailedCommit =
            includeTimestamps == true || Bool(streamingOptions, "includeLanguageDetection") == true;

        var query = new List<(string Name, string? Value)>
        {
            ("model_id", modelId),
            ("audio_format", inputFormat),
            ("commit_strategy", Text(streamingOptions, "commitStrategy")),
            ("enable_logging", Flag(Bool(streamingOptions, "enableLogging"))),
            ("filter_background_audio", Flag(Bool(streamingOptions, "filterBackgroundAudio"))),
            (
                "include_language_detection",
                Flag(Bool(streamingOptions, "includeLanguageDetection"))
            ),
            ("include_timestamps", includeDetailedCommit ? "true" : Flag(includeTimestamps)),
            ("language_code", languageCode),
            ("min_silence_duration_ms", Digits(streamingOptions, "minSilenceDurationMs")),
            ("min_speech_duration_ms", Digits(streamingOptions, "minSpeechDurationMs")),
            ("no_verbatim", Flag(Bool(streamingOptions, "noVerbatim"))),
            ("vad_silence_threshold_secs", Digits(streamingOptions, "vadSilenceThresholdSecs")),
            ("vad_threshold", Digits(streamingOptions, "vadThreshold")),
        };
        foreach (var keyterm in Texts(streamingOptions, "keyterms"))
        {
            query.Add(("keyterms", keyterm));
        }

        foreach (var secondaryLanguage in Texts(streamingOptions, "secondaryLanguages"))
        {
            query.Add(("secondary_languages", secondaryLanguage));
        }

        var builder = new UriBuilder(baseUrl)
        {
            Query = string.Join(
                "&",
                query
                    .Where(entry => entry.Value is not null)
                    .Select(entry => FormEncode(entry.Name) + "=" + FormEncode(entry.Value!))
            ),
        };
        return builder.Uri;
    }

    /// <summary>Maps to <c>createElevenLabsRealtimeTranscriptionStream</c>.</summary>
    internal static Session CreateElevenLabsRealtimeTranscriptionStream(
        TranscriptionStreamCall call,
        IReadOnlyDictionary<string, string?> headers,
        bool includeLanguageDetection,
        bool includeTimestamps,
        string? language,
        string? previousText,
        double sampleRate,
        Uri url,
        IReadOnlyList<OperationWarning> warnings,
        WebSocketFactory? webSocket
    )
    {
        var session = new Session(
            call,
            includeLanguageDetection,
            includeTimestamps,
            language,
            previousText,
            sampleRate,
            warnings
        );
        session.Start(url, headers, webSocket);
        return session;
    }

    private static bool? Bool(JsonElement? options, string name)
    {
        return
            options is { ValueKind: JsonValueKind.Object } element
            && element.TryGetProperty(name, out var value)
            && value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : null;
    }

    private static string? Text(JsonElement? options, string name)
    {
        return
            options is { ValueKind: JsonValueKind.Object } element
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static string? Digits(JsonElement? options, string name)
    {
        return
            options is { ValueKind: JsonValueKind.Object } element
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
            ? Number(value.GetDouble())
            : null;
    }

    private static IEnumerable<string> Texts(JsonElement? options, string name)
    {
        if (
            options is { ValueKind: JsonValueKind.Object } element
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var item in value.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    yield return item.GetString()!;
                }
            }
        }
    }

    private static string? Flag(bool? value) =>
        value is null ? null
        : value.Value ? "true"
        : "false";

    private static string Number(double value) => value.ToString("R", CultureInfo.InvariantCulture);

    // Matches `URLSearchParams` serialization (application/x-www-form-urlencoded).
    private static string FormEncode(string value)
    {
        var builder = new StringBuilder();
        foreach (var b in Encoding.UTF8.GetBytes(value))
        {
            var c = (char)b;
            if (
                c is >= 'a' and <= 'z'
                || c is >= 'A' and <= 'Z'
                || c is >= '0' and <= '9'
                || c is '*' or '-' or '.' or '_'
            )
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

    /// <summary>The stream controller of <c>createElevenLabsRealtimeTranscriptionStream</c>.</summary>
    internal sealed class Session
    {
        private readonly TranscriptionStreamCall _call;
        private readonly bool _includeTimestamps;
        private readonly bool _expectDetailedCommit;
        private readonly string? _previousText;
        private readonly double _sampleRate;
        private readonly IReadOnlyList<OperationWarning> _warnings;
        private readonly object _gate = new object();
        private readonly List<TranscriptSegment> _finalSegments = new List<TranscriptSegment>();
        private readonly List<string> _finalTexts = new List<string>();
        private WebSocketConnection? _connection;
        private AudioChunkReader? _reader;
        private Timer? _finalCommitGracePeriod;
        private string? _detectedLanguage;
        private string? _sessionId;
        private bool _finished;
        private bool _endOfInput;
        private bool _receivedPostInputCommit;
        private int _segmentIndex;
        private int _committedEventCount;
        private int _committedEventsAtEndOfInput;
        private int? _finalCommitEventCount;
        private int _timestampedCommitCount;
        private bool _hasWebSocketFactory;

        internal Session(
            TranscriptionStreamCall call,
            bool includeLanguageDetection,
            bool includeTimestamps,
            string? language,
            string? previousText,
            double sampleRate,
            IReadOnlyList<OperationWarning> warnings
        )
        {
            _call = call;
            _includeTimestamps = includeTimestamps;
            _expectDetailedCommit = includeTimestamps || includeLanguageDetection;
            _detectedLanguage = language;
            _previousText = previousText;
            _sampleRate = sampleRate;
            _warnings = warnings;
        }

        internal PartQueue<ModelStreamPart> Queue { get; } = new PartQueue<ModelStreamPart>();

        internal void Start(
            Uri url,
            IReadOnlyDictionary<string, string?> headers,
            WebSocketFactory? webSocket
        )
        {
            _hasWebSocketFactory = webSocket is not null;
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
                    OnMessageText = OnMessageTextAsync,
                    OnSocketError = () =>
                        FinishWithError(
                            new AiSdkException(
                                "ElevenLabs realtime transcription error."
                                    + (
                                        _hasWebSocketFactory
                                            ? string.Empty
                                            : " Note: ClientWebSocket may not be able to send the xi-api-key header required by ElevenLabs on every runtime."
                                                + " Pass a header-capable WebSocket implementation via ElevenLabsOptions.WebSocket."
                                    )
                            )
                        ),
                    OnClose = (_, _) => OnClose(),
                }
            );
            lock (_gate)
            {
                _connection = connection;
            }

            // A factory throw or an already-cancelled token finished the session before the connection existed.
            bool alreadyFinished;
            lock (_gate)
            {
                alreadyFinished = _finished;
            }

            if (alreadyFinished)
            {
                connection.Close(null);
            }
        }

        /// <summary>The <c>cancel</c> callback of the <c>ReadableStream</c>.</summary>
        internal void Cancel()
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

        private void Cleanup(int? closeCode)
        {
            AudioChunkReader? reader;
            WebSocketConnection? connection;
            lock (_gate)
            {
                _finalCommitGracePeriod?.Dispose();
                _finalCommitGracePeriod = null;
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

        private void Finish()
        {
            lock (_gate)
            {
                if (_finished)
                {
                    return;
                }

                _finished = true;
                Queue.Enqueue(
                    new ModelStreamPart
                    {
                        Type = "finish",
                        Text = string.Join(" ", _finalTexts).Trim(),
                        Segments = _finalSegments.ToList(),
                        Language = _detectedLanguage,
                        DurationInSeconds =
                            _finalSegments.Count > 0
                                ? _finalSegments[_finalSegments.Count - 1].EndSecond
                                : null,
                    }
                );
            }

            Queue.Complete(null);
            Cleanup(1000);
        }

        private void ScheduleFinish()
        {
            lock (_gate)
            {
                _finalCommitGracePeriod?.Dispose();
                _finalCommitGracePeriod = new Timer(
                    _ => Finish(),
                    null,
                    FinalCommitGracePeriodMs,
                    Timeout.Infinite
                );
            }
        }

        private void OnClose()
        {
            bool completed;
            lock (_gate)
            {
                if (_finished)
                {
                    return;
                }

                completed = _endOfInput && _receivedPostInputCommit;
            }

            if (completed)
            {
                Finish();
                return;
            }

            FinishWithError(
                new AiSdkException(
                    "ElevenLabs realtime transcription stream closed before completion."
                )
            );
        }

        private async Task SendAudioAsync(IWebSocketLike socket)
        {
            var reader = _call.Audio.GetReader();
            lock (_gate)
            {
                _reader = reader;
            }

            var firstChunk = true;
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

                    var message = new JsonObject
                    {
                        ["message_type"] = "input_audio_chunk",
                        ["audio_base_64"] = Convert.ToBase64String(bytes),
                        ["commit"] = false,
                        ["sample_rate"] = _sampleRate,
                    };
                    if (firstChunk && _previousText is not null)
                    {
                        message["previous_text"] = _previousText;
                    }

                    socket.Send(message.ToJsonString());
                    firstChunk = false;

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

            bool send;
            lock (_gate)
            {
                send = !_finished;
                if (send)
                {
                    _committedEventsAtEndOfInput = _committedEventCount;
                    _endOfInput = true;
                }
            }

            if (send)
            {
                socket.Send(
                    new JsonObject
                    {
                        ["message_type"] = "input_audio_chunk",
                        ["audio_base_64"] = string.Empty,
                        ["commit"] = true,
                        ["sample_rate"] = _sampleRate,
                    }.ToJsonString()
                );
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

            var messageType = ReadString(raw, "message_type");
            if (messageType is not null && RealtimeErrorTypes.Contains(messageType))
            {
                bool late;
                lock (_gate)
                {
                    late =
                        _endOfInput
                        && (_committedEventCount > 0 || _timestampedCommitCount > 0)
                        && LateFinalizationErrorTypes.Contains(messageType);
                    if (late)
                    {
                        _receivedPostInputCommit = true;
                    }
                }

                if (late)
                {
                    Finish();
                    return Task.CompletedTask;
                }

                FinishWithError(
                    new AiSdkException(
                        ReadString(raw, "error") ?? "ElevenLabs realtime transcription error"
                    )
                );
                return Task.CompletedTask;
            }

            switch (messageType)
            {
                case "session_started":
                    OnSessionStarted(raw);
                    break;

                case "partial_transcript":
                    string partialId;
                    lock (_gate)
                    {
                        partialId = (_sessionId ?? "session") + ":" + _segmentIndex;
                    }

                    Queue.Enqueue(
                        new ModelStreamPart
                        {
                            Type = "transcript-partial",
                            Id = partialId,
                            Text = ReadString(raw, "text") ?? string.Empty,
                        }
                    );
                    break;

                case "final_transcript":
                case "committed_transcript":
                    OnCommitted(raw, messageType);
                    break;

                case "final_transcript_with_timestamps":
                case "committed_transcript_with_timestamps":
                    OnCommittedWithTimestamps(raw);
                    break;
            }

            return Task.CompletedTask;
        }

        private void OnSessionStarted(JsonElement raw)
        {
            lock (_gate)
            {
                _sessionId = ReadString(raw, "session_id");
            }

            Queue.Enqueue(new ModelStreamPart { Type = "stream-start", Warnings = _warnings });
            WebSocketConnection? connection;
            lock (_gate)
            {
                connection = _connection;
            }

            var socket = connection?.Socket;
            if (socket is null)
            {
                FinishWithError(new AiSdkException("WebSocket is not connected."));
                return;
            }

            _ = SendAudioAsync(socket)
                .ContinueWith(
                    task => FinishWithError(task.Exception!.GetBaseException()),
                    TaskContinuationOptions.OnlyOnFaulted
                );
        }

        private void OnCommitted(JsonElement raw, string messageType)
        {
            var scheduleFinish = false;
            lock (_gate)
            {
                _committedEventCount++;
                var text = (ReadString(raw, "text") ?? string.Empty).Trim();
                var id = (_sessionId ?? "session") + ":" + _segmentIndex;

                if (text.Length > 0)
                {
                    _finalTexts.Add(text);
                    _segmentIndex++;
                    Queue.Enqueue(
                        new ModelStreamPart
                        {
                            Type = "transcript-final",
                            Id = id,
                            Text = text,
                        }
                    );
                }

                // When timestamps are requested, ElevenLabs follows this event with
                // committed_transcript_with_timestamps for the same commit.
                if (_endOfInput && _committedEventCount > _committedEventsAtEndOfInput)
                {
                    _receivedPostInputCommit = true;
                    _finalCommitEventCount = _committedEventCount;
                    if (!_expectDetailedCommit || messageType == "final_transcript")
                    {
                        scheduleFinish = true;
                    }
                }
            }

            if (scheduleFinish)
            {
                ScheduleFinish();
            }
        }

        private void OnCommittedWithTimestamps(JsonElement raw)
        {
            var scheduleFinish = false;
            lock (_gate)
            {
                var text = (ReadString(raw, "text") ?? string.Empty).Trim();
                var timestampedWords = new List<(string Text, double Start, double End)>();
                if (
                    raw.TryGetProperty("words", out var words)
                    && words.ValueKind == JsonValueKind.Array
                )
                {
                    foreach (var word in words.EnumerateArray())
                    {
                        if (
                            word.ValueKind == JsonValueKind.Object
                            && ReadDouble(word, "start") is { } start
                            && ReadDouble(word, "end") is { } end
                        )
                        {
                            timestampedWords.Add(
                                (ReadString(word, "text") ?? string.Empty, start, end)
                            );
                        }
                    }
                }

                _detectedLanguage = ReadString(raw, "language_code") ?? _detectedLanguage;

                // Normally this is paired with the preceding committed_transcript.
                // Still handle a timestamp-only server response defensively.
                if (_finalTexts.Count <= _timestampedCommitCount && text.Length > 0)
                {
                    var id = (_sessionId ?? "session") + ":" + _segmentIndex++;
                    _finalTexts.Add(text);
                    var part = new ModelStreamPart
                    {
                        Type = "transcript-final",
                        Id = id,
                        Text = text,
                    };
                    if (_includeTimestamps)
                    {
                        part.StartSecond =
                            timestampedWords.Count > 0 ? timestampedWords[0].Start : null;
                        part.EndSecond =
                            timestampedWords.Count > 0
                                ? timestampedWords[timestampedWords.Count - 1].End
                                : null;
                    }

                    Queue.Enqueue(part);
                }

                _timestampedCommitCount++;
                if (_includeTimestamps)
                {
                    _finalSegments.AddRange(
                        timestampedWords.Select(word => new TranscriptSegment(
                            word.Text,
                            word.Start,
                            word.End
                        ))
                    );
                }

                if (
                    _endOfInput
                    && _timestampedCommitCount > _committedEventsAtEndOfInput
                    && (
                        _finalCommitEventCount is null
                        || _timestampedCommitCount >= _finalCommitEventCount
                    )
                )
                {
                    _receivedPostInputCommit = true;
                    scheduleFinish = true;
                }
            }

            if (scheduleFinish)
            {
                ScheduleFinish();
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
    }
}
