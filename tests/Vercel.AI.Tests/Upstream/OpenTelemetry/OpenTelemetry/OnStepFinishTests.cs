// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetryEvents;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of the output message tests of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>onStepFinish</c>.</summary>
public sealed class OnStepFinishTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > onStepFinish::";

    [Fact]
    [UpstreamTest(
        Test + "formats output messages in SemConv format",
        Coverage = UpstreamCoverage.Covered,
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Formats_output_messages_in_SemConv_format()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnLanguageModelCallEnd!(MakeLanguageModelCallEndEvent(), default);

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """[{"finish_reason":"stop","parts":[{"content":"Hello world","type":"text"}],"role":"assistant"}]"""
                ),
                ParseJsonAttribute(tracer.Spans[2].Attributes, "gen_ai.output.messages")
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "includes tool calls in output messages",
        Coverage = UpstreamCoverage.Covered,
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Includes_tool_calls_in_output_messages()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnLanguageModelCallEnd!(
            MakeLanguageModelCallEndEvent(
                [new GeneratedToolCall("tc1", "search", """{"q":"test"}""")],
                FinishReason.ToolCalls
            ),
            default
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """[{"finish_reason":"tool_call","parts":[{"arguments":{"q":"test"},"id":"tc1","name":"search","type":"tool_call"}],"role":"assistant"}]"""
                ),
                ParseJsonAttribute(tracer.Spans[2].Attributes, "gen_ai.output.messages")
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "records provider-executed tool results and observed definitions",
        Coverage = UpstreamCoverage.Partial,
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e",
        Note = "Only the gen_ai.output.messages and gen_ai.tool.definitions checks. The execute_tool span of a provider-executed call is tracked in #1707."
    )]
    public async Task Records_provider_executed_tool_results_and_observed_definitions()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        using var args = System.Text.Json.JsonDocument.Parse("{}");
        await integration.OnLanguageModelCallStart!(
            MakeLanguageModelCallStartEvent(
                tools:
                [
                    LanguageModelTool.ProviderDefined(
                        "openai.mcp",
                        "mcp",
                        args.RootElement.Clone()
                    ),
                ]
            ),
            default
        );
        await integration.OnLanguageModelCallEnd!(
            MakeLanguageModelCallEndEvent([
                new GeneratedToolCall(
                    "tc1",
                    "mcp.ask_question",
                    """{"question":"What is this repository?"}"""
                )
                {
                    ProviderExecuted = true,
                    Dynamic = true,
                },
                new GeneratedToolResult(
                    "tc1",
                    "mcp.ask_question",
                    """{"answer":"An AI SDK repository."}"""
                ),
            ]),
            default
        );

        var chatSpan = tracer.Spans[2];
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """[{"finish_reason":"stop","parts":[{"arguments":{"question":"What is this repository?"},"id":"tc1","name":"mcp.ask_question","type":"tool_call"},{"id":"tc1","response":{"answer":"An AI SDK repository."},"type":"tool_call_response"}],"role":"assistant"}]"""
                ),
                ParseJsonAttribute(chatSpan.Attributes, "gen_ai.output.messages")
            )
        );
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """[{"id":"openai.mcp","name":"mcp","type":"provider","args":{}},{"name":"mcp.ask_question","type":"extension"}]"""
                ),
                ParseJsonAttribute(chatSpan.Attributes, "gen_ai.tool.definitions")
            )
        );
    }
}
