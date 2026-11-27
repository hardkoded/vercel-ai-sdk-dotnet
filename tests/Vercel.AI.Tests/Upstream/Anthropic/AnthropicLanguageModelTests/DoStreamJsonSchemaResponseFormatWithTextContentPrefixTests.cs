// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream &gt; json schema response format with text content prefix</c>.</summary>
public sealed class DoStreamJsonSchemaResponseFormatWithTextContentPrefixTests
{
    private static async Task<(CaptureHandler Handler, List<LanguageModelStreamPart> Result)> Run()
    {
        var handler = ChunksFixture("anthropic-json-tool.2");
        var options = Options();
        options.JsonResponse = true;
        options.JsonSchema = AnthropicParity.Json(
            """{"type":"object","properties":{"name":{"type":"string"}},"required":["name"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}"""
        );

        return (handler, await DoStream(handler, options));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doStream > json schema response format with text content prefix::should pass json schema response format as a tool",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_json_schema_response_format_as_a_tool()
    {
        var (handler, _) = await Run();

        AssertJson(
            HelloBody(
                """
                {
                  "stream": true,
                  "tool_choice": {"disable_parallel_tool_use":true,"type":"any"},
                  "tools": [{
                    "description": "Respond with a JSON object.",
                    "eager_input_streaming": true,
                    "input_schema": {
                      "$schema": "http://json-schema.org/draft-07/schema#",
                      "additionalProperties": false,
                      "properties": {"name":{"type":"string"}},
                      "required": ["name"],
                      "type": "object"
                    },
                    "name": "json"
                  }]
                }
                """
            ),
            RequestBody(handler)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doStream > json schema response format with text content prefix::should stream the response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_the_response()
    {
        var (_, result) = await Run();

        AssertJson(
            """
            [
              {"type":"stream-start","warnings":[]},
              {"id":"msg_01K2JbSUMYhez5RHoK9ZCj9U","modelId":"claude-haiku-4-5-20251001","type":"response-metadata"},
              {
                "kind":"anthropic.message_start",
                "providerMetadata":{"anthropic":{"id":"msg_01K2JbSUMYhez5RHoK9ZCj9U","model":"claude-haiku-4-5-20251001","usage":{
                  "cache_creation":{"ephemeral_1h_input_tokens":0,"ephemeral_5m_input_tokens":0},
                  "cache_creation_input_tokens":0,"cache_read_input_tokens":0,"input_tokens":849,"output_tokens":10,"service_tier":"standard"}}},
                "type":"custom"
              },
              {"id":"1","type":"text-start"},
              {"delta":"{\"elements\": [{\"location\": \"San Francisco\", \"temperature\": 58, \"condition\": \"sunny\"}]","id":"1","type":"text-delta"},
              {"delta":"}","id":"1","type":"text-delta"},
              {"id":"1","type":"text-end"},
              {
                "finishReason":{"raw":"tool_use","unified":"stop"},
                "providerMetadata":{"anthropic":{
                  "container":null,"contextManagement":null,"iterations":null,"serviceTier":"standard","stopSequence":null,
                  "usage":{"cache_creation":{"ephemeral_1h_input_tokens":0,"ephemeral_5m_input_tokens":0},"cache_creation_input_tokens":0,"cache_read_input_tokens":0,"input_tokens":849,"output_tokens":47,"service_tier":"standard"}
                }},
                "type":"finish",
                "usage":{
                  "inputTokens":{"cacheRead":0,"cacheWrite":0,"noCache":849,"total":849},
                  "outputTokens":{"total":47},
                  "raw":{"cache_creation":{"ephemeral_1h_input_tokens":0,"ephemeral_5m_input_tokens":0},"cache_creation_input_tokens":0,"cache_read_input_tokens":0,"input_tokens":849,"output_tokens":47,"service_tier":"standard"}
                }
              }
            ]
            """,
            StreamJson(result)
        );
    }
}
