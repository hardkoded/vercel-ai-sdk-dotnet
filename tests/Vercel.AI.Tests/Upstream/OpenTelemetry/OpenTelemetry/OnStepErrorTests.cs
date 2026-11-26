// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetrySupport;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>onStepError</c>.</summary>
public sealed class OnStepErrorTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > onStepError::records error on step and chat spans and keeps the root span open for onEnd",
        Coverage = UpstreamCoverage.Covered,
        Commit = "95b78c93e7a875ea134a036490916c5d7ef87f90"
    )]
    public async Task Records_error_on_step_and_chat_spans_and_keeps_the_root_span_open()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);

        await integration.OnStepError!(
            new GenerateTextStepErrorEvent(MakeStepStartEvent(), new Exception("step failed")),
            default
        );

        Assert.Equal(
            [
                ("invoke_agent gpt-4", ActivityStatusCode.Unset, (string?)null, false),
                ("step 1", ActivityStatusCode.Error, "step failed", true),
                ("chat gpt-4", ActivityStatusCode.Error, "step failed", true),
            ],
            Summarize(tracer)
        );

        var step = new StepResult(
            "Hello world",
            null,
            [],
            [],
            FinishReason.Stop,
            LanguageModelUsage.Empty,
            []
        )
        {
            CallId = CallId,
        };
        await integration.OnEnd!(
            new GenerateTextResult(
                "Hello world",
                null,
                [step],
                FinishReason.Stop,
                LanguageModelUsage.Empty,
                null,
                []
            ),
            default
        );

        var root = tracer.Spans[0];
        Assert.True(root.Ended);
        Assert.Equal(ActivityStatusCode.Unset, root.Activity.Status);
    }
}
