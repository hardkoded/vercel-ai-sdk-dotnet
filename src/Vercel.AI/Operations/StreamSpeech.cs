// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using Vercel.AI.Util;

namespace Vercel.AI.Operations;

/// <summary>Audio format of a speech stream, such as <c>audio/pcm</c> at 24000 Hz or <c>audio/mpeg</c>.</summary>
public sealed class SpeechAudioFormat
{
    /// <summary>Creates a format.</summary>
    public SpeechAudioFormat(string type, int? rate = null)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Rate = rate;
    }

    /// <summary>Audio format type, such as <c>audio/pcm</c>, <c>audio/pcmu</c>, or <c>audio/pcma</c>.</summary>
    public string Type { get; }

    /// <summary>Sample rate in Hz. Only applicable for formats that require a rate.</summary>
    public int? Rate { get; }
}

/// <summary>
/// One piece of streamed input text. Consecutive parts are synthesized in order as one continuous script.
/// Maps to <c>SpeechTextPart</c> and <c>SpeechModelV4StreamTextPart</c>.
/// </summary>
public sealed class SpeechTextPart
{
    /// <summary>Creates a text part.</summary>
    public SpeechTextPart(string text, string? voice = null, JsonElement? providerOptions = null)
    {
        Text = text ?? string.Empty;
        Voice = voice;
        ProviderOptions = providerOptions;
    }

    /// <summary>The text to synthesize. Parts may split anywhere, including mid-word.</summary>
    public string Text { get; }

    /// <summary>
    /// The voice that speaks this part (provider-specific, like the call's voice). Defaults to the call's voice.
    /// Single-voice providers fail the stream when it differs from the voice of the call.
    /// </summary>
    public string? Voice { get; }

    /// <summary>Additional provider-specific options for this part, keyed by provider name.</summary>
    public JsonElement? ProviderOptions { get; }

    /// <summary>Wraps a plain string as a text part.</summary>
    public static implicit operator SpeechTextPart(string text)
    {
        return new SpeechTextPart(text);
    }
}

/// <summary>
/// The text a speech model reads. The provider owns it once <c>doStream</c> is called: it must enumerate it to the end
/// or dispose its enumerator on every terminal path.
/// </summary>
public sealed class SpeechTextStream : IAsyncEnumerable<SpeechTextPart>
{
    private readonly IAsyncEnumerable<SpeechTextPart> _source;

    /// <summary>Creates a stream over <paramref name="source"/>.</summary>
    public SpeechTextStream(IAsyncEnumerable<SpeechTextPart> source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <summary>True after the model takes the enumerator.</summary>
    public bool Locked { get; private set; }

    /// <summary>Takes the single enumerator.</summary>
    public IAsyncEnumerator<SpeechTextPart> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        if (Locked)
        {
            throw new InvalidOperationException("The text stream is locked.");
        }

        Locked = true;
        return _source.GetAsyncEnumerator(cancellationToken);
    }

    /// <summary>Releases a stream no model took. A locked stream stays with the model.</summary>
    public async Task CancelAsync()
    {
        if (Locked)
        {
            return;
        }

        Locked = true;
        await _source.GetAsyncEnumerator().DisposeAsync().ConfigureAwait(false);
    }

    internal static async IAsyncEnumerable<SpeechTextPart> FromSync(IEnumerable<SpeechTextPart> parts, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var part in parts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return part;
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}

/// <summary>Usage of a speech stream call. Every field is optional because providers bill speech in different units.</summary>
public sealed class SpeechStreamUsage
{
    /// <summary>Number of input text characters (Unicode code points) sent for synthesis, if known.</summary>
    public int? InputCharacters { get; set; }

    /// <summary>Number of input text tokens, if reported.</summary>
    public int? InputTokens { get; set; }

    /// <summary>Number of output audio tokens, if reported.</summary>
    public int? OutputTokens { get; set; }

