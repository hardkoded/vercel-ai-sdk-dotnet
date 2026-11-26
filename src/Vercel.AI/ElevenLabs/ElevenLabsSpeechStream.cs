// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.ElevenLabs;

/// <summary>Text to send, with the speaker it belongs to. Maps to <c>ElevenLabsTextSegment</c>.</summary>
internal sealed class ElevenLabsTextSegment
{
    /// <summary>Creates a segment.</summary>
    public ElevenLabsTextSegment(string text, string voice, bool newTurn = false)
    {
        Text = text;
        Voice = voice;
        NewTurn = newTurn;
    }

    /// <summary>The text to send.</summary>
    public string Text { get; }

    /// <summary>The resolved voice (see <see cref="ElevenLabsStreamProtocol.ResolveVoice"/>).</summary>
    public string Voice { get; }

    /// <summary>Forces a new dialogue turn even when the voice is unchanged.</summary>
    public bool NewTurn { get; set; }
}

/// <summary>One decoded server message, normalized across the WebSockets. Maps to <c>ElevenLabsStreamEvent</c>.</summary>
internal sealed class ElevenLabsStreamEvent
{
    /// <summary>Base64 audio.</summary>
    public string? Audio { get; init; }

    /// <summary>Server error text.</summary>
    public string? Error { get; init; }

    /// <summary>True on the final message.</summary>
    public bool IsFinal { get; init; }

    /// <summary>Metadata for the audio part.</summary>
    public JsonObject? ProviderMetadata { get; init; }
}

/// <summary>The messages for some text, and the characters they carry.</summary>
internal sealed class ElevenLabsEncodedText
{
    /// <summary>Creates the encoded text.</summary>
    public ElevenLabsEncodedText(IReadOnlyList<JsonObject> messages, int characters)
    {
        Messages = messages;
        Characters = characters;
    }

    /// <summary>Messages to send.</summary>
    public IReadOnlyList<JsonObject> Messages { get; }

    /// <summary>Characters (code points) billed.</summary>
    public int Characters { get; }
}

/// <summary>
/// Wire protocol for one ElevenLabs streaming TTS WebSocket. <c>ResolveVoice</c> applies the default voice to a text part
/// and throws for a voice the socket cannot speak; <c>EncodeText</c> returns the messages for a text segment and the
/// characters they carry. Maps to <c>ElevenLabsStreamProtocol</c>.
/// </summary>
internal sealed class ElevenLabsStreamProtocol
{
    /// <summary>The first message.</summary>
    public JsonObject InitMessage { get; init; } = null!;

    /// <summary>Init-message fields that authenticate the socket with the API key.</summary>
    public Func<string, JsonObject> AuthFields { get; init; } = null!;

    /// <summary>Resets the server's inactivity timeout without generating audio.</summary>
    public JsonObject KeepaliveMessage { get; init; } = null!;

    /// <summary>Applies the default voice to a text part.</summary>
    public Func<string?, string> ResolveVoice { get; init; } = null!;

    /// <summary>Returns the messages for a text segment.</summary>
    public Func<ElevenLabsTextSegment, ElevenLabsEncodedText> EncodeText { get; init; } = null!;

    /// <summary>Returns the end-of-input messages.</summary>
    public Func<ElevenLabsEncodedText> EndMessages { get; init; } = null!;

    /// <summary>Decodes one server message.</summary>
    public Func<JsonElement, ElevenLabsStreamEvent> DecodeEvent { get; init; } = null!;
}

/// <summary>Maps to <c>elevenlabs-speech-stream.ts</c>.</summary>
internal static class ElevenLabsSpeechStream
{
    /// <summary>
    /// Milliseconds of send inactivity after which a keepalive is sent, ahead of the servers' default 20 s inactivity
    /// timeout. Maps to <c>KEEPALIVE_INTERVAL_MS</c>.
    /// </summary>
    internal const int KeepaliveIntervalMs = 15_000;

    // Bounds buffering for scripts without spaces (e.g. CJK) or very long words.
    private const int MaxHeldCodePoints = 100;

