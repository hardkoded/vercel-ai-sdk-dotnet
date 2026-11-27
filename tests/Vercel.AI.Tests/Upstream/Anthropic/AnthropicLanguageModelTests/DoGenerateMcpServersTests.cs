// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; mcp servers</c>.</summary>
public sealed class DoGenerateMcpServersTests
{
    private const string McpServers =
        """{"mcpServers":[{"type":"url","name":"echo","url":"https://echo.mcp.inevitable.fyi/mcp"}]}""";

    [Fact]
    [UpstreamTest(
        Prefix + "doGenerate > mcp servers::should send request body with include and tool",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_request_body_with_include_and_tool()
    {
        var handler = JsonFixture("anthropic-mcp.1");

        await DoGenerate(handler, Options(McpServers));

        AssertJson(
            HelloBody(
                """{"mcp_servers":[{"name":"echo","type":"url","url":"https://echo.mcp.inevitable.fyi/mcp"}]}"""
            ),
            RequestBody(handler)
        );
        AssertRequestHeaders(
            handler,
            ("anthropic-beta", "mcp-client-2025-04-04"),
            ("anthropic-version", "2023-06-01"),
            ("content-type", "application/json"),
            ("x-api-key", "test-api-key")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "doGenerate > mcp servers::should include mcp tool call and result in content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_mcp_tool_call_and_result_in_content()
    {
        var result = await DoGenerate(JsonFixture("anthropic-mcp.1"), Options(McpServers));

        AssertJson(
            Snapshot(
                "doGenerate > mcp servers > should include mcp tool call and result in content"
            ),
            ContentJson(result.Content)
        );
    }
}
