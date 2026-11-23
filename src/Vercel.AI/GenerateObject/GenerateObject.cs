// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.GenerateObject;

/// <summary>Thrown when a model response cannot produce the requested object. Maps to <c>NoObjectGeneratedError</c>.</summary>
public sealed class NoObjectGeneratedException : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_NoObjectGeneratedError";

    /// <summary>Creates the exception and preserves the failed generation details.</summary>
    public NoObjectGeneratedException(
        string message,
        Exception? cause,
        string? text,
        StepResponse response,
        LanguageModelUsage usage,
        FinishReason finishReason
    )
        : base("AI_NoObjectGeneratedError", message, cause)
    {
        Text = text;
        Response = response ?? throw new ArgumentNullException(nameof(response));
        Usage = usage ?? throw new ArgumentNullException(nameof(usage));
        FinishReason = finishReason;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>The selected text that failed to parse or validate.</summary>
    public string? Text { get; }

    /// <summary>Response metadata from the model call.</summary>
    public StepResponse Response { get; }

    /// <summary>Token usage from the model call.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Why the model finished generating.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error) => HasMarker(error, ErrorMarker);
}

/// <summary>Options for <see cref="IAiClient.GenerateObjectAsync{T}"/>. Maps to <c>generateObject</c>.</summary>
public sealed class GenerateObjectOptions<T>
{
    /// <summary>Gateway model id such as <c>openai/gpt-4.1-mini</c>.</summary>
    public string? ModelId { get; set; }

    /// <summary>Explicit language model. Wins over <see cref="ModelId"/>.</summary>
    public ILanguageModel? Model { get; set; }

    /// <summary>System prompt. <see cref="Instructions"/> wins when both are set.</summary>
    public string? System { get; set; }

    /// <summary>Instructions supplied before the prompt.</summary>
    public Instructions? Instructions { get; set; }

    /// <summary>A single user prompt.</summary>
    public string? Prompt { get; set; }

    /// <summary>Full prompt messages.</summary>
    public IReadOnlyList<ModelMessage>? Messages { get; set; }

    /// <summary>JSON Schema for the generated object.</summary>
    public string Schema { get; set; } = string.Empty;

    /// <summary>Optional schema name forwarded to the provider.</summary>
    public string? SchemaName { get; set; }

    /// <summary>Optional schema description forwarded to the provider.</summary>
    public string? SchemaDescription { get; set; }

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

    /// <summary>Total or per-step timeout.</summary>
    public TimeoutConfiguration? Timeout { get; set; }

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

    /// <summary>Called before generation begins.</summary>
    public Func<GenerateObjectStartEvent, CancellationToken, Task>? OnStart { get; set; }

    /// <summary>Called immediately before the model call.</summary>
    public Func<GenerateObjectStepStartEvent, CancellationToken, Task>? OnStepStart { get; set; }

    /// <summary>Called with the raw model result before the selected text is parsed.</summary>
    public Func<GenerateObjectStepEndEvent, CancellationToken, Task>? OnStepEnd { get; set; }

    /// <summary>Called after the object is parsed and validated.</summary>
    public Func<GenerateObjectEndEvent<T>, CancellationToken, Task>? OnFinish { get; set; }

    /// <summary>Generates the id of the call. Tests replace it. Maps to <c>_internal.generateId</c>.</summary>
    internal Func<string>? GenerateId { get; set; }
}

/// <summary>Input passed to <see cref="GenerateObjectOptions{T}.RepairText"/>.</summary>
public sealed class GenerateObjectRepairContext
{
    internal GenerateObjectRepairContext(string text, Exception error)
    {
        Text = text;
        Error = error;
    }

    /// <summary>The text that failed parsing or validation.</summary>
    public string Text { get; }

    /// <summary>The parse or validation error.</summary>
    public Exception Error { get; }
}

/// <summary>Base information shared by generate-object callback events.</summary>
public abstract class GenerateObjectEvent
{
    private protected GenerateObjectEvent(string callId, string provider, string modelId)
    {
        CallId = callId;
        Provider = provider;
        ModelId = modelId;
    }

    /// <summary>Identifier shared by every callback from this call.</summary>
    public string CallId { get; }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Requested model id.</summary>
    public string ModelId { get; }

    /// <summary>Whether the call records inputs. Set by the telemetry dispatcher. Maps to <c>recordInputs</c>.</summary>
    public bool? RecordInputs { get; private protected set; }

    /// <summary>Whether the call records outputs. Set by the telemetry dispatcher. Maps to <c>recordOutputs</c>.</summary>
    public bool? RecordOutputs { get; private protected set; }

