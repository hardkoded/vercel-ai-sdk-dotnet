// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AmazonBedrock;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockAnthropicModelSupport;

public sealed class AmazonBedrockAnthropicModelSupportTests
{
    [Theory]
    [InlineData("anthropic.claude-3-7-sonnet-20250219-v1:0", true)]
    [InlineData("anthropic.claude-sonnet-4-20250514-v1:0", true)]
    [InlineData("us.anthropic.claude-opus-4-1-20250805-v1:0", true)]
    [InlineData("anthropic.claude-sonnet-4-5-20250929-v1:0", true)]
    [InlineData("anthropic.claude-opus-4-6-v1", true)]
    [InlineData("anthropic.claude-sonnet-4-6-v1", true)]
    [InlineData("anthropic.claude-haiku-4-5-20251001-v1:0", true)]
    [InlineData("meta.llama3-70b-instruct-v1:0", true)]
    [InlineData("arn:aws:bedrock:us-east-1:123456789012:application-inference-profile/custom", true)]
    [InlineData("anthropic.claude-opus-4-7", false)]
    [InlineData("anthropic.claude-opus-5", false)]
    [InlineData("anthropic.claude-sonnet-5-5", false)]
    [InlineData("us.anthropic.claude-opus-6-v1:0", false)]
    [InlineData("global.anthropic.claude-future-v1:0", false)]
    [InlineData("anthropic.claude-opus-4-60-v1", false)]
    public void Supports_strict_tools_follows_the_Claude_allowlist_for(string modelId, bool supportsStrictTools)
    {
        Assert.Equal(supportsStrictTools, AmazonBedrockTools.SupportsStrictTools(modelId));
    }
}
