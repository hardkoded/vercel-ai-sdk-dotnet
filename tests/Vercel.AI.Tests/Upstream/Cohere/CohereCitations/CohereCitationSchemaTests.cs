// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Cohere;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereCitations;

/// <summary>Port of <c>cohere-citations.test.ts</c> &gt; <c>cohereCitationSchema</c>.</summary>
public sealed class CohereCitationSchemaTests
{
    private const string UpstreamCommit = "3aad18a4178eada4ef31b4c169933a2dfe0f792f";
    private const string File = "packages/cohere/src/cohere-citations.test.ts::cohereCitationSchema::";

    [Fact]
    [UpstreamTest(File + "should accept a citation without optional fields", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_accept_a_citation_without_optional_fields()
    {
        var citation = CohereCitation.Parse(Json("{}"));

        Assert.Null(citation.Start);
        Assert.Null(citation.End);
        Assert.Null(citation.Text);
        Assert.Null(citation.Sources);
        Assert.Null(citation.Type);
    }

    [Fact]
    [UpstreamTest(File + "should accept a source without a document", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_accept_a_source_without_a_document()
    {
        var citation = CohereCitation.Parse(Json("{\"sources\":[{\"type\":\"tool\",\"id\":\"getWeather:0\",\"tool_output\":{\"sunny\":true}}]}"));

        var source = Assert.Single(citation.Sources!);
        Assert.Equal("tool", source.Type);
        Assert.Equal("getWeather:0", source.Id);
        Assert.Null(source.Document);
        JsonAssert.Equal(source.ToJson(), "{\"type\":\"tool\",\"id\":\"getWeather:0\",\"tool_output\":{\"sunny\":true}}");
    }
}
