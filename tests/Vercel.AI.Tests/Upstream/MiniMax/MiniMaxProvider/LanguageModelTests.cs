// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.MiniMax;

namespace Vercel.AI.Tests.MiniMaxProvider;

/// <summary>Port of <c>minimax-provider.test.ts</c> &gt; <c>MiniMaxProvider &gt; languageModel</c>.</summary>
public sealed class LanguageModelTests
{
    [Fact]
    [UpstreamTest(
        "packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > languageModel::should construct a language model",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_construct_a_language_model()
    {
        var provider = Vercel.AI.MiniMax.MiniMaxProvider.Create();
        var model = provider.LanguageModel("minimax-m2.5");

        Assert.IsType<Vercel.AI.MiniMax.MiniMaxChatLanguageModel>(model);
        Assert.Equal("minimax-m2.5", model.ModelId);
    }
}
