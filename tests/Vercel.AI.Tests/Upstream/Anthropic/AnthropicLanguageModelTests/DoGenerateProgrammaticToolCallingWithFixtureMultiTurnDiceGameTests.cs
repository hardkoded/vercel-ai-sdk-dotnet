// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; programmatic tool calling &gt; with fixture (multi-turn dice game)</c>.</summary>
public sealed class DoGenerateProgrammaticToolCallingWithFixtureMultiTurnDiceGameTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > programmatic tool calling > with fixture (multi-turn dice game)::";

    // Maps the beforeEach of the describe.
    private static async Task<LanguageModelGenerateResult> GenerateAsync()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-programmatic-tool-calling.1");
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
                    "{\"type\":\"object\",\"properties\":{\"player\":{\"type\":\"string\"}}}"
                )
            ),
        };

        return await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            options
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should parse content with text, server_tool_use, tool_use with caller, and code_execution_tool_result",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_parse_content_with_text_server_tool_use_tool_use_with_caller_and_code_execution_tool_result()
    {
        var result = await GenerateAsync();

        Assert.True(
            JsonNode.DeepEquals(
                LanguageModelTestSupport.SnapshotFile("dice-game-content"),
                LanguageModelTestSupport.Snapshot(result.Content)
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract caller metadata for programmatic tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_caller_metadata_for_programmatic_tool_calls()
    {
        var result = await GenerateAsync();

        var toolCalls = result.Content.OfType<GeneratedToolCall>().ToList();

        // The rollDie calls should have caller metadata pointing to code_execution
        var rollDieCalls = toolCalls.Where(call => call.ToolName == "rollDie").ToList();
        Assert.True(rollDieCalls.Count > 0);

        foreach (var call in rollDieCalls)
        {
            JsonAssert.Equal(
                call.ProviderMetadata!.Value.GetProperty("anthropic").GetProperty("caller"),
                "{\"type\":\"code_execution_20250825\",\"toolId\":\"srvtoolu_01CberhXc9TgYXrCZU8bQoks\"}"
            );
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include code_execution as provider-executed tool call",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_code_execution_as_provider_executed_tool_call()
    {
        var result = await GenerateAsync();

        var codeExecCall = result
            .Content.OfType<GeneratedToolCall>()
            .FirstOrDefault(call => call.ToolName == "code_execution");
        Assert.NotNull(codeExecCall);
        Assert.Equal("tool-call", codeExecCall.Type);
        Assert.True(codeExecCall.ProviderExecuted);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include code_execution_tool_result as provider-executed tool result",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_code_execution_tool_result_as_provider_executed_tool_result()
    {
        var result = await GenerateAsync();

        var codeExecResult = result
            .Content.OfType<AnthropicToolResultContent>()
            .FirstOrDefault(part => part.ToolName == "code_execution");
        Assert.NotNull(codeExecResult);
        Assert.Equal("tool-result", codeExecResult.Type);
        var value = JsonNode.Parse(codeExecResult.ResultJson)!;
        Assert.Contains(
            "PLAYER 1 WINS THE GAME!",
            value["stdout"]!.GetValue<string>(),
            StringComparison.Ordinal
        );
        Assert.Equal(string.Empty, value["stderr"]!.GetValue<string>());
        Assert.Equal(0, value["return_code"]!.GetValue<int>());
    }
}
