// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;

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

    public Func<Exception, CancellationToken, Task>? OnError { get; init; }

    public Func<
        GenerateObjectStepStartEvent,
        CancellationToken,
        Task
    >? OnObjectStepStart { get; init; }

    public Func<GenerateObjectStepEndEvent, CancellationToken, Task>? OnObjectStepEnd { get; init; }

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
}

/// <summary>
/// The global integrations (<c>AI_SDK_TELEMETRY_INTEGRATIONS</c>) apply to every call in the process. Tests that set them run alone,
/// like upstream's sequential test file.
/// </summary>
[CollectionDefinition("TelemetryRegistry", DisableParallelization = true)]
public sealed class TelemetryRegistryCollection;
