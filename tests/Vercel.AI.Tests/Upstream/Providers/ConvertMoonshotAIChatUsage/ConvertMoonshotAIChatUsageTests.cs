// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Moonshot;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ConvertMoonshotAIChatUsage;

/// <summary>Port of <c>convert-moonshotai-chat-usage.test.ts</c> &gt; <c>convertMoonshotAIChatUsage</c>.</summary>
public sealed class ConvertMoonshotAIChatUsageTests
{
    private const string Prefix =
        "packages/moonshotai/src/convert-moonshotai-chat-usage.test.ts::convertMoonshotAIChatUsage::";

    [Fact]
    [UpstreamTest(Prefix + "should handle null usage", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_null_usage()
    {
        var result = MoonshotChatUsage.ConvertMoonshotAIChatUsage(null);

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
    [UpstreamTest(Prefix + "should handle undefined usage", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_undefined_usage()
    {
        var result = MoonshotChatUsage.ConvertMoonshotAIChatUsage(null);

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
        Prefix + "should convert basic usage without caching or reasoning",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_basic_usage_without_caching_or_reasoning()
    {
        var raw = JsonDocument
            .Parse("""{"prompt_tokens":100,"completion_tokens":50}""")
            .RootElement;

        var result = MoonshotChatUsage.ConvertMoonshotAIChatUsage(raw);

        Assert.Equal(100, result.InputTokens);
        Assert.Equal(100, result.NoCacheInputTokens);
        Assert.Equal(0, result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(50, result.OutputTokens);
        Assert.Equal(50, result.TextTokens);
        Assert.Equal(0, result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert usage with top-level cached_tokens (Moonshot format)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_usage_with_top_level_cached_tokens_Moonshot_format()
    {
        var raw = JsonDocument
            .Parse("""{"prompt_tokens":100,"completion_tokens":50,"cached_tokens":30}""")
            .RootElement;

        var result = MoonshotChatUsage.ConvertMoonshotAIChatUsage(raw);

        Assert.Equal(100, result.InputTokens);
        Assert.Equal(70, result.NoCacheInputTokens);
        Assert.Equal(30, result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(50, result.OutputTokens);
        Assert.Equal(50, result.TextTokens);
        Assert.Equal(0, result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should report cache_write_tokens and exclude them from noCache",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public void Should_report_cache_write_tokens_and_exclude_them_from_noCache()
    {
        var raw = JsonDocument
            .Parse(
                """{"prompt_tokens":100,"completion_tokens":50,"prompt_tokens_details":{"cached_tokens":20,"cache_write_tokens":60}}"""
            )
            .RootElement;

        var result = MoonshotChatUsage.ConvertMoonshotAIChatUsage(raw);

        Assert.Equal(100, result.InputTokens);
        Assert.Equal(20, result.NoCacheInputTokens);
        Assert.Equal(20, result.CacheReadTokens);
        Assert.Equal(60, result.CacheWriteTokens);
        Assert.Equal(50, result.OutputTokens);
        Assert.Equal(50, result.TextTokens);
        Assert.Equal(0, result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert usage with nested cached_tokens (OpenAI format)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_usage_with_nested_cached_tokens_OpenAI_format()
    {
        var raw = JsonDocument
            .Parse(
                """{"prompt_tokens":100,"completion_tokens":50,"prompt_tokens_details":{"cached_tokens":25}}"""
            )
            .RootElement;

        var result = MoonshotChatUsage.ConvertMoonshotAIChatUsage(raw);

        Assert.Equal(100, result.InputTokens);
        Assert.Equal(75, result.NoCacheInputTokens);
        Assert.Equal(25, result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(50, result.OutputTokens);
        Assert.Equal(50, result.TextTokens);
        Assert.Equal(0, result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prioritize top-level cached_tokens over nested",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_prioritize_top_level_cached_tokens_over_nested()
    {
        var raw = JsonDocument
            .Parse(
                """{"prompt_tokens":100,"completion_tokens":50,"cached_tokens":40,"prompt_tokens_details":{"cached_tokens":25}}"""
            )
            .RootElement;

        var result = MoonshotChatUsage.ConvertMoonshotAIChatUsage(raw);

        Assert.Equal(100, result.InputTokens);
        Assert.Equal(60, result.NoCacheInputTokens);
        Assert.Equal(40, result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(50, result.OutputTokens);
        Assert.Equal(50, result.TextTokens);
        Assert.Equal(0, result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert usage with reasoning tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_usage_with_reasoning_tokens()
    {
        var raw = JsonDocument
            .Parse(
                """{"prompt_tokens":100,"completion_tokens":80,"completion_tokens_details":{"reasoning_tokens":30}}"""
            )
            .RootElement;

        var result = MoonshotChatUsage.ConvertMoonshotAIChatUsage(raw);

        Assert.Equal(100, result.InputTokens);
        Assert.Equal(100, result.NoCacheInputTokens);
        Assert.Equal(0, result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(80, result.OutputTokens);
        Assert.Equal(50, result.TextTokens);
        Assert.Equal(30, result.ReasoningTokens);
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

        var result = MoonshotChatUsage.ConvertMoonshotAIChatUsage(raw);

        Assert.Equal(951, result.InputTokens);
        Assert.Equal(951, result.NoCacheInputTokens);
        Assert.Equal(0, result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(6000, result.OutputTokens);
        Assert.Equal(0, result.TextTokens);
        Assert.Equal(6001, result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert usage with both cached and reasoning tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_usage_with_both_cached_and_reasoning_tokens()
    {
        var raw = JsonDocument
            .Parse(
                """{"prompt_tokens":100,"completion_tokens":80,"cached_tokens":35,"completion_tokens_details":{"reasoning_tokens":30}}"""
            )
            .RootElement;

        var result = MoonshotChatUsage.ConvertMoonshotAIChatUsage(raw);

        Assert.Equal(100, result.InputTokens);
        Assert.Equal(65, result.NoCacheInputTokens);
        Assert.Equal(35, result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(80, result.OutputTokens);
        Assert.Equal(50, result.TextTokens);
        Assert.Equal(30, result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle null values in usage fields",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_null_values_in_usage_fields()
    {
        var raw = JsonDocument
            .Parse("""{"prompt_tokens":null,"completion_tokens":null,"cached_tokens":null}""")
            .RootElement;

        var result = MoonshotChatUsage.ConvertMoonshotAIChatUsage(raw);

        Assert.Equal(0, result.InputTokens);
        Assert.Equal(0, result.NoCacheInputTokens);
        Assert.Equal(0, result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(0, result.OutputTokens);
        Assert.Equal(0, result.TextTokens);
        Assert.Equal(0, result.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), result.Raw!.Value.GetRawText());
    }
}
