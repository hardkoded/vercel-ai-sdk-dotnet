// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekChatMessages.ConvertToDeepSeekChatMessagesSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekChatMessages;

/// <summary>Port of <c>convert-to-deepseek-chat-messages.test.ts</c> &gt; <c>convertToDeepSeekChatMessages &gt; assistant prefix completion</c>.</summary>
public sealed class AssistantPrefixCompletionTests
{
    private const string Prefix = TestFile + " > assistant prefix completion::";

    private static Dictionary<string, System.Text.Json.JsonElement> PrefixOptions()
    {
        return Opts(("deepseek", """{"prefix":true}"""));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should serialize prefix true on the final assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_serialize_prefix_true_on_the_final_assistant_message()
    {
        var result = Run(
            "deepseek-chat",
            [
                User(new TextContentPart("Complete this sentence.")),
                new AssistantModelMessage([new AssistantTextPart("The answer is")])
                {
                    ProviderOptions = PrefixOptions(),
                },
            ],
            supportsAssistantPrefixCompletion: true
        );

        JsonAssert.Equal(
            result.Messages,
            """
            [{"role":"user","content":"Complete this sentence."},
             {"role":"assistant","content":"The answer is","prefix":true}]
            """
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject prefix completion on a non-assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_prefix_completion_on_a_non_assistant_message()
    {
        Assert.Throws<InvalidPromptException>(() =>
            Run(
                "deepseek-chat",
                [
                    new UserModelMessage([new TextContentPart("Complete this sentence.")])
                    {
                        ProviderOptions = PrefixOptions(),
                    },
                ],
                supportsAssistantPrefixCompletion: true
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject prefix completion on a non-final assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_prefix_completion_on_a_non_final_assistant_message()
    {
        Assert.Throws<InvalidPromptException>(() =>
            Run(
                "deepseek-chat",
                [
                    new AssistantModelMessage([new AssistantTextPart("The answer is")])
                    {
                        ProviderOptions = PrefixOptions(),
                    },
                    User(new TextContentPart("Continue.")),
                ],
                supportsAssistantPrefixCompletion: true
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject prefix completion without a beta base URL capability",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_prefix_completion_without_a_beta_base_URL_capability()
    {
        Assert.Throws<UnsupportedFunctionalityException>(() =>
            Run(
                "deepseek-chat",
                [
                    new AssistantModelMessage([new AssistantTextPart("The answer is")])
                    {
                        ProviderOptions = PrefixOptions(),
                    },
                ]
            )
        );
    }
}
