// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekChatMessages.ConvertToDeepSeekChatMessagesSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekChatMessages;

/// <summary>Port of <c>convert-to-deepseek-chat-messages.test.ts</c> &gt; <c>convertToDeepSeekChatMessages &gt; message names</c>.</summary>
public sealed class MessageNamesTests
{
    private const string Prefix = TestFile + " > message names::";

    [Fact]
    [UpstreamTest(
        Prefix + "should serialize names for system, user, and assistant messages",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public void Should_serialize_names_for_system_user_and_assistant_messages()
    {
        var result = Run(
            "deepseek-v4-flash-vision-exp",
            [
                new SystemModelMessage(
                    "You are a helpful assistant.",
                    Opts(("deepseek", """{"name":"guide"}"""))
                ),
                new UserModelMessage([
                    new TextContentPart("Describe this image."),
                    ImageUrl("image/png", "https://example.com/image.png"),
                ])
                {
                    ProviderOptions = Opts(("deepseek", """{"name":"alice"}""")),
                },
                new AssistantModelMessage([
                    new AssistantTextPart("I will inspect it."),
                    Call("call-1", "inspectImage", """{"detail":"high"}"""),
                ])
                {
                    ProviderOptions = Opts(("deepseek", """{"name":"vision_assistant"}""")),
                },
            ]
        );

        JsonAssert.Equal(
            result.Messages,
            """
            [
              {"content":"You are a helpful assistant.","name":"guide","role":"system"},
              {"content":[
                 {"text":"Describe this image.","type":"text"},
                 {"image_url":{"url":"https://example.com/image.png"},"type":"image_url"}],
               "name":"alice","role":"user"},
              {"content":"I will inspect it.","name":"vision_assistant","reasoning_content":"","role":"assistant",
               "tool_calls":[{"function":{"arguments":"{\"detail\":\"high\"}","name":"inspectImage"},"id":"call-1","type":"function"}]}
            ]
            """
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should ignore a name on a tool message with an unsupported warning",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public void Should_ignore_a_name_on_a_tool_message_with_an_unsupported_warning()
    {
        var result = Run(
            "deepseek-chat",
            [
                new ToolModelMessage("call-1", "weather", "sunny", false, "text")
                {
                    MessageProviderOptions = Opts(("deepseek", """{"name":"weather_tool"}""")),
                },
            ]
        );

        JsonAssert.Equal(
            result.Messages,
            """[{"role":"tool","tool_call_id":"call-1","content":"sunny"}]"""
        );
        Assert.Equal(
            """[{"type":"unsupported","feature":"message name on tool messages"}]""",
            WarningsJson(result)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject a non-string name",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public void Should_reject_a_non_string_name()
    {
        var error = Assert.Throws<InvalidArgumentError>(() =>
            Run(
                "deepseek-chat",
                [
                    new UserModelMessage([new TextContentPart("Hello")])
                    {
                        ProviderOptions = Opts(("deepseek", """{"name":123}""")),
                    },
                ]
            )
        );

        Assert.Contains("invalid deepseek provider options", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should serialize a name from a custom provider options namespace",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public void Should_serialize_a_name_from_a_custom_provider_options_namespace()
    {
        var result = Run(
            "deepseek-chat",
            [
                new UserModelMessage([new TextContentPart("Hello")])
                {
                    ProviderOptions = Opts(("azure", """{"name":"alice"}""")),
                },
            ],
            "azure"
        );

        JsonAssert.Equal(result.Messages, """[{"role":"user","content":"Hello","name":"alice"}]""");
    }
}