    /// <summary>Raw usage information from the provider, in the shape the provider returns it.</summary>
    public JsonElement? Raw { get; set; }
}

/// <summary>A part emitted by a streaming speech model. Maps to <c>SpeechModelV4StreamPart</c>.</summary>
public sealed class SpeechModelStreamPart
{
    /// <summary>Part type: <c>stream-start</c>, <c>audio</c>, <c>response-metadata</c>, <c>finish</c>, <c>raw</c>, or <c>error</c>.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Warnings carried by <c>stream-start</c>.</summary>
    public IReadOnlyList<OperationWarning>? Warnings { get; set; }

    /// <summary>Optional provider identifier for an <c>audio</c> chunk.</summary>
    public string? Id { get; set; }

    /// <summary>Raw audio bytes of an <c>audio</c> chunk.</summary>
    public byte[]? Audio { get; set; }

    /// <summary>Base64-encoded raw audio bytes of an <c>audio</c> chunk. Used when <see cref="Audio"/> is null.</summary>
    public string? AudioBase64 { get; set; }

    /// <summary>Format of an <c>audio</c> chunk.</summary>
    public SpeechAudioFormat? AudioFormat { get; set; }

    /// <summary>Provider metadata of an <c>audio</c> or <c>finish</c> part.</summary>
    public JsonElement? ProviderMetadata { get; set; }

    /// <summary>Response timestamp of a <c>response-metadata</c> part.</summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>Response model id of a <c>response-metadata</c> part.</summary>
    public string? ModelId { get; set; }

    /// <summary>Response headers of a <c>response-metadata</c> part.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; set; }

    /// <summary>Response body of a <c>response-metadata</c> part.</summary>
    public JsonElement? Body { get; set; }

    /// <summary>Usage of a <c>finish</c> part.</summary>
    public SpeechStreamUsage? Usage { get; set; }

    /// <summary>Raw provider chunk of a <c>raw</c> part.</summary>
    public JsonElement? RawValue { get; set; }

    /// <summary>Non-fatal error of an <c>error</c> part. The stream continues.</summary>
    public Exception? Error { get; set; }
}

/// <summary>A part of <see cref="StreamSpeechResult.FullStream"/>.</summary>
public sealed class SpeechStreamPart
{
    /// <summary>Part type: <c>audio</c>, <c>raw</c>, or <c>error</c>.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Provider identifier of an <c>audio</c> chunk.</summary>
    public string? Id { get; set; }

    /// <summary>
    /// Audio chunk in generation order. Chunks are not necessarily standalone files; concatenate them to reconstruct the audio.
    /// A chunk is empty only when it carries <see cref="ProviderMetadata"/> (for example a turn marker).
    /// </summary>
    public byte[] Audio { get; set; } = Array.Empty<byte>();

    /// <summary>Format of an <c>audio</c> chunk.</summary>
    public SpeechAudioFormat? AudioFormat { get; set; }

    /// <summary>Provider metadata of an <c>audio</c> chunk.</summary>
    public JsonElement? ProviderMetadata { get; set; }

    /// <summary>Raw provider chunk of a <c>raw</c> part.</summary>
    public JsonElement? RawValue { get; set; }

