// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel.TencentVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel;

/// <summary>Port of <c>tencent-video-model.test.ts</c> &gt; <c>TencentVideoModel &gt; constructor</c>.</summary>
public sealed class ConstructorTests
{
    private const string Prefix = UpstreamFile + "::TencentVideoModel > constructor::";

    [Fact]
    [UpstreamTest(Prefix + "exposes provider and model information", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Exposes_provider_and_model_information()
    {
        var model = CreateModel(new Server(), "kling-video-v3");

        Assert.Equal("tencent.video", model.Provider);
        Assert.Equal("kling-video-v3", model.ModelId);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(1, model.MaxVideosPerCall);
    }

    [Theory]
    [InlineData("unknown-model")]
    [InlineData("vod/VS")]
    [InlineData("vod//2.0")]
    [InlineData("vod/VS/")]
    [UpstreamTest(Prefix + "rejects the unsupported model ID %s", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Rejects_the_unsupported_model_ID(string modelId)
    {
        Assert.Throws<NoSuchModelError>(() => CreateModel(new Server(), modelId));
    }
}
