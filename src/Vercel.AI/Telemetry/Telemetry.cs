// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Operations;

namespace Vercel.AI;

/// <summary>
/// Implement this interface to create custom telemetry integrations. A callback is optional: return <c>null</c> from a property
/// to skip it. Maps to <c>Telemetry</c> in <c>telemetry/telemetry.ts</c>. Only <c>onStart</c>, <c>onObjectStepStart</c>, <c>onObjectStepEnd</c> and the text generation callbacks ( <c>onStepStart</c>, <c>onLanguageModelCallStart</c>,
/// <c>onLanguageModelCallEnd</c>, <c>onToolExecutionStart</c>, <c>onToolExecutionEnd</c>, <c>onStepEnd</c>, <c>onStepFinish</c>, <c>onEnd</c>,
/// <c>onAbort</c> and <c>onError</c>) and the extract and decide callbacks are ported so far.
/// </summary>
public interface ITelemetry
{
    /// <summary>
    /// Called when an operation begins, before any model call. Maps to <c>onStart</c>. Receives a <see cref="GenerateTextStartEvent"/> or a <see cref="GenerateObjectStartEvent"/>.
    /// </summary>
    Func<IOperationStartEvent, CancellationToken, Task>? OnStart { get; }

    /// <summary>Called before the model call of <c>generateObject</c> and <c>streamObject</c>. Maps to <c>onObjectStepStart</c>.</summary>
    Func<GenerateObjectStepStartEvent, CancellationToken, Task>? OnObjectStepStart { get; }

    /// <summary>Called after the model call of <c>generateObject</c> and <c>streamObject</c>. Maps to <c>onObjectStepEnd</c>.</summary>
    Func<GenerateObjectStepEndEvent, CancellationToken, Task>? OnObjectStepEnd { get; }

    /// <summary>Called when a step begins, before the provider is called. Maps to <c>onStepStart</c>.</summary>
    Func<GenerateTextStepStartEvent, CancellationToken, Task>? OnStepStart { get; }

    /// <summary>Called when a tool execution begins, before the tool runs. Maps to <c>onToolExecutionStart</c>.</summary>
    Func<ToolExecutionStartEvent, CancellationToken, Task>? OnToolExecutionStart { get; }

    /// <summary>Called when a tool execution completes, with a result or an error. Maps to <c>onToolExecutionEnd</c>.</summary>
    Func<ToolExecutionEndEvent, CancellationToken, Task>? OnToolExecutionEnd { get; }

    /// <summary>Called when a step completes. Maps to <c>onStepEnd</c>.</summary>
    Func<StepResult, CancellationToken, Task>? OnStepEnd { get; }

    /// <summary>Deprecated alias of <see cref="OnStepEnd"/>. An integration with both gets both calls. Maps to <c>onStepFinish</c>.</summary>
    Func<StepResult, CancellationToken, Task>? OnStepFinish { get; }

    /// <summary>Called when a streamed generation is aborted before it completes. Maps to <c>onAbort</c>.</summary>
    Func<StreamAbortContext, CancellationToken, Task>? OnAbort { get; }

    /// <summary>Called when an unrecoverable error occurs during the generation. Maps to <c>onError</c>.</summary>
    Func<TelemetryErrorEvent, CancellationToken, Task>? OnError { get; }

    /// <summary>Called when a step ends with an error before it produces a step result. Maps to <c>onStepError</c>.</summary>
    Func<GenerateTextStepErrorEvent, CancellationToken, Task>? OnStepError { get; }

    /// <summary>
    /// Called when an operation completes. Receives a <see cref="GenerateTextResult"/>, an <see cref="EmbedEndEvent"/>, a <see cref="RerankEndEvent"/>
    /// or a <see cref="TranscriptionEndEvent"/>. Maps to <c>onEnd</c>.
    /// </summary>
    Func<IOperationEndEvent, CancellationToken, Task>? OnEnd { get; }

    /// <summary>Called before each <c>doEmbed</c> call of <c>embed</c> and <c>embedMany</c>. Maps to <c>onEmbedStart</c>.</summary>
    Func<EmbeddingModelCallStartEvent, CancellationToken, Task>? OnEmbedStart { get; }

    /// <summary>Called after each <c>doEmbed</c> call of <c>embed</c> and <c>embedMany</c>. Maps to <c>onEmbedEnd</c>.</summary>
    Func<EmbeddingModelCallEndEvent, CancellationToken, Task>? OnEmbedEnd { get; }

