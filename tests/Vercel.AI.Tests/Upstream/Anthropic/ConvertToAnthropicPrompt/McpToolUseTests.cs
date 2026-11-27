// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.ConvertToAnthropicPrompt.PromptRunner;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>assistant messages &gt; mcp tool use</c>.</summary>
public sealed class McpToolUseTests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages > mcp tool use::";

    [Fact]
    [UpstreamTest(
        Prefix + "should convert anthropic mcp tool use parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_anthropic_mcp_tool_use_parts()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"type":"tool-call","toolCallId":"mcptoolu_01HXPYHs79HH36fBbKHysCrp","toolName":"echo","input":{},"providerExecuted":true,"providerOptions":{"anthropic":{"type":"mcp-tool-use","serverName":"echo"}}},
              {"type":"tool-result","toolCallId":"mcptoolu_01HXPYHs79HH36fBbKHysCrp","toolName":"echo","output":{"type":"json","value":[{"type":"text","text":"Tool echo: hello world"}]}},
              {"type":"text","text":"The echo tool responded back with \"hello world\" - it simply echoed the message I sent to it!"}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """
            [{"content":[
              {"id":"mcptoolu_01HXPYHs79HH36fBbKHysCrp","input":{},"name":"echo","server_name":"echo","type":"mcp_tool_use"},
              {"content":[{"text":"Tool echo: hello world","type":"text"}],"is_error":false,"tool_use_id":"mcptoolu_01HXPYHs79HH36fBbKHysCrp","type":"mcp_tool_result"},
              {"text":"The echo tool responded back with \"hello world\" - it simply echoed the message I sent to it!","type":"text"}],
              "role":"assistant"}]
            """,
            result.Messages
        );
        Assert.Null(result.System);
        Assert.Empty(result.Betas);
        AssertWarnings(result, "provider executed tool result for tool echo is not supported");
    }
}
