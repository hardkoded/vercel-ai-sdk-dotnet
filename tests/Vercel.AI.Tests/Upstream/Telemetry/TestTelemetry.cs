// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Telemetry;

/// <summary>Stands in for the object literal integrations (<c>{ onEnd }</c>) that upstream tests pass.</summary>
internal sealed class TestTelemetry : ITelemetry
{
    public Func<GenerateTextResult, CancellationToken, Task>? OnEnd { get; init; }

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
