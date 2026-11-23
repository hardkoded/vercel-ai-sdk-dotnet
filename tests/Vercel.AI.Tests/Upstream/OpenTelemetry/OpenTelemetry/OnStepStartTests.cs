// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetryEvents;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>onStepStart</c>.</summary>
public sealed class OnStepStartTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry::onStepStart::";

    private static async Task<(MockTracer Tracer, OtelIntegration Integration)> Start()
    {
        var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        return (tracer, integration);
    }

    [Fact]
    [UpstreamTest(
        Test + "creates agent_step and chat spans with correct attributes",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Creates_agent_step_and_chat_spans_with_correct_attributes()
    {
        var (tracer, integration) = await Start();
        using var _ = tracer;
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);

        Assert.Equal(3, tracer.Spans.Count);
        Assert.False(tracer.Spans[1].Ended);
        Assert.Equal("step 1", tracer.Spans[1].Name);
        Assert.Equal(
            new Dictionary<string, object?> { ["gen_ai.operation.name"] = "agent_step" },
            tracer.Spans[1].InitAttributes
        );
        Assert.Equal("chat gpt-4", tracer.Spans[2].Name);
        Assert.Equal(
            new Dictionary<string, object?>
            {
                ["gen_ai.operation.name"] = "chat",
                ["gen_ai.provider.name"] = "openai",
                ["gen_ai.request.max_tokens"] = 100,
                ["gen_ai.request.model"] = "gpt-4",
                ["gen_ai.request.temperature"] = 0.7,
            },
            tracer.Spans[2].InitAttributes
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "sets gen_ai.input.messages when messages are provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Sets_gen_ai_input_messages_when_messages_are_provided()
    {
        var (tracer, integration) = await Start();
        using var _ = tracer;
        await integration.OnLanguageModelCallStart!(
            MakeLanguageModelCallStartEvent(messages: [new UserModelMessage("Hello")]),
            default
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("""[{"parts":[{"content":"Hello","type":"text"}],"role":"user"}]"""),
                ParseJsonAttribute(tracer.Spans[2].InitAttributes, "gen_ai.input.messages")
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "sets gen_ai.system_instructions when instructions are provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Sets_gen_ai_system_instructions_when_instructions_are_provided()
    {
        var (tracer, integration) = await Start();
        using var _ = tracer;
        await integration.OnLanguageModelCallStart!(
            MakeLanguageModelCallStartEvent(instructions: Instructions.From("You are helpful")),
            default
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("""[{"content":"You are helpful","type":"text"}]"""),
                ParseJsonAttribute(tracer.Spans[2].InitAttributes, "gen_ai.system_instructions")
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "sets gen_ai.tool.definitions when tools provided",
        Coverage = UpstreamCoverage.Partial,
        Note = "Upstream passes a tool without an input schema. The port's function tool always carries one, so the definition has inputSchema too."
    )]
    public async Task Sets_gen_ai_tool_definitions_when_tools_provided()
    {
        var (tracer, integration) = await Start();
        using var _ = tracer;
        using var schema = JsonDocument.Parse("""{"type":"object"}""");
        await integration.OnLanguageModelCallStart!(
            MakeLanguageModelCallStartEvent(
                tools:
                [
                    new LanguageModelTool("get_weather", "Get weather", schema.RootElement.Clone()),
                ]
            ),
            default
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """[{"description":"Get weather","inputSchema":{"type":"object"},"name":"get_weather","type":"function"}]"""
                ),
                ParseJsonAttribute(tracer.Spans[2].InitAttributes, "gen_ai.tool.definitions")
            )
        );
    }
}
