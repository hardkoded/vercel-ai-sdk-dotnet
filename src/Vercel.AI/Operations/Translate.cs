// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Operations;

/// <summary>Arguments for a speech-translation stream.</summary>
public sealed class TranslationStreamCall
{
    /// <summary>Creates a translation call.</summary>
    public TranslationStreamCall(AudioChunkStream audio, string inputAudioFormat, string targetLanguage, string? sourceLanguage, string? outputAudioFormat, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, bool? includeRawChunks, CancellationToken cancellationToken)
    {
        Audio = audio;
        InputAudioFormat = inputAudioFormat;
        TargetLanguage = targetLanguage ?? string.Empty;
        SourceLanguage = sourceLanguage;
        OutputAudioFormat = outputAudioFormat;
        ProviderOptions = providerOptions;
        Headers = headers;
        IncludeRawChunks = includeRawChunks;
        CancellationToken = cancellationToken;
    }

    /// <summary>Audio stream.</summary>
    public AudioChunkStream Audio { get; }

    /// <summary>Input audio format.</summary>
    public string InputAudioFormat { get; }

    /// <summary>Target language.</summary>
    public string TargetLanguage { get; }

    /// <summary>Source language. Null asks the provider to detect it.</summary>
    public string? SourceLanguage { get; }

    /// <summary>Requested output audio format.</summary>
    public string? OutputAudioFormat { get; }

    /// <summary>Provider options.</summary>
    public JsonElement ProviderOptions { get; }

    /// <summary>Headers including the user-agent suffix.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>When true, raw parts are requested.</summary>
    public bool? IncludeRawChunks { get; }

    /// <summary>Cancellation token shared with <c>fullStream</c> cancellation.</summary>
    public CancellationToken CancellationToken { get; }
}

/// <summary>A started translation stream.</summary>
public sealed class TranslationStreamStart
{
    /// <summary>Creates a start payload.</summary>
    public TranslationStreamStart(ModelPartStream stream, ProviderResponse? response = null)
    {
        Stream = stream;
        Response = response;
    }

    /// <summary>Model parts.</summary>
    public ModelPartStream Stream { get; }

    /// <summary>Initial response metadata.</summary>
    public ProviderResponse? Response { get; }
}

/// <summary>Speech translation model used by <see cref="StreamTranslate"/>.</summary>
public interface ISpeechTranslationCaller
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Specification version. String ids are resolved before this is checked.</summary>
    string SpecificationVersion { get; }

    /// <summary>Starts a translation stream.</summary>
    Task<TranslationStreamStart> DoStreamAsync(TranslationStreamCall call, CancellationToken cancellationToken);
}

/// <summary>Resolves string speech-translation model ids.</summary>
public static class SpeechTranslationModels
{
    /// <summary>Default provider factory. Null means the provider has no speech translation models.</summary>
    public static Func<string, ISpeechTranslationCaller?>? Default { get; set; }
}

/// <summary>Options for <see cref="StreamTranslate.Start"/>.</summary>
public sealed class StreamTranslateRequest : OperationRequest
{
    /// <summary>Model instance or string id.</summary>
    public object? Model { get; set; }

    /// <summary>Audio stream.</summary>
    public AudioChunkStream? Audio { get; set; }

    /// <summary>Input audio format.</summary>
    public string InputAudioFormat { get; set; } = string.Empty;

    /// <summary>Target language.</summary>
    public string TargetLanguage { get; set; } = string.Empty;

    /// <summary>Source language.</summary>
    public string? SourceLanguage { get; set; }

    /// <summary>Output audio format.</summary>
    public string? OutputAudioFormat { get; set; }

    /// <summary>Requests raw provider chunks.</summary>
    public bool? IncludeRawChunks { get; set; }
}

/// <summary>Streaming translation result.</summary>
public sealed class StreamTranslateResult
{
    private readonly PendingPromise<string> _source = new PendingPromise<string>();
    private readonly PendingPromise<string> _translation = new PendingPromise<string>();
    private readonly PendingPromise<double?> _duration = new PendingPromise<double?>();
    private readonly PendingPromise<OperationUsage?> _usage = new PendingPromise<OperationUsage?>();
    private readonly PendingPromise<IReadOnlyList<OperationWarning>> _warnings = new PendingPromise<IReadOnlyList<OperationWarning>>();
    private readonly PendingPromise<ProviderResponse> _responsePromise = new PendingPromise<ProviderResponse>();
    private readonly PendingPromise<JsonElement> _metadata = new PendingPromise<JsonElement>();
    private readonly PartQueue<ModelStreamPart> _queue = new PartQueue<ModelStreamPart>();
    private readonly CancellationTokenSource _pipe = new CancellationTokenSource();
    private readonly object _gate = new object();
    private readonly ISpeechTranslationCaller _model;
    private readonly DateTime _startedAt;
    private string _owner = "unclaimed";
    private ProviderResponse? _response;
    private bool _hasAudio;

