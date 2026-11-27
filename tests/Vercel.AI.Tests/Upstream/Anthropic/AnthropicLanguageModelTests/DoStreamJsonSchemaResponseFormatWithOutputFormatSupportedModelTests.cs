// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream &gt; json schema response format with output format (supported model)</c>.</summary>
public sealed class DoStreamJsonSchemaResponseFormatWithOutputFormatSupportedModelTests
{
    private static async Task<(CaptureHandler Handler, List<LanguageModelStreamPart> Result)> Run()
    {
        var handler = ChunksFixture("anthropic-json-output-format.1");
        var options = Options();
        options.JsonResponse = true;
        options.JsonSchema = AnthropicParity.Json(
            """
            {"type":"object","properties":{"characters":{"type":"array","items":{"type":"object","properties":{"name":{"type":"string"},"class":{"type":"string"},"description":{"type":"string"}},"required":["name","class","description"],"additionalProperties":false}}},"required":["characters"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}
            """
        );

        return (handler, await DoStream(handler, options, "claude-sonnet-4-5"));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doStream > json schema response format with output format (supported model)::should pass json schema response format as output_config.format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_json_schema_response_format_as_output_config_format()
    {
        var (handler, _) = await Run();

        AssertJson(
            HelloBody(
                """
                {
                  "output_config": {"format":{
                    "schema": {
                      "$schema": "http://json-schema.org/draft-07/schema#",
                      "additionalProperties": false,
                      "properties": {"characters":{
                        "items": {
                          "additionalProperties": false,
                          "properties": {"class":{"type":"string"},"description":{"type":"string"},"name":{"type":"string"}},
                          "required": ["name","class","description"],
                          "type": "object"
                        },
                        "type": "array"
                      }},
                      "required": ["characters"],
                      "type": "object"
                    },
                    "type": "json_schema"
                  }},
                  "stream": true
                }
                """,
                "claude-sonnet-4-5",
                64000
            ),
            RequestBody(handler)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doStream > json schema response format with output format (supported model)::should stream the text output",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_the_text_output()
    {
        var (_, result) = await Run();

        AssertJson(
            Snapshot(
                "doStream > json schema response format with output format (supported model) > should stream the text output"
            ),
            StreamJson(result)
        );
    }
}
