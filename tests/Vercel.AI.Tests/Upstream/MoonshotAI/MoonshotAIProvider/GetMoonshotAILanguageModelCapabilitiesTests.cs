// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Moonshot;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIProvider;

/// <summary>Port of <c>moonshotai-provider.test.ts</c> &gt; <c>getMoonshotAILanguageModelCapabilities</c>.</summary>
public sealed class GetMoonshotAILanguageModelCapabilitiesTests
{
    [Theory]
    [InlineData("kimi-k2.5", true)]
    [InlineData("kimi-k2.6", true)]
    [InlineData("kimi-k2.7-code", true)]
    [InlineData("kimi-k3", true)]
    [InlineData("moonshot-v1-8k", true)]
    [InlineData("moonshot-v1-32k", true)]
    [InlineData("moonshot-v1-128k", true)]
    [InlineData("moonshot-v1-auto", true)]
    [InlineData("moonshot-v1-8k-vision-preview", true)]
    [InlineData("moonshot-v1-32k-vision-preview", true)]
    [InlineData("moonshot-v1-128k-vision-preview", true)]
    [InlineData("moonshot-v1-custom", true)]
    [InlineData("custom-model-id", true)]
    [UpstreamTest(
        "packages/moonshotai/src/moonshotai-provider.test.ts::getMoonshotAILanguageModelCapabilities::supportsStructuredOutputs for %s is %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void SupportsStructuredOutputs_for_model_is(string modelId, bool expected)
    {
        Assert.Equal(expected, MoonshotProvider.SupportsStructuredOutputs(modelId));
        Assert.Equal(
            expected,
            MoonshotProvider.Create().CreateChatModel(modelId).SupportsStructuredOutputs
        );
    }
}
