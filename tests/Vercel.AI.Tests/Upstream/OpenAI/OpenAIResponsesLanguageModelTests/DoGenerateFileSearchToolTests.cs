// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; file search tool</c>.</summary>
public sealed class DoGenerateFileSearchToolTests
{
    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > file search tool::exposes uncited retrieved files and deduplicates multiple passages from one file",
        Coverage = UpstreamCoverage.Covered)]
    public void Exposes_uncited_retrieved_files_and_deduplicates_multiple_passages_from_one_file()
    {
        var fixture = OpenAIResponsesSupport.FixtureBody("openai-file-search-tool.2.json");
        var search = fixture["output"]!.AsArray().First(part => part!["type"]!.GetValue<string>() == "file_search_call")!;
        var results = search["results"]!.AsArray();
        var uncited = results[0]!.DeepClone();
        uncited["file_id"] = "uncited-file";
        uncited["filename"] = "uncited.pdf";
        results.Add(results[0]!.DeepClone());
        results.Add(uncited);

        var result = OpenAIResponsesSupport.Generate(fixture);

        var sources = result.Content.Where(part => part is GeneratedSource || part is GeneratedDocumentSource).ToList();
        Assert.Equal(2, sources.Count);
        Assert.Contains(sources, source => source is GeneratedDocumentSource { Filename: "uncited.pdf" });
        Assert.Single(result.Content.OfType<GeneratedText>().SelectMany(part => part.Citations ?? Array.Empty<Citation>()));
    }
}
