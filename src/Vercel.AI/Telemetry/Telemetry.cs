// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI;

/// <summary>
/// Implement this interface to create custom telemetry integrations. A callback is optional: return <c>null</c> from a property
/// to skip it. Maps to <c>Telemetry</c> in <c>telemetry/telemetry.ts</c>. Only the text generation callbacks (<c>onStart</c>, <c>onStepStart</c>, <c>onLanguageModelCallStart</c>,
/// <c>onLanguageModelCallEnd</c>, <c>onToolExecutionStart</c>, <c>onToolExecutionEnd</c>, <c>onStepEnd</c>, <c>onStepFinish</c>, <c>onEnd</c>,
/// <c>onAbort</c> and <c>onError</c>) are ported so far.
/// </summary>
public interface ITelemetry
{
    /// <summary>Called when a text generation begins, before any model call. Maps to <c>onStart</c>.</summary>
    Func<GenerateTextStartEvent, CancellationToken, Task>? OnStart { get; }

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
    Func<Exception, CancellationToken, Task>? OnError { get; }

    /// <summary>Called when a text generation completes. Maps to <c>onEnd</c>.</summary>
    Func<GenerateTextResult, CancellationToken, Task>? OnEnd { get; }

    /// <summary>Called before each provider model call. Maps to <c>onLanguageModelCallStart</c>.</summary>
    Func<LanguageModelCallStartEvent, CancellationToken, Task>? OnLanguageModelCallStart { get; }

    /// <summary>Called after each provider model call returns, before tools run. Maps to <c>onLanguageModelCallEnd</c>.</summary>
    Func<LanguageModelCallEndEvent, CancellationToken, Task>? OnLanguageModelCallEnd { get; }
}
