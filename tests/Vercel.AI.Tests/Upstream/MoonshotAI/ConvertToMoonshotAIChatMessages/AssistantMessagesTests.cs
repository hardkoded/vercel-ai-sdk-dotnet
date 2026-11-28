// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.MoonshotAI.ConvertToMoonshotAIChatMessages.MoonshotMessagesSupport;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.ConvertToMoonshotAIChatMessages;

/// <summary>Port of <c>convert-to-moonshotai-chat-messages.test.ts</c> &gt; <c>assistant messages</c>.</summary>
public sealed class AssistantMessagesTests
{
    private const string Suite = Prefix + "assistant messages::";

    [Fact]
    [UpstreamTest(
        Suite + "should emit reasoning_content for reasoning parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_emit_reasoning_content_for_reasoning_parts()
    {
        var result = ConvertMessages(
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantReasoningPart("Let me think."),
                    new AssistantTextPart("The answer."),
                }
            )
        );

        Equal(
            result,
            "[{\"role\":\"assistant\",\"content\":\"The answer.\",\"reasoning_content\":\"Let me think.\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should convert tool calls and set content to null when there is no text",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_tool_calls_and_set_content_to_null_when_there_is_no_text()
    {
        var result = ConvertMessages(
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantToolCallPart(
                        new GeneratedToolCall("call-1", "get_weather", "{\"city\":\"Paris\"}")
                    ),
                }
            )
        );

        Equal(
            result,
            """[{"role":"assistant","content":null,"tool_calls":[{"id":"call-1","type":"function","function":{"name":"get_weather","arguments":"{\"city\":\"Paris\"}"}}]}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should keep text content when text and tool calls are present",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_keep_text_content_when_text_and_tool_calls_are_present()
    {
        var result = ConvertMessages(
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantTextPart("Checking."),
                    new AssistantToolCallPart(
                        new GeneratedToolCall("call-2", "get_weather", "{\"city\":\"Berlin\"}")
                    ),
                }
            )
        );

        Equal(
            result,
            """[{"role":"assistant","content":"Checking.","tool_calls":[{"id":"call-2","type":"function","function":{"name":"get_weather","arguments":"{\"city\":\"Berlin\"}"}}]}]"""
        );
    }
}
