// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.PrepareGoogleInteractionsTools;

/// <summary>Port of <c>prepare-google-interactions-tools.test.ts</c> &gt; <c>prepareGoogleInteractionsTools</c> &gt; <c>provider-defined tools</c>.</summary>
public sealed class PrepareGoogleInteractionsToolsProviderDefinedToolsTests
{
    private const string Prefix =
        "packages/google/src/interactions/prepare-google-interactions-tools.test.ts::prepareGoogleInteractionsTools > provider-defined tools::";

    public static TheoryData<string, string, string, string> Cases()
    {
        return new TheoryData<string, string, string, string>
        {
            {
                "google.google_search (no args)",
                "google.google_search",
                "{}",
                "{\"type\":\"google_search\"}"
            },
            {
                "google.google_search with searchTypes",
                "google.google_search",
                "{\"searchTypes\":{\"webSearch\":{},\"imageSearch\":{}}}",
                "{\"type\":\"google_search\",\"search_types\":[\"web_search\",\"image_search\"]}"
            },
            {
                "google.code_execution",
                "google.code_execution",
                "{}",
                "{\"type\":\"code_execution\"}"
            },
            { "google.url_context", "google.url_context", "{}", "{\"type\":\"url_context\"}" },
            {
                "google.file_search",
                "google.file_search",
                "{\"fileSearchStoreNames\":[\"fileSearchStores/foo\"],\"topK\":4,\"metadataFilter\":\"a = \\\"b\\\"\"}",
                "{\"type\":\"file_search\",\"file_search_store_names\":[\"fileSearchStores/foo\"],\"top_k\":4,\"metadata_filter\":\"a = \\\"b\\\"\"}"
            },
            {
                "google.google_maps",
                "google.google_maps",
                "{\"latitude\":37.7749,\"longitude\":-122.4194,\"enableWidget\":true}",
                "{\"type\":\"google_maps\",\"latitude\":37.7749,\"longitude\":-122.4194,\"enable_widget\":true}"
            },
            {
                "google.computer_use",
                "google.computer_use",
                "{}",
                "{\"type\":\"computer_use\",\"environment\":\"browser\"}"
            },
            {
                "google.mcp_server",
                "google.mcp_server",
                "{\"name\":\"my-mcp\",\"url\":\"https://example.com/mcp\",\"headers\":{\"Authorization\":\"Bearer x\"},\"allowedTools\":[\"foo\"]}",
                "{\"type\":\"mcp_server\",\"name\":\"my-mcp\",\"url\":\"https://example.com/mcp\",\"headers\":{\"Authorization\":\"Bearer x\"},\"allowed_tools\":[\"foo\"]}"
            },
            {
                "google.retrieval",
                "google.retrieval",
                "{\"retrievalTypes\":[\"vertex_ai_search\"],\"vertexAiSearchConfig\":{\"datastores\":[\"projects/p/locations/l/dataStores/d\"]}}",
                "{\"type\":\"retrieval\",\"retrieval_types\":[\"vertex_ai_search\"],\"vertex_ai_search_config\":{\"datastores\":[\"projects/p/locations/l/dataStores/d\"]}}"
            },
        };
    }

    private static LanguageModelTool Tool(string id, string args)
    {
        return LanguageModelTool.ProviderDefined(
            id,
            id.Split('.')[1],
            JsonDocument.Parse(args).RootElement.Clone()
        );
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(Prefix + "maps ${title}", Coverage = UpstreamCoverage.Covered)]
    public void Maps_title(string title, string id, string args, string expected)
    {
        Assert.NotEmpty(title);

        var result = GoogleInteractionsTools.Prepare([Tool(id, args)]);

        Assert.Empty(result.ToolWarnings);
        Assert.Single(result.Tools!);
        PrepareGoogleInteractionsToolsTests.AssertJson(expected, result.Tools![0]);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(
        Prefix + "omits ${title} when toolChoice is none",
        Coverage = UpstreamCoverage.Covered,
        Commit = "83d6b7da4df4bdf62f2cc17d3a8a5bb4647b1023"
    )]
    public void Omits_title_when_toolChoice_is_none(
        string title,
        string id,
        string args,
        string expected
    )
    {
        Assert.NotEmpty(title);
        Assert.NotEmpty(expected);

        PrepareGoogleInteractionsToolsTests.AssertEmpty(
            GoogleInteractionsTools.Prepare([Tool(id, args)], ToolChoice.None)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "warns and drops unknown google.* provider-defined tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Warns_and_drops_unknown_google_provider_defined_tools()
    {
        var result = GoogleInteractionsTools.Prepare([Tool("google.unknown_tool", "{}")]);

        Assert.Null(result.Tools);
        var warning = Assert.Single(result.ToolWarnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("provider-defined tool google.unknown_tool", warning.Feature);
        Assert.Equal(
            "provider-defined tool google.unknown_tool is not supported by google.interactions; tool dropped.",
            warning.Message
        );
    }
}