    /// <summary>Counts Unicode code points, the unit ElevenLabs bills text-to-speech in. Maps to <c>countCodePoints</c>.</summary>
    internal static int CountCodePoints(string text)
    {
        var count = 0;
        for (var index = 0; index < text.Length; index++)
        {
            if (char.IsHighSurrogate(text[index]) && index + 1 < text.Length)
            {
                if (char.IsLowSurrogate(text[index + 1]))
                {
                    index++;
                }
            }

            count++;
        }

        return count;
    }

    /// <summary>
    /// Splits buffered text at the last word boundary. Returns the text to send now (possibly empty) and the text to keep
    /// buffering. Maps to <c>takeCompleteWords</c>.
    /// </summary>
    internal static (string Send, string Hold) TakeCompleteWords(string buffered)
    {
        for (var index = buffered.Length - 1; index >= 0; index--)
        {
            if (IsWordBoundary(buffered[index]))
            {
                return (buffered.Substring(0, index + 1), buffered.Substring(index + 1));
            }
        }

        return CountCodePoints(buffered) >= MaxHeldCodePoints
            ? (buffered, string.Empty)
            : (string.Empty, buffered);
    }

    /// <summary>Reads <c>providerOptions.elevenlabs.newTurn</c> from a text part. Maps to <c>getNewTurn</c>.</summary>
    internal static bool GetNewTurn(SpeechTextPart part)
    {
        if (
            part.ProviderOptions is { ValueKind: JsonValueKind.Object } bag
            && bag.TryGetProperty("elevenlabs", out var elevenlabs)
            && elevenlabs.ValueKind == JsonValueKind.Object
            && elevenlabs.TryGetProperty("newTurn", out var newTurn)
            && newTurn.ValueKind != JsonValueKind.Null
        )
        {
            if (newTurn.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            {
                throw new InvalidArgumentError(
                    "providerOptions.elevenlabs.newTurn",
                    newTurn.ToString(),
                    "providerOptions.elevenlabs.newTurn must be a boolean."
                );
            }

            return newTurn.GetBoolean();
        }

        return false;
    }

    /// <summary>
    /// Maps an ElevenLabs <c>output_format</c> (e.g. <c>mp3_44100_128</c>, <c>pcm_24000</c>) to the format of the audio
    /// chunks it produces. Maps to <c>getElevenLabsAudioFormat</c>.
    /// </summary>
    internal static SpeechAudioFormat GetElevenLabsAudioFormat(string outputFormat)
    {
        var parts = outputFormat.Split('_');
        var codec = parts[0];
        int? rate =
            parts.Length > 1
            && double.TryParse(
                parts[1],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var parsed
            )
                ? (int)parsed
                : null;
        return codec switch
        {
            "mp3" => new SpeechAudioFormat("audio/mpeg", rate),
            "pcm" => new SpeechAudioFormat("audio/pcm", rate),
            "ulaw" => new SpeechAudioFormat("audio/pcmu", rate),
            "alaw" => new SpeechAudioFormat("audio/pcma", rate),
            // ElevenLabs Opus audio arrives in an Ogg container.
            "opus" => new SpeechAudioFormat("audio/ogg", rate),
            "wav" => new SpeechAudioFormat("audio/wav", rate),
            _ => new SpeechAudioFormat("application/octet-stream"),
        };
    }

    /// <summary><c>stream-input</c>: one implicit context and one voice; <c>{"text":""}</c> ends the input. Maps to <c>createSingleContextProtocol</c>.</summary>
    internal static ElevenLabsStreamProtocol CreateSingleContextProtocol(
        JsonObject initMessage,
        string voice
    )
    {
        return new ElevenLabsStreamProtocol
        {
            InitMessage = initMessage,
            AuthFields = apiKey => new JsonObject { ["xi-api-key"] = apiKey },
            KeepaliveMessage = new JsonObject { ["text"] = " " },
            ResolveVoice = partVoice =>
            {
                if (partVoice != null && partVoice != voice)
                {
                    throw new InvalidArgumentError(
                        "text",
                        partVoice,
                        "The ElevenLabs text-to-speech WebSocket speaks a single voice per stream (\""
                            + voice
                            + "\"); a text part requested \""
                            + partVoice
                            + "\". Use an eleven_v3 or eleven_v4 model with dialogue options for several voices."
                    );
                }

                return voice;
            },
            EncodeText = segment => new ElevenLabsEncodedText(
                [new JsonObject { ["text"] = segment.Text }],
                CountCodePoints(segment.Text)
            ),
            EndMessages = () =>
                new ElevenLabsEncodedText([new JsonObject { ["text"] = string.Empty }], 0),
            DecodeEvent = DecodeTextToSpeechEvent,
        };
    }

    /// <summary>
    /// Text-to-Dialogue <c>stream-input</c>. Each text part's <c>voice</c> (default <paramref name="defaultVoice"/>) must be
    /// one of <paramref name="voices"/>; a voice change, or a part with <c>newTurn</c>, starts a new turn. With
    /// <paramref name="incremental"/> false (the v3 family) all inputs go in one final message. Maps to
    /// <c>createDialogueProtocol</c>.
    /// </summary>
    internal static ElevenLabsStreamProtocol CreateDialogueProtocol(
        JsonObject initMessage,
        IReadOnlyList<string> voices,
        string defaultVoice,
        bool incremental
    )
    {
        var buffered = new List<JsonObject>();
        string? currentVoice = null;

        string ResolveVoice(string? partVoice)
        {
            var voice = partVoice ?? defaultVoice;
            if (!voices.Contains(voice))
            {
                throw new InvalidArgumentError(
                    "text",
                    voice,
                    "Dialogue text part voice \""
                        + voice
                        + "\" is not listed in providerOptions.elevenlabs.dialogue.voices."
                );
            }

            return voice;
        }

        ElevenLabsEncodedText Emit(ElevenLabsTextSegment segment)
        {
            var voice = segment.Voice;
            var newTurn = currentVoice != null && (voice != currentVoice || segment.NewTurn);
            currentVoice = voice;
            var input = new JsonObject { ["text"] = segment.Text, ["voice_id"] = voice };
            if (newTurn)
            {
                input["new_turn"] = true;
            }

            var characters = CountCodePoints(segment.Text);
            if (!incremental)
            {
                buffered.Add(input);
                return new ElevenLabsEncodedText([], characters);
            }

            return new ElevenLabsEncodedText(
                [new JsonObject { ["inputs"] = new JsonArray(input) }],
                characters
            );
        }

        var init = (JsonObject)initMessage.DeepClone();
        init["voices"] = new JsonArray(voices.Select(voice => (JsonNode?)voice).ToArray());
        return new ElevenLabsStreamProtocol
        {
            InitMessage = init,
            AuthFields = apiKey => new JsonObject { ["xi_api_key"] = apiKey },
            KeepaliveMessage = new JsonObject { ["keep_alive"] = true },
            ResolveVoice = ResolveVoice,
            EncodeText = Emit,
            EndMessages = () =>
            {
                if (incremental)
                {
                    return new ElevenLabsEncodedText(
                        [new JsonObject { ["close_socket"] = true }],
                        0
                    );
                }

                return new ElevenLabsEncodedText(
                    [
                        new JsonObject
                        {
                            ["inputs"] = new JsonArray(
                                buffered.Select(i => (JsonNode?)i).ToArray()
                            ),
                            ["close_socket"] = true,
                        },
                    ],
                    0
                );
            },
            DecodeEvent = raw =>
            {
                var error = GetErrorText(raw);
                if (error != null)
                {
                    return new ElevenLabsStreamEvent { Error = error };
                }

                var alignment = CamelizeAlignment(Property(raw, "alignment"));
                var isFinalAudioForTurn =
                    Property(raw, "is_final_audio_for_turn") is { ValueKind: JsonValueKind.True };
                JsonObject? metadata = null;
                if (alignment != null || isFinalAudioForTurn)
                {
                    metadata = new JsonObject();
                    if (alignment != null)
                    {
                        metadata["alignment"] = alignment;
                    }

                    if (isFinalAudioForTurn)
                    {
                        metadata["isFinalAudioForTurn"] = true;
                    }
                }

                return new ElevenLabsStreamEvent
                {
                    Audio = AudioText(raw),
                    IsFinal = Property(raw, "is_final") is { ValueKind: JsonValueKind.True },
                    ProviderMetadata = metadata,
                };
            },
        };
    }

    /// <summary>
    /// Streams text-to-speech over an ElevenLabs WebSocket using the protocol: an init message carrying the API key, text
    /// parts ending at word boundaries (held back per speaker), keepalives while idle, end-of-input messages, then audio
    /// until the final event. Maps to <c>createElevenLabsSpeechStream</c>.
    /// </summary>
    internal static IAsyncEnumerable<SpeechModelStreamPart> CreateElevenLabsSpeechStream(
        ElevenLabsSpeechStreamOptions options
    )
    {
        var session = new ElevenLabsSpeechStreamSession(options);
        session.Start();
        return Read(session);
    }

    private static async IAsyncEnumerable<SpeechModelStreamPart> Read(
        ElevenLabsSpeechStreamSession session,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        try
        {
            while (await session.Queue.WaitAsync(cancellationToken).ConfigureAwait(false))
            {
                yield return session.Queue.Dequeue();
            }
        }
        finally
        {
            // A consumer that stops early, or a cancel, releases the socket and the text stream.
            session.Cancel();
        }
    }

    private static bool IsWordBoundary(char value)
    {
        // Deltas are held back to the last word boundary: the WebSockets expect text messages to end at whitespace, and
        // LLM deltas often split words. CJK sentence punctuation also counts, since those scripts have no spaces.
        return char.IsWhiteSpace(value)
            || value == '﻿'
            || value is '。' or '、' or '！' or '？' or '，' or '；' or '：';
    }

    private static ElevenLabsStreamEvent DecodeTextToSpeechEvent(JsonElement raw)
    {
        var error = GetErrorText(raw);
        if (error != null && Property(raw, "audio") is not { ValueKind: JsonValueKind.String })
        {
            return new ElevenLabsStreamEvent { Error = error };
        }

        var metadata = new JsonObject();
        if (Property(raw, "alignment") is { } alignment)
        {
            metadata["alignment"] = JsonNode.Parse(alignment.GetRawText());
        }

        if (Property(raw, "normalizedAlignment") is { } normalizedAlignment)
        {
            metadata["normalizedAlignment"] = JsonNode.Parse(normalizedAlignment.GetRawText());
        }

        return new ElevenLabsStreamEvent
        {
            Audio = AudioText(raw),
            IsFinal =
                Property(raw, "isFinal") is { ValueKind: JsonValueKind.True }
                || Property(raw, "is_final") is { ValueKind: JsonValueKind.True },
            ProviderMetadata = metadata.Count > 0 ? metadata : null,
        };
    }

    private static string? AudioText(JsonElement raw)
    {
        return
            Property(raw, "audio") is { ValueKind: JsonValueKind.String } audio
            && audio.GetString() is { Length: > 0 } text
            ? text
            : null;
    }

    private static string? GetErrorText(JsonElement raw)
    {
        var error = Property(raw, "error");
        var message = Property(raw, "message");
        var isFinal = Property(raw, "isFinal") is { ValueKind: JsonValueKind.True };
        if (error == null && (message is not { ValueKind: JsonValueKind.String } || isFinal))
        {
            return null;
        }

        if (message is { ValueKind: JsonValueKind.String } messageText)
        {
            return messageText.GetString();
        }

        return error is { ValueKind: JsonValueKind.String } errorText
            ? errorText.GetString()
            : error?.GetRawText();
    }

    // Dialogue alignment is snake_case on the wire; expose the TTS (camelCase) shape.
    private static JsonObject? CamelizeAlignment(JsonElement? value)
    {
        if (value is not { ValueKind: JsonValueKind.Object } alignment)
        {
            return null;
        }

        var result = new JsonObject();
        if (Property(alignment, "chars") is { } chars)
        {
            result["chars"] = JsonNode.Parse(chars.GetRawText());
        }

        if (Property(alignment, "char_start_times_ms") is { } starts)
        {
            result["charStartTimesMs"] = JsonNode.Parse(starts.GetRawText());
        }

        if (Property(alignment, "char_durations_ms") is { } durations)
        {
            result["charDurationsMs"] = JsonNode.Parse(durations.GetRawText());
        }

        return result;
    }

    // A missing property and a JSON null both read as undefined, like `== null` upstream.
    private static JsonElement? Property(JsonElement parent, string name)
    {
        return
            parent.ValueKind == JsonValueKind.Object
            && parent.TryGetProperty(name, out var value)
            && value.ValueKind != JsonValueKind.Null
            ? value
            : null;
    }
}

/// <summary>Options of <c>createElevenLabsSpeechStream</c>.</summary>
internal sealed class ElevenLabsSpeechStreamOptions
{
    /// <summary>Aborts the stream. Maps to <c>abortSignal</c>.</summary>
    public CancellationToken AbortSignal { get; init; }

