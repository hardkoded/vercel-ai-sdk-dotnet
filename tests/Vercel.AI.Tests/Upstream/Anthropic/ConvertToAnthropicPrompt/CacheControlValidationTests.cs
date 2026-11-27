// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using static Vercel.AI.Tests.ConvertToAnthropicPrompt.PromptRunner;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>cache control &gt; cache control validation</c>.</summary>
public sealed class CacheControlValidationTests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control > cache control validation::";

    [Fact]
    [UpstreamTest(
        Prefix + "should reject cache_control on thinking blocks",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_cache_control_on_thinking_blocks()
    {
        var validator = new AnthropicCacheControlValidator();

        var result = Run(
            "[{\"role\":\"assistant\",\"content\":[{\"type\":\"reasoning\",\"text\":\"thinking content\",\"providerOptions\":{\"anthropic\":{\"signature\":\"test-sig\",\"cacheControl\":{\"type\":\"ephemeral\"}}}}]}]",
            cacheControl: validator
        );

        AssertJson(
            "[{\"role\":\"assistant\",\"content\":[{\"type\":\"thinking\",\"thinking\":\"thinking content\",\"signature\":\"test-sig\"}]}]",
            result.Messages
        );
        Assert.Empty(result.Betas);
        var warning = Assert.Single(validator.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("cache_control on non-cacheable context", warning.Feature);
        Assert.Equal(
            "cache_control cannot be set on thinking block. It will be ignored.",
            warning.Details
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject cache_control on redacted thinking blocks",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_cache_control_on_redacted_thinking_blocks()
    {
        var validator = new AnthropicCacheControlValidator();

        var result = Run(
            "[{\"role\":\"assistant\",\"content\":[{\"type\":\"reasoning\",\"text\":\"redacted\",\"providerOptions\":{\"anthropic\":{\"redactedData\":\"abc123\",\"cacheControl\":{\"type\":\"ephemeral\"}}}}]}]",
            cacheControl: validator
        );

        Assert.False(
            ((JsonObject)result.Messages[0]!["content"]![0]!).ContainsKey("cache_control")
        );
        var warning = Assert.Single(validator.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("cache_control on non-cacheable context", warning.Feature);
        Assert.Equal(
            "cache_control cannot be set on redacted thinking block. It will be ignored.",
            warning.Details
        );
    }
}
