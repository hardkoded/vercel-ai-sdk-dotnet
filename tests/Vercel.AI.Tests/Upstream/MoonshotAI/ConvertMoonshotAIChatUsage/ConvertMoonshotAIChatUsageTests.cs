// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Moonshot;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.ConvertMoonshotAIChatUsage;

/// <summary>Port of <c>convert-moonshotai-chat-usage.test.ts</c> &gt; <c>convertMoonshotAIChatUsage</c>.</summary>
public sealed class ConvertMoonshotAIChatUsageTests
{
    private const string Prefix =
        "packages/moonshotai/src/convert-moonshotai-chat-usage.test.ts::convertMoonshotAIChatUsage::";

    private static void Check(
        string json,
        int input,
        int noCache,
        int cacheRead,
        int? cacheWrite,
        int output,
        int text,
        int reasoning
    )
    {
        using var doc = JsonDocument.Parse(json);
        var result = MoonshotChatUsage.ConvertMoonshotAIChatUsage(doc.RootElement);

        Assert.Equal(input, result.InputTokens);
        Assert.Equal(noCache, result.NoCacheInputTokens);
        Assert.Equal(cacheRead, result.CacheReadTokens);
        Assert.Equal(cacheWrite, result.CacheWriteTokens);
        Assert.Equal(output, result.OutputTokens);
        Assert.Equal(text, result.TextTokens);
        Assert.Equal(reasoning, result.ReasoningTokens);
        JsonAssert.Equal(result.Raw!.Value, json);
    }

    private static void AssertEmpty(LanguageModelUsage result)
    {
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
    [UpstreamTest(Prefix + "should handle null usage", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_null_usage()
    {
        AssertEmpty(
            MoonshotChatUsage.ConvertMoonshotAIChatUsage(JsonDocument.Parse("null").RootElement)
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle undefined usage", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_undefined_usage()
    {
        AssertEmpty(MoonshotChatUsage.ConvertMoonshotAIChatUsage(null));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert basic usage without caching or reasoning",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_basic_usage_without_caching_or_reasoning()
    {
        Check("{\"prompt_tokens\":100,\"completion_tokens\":50}", 100, 100, 0, null, 50, 50, 0);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert usage with top-level cached_tokens (Moonshot format)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_usage_with_top_level_cached_tokens_moonshot_format()
    {
        Check(
            "{\"prompt_tokens\":100,\"completion_tokens\":50,\"cached_tokens\":30}",
            100,
            70,
            30,
            null,
            50,
            50,
            0
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert usage with nested cached_tokens (OpenAI format)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_usage_with_nested_cached_tokens_openai_format()
    {
        Check(
            "{\"prompt_tokens\":100,\"completion_tokens\":50,\"prompt_tokens_details\":{\"cached_tokens\":25}}",
            100,
            75,
            25,
            null,
            50,
            50,
            0
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prioritize top-level cached_tokens over nested",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_prioritize_top_level_cached_tokens_over_nested()
    {
        Check(
            "{\"prompt_tokens\":100,\"completion_tokens\":50,\"cached_tokens\":40,\"prompt_tokens_details\":{\"cached_tokens\":25}}",
            100,
            60,
            40,
            null,
            50,
            50,
            0
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert usage with reasoning tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_usage_with_reasoning_tokens()
    {
        Check(
            "{\"prompt_tokens\":100,\"completion_tokens\":80,\"completion_tokens_details\":{\"reasoning_tokens\":30}}",
            100,
            100,
            0,
            null,
            80,
            50,
            30
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "clamps text tokens at 0 when reasoning exceeds completion",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Clamps_text_tokens_at_0_when_reasoning_exceeds_completion()
    {
        Check(
            "{\"prompt_tokens\":951,\"completion_tokens\":6000,\"completion_tokens_details\":{\"reasoning_tokens\":6001}}",
            951,
            951,
            0,
            null,
            6000,
            0,
            6001
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert usage with both cached and reasoning tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_usage_with_both_cached_and_reasoning_tokens()
    {
        Check(
            "{\"prompt_tokens\":100,\"completion_tokens\":80,\"cached_tokens\":35,\"completion_tokens_details\":{\"reasoning_tokens\":30}}",
            100,
            65,
            35,
            null,
            80,
            50,
            30
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle null values in usage fields",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_null_values_in_usage_fields()
    {
        Check(
            "{\"prompt_tokens\":null,\"completion_tokens\":null,\"cached_tokens\":null}",
            0,
            0,
            0,
            null,
            0,
            0,
            0
        );
    }
}
