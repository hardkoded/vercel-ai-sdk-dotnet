// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel.DeepSeekChatLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Port of <c>deepseek-chat-language-model.test.ts</c> &gt; <c>DeepSeekChatLanguageModel &gt; doGenerate &gt; reasoning</c>.</summary>
public sealed class DoGenerateReasoningTests
{
    private const string Prefix = TestFile + " > doGenerate > reasoning::";

    [Fact]
    [UpstreamTest(
        Prefix + "should send correct request body",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_correct_request_body()
    {
        var capture = JsonFixture("deepseek-reasoning");
        var options = UpstreamChat.Prompt("How many \"r\"s are in the word \"strawberry\"?");
        options.ProviderOptions = DeepSeekOptions("""{"thinking":{"type":"enabled"}}""");

        await Model(capture, "deepseek-reasoner").DoGenerateAsync(options, CancellationToken.None);

        AssertJson(
            RequestBody(capture),
            """
            {
              "messages": [
                { "content": "How many \"r\"s are in the word \"strawberry\"?", "role": "user" }
              ],
              "model": "deepseek-reasoner",
              "thinking": { "type": "enabled" }
            }
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract text content",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_extract_text_content()
    {
        var capture = JsonFixture("deepseek-reasoning");

        var result = await Model(capture, "deepseek-chat")
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        MatchSnapshot("deepseek-reasoning.generate.snap.json", result, capture);
    }
}