    /// <summary>Sent in the init message, so native (header-less) WebSockets work.</summary>
    public string? ApiKey { get; init; }

    /// <summary>Format of the audio chunks.</summary>
    public SpeechAudioFormat AudioFormat { get; init; } = null!;

    /// <summary>Handshake headers.</summary>
    public IReadOnlyDictionary<string, string?> Headers { get; init; } = null!;

    /// <summary>When true, raw server messages are emitted.</summary>
    public bool? IncludeRawChunks { get; init; }

    /// <summary>Send keepalives while the text stream is idle.</summary>
    public bool Keepalive { get; init; }

    /// <summary>Milliseconds of send inactivity before a keepalive.</summary>
    public int KeepaliveIntervalMs { get; init; } = ElevenLabsSpeechStream.KeepaliveIntervalMs;

    /// <summary>The wire protocol.</summary>
    public ElevenLabsStreamProtocol Protocol { get; init; } = null!;

    /// <summary>The text to speak.</summary>
    public SpeechTextStream Text { get; init; } = null!;

    /// <summary>The WebSocket URL.</summary>
    public Uri Url { get; init; } = null!;

    /// <summary>Warnings of the <c>stream-start</c> part.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; init; } = null!;

    /// <summary>Custom WebSocket implementation.</summary>
    public WebSocketFactory? WebSocket { get; init; }

