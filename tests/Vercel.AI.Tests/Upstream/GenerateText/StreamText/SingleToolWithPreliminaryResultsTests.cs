// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.SingleToolWithPreliminaryResultsTests;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; tools with preliminary results &gt; single tool with preliminary results</c>.</summary>
public sealed class SingleToolWithPreliminaryResultsTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > tools with preliminary results > single tool with preliminary results::";

    private const string Loading =
        "{\"status\":\"loading\",\"text\":\"Getting weather for San Francisco\"}";

    private const string Success =
        "{\"status\":\"success\",\"text\":\"The weather in San Francisco is 72°F\",\"temperature\":72}";

    [Fact]
    [UpstreamTest(
        Prefix + "should include preliminary tool results in full stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_preliminary_tool_results_in_full_stream()
    {
        var parts = new List<TextStreamPart>();
        await foreach (var part in Run().Stream())
        {
            parts.Add(part);
        }

        Assert.Equal(
            new[]
            {
                "start",
                "start-step",
                "tool-call",
                "tool-result",
                "tool-result",
                "tool-result",
                "finish-step",
                "finish",
            },
            parts.Select(part => part.Type).ToArray()
        );
        var first = Assert.IsType<ToolResultPart>(parts[3]).Result;
        var second = Assert.IsType<ToolResultPart>(parts[4]).Result;
        var final = Assert.IsType<ToolResultPart>(parts[5]).Result;
        AssertJson(Loading, first.OutputJson);
        AssertJson(Success, second.OutputJson);
        AssertJson(Success, final.OutputJson);
        Assert.True(first.Preliminary);
        Assert.True(second.Preliminary);
        Assert.Null(final.Preliminary);
        Assert.False(final.Dynamic);
        foreach (var executed in new[] { first, second, final })
        {
            Assert.Equal("call-1", executed.ToolCallId);
            Assert.Equal("cityAttractions", executed.ToolName);
            AssertJson("{\"city\":\"San Francisco\"}", executed.InputJson!);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include preliminary tool results in ui message stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_preliminary_tool_results_in_ui_message_stream()
    {
        var chunks = await SingleProviderExecutedToolCallAndResultTests.ReadUiChunks(Run());

        var expected = JsonNode.Parse(
            """
            [
              { "type": "start" },
              { "type": "start-step" },
              { "input": { "city": "San Francisco" }, "toolCallId": "call-1", "toolName": "cityAttractions", "type": "tool-input-available" },
              { "output": { "status": "loading", "text": "Getting weather for San Francisco" }, "preliminary": true, "toolCallId": "call-1", "type": "tool-output-available" },
              { "output": { "status": "success", "temperature": 72, "text": "The weather in San Francisco is 72°F" }, "preliminary": true, "toolCallId": "call-1", "type": "tool-output-available" },
              { "output": { "status": "success", "temperature": 72, "text": "The weather in San Francisco is 72°F" }, "toolCallId": "call-1", "type": "tool-output-available" },
              { "type": "finish-step" },
              { "finishReason": "stop", "type": "finish" }
            ]
            """
        );
        Assert.True(JsonNode.DeepEquals(expected, chunks), chunks.ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should only include final tool result in content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_only_include_final_tool_result_in_content()
    {
        var step = Assert.Single(await Run().Steps);

        Assert.Equal(new[] { "tool-call", "tool-result" }, step.Content.Select(part => part.Type));
        AssertJson(Success, Assert.IsType<GeneratedToolResult>(step.Content[1]).ResultJson);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should only include final tool result in step content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_only_include_final_tool_result_in_step_content()
    {
        var step = Assert.Single(await Run().Steps);

        var executed = Assert.Single(step.ToolResults);
        Assert.Null(executed.Preliminary);
        AssertJson(Success, executed.OutputJson);
        Assert.Equal(FinishReason.Stop, step.FinishReason);
    }

    private static StreamTextResult Run()
    {
        return new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).StreamTextAsync(
            new StreamTextOptions
            {
                Model = new TestLanguageModel
                {
                    StreamParts = new LanguageModelStreamPart[]
                    {
                        new ToolCallStreamPart(
                            "call-1",
                            "cityAttractions",
                            "{ \"city\": \"San Francisco\" }"
                        ),
                        new FinishStreamPart(
                            FinishReason.Stop,
                            new LanguageModelUsage(3, 10, 13),
                            "stop"
                        ),
                    },
                },
                Prompt = "test-input",
                Tools = new[] { CityAttractions() },
            }
        );
    }
}
