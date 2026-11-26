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

    [Fact]
    [UpstreamTest(
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > onStart (generateText)::creates root span with correct attributes",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Creates_root_span_with_correct_attributes()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);

        Assert.Single(tracer.Spans);
        OtelJson.Equal(
            """
            {"ended":false,"initAttributes":{"gen_ai.input.messages":"[{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello\"}]}]","gen_ai.operation.name":"invoke_agent","gen_ai.provider.name":"openai","gen_ai.request.max_tokens":100,"gen_ai.request.model":"gpt-4","gen_ai.request.temperature":0.7},"name":"invoke_agent gpt-4","runtimeAttributes":{}}
            """,
            SerializeSpan(tracer.Spans[0])
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > onStart (generateText)::sets system_instructions when instructions are provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Sets_system_instructions_when_instructions_are_provided()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(
            MakeOnStartEvent(instructions: Vercel.AI.Prompt.Instructions.From("You are helpful")),
            default
        );

        var attrs = tracer.Spans[0].InitAttributes;
        OtelJson.Equal(
            """
            [{"content":"You are helpful","type":"text"}]
            """,
            ParseJsonAttribute(attrs, "gen_ai.system_instructions")
        );
    }
}
