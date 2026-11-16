// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Cartesia;

/// <summary>Capabilities of a Cartesia transcription model id. Maps to <c>getCartesiaTranscriptionModelEntry</c>.</summary>
public sealed class CartesiaTranscriptionCapabilities
{
    internal CartesiaTranscriptionCapabilities(bool streaming)
    {
        Streaming = streaming;
    }

    /// <summary>True when the model streams over WebSocket. False for the batch-only Ink Whisper family.</summary>
    public bool Streaming { get; }
}

/// <summary>Cartesia model catalog. Maps to <c>cartesia-model-catalog.ts</c>.</summary>
public static class CartesiaModelCatalog
{
    /// <summary>Returns the capabilities of a transcription model id. Future ids inherit the streaming protocol.</summary>
    public static CartesiaTranscriptionCapabilities GetCartesiaTranscriptionModelEntry(string modelId)
    {
        var id = modelId ?? string.Empty;
        return new CartesiaTranscriptionCapabilities(id != "ink-whisper" && !id.StartsWith("ink-whisper-", StringComparison.Ordinal));
    }
}

/// <summary>Realtime Ink 2 transcription over the Cartesia WebSocket. Maps to the streaming half of <c>cartesia-transcription-model.ts</c>.</summary>
internal static class CartesiaStreamingTranscription
{
    private static readonly string[] StreamingEncodings = { "pcm_alaw", "pcm_f16le", "pcm_f32le", "pcm_mulaw", "pcm_s16le", "pcm_s32le" };

    private static readonly string[] G711StreamingEncodings = { "pcm_mulaw", "pcm_alaw" };

    private static readonly (string Option, string Parameter)[] TurnThresholdParams =
    {
        ("turnStartThreshold", "turn_start_threshold"),
        ("turnEagerEndThreshold", "turn_eager_end_threshold"),
        ("turnEndThreshold", "turn_end_threshold"),
        ("turnEndTimeoutMs", "turn_end_timeout_ms"),
    };

    public static async Task<TranscriptionStreamStart> StreamAsync(string modelId, CartesiaProvider provider, TranscriptionStreamCall options, DateTime timestamp, Func<IReadOnlyDictionary<string, string?>, CancellationToken, Task<string>> createStreamingAccessToken, CancellationToken cancellationToken)
    {
        var warnings = new List<OperationWarning>();
        var cartesia = CartesiaProvider.CartesiaOptions(options.ProviderOptions);
        ValidateProviderOptions(cartesia);
        var language = CartesiaProvider.ReadString(cartesia, "language");
        if (language is not null && language != "en")
        {
            throw new InvalidArgumentException("providerOptions", options.ProviderOptions.GetRawText(), "Cartesia Ink 2 currently supports English only.");
        }

        if (cartesia is { } bag && bag.TryGetProperty("timestampGranularities", out var granularities) && granularities.ValueKind != JsonValueKind.Null)
        {
            warnings.Add(OperationWarning.Unsupported("providerOptions.cartesia.timestampGranularities", "Cartesia streaming transcription does not support timestamp granularities."));
        }

        var streaming = Child(cartesia, "streaming");
        var turnDetection = streaming is { } s && s.TryGetProperty("turnDetection", out var td) && td.ValueKind == JsonValueKind.False ? false : true;
        foreach (var (option, _) in TurnThresholdParams)
        {
            if (!turnDetection && streaming is { } values && values.TryGetProperty(option, out var value) && value.ValueKind != JsonValueKind.Null)
            {
                warnings.Add(OperationWarning.Unsupported("providerOptions.cartesia.streaming." + option, "Turn thresholds require turnDetection to be enabled."));
            }
        }

        // Always validate the declared media type: an explicit streaming.encoding override must not bypass the allowlist.
        var inferredEncoding = CartesiaEncodingFromInputAudioFormat(options.InputAudioFormat.Type);
        var encoding = CartesiaProvider.ReadString(streaming, "encoding") ?? inferredEncoding;
        // audio/pcm is generic linear PCM, so widening its default 16-bit reading is the intended use of the option.
        // Overriding a G.711 media type (or turning generic PCM into G.711) contradicts the declared format.
        if (encoding != inferredEncoding && !(inferredEncoding == "pcm_s16le" && Array.IndexOf(G711StreamingEncodings, encoding) < 0))
        {
            warnings.Add(OperationWarning.Other(
                "providerOptions.cartesia.streaming.encoding '" + encoding + "' contradicts inputAudioFormat.type '" + options.InputAudioFormat.Type + "' (inferred '" + inferredEncoding + "'); sending '" + encoding + "'."));
        }

        var token = await createStreamingAccessToken(options.Headers, cancellationToken).ConfigureAwait(false);
        var url = BuildCartesiaStreamingTranscriptionUrl(provider.Options.BaseUrl, provider.Version, modelId, encoding, options.InputAudioFormat, cartesia, token, turnDetection, out var requestUrl);
        var session = new Session(options, warnings, language ?? "en", turnDetection);
        session.Start(url, provider.WebSocket);
        return new TranscriptionStreamStart(
            new ModelPartStream(session.Queue, session.Cancel),
            new ProviderResponse(timestamp: timestamp, modelId: modelId),
            new ProviderRequest(requestUrl));
    }

