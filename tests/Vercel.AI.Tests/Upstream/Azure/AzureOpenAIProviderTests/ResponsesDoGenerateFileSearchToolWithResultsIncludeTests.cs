// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;
using static Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests.AzureResponsesToolSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses > doGenerate > file search tool > with results include</c>.</summary>
public sealed class ResponsesDoGenerateFileSearchToolWithResultsIncludeTests
{
    private const string Id =
        "packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > file search tool > with results include::";

    private static async Task<(OpenAICapture Capture, LanguageModelGenerateResult Result)> Run()
    {
        var capture = JsonFixture("openai-file-search-tool.2");
        var result = await Generate(
            capture,
            Call(
                ProviderTool(
                    "openai.file_search",
                    "file_search",
                    "{\"vectorStoreIds\":[\"vs_68caad8bd5d88191ab766cf043d89a18\"],\"maxNumResults\":5,\"filters\":{\"key\":\"author\",\"type\":\"eq\",\"value\":\"Jane Smith\"},\"ranking\":{\"ranker\":\"auto\",\"scoreThreshold\":0.5}}"
                ),
                azureOptions: "{\"include\":[\"file_search_call.results\"]}"
            )
        );
        return (capture, result);
    }

    [Fact]
    [UpstreamTest(Id + "should send request body with tool", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_request_body_with_tool()
    {
        var (capture, _) = await Run();

        var body = RequestBody(capture);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"include\":[\"file_search_call.results\"],\"input\":[{\"content\":[{\"text\":\"Hello\",\"type\":\"input_text\"}],\"role\":\"user\"}],\"model\":\"test-deployment\",\"tools\":[{\"filters\":{\"key\":\"author\",\"type\":\"eq\",\"value\":\"Jane Smith\"},\"max_num_results\":5,\"ranking_options\":{\"ranker\":\"auto\",\"score_threshold\":0.5},\"type\":\"file_search\",\"vector_store_ids\":[\"vs_68caad8bd5d88191ab766cf043d89a18\"]}]}"
                ),
                body
            ),
            body.ToJsonString()
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should include file search tool call and result in content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_file_search_tool_call_and_result_in_content()
    {
        var (_, result) = await Run();

        OpenAIResponsesSupport.AssertContentSnapshot(
            result.Content,
            "azure-file-search-tool.2",
            includeCitations: true,
            SequentialIds()
        );
    }
}
