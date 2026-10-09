// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Usage;

public sealed class AddLanguageModelUsageTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/types/usage.test.ts::addLanguageModelUsage::should preserve raw usage when aggregating a single step", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_raw_usage_when_aggregating_a_single_step()
    {
        using var raw = JsonDocument.Parse("""{ "totalTokens": 20 }""");
        var usage = new LanguageModelUsage(10, 5, 15, reasoningTokens: 5, raw: raw.RootElement.Clone());

        var result = LanguageModelUsage.Add(LanguageModelUsage.Null, usage);

        Assert.Equal(10, result.InputTokens);
        Assert.Null(result.NoCacheInputTokens);
        Assert.Null(result.CacheReadTokens);
        Assert.Null(result.CacheWriteTokens);
        Assert.Equal(5, result.OutputTokens);
        Assert.Null(result.TextTokens);
        Assert.Equal(5, result.ReasoningTokens);
        Assert.Equal(15, result.TotalTokens);
        Assert.Equal(20, result.Raw!.Value.GetProperty("totalTokens").GetInt32());
    }
}