    /// <summary>Provider error of an <c>error</c> part.</summary>
    public Exception? Error { get; set; }
}

/// <summary>Arguments for <c>doStream</c> on a speech model. Maps to <c>SpeechModelV4StreamOptions</c>.</summary>
public sealed class SpeechModelStreamCall
{
    /// <summary>Creates a speech stream call.</summary>
    public SpeechModelStreamCall(SpeechTextStream text, string? voice, SpeechAudioFormat? outputFormat, string? instructions, double? speed, string? language, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, bool? includeRawChunks, CancellationToken cancellationToken)
    {
        Text = text;
        Voice = voice;
        OutputFormat = outputFormat;
        Instructions = instructions;
        Speed = speed;
        Language = language;
        ProviderOptions = providerOptions;
        Headers = headers;
        IncludeRawChunks = includeRawChunks;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// Text to convert to speech, as a stream of text parts. The provider owns it once <c>doStream</c> is called and
    /// releases it on every terminal path.
    /// </summary>
    public SpeechTextStream Text { get; }

    /// <summary>Voice to use. Provider-specific.</summary>
    public string? Voice { get; }

    /// <summary>Desired format of the output audio chunks. The provider default applies when null.</summary>
    public SpeechAudioFormat? OutputFormat { get; }

    /// <summary>Speaking instructions.</summary>
    public string? Instructions { get; }

    /// <summary>Playback speed.</summary>
    public double? Speed { get; }

    /// <summary>ISO 639-1 language code or <c>auto</c>.</summary>
    public string? Language { get; }

    /// <summary>Provider options. Empty object when omitted.</summary>
    public JsonElement ProviderOptions { get; }

    /// <summary>Headers including the user-agent suffix.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>When true, providers include raw provider chunks in the stream.</summary>
    public bool? IncludeRawChunks { get; }

    /// <summary>Cancels the operation. Also fires when the consumer cancels the result stream.</summary>
    public CancellationToken CancellationToken { get; }
}

/// <summary>Response data returned by <c>doStream</c>.</summary>
public sealed class SpeechModelStreamResponse
{
    /// <summary>Timestamp for the start of the streamed response.</summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>Id of the response model.</summary>
    public string? ModelId { get; set; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; set; }

    /// <summary>Response body.</summary>
    public JsonElement? Body { get; set; }
}

/// <summary>The result of a speech model <c>doStream</c> call. Maps to <c>SpeechModelV4StreamResult</c>.</summary>
public sealed class SpeechModelStreamResult
{
    /// <summary>Creates a stream result.</summary>
    public SpeechModelStreamResult(IAsyncEnumerable<SpeechModelStreamPart> stream, SpeechModelStreamResponse? response = null, JsonElement? requestBody = null)
    {
        Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        Response = response;
        RequestBody = requestBody;
    }

    /// <summary>The stream.</summary>
    public IAsyncEnumerable<SpeechModelStreamPart> Stream { get; }

    /// <summary>Optional response data.</summary>
    public SpeechModelStreamResponse? Response { get; }

    /// <summary>Request body or setup payload sent to the provider API.</summary>
    public JsonElement? RequestBody { get; }
}

/// <summary>
/// A speech model that can also stream speech audio for streamed text input.
/// Maps to <c>Experimental_StreamingSpeechModelV4</c>.
/// </summary>
public interface IStreamingSpeechCaller : ISpeechCaller
{
    /// <summary>Streams speech audio for streamed text input.</summary>
    Task<SpeechModelStreamResult> DoStreamAsync(SpeechModelStreamCall call, CancellationToken cancellationToken);
}

/// <summary>Resolves string speech model ids.</summary>
public static class SpeechModels
{
    /// <summary>Default provider factory. Null means the provider has no speech models.</summary>
    public static Func<string, ISpeechCaller?>? Default { get; set; }
}

/// <summary>Options for <see cref="StreamSpeech.Start"/>.</summary>
public sealed class StreamSpeechRequest : OperationRequest
{
    /// <summary>Speech model instance. Takes precedence over <see cref="ModelId"/>. It must support streaming speech.</summary>
    public ISpeechCaller? Model { get; set; }

    /// <summary>Model id resolved through <see cref="SpeechModels.Default"/>.</summary>
    public string? ModelId { get; set; }

    /// <summary>The text to convert to speech, as one complete string.</summary>
    public string? Text { get; set; }

    /// <summary>The text to convert to speech, as text parts. Strings convert implicitly to parts.</summary>
    public IEnumerable<SpeechTextPart>? TextParts { get; set; }

    /// <summary>The text to convert to speech, as a stream of text deltas or parts that is synthesized as it arrives.</summary>
    public IAsyncEnumerable<SpeechTextPart>? TextStream { get; set; }

    /// <summary>Voice to use.</summary>
    public string? Voice { get; set; }

