// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.GenerateObject;

/// <summary>The type of the output of <c>streamObject</c>. Maps to the <c>output</c> option.</summary>
public enum StreamObjectOutput
{
    /// <summary>The output is an object.</summary>
    Object,

    /// <summary>The output is an array.</summary>
    Array,

    /// <summary>The output is an enum.</summary>
    Enum,

    /// <summary>The output is not a schema.</summary>
    NoSchema,
}

/// <summary>Options for <see cref="IAiClient.StreamObjectAsync{T}"/>. Maps to <c>streamObject</c>.</summary>
/// <typeparam name="T">The result: the object, the array, or the enum value (a string).</typeparam>
public sealed class StreamObjectOptions<T>
{
    /// <summary>Gateway model id such as <c>openai/gpt-4.1-mini</c>.</summary>
    public string? ModelId { get; set; }

    /// <summary>Explicit language model. Wins over <see cref="ModelId"/>.</summary>
    public ILanguageModel? Model { get; set; }

    /// <summary>The type of the output. Default: <see cref="StreamObjectOutput.Object"/>.</summary>
    public StreamObjectOutput Output { get; set; } = StreamObjectOutput.Object;

    /// <summary>System prompt. <see cref="Instructions"/> wins when both are set.</summary>
    public string? System { get; set; }

    /// <summary>Instructions supplied before the prompt.</summary>
    public Instructions? Instructions { get; set; }

    /// <summary>A single user prompt.</summary>
    public string? Prompt { get; set; }

    /// <summary>Full prompt messages.</summary>
    public IReadOnlyList<ModelMessage>? Messages { get; set; }

    /// <summary>
    /// Whether system messages are allowed in <see cref="Messages"/>. Default: <see langword="false"/>, which throws an
    /// <see cref="InvalidPromptException"/> for a system message; use <see cref="Instructions"/> instead. Maps to <c>allowSystemInMessages</c>.
    /// </summary>
    public bool AllowSystemInMessages { get; set; }

    /// <summary>
    /// Custom download function to use for URLs. By default, files are downloaded if the model does not support the URL for the
    /// given media type. The function receives one <see cref="DownloadRequestItem"/> per prompt URL, with
    /// <see cref="DownloadRequestItem.IsUrlSupportedByModel"/> set, and returns <c>null</c> for a URL the model can read itself.
    /// Maps to <c>experimental_download</c>.
    /// </summary>
    public Func<
        IReadOnlyList<DownloadRequestItem>,
        Task<IReadOnlyList<DownloadResult>>
    >? ExperimentalDownload { get; set; }

    /// <summary>
    /// JSON Schema for the generated object. For array output it is the schema of one element. Not supported for enum and
    /// no-schema output.
    /// </summary>
    public string? Schema { get; set; }

    /// <summary>Optional schema name forwarded to the provider.</summary>
    public string? SchemaName { get; set; }

    /// <summary>Optional schema description forwarded to the provider.</summary>
    public string? SchemaDescription { get; set; }

    /// <summary>The enum values that the model should use. Only for enum output. Maps to <c>enum</c>.</summary>
    public IReadOnlyList<string>? EnumValues { get; set; }

    /// <summary>Serializer settings used to materialize <typeparamref name="T"/>.</summary>
    public JsonSerializerOptions? SerializerOptions { get; set; }

    /// <summary>Maximum output tokens.</summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>Temperature.</summary>
    public double? Temperature { get; set; }

    /// <summary>Top-p sampling.</summary>
    public double? TopP { get; set; }

    /// <summary>Top-k sampling.</summary>
    public int? TopK { get; set; }

    /// <summary>Presence penalty.</summary>
    public double? PresencePenalty { get; set; }

    /// <summary>Frequency penalty.</summary>
    public double? FrequencyPenalty { get; set; }

    /// <summary>Random seed.</summary>
    public int? Seed { get; set; }

    /// <summary>Reasoning effort.</summary>
    public string? Reasoning { get; set; }

    /// <summary>Retries for provider calls made by this request.</summary>
    public int? MaxRetries { get; set; }

    /// <summary>Extra headers forwarded to the model.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Provider-specific options.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }

    /// <summary>Attempts to repair text that failed JSON parsing or schema validation.</summary>
    public Func<
        GenerateObjectRepairContext,
        CancellationToken,
        Task<string?>
    >? RepairText { get; set; }

    /// <summary>Deprecated alias of <see cref="RepairText"/>. Maps to <c>experimental_repairText</c>.</summary>
    public Func<
        GenerateObjectRepairContext,
        CancellationToken,
        Task<string?>
    >? ExperimentalRepairText { get; set; }

