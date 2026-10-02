// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.DeepSeek;

namespace Vercel.AI.Tests;

public sealed class DeepSeekModelTests
{
    [Fact]
    [UpstreamTest("packages/deepseek/src/chat/is-deepseek-v4-model.test.ts::isDeepSeekV4Model::should treat %s as V4", Coverage = UpstreamCoverage.Covered)]
    public void Treats_v4_ids_as_v4()
    {
        foreach (var modelId in new[]
        {
            "deepseek-v4-pro",
            "deepseek-v4-pro-0813",
            "deepseek-v4-flash",
            "deepseek-v4-flash-0731",
            "deepseek-v4-flash-vision-exp",
            "deepseek-flash",
            "deepseek-pro",
        })
        {
            Assert.True(DeepSeekModels.IsV4(modelId));
        }
    }

    [Fact]
    [UpstreamTest("packages/deepseek/src/chat/is-deepseek-v4-model.test.ts::isDeepSeekV4Model::should not treat legacy %s as V4", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_legacy_ids_as_not_v4()
    {
        Assert.False(DeepSeekModels.IsV4("deepseek-chat"));
        Assert.False(DeepSeekModels.IsV4("deepseek-reasoner"));
    }
}
