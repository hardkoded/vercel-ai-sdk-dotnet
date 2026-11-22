// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;

namespace Vercel.AI.Tests.Upstream.Telemetry;

/// <summary>Stands in for the object literal integrations (<c>{ onEnd }</c>) that upstream tests pass.</summary>
internal sealed class TestTelemetry : ITelemetry
{
    public Func<IOperationStartEvent, CancellationToken, Task>? OnStart { get; init; }

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