    /// <summary>Runs once when the session ends, whether or not the output is read. Releases what the caller linked to <see cref="AbortSignal"/>.</summary>
    public Action? OnStreamEnded { get; init; }
}

/// <summary>The state of one <see cref="ElevenLabsSpeechStream.CreateElevenLabsSpeechStream"/> call.</summary>
internal sealed class ElevenLabsSpeechStreamSession
{
    private readonly ElevenLabsSpeechStreamOptions _options;
    private readonly ElevenLabsStreamProtocol _protocol;
    private readonly object _gate = new object();
    private readonly object _sendGate = new object();
    private readonly CancellationTokenSource _textCancellation = new CancellationTokenSource();
    private readonly object _textGate = new object();
    private bool _textDisposed;
    private Task? _pendingRead;
    private WebSocketConnection? _connection;
    private Timer? _keepaliveTimer;
    private bool _finished;
    private bool _inputEnded;
    private bool _textStarted;
    private int _inputCharacters;

    public ElevenLabsSpeechStreamOptions Options => _options;

    public ElevenLabsSpeechStreamSession(ElevenLabsSpeechStreamOptions options)
    {
        _options = options;
        _protocol = options.Protocol;
    }

    public PartQueue<SpeechModelStreamPart> Queue { get; } = new PartQueue<SpeechModelStreamPart>();