    /// <summary>Desired format of the output audio chunks. Supported formats are provider-specific.</summary>
    public SpeechAudioFormat? OutputFormat { get; set; }

    /// <summary>Speaking instructions.</summary>
    public string? Instructions { get; set; }

    /// <summary>Playback speed.</summary>
    public double? Speed { get; set; }

    /// <summary>ISO 639-1 language code or <c>auto</c>.</summary>
    public string? Language { get; set; }

    /// <summary>When true, providers include raw provider chunks in <c>fullStream</c>.</summary>
    public bool? IncludeRawChunks { get; set; }

    /// <summary>
    /// Called for each error part the provider reports. Error parts are not fatal: <c>fullStream</c> includes them and
    /// <c>audioStream</c> skips them. The callback is not awaited, and errors it throws are ignored.
    /// Defaults to writing the error to <see cref="Console.Error"/>.
    /// </summary>
    public Func<Exception, Task>? OnError { get; set; }

    /// <summary>True when the model was resolved from <see cref="ModelId"/>.</summary>
    internal bool ModelWasString { get; set; }
}

/// <summary>Result of <see cref="StreamSpeech.Start"/>. Maps to <c>StreamSpeechResult</c>.</summary>
public sealed class StreamSpeechResult
{
    private readonly PendingPromise<SpeechStreamUsage> _usage = new PendingPromise<SpeechStreamUsage>();
    private readonly PendingPromise<IReadOnlyList<OperationWarning>> _warnings = new PendingPromise<IReadOnlyList<OperationWarning>>();
    private readonly PendingPromise<IReadOnlyList<ProviderResponse>> _responses = new PendingPromise<IReadOnlyList<ProviderResponse>>();
    private readonly PendingPromise<JsonElement> _providerMetadata = new PendingPromise<JsonElement>();
    private readonly PartQueue<SpeechStreamPart> _queue = new PartQueue<SpeechStreamPart>();
    private readonly CancellationTokenSource _pipe = new CancellationTokenSource();
    private readonly object _gate = new object();
    private readonly IStreamingSpeechCaller _model;
    private readonly DateTime _startedAt;
    private readonly Func<Exception, Task>? _onError;
    private string _owner = "unclaimed";
    private ProviderResponse? _response;
    private bool _hasAudio;
    private bool _finished;
    private Exception? _lastError;

    internal StreamSpeechResult(IStreamingSpeechCaller model, StreamSpeechRequest request, SpeechTextStream text, CancellationToken cancellationToken)
    {
        _model = model;
        _startedAt = request.Now?.Invoke() ?? DateTime.UtcNow;
        _onError = request.OnError;
        FullStreamCore = new OperationFullStream<SpeechStreamPart>(token => new QueueEnumerator<SpeechStreamPart>(this, token, part => part), CancelFromConsumer);
        AudioStreamCore = new OperationFullStream<byte[]>(token => new AudioEnumerator(this, token), CancelFromConsumer);
        // A rejection nobody awaits (for example a result promise the caller never reads) must not surface as an unobserved task exception.
        Observe(_usage.Task);
        Observe(_warnings.Task);
        Observe(_responses.Task);
        Observe(_providerMetadata.Task);
        _ = PumpAsync(request, text, cancellationToken);
    }