    // Maps to cartesiaTranscriptionModelOptionsSchema.
    internal static void ValidateProviderOptions(JsonElement? cartesia)
    {
        if (cartesia is not { } bag)
        {
            return;
        }

        var valid = true;
        if (bag.TryGetProperty("language", out var language))
        {
            valid &= language.ValueKind is JsonValueKind.Null or JsonValueKind.String;
        }

        if (bag.TryGetProperty("timestampGranularities", out var granularities))
        {
            valid &= granularities.ValueKind == JsonValueKind.Null
                || (granularities.ValueKind == JsonValueKind.Array && granularities.EnumerateArray().All(entry => entry.ValueKind == JsonValueKind.String && entry.GetString() == "word"));
        }

        if (bag.TryGetProperty("streaming", out var streaming))
        {
            valid &= IsValidStreaming(streaming);
        }

        if (!valid)
        {
            throw new InvalidArgumentException("providerOptions", bag.GetRawText(), "Invalid cartesia provider options.");
        }
    }

    internal static string CartesiaEncodingFromInputAudioFormat(string type)
    {
        return type switch
        {
            "audio/pcm" => "pcm_s16le",
            "audio/pcmu" => "pcm_mulaw",
            "audio/pcma" => "pcm_alaw",
            _ => throw new InvalidArgumentException("inputAudioFormat", type, "Unsupported Cartesia streaming audio format: " + type),
        };
    }