    /// <summary>Telemetry configuration. Maps to <c>telemetry</c>.</summary>
    public TelemetryOptions? Telemetry { get; set; }

    /// <summary>Deprecated alias of <see cref="Telemetry"/>. Maps to <c>experimental_telemetry</c>.</summary>
    public TelemetryOptions? ExperimentalTelemetry { get; set; }

    /// <summary>Called when the streamObject operation begins, before the LLM call is made.</summary>
    public Func<GenerateObjectStartEvent, CancellationToken, Task>? OnStart { get; set; }

    /// <summary>Called when the model call (step) begins, before the provider is called.</summary>
    public Func<GenerateObjectStepStartEvent, CancellationToken, Task>? OnStepStart { get; set; }

    /// <summary>Called when the model streaming step completes, with the raw accumulated text before final schema validation.</summary>
    public Func<GenerateObjectStepEndEvent, CancellationToken, Task>? OnStepEnd { get; set; }

    /// <summary>
    /// Called when an error occurs during streaming. You can use it to log errors. The stream processing pauses until the
    /// callback task completes.
    /// </summary>
    public Func<Exception, CancellationToken, Task>? OnError { get; set; }

    /// <summary>Called when the LLM response and the final object validation are finished.</summary>
    public Func<GenerateObjectEndEvent<T>, CancellationToken, Task>? OnFinish { get; set; }

    /// <summary>The clock for step timing in milliseconds. Tests replace it. Maps to <c>_internal.now</c>.</summary>
    internal Func<double>? NowMs { get; set; }

    /// <summary>Generates the id of the call. Tests replace it. Maps to <c>_internal.generateId</c>.</summary>
    internal Func<string>? GenerateId { get; set; }

    /// <summary>The clock for the response timestamp. Tests replace it. Maps to <c>_internal.currentDate</c>.</summary>
    internal Func<DateTimeOffset>? CurrentDate { get; set; }
}

/// <summary>A part of the full stream of a <c>streamObject</c> call. Maps to <c>ObjectStreamPart</c>.</summary>
public abstract class ObjectStreamPart
{
    private protected ObjectStreamPart(string type)
    {
        Type = type;
    }

    /// <summary>Part type: <c>object</c>, <c>text-delta</c>, <c>error</c>, or <c>finish</c>.</summary>
    public string Type { get; }
}

/// <summary>A new partial object. Maps to <c>{ type: 'object' }</c>.</summary>
public sealed class ObjectStreamObjectPart : ObjectStreamPart
{
    internal ObjectStreamObjectPart(JsonElement value)
        : base("object")
    {
        Object = value;
    }

    /// <summary>The partial object. It is not validated.</summary>
    public JsonElement Object { get; }
}

/// <summary>A text delta of the JSON representation of the object. Maps to <c>{ type: 'text-delta' }</c>.</summary>
public sealed class ObjectStreamTextDeltaPart : ObjectStreamPart
{
    internal ObjectStreamTextDeltaPart(string textDelta)
        : base("text-delta")
    {
        TextDelta = textDelta;
    }

    /// <summary>The text delta.</summary>
    public string TextDelta { get; }
}

/// <summary>An error. Maps to <c>{ type: 'error' }</c>.</summary>
public sealed class ObjectStreamErrorPart : ObjectStreamPart
{
    internal ObjectStreamErrorPart(Exception error)
        : base("error")
    {
        Error = error;
    }

    /// <summary>The error.</summary>
    public Exception Error { get; }
}

