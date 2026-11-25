// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateText;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetryEvents;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>onStepFinish</c>: the provider-executed tool tests.</summary>
public sealed class OnStepFinishTests
{
    private const string UpstreamCommit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e";
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry::onStepFinish::";

    private static readonly LanguageModelUsage Usage = new(2, 1, 3);

    private static async Task<IReadOnlyList<MockSpan>> RunAsync(params GeneratedContent[] content)
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        await new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = new StepErrorModel(
                    doGenerate: (_, _) =>
                        Task.FromResult(
                            new LanguageModelGenerateResult(content, FinishReason.Stop, Usage)
                        )
                ),
                Prompt = "hello",
                Telemetry = new TelemetryOptions { Integrations = [integration] },
            }
        );
        return tracer.Spans;
    }

    private static async Task AssertOutputMessagesAndObservedDefinitionsAsync()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        using var args = JsonDocument.Parse("{}");
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

    [Fact]
    [UpstreamTest(
        Test + "formats output messages in SemConv format",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
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
        Commit = UpstreamCommit
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
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Records_provider_executed_tool_results_and_observed_definitions()
    {
        await AssertOutputMessagesAndObservedDefinitionsAsync();

        var spans = await RunAsync(
            new GeneratedToolCall(
                "tc1",
                "mcp.ask_question",
                "{\"question\":\"What is this repository?\"}"
            )
            {
                ProviderExecuted = true,
                Dynamic = true,
            },
            new GeneratedToolResult(
                "tc1",
                "mcp.ask_question",
                "{\"answer\":\"An AI SDK repository.\"}"
            )
            {
                Dynamic = true,
            }
        );

        var chatSpan = spans[2];
        var toolSpan = spans[3];
        Assert.True(toolSpan.Ended);
        Assert.Equal("execute_tool mcp.ask_question", toolSpan.Name);
        Assert.Equal(
            new Dictionary<string, object?>
            {
                ["gen_ai.operation.name"] = "execute_tool",
                ["gen_ai.tool.call.arguments"] = "{\"question\":\"What is this repository?\"}",
                ["gen_ai.tool.call.id"] = "tc1",
                ["gen_ai.tool.name"] = "mcp.ask_question",
                ["gen_ai.tool.type"] = "extension",
            },
            toolSpan.InitAttributes
        );
        Assert.Equal(
            new Dictionary<string, object?>
            {
                ["gen_ai.tool.call.result"] = "{\"answer\":\"An AI SDK repository.\"}",
            },
            toolSpan.RuntimeAttributes
        );
        Assert.Equal(chatSpan.Activity.SpanId, toolSpan.Activity.ParentSpanId);
    }

    [Fact]
    [UpstreamTest(
        Test + "records only the final provider-executed tool result",
        Coverage = UpstreamCoverage.Partial,
        Commit = UpstreamCommit,
        Note = "The port's tool result has no preliminary flag, so only the final result is passed."
    )]
    public async Task Records_only_the_final_provider_executed_tool_result()
    {
        var spans = await RunAsync(
            new GeneratedToolCall("tc1", "code_execution", "{\"code\":\"1 + 1\"}")
            {
                ProviderExecuted = true,
            },
            new GeneratedToolResult("tc1", "code_execution", "{\"value\":2}")
        );

        Assert.Equal("{\"value\":2}", spans[3].Attributes["gen_ai.tool.call.result"]);
    }

    [Fact]
    [UpstreamTest(
        Test + "records provider-executed tool errors on the observed span",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Records_provider_executed_tool_errors_on_the_observed_span()
    {
        var error = new InvalidOperationException("provider tool failed");
        var spans = await RunAsync(
            new GeneratedToolCall("tc1", "web_search", "{\"query\":\"AI SDK\"}")
            {
                ProviderExecuted = true,
            },
            new GeneratedToolResult("tc1", "web_search", "null", isError: true) { Error = error }
        );

        var toolSpan = spans[3];
        Assert.Equal(System.Diagnostics.ActivityStatusCode.Error, toolSpan.Activity.Status);
        Assert.Equal(1, toolSpan.ExceptionCount);
        Assert.True(toolSpan.Ended);
    }
}
