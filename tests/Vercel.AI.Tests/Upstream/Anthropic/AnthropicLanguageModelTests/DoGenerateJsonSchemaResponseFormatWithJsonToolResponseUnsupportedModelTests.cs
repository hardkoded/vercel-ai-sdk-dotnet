// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; json schema response format with json tool response (unsupported model)</c>.</summary>
public sealed class DoGenerateJsonSchemaResponseFormatWithJsonToolResponseUnsupportedModelTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > json schema response format with json tool response (unsupported model)::";

    // Maps the beforeEach of the describe.
    private static async Task<(
        CaptureHandler Handler,
        LanguageModelGenerateResult Result
    )> GenerateAsync()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-json-tool.1");
        var options = AnthropicParity.Hello();
        options.JsonResponse = true;
        options.JsonSchema = AnthropicParity.Json(
            """
            {
              "type": "object",
              "properties": { "name": { "type": "string" } },
              "required": ["name"],
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
        Prefix + "should pass json schema response format as a tool",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_json_schema_response_format_as_a_tool()
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
                  "description": "Respond with a JSON object.",
                  "input_schema": {
                    "$schema": "http://json-schema.org/draft-07/schema#",
                    "additionalProperties": false,
                    "properties": { "name": { "type": "string" } },
                    "required": ["name"],
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
    [UpstreamTest(Prefix + "should return the json response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_the_json_response()
    {
        var (_, result) = await GenerateAsync();

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal(
            "{\"elements\":[{\"location\":\"San Francisco\",\"temperature\":-5,\"condition\":\"snowy\"},{\"location\":\"London\",\"temperature\":0,\"condition\":\"snowy\"},{\"location\":\"Paris\",\"temperature\":23,\"condition\":\"cloudy\"},{\"location\":\"Berlin\",\"temperature\":-9,\"condition\":\"snowy\"}]}",
            text.Text
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should send stop finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_stop_finish_reason()
    {
        var (_, result) = await GenerateAsync();

        Assert.Equal("tool_use", result.RawFinishReason);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }
}
