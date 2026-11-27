// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.ConvertToCohereChatPrompt.PromptRunner;

namespace Vercel.AI.Tests.Upstream.Cohere.ConvertToCohereChatPrompt;

/// <summary>Port of <c>convert-to-cohere-chat-prompt.test.ts</c> &gt; <c>tool messages</c>.</summary>
public sealed class ToolMessagesTests
{
    private const string Suite = Prefix + "tool messages::";

    [Fact]
    [UpstreamTest(
        Suite + "should convert a tool call into a cohere chatbot message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_a_tool_call_into_a_cohere_chatbot_message()
    {
        var result = Run(
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantTextPart("Calling a tool"),
                    new AssistantToolCallPart(
                        new GeneratedToolCall(
                            "tool-call-1",
                            "tool-1",
                            "{\"test\":\"This is a tool message\"}"
                        )
                    ),
                }
            )
        );

        JsonAssert.Equal(
            result,
            "{\"messages\":[{\"role\":\"assistant\",\"tool_calls\":[{\"id\":\"tool-call-1\",\"type\":\"function\",\"function\":{\"name\":\"tool-1\",\"arguments\":\"{\\\"test\\\":\\\"This is a tool message\\\"}\"}}]}],\"documents\":[],\"warnings\":[]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should convert a single tool result into a cohere tool message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_a_single_tool_result_into_a_cohere_tool_message()
    {
        var result = Run(
            new ToolModelMessage(
                "tool-call-1",
                "tool-1",
                "{\"test\":\"This is a tool message\"}",
                false
            )
        );

        JsonAssert.Equal(
            result,
            "{\"messages\":[{\"role\":\"tool\",\"content\":\"{\\\"test\\\":\\\"This is a tool message\\\"}\",\"tool_call_id\":\"tool-call-1\"}],\"documents\":[],\"warnings\":[]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should convert multiple tool results into a cohere tool message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_multiple_tool_results_into_a_cohere_tool_message()
    {
        var result = Run(
            new ToolModelMessage(
                "tool-call-1",
                "tool-1",
                "{\"test\":\"This is a tool message\"}",
                false
            ),
            new ToolModelMessage("tool-call-2", "tool-2", "{\"something\":\"else\"}", false)
        );

        JsonAssert.Equal(
            result,
            "{\"messages\":[{\"role\":\"tool\",\"content\":\"{\\\"test\\\":\\\"This is a tool message\\\"}\",\"tool_call_id\":\"tool-call-1\"},{\"role\":\"tool\",\"content\":\"{\\\"something\\\":\\\"else\\\"}\",\"tool_call_id\":\"tool-call-2\"}],\"documents\":[],\"warnings\":[]}"
        );
    }
}
