// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetryEvents;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>generateText integration</c>.</summary>
public sealed class GenerateTextIntegrationTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry::generateText integration::";

    private static AiClient Client() =>
        new(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));

    private static TestLanguageModel Model(params GeneratedContent[][] responses)
    {
        var call = 0;
        return new TestLanguageModel("gpt-4")
        {
            OnGenerate = _ =>
            {
                var content = responses[Math.Min(call++, responses.Length - 1)];
                var hasToolCall = content.OfType<GeneratedToolCall>().Any();
                return new LanguageModelGenerateResult(
                    content,
                    hasToolCall ? FinishReason.ToolCalls : FinishReason.Stop,
                    new LanguageModelUsage(2, 1, 3)
                );
            },
        };
    }

    private static TelemetryOptions Telemetry(MockTracer tracer) =>
        new() { Integrations = [new OtelIntegration(new OtelOptions { Tracer = tracer.Source })] };

    private static MockSpan Span(MockTracer tracer, string name) =>
        tracer.Spans.First(span => span.Name == name);

    private static JsonNode? Json(MockSpan span, string key) =>
        ParseJsonAttribute(span.Attributes, key);

    private static void AssertJson(string expected, JsonNode? actual) =>
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expected), actual),
            "Actual: " + actual?.ToJsonString()
        );

    [Fact]
    [UpstreamTest(
        Test + "records system instructions on the chat span",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Records_system_instructions_on_the_chat_span()
    {
        using var tracer = new MockTracer();

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = Model([new GeneratedText("Hello")]),
                    System = "You are helpful",
                    Prompt = "Hello",
                    Telemetry = Telemetry(tracer),
                }
            );

        var chatSpan = Span(tracer, "chat gpt-4");
        AssertJson(
            """[{"parts":[{"content":"Hello","type":"text"}],"role":"user"}]""",
            Json(chatSpan, "gen_ai.input.messages")
        );
        AssertJson(
            """[{"content":"You are helpful","type":"text"}]""",
            Json(chatSpan, "gen_ai.system_instructions")
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "preserves allowed system messages in chat history order",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_allowed_system_messages_in_chat_history_order()
    {
        using var tracer = new MockTracer();

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = Model([new GeneratedText("Hello")]),
                    Messages =
                    [
                        new UserModelMessage("First"),
                        new SystemModelMessage("Apply this to the next request"),
                        new UserModelMessage("Second"),
                    ],
                    AllowSystemInMessages = true,
                    Telemetry = Telemetry(tracer),
                }
            );

        var chatSpan = Span(tracer, "chat gpt-4");
        AssertJson(
            """
            [
              {"parts":[{"content":"First","type":"text"}],"role":"user"},
              {"parts":[{"content":"Apply this to the next request","type":"text"}],"role":"system"},
              {"parts":[{"content":"Second","type":"text"}],"role":"user"}
            ]
            """,
            Json(chatSpan, "gen_ai.input.messages")
        );
        Assert.Null(Json(chatSpan, "gen_ai.system_instructions"));
    }

    [Fact]
    [UpstreamTest(
        Test + "records output message parts in the order they were produced",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Records_output_message_parts_in_the_order_they_were_produced()
    {
        using var tracer = new MockTracer();

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = Model([
                        new GeneratedText("Summary of earlier work."),
                        new GeneratedReasoning("I should answer."),
                        new GeneratedText("The answer is 7."),
                    ]),
                    Prompt = "Hello",
                    Telemetry = Telemetry(tracer),
                }
            );

        var chatSpan = Span(tracer, "chat gpt-4");
        var rootSpan = Span(tracer, "invoke_agent gpt-4");
        Assert.True(
            JsonNode.DeepEquals(
                Json(rootSpan, "gen_ai.output.messages"),
                Json(chatSpan, "gen_ai.output.messages")
            )
        );
        AssertJson(
            """
            [{"finish_reason":"stop","parts":[
              {"content":"Summary of earlier work.","type":"text"},
              {"content":"I should answer.","type":"reasoning"},
              {"content":"The answer is 7.","type":"text"}
            ],"role":"assistant"}]
            """,
            Json(chatSpan, "gen_ai.output.messages")
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "records step output parts the same way as the next step input",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Records_step_output_parts_the_same_way_as_the_next_step_input()
    {
        using var tracer = new MockTracer();
        var lookup = Tool.Function(
            "lookup",
            null,
            """{"type":"object","properties":{"id":{"type":"number"}},"required":["id"]}""",
            (_, _) => Task.FromResult("seven")
        );

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = Model(
                        [
                            new GeneratedText("Summary of earlier work."),
                            new GeneratedReasoning("I should look this up."),
                            new GeneratedText("Looking it up now."),
                            new GeneratedToolCall("call-1", "lookup", """{"id":7}"""),
                        ],
                        [new GeneratedText("It is seven.")]
                    ),
                    Prompt = "Find 7",
                    Tools = [lookup],
                    StopWhen = StopWhen.IsStepCount(2),
                    Telemetry = Telemetry(tracer),
                }
            );

        var chatSpans = tracer.Spans.Where(span => span.Name == "chat gpt-4").ToList();
        var firstOutput = (JsonArray)Json(chatSpans[0], "gen_ai.output.messages")!;
        var secondInput = (JsonArray)Json(chatSpans[1], "gen_ai.input.messages")!;
        var firstParts = firstOutput[0]!["parts"];
        var assistantParts = secondInput.First(message =>
            message!["role"]!.GetValue<string>() == "assistant"
        )!["parts"];
        Assert.True(JsonNode.DeepEquals(firstParts, assistantParts));
        AssertJson(
            """
            [
              {"content":"Summary of earlier work.","type":"text"},
              {"content":"I should look this up.","type":"reasoning"},
              {"content":"Looking it up now.","type":"text"},
              {"arguments":{"id":7},"id":"call-1","name":"lookup","type":"tool_call"}
            ]
            """,
            firstParts
        );
        AssertJson(
            """
            [{"finish_reason":"stop","parts":[
              {"content":"Summary of earlier work.","type":"text"},
              {"content":"I should look this up.","type":"reasoning"},
              {"content":"Looking it up now.","type":"text"},
              {"arguments":{"id":7},"id":"call-1","name":"lookup","type":"tool_call"},
              {"id":"call-1","response":"seven","type":"tool_call_response"},
              {"content":"It is seven.","type":"text"}
            ],"role":"assistant"}]
            """,
            Json(Span(tracer, "invoke_agent gpt-4"), "gen_ai.output.messages")
        );
    }
}
