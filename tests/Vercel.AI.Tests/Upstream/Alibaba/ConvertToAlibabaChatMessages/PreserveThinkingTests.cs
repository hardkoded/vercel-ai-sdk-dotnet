// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Alibaba;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests.AlibabaChatTestSupport;

namespace Vercel.AI.Tests.Upstream.Alibaba.ConvertToAlibabaChatMessages;

/// <summary>Port of <c>convert-to-alibaba-chat-messages.test.ts</c> &gt; <c>convertToAlibabaChatMessages &gt; preserveThinking</c>.</summary>
public sealed class PreserveThinkingTests
{
    private const string Prefix =
        "packages/alibaba/src/convert-to-alibaba-chat-messages.test.ts::convertToAlibabaChatMessages > preserveThinking::";

    private const string Ephemeral = """{"cacheControl":{"type":"ephemeral"}}""";

    private const string WeatherToolCall =
        """{"id":"call-1","type":"function","function":{"name":"get_weather","arguments":"{\"location\":\"San Francisco\"}"}}""";

    private const string ContinueUser =
        """{"role":"user","content":[{"type":"text","text":"Continue."}]}""";

    private const string ThinkUser =
        """{"role":"user","content":[{"type":"text","text":"Think before answering."}]}""";

    private static AssistantModelMessage ReasoningAndAnswer()
    {
        return new AssistantModelMessage([
            new AssistantReasoningPart("Hidden reasoning."),
            new AssistantTextPart("Visible answer."),
        ]);
    }

    private static ModelMessage[] ReasoningOnlyRound()
    {
        return
        [
            new UserModelMessage([new TextContentPart("Think before answering.")]),
            new AssistantModelMessage([new AssistantReasoningPart("Hidden reasoning.")]),
            new UserModelMessage([new TextContentPart("Continue.")]),
        ];
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit prior-round reasoning when preserveThinking is explicitly false",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_omit_prior_round_reasoning_when_preserveThinking_is_explicitly_false()
    {
        var result = AlibabaChatMessages.Convert(
            [ReasoningAndAnswer(), new UserModelMessage([new TextContentPart("Continue.")])],
            preserveThinking: false
        );

        JsonAssert.Equal(
            result,
            "[" + """{"role":"assistant","content":"Visible answer."}""" + "," + ContinueUser + "]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should emit prior-round reasoning as reasoning_content when preserveThinking is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_emit_prior_round_reasoning_as_reasoning_content_when_preserveThinking_is_true()
    {
        var result = AlibabaChatMessages.Convert(
            [ReasoningAndAnswer(), new UserModelMessage([new TextContentPart("Continue.")])],
            preserveThinking: true
        );

        JsonAssert.Equal(
            result,
            "["
                + """{"role":"assistant","content":"Visible answer.","reasoning_content":"Hidden reasoning."}"""
                + ","
                + ContinueUser
                + "]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should concatenate multiple reasoning parts in order",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_concatenate_multiple_reasoning_parts_in_order()
    {
        var result = AlibabaChatMessages.Convert(
            [
                new AssistantModelMessage([
                    new AssistantReasoningPart("First thought. "),
                    new AssistantReasoningPart("Second thought."),
                    new AssistantTextPart("Visible answer."),
                ]),
                new UserModelMessage([new TextContentPart("Continue.")]),
            ],
            preserveThinking: true
        );

        JsonAssert.Equal(
            result,
            "["
                + """{"role":"assistant","content":"Visible answer.","reasoning_content":"First thought. Second thought."}"""
                + ","
                + ContinueUser
                + "]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should emit reasoning_content alongside tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_emit_reasoning_content_alongside_tool_calls()
    {
        var result = AlibabaChatMessages.Convert(
            [
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
                new UserModelMessage([new TextContentPart("Continue.")]),
            ],
            preserveThinking: true
        );

        JsonAssert.Equal(
            result,
            "["
                + """{"role":"assistant","content":null,"reasoning_content":"Need to check the weather.","tool_calls":["""
                + WeatherToolCall
                + "]},"
                + ContinueUser
                + "]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should retain a reasoning-only assistant message when preserveThinking is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_retain_a_reasoning_only_assistant_message_when_preserveThinking_is_true()
    {
        var result = AlibabaChatMessages.Convert(ReasoningOnlyRound(), preserveThinking: true);

        JsonAssert.Equal(
            result,
            "["
                + ThinkUser
                + ","
                + """{"role":"assistant","content":null,"reasoning_content":"Hidden reasoning."}"""
                + ","
                + ContinueUser
                + "]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit a reasoning-only assistant message when preserveThinking is false",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_omit_a_reasoning_only_assistant_message_when_preserveThinking_is_false()
    {
        var result = AlibabaChatMessages.Convert(ReasoningOnlyRound(), preserveThinking: false);

        JsonAssert.Equal(result, "[" + ThinkUser + "," + ContinueUser + "]");
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should emit content null for a preserved reasoning-only message with message-level cache control",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_emit_content_null_for_a_preserved_reasoning_only_message_with_message_level_cache_control()
    {
        var validator = new AlibabaCacheControlValidator();

        var result = AlibabaChatMessages.Convert(
            new ModelMessage[]
            {
                new AssistantModelMessage(
                    new AssistantContentPart[] { new AssistantReasoningPart("Hidden reasoning.") }
                )
                {
                    ProviderOptions = AlibabaOptions(Ephemeral),
                },
                new UserModelMessage("Continue."),
            },
            preserveThinking: true,
            validator
        );

        JsonAssert.Equal(
            result,
            """[{"role":"assistant","content":null,"reasoning_content":"Hidden reasoning."},{"role":"user","content":[{"type":"text","text":"Continue."}]}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should retain the cached content block for a tool-call-only message with message-level cache control",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_retain_the_cached_content_block_for_a_tool_call_only_message_with_message_level_cache_control()
    {
        var validator = new AlibabaCacheControlValidator();

        var result = AlibabaChatMessages.Convert(
            new ModelMessage[]
            {
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantToolCallPart(
                            new GeneratedToolCall(
                                "call-1",
                                "get_weather",
                                """{"location":"San Francisco"}"""
                            )
                        ),
                    }
                )
                {
                    ProviderOptions = AlibabaOptions(Ephemeral),
                },
            },
            preserveThinking: true,
            validator
        );

        JsonAssert.Equal(
            result,
            """[{"role":"assistant","content":[{"type":"text","text":"","cache_control":{"type":"ephemeral"}}],"tool_calls":[{"id":"call-1","type":"function","function":{"name":"get_weather","arguments":"{\"location\":\"San Francisco\"}"}}]}]"""
        );
    }
}
