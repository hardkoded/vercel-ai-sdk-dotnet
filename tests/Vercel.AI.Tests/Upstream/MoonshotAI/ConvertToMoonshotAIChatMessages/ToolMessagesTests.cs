// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.MoonshotAI.ConvertToMoonshotAIChatMessages.MoonshotMessagesSupport;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.ConvertToMoonshotAIChatMessages;

/// <summary>Port of <c>convert-to-moonshotai-chat-messages.test.ts</c> &gt; <c>tool messages</c>.</summary>
public sealed class ToolMessagesTests
{
    private const string Suite = Prefix + "tool messages::";

    [Fact]
    [UpstreamTest(
        Suite + "should convert text tool results to tool messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_text_tool_results_to_tool_messages()
    {
        var result = ConvertMessages(
            new ToolModelMessage("call-1", "get_weather", "sunny", false, "text")
        );

        Equal(result, "[{\"role\":\"tool\",\"tool_call_id\":\"call-1\",\"content\":\"sunny\"}]");
    }

    [Fact]
    [UpstreamTest(
        Suite + "should stringify json tool results",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_stringify_json_tool_results()
    {
        var result = ConvertMessages(
            new ToolModelMessage("call-1", "get_weather", "{\"temp\":20}", false, "json")
        );

        Equal(result, """[{"role":"tool","tool_call_id":"call-1","content":"{\"temp\":20}"}]""");
    }
}
