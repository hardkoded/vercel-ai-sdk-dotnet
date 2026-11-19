// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText</c> &gt; <c>provider-executed tools</c> &gt; <c>provider-executed tool with preliminary result</c>.</summary>
public sealed class ProviderExecutedToolWithPreliminaryResultTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > provider-executed tools > provider-executed tool with preliminary result::should include the preliminary flag in the full stream",
        Coverage = UpstreamCoverage.Covered,
        Commit = "6815153d64d39d7e6de18a5b5c814642139757dc"
    )]
    public async Task Should_include_the_preliminary_flag_in_the_full_stream()
    {
        var parts = new List<TextStreamPart>();
        await foreach (var part in Run().Stream())
        {
            parts.Add(part);
        }

        var results = parts.OfType<ToolResultPart>().Select(part => part.Result).ToArray();
        Assert.Equal(2, results.Length);
        Assert.Equal("{\"result\":\"partial-image\"}", results[0].OutputJson);
        Assert.True(results[0].Preliminary);
        Assert.True(results[0].ProviderExecuted);
        Assert.Equal("{}", results[0].InputJson);
        Assert.Equal("call-1", results[0].ToolCallId);
        Assert.Equal("image_generation", results[0].ToolName);
        Assert.Equal("{\"result\":\"final-image\"}", results[1].OutputJson);
        Assert.Null(results[1].Preliminary);
        Assert.True(results[1].ProviderExecuted);
        Assert.Equal("{}", results[1].InputJson);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > provider-executed tools > provider-executed tool with preliminary result::should only include the final tool result in content",
        Coverage = UpstreamCoverage.Covered,
        Commit = "6815153d64d39d7e6de18a5b5c814642139757dc"
    )]
    public async Task Should_only_include_the_final_tool_result_in_content()
    {
        var step = Assert.Single(await Run().Steps);

        var result = Assert.IsType<GeneratedToolResult>(
            Assert.Single(step.Content, part => part.Type == "tool-result")
        );
        Assert.Equal("{\"result\":\"final-image\"}", result.ResultJson);
        Assert.Equal("call-1", result.ToolCallId);
        Assert.Equal("image_generation", result.ToolName);
    }

    private static StreamTextResult Run()
    {
        var args = JsonDocument.Parse("{}").RootElement;
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ToolCallStreamPart("call-1", "image_generation", "{}")
                {
                    ProviderExecuted = true,
                },
                new ToolResultStreamPart(
                    "call-1",
                    "image_generation",
                    JsonDocument.Parse("{\"result\":\"partial-image\"}").RootElement.Clone()
                )
                {
                    Preliminary = true,
                },
                new ToolResultStreamPart(
                    "call-1",
                    "image_generation",
                    JsonDocument.Parse("{\"result\":\"final-image\"}").RootElement.Clone()
                ),
                new FinishStreamPart(
                    FinishReason.Stop,
                    new LanguageModelUsage(3, 10, 13, noCacheInputTokens: 3, textTokens: 10),
                    "stop"
                ),
            },
        };
        return new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).StreamTextAsync(
            new StreamTextOptions
            {
                Model = model,
                Prompt = "test-input",
                Tools = new[]
                {
                    Tool.ProviderDefined("test.image_generation", "image_generation", args),
                },
                StopWhen = StopWhen.IsStepCount(4),
                NowMs = () => 0,
            }
        );
    }
}
