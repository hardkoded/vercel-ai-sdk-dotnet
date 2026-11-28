// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Moonshot;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIProvider;

/// <summary>Port of <c>moonshotai-provider.test.ts</c> &gt; <c>MoonshotAIProvider</c> &gt; <c>chatModel</c>.</summary>
public sealed class ChatModelTests
{
    [Fact]
    [UpstreamTest(
        "packages/moonshotai/src/moonshotai-provider.test.ts::MoonshotAIProvider > chatModel::should construct a chat model with correct configuration",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_construct_a_chat_model_with_correct_configuration()
    {
        var provider = MoonshotProvider.Create();

        var model = provider.CreateChatModel("moonshot-v1-8k");

        Assert.IsType<OpenAICompatibleLanguageModel>(model);
        Assert.Equal("moonshot-v1-8k", model.ModelId);
        Assert.Equal("moonshotai.chat", model.Provider);
    }
}
