// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

using Vercel.AI.Tests.Upstream.GenerateText;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

public sealed class WhenAProviderExecutedToolEmitsToolApprovalRequestTests
{
    private const string Suite = "packages/ai/src/generate-text/stream-text.test.ts::streamText > tool execution approval > provider-executed tool (MCP) approval > when a provider-executed tool emits tool-approval-request::";

    [Fact]
    [UpstreamTest(Suite + "should add provider-executed tool approval request to full stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_add_provider_executed_tool_approval_request_to_full_stream()
    {
        var result = Run();
        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        Assert.Equal(new[] { "start", "start-step", "tool-call", "tool-approval-request", "finish-step", "finish" }, parts.Select(part => part.Type).ToArray());
        var startStep = Assert.IsType<StartStepPart>(parts[1]);
        Assert.Null(startStep.Request.Body);
        Assert.Null(startStep.Request.Messages);
        Assert.Empty(startStep.Warnings);
        var call = Assert.IsType<ToolCallPart>(parts[2]).ToolCall;
        Assert.True(call.ProviderExecuted);
        Assert.Equal("mcp-call-1", call.ToolCallId);
        Assert.Equal("mcp_tool", call.ToolName);
        Assert.Equal("{ \"query\": \"test\" }", call.ArgumentsJson);
        var request = Assert.IsType<ToolApprovalRequestPart>(parts[3]);
        Assert.Equal("mcp-approval-1", request.ApprovalId);
        Assert.Same(call, request.ToolCall);
        var finishStep = Assert.IsType<StepFinishPart>(parts[4]);
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
        Assert.Null(performance.TimeBetweenOutputChunksMs);
        Assert.Equal(0, performance.TimeToFirstOutputMs);
        Assert.Empty(performance.ToolExecutionMs);
        var finish = Assert.IsType<FinishPart>(parts[5]);
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
    [UpstreamTest(Suite + "should add provider-executed tool approval request to content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_add_provider_executed_tool_approval_request_to_content()
    {
        var step = Assert.Single(await Run().Steps);
        Assert.Equal(new[] { "tool-call", "tool-approval-request" }, step.Content.Select(part => part.Type).ToArray());
        var call = Assert.IsType<GeneratedToolCall>(step.Content[0]);
        Assert.True(call.ProviderExecuted);
        var request = Assert.IsType<GeneratedToolApprovalRequest>(step.Content[1]);
        Assert.Equal("mcp-approval-1", request.ApprovalId);
        Assert.Same(call, request.ToolCall);
    }

    [Fact]
    [UpstreamTest(Suite + "should have tool-calls finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_tool_calls_finish_reason()
    {
        Assert.Equal(FinishReason.ToolCalls, await Run().FinishReason);
    }

    private static StreamTextResult Run()
    {
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new StreamStartStreamPart(Array.Empty<CallWarning>()),
                new ResponseMetadataStreamPart("id-0", "mock-model-id", DateTimeOffset.FromUnixTimeMilliseconds(0)),
                new ToolCallStreamPart("mcp-call-1", "mcp_tool", "{ \"query\": \"test\" }") { ProviderExecuted = true },
                new ToolApprovalRequestStreamPart("mcp-approval-1", "mcp-call-1"),
                new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13, noCacheInputTokens: 3, textTokens: 10), null),
            },
        };
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[] { ProviderExecutedToolApprovalSupport.McpTool() },
            NowMs = () => 0,
        });
    }
}
