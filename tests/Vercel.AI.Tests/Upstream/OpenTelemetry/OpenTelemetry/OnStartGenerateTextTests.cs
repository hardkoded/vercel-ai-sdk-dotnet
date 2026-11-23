// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Prompt;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetryEvents;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>onStart (generateText)</c>.</summary>
public sealed class OnStartGenerateTextTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry::onStart (generateText)::";

    [Fact]
    [UpstreamTest(
        Test + "creates root span with correct attributes",
        Coverage = UpstreamCoverage.Covered,
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Creates_root_span_with_correct_attributes()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);

        var span = Assert.Single(tracer.Spans);
        Assert.False(span.Ended);
        Assert.Equal("invoke_agent gpt-4", span.Name);
        Assert.Equal(
            new Dictionary<string, object?>
            {
                ["gen_ai.input.messages"] =
                    """[{"role":"user","parts":[{"type":"text","content":"Hello"}]}]""",
                ["gen_ai.operation.name"] = "invoke_agent",
                ["gen_ai.provider.name"] = "openai",
                ["gen_ai.request.max_tokens"] = 100,
                ["gen_ai.request.model"] = "gpt-4",
                ["gen_ai.request.temperature"] = 0.7,
            },
            span.InitAttributes
        );
        Assert.Empty(span.RuntimeAttributes);
    }

    [Fact]
    [UpstreamTest(
        Test + "sets system_instructions when instructions are provided",
        Coverage = UpstreamCoverage.Covered,
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Sets_system_instructions_when_instructions_are_provided()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(Instructions.From("You are helpful")), default);

        var parsed = ParseJsonAttribute(
            tracer.Spans[0].InitAttributes,
            "gen_ai.system_instructions"
        );
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("""[{"content":"You are helpful","type":"text"}]"""),
                parsed
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "preserves functionId as gen_ai.agent.name",
        Coverage = UpstreamCoverage.Covered,
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Preserves_functionId_as_gen_ai_agent_name()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(functionId: "my-agent"), default);

        Assert.Equal("my-agent", tracer.Spans[0].InitAttributes["gen_ai.agent.name"]);
    }
}
