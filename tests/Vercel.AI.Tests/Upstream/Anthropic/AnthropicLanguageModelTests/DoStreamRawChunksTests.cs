// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream &gt; raw chunks</c>.</summary>
public sealed class DoStreamRawChunksTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream > raw chunks::";

    [Fact]
    [UpstreamTest(
        Prefix + "should include raw chunks when includeRawChunks is enabled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_raw_chunks_when_includeRawChunks_is_enabled()
    {
        var options = AnthropicParity.Hello();
        options.IncludeRawChunks = true;
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    """{"type":"message_start","message":{"id":"msg_01KfpJoAEabmH2iHRRFjQMAG","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":17,"output_tokens":1}}}""",
                    """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello"}}""",
                    """{"type":"content_block_stop","index":0}""",
                    """{"type":"message_delta","delta":{"stop_reason":"end_turn","stop_sequence":null},"usage":{"output_tokens":227}}""",
                    """{"type":"message_stop"}"""
                )
            ),
            "claude-3-haiku-20240307",
            options
        );

        DoStreamSupport.AssertStreamSnapshot(
            parts.Where(part => part is RawStreamPart).ToList(),
            "raw-chunks-enabled"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not include raw chunks when includeRawChunks is false",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_include_raw_chunks_when_includeRawChunks_is_false()
    {
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    """{"type":"message_start","message":{"id":"msg_01KfpJoAEabmH2iHRRFjQMAG","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":17,"output_tokens":1}}}""",
                    """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello"}}""",
                    """{"type":"content_block_stop","index":0}""",
                    """{"type":"message_delta","delta":{"stop_reason":"end_turn","stop_sequence":null},"usage":{"output_tokens":227}}""",
                    """{"type":"message_stop"}"""
                )
            ),
            "claude-3-haiku-20240307"
        );

        Assert.Empty(parts.OfType<RawStreamPart>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should process PDF citation responses in streaming",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_process_PDF_citation_responses_in_streaming()
    {
        // Create a model with predictable ID generation for testing
        var provider = DoStreamSupport.Provider(
            // Mock streaming response with PDF citations
            DoStreamSupport.Stream(
                """{"type":"message_start","message":{"id":"msg_01KfpJoAEabmH2iHRRFjQMAG","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":17,"output_tokens":1}}}""",
                """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}""",
                """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Based on the document"}}""",
                """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":", results show growth."}}""",
                """{"type":"content_block_delta","index":0,"delta":{"type":"citations_delta","citation":{"type":"page_location","cited_text":"Revenue increased by 25% year over year","document_index":0,"document_title":"Financial Report 2023","start_page_number":5,"end_page_number":6}}}""",
                """{"type":"content_block_stop","index":0}""",
                """{"type":"message_delta","delta":{"stop_reason":"end_turn","stop_sequence":null},"usage":{"output_tokens":227}}""",
                """{"type":"message_stop"}"""
            )
        );
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
                            System.Text.Encoding.UTF8.GetBytes("base64PDFdata"),
                            "financial-report.pdf"
                        )
                        {
                            ProviderOptions = new Dictionary<string, JsonElement>
                            {
                                ["anthropic"] = AnthropicParity.Json(
                                    "{\"citations\":{\"enabled\":true}}"
                                ),
                            },
                        },
                        new TextContentPart("What do the results show?"),
                    }
                ),
            },
        };

        var parts = await DoStreamSupport.Collect(provider, "claude-3-haiku-20240307", options);

        DoStreamSupport.AssertStreamSnapshot(parts, "pdf-citation-stream");
    }
}