    private static void Observe(Task task)
    {
        _ = task.ContinueWith(completed => _ = completed.Exception, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
    }

    /// <summary>Usage reported for the call, if available. Accessing it claims the stream.</summary>
    public Task<SpeechStreamUsage> Usage
    {
        get { Claim(); return _usage.Task; }
    }

    /// <summary>Warnings for the call, such as unsupported settings. Accessing it claims the stream.</summary>
    public Task<IReadOnlyList<OperationWarning>> Warnings
    {
        get { Claim(); return _warnings.Task; }
    }

    /// <summary>Response metadata. Accessing it claims the stream.</summary>
    public Task<IReadOnlyList<ProviderResponse>> Responses
    {
        get { Claim(); return _responses.Task; }
    }

    /// <summary>Additional provider-specific metadata. Accessing it claims the stream.</summary>
    public Task<JsonElement> ProviderMetadata
    {
        get { Claim(); return _providerMetadata.Task; }
    }

    /// <summary>
    /// Stream of audio bytes in generation order. <see cref="AudioStream"/> and <see cref="FullStream"/> share one live
    /// stream: only one of them can be accessed, and only once. Access it before any result promise.
    /// </summary>
    public OperationFullStream<byte[]> AudioStream
    {
        get
        {
            ClaimStream("audio-stream");
            return AudioStreamCore;
        }
    }

    /// <summary>
    /// Full stream of speech parts (audio, raw, and error parts). <see cref="AudioStream"/> and <see cref="FullStream"/>
    /// share one live stream: only one of them can be accessed, and only once. Access it before any result promise.
    /// </summary>
    public OperationFullStream<SpeechStreamPart> FullStream
    {
        get
        {
            ClaimStream("full-stream");
            return FullStreamCore;
        }
    }

    private OperationFullStream<SpeechStreamPart> FullStreamCore { get; }

    private OperationFullStream<byte[]> AudioStreamCore { get; }

    private void Claim()
    {
        lock (_gate)
        {
            // Speech streams can be unbounded, so the output has exactly one owner and is never buffered.
            if (_owner != "unclaimed")
            {
                return;
            }

            _owner = "result-promises";
        }

        _ = DrainAsync();
    }

    private void ClaimStream(string owner)
    {
        lock (_gate)
        {
            if (_owner != "unclaimed")
            {
                throw new InvalidOperationException(_owner == "result-promises"
                    ? (owner == "full-stream" ? "fullStream" : "audioStream") + " cannot be accessed after a result promise."
                    : "Only one of audioStream or fullStream can be accessed, and only once.");
            }

            _owner = owner;
        }
    }

    private async Task DrainAsync()
    {
        try
        {
            while (await _queue.WaitAsync(CancellationToken.None).ConfigureAwait(false))
            {
                _queue.Dequeue();
            }
        }
        catch (Exception)
        {
            // The pump rejects the promises.
        }
    }

    private async Task PumpAsync(StreamSpeechRequest request, SpeechTextStream text, CancellationToken userToken)
    {
        // Merged so cancelling the output stream also aborts a pending doStream.
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(userToken, _pipe.Token);
        var token = linked.Token;
        // A user abort settles the result and errors the output immediately, even while doStream is pending or the provider ignores the token.
        using var abort = userToken.Register(() =>
        {
            var reason = request.AbortReason ?? new OperationCanceledException(userToken);
            RejectPending(reason);
            _queue.Complete(reason);
            CancelPipe();
        });
        try
        {
            var headers = OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent);
            var providerOptions = request.ProviderOptions ?? OperationJson.Parse("{}");
            var call = new SpeechModelStreamCall(text, request.Voice, request.OutputFormat, request.Instructions, request.Speed, request.Language, providerOptions, headers, request.IncludeRawChunks, token);
            var result = await _model.DoStreamAsync(call, token).ConfigureAwait(false);
            _response = new ProviderResponse(result.Response?.Headers, result.Response?.Body, null, result.Response?.Timestamp ?? _startedAt, result.Response?.ModelId ?? _model.ModelId);
            await foreach (var part in result.Stream.WithCancellation(token).ConfigureAwait(false))
            {
                Accept(part);
            }

            if (!_finished)
            {
                throw IncompleteStreamError();
            }

            _queue.Complete(null);
        }
        catch (Exception error)
        {
            var reason = error is OperationCanceledException && request.AbortReason != null && userToken.IsCancellationRequested ? request.AbortReason : error;
            RejectPending(reason);
            // When doStream rejects before the model took the enumerator, nothing owns the text stream yet, so cancel it here.
            try
            {
                await text.CancelAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                // A model-owned stream stays with the model.
            }

            _queue.Complete(reason);
        }
    }

