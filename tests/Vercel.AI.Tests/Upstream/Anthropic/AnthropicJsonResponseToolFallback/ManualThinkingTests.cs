// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicJsonResponseToolFallback.JsonResponseToolFallbackSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicJsonResponseToolFallback;

public sealed class ManualThinkingTests
{
    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-json-response-tool-fallback.test.ts::JSON response tool fallback > manual thinking::uses the JSON response tool with auto tool choice",
        Coverage = UpstreamCoverage.Covered,
        Commit = "e10aebbad661c641a5463c645272168d03a5c2f2"
    )]
    public async Task Uses_the_JSON_response_tool_with_auto_tool_choice()
    {
        var (body, _) = await Generate(
            "claude-sonnet-4-5",
            ManualThinkingCall(),
            MessageResponse(Text("{\"answer\":\"42\"}"))
        );

        Assert.Equal("enabled", body["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal(5000, body["thinking"]!["budget_tokens"]!.GetValue<int>());
        Assert.Equal(
            ["json"],
            body["tools"]!.AsArray().Select(tool => tool!["name"]!.GetValue<string>())
        );
        Assert.Equal("auto", body["tool_choice"]!["type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-json-response-tool-fallback.test.ts::JSON response tool fallback > manual thinking::returns text when the model answers without calling the JSON response tool (generate)",
        Coverage = UpstreamCoverage.Covered,
        Commit = "e10aebbad661c641a5463c645272168d03a5c2f2"
    )]
    public async Task Returns_text_when_the_model_answers_without_calling_the_JSON_response_tool_generate()
    {
        var (_, result) = await Generate(
            "claude-sonnet-4-5",
            ManualThinkingCall(),
            MessageResponse(
                new JsonObject
                {
                    ["type"] = "thinking",
                    ["thinking"] = "Working it out.",
                    ["signature"] = "sig",
                },
                Text("{\"answer\":\"42\"}")
            )
        );

        Assert.Equal(["reasoning", "text"], result.Content.Select(part => part.Type));
        Assert.Equal("{\"answer\":\"42\"}", Assert.IsType<GeneratedText>(result.Content[1]).Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-json-response-tool-fallback.test.ts::JSON response tool fallback > manual thinking::emits held-back text after reasoning when the model answers without calling the JSON response tool (stream)",
        Coverage = UpstreamCoverage.Covered,
        Commit = "e10aebbad661c641a5463c645272168d03a5c2f2"
    )]
    public async Task Emits_held_back_text_after_reasoning_when_the_model_answers_without_calling_the_JSON_response_tool_stream()
    {
        var parts = await Stream(
            "claude-sonnet-4-5",
            ManualThinkingCall(),
            MessageEvents([
                .. ThinkingBlockEvents(0, "Working it out."),
                .. TextBlockEvents(1, "{\"answer\"", ":\"42\"}"),
            ])
        );

        Assert.Equal(
            [
                "reasoning-start",
                "reasoning-delta",
                "reasoning-delta",
                "reasoning-end",
                "text-start",
                "text-delta",
                "text-end",
            ],
            parts
                .Where(part =>
                    part.Type.StartsWith("reasoning-", StringComparison.Ordinal)
                    || part.Type.StartsWith("text-", StringComparison.Ordinal)
                )
                .Select(part => part.Type)
        );
        var delta = Assert.Single(parts.OfType<TextDeltaStreamPart>());
        Assert.Equal("1", delta.Id);
        Assert.Equal("{\"answer\":\"42\"}", delta.Delta);
        Assert.Equal(FinishReason.Stop, Assert.IsType<FinishStreamPart>(parts[^1]).FinishReason);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-json-response-tool-fallback.test.ts::JSON response tool fallback > manual thinking::drops held-back text when the JSON response tool is called after thinking (stream)",
        Coverage = UpstreamCoverage.Covered,
        Commit = "e10aebbad661c641a5463c645272168d03a5c2f2"
    )]
    public async Task Drops_held_back_text_when_the_JSON_response_tool_is_called_after_thinking_stream()
    {
        var parts = await Stream(
            "claude-sonnet-4-5",
            ManualThinkingCall(),
            MessageEvents([
                .. ThinkingBlockEvents(0, "Working it out."),
                .. TextBlockEvents(1, "Here is the JSON:"),
                .. JsonToolBlockEvents(2, "{\"answer\":\"42\"}"),
            ])
        );

        Assert.Equal("{\"answer\":\"42\"}", TextOf(parts));
    }
}
