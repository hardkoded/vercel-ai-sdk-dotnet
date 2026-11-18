// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolCall.ExecuteToolCallSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolCall;

/// <summary>Port of <c>execute-tool-call.test.ts</c> &gt; <c>executeToolCall</c> &gt; <c>onToolExecutionStart callback</c>.</summary>
public sealed class OnToolExecutionStartCallbackTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/execute-tool-call.test.ts::executeToolCall > onToolExecutionStart callback::";

    [Fact]
    [UpstreamTest(
        Prefix + "should be called with correct data before execution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_called_with_correct_data_before_execution()
    {
        var toolExecutionStartEvents = new List<ToolExecutionStartEvent>();
        var executionOrder = new List<string>();
        var tool = ValueTool(
            (value, _) =>
            {
                executionOrder.Add("execute");
                return Task.FromResult(JsonSerializer.Serialize(value + "-result"));
            },
            Key1StringSchema
        );

        await Execute(
            CreateToolCall(),
            [tool],
            messages: [new UserModelMessage("test message")],
            toolsContext: Key1Context("value1"),
            onToolExecutionStart:
            [
                (@event, _) =>
                {
                    executionOrder.Add("onToolExecutionStart");
                    toolExecutionStartEvents.Add(@event);
                    return Task.CompletedTask;
                },
            ]
        );

        var start = Assert.Single(toolExecutionStartEvents);
        Assert.Equal("test-telemetry-call-id", start.CallId);
        Assert.Equal("test message", MessageText(Assert.Single(start.Messages)));
        Assert.Equal("call-1", start.ToolCall.ToolCallId);
        Assert.Equal("testTool", start.ToolCall.ToolName);
        Assert.Equal("{\"value\":\"test\"}", start.ToolCall.ArgumentsJson);
        Assert.False(start.ToolCall.Dynamic);
        Assert.Equal("{\"key1\":\"value1\"}", start.ToolContext!.Value.GetRawText());
        Assert.Equal(["onToolExecutionStart", "execute"], executionOrder);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not break execution when callback throws",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_break_execution_when_callback_throws()
    {
        var result = await Execute(
            CreateToolCall(),
            [ResultTool()],
            onToolExecutionStart: [(_, _) => throw new InvalidOperationException("callback error")]
        );

        var output = result!.Value.Output;
        Assert.False(output.IsError);
        Assert.Equal("\"test-result\"", output.OutputJson);
    }
}
