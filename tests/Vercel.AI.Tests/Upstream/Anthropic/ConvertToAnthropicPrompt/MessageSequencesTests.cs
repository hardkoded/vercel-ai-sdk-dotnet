// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.ConvertToAnthropicPrompt.PromptRunner;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>citations &gt; message sequences</c>.</summary>
public sealed class MessageSequencesTests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::citations > message sequences::";

    [Fact]
    [UpstreamTest(
        Prefix
            + "should convert user-assistant-tool-assistant-user message sequence with multiple tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_user_assistant_tool_assistant_user_message_sequence_with_multiple_tool_calls()
    {
        var result = Run(
            "["
                + "{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"weather for berlin, london and paris\"}]},"
                + "{\"role\":\"assistant\",\"content\":["
                + "{\"type\":\"text\",\"text\":\"I will use the weather tool to get the weather for berlin, london and paris\"},"
                + "{\"type\":\"tool-call\",\"toolName\":\"weather\",\"toolCallId\":\"weather-call-1\",\"input\":{\"location\":\"berlin\"}},"
                + "{\"type\":\"tool-call\",\"toolName\":\"weather\",\"toolCallId\":\"weather-call-2\",\"input\":{\"location\":\"london\"}},"
                + "{\"type\":\"tool-call\",\"toolName\":\"weather\",\"toolCallId\":\"weather-call-3\",\"input\":{\"location\":\"paris\"}}]},"
                + "{\"role\":\"tool\",\"content\":["
                + "{\"type\":\"tool-result\",\"toolName\":\"weather\",\"toolCallId\":\"weather-call-1\",\"output\":{\"type\":\"json\",\"value\":{\"weather\":\"sunny\"}}},"
                + "{\"type\":\"tool-result\",\"toolName\":\"weather\",\"toolCallId\":\"weather-call-2\",\"output\":{\"type\":\"json\",\"value\":{\"weather\":\"cloudy\"}}},"
                + "{\"type\":\"tool-result\",\"toolName\":\"weather\",\"toolCallId\":\"weather-call-3\",\"output\":{\"type\":\"json\",\"value\":{\"weather\":\"rainy\"}}}]},"
                + "{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"The weather for berlin is sunny, the weather for london is cloudy, and the weather for paris is rainy\"}]},"
                + "{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"and for new york?\"}]}]"
        );

        AssertJson(
            "["
                + "{\"content\":[{\"text\":\"weather for berlin, london and paris\",\"type\":\"text\"}],\"role\":\"user\"},"
                + "{\"content\":["
                + "{\"text\":\"I will use the weather tool to get the weather for berlin, london and paris\",\"type\":\"text\"},"
                + "{\"id\":\"weather-call-1\",\"input\":{\"location\":\"berlin\"},\"name\":\"weather\",\"type\":\"tool_use\"},"
                + "{\"id\":\"weather-call-2\",\"input\":{\"location\":\"london\"},\"name\":\"weather\",\"type\":\"tool_use\"},"
                + "{\"id\":\"weather-call-3\",\"input\":{\"location\":\"paris\"},\"name\":\"weather\",\"type\":\"tool_use\"}],\"role\":\"assistant\"},"
                + "{\"content\":["
                + "{\"content\":\"{\\\"weather\\\":\\\"sunny\\\"}\",\"tool_use_id\":\"weather-call-1\",\"type\":\"tool_result\"},"
                + "{\"content\":\"{\\\"weather\\\":\\\"cloudy\\\"}\",\"tool_use_id\":\"weather-call-2\",\"type\":\"tool_result\"},"
                + "{\"content\":\"{\\\"weather\\\":\\\"rainy\\\"}\",\"tool_use_id\":\"weather-call-3\",\"type\":\"tool_result\"}],\"role\":\"user\"},"
                + "{\"content\":[{\"text\":\"The weather for berlin is sunny, the weather for london is cloudy, and the weather for paris is rainy\",\"type\":\"text\"}],\"role\":\"assistant\"},"
                + "{\"content\":[{\"text\":\"and for new york?\",\"type\":\"text\"}],\"role\":\"user\"}]",
            result.Messages
        );
        Assert.Null(result.System);
    }
}
