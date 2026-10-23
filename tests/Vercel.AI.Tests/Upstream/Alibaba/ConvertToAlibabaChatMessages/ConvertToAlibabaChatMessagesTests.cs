// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Alibaba;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests.AlibabaChatTestSupport;

namespace Vercel.AI.Tests.Upstream.Alibaba.ConvertToAlibabaChatMessages;

/// <summary>Port of the cache control cases of <c>convert-to-alibaba-chat-messages.test.ts</c> &gt; <c>convertToAlibabaChatMessages</c>.</summary>
public sealed class ConvertToAlibabaChatMessagesTests
{
    private const string Prefix = "packages/alibaba/src/convert-to-alibaba-chat-messages.test.ts::convertToAlibabaChatMessages::";

    private const string Ephemeral = """{"cacheControl":{"type":"ephemeral"}}""";

    private const string ShouldNotUse = """{"cacheControl":{"type":"should-not-use"}}""";

    private const string Weather = """{"temperature":72,"condition":"sunny"}""";

    private static JsonArray Convert(params ModelMessage[] prompt)
    {
        return AlibabaChatMessages.Convert(prompt, preserveThinking: false, new AlibabaCacheControlValidator());
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
    [UpstreamTest(Prefix + "should inject cache control into system message content block", Coverage = UpstreamCoverage.Covered)]
    public void Should_inject_cache_control_into_system_message_content_block()
    {
        var result = Convert(new SystemModelMessage("You are a helpful assistant.", AlibabaOptions(Ephemeral)));

        JsonAssert.Equal(
            result,
            """[{"content":[{"cache_control":{"type":"ephemeral"},"text":"You are a helpful assistant.","type":"text"}],"role":"system"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should inject cache control into single text user message", Coverage = UpstreamCoverage.Covered)]
    public void Should_inject_cache_control_into_single_text_user_message()
    {
        var result = Convert(new UserModelMessage(new UserContentPart[] { new TextContentPart("Hello") }) { ProviderOptions = AlibabaOptions(Ephemeral) });

        JsonAssert.Equal(
            result,
            """[{"content":[{"cache_control":{"type":"ephemeral"},"text":"Hello","type":"text"}],"role":"user"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should use part-level cache control for single-part user message", Coverage = UpstreamCoverage.Covered)]
    public void Should_use_part_level_cache_control_for_single_part_user_message()
    {
        var result = Convert(new UserModelMessage(new UserContentPart[]
        {
            new TextContentPart("Hello") { ProviderOptions = AlibabaOptions(Ephemeral) },
        }));

        JsonAssert.Equal(
            result,
            """[{"content":[{"cache_control":{"type":"ephemeral"},"text":"Hello","type":"text"}],"role":"user"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should prefer part-level over message-level cache control for single-part user message", Coverage = UpstreamCoverage.Covered)]
    public void Should_prefer_part_level_over_message_level_cache_control_for_single_part_user_message()
    {
        var result = Convert(new UserModelMessage(new UserContentPart[]
        {
            new TextContentPart("Hello") { ProviderOptions = AlibabaOptions(Ephemeral) },
        })
        {
            ProviderOptions = AlibabaOptions(ShouldNotUse),
        });

        JsonAssert.Equal(
            result,
            """[{"content":[{"cache_control":{"type":"ephemeral"},"text":"Hello","type":"text"}],"role":"user"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should use part-level cache control for multi-part user message", Coverage = UpstreamCoverage.Covered)]
    public void Should_use_part_level_cache_control_for_multi_part_user_message()
    {
        var image = new FileContentPart("image/png", null, new byte[] { 0, 1, 2, 3 }, null) { ProviderOptions = AlibabaOptions(Ephemeral) };

        var result = Convert(new UserModelMessage(new UserContentPart[] { new TextContentPart("What is in this image?"), image }));

        JsonAssert.Equal(
            result,
            """[{"content":[{"text":"What is in this image?","type":"text"},{"cache_control":{"type":"ephemeral"},"image_url":{"url":"data:image/png;base64,AAECAw=="},"type":"image_url"}],"role":"user"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should apply message-level cache control to last part of multi-part user message", Coverage = UpstreamCoverage.Covered)]
    public void Should_apply_message_level_cache_control_to_last_part_of_multi_part_user_message()
    {
        var result = Convert(new UserModelMessage(new UserContentPart[] { new TextContentPart("What is in this image?"), Image() })
        {
            ProviderOptions = AlibabaOptions(Ephemeral),
        });

        JsonAssert.Equal(
            result,
            """[{"content":[{"text":"What is in this image?","type":"text"},{"cache_control":{"type":"ephemeral"},"image_url":{"url":"data:image/png;base64,AAECAw=="},"type":"image_url"}],"role":"user"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should inject cache control into assistant message", Coverage = UpstreamCoverage.Covered)]
    public void Should_inject_cache_control_into_assistant_message()
    {
        var result = Convert(new AssistantModelMessage(new AssistantContentPart[] { new AssistantTextPart("Hello, how can I help?") })
        {
            ProviderOptions = AlibabaOptions(Ephemeral),
        });

        JsonAssert.Equal(
            result,
            """[{"content":[{"cache_control":{"type":"ephemeral"},"text":"Hello, how can I help?","type":"text"}],"role":"assistant"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should inject cache control into single-part tool message", Coverage = UpstreamCoverage.Covered)]
    public void Should_inject_cache_control_into_single_part_tool_message()
    {
        var tool = WeatherResult();
        var result = Convert(new ToolModelMessage(new[] { tool }) { MessageProviderOptions = AlibabaOptions(Ephemeral) });

        JsonAssert.Equal(
            result,
            """[{"content":[{"cache_control":{"type":"ephemeral"},"text":"{\"temperature\":72,\"condition\":\"sunny\"}","type":"text"}],"role":"tool","tool_call_id":"call-1"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should use part-level cache control for single-part tool message", Coverage = UpstreamCoverage.Covered)]
    public void Should_use_part_level_cache_control_for_single_part_tool_message()
    {
        var tool = new ToolModelMessage("call-1", "get_weather", Weather, false) { ProviderOptions = AlibabaOptions(Ephemeral) };

        var result = Convert(tool);

        JsonAssert.Equal(
            result,
            """[{"content":[{"cache_control":{"type":"ephemeral"},"text":"{\"temperature\":72,\"condition\":\"sunny\"}","type":"text"}],"role":"tool","tool_call_id":"call-1"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should prefer part-level over message-level cache control for single-part tool message", Coverage = UpstreamCoverage.Covered)]
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
            """[{"content":[{"cache_control":{"type":"ephemeral"},"text":"{\"temperature\":72,\"condition\":\"sunny\"}","type":"text"}],"role":"tool","tool_call_id":"call-1"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should apply message-level cache control to last part of multi-part tool message", Coverage = UpstreamCoverage.Covered)]
    public void Should_apply_message_level_cache_control_to_last_part_of_multi_part_tool_message()
    {
        var tool = new ToolModelMessage(new[] { WeatherResult(), new ToolModelMessage("call-2", "get_time", "2:30 PM", false) })
        {
            MessageProviderOptions = AlibabaOptions(Ephemeral),
        };

        var result = Convert(tool);

        JsonAssert.Equal(
            result,
            """[{"content":"{\"temperature\":72,\"condition\":\"sunny\"}","role":"tool","tool_call_id":"call-1"},{"content":[{"cache_control":{"type":"ephemeral"},"text":"2:30 PM","type":"text"}],"role":"tool","tool_call_id":"call-2"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should use part-level cache control for multi-part tool message", Coverage = UpstreamCoverage.Covered)]
    public void Should_use_part_level_cache_control_for_multi_part_tool_message()
    {
        var tool = new ToolModelMessage(new[]
        {
            WeatherResult(),
            new ToolModelMessage("call-2", "get_time", "2:30 PM", false) { ProviderOptions = AlibabaOptions(Ephemeral) },
        });

        var result = Convert(tool);

        JsonAssert.Equal(
            result,
            """[{"content":"{\"temperature\":72,\"condition\":\"sunny\"}","role":"tool","tool_call_id":"call-1"},{"content":[{"cache_control":{"type":"ephemeral"},"text":"2:30 PM","type":"text"}],"role":"tool","tool_call_id":"call-2"}]""");
    }
}
