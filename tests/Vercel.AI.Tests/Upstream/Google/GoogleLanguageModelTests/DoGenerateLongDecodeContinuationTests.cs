// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleLanguageModelTests;

/// <summary>Port of <c>google-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>long decode continuation</c> (the test added for grounding metadata across continuation).</summary>
public sealed class DoGenerateLongDecodeContinuationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > long decode continuation::preserves grounding per request (terminal grounding: %s)", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_grounding_per_request(bool terminalGrounding)
    {
        var (first, last) = ReadContinuationGroundingResponses();
        if (terminalGrounding)
        {
            last["candidates"]![0]!["groundingMetadata"] = first["candidates"]![0]!["groundingMetadata"]!.DeepClone();
        }

        var handler = new SequencedHandler().Json(first.ToJsonString()).Json(last.ToJsonString());
        var options = GoogleUpstream.Hello();
        options.MaxOutputTokens = 33_280;
        var provider = GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key" }, handler);

        var result = await provider.LanguageModel("gemini-pro").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(2, handler.Bodies.Count);
        var google = result.ProviderMetadata!.Value.GetProperty("google");
        var lastGrounding = last["candidates"]![0]!["groundingMetadata"];
        var expected = new JsonArray(first["candidates"]![0]!["groundingMetadata"]!.DeepClone(), lastGrounding?.DeepClone());
        GoogleUpstream.JsonEqual(google.GetProperty("groundingMetadataPerRequest"), expected.ToJsonString());
        GoogleUpstream.JsonEqual(google.GetProperty("groundingMetadata"), lastGrounding?.ToJsonString() ?? "null");
        Assert.Equal(33_246, result.Usage.InputTokens);
        Assert.Equal(33_306, result.Usage.OutputTokens);
    }

    internal static (JsonNode First, JsonNode Last) ReadContinuationGroundingResponses()
    {
        var responses = new[] { 1, 2 }
            .Select(index => JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "google-continuation-grounding-" + index + ".json")))!)
            .ToArray();
        return (responses[0], responses[1]);
    }
}
