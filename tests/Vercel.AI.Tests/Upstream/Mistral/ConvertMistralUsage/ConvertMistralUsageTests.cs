// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Mistral;
using Vercel.AI.Provider;
using UsageConverter = Vercel.AI.Mistral.ConvertMistralUsage;

namespace Vercel.AI.Tests.Upstream.Mistral.ConvertMistralUsage;

/// <summary>Port of <c>convert-mistral-usage.test.ts</c> &gt; <c>convertMistralUsage</c>.</summary>
public sealed class ConvertMistralUsageTests
{
    private const string Prefix =
        "packages/mistral/src/convert-mistral-usage.test.ts::convertMistralUsage::";

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    private static void AssertUsage(
        LanguageModelUsage usage,
        int total,
        int noCache,
        int? cacheRead,
        int output,
        JsonElement raw
    )
    {
        Assert.Equal(total, usage.InputTokens);
        Assert.Equal(noCache, usage.NoCacheInputTokens);
        Assert.Equal(cacheRead, usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(output, usage.OutputTokens);
        Assert.Equal(output, usage.TextTokens);
        Assert.Null(usage.ReasoningTokens);
        Assert.Equal(raw.GetRawText(), usage.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return undefined values when usage is null",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_undefined_values_when_usage_is_null()
    {
        var usage = UsageConverter.Convert(null);

        Assert.Null(usage.InputTokens);
        Assert.Null(usage.NoCacheInputTokens);
        Assert.Null(usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Null(usage.OutputTokens);
        Assert.Null(usage.TextTokens);
        Assert.Null(usage.ReasoningTokens);
        Assert.Null(usage.Raw);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map basic usage without cached tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_map_basic_usage_without_cached_tokens()
    {
        var raw = Json("{\"prompt_tokens\":100,\"completion_tokens\":50,\"total_tokens\":150}");

        AssertUsage(UsageConverter.Convert(raw), 100, 100, null, 50, raw);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map cached tokens from num_cached_tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_map_cached_tokens_from_num_cached_tokens()
    {
        var raw = Json(
            "{\"prompt_tokens\":100,\"completion_tokens\":50,\"total_tokens\":150,\"num_cached_tokens\":60}"
        );

        AssertUsage(UsageConverter.Convert(raw), 100, 40, 60, 50, raw);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map cached tokens from prompt_tokens_details.cached_tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_map_cached_tokens_from_prompt_tokens_details_cached_tokens()
    {
        var raw = Json(
            "{\"prompt_tokens\":200,\"completion_tokens\":30,\"total_tokens\":230,\"prompt_tokens_details\":{\"cached_tokens\":80}}"
        );

        AssertUsage(UsageConverter.Convert(raw), 200, 120, 80, 30, raw);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map cached tokens from prompt_token_details.cached_tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_map_cached_tokens_from_prompt_token_details_cached_tokens()
    {
        var raw = Json(
            "{\"prompt_tokens\":150,\"completion_tokens\":25,\"total_tokens\":175,\"prompt_token_details\":{\"cached_tokens\":50}}"
        );

        AssertUsage(UsageConverter.Convert(raw), 150, 100, 50, 25, raw);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer num_cached_tokens over prompt_tokens_details",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_prefer_num_cached_tokens_over_prompt_tokens_details()
    {
        var raw = Json(
            "{\"prompt_tokens\":100,\"completion_tokens\":20,\"total_tokens\":120,\"num_cached_tokens\":40,\"prompt_tokens_details\":{\"cached_tokens\":30}}"
        );

        AssertUsage(UsageConverter.Convert(raw), 100, 60, 40, 20, raw);
    }
}