    /// <summary>Identifier of the telemetry function. Set by the telemetry dispatcher. Maps to <c>functionId</c>.</summary>
    public string? FunctionId { get; private protected set; }

    /// <summary>Version of the AI SDK. Set by the telemetry dispatcher. Maps to <c>aiVersion</c>.</summary>
    public string? AiVersion { get; private protected set; }

    /// <summary>Whether the telemetry settings hide the inputs.</summary>
    private protected static bool HideInputs(TelemetryOptions? telemetry) =>
        telemetry?.RecordInputs == false;

    /// <summary>Whether the telemetry settings hide the outputs.</summary>
    private protected static bool HideOutputs(TelemetryOptions? telemetry) =>
        telemetry?.RecordOutputs == false;

    /// <summary>Copies the event and adds the telemetry settings and the SDK version. Maps to the first step of <c>augmentEvent</c>.</summary>
    private protected TEvent CloneWithTelemetry<TEvent>(TelemetryOptions? telemetry)
        where TEvent : GenerateObjectEvent
    {
        var copy = (TEvent)MemberwiseClone();
        copy.RecordInputs = telemetry?.RecordInputs;
        copy.RecordOutputs = telemetry?.RecordOutputs;
        copy.FunctionId = telemetry?.FunctionId;
        copy.AiVersion = AiSdkVersion.Version;
        return copy;
    }

    /// <summary>Drops the bodies and, for a hidden side, the messages of request and response metadata. Maps to the <c>request</c> and <c>response</c> branch of <c>filterTelemetryEvent</c>.</summary>
    private protected static LanguageModelRequestMetadata FilterRequest(
        LanguageModelRequestMetadata request,
        TelemetryOptions? telemetry
    )
    {
        if (!HideInputs(telemetry) && !HideOutputs(telemetry))
        {
            return request;
        }

        return new LanguageModelRequestMetadata
        {
            Url = request.Url,
            Messages = HideInputs(telemetry) ? [] : request.Messages,
        };
    }

    private protected static StepResponse FilterResponse(
        StepResponse response,
        TelemetryOptions? telemetry
    )
    {
        if (!HideInputs(telemetry) && !HideOutputs(telemetry))
        {
            return response;
        }

        return new StepResponse(
            response.Id,
            response.Timestamp,
            response.ModelId,
            response.Headers,
            HideOutputs(telemetry) ? [] : response.Messages
        );
    }

    /// <summary>Applies the provider redactors to the bodies. Maps to <c>redactMetadata</c>.</summary>
    private protected static LanguageModelRequestMetadata RedactRequest(
        LanguageModelRequestMetadata request
    )
    {
        if (request.RedactBody == null || request.Body is not { } body)
        {
            return request;
        }

        return new LanguageModelRequestMetadata
        {
            Url = request.Url,
            Messages = request.Messages,
            Body = TelemetryRedaction.RedactTelemetryValue(body, request.RedactBody),
        };
    }

    private protected static StepResponse RedactResponse(StepResponse response)
    {
        if (response.RedactBody == null || response.Body is not { } body)
        {
            return response;
        }

        return new StepResponse(
            response.Id,
            response.Timestamp,
            response.ModelId,
            response.Headers,
            response.Messages,
            TelemetryRedaction.RedactTelemetryValue(body, response.RedactBody)
        );
    }
}

/// <summary>Event sent before object generation begins.</summary>
public sealed class GenerateObjectStartEvent : GenerateObjectEvent, IOperationStartEvent
{
    internal GenerateObjectStartEvent(
        string callId,
        string provider,
        string modelId,
        string? prompt,
        IReadOnlyList<ModelMessage>? messages
    )
        : base(callId, provider, modelId)
    {
        Prompt = prompt;
        Messages = messages;
    }

    /// <summary>The simple prompt, when supplied.</summary>
    public string? Prompt { get; internal set; }

    /// <summary>The prompt messages, when supplied.</summary>
    public IReadOnlyList<ModelMessage>? Messages { get; internal set; }

    /// <summary>Identifies the operation type, <c>ai.generateObject</c> or <c>ai.streamObject</c>. Maps to <c>operationId</c>.</summary>
    public string OperationId { get; init; } = "ai.generateObject";

    /// <summary>The system message(s) provided to the model. Maps to <c>system</c>.</summary>
    public Instructions? System { get; internal set; }

    /// <summary>Maximum number of tokens to generate. Maps to <c>maxOutputTokens</c>.</summary>
    public int? MaxOutputTokens { get; init; }

