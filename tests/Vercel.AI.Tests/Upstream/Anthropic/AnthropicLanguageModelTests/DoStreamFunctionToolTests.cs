// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream &gt; function tool</c>.</summary>
public sealed class DoStreamFunctionToolTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream > function tool::";

    [Fact]
    [UpstreamTest(Prefix + "should stream tool deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_tool_deltas()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "test-tool",
                null,
                AnthropicParity.Json(
                    """{"type":"object","properties":{"value":{"type":"string"}},"required":["value"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}"""
                )
            ),
        };
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    """{"type":"message_start","message":{"id":"msg_01GouTqNCGXzrj5LQ5jEkw67","type":"message","role":"assistant","model":"claude-3-haiku-20240307","stop_sequence":null,"usage":{"input_tokens":441,"output_tokens":2},"content":[],"stop_reason":null}            }""",
                    """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}      }""",
                    """{"type": "ping"}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Okay"}    }""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"!"}   }""",
                    """{"type":"content_block_stop","index":0    }""",
                    """{"type":"content_block_start","index":1,"content_block":{"type":"tool_use","id":"toolu_01DBsB4vvYLnBDzZ5rBSxSLs","name":"test-tool","input":{}}      }""",
                    """{"type":"content_block_delta","index":1,"delta":{"type":"input_json_delta","partial_json":""}           }""",
                    """{"type":"content_block_delta","index":1,"delta":{"type":"input_json_delta","partial_json":"{\"value"}              }""",
                    """{"type":"content_block_delta","index":1,"delta":{"type":"input_json_delta","partial_json":"\":"}      }""",
                    """{"type":"content_block_delta","index":1,"delta":{"type":"input_json_delta","partial_json":"\"Spark"}          }""",
                    """{"type":"content_block_delta","index":1,"delta":{"type":"input_json_delta","partial_json":"le"}          }""",
                    """{"type":"content_block_delta","index":1,"delta":{"type":"input_json_delta","partial_json":" Day\"}"}               }""",
                    """{"type":"content_block_stop","index":1              }""",
                    """{"type":"message_delta","delta":{"stop_reason":"tool_use","stop_sequence":null},"usage":{"output_tokens":65}           }""",
                    """{"type":"message_stop"           }"""
                )
            ),
            "claude-3-haiku-20240307",
            options
        );

        DoStreamSupport.AssertStreamSnapshot(parts, "stream-tool-deltas");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should support tools with empty parameters in streaming",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_support_tools_with_empty_parameters_in_streaming()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "test-tool",
                null,
                AnthropicParity.Json(
                    """{"type":"object","properties":{},"required":[],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}"""
                )
            ),
        };
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(DoStreamSupport.FixtureStream("anthropic-tool-no-args")),
            "claude-3-haiku-20240307",
            options
        );

        DoStreamSupport.AssertStreamSnapshot(parts, "tool-no-args-stream");
    }
}