    internal static Uri BuildCartesiaStreamingTranscriptionUrl(string baseUrl, string version, string modelId, string encoding, AudioFormat inputAudioFormat, JsonElement? providerOptions, string token, bool useTurnDetection, out string requestUrl)
    {
        var root = WebSocketUtils.ToWebSocketUrl(new Uri(new Uri(baseUrl), useTurnDetection ? "/stt/turns/websocket" : "/stt/websocket"));
        var query = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("model", modelId),
            new KeyValuePair<string, string>("encoding", encoding),
            new KeyValuePair<string, string>("sample_rate", (inputAudioFormat.Rate ?? 24000).ToString(CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("cartesia_version", version),
        };
        var tokenIndex = query.Count;
        query.Add(new KeyValuePair<string, string>("access_token", token));
        if (!useTurnDetection && CartesiaProvider.ReadString(providerOptions, "language") is { } language)
        {
            query.Add(new KeyValuePair<string, string>("language", language));
        }

        var streaming = Child(providerOptions, "streaming");
        if (streaming is { } values && values.TryGetProperty("keyterms", out var keyterms) && keyterms.ValueKind == JsonValueKind.Array)
        {
            foreach (var keyterm in keyterms.EnumerateArray())
            {
                query.Add(new KeyValuePair<string, string>("keyterm", keyterm.GetString()!));
            }
        }

        if (useTurnDetection && streaming is { } thresholds)
        {
            foreach (var (option, parameter) in TurnThresholdParams)
            {
                if (thresholds.TryGetProperty(option, out var value) && value.ValueKind == JsonValueKind.Number)
                {
                    query.Add(new KeyValuePair<string, string>(parameter, value.GetDouble().ToString(CultureInfo.InvariantCulture)));
                }
            }
        }

        string Build(IEnumerable<KeyValuePair<string, string>> pairs)
        {
            return new UriBuilder(root) { Query = string.Join("&", pairs.Select(pair => Escape(pair.Key) + "=" + Escape(pair.Value))) }.Uri.AbsoluteUri;
        }

        requestUrl = Build(query.Where((_, index) => index != tokenIndex));
        return new Uri(Build(query));
    }

    private static string Escape(string value)
    {
        // URLSearchParams encodes a space as a plus sign.
        return Uri.EscapeDataString(value).Replace("%20", "+");
    }

    private static JsonElement? Child(JsonElement? parent, string name)
    {
        return parent is { } element && element.TryGetProperty(name, out var child) && child.ValueKind == JsonValueKind.Object ? child : (JsonElement?)null;
    }

    private static bool IsValidStreaming(JsonElement streaming)
    {
        if (streaming.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        foreach (var property in streaming.EnumerateObject())
        {
            var value = property.Value;
            var valid = property.Name switch
            {
                "encoding" => value.ValueKind == JsonValueKind.String && Array.IndexOf(StreamingEncodings, value.GetString()) >= 0,
                "turnDetection" => value.ValueKind is JsonValueKind.True or JsonValueKind.False,
                "keyterms" => value.ValueKind == JsonValueKind.Array && value.GetArrayLength() <= 100 && value.EnumerateArray().All(entry => entry.ValueKind == JsonValueKind.String),
                "turnStartThreshold" or "turnEagerEndThreshold" or "turnEndThreshold" => value.ValueKind == JsonValueKind.Number,
                "turnEndTimeoutMs" => value.ValueKind == JsonValueKind.Number && value.GetDouble() == Math.Floor(value.GetDouble()),
                _ => true,
            };
            if (!valid)
            {
                return false;
            }
        }

        return true;
    }

    private sealed class Session
    {
        private readonly TranscriptionStreamCall _options;
        private readonly IReadOnlyList<OperationWarning> _warnings;
        private readonly string _language;
        private readonly bool _useTurnDetection;
        private readonly object _gate = new object();
        private readonly List<string> _finalTexts = new List<string>();
        private WebSocketConnection? _connection;
        private AudioChunkReader? _reader;
        private bool _finished;
        private double _durationInSeconds;

        public Session(TranscriptionStreamCall options, IReadOnlyList<OperationWarning> warnings, string language, bool useTurnDetection)
        {
            _options = options;
            _warnings = warnings;
            _language = language;
            _useTurnDetection = useTurnDetection;
        }

        public PartQueue<ModelStreamPart> Queue { get; } = new PartQueue<ModelStreamPart>();

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

        public void Start(Uri url, WebSocketFactory? webSocket)
        {
            _connection = ConnectToWebSocket.Connect(new ConnectToWebSocketOptions
            {
                Url = url,
                WebSocket = webSocket,
                AbortSignal = _options.CancellationToken,
                AbortReason = _options.AbortReason,
                OnAbort = Fail,
                OnProcessingError = Fail,
                OnOpen = OnOpen,
                OnMessageText = OnMessageTextAsync,
                OnSocketError = () => Fail(new AiSdkException("Cartesia streaming transcription error")),
                OnClose = (_, _) =>
                {
                    if (!IsFinished)
                    {
                        Finish();
                    }
                },
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

            Cleanup(null);
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
                    // Pre-open failure or abort: cancel the caller's audio so a producer piping into it does not hang.
                    _options.Audio.Cancel();
                }
            }
            catch (Exception)
            {
                // A model-owned stream stays with its owner.
            }

            _connection?.Close(closeCode);
        }

        private void Fail(Exception error)
        {
            if (!TryFinish())
            {
                return;
            }

            Queue.Complete(error);
            Cleanup(null);
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
                Text = string.Join(_useTurnDetection ? " " : string.Empty, _finalTexts),
                Segments = new List<TranscriptSegment>(),
                Language = _language,
                DurationInSeconds = _durationInSeconds > 0 ? _durationInSeconds : (double?)null,
            });
            Queue.Complete(null);
            Cleanup(1000);
        }

        private void OnOpen(IWebSocketLike socket)
        {
            Queue.Enqueue(new ModelStreamPart { Type = "stream-start", Warnings = _warnings });
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
                    var bytes = await reader.ReadAsync(_options.CancellationToken).ConfigureAwait(false);
                    if (bytes is null || IsFinished)
                    {
                        break;
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
                socket.Send(_useTurnDetection ? "{\"type\":\"close\"}" : "finalize");
            }
        }

        private Task OnMessageTextAsync(string text)
        {
            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(text);
            }
            catch (JsonException)
            {
                return Task.CompletedTask;
            }

            using (document)
            {
                var raw = document.RootElement;
                if (raw.ValueKind != JsonValueKind.Object)
                {
                    return Task.CompletedTask;
                }

                if (_options.IncludeRawChunks == true)
                {
                    Queue.Enqueue(new ModelStreamPart { Type = "raw", Raw = raw.Clone() });
                }

                var id = CartesiaProvider.ReadString(raw, "request_id");
                switch (CartesiaProvider.ReadString(raw, "type"))
                {
                    case "turn.update":
                    case "turn.eager_end":
                        Queue.Enqueue(new ModelStreamPart { Type = "transcript-partial", Id = id, Text = CartesiaProvider.ReadString(raw, "transcript") ?? string.Empty });
                        break;
                    case "turn.end":
                        var turn = CartesiaProvider.ReadString(raw, "transcript") ?? string.Empty;
                        _finalTexts.Add(turn);
                        Queue.Enqueue(new ModelStreamPart { Type = "transcript-final", Id = id, Text = turn });
                        break;
                    case "transcript":
                        var transcript = CartesiaProvider.ReadString(raw, "text") ?? string.Empty;
                        var duration = CartesiaProvider.ReadNumber(raw, "duration");
                        if (raw.TryGetProperty("is_final", out var isFinal) && isFinal.ValueKind == JsonValueKind.True)
                        {
                            _finalTexts.Add(transcript);
                            _durationInSeconds += duration ?? 0;
                            Queue.Enqueue(new ModelStreamPart { Type = "transcript-final", Id = id, Text = transcript });
                        }
                        else
                        {
                            Queue.Enqueue(new ModelStreamPart { Type = "transcript-partial", Id = id, Text = transcript, DurationInSeconds = duration });
                        }

                        break;
                    case "flush_done":
                        _connection?.Socket?.Send("close");
                        break;
                    case "done":
                        Finish();
                        break;
                    case "error":
                        Fail(new AiSdkException(CartesiaProvider.ReadString(raw, "message") ?? CartesiaProvider.ReadString(raw, "error_code") ?? "Cartesia streaming transcription error"));
                        break;
                }
            }

            return Task.CompletedTask;
        }
    }
}