    /// <summary>Called before the <c>doRerank</c> call of <c>rerank</c>. Maps to <c>onRerankStart</c>.</summary>
    Func<RerankingModelCallStartEvent, CancellationToken, Task>? OnRerankStart { get; }

    /// <summary>Called after the <c>doRerank</c> call of <c>rerank</c>. Maps to <c>onRerankEnd</c>.</summary>
    Func<RerankingModelCallEndEvent, CancellationToken, Task>? OnRerankEnd { get; }

    /// <summary>Called when a stream transcription begins. Maps to <c>experimental_onStreamTranscriptionStart</c>.</summary>
    Func<
        StreamTranscriptionStartEvent,
        CancellationToken,
        Task
    >? OnStreamTranscriptionStart { get; }

    /// <summary>Called when a stream transcription completes. Maps to <c>experimental_onStreamTranscriptionEnd</c>.</summary>
    Func<StreamTranscriptionEndEvent, CancellationToken, Task>? OnStreamTranscriptionEnd { get; }

    /// <summary>
    /// Runs the provider call of a model call in an integration-specific context, so auto-instrumented provider requests can be
    /// associated with the model work. Maps to <c>executeLanguageModelCall</c>.
    /// </summary>
    IExecuteLanguageModelCall? ExecuteLanguageModelCall { get; }

    /// <summary>
    /// Runs the <c>execute</c> function of a tool in an integration-specific context, so nested calls become children of the tool
    /// span. Maps to <c>executeTool</c>.
    /// </summary>
    IExecuteTool? ExecuteTool { get; }

    /// <summary>Called before each provider model call. Maps to <c>onLanguageModelCallStart</c>.</summary>
    Func<LanguageModelCallStartEvent, CancellationToken, Task>? OnLanguageModelCallStart { get; }

    /// <summary>Called after each provider model call returns, before tools run. Maps to <c>onLanguageModelCallEnd</c>.</summary>
    Func<LanguageModelCallEndEvent, CancellationToken, Task>? OnLanguageModelCallEnd { get; }

    /// <summary>Called when an experimental decision begins. Maps to <c>experimental_onDecideStart</c>.</summary>
    Func<DecideEvent, CancellationToken, Task>? OnDecideStart { get; }

    /// <summary>Called before the decision model call begins. Maps to <c>experimental_onDecisionModelCallStart</c>.</summary>
    Func<DecisionModelEvent, CancellationToken, Task>? OnDecisionModelCallStart { get; }

    /// <summary>Called after the decision model response has been validated. Maps to <c>experimental_onDecisionModelCallEnd</c>.</summary>
    Func<DecisionModelEvent, CancellationToken, Task>? OnDecisionModelCallEnd { get; }

    /// <summary>Called when an experimental decision completes. Maps to <c>experimental_onDecideEnd</c>.</summary>
    Func<DecideEvent, CancellationToken, Task>? OnDecideEnd { get; }

    /// <summary>Deprecated alias of <see cref="OnDecideStart"/>. Maps to <c>experimental_onEvaluateStart</c>.</summary>
    [Obsolete("Use OnDecideStart instead.")]
    Func<EvaluateEvent, CancellationToken, Task>? OnEvaluateStart { get; }

    /// <summary>Deprecated alias of <see cref="OnDecisionModelCallStart"/>. Maps to <c>experimental_onEvaluationModelCallStart</c>.</summary>
    [Obsolete("Use OnDecisionModelCallStart instead.")]
    Func<EvaluateModelEvent, CancellationToken, Task>? OnEvaluationModelCallStart { get; }

    /// <summary>Deprecated alias of <see cref="OnDecisionModelCallEnd"/>. Maps to <c>experimental_onEvaluationModelCallEnd</c>.</summary>
    [Obsolete("Use OnDecisionModelCallEnd instead.")]
    Func<EvaluateModelEvent, CancellationToken, Task>? OnEvaluationModelCallEnd { get; }

    /// <summary>Deprecated alias of <see cref="OnDecideEnd"/>. Maps to <c>experimental_onEvaluateEnd</c>.</summary>
    [Obsolete("Use OnDecideEnd instead.")]
    Func<EvaluateEvent, CancellationToken, Task>? OnEvaluateEnd { get; }

    /// <summary>Called when an experimental extraction begins. Maps to <c>experimental_onExtractStart</c>.</summary>
    Func<ExtractStartEvent, CancellationToken, Task>? OnExtractStart { get; }

