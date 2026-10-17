// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

public sealed class SingleProviderExecutedToolCallAndResultTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/stream-text.test.ts::streamText > provider-executed tools > single provider-executed tool call and result::should only execute a single step", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_only_execute_a_single_step()
    {
        var result = Run();
        Assert.Single(await result.Steps);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > provider-executed tools > single provider-executed tool call and result::should include provider-executed tool call and result content",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_provider_executed_tool_call_and_result_content()
    {
        var result = Run();
        var step = Assert.Single(await result.Steps);
        Assert.Equal(new[] { "tool-call", "tool-result", "tool-call", "tool-result" }, step.Content.Select(part => part.Type).ToArray());
        Assert.True(Assert.IsType<GeneratedToolCall>(step.Content[0]).ProviderExecuted);
        var result1 = Assert.IsType<GeneratedToolResult>(step.Content[1]);
        Assert.False(result1.IsError);
        Assert.Equal("result-1", result1.ProviderMetadata!.Value.GetProperty("provider").GetProperty("itemId").GetString());
        Assert.True(Assert.IsType<GeneratedToolCall>(step.Content[2]).ProviderExecuted);
        var result2 = Assert.IsType<GeneratedToolResult>(step.Content[3]);
        Assert.True(result2.IsError);
        Assert.Equal("error-1", result2.ProviderMetadata!.Value.GetProperty("provider").GetProperty("itemId").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > provider-executed tools > single provider-executed tool call and result::should include provider-executed tool call and result in the full stream",
        Coverage = UpstreamCoverage.Partial,
        Note = "The start-step part has no request, and finish-step has no response metadata. Performance values come from a real clock, so only their shape is asserted.")]
    public async Task Should_include_provider_executed_tool_call_and_result_in_the_full_stream()
    {
        var result = Run();
        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        Assert.Equal(
            new[] { "start", "start-step", "tool-input-start", "tool-input-delta", "tool-input-end", "tool-call", "tool-result", "tool-call", "tool-error", "finish-step", "finish" },
            parts.Select(part => part.Type).ToArray());
        Assert.Empty(Assert.IsType<StartStepPart>(parts[1]).Warnings);
        var inputStart = Assert.IsType<ToolInputStartPart>(parts[2]);
        Assert.Equal("call-1", inputStart.Id);
        Assert.Equal("web_search", inputStart.ToolName);
        Assert.True(inputStart.ProviderExecuted);
        Assert.False(inputStart.Dynamic);
        Assert.Equal("{ \"value\": \"value\" }", Assert.IsType<ToolInputDeltaPart>(parts[3]).Delta);
        Assert.Equal("call-1", Assert.IsType<ToolInputEndPart>(parts[4]).Id);
        var call1 = Assert.IsType<ToolCallPart>(parts[5]).ToolCall;
        Assert.Equal("call-1", call1.ToolCallId);
        Assert.True(call1.ProviderExecuted);
        var toolResult = Assert.IsType<ToolResultPart>(parts[6]).Result;
        Assert.Equal("call-1", toolResult.ToolCallId);
        Assert.Equal("{\"value\":\"result1\"}", toolResult.OutputJson);
        Assert.Equal("call-2", Assert.IsType<ToolCallPart>(parts[7]).ToolCall.ToolCallId);
        var toolError = Assert.IsType<ToolErrorPart>(parts[8]).Result;
        Assert.Equal("call-2", toolError.ToolCallId);
        Assert.Equal("\"ERROR\"", toolError.OutputJson);
        var finishStep = Assert.IsType<StepFinishPart>(parts[9]);
        Assert.Equal(FinishReason.Stop, finishStep.Step.FinishReason);
        Assert.Equal(13, finishStep.Step.Usage.TotalTokens);
        Assert.True(finishStep.Performance.StepTimeMs >= 0);
        Assert.True(finishStep.Performance.ResponseTimeMs >= 0);
        Assert.Empty(finishStep.Performance.ToolExecutionMs);
        Assert.NotNull(finishStep.Performance.TimeToFirstOutputMs);
        var finish = Assert.IsType<FinishPart>(parts[10]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal(13, finish.Usage.TotalTokens);
    }

    private static StreamTextResult Run()
    {
        var args = JsonDocument.Parse("{}").RootElement;
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ToolInputStartStreamPart("call-1", "web_search", providerExecuted: true),
                new ToolInputDeltaStreamPart("call-1", "{ \"value\": \"value\" }"),
                new ToolInputEndStreamPart("call-1"),
                new ToolCallStreamPart("call-1", "web_search", "{ \"value\": \"value\" }") { ProviderExecuted = true },
                new ToolResultStreamPart("call-1", "web_search", JsonDocument.Parse("{\"value\":\"result1\"}").RootElement.Clone()) { ProviderMetadata = JsonDocument.Parse("{\"provider\":{\"itemId\":\"result-1\"}}").RootElement.Clone() },
                new ToolCallStreamPart("call-2", "web_search", "{ \"value\": \"value\" }") { ProviderExecuted = true },
                new ToolResultStreamPart("call-2", "web_search", JsonDocument.Parse("\"ERROR\"").RootElement.Clone()) { IsError = true, ProviderMetadata = JsonDocument.Parse("{\"provider\":{\"itemId\":\"error-1\"}}").RootElement.Clone() },
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        };
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[] { Tool.ProviderDefined("test.web_search", "web_search", args) },
            StopWhen = StopWhen.IsStepCount(4),
        });
    }
}
