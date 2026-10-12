// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Xai;

namespace Vercel.AI.Tests.Upstream.Xai.ConvertXaiResponsesUsage;

/// <summary>Port of <c>convert-xai-responses-usage.test.ts</c> &gt; <c>convertXaiResponsesUsage</c>.</summary>
public sealed class ConvertXaiResponsesUsageTests
{
    private const string Prefix = "packages/xai/src/responses/convert-xai-responses-usage.test.ts::convertXaiResponsesUsage::";

    [Fact]
    [UpstreamTest(Prefix + "should convert basic usage without caching or reasoning", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_basic_usage_without_caching_or_reasoning()
    {
        var result = Convert("""{"input_tokens":100,"output_tokens":50}""");

        Assert.Equal(0, result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(100, result.NoCacheInputTokens);
        Assert.Equal(100, result.InputTokens);
        Assert.Equal(0, result.ReasoningTokens);
        Assert.Equal(50, result.TextTokens);
        Assert.Equal(50, result.OutputTokens);
        JsonAssert.Equal(result.Raw!.Value, """{"input_tokens":100,"output_tokens":50}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert usage with reasoning tokens", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_usage_with_reasoning_tokens()
    {
        var result = Convert("""{"input_tokens":1941,"output_tokens":583,"total_tokens":2524,"output_tokens_details":{"reasoning_tokens":380}}""");

        Assert.Equal(380, result.ReasoningTokens);
        Assert.Equal(203, result.TextTokens);
        Assert.Equal(583, result.OutputTokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert usage with cached input tokens", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_usage_with_cached_input_tokens()
    {
        var result = Convert("""{"input_tokens":200,"output_tokens":50,"input_tokens_details":{"cached_tokens":150}}""");

        Assert.Equal(150, result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(50, result.NoCacheInputTokens);
        Assert.Equal(200, result.InputTokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle cached_tokens exceeding input_tokens (non-inclusive reporting)", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_cached_tokens_exceeding_input_tokens_non_inclusive_reporting()
    {
        var result = Convert("""{"input_tokens":4142,"output_tokens":254,"input_tokens_details":{"cached_tokens":4328}}""");

        Assert.Equal(4328, result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(4142, result.NoCacheInputTokens);
        Assert.Equal(8470, result.InputTokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert usage with both cached input and reasoning", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_usage_with_both_cached_input_and_reasoning()
    {
        var json = """{"input_tokens":200,"output_tokens":583,"input_tokens_details":{"cached_tokens":150},"output_tokens_details":{"reasoning_tokens":380}}""";

        var result = Convert(json);

        Assert.Equal(150, result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(50, result.NoCacheInputTokens);
        Assert.Equal(200, result.InputTokens);
        Assert.Equal(380, result.ReasoningTokens);
        Assert.Equal(203, result.TextTokens);
        Assert.Equal(583, result.OutputTokens);
        JsonAssert.Equal(result.Raw!.Value, json);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve raw usage data", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_raw_usage_data()
    {
        var json = """{"input_tokens":12,"output_tokens":319,"total_tokens":331,"input_tokens_details":{"cached_tokens":2},"output_tokens_details":{"reasoning_tokens":317}}""";

        JsonAssert.Equal(Convert(json).Raw!.Value, json);
    }

    private static Vercel.AI.Provider.LanguageModelUsage Convert(string json)
    {
        return XaiResponsesResults.ConvertUsage(UpstreamChat.Json(json));
    }
}