    public void Start()
    {
        WebSocketConnection connection;
        try
        {
            connection = ConnectToWebSocket.Connect(
                new ConnectToWebSocketOptions
                {
                    Url = _options.Url,
                    Headers = _options.Headers,
                    WebSocket = _options.WebSocket,
                    AbortSignal = _options.AbortSignal,
                    OnAbort = FinishWithError,
                    OnOpen = OnOpen,
                    OnMessageText = OnMessageTextAsync,
                    OnProcessingError = FinishWithError,
                    OnSocketError = () =>
                        FinishWithError(new AiSdkException("ElevenLabs speech stream error.")),
                    OnClose = (code, reason) =>
                    {
                        if (IsFinished)
                        {
                            return;
                        }

                        FinishWithError(
                            new AiSdkException(
                                "ElevenLabs speech stream closed before completion"
                                    + (
                                        code != null
                                            ? " (code "
                                                + code
                                                + (
                                                    string.IsNullOrEmpty(reason)
                                                        ? ""
                                                        : ": " + reason
                                                )
                                                + ")"
                                            : string.Empty
                                    )
                                    + "."
                            )
                        );
                    },
                }
            );
        }
        catch (Exception error)
        {
            FinishWithError(error);
            return;
        }

        bool finished;
        lock (_gate)
        {
            finished = _finished;
            if (!finished)
            {
                _connection = connection;
            }
        }

        // An abort that finished the session first leaves no published connection, so cleanup could not close it.
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
        NotifyEnded();
    }

