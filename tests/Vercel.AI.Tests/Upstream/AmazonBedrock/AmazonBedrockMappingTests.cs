// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class AmazonBedrockErrorMappingTests
{
    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-error.test.ts::amazonBedrockFailedResponseHandler::preserves the provider message when the error type is omitted", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_the_provider_message_when_the_error_type_is_omitted()
    {
        Assert.Equal("boom", AmazonBedrockErrors.FormatMessage("{\"message\":\"boom\"}"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-error.test.ts::amazonBedrockFailedResponseHandler::prefixes the provider message when the error type is present", Coverage = UpstreamCoverage.Covered)]
    public void Prefixes_the_provider_message_when_the_error_type_is_present()
    {
        Assert.Equal("ValidationException: boom", AmazonBedrockErrors.FormatMessage("{\"type\":\"ValidationException\",\"message\":\"boom\"}"));
    }
}

public sealed class AmazonBedrockUsageMappingTests
{
    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should convert basic usage without cache tokens",
        Coverage = UpstreamCoverage.Partial,
        Note = "Input total, no-cache, and zero cache counts match. Output text tokens stay unset because reasoning tokens are not reported, and TotalTokens is the sum.")]
    public void Converts_basic_usage_without_cache_tokens()
    {
        var usage = AmazonBedrockUsage.Convert(Json("{\"inputTokens\":100,\"outputTokens\":50}"));
        Assert.Equal(100, usage.InputTokens);
        Assert.Equal(100, usage.NoCacheInputTokens);
        Assert.Equal(0, usage.CacheReadTokens);
        Assert.Equal(0, usage.CacheWriteTokens);
        Assert.Equal(50, usage.OutputTokens);
        Assert.Equal(100, usage.Raw!.Value.GetProperty("inputTokens").GetInt32());
        Assert.Equal(50, usage.Raw.Value.GetProperty("outputTokens").GetInt32());
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should convert usage with cache read tokens",
        Coverage = UpstreamCoverage.Partial,
        Note = "Input total is 180 and no-cache stays 100. Output text tokens stay unset.")]
    public void Converts_usage_with_cache_read_tokens()
    {
        var usage = AmazonBedrockUsage.Convert(Json("{\"inputTokens\":100,\"outputTokens\":50,\"cacheReadInputTokens\":80}"));
        Assert.Equal(180, usage.InputTokens);
        Assert.Equal(100, usage.NoCacheInputTokens);
        Assert.Equal(80, usage.CacheReadTokens);
        Assert.Equal(0, usage.CacheWriteTokens);
        Assert.Equal(50, usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should convert usage with cache write tokens",
        Coverage = UpstreamCoverage.Partial,
        Note = "Input total is 160 and no-cache stays 100. Output text tokens stay unset.")]
    public void Converts_usage_with_cache_write_tokens()
    {
        var usage = AmazonBedrockUsage.Convert(Json("{\"inputTokens\":100,\"outputTokens\":50,\"cacheWriteInputTokens\":60}"));
        Assert.Equal(160, usage.InputTokens);
        Assert.Equal(100, usage.NoCacheInputTokens);
        Assert.Equal(0, usage.CacheReadTokens);
        Assert.Equal(60, usage.CacheWriteTokens);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should convert usage with both cache read and write tokens",
        Coverage = UpstreamCoverage.Partial,
        Note = "Input total is 240 and no-cache stays 100. Output text tokens stay unset.")]
    public void Converts_usage_with_both_cache_counts()
    {
        var usage = AmazonBedrockUsage.Convert(Json("{\"inputTokens\":100,\"outputTokens\":50,\"cacheReadInputTokens\":80,\"cacheWriteInputTokens\":60}"));
        Assert.Equal(240, usage.InputTokens);
        Assert.Equal(100, usage.NoCacheInputTokens);
        Assert.Equal(80, usage.CacheReadTokens);
        Assert.Equal(60, usage.CacheWriteTokens);
        Assert.Equal(50, usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should handle null cache tokens",
        Coverage = UpstreamCoverage.Partial,
        Note = "Null cache counts are treated as zero and the raw object keeps the nulls.")]
    public void Handles_null_cache_tokens()
    {
        var usage = AmazonBedrockUsage.Convert(Json("{\"inputTokens\":100,\"outputTokens\":50,\"cacheReadInputTokens\":null,\"cacheWriteInputTokens\":null}"));
        Assert.Equal(100, usage.InputTokens);
        Assert.Equal(100, usage.NoCacheInputTokens);
        Assert.Equal(0, usage.CacheReadTokens);
        Assert.Equal(0, usage.CacheWriteTokens);
        Assert.Equal(JsonValueKind.Null, usage.Raw!.Value.GetProperty("cacheReadInputTokens").ValueKind);
        Assert.Equal(JsonValueKind.Null, usage.Raw.Value.GetProperty("cacheWriteInputTokens").ValueKind);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should handle null usage",
        Coverage = UpstreamCoverage.Partial,
        Note = "Input and output stay unset. TotalTokens becomes 0 because the usage type sums missing counts.")]
    public void Handles_null_usage()
    {
        var usage = AmazonBedrockUsage.Convert(null);
        Assert.Null(usage.InputTokens);
        Assert.Null(usage.OutputTokens);
        Assert.Null(usage.NoCacheInputTokens);
        Assert.Null(usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Null(usage.Raw);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should handle undefined usage",
        Coverage = UpstreamCoverage.Partial,
        Note = "A missing usage object leaves the token counts unset. TotalTokens becomes 0.")]
    public void Handles_undefined_usage()
    {
        var usage = AmazonBedrockUsage.Convert(default(JsonElement));
        Assert.Null(usage.InputTokens);
        Assert.Null(usage.OutputTokens);
        Assert.Null(usage.Raw);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should include totalTokens in raw when provided", Coverage = UpstreamCoverage.Covered)]
    public void Includes_total_tokens_in_raw_when_provided()
    {
        var usage = AmazonBedrockUsage.Convert(Json("{\"inputTokens\":100,\"outputTokens\":50,\"totalTokens\":150}"));
        Assert.Equal(100, usage.Raw!.Value.GetProperty("inputTokens").GetInt32());
        Assert.Equal(50, usage.Raw.Value.GetProperty("outputTokens").GetInt32());
        Assert.Equal(150, usage.Raw.Value.GetProperty("totalTokens").GetInt32());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should preserve raw usage data", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_raw_usage_data()
    {
        var usage = AmazonBedrockUsage.Convert(Json("{\"inputTokens\":100,\"outputTokens\":50,\"totalTokens\":150,\"cacheReadInputTokens\":80,\"cacheWriteInputTokens\":60}"));
        var raw = usage.Raw!.Value;
        Assert.Equal(100, raw.GetProperty("inputTokens").GetInt32());
        Assert.Equal(50, raw.GetProperty("outputTokens").GetInt32());
        Assert.Equal(150, raw.GetProperty("totalTokens").GetInt32());
        Assert.Equal(80, raw.GetProperty("cacheReadInputTokens").GetInt32());
        Assert.Equal(60, raw.GetProperty("cacheWriteInputTokens").GetInt32());
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}

public sealed class AmazonBedrockToolIdTests
{
    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::isMistralModel::should return true for mistral models", Coverage = UpstreamCoverage.Covered)]
    public void Returns_true_for_mistral_models()
    {
        Assert.True(AmazonBedrockToolIds.IsMistralModel("mistral.mistral-7b-instruct-v0:2"));
        Assert.True(AmazonBedrockToolIds.IsMistralModel("mistral.mixtral-8x7b-instruct-v0:1"));
        Assert.True(AmazonBedrockToolIds.IsMistralModel("mistral.mistral-large-2402-v1:0"));
        Assert.True(AmazonBedrockToolIds.IsMistralModel("mistral.mistral-small-2402-v1:0"));
        Assert.True(AmazonBedrockToolIds.IsMistralModel("mistral.mistral-large-2407-v1:0"));
        Assert.True(AmazonBedrockToolIds.IsMistralModel("mistral.ministral-3-14b-instruct"));
        Assert.True(AmazonBedrockToolIds.IsMistralModel("mistral.ministral-3-8b-instruct"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::isMistralModel::should return true for region-prefixed mistral models", Coverage = UpstreamCoverage.Covered)]
    public void Returns_true_for_region_prefixed_mistral_models()
    {
        Assert.True(AmazonBedrockToolIds.IsMistralModel("us.mistral.pixtral-large-2502-v1:0"));
        Assert.True(AmazonBedrockToolIds.IsMistralModel("eu.mistral.mistral-large-2407-v1:0"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::isMistralModel::should return false for non-mistral models", Coverage = UpstreamCoverage.Covered)]
    public void Returns_false_for_non_mistral_models()
    {
        Assert.False(AmazonBedrockToolIds.IsMistralModel("anthropic.claude-3-5-sonnet-20241022-v2:0"));
        Assert.False(AmazonBedrockToolIds.IsMistralModel("amazon.nova-pro-v1:0"));
        Assert.False(AmazonBedrockToolIds.IsMistralModel("openai.gpt-4o"));
        Assert.False(AmazonBedrockToolIds.IsMistralModel("meta.llama3-70b-instruct-v1:0"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::normalizeToolCallId::should return the original ID when not a Mistral model", Coverage = UpstreamCoverage.Covered)]
    public void Returns_the_original_id_when_not_a_mistral_model()
    {
        const string original = "tooluse_bpe71yCfRu2b5i-nKGDr5g";
        Assert.Equal(original, AmazonBedrockToolIds.Normalize(original, false));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::normalizeToolCallId::should extract first 9 alphanumeric characters for Mistral models", Coverage = UpstreamCoverage.Covered)]
    public void Extracts_the_first_nine_alphanumeric_characters()
    {
        Assert.Equal("toolusebp", AmazonBedrockToolIds.Normalize("tooluse_bpe71yCfRu2b5i-nKGDr5g", true));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::normalizeToolCallId::should handle IDs with various special characters", Coverage = UpstreamCoverage.Covered)]
    public void Handles_ids_with_special_characters()
    {
        Assert.Equal("tooluse12", AmazonBedrockToolIds.Normalize("tool-use_123ABC456", true));
        Assert.Equal("abc123DEF", AmazonBedrockToolIds.Normalize("___abc123DEF___", true));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::normalizeToolCallId::should handle IDs that are already alphanumeric", Coverage = UpstreamCoverage.Covered)]
    public void Handles_ids_that_are_already_alphanumeric()
    {
        Assert.Equal("abcdefghi", AmazonBedrockToolIds.Normalize("abcdefghi", true));
        Assert.Equal("abc123XYZ", AmazonBedrockToolIds.Normalize("abc123XYZ", true));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::normalizeToolCallId::should handle short IDs", Coverage = UpstreamCoverage.Covered)]
    public void Handles_short_ids()
    {
        Assert.Equal("abc", AmazonBedrockToolIds.Normalize("abc", true));
        Assert.Equal("12345", AmazonBedrockToolIds.Normalize("12345", true));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::normalizeToolCallId::should handle IDs with only special characters", Coverage = UpstreamCoverage.Covered)]
    public void Handles_ids_with_only_special_characters()
    {
        Assert.Equal(string.Empty, AmazonBedrockToolIds.Normalize("___---___", true));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::normalizeToolCallId::should produce valid Mistral tool call IDs (9 alphanumeric chars)", Coverage = UpstreamCoverage.Covered)]
    public void Produces_a_valid_mistral_tool_call_id()
    {
        var normalized = AmazonBedrockToolIds.Normalize("tooluse_bpe71yCfRu2b5i-nKGDr5g", true);
        Assert.Matches("^[a-zA-Z0-9]{1,9}$", normalized);
    }
}
