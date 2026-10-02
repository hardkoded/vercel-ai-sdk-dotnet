// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Groq;

namespace Vercel.AI.Tests;

public sealed class GroqUsageTests
{
    [Fact]
    [UpstreamTest("packages/groq/src/convert-groq-usage.test.ts::convertGroqUsage::should return undefined values when usage is null", Coverage = UpstreamCoverage.Covered)]
    public void Null_usage_is_undefined()
    {
        var usage = GroqUsage.Convert(Parse("null"));
        AssertUndefined(usage);
        Assert.Null(usage.Raw);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-groq-usage.test.ts::convertGroqUsage::should return undefined values when usage is undefined", Coverage = UpstreamCoverage.Covered)]
    public void Missing_usage_is_undefined()
    {
        var usage = GroqUsage.Convert(null);
        AssertUndefined(usage);
        Assert.Null(usage.Raw);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-groq-usage.test.ts::convertGroqUsage::should convert basic usage without token details", Coverage = UpstreamCoverage.Covered)]
    public void Basic_usage_leaves_cache_and_reasoning_unset()
    {
        var usage = GroqUsage.Convert(Parse("{\"prompt_tokens\":20,\"completion_tokens\":10}"));
        Assert.Equal(20, usage.InputTotal);
        Assert.Equal(20, usage.NoCache);
        Assert.Null(usage.CacheRead);
        Assert.Null(usage.CacheWrite);
        Assert.Equal(10, usage.OutputTotal);
        Assert.Equal(10, usage.Text);
        Assert.Null(usage.Reasoning);
        Assert.Equal(20, usage.Raw!.Value.GetProperty("prompt_tokens").GetInt32());
        Assert.Equal(10, usage.Raw.Value.GetProperty("completion_tokens").GetInt32());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-groq-usage.test.ts::convertGroqUsage::should extract reasoning tokens from completion_tokens_details", Coverage = UpstreamCoverage.Covered)]
    public void Reasoning_tokens_reduce_text_tokens()
    {
        var usage = GroqUsage.Convert(Parse("{\"prompt_tokens\":79,\"completion_tokens\":40,\"completion_tokens_details\":{\"reasoning_tokens\":21}}"));
        Assert.Equal(79, usage.NoCache);
        Assert.Equal(40, usage.OutputTotal);
        Assert.Equal(19, usage.Text);
        Assert.Equal(21, usage.Reasoning);
        Assert.Null(usage.CacheRead);
        Assert.Null(usage.CacheWrite);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-groq-usage.test.ts::convertGroqUsage::should handle null reasoning_tokens in completion_tokens_details", Coverage = UpstreamCoverage.Covered)]
    public void Null_reasoning_tokens_stay_unset()
    {
        var usage = GroqUsage.Convert(Parse("{\"prompt_tokens\":20,\"completion_tokens\":10,\"completion_tokens_details\":{\"reasoning_tokens\":null}}"));
        Assert.Equal(10, usage.Text);
        Assert.Null(usage.Reasoning);
        Assert.Equal(JsonValueKind.Null, usage.Raw!.Value.GetProperty("completion_tokens_details").GetProperty("reasoning_tokens").ValueKind);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-groq-usage.test.ts::convertGroqUsage::should handle null completion_tokens_details", Coverage = UpstreamCoverage.Covered)]
    public void Null_completion_details_leave_reasoning_unset()
    {
        var usage = GroqUsage.Convert(Parse("{\"prompt_tokens\":20,\"completion_tokens\":10,\"completion_tokens_details\":null}"));
        Assert.Equal(10, usage.Text);
        Assert.Null(usage.Reasoning);
        Assert.Equal(JsonValueKind.Null, usage.Raw!.Value.GetProperty("completion_tokens_details").ValueKind);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-groq-usage.test.ts::convertGroqUsage::should handle zero reasoning tokens", Coverage = UpstreamCoverage.Covered)]
    public void Zero_reasoning_tokens_are_reported()
    {
        var usage = GroqUsage.Convert(Parse("{\"prompt_tokens\":20,\"completion_tokens\":10,\"completion_tokens_details\":{\"reasoning_tokens\":0}}"));
        Assert.Equal(0, usage.Reasoning);
        Assert.Equal(10, usage.Text);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-groq-usage.test.ts::convertGroqUsage::should handle all tokens being reasoning tokens", Coverage = UpstreamCoverage.Covered)]
    public void All_reasoning_tokens_leave_zero_text()
    {
        var usage = GroqUsage.Convert(Parse("{\"prompt_tokens\":20,\"completion_tokens\":50,\"completion_tokens_details\":{\"reasoning_tokens\":50}}"));
        Assert.Equal(50, usage.Reasoning);
        Assert.Equal(0, usage.Text);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-groq-usage.test.ts::convertGroqUsage::clamps text tokens at 0 when reasoning exceeds completion", Coverage = UpstreamCoverage.Covered)]
    public void Text_tokens_clamp_at_zero()
    {
        var usage = GroqUsage.Convert(Parse("{\"prompt_tokens\":951,\"completion_tokens\":6000,\"completion_tokens_details\":{\"reasoning_tokens\":6001}}"));
        Assert.Equal(6000, usage.OutputTotal);
        Assert.Equal(0, usage.Text);
        Assert.Equal(6001, usage.Reasoning);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-groq-usage.test.ts::convertGroqUsage::should map cached_tokens to cacheRead and subtract from noCache", Coverage = UpstreamCoverage.Covered)]
    public void Cached_tokens_reduce_no_cache()
    {
        var usage = GroqUsage.Convert(Parse("{\"prompt_tokens\":4641,\"completion_tokens\":1817,\"prompt_tokens_details\":{\"cached_tokens\":4608}}"));
        Assert.Equal(4641, usage.InputTotal);
        Assert.Equal(33, usage.NoCache);
        Assert.Equal(4608, usage.CacheRead);
        Assert.Null(usage.CacheWrite);
        Assert.Equal(1817, usage.Text);
        Assert.Null(usage.Reasoning);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-groq-usage.test.ts::convertGroqUsage::should treat zero cached_tokens as a cache miss (cacheRead 0)", Coverage = UpstreamCoverage.Covered)]
    public void Zero_cached_tokens_are_a_miss()
    {
        var usage = GroqUsage.Convert(Parse("{\"prompt_tokens\":20,\"completion_tokens\":10,\"prompt_tokens_details\":{\"cached_tokens\":0}}"));
        Assert.Equal(20, usage.InputTotal);
        Assert.Equal(20, usage.NoCache);
        Assert.Equal(0, usage.CacheRead);
        Assert.Null(usage.CacheWrite);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-groq-usage.test.ts::convertGroqUsage::should leave cacheRead undefined when cached_tokens is null", Coverage = UpstreamCoverage.Covered)]
    public void Null_cached_tokens_leave_cache_read_unset()
    {
        var usage = GroqUsage.Convert(Parse("{\"prompt_tokens\":20,\"completion_tokens\":10,\"prompt_tokens_details\":{\"cached_tokens\":null}}"));
        Assert.Equal(20, usage.NoCache);
        Assert.Null(usage.CacheRead);
        Assert.Null(usage.CacheWrite);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-groq-usage.test.ts::convertGroqUsage::should handle missing prompt_tokens and completion_tokens", Coverage = UpstreamCoverage.Covered)]
    public void Missing_token_counts_are_zero()
    {
        var usage = GroqUsage.Convert(Parse("{}"));
        Assert.Equal(0, usage.InputTotal);
        Assert.Equal(0, usage.NoCache);
        Assert.Null(usage.CacheRead);
        Assert.Null(usage.CacheWrite);
        Assert.Equal(0, usage.OutputTotal);
        Assert.Equal(0, usage.Text);
        Assert.Null(usage.Reasoning);
        Assert.Equal(JsonValueKind.Object, usage.Raw!.Value.ValueKind);
    }

    private static void AssertUndefined(GroqUsage usage)
    {
        Assert.Null(usage.InputTotal);
        Assert.Null(usage.NoCache);
        Assert.Null(usage.CacheRead);
        Assert.Null(usage.CacheWrite);
        Assert.Null(usage.OutputTotal);
        Assert.Null(usage.Text);
        Assert.Null(usage.Reasoning);
    }

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
