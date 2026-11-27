// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; web search tool</c>.</summary>
public sealed partial class DoGenerateWebSearchToolTests
{
    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > web search tool::should handle server-side web search results with citations",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_server_side_web_search_results_with_citations()
    {
        var handler = JsonValue(
            """
            {"type":"message","id":"msg_test","content":[
              {"type":"server_tool_use","id":"tool_1","name":"web_search","input":{"query":"latest AI news"}},
              {"type":"web_search_tool_result","tool_use_id":"tool_1","content":[
                {"type":"web_search_result","url":"https://example.com/ai-news","title":"Latest AI Developments","encrypted_content":"encrypted_content_123","page_age":"January 15, 2025"}
              ]},
              {"type":"text","text":"Based on recent articles, AI continues to advance rapidly."}
            ],"stop_reason":"end_turn","usage":{"input_tokens":10,"output_tokens":20,"server_tool_use":{"web_search_requests":1}}}
            """
        );

        var result = await DoGenerate(
            handler,
            Options(
                tools:
                [
                    ProviderTool(
                        "anthropic.web_search_20250305",
                        "web_search",
                        """{"maxUses":5}"""
                    ),
                ]
            ),
            "claude-3-5-sonnet-latest"
        );

        AssertJson(
            """
            [
              {"input":"{\"query\":\"latest AI news\"}","providerExecuted":true,"toolCallId":"tool_1","toolName":"web_search","type":"tool-call"},
              {"result":[{"encryptedContent":"encrypted_content_123","pageAge":"January 15, 2025","title":"Latest AI Developments","type":"web_search_result","url":"https://example.com/ai-news"}],"toolCallId":"tool_1","toolName":"web_search","type":"tool-result"},
              {"id":"id-0","providerMetadata":{"anthropic":{"pageAge":"January 15, 2025"}},"sourceType":"url","title":"Latest AI Developments","type":"source","url":"https://example.com/ai-news"},
              {"text":"Based on recent articles, AI continues to advance rapidly.","type":"text"}
            ]
            """,
            ContentJson(result.Content)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "doGenerate > web search tool::should handle server-side web search errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_server_side_web_search_errors()
    {
        var handler = JsonValue(
            """
            {"type":"message","id":"msg_test","content":[
              {"type":"web_search_tool_result","tool_use_id":"tool_1","content":{"type":"web_search_tool_result_error","error_code":"max_uses_exceeded"}},
              {"type":"text","text":"I cannot search further due to limits."}
            ],"stop_reason":"end_turn","usage":{"input_tokens":10,"output_tokens":20}}
            """
        );

        var result = await DoGenerate(
            handler,
            Options(
                tools:
                [
                    ProviderTool(
                        "anthropic.web_search_20250305",
                        "web_search",
                        """{"maxUses":1}"""
                    ),
                ]
            )
        );

        AssertJson(
            """
            [
              {"isError":true,"result":{"errorCode":"max_uses_exceeded","type":"web_search_tool_result_error"},"toolCallId":"tool_1","toolName":"web_search","type":"tool-result"},
              {"text":"I cannot search further due to limits.","type":"text"}
            ]
            """,
            ContentJson(result.Content)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "doGenerate > web search tool::should work alongside regular client-side tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_work_alongside_regular_client_side_tools()
    {
        var handler = JsonValue(
            """
            {"type":"message","id":"msg_test","content":[{"type":"text","text":"I can search and calculate."}],"stop_reason":"end_turn","usage":{"input_tokens":10,"output_tokens":20}}
            """
        );

        await DoGenerate(
            handler,
            Options(
                tools:
                [
                    FunctionTool(
                        "calculator",
                        "Calculate math",
                        """{"type":"object","properties":{}}""",
                        """{"anthropic":{"eagerInputStreaming":true}}"""
                    ),
                    ProviderTool(
                        "anthropic.web_search_20250305",
                        "web_search",
                        """{"maxUses":1}"""
                    ),
                ]
            )
        );

        var tools = RequestBody(handler)["tools"]!.AsArray();
        Assert.Equal(2, tools.Count);
        AssertJson(
            """{"name":"calculator","description":"Calculate math","input_schema":{"type":"object","properties":{}},"eager_input_streaming":true}""",
            tools[0]
        );
        AssertJson("""{"type":"web_search_20250305","name":"web_search","max_uses":1}""", tools[1]);
    }
}
