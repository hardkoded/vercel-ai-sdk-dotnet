// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream</c>. Other parts of the class sit in the sibling files.</summary>
public sealed partial class DoStreamTests
{
    private const string DoStreamId = AnthropicLanguageModelTestSupport.Prefix + "doStream::";

    private static string Sse(params string[] events)
    {
        return string.Concat(events.Select(@event => $"data: {@event}\n\n"));
    }

    [Theory]
    [InlineData(
        """{"type":"page_location","cited_text":"Supporting passage","document_index":0,"document_title":"Report","start_page_number":1,"end_page_number":2}"""
    )]
    [InlineData(
        """{"type":"char_location","cited_text":"Supporting passage","document_index":0,"document_title":"Report","start_char_index":0,"end_char_index":18}"""
    )]
    [InlineData(
        """{"type":"content_block_location","cited_text":"Supporting passage","document_index":0,"document_title":"Report","start_block_index":0,"end_block_index":1,"file_id":null}"""
    )]
    [InlineData(
        """{"type":"web_search_result_location","cited_text":"Supporting passage","title":"Report","url":"https://example.com/report","encrypted_index":"opaque"}"""
    )]
    [InlineData(
        """{"type":"search_result_location","cited_text":"Supporting passage","title":"Report","source":"https://example.com/report","search_result_index":0,"start_block_index":0,"end_block_index":1}"""
    )]
    [UpstreamTest(
        DoStreamId + "preserves $type citations on streamed text with generation parity",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_type_citations_on_streamed_text_with_generation_parity(
        string citationJson
    )
    {
        var citation = JsonNode.Parse(citationJson)!.AsObject();
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(
                    new UserContentPart[]
                    {
                        new FileContentPart(
                            "application/pdf",
                            null,
                            Encoding.UTF8.GetBytes("base64PDFdata"),
                            "report.pdf"
                        )
                        {
                            ProviderOptions = new Dictionary<string, JsonElement>
                            {
                                ["anthropic"] = AnthropicParity.Json(
                                    """{"citations":{"enabled":true}}"""
                                ),
                            },
                        },
                    }
                ),
            },
        };
        var generated = await DoGenerate(
            JsonValue(
                new JsonObject
                {
                    ["id"] = "msg_citations",
                    ["type"] = "message",
                    ["role"] = "assistant",
                    ["model"] = Haiku,
                    ["content"] = new JsonArray(
                        new JsonObject
                        {
                            ["type"] = "text",
                            ["text"] = "Answer",
                            ["citations"] = new JsonArray(citation.DeepClone()),
                        }
                    ),
                    ["stop_reason"] = "end_turn",
                    ["stop_sequence"] = null,
                    ["usage"] = new JsonObject { ["input_tokens"] = 1, ["output_tokens"] = 1 },
                }.ToJsonString()
            ),
            options
        );

        var events = await DoStream(
            StreamChunks([
                Sse(
                    new JsonObject
                    {
                        ["type"] = "message_start",
                        ["message"] = new JsonObject
                        {
                            ["id"] = "msg_citations",
                            ["model"] = Haiku,
                            ["usage"] = new JsonObject
                            {
                                ["input_tokens"] = 1,
                                ["output_tokens"] = 0,
                            },
                        },
                    }.ToJsonString(),
                    """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Answer"}}""",
                    new JsonObject
                    {
                        ["type"] = "content_block_delta",
                        ["index"] = 0,
                        ["delta"] = new JsonObject
                        {
                            ["type"] = "citations_delta",
                            ["citation"] = citation.DeepClone(),
                        },
                    }.ToJsonString(),
                    """{"type":"content_block_stop","index":0}""",
                    """{"type":"message_delta","delta":{"stop_reason":"end_turn","stop_sequence":null},"usage":{"output_tokens":1}}""",
                    """{"type":"message_stop"}"""
                ),
            ]),
            options
        );

        Assert.Empty(events.OfType<ErrorStreamPart>());
        var end = events.OfType<TextEndStreamPart>().First();
        var text = generated.Content.OfType<GeneratedText>().First();
        AssertJson(CitationsJson(text.Citations!), CitationsJson(end.Citations!));
        Assert.Single(end.Citations!);
        Assert.Equal("Supporting passage", end.Citations![0].CitedText);
        Assert.Equal("Report", SourceTitle(end.Citations[0].Source));
        AssertJson(
            new JsonObject
            {
                ["anthropic"] = new JsonObject
                {
                    ["citations"] = new JsonArray(citation.DeepClone()),
                },
            },
            JsonNode.Parse(end.ProviderMetadata!.Value.GetRawText())
        );
        if ((string?)citation["type"] == "web_search_result_location")
        {
            const string ExpectedMetadata =
                """{"anthropic":{"citedText":"Supporting passage","encryptedIndex":"opaque"}}""";
            AssertJson(
                ExpectedMetadata,
                JsonNode.Parse(
                    events.OfType<SourceStreamPart>().First().ProviderMetadata!.Value.GetRawText()
                )
            );
            AssertJson(
                ExpectedMetadata,
                JsonNode.Parse(
                    generated
                        .Content.OfType<GeneratedSource>()
                        .First()
                        .ProviderMetadata!.Value.GetRawText()
                )
            );
        }
    }

