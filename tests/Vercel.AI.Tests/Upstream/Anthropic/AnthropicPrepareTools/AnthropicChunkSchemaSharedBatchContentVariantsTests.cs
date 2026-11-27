// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Anthropic;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

public sealed class AnthropicChunkSchemaSharedBatchContentVariantsTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-prepare-tools.test.ts::anthropicChunkSchema - shared batch content variants::";

    [Fact]
    [UpstreamTest(
        Prefix + "accepts a string MCP tool result in a streaming content block",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Accepts_a_string_MCP_tool_result_in_a_streaming_content_block()
    {
        using var document = JsonDocument.Parse(
            "{\"content_block\":{\"content\":\"tool output\",\"is_error\":false,\"tool_use_id\":\"mcp_123\",\"type\":\"mcp_tool_result\"},\"index\":0,\"type\":\"content_block_start\"}"
        );

        Assert.True(AnthropicResponseSchema.IsValidChunk(document.RootElement));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "accepts opaque MCP tool result citations",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Accepts_opaque_MCP_tool_result_citations()
    {
        using var document = JsonDocument.Parse(
            "{\"content_block\":{\"content\":[{\"citations\":[{\"reference\":\"opaque-reference\",\"type\":\"future_citation_variant\"}],\"text\":\"tool output\",\"type\":\"text\"}],\"is_error\":false,\"tool_use_id\":\"mcp_123\",\"type\":\"mcp_tool_result\"},\"index\":0,\"type\":\"content_block_start\"}"
        );

        Assert.True(AnthropicResponseSchema.IsValidChunk(document.RootElement));
    }

    [Theory]
    [InlineData(
        "{\"cited_text\":\"block\",\"document_index\":0,\"document_title\":null,\"end_block_index\":2,\"file_id\":null,\"start_block_index\":1,\"type\":\"content_block_location\"}"
    )]
    [InlineData(
        "{\"cited_text\":\"result\",\"end_block_index\":2,\"search_result_index\":0,\"source\":\"https://example.com\",\"start_block_index\":1,\"title\":null,\"type\":\"search_result_location\"}"
    )]
    [UpstreamTest(Prefix + "accepts $type citation deltas", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_type_citation_deltas(string citation)
    {
        using var document = JsonDocument.Parse(
            "{\"delta\":{\"citation\":"
                + citation
                + ",\"type\":\"citations_delta\"},\"index\":0,\"type\":\"content_block_delta\"}"
        );

        Assert.True(AnthropicResponseSchema.IsValidChunk(document.RootElement));
    }
}
