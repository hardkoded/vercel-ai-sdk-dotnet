// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doGenerate &gt; file_search tool</c>.</summary>
public sealed class DoGenerateFileSearchToolTests
{
    private const string Prefix = TestFile + " > doGenerate > file_search tool::";

    [Fact]
    [UpstreamTest(Prefix + "should include file_search tool call and result with providerExecuted true", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_file_search_tool_call_and_result_with_providerExecuted_true()
    {
        var result = await Model(JsonFixture("xai-file-search-tool.1")).DoGenerateAsync(FileSearchOptions(), CancellationToken.None);

        JsonAssert.Equal(ToJson(result.Content), """
            [{"input":"","providerExecuted":true,"toolCallId":"fs_123","toolName":"file_search","type":"tool-call"},
             {"result":{"queries":["AI safety research"],"results":[
                {"fileId":"file_abc123","filename":"ai-safety-paper.pdf","score":0.95,"text":"Recent advances in AI safety research have focused on alignment techniques..."},
                {"fileId":"file_def456","filename":"research-notes.md","score":0.82,"text":"Key findings from the AI safety workshop include recommendations for..."}]},
              "toolCallId":"fs_123","toolName":"file_search","type":"tool-result"},
             {"text":"Based on the documents in your collection, I found relevant information about AI safety research. The main findings indicate that recent advances have focused on alignment techniques and the workshop recommendations emphasize the importance of interpretability.","type":"text"}]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle file_search with null results", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_file_search_with_null_results()
    {
        var capture = JsonResponse("""
            {"id":"resp_123","object":"response","status":"completed","model":"grok-4-fast-non-reasoning","output":[
              {"type":"file_search_call","id":"fs_456","status":"completed","queries":["nonexistent topic"],"results":null},
              {"type":"message","id":"msg_456","status":"completed","role":"assistant","content":[{"type":"output_text","text":"No relevant documents found.","annotations":[]}]}],"usage":{"input_tokens":100,"output_tokens":20}}
            """);

        var result = await Model(capture).DoGenerateAsync(FileSearchOptions(), CancellationToken.None);

        JsonAssert.Equal(ToJson(result.Content), """
            [{"input":"","providerExecuted":true,"toolCallId":"fs_456","toolName":"file_search","type":"tool-call"},
             {"result":{"queries":["nonexistent topic"],"results":null},"toolCallId":"fs_456","toolName":"file_search","type":"tool-result"},
             {"text":"No relevant documents found.","type":"text"}]
            """);
    }

    private static LanguageModelCallOptions FileSearchOptions()
    {
        return WithTools(ProviderTool("xai.file_search", "file_search", """{"vectorStoreIds":["collection_test123"]}"""));
    }
}
