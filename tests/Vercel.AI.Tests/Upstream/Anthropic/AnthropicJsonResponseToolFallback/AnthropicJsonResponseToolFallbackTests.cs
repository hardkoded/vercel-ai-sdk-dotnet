// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicJsonResponseToolFallback.JsonResponseToolFallbackSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicJsonResponseToolFallback;

public sealed class AnthropicJsonResponseToolFallbackTests
{
    public static TheoryData<string> Citations =>
        new()
        {
            "{\"type\":\"page_location\",\"cited_text\":\"Supporting passage\",\"document_index\":0,\"document_title\":\"Report\",\"start_page_number\":1,\"end_page_number\":2}",
            "{\"type\":\"web_search_result_location\",\"cited_text\":\"Supporting passage\",\"title\":\"Report\",\"url\":\"https://example.com/report\",\"encrypted_index\":\"opaque\"}",
        };

    [Theory]
    [MemberData(nameof(Citations))]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-json-response-tool-fallback.test.ts::JSON response tool fallback::preserves $type citations on held-back text with generation parity",
        Coverage = UpstreamCoverage.Covered,
        Commit = "e10aebbad661c641a5463c645272168d03a5c2f2"
    )]
    public async Task Preserves_type_citations_on_held_back_text_with_generation_parity(
        string citationJson
    )
    {
        var citation = JsonNode.Parse(citationJson)!;
        var call = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage([
                    new FileDataContentPart("base64PDFdata", "application/pdf", "report.pdf")
                    {
                        ProviderOptions = new Dictionary<string, System.Text.Json.JsonElement>
                        {
                            ["anthropic"] = System
                                .Text.Json.JsonDocument.Parse("{\"citations\":{\"enabled\":true}}")
                                .RootElement.Clone(),
                        },
                    },
                ]),
            },
            JsonSchema = System.Text.Json.JsonDocument.Parse(CircularSchema).RootElement.Clone(),
        };

        var (_, generated) = await Generate(
            "claude-sonnet-5-5",
            call,
            MessageResponse(Text("{\"root\":{}}", citation.DeepClone()))
        );

        var parts = await Stream(
            "claude-sonnet-5-5",
            call,
            MessageEvents([
                new JsonObject
                {
                    ["type"] = "content_block_start",
                    ["index"] = 0,
                    ["content_block"] = new JsonObject { ["type"] = "text", ["text"] = "" },
                },
                Delta(0, new JsonObject { ["type"] = "text_delta", ["text"] = "{\"root\":{}}" }),
                Delta(
                    0,
                    new JsonObject
                    {
                        ["type"] = "citations_delta",
                        ["citation"] = citation.DeepClone(),
                    }
                ),
                Stop(0),
            ])
        );

        var end = Assert.Single(parts.OfType<TextEndStreamPart>());
        var text = Assert.Single(generated.Content.OfType<GeneratedText>());
        Assert.Single(text.Citations!);
        Assert.Equal(
            System.Text.Json.JsonSerializer.Serialize(text.Citations),
            System.Text.Json.JsonSerializer.Serialize(end.Citations)
        );
        var metadata = JsonNode.Parse(end.ProviderMetadata!.Value.GetRawText())!;
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"anthropic\":{\"citations\":[" + citationJson + "]}}"),
                metadata
            )
        );
    }
}
