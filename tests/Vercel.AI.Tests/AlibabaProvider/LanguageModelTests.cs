// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAICompatible;
using AlibabaProviderType = Vercel.AI.Alibaba.AlibabaProvider;

namespace Vercel.AI.Tests.AlibabaProvider;

/// <summary>Port of <c>alibaba-provider.test.ts</c> &gt; <c>AlibabaProvider</c> &gt; <c>languageModel</c>.</summary>
public sealed class LanguageModelTests
{
    [Fact]
    [UpstreamTest(
        "packages/alibaba/src/alibaba-provider.test.ts::AlibabaProvider > languageModel::should construct a language model with correct configuration",
        Coverage = UpstreamCoverage.Covered,
        Note = "The port's Alibaba chat model is an OpenAICompatibleLanguageModel with the alibaba.chat provider id."
    )]
    public void Should_construct_a_language_model_with_correct_configuration()
    {
        var provider = AlibabaProviderType.Create();
        var modelId = "qwen3-max";

        var model = provider.LanguageModel(modelId);

        var chat = Assert.IsType<OpenAICompatibleLanguageModel>(model);
        Assert.Equal("alibaba.chat", chat.Provider);
        Assert.Equal(modelId, chat.ModelId);
    }
}
