// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;

namespace Vercel.AI.Tests.Upstream.Google.ExtractGoogleInteractionsSources;

/// <summary>Port of <c>extract-google-interactions-sources.test.ts</c> &gt; <c>annotationToSource</c>.</summary>
public sealed class AnnotationToSourceTests
{
    private const string File = "packages/google/src/interactions/extract-google-interactions-sources.test.ts::annotationToSource::";

    [Fact]
    [UpstreamTest(File + "maps a url_citation annotation to a url source", Coverage = UpstreamCoverage.Covered)]
    public void Maps_a_url_citation_annotation_to_a_url_source()
    {
        var ids = new IdCounter();

        var source = GoogleInteractionsSources.AnnotationToSource(
            JsonNode.Parse("{\"type\":\"url_citation\",\"url\":\"https://example.com/article\",\"title\":\"Example Article\"}")!.AsObject(),
            ids.Next);

        SourceAssert.Equal("{\"type\":\"source\",\"sourceType\":\"url\",\"id\":\"id-1\",\"url\":\"https://example.com/article\",\"title\":\"Example Article\"}", source);
    }

    [Fact]
    [UpstreamTest(File + "returns undefined for url_citation without a url", Coverage = UpstreamCoverage.Covered)]
    public void Returns_undefined_for_url_citation_without_a_url()
    {
        var source = GoogleInteractionsSources.AnnotationToSource(JsonNode.Parse("{\"type\":\"url_citation\"}")!.AsObject(), new IdCounter().Next);

        Assert.Null(source);
    }

    [Fact]
    [UpstreamTest(File + "maps a file_citation with http document_uri to a url source", Coverage = UpstreamCoverage.Covered)]
    public void Maps_a_file_citation_with_http_document_uri_to_a_url_source()
    {
        var source = GoogleInteractionsSources.AnnotationToSource(
            JsonNode.Parse("{\"type\":\"file_citation\",\"document_uri\":\"https://files.example.com/foo.pdf\",\"file_name\":\"foo.pdf\"}")!.AsObject(),
            new IdCounter().Next);

        SourceAssert.Equal("{\"type\":\"source\",\"sourceType\":\"url\",\"id\":\"id-1\",\"url\":\"https://files.example.com/foo.pdf\",\"title\":\"foo.pdf\"}", source);
    }

    [Fact]
    [UpstreamTest(File + "maps a file_citation with non-http uri to a document source", Coverage = UpstreamCoverage.Covered)]
    public void Maps_a_file_citation_with_non_http_uri_to_a_document_source()
    {
        var source = GoogleInteractionsSources.AnnotationToSource(
            JsonNode.Parse("{\"type\":\"file_citation\",\"document_uri\":\"gs://bucket/path/report.pdf\",\"file_name\":\"report.pdf\"}")!.AsObject(),
            new IdCounter().Next);

        SourceAssert.Equal("{\"type\":\"source\",\"sourceType\":\"document\",\"id\":\"id-1\",\"mediaType\":\"application/pdf\",\"title\":\"report.pdf\",\"filename\":\"report.pdf\"}", source);
    }

    [Fact]
    [UpstreamTest(File + "maps a place_citation to a url source when url is present", Coverage = UpstreamCoverage.Covered)]
    public void Maps_a_place_citation_to_a_url_source_when_url_is_present()
    {
        var source = GoogleInteractionsSources.AnnotationToSource(
            JsonNode.Parse("{\"type\":\"place_citation\",\"url\":\"https://maps.google.com/?q=foo\",\"name\":\"Foo Place\"}")!.AsObject(),
            new IdCounter().Next);

        SourceAssert.Equal("{\"type\":\"source\",\"sourceType\":\"url\",\"id\":\"id-1\",\"url\":\"https://maps.google.com/?q=foo\",\"title\":\"Foo Place\"}", source);
    }

    [Fact]
    [UpstreamTest(File + "returns undefined for unknown annotation types", Coverage = UpstreamCoverage.Covered)]
    public void Returns_undefined_for_unknown_annotation_types()
    {
        var source = GoogleInteractionsSources.AnnotationToSource(JsonNode.Parse("{\"type\":\"unknown_kind\"}")!.AsObject(), new IdCounter().Next);

        Assert.Null(source);
    }
}
