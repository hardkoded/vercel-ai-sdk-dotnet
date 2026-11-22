// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;

namespace Vercel.AI;

/// <summary>
/// Implement this interface to create custom telemetry integrations. A callback is optional: return <c>null</c> from a property
/// to skip it. Maps to <c>Telemetry</c> in <c>telemetry/telemetry.ts</c>. Only <c>onEnd</c>, <c>onLanguageModelCallStart</c>, <c>onLanguageModelCallEnd</c>, <c>onError</c> and the extract and decide
/// callbacks are ported so far.
/// </summary>
public interface ITelemetry
{
    /// <summary>Called when a text generation completes. Maps to <c>onEnd</c>.</summary>
    Func<GenerateTextResult, CancellationToken, Task>? OnEnd { get; }

    /// <summary>Called before each provider model call. Maps to <c>onLanguageModelCallStart</c>.</summary>
    Func<LanguageModelCallStartEvent, CancellationToken, Task>? OnLanguageModelCallStart { get; }

    /// <summary>Called after each provider model call returns, before tools run. Maps to <c>onLanguageModelCallEnd</c>.</summary>
    Func<LanguageModelCallEndEvent, CancellationToken, Task>? OnLanguageModelCallEnd { get; }

    /// <summary>Called when an operation fails. Maps to <c>onError</c>.</summary>
    Func<TelemetryErrorEvent, CancellationToken, Task>? OnError { get; }

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