    private bool InputEnded
    {
        get
        {
            lock (_gate)
            {
                return _inputEnded;
            }
        }
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
        WebSocketConnection? connection;
        bool textStarted;
        lock (_gate)
        {
            connection = _connection;
            textStarted = _textStarted;
            _keepaliveTimer?.Dispose();
            _keepaliveTimer = null;
        }

        CancelText();
        if (!textStarted)
        {
            // No sender will run, so nothing else uses the source.
            DisposeText();
            // No reader took the text yet: release it, so an upstream producer piping into it does not hang.
            _ = ReleaseTextAsync();
        }

        connection?.Close(closeCode);
    }

    private async Task ReleaseTextAsync()
    {
        try
        {
            await _options.Text.CancelAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // A stream owned by the caller stays with its owner.
        }
    }

    private void FinishWithError(Exception error)
    {
        if (!TryFinish())
        {
            return;
        }

        Cleanup(null);
        Queue.Complete(error);
        NotifyEnded();
    }

    private void Finish()
    {
        if (!TryFinish())
        {
            return;
        }

        Queue.Enqueue(
            new SpeechModelStreamPart
            {
                Type = "finish",
                Usage = new SpeechStreamUsage { InputCharacters = _inputCharacters },
            }
        );
        Queue.Complete(null);
        Cleanup(1000);
        NotifyEnded();
    }

    // Runs once per session, from whichever terminal path wins, whether or not the output is ever read.
    private void NotifyEnded()
    {
        try
        {
            _options.OnStreamEnded?.Invoke();
        }
        catch (Exception)
        {
            // Releasing what the caller linked must not fail the stream.
        }
    }

    // Every send restarts the idle clock; a slow text producer (e.g. a reasoning model) would otherwise hit the server's
    // inactivity timeout. No keepalive follows the end of input.
    private void Send(IWebSocketLike socket, JsonObject message)
    {
        lock (_sendGate)
        {
            socket.Send(message.ToJsonString());
            lock (_gate)
            {
                _keepaliveTimer?.Dispose();
                _keepaliveTimer = null;
                if (!_options.Keepalive || _inputEnded || _finished)
                {
                    return;
                }

                _keepaliveTimer = new Timer(
                    _ =>
                    {
                        try
                        {
                            lock (_sendGate)
                            {
                                if (IsFinished || InputEnded)
                                {
                                    return;
                                }

                                Send(socket, _protocol.KeepaliveMessage);
                            }
                        }
                        catch (Exception error)
                        {
                            FinishWithError(error);
                        }
                    },
                    null,
                    _options.KeepaliveIntervalMs,
                    Timeout.Infinite
                );
            }
        }
    }

