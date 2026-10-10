// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doStream &gt; web search tool</c>.</summary>
public sealed class DoStreamWebSearchToolTests
{
    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doStream > web search tool::keeps citation metadata associated with each completed text block",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_citation_metadata_associated_with_each_completed_text_block()
    {
        var first = new JsonObject { ["type"] = "url_citation", ["url"] = "https://example.com/first", ["title"] = "First", ["start_index"] = 0, ["end_index"] = 5 };
        var second = new JsonObject { ["type"] = "url_citation", ["url"] = "https://example.com/second", ["title"] = "Second", ["start_index"] = 0, ["end_index"] = 6 };
        var events = new List<JsonNode>();
        var index = 0;
        foreach (var annotation in new[] { first, second })
        {
            events.Add(new JsonObject
            {
                ["type"] = "response.output_item.added",
                ["output_index"] = index,
                ["item"] = new JsonObject { ["type"] = "message", ["id"] = $"message-{index}", ["role"] = "assistant" },
            });
            events.Add(new JsonObject { ["type"] = "response.output_text.annotation.added", ["annotation"] = annotation.DeepClone() });
            events.Add(new JsonObject
            {
                ["type"] = "response.output_item.done",
                ["output_index"] = index,
                ["item"] = new JsonObject { ["type"] = "message", ["id"] = $"message-{index}" },
            });
            index++;
        }

        var ends = (await OpenAIResponsesSupport.Stream(OpenAIResponsesSupport.Sse(events))).OfType<TextEndStreamPart>().ToList();

        Assert.Equal(2, ends.Count);
        JsonAssert.Equal(ends[0].ProviderMetadata!.Value.GetProperty("openai").GetProperty("annotations"), new JsonArray(first.DeepClone()).ToJsonString());
        JsonAssert.Equal(ends[1].ProviderMetadata!.Value.GetProperty("openai").GetProperty("annotations"), new JsonArray(second.DeepClone()).ToJsonString());
        var firstSource = Assert.IsType<GeneratedSource>(ends[0].Citations![0].Source);
        Assert.Equal("https://example.com/first", firstSource.Url);
        Assert.Equal("First", firstSource.Title);
        var secondSource = Assert.IsType<GeneratedSource>(ends[1].Citations![0].Source);
        Assert.Equal("https://example.com/second", secondSource.Url);
        Assert.Equal("Second", secondSource.Title);
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doStream > web search tool::should stream web search results (sources, tool calls, tool results)",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks the sources, the web_search tool results, and the citations on text-end. The port streams provider tool results without tool-call parts, and does not compare the full event snapshot.")]
    public async Task Should_stream_web_search_results_sources_tool_calls_tool_results()
    {
        var events = OpenAIResponsesSupport.Fixture("openai-web-search-tool.1.chunks.txt")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => JsonNode.Parse(line)!);

        var parts = await OpenAIResponsesSupport.Stream(OpenAIResponsesSupport.Sse(events));

        var sources = parts.OfType<SourceStreamPart>().ToList();
        Assert.Equal(21, sources.Count);
        Assert.DoesNotContain("https://www.wired.com/story/the-big-interview-2025-recap?utm_source=openai", sources.Select(source => source.Url));
        Assert.Equal(6, parts.OfType<ToolResultStreamPart>().Count(result => result.ToolName == "web_search"));
        Assert.Contains(parts.OfType<TextEndStreamPart>(), end => end.Citations is { Count: > 0 });
        Assert.Empty(parts.OfType<ErrorStreamPart>());
    }
}
