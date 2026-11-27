// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.ConvertToAnthropicPrompt.PromptRunner;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>cache control &gt; user message</c>.</summary>
public sealed class CacheControlUserMessageTests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control > user message::";

    [Fact]
    [UpstreamTest(
        Prefix + "should set cache_control on user message part with part cache control",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_set_cache_control_on_user_message_part_with_part_cache_control()
    {
        var result = Run(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"test\",\"providerOptions\":{\"anthropic\":{\"cacheControl\":{\"type\":\"ephemeral\"}}}}]}]"
        );

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"test\",\"cache_control\":{\"type\":\"ephemeral\"}}]}]",
            result.Messages
        );
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should set cache_control on last user message part with message cache control",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_set_cache_control_on_last_user_message_part_with_message_cache_control()
    {
        var result = Run(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"part1\"},{\"type\":\"text\",\"text\":\"part2\"}],\"providerOptions\":{\"anthropic\":{\"cacheControl\":{\"type\":\"ephemeral\"}}}}]"
        );

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"part1\"},{\"type\":\"text\",\"text\":\"part2\",\"cache_control\":{\"type\":\"ephemeral\"}}]}]",
            result.Messages
        );
        Assert.Empty(result.Betas);
    }
}
