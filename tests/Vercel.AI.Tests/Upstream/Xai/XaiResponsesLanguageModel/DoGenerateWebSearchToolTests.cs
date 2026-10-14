// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doGenerate &gt; web_search tool</c>.</summary>
public sealed class DoGenerateWebSearchToolTests
{
    private const string Prefix = TestFile + " > doGenerate > web_search tool::";

    [Fact]
    [UpstreamTest(Prefix + "should include web_search tool call with providerExecuted true", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_web_search_tool_call_with_providerExecuted_true()
    {
        var result = await Model(JsonFixture("xai-web-search-tool.1")).DoGenerateAsync(WithTools(ProviderTool("xai.web_search", "web_search")), CancellationToken.None);

        JsonAssert.Equal(ToJson(result.Content), Expected("xai-web-search-tool.1.content.json").ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should send web_search tool with args in request", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_web_search_tool_with_args_in_request()
    {
        var capture = EmptyResponse();
        var options = WithTools(ProviderTool(
            "xai.web_search",
            "web_search",
            """{"allowedDomains":["wikipedia.org"],"enableImageSearch":true,"enableImageUnderstanding":true}"""));

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(Body(capture), """
            {"input":[{"content":[{"text":"hello","type":"input_text"}],"role":"user"}],"model":"grok-4-fast-non-reasoning",
             "tools":[{"allowed_domains":["wikipedia.org"],"enable_image_search":true,"enable_image_understanding":true,"type":"web_search"}]}
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward web_search_call action and sources to tool-result", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_web_search_call_action_and_sources_to_tool_result()
    {
        var capture = JsonResponse("""
            {"id":"resp_123","object":"response","status":"completed","model":"grok-4-fast-non-reasoning","output":[
              {"type":"web_search_call","id":"ws_action_1","name":"web_search","call_id":"","status":"completed",
               "action":{"type":"search","query":"latest AI news","sources":[{"type":"url","url":"https://example.com/a"},{"type":"url","url":"https://example.com/b"}]}},
              {"type":"web_search_call","id":"ws_action_2","name":"web_search","call_id":"","status":"completed","action":{"type":"open_page","url":"https://example.com/a"}},
              {"type":"web_search_call","id":"ws_action_3","name":"web_search","call_id":"","status":"completed","action":{"type":"find_in_page","url":"https://example.com/a","pattern":"climate"}},
              {"type":"web_search_call","id":"ws_action_4","name":"web_search","call_id":"","status":"completed","action":{"type":"search","query":null,"sources":null}},
              {"type":"web_search_call","id":"ws_action_5","name":"web_search","call_id":"","status":"completed","action":{"type":"open_page","url":null}}],"usage":{"input_tokens":10,"output_tokens":5}}
            """);

        var result = await Model(capture).DoGenerateAsync(WithTools(ProviderTool("xai.web_search", "web_search")), CancellationToken.None);

        var content = ToJson(result.Content);
        ContainsEqual(content, """
            {"type":"tool-result","toolCallId":"ws_action_1","toolName":"web_search",
             "result":{"action":{"type":"search","query":"latest AI news"},"sources":[{"type":"url","url":"https://example.com/a"},{"type":"url","url":"https://example.com/b"}]}}
            """);
        ContainsEqual(content, """{"type":"tool-result","toolCallId":"ws_action_2","toolName":"web_search","result":{"action":{"type":"openPage","url":"https://example.com/a"}}}""");
        ContainsEqual(content, """
            {"type":"tool-result","toolCallId":"ws_action_3","toolName":"web_search","result":{"action":{"type":"findInPage","url":"https://example.com/a","pattern":"climate"}}}
            """);
        ContainsEqual(content, """{"type":"tool-result","toolCallId":"ws_action_4","toolName":"web_search","result":{"action":{"type":"search"}}}""");
        ContainsEqual(content, """{"type":"tool-result","toolCallId":"ws_action_5","toolName":"web_search","result":{"action":{"type":"openPage","url":null}}}""");
    }
}
