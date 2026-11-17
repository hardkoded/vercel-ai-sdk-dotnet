// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using ConvertToDeepSeekUsageType = Vercel.AI.DeepSeek.ConvertToDeepSeekUsage;

namespace Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekUsage;

/// <summary>Port of <c>convert-to-deepseek-usage.test.ts</c> &gt; <c>convertDeepSeekUsage</c>.</summary>
public sealed class ConvertDeepSeekUsageTests
{
    private const string Prefix =
        "packages/deepseek/src/chat/convert-to-deepseek-usage.test.ts::convertDeepSeekUsage::";

    [Fact]
    [UpstreamTest(
        Prefix + "uses OpenAI-compatible cached prompt tokens as a fallback",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Uses_OpenAI_compatible_cached_prompt_tokens_as_a_fallback()
    {
        const string usageJson = """
            {
              "prompt_tokens": 100,
              "completion_tokens": 10,
              "prompt_tokens_details": { "cached_tokens": 80 }
            }
            """;
        var usage = UpstreamChat.Json(usageJson);

        var result = ConvertToDeepSeekUsageType.ConvertDeepSeekUsage(usage);

        Assert.Equal(100, result.InputTokens);
        Assert.Equal(20, result.NoCacheInputTokens);
        Assert.Equal(80, result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(10, result.OutputTokens);
        Assert.Equal(10, result.TextTokens);
        Assert.Equal(0, result.ReasoningTokens);
        JsonAssert.Equal(result.Raw!.Value, usageJson);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "prefers native DeepSeek cached prompt tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Prefers_native_DeepSeek_cached_prompt_tokens()
    {
        var usage = UpstreamChat.Json(
            """
            {
              "prompt_tokens": 100,
              "completion_tokens": 10,
              "prompt_cache_hit_tokens": 60,
              "prompt_tokens_details": { "cached_tokens": 80 }
            }
            """
        );

        var result = ConvertToDeepSeekUsageType.ConvertDeepSeekUsage(usage);

        Assert.Equal(100, result.InputTokens);
        Assert.Equal(40, result.NoCacheInputTokens);
        Assert.Equal(60, result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "clamps text tokens at 0 when reasoning exceeds completion",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Clamps_text_tokens_at_0_when_reasoning_exceeds_completion()
    {
        const string usageJson = """
            {
              "prompt_tokens": 951,
              "completion_tokens": 6000,
              "completion_tokens_details": { "reasoning_tokens": 6001 }
            }
            """;
        var usage = UpstreamChat.Json(usageJson);

        var result = ConvertToDeepSeekUsageType.ConvertDeepSeekUsage(usage);

        Assert.Equal(951, result.InputTokens);
        Assert.Equal(951, result.NoCacheInputTokens);
        Assert.Equal(0, result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(6000, result.OutputTokens);
        Assert.Equal(0, result.TextTokens);
        Assert.Equal(6001, result.ReasoningTokens);
        JsonAssert.Equal(result.Raw!.Value, usageJson);
    }
}
