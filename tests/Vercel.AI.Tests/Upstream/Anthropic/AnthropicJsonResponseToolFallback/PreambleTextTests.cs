// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicJsonResponseToolFallback.JsonResponseToolFallbackSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicJsonResponseToolFallback;

public sealed class PreambleTextTests
{
    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-json-response-tool-fallback.test.ts::JSON response tool fallback > preamble text::drops text from an earlier message when a later message calls the JSON response tool",
        Coverage = UpstreamCoverage.Covered,
        Commit = "e10aebbad661c641a5463c645272168d03a5c2f2"
    )]
    public async Task Drops_text_from_an_earlier_message_when_a_later_message_calls_the_JSON_response_tool()
    {
        var parts = await Stream(
            "claude-sonnet-5-5",
            JsonCall(CircularSchema),
            MessageEvents(
                [.. TextBlockEvents(0, "I will check first."), .. ToolBlockEvents(1, "lookup")],
                "msg_1",
                "tool_use"
            ),
            MessageEvents(JsonToolBlockEvents(0, "{\"answer\":\"42\"}"), "msg_2", "tool_use")
        );

        Assert.Equal("{\"answer\":\"42\"}", TextOf(parts));
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-json-response-tool-fallback.test.ts::JSON response tool fallback > preamble text::returns only the final message text when the JSON response tool is never called",
        Coverage = UpstreamCoverage.Covered,
        Commit = "e10aebbad661c641a5463c645272168d03a5c2f2"
    )]
    public async Task Returns_only_the_final_message_text_when_the_JSON_response_tool_is_never_called()
    {
        var parts = await Stream(
            "claude-sonnet-5-5",
            JsonCall(CircularSchema),
            MessageEvents(
                [.. TextBlockEvents(0, "I will check first."), .. ToolBlockEvents(1, "lookup")],
                "msg_1",
                "tool_use"
            ),
            MessageEvents(TextBlockEvents(0, "{\"answer\":\"42\"}"), "msg_2")
        );

        Assert.Equal("{\"answer\":\"42\"}", TextOf(parts));
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-json-response-tool-fallback.test.ts::JSON response tool fallback > preamble text::drops text from a paused message before the continuation answers",
        Coverage = UpstreamCoverage.Covered,
        Commit = "e10aebbad661c641a5463c645272168d03a5c2f2"
    )]
    public async Task Drops_text_from_a_paused_message_before_the_continuation_answers()
    {
        var parts = await Stream(
            "claude-sonnet-5-5",
            JsonCall(CircularSchema),
            MessageEvents(TextBlockEvents(0, "Still working."), "msg_1", "pause_turn"),
            MessageEvents(TextBlockEvents(0, "{\"answer\":\"42\"}"), "msg_2")
        );

        Assert.Equal("{\"answer\":\"42\"}", TextOf(parts));
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-json-response-tool-fallback.test.ts::JSON response tool fallback > preamble text::drops text before a tool call within a message (stream)",
        Coverage = UpstreamCoverage.Covered,
        Commit = "e10aebbad661c641a5463c645272168d03a5c2f2"
    )]
    public async Task Drops_text_before_a_tool_call_within_a_message_stream()
    {
        var parts = await Stream(
            "claude-sonnet-5-5",
            JsonCall(CircularSchema),
            MessageEvents([
                .. TextBlockEvents(0, "I will check first."),
                .. ToolBlockEvents(1, "lookup"),
                .. TextBlockEvents(2, "{\"answer\":\"42\"}"),
            ])
        );

        Assert.Equal("{\"answer\":\"42\"}", TextOf(parts));
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-json-response-tool-fallback.test.ts::JSON response tool fallback > preamble text::drops text before a tool call or result (generate)",
        Coverage = UpstreamCoverage.Covered,
        Commit = "e10aebbad661c641a5463c645272168d03a5c2f2"
    )]
    public async Task Drops_text_before_a_tool_call_or_result_generate()
    {
        var (_, result) = await Generate(
            "claude-sonnet-5-5",
            JsonCall(CircularSchema),
            MessageResponse(
                Text("I will check first."),
                new JsonObject
                {
                    ["type"] = "server_tool_use",
                    ["id"] = "srvtoolu_1",
                    ["name"] = "web_search",
                    ["input"] = new JsonObject { ["query"] = "answer" },
                },
                new JsonObject
                {
                    ["type"] = "web_search_tool_result",
                    ["tool_use_id"] = "srvtoolu_1",
                    ["content"] = new JsonArray(),
                },
                Text("{\"answer\":\"42\"}")
            )
        );

        var text = Assert.Single(result.Content.OfType<GeneratedText>());
        Assert.Equal("{\"answer\":\"42\"}", text.Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-json-response-tool-fallback.test.ts::JSON response tool fallback > preamble text::returns no text when the response stops for a tool call (generate)",
        Coverage = UpstreamCoverage.Covered,
        Commit = "e10aebbad661c641a5463c645272168d03a5c2f2"
    )]
    public async Task Returns_no_text_when_the_response_stops_for_a_tool_call_generate()
    {
        var (_, result) = await Generate(
            "claude-sonnet-5-5",
            JsonCall(CircularSchema),
            MessageResponse(Text("I will check first."), ToolUse("toolu_1", "lookup", "{}"))
        );

        Assert.Empty(result.Content.OfType<GeneratedText>());
    }
}
