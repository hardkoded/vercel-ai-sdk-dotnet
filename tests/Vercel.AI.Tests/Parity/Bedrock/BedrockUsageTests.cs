// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Converse usage conversion and raw usage preservation.</summary>
public sealed class BedrockUsageTests
{
    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should convert basic usage without cache tokens", Coverage = UpstreamCoverage.Covered)]
    public void Converts_usage_without_cache_tokens()
    {
        var usage = AmazonBedrockUsage.Convert(BedrockParity.Element("{\"inputTokens\":100,\"outputTokens\":50}"));

        Assert.Equal(100, usage.InputTotal);
        Assert.Equal(100, usage.NoCache);
        Assert.Equal(0, usage.CacheRead);
        Assert.Equal(0, usage.CacheWrite);
        Assert.Equal(50, usage.OutputTotal);
        Assert.Equal(50, usage.OutputText);
        Assert.Null(usage.OutputReasoning);
        Assert.Equal(100, usage.Raw!.Value.GetProperty("inputTokens").GetInt32());
        Assert.False(usage.Raw.Value.TryGetProperty("cacheReadInputTokens", out _));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should convert usage with cache read tokens", Coverage = UpstreamCoverage.Covered)]
    public void Adds_cache_read_tokens_into_the_input_total()
    {
        var usage = AmazonBedrockUsage.Convert(BedrockParity.Element("{\"inputTokens\":100,\"outputTokens\":50,\"cacheReadInputTokens\":80}"));

        Assert.Equal(180, usage.InputTotal);
        Assert.Equal(100, usage.NoCache);
        Assert.Equal(80, usage.CacheRead);
        Assert.Equal(0, usage.CacheWrite);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should convert usage with cache write tokens", Coverage = UpstreamCoverage.Covered)]
    public void Adds_cache_write_tokens_into_the_input_total()
    {
        var usage = AmazonBedrockUsage.Convert(BedrockParity.Element("{\"inputTokens\":100,\"outputTokens\":50,\"cacheWriteInputTokens\":60}"));

        Assert.Equal(160, usage.InputTotal);
        Assert.Equal(0, usage.CacheRead);
        Assert.Equal(60, usage.CacheWrite);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should convert usage with both cache read and write tokens", Coverage = UpstreamCoverage.Covered)]
    public void Adds_cache_read_and_write_tokens()
    {
        var usage = AmazonBedrockUsage.Convert(BedrockParity.Element("{\"inputTokens\":100,\"outputTokens\":50,\"cacheReadInputTokens\":80,\"cacheWriteInputTokens\":60}"));

        Assert.Equal(240, usage.InputTotal);
        Assert.Equal(100, usage.NoCache);
        Assert.Equal(80, usage.CacheRead);
        Assert.Equal(60, usage.CacheWrite);
        Assert.Equal(50, usage.OutputText);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should handle null cache tokens", Coverage = UpstreamCoverage.Covered)]
    public void Treats_null_cache_tokens_as_zero_and_keeps_them_in_raw()
    {
        var usage = AmazonBedrockUsage.Convert(BedrockParity.Element("{\"inputTokens\":100,\"outputTokens\":50,\"cacheReadInputTokens\":null,\"cacheWriteInputTokens\":null}"));

        Assert.Equal(100, usage.InputTotal);
        Assert.Equal(0, usage.CacheRead);
        Assert.Equal(0, usage.CacheWrite);
        Assert.Equal(JsonValueKind.Null, usage.Raw!.Value.GetProperty("cacheReadInputTokens").ValueKind);
        Assert.Equal(JsonValueKind.Null, usage.Raw.Value.GetProperty("cacheWriteInputTokens").ValueKind);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should handle null usage", Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_counts_for_null_usage()
    {
        var usage = AmazonBedrockUsage.Convert(BedrockParity.Element("null"));

        Assert.Null(usage.InputTotal);
        Assert.Null(usage.NoCache);
        Assert.Null(usage.CacheRead);
        Assert.Null(usage.CacheWrite);
        Assert.Null(usage.OutputTotal);
        Assert.Null(usage.OutputText);
        Assert.Null(usage.OutputReasoning);
        Assert.Null(usage.Raw);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should handle undefined usage", Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_counts_when_usage_is_absent()
    {
        var usage = AmazonBedrockUsage.Convert((JsonElement?)null);

        Assert.Null(usage.InputTotal);
        Assert.Null(usage.OutputTotal);
        Assert.Null(usage.Raw);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should include totalTokens in raw when provided", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_total_tokens_on_the_raw_usage_object()
    {
        var usage = AmazonBedrockUsage.Convert(BedrockParity.Element("{\"inputTokens\":100,\"outputTokens\":50,\"totalTokens\":150}"));

        Assert.Equal(150, usage.Raw!.Value.GetProperty("totalTokens").GetInt32());
        Assert.Equal(150, usage.ToLanguageModelUsage().TotalTokens);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-amazon-bedrock-usage.test.ts::convertAmazonBedrockUsage::should preserve raw usage data", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_the_provider_usage_object()
    {
        var raw = "{\"inputTokens\":100,\"outputTokens\":50,\"totalTokens\":150,\"cacheReadInputTokens\":80,\"cacheWriteInputTokens\":60,\"serverToolUsage\":{}}";
        var usage = AmazonBedrockUsage.Convert(BedrockParity.Element(raw));

        Assert.Equal(JsonValueKind.Object, usage.Raw!.Value.GetProperty("serverToolUsage").ValueKind);
        Assert.Equal(80, usage.Raw.Value.GetProperty("cacheReadInputTokens").GetInt32());
        Assert.Equal(240, usage.InputTotal);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-usage-raw.test.ts::AmazonBedrockChatLanguageModel raw usage::preserves complete raw usage through doGenerate parsing", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_complete_raw_usage_through_generate()
    {
        var handler = new BedrockJsonHandler(@"{
            ""output"": { ""message"": { ""content"": [] } },
            ""stopReason"": ""end_turn"",
            ""usage"": {
                ""inputTokens"": 13,
                ""outputTokens"": 5,
                ""totalTokens"": 18,
                ""cacheReadInputTokens"": 3,
                ""cacheWriteInputTokens"": 2,
                ""cacheDetails"": [{ ""inputTokens"": 2, ""ttl"": ""T5M"", ""providerCacheMetadata"": { ""source"": ""provider"" } }],
                ""providerUsageMetadata"": { ""serverToolUsage"": {} }
            }
        }");
        var model = BedrockParity.Model(handler, "anthropic.claude-3-haiku-20240307-v1:0");
        var result = await model.DoGenerateAsync(BedrockParity.Call("Hello", null), CancellationToken.None);

        Assert.Equal(18, result.Usage.InputTokens);
        Assert.Equal(13, result.Usage.NoCacheInputTokens);
        Assert.Equal(3, result.Usage.CacheReadTokens);
        Assert.Equal(2, result.Usage.CacheWriteTokens);
        Assert.Equal(5, result.Usage.OutputTokens);
        Assert.Null(result.Usage.ReasoningTokens);
        Assert.Equal(18, result.Usage.Raw!.Value.GetProperty("totalTokens").GetInt32());
        Assert.Equal("provider", result.Usage.Raw.Value.GetProperty("cacheDetails")[0].GetProperty("providerCacheMetadata").GetProperty("source").GetString());
        Assert.True(result.Usage.Raw.Value.TryGetProperty("providerUsageMetadata", out _));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-usage-raw.test.ts::AmazonBedrockChatLanguageModel raw usage::validates known cache detail fields during doGenerate parsing", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_non_numeric_cache_detail_input_tokens()
    {
        var handler = new BedrockJsonHandler(@"{
            ""output"": { ""message"": { ""content"": [] } },
            ""stopReason"": ""end_turn"",
            ""usage"": {
                ""inputTokens"": 13,
                ""outputTokens"": 5,
                ""totalTokens"": 18,
                ""cacheDetails"": [{ ""inputTokens"": ""2"", ""ttl"": ""T5M"" }]
            }
        }");
        var model = BedrockParity.Model(handler, "anthropic.claude-3-haiku-20240307-v1:0");

        var error = await Assert.ThrowsAsync<AmazonBedrockResponseException>(() => model.DoGenerateAsync(BedrockParity.Call("Hello", null), CancellationToken.None));

        Assert.Contains("\"inputTokens\"", error.Message, StringComparison.Ordinal);
    }
}
