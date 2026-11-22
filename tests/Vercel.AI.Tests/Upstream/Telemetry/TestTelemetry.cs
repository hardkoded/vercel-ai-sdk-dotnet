// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Telemetry;

/// <summary>Stands in for the object literal integrations (<c>{ onEnd }</c>) that upstream tests pass.</summary>
internal sealed class TestTelemetry : ITelemetry
{
    public Func<IOperationStartEvent, CancellationToken, Task>? OnStart { get; init; }

    public Func<GenerateTextStepStartEvent, CancellationToken, Task>? OnStepStart { get; init; }

    public Func<
        ToolExecutionStartEvent,
        CancellationToken,
        Task
    >? OnToolExecutionStart { get; init; }

    public Func<ToolExecutionEndEvent, CancellationToken, Task>? OnToolExecutionEnd { get; init; }

    public Func<StepResult, CancellationToken, Task>? OnStepEnd { get; init; }

    public Func<StepResult, CancellationToken, Task>? OnStepFinish { get; init; }

    public Func<StreamAbortContext, CancellationToken, Task>? OnAbort { get; init; }

    public Func<
        GenerateObjectStepStartEvent,
        CancellationToken,
        Task
    >? OnObjectStepStart { get; init; }

    public Func<GenerateObjectStepEndEvent, CancellationToken, Task>? OnObjectStepEnd { get; init; }

    public Func<GenerateTextStepErrorEvent, CancellationToken, Task>? OnStepError { get; init; }

    /// <summary>Receives the end event of a text generation.</summary>
    public Func<GenerateTextResult, CancellationToken, Task>? OnEnd { get; init; }

    /// <summary>Receives the end event of every other operation (embed, rerank, transcribe).</summary>
    public Func<IOperationEndEvent, CancellationToken, Task>? OnOperationEnd { get; init; }

    Func<IOperationEndEvent, CancellationToken, Task>? ITelemetry.OnEnd =>
        OnEnd is null && OnOperationEnd is null
            ? null
            : (e, cancellationToken) =>
                e is GenerateTextResult result && OnEnd is not null
                    ? OnEnd(result, cancellationToken)
                : e is not GenerateTextResult && OnOperationEnd is not null
                    ? OnOperationEnd(e, cancellationToken)
                : Task.CompletedTask;

    public Func<EmbeddingModelCallStartEvent, CancellationToken, Task>? OnEmbedStart { get; init; }

    public Func<EmbeddingModelCallEndEvent, CancellationToken, Task>? OnEmbedEnd { get; init; }

    public Func<RerankingModelCallStartEvent, CancellationToken, Task>? OnRerankStart { get; init; }

    public Func<RerankingModelCallEndEvent, CancellationToken, Task>? OnRerankEnd { get; init; }

    public Func<
        StreamTranscriptionStartEvent,
        CancellationToken,
        Task
    >? OnStreamTranscriptionStart { get; init; }

    public Func<
        StreamTranscriptionEndEvent,
        CancellationToken,
        Task
    >? OnStreamTranscriptionEnd { get; init; }

    public IExecuteLanguageModelCall? ExecuteLanguageModelCall { get; init; }

    public IExecuteTool? ExecuteTool { get; init; }

    public Func<
        LanguageModelCallStartEvent,
        CancellationToken,
        Task
    >? OnLanguageModelCallStart { get; init; }

    public Func<
        LanguageModelCallEndEvent,
        CancellationToken,
        Task
    >? OnLanguageModelCallEnd { get; init; }

    public Func<TelemetryErrorEvent, CancellationToken, Task>? OnError { get; init; }

    public Func<DecideEvent, CancellationToken, Task>? OnDecideStart { get; init; }

    public Func<
        DecisionModelEvent,
        CancellationToken,
        Task
    >? OnDecisionModelCallStart { get; init; }

    public Func<DecisionModelEvent, CancellationToken, Task>? OnDecisionModelCallEnd { get; init; }

    public Func<DecideEvent, CancellationToken, Task>? OnDecideEnd { get; init; }

    [Obsolete("Use OnDecideStart instead.")]
    public Func<EvaluateEvent, CancellationToken, Task>? OnEvaluateStart { get; init; }

    [Obsolete("Use OnDecisionModelCallStart instead.")]
    public Func<
        EvaluateModelEvent,
        CancellationToken,
        Task
    >? OnEvaluationModelCallStart { get; init; }

    [Obsolete("Use OnDecisionModelCallEnd instead.")]
    public Func<
        EvaluateModelEvent,
        CancellationToken,
        Task
    >? OnEvaluationModelCallEnd { get; init; }

    [Obsolete("Use OnDecideEnd instead.")]
    public Func<EvaluateEvent, CancellationToken, Task>? OnEvaluateEnd { get; init; }

    public Func<ExtractStartEvent, CancellationToken, Task>? OnExtractStart { get; init; }

    public Func<
        ExtractionModelCallStartEvent,
        CancellationToken,
        Task
    >? OnExtractionModelCallStart { get; init; }

    public Func<
        ExtractionModelCallEndEvent,
        CancellationToken,
        Task
    >? OnExtractionModelCallEnd { get; init; }

    public Func<ExtractEndEvent, CancellationToken, Task>? OnExtractEnd { get; init; }
}

/// <summary>
/// The global integrations (<c>AI_SDK_TELEMETRY_INTEGRATIONS</c>) apply to every call in the process. Tests that set them run alone,
/// like upstream's sequential test file.
/// </summary>
[CollectionDefinition("TelemetryRegistry", DisableParallelization = true)]
public sealed class TelemetryRegistryCollection;
