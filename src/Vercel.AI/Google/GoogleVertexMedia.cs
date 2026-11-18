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

/// <summary>Cloud Text-to-Speech <c>text:synthesize</c> for Chirp 3 HD voices.</summary>
public sealed class GoogleVertexCloudSpeechModel : ISpeechModel
{
    /// <summary>Synthesize endpoint. Cloud TTS uses one non-regional host.</summary>
    public const string SynthesizeUrl = "https://texttospeech.googleapis.com/v1/text:synthesize";

    private readonly GoogleVertexProvider _provider;

    /// <summary>Creates a Cloud TTS model.</summary>
    public GoogleVertexCloudSpeechModel(GoogleVertexProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string Provider
    {
        get { return "google.vertex.speech"; }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Warnings from the last call.</summary>
    public IReadOnlyList<GoogleWarning> Warnings { get; private set; } =
        Array.Empty<GoogleWarning>();

    /// <inheritdoc />
    public async Task<SpeechResult> DoGenerateAsync(
        SpeechCallOptions options,
        CancellationToken cancellationToken
    )
    {
        var prepared = Prepare(
            options.Text,
            options.Voice,
            options.Language,
            options.Speed,
            options.Instructions
        );
        Warnings = prepared.Warnings;
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        using var document = await _provider
            .Http.SendJsonAsync(
                HttpMethod.Post,
                new Uri(SynthesizeUrl),
                GoogleJson.Write(prepared.Body),
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
        var audio = Array.Empty<byte>();
        if (
            document.RootElement.TryGetProperty("audioContent", out var content)
            && content.ValueKind == System.Text.Json.JsonValueKind.String
            && !string.IsNullOrEmpty(content.GetString())
        )
        {
            audio = Convert.FromBase64String(content.GetString()!);
        }

        return new SpeechResult(audio, "audio/wav", null);
    }

    /// <summary>Builds the synthesize body.</summary>
    public static GoogleSpeechPreparation Prepare(
        string text,
        string? voice,
        string? language,
        double? speed,
        string? instructions
    )
    {
        var warnings = new List<GoogleWarning>();
        const string infix = "Chirp3-HD";
        var selected = string.IsNullOrEmpty(voice) ? "Kore" : voice!;
        string voiceName;
        string languageCode;
        if (selected.IndexOf(infix, StringComparison.Ordinal) >= 0)
        {
            voiceName = selected;
            var prefix = selected
                .Substring(0, selected.IndexOf(infix, StringComparison.Ordinal))
                .TrimEnd('-');
            languageCode = language ?? (prefix.Length == 0 ? "en-US" : prefix);
        }
        else
        {
            languageCode = language ?? "en-US";
            voiceName = languageCode + "-" + infix + "-" + selected;
        }

        if (!string.IsNullOrEmpty(instructions))
        {
            warnings.Add(
                GoogleWarning.Unsupported(
                    "instructions",
                    "Google Cloud Text-to-Speech Chirp 3: HD voices do not support the instructions option. It was ignored."
                )
            );
        }

        var audio = new JsonObject { ["audioEncoding"] = "LINEAR16" };
        if (speed != null)
        {
            audio["speakingRate"] = speed.Value;
        }

        var body = new JsonObject
        {
            ["input"] = new JsonObject { ["text"] = text ?? string.Empty },
            ["voice"] = new JsonObject { ["languageCode"] = languageCode, ["name"] = voiceName },
            ["audioConfig"] = audio,
        };
        return new GoogleSpeechPreparation(body, warnings);
    }
}

/// <summary>Cloud Speech-to-Text recognize.</summary>
public sealed class GoogleVertexSpeechTranscriptionModel : ITranscriptionModel
{
    private readonly GoogleVertexProvider _provider;

    /// <summary>Creates a transcription model.</summary>
    public GoogleVertexSpeechTranscriptionModel(GoogleVertexProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string Provider
    {
        get { return "google.vertex.transcription"; }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(
        AudioInput audio,
        CancellationToken cancellationToken
    )
    {
        var url = GoogleVertexEndpoints.RecognizeUrl(
            _provider.Vertex.Project,
            _provider.Vertex.Region
        );
        var body = Request(ModelId, audio.Data, null);
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        using var document = await _provider
            .Http.SendJsonAsync(
                HttpMethod.Post,
                new Uri(url),
                GoogleJson.Write(body),
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
        return new TranscriptionResult(ReadTranscript(document.RootElement), null);
    }

    /// <summary>Builds the recognize body. Audio bytes are base64-encoded.</summary>
    public static JsonObject Request(string modelId, byte[] audio, string[]? languageCodes)
    {
        var codes = new JsonArray();
        if (languageCodes == null || languageCodes.Length == 0)
        {
            codes.Add(JsonValue.Create("auto"));
        }
        else
        {
            foreach (var code in languageCodes)
            {
                codes.Add(JsonValue.Create(code));
            }
        }

        return new JsonObject
        {
            ["config"] = new JsonObject
            {
                ["model"] = modelId,
                ["languageCodes"] = codes,
                ["autoDecodingConfig"] = new JsonObject(),
                ["features"] = new JsonObject
                {
                    ["enableWordTimeOffsets"] = true,
                    ["enableAutomaticPunctuation"] = true,
                },
            },
            ["content"] = Convert.ToBase64String(audio ?? Array.Empty<byte>()),
        };
    }

    /// <summary>Maps a BCP-47 tag onto a two-letter ISO-639-1 code.</summary>
    public static string? ToIso6391(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return null;
        }

        var language = tag!;
        var dash = language.IndexOf('-');
        if (dash > 0)
        {
            language = language.Substring(0, dash);
        }

        return language.Length == 2 ? language.ToLowerInvariant() : null;
    }

    /// <summary>Parses a duration such as <c>1.200s</c>.</summary>
    public static double? ParseDuration(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        var text = value!.EndsWith("s", StringComparison.Ordinal)
            ? value.Substring(0, value.Length - 1)
            : value;
        return double.TryParse(
            text,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var seconds
        )
            ? seconds
            : null;
    }

    private static string ReadTranscript(System.Text.Json.JsonElement root)
    {
        if (!root.TryGetProperty("results", out var results))
        {
            return string.Empty;
        }

        var parts = new List<string>();
        foreach (var result in results.EnumerateArray())
        {
            if (
                result.TryGetProperty("alternatives", out var alternatives)
                && alternatives.GetArrayLength() > 0
            )
            {
                var transcript = GoogleJson.String(alternatives[0], "transcript");
                if (!string.IsNullOrEmpty(transcript))
                {
                    parts.Add(transcript!);
                }
            }
        }

        return string.Join(" ", parts).Trim();
    }
}

/// <summary>Vertex Gemini transcription through generateContent.</summary>
public sealed class GoogleVertexGeminiTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
{
    private const string LiveWebSocketPath =
        "google.cloud.aiplatform.v1.LlmBidiService/BidiGenerateContent";

    // After the input audio has ended, finish when no terminal signal arrives within this window.
    private const int DefaultFinishGraceMs = 3000;

    private static readonly string[] OptionNamespaces = { "googleVertex", "vertex", "google" };
    private static readonly string[] ConfigFields =
    {
        "languageCodes",
        "customVocabulary",
        "wordTimestamp",
        "diarization",
        "mode",
    };

    private readonly GoogleVertexProvider _provider;

    /// <summary>Creates a Gemini transcription model.</summary>
    public GoogleVertexGeminiTranscriptionModel(GoogleVertexProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string Provider
    {
        get { return "google.vertex.transcription"; }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion
    {
        get { return "v4"; }
    }

    /// <summary>True. Live model ids stream over the Vertex Live API WebSocket.</summary>
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
    /// Transcribes audio. Options are read from <c>googleVertex</c>, then <c>vertex</c>, then <c>google</c>.
    /// Text parts win over the <c>audioTranscription</c> text, which also carries the language and word timings.
    /// </summary>
    public async Task<TranscriptionModelResult> DoGenerateAsync(
        TranscriptionModelCall call,
        CancellationToken cancellationToken
    )
    {
        if (GoogleTranscriptionModel.IsLive(ModelId))
        {
            throw new ArgumentException(
                "Model '" + ModelId + "' only supports streaming transcription.",
                nameof(ModelId)
            );
        }

        var body = new JsonObject
        {
            ["contents"] = new JsonArray
            {
                new JsonObject
                {
                    ["role"] = "user",
                    ["parts"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["inlineData"] = new JsonObject
                            {
                                ["mimeType"] = call.MediaType,
                                ["data"] = Convert.ToBase64String(call.Audio),
                            },
                        },
                    },
                },
            },
        };
        var warnings = new List<OperationWarning>();
        var wantsWord = call.TimestampGranularities?.Contains("word") == true;
        var wordTimestamp = WordTimestampOption(call.ProviderOptions);
        if (wantsWord && wordTimestamp == false)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "timestampGranularities",
                    "The provider wordTimestamp option is false and overrides the requested word timings."
                )
            );
        }

        if (call.TimestampGranularities?.Contains("segment") == true)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "timestampGranularities",
                    "Vertex Gemini transcription returns word timings and does not provide separate caption segments."
                )
            );
        }

        if (
            TranscriptionConfig(call.ProviderOptions, wantsWord && wordTimestamp == null) is
            { } config
        )
        {
            body["generationConfig"] = new JsonObject { ["audioTranscriptionConfig"] = config };
        }

        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        foreach (var header in call.Headers)
        {
            headers[header.Key] = header.Value;
        }

        using var document = await _provider
            .Http.SendJsonAsync(
                HttpMethod.Post,
                ApiKeys.Combine(
                    _provider.Options.BaseUrl,
                    GoogleModelPath.Get(ModelId) + ":generateContent"
                ),
                GoogleJson.Write(body),
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
        var root = document.RootElement;
        var plainText = string.Empty;
        var transcriptionText = string.Empty;
        string? language = null;
        var segments = new List<TranscriptSegment>();
        var wordTimings = new List<TranscriptSegment>();
        if (
            root.TryGetProperty("candidates", out var candidates)
            && candidates.GetArrayLength() > 0
            && GoogleJson.TryObject(candidates[0], "content", out var content)
            && content.TryGetProperty("parts", out var parts)
            && parts.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var part in parts.EnumerateArray())
            {
                plainText += GoogleJson.String(part, "text");
                if (!GoogleJson.TryObject(part, "audioTranscription", out var transcription))
                {
                    continue;
                }

                transcriptionText += GoogleJson.String(transcription, "text");
                language ??= GoogleJson.String(transcription, "languageCode");
                if (
                    !transcription.TryGetProperty("words", out var words)
                    || words.ValueKind != JsonValueKind.Array
                )
                {
                    continue;
                }

                foreach (var word in words.EnumerateArray())
                {
                    var text = GoogleJson.String(word, "word");
                    var start = GoogleVertexSpeechTranscriptionModel.ParseDuration(
                        GoogleJson.String(word, "startOffset")
                    );
                    var end = GoogleVertexSpeechTranscriptionModel.ParseDuration(
                        GoogleJson.String(word, "endOffset")
                    );
                    if (text != null && start != null && end != null)
                    {
                        wordTimings.Add(new TranscriptSegment(text, start.Value, end.Value));
                        segments.Add(new TranscriptSegment(text, start.Value, end.Value));
                    }
                }
            }
        }

        JsonElement? metadata = null;
        if (GoogleJson.TryObject(root, "usageMetadata", out var usage))
        {
            metadata = JsonSerializer.SerializeToElement(
                new JsonObject
                {
                    ["google"] = new JsonObject
                    {
                        ["usageMetadata"] = JsonNode.Parse(usage.GetRawText()),
                    },
                }
            );
        }

        var transcript = plainText.Length > 0 ? plainText : transcriptionText;
        if (
            wantsWord
            && wordTimestamp != false
            && wordTimings.Count == 0
            && transcript.Trim().Length > 0
        )
        {
            warnings.Add(
                OperationWarning.Other(
                    "The Vertex Gemini transcription response omitted the requested word timings."
                )
            );
        }

        return new TranscriptionModelResult(
            transcript,
            segments,
            language,
            warnings: warnings,
            providerMetadata: metadata,
            response: new ProviderResponse(
                body: root.Clone(),
                timestamp: DateTime.UtcNow,
                modelId: ModelId
            ),
            words: wordTimings.Count > 0 ? wordTimings : null
        );
    }

