// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doStream</c>.</summary>
public sealed class DoStreamTests
{
    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doStream::preserves repeated citation ranges and versioned fragment URLs with Unicode text in generation and streaming",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_repeated_citation_ranges_and_versioned_fragment_URLs_with_Unicode_text_in_generation_and_streaming()
    {
        const string text = "🧪 café — 東京 café";
        const string url = "https://example.com/article?oldid=42#:~:text=caf%C3%A9";
        JsonArray Annotations() => new()
        {
            new JsonObject { ["type"] = "url_citation", ["url"] = url, ["title"] = "Café", ["start_index"] = 2, ["end_index"] = 6 },
            new JsonObject { ["type"] = "url_citation", ["url"] = url, ["title"] = "Café", ["start_index"] = 12, ["end_index"] = 16 },
        };

        var generated = OpenAIResponsesSupport.Generate(new JsonObject
        {
            ["id"] = "response",
            ["model"] = "gpt-5-nano",
            ["output"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = "message",
                    ["type"] = "message",
                    ["role"] = "assistant",
                    ["content"] = new JsonArray
                    {
                        new JsonObject { ["type"] = "output_text", ["text"] = text, ["annotations"] = Annotations() },
                    },
                },
            },
            ["usage"] = new JsonObject { ["input_tokens"] = 1, ["output_tokens"] = 1 },
        });
        var generatedText = Assert.IsType<OpenAIText>(Assert.Single(generated.Content.OfType<GeneratedText>()));
        var expectedCitations = $$"""
            [
              { "source": { "type": "source", "sourceType": "url", "id": "{{url}}", "url": "{{url}}", "title": "Café" }, "startIndex": 2, "endIndex": 6 },
              { "source": { "type": "source", "sourceType": "url", "id": "{{url}}", "url": "{{url}}", "title": "Café" }, "startIndex": 12, "endIndex": 16 }
            ]
            """;
        Assert.Equal(text, generatedText.Text);
        JsonAssert.Equal(OpenAIResponsesSupport.CitationsJson(generatedText.Citations), expectedCitations);

        var events = new List<JsonNode>
        {
            new JsonObject
            {
                ["type"] = "response.output_item.added",
                ["output_index"] = 0,
                ["item"] = new JsonObject { ["type"] = "message", ["id"] = "message", ["role"] = "assistant" },
            },
            new JsonObject { ["type"] = "response.output_text.delta", ["item_id"] = "message", ["delta"] = text },
        };
        foreach (var annotation in Annotations())
        {
            events.Add(new JsonObject { ["type"] = "response.output_text.annotation.added", ["annotation"] = annotation!.DeepClone() });
        }

        events.Add(new JsonObject
        {
            ["type"] = "response.output_item.done",
            ["output_index"] = 0,
            ["item"] = new JsonObject { ["type"] = "message", ["id"] = "message" },
        });

        var parts = await OpenAIResponsesSupport.Stream(OpenAIResponsesSupport.Sse(events));

        Assert.Empty(parts.OfType<ErrorStreamPart>());
        Assert.Equal(text, parts.OfType<TextDeltaStreamPart>().First().Delta);
        var end = parts.OfType<TextEndStreamPart>().First();
        JsonAssert.Equal(OpenAIResponsesSupport.CitationsJson(end.Citations), expectedCitations);
        var annotations = end.ProviderMetadata!.Value.GetProperty("openai").GetProperty("annotations");
        JsonAssert.Equal(annotations, Annotations().ToJsonString());
    }
}
