// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;

namespace Vercel.AI.Tests.Upstream.Google.ExtractGoogleInteractionsSources;

/// <summary>Port of <c>extract-google-interactions-sources.test.ts</c> &gt; <c>annotationsToSources</c>.</summary>
public sealed class AnnotationsToSourcesTests
{
    private const string File = "packages/google/src/interactions/extract-google-interactions-sources.test.ts::annotationsToSources::";

    [Fact]
    [UpstreamTest(File + "returns [] for null/undefined annotations", Coverage = UpstreamCoverage.Covered)]
    public void Returns_empty_for_null_undefined_annotations()
    {
        // The port has no undefined: a missing annotations field reads as null.
        Assert.Empty(GoogleInteractionsSources.AnnotationsToSources(null, new IdCounter().Next));
        Assert.Empty(GoogleInteractionsSources.AnnotationsToSources(null, new IdCounter().Next));
    }

    [Fact]
    [UpstreamTest(File + "de-duplicates url citations across multiple annotations", Coverage = UpstreamCoverage.Covered)]
    public void De_duplicates_url_citations_across_multiple_annotations()
    {
        var sources = GoogleInteractionsSources.AnnotationsToSources(
            JsonNode.Parse("[{\"type\":\"url_citation\",\"url\":\"https://a.example.com\",\"title\":\"A\"},{\"type\":\"url_citation\",\"url\":\"https://a.example.com\",\"title\":\"A\"},{\"type\":\"url_citation\",\"url\":\"https://b.example.com\",\"title\":\"B\"}]")!.AsArray(),
            new IdCounter().Next);

        Assert.Equal(new List<string?> { "https://a.example.com", "https://b.example.com" }, SourceAssert.Urls(sources));
    }
}
