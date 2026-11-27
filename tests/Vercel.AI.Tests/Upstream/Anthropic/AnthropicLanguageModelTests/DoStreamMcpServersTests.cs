// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream &gt; mcp servers</c>.</summary>
public sealed class DoStreamMcpServersTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream > mcp servers::";

    [Fact]
    [UpstreamTest(
        Prefix + "should stream code execution tool results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_code_execution_tool_results()
    {
        var options = AnthropicParity.WithProvider(
            "{\"mcpServers\":[{\"type\":\"url\",\"name\":\"echo\",\"url\":\"https://echo.mcp.inevitable.fyi/mcp\"}]}"
        );
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(DoStreamSupport.FixtureStream("anthropic-mcp.1")),
            "claude-3-haiku-20240307",
            options
        );

        DoStreamSupport.AssertStreamSnapshot(parts, "mcp-servers-stream");
    }
}
