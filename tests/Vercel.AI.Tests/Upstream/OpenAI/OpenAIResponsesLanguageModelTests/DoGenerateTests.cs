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
}
