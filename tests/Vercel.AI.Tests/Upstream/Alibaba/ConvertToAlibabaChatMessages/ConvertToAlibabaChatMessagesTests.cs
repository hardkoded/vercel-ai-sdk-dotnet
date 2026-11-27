// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Alibaba;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests.AlibabaChatTestSupport;

namespace Vercel.AI.Tests.Upstream.Alibaba.ConvertToAlibabaChatMessages;

/// <summary>Port of <c>convert-to-alibaba-chat-messages.test.ts</c> &gt; <c>convertToAlibabaChatMessages</c>.</summary>
public sealed class ConvertToAlibabaChatMessagesTests
{
    private const string Prefix =
        "packages/alibaba/src/convert-to-alibaba-chat-messages.test.ts::convertToAlibabaChatMessages::";

    private const string Ephemeral = """{"cacheControl":{"type":"ephemeral"}}""";

    private const string ShouldNotUse = """{"cacheControl":{"type":"should-not-use"}}""";

    private const string Weather = """{"temperature":72,"condition":"sunny"}""";

    private static JsonArray Convert(params ModelMessage[] prompt)
    {
        return AlibabaChatMessages.Convert(
            prompt,
            preserveThinking: false,
            new AlibabaCacheControlValidator()
        );
    }

    private static ToolModelMessage WeatherResult()
    {
        return new ToolModelMessage("call-1", "get_weather", Weather, false);
    }

