// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AmazonBedrock;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockAnthropicModelSupport;

public sealed class AmazonBedrockAnthropicModelSupportTests
{
    [Theory]
    [InlineData("anthropic.claude-3-7-sonnet-20250219-v1:0", true, true)]
    [InlineData("anthropic.claude-sonnet-4-20250514-v1:0", true, true)]
    [InlineData("us.anthropic.claude-opus-4-1-20250805-v1:0", true, true)]
    [InlineData("anthropic.claude-sonnet-4-5-20250929-v1:0", true, true)]
    [InlineData("anthropic.claude-opus-4-6-v1", true, true)]
    [InlineData("anthropic.claude-sonnet-4-6-v1", false, true)]
    [InlineData("anthropic.claude-haiku-4-5-20251001-v1:0", false, true)]
    [InlineData("anthropic.claude-opus-4-7", false, false)]
    [InlineData("anthropic.claude-opus-5", true, true)]
    [InlineData("anthropic.claude-opus-5-5", true, true)]
    [InlineData("anthropic.claude-sonnet-5", true, true)]
    [InlineData("anthropic.claude-sonnet-5-5", true, true)]
    [InlineData("global.anthropic.claude-sonnet-5-5", true, true)]
    [InlineData("global.anthropic.claude-haiku-5-5", true, true)]
    [InlineData("anthropic.claude-haiku-5", false, false)]
    [InlineData("anthropic.claude-haiku-5-5-preview", false, false)]
    [InlineData("anthropic.claude-haiku-5-5-9", false, false)]
    [InlineData("anthropic.claude-sonnet-5-9", false, false)]
    [InlineData("anthropic.claude-opus-5-60-v1", false, false)]
    [InlineData("us.anthropic.claude-opus-6-v1:0", false, false)]
    [InlineData("global.anthropic.claude-future-v1:0", false, false)]
    [InlineData("anthropic.claude-opus-4-60-v1", false, false)]
    [InlineData("meta.llama3-70b-instruct-v1:0", true, true)]
    [InlineData("arn:aws:bedrock:us-east-1:123456789012:application-inference-profile/custom", true, true)]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-anthropic-model-support.test.ts::preserves Bedrock capabilities for %s", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_Bedrock_capabilities_for(string modelId, bool native, bool strict)
    {
        Assert.Equal(native, AmazonBedrockModelSupport.SupportsNativeStructuredOutput(modelId));
        Assert.Equal(strict, AmazonBedrockModelSupport.SupportsStrictTools(modelId));
    }
}