    [Fact]
    [UpstreamTest(DoStreamId + "should stream text deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_text_deltas()
    {
        var handler = StreamChunks([
            """data: {"type":"message_start","message":{"id":"msg_01KfpJoAEabmH2iHRRFjQMAG","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":17,"output_tokens":1}}}"""
                + "\n\n",
            """data: {"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}"""
                + "\n\n",
            """data: {"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello"}}"""
                + "\n\n",
            """data: {"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":", "}}"""
                + "\n\n",
            """data: {"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"World!"}}"""
                + "\n\n",
            """data: {"type":"content_block_stop","index":0}""" + "\n\n",
            """data: {"type":"message_delta","delta":{"stop_reason":"end_turn","stop_sequence":null},"usage":{"output_tokens":227}}"""
                + "\n\n",
            """data: {"type":"message_stop"}""" + "\n\n",
        ]);

        var parts = await DoStream(handler, Options());

        AssertJson(
            """
            [
              {"type":"stream-start","warnings":[]},
              {"id":"msg_01KfpJoAEabmH2iHRRFjQMAG","modelId":"claude-3-haiku-20240307","type":"response-metadata"},
              {"kind":"anthropic.message_start","providerMetadata":{"anthropic":{"id":"msg_01KfpJoAEabmH2iHRRFjQMAG","model":"claude-3-haiku-20240307","usage":{"input_tokens":17,"output_tokens":1}}},"type":"custom"},
              {"id":"0","type":"text-start"},
              {"delta":"Hello","id":"0","type":"text-delta"},
              {"delta":", ","id":"0","type":"text-delta"},
              {"delta":"World!","id":"0","type":"text-delta"},
              {"id":"0","type":"text-end"},
              {
                "finishReason":{"raw":"end_turn","unified":"stop"},
                "providerMetadata":{"anthropic":{"container":null,"contextManagement":null,"iterations":null,"stopSequence":null,"usage":{"input_tokens":17,"output_tokens":227}}},
                "type":"finish",
                "usage":{
                  "inputTokens":{"cacheRead":0,"cacheWrite":0,"noCache":17,"total":17},
                  "outputTokens":{"total":227},
                  "raw":{"input_tokens":17,"output_tokens":227}
                }
              }
            ]
            """,
            StreamJson(parts)
        );
    }

    [Theory]
    [InlineData(2)]
    [InlineData(0)]
    [InlineData(null)]
    [UpstreamTest(
        DoStreamId + "should emit raw message-start usage with output tokens %s by default",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_emit_raw_message_start_usage_with_output_tokens_by_default(
        int? outputTokens
    )
    {
        var initialUsage = new JsonObject { ["input_tokens"] = 10 };
        if (outputTokens != null)
        {
            initialUsage["output_tokens"] = outputTokens;
        }

        initialUsage["cache_read_input_tokens"] = 20;
        initialUsage["cache_creation_input_tokens"] = 30;
        initialUsage["service_tier"] = "standard";
        var handler = StreamChunks([
            Sse(
                new JsonObject
                {
                    ["type"] = "message_start",
                    ["message"] = new JsonObject
                    {
                        ["id"] = "msg_initial_usage",
                        ["model"] = "claude-haiku-4-5",
                        ["usage"] = initialUsage.DeepClone(),
                    },
                }.ToJsonString(),
                """{"type":"message_delta","delta":{"stop_reason":"end_turn","stop_sequence":null},"usage":{"input_tokens":12,"output_tokens":7,"cache_read_input_tokens":22,"cache_creation_input_tokens":32}}""",
                """{"type":"message_stop"}"""
            ),
        ]);

        var parts = await DoStream(handler, Options());

        Assert.Equal(4, parts.Count);
        Assert.IsType<StreamStartStreamPart>(parts[0]);
        AssertJson(
            """{"type":"response-metadata","id":"msg_initial_usage","modelId":"claude-haiku-4-5"}""",
            StreamPartJson(parts[1])
        );
        var start = parts[2];
        var expectedStart = new JsonObject
        {
            ["type"] = "custom",
            ["kind"] = "anthropic.message_start",
            ["providerMetadata"] = new JsonObject
            {
                ["anthropic"] = new JsonObject
                {
                    ["id"] = "msg_initial_usage",
                    ["model"] = "claude-haiku-4-5",
                    ["usage"] = initialUsage.DeepClone(),
                },
            },
        };
        AssertJson(expectedStart, StreamPartJson(start));
        var finish = Assert.IsType<FinishStreamPart>(parts[3]);
        AssertJsonContains(
            JsonNode.Parse(
                """{"type":"finish","usage":{"inputTokens":{"total":66,"noCache":12,"cacheRead":22,"cacheWrite":32},"outputTokens":{"total":7}}}"""
            ),
            StreamPartJson(finish)
        );

        // Terminal deltas must not mutate the initial snapshot.
        AssertJson(expectedStart, StreamPartJson(start));
    }

    private static JsonNode CitationsJson(IReadOnlyList<Citation> citations)
    {
        return new JsonArray(
            citations
                .Select(citation =>
                {
                    var node = new JsonObject
                    {
                        ["startIndex"] = citation.StartIndex,
                        ["endIndex"] = citation.EndIndex,
                        ["citedText"] = citation.CitedText,
                    };
                    switch (citation.Source)
                    {
                        case GeneratedSource source:
                            node["source"] = new JsonObject
                            {
                                ["sourceType"] = "url",
                                ["id"] = source.Id,
                                ["url"] = source.Url,
                                ["title"] = source.Title,
                                ["providerMetadata"] = source.ProviderMetadata is { } metadata
                                    ? JsonNode.Parse(metadata.GetRawText())
                                    : null,
                            };
                            break;
                        case GeneratedDocumentSource document:
                            node["source"] = new JsonObject
                            {
                                ["sourceType"] = "document",
                                ["id"] = document.Id,
                                ["mediaType"] = document.MediaType,
                                ["title"] = document.Title,
                                ["filename"] = document.Filename,
                            };
                            break;
                    }

                    return (JsonNode?)node;
                })
                .ToArray()
        );
    }

    private static string? SourceTitle(GeneratedContent source)
    {
        return source switch
        {
            GeneratedSource url => url.Title,
            GeneratedDocumentSource document => document.Title,
            _ => null,
        };
    }
}
