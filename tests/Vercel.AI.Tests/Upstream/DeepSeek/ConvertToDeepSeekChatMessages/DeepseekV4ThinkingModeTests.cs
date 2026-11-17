// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekChatMessages.ConvertToDeepSeekChatMessagesSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekChatMessages;

/// <summary>Port of <c>convert-to-deepseek-chat-messages.test.ts</c> &gt; <c>convertToDeepSeekChatMessages &gt; deepseek-v4 thinking mode</c>.</summary>
public sealed class DeepseekV4ThinkingModeTests
{
    private const string Prefix = TestFile + " > deepseek-v4 thinking mode::";

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve reasoning_content from prior turns for deepseek-v4",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_reasoning_content_from_prior_turns_for_deepseek_v4()
    {
        var result = Run(
            "deepseek-v4-pro",
            [
                User(new TextContentPart("Hello")),
                Assistant(
                    new AssistantReasoningPart("I think the tool will return the correct value."),
                    Call("quux", "thwomp", """{"foo":"bar123"}""")
                ),
                ToolJson("quux", "thwomp", """{"oof":"321rab"}"""),
                User(new TextContentPart("Goodbye")),
            ]
        );

        JsonAssert.Equal(
            result.Messages,
            """
            [{"content":"Hello","role":"user"},
             {"content":"","reasoning_content":"I think the tool will return the correct value.","role":"assistant",
              "tool_calls":[{"function":{"arguments":"{\"foo\":\"bar123\"}","name":"thwomp"},"id":"quux","type":"function"}]},
             {"content":"{\"oof\":\"321rab\"}","role":"tool","tool_call_id":"quux"},
             {"content":"Goodbye","role":"user"}]
            """
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve reasoning_content from prior turns for the deepseek-flash alias",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_reasoning_content_from_prior_turns_for_the_deepseek_flash_alias()
    {
        var result = Run(
            "deepseek-flash",
            [
                User(new TextContentPart("Hello")),
                Assistant(
                    new AssistantReasoningPart("Prior-turn reasoning."),
                    new AssistantTextPart("Hi there")
                ),
                User(new TextContentPart("Again")),
            ]
        );

        JsonAssert.Equal(
            result.Messages[1],
            """{"role":"assistant","content":"Hi there","reasoning_content":"Prior-turn reasoning."}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should back-fill empty reasoning_content for deepseek-v4 assistant messages with no reasoning part",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_back_fill_empty_reasoning_content_for_deepseek_v4_assistant_messages_with_no_reasoning_part()
    {
        var result = Run(
            "deepseek-v4-pro",
            [
                User(new TextContentPart("Hello")),
                Assistant(new AssistantTextPart("Hi there")),
                User(new TextContentPart("Again")),
            ]
        );

        JsonAssert.Equal(
            result.Messages,
            """
            [{"content":"Hello","role":"user"},
             {"content":"Hi there","reasoning_content":"","role":"assistant"},
             {"content":"Again","role":"user"}]
            """
        );
        Assert.Empty(result.Warnings);
    }
}
