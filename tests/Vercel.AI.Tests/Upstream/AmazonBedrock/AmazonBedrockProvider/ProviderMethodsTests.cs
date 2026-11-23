// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using BedrockProvider = Vercel.AI.AmazonBedrock.AmazonBedrockProvider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockProvider;

[Collection("BedrockEndpointEnvironment")]
public sealed class ProviderMethodsTests : BedrockEndpointEnvironment
{
    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > provider methods::should expose anthropicTools",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_expose_anthropicTools()
    {
        var provider = BedrockProvider.Create();
        Assert.Same(AnthropicTools.Factories, provider.Tools);
    }
}
