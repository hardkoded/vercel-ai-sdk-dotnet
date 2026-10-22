// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Alibaba;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests.AlibabaChatTestSupport;

namespace Vercel.AI.Tests.Upstream.Alibaba.ConvertToAlibabaChatMessages;

/// <summary>Port of the cache control cases of <c>convert-to-alibaba-chat-messages.test.ts</c> &gt; <c>convertToAlibabaChatMessages &gt; preserveThinking</c>.</summary>
public sealed class PreserveThinkingTests
{
    private const string Prefix = "packages/alibaba/src/convert-to-alibaba-chat-messages.test.ts::convertToAlibabaChatMessages > preserveThinking::";

    private const string Ephemeral = """{"cacheControl":{"type":"ephemeral"}}""";

    [Fact]
    [UpstreamTest(Prefix + "should emit content null for a preserved reasoning-only message with message-level cache control", Coverage = UpstreamCoverage.Covered)]
    public void Should_emit_content_null_for_a_preserved_reasoning_only_message_with_message_level_cache_control()
    {
        var validator = new AlibabaCacheControlValidator();

        var result = AlibabaChatMessages.Convert(
            new ModelMessage[]
            {
                new AssistantModelMessage(new AssistantContentPart[] { new AssistantReasoningPart("Hidden reasoning.") })
                {
                    ProviderOptions = AlibabaOptions(Ephemeral),
                },
                new UserModelMessage("Continue."),
            },
            preserveThinking: true,
            validator);

        JsonAssert.Equal(
            result,
            """[{"role":"assistant","content":null,"reasoning_content":"Hidden reasoning."},{"role":"user","content":[{"type":"text","text":"Continue."}]}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should retain the cached content block for a tool-call-only message with message-level cache control", Coverage = UpstreamCoverage.Covered)]
    public void Should_retain_the_cached_content_block_for_a_tool_call_only_message_with_message_level_cache_control()
    {
        var validator = new AlibabaCacheControlValidator();

        var result = AlibabaChatMessages.Convert(
            new ModelMessage[]
            {
                new AssistantModelMessage(new AssistantContentPart[]
                {
                    new AssistantToolCallPart(new GeneratedToolCall("call-1", "get_weather", """{"location":"San Francisco"}""")),
                })
                {
                    ProviderOptions = AlibabaOptions(Ephemeral),
                },
            },
            preserveThinking: true,
            validator);

        JsonAssert.Equal(
            result,
            """[{"role":"assistant","content":[{"type":"text","text":"","cache_control":{"type":"ephemeral"}}],"tool_calls":[{"id":"call-1","type":"function","function":{"name":"get_weather","arguments":"{\"location\":\"San Francisco\"}"}}]}]""");
    }
}