    /// <summary>Sampling temperature. Maps to <c>temperature</c>.</summary>
    public double? Temperature { get; init; }

    /// <summary>Top-p sampling parameter. Maps to <c>topP</c>.</summary>
    public double? TopP { get; init; }

    /// <summary>Top-k sampling parameter. Maps to <c>topK</c>.</summary>
    public int? TopK { get; init; }

    /// <summary>Presence penalty. Maps to <c>presencePenalty</c>.</summary>
    public double? PresencePenalty { get; init; }

    /// <summary>Frequency penalty. Maps to <c>frequencyPenalty</c>.</summary>
    public double? FrequencyPenalty { get; init; }

    /// <summary>Random seed. Maps to <c>seed</c>.</summary>
    public int? Seed { get; init; }

    /// <summary>Maximum number of retries for failed requests. Maps to <c>maxRetries</c>.</summary>
    public int MaxRetries { get; init; }

    /// <summary>Additional HTTP headers sent with the request. Maps to <c>headers</c>.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; init; }

    /// <summary>Additional provider-specific options. Maps to <c>providerOptions</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; internal set; }

    /// <summary>Maps to <c>augmentEvent</c>: a copy with the telemetry settings, without the inputs the call does not record.</summary>
    internal GenerateObjectStartEvent AugmentEvent(TelemetryOptions? telemetry)
    {
        var copy = CloneWithTelemetry<GenerateObjectStartEvent>(telemetry);
        if (HideInputs(telemetry))
        {
            copy.System = null;
            copy.Prompt = null;
            copy.Messages = [];
            copy.Schema = null;
            copy.SchemaName = null;
            copy.SchemaDescription = null;
            copy.ProviderOptions = new Dictionary<string, JsonElement>();
        }

        return copy;
    }

    /// <summary>The output strategy type: <c>object</c>, <c>array</c>, <c>enum</c> or <c>no-schema</c>. Maps to <c>output</c>.</summary>
    public string Output { get; init; } = "object";

    /// <summary>The JSON Schema used for object generation, if any. Maps to <c>schema</c>.</summary>
    public JsonElement? Schema { get; internal set; }

    /// <summary>Optional name of the schema. Maps to <c>schemaName</c>.</summary>
    public string? SchemaName { get; internal set; }

    /// <summary>Optional description of the schema. Maps to <c>schemaDescription</c>.</summary>
    public string? SchemaDescription { get; internal set; }
}

/// <summary>Event sent immediately before the generate-object model call.</summary>
public sealed class GenerateObjectStepStartEvent : GenerateObjectEvent
{
    internal GenerateObjectStepStartEvent(string callId, string provider, string modelId)
        : base(callId, provider, modelId) { }

    /// <summary>The only step number made by generateObject.</summary>
    public int StepNumber => 0;

    /// <summary>Additional provider-specific options. Maps to <c>providerOptions</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; internal set; }

    /// <summary>Additional HTTP headers sent with the request. Maps to <c>headers</c>.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; init; }

    /// <summary>The prompt messages in provider format. Maps to <c>promptMessages</c>.</summary>
    public IReadOnlyList<ModelMessage>? PromptMessages { get; internal set; }

    /// <summary>Maps to <c>augmentEvent</c>: a copy with the telemetry settings, without the inputs the call does not record.</summary>
    internal GenerateObjectStepStartEvent AugmentEvent(TelemetryOptions? telemetry)
    {
        var copy = CloneWithTelemetry<GenerateObjectStepStartEvent>(telemetry);
        if (HideInputs(telemetry))
        {
            copy.ProviderOptions = new Dictionary<string, JsonElement>();
            copy.PromptMessages = [];
        }

        return copy;
    }
}

/// <summary>The raw result of the one model step made by <c>generateObject</c>.</summary>
public sealed class GenerateObjectStepEndEvent : GenerateObjectEvent
{
    internal GenerateObjectStepEndEvent(
        string callId,
        string provider,
        string modelId,
        StepResult step
    )
        : base(callId, provider, modelId)
    {
        ObjectText = step.Text;
        Reasoning = step.ReasoningText;
        FinishReason = step.FinishReason;
        Usage = step.Usage;
        Warnings = step.Warnings;
        Request = step.Request;
        Response = step.Response;
        ProviderMetadata = step.ProviderMetadata;
    }

