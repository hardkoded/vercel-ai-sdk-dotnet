// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; web search sources schema resilience</c>.</summary>
public sealed class DoGenerateWebSearchSourcesSchemaResilienceTests
{
    private const string Id = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > web search sources schema resilience::";

    private const string Note = "The port emits no tool-call part for a provider-executed web_search_call and reports the tool name web_search, so those parts are not asserted.";

    private static LanguageModelGenerateResult Generate(string id, string webSearchCall, string text)
    {
        var body = JsonNode.Parse(
            $$$"""
            {
              "id": "{{{id}}}",
              "object": "response",
              "created_at": 1741631111,
              "status": "completed",
              "error": null,
              "incomplete_details": null,
              "instructions": null,
              "max_output_tokens": null,
              "model": "gpt-4o",
              "output": [
                {{{webSearchCall}}},
                {"type":"message","id":"msg_done","status":"completed","role":"assistant","content":[{"type":"output_text","text":"{{{text}}}","annotations":[]}]}
              ],
              "usage": {"input_tokens":10,"output_tokens":5,"total_tokens":15},
              "previous_response_id": null,
              "parallel_tool_calls": true,
              "reasoning": {"effort":null,"summary":null},
              "store": true,
              "temperature": 0,
              "text": {"format":{"type":"text"}},
              "tool_choice": "auto",
              "tools": [{"type":"web_search","search_context_size":"medium"}],
              "top_p": 1,
              "truncation": "disabled",
              "user": null,
              "metadata": {}
            }
            """)!;
        return OpenAIResponsesSupport.Generate(body);
    }

    [Fact]
    [UpstreamTest(Id + "should accept api-type sources without throwing", Coverage = UpstreamCoverage.Partial, Note = Note)]
    public void Should_accept_api_type_sources_without_throwing()
    {
        var result = Generate(
            "resp_api_sources",
            """{"type":"web_search_call","id":"ws_api_sources","status":"completed","action":{"type":"search","query":"current price of BTC","sources":[{"type":"url","url":"https://example.com?a=1&utm_source=openai"},{"type":"api","name":"oai-finance"}]}}""",
            "BTC is trading at ...");

        Assert.Equal(3, result.Content.Count);
        var toolResult = Assert.IsType<GeneratedToolResult>(result.Content[0]);
        Assert.Equal("ws_api_sources", toolResult.ToolCallId);
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("""{"action":{"query":"current price of BTC","type":"search"},"sources":[{"type":"url","url":"https://example.com?a=1&utm_source=openai"},{"name":"oai-finance","type":"api"}]}"""),
            JsonNode.Parse(toolResult.ResultJson)));
        var source = Assert.IsType<GeneratedSource>(result.Content[1]);
        Assert.Equal("id-0", source.Id);
        Assert.Equal("https://example.com?a=1&utm_source=openai", source.Url);
        var text = Assert.IsType<OpenAIText>(result.Content[2]);
        Assert.Equal("BTC is trading at ...", text.Text);
        Assert.Equal("msg_done", text.ProviderMetadata.GetProperty("openai").GetProperty("itemId").GetString());
    }

    [Fact]
    [UpstreamTest(Id + "should accept web_search_call without action", Coverage = UpstreamCoverage.Partial, Note = Note)]
    public void Should_accept_web_search_call_without_action()
    {
        var result = Generate(
            "resp_missing_web_search_action",
            """{"type":"web_search_call","id":"ws_missing_action","status":"completed"}""",
            "No action payload was returned.");

        var toolResult = Assert.IsType<GeneratedToolResult>(Assert.Single(result.Content.OfType<GeneratedToolResult>()));
        Assert.Equal("{}", JsonNode.Parse(toolResult.ResultJson)!.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Id + "should forward web_search_call.action.queries to tool-result", Coverage = UpstreamCoverage.Partial, Note = Note)]
    public void Should_forward_web_search_call_action_queries_to_tool_result()
    {
        var result = Generate(
            "resp_queries",
            """{"type":"web_search_call","id":"ws_queries","status":"completed","action":{"type":"search","query":"sf news","queries":["sf news","bay area tech"]}}""",
            "Here is what I found.");

        var toolResult = Assert.IsType<GeneratedToolResult>(Assert.Single(result.Content.OfType<GeneratedToolResult>()));
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("""{"action":{"type":"search","query":"sf news","queries":["sf news","bay area tech"]}}"""),
            JsonNode.Parse(toolResult.ResultJson)));
    }
}
