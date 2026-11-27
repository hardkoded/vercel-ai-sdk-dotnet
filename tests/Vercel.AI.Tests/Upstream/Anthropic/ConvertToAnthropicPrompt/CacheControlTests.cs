// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using static Vercel.AI.Tests.ConvertToAnthropicPrompt.PromptRunner;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>cache control</c>.</summary>
public sealed class CacheControlTests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control::";

    private const string Ephemeral =
        "\"providerOptions\":{\"anthropic\":{\"cacheControl\":{\"type\":\"ephemeral\"}}}";

    [Fact]
    [UpstreamTest(
        Prefix + "should limit cache breakpoints to 4",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_limit_cache_breakpoints_to_4()
    {
        var validator = new AnthropicCacheControlValidator();
        var result = Run(
            "[{\"role\":\"system\",\"content\":\"system 1\","
                + Ephemeral
                + "},{\"role\":\"system\",\"content\":\"system 2\","
                + Ephemeral
                + "},{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"user 1\","
                + Ephemeral
                + "}]},{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"assistant 1\","
                + Ephemeral
                + "}]},{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"user 2 (should be rejected)\","
                + Ephemeral
                + "}]}]",
            cacheControl: validator
        );

        // First 4 should have cache_control
        AssertJson("{\"type\":\"ephemeral\"}", result.System![0]!["cache_control"]);
        AssertJson("{\"type\":\"ephemeral\"}", result.System[1]!["cache_control"]);
        AssertJson(
            "{\"type\":\"ephemeral\"}",
            result.Messages[0]!["content"]![0]!["cache_control"]
        );
        AssertJson(
            "{\"type\":\"ephemeral\"}",
            result.Messages[1]!["content"]![0]!["cache_control"]
        );

        // 5th should be rejected
        Assert.Null(result.Messages[2]!["content"]![0]!["cache_control"]);

        // Should have warning about exceeding limit
        var warning = Assert.Single(validator.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("cacheControl breakpoint limit", warning.Feature);
        Assert.Equal(
            "Maximum 4 cache breakpoints exceeded (found 5). This breakpoint will be ignored.",
            warning.Details
        );
    }
}
