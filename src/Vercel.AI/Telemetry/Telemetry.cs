// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI;

/// <summary>
/// Implement this interface to create custom telemetry integrations. A callback is optional: return <c>null</c> from a property
/// to skip it. Maps to <c>Telemetry</c> in <c>telemetry/telemetry.ts</c>. Only <c>onEnd</c>, <c>onLanguageModelCallStart</c> and <c>onLanguageModelCallEnd</c> are
/// ported so far.
/// </summary>
public interface ITelemetry
{
    /// <summary>Called when a text generation completes. Maps to <c>onEnd</c>.</summary>
    Func<GenerateTextResult, CancellationToken, Task>? OnEnd { get; }

    /// <summary>Called before each provider model call. Maps to <c>onLanguageModelCallStart</c>.</summary>
    Func<LanguageModelCallStartEvent, CancellationToken, Task>? OnLanguageModelCallStart { get; }

    /// <summary>Called after each provider model call returns, before tools run. Maps to <c>onLanguageModelCallEnd</c>.</summary>
    Func<LanguageModelCallEndEvent, CancellationToken, Task>? OnLanguageModelCallEnd { get; }
}