    internal StreamTranslateResult(ISpeechTranslationCaller model, StreamTranslateRequest request, CancellationToken cancellationToken)
    {
        _model = model;
        _startedAt = request.Now?.Invoke() ?? DateTime.UtcNow;
        var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _pipe.Token);
        FullStreamCore = new OperationFullStream<ModelStreamPart>(() => new QueueEnumerator(this, linked.Token), () => _pipe.Cancel());
        _ = PumpAsync(request, linked.Token);
    }

    /// <summary>Source transcript. Accessing it claims the stream.</summary>
    public Task<string> SourceText
    {
        get { Claim(); return _source.Task; }
    }

    /// <summary>Translated text. Accessing it claims the stream.</summary>
    public Task<string> TranslationText
    {
        get { Claim(); return _translation.Task; }
    }

    /// <summary>Duration. Accessing it claims the stream.</summary>
    public Task<double?> DurationInSeconds
    {
        get { Claim(); return _duration.Task; }
    }

    /// <summary>Usage. Accessing it claims the stream.</summary>
    public Task<OperationUsage?> Usage
    {
        get { Claim(); return _usage.Task; }
    }

    /// <summary>Warnings. Accessing it claims the stream.</summary>
    public Task<IReadOnlyList<OperationWarning>> Warnings
    {
        get { Claim(); return _warnings.Task; }
    }

    /// <summary>Response. Accessing it claims the stream.</summary>
    public Task<ProviderResponse> Response
    {
        get { Claim(); return _responsePromise.Task; }
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

    private async Task PumpAsync(StreamTranslateRequest request, CancellationToken cancellationToken)
    {
        var audio = request.Audio ?? new AudioChunkStream();
        ModelPartStream? stream = null;
        try
        {
            var headers = OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent);
            var providerOptions = request.ProviderOptions ?? OperationJson.Parse("{}");
            var start = await _model.DoStreamAsync(new TranslationStreamCall(audio, request.InputAudioFormat, request.TargetLanguage, request.SourceLanguage, request.OutputAudioFormat, providerOptions, headers, request.IncludeRawChunks, cancellationToken), cancellationToken).ConfigureAwait(false);
            stream = start.Stream;
            _response = new ProviderResponse(start.Response?.Headers, null, start.Response?.Id, start.Response?.Timestamp ?? _startedAt, start.Response?.ModelId ?? _model.ModelId);
            await foreach (var part in start.Stream.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                Accept(part);
            }

            if (_translation.IsPending)
            {
                throw new NoTranslationGeneratedException(CurrentResponse());
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
            case "audio":
                _hasAudio = true;
                _queue.Enqueue(part);
                break;
            case "output-text-delta":
            case "output-text-final":
            case "source-transcript-delta":
            case "source-transcript-partial":
            case "source-transcript-final":
            case "raw":
            case "error":
                _queue.Enqueue(part);
                break;
            case "finish":
                if (_warnings.IsPending)
                {
                    ResolveWarnings(Array.Empty<OperationWarning>());
                }

                if (!_hasAudio && string.IsNullOrEmpty(part.OutputText))
                {
                    throw new NoTranslationGeneratedException(CurrentResponse());
                }

                _source.Resolve(part.SourceText ?? string.Empty);
                _translation.Resolve(part.OutputText ?? string.Empty);
                _duration.Resolve(part.DurationInSeconds);
                _usage.Resolve(part.Usage);
                _responsePromise.Resolve(CurrentResponse());
                _metadata.Resolve(part.ProviderMetadata ?? OperationJson.Parse("{}"));
                break;
            default:
                throw new InvalidOperationException("Unsupported part type: " + part.Type);
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
        _source.Reject(error);
        _translation.Reject(error);
        _duration.Reject(error);
        _usage.Reject(error);
        _warnings.Reject(error);
        _responsePromise.Reject(error);
        _metadata.Reject(error);
    }

    private sealed class QueueEnumerator : IAsyncEnumerator<ModelStreamPart>
    {
        private readonly StreamTranslateResult _owner;
        private readonly CancellationToken _cancellationToken;
        private ModelStreamPart? _current;

        public QueueEnumerator(StreamTranslateResult owner, CancellationToken cancellationToken)
        {
            _owner = owner;
            _cancellationToken = cancellationToken;
        }

        public ModelStreamPart Current
        {
            get { return _current!; }
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (!await _owner._queue.WaitAsync(_cancellationToken).ConfigureAwait(false))
            {
                return false;
            }

            _current = _owner._queue.Dequeue();
            return true;
        }

        public ValueTask DisposeAsync()
        {
            if (_owner._translation.IsPending)
            {
                _owner._pipe.Cancel();
            }

            return default;
        }
    }
}

/// <summary>Streams a speech translation. Maps to <c>streamTranslate</c>.</summary>
public static class StreamTranslate
{
    /// <summary>Message thrown when the default provider has no speech translation models.</summary>
    public const string MissingProviderMessage = "The default provider does not support speech translation models. Please pass a provider model instance that implements the experimental speech translation model specification.";

    /// <summary>Starts a translation stream. <c>doStream</c> is invoked immediately.</summary>
    public static StreamTranslateResult Start(StreamTranslateRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var model = Resolve(request.Model);
        var token = request.CancellationToken.CanBeCanceled ? request.CancellationToken : cancellationToken;
        return new StreamTranslateResult(model, request, token);
    }

    private static ISpeechTranslationCaller Resolve(object? model)
    {
        if (model is string id)
        {
            return SpeechTranslationModels.Default?.Invoke(id) ?? throw new InvalidOperationException(MissingProviderMessage);
        }

        if (model is ISpeechTranslationCaller caller)
        {
            if (caller.SpecificationVersion != "v4")
            {
                throw new UnsupportedModelVersionException(caller.SpecificationVersion, caller.Provider, caller.ModelId);
            }

            return caller;
        }

        throw new InvalidArgumentException("model", model, "model is required");
    }
}
