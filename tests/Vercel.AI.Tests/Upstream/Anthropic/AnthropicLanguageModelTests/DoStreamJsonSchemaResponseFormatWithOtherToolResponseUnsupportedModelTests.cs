// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream &gt; json schema response format with other tool response (unsupported model)</c>.</summary>
public sealed class DoStreamJsonSchemaResponseFormatWithOtherToolResponseUnsupportedModelTests
{
    private static async Task<(CaptureHandler Handler, List<LanguageModelStreamPart> Result)> Run()
    {
        var handler = ChunksFixture("anthropic-json-other-tool.1");
        var options = Options(
            tools:
            [
                FunctionTool(
                    "weather",
                    "Get the weather in a location",
                    """{"type":"object","properties":{"location":{"type":"string"}},"required":["location"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}"""
                ),
            ]
        );
        options.JsonResponse = true;
        options.JsonSchema = AnthropicParity.Json(
            """{"type":"object","properties":{"weather":{"type":"string"},"temperature":{"type":"number"}},"required":["weather","temperature"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}"""
        );

        return (handler, await DoStream(handler, options));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doStream > json schema response format with other tool response (unsupported model)::should pass json schema response format as a tool",
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
                  "tools": [
                    {
                      "description": "Get the weather in a location",
                      "eager_input_streaming": true,
                      "input_schema": {
                        "$schema": "http://json-schema.org/draft-07/schema#",
                        "additionalProperties": false,
                        "properties": {"location":{"type":"string"}},
                        "required": ["location"],
                        "type": "object"
                      },
                      "name": "weather"
                    },
                    {
                      "description": "Respond with a JSON object.",
                      "eager_input_streaming": true,
                      "input_schema": {
                        "$schema": "http://json-schema.org/draft-07/schema#",
                        "additionalProperties": false,
                        "properties": {"temperature":{"type":"number"},"weather":{"type":"string"}},
                        "required": ["weather","temperature"],
                        "type": "object"
                      },
                      "name": "json"
                    }
                  ]
                }
                """
            ),
            RequestBody(handler)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doStream > json schema response format with other tool response (unsupported model)::should stream the tool call",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_the_tool_call()
    {
        var (_, result) = await Run();

        AssertJson(
            """
            [
              {"type":"stream-start","warnings":[]},
              {"id":"msg_01CD3XaZfhNabxRt1SG5ybtK","modelId":"claude-haiku-4-5-20251001","type":"response-metadata"},
              {
                "kind":"anthropic.message_start",
                "providerMetadata":{"anthropic":{"id":"msg_01CD3XaZfhNabxRt1SG5ybtK","model":"claude-haiku-4-5-20251001","usage":{
                  "cache_creation":{"ephemeral_1h_input_tokens":0,"ephemeral_5m_input_tokens":0},
                  "cache_creation_input_tokens":0,"cache_read_input_tokens":0,"input_tokens":843,"output_tokens":16,"service_tier":"standard"}}},
                "type":"custom"
              },
              {"id":"toolu_019Zvehfe1XQWweT1pm7okyt","toolName":"weather","type":"tool-input-start"},
              {"delta":"{\"location\": \"San Francisco","id":"toolu_019Zvehfe1XQWweT1pm7okyt","type":"tool-input-delta"},
              {"delta":"\"}","id":"toolu_019Zvehfe1XQWweT1pm7okyt","type":"tool-input-delta"},
              {"id":"toolu_019Zvehfe1XQWweT1pm7okyt","type":"tool-input-end"},
              {"input":"{\"location\": \"San Francisco\"}","toolCallId":"toolu_019Zvehfe1XQWweT1pm7okyt","toolName":"weather","type":"tool-call"},
              {
                "finishReason":{"raw":"tool_use","unified":"tool-calls"},
                "providerMetadata":{"anthropic":{
                  "container":null,"contextManagement":null,"iterations":null,"serviceTier":"standard","stopSequence":null,
                  "usage":{"cache_creation":{"ephemeral_1h_input_tokens":0,"ephemeral_5m_input_tokens":0},"cache_creation_input_tokens":0,"cache_read_input_tokens":0,"input_tokens":843,"output_tokens":28,"service_tier":"standard"}
                }},
                "type":"finish",
                "usage":{
                  "inputTokens":{"cacheRead":0,"cacheWrite":0,"noCache":843,"total":843},
                  "outputTokens":{"total":28},
                  "raw":{"cache_creation":{"ephemeral_1h_input_tokens":0,"ephemeral_5m_input_tokens":0},"cache_creation_input_tokens":0,"cache_read_input_tokens":0,"input_tokens":843,"output_tokens":28,"service_tier":"standard"}
                }
              }
            ]
            """,
            StreamJson(result)
        );
    }
}
