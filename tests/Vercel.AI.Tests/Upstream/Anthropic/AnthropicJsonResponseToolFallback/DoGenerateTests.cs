// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicJsonResponseToolFallback.JsonResponseToolFallbackSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicJsonResponseToolFallback;

public sealed class DoGenerateTests
{
    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-json-response-tool-fallback.test.ts::JSON response tool fallback > doGenerate::returns text when the model answers without calling the JSON response tool",
        Coverage = UpstreamCoverage.Covered,
        Commit = "e10aebbad661c641a5463c645272168d03a5c2f2"
    )]
    public async Task Returns_text_when_the_model_answers_without_calling_the_JSON_response_tool()
    {
        var (_, result) = await Generate(
            "claude-sonnet-5-5",
            JsonCall(CircularSchema),
            MessageResponse(Text("{\"root\":{}}"))
        );

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("{\"root\":{}}", text.Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-json-response-tool-fallback.test.ts::JSON response tool fallback > doGenerate::drops other text when the JSON response tool is called",
        Coverage = UpstreamCoverage.Covered,
        Commit = "e10aebbad661c641a5463c645272168d03a5c2f2"
    )]
    public async Task Drops_other_text_when_the_JSON_response_tool_is_called()
    {
        var (_, result) = await Generate(
            "claude-sonnet-5-5",
            JsonCall(CircularSchema),
            MessageResponse(Text("Here is the JSON:"), ToolUse("toolu_1", "json", "{\"root\":{}}"))
        );

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("{\"root\":{}}", text.Text);
    }
}
