// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Anthropic;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

public sealed class AnthropicResponseSchemaWebFetchToolResultTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-prepare-tools.test.ts::anthropicResponseSchema - web_fetch_tool_result::";

    [Fact]
    [UpstreamTest(
        Prefix + "should accept PDF response with base64 source",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_accept_PDF_response_with_base64_source()
    {
        using var document = JsonDocument.Parse(
            "{\"type\":\"message\",\"id\":\"123\",\"model\":\"claude-3-5-sonnet-20241022\",\"content\":[{\"type\":\"web_fetch_tool_result\",\"tool_use_id\":\"srvtoolu_01234567890abcdef\",\"content\":{\"type\":\"web_fetch_result\",\"url\":\"https://test.com\",\"retrieved_at\":\"2025-12-08T20:46:31.114158\",\"content\":{\"type\":\"document\",\"title\":\"Example Title\",\"source\":{\"type\":\"base64\",\"media_type\":\"application/pdf\",\"data\":\"JVBERi0xLjcNJeLjz9MNC\"}}}}],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":100,\"output_tokens\":50}}"
        );

        Assert.True(AnthropicResponseSchema.IsValidResponse(document.RootElement));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept text source in response",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_accept_text_source_in_response()
    {
        using var document = JsonDocument.Parse(
            "{\"type\":\"message\",\"id\":\"123\",\"model\":\"claude-3-5-sonnet-20241022\",\"content\":[{\"type\":\"web_fetch_tool_result\",\"tool_use_id\":\"srvtoolu_01234567890abcdef\",\"content\":{\"type\":\"web_fetch_result\",\"url\":\"https://test.com\",\"retrieved_at\":\"2025-12-08T20:46:31.114158\",\"content\":{\"type\":\"document\",\"title\":\"Example Title\",\"source\":{\"type\":\"text\",\"media_type\":\"text/plain\",\"data\":\"content\"}}}}],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":100,\"output_tokens\":50}}"
        );

        Assert.True(AnthropicResponseSchema.IsValidResponse(document.RootElement));
    }
}
