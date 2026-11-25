// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetrySupport;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>onStepStart</c>.</summary>
public sealed class OnStepStartTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > onStepStart::creates agent_step and chat spans with correct attributes",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Creates_agent_step_and_chat_spans_with_correct_attributes()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);

        Assert.Equal(3, tracer.Spans.Count);

        var step = tracer.Spans[1];
        Assert.False(step.Ended);
        Assert.Equal("step 1", step.Name);
        Assert.Equal(
            new Dictionary<string, object?> { ["gen_ai.operation.name"] = "agent_step" },
            step.InitAttributes
        );
        Assert.Empty(step.RuntimeAttributes);

        var chat = tracer.Spans[2];
        Assert.False(chat.Ended);
        Assert.Equal("chat gpt-4", chat.Name);
        Assert.Equal(
            new Dictionary<string, object?>
            {
                ["gen_ai.operation.name"] = "chat",
                ["gen_ai.provider.name"] = "openai",
                ["gen_ai.request.max_tokens"] = 100,
                ["gen_ai.request.model"] = "gpt-4",
                ["gen_ai.request.temperature"] = 0.7,
            },
            chat.InitAttributes
        );
        Assert.Empty(chat.RuntimeAttributes);
    }
}
