// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.AmazonBedrock;

namespace Vercel.AI.Tests;

/// <summary>Mistral tool-call id normalization.</summary>
public sealed class BedrockToolCallIdTests
{
    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::isMistralModel::should return true for mistral models", Coverage = UpstreamCoverage.Covered)]
    public void Detects_mistral_model_ids()
    {
        Assert.True(AmazonBedrockToolCallId.IsMistralModel("mistral.mistral-7b-instruct-v0:2"));
        Assert.True(AmazonBedrockToolCallId.IsMistralModel("mistral.mixtral-8x7b-instruct-v0:1"));
        Assert.True(AmazonBedrockToolCallId.IsMistralModel("mistral.mistral-large-2402-v1:0"));
        Assert.True(AmazonBedrockToolCallId.IsMistralModel("mistral.mistral-small-2402-v1:0"));
        Assert.True(AmazonBedrockToolCallId.IsMistralModel("mistral.mistral-large-2407-v1:0"));
        Assert.True(AmazonBedrockToolCallId.IsMistralModel("mistral.ministral-3-14b-instruct"));
        Assert.True(AmazonBedrockToolCallId.IsMistralModel("mistral.ministral-3-8b-instruct"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::isMistralModel::should return true for region-prefixed mistral models", Coverage = UpstreamCoverage.Covered)]
    public void Detects_region_prefixed_mistral_models()
    {
        Assert.True(AmazonBedrockToolCallId.IsMistralModel("us.mistral.pixtral-large-2502-v1:0"));
        Assert.True(AmazonBedrockToolCallId.IsMistralModel("eu.mistral.mistral-large-2407-v1:0"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::isMistralModel::should return false for non-mistral models", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_non_mistral_model_ids()
    {
        Assert.False(AmazonBedrockToolCallId.IsMistralModel("anthropic.claude-3-5-sonnet-20241022-v2:0"));
        Assert.False(AmazonBedrockToolCallId.IsMistralModel("amazon.nova-pro-v1:0"));
        Assert.False(AmazonBedrockToolCallId.IsMistralModel("openai.gpt-4o"));
        Assert.False(AmazonBedrockToolCallId.IsMistralModel("meta.llama3-70b-instruct-v1:0"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::normalizeToolCallId::should return the original ID when not a Mistral model", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_non_mistral_ids_unchanged()
    {
        const string original = "tooluse_bpe71yCfRu2b5i-nKGDr5g";

        Assert.Equal(original, AmazonBedrockToolCallId.Normalize(original, false));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::normalizeToolCallId::should extract first 9 alphanumeric characters for Mistral models", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_the_first_nine_alphanumeric_characters()
    {
        Assert.Equal("toolusebp", AmazonBedrockToolCallId.Normalize("tooluse_bpe71yCfRu2b5i-nKGDr5g", true));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::normalizeToolCallId::should handle IDs with various special characters", Coverage = UpstreamCoverage.Covered)]
    public void Strips_special_characters_before_truncating()
    {
        Assert.Equal("tooluse12", AmazonBedrockToolCallId.Normalize("tool-use_123ABC456", true));
        Assert.Equal("abc123DEF", AmazonBedrockToolCallId.Normalize("___abc123DEF___", true));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::normalizeToolCallId::should handle IDs that are already alphanumeric", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_alphanumeric_ids_that_are_already_nine_characters()
    {
        Assert.Equal("abcdefghi", AmazonBedrockToolCallId.Normalize("abcdefghi", true));
        Assert.Equal("abc123XYZ", AmazonBedrockToolCallId.Normalize("abc123XYZ", true));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::normalizeToolCallId::should handle short IDs", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_short_alphanumeric_ids()
    {
        Assert.Equal("abc", AmazonBedrockToolCallId.Normalize("abc", true));
        Assert.Equal("12345", AmazonBedrockToolCallId.Normalize("12345", true));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::normalizeToolCallId::should handle IDs with only special characters", Coverage = UpstreamCoverage.Covered)]
    public void Returns_empty_when_no_alphanumeric_characters_remain()
    {
        Assert.Equal(string.Empty, AmazonBedrockToolCallId.Normalize("___---___", true));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::normalizeToolCallId::should produce valid Mistral tool call IDs (9 alphanumeric chars)", Coverage = UpstreamCoverage.Covered)]
    public void Produces_an_id_mistral_accepts()
    {
        var normalized = AmazonBedrockToolCallId.Normalize("tooluse_bpe71yCfRu2b5i-nKGDr5g", true);

        Assert.Matches(new Regex("^[a-zA-Z0-9]{1,9}$"), normalized);
    }
}
