// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetrySupport;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>onStart (generateText)</c>.</summary>
public sealed class OnStartGenerateTextTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > onStart (generateText)::preserves functionId as gen_ai.agent.name",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_functionId_as_gen_ai_agent_name()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(functionId: "my-agent"), default);

        var span = Assert.Single(tracer.Spans);
        Assert.Equal("my-agent", span.InitAttributes["gen_ai.agent.name"]);
    }
}
