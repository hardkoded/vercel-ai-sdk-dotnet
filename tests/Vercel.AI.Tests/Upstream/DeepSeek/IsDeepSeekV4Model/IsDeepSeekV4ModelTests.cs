// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using IsDeepSeekV4ModelType = Vercel.AI.DeepSeek.IsDeepSeekV4Model;

namespace Vercel.AI.Tests.Upstream.DeepSeek.IsDeepSeekV4Model;

/// <summary>Port of <c>is-deepseek-v4-model.test.ts</c> &gt; <c>isDeepSeekV4Model</c>.</summary>
public sealed class IsDeepSeekV4ModelTests
{
    private const string Prefix =
        "packages/deepseek/src/chat/is-deepseek-v4-model.test.ts::isDeepSeekV4Model::";

    [Theory]
    [InlineData("deepseek-v4-pro")]
    [InlineData("deepseek-v4-pro-0813")]
    [InlineData("deepseek-v4-flash")]
    [InlineData("deepseek-v4-flash-0731")]
    [InlineData("deepseek-v4-flash-vision-exp")]
    [InlineData("deepseek-flash")]
    [InlineData("deepseek-pro")]
    [InlineData("deepseek-v5-pro")]
    [InlineData("deepseek-v10-flash")]
    [InlineData("deepseek-next")]
    [InlineData("provider/deepseek-v5-pro")]
    [UpstreamTest(Prefix + "should treat %s as V4", Coverage = UpstreamCoverage.Covered)]
    public void Should_treat_modelId_as_V4(string modelId)
    {
        Assert.True(IsDeepSeekV4ModelType.Check(modelId));
    }

    [Theory]
    [InlineData("deepseek-chat")]
    [InlineData("deepseek-reasoner")]
    [InlineData("deepseek-v3.2")]
    [InlineData("provider/deepseek-v2.5")]
    [UpstreamTest(Prefix + "should not treat legacy %s as V4", Coverage = UpstreamCoverage.Covered)]
    public void Should_not_treat_legacy_modelId_as_V4(string modelId)
    {
        Assert.False(IsDeepSeekV4ModelType.Check(modelId));
    }
}
