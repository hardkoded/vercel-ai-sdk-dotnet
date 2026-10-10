// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; mixed citation types</c>.</summary>
public sealed class MixedCitationTypesTests
{
    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > mixed citation types::should retain streamed citation sources when retrieved sources are unavailable",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_retain_streamed_citation_sources_when_retrieved_sources_are_unavailable()
    {
        var annotations = """
            [
              { "type": "url_citation", "start_index": 0, "end_index": 10, "url": "https://example.com", "title": "Example URL" },
              { "type": "file_citation", "index": 123, "file_id": "file-abc123", "filename": "resource1.json" }
            ]
            """;
        var events = new List<JsonNode>
        {
            JsonNode.Parse("""{"type":"response.content_part.added","item_id":"msg_123","output_index":0,"content_index":0,"part":{"type":"output_text","text":"","annotations":[]}}""")!,
            JsonNode.Parse("""{"type":"response.output_text.annotation.added","item_id":"msg_123","output_index":0,"content_index":0,"annotation_index":0,"annotation":{"type":"url_citation","url":"https://example.com","title":"Example URL","start_index":123,"end_index":234}}""")!,
            JsonNode.Parse("""{"type":"response.output_text.annotation.added","item_id":"msg_123","output_index":0,"content_index":0,"annotation_index":1,"annotation":{"type":"file_citation","index":123,"file_id":"file-abc123","filename":"resource1.json"}}""")!,
            JsonNode.Parse("{\"type\":\"response.content_part.done\",\"item_id\":\"msg_123\",\"output_index\":0,\"content_index\":0,\"part\":{\"type\":\"output_text\",\"text\":\"Based on web search and file content.\",\"annotations\":" + annotations + "}}")!,
            JsonNode.Parse("{\"type\":\"response.output_item.done\",\"output_index\":0,\"item\":{\"id\":\"msg_123\",\"type\":\"message\",\"status\":\"completed\",\"role\":\"assistant\",\"content\":[{\"type\":\"output_text\",\"text\":\"Based on web search and file content.\",\"annotations\":" + annotations + "}]}}")!,
            JsonNode.Parse("""{"type":"response.completed","response":{"id":"resp_123","object":"response","created_at":1234567890,"status":"completed","error":null,"incomplete_details":null,"model":"gpt-4o","output":[],"usage":{"input_tokens":100,"input_tokens_details":{"cached_tokens":0},"output_tokens":50,"output_tokens_details":{"reasoning_tokens":0},"total_tokens":150}}}""")!,
        };

        var parts = await OpenAIResponsesSupport.Stream(OpenAIResponsesSupport.Sse(events));

        var kinds = parts.Select(part => part.GetType().Name).ToList();
        Assert.Equal(
            new[]
            {
                nameof(StreamStartStreamPart),
                nameof(TextEndStreamPart),
                nameof(SourceStreamPart),
                nameof(DocumentSourceStreamPart),
                nameof(FinishStreamPart),
            },
            kinds);

        var end = Assert.IsType<TextEndStreamPart>(parts[1]);
        Assert.Equal("msg_123", end.Id);
        JsonAssert.Equal(OpenAIResponsesSupport.CitationsJson(end.Citations), """
            [
              {
                "source": { "type": "source", "sourceType": "url", "id": "https://example.com", "url": "https://example.com", "title": "Example URL" },
                "startIndex": 123,
                "endIndex": 234
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
        JsonAssert.Equal(end.ProviderMetadata!.Value, """
            {
              "openai": {
                "itemId": "msg_123",
                "annotations": [
                  { "type": "url_citation", "start_index": 123, "end_index": 234, "url": "https://example.com", "title": "Example URL" },
                  { "type": "file_citation", "index": 123, "file_id": "file-abc123", "filename": "resource1.json" }
                ]
              }
            }
            """);

        var url = Assert.IsType<SourceStreamPart>(parts[2]);
        Assert.Equal("https://example.com", url.Url);
        Assert.Equal("Example URL", url.Title);
        var document = Assert.IsType<DocumentSourceStreamPart>(parts[3]);
        Assert.Equal("text/plain", document.MediaType);
        Assert.Equal("resource1.json", document.Title);
        Assert.Equal("resource1.json", document.Filename);
        JsonAssert.Equal(document.ProviderMetadata!.Value, """{ "openai": { "type": "file_citation", "fileId": "file-abc123", "index": 123 } }""");

        var finish = Assert.IsType<FinishStreamPart>(parts[4]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal(100, finish.Usage.InputTokens);
        Assert.Equal(50, finish.Usage.OutputTokens);
    }
}
