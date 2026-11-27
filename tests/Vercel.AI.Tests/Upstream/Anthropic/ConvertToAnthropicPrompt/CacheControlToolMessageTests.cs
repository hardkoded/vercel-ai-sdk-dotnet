// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.ConvertToAnthropicPrompt.PromptRunner;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>cache control &gt; tool message</c>.</summary>
public sealed class CacheControlToolMessageTests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control > tool message::";

    private const string Ephemeral =
        "\"providerOptions\":{\"anthropic\":{\"cacheControl\":{\"type\":\"ephemeral\"}}}";

    [Fact]
    [UpstreamTest(
        Prefix + "should set cache_control on tool result message part with part cache control",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_set_cache_control_on_tool_result_message_part_with_part_cache_control()
    {
        var result = Run(
            "[{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolName\":\"test\",\"toolCallId\":\"test\",\"output\":{\"type\":\"json\",\"value\":{\"test\":\"test\"}},"
                + Ephemeral
                + "}]}]"
        );

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"tool_result\",\"content\":\"{\\\"test\\\":\\\"test\\\"}\",\"tool_use_id\":\"test\",\"cache_control\":{\"type\":\"ephemeral\"}}]}]",
            result.Messages
        );
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should set cache_control on tool result with output cache control",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_set_cache_control_on_tool_result_with_output_cache_control()
    {
        var result = Run(
            "[{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolName\":\"test\",\"toolCallId\":\"test\",\"output\":{\"type\":\"text\",\"value\":\"test\","
                + Ephemeral
                + "}}]}]"
        );

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"tool_result\",\"content\":\"test\",\"tool_use_id\":\"test\",\"cache_control\":{\"type\":\"ephemeral\"}}]}]",
            result.Messages
        );
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should set cache_control on tool result with content output cache control",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_set_cache_control_on_tool_result_with_content_output_cache_control()
    {
        var result = Run(
            "[{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolName\":\"test\",\"toolCallId\":\"test\",\"output\":{\"type\":\"content\",\"value\":[{\"type\":\"text\",\"text\":\"test\","
                + Ephemeral
                + "}]}}]}]"
        );

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"tool_result\",\"content\":[{\"type\":\"text\",\"text\":\"test\"}],\"tool_use_id\":\"test\",\"cache_control\":{\"type\":\"ephemeral\"}}]}]",
            result.Messages
        );
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should set cache_control on last tool result message part with message cache control",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_set_cache_control_on_last_tool_result_message_part_with_message_cache_control()
    {
        var result = Run(
            "[{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolName\":\"test\",\"toolCallId\":\"part1\",\"output\":{\"type\":\"json\",\"value\":{\"test\":\"part1\"}}},{\"type\":\"tool-result\",\"toolName\":\"test\",\"toolCallId\":\"part2\",\"output\":{\"type\":\"json\",\"value\":{\"test\":\"part2\"}}}],"
                + Ephemeral
                + "}]"
        );

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"tool_result\",\"tool_use_id\":\"part1\",\"content\":\"{\\\"test\\\":\\\"part1\\\"}\"},{\"type\":\"tool_result\",\"tool_use_id\":\"part2\",\"content\":\"{\\\"test\\\":\\\"part2\\\"}\",\"cache_control\":{\"type\":\"ephemeral\"}}]}]",
            result.Messages
        );
        Assert.Empty(result.Betas);
    }
}
