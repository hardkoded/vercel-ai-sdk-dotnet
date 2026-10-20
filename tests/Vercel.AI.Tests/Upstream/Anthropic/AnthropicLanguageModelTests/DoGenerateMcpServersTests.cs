// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; mcp servers</c>.</summary>
public sealed class DoGenerateMcpServersTests
{
    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > mcp servers::should include mcp tool call and result in content",
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts the dynamic, provider-executed call and the result by property. Upstream compares a whole-content snapshot.")]
    public void Should_include_mcp_tool_call_and_result_in_content()
    {
        var result = AnthropicResponse.Parse(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "anthropic-mcp.1.json")),
            new AnthropicParseContext(false, "anthropic", false));

        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("mcptoolu_015oTj2fXVKLrDohFetJd5UL", call.ToolCallId);
        Assert.Equal("echo", call.ToolName);
        Assert.Contains("hello world", call.ArgumentsJson);
        Assert.True(call.ProviderExecuted);
        Assert.True(call.Dynamic);
        var toolResult = Assert.IsType<AnthropicToolResultContent>(result.Content[1]);
        Assert.Equal(call.ToolCallId, toolResult.ToolCallId);
        Assert.IsAssignableFrom<GeneratedText>(result.Content[2]);
    }
}