/// <summary>The end of the stream. Maps to <c>{ type: 'finish' }</c>.</summary>
public sealed class ObjectStreamFinishPart : ObjectStreamPart
{
    internal ObjectStreamFinishPart(
        FinishReason finishReason,
        LanguageModelUsage usage,
        LanguageModelResponseMetadata response,
        JsonElement? providerMetadata
    )
        : base("finish")
    {
        FinishReason = finishReason;
        Usage = usage;
        Response = response;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Why the model finished.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Token usage.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Response metadata.</summary>
    public LanguageModelResponseMetadata Response { get; }

    /// <summary>Provider-specific metadata.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>The result of a <c>streamObject</c> call. Maps to <c>StreamObjectResult</c>.</summary>
/// <typeparam name="T">The result: the object, the array, or the enum value (a string).</typeparam>
public sealed class StreamObjectResult<T>
{
    private readonly ObjectPartBuffer _buffer;
    private readonly ObjectOutputStrategy _outputStrategy;
    private readonly TaskCompletionSource<T> _object = Pending<T>();
    private readonly TaskCompletionSource<LanguageModelUsage> _usage =
        Pending<LanguageModelUsage>();
    private readonly TaskCompletionSource<JsonElement?> _providerMetadata = Pending<JsonElement?>();
    private readonly TaskCompletionSource<IReadOnlyList<CallWarning>?> _warnings =
        Pending<IReadOnlyList<CallWarning>?>();
    private readonly TaskCompletionSource<LanguageModelRequestMetadata> _request =
        Pending<LanguageModelRequestMetadata>();
    private readonly TaskCompletionSource<LanguageModelResponseMetadata> _response =
        Pending<LanguageModelResponseMetadata>();
    private readonly TaskCompletionSource<FinishReason> _finishReason = Pending<FinishReason>();

    internal StreamObjectResult(ObjectOutputStrategy outputStrategy, ObjectPartBuffer buffer)
    {
        _outputStrategy = outputStrategy;
        _buffer = buffer;
    }

    /// <summary>Warnings from the model provider (e.g. unsupported settings).</summary>
    public Task<IReadOnlyList<CallWarning>?> Warnings => _warnings.Task;

    /// <summary>The token usage of the generated response. Resolved when the response is finished.</summary>
    public Task<LanguageModelUsage> Usage => _usage.Task;

    /// <summary>Additional provider-specific metadata.</summary>
    public Task<JsonElement?> ProviderMetadata => _providerMetadata.Task;

    /// <summary>Additional request information.</summary>
    public Task<LanguageModelRequestMetadata> Request => _request.Task;

    /// <summary>Additional response information.</summary>
    public Task<LanguageModelResponseMetadata> Response => _response.Task;

    /// <summary>The reason why the generation finished. Resolved when the response is finished.</summary>
    public Task<FinishReason> FinishReason => _finishReason.Task;

    /// <summary>The generated object. Resolved when the response is finished.</summary>
    public Task<T> Object => _object.Task;

    /// <summary>
    /// Stream of partial objects. It gets more complete as the stream progresses. The partial object is not validated.
    /// Errors are suppressed (use <see cref="StreamObjectOptions{T}.OnError"/> instead).
    /// </summary>
    public async IAsyncEnumerable<JsonElement> PartialObjectStream(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await foreach (var part in _buffer.Read(cancellationToken).ConfigureAwait(false))
        {
            if (part is ObjectStreamObjectPart objectPart)
            {
                yield return objectPart.Object;
            }
        }
    }

    /// <summary>
    /// Stream over complete array elements. Only available if the output is <see cref="StreamObjectOutput.Array"/>.
    /// Errors are suppressed (use <see cref="StreamObjectOptions{T}.OnError"/> instead).
    /// </summary>
    /// <exception cref="UnsupportedFunctionalityException">The output is not an array.</exception>
    public IAsyncEnumerable<JsonElement> ElementStream(
        CancellationToken cancellationToken = default
    )
    {
        if (_outputStrategy.Type != StreamObjectOutput.Array)
        {
            // throws the unsupported functionality error of the strategy:
            var published = 0;
            _outputStrategy.NewElements(default, ref published);
        }

        return ReadElements(cancellationToken);
    }

    /// <summary>
    /// Text stream of the JSON representation of the generated object. It contains text chunks. When the stream is
    /// finished, the object is valid JSON that can be parsed. Errors are suppressed.
    /// </summary>
    public async IAsyncEnumerable<string> TextStream(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await foreach (var part in _buffer.Read(cancellationToken).ConfigureAwait(false))
        {
            if (part is ObjectStreamTextDeltaPart delta)
            {
                yield return delta.TextDelta;
            }
        }
    }

    /// <summary>
    /// Stream of different types of events, including partial objects, errors, and finish events. Only errors that stop the
    /// stream, such as network errors, are thrown.
    /// </summary>
    public IAsyncEnumerable<ObjectStreamPart> FullStream(
        CancellationToken cancellationToken = default
    ) => _buffer.Read(cancellationToken);

    /// <summary>
    /// Creates a simple text stream response. The response has a <c>content-type</c> header set to
    /// <c>text/plain; charset=utf-8</c>. Each text delta is encoded as UTF-8 and sent as a separate chunk.
    /// Maps to <c>toTextStreamResponse</c>.
    /// </summary>
    public TextStreamResponse ToTextStreamResponse(
        int? status = null,
        string? statusText = null,
        HeaderCollection? headers = null
    ) => TextStreams.CreateTextStreamResponse(ToReadableStream(), status, statusText, headers);

    /// <summary>
    /// Writes text delta output to a response. It sets a <c>content-type</c> header to <c>text/plain; charset=utf-8</c>
    /// and writes each text delta as a separate chunk. Maps to <c>pipeTextStreamToResponse</c>.
    /// </summary>
    public Task PipeTextStreamToResponse(
        ServerResponse response,
        int? status = null,
        string? statusText = null,
        HeaderCollection? headers = null
    ) =>
        TextStreams.PipeTextStreamToResponseAsync(
            response,
            ToReadableStream(),
            status,
            statusText,
            headers
        );

    internal void ResolveUsage(LanguageModelUsage value) => _usage.TrySetResult(value);

    internal void ResolveProviderMetadata(JsonElement? value) =>
        _providerMetadata.TrySetResult(value);

    internal void ResolveWarnings(IReadOnlyList<CallWarning>? value) =>
        _warnings.TrySetResult(value);

    internal void ResolveRequest(LanguageModelRequestMetadata value) =>
        _request.TrySetResult(value);

    internal void ResolveResponse(LanguageModelResponseMetadata value) =>
        _response.TrySetResult(value);

    internal void ResolveFinishReason(FinishReason value) => _finishReason.TrySetResult(value);

    internal void ResolveObject(T value) => _object.TrySetResult(value);

    internal void RejectObject(Exception error) => _object.TrySetException(error);

    // Maps to rejectResultPromises. A promise that nobody awaits does not leave an unobserved exception.
    internal void RejectResultPromises(Exception error)
    {
        Reject(_object, error);
        Reject(_usage, error);
        Reject(_providerMetadata, error);
        Reject(_warnings, error);
        Reject(_request, error);
        Reject(_response, error);
        Reject(_finishReason, error);
    }

    private static TaskCompletionSource<TValue> Pending<TValue>() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static void Reject<TValue>(TaskCompletionSource<TValue> source, Exception error)
    {
        if (source.TrySetException(error))
        {
            _ = source.Task.ContinueWith(
                task => _ = task.Exception,
                TaskContinuationOptions.OnlyOnFaulted
            );
        }
    }

    private async IAsyncEnumerable<JsonElement> ReadElements(
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var published = 0;
        await foreach (var part in _buffer.Read(cancellationToken).ConfigureAwait(false))
        {
            if (part is ObjectStreamObjectPart objectPart)
            {
                foreach (
                    var element in _outputStrategy.NewElements(objectPart.Object, ref published)
                )
                {
                    yield return element;
                }
            }
        }
    }

    private ReadableStream<string> ToReadableStream()
    {
        var enumerator = TextStream().GetAsyncEnumerator();
        return new ReadableStream<string>(
            pull: async delegate(ReadableStreamController<string> controller)
            {
                try
                {
                    if (await enumerator.MoveNextAsync().ConfigureAwait(false))
                    {
                        controller.Enqueue(enumerator.Current);
                        return;
                    }

                    controller.Close();
                }
                catch (Exception error)
                {
                    controller.Error(error);
                }
            }
        );
    }
}

/// <summary>A buffer that every reader of the full stream replays from the start.</summary>
internal sealed class ObjectPartBuffer
{
    private readonly List<ObjectStreamPart> _parts = [];
    private readonly object _gate = new();
    private TaskCompletionSource<bool> _changed = NewSignal();
    private Exception? _error;
    private bool _completed;

    public void Add(ObjectStreamPart part)
    {
        lock (_gate)
        {
            _parts.Add(part);
            Pulse();
        }
    }

    public void Complete()
    {
        lock (_gate)
        {
            _completed = true;
            Pulse();
        }
    }

    // The readers throw the error after the parts that came before it.
    public void Fail(Exception error)
    {
        lock (_gate)
        {
            _error ??= error;
        }
    }

    public async IAsyncEnumerable<ObjectStreamPart> Read(
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var index = 0;
        while (true)
        {
            Task wait;
            ObjectStreamPart? next = null;
            lock (_gate)
            {
                if (index < _parts.Count)
                {
                    next = _parts[index++];
                    wait = Task.CompletedTask;
                }
                else if (_completed)
                {
                    if (_error != null)
                    {
                        ExceptionDispatchInfo.Capture(_error).Throw();
                    }

                    yield break;
                }
                else
                {
                    wait = _changed.Task;
                }
            }

            if (next != null)
            {
                yield return next;
                continue;
            }

            cancellationToken.ThrowIfCancellationRequested();
            var cancelled = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            using (cancellationToken.Register(() => cancelled.TrySetResult(true)))
            {
                await Task.WhenAny(wait, cancelled.Task).ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private static TaskCompletionSource<bool> NewSignal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private void Pulse()
    {
        var previous = _changed;
        _changed = NewSignal();
        previous.TrySetResult(true);
    }
}

/// <summary>Maps to <c>stream-object.ts</c> and <c>validate-object-generation-input.ts</c>.</summary>
internal static class StreamObject
{
    // Maps to validateObjectGenerationInput.
    internal static void ValidateObjectGenerationInput(
        StreamObjectOutput output,
        string? schema,
        string? schemaName,
        string? schemaDescription,
        IReadOnlyList<string>? enumValues
    )
    {
        if (output == StreamObjectOutput.NoSchema)
        {
            Require(
                schema == null,
                "schema",
                schema,
                "Schema is not supported for no-schema output."
            );
            Require(
                schemaDescription == null,
                "schemaDescription",
                schemaDescription,
                "Schema description is not supported for no-schema output."
            );
            Require(
                schemaName == null,
                "schemaName",
                schemaName,
                "Schema name is not supported for no-schema output."
            );
            Require(
                enumValues == null,
                "enumValues",
                enumValues,
                "Enum values are not supported for no-schema output."
            );
        }

        if (output == StreamObjectOutput.Object)
        {
            Require(
                enumValues == null,
                "enumValues",
                enumValues,
                "Enum values are not supported for object output."
            );
        }

        if (output == StreamObjectOutput.Array)
        {
            Require(
                enumValues == null,
                "enumValues",
                enumValues,
                "Enum values are not supported for array output."
            );
        }

        if (output == StreamObjectOutput.Enum)
        {
            Require(schema == null, "schema", schema, "Schema is not supported for enum output.");
            Require(
                schemaDescription == null,
                "schemaDescription",
                schemaDescription,
                "Schema description is not supported for enum output."
            );
            Require(
                schemaName == null,
                "schemaName",
                schemaName,
                "Schema name is not supported for enum output."
            );
        }
    }

    private static void Require(bool condition, string parameter, object? value, string message)
    {
        if (!condition)
        {
            throw new InvalidArgumentException(parameter, value, message);
        }
    }

    // Maps to streamObject.
    internal static StreamObjectResult<T> Start<T>(
        ILanguageModel model,
        StreamObjectOptions<T> options,
        IAiTelemetry? telemetry,
        CancellationToken cancellationToken
    )
    {
        LogWarnings.Log(
            new LogWarningsOptions([
                new DeprecatedWarning(
                    "streamObject",
                    "Use streamText with an output setting instead."
                ),
            ])
        );

        ValidateObjectGenerationInput(
            options.Output,
            options.Schema,
            options.SchemaName,
            options.SchemaDescription,
            options.EnumValues
        );
        OperationRetry.ResolveMaxRetries(options.MaxRetries);

        JsonElement? schema = null;
        if (options.Schema != null)
        {
            using var document = JsonDocument.Parse(options.Schema);
            schema = document.RootElement.Clone();
        }

        var outputStrategy = ObjectOutputStrategy.Create(
            options.Output,
            schema,
            options.EnumValues
        );
        var buffer = new ObjectPartBuffer();
        var result = new StreamObjectResult<T>(outputStrategy, buffer);
        _ = Task.Run(
            () =>
                new Driver<T>(model, options, telemetry, outputStrategy, result, buffer).RunAsync(
                    cancellationToken
                ),
            CancellationToken.None
        );
        return result;
    }

    private sealed class Driver<T>(
        ILanguageModel model,
        StreamObjectOptions<T> options,
        IAiTelemetry? telemetry,
        ObjectOutputStrategy outputStrategy,
        StreamObjectResult<T> result,
        ObjectPartBuffer buffer
    )
    {
        private CancellationToken _cancellationToken;

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            try
            {
                await StreamAsync().ConfigureAwait(false);
            }
            catch (Exception error)
            {
                result.RejectResultPromises(GatewayErrorWrappers.WrapGatewayError(error));
                await EmitAsync(new ObjectStreamErrorPart(error)).ConfigureAwait(false);
            }
            finally
            {
                buffer.Complete();
            }
        }

        // Maps to eventProcessor: the part is published, then onError is told about an error part.
        private async Task EmitAsync(ObjectStreamPart part)
        {
            buffer.Add(part);
            if (part is ObjectStreamErrorPart error && options.OnError != null)
            {
                await Generation
                    .NotifyAsync(
                        () =>
                            options.OnError(
                                GatewayErrorWrappers.WrapGatewayError(error.Error),
                                _cancellationToken
                            ),
                        _cancellationToken
                    )
                    .ConfigureAwait(false);
            }
        }

        private async Task StreamAsync()
        {
            using var scope = telemetry?.Begin("streamObject", model.ModelId);
            var callId = options.GenerateId?.Invoke()
                ?? ProviderUtils.JsonValues.GenerateId("aiobj-");
            var jsonSchema = outputStrategy.JsonSchema;

            if (options.OnStart != null)
            {
                await Generation
                    .NotifyAsync(
                        () =>
                            options.OnStart(
                                new GenerateObjectStartEvent(
                                    callId,
                                    model.Provider,
                                    model.ModelId,
                                    options.Prompt,
                                    options.Messages
                                ),
                                _cancellationToken
                            ),
                        _cancellationToken
                    )
                    .ConfigureAwait(false);
            }

            if (options.OnStepStart != null)
            {
                await Generation
                    .NotifyAsync(
                        () =>
                            options.OnStepStart(
                                new GenerateObjectStepStartEvent(
                                    callId,
                                    model.Provider,
                                    model.ModelId
                                ),
                                _cancellationToken
                            ),
                        _cancellationToken
                    )
                    .ConfigureAwait(false);
            }

            var callOptions = new StreamLanguageModelCallOptions
            {
                Model = model,
                System = options.System,
                Instructions = options.Instructions,
                Prompt = options.Prompt,
                Messages = options.Messages,
                AllowSystemInMessages = options.AllowSystemInMessages,
                ExperimentalDownload = options.ExperimentalDownload,
                Output = ResponseFormat(jsonSchema),
                MaxOutputTokens = options.MaxOutputTokens,
                Temperature = options.Temperature,
                TopP = options.TopP,
                TopK = options.TopK,
                PresencePenalty = options.PresencePenalty,
                FrequencyPenalty = options.FrequencyPenalty,
                Seed = options.Seed,
                Reasoning = options.Reasoning,
                Headers = options.Headers,
                ProviderOptions = options.ProviderOptions,
                CallId = callId,
                NowMs = options.NowMs,
            };

            var now = options.NowMs ?? Generation.DefaultNowMs;
            var startTimestampMs = now();
            var call = await OperationRetry
                .ExecuteAsync(
                    options.MaxRetries,
                    _cancellationToken,
                    null,
                    token => LanguageModelCallStreaming.StreamLanguageModelCall(callOptions, token),
                    null
                )
                .ConfigureAwait(false);
            try
            {
                await ProcessAsync(call, callId, now, startTimestampMs).ConfigureAwait(false);
            }
            finally
            {
                await call.Disposal.DisposeAsync().ConfigureAwait(false);
            }
        }

        // The model is asked for JSON with the schema of the strategy. Only the response format of the call is read.
        private OutputSpec ResponseFormat(JsonElement? jsonSchema)
        {
            if (jsonSchema == null)
            {
                return OutputSpec.Json(options.SchemaName, options.SchemaDescription);
            }

            return OutputSpec.Object(
                jsonSchema.Value,
                options.SchemaName,
                options.SchemaDescription
            );
        }

        private async Task ProcessAsync(
            StreamLanguageModelCallResult call,
            string callId,
            Func<double> now,
            double startTimestampMs
        )
        {
            var sanitized = RequestOrigin.SanitizeRequestMetadata(call.Request);
            var request = new LanguageModelRequestMetadata
            {
                Body = sanitized?.Body,
                Url = sanitized?.Url,
            };
            result.ResolveRequest(request);

            IReadOnlyList<CallWarning>? warnings = null;
            var usage = LanguageModelUsage.Empty;
            FinishReason? finishReason = null;
            JsonElement? providerMetadata = null;
            T? value = default;
            Exception? error = null;
            Exception? terminalError = null;
            var arrayStreamParser = outputStrategy.CreateArrayStreamParser();
            var accumulatedText = new StringBuilder();
            var textDelta = new StringBuilder();
            var currentDate = options.CurrentDate ?? (() => DateTimeOffset.UtcNow);
            var fullResponse = new LanguageModelResponseMetadata(
                options.GenerateId?.Invoke() ?? ProviderUtils.JsonValues.GenerateId("aiobj-"),
                currentDate(),
                model.ModelId
            );
            JsonElement? latestObjectJson = null;
            JsonElement? latestObject = null;
            var isFirstDelta = true;
            var isFirstChunk = true;
            double? msToFirstChunk = null;

            LanguageModelResponseMetadata ResponseWithHeaders() =>
                new(
                    fullResponse.Id,
                    fullResponse.Timestamp,
                    fullResponse.ModelId,
                    call.Response?.Headers
                );

            try
            {
                await foreach (
                    var chunk in call
                        .Stream.WithCancellation(_cancellationToken)
                        .ConfigureAwait(false)
                )
                {
                    if (chunk is not ModelCallStartPart && isFirstChunk)
                    {
                        msToFirstChunk = now() - startTimestampMs;
                        isFirstChunk = false;
                    }

                    switch (chunk)
                    {
                        case ModelCallStartPart start:
                            warnings = start.Warnings;
                            continue;

                        case TextDeltaPart delta:
                        {
                            accumulatedText.Append(delta.Text);
                            textDelta.Append(delta.Text);

                            if (arrayStreamParser != null)
                            {
                                var parsed = arrayStreamParser(delta.Text);
                                if (!ReferenceEquals(parsed, ArrayOutputStreamParseResult.Fallback))
                                {
                                    if (parsed != null)
                                    {
                                        latestObject = JsonSerializer.SerializeToElement(
                                            parsed.Partial
                                        );
                                        await EmitAsync(
                                                new ObjectStreamObjectPart(latestObject.Value)
                                            )
                                            .ConfigureAwait(false);
                                        await EmitAsync(
                                                new ObjectStreamTextDeltaPart(parsed.TextDelta)
                                            )
                                            .ConfigureAwait(false);
                                        textDelta.Clear();
                                        isFirstDelta = false;
                                    }

                                    break;
                                }

                                arrayStreamParser = null;
                            }

                            var parseResult = PartialJson.Parse(accumulatedText.ToString());
                            if (
                                parseResult.State
                                is PartialJson.FailedParse
                                    or PartialJson.UndefinedInput
                            )
                            {
                                break;
                            }

                            var currentObjectJson = JsonSerializer.SerializeToElement(
                                parseResult.Value
                            );
                            if (
                                latestObjectJson != null
                                && IsDeepEqual(latestObjectJson.Value, currentObjectJson)
                            )
                            {
                                break;
                            }

                            var validationResult = outputStrategy.ValidatePartialResult(
                                currentObjectJson,
                                textDelta.ToString(),
                                latestObject,
                                isFirstDelta,
                                parseResult.State == PartialJson.SuccessfulParse
                            );
                            if (
                                validationResult.Success
                                && (
                                    latestObject == null
                                    || !IsDeepEqual(
                                        latestObject.Value,
                                        validationResult.Value.Partial
                                    )
                                )
                            )
                            {
                                latestObjectJson = currentObjectJson;
                                latestObject = validationResult.Value.Partial;
                                await EmitAsync(new ObjectStreamObjectPart(latestObject.Value))
                                    .ConfigureAwait(false);
                                await EmitAsync(
                                        new ObjectStreamTextDeltaPart(
                                            validationResult.Value.TextDelta
                                        )
                                    )
                                    .ConfigureAwait(false);
                                textDelta.Clear();
                                isFirstDelta = false;
                            }

                            break;
                        }

                        case ModelCallResponseMetadataPart metadata:
                            fullResponse = new LanguageModelResponseMetadata(
                                metadata.Id ?? fullResponse.Id,
                                metadata.Timestamp ?? fullResponse.Timestamp,
                                metadata.ModelId ?? fullResponse.ModelId
                            );
                            break;

                        case ErrorPart errorPart:
                            if (terminalError == null)
                            {
                                terminalError = GatewayErrorWrappers.WrapGatewayError(
                                    errorPart.Error ?? new AiSdkException(errorPart.Message)
                                );
                                error = terminalError;
                                finishReason = FinishReason.Error;
                                result.RejectResultPromises(terminalError);
                            }

                            await EmitAsync(new ObjectStreamErrorPart(terminalError))
                                .ConfigureAwait(false);
                            break;

                        case ModelCallEndPart end:
                        {
                            if (textDelta.Length > 0)
                            {
                                await EmitAsync(new ObjectStreamTextDeltaPart(textDelta.ToString()))
                                    .ConfigureAwait(false);
                            }

                            finishReason =
                                terminalError == null ? end.FinishReason : FinishReason.Error;
                            usage = end.Usage;
                            providerMetadata = end.ProviderMetadata;

                            await EmitAsync(
                                    new ObjectStreamFinishPart(
                                        finishReason.Value,
                                        usage,
                                        fullResponse,
                                        providerMetadata
                                    )
                                )
                                .ConfigureAwait(false);

                            LogCallWarnings(warnings);

                            if (terminalError != null)
                            {
                                break;
                            }

                            result.ResolveUsage(usage);
                            result.ResolveProviderMetadata(providerMetadata);
                            result.ResolveWarnings(warnings);
                            result.ResolveResponse(ResponseWithHeaders());
                            result.ResolveFinishReason(finishReason.Value);

                            try
                            {
                                var element = await ParseAndValidateObjectResult
                                    .ParseAndValidateObjectResultWithRepairAsync(
                                        accumulatedText.ToString(),
                                        outputStrategy,
                                        options.RepairText ?? options.ExperimentalRepairText,
                                        fullResponse,
                                        usage,
                                        finishReason.Value,
                                        _cancellationToken
                                    )
                                    .ConfigureAwait(false);
                                value = Materialize(
                                    element,
                                    accumulatedText.ToString(),
                                    usage,
                                    finishReason.Value,
                                    fullResponse
                                );
                                result.ResolveObject(value!);
                            }
                            catch (Exception e)
                            {
                                error = e;
                                result.RejectObject(e);
                                _ = result.Object.ContinueWith(
                                    task => _ = task.Exception,
                                    TaskContinuationOptions.OnlyOnFaulted
                                );
                            }

                            break;
                        }
                    }
                }
            }
            catch (Exception streamError) when (!_cancellationToken.IsCancellationRequested)
            {
                // A raw stream error stops the stream: readers of the full stream throw it, and flush does not run.
                var wrappedError = GatewayErrorWrappers.WrapGatewayError(streamError);
                result.RejectResultPromises(wrappedError);
                if (options.OnError != null)
                {
                    await Generation
                        .NotifyAsync(
                            () => options.OnError(wrappedError, _cancellationToken),
                            _cancellationToken
                        )
                        .ConfigureAwait(false);
                }

                buffer.Fail(wrappedError);
                return;
            }

            // Maps to flush.
            try
            {
                if (options.OnStepEnd != null)
                {
                    await Generation
                        .NotifyAsync(
                            () =>
                                options.OnStepEnd(
                                    new GenerateObjectStepEndEvent(
                                        callId,
                                        model.Provider,
                                        model.ModelId,
                                        accumulatedText.ToString(),
                                        finishReason ?? FinishReason.Other,
                                        usage,
                                        warnings ?? [],
                                        request,
                                        ResponseWithHeaders(),
                                        providerMetadata,
                                        msToFirstChunk
                                    ),
                                    _cancellationToken
                                ),
                            _cancellationToken
                        )
                        .ConfigureAwait(false);
                }

                if (options.OnFinish != null)
                {
                    await Generation
                        .NotifyAsync(
                            () =>
                                options.OnFinish(
                                    new GenerateObjectEndEvent<T>(
                                        callId,
                                        model.Provider,
                                        model.ModelId,
                                        value,
                                        error,
                                        finishReason ?? FinishReason.Other,
                                        usage,
                                        warnings ?? [],
                                        request,
                                        ResponseWithHeaders(),
                                        providerMetadata
                                    ),
                                    _cancellationToken
                                ),
                            _cancellationToken
                        )
                        .ConfigureAwait(false);
                }
            }
            catch (Exception flushError) when (!_cancellationToken.IsCancellationRequested)
            {
                await EmitAsync(new ObjectStreamErrorPart(flushError)).ConfigureAwait(false);
            }
        }

        // Maps the validated JSON to T. A value that does not fit T did not match the schema.
        private T Materialize(
            JsonElement element,
            string text,
            LanguageModelUsage usage,
            FinishReason finishReason,
            LanguageModelResponseMetadata response
        )
        {
            T? materialized;
            try
            {
                materialized = JsonSerializer.Deserialize<T>(
                    element.GetRawText(),
                    options.SerializerOptions
                );
            }
            catch (Exception cause) when (cause is JsonException or NotSupportedException)
            {
                throw NoObject(cause);
            }

            return materialized ?? throw NoObject(null);

            NoObjectGeneratedException NoObject(Exception? cause) =>
                new(
                    "No object generated: response did not match schema.",
                    cause,
                    text,
                    response as StepResponse
                        ?? new StepResponse(
                            response.Id,
                            response.Timestamp,
                            response.ModelId,
                            response.Headers
                        ),
                    usage,
                    finishReason
                );
        }

        private void LogCallWarnings(IReadOnlyList<CallWarning>? warnings)
        {
            LogWarnings.Log(
                new LogWarningsOptions(
                    (warnings ?? []).Select(ToModelWarning).ToArray(),
                    model.Provider,
                    model.ModelId
                )
            );
        }

        private static ModelWarning ToModelWarning(CallWarning warning) =>
            warning.Type switch
            {
                "unsupported" => new UnsupportedWarning(
                    warning.Feature ?? warning.Message,
                    warning.Feature == null ? null : warning.Message
                ),
                "compatibility" => new CompatibilityWarning(
                    warning.Feature ?? warning.Message,
                    warning.Feature == null ? null : warning.Message
                ),
                "deprecated" => new DeprecatedWarning(
                    warning.Setting ?? string.Empty,
                    warning.Message
                ),
                _ => new OtherWarning(warning.Message),
            };

        // Maps to isDeepEqualData for JSON values.
        private static bool IsDeepEqual(JsonElement left, JsonElement right) =>
            left.GetRawText() == right.GetRawText();
    }
}
