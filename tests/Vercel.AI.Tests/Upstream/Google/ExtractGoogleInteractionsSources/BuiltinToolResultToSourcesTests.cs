// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;

namespace Vercel.AI.Tests.Upstream.Google.ExtractGoogleInteractionsSources;

/// <summary>Port of <c>extract-google-interactions-sources.test.ts</c> &gt; <c>builtinToolResultToSources</c>.</summary>
public sealed class BuiltinToolResultToSourcesTests
{
    private const string File = "packages/google/src/interactions/extract-google-interactions-sources.test.ts::builtinToolResultToSources::";

    private static IReadOnlyList<Vercel.AI.Provider.LanguageModelStreamPart> Map(string block)
    {
        return GoogleInteractionsSources.BuiltinToolResultToSources(JsonNode.Parse(block)!.AsObject(), new IdCounter().Next);
    }

    [Fact]
    [UpstreamTest(File + "maps url_context_result entries with status:success to url sources", Coverage = UpstreamCoverage.Covered)]
    public void Maps_url_context_result_entries_with_status_success_to_url_sources()
    {
        var sources = Map("{\"type\":\"url_context_result\",\"call_id\":\"c1\",\"result\":[{\"url\":\"https://a.com\",\"status\":\"success\"},{\"url\":\"https://b.com\",\"status\":\"error\"},{\"url\":\"https://c.com\"}]}");

        Assert.Equal(new List<string?> { "https://a.com", "https://c.com" }, SourceAssert.Urls(sources));
    }

    [Fact]
    [UpstreamTest(File + "maps google_search_result entries with url to url sources", Coverage = UpstreamCoverage.Covered)]
    public void Maps_google_search_result_entries_with_url_to_url_sources()
    {
        var sources = Map("{\"type\":\"google_search_result\",\"call_id\":\"c1\",\"result\":[{\"url\":\"https://news.example.com/1\",\"title\":\"Article 1\"},{\"search_suggestions\":\"<html>...</html>\"},{\"url\":\"https://news.example.com/2\"}]}");

        SourceAssert.Equal(
            "[{\"type\":\"source\",\"sourceType\":\"url\",\"id\":\"id-1\",\"url\":\"https://news.example.com/1\",\"title\":\"Article 1\"},{\"type\":\"source\",\"sourceType\":\"url\",\"id\":\"id-2\",\"url\":\"https://news.example.com/2\"}]",
            sources);
    }

    [Fact]
    [UpstreamTest(File + "maps google_maps_result places with url to url sources", Coverage = UpstreamCoverage.Covered)]
    public void Maps_google_maps_result_places_with_url_to_url_sources()
    {
        // The second place has no URL, so it is skipped.
        var sources = Map("{\"type\":\"google_maps_result\",\"call_id\":\"c1\",\"result\":[{\"places\":[{\"name\":\"Foo Cafe\",\"url\":\"https://maps.google.com/?q=foo\"},{\"name\":\"Bar Bar\"}]}]}");

        SourceAssert.Equal(
            "[{\"type\":\"source\",\"sourceType\":\"url\",\"id\":\"id-1\",\"url\":\"https://maps.google.com/?q=foo\",\"title\":\"Foo Cafe\"}]",
            sources);
    }

    [Fact]
    [UpstreamTest(File + "maps file_search_result entries to document sources", Coverage = UpstreamCoverage.Covered)]
    public void Maps_file_search_result_entries_to_document_sources()
    {
        var sources = Map("{\"type\":\"file_search_result\",\"call_id\":\"c1\",\"result\":[{\"file_name\":\"report.pdf\",\"source\":\"fileSearchStores/x/report.pdf\"},{\"document_uri\":\"https://storage.example.com/file.txt\"}]}");

        SourceAssert.Equal(
            "[{\"type\":\"source\",\"sourceType\":\"document\",\"id\":\"id-1\",\"mediaType\":\"application/pdf\",\"title\":\"report.pdf\",\"filename\":\"report.pdf\"},{\"type\":\"source\",\"sourceType\":\"url\",\"id\":\"id-2\",\"url\":\"https://storage.example.com/file.txt\"}]",
            sources);
    }

    [Fact]
    [UpstreamTest(File + "returns [] for code_execution_result (no source-able output)", Coverage = UpstreamCoverage.Covered)]
    public void Returns_empty_for_code_execution_result_no_source_able_output()
    {
        Assert.Empty(Map("{\"type\":\"code_execution_result\",\"call_id\":\"c1\",\"result\":\"hello\"}"));
    }
}
