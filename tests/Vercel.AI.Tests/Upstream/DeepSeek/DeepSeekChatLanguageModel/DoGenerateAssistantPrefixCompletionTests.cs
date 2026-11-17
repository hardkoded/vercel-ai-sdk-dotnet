// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel.DeepSeekChatLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Port of <c>deepseek-chat-language-model.test.ts</c> &gt; <c>DeepSeekChatLanguageModel &gt; doGenerate &gt; assistant prefix completion</c>.</summary>
public sealed class DoGenerateAssistantPrefixCompletionTests
{
    private const string Prefix = TestFile + " > doGenerate > assistant prefix completion::";

    [Fact]
    [UpstreamTest(
        Prefix + "should send name and prefix on the final assistant message",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_name_and_prefix_on_the_final_assistant_message()
    {
        var capture = JsonFixture("deepseek-text");
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("Complete this sentence."),
                new AssistantModelMessage("The answer is", null, null)
                {
                    ProviderOptions = DeepSeekOptions("""{"name":"assistant","prefix":true}"""),
                },
            },
        };

        await BetaModel(capture, "deepseek-chat").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(
            "https://api.deepseek.com/beta/chat/completions",
            capture.Requests[0].Uri!.ToString()
        );
        AssertMatchObject(
            RequestBody(capture),
            """
            {
              "messages": [
                { "role": "user", "content": "Complete this sentence." },
                { "role": "assistant", "content": "The answer is", "name": "assistant", "prefix": true }
              ],
              "model": "deepseek-chat"
            }
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject prefix completion with the default base URL",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_reject_prefix_completion_with_the_default_base_URL()
    {
        var capture = JsonFixture("deepseek-text");
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new AssistantModelMessage("The answer is", null, null)
                {
                    ProviderOptions = DeepSeekOptions("""{"prefix":true}"""),
                },
            },
        };

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            Model(capture, "deepseek-chat").DoGenerateAsync(options, CancellationToken.None)
        );

        Assert.Contains(
            "DeepSeek assistant prefix completion requires a beta base URL ending in `/beta`.",
            error.Message
        );
        Assert.Empty(capture.Requests);
    }
}
