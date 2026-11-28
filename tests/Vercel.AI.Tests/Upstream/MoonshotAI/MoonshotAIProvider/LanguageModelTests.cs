// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Moonshot;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIProvider;

/// <summary>Port of <c>moonshotai-provider.test.ts</c> &gt; <c>MoonshotAIProvider</c> &gt; <c>languageModel</c>.</summary>
public sealed class LanguageModelTests
{
    [Fact]
    [UpstreamTest(
        "packages/moonshotai/src/moonshotai-provider.test.ts::MoonshotAIProvider > languageModel::should construct a language model with correct configuration",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_construct_a_language_model_with_correct_configuration()
    {
        var provider = MoonshotProvider.Create();

        var model = provider.LanguageModel("moonshot-v1-32k");

        var chat = Assert.IsType<OpenAICompatibleLanguageModel>(model);
        Assert.Equal("moonshot-v1-32k", chat.ModelId);
        Assert.Equal("moonshotai.chat", chat.Provider);
    }
}
