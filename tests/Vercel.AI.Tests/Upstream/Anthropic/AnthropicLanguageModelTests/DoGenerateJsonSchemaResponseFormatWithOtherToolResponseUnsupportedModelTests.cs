// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; json schema response format with other tool response (unsupported model)</c>.</summary>
public sealed class DoGenerateJsonSchemaResponseFormatWithOtherToolResponseUnsupportedModelTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > json schema response format with other tool response (unsupported model)::";

    // Maps the beforeEach of the describe.
    private static async Task<(
        CaptureHandler Handler,
        LanguageModelGenerateResult Result
    )> GenerateAsync()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-json-other-tool.1");
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "get-weather",
                "Get the weather in a location",
                AnthropicParity.Json(
                    """
                    {
                      "type": "object",
                      "properties": { "location": { "type": "string" } },
                      "required": ["location"],
                      "additionalProperties": false,
                      "$schema": "http://json-schema.org/draft-07/schema#"
                    }
                    """
                )
            ),
        };
        options.JsonResponse = true;
        options.JsonSchema = AnthropicParity.Json(
            """
            {
              "type": "object",
              "properties": {
                "weather": { "type": "string" },
                "temperature": { "type": "number" }
              },
              "required": ["weather", "temperature"],
              "additionalProperties": false,
              "$schema": "http://json-schema.org/draft-07/schema#"
            }
            """
        );

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            options
        );
        return (handler, result);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass the tool and the json schema response format as tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_the_tool_and_the_json_schema_response_format_as_tools()
    {
        var (handler, _) = await GenerateAsync();

        LanguageModelTestSupport.AssertJson(
            """
            {
              "max_tokens": 4096,
              "messages": [
                { "content": [{ "text": "Hello", "type": "text" }], "role": "user" }
              ],
              "model": "claude-3-haiku-20240307",
              "tool_choice": { "disable_parallel_tool_use": true, "type": "any" },
              "tools": [
                {
                  "description": "Get the weather in a location",
                  "input_schema": {
                    "$schema": "http://json-schema.org/draft-07/schema#",
                    "additionalProperties": false,
                    "properties": { "location": { "type": "string" } },
                    "required": ["location"],
                    "type": "object"
                  },
                  "name": "get-weather"
                },
                {
                  "description": "Respond with a JSON object.",
                  "input_schema": {
                    "$schema": "http://json-schema.org/draft-07/schema#",
                    "additionalProperties": false,
                    "properties": {
                      "temperature": { "type": "number" },
                      "weather": { "type": "string" }
                    },
                    "required": ["weather", "temperature"],
                    "type": "object"
                  },
                  "name": "json"
                }
              ]
            }
            """,
            LanguageModelTestSupport.RequestBody(handler)
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should return the tool call", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_the_tool_call()
    {
        var (_, result) = await GenerateAsync();

        var call = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content));
        Assert.Equal("{\"location\":\"San Francisco\"}", call.ArgumentsJson);
        Assert.Equal("toolu_01PQjhxo3eirCdKNvCJrKc8f", call.ToolCallId);
        Assert.Equal("weather", call.ToolName);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send tool-calls finish reason",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_tool_calls_finish_reason()
    {
        var (_, result) = await GenerateAsync();

        Assert.Equal("tool_use", result.RawFinishReason);
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
    }
}
