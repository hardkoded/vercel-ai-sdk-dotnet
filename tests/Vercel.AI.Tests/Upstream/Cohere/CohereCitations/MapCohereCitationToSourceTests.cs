// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Cohere;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereCitations;

/// <summary>Port of <c>cohere-citations.test.ts</c> &gt; <c>mapCohereCitationToSource</c>.</summary>
public sealed class MapCohereCitationToSourceTests
{
    private const string UpstreamCommit = "3aad18a4178eada4ef31b4c169933a2dfe0f792f";
    private const string File = "packages/cohere/src/cohere-citations.test.ts::mapCohereCitationToSource::";

    private static string GenerateId() => "generated-id";

    [Fact]
    [UpstreamTest(File + "should map a document citation to a document source", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_map_a_document_citation_to_a_document_source()
    {
        var result = CohereCitation.Parse(Json("{\"start\":1,\"end\":5,\"text\":\"sunny\",\"type\":\"TEXT_CONTENT\",\"sources\":[{\"type\":\"document\",\"id\":\"doc:0\",\"document\":{\"id\":\"doc:0\",\"text\":\"It is sunny.\",\"title\":\"weather.txt\"}}]}"))
            .ToSource(GenerateId);

        AssertSource(
            Assert.IsType<GeneratedDocumentSource>(result),
            "generated-id",
            "weather.txt",
            "{\"cohere\":{\"start\":1,\"end\":5,\"text\":\"sunny\",\"sources\":[{\"type\":\"document\",\"id\":\"doc:0\",\"document\":{\"id\":\"doc:0\",\"text\":\"It is sunny.\",\"title\":\"weather.txt\"}}],\"citationType\":\"TEXT_CONTENT\"}}");
    }

    [Fact]
    [UpstreamTest(File + "should use the first source with a document", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_use_the_first_source_with_a_document()
    {
        var citation = CohereCitation.Parse(Json("{\"sources\":[{\"type\":\"tool\",\"id\":\"getWeather:0\",\"tool_output\":{}},{\"type\":\"document\",\"document\":{\"title\":\"second.txt\"}},{\"type\":\"document\",\"document\":{\"title\":\"third.txt\"}}]}"));

        Assert.Equal("second.txt", citation.ToSource(GenerateId)?.Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("null")]
    [UpstreamTest(File + "should fall back to \"Document\" for an empty title (%s)", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_fall_back_to_Document_for_an_empty_title(string? title)
    {
        // null: the title key is absent (upstream undefined). "null": the title is a JSON null. "": an empty string.
        var document = title == null ? "{}" : title == "null" ? "{\"title\":null}" : "{\"title\":\"\"}";
        var citation = CohereCitation.Parse(Json("{\"sources\":[{\"type\":\"document\",\"document\":" + document + "}]}"));

        Assert.Equal("Document", citation.ToSource(GenerateId)?.Title);
    }

    [Fact]
    [UpstreamTest(File + "should omit absent citation fields from the provider metadata", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_omit_absent_citation_fields_from_the_provider_metadata()
    {
        var result = CohereCitation.Parse(Json("{\"sources\":[{\"document\":{\"text\":\"It is sunny.\"}}]}")).ToSource(GenerateId);

        JsonAssert.Equal(result!.ProviderMetadata!.Value, "{\"cohere\":{\"sources\":[{\"document\":{\"text\":\"It is sunny.\"}}]}}");
    }

    [Fact]
    [UpstreamTest(File + "should not emit a source for tool-only citations", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_not_emit_a_source_for_tool_only_citations()
    {
        var citation = CohereCitation.Parse(Json("{\"start\":1,\"end\":5,\"text\":\"sunny\",\"sources\":[{\"type\":\"tool\",\"id\":\"getWeather:0\",\"tool_output\":{\"sunny\":true}}],\"type\":\"TEXT_CONTENT\"}"));

        Assert.Null(citation.ToSource(GenerateId));
    }

    [Fact]
    [UpstreamTest(File + "should not emit a source for citations without sources", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_not_emit_a_source_for_citations_without_sources()
    {
        Assert.Null(CohereCitation.Parse(Json("{}")).ToSource(GenerateId));
        Assert.Null(CohereCitation.Parse(Json("{\"sources\":null}")).ToSource(GenerateId));
    }
}
