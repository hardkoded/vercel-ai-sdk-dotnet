// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; tools with preliminary results &gt; single tool with preliminary results</c>.</summary>
public sealed class SingleToolWithPreliminaryResultsTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > tools with preliminary results > single tool with preliminary results::";

    private const string FinalOutput =
        "{\"status\":\"success\",\"text\":\"The weather in San Francisco is 72°F\",\"temperature\":72}";

    [Fact]
    [UpstreamTest(
        Prefix + "should only include final tool result in content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_only_include_final_tool_result_in_content()
    {
        var result = await Run();

        Assert.Equal(
            new[] { "tool-call", "tool-result" },
            result.Content.Select(part => part.Type)
        );
        var toolResult = Assert.IsType<GeneratedToolResult>(result.Content[1]);
        Assert.False(toolResult.Dynamic);
        Assert.Equal("call-1", toolResult.ToolCallId);
        Assert.Equal("cityAttractions", toolResult.ToolName);
        AssertJson(FinalOutput, toolResult.ResultJson);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should only include final tool result in step content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_only_include_final_tool_result_in_step_content()
    {
        var step = Assert.Single((await Run()).Steps);

        Assert.Equal(new[] { "tool-call", "tool-result" }, step.Content.Select(part => part.Type));
        AssertJson(FinalOutput, Assert.IsType<GeneratedToolResult>(step.Content[1]).ResultJson);
        var executed = Assert.Single(step.ToolResults);
        Assert.Null(executed.Preliminary);
        AssertJson(FinalOutput, executed.OutputJson);
    }

    internal static void AssertJson(string expected, string actual)
    {
        Assert.True(
            System.Text.Json.Nodes.JsonNode.DeepEquals(
                System.Text.Json.Nodes.JsonNode.Parse(expected),
                System.Text.Json.Nodes.JsonNode.Parse(actual)
            ),
            actual
        );
    }

    internal static Tool CityAttractions()
    {
        var tool = Tool.Function(
            "cityAttractions",
            null,
            "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"]}",
            null
        );
        tool.ExecuteStream = (input, _) => Stream(input.GetProperty("city").GetString()!);
        return tool;
    }

    private static async IAsyncEnumerable<string> Stream(string city)
    {
        await Task.Yield();
        yield return JsonSerializer.Serialize(
            new { status = "loading", text = $"Getting weather for {city}" }
        );
        yield return JsonSerializer.Serialize(
            new
            {
                status = "success",
                text = $"The weather in {city} is 72°F",
                temperature = 72,
            }
        );
    }

    private static Task<GenerateTextResult<string>> Run()
    {
        return new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = new TestLanguageModel
                {
                    OnGenerate = _ => new LanguageModelGenerateResult(
                        new GeneratedContent[]
                        {
                            new GeneratedToolCall(
                                "call-1",
                                "cityAttractions",
                                "{ \"city\": \"San Francisco\" }"
                            ),
                        },
                        FinishReason.ToolCalls,
                        new LanguageModelUsage(10, 20, 30),
                        "tool-calls"
                    ),
                },
                Prompt = "test-input",
                Tools = new[] { CityAttractions() },
            }
        );
    }
}
