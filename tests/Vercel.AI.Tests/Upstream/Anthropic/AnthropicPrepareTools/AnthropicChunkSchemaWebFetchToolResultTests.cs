// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Anthropic;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

public sealed class AnthropicChunkSchemaWebFetchToolResultTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-prepare-tools.test.ts::anthropicChunkSchema - web_fetch_tool_result::";

    [Fact]
    [UpstreamTest(
        Prefix + "should accept base64 PDF source in streaming response",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_accept_base64_PDF_source_in_streaming_response()
    {
        using var document = JsonDocument.Parse(
            "{\"type\":\"content_block_start\",\"index\":10,\"content_block\":{\"type\":\"web_fetch_tool_result\",\"tool_use_id\":\"srvtoolu_01234567890abcdef\",\"content\":{\"type\":\"web_fetch_result\",\"url\":\"https://test.com\",\"retrieved_at\":\"2025-12-08T20:46:31.114158\",\"content\":{\"type\":\"document\",\"title\":null,\"source\":{\"type\":\"base64\",\"media_type\":\"application/pdf\",\"data\":\"JVBERi0xLjcNJeLjz9MNC\"}}}}}"
        );

        Assert.True(AnthropicResponseSchema.IsValidChunk(document.RootElement));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept text source in streaming response",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_accept_text_source_in_streaming_response()
    {
        using var document = JsonDocument.Parse(
            "{\"type\":\"content_block_start\",\"index\":10,\"content_block\":{\"type\":\"web_fetch_tool_result\",\"tool_use_id\":\"srvtoolu_01234567890abcdef\",\"content\":{\"type\":\"web_fetch_result\",\"url\":\"https://test.com\",\"retrieved_at\":\"2025-12-08T20:46:31.114158\",\"content\":{\"type\":\"document\",\"title\":null,\"source\":{\"type\":\"text\",\"media_type\":\"text/plain\",\"data\":\"content\"}}}}}"
        );

        Assert.True(AnthropicResponseSchema.IsValidChunk(document.RootElement));
    }
}
