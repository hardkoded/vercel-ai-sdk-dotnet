// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream &gt; programmatic tool calling</c>.</summary>
public sealed class DoStreamProgrammaticToolCallingTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream > programmatic tool calling::";

    [Fact]
    [UpstreamTest(
        Prefix + "should include caller info when tool_use has caller field from code_execution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_caller_info_when_tool_use_has_caller_field_from_code_execution()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "query_database",
                null,
                AnthropicParity.Json("""{"type":"object","properties":{"sql":{"type":"string"}}}""")
            ),
        };
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    """{"type":"message_start","message":{"id":"msg_01Test","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":100,"output_tokens":1}}}""",
                    """{"type":"content_block_start","index":0,"content_block":{"type":"tool_use","id":"toolu_01Test","name":"query_database","input":{},"caller":{"type":"code_execution_20250825","tool_id":"srvtoolu_01CodeExec"}}}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"input_json_delta","partial_json":"{\"sql\""}}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"input_json_delta","partial_json":": \"SELECT * FROM users\"}"}}""",
                    """{"type":"content_block_stop","index":0}""",
                    """{"type":"message_delta","delta":{"stop_reason":"tool_use","stop_sequence":null},"usage":{"output_tokens":50}}""",
                    """[DONE]"""
                )
            ),
            "claude-3-haiku-20240307",
            options
        );

        var toolCall = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("{\"sql\": \"SELECT * FROM users\"}", toolCall.ArgumentsJson);
        JsonAssert.Equal(
            toolCall.ProviderMetadata!.Value,
            """{"anthropic":{"caller":{"type":"code_execution_20250825","toolId":"srvtoolu_01CodeExec"}}}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include caller info when tool_use has direct caller type",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_caller_info_when_tool_use_has_direct_caller_type()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "get_weather",
                null,
                AnthropicParity.Json(
                    """{"type":"object","properties":{"city":{"type":"string"}}}"""
                )
            ),
        };
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    """{"type":"message_start","message":{"id":"msg_01Test","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":100,"output_tokens":1}}}""",
                    """{"type":"content_block_start","index":0,"content_block":{"type":"tool_use","id":"toolu_01Test","name":"get_weather","input":{},"caller":{"type":"direct"}}}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"input_json_delta","partial_json":"{\"city\""}}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"input_json_delta","partial_json":": \"Tokyo\"}"}}""",
                    """{"type":"content_block_stop","index":0}""",
                    """{"type":"message_delta","delta":{"stop_reason":"tool_use","stop_sequence":null},"usage":{"output_tokens":50}}""",
                    """[DONE]"""
                )
            ),
            "claude-3-haiku-20240307",
            options
        );

        var toolCall = Assert.Single(parts.OfType<ToolCallStreamPart>());
        JsonAssert.Equal(
            toolCall.ProviderMetadata!.Value,
            """{"anthropic":{"caller":{"type":"direct"}}}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use non-empty input from content_block_start for deferred tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_non_empty_input_from_content_block_start_for_deferred_tool_calls()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "query_database",
                null,
                AnthropicParity.Json("""{"type":"object","properties":{"sql":{"type":"string"}}}""")
            ),
        };
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    """{"type":"message_start","message":{"id":"msg_01Test","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":100,"output_tokens":1}}}""",
                    """{"type":"content_block_start","index":0,"content_block":{"type":"tool_use","id":"toolu_01Deferred","name":"query_database","input":{"sql":"SELECT COUNT(*) FROM orders"},"caller":{"type":"code_execution_20250825","tool_id":"srvtoolu_01CodeExec"}}}""",
                    """{"type":"content_block_stop","index":0}""",
                    """{"type":"message_delta","delta":{"stop_reason":"tool_use","stop_sequence":null},"usage":{"output_tokens":30}}""",
                    """[DONE]"""
                )
            ),
            "claude-3-haiku-20240307",
            options
        );

        var toolCall = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("toolu_01Deferred", toolCall.ToolCallId);
        Assert.Equal("{\"sql\":\"SELECT COUNT(*) FROM orders\"}", toolCall.ArgumentsJson);
        JsonAssert.Equal(
            toolCall.ProviderMetadata!.Value,
            """{"anthropic":{"caller":{"type":"code_execution_20250825","toolId":"srvtoolu_01CodeExec"}}}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should NOT prepend empty {} input when deltas follow content_block_start",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_NOT_prepend_empty_input_when_deltas_follow_content_block_start()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "get_weather",
                null,
                AnthropicParity.Json(
                    """{"type":"object","properties":{"city":{"type":"string"}}}"""
                )
            ),
        };
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    """{"type":"message_start","message":{"id":"msg_01Test","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":100,"output_tokens":1}}}""",
                    """{"type":"content_block_start","index":0,"content_block":{"type":"tool_use","id":"toolu_01Test","name":"get_weather","input":{}}}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"input_json_delta","partial_json":""}}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"input_json_delta","partial_json":"{\"city\""}}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"input_json_delta","partial_json":": \"London\"}"}}""",
                    """{"type":"content_block_stop","index":0}""",
                    """{"type":"message_delta","delta":{"stop_reason":"tool_use","stop_sequence":null},"usage":{"output_tokens":30}}""",
                    """[DONE]"""
                )
            ),
            "claude-3-haiku-20240307",
            options
        );

        var toolCall = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("{\"city\": \"London\"}", toolCall.ArgumentsJson);
    }
}
