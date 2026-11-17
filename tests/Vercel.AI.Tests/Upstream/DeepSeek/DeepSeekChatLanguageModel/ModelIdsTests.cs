// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel.DeepSeekChatLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Port of <c>deepseek-chat-language-model.test.ts</c> &gt; <c>DeepSeekChatLanguageModel &gt; model IDs</c>.</summary>
public sealed class ModelIdsTests
{
    [Theory]
    [InlineData("deepseek-v4-flash")]
    [InlineData("deepseek-v4-pro")]
    [UpstreamTest(
        TestFile + " > model IDs::should forward the %s model ID",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_forward_the_modelId_model_ID(string modelId)
    {
        var capture = new UpstreamCapture
        {
            ResponseBody =
                "{\"id\":\"test-id\",\"choices\":[{\"finish_reason\":\"stop\",\"index\":0,\"message\":{\"content\":\"Hello\",\"role\":\"assistant\"}}],\"created\":0,\"model\":\""
                + modelId
                + "\",\"object\":\"chat.completion\",\"usage\":{\"completion_tokens\":1,\"prompt_tokens\":1,\"total_tokens\":2}}",
        };

        await CreateProvider(capture)
            .ChatModel(modelId)
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal(modelId, RequestBody(capture)["model"]!.GetValue<string>());
    }
}
