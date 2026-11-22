// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicJsonResponseToolFallback.JsonResponseToolFallbackSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicJsonResponseToolFallback;

public sealed class DoStreamTests
{
    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-json-response-tool-fallback.test.ts::JSON response tool fallback > doStream::emits held-back text when the model answers without calling the JSON response tool",
        Coverage = UpstreamCoverage.Covered,
        Commit = "e10aebbad661c641a5463c645272168d03a5c2f2"
    )]
    public async Task Emits_held_back_text_when_the_model_answers_without_calling_the_JSON_response_tool()
    {
        var parts = await Stream(
            "claude-sonnet-5-5",
            JsonCall(CircularSchema),
            MessageEvents(TextBlockEvents(0, "{\"root\"", ":{}}"))
        );

        var text = parts
            .Where(part => part.Type.StartsWith("text-", StringComparison.Ordinal))
            .ToList();
        Assert.Equal(3, text.Count);
        Assert.Equal("0", Assert.IsType<TextStartStreamPart>(text[0]).Id);
        var delta = Assert.IsType<TextDeltaStreamPart>(text[1]);
        Assert.Equal("0", delta.Id);
        Assert.Equal("{\"root\":{}}", delta.Delta);
        Assert.Equal("0", Assert.IsType<TextEndStreamPart>(text[2]).Id);
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-json-response-tool-fallback.test.ts::JSON response tool fallback > doStream::drops held-back text when the JSON response tool is called",
        Coverage = UpstreamCoverage.Covered,
        Commit = "e10aebbad661c641a5463c645272168d03a5c2f2"
    )]
    public async Task Drops_held_back_text_when_the_JSON_response_tool_is_called()
    {
        var parts = await Stream(
            "claude-sonnet-5-5",
            JsonCall(CircularSchema),
            MessageEvents([
                .. TextBlockEvents(0, "Here is the JSON:"),
                .. JsonToolBlockEvents(1, "{\"root\":{}}"),
            ])
        );

        Assert.Equal("{\"root\":{}}", TextOf(parts));
    }
}