    /// <summary>Called before the extraction model call begins. Maps to <c>experimental_onExtractionModelCallStart</c>.</summary>
    Func<
        ExtractionModelCallStartEvent,
        CancellationToken,
        Task
    >? OnExtractionModelCallStart { get; }

    /// <summary>Called after the extraction model response has been validated. Maps to <c>experimental_onExtractionModelCallEnd</c>.</summary>
    Func<ExtractionModelCallEndEvent, CancellationToken, Task>? OnExtractionModelCallEnd { get; }

    /// <summary>Called when an experimental extraction completes. Maps to <c>experimental_onExtractEnd</c>.</summary>
    Func<ExtractEndEvent, CancellationToken, Task>? OnExtractEnd { get; }
}

/// <summary>
/// An event passed to <see cref="ITelemetry.OnEnd"/> when an operation completes. Stands in for the <c>OperationEndEvent</c> union in
/// <c>telemetry/telemetry.ts</c>, which C# cannot express.
/// </summary>
public interface IOperationEndEvent { }

/// <summary>
/// The wrapper an integration provides as <see cref="ITelemetry.ExecuteLanguageModelCall"/>. Upstream types it as a generic function,
/// which a C# delegate property cannot express.
/// </summary>
public interface IExecuteLanguageModelCall
{
    /// <summary>Runs <see cref="ExecuteLanguageModelCallOptions{T}.Execute"/> and returns its result.</summary>
    Task<T> ExecuteAsync<T>(
        ExecuteLanguageModelCallOptions<T> options,
        CancellationToken cancellationToken
    );
}

/// <summary>
/// The wrapper an integration provides as <see cref="ITelemetry.ExecuteTool"/>. Upstream types it as a generic function, which a C#
/// delegate property cannot express.
/// </summary>
public interface IExecuteTool
{
    /// <summary>Runs <see cref="ExecuteToolOptions{T}.Execute"/> and returns its result.</summary>
    Task<T> ExecuteAsync<T>(ExecuteToolOptions<T> options, CancellationToken cancellationToken);
}

/// <summary>
/// The options of <see cref="ITelemetry.ExecuteLanguageModelCall"/>: the model-call start event as context, the call id and the
/// function that performs the model call. Maps to <c>Partial&lt;LanguageModelCallStartEvent&gt; &amp; { callId, execute }</c>.
/// </summary>
public sealed class ExecuteLanguageModelCallOptions<T>
{
    /// <summary>Creates the options.</summary>
    public ExecuteLanguageModelCallOptions(
        LanguageModelCallStartEvent @event,
        Func<Task<T>> execute
    )
    {
        Event = @event;
        Execute = execute;
    }

    /// <summary>The start event of the model call, as the integrations see it.</summary>
    public LanguageModelCallStartEvent Event { get; }

    /// <summary>Id of the model call. Maps to <c>callId</c>.</summary>
    public string CallId => Event.CallId;

    /// <summary>Performs the model call. Maps to <c>execute</c>.</summary>
    public Func<Task<T>> Execute { get; }
}

/// <summary>
/// The options of <see cref="ITelemetry.ExecuteTool"/>: the tool-execution start event as context, the ids and the function that
/// runs the tool. Maps to <c>Partial&lt;ToolExecutionStartEvent&gt; &amp; { callId, toolCallId, execute }</c>.
/// </summary>
public sealed class ExecuteToolOptions<T>
{
    /// <summary>Creates the options.</summary>
    public ExecuteToolOptions(ToolExecutionStartEvent @event, Func<Task<T>> execute)
    {
        Event = @event;
        Execute = execute;
    }

    /// <summary>The start event of the tool execution, as the integrations see it.</summary>
    public ToolExecutionStartEvent Event { get; }

    /// <summary>Id of the generation call. Maps to <c>callId</c>.</summary>
    public string? CallId => Event.CallId;

    /// <summary>Id of the tool call. Maps to <c>toolCallId</c>.</summary>
    public string ToolCallId => Event.ToolCall.ToolCallId;

    /// <summary>Runs the tool. Maps to <c>execute</c>.</summary>
    public Func<Task<T>> Execute { get; }
}

/// <summary>
/// An event passed to <see cref="ITelemetry.OnStart"/> when an operation begins. Stands in for the <c>OperationStartEvent</c> union in
/// <c>telemetry/telemetry.ts</c>, which C# cannot express.
/// </summary>
public interface IOperationStartEvent { }
