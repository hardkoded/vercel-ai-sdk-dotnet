// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; programmatic tool calling</c>.</summary>
public sealed class DoGenerateProgrammaticToolCallingTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > programmatic tool calling::";

    private static LanguageModelCallOptions Options(string toolName, string property)
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            new LanguageModelTool(
                toolName,
                null,
                AnthropicParity.Json(
                    "{\"type\":\"object\",\"properties\":{\""
                        + property
                        + "\":{\"type\":\"string\"}}}"
                )
            ),
        };
        return options;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include caller info when tool_use has caller field from code_execution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_caller_info_when_tool_use_has_caller_field_from_code_execution()
    {
        var handler = AnthropicParity.Ok(
            "{\"id\":\"msg_01Test\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"tool_use\",\"id\":\"toolu_01Test\",\"name\":\"query_database\",\"input\":{\"sql\":\"SELECT * FROM users\"},\"caller\":{\"type\":\"code_execution_20250825\",\"tool_id\":\"srvtoolu_01CodeExec\"}}],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":\"tool_use\",\"usage\":{\"input_tokens\":100,\"output_tokens\":50}}"
        );

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            Options("query_database", "sql")
        );

        LanguageModelTestSupport.AssertJson(
            """
            [
              {
                "type": "tool-call",
                "input": "{\"sql\":\"SELECT * FROM users\"}",
                "providerMetadata": {
                  "anthropic": {
                    "caller": {
                      "toolId": "srvtoolu_01CodeExec",
                      "type": "code_execution_20250825"
                    }
                  }
                },
                "toolCallId": "toolu_01Test",
                "toolName": "query_database"
              }
            ]
            """,
            LanguageModelTestSupport.Snapshot(result.Content)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include caller info when tool_use has direct caller type",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_caller_info_when_tool_use_has_direct_caller_type()
    {
        var handler = AnthropicParity.Ok(
            "{\"id\":\"msg_01Test\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"tool_use\",\"id\":\"toolu_01Test\",\"name\":\"get_weather\",\"input\":{\"city\":\"Tokyo\"},\"caller\":{\"type\":\"direct\"}}],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":\"tool_use\",\"usage\":{\"input_tokens\":100,\"output_tokens\":50}}"
        );

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            Options("get_weather", "city")
        );

        // toolId is undefined upstream, so it is not in the metadata.
        LanguageModelTestSupport.AssertJson(
            """
            [
              {
                "type": "tool-call",
                "input": "{\"city\":\"Tokyo\"}",
                "providerMetadata": { "anthropic": { "caller": { "type": "direct" } } },
                "toolCallId": "toolu_01Test",
                "toolName": "get_weather"
              }
            ]
            """,
            LanguageModelTestSupport.Snapshot(result.Content)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not include caller info when tool_use has no caller field",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_include_caller_info_when_tool_use_has_no_caller_field()
    {
        var handler = AnthropicParity.Ok(
            "{\"id\":\"msg_01Test\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"tool_use\",\"id\":\"toolu_01Test\",\"name\":\"get_weather\",\"input\":{\"city\":\"Tokyo\"}}],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":\"tool_use\",\"usage\":{\"input_tokens\":100,\"output_tokens\":50}}"
        );

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            Options("get_weather", "city")
        );

        LanguageModelTestSupport.AssertJson(
            """
            [
              {
                "type": "tool-call",
                "input": "{\"city\":\"Tokyo\"}",
                "toolCallId": "toolu_01Test",
                "toolName": "get_weather"
              }
            ]
            """,
            LanguageModelTestSupport.Snapshot(result.Content)
        );
    }
}
