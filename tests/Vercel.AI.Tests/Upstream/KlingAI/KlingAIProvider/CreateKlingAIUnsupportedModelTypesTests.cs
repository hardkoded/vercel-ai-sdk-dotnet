// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.KlingAI;
using Vercel.AI.Util;
using KlingAIProviderType = Vercel.AI.KlingAI.KlingAIProvider;

namespace Vercel.AI.Tests.Upstream.KlingAI.KlingAIProvider;

/// <summary>Port of <c>klingai-provider.test.ts</c> &gt; <c>createKlingAI &gt; unsupported model types</c>.</summary>
public sealed class CreateKlingAIUnsupportedModelTypesTests
{
    private const string Prefix = "packages/klingai/src/klingai-provider.test.ts::createKlingAI > unsupported model types::";

    [Fact]
    [UpstreamTest(Prefix + "should throw NoSuchModelError for languageModel", Coverage = UpstreamCoverage.Covered)]
    public void Should_throw_NoSuchModelError_for_languageModel()
    {
        var provider = KlingAIProviderType.Create();

        Assert.Throws<NoSuchModelError>(() => provider.LanguageModel("some-model"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw NoSuchModelError for embeddingModel", Coverage = UpstreamCoverage.Covered)]
    public void Should_throw_NoSuchModelError_for_embeddingModel()
    {
        var provider = KlingAIProviderType.Create();

        Assert.Throws<NoSuchModelError>(() => provider.EmbeddingModel("some-model"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw NoSuchModelError for imageModel", Coverage = UpstreamCoverage.Covered)]
    public void Should_throw_NoSuchModelError_for_imageModel()
    {
        var provider = KlingAIProviderType.Create();

        Assert.Throws<NoSuchModelError>(() => provider.ImageModel("some-model"));
    }
}
