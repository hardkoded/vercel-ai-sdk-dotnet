// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; code execution 20250522</c>.</summary>
public sealed class DoGenerateCodeExecution20250522Tests
{
    // The describe block shadows TEST_PROMPT.
    private const string Prompt = "Write a Python function to calculate factorial";

    private static JsonObject BodyWith(string extra)
    {
        var body = HelloBody(extra);
        body["messages"] = JsonNode.Parse(
            """[{"content":[{"text":"Write a Python function to calculate factorial","type":"text"}],"role":"user"}]"""
        );
        return body;
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > code execution 20250522::should enable server-side code execution when using anthropic.tools.codeExecution_20250522",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_enable_server_side_code_execution_when_using_anthropic_tools_codeExecution_20250522()
    {
        var handler = JsonValue(
            """
            {"type":"message","id":"msg_test","content":[{"type":"text","text":"Here is a Python function to calculate factorial"}],"stop_reason":"end_turn","usage":{"input_tokens":10,"output_tokens":20}}
            """
        );

        await DoGenerate(
            handler,
            Options(
                tools: [ProviderTool("anthropic.code_execution_20250522", "code_execution")],
                text: Prompt
            )
        );

        AssertJson(
            BodyWith("""{"tools":[{"name":"code_execution","type":"code_execution_20250522"}]}"""),
            RequestBody(handler)
        );
        AssertRequestHeaders(
            handler,
            ("anthropic-beta", "code-execution-2025-05-22"),
            ("anthropic-version", "2023-06-01"),
            ("content-type", "application/json"),
            ("x-api-key", "test-api-key")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > code execution 20250522::should handle server-side code execution results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_server_side_code_execution_results()
    {
        var handler = JsonValue(
            """
            {"type":"message","id":"msg_test","content":[
              {"type":"server_tool_use","id":"tool_1","name":"code_execution","input":{"code":"print(\"Hello, World!\")"}},
              {"type":"code_execution_tool_result","tool_use_id":"tool_1","content":{"type":"code_execution_result","stdout":"Hello, World!\n","stderr":"","return_code":0}},
              {"type":"text","text":"The code executed successfully with output: Hello, World!"}
            ],"stop_reason":"end_turn","usage":{"input_tokens":15,"output_tokens":25,"server_tool_use":{"code_execution_requests":1}}}
            """
        );

        var result = await DoGenerate(
            handler,
            Options(
                tools: [ProviderTool("anthropic.code_execution_20250522", "code_execution")],
                text: Prompt
            )
        );

        AssertJson(
            """
            [
              {"input":"{\"type\":\"programmatic-tool-call\",\"code\":\"print(\\\"Hello, World!\\\")\"}","providerExecuted":true,"toolCallId":"tool_1","toolName":"code_execution","type":"tool-call"},
              {"result":{"content":[],"return_code":0,"stderr":"","stdout":"Hello, World!\n","type":"code_execution_result"},"toolCallId":"tool_1","toolName":"code_execution","type":"tool-result"},
              {"text":"The code executed successfully with output: Hello, World!","type":"text"}
            ]
            """,
            ContentJson(result.Content)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > code execution 20250522::should handle server-side code execution errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_server_side_code_execution_errors()
    {
        var handler = JsonValue(
            """
            {"type":"message","id":"msg_test","content":[
              {"type":"code_execution_tool_result","tool_use_id":"tool_1","content":{"type":"code_execution_tool_result_error","error_code":"unavailable"}},
              {"type":"text","text":"The code execution service is currently unavailable."}
            ],"stop_reason":"end_turn","usage":{"input_tokens":10,"output_tokens":20}}
            """
        );

        var result = await DoGenerate(
            handler,
            Options(
                tools: [ProviderTool("anthropic.code_execution_20250522", "code_execution")],
                text: Prompt
            )
        );

        AssertJson(
            """
            [
              {"isError":true,"result":{"errorCode":"unavailable","type":"code_execution_tool_result_error"},"toolCallId":"tool_1","toolName":"code_execution","type":"tool-result"},
              {"text":"The code execution service is currently unavailable.","type":"text"}
            ]
            """,
            ContentJson(result.Content)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > code execution 20250522::should work alongside regular client-side tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_work_alongside_regular_client_side_tools()
    {
        var handler = JsonValue(
            """
            {"type":"message","id":"msg_test","content":[{"type":"text","text":"I can execute code and calculate."}],"stop_reason":"end_turn","usage":{"input_tokens":10,"output_tokens":20}}
            """
        );

        await DoGenerate(
            handler,
            Options(
                tools:
                [
                    FunctionTool(
                        "calculator",
                        "Calculate math expressions",
                        """{"type":"object","properties":{}}"""
                    ),
                    ProviderTool("anthropic.code_execution_20250522", "code_execution"),
                ],
                text: Prompt
            )
        );

        AssertJson(
            BodyWith(
                """
                {"tools":[
                  {"description":"Calculate math expressions","input_schema":{"properties":{},"type":"object"},"name":"calculator"},
                  {"name":"code_execution","type":"code_execution_20250522"}
                ]}
                """
            ),
            RequestBody(handler)
        );
        AssertRequestHeaders(
            handler,
            ("anthropic-beta", "code-execution-2025-05-22"),
            ("anthropic-version", "2023-06-01"),
            ("content-type", "application/json"),
            ("x-api-key", "test-api-key")
        );
    }
}
