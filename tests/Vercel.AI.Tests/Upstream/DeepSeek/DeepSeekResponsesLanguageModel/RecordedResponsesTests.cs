// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekResponsesLanguageModel.DeepSeekResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekResponsesLanguageModel;

/// <summary>Port of <c>deepseek-responses-language-model.test.ts</c> &gt; <c>DeepSeekResponsesLanguageModel &gt; recorded responses</c>.</summary>
public sealed class RecordedResponsesTests
{
    private const string Prefix = TestFile + " > recorded responses::";

    [Fact]
    [UpstreamTest(Prefix + "maps a recorded web search response", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Maps_a_recorded_web_search_response()
    {
        var capture = JsonFixture("deepseek-responses-web-search");
        var options = TestPrompt();
        options.Tools = new[] { WebSearchLanguageModelTool() };

        var result = await Model(capture, "deepseek-v4-pro").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(new[] { "reasoning", "tool-call", "tool-result", "text" }, result.Content.Select(part => part.Type));
        var toolResult = Assert.IsType<GeneratedToolResult>(result.Content[2]);
        Assert.Equal("web_search", toolResult.ToolName);
        var value = JsonNode.Parse(toolResult.ResultJson)!;
        Assert.Equal("completed", (string?)value["status"]);
        Assert.Equal("search", (string?)value["action"]!["type"]);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("completed", result.RawFinishReason);
        JsonAssert.Equal(
            JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!["deepseek"]!["serverToolUse"],
            """{"toolSearch":1,"toolFetch":0}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "maps a recorded web search stream", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Maps_a_recorded_web_search_stream()
    {
        var capture = ChunksFixture("deepseek-responses-web-search");
        var options = TestPrompt();
        options.Tools = new[] { WebSearchLanguageModelTool() };

        var result = await Model(capture, "deepseek-v4-pro").DoStreamAsync(options, CancellationToken.None);
        var parts = await UpstreamChat.Read(result.Stream);

        Assert.Equal(
            new[]
            {
                "stream-start", "response-metadata", "reasoning-start", "reasoning-end", "tool-call", "tool-result",
                "reasoning-start", "reasoning-end", "tool-call", "tool-result", "text-start", "text-end", "finish",
            },
            parts.Where(part => !part.Type.EndsWith("-delta", StringComparison.Ordinal)).Select(part => part.Type));
        var results = parts.OfType<ToolResultStreamPart>().Select(part => JsonNode.Parse(part.Result.GetRawText())!).ToList();
        Assert.Equal(2, results.Count);
        Assert.Equal("search", (string?)results[0]["action"]!["type"]);
        Assert.Equal("open_page", (string?)results[1]["action"]!["type"]);
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("completed", finish.RawFinishReason);
        JsonAssert.Equal(
            JsonNode.Parse(finish.ProviderMetadata!.Value.GetRawText())!["deepseek"]!["serverToolUse"],
            """{"toolSearch":1,"toolFetch":1}""");
    }
}
