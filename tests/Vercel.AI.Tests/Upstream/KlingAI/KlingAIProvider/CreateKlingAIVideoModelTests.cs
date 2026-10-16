// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.KlingAI;
using static Vercel.AI.Tests.Upstream.KlingAI.KlingAIProvider.KlingAIProviderSupport;
using KlingAIProviderType = Vercel.AI.KlingAI.KlingAIProvider;

namespace Vercel.AI.Tests.Upstream.KlingAI.KlingAIProvider;

/// <summary>Port of <c>klingai-provider.test.ts</c> &gt; <c>createKlingAI &gt; videoModel</c>.</summary>
public sealed class CreateKlingAIVideoModelTests
{
    private const string Prefix = "packages/klingai/src/klingai-provider.test.ts::createKlingAI > videoModel::";

    [Fact]
    [UpstreamTest(Prefix + "should be an alias for video", Coverage = UpstreamCoverage.Covered)]
    public void Should_be_an_alias_for_video()
    {
        var provider = KlingAIProviderType.Create(new KlingAIOptions { ApiKey = "test-api-key" });

        var model = provider.VideoModel(ModelId);

        Assert.IsType<Vercel.AI.KlingAI.KlingAIVideoModel>(model);
        Assert.Equal(ModelId, model.ModelId);
        Assert.Equal("klingai.video", model.Provider);
    }
}
