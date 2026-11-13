// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate::should retain citation sources when retrieved sources are unavailable",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_retain_citation_sources_when_retrieved_sources_are_unavailable()
    {
        var result = OpenAIResponsesSupport.Generate(JsonNode.Parse("""
            {
              "id": "resp_123",
              "object": "response",
              "created_at": 1234567890,
              "status": "completed",
              "error": null,
              "incomplete_details": null,
              "input": [],
              "instructions": null,
              "max_output_tokens": null,
              "model": "gpt-4o",
              "output": [
                {
                  "id": "msg_123",
                  "type": "message",
                  "status": "completed",
                  "role": "assistant",
                  "content": [
                    {
                      "type": "output_text",
                      "text": "Based on web search and file content.",
                      "annotations": [
                        { "type": "url_citation", "start_index": 0, "end_index": 10, "url": "https://example.com", "title": "Example URL" },
                        { "type": "file_citation", "file_id": "file-abc123", "filename": "resource1.json", "index": 123 }
                      ]
                    }
                  ]
                }
              ],
              "usage": {
                "input_tokens": 100,
                "input_tokens_details": { "cached_tokens": 0 },
                "output_tokens": 50,
                "output_tokens_details": { "reasoning_tokens": 0 },
                "total_tokens": 150
              }
            }
            """)!);

        Assert.Equal(3, result.Content.Count);
        var text = Assert.IsType<OpenAIText>(result.Content[0]);
        Assert.Equal("Based on web search and file content.", text.Text);
        JsonAssert.Equal(OpenAIResponsesSupport.CitationsJson(text.Citations), """
            [
              {
                "source": { "type": "source", "sourceType": "url", "id": "https://example.com", "url": "https://example.com", "title": "Example URL" },
                "startIndex": 0,
                "endIndex": 10
              },
              {
                "source": {
                  "type": "source",
                  "sourceType": "document",
                  "id": "file-abc123",
                  "mediaType": "text/plain",
                  "title": "resource1.json",
                  "filename": "resource1.json",
                  "providerMetadata": { "openai": { "type": "file_citation", "fileId": "file-abc123", "index": 123 } }
                }
              }
            ]
            """);
        JsonAssert.Equal(text.ProviderMetadata, """
            {
              "openai": {
                "itemId": "msg_123",
                "annotations": [
                  { "type": "url_citation", "start_index": 0, "end_index": 10, "url": "https://example.com", "title": "Example URL" },
                  { "type": "file_citation", "file_id": "file-abc123", "filename": "resource1.json", "index": 123 }
                ]
              }
            }
            """);

        var url = Assert.IsType<GeneratedSource>(result.Content[1]);
        Assert.Equal("id-0", url.Id);
        Assert.Equal("https://example.com", url.Url);
        Assert.Equal("Example URL", url.Title);

        var document = Assert.IsType<GeneratedDocumentSource>(result.Content[2]);
        Assert.Equal("id-1", document.Id);
        Assert.Equal("text/plain", document.MediaType);
        Assert.Equal("resource1.json", document.Title);
        Assert.Equal("resource1.json", document.Filename);
        JsonAssert.Equal(document.ProviderMetadata!.Value, """{ "openai": { "type": "file_citation", "fileId": "file-abc123", "index": 123 } }""");
    }

    private const string RefusalGenerate = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate::";

    private static JsonNode MessageResponse(string id, params string[] items)
    {
        return JsonNode.Parse("{\"id\":\"" + id + "\",\"model\":\"gpt-4o\",\"output\":[" + string.Join(",", items)
            + "],\"usage\":{\"input_tokens\":13,\"output_tokens\":7}}")!;
    }

    [Fact]
    [UpstreamTest(RefusalGenerate + "should surface a refusal content part as text and map the finish reason to content-filter", Coverage = UpstreamCoverage.Covered)]
    public void Should_surface_a_refusal_content_part_as_text_and_map_the_finish_reason_to_content_filter()
    {
        var result = OpenAIResponsesSupport.Generate(OpenAIResponsesSupport.FixtureBody("openai-refusal.1.json"), jsonResponse: true);

        var text = Assert.IsType<OpenAIText>(Assert.Single(result.Content));
        Assert.Equal("I'm sorry, I can't assist with that.", text.Text);
        JsonAssert.Equal(text.ProviderMetadata, """{ "openai": { "itemId": "msg_0390d4b158946a22006aebf39c868487d0aa03f83805e9f8a7" } }""");
        Assert.Null(result.StructuredOutputText);
        Assert.Equal(FinishReason.ContentFilter, result.FinishReason);
        Assert.Null(result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(RefusalGenerate + "should surface output_text and refusal parts from the same message", Coverage = UpstreamCoverage.Covered)]
    public void Should_surface_output_text_and_refusal_parts_from_the_same_message()
    {
        var result = OpenAIResponsesSupport.Generate(MessageResponse(
            "resp_refusal_mixed",
            """{"id":"msg_refusal_mixed","type":"message","status":"completed","role":"assistant","content":[{"type":"output_text","text":"Here is what I can share.","annotations":[]},{"type":"refusal","refusal":"I can't share the rest."}]}"""));

        Assert.Collection(
            result.Content,
            first =>
            {
                var text = Assert.IsType<OpenAIText>(first);
                Assert.Equal("Here is what I can share.", text.Text);
                JsonAssert.Equal(text.ProviderMetadata, """{ "openai": { "itemId": "msg_refusal_mixed" } }""");
            },
            second =>
            {
                var text = Assert.IsType<OpenAIText>(second);
                Assert.Equal("I can't share the rest.", text.Text);
                JsonAssert.Equal(text.ProviderMetadata, """{ "openai": { "itemId": "msg_refusal_mixed" } }""");
            });
        Assert.Equal(FinishReason.ContentFilter, result.FinishReason);
        Assert.Null(result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(RefusalGenerate + "should keep two message items as separate text parts", Coverage = UpstreamCoverage.Covered)]
    public void Should_keep_two_message_items_as_separate_text_parts()
    {
        var result = OpenAIResponsesSupport.Generate(MessageResponse(
            "resp_two_messages",
            """{"id":"msg_first","type":"message","status":"completed","role":"assistant","content":[{"type":"output_text","text":"First message.","annotations":[]}]}""",
            """{"id":"msg_second","type":"message","status":"completed","role":"assistant","content":[{"type":"output_text","text":"Second message.","annotations":[]}]}"""));

        Assert.Collection(
            result.Content,
            first =>
            {
                var text = Assert.IsType<OpenAIText>(first);
                Assert.Equal("First message.", text.Text);
                JsonAssert.Equal(text.ProviderMetadata, """{ "openai": { "itemId": "msg_first" } }""");
            },
            second =>
            {
                var text = Assert.IsType<OpenAIText>(second);
                Assert.Equal("Second message.", text.Text);
                JsonAssert.Equal(text.ProviderMetadata, """{ "openai": { "itemId": "msg_second" } }""");
            });
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Null(result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(RefusalGenerate + "should include a refusal in structuredOutputText for a final answer", Coverage = UpstreamCoverage.Covered)]
    public void Should_include_a_refusal_in_structuredOutputText_for_a_final_answer()
    {
        var result = OpenAIResponsesSupport.Generate(
            MessageResponse(
                "resp_refusal_final",
                """{"id":"msg_refusal_final","type":"message","status":"completed","role":"assistant","phase":"final_answer","content":[{"type":"refusal","refusal":"I'm sorry, but I can't assist with that request."}]}"""),
            jsonResponse: true);

        Assert.Equal("I'm sorry, but I can't assist with that request.", result.StructuredOutputText);
        Assert.Equal(FinishReason.ContentFilter, result.FinishReason);
        Assert.Null(result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(RefusalGenerate + "should prefer tool-calls over content-filter when a refusal is accompanied by a function call", Coverage = UpstreamCoverage.Covered)]
    public void Should_prefer_tool_calls_over_content_filter_when_a_refusal_is_accompanied_by_a_function_call()
    {
        var result = OpenAIResponsesSupport.Generate(
            MessageResponse(
                "resp_refusal_function_call",
                """{"id":"msg_refusal_function_call","type":"message","status":"completed","role":"assistant","content":[{"type":"refusal","refusal":"I can't do that, but I can check the weather."}]}""",
                """{"type":"function_call","id":"fc_1","call_id":"call_1","name":"weather","arguments":"{\"location\":\"Paris\"}","status":"completed"}"""),
            OpenAIResponsesSupport.TestTools);

        Assert.Collection(
            result.Content,
            first => Assert.IsType<OpenAIText>(first),
            second => Assert.IsType<GeneratedToolCall>(second));
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        Assert.Null(result.RawFinishReason);
    }
}
