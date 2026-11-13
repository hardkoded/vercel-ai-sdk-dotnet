// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using static Vercel.AI.Tests.Upstream.ExtractionProviderFixtures;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIExtraction;

/// <summary>Port of <c>openai-extraction.test.ts</c>.</summary>
public sealed class OpenAIExtractionTests
{
    [Fact]
    [UpstreamTest("packages/openai/src/openai-extraction.test.ts::extracts typed fields with Responses structured output", Coverage = UpstreamCoverage.Covered, Commit = "6d5094711e57f0ccf62fd60270865a74863bbb08")]
    public async Task Extracts_typed_fields_with_Responses_structured_output()
    {
        var capture = new OpenAICapture { ResponseJson = Fixture("openai-extraction.json").ToJsonString() };
        var model = OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test-key", BaseUrl = "https://example.com/v1" }, capture)
            .ExtractionModel("gpt-6.1-sol");

        var result = await model.DoExtractAsync(InvoiceCall());

        Assert.Equal("openai.extraction", model.Provider);
        Assert.Equal("gpt-6.1-sol", model.ModelId);
        AssertInvoiceAnswers(result.Answers);
        Assert.Equal(1, capture.Calls);
        Assert.Equal("https://example.com/v1/responses", capture.Uri);
        var format = JsonNode.Parse(capture.Body)!["text"]!["format"]!;
        Assert.Equal("json_schema", (string?)format["type"]);
        Assert.Equal("extraction", (string?)format["name"]);
        Assert.True((bool)format["strict"]!);
        Assert.Equal(["f0", "f1", "f2", "f3", "f4"], format["schema"]!["required"]!.AsArray().Select(node => (string)node!));
    }
}