    private void OnOpen(IWebSocketLike socket)
    {
        Queue.Enqueue(
            new SpeechModelStreamPart { Type = "stream-start", Warnings = _options.Warnings }
        );
        var init = (JsonObject)_protocol.InitMessage.DeepClone();
        if (_options.ApiKey != null)
        {
            foreach (var field in _protocol.AuthFields(_options.ApiKey))
            {
                init[field.Key] = field.Value?.DeepClone();
            }
        }

        Send(socket, init);
        _ = SendTextAsync(socket)
            .ContinueWith(
                task => FinishWithError(task.Exception!.GetBaseException()),
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default
            );
    }

    private async Task SendAllAsync(IWebSocketLike socket, ElevenLabsEncodedText encoded)
    {
        Interlocked.Add(ref _inputCharacters, encoded.Characters);
        foreach (var message in encoded.Messages)
        {
            Send(socket, message);
            await WebSocketUtils
                .WaitForWebSocketBufferDrainAsync(
                    socket,
                    cancellationToken: _textCancellation.Token
                )
                .ConfigureAwait(false);
        }
    }

    // Text is held back per segment (speaker + turn): a part with another voice or a new turn first flushes the held text
    // of the previous one. An empty text message ends the input, so never send one here.
    private async Task SendTextAsync(IWebSocketLike socket)
    {
        try
        {
            await SendTextCoreAsync(socket).ConfigureAwait(false);
        }
        finally
        {
            // Cancel first, so a failure here still releases a source that is waiting on the token.
            CancelText();
            var pending = _pendingRead;
            if (pending == null)
            {
                DisposeText();
            }
            else
            {
                // The source ignored the cancel and is still reading with the token: dispose once the read settles.
                _ = DisposeTextAfterAsync(pending);
            }
        }
    }

    private async Task DisposeTextAfterAsync(Task read)
    {
        try
        {
            await read.ConfigureAwait(false);
        }
        catch (Exception)
        {
            // The read failed after the stream ended; nothing is left to report it to.
        }

        DisposeText();
    }

    // Cancel and dispose share one lock, because a CancellationTokenSource does not support concurrent disposal and use.
    private void CancelText()
    {
        lock (_textGate)
        {
            if (_textDisposed)
            {
                return;
            }

            try
            {
                _textCancellation.Cancel();
            }
            catch (Exception)
            {
                // A cancellation callback of the text source threw. The stream is already ending.
            }
        }
    }

    private void DisposeText()
    {
        lock (_textGate)
        {
            if (_textDisposed)
            {
                return;
            }

            _textDisposed = true;
            _textCancellation.Dispose();
        }
    }

