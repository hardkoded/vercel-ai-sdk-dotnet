// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Converter = Vercel.AI.Alibaba.ConvertAlibabaUsage;

namespace Vercel.AI.Tests.Upstream.Alibaba.ConvertAlibabaUsage;

/// <summary>Port of <c>convert-alibaba-usage.test.ts</c> &gt; <c>convertAlibabaUsage</c>.</summary>
public sealed class ConvertAlibabaUsageTests
{
    private const string Prefix =
        "packages/alibaba/src/convert-alibaba-usage.test.ts::convertAlibabaUsage::";

    private static LanguageModelUsage Convert(string json)
    {
        using var document = JsonDocument.Parse(json);
        return Converter.Convert(document.RootElement);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly calculate token distribution with cache tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_calculate_token_distribution_with_cache_tokens()
    {
        var result = Convert(
            """
            {
              "prompt_tokens": 200,
              "completion_tokens": 75,
              "prompt_tokens_details": { "cached_tokens": 120, "cache_creation_input_tokens": 50 },
              "completion_tokens_details": { "reasoning_tokens": 25 }
            }
            """
        );

        Assert.Equal(200, result.InputTokens);
        Assert.Equal(120, result.CacheReadTokens);
        Assert.Equal(50, result.CacheWriteTokens);
        Assert.Equal(30, result.NoCacheInputTokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "clamps text tokens at 0 when reasoning exceeds completion",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Clamps_text_tokens_at_0_when_reasoning_exceeds_completion()
    {
        var result = Convert(
            """
            {
              "prompt_tokens": 951,
              "completion_tokens": 6000,
              "completion_tokens_details": { "reasoning_tokens": 6001 }
            }
            """
        );

        Assert.Equal(6000, result.OutputTokens);
        Assert.Equal(0, result.TextTokens);
        Assert.Equal(6001, result.ReasoningTokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return null usage when the response carries none",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_null_usage_when_the_response_carries_none()
    {
        var result = Converter.Convert(null);

        Assert.Null(result.InputTokens);
        Assert.Null(result.NoCacheInputTokens);
        Assert.Null(result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Null(result.OutputTokens);
        Assert.Null(result.TextTokens);
        Assert.Null(result.ReasoningTokens);
        Assert.Null(result.Raw);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should expose the explicit-cache discriminator through raw",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_expose_the_explicit_cache_discriminator_through_raw()
    {
        var result = Convert(
            """
            {
              "prompt_tokens": 200,
              "completion_tokens": 10,
              "prompt_tokens_details": {
                "cached_tokens": 120,
                "cache_creation_input_tokens": 0,
                "cache_type": "ephemeral"
              }
            }
            """
        );

        Assert.Equal(
            "ephemeral",
            result
                .Raw!.Value.GetProperty("prompt_tokens_details")
                .GetProperty("cache_type")
                .GetString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass undeclared usage fields through raw untouched",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_undeclared_usage_fields_through_raw_untouched()
    {
        const string Usage = """
            {
              "prompt_tokens": 10,
              "completion_tokens": 5,
              "some_future_field": "kept",
              "prompt_tokens_details": { "cached_tokens": 0, "some_future_detail": 42 }
            }
            """;

        JsonAssert.Equal(Convert(Usage).Raw!.Value, Usage);
    }
}
