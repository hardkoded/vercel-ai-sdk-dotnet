// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Groq;

namespace Vercel.AI.Tests.ConvertGroqUsage;

/// <summary>Port of <c>convert-groq-usage.test.ts</c> &gt; <c>convertGroqUsage</c>.</summary>
public sealed class ConvertGroqUsageTests
{
    private const string Prefix =
        "packages/groq/src/convert-groq-usage.test.ts::convertGroqUsage::";

    [Fact]
    [UpstreamTest(
        Prefix + "should return undefined values when usage is null",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_undefined_values_when_usage_is_null()
    {
        var result = global::Vercel.AI.Groq.ConvertGroqUsage.Convert(null);

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
        Prefix + "should return undefined values when usage is undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_undefined_values_when_usage_is_undefined()
    {
        var result = global::Vercel.AI.Groq.ConvertGroqUsage.Convert(null);

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
        Prefix + "should convert basic usage without token details",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_basic_usage_without_token_details()
    {
        var raw = JsonDocument.Parse("""{"prompt_tokens":20,"completion_tokens":10}""").RootElement;

        var result = global::Vercel.AI.Groq.ConvertGroqUsage.Convert(raw);

        Assert.Equal(20, result.InputTokens);
        Assert.Equal(20, result.NoCacheInputTokens);
        Assert.Null(result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(10, result.OutputTokens);
        Assert.Equal(10, result.TextTokens);
        Assert.Null(result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract reasoning tokens from completion_tokens_details",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_extract_reasoning_tokens_from_completiontokensdetails()
    {
        var raw = JsonDocument
            .Parse(
                """{"prompt_tokens":79,"completion_tokens":40,"completion_tokens_details":{"reasoning_tokens":21}}"""
            )
            .RootElement;

        var result = global::Vercel.AI.Groq.ConvertGroqUsage.Convert(raw);

        Assert.Equal(79, result.InputTokens);
        Assert.Equal(79, result.NoCacheInputTokens);
        Assert.Null(result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(40, result.OutputTokens);
        Assert.Equal(19, result.TextTokens);
        Assert.Equal(21, result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle null reasoning_tokens in completion_tokens_details",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_null_reasoningtokens_in_completiontokensdetails()
    {
        var raw = JsonDocument
            .Parse(
                """{"prompt_tokens":20,"completion_tokens":10,"completion_tokens_details":{"reasoning_tokens":null}}"""
            )
            .RootElement;

        var result = global::Vercel.AI.Groq.ConvertGroqUsage.Convert(raw);

        Assert.Equal(20, result.InputTokens);
        Assert.Equal(20, result.NoCacheInputTokens);
        Assert.Null(result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(10, result.OutputTokens);
        Assert.Equal(10, result.TextTokens);
        Assert.Null(result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle null completion_tokens_details",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_null_completiontokensdetails()
    {
        var raw = JsonDocument
            .Parse(
                """{"prompt_tokens":20,"completion_tokens":10,"completion_tokens_details":null}"""
            )
            .RootElement;

        var result = global::Vercel.AI.Groq.ConvertGroqUsage.Convert(raw);

        Assert.Equal(20, result.InputTokens);
        Assert.Equal(20, result.NoCacheInputTokens);
        Assert.Null(result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(10, result.OutputTokens);
        Assert.Equal(10, result.TextTokens);
        Assert.Null(result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle zero reasoning tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_zero_reasoning_tokens()
    {
        var raw = JsonDocument
            .Parse(
                """{"prompt_tokens":20,"completion_tokens":10,"completion_tokens_details":{"reasoning_tokens":0}}"""
            )
            .RootElement;

        var result = global::Vercel.AI.Groq.ConvertGroqUsage.Convert(raw);

        Assert.Equal(20, result.InputTokens);
        Assert.Equal(20, result.NoCacheInputTokens);
        Assert.Null(result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(10, result.OutputTokens);
        Assert.Equal(10, result.TextTokens);
        Assert.Equal(0, result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle all tokens being reasoning tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_all_tokens_being_reasoning_tokens()
    {
        var raw = JsonDocument
            .Parse(
                """{"prompt_tokens":20,"completion_tokens":50,"completion_tokens_details":{"reasoning_tokens":50}}"""
            )
            .RootElement;

        var result = global::Vercel.AI.Groq.ConvertGroqUsage.Convert(raw);

        Assert.Equal(20, result.InputTokens);
        Assert.Equal(20, result.NoCacheInputTokens);
        Assert.Null(result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(50, result.OutputTokens);
        Assert.Equal(0, result.TextTokens);
        Assert.Equal(50, result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "clamps text tokens at 0 when reasoning exceeds completion",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Clamps_text_tokens_at_0_when_reasoning_exceeds_completion()
    {
        var raw = JsonDocument
            .Parse(
                """{"prompt_tokens":951,"completion_tokens":6000,"completion_tokens_details":{"reasoning_tokens":6001}}"""
            )
            .RootElement;

        var result = global::Vercel.AI.Groq.ConvertGroqUsage.Convert(raw);

        Assert.Equal(951, result.InputTokens);
        Assert.Equal(951, result.NoCacheInputTokens);
        Assert.Null(result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(6000, result.OutputTokens);
        Assert.Equal(0, result.TextTokens);
        Assert.Equal(6001, result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map cached_tokens to cacheRead and subtract from noCache",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_map_cachedtokens_to_cacheread_and_subtract_from_nocache()
    {
        var raw = JsonDocument
            .Parse(
                """{"prompt_tokens":4641,"completion_tokens":1817,"prompt_tokens_details":{"cached_tokens":4608}}"""
            )
            .RootElement;

        var result = global::Vercel.AI.Groq.ConvertGroqUsage.Convert(raw);

        Assert.Equal(4641, result.InputTokens);
        Assert.Equal(33, result.NoCacheInputTokens);
        Assert.Equal(4608, result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(1817, result.OutputTokens);
        Assert.Equal(1817, result.TextTokens);
        Assert.Null(result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should treat zero cached_tokens as a cache miss (cacheRead 0)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_treat_zero_cachedtokens_as_a_cache_miss_cacheread_0()
    {
        var raw = JsonDocument
            .Parse(
                """{"prompt_tokens":20,"completion_tokens":10,"prompt_tokens_details":{"cached_tokens":0}}"""
            )
            .RootElement;

        var result = global::Vercel.AI.Groq.ConvertGroqUsage.Convert(raw);

        Assert.Equal(20, result.InputTokens);
        Assert.Equal(20, result.NoCacheInputTokens);
        Assert.Equal(0, result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(10, result.OutputTokens);
        Assert.Equal(10, result.TextTokens);
        Assert.Null(result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should leave cacheRead undefined when cached_tokens is null",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_leave_cacheread_undefined_when_cachedtokens_is_null()
    {
        var raw = JsonDocument
            .Parse(
                """{"prompt_tokens":20,"completion_tokens":10,"prompt_tokens_details":{"cached_tokens":null}}"""
            )
            .RootElement;

        var result = global::Vercel.AI.Groq.ConvertGroqUsage.Convert(raw);

        Assert.Equal(20, result.InputTokens);
        Assert.Equal(20, result.NoCacheInputTokens);
        Assert.Null(result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(10, result.OutputTokens);
        Assert.Equal(10, result.TextTokens);
        Assert.Null(result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle missing prompt_tokens and completion_tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_missing_prompttokens_and_completiontokens()
    {
        var raw = JsonDocument.Parse("""{}""").RootElement;

        var result = global::Vercel.AI.Groq.ConvertGroqUsage.Convert(raw);

        Assert.Equal(0, result.InputTokens);
        Assert.Equal(0, result.NoCacheInputTokens);
        Assert.Null(result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(0, result.OutputTokens);
        Assert.Equal(0, result.TextTokens);
        Assert.Null(result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }
}
