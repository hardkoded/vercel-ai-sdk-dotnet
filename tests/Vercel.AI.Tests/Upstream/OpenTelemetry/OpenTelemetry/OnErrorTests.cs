// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetrySupport;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>onError</c>.</summary>
public sealed class OnErrorTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > onError::records error on root, step, and chat spans",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Records_error_on_root_step_and_chat_spans()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);

        await integration.OnError!(
            new TelemetryErrorEvent(CallId, new Exception("something went wrong")),
            default
        );

        Assert.Equal(
            [
                ("invoke_agent gpt-4", ActivityStatusCode.Error, "something went wrong", true),
                ("step 1", ActivityStatusCode.Error, "something went wrong", true),
                ("chat gpt-4", ActivityStatusCode.Error, "something went wrong", true),
            ],
            Summarize(tracer)
        );
    }
}