    /// <summary>Streams transcription over the Vertex Live API WebSocket (<c>LlmBidiService/BidiGenerateContent</c>).</summary>
    public async Task<TranscriptionStreamStart?> DoStreamAsync(
        TranscriptionStreamCall call,
        CancellationToken cancellationToken
    )
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        if (!GoogleTranscriptionModel.IsLive(ModelId))
        {
            throw new InvalidArgumentException(
                "modelId",
                ModelId,
                "Model '"
                    + ModelId
                    + "' does not support streaming transcription. Use a live model such as 'gemini-3.5-transcribe-live'."
            );
        }

        var timestamp = _provider.Clock().UtcDateTime;
        var warnings = new List<OperationWarning>();
        ValidateLiveInputAudioFormat(call.InputAudioFormat);

        // Vertex Live authenticates with the same OAuth Bearer header the HTTP surface uses.
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false))
        {
            headers[pair.Key] = pair.Value;
        }

        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        var vertex = _provider.Vertex;
        var modelResource =
            "projects/"
            + vertex.Project
            + "/locations/"
            + vertex.Region
            + "/publishers/google/models/"
            + ModelId;
        var url = new Uri(
            "wss://" + VertexHost(vertex.Region) + "/ws/" + LiveWebSocketPath,
            UriKind.Absolute
        );

        // Mirrors the Developer API model: sending generationConfig.responseModalities suppresses the final
        // inputTranscription segments, so the setup omits generationConfig.
        var setup = new JsonObject
        {
            ["model"] = modelResource,
            ["inputAudioTranscription"] = TranscriptionConfig(call.ProviderOptions, false)
                ?? new JsonObject(),
        };
        var session = new LiveSession(
            call,
            setup,
            (int)(call.InputAudioFormat.Rate ?? 16000),
            vertex.FinishGraceMs ?? DefaultFinishGraceMs,
            warnings
        );
        session.Start(url, headers, vertex.WebSocket);
        return new TranscriptionStreamStart(
            new ModelPartStream(session.Queue, session.Cancel),
            new ProviderResponse(timestamp: timestamp, modelId: ModelId),
            new ProviderRequest(setup.ToJsonString())
        );
    }

    /// <summary>Regional Vertex hostname (mirrors the provider's base-URL host rules). Maps to <c>vertexHost</c>.</summary>
    private static string VertexHost(string location)
    {
        if (location == "global")
        {
            return "aiplatform.googleapis.com";
        }

        if (location == "eu" || location == "us")
        {
            return "aiplatform." + location + ".rep.googleapis.com";
        }

        return location + "-aiplatform.googleapis.com";
    }

    private static void ValidateLiveInputAudioFormat(AudioFormat inputAudioFormat)
    {
        if (
            inputAudioFormat.Type != "audio/pcm"
            || (inputAudioFormat.Rate != null && inputAudioFormat.Rate != 16000)
        )
        {
            throw new InvalidArgumentException(
                "inputAudioFormat",
                inputAudioFormat,
                "The Gemini Live transcription API only supports 16kHz 16-bit PCM input audio."
            );
        }
    }

    private static bool? WordTimestampOption(JsonElement providerOptions)
    {
        foreach (var name in OptionNamespaces)
        {
            if (GoogleJson.TryObject(providerOptions, name, out var options))
            {
                return
                    options.TryGetProperty("wordTimestamp", out var value)
                    && value.ValueKind is JsonValueKind.True or JsonValueKind.False
                    ? value.GetBoolean()
                    : null;
            }
        }

        return null;
    }

    private static JsonObject? TranscriptionConfig(
        JsonElement providerOptions,
        bool forceWordTimestamp
    )
    {
        var config = new JsonObject();
        foreach (var name in OptionNamespaces)
        {
            if (!GoogleJson.TryObject(providerOptions, name, out var options))
            {
                continue;
            }

            foreach (var field in ConfigFields)
            {
                if (options.TryGetProperty(field, out var value))
                {
                    GoogleJson.Set(config, field, GoogleJson.Clone(value));
                }
            }

            break;
        }

        if (forceWordTimestamp)
        {
            config["wordTimestamp"] = true;
        }

        return config.Count > 0 ? config : null;
    }

    /// <summary>Maps to <c>createVertexLiveTranscriptionStream</c>.</summary>
    private sealed class LiveSession
    {
        private readonly TranscriptionStreamCall _call;
        private readonly JsonObject _setup;
        private readonly int _inputAudioRate;
        private readonly int _finishGraceMs;
        private readonly IReadOnlyList<OperationWarning> _warnings;
        private readonly object _gate = new object();
        private readonly TaskCompletionSource<bool> _setupComplete = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        private WebSocketConnection? _connection;
        private AudioChunkReader? _reader;
        private Timer? _finishTimer;
        private int _finishTimerGeneration;
        private bool _finished;
        private bool _audioEnded;
        private int _segmentCounter;
        private string _segmentBuffer = string.Empty;
        private string _fullText = string.Empty;
        private string _latestInterim = string.Empty;
        private string? _language;
        private JsonElement? _usageMetadata;

        public LiveSession(
            TranscriptionStreamCall call,
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
                        FinishWithError(
                            new AiSdkException(
                                "Vertex Live transcription error."
                                    + (
                                        webSocket is null
                                            ? " Note: the default WebSocket implementation may not send the Authorization header required by Vertex."
                                            : string.Empty
                                    )
                            )
                        ),
                    OnClose = OnClose,
                }
            );

            // An abort during Connect finishes the session before the connection is published. Close it then.
            bool alreadyFinished;
            lock (_gate)
            {
                alreadyFinished = _finished;
                _connection = connection;
            }

            if (alreadyFinished)
            {
                connection.Close(1000);
            }
        }

        public void Cancel()
        {
            if (!TryFinish())
            {
                return;
            }

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

        private void Cleanup(int? closeCode = null)
        {
            AudioChunkReader? reader;
            lock (_gate)
            {
                _finishTimer?.Dispose();
                _finishTimer = null;
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
                    // Pre-open failure or abort: cancel the caller's audio stream so an upstream producer
                    // piping into it does not hang.
                    _call.Audio.Cancel();
                }
            }
            catch (Exception)
            {
                // A stream that is locked by its owner stays with its owner.
            }

            _connection?.Close(closeCode);
        }

        private void FinishWithError(Exception error)
        {
            if (!TryFinish())
            {
                return;
            }

            Queue.Complete(error);
            Cleanup();
        }

        // Trailing transcripts can arrive after audioStreamEnd. Without a terminal signal, finish after a quiet
        // grace window. Transcript activity reschedules the timer.
        private void SchedulePendingFinish()
        {
            lock (_gate)
            {
                if (_finished || !_audioEnded)
                {
                    return;
                }

                _finishTimer?.Dispose();
                var generation = ++_finishTimerGeneration;
                _finishTimer = new Timer(
                    _ => FinishFromTimer(generation),
                    null,
                    _finishGraceMs,
                    Timeout.Infinite
                );
            }
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
                    Id = SegmentId(),
                    Text = _segmentBuffer,
                }
            );
            _fullText += _fullText.Length == 0 ? _segmentBuffer : " " + _segmentBuffer;
            _segmentBuffer = string.Empty;
            _segmentCounter++;
        }

        private string SegmentId()
        {
            return "google-segment-" + _segmentCounter.ToString(CultureInfo.InvariantCulture);
        }

        // Timer.Dispose does not stop a callback that is already queued: ignore callbacks of replaced timers.
        private void FinishFromTimer(int generation)
        {
            lock (_gate)
            {
                if (generation != _finishTimerGeneration)
                {
                    return;
                }

                Finish();
            }
        }

        private void Finish()
        {
            lock (_gate)
            {
                if (_finished)
                {
                    return;
                }

                CompleteSegment();
                _finished = true;
            }

            OperationUsage? usage = null;
            JsonElement? metadata = null;
            if (_usageMetadata is { } raw)
            {
                usage = new OperationUsage(
                    inputTokens: ReadInt(raw, "promptTokenCount"),
                    outputTokens: ReadInt(raw, "candidatesTokenCount"),
                    totalTokens: ReadInt(raw, "totalTokenCount")
                );
                metadata = JsonSerializer.SerializeToElement(
                    new JsonObject
                    {
                        ["google"] = new JsonObject
                        {
                            ["usageMetadata"] = JsonNode.Parse(raw.GetRawText()),
                        },
                    }
                );
            }

            Queue.Enqueue(
                new ModelStreamPart
                {
                    Type = "finish",
                    Text = _fullText,
                    Segments = new List<TranscriptSegment>(),
                    Language = _language,
                    Usage = usage,
                    UsageValue = _usageMetadata,
                    ProviderMetadata = metadata,
                }
            );
            Queue.Complete(null);
            Cleanup(1000);
        }

        private void OnOpen(IWebSocketLike socket)
        {
            // The finished check and the setup send share the lifecycle lock, so completion cannot land between them.
            lock (_gate)
            {
                if (_finished)
                {
                    return;
                }

                Queue.Enqueue(new ModelStreamPart { Type = "stream-start", Warnings = _warnings });
                socket.Send(new JsonObject { ["setup"] = _setup.DeepClone() }.ToJsonString());
            }

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
                                        + _inputAudioRate.ToString(CultureInfo.InvariantCulture),
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
                    _reader = null;
                }
            }

            if (!IsFinished)
            {
                socket.Send("{\"realtimeInput\":{\"audioStreamEnd\":true}}");
                lock (_gate)
                {
                    _audioEnded = true;
                }

                SchedulePendingFinish();
            }
        }

        // Message handling, segment emission, and timer completion share one lock, so a finished segment is
        // never emitted twice.
        private Task OnMessageTextAsync(string text)
        {
            lock (_gate)
            {
                return ProcessMessage(text);
            }
        }

        private Task ProcessMessage(string text)
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

            if (message.TryGetProperty("setupComplete", out var setup) && IsPresent(setup))
            {
                _setupComplete.TrySetResult(true);
            }

            if (
                message.TryGetProperty("usageMetadata", out var usage)
                && usage.ValueKind == JsonValueKind.Object
            )
            {
                _usageMetadata = usage.Clone();
            }

            if (
                message.TryGetProperty("error", out var error)
                && error.ValueKind == JsonValueKind.Object
            )
            {
                FinishWithError(
                    new AiSdkException(
                        GoogleJson.String(error, "message") ?? "Vertex Live API error"
                    )
                );
                return Task.CompletedTask;
            }

            JsonElement? serverContent = GoogleJson.TryObject(
                message,
                "serverContent",
                out var content
            )
                ? content
                : null;

            // Low-latency revisable transcription while the user is speaking.
            if (
                serverContent is { } sc
                && GoogleJson.TryObject(sc, "interimInputTranscription", out var interim)
                && GoogleJson.String(interim, "text") is { Length: > 0 } interimText
            )
            {
                SchedulePendingFinish();
                _latestInterim = interimText;
                Queue.Enqueue(
                    new ModelStreamPart
                    {
                        Type = "transcript-partial",
                        Id = SegmentId(),
                        Text = interimText,
                    }
                );
            }

            JsonElement? transcription = null;
            if (
                serverContent is { } sc2
                && GoogleJson.TryObject(sc2, "inputTranscription", out var inner)
            )
            {
                transcription = inner;
            }
            else if (GoogleJson.TryObject(message, "inputTranscription", out var outer))
            {
                transcription = outer;
            }

            if (transcription is { } t)
            {
                if (GoogleJson.String(t, "languageCode") is { } languageCode)
                {
                    _language = languageCode;
                }

                if (GoogleJson.String(t, "text") is { Length: > 0 } delta)
                {
                    SchedulePendingFinish();
                    // A real transcription delta supersedes interim fallback text.
                    _latestInterim = string.Empty;
                    _segmentBuffer += delta;
                    Queue.Enqueue(
                        new ModelStreamPart
                        {
                            Type = "transcript-delta",
                            Id = SegmentId(),
                            Delta = delta,
                        }
                    );
                }

                if (
                    t.TryGetProperty("finished", out var finished)
                    && finished.ValueKind == JsonValueKind.True
                )
                {
                    CompleteSegment();
                }
            }

            var turnComplete =
                serverContent is { } sc3
                && sc3.TryGetProperty("turnComplete", out var tc)
                && tc.ValueKind == JsonValueKind.True;
            if (turnComplete)
            {
                CompleteSegment();
            }

            // interactionStatus idle (REQUIRES_ACTION in the EAP builds) is the definitive
            // all-processing-complete signal: finish as soon as the input audio has ended.
            var interactionStatus = serverContent is { } sc4
                ? GoogleJson.String(sc4, "interactionStatus")
                : null;
            bool ended;
            lock (_gate)
            {
                ended = _audioEnded;
            }

            if (
                ended
                && (
                    interactionStatus == "IDLE"
                    || interactionStatus == "REQUIRES_ACTION"
                    || (turnComplete && interactionStatus is null)
                )
            )
            {
                Finish();
            }

            return Task.CompletedTask;
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

                ended = _audioEnded;
            }

            // A close after the input audio ended means the server delivered everything it will deliver.
            if (ended)
            {
                Finish();
                return;
            }

            FinishWithError(
                new AiSdkException(
                    "Vertex Live transcription WebSocket closed unexpectedly before finishing (code "
                        + (
                            code is { } value
                                ? value.ToString(CultureInfo.InvariantCulture)
                                : "unknown"
                        )
                        + (string.IsNullOrEmpty(reason) ? string.Empty : ", reason: " + reason)
                        + ")."
                )
            );
        }

        private static bool IsPresent(JsonElement value)
        {
            return value.ValueKind != JsonValueKind.Null
                && value.ValueKind != JsonValueKind.Undefined;
        }

        private static int? ReadInt(JsonElement element, string name)
        {
            return
                element.TryGetProperty(name, out var value)
                && value.ValueKind == JsonValueKind.Number
                && value.TryGetInt32(out var number)
                ? number
                : (int?)null;
        }
    }
}
