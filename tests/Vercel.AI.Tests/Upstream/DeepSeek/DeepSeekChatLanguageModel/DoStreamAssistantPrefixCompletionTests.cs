// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.DeepSeek;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel.DeepSeekChatLanguageModelStreamSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Port of <c>deepseek-chat-language-model.test.ts</c> &gt; <c>DeepSeekChatLanguageModel &gt; doStream &gt; assistant prefix completion</c>.</summary>
public sealed class DoStreamAssistantPrefixCompletionTests
{
    private const string Prefix = TestFile + " > doStream > assistant prefix completion::";

    [Fact]
    [UpstreamTest(
        Prefix + "should send prefix true on the final assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_prefix_true_on_the_final_assistant_message()
    {
        var handler = ChunksFixture("deepseek-text");

        await BetaProvider(handler)
            .ChatModel("deepseek-chat")
            .DoStreamAsync(
                new LanguageModelCallOptions
                {
                    Prompt = new ModelMessage[]
                    {
                        new UserModelMessage(
                            new UserContentPart[] { new TextContentPart("Complete this sentence.") }
                        ),
                        new AssistantModelMessage(
                            new AssistantContentPart[] { new AssistantTextPart("The answer is") }
                        )
                        {
                            ProviderOptions = new DeepSeekAssistantMessageProviderOptions
                            {
                                Prefix = true,
                            }.ToProviderOptions(),
                        },
                    },
                },
                CancellationToken.None
            );

        Assert.Equal("https://api.deepseek.com/beta/chat/completions", handler.Uri);
        var body = RequestBody(handler);
        AssertJson(
            body["messages"],
            """
            [
              { "role": "user", "content": "Complete this sentence." },
              { "role": "assistant", "content": "The answer is", "prefix": true }
            ]
            """
        );
        Assert.Equal("deepseek-chat", (string)body["model"]!);
        Assert.True((bool)body["stream"]!);
    }
}