    private static FileContentPart Image()
    {
        return new FileContentPart("image/png", null, new byte[] { 0, 1, 2, 3 }, null);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should inject cache control into system message content block",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_inject_cache_control_into_system_message_content_block()
    {
        var result = Convert(
            new SystemModelMessage("You are a helpful assistant.", AlibabaOptions(Ephemeral))
        );

        JsonAssert.Equal(
            result,
            """[{"content":[{"cache_control":{"type":"ephemeral"},"text":"You are a helpful assistant.","type":"text"}],"role":"system"}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should inject cache control into single text user message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_inject_cache_control_into_single_text_user_message()
    {
        var result = Convert(
            new UserModelMessage(new UserContentPart[] { new TextContentPart("Hello") })
            {
                ProviderOptions = AlibabaOptions(Ephemeral),
            }
        );

        JsonAssert.Equal(
            result,
            """[{"content":[{"cache_control":{"type":"ephemeral"},"text":"Hello","type":"text"}],"role":"user"}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use part-level cache control for single-part user message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_part_level_cache_control_for_single_part_user_message()
    {
        var result = Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    new TextContentPart("Hello") { ProviderOptions = AlibabaOptions(Ephemeral) },
                }
            )
        );

        JsonAssert.Equal(
            result,
            """[{"content":[{"cache_control":{"type":"ephemeral"},"text":"Hello","type":"text"}],"role":"user"}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should prefer part-level over message-level cache control for single-part user message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_prefer_part_level_over_message_level_cache_control_for_single_part_user_message()
    {
        var result = Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    new TextContentPart("Hello") { ProviderOptions = AlibabaOptions(Ephemeral) },
                }
            )
            {
                ProviderOptions = AlibabaOptions(ShouldNotUse),
            }
        );

        JsonAssert.Equal(
            result,
            """[{"content":[{"cache_control":{"type":"ephemeral"},"text":"Hello","type":"text"}],"role":"user"}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use part-level cache control for multi-part user message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_part_level_cache_control_for_multi_part_user_message()
    {
        var image = new FileContentPart("image/png", null, new byte[] { 0, 1, 2, 3 }, null)
        {
            ProviderOptions = AlibabaOptions(Ephemeral),
        };

        var result = Convert(
            new UserModelMessage(
                new UserContentPart[] { new TextContentPart("What is in this image?"), image }
            )
        );

        JsonAssert.Equal(
            result,
            """[{"content":[{"text":"What is in this image?","type":"text"},{"cache_control":{"type":"ephemeral"},"image_url":{"url":"data:image/png;base64,AAECAw=="},"type":"image_url"}],"role":"user"}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should apply message-level cache control to last part of multi-part user message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_apply_message_level_cache_control_to_last_part_of_multi_part_user_message()
    {
        var result = Convert(
            new UserModelMessage(
                new UserContentPart[] { new TextContentPart("What is in this image?"), Image() }
            )
            {
                ProviderOptions = AlibabaOptions(Ephemeral),
            }
        );

        JsonAssert.Equal(
            result,
            """[{"content":[{"text":"What is in this image?","type":"text"},{"cache_control":{"type":"ephemeral"},"image_url":{"url":"data:image/png;base64,AAECAw=="},"type":"image_url"}],"role":"user"}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should inject cache control into assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_inject_cache_control_into_assistant_message()
    {
        var result = Convert(
            new AssistantModelMessage(
                new AssistantContentPart[] { new AssistantTextPart("Hello, how can I help?") }
            )
            {
                ProviderOptions = AlibabaOptions(Ephemeral),
            }
        );

        JsonAssert.Equal(
            result,
            """[{"content":[{"cache_control":{"type":"ephemeral"},"text":"Hello, how can I help?","type":"text"}],"role":"assistant"}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should inject cache control into single-part tool message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_inject_cache_control_into_single_part_tool_message()
    {
        var tool = WeatherResult();
        var result = Convert(
            new ToolModelMessage(new[] { tool })
            {
                MessageProviderOptions = AlibabaOptions(Ephemeral),
            }
        );

        JsonAssert.Equal(
            result,
            """[{"content":[{"cache_control":{"type":"ephemeral"},"text":"{\"temperature\":72,\"condition\":\"sunny\"}","type":"text"}],"role":"tool","tool_call_id":"call-1"}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use part-level cache control for single-part tool message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_part_level_cache_control_for_single_part_tool_message()
    {
        var tool = new ToolModelMessage("call-1", "get_weather", Weather, false)
        {
            ProviderOptions = AlibabaOptions(Ephemeral),
        };

        var result = Convert(tool);

        JsonAssert.Equal(
            result,
            """[{"content":[{"cache_control":{"type":"ephemeral"},"text":"{\"temperature\":72,\"condition\":\"sunny\"}","type":"text"}],"role":"tool","tool_call_id":"call-1"}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should prefer part-level over message-level cache control for single-part tool message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_prefer_part_level_over_message_level_cache_control_for_single_part_tool_message()
    {
        var tool = new ToolModelMessage("call-1", "get_weather", Weather, false)
        {
            ProviderOptions = AlibabaOptions(Ephemeral),
            MessageProviderOptions = AlibabaOptions(ShouldNotUse),
        };

        var result = Convert(tool);

        JsonAssert.Equal(
            result,
            """[{"content":[{"cache_control":{"type":"ephemeral"},"text":"{\"temperature\":72,\"condition\":\"sunny\"}","type":"text"}],"role":"tool","tool_call_id":"call-1"}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should apply message-level cache control to last part of multi-part tool message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_apply_message_level_cache_control_to_last_part_of_multi_part_tool_message()
    {
        var tool = new ToolModelMessage(
            new[] { WeatherResult(), new ToolModelMessage("call-2", "get_time", "2:30 PM", false) }
        )
        {
            MessageProviderOptions = AlibabaOptions(Ephemeral),
        };

        var result = Convert(tool);

        JsonAssert.Equal(
            result,
            """[{"content":"{\"temperature\":72,\"condition\":\"sunny\"}","role":"tool","tool_call_id":"call-1"},{"content":[{"cache_control":{"type":"ephemeral"},"text":"2:30 PM","type":"text"}],"role":"tool","tool_call_id":"call-2"}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use part-level cache control for multi-part tool message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_part_level_cache_control_for_multi_part_tool_message()
    {
        var tool = new ToolModelMessage(
            new[]
            {
                WeatherResult(),
                new ToolModelMessage("call-2", "get_time", "2:30 PM", false)
                {
                    ProviderOptions = AlibabaOptions(Ephemeral),
                },
            }
        );

        var result = Convert(tool);

        JsonAssert.Equal(
            result,
            """[{"content":"{\"temperature\":72,\"condition\":\"sunny\"}","role":"tool","tool_call_id":"call-1"},{"content":[{"cache_control":{"type":"ephemeral"},"text":"2:30 PM","type":"text"}],"role":"tool","tool_call_id":"call-2"}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use array format for single text user message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_array_format_for_single_text_user_message()
    {
        var result = AlibabaChatMessages.Convert(
            [new UserModelMessage([new TextContentPart("Hello")])],
            preserveThinking: false
        );

        JsonAssert.Equal(
            result,
            """[{"content":[{"text":"Hello","type":"text"}],"role":"user"}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use array format for multi-part user message with image",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_array_format_for_multi_part_user_message_with_image()
    {
        var result = AlibabaChatMessages.Convert(
            [new UserModelMessage([new TextContentPart("What is in this image?"), Image()])],
            preserveThinking: false
        );

        JsonAssert.Equal(
            result,
            """[{"content":[{"text":"What is in this image?","type":"text"},{"image_url":{"url":"data:image/png;base64,AAECAw=="},"type":"image_url"}],"role":"user"}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert assistant message with tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_assistant_message_with_tool_calls()
    {
        var result = AlibabaChatMessages.Convert(
            [
                new AssistantModelMessage([
                    new AssistantToolCallPart(
                        new GeneratedToolCall(
                            "call-1",
                            "get_weather",
                            """{"location":"San Francisco"}"""
                        )
                    ),
                ]),
            ],
            preserveThinking: false
        );

        JsonAssert.Equal(
            result,
            """[{"content":null,"role":"assistant","tool_calls":[{"function":{"arguments":"{\"location\":\"San Francisco\"}","name":"get_weather"},"id":"call-1","type":"function"}]}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit prior-round reasoning from assistant message content by default",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_omit_prior_round_reasoning_from_assistant_message_content_by_default()
    {
        var result = AlibabaChatMessages.Convert(
            [
                new AssistantModelMessage([
                    new AssistantReasoningPart("Hidden reasoning."),
                    new AssistantTextPart("Visible answer."),
                ]),
                new UserModelMessage([new TextContentPart("Continue.")]),
            ],
            preserveThinking: false
        );

        JsonAssert.Equal(
            result,
            """[{"role":"assistant","content":"Visible answer."},{"role":"user","content":[{"type":"text","text":"Continue."}]}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit an assistant message when reasoning is its only content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_omit_an_assistant_message_when_reasoning_is_its_only_content()
    {
        var result = AlibabaChatMessages.Convert(
            [
                new UserModelMessage([new TextContentPart("Think before answering.")]),
                new AssistantModelMessage([new AssistantReasoningPart("Hidden reasoning.")]),
                new UserModelMessage([new TextContentPart("Continue.")]),
            ],
            preserveThinking: false
        );

        JsonAssert.Equal(
            result,
            """[{"role":"user","content":[{"type":"text","text":"Think before answering."}]},{"role":"user","content":[{"type":"text","text":"Continue."}]}]"""
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert tool results", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_tool_results()
    {
        var result = AlibabaChatMessages.Convert([WeatherResult()], preserveThinking: false);

        JsonAssert.Equal(
            result,
            """[{"content":"{\"temperature\":72,\"condition\":\"sunny\"}","role":"tool","tool_call_id":"call-1"}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw for file parts with provider references",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_for_file_parts_with_provider_references()
    {
        var exception = Assert.ThrowsAny<AiSdkException>(() =>
            AlibabaChatMessages.Convert(
                [
                    new UserModelMessage([
                        FileContentPart.FromProviderReference(
                            "image/png",
                            new Dictionary<string, string> { ["alibaba"] = "file-ref-123" }
                        ),
                    ]),
                ],
                preserveThinking: false
            )
        );

        Assert.Contains(
            "'file parts with provider references' functionality not supported",
            exception.Message,
            StringComparison.Ordinal
        );
    }
}
