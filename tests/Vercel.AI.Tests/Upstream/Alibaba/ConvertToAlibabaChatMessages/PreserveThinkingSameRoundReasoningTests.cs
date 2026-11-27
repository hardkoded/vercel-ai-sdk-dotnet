// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Alibaba;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Alibaba.ConvertToAlibabaChatMessages;

/// <summary>Port of <c>convert-to-alibaba-chat-messages.test.ts</c> &gt; <c>convertToAlibabaChatMessages &gt; preserveThinking &gt; same-round reasoning</c>.</summary>
public sealed class PreserveThinkingSameRoundReasoningTests
{
    private const string Prefix =
        "packages/alibaba/src/convert-to-alibaba-chat-messages.test.ts::convertToAlibabaChatMessages > preserveThinking > same-round reasoning::";

    [Fact]
    [UpstreamTest(
        Prefix
            + "should keep reasoning with tool calls after the last user message even when preserveThinking is false",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_keep_reasoning_with_tool_calls_after_the_last_user_message_even_when_preserveThinking_is_false()
    {
        var result = AlibabaChatMessages.Convert(
            [
                new UserModelMessage([new TextContentPart("Weather in San Francisco?")]),
                new AssistantModelMessage([
                    new AssistantReasoningPart("Need to check the weather."),
                    new AssistantToolCallPart(
                        new GeneratedToolCall(
                            "call-1",
                            "get_weather",
                            """{"location":"San Francisco"}"""
                        )
                    ),
                ]),
                new ToolModelMessage("call-1", "get_weather", "Sunny, 72F.", false, "text"),
            ],
            preserveThinking: false
        );

        JsonAssert.Equal(
            result,
            """[{"role":"user","content":[{"type":"text","text":"Weather in San Francisco?"}]},{"role":"assistant","content":null,"reasoning_content":"Need to check the weather.","tool_calls":[{"id":"call-1","type":"function","function":{"name":"get_weather","arguments":"{\"location\":\"San Francisco\"}"}}]},{"role":"tool","tool_call_id":"call-1","content":"Sunny, 72F."}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should retain a reasoning-only assistant message after the last user message by default",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_retain_a_reasoning_only_assistant_message_after_the_last_user_message_by_default()
    {
        var result = AlibabaChatMessages.Convert(
            [
                new UserModelMessage([new TextContentPart("Think before answering.")]),
                new AssistantModelMessage([new AssistantReasoningPart("Hidden reasoning.")]),
            ],
            preserveThinking: false
        );

        JsonAssert.Equal(
            result,
            """[{"role":"user","content":[{"type":"text","text":"Think before answering."}]},{"role":"assistant","content":null,"reasoning_content":"Hidden reasoning."}]"""
        );
    }
}
