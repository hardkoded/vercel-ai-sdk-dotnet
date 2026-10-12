// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Xai;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools.PrepareResponsesToolsSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools;

/// <summary>Port of <c>xai-responses-prepare-tools.test.ts</c> &gt; <c>prepareResponsesTools &gt; file_search</c>.</summary>
public sealed class PrepareResponsesToolsFileSearchTests
{
    private const string Prefix = TestFile + " > file_search::";

    [Fact]
    [UpstreamTest(Prefix + "should prepare file_search tool with vector store IDs", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_file_search_tool_with_vector_store_IDs()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.file_search", "file_search", """{"vectorStoreIds":["collection_1","collection_2"]}""") }, null);

        JsonAssert.Equal(result.Tools, """[{"type":"file_search","vector_store_ids":["collection_1","collection_2"]}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should prepare file_search tool with max num results", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_file_search_tool_with_max_num_results()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.file_search", "file_search", """{"vectorStoreIds":["collection_1"],"maxNumResults":10}""") }, null);

        JsonAssert.Equal(result.Tools, """[{"max_num_results":10,"type":"file_search","vector_store_ids":["collection_1"]}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle multiple tools including file_search", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_multiple_tools_including_file_search()
    {
        var result = XaiResponsesTools.Prepare(
            new[]
            {
                ProviderTool("xai.web_search", "web_search"),
                ProviderTool("xai.file_search", "file_search", """{"vectorStoreIds":["collection_1"]}"""),
                Function("calculator", "calculate numbers"),
            },
            null);

        Assert.Equal(new[] { "web_search", "file_search", "function" }, Types(result));
    }
}
