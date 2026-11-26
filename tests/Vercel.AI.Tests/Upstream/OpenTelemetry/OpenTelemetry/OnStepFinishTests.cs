// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateText;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetrySupport;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>onStepFinish</c>: the provider-executed tool tests.</summary>
public sealed class OnStepFinishTests
{
    private const string UpstreamCommit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e";
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > onStepFinish::";

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

    [Fact]
    [UpstreamTest(
        Test + "records provider-executed tool results and observed definitions",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Records_provider_executed_tool_results_and_observed_definitions()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        LanguageModelTool[] tools =
        [
            LanguageModelTool.ProviderDefined(
                "openai.mcp",
                "mcp",
                JsonDocument.Parse("{}").RootElement.Clone()
            ),
        ];

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(
            MakeLanguageModelCallStartEvent(tools: tools),
            default
        );
        await integration.OnLanguageModelCallEnd!(
            MakeLanguageModelCallEndEvent(
                content:
                [
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
                    },
                ]
            ),
            default
        );

        var chatSpan = tracer.Spans[2];
        var observed = new JsonObject
        {
            ["gen_ai.output.messages"] = ParseJsonAttribute(
                chatSpan.Attributes,
                "gen_ai.output.messages"
            ),
            ["gen_ai.tool.definitions"] = ParseJsonAttribute(
                chatSpan.Attributes,
                "gen_ai.tool.definitions"
            ),
        };
        OtelJson.Equal(
            """
            {"gen_ai.output.messages":[{"finish_reason":"stop","parts":[{"arguments":{"question":"What is this repository?"},"id":"tc1","name":"mcp.ask_question","type":"tool_call"},{"id":"tc1","response":{"answer":"An AI SDK repository."},"type":"tool_call_response"}],"role":"assistant"}],"gen_ai.tool.definitions":[{"args":{},"id":"openai.mcp","name":"mcp","type":"provider"},{"name":"mcp.ask_question","type":"extension"}]}
            """,
            observed
        );
        OtelJson.Equal(
            """
            {"ended":true,"initAttributes":{"gen_ai.operation.name":"execute_tool","gen_ai.tool.call.arguments":"{\"question\":\"What is this repository?\"}","gen_ai.tool.call.id":"tc1","gen_ai.tool.name":"mcp.ask_question","gen_ai.tool.type":"extension"},"name":"execute_tool mcp.ask_question","runtimeAttributes":{"gen_ai.tool.call.result":"{\"answer\":\"An AI SDK repository.\"}"}}
            """,
            SerializeSpan(tracer.Spans[3])
        );
        Assert.Equal(chatSpan.Activity.SpanId, tracer.Spans[3].Activity.ParentSpanId);
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

    [Fact]
    [UpstreamTest(
        Test + "sets response attributes and token usage on the chat span",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Sets_response_attributes_and_token_usage_on_the_chat_span()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnLanguageModelCallEnd!(
            MakeLanguageModelCallEndEvent(modelId: "response-model"),
            default
        );
        await integration.OnStepEnd!(MakeStepFinishEvent(), default);

        OtelJson.Equal(
            """
            {"ended":true,"initAttributes":{"gen_ai.operation.name":"chat","gen_ai.provider.name":"openai","gen_ai.request.max_tokens":100,"gen_ai.request.model":"gpt-4","gen_ai.request.temperature":0.7},"name":"chat gpt-4","runtimeAttributes":{"gen_ai.client.operation.duration":1,"gen_ai.output.messages":"[{\"role\":\"assistant\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello world\"}],\"finish_reason\":\"stop\"}]","gen_ai.provider.name":"openai","gen_ai.response.finish_reasons":["stop"],"gen_ai.response.id":"test-response-id","gen_ai.response.model":"response-model","gen_ai.usage.input_tokens":10,"gen_ai.usage.output_tokens":20}}
            """,
            SerializeSpan(tracer.Spans[2])
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "updates provider attribution when the response provider differs",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Updates_provider_attribution_when_the_response_provider_differs()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnLanguageModelCallEnd!(
            MakeLanguageModelCallEndEvent(
                provider: "anthropic.messages",
                modelId: "fallback-model"
            ),
            default
        );

        var attributes = tracer.Spans[2].Attributes;
        Assert.Equal("anthropic", attributes["gen_ai.provider.name"]);
        Assert.Equal("fallback-model", attributes["gen_ai.response.model"]);
    }

    [Fact]
    [UpstreamTest(
        Test + "sets GenAI client performance attributes on the chat span",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Sets_GenAI_client_performance_attributes_on_the_chat_span()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnLanguageModelCallEnd!(
            MakeLanguageModelCallEndEvent(
                performance: new ModelCallPerformance
                {
                    ResponseTimeMs = 1234,
                    EffectiveOutputTokensPerSecond = 20,
                    OutputTokensPerSecond = 25,
                    InputTokensPerSecond = 10,
                    EffectiveTotalTokensPerSecond = 30,
                    TimeToFirstOutputMs = 345,
                    TimeBetweenOutputChunksMs = new OutputChunkTimingStats(
                        min: 10,
                        max: 100,
                        avg: 67,
                        median: 50,
                        p10: 20,
                        p90: 90
                    ),
                }
            ),
            default
        );

        var attributes = tracer.Spans[2].Attributes;
        var observed = new Dictionary<string, object?>
        {
            ["duration"] = attributes["gen_ai.client.operation.duration"],
            ["timeToFirstChunk"] = attributes["gen_ai.client.operation.time_to_first_chunk"],
            ["timePerOutputChunk"] = attributes["gen_ai.client.operation.time_per_output_chunk"],
        };
        OtelJson.Equal(
            """
            {"duration":1.234,"timePerOutputChunk":0.067,"timeToFirstChunk":0.345}
            """,
            observed
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "formats output messages in SemConv format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Formats_output_messages_in_SemConv_format()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnLanguageModelCallEnd!(MakeLanguageModelCallEndEvent(), default);
        await integration.OnStepEnd!(MakeStepFinishEvent(), default);

        var chatSpan = tracer.Spans[2];
        OtelJson.Equal(
            """
            [{"finish_reason":"stop","parts":[{"content":"Hello world","type":"text"}],"role":"assistant"}]
            """,
            ParseJsonAttribute(chatSpan.Attributes, "gen_ai.output.messages")
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "includes tool calls in output messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Includes_tool_calls_in_output_messages()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        var toolCall = new GeneratedToolCall("tc1", "search", "{\"q\":\"test\"}");
        await integration.OnLanguageModelCallEnd!(
            MakeLanguageModelCallEndEvent(
                content: [toolCall],
                finishReason: FinishReason.ToolCalls
            ),
            default
        );
        await integration.OnStepEnd!(
            MakeStepFinishEvent(
                text: null,
                toolCalls: [toolCall],
                finishReason: FinishReason.ToolCalls
            ),
            default
        );

        var chatSpan = tracer.Spans[2];
        OtelJson.Equal(
            """
            [{"finish_reason":"tool_call","parts":[{"arguments":{"q":"test"},"id":"tc1","name":"search","type":"tool_call"}],"role":"assistant"}]
            """,
            ParseJsonAttribute(chatSpan.Attributes, "gen_ai.output.messages")
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "keeps deferred provider tool calls visible before their result arrives",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Keeps_deferred_provider_tool_calls_visible_before_their_result_arrives()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnLanguageModelCallEnd!(
            MakeLanguageModelCallEndEvent(
                content:
                [
                    new GeneratedToolCall("tc1", "code_execution", "{\"code\":\"1 + 1\"}")
                    {
                        ProviderExecuted = true,
                    },
                ]
            ),
            default
        );

        var toolSpan = tracer.Spans[3];
        Assert.Equal("execute_tool code_execution", toolSpan.Name);
        Assert.True(toolSpan.Ended);
        Assert.False(toolSpan.Attributes.ContainsKey("gen_ai.tool.call.result"));

        await integration.OnStepEnd!(MakeStepFinishEvent(), default);
        await integration.OnStepStart!(
            new GenerateTextStepStartEvent
            {
                CallId = CallId,
                Provider = "openai.chat",
                ModelId = "gpt-4",
                StepNumber = 1,
            },
            default
        );
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnLanguageModelCallEnd!(
            MakeLanguageModelCallEndEvent(
                content: [new GeneratedToolResult("tc1", "code_execution", "{\"value\":2}")]
            ),
            default
        );

        Assert.Single(tracer.Spans, span => span.Name.StartsWith("execute_tool"));
        OtelJson.Equal(
            """
            [{"finish_reason":"stop","parts":[{"id":"tc1","response":{"value":2},"type":"tool_call_response"}],"role":"assistant"}]
            """,
            ParseJsonAttribute(tracer.Spans[5].Attributes, "gen_ai.output.messages")
        );
    }
}