    internal GenerateObjectStepEndEvent(
        string callId,
        string provider,
        string modelId,
        string objectText,
        FinishReason finishReason,
        LanguageModelUsage usage,
        IReadOnlyList<CallWarning> warnings,
        LanguageModelRequestMetadata request,
        LanguageModelResponseMetadata response,
        JsonElement? providerMetadata,
        double? msToFirstChunk = null
    )
        : base(callId, provider, modelId)
    {
        MsToFirstChunk = msToFirstChunk;
        ObjectText = objectText;
        FinishReason = finishReason;
        Usage = usage;
        Warnings = warnings;
        Request = request;
        Response =
            response as StepResponse
            ?? new StepResponse(
                response.Id,
                response.Timestamp,
                response.ModelId,
                response.Headers
            );
        ProviderMetadata = providerMetadata;
    }

    /// <summary>The only step number made by generateObject.</summary>
    public int StepNumber => 0;

    /// <summary>All text returned by the model, before provider-selected text is parsed.</summary>
    public string ObjectText { get; internal set; }

    /// <summary>Reasoning returned by the model.</summary>
    public string? Reasoning { get; internal set; }

    /// <summary>Finish reason.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Token usage.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Provider warnings.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }

    /// <summary>Request metadata.</summary>
    public LanguageModelRequestMetadata Request { get; internal set; }

    /// <summary>Response metadata, including the raw response body.</summary>
    public StepResponse Response { get; internal set; }

    /// <summary>Provider-specific response metadata.</summary>
    public JsonElement? ProviderMetadata { get; internal set; }

    /// <summary>Milliseconds from the start of the stream to the first chunk. Set only by <c>streamObject</c>.</summary>
    public double? MsToFirstChunk { get; }

    /// <summary>Maps to <c>augmentEvent</c>: a copy with the telemetry settings, the content filter and the credential redaction.</summary>
    internal GenerateObjectStepEndEvent AugmentEvent(TelemetryOptions? telemetry)
    {
        var copy = CloneWithTelemetry<GenerateObjectStepEndEvent>(telemetry);
        if (HideOutputs(telemetry))
        {
            copy.ObjectText = "";
            copy.Reasoning = null;
        }

        if (HideInputs(telemetry) || HideOutputs(telemetry))
        {
            // Raw provider payloads can hold either inputs or outputs.
            copy.ProviderMetadata = null;
        }

        copy.Request = FilterRequest(Request, telemetry);
        copy.Response = FilterResponse(Response, telemetry);
        copy.Request = RedactRequest(copy.Request);
        copy.Response = RedactResponse(copy.Response);
        return copy;
    }
}

