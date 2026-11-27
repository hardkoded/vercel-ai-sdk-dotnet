// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.ConvertToAnthropicPrompt.PromptRunner;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>assistant messages &gt; code_execution 20250825</c>.</summary>
public sealed class CodeExecution20250825Tests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages > code_execution 20250825::";

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
              {"type":"tool-call","toolCallId":"srvtoolu_01Hq9rR6fZwwDGHkTYRafn7k","toolName":"code_execution","input":{"type":"text_editor_code_execution","command":"create","path":"/tmp/fibonacci.py","file_text":"def.."},"providerExecuted":true},
              {"type":"tool-result","toolCallId":"srvtoolu_01Hq9rR6fZwwDGHkTYRafn7k","toolName":"code_execution","output":{"type":"json","value":{"type":"text_editor_code_execution_create_result","is_file_update":false}}},
              {"type":"tool-call","toolCallId":"srvtoolu_0193G3ttnkiTfZASwHQSKc2V","toolName":"code_execution","input":{"type":"bash_code_execution","command":"python /tmp/fibonacci.py"},"providerExecuted":true},
              {"type":"tool-result","toolCallId":"srvtoolu_0193G3ttnkiTfZASwHQSKc2V","toolName":"code_execution","output":{"type":"json","value":{"type":"bash_code_execution_result","content":[],"stdout":"The 10th Fibonacci number is: 34\n","stderr":"","return_code":0}}}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """
            [{"content":[
              {"id":"srvtoolu_01Hq9rR6fZwwDGHkTYRafn7k","input":{"command":"create","file_text":"def..","path":"/tmp/fibonacci.py"},"name":"text_editor_code_execution","type":"server_tool_use"},
              {"content":{"is_file_update":false,"type":"text_editor_code_execution_create_result"},"tool_use_id":"srvtoolu_01Hq9rR6fZwwDGHkTYRafn7k","type":"text_editor_code_execution_tool_result"},
              {"id":"srvtoolu_0193G3ttnkiTfZASwHQSKc2V","input":{"command":"python /tmp/fibonacci.py"},"name":"bash_code_execution","type":"server_tool_use"},
              {"content":{"content":[],"return_code":0,"stderr":"","stdout":"The 10th Fibonacci number is: 34\n","type":"bash_code_execution_result"},"tool_use_id":"srvtoolu_0193G3ttnkiTfZASwHQSKc2V","type":"bash_code_execution_tool_result"}],
              "role":"assistant"}]
            """,
            result.Messages
        );
        Assert.Null(result.System);
        Assert.Empty(result.Betas);
        Assert.Empty(result.Warnings);
    }
}
