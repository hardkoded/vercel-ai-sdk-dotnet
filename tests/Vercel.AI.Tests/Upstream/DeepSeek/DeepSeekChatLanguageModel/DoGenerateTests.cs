// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel.DeepSeekChatLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Port of <c>deepseek-chat-language-model.test.ts</c> &gt; <c>DeepSeekChatLanguageModel &gt; doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Prefix = TestFile + " > doGenerate::";

    [Fact]
    [UpstreamTest(
        Prefix + "should reject a response without choices",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_reject_a_response_without_choices()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody =
                "{\"id\":\"chatcmpl-empty\",\"object\":\"chat.completion\",\"created\":0,\"model\":\"deepseek-chat\",\"choices\":[],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":0,\"total_tokens\":1}}",
        };

        var error = await Assert.ThrowsAsync<InvalidResponseDataException>(() =>
            Model(capture, "deepseek-chat").DoGenerateAsync(TestPrompt(), CancellationToken.None)
        );

        Assert.Equal("Response did not contain any choices.", error.Message);
    }
}
