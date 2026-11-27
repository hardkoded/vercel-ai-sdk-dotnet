// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.ConvertToAnthropicPrompt.PromptRunner;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>assistant messages &gt; code_execution 20260120</c>.</summary>
public sealed class CodeExecution20260120Tests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages > code_execution 20260120::";

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve caller metadata and response order for dynamic filtering",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_caller_metadata_and_response_order_for_dynamic_filtering()
    {
        const string caller =
            "\"providerOptions\":{\"anthropic\":{\"caller\":{\"type\":\"code_execution_20260120\",\"toolId\":\"code-execution-call\"}}}";
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"type":"tool-call","toolCallId":"code-execution-call","toolName":"code_execution","input":{"type":"programmatic-tool-call","code":"await web_search({ query: \"AI SDK\" })"},"providerExecuted":true,"providerOptions":{"anthropic":{"caller":{"type":"direct"}}}},
              {"type":"tool-call","toolCallId":"web-search-call-1","toolName":"web_search","input":{"query":"AI SDK"},"providerExecuted":true,CALLER},
              {"type":"tool-result","toolCallId":"web-search-call-1","toolName":"web_search","output":{"type":"json","value":[]},CALLER},
              {"type":"tool-call","toolCallId":"web-search-call-2","toolName":"web_search","input":{"query":"Anthropic"},"providerExecuted":true,CALLER},
              {"type":"tool-result","toolCallId":"web-search-call-2","toolName":"web_search","output":{"type":"json","value":[]},CALLER},
              {"type":"tool-result","toolCallId":"code-execution-call","toolName":"code_execution","output":{"type":"json","value":{"type":"encrypted_code_execution_result","encrypted_stdout":"encrypted-output","stderr":"","return_code":0,"content":[]}}}]}]
            """.Replace("CALLER", caller),
            sendReasoning: false
        );

        Assert.Single(result.Messages);
        var content = result.Messages[0]!["content"]!.AsArray();
        Assert.Equal("assistant", (string?)result.Messages[0]!["role"]);
        Assert.Equal(6, content.Count);
        var codeExecutionCaller =
            """{"type":"code_execution_20260120","tool_id":"code-execution-call"}""";
        Assert.Equal("server_tool_use", (string?)content[0]!["type"]);
        Assert.Equal("code-execution-call", (string?)content[0]!["id"]);
        AssertJson("""{"type":"direct"}""", content[0]!["caller"]);
        Assert.Equal("server_tool_use", (string?)content[1]!["type"]);
        Assert.Equal("web-search-call-1", (string?)content[1]!["id"]);
        AssertJson(codeExecutionCaller, content[1]!["caller"]);
        Assert.Equal("web_search_tool_result", (string?)content[2]!["type"]);
        Assert.Equal("web-search-call-1", (string?)content[2]!["tool_use_id"]);
        AssertJson(codeExecutionCaller, content[2]!["caller"]);
        Assert.Equal("server_tool_use", (string?)content[3]!["type"]);
        Assert.Equal("web-search-call-2", (string?)content[3]!["id"]);
        AssertJson(codeExecutionCaller, content[3]!["caller"]);
        Assert.Equal("web_search_tool_result", (string?)content[4]!["type"]);
        Assert.Equal("web-search-call-2", (string?)content[4]!["tool_use_id"]);
        AssertJson(codeExecutionCaller, content[4]!["caller"]);
        Assert.Equal("code_execution_tool_result", (string?)content[5]!["type"]);
        Assert.Equal("code-execution-call", (string?)content[5]!["tool_use_id"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve a deferred server result across a client tool continuation",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_a_deferred_server_result_across_a_client_tool_continuation()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
               {"type":"tool-call","toolCallId":"code-execution-call","toolName":"code_execution","input":{"type":"programmatic-tool-call","code":"await fetch_url({ url: \"https://example.com\" })"},"providerExecuted":true,"providerOptions":{"anthropic":{"caller":{"type":"direct"}}}},
               {"type":"tool-call","toolCallId":"client-tool-call","toolName":"fetch_url","input":{"url":"https://example.com"},"providerOptions":{"anthropic":{"caller":{"type":"code_execution_20260120","toolId":"code-execution-call"}}}}]},
             {"role":"tool","content":[{"type":"tool-result","toolCallId":"client-tool-call","toolName":"fetch_url","output":{"type":"text","value":"Example Domain"}}]},
             {"role":"assistant","content":[
               {"type":"tool-result","toolCallId":"code-execution-call","toolName":"code_execution","output":{"type":"json","value":{"type":"encrypted_code_execution_result","encrypted_stdout":"encrypted-output","stderr":"","return_code":0,"content":[]}}}]}]
            """,
            sendReasoning: false
        );

        // Anthropic pairs a deferred server result by tool_use_id across
        // responses; the intervening client tool exchange must stay intact.
        Assert.Equal(3, result.Messages.Count);
        var first = result.Messages[0]!["content"]!.AsArray();
        Assert.Equal("assistant", (string?)result.Messages[0]!["role"]);
        Assert.Equal("server_tool_use", (string?)first[0]!["type"]);
        Assert.Equal("code-execution-call", (string?)first[0]!["id"]);
        AssertJson("""{"type":"direct"}""", first[0]!["caller"]);
        Assert.Equal("tool_use", (string?)first[1]!["type"]);
        Assert.Equal("client-tool-call", (string?)first[1]!["id"]);
        AssertJson(
            """{"type":"code_execution_20260120","tool_id":"code-execution-call"}""",
            first[1]!["caller"]
        );
        var second = result.Messages[1]!["content"]!.AsArray();
        Assert.Equal("user", (string?)result.Messages[1]!["role"]);
        Assert.Equal("tool_result", (string?)second[0]!["type"]);
        Assert.Equal("client-tool-call", (string?)second[0]!["tool_use_id"]);
        var third = result.Messages[2]!["content"]!.AsArray();
        Assert.Equal("assistant", (string?)result.Messages[2]!["role"]);
        Assert.Equal("code_execution_tool_result", (string?)third[0]!["type"]);
        Assert.Equal("code-execution-call", (string?)third[0]!["tool_use_id"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should replay server_tool_use input without the internal discriminator",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_replay_server_tool_use_input_without_the_internal_discriminator()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"type":"tool-call","toolCallId":"srvtoolu_cache_replay","toolName":"code_execution","input":{"type":"bash_code_execution","command":"echo cache-compatible"},"providerExecuted":true}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """
            [{"role":"assistant","content":[{"type":"server_tool_use","id":"srvtoolu_cache_replay","name":"bash_code_execution","input":{"command":"echo cache-compatible"}}]}]
            """,
            result.Messages
        );
        Assert.Empty(result.Warnings);
    }

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