    private async Task SendTextCoreAsync(IWebSocketLike socket)
    {
        ElevenLabsTextSegment? held = null;
        async Task FlushHeldAsync()
        {
            if (held != null && held.Text.Length > 0)
            {
                await SendAllAsync(socket, _protocol.EncodeText(held)).ConfigureAwait(false);
            }

            held = null;
        }

        IAsyncEnumerator<SpeechTextPart> enumerator;
        lock (_gate)
        {
            if (_finished)
            {
                return;
            }

            _textStarted = true;
            enumerator = _options.Text.GetAsyncEnumerator(_textCancellation.Token);
        }

        var next = default(Task<bool>);
        try
        {
            while (true)
            {
                next = enumerator.MoveNextAsync().AsTask();
                if (!await AwaitReadAsync(next).ConfigureAwait(false) || IsFinished)
                {
                    // A read that is still pending keeps the enumerator alive until it settles.
                    if (next.IsCompleted)
                    {
                        next = null;
                    }

                    break;
                }

                var value = enumerator.Current;
                next = null;
                var voice = _protocol.ResolveVoice(value.Voice);
                var newTurn = ElevenLabsSpeechStream.GetNewTurn(value);
                if (held != null && (held.Voice != voice || newTurn))
                {
                    await FlushHeldAsync().ConfigureAwait(false);
                }

                var segment = held ?? new ElevenLabsTextSegment(string.Empty, voice, newTurn);
                var (complete, hold) = ElevenLabsSpeechStream.TakeCompleteWords(
                    segment.Text + value.Text
                );
                if (complete.Length > 0)
                {
                    await SendAllAsync(
                            socket,
                            _protocol.EncodeText(
                                new ElevenLabsTextSegment(complete, segment.Voice, segment.NewTurn)
                            )
                        )
                        .ConfigureAwait(false);
                    // The rest of the segment continues the same turn.
                    segment.NewTurn = false;
                }

                held = new ElevenLabsTextSegment(hold, segment.Voice, segment.NewTurn);
            }
        }
        finally
        {
            if (next == null)
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                // The source ignored the cancel and is still reading: dispose it once the read ends.
                _pendingRead = next;
                _ = DisposeAfterAsync(next, enumerator);
            }
        }

        if (!IsFinished)
        {
            await FlushHeldAsync().ConfigureAwait(false);
            // Serialized with the keepalive timer, so no keepalive frame follows the end of input.
            lock (_sendGate)
            {
                lock (_gate)
                {
                    _inputEnded = true;
                }
            }

            await SendAllAsync(socket, _protocol.EndMessages()).ConfigureAwait(false);
        }
    }

    private static async Task DisposeAfterAsync(
        Task read,
        IAsyncEnumerator<SpeechTextPart> enumerator
    )
    {
        try
        {
            await read.ConfigureAwait(false);
        }
        catch (Exception)
        {
            // The read failed after the stream ended; nothing is left to report it to.
        }

        try
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // A source owned by the caller stays with its owner.
        }
    }

    // Returns false when the stream ended while the read was pending, so a source that ignores cancellation cannot hang it.
    private async Task<bool> AwaitReadAsync(Task<bool> read)
    {
        var cancelled = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        using (_textCancellation.Token.Register(() => cancelled.TrySetResult(true)))
        {
            var finished = await Task.WhenAny(read, cancelled.Task).ConfigureAwait(false);
            if (finished != read)
            {
                return false;
            }
        }

        try
        {
            return await read.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (_textCancellation.IsCancellationRequested)
        {
            return false;
        }
    }

    private Task OnMessageTextAsync(string text)
    {
        if (IsFinished)
        {
            return Task.CompletedTask;
        }

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

        if (_options.IncludeRawChunks == true)
        {
            Queue.Enqueue(new SpeechModelStreamPart { Type = "raw", RawValue = raw });
        }

        var decoded = _protocol.DecodeEvent(raw);
        if (decoded.Error != null)
        {
            FinishWithError(new AiSdkException("ElevenLabs speech stream error: " + decoded.Error));
            return Task.CompletedTask;
        }

        // Turn boundaries arrive as standalone frames, so they are emitted as metadata-only (empty) audio parts.
        var isFinalAudioForTurn =
            decoded.ProviderMetadata?["isFinalAudioForTurn"] is JsonValue flag
            && flag.TryGetValue<bool>(out var isTurnEnd)
            && isTurnEnd;
        if (decoded.Audio != null || isFinalAudioForTurn)
        {
            var part = new SpeechModelStreamPart
            {
                Type = "audio",
                AudioBase64 = decoded.Audio ?? string.Empty,
                AudioFormat = _options.AudioFormat,
            };
            if (decoded.ProviderMetadata != null)
            {
                part.ProviderMetadata = JsonSerializer.SerializeToElement(
                    new JsonObject { ["elevenlabs"] = decoded.ProviderMetadata.DeepClone() }
                );
            }

            Queue.Enqueue(part);
        }

        if (decoded.IsFinal)
        {
            Finish();
        }

        return Task.CompletedTask;
    }
}
