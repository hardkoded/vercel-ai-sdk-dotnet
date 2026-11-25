// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>The event factories and span helpers at the top of <c>open-telemetry.test.ts</c>.</summary>
internal static class OpenTelemetrySupport
{
    public const string CallId = "test-call-1";

    /// <summary>Port of <c>makeOnStartEvent</c>.</summary>
    public static GenerateTextStartEvent MakeOnStartEvent(string? functionId = null) =>
        new GenerateTextStartEvent
        {
            CallId = CallId,
            OperationId = "ai.generateText",
            Provider = "openai.chat",
            ModelId = "gpt-4",
            MaxOutputTokens = 100,
            Temperature = 0.7,
        }.WithTelemetry(new TelemetryOptions { FunctionId = functionId });

    /// <summary>Port of <c>makeStepStartEvent</c>.</summary>
    public static GenerateTextStepStartEvent MakeStepStartEvent() =>
        new()
        {
            CallId = CallId,
            Provider = "openai.chat",
            ModelId = "gpt-4",
            StepNumber = 0,
        };

    /// <summary>Port of <c>makeLanguageModelCallStartEvent</c>.</summary>
    public static LanguageModelCallStartEvent MakeLanguageModelCallStartEvent() =>
        new()
        {
            CallId = CallId,
            Provider = "openai.chat",
            ModelId = "gpt-4",
        };

    /// <summary>Port of the <c>{ name, status, ended }</c> projection of the mock spans.</summary>
    public static IReadOnlyList<(
        string Name,
        ActivityStatusCode Status,
        string? Message,
        bool Ended
    )> Summarize(MockTracer tracer) =>
        [
            .. tracer.Spans.Select(span =>
                (span.Name, span.Activity.Status, span.Activity.StatusDescription, span.Ended)
            ),
        ];
}
