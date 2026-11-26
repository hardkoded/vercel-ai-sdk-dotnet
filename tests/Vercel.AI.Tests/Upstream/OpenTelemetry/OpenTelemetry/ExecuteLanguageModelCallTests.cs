// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetrySupport;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>executeLanguageModelCall</c>.</summary>
public sealed class ExecuteLanguageModelCallTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > executeLanguageModelCall::";

    [Fact]
    [UpstreamTest(
        Test + "runs the model call inside the active chat span context",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Runs_the_model_call_inside_the_active_chat_span_context()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        Activity? activeSpan = null;

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);

        var result = await integration.ExecuteLanguageModelCall!.ExecuteAsync(
            new ExecuteLanguageModelCallOptions<string>(MakeLanguageModelCallStartEvent(), () =>
            {
                activeSpan = Activity.Current;
                return Task.FromResult("result");
            }),
            default
        );

        Assert.Equal("result", result);
        var activeSpanRecord = tracer.Spans.FirstOrDefault(span =>
            ReferenceEquals(span.Activity, activeSpan)
        );
        Assert.Equal("chat gpt-4", activeSpanRecord?.Name);
    }
}
