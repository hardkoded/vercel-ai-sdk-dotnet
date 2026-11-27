// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using static Vercel.AI.Tests.ConvertToAnthropicPrompt.PromptRunner;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>assistant messages &gt; code_execution 20250522</c>.</summary>
public sealed class CodeExecution20250522Tests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages > code_execution 20250522::";

    [Fact]
    [UpstreamTest(
        Prefix + "should convert anthropic code_execution tool call and result parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_anthropic_code_execution_tool_call_and_result_parts()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"input":{"code":"print(\"Hello, world!\")"},"providerExecuted":true,"toolCallId":"srvtoolu_01XyZ1234567890","toolName":"code_execution","type":"tool-call"},
              {"output":{"type":"json","value":{"type":"code_execution_result","stdout":"Hello, world!","stderr":"","return_code":0}},"toolCallId":"srvtoolu_01XyZ1234567890","toolName":"code_execution","type":"tool-result"}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """
            [{"content":[
              {"id":"srvtoolu_01XyZ1234567890","input":{"code":"print(\"Hello, world!\")"},"name":"code_execution","type":"server_tool_use"},
              {"content":{"content":[],"return_code":0,"stderr":"","stdout":"Hello, world!","type":"code_execution_result"},"tool_use_id":"srvtoolu_01XyZ1234567890","type":"code_execution_tool_result"}],
              "role":"assistant"}]
            """,
            result.Messages
        );
        Assert.Null(result.System);
        Assert.Empty(result.Betas);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should pass back encrypted_code_execution_result for multi-turn (web_fetch_20260209/web_search_20260209)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_back_encrypted_code_execution_result_for_multi_turn()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"type":"tool-call","toolCallId":"srvtoolu_webfetch_01","toolName":"web_fetch","input":{"url":"https://example.com"},"providerExecuted":true},
              {"type":"tool-result","toolCallId":"srvtoolu_webfetch_01","toolName":"web_fetch","output":{"type":"json","value":{"type":"web_fetch_result","url":"https://example.com","retrievedAt":"2026-01-01T00:00:00Z","content":{"type":"document","title":"Example","source":{"type":"text","mediaType":"text/plain","data":"hello"}}}}},
              {"type":"tool-call","toolCallId":"srvtoolu_codeexec_01","toolName":"code_execution","input":{"code":"print(\"done\")"},"providerExecuted":true},
              {"type":"tool-result","toolCallId":"srvtoolu_codeexec_01","toolName":"code_execution","output":{"type":"json","value":{"type":"encrypted_code_execution_result","encrypted_stdout":"enc_abc123","stderr":"","return_code":0,"content":[]}}}]}]
            """,
            sendReasoning: false
        );

        var assistantMessage = result.Messages[0]!;
        Assert.Equal("assistant", (string?)assistantMessage["role"]);
        var codeExecResult = ((JsonArray)assistantMessage["content"]!).First(c =>
            (string?)c!["type"] == "code_execution_tool_result"
        )!;
        AssertJson(
            """{"type":"encrypted_code_execution_result","encrypted_stdout":"enc_abc123","stderr":"","return_code":0,"content":[]}""",
            codeExecResult["content"]
        );
        Assert.Empty(result.Warnings);
    }
}
