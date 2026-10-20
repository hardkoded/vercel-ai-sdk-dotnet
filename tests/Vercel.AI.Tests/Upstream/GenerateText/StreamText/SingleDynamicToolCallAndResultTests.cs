// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Tests.Upstream.GenerateText;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; dynamic tools &gt; single dynamic tool call and result</c>.</summary>
public sealed class SingleDynamicToolCallAndResultTests
{
    private const string Prefix = "packages/ai/src/generate-text/stream-text.test.ts::streamText > dynamic tools > single dynamic tool call and result::";

    private const string Input = "{ \"value\": \"value\" }";

    [Fact]
    [UpstreamTest(Prefix + "should include dynamic tool call and result content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_dynamic_tool_call_and_result_content()
    {
        var step = Assert.Single(await Run().Steps);

        Assert.Equal(new[] { "tool-call", "tool-result" }, step.Content.Select(part => part.Type));
        var call = Assert.IsType<GeneratedToolCall>(step.Content[0]);
        Assert.True(call.Dynamic);
        Assert.Equal("{\"value\":\"value\"}", Compact(call.ArgumentsJson));
        Assert.False(call.ProviderExecuted);
        Assert.Null(call.ProviderMetadata);
        Assert.Null(call.Title);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("dynamicTool", call.ToolName);
        var toolResult = Assert.IsType<GeneratedToolResult>(step.Content[1]);
        Assert.True(toolResult.Dynamic);
        Assert.Equal("{\"value\":\"test-result\"}", Compact(toolResult.ResultJson));
        Assert.Equal("call-1", toolResult.ToolCallId);
        Assert.Equal("dynamicTool", toolResult.ToolName);
        var executed = Assert.Single(step.ToolResults);
        Assert.True(executed.Dynamic);
        Assert.Equal("{\"value\":\"value\"}", Compact(executed.InputJson!));
    }

    [Fact]
    [UpstreamTest(Prefix + "should include dynamic tool call and result in the full stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_dynamic_tool_call_and_result_in_the_full_stream()
    {
        var parts = new List<TextStreamPart>();
        await foreach (var part in Run().Stream())
        {
            parts.Add(part);
        }

        Assert.Equal(
            new[] { "start", "start-step", "tool-input-start", "tool-input-delta", "tool-input-end", "tool-call", "tool-result", "finish-step", "finish" },
            parts.Select(part => part.Type).ToArray());
        var startStep = Assert.IsType<StartStepPart>(parts[1]);
        Assert.Null(startStep.Request.Body);
        Assert.Null(startStep.Request.Messages);
        Assert.Empty(startStep.Warnings);
        var inputStart = Assert.IsType<ToolInputStartPart>(parts[2]);
        Assert.True(inputStart.Dynamic);
        Assert.Equal("call-1", inputStart.Id);
        Assert.Null(inputStart.Title);
        Assert.Equal("dynamicTool", inputStart.ToolName);
        var inputDelta = Assert.IsType<ToolInputDeltaPart>(parts[3]);
        Assert.Equal(Input, inputDelta.Delta);
        Assert.Equal("call-1", inputDelta.Id);
        Assert.Equal("call-1", Assert.IsType<ToolInputEndPart>(parts[4]).Id);
        var call = Assert.IsType<ToolCallPart>(parts[5]).ToolCall;
        Assert.True(call.Dynamic);
        Assert.Equal("{\"value\":\"value\"}", Compact(call.ArgumentsJson));
        Assert.False(call.ProviderExecuted);
        Assert.Null(call.ProviderMetadata);
        Assert.Null(call.Title);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("dynamicTool", call.ToolName);
        var toolResult = Assert.IsType<ToolResultPart>(parts[6]).Result;
        Assert.True(toolResult.Dynamic);
        Assert.Equal("{\"value\":\"value\"}", Compact(toolResult.InputJson!));
        Assert.Equal("{\"value\":\"test-result\"}", Compact(toolResult.OutputJson));
        Assert.Equal("call-1", toolResult.ToolCallId);
        Assert.Equal("dynamicTool", toolResult.ToolName);
        var finishStep = Assert.IsType<StepFinishPart>(parts[7]);
        Assert.Equal(FinishReason.ToolCalls, finishStep.Step.FinishReason);
        Assert.Null(finishStep.RawFinishReason);
        Assert.Null(finishStep.ProviderMetadata);
        Assert.Null(finishStep.Response.Headers);
        Assert.Equal("id-0", finishStep.Response.Id);
        Assert.Equal("mock-model-id", finishStep.Response.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(0), finishStep.Response.Timestamp);
        var performance = finishStep.Performance;
        Assert.Equal(0, performance.EffectiveOutputTokensPerSecond);
        Assert.Equal(0, performance.EffectiveTotalTokensPerSecond);
        Assert.Equal(0, performance.InputTokensPerSecond);
        Assert.Equal(0, performance.OutputTokensPerSecond);
        Assert.Equal(0, performance.ResponseTimeMs);
        Assert.Equal(0, performance.StepTimeMs);
        Assert.Equal(0, performance.TimeToFirstOutputMs);
        Assert.Equal(0, performance.ToolExecutionMs["call-1"]);
        var finish = Assert.IsType<FinishPart>(parts[8]);
        Assert.Equal(FinishReason.ToolCalls, finish.FinishReason);
        foreach (var usage in new[] { finishStep.Step.Usage, finish.Usage })
        {
            Assert.Equal(3, usage.InputTokens);
            Assert.Equal(10, usage.OutputTokens);
            Assert.Equal(13, usage.TotalTokens);
            Assert.Equal(3, usage.NoCacheInputTokens);
            Assert.Equal(10, usage.TextTokens);
            Assert.Null(usage.CacheReadTokens);
            Assert.Null(usage.CacheWriteTokens);
            Assert.Null(usage.ReasoningTokens);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "should include dynamic tool call and result in the ui message stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_dynamic_tool_call_and_result_in_the_ui_message_stream()
    {
        var chunks = await SingleProviderExecutedToolCallAndResultTests.ReadUiChunks(Run());

        // The .NET stream always sends a generated messageId on start.
        Assert.NotNull(chunks[0]!["messageId"]);
        chunks[0]!.AsObject().Remove("messageId");
        var expected = JsonNode.Parse("""
            [
              { "type": "start" },
              { "type": "start-step" },
              { "dynamic": true, "toolCallId": "call-1", "toolName": "dynamicTool", "type": "tool-input-start" },
              { "inputTextDelta": "{ \"value\": \"value\" }", "toolCallId": "call-1", "type": "tool-input-delta" },
              { "dynamic": true, "input": { "value": "value" }, "toolCallId": "call-1", "toolName": "dynamicTool", "type": "tool-input-available" },
              { "dynamic": true, "output": { "value": "test-result" }, "toolCallId": "call-1", "type": "tool-output-available" },
              { "type": "finish-step" },
              { "finishReason": "tool-calls", "type": "finish" }
            ]
            """);
        Assert.True(JsonNode.DeepEquals(expected, chunks), chunks.ToJsonString());
    }

    private static string Compact(string json) => JsonSerializer.Serialize(JsonDocument.Parse(json).RootElement);

    private static StreamTextResult Run()
    {
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ResponseMetadataStreamPart("id-0", "mock-model-id", DateTimeOffset.FromUnixTimeMilliseconds(0)),
                new ToolInputStartStreamPart("call-1", "dynamicTool"),
                new ToolInputDeltaStreamPart("call-1", Input),
                new ToolInputEndStreamPart("call-1"),
                new ToolCallStreamPart("call-1", "dynamicTool", Input),
                new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13, noCacheInputTokens: 3, textTokens: 10), null),
            },
        };
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[]
            {
                Tool.DynamicTool("dynamicTool", null, "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}", (_, _) => Task.FromResult("{\"value\":\"test-result\"}")),
            },
            NowMs = () => 0,
        });
    }
}
