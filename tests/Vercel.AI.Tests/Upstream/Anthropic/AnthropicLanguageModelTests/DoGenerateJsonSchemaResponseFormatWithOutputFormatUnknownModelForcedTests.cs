// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; json schema response format with output format (unknown model, forced)</c>.</summary>
public sealed class DoGenerateJsonSchemaResponseFormatWithOutputFormatUnknownModelForcedTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > json schema response format with output format (unknown model, forced)::";

    // Maps the beforeEach of the describe.
    private static async Task<(
        CaptureHandler Handler,
        LanguageModelGenerateResult Result
    )> GenerateAsync()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-json-output-format.1");
        var options = AnthropicParity.WithProvider("{\"structuredOutputMode\":\"outputFormat\"}");
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
            "claude-unknown",
            options
        );
        return (handler, result);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass json schema response format as output_config.format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_json_schema_response_format_as_output_config_format()
    {
        var (handler, _) = await GenerateAsync();

        LanguageModelTestSupport.AssertJson(
            """
            {
              "max_tokens": 128000,
              "messages": [
                { "content": [{ "text": "Hello", "type": "text" }], "role": "user" }
              ],
              "model": "claude-unknown",
              "output_config": {
                "format": {
                  "schema": {
                    "$schema": "http://json-schema.org/draft-07/schema#",
                    "additionalProperties": false,
                    "properties": { "name": { "type": "string" } },
                    "required": ["name"],
                    "type": "object"
                  },
                  "type": "json_schema"
                }
              }
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
        Assert.Equal(RecipeJsonText.Value, text.Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send stop finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_stop_finish_reason()
    {
        var (_, result) = await GenerateAsync();

        Assert.Equal("end_turn", result.RawFinishReason);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }
}
