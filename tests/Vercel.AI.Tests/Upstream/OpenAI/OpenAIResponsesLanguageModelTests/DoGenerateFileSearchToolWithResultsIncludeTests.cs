// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; file search tool &gt; with results include</c>.</summary>
public sealed class DoGenerateFileSearchToolWithResultsIncludeTests
{
    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > file search tool > with results include::should keep retrieved results separate from inline file citations",
        Coverage = UpstreamCoverage.Partial,
        Note = "The Responses model has no provider-defined tools, so the result is named file_search, not the fileSearch tool name the upstream request maps.")]
    public void Should_keep_retrieved_results_separate_from_inline_file_citations()
    {
        var result = OpenAIResponsesSupport.Generate(OpenAIResponsesSupport.FixtureBody("openai-file-search-tool.2.json"));

        var toolResult = Assert.Single(result.Content.OfType<GeneratedToolResult>(), part => part.ToolName == "file_search");
        var textPart = Assert.IsType<OpenAIText>(Assert.Single(result.Content.OfType<GeneratedText>()));
        var annotations = textPart.ProviderMetadata.GetProperty("openai").GetProperty("annotations");

        var searchResults = JsonNode.Parse(toolResult.ResultJson)!["results"]!.AsArray();
        var searchResult = Assert.Single(searchResults)!;
        Assert.Equal("file-Ebzhf8H4DPGPr9pUhr7n7v", searchResult["fileId"]!.GetValue<string>());
        Assert.Equal("ai.pdf", searchResult["filename"]!.GetValue<string>());
        Assert.Equal(0.9311, searchResult["score"]!.GetValue<double>());

        var citation = Assert.Single(textPart.Citations!);
        Assert.Equal("ai.pdf", Assert.IsType<GeneratedDocumentSource>(citation.Source).Filename);

        var source = Assert.IsType<GeneratedDocumentSource>(Assert.Single(result.Content, part => part is GeneratedSource || part is GeneratedDocumentSource));
        Assert.Equal("ai.pdf", source.Filename);
        JsonAssert.Equal(source.ProviderMetadata!.Value, "{\"openai\":{\"type\":\"file_search\",\"fileId\":\"file-Ebzhf8H4DPGPr9pUhr7n7v\"}}");

        var annotation = Assert.Single(annotations.EnumerateArray());
        Assert.Equal("file_citation", annotation.GetProperty("type").GetString());
        Assert.Equal("file-Ebzhf8H4DPGPr9pUhr7n7v", annotation.GetProperty("file_id").GetString());
        Assert.Equal("ai.pdf", annotation.GetProperty("filename").GetString());
    }
}
