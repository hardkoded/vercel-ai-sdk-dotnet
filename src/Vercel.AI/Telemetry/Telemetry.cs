// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;

namespace Vercel.AI;

/// <summary>
/// Implement this interface to create custom telemetry integrations. A callback is optional: return <c>null</c> from a property
/// to skip it. Maps to <c>Telemetry</c> in <c>telemetry/telemetry.ts</c>. Only <c>onStart</c>, <c>onObjectStepStart</c>, <c>onObjectStepEnd</c>, <c>onEnd</c>, <c>onLanguageModelCallStart</c> and
/// <c>onLanguageModelCallEnd</c> are ported so far.
/// </summary>
public interface ITelemetry
{
    /// <summary>
    /// Called when an operation begins, before any model call. Maps to <c>onStart</c>. Only <see cref="GenerateObjectStartEvent"/> is dispatched so far.
    /// </summary>
    Func<IOperationStartEvent, CancellationToken, Task>? OnStart { get; }

    /// <summary>Called before the model call of <c>generateObject</c> and <c>streamObject</c>. Maps to <c>onObjectStepStart</c>.</summary>
    Func<GenerateObjectStepStartEvent, CancellationToken, Task>? OnObjectStepStart { get; }

    /// <summary>Called after the model call of <c>generateObject</c> and <c>streamObject</c>. Maps to <c>onObjectStepEnd</c>.</summary>
    Func<GenerateObjectStepEndEvent, CancellationToken, Task>? OnObjectStepEnd { get; }

    /// <summary>Called when a text generation completes. Maps to <c>onEnd</c>.</summary>
    Func<GenerateTextResult, CancellationToken, Task>? OnEnd { get; }

    /// <summary>Called before each provider model call. Maps to <c>onLanguageModelCallStart</c>.</summary>
    Func<LanguageModelCallStartEvent, CancellationToken, Task>? OnLanguageModelCallStart { get; }

    /// <summary>Called after each provider model call returns, before tools run. Maps to <c>onLanguageModelCallEnd</c>.</summary>
    Func<LanguageModelCallEndEvent, CancellationToken, Task>? OnLanguageModelCallEnd { get; }
}

/// <summary>
/// An event passed to <see cref="ITelemetry.OnStart"/> when an operation begins. Stands in for the <c>OperationStartEvent</c> union in
/// <c>telemetry/telemetry.ts</c>, which C# cannot express.
/// </summary>
public interface IOperationStartEvent { }