    // A stream that ends without a finish part: when nothing was heard it is a no-speech failure;
    // after audio it surfaces the provider's error instead.
    private Exception IncompleteStreamError()
    {
        return _hasAudio
            ? _lastError ?? new StreamProviderError("The speech stream ended before it finished.")
            : new NoSpeechGeneratedException(new[] { CurrentResponse() }, _lastError);
    }

    private void CancelPipe()
    {
        try
        {
            _pipe.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // The pump already ended and released the source.
        }
    }

    private void CancelFromConsumer()
    {
        // Reject now rather than when the provider reacts to the cancel, which may be late while doStream is still pending.
        if (_usage.IsPending)
        {
            var reason = new OperationCanceledException("Speech stream was cancelled.");
            RejectPending(reason);
            _queue.Complete(reason);
        }

        CancelPipe();
    }

    private void Accept(SpeechModelStreamPart part)
    {
        switch (part.Type)
        {
            case "stream-start":
                ResolveWarnings(part.Warnings ?? Array.Empty<OperationWarning>());
                break;
            case "response-metadata":
                var current = CurrentResponse();
                _response = new ProviderResponse(part.Headers ?? _response?.Headers, part.Body ?? _response?.Body, null, part.Timestamp ?? current.Timestamp, part.ModelId ?? current.ModelId);
                break;
            case "audio":
                var audio = part.Audio ?? (part.AudioBase64 != null ? Convert.FromBase64String(part.AudioBase64) : Array.Empty<byte>());
                // Empty chunks are dropped unless they carry provider metadata (e.g. an end-of-turn marker); those never count as audio.
                if (audio.Length == 0 && part.ProviderMetadata == null)
                {
                    break;
                }

                _hasAudio |= audio.Length > 0;
                _queue.Enqueue(new SpeechStreamPart { Type = "audio", Id = part.Id, Audio = audio, AudioFormat = part.AudioFormat, ProviderMetadata = part.ProviderMetadata });
                break;
            case "raw":
                _queue.Enqueue(new SpeechStreamPart { Type = "raw", RawValue = part.RawValue });
                break;
            case "error":
                _lastError = part.Error;
                ReportError(part.Error);
                _queue.Enqueue(new SpeechStreamPart { Type = "error", Error = part.Error });
                break;
            case "finish":
                if (_warnings.IsPending)
                {
                    ResolveWarnings(Array.Empty<OperationWarning>());
                }

                if (!_hasAudio)
                {
                    throw new NoSpeechGeneratedException(new[] { CurrentResponse() }, _lastError);
                }

                _finished = true;
                _usage.Resolve(part.Usage ?? new SpeechStreamUsage());
                _responses.Resolve(new[] { CurrentResponse() });
                _providerMetadata.Resolve(part.ProviderMetadata ?? OperationJson.Parse("{}"));
                break;
        }
    }

    private void ReportError(Exception? error)
    {
        if (error == null)
        {
            return;
        }

        var onError = _onError;
        _ = Task.Run(async () =>
        {
            try
            {
                if (onError == null)
                {
                    Console.Error.WriteLine(error);
                }
                else
                {
                    await onError(error).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                // Errors thrown by the callback are ignored.
            }
        });
    }

    private void ResolveWarnings(IReadOnlyList<OperationWarning> warnings)
    {
        _warnings.Resolve(warnings);
        WarningLog.Write(warnings, _model.Provider, _model.ModelId);
    }

    private ProviderResponse CurrentResponse()
    {
        return _response ?? new ProviderResponse(timestamp: _startedAt, modelId: _model.ModelId);
    }

    private void RejectPending(Exception error)
    {
        _usage.Reject(error);
        _warnings.Reject(error);
        _responses.Reject(error);
        _providerMetadata.Reject(error);
    }

    private class QueueEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly StreamSpeechResult _owner;
        private readonly CancellationToken _token;
        private readonly Func<SpeechStreamPart, T> _project;
        private T _current = default!;

        public QueueEnumerator(StreamSpeechResult owner, CancellationToken token, Func<SpeechStreamPart, T> project)
        {
            _owner = owner;
            _token = token;
            _project = project;
        }

        public T Current
        {
            get { return _current; }
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            // The pump completes the queue on every exit, including cancellation with the abort reason.
            while (await _owner._queue.WaitAsync(_token).ConfigureAwait(false))
            {
                var part = _owner._queue.Dequeue();
                if (Accepts(part))
                {
                    _current = _project(part);
                    return true;
                }
            }

            return false;
        }

        public ValueTask DisposeAsync()
        {
            if (_owner._usage.IsPending)
            {
                _owner.CancelFromConsumer();
            }

            return default;
        }

        protected virtual bool Accepts(SpeechStreamPart part)
        {
            return true;
        }
    }

