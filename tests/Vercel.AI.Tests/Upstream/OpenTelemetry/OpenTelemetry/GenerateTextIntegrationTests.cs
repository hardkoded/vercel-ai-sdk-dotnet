// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.OpenTelemetry;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetrySupport;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>generateText integration</c>.</summary>
public sealed class GenerateTextIntegrationTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > generateText integration::";

    [Fact]
    [UpstreamTest(
        Test + "records system instructions on the chat span",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Records_system_instructions_on_the_chat_span()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = OpenAiChatMockModel.Generate([new GeneratedText("Hello")]),
                Instructions = Instructions.From("You are helpful"),
                Prompt = "Hello",
                Telemetry = new TelemetryOptions { Integrations = [integration] },
            }
        );

        var chatSpan = tracer.FinishedSpans.First(span => span.Name == "chat gpt-4");
        var observed = new JsonObject
        {
            ["gen_ai.input.messages"] = ParseJsonAttribute(
                chatSpan.Attributes,
                "gen_ai.input.messages"
            ),
            ["gen_ai.system_instructions"] = ParseJsonAttribute(
                chatSpan.Attributes,
                "gen_ai.system_instructions"
            ),
        };
        OtelJson.Equal(
            """
            {"gen_ai.input.messages":[{"parts":[{"content":"Hello","type":"text"}],"role":"user"}],"gen_ai.system_instructions":[{"content":"You are helpful","type":"text"}]}
            """,
            observed
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
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = OpenAiChatMockModel.Generate([new GeneratedText("Hello")]),
                Messages =
                [
                    new UserModelMessage("First"),
                    new SystemModelMessage("Apply this to the next request"),
                    new UserModelMessage("Second"),
                ],
                AllowSystemInMessages = true,
                Telemetry = new TelemetryOptions { Integrations = [integration] },
            }
        );

        var chatSpan = tracer.FinishedSpans.First(span => span.Name == "chat gpt-4");
        var observed = new JsonObject
        {
            ["gen_ai.input.messages"] = ParseJsonAttribute(
                chatSpan.Attributes,
                "gen_ai.input.messages"
            ),
        };
        OtelJson.Equal(
            """
            {"gen_ai.input.messages":[{"parts":[{"content":"First","type":"text"}],"role":"user"},{"parts":[{"content":"Apply this to the next request","type":"text"}],"role":"system"},{"parts":[{"content":"Second","type":"text"}],"role":"user"}]}
            """,
            observed
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "records output message parts in the order they were produced",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7dc49985c58ae25456aa1f23c28f2f891266629f"
    )]
    public async Task Records_output_message_parts_in_the_order_they_were_produced()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = OpenAiChatMockModel.Generate([
                    new GeneratedText("Summary of earlier work."),
                    new GeneratedReasoning("I should answer."),
                    new GeneratedText("The answer is 7."),
                ]),
                Prompt = "Hello",
                Telemetry = new TelemetryOptions { Integrations = [integration] },
            }
        );

        var chatSpan = tracer.FinishedSpans.First(span => span.Name == "chat gpt-4");
        var rootSpan = tracer.FinishedSpans.First(span => span.Name == "invoke_agent gpt-4");
        Assert.True(
            JsonNode.DeepEquals(
                ParseJsonAttribute(rootSpan.Attributes, "gen_ai.output.messages"),
                ParseJsonAttribute(chatSpan.Attributes, "gen_ai.output.messages")
            )
        );
        OtelJson.Equal(
            """
            [{"finish_reason":"stop","parts":[{"content":"Summary of earlier work.","type":"text"},{"content":"I should answer.","type":"reasoning"},{"content":"The answer is 7.","type":"text"}],"role":"assistant"}]
            """,
            ParseJsonAttribute(chatSpan.Attributes, "gen_ai.output.messages")
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "records step output parts the same way as the next step input",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7dc49985c58ae25456aa1f23c28f2f891266629f"
    )]
    public async Task Records_step_output_parts_the_same_way_as_the_next_step_input()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        var lookup = Tool.Function(
            "lookup",
            null,
            "{\"type\":\"object\",\"properties\":{\"id\":{\"type\":\"number\"}},\"required\":[\"id\"]}",
            (_, _) => Task.FromResult("seven")
        );

        await new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = OpenAiChatMockModel.Generate(
                    [
                        new GeneratedText("Summary of earlier work."),
                        new GeneratedReasoning("I should look this up."),
                        new GeneratedText("Looking it up now."),
                        new GeneratedToolCall("call-1", "lookup", "{\"id\":7}"),
                    ],
                    [new GeneratedText("It is seven.")]
                ),
                Prompt = "Find 7",
                Tools = [lookup],
                StopWhen = StopWhen.IsStepCount(2),
                Telemetry = new TelemetryOptions { Integrations = [integration] },
            }
        );

        var chatSpans = tracer.FinishedSpans.Where(span => span.Name == "chat gpt-4").ToList();
        var firstOutput = ParseJsonAttribute(chatSpans[0].Attributes, "gen_ai.output.messages")!
            .AsArray()[0]!["parts"]!;
        var secondInput = ParseJsonAttribute(chatSpans[1].Attributes, "gen_ai.input.messages")!
            .AsArray();
        var assistantParts = secondInput.First(message =>
            (string?)message!["role"] == "assistant"
        )!["parts"];
        Assert.True(JsonNode.DeepEquals(firstOutput, assistantParts));
        OtelJson.Equal(
            """
            [{"content":"Summary of earlier work.","type":"text"},{"content":"I should look this up.","type":"reasoning"},{"content":"Looking it up now.","type":"text"},{"arguments":{"id":7},"id":"call-1","name":"lookup","type":"tool_call"}]
            """,
            firstOutput
        );
        var rootSpan = tracer.FinishedSpans.First(span => span.Name == "invoke_agent gpt-4");
        OtelJson.Equal(
            """
            [{"finish_reason":"stop","parts":[{"content":"Summary of earlier work.","type":"text"},{"content":"I should look this up.","type":"reasoning"},{"content":"Looking it up now.","type":"text"},{"arguments":{"id":7},"id":"call-1","name":"lookup","type":"tool_call"},{"id":"call-1","response":"seven","type":"tool_call_response"},{"content":"It is seven.","type":"text"}],"role":"assistant"}]
            """,
            ParseJsonAttribute(rootSpan.Attributes, "gen_ai.output.messages")
        );
    }
}
