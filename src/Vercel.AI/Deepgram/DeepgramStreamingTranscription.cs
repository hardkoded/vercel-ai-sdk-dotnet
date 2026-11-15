// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Deepgram;

/// <summary>Live transcription over the Deepgram <c>/v1/listen</c> WebSocket. Maps to <c>deepgram-streaming-transcription.ts</c>.</summary>
internal static class DeepgramStreamingTranscription
{
    private static readonly string[] ProviderOptionOrder =
    {
        "language", "detectLanguage", "smartFormat", "punctuate", "paragraphs", "summarize", "topics", "intents", "sentiment",
        "detectEntities", "redact", "replace", "search", "keyterm", "diarize", "utterances", "uttSplit", "fillerWords",
    };

    private static readonly Dictionary<string, string> Mappings = new Dictionary<string, string>
    {
        ["language"] = "language",
        ["smartFormat"] = "smart_format",
        ["punctuate"] = "punctuate",
        ["diarize"] = "diarize",
        ["fillerWords"] = "filler_words",
        ["redact"] = "redact",
        ["replace"] = "replace",
        ["search"] = "search",
        ["keyterm"] = "keyterm",
    };

    private static readonly (string Option, string Parameter)[] StreamingMappings =
    {
        ("interimResults", "interim_results"),
        ("endpointing", "endpointing"),
        ("channels", "channels"),
        ("multichannel", "multichannel"),
        ("vadEvents", "vad_events"),
        ("utteranceEndMs", "utterance_end_ms"),
    };

    private static readonly Regex Credentials = new Regex(@"^(Token|Bearer) (\S+)\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    public static async Task<TranscriptionStreamStart> StreamAsync(string modelId, DeepgramProvider provider, TranscriptionStreamCall options, CancellationToken cancellationToken)
    {
        if (!DeepgramModelCatalog.GetDeepgramTranscriptionModelEntry(modelId).Streaming)
        {
            throw new UnsupportedFunctionalityException("Deepgram Flux streaming transcription (/v2/listen)", "'Deepgram Flux streaming transcription (/v2/listen)' functionality not supported.");
        }

        var encoding = options.InputAudioFormat.Type switch
        {
            "audio/pcm" => "linear16",
            "audio/pcmu" => "mulaw",
            "audio/pcma" => "alaw",
            _ => null,
        };
        var rate = options.InputAudioFormat.Rate;
        if (encoding is null || rate is null || rate <= 0 || rate != Math.Floor(rate.Value) || double.IsInfinity(rate.Value))
        {
            throw new InvalidArgumentException("inputAudioFormat", options.InputAudioFormat, "Deepgram streaming requires audio/pcm (16-bit little-endian), audio/pcmu, or audio/pcma and a positive integer sample rate.");
        }

        var warnings = new List<OperationWarning>();
        var query = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("model", modelId),
            new KeyValuePair<string, string>("encoding", encoding),
            new KeyValuePair<string, string>("sample_rate", rate.Value.ToString("0", CultureInfo.InvariantCulture)),
        };
        var deepgram = options.ProviderOptions.ValueKind == JsonValueKind.Object && options.ProviderOptions.TryGetProperty("deepgram", out var bag) && bag.ValueKind == JsonValueKind.Object ? bag : (JsonElement?)null;
        string? language = null;
        if (deepgram is { } values)
        {
            foreach (var key in ProviderOptionOrder)
            {
                if (!values.TryGetProperty(key, out var value) || value.ValueKind == JsonValueKind.Null)
                {
                    continue;
                }

                if (key == "language" && value.ValueKind == JsonValueKind.String)
                {
                    language = value.GetString();
                }

                if (!Mappings.TryGetValue(key, out var parameter))
                {
                    if (value.ValueKind == JsonValueKind.False)
                    {
                        continue;
                    }

                    warnings.Add(OperationWarning.Unsupported("providerOptions.deepgram." + key, "This option is not supported for Deepgram streaming transcription."));
                    continue;
                }

                foreach (var entry in value.ValueKind == JsonValueKind.Array ? value.EnumerateArray().ToArray() : new[] { value })
                {
                    query.Add(new KeyValuePair<string, string>(parameter, QueryValue(entry)));
                }
            }

            if (values.TryGetProperty("streaming", out var streaming) && streaming.ValueKind == JsonValueKind.Object)
            {
                foreach (var (option, parameter) in StreamingMappings)
                {
                    if (streaming.TryGetProperty(option, out var value) && value.ValueKind != JsonValueKind.Null)
                    {
                        ValidateStreamingOption(option, value);
                        query.Add(new KeyValuePair<string, string>(parameter, QueryValue(value)));
                    }
                }
            }
        }

        var url = new UriBuilder(WebSocketUtils.ToWebSocketUrl(ApiKeys.Combine(provider.Options.BaseUrl, "/v1/listen")))
        {
            Query = string.Join("&", query.Select(pair => Uri.EscapeDataString(pair.Key) + "=" + Uri.EscapeDataString(pair.Value))),
        }.Uri;

        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var source in new IEnumerable<KeyValuePair<string, string?>>[] { await provider.CreateHeadersAsync(cancellationToken).ConfigureAwait(false), options.Headers.Select(pair => new KeyValuePair<string, string?>(pair.Key, pair.Value)) })
        {
            foreach (var pair in source)
            {
                headers[pair.Key.ToLowerInvariant()] = pair.Value;
            }
        }