    // Error parts are non-fatal (reported through OnError); only stream failures end the audio.
    private sealed class AudioEnumerator : QueueEnumerator<byte[]>
    {
        public AudioEnumerator(StreamSpeechResult owner, CancellationToken token)
            : base(owner, token, part => part.Audio)
        {
        }

        protected override bool Accepts(SpeechStreamPart part)
        {
            return part.Type == "audio" && part.Audio.Length > 0;
        }
    }
}

/// <summary>Streams speech audio. Maps to <c>experimental_streamSpeech</c>.</summary>
public static class StreamSpeech
{
    /// <summary>Starts a speech stream. <c>doStream</c> is invoked immediately.</summary>
    /// <exception cref="UnsupportedFunctionalityException">The model does not support streaming speech.</exception>
    public static StreamSpeechResult Start(StreamSpeechRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        ISpeechCaller model;
        if (request.Model is null && request.ModelId is { } id)
        {
            request.ModelWasString = true;
            model = SpeechModels.Default?.Invoke(id) ?? throw Unsupported(new MissingSpeechCaller(id), true);
        }
        else
        {
            model = request.Model ?? throw new InvalidArgumentException("model", null, "model is required");
        }

        if (model is not IStreamingSpeechCaller streaming)
        {
            throw Unsupported(model, request.ModelWasString);
        }

        var text = new SpeechTextStream(ToTextStream(request));
        var token = request.CancellationToken.CanBeCanceled ? request.CancellationToken : cancellationToken;
        return new StreamSpeechResult(streaming, request, text, token);
    }

    private static IAsyncEnumerable<SpeechTextPart> ToTextStream(StreamSpeechRequest request)
    {
        if (request.Text != null)
        {
            return SpeechTextStream.FromSync(new[] { new SpeechTextPart(request.Text) });
        }

        if (request.TextStream != null)
        {
            return request.TextStream;
        }

        if (request.TextParts != null)
        {
            return SpeechTextStream.FromSync(request.TextParts);
        }

        throw new InvalidArgumentException("text", null, "Expected a string, an iterable or async iterable, or a stream of strings or text parts.");
    }

    private static Exception Unsupported(ISpeechCaller model, bool modelWasString)
    {
        var message = "The " + model.Provider + " model \"" + model.ModelId + "\" does not support streaming speech.";
        if (modelWasString)
        {
            message += " String model IDs resolve through the global provider (AI Gateway by default). If that provider does not support streaming speech, pass a provider model instance instead (e.g. elevenLabs.speech('eleven_flash_v2_5')) or upgrade @ai-sdk/gateway to a version with streaming speech support.";
        }

        return new UnsupportedFunctionalityException("streaming speech", message);
    }

    private sealed class MissingSpeechCaller : ISpeechCaller
    {
        public MissingSpeechCaller(string modelId)
        {
            ModelId = modelId;
        }

        public string Provider { get { return "gateway"; } }

        public string ModelId { get; }

        public Task<SpeechModelResult> DoGenerateAsync(SpeechModelCall call, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
