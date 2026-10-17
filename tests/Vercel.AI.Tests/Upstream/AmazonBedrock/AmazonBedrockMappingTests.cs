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
        Coverage = UpstreamCoverage.Covered)]
    public void Converts_basic_usage_without_cache_tokens()
    {
        const string json = "{\"inputTokens\":100,\"outputTokens\":50}";
        var usage = AmazonBedrockUsage.Convert(Json(json));
        Assert.Equal(100, usage.InputTokens);
        Assert.Equal(100, usage.NoCacheInputTokens);
        Assert.Equal(0, usage.CacheReadTokens);
        Assert.Equal(0, usage.CacheWriteTokens);
        AssertOutputAndRaw(usage, json);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should convert usage with cache read tokens",
        Coverage = UpstreamCoverage.Covered)]
    public void Converts_usage_with_cache_read_tokens()
    {
        const string json = "{\"inputTokens\":100,\"outputTokens\":50,\"cacheReadInputTokens\":80}";
        var usage = AmazonBedrockUsage.Convert(Json(json));
        Assert.Equal(180, usage.InputTokens);
        Assert.Equal(100, usage.NoCacheInputTokens);
        Assert.Equal(80, usage.CacheReadTokens);
        Assert.Equal(0, usage.CacheWriteTokens);
        AssertOutputAndRaw(usage, json);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should convert usage with cache write tokens",
        Coverage = UpstreamCoverage.Covered)]
    public void Converts_usage_with_cache_write_tokens()
    {
        const string json = "{\"inputTokens\":100,\"outputTokens\":50,\"cacheWriteInputTokens\":60}";
        var usage = AmazonBedrockUsage.Convert(Json(json));
        Assert.Equal(160, usage.InputTokens);
        Assert.Equal(100, usage.NoCacheInputTokens);
        Assert.Equal(0, usage.CacheReadTokens);
        Assert.Equal(60, usage.CacheWriteTokens);
        AssertOutputAndRaw(usage, json);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should convert usage with both cache read and write tokens",
        Coverage = UpstreamCoverage.Covered)]
    public void Converts_usage_with_both_cache_counts()
    {
        const string json = "{\"inputTokens\":100,\"outputTokens\":50,\"cacheReadInputTokens\":80,\"cacheWriteInputTokens\":60}";
        var usage = AmazonBedrockUsage.Convert(Json(json));
        Assert.Equal(240, usage.InputTokens);
        Assert.Equal(100, usage.NoCacheInputTokens);
        Assert.Equal(80, usage.CacheReadTokens);
        Assert.Equal(60, usage.CacheWriteTokens);
        AssertOutputAndRaw(usage, json);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should handle null cache tokens",
        Coverage = UpstreamCoverage.Covered)]
    public void Handles_null_cache_tokens()
    {
        const string json = "{\"inputTokens\":100,\"outputTokens\":50,\"cacheReadInputTokens\":null,\"cacheWriteInputTokens\":null}";
        var usage = AmazonBedrockUsage.Convert(Json(json));
        Assert.Equal(100, usage.InputTokens);
        Assert.Equal(100, usage.NoCacheInputTokens);
        Assert.Equal(0, usage.CacheReadTokens);
        Assert.Equal(0, usage.CacheWriteTokens);
        AssertOutputAndRaw(usage, json);
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

    private static void AssertOutputAndRaw(LanguageModelUsage usage, string raw)
    {
        Assert.Equal(50, usage.OutputTokens);
        Assert.Equal(50, usage.TextTokens);
        Assert.Null(usage.ReasoningTokens);
        Assert.Equal(raw, usage.Raw!.Value.GetRawText());
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
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::normalizeToolCallId::should hash incompatible IDs deterministically for Mistral models", Coverage = UpstreamCoverage.Covered)]
    public void Hashes_incompatible_ids_deterministically_for_mistral_models()
    {
        const string toolCallId = "tooluse_bpe71yCfRu2b5i-nKGDr5g";
        Assert.Equal(
            new[] { "8eHypBDcw", "8eHypBDcw" },
            new[] { AmazonBedrockToolIds.Normalize(toolCallId, true), AmazonBedrockToolIds.Normalize(toolCallId, true) });
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::normalizeToolCallId::should produce 9 alphanumeric characters for incompatible IDs", Coverage = UpstreamCoverage.Covered)]
    public void Produces_nine_alphanumeric_characters_for_incompatible_ids()
    {
        var normalizedIds = new[]
        {
            AmazonBedrockToolIds.Normalize("tool-use_123ABC456", true),
            AmazonBedrockToolIds.Normalize("___abc123DEF___", true),
            AmazonBedrockToolIds.Normalize("abc", true),
            AmazonBedrockToolIds.Normalize("12345", true),
            AmazonBedrockToolIds.Normalize("___---___", true),
        };

        Assert.Equal(new[] { "hvVDqPNyj", "TnzPqldGU", "GRuIyUwcV", "PceAgWDYe", "5C589HVqG" }, normalizedIds);
        foreach (var normalizedId in normalizedIds)
        {
            Assert.Matches("^[a-zA-Z0-9]{9}$", normalizedId);
        }
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::normalizeToolCallId::should preserve IDs that are already valid Mistral tool call IDs", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_ids_that_are_already_valid_mistral_tool_call_ids()
    {
        Assert.Equal("abcdefghi", AmazonBedrockToolIds.Normalize("abcdefghi", true));
        Assert.Equal("abc123XYZ", AmazonBedrockToolIds.Normalize("abc123XYZ", true));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/normalize-tool-call-id.test.ts::normalizeToolCallId::should keep distinct Bedrock IDs distinct when their prefixes match", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_distinct_bedrock_ids_distinct_when_their_prefixes_match()
    {
        Assert.Equal(
            new[] { "7rDVWRig0", "bNZvZKNBZ" },
            new[]
            {
                AmazonBedrockToolIds.Normalize("tooluse_Ac1Xq9ZklmNoPq", true),
                AmazonBedrockToolIds.Normalize("tooluse_Ac2Yt7WrstUvWx", true),
            });
    }
}
