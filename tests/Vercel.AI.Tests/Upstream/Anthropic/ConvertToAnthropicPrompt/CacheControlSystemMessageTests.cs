// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.ConvertToAnthropicPrompt.PromptRunner;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>cache control &gt; system message</c>.</summary>
public sealed class CacheControlSystemMessageTests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control > system message::";

    [Fact]
    [UpstreamTest(
        Prefix + "should set cache_control on system message with message cache control",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_set_cache_control_on_system_message_with_message_cache_control()
    {
        var result = Run(
            "[{\"role\":\"system\",\"content\":\"system message\",\"providerOptions\":{\"anthropic\":{\"cacheControl\":{\"type\":\"ephemeral\"}}}}]"
        );

        AssertJson("[]", result.Messages);
        AssertJson(
            "[{\"type\":\"text\",\"text\":\"system message\",\"cache_control\":{\"type\":\"ephemeral\"}}]",
            result.System
        );
        Assert.Empty(result.Betas);
    }
}
