// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekChatMessages.ConvertToDeepSeekChatMessagesSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekChatMessages;

/// <summary>Port of <c>convert-to-deepseek-chat-messages.test.ts</c> &gt; <c>convertToDeepSeekChatMessages &gt; tool calls</c>.</summary>
public sealed class ToolCallsTests
{
    private const string Prefix = TestFile + " > tool calls::";

    [Fact]
    [UpstreamTest(
        Prefix + "should stringify arguments to tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_stringify_arguments_to_tool_calls()
    {
        var result = Run(
            "deepseek-chat",
            [
                Assistant(Call("quux", "thwomp", """{"foo":"bar123"}""")),
                ToolJson("quux", "thwomp", """{"oof":"321rab"}"""),
            ]
        );

        JsonAssert.Equal(
            result.Messages,
            """
            [{"content":"","role":"assistant",
              "tool_calls":[{"function":{"arguments":"{\"foo\":\"bar123\"}","name":"thwomp"},"id":"quux","type":"function"}]},
             {"content":"{\"oof\":\"321rab\"}","role":"tool","tool_call_id":"quux"}]
            """
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle text output type in tool results",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_text_output_type_in_tool_results()
    {
        var result = Run(
            "deepseek-chat",
            [
                Assistant(Call("call-1", "getWeather", """{"query":"weather"}""")),
                ToolText("call-1", "getWeather", "It is sunny today"),
            ]
        );

        JsonAssert.Equal(
            result.Messages,
            """
            [{"content":"","role":"assistant",
              "tool_calls":[{"function":{"arguments":"{\"query\":\"weather\"}","name":"getWeather"},"id":"call-1","type":"function"}]},
             {"content":"It is sunny today","role":"tool","tool_call_id":"call-1"}]
            """
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should convert inline, URL, and referenced images in tool results to content parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_inline_URL_and_referenced_images_in_tool_results_to_content_parts()
    {
        var reference = new Dictionary<string, string>
        {
            ["deepseek"] = "file-api-deepseek",
            ["openai"] = "file-openai",
        };

        var result = Run(
            "deepseek-flash",
            [
                new ToolModelMessage(
                    "call-1",
                    "inspectImage",
                    new UserContentPart[]
                    {
                        new TextContentPart("Screenshot captured."),
                        ImageData("image/png", Bytes0123),
                        ImageUrl(
                            "image/webp",
                            "https://example.com/image.webp",
                            Opts(("deepseek", """{"imageDetail":"low"}"""))
                        ),
                        FileContentPart.FromProviderReference("image/png", reference),
                    }
                ),
            ]
        );

        JsonAssert.Equal(
            result.Messages,
            """
            [{"role":"tool","tool_call_id":"call-1","content":[
               {"type":"text","text":"Screenshot captured."},
               {"type":"image_url","image_url":{"url":"data:image/png;base64,AAECAw=="}},
               {"type":"image_url","image_url":{"url":"https://example.com/image.webp","detail":"low"}},
               {"type":"file","file_id":"file-api-deepseek"}]}]
            """
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep tool result content without images JSON-stringified",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_keep_tool_result_content_without_images_JSON_stringified()
    {
        var result = Run(
            "deepseek-chat",
            [
                new ToolModelMessage(
                    "call-1",
                    "getWeather",
                    new UserContentPart[] { new TextContentPart("It is sunny today") }
                ),
            ]
        );

        Assert.Equal(
            "[{\"type\":\"text\",\"text\":\"It is sunny today\"}]",
            result.Messages[0]!["content"]!.GetValue<string>()
        );
        JsonAssert.Equal(
            result.Messages,
            """
            [{"role":"tool","tool_call_id":"call-1","content":"[{\"type\":\"text\",\"text\":\"It is sunny today\"}]"}]
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should support reasoning content in tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_support_reasoning_content_in_tool_calls()
    {
        var result = Run(
            "deepseek-chat",
            [
                User(new TextContentPart("Hello")),
                Assistant(
                    new AssistantReasoningPart("I think the tool will return the correct value."),
                    Call("quux", "thwomp", """{"foo":"bar123"}""")
                ),
                ToolJson("quux", "thwomp", """{"oof":"321rab"}"""),
            ]
        );

        JsonAssert.Equal(
            result.Messages,
            """
            [{"content":"Hello","role":"user"},
             {"content":"","reasoning_content":"I think the tool will return the correct value.","role":"assistant",
              "tool_calls":[{"function":{"arguments":"{\"foo\":\"bar123\"}","name":"thwomp"},"id":"quux","type":"function"}]},
             {"content":"{\"oof\":\"321rab\"}","role":"tool","tool_call_id":"quux"}]
            """
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should filter out reasoning content from turns before the last user message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_filter_out_reasoning_content_from_turns_before_the_last_user_message()
    {
        var result = Run(
            "deepseek-chat",
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
             {"content":"","role":"assistant",
              "tool_calls":[{"function":{"arguments":"{\"foo\":\"bar123\"}","name":"thwomp"},"id":"quux","type":"function"}]},
             {"content":"{\"oof\":\"321rab\"}","role":"tool","tool_call_id":"quux"},
             {"content":"Goodbye","role":"user"}]
            """
        );
        Assert.Empty(result.Warnings);
    }
}