        headers.TryGetValue("authorization", out var authorization);
        // The subprotocol authentication channel also works with native WebSockets, which cannot send custom HTTP
        // headers. Never put credentials in the URL.
        var match = authorization is null ? null : Credentials.Match(authorization);
        if (authorization is not null && !match!.Success)
        {
            throw new InvalidArgumentException("headers", null, "Deepgram streaming authorization must use the Token or Bearer scheme.");
        }

        var protocols = match is not null ? new[] { match.Groups[1].Value.ToLowerInvariant(), match.Groups[2].Value } : null;
        var timestamp = provider.Now().UtcDateTime;
        var session = new Session(modelId, options, timestamp, warnings, language);
        session.Start(url, protocols, headers, provider.WebSocket);
        return new TranscriptionStreamStart(
            new ModelPartStream(session.Queue, session.Cancel),
            new ProviderResponse(timestamp: timestamp, modelId: modelId),
            new ProviderRequest(url.ToString()));
    }

    private static void ValidateStreamingOption(string option, JsonElement value)
    {
        var valid = option switch
        {
            "interimResults" or "multichannel" or "vadEvents" => value.ValueKind is JsonValueKind.True or JsonValueKind.False,
            "endpointing" => value.ValueKind == JsonValueKind.False || (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var ms) && ms >= 0),
            "channels" => value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var channels) && channels > 0,
            "utteranceEndMs" => value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var utterance) && utterance >= 1000,
            _ => true,
        };
        if (!valid)
        {
            throw new InvalidArgumentException("providerOptions", value.GetRawText(), "Invalid deepgram.streaming." + option + " option.");
        }
    }

    private static string QueryValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString()!,
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Number => value.GetDouble().ToString(CultureInfo.InvariantCulture),
            _ => value.GetRawText(),
        };
    }

    private sealed class FinalText
    {
        public string Text = string.Empty;
        public double StartSecond;
        public int ChannelIndex;
    }

    private sealed class Session
    {
        private readonly string _modelId;
        private readonly TranscriptionStreamCall _options;
        private readonly DateTime _timestamp;
        private readonly IReadOnlyList<OperationWarning> _warnings;
        private readonly object _gate = new object();
        private readonly List<FinalText> _finalTexts = new List<FinalText>();
        private readonly List<TranscriptSegment> _segments = new List<TranscriptSegment>();
        private WebSocketConnection? _connection;
        private AudioChunkReader? _reader;
        private Timer? _keepAlive;
        private bool _finished;
        private bool _inputEnded;
        private double? _duration;
        private double? _reportedDuration;
        private string? _language;

        public Session(string modelId, TranscriptionStreamCall options, DateTime timestamp, IReadOnlyList<OperationWarning> warnings, string? language)
        {
            _modelId = modelId;
            _options = options;
            _timestamp = timestamp;
            _warnings = warnings;
            _language = language;
        }

        public PartQueue<ModelStreamPart> Queue { get; } = new PartQueue<ModelStreamPart>();

        public void Start(Uri url, IReadOnlyList<string>? protocols, IReadOnlyDictionary<string, string?> headers, WebSocketFactory? webSocket)
        {
            _connection = ConnectToWebSocket.Connect(new ConnectToWebSocketOptions
            {
                Url = url,
                Headers = headers,
                Protocols = protocols,
                WebSocket = webSocket,
                AbortSignal = _options.CancellationToken,
                OnAbort = Fail,
                OnProcessingError = Fail,
                OnOpen = OnOpen,
                OnMessageText = OnMessageTextAsync,
                OnSocketError = () => Fail(new AiSdkException("Deepgram streaming WebSocket error.")),
                OnClose = OnClose,
            });
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

            Queue.Complete(null);
            Cleanup();
        }

        private void Cleanup()
        {
            AudioChunkReader? reader;
            lock (_gate)
            {
                _keepAlive?.Dispose();
                _keepAlive = null;
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
                    _options.Audio.Cancel();
                }
            }
            catch (Exception)
            {
                // A model-owned stream stays with its owner.
            }

            _connection?.Close(1000);
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

        private void Fail(Exception error)
        {
            if (!TryFinish())
            {
                return;
            }

            Queue.Complete(error);
            Cleanup();
        }

        private void Finish()
        {
            if (!TryFinish())
            {
                return;
            }

            Queue.Enqueue(new ModelStreamPart
            {
                Type = "finish",
                Text = string.Join(" ", _finalTexts.OrderBy(segment => segment.StartSecond).ThenBy(segment => segment.ChannelIndex).Select(segment => segment.Text)),
                Segments = _segments.OrderBy(segment => segment.StartSecond).ToList(),
                Language = _language,
                DurationInSeconds = _duration,
                Usage = _reportedDuration is { } seconds ? new OperationUsage(seconds: seconds) : null,
            });
            Queue.Complete(null);
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

        private void OnOpen(IWebSocketLike socket)
        {
            Queue.Enqueue(new ModelStreamPart { Type = "stream-start", Warnings = _warnings });
            // Deepgram closes idle connections after ten seconds. Keep an open microphone alive even while its
            // producer has no audio to send.
            lock (_gate)
            {
                _keepAlive = new Timer(
                    _ =>
                    {
                        if (IsFinished || socket.ReadyState != 1)
                        {
                            return;
                        }

                        try
                        {
                            socket.Send("{\"type\":\"KeepAlive\"}");
                        }
                        catch (Exception error)
                        {
                            Fail(error);
                        }
                    },
                    null,
                    4000,
                    4000);
            }

            _ = SendAudioAsync(socket).ContinueWith(task => Fail(task.Exception!.GetBaseException()), TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task SendAudioAsync(IWebSocketLike socket)
        {
            var reader = _options.Audio.GetReader();
            lock (_gate)
            {
                _reader = reader;
            }

            try
            {
                while (true)
                {
                    if (IsFinished)
                    {
                        break;
                    }

                    var bytes = await reader.ReadAsync(_options.CancellationToken).ConfigureAwait(false);
                    if (bytes is null || IsFinished)
                    {
                        break;
                    }

                    if (bytes.Length == 0)
                    {
                        continue;
                    }

                    socket.Send(bytes);
                    await WebSocketUtils.WaitForWebSocketBufferDrainAsync(socket, cancellationToken: _options.CancellationToken).ConfigureAwait(false);
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

            if (!IsFinished)
            {
                lock (_gate)
                {
                    _inputEnded = true;
                    _keepAlive?.Dispose();
                    _keepAlive = null;
                }

                socket.Send("{\"type\":\"CloseStream\"}");
            }
        }

        private Task OnMessageTextAsync(string text)
        {
            if (IsFinished)
            {
                return Task.CompletedTask;
            }

            using var document = JsonDocument.Parse(text);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("type", out var typeElement) || typeElement.ValueKind != JsonValueKind.String)
            {
                throw new AiSdkException("Deepgram streaming event is missing a type.");
            }

            if (_options.IncludeRawChunks == true)
            {
                Queue.Enqueue(new ModelStreamPart { Type = "raw", Raw = root.Clone() });
            }

            switch (typeElement.GetString())
            {
                case "Error":
                    Fail(new AiSdkException(ReadString(root, "description") ?? ReadString(root, "message") ?? "Deepgram streaming transcription failed."));
                    break;
                case "Metadata":
                    var duration = ReadNumber(root, "duration");
                    lock (_gate)
                    {
                        _reportedDuration = duration ?? _reportedDuration;
                        _duration = duration ?? _duration;
                    }

                    Queue.Enqueue(new ModelStreamPart { Type = "response-metadata", Timestamp = _timestamp, ModelId = _modelId });
                    bool ended;
                    lock (_gate)
                    {
                        ended = _inputEnded;
                    }

                    if (ended)
                    {
                        Finish();
                    }

                    break;
                case "Results":
                    HandleResults(root);
                    break;
            }

            return Task.CompletedTask;
        }

        private void HandleResults(JsonElement root)
        {
            if (!root.TryGetProperty("channel", out var channel) || channel.ValueKind != JsonValueKind.Object
                || !channel.TryGetProperty("alternatives", out var alternatives) || alternatives.ValueKind != JsonValueKind.Array || alternatives.GetArrayLength() == 0)
            {
                throw new AiSdkException("Deepgram Results event is missing a transcript alternative.");
            }

            var alternative = alternatives[0];
            var transcript = ReadString(alternative, "transcript") ?? throw new AiSdkException("Deepgram Results event is missing a transcript.");
            if (alternative.TryGetProperty("languages", out var languages) && languages.ValueKind == JsonValueKind.Array && languages.GetArrayLength() > 0 && languages[0].ValueKind == JsonValueKind.String)
            {
                _language = languages[0].GetString();
            }

            var start = ReadNumber(root, "start") ?? 0;
            var eventDuration = ReadNumber(root, "duration");
            var end = start + (eventDuration ?? 0);
            lock (_gate)
            {
                _duration = Math.Max(_duration ?? 0, end);
            }

            int? channelIndex = root.TryGetProperty("channel_index", out var indexes) && indexes.ValueKind == JsonValueKind.Array && indexes.GetArrayLength() > 0 && indexes[0].TryGetInt32(out var index) ? index : (int?)null;
            var speechFinal = root.TryGetProperty("speech_final", out var speech) && speech.ValueKind == JsonValueKind.True;
            var metadata = JsonSerializer.SerializeToElement(new Dictionary<string, object> { ["deepgram"] = new Dictionary<string, object> { ["speechFinal"] = speechFinal } });
            if (root.TryGetProperty("is_final", out var isFinal) && isFinal.ValueKind == JsonValueKind.True)
            {
                if (transcript.Length > 0)
                {
                    _finalTexts.Add(new FinalText { Text = transcript, StartSecond = start, ChannelIndex = channelIndex ?? 0 });
                }

                if (alternative.TryGetProperty("words", out var words) && words.ValueKind == JsonValueKind.Array)
                {
                    foreach (var word in words.EnumerateArray())
                    {
                        _segments.Add(new TranscriptSegment(ReadString(word, "punctuated_word") ?? ReadString(word, "word") ?? string.Empty, ReadNumber(word, "start") ?? 0, ReadNumber(word, "end") ?? 0));
                    }
                }

                Queue.Enqueue(new ModelStreamPart { Type = "transcript-final", Text = transcript, StartSecond = start, EndSecond = end, ChannelIndex = channelIndex, ProviderMetadata = metadata });
            }
            else
            {
                Queue.Enqueue(new ModelStreamPart { Type = "transcript-partial", Text = transcript, StartSecond = start, DurationInSeconds = eventDuration, ChannelIndex = channelIndex, ProviderMetadata = metadata });
            }
        }

        private void OnClose(int? code, string? reason)
        {
            bool ended;
            lock (_gate)
            {
                if (_finished)
                {
                    return;
                }

                ended = _inputEnded;
            }

            if (!ended || (code != null && code != 1000))
            {
                Fail(new AiSdkException("Deepgram streaming connection closed unexpectedly" + (code != null ? " (" + code.Value.ToString(CultureInfo.InvariantCulture) + ")" : string.Empty) + (string.IsNullOrEmpty(reason) ? "." : ": " + reason)));
            }
            else
            {
                Finish();
            }
        }

        private static string? ReadString(JsonElement element, string name)
        {
            return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        }

        private static double? ReadNumber(JsonElement element, string name)
        {
            return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number ? value.GetDouble() : (double?)null;
        }
    }
}
