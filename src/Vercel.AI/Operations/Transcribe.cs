// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Operations;

/// <summary>Downloaded media.</summary>
public sealed class DownloadedMedia
{
    /// <summary>Creates a download.</summary>
    public DownloadedMedia(byte[] data, string? mediaType)
    {
        Data = data ?? Array.Empty<byte>();
        MediaType = mediaType;
    }

    /// <summary>Bytes.</summary>
    public byte[] Data { get; }

    /// <summary>Content type reported by the download.</summary>
    public string? MediaType { get; }
}

/// <summary>Arguments for a transcription <c>doGenerate</c>.</summary>
public sealed class TranscriptionModelCall
{
    /// <summary>Creates a call.</summary>
    public TranscriptionModelCall(byte[] audio, string mediaType, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
    {
        Audio = audio ?? Array.Empty<byte>();
        MediaType = mediaType;
        ProviderOptions = providerOptions;
        Headers = headers;
        CancellationToken = cancellationToken;
    }

    /// <summary>Audio bytes.</summary>
    public byte[] Audio { get; }

    /// <summary>Detected media type, or <c>audio/wav</c>.</summary>
    public string MediaType { get; }

    /// <summary>Provider options.</summary>
    public JsonElement ProviderOptions { get; }

    /// <summary>Headers including the user-agent suffix.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Cancellation token.</summary>
    public CancellationToken CancellationToken { get; }
}

/// <summary>Transcription model response.</summary>
public sealed class TranscriptionModelResult
{
    /// <summary>Creates a result.</summary>
    public TranscriptionModelResult(string? text, IReadOnlyList<TranscriptSegment>? segments = null, string? language = null, double? durationInSeconds = null, IReadOnlyList<OperationWarning>? warnings = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
    {
        Text = text ?? string.Empty;
        Segments = segments ?? Array.Empty<TranscriptSegment>();
        Language = language;
        DurationInSeconds = durationInSeconds;
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        ProviderMetadata = providerMetadata;
        Response = response ?? new ProviderResponse();
    }

    /// <summary>Transcript text.</summary>
    public string Text { get; }

    /// <summary>Segments.</summary>
    public IReadOnlyList<TranscriptSegment> Segments { get; }

    /// <summary>Language.</summary>
    public string? Language { get; }

    /// <summary>Duration in seconds.</summary>
    public double? DurationInSeconds { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata.</summary>
    public ProviderResponse Response { get; }
}

/// <summary>Arguments for streaming transcription.</summary>
public sealed class TranscriptionStreamCall
{
    /// <summary>Creates a stream call.</summary>
    public TranscriptionStreamCall(AudioChunkStream audio, string inputAudioFormat, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, bool? includeRawChunks, CancellationToken cancellationToken)
    {
        Audio = audio;
        InputAudioFormat = inputAudioFormat;
        ProviderOptions = providerOptions;
        Headers = headers;
        IncludeRawChunks = includeRawChunks;
        CancellationToken = cancellationToken;
    }

    /// <summary>Audio stream.</summary>
    public AudioChunkStream Audio { get; }

    /// <summary>Input audio format.</summary>
    public string InputAudioFormat { get; }

    /// <summary>Provider options.</summary>
    public JsonElement ProviderOptions { get; }

    /// <summary>Headers including the user-agent suffix.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>When true, raw parts are requested.</summary>
    public bool? IncludeRawChunks { get; }

    /// <summary>Cancellation token shared with <c>fullStream</c> cancellation.</summary>
    public CancellationToken CancellationToken { get; }
}

/// <summary>A started transcription stream.</summary>
public sealed class TranscriptionStreamStart
{
    /// <summary>Creates a start payload.</summary>
    public TranscriptionStreamStart(ModelPartStream stream, ProviderResponse? response = null)
    {
        Stream = stream;
        Response = response;
    }

    /// <summary>Model parts.</summary>
    public ModelPartStream Stream { get; }

    /// <summary>Initial response metadata.</summary>
    public ProviderResponse? Response { get; }
}

/// <summary>Transcription model used by <see cref="Transcribe"/>.</summary>
public interface ITranscriptionCaller
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Specification version.</summary>
    string SpecificationVersion { get; }

    /// <summary>True when <see cref="DoStreamAsync"/> is implemented.</summary>
    bool CanStream { get; }

    /// <summary>Transcribes audio.</summary>
    Task<TranscriptionModelResult> DoGenerateAsync(TranscriptionModelCall call, CancellationToken cancellationToken);

    /// <summary>Starts a stream, or null when streaming is unsupported.</summary>
    Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken);
}

/// <summary>Resolves string transcription model ids.</summary>
public static class TranscriptionModels
{
    /// <summary>Default provider factory. Null means the provider has no transcription models.</summary>
    public static Func<string, ITranscriptionCaller?>? Default { get; set; }
}

/// <summary>Transcription result.</summary>
public sealed class TranscribeResult
{
    /// <summary>Creates a result.</summary>
    public TranscribeResult(string text, IReadOnlyList<TranscriptSegment> segments, string? language, double? durationInSeconds, IReadOnlyList<OperationWarning> warnings, IReadOnlyList<ProviderResponse> responses, JsonElement providerMetadata)
    {
        Text = text;
        Segments = segments ?? Array.Empty<TranscriptSegment>();
        Language = language;
        DurationInSeconds = durationInSeconds;
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        Responses = responses ?? Array.Empty<ProviderResponse>();
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Transcript.</summary>
    public string Text { get; }

    /// <summary>Segments.</summary>
    public IReadOnlyList<TranscriptSegment> Segments { get; }

    /// <summary>Language.</summary>
    public string? Language { get; }

    /// <summary>Duration in seconds.</summary>
    public double? DurationInSeconds { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Responses.</summary>
    public IReadOnlyList<ProviderResponse> Responses { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement ProviderMetadata { get; }
}

/// <summary>Options for <see cref="Transcribe.TranscribeAsync"/>.</summary>
public sealed class TranscribeRequest : OperationRequest
{
    /// <summary>Model instance or string id.</summary>
    public object? Model { get; set; }

    /// <summary>Audio bytes.</summary>
    public byte[]? Audio { get; set; }

    /// <summary>Audio URL. Downloaded when set.</summary>
    public string? AudioUrl { get; set; }

    /// <summary>Downloads URL audio.</summary>
    public Func<string, CancellationToken, Task<DownloadedMedia>>? Download { get; set; }
}

/// <summary>Options for <see cref="StreamTranscribe.Start"/>.</summary>
public sealed class StreamTranscribeRequest : OperationRequest
{
    /// <summary>Model instance or string id.</summary>
    public object? Model { get; set; }

    /// <summary>Audio stream.</summary>
    public AudioChunkStream? Audio { get; set; }

    /// <summary>Input audio format.</summary>
    public string InputAudioFormat { get; set; } = string.Empty;

    /// <summary>Requests raw provider chunks.</summary>
    public bool? IncludeRawChunks { get; set; }

    /// <summary>True when <see cref="Model"/> was a string id.</summary>
    public bool ModelWasString { get; set; }
}

/// <summary>Streaming transcription result.</summary>
public sealed class StreamTranscribeResult
{
    private readonly PendingPromise<string> _text = new PendingPromise<string>();
    private readonly PendingPromise<IReadOnlyList<TranscriptSegment>> _segments = new PendingPromise<IReadOnlyList<TranscriptSegment>>();
    private readonly PendingPromise<string?> _language = new PendingPromise<string?>();
    private readonly PendingPromise<double?> _duration = new PendingPromise<double?>();
    private readonly PendingPromise<IReadOnlyList<OperationWarning>> _warnings = new PendingPromise<IReadOnlyList<OperationWarning>>();
    private readonly PendingPromise<IReadOnlyList<ProviderResponse>> _responses = new PendingPromise<IReadOnlyList<ProviderResponse>>();
    private readonly PendingPromise<JsonElement> _metadata = new PendingPromise<JsonElement>();
    private readonly PartQueue<ModelStreamPart> _queue = new PartQueue<ModelStreamPart>();
    private readonly CancellationTokenSource _pipe = new CancellationTokenSource();
    private readonly object _gate = new object();
    private string _owner = "unclaimed";
    private ProviderResponse? _response;
    private readonly ITranscriptionCaller _model;
    private readonly DateTime _startedAt;

    internal StreamTranscribeResult(ITranscriptionCaller model, StreamTranscribeRequest request, CancellationToken cancellationToken)
    {
        _model = model;
        _startedAt = request.Now?.Invoke() ?? DateTime.UtcNow;
        var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _pipe.Token);
        FullStreamCore = new OperationFullStream<ModelStreamPart>(() => new QueueEnumerator(this), () => _pipe.Cancel());
        _ = PumpAsync(model, request, linked.Token);
    }

    /// <summary>Final transcript. Accessing it claims the stream.</summary>
    public Task<string> Text
    {
        get { Claim(); return _text.Task; }
    }

    /// <summary>Segments. Accessing it claims the stream.</summary>
    public Task<IReadOnlyList<TranscriptSegment>> Segments
    {
        get { Claim(); return _segments.Task; }
    }

    /// <summary>Language. Accessing it claims the stream.</summary>
    public Task<string?> Language
    {
        get { Claim(); return _language.Task; }
    }

    /// <summary>Duration. Accessing it claims the stream.</summary>
    public Task<double?> DurationInSeconds
    {
        get { Claim(); return _duration.Task; }
    }

    /// <summary>Warnings. Accessing it claims the stream.</summary>
    public Task<IReadOnlyList<OperationWarning>> Warnings
    {
        get { Claim(); return _warnings.Task; }
    }

    /// <summary>Responses. Accessing it claims the stream.</summary>
    public Task<IReadOnlyList<ProviderResponse>> Responses
    {
        get { Claim(); return _responses.Task; }
    }

    /// <summary>Provider metadata. Accessing it claims the stream.</summary>
    public Task<JsonElement> ProviderMetadata
    {
        get { Claim(); return _metadata.Task; }
    }

    /// <summary>Single-consumer part stream.</summary>
    public OperationFullStream<ModelStreamPart> FullStream
    {
        get
        {
            lock (_gate)
            {
                if (_owner == "full-stream")
                {
                    throw new InvalidOperationException("fullStream can only be accessed once.");
                }

                if (_owner == "result-promises")
                {
                    throw new InvalidOperationException("fullStream cannot be accessed after a result promise.");
                }

                _owner = "full-stream";
            }

            return FullStreamCore;
        }
    }

    internal OperationFullStream<ModelStreamPart> FullStreamCore { get; }

    private void Claim()
    {
        lock (_gate)
        {
            if (_owner != "unclaimed")
            {
                return;
            }

            _owner = "result-promises";
        }

        _ = DrainAsync();
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

    private async Task PumpAsync(ITranscriptionCaller model, StreamTranscribeRequest request, CancellationToken cancellationToken)
    {
        var audio = request.Audio ?? new AudioChunkStream();
        ModelPartStream? stream = null;
        try
        {
            var headers = OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent);
            var providerOptions = request.ProviderOptions ?? OperationJson.Parse("{}");
            var start = await model.DoStreamAsync(new TranscriptionStreamCall(audio, request.InputAudioFormat, providerOptions, headers, request.IncludeRawChunks, cancellationToken), cancellationToken).ConfigureAwait(false);
            if (start == null)
            {
                throw Unsupported(model, request.ModelWasString);
            }

            stream = start.Stream;
            _response = new ProviderResponse(start.Response?.Headers, null, start.Response?.Id, start.Response?.Timestamp ?? _startedAt, start.Response?.ModelId ?? model.ModelId);
            await foreach (var part in start.Stream.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                Accept(part);
            }

            if (_text.IsPending)
            {
                throw new NoTranscriptGeneratedException(new[] { CurrentResponse() });
            }

            _queue.Complete(null);
        }
        catch (Exception error)
        {
            stream?.Cancel();
            var reason = error is OperationCanceledException && request.AbortReason != null ? request.AbortReason : error;
            RejectPending(reason);
            if (!audio.Locked)
            {
                try
                {
                    audio.Cancel(reason);
                }
                catch (Exception)
                {
                    // A model-owned stream stays with the model.
                }
            }

            _queue.Complete(reason);
        }
    }

    private sealed class QueueEnumerator : IAsyncEnumerator<ModelStreamPart>
    {
        private readonly StreamTranscribeResult _owner;
        private ModelStreamPart? _current;

        public QueueEnumerator(StreamTranscribeResult owner)
        {
            _owner = owner;
        }

        public ModelStreamPart Current
        {
            get { return _current!; }
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            // The pump completes the queue on every exit, including cancellation with the abort reason.
            if (!await _owner._queue.WaitAsync(CancellationToken.None).ConfigureAwait(false))
            {
                return false;
            }

            _current = _owner._queue.Dequeue();
            return true;
        }

        public ValueTask DisposeAsync()
        {
            if (_owner._text.IsPending)
            {
                _owner._pipe.Cancel();
            }

            return default;
        }
    }

    private void Accept(ModelStreamPart part)
    {
        switch (part.Type)
        {
            case "stream-start":
                ResolveWarnings(part.Warnings ?? Array.Empty<OperationWarning>());
                break;
            case "response-metadata":
                var current = CurrentResponse();
                _response = new ProviderResponse(part.Headers ?? current.Headers, null, current.Id, part.Timestamp ?? current.Timestamp, part.ModelId ?? current.ModelId);
                break;
            case "transcript-delta":
            case "transcript-partial":
            case "transcript-final":
            case "raw":
            case "error":
                _queue.Enqueue(part);
                break;
            case "finish":
                if (_warnings.IsPending)
                {
                    ResolveWarnings(Array.Empty<OperationWarning>());
                }

                if (string.IsNullOrEmpty(part.Text))
                {
                    throw new NoTranscriptGeneratedException(new[] { CurrentResponse() });
                }

                _text.Resolve(part.Text!);
                _segments.Resolve(part.Segments ?? Array.Empty<TranscriptSegment>());
                _language.Resolve(part.Language);
                _duration.Resolve(part.DurationInSeconds);
                _responses.Resolve(new[] { CurrentResponse() });
                _metadata.Resolve(part.ProviderMetadata ?? OperationJson.Parse("{}"));
                break;
        }
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
        _text.Reject(error);
        _segments.Reject(error);
        _language.Reject(error);
        _duration.Reject(error);
        _warnings.Reject(error);
        _responses.Reject(error);
        _metadata.Reject(error);
    }

    internal static Exception Unsupported(ITranscriptionCaller model, bool modelWasString)
    {
        var message = "The " + model.Provider + " model \"" + model.ModelId + "\" does not support streaming transcription.";
        if (modelWasString)
        {
            message += " String model IDs resolve through the global provider (AI Gateway by default). If that provider does not support streaming transcription, pass a provider model instance instead (e.g. openai.transcription('gpt-realtime-whisper')) or upgrade @ai-sdk/gateway to a version with streaming transcription support.";
        }

        return new UnsupportedFunctionalityException("streaming transcription", message);
    }
}

/// <summary>Transcribes audio. Maps to <c>transcribe</c>.</summary>
public static class Transcribe
{
    /// <summary>Transcribes one audio payload.</summary>
    public static async Task<TranscribeResult> TranscribeAsync(TranscribeRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var model = Resolve(request.Model);
        var token = request.CancellationToken.CanBeCanceled ? request.CancellationToken : cancellationToken;
        var headers = OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent);
        var providerOptions = request.ProviderOptions ?? OperationJson.Parse("{}");
        byte[] audio;
        if (!string.IsNullOrEmpty(request.AudioUrl))
        {
            var download = request.Download ?? throw new InvalidArgumentException("download", null, "download is required for URL audio");
            audio = (await download(request.AudioUrl!, token).ConfigureAwait(false)).Data;
        }
        else
        {
            audio = request.Audio ?? Array.Empty<byte>();
        }

        var mediaType = MediaTypeDetector.Detect(audio, "audio") ?? "audio/wav";
        var result = await OperationRetry.ExecuteAsync(request.MaxRetries, token, request.AbortReason, ct => model.DoGenerateAsync(new TranscriptionModelCall(audio, mediaType, providerOptions, headers, ct), ct), null).ConfigureAwait(false);
        WarningLog.Write(result.Warnings, model.Provider, model.ModelId);
        if (string.IsNullOrEmpty(result.Text))
        {
            throw new NoTranscriptGeneratedException(new[] { result.Response });
        }

        return new TranscribeResult(result.Text, result.Segments, result.Language, result.DurationInSeconds, result.Warnings, new[] { result.Response }, result.ProviderMetadata ?? OperationJson.Parse("{}"));
    }

    internal static ITranscriptionCaller Resolve(object? model)
    {
        if (model is ITranscriptionCaller caller)
        {
            if (caller.SpecificationVersion != "v4" && caller.SpecificationVersion != "v3" && caller.SpecificationVersion != "v2")
            {
                throw new UnsupportedModelVersionException(caller.SpecificationVersion, caller.Provider, caller.ModelId);
            }

            return caller;
        }

        if (model is string id)
        {
            return TranscriptionModels.Default?.Invoke(id) ?? throw new InvalidOperationException("The default provider does not support transcription models.");
        }

        throw new InvalidArgumentException("model", model, "model is required");
    }
}

/// <summary>Streams a transcript. Maps to <c>streamTranscribe</c>.</summary>
public static class StreamTranscribe
{
    /// <summary>Starts a transcription stream. <c>doStream</c> is invoked immediately.</summary>
    public static StreamTranscribeResult Start(StreamTranscribeRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        ITranscriptionCaller model;
        if (request.Model is string id)
        {
            request.ModelWasString = true;
            model = TranscriptionModels.Default?.Invoke(id) ?? throw StreamTranscribeResult.Unsupported(new MissingTranscriptionCaller(id), true);
        }
        else
        {
            model = request.Model as ITranscriptionCaller ?? throw new InvalidArgumentException("model", request.Model, "model is required");
        }

        if (!model.CanStream)
        {
            throw StreamTranscribeResult.Unsupported(model, request.ModelWasString);
        }

        var token = request.CancellationToken.CanBeCanceled ? request.CancellationToken : cancellationToken;
        return new StreamTranscribeResult(model, request, token);
    }

    private sealed class MissingTranscriptionCaller : ITranscriptionCaller
    {
        public MissingTranscriptionCaller(string modelId)
        {
            ModelId = modelId;
        }

        public string Provider { get { return "gateway"; } }

        public string ModelId { get; }

        public string SpecificationVersion { get { return "v4"; } }

        public bool CanStream { get { return false; } }

        public Task<TranscriptionModelResult> DoGenerateAsync(TranscriptionModelCall call, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
        {
            return Task.FromResult<TranscriptionStreamStart?>(null);
        }
    }
}
