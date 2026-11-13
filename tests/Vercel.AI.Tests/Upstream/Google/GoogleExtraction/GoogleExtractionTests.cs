// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Tests.Upstream.Google.GoogleDecision;
using static Vercel.AI.Tests.Upstream.ExtractionProviderFixtures;

namespace Vercel.AI.Tests.Upstream.Google.GoogleExtraction;

/// <summary>Port of <c>google-extraction.test.ts</c>.</summary>
public sealed class GoogleExtractionTests
{
    [Fact]
    [UpstreamTest("packages/google/src/google-extraction.test.ts::extracts typed fields with Gemini structured output", Coverage = UpstreamCoverage.Covered, Commit = "6d5094711e57f0ccf62fd60270865a74863bbb08")]
    public async Task Extracts_typed_fields_with_Gemini_structured_output()
    {
        var capture = new GoogleDecisionCapture { ResponseJson = Fixture("google-extraction.json").ToJsonString() };
        var model = GoogleProvider.Create(new GoogleOptions { ApiKey = "test-key", BaseUrl = "https://example.com/v1beta" }, capture)
            .ExtractionModel("gemini-3.8-flash");

        var result = await model.DoExtractAsync(InvoiceCall());

        Assert.Equal("google.extraction", model.Provider);
        Assert.Equal("gemini-3.8-flash", model.ModelId);
        AssertInvoiceAnswers(result.Answers);
        Assert.Equal(1, capture.Calls);
        Assert.Equal("https://example.com/v1beta/models/gemini-3.8-flash:generateContent", capture.Uri);
        var generationConfig = JsonNode.Parse(capture.Body)!["generationConfig"]!;
        Assert.Equal("application/json", (string?)generationConfig["responseMimeType"]);
        Assert.Equal(["f0", "f1", "f2", "f3", "f4"], generationConfig["responseJsonSchema"]!["required"]!.AsArray().Select(node => (string)node!));
    }
}