/// <summary>Event sent after the generated object has been parsed and validated.</summary>
public sealed class GenerateObjectEndEvent<T>
    : GenerateObjectEvent,
        IOperationEndEvent,
        IRecordingFilterableEndEvent,
        IAugmentableEndEvent,
        IGenerateObjectEndEvent
{
    internal GenerateObjectEndEvent(
        string callId,
        string provider,
        string modelId,
        GenerateObjectResult<T> result
    )
        : base(callId, provider, modelId)
    {
        Object = result.Object;
        Reasoning = result.Reasoning;
        FinishReason = result.FinishReason;
        Usage = result.Usage;
        Warnings = result.Warnings;
        Request = result.Request;
        Response = result.Response;
        ProviderMetadata = result.ProviderMetadata;
    }

    internal GenerateObjectEndEvent(
        string callId,
        string provider,
        string modelId,
        T? value,
        Exception? error,
        FinishReason finishReason,
        LanguageModelUsage usage,
        IReadOnlyList<CallWarning> warnings,
        LanguageModelRequestMetadata request,
        LanguageModelResponseMetadata response,
        JsonElement? providerMetadata
    )
        : base(callId, provider, modelId)
    {
        Object = value!;
        Error = error;
        FinishReason = finishReason;
        Usage = usage;
        Warnings = warnings;
        Request = request;
        Response =
            response as StepResponse
            ?? new StepResponse(
                response.Id,
                response.Timestamp,
                response.ModelId,
                response.Headers
            );
        ProviderMetadata = providerMetadata;
    }

    /// <summary>The parsed object. It is <see langword="default"/> when <see cref="Error"/> is set, as <c>streamObject</c> reports an object that does not match the schema.</summary>
    public T Object { get; }

    /// <summary>The error, such as a type validation error when the final object does not match the schema. Set only by <c>streamObject</c>.</summary>
    public Exception? Error { get; internal set; }

    /// <summary>Reasoning returned by the model.</summary>
    public string? Reasoning { get; init; }

    /// <summary>Finish reason.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Token usage.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Provider warnings.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }

    /// <summary>Request metadata.</summary>
    public LanguageModelRequestMetadata Request { get; internal set; }

    /// <summary>Response metadata.</summary>
    public StepResponse Response { get; internal set; }

    /// <summary>Provider-specific response metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Maps to <c>augmentEvent</c>: the settings and version, the content filter and the credential redaction.</summary>
    IOperationEndEvent IAugmentableEndEvent.AugmentEvent(TelemetryOptions? telemetry)
    {
        var source =
            HideInputs(telemetry) || HideOutputs(telemetry)
                ? (GenerateObjectEndEvent<T>)
                    ((IRecordingFilterableEndEvent)this).FilterTelemetryEvent(
                        HideInputs(telemetry),
                        HideOutputs(telemetry)
                    )
                : this;
        var copy = source.CloneWithTelemetry<GenerateObjectEndEvent<T>>(telemetry);
        copy.Error = source.Error is null
            ? null
            : RedactTelemetryCredentialsHelper.RedactTelemetryCredentials(source.Error);
        copy.Request = RedactRequest(source.Request);
        copy.Response = RedactResponse(source.Response);
        return copy;
    }

    string? IGenerateObjectEndEvent.ObjectJson =>
        Object is null ? null : JsonSerializer.Serialize(Object);

    /// <summary>Maps to the object branches of <c>filterTelemetryEvent</c>: a copy without the output, the provider metadata, the error text and the request and response bodies.</summary>
    IOperationEndEvent IRecordingFilterableEndEvent.FilterTelemetryEvent(
        bool hideInputs,
        bool hideOutputs
    )
    {
        return new GenerateObjectEndEvent<T>(
            CallId,
            Provider,
            ModelId,
            hideOutputs ? default : Object,
            Error is null ? null : new InvalidOperationException("AI SDK operation failed"),
            FinishReason,
            Usage,
            Warnings,
            // The bodies can hold either inputs or outputs, so they are always dropped. The messages go with their flag.
            new LanguageModelRequestMetadata
            {
                Url = Request.Url,
                Messages = hideInputs ? [] : Request.Messages,
            },
            new StepResponse(
                Response.Id,
                Response.Timestamp,
                Response.ModelId,
                Response.Headers,
                hideOutputs ? [] : Response.Messages
            ),
            null
        )
        {
            Reasoning = hideOutputs ? null : Reasoning,
        };
    }
}

/// <summary>Adds the telemetry settings to an end event and redacts its credentials on a copy.</summary>
internal interface IAugmentableEndEvent
{
    IOperationEndEvent AugmentEvent(TelemetryOptions? telemetry);
}

/// <summary>
/// The members of <see cref="GenerateObjectEndEvent{T}"/> an integration reads, without the object type. .NET-only: C# cannot match a
/// generic event type without knowing <c>T</c>.
/// </summary>
internal interface IGenerateObjectEndEvent
{
    string CallId { get; }

    FinishReason FinishReason { get; }

    LanguageModelUsage Usage { get; }

    JsonElement? ProviderMetadata { get; }

    /// <summary>The object as JSON text, or null when there is no object. Maps to <c>JSON.stringify(event.object)</c>.</summary>
    string? ObjectJson { get; }
}

/// <summary>Filters an end event on a copy when <c>recordInputs</c> or <c>recordOutputs</c> is <c>false</c>.</summary>
internal interface IRecordingFilterableEndEvent
{
    IOperationEndEvent FilterTelemetryEvent(bool hideInputs, bool hideOutputs);
}

/// <summary>Result of a <c>generateObject</c> call.</summary>
public sealed class GenerateObjectResult<T>
{
    internal GenerateObjectResult(T value, GenerateTextResult result)
    {
        Object = value;
        Reasoning = result.ReasoningText;
        FinishReason = result.FinishReason;
        Usage = result.Usage;
        Warnings = result.Warnings;
        Request = result.Request;
        Response = result.Response;
        ProviderMetadata = result.ProviderMetadata;
    }

    /// <summary>The generated object.</summary>
    public T Object { get; }

    /// <summary>Reasoning returned by the model.</summary>
    public string? Reasoning { get; }

    /// <summary>Finish reason.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Token usage.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Provider warnings.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }

    /// <summary>Request metadata.</summary>
    public LanguageModelRequestMetadata Request { get; }

    /// <summary>Response metadata, including the raw response body.</summary>
    public StepResponse Response { get; }

    /// <summary>Provider-specific response metadata.</summary>
    public JsonElement? ProviderMetadata { get; }
}
