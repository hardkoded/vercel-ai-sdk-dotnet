// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream &gt; programmatic tool calling &gt; with fixture (multi-turn dice game)</c>.</summary>
public sealed class DoStreamProgrammaticToolCallingWithFixtureMultiTurnDiceGameTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream > programmatic tool calling > with fixture (multi-turn dice game)::";

    [Fact]
    [UpstreamTest(
        Prefix
            + "should stream programmatic tool calling with multiple message_start/stop sequences",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_programmatic_tool_calling_with_multiple_message_start_stop_sequences()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            LanguageModelTool.ProviderDefined(
                "anthropic.code_execution_20250825",
                "code_execution",
                AnthropicParity.Json("{}")
            ),
            new LanguageModelTool(
                "rollDie",
                null,
                AnthropicParity.Json(
                    """{"type":"object","properties":{"player":{"type":"string"}}}"""
                )
            ),
        };
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.FixtureStream("anthropic-programmatic-tool-calling.1")
            ),
            "claude-3-haiku-20240307",
            options
        );

        DoStreamSupport.AssertStreamSnapshot(
            parts,
            "programmatic-tool-calling-multiple-message-start-stop"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract caller metadata from streamed tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_caller_metadata_from_streamed_tool_calls()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            LanguageModelTool.ProviderDefined(
                "anthropic.code_execution_20250825",
                "code_execution",
                AnthropicParity.Json("{}")
            ),
            new LanguageModelTool(
                "rollDie",
                null,
                AnthropicParity.Json(
                    """{"type":"object","properties":{"player":{"type":"string"}}}"""
                )
            ),
        };
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.FixtureStream("anthropic-programmatic-tool-calling.1")
            ),
            "claude-3-haiku-20240307",
            options
        );

        // Filter rollDie calls (not code_execution)
        var rollDieCalls = parts
            .OfType<ToolCallStreamPart>()
            .Where(call => call.ToolName == "rollDie")
            .ToList();
        Assert.True(rollDieCalls.Count > 0);

        // Each rollDie call should have caller metadata
        foreach (var call in rollDieCalls)
        {
            var caller = call.ProviderMetadata!.Value.GetProperty("anthropic")
                .GetProperty("caller");
            Assert.Equal("code_execution_20250825", caller.GetProperty("type").GetString());
            Assert.Matches("^srvtoolu_", caller.GetProperty("toolId").GetString());
            Assert.Equal(2, caller.EnumerateObject().Count());
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include code_execution_tool_result in stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_code_execution_tool_result_in_stream()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            LanguageModelTool.ProviderDefined(
                "anthropic.code_execution_20250825",
                "code_execution",
                AnthropicParity.Json("{}")
            ),
            new LanguageModelTool(
                "rollDie",
                null,
                AnthropicParity.Json(
                    """{"type":"object","properties":{"player":{"type":"string"}}}"""
                )
            ),
        };
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.FixtureStream("anthropic-programmatic-tool-calling.1")
            ),
            "claude-3-haiku-20240307",
            options
        );

        var toolResults = parts.OfType<ToolResultStreamPart>().ToList();

        // Should have code_execution result
        var codeExecResult = toolResults.FirstOrDefault(result =>
            result.ToolName == "code_execution"
        );
        Assert.NotNull(codeExecResult);
        Assert.Equal("tool-result", codeExecResult!.Type);
        Assert.Equal("code_execution", codeExecResult.ToolName);
    }
}
