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

/// <summary>Port of <c>execute-tool-call.test.ts</c> &gt; <c>executeToolCall</c> &gt; <c>onToolExecutionEnd callback</c>.</summary>
public sealed class OnToolExecutionEndCallbackTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/execute-tool-call.test.ts::executeToolCall > onToolExecutionEnd callback::";

    [Fact]
    [UpstreamTest(
        Prefix + "should be called with success data when tool succeeds",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_called_with_success_data_when_tool_succeeds()
    {
        var toolExecutionEndEvents = new List<ToolExecutionEndEvent>();

        await Execute(
            CreateToolCall(),
            [ResultTool(Key1StringSchema)],
            messages: [new UserModelMessage("test message")],
            toolsContext: Key1Context("value1"),
            now: MockNow(1000, 1050),
            onToolExecutionEnd:
            [
                (@event, _) =>
                {
                    toolExecutionEndEvents.Add(@event);
                    return Task.CompletedTask;
                },
            ]
        );

        var end = Assert.Single(toolExecutionEndEvents);
        Assert.Equal("test-telemetry-call-id", end.CallId);
        Assert.Equal("test message", MessageText(Assert.Single(end.Messages)));
        Assert.Equal("call-1", end.ToolCall.ToolCallId);
        Assert.Equal("testTool", end.ToolCall.ToolName);
        Assert.Equal("{\"value\":\"test\"}", end.ToolCall.ArgumentsJson);
        Assert.Equal("{\"key1\":\"value1\"}", end.ToolContext!.Value.GetRawText());
        Assert.Equal(TimeSpan.FromMilliseconds(50), end.ToolExecutionTime);
        Assert.False(end.ToolOutput.IsError);
        Assert.False(end.ToolOutput.Dynamic);
        Assert.Equal("{\"value\":\"test\"}", end.ToolOutput.InputJson);
        Assert.Equal("\"test-result\"", end.ToolOutput.OutputJson);
        Assert.Equal("call-1", end.ToolOutput.ToolCallId);
        Assert.Equal("testTool", end.ToolOutput.ToolName);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should be called with error data when tool fails",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_called_with_error_data_when_tool_fails()
    {
        var toolExecutionEndEvents = new List<ToolExecutionEndEvent>();
        var toolError = new InvalidOperationException("execution failed");

        await Execute(
            CreateToolCall(),
            [FailingTool(toolError, Key1StringSchema)],
            toolsContext: Key1Context("value1"),
            now: MockNow(2000, 2100),
            onToolExecutionEnd:
            [
                (@event, _) =>
                {
                    toolExecutionEndEvents.Add(@event);
                    return Task.CompletedTask;
                },
            ]
        );

        var end = Assert.Single(toolExecutionEndEvents);
        Assert.Equal("test-telemetry-call-id", end.CallId);
        Assert.Empty(end.Messages);
        Assert.Equal("call-1", end.ToolCall.ToolCallId);
        Assert.Equal("testTool", end.ToolCall.ToolName);
        Assert.Equal("{\"key1\":\"value1\"}", end.ToolContext!.Value.GetRawText());
        Assert.Equal(TimeSpan.FromMilliseconds(100), end.ToolExecutionTime);
        Assert.True(end.ToolOutput.IsError);
        Assert.False(end.ToolOutput.Dynamic);
        Assert.Same(toolError, end.ToolOutput.Error);
        Assert.Equal("{\"value\":\"test\"}", end.ToolOutput.InputJson);
        Assert.Equal("call-1", end.ToolOutput.ToolCallId);
        Assert.Equal("testTool", end.ToolOutput.ToolName);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not break execution when callback throws on success",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_break_execution_when_callback_throws_on_success()
    {
        var result = await Execute(
            CreateToolCall(),
            [ResultTool()],
            onToolExecutionEnd: [(_, _) => throw new InvalidOperationException("callback error")]
        );

        var output = result!.Value.Output;
        Assert.False(output.IsError);
        Assert.Equal("\"test-result\"", output.OutputJson);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not break execution when callback throws on error",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_break_execution_when_callback_throws_on_error()
    {
        var toolError = new InvalidOperationException("tool error");

        var result = await Execute(
            CreateToolCall(),
            [FailingTool(toolError)],
            onToolExecutionEnd: [(_, _) => throw new InvalidOperationException("callback error")]
        );

        var output = result!.Value.Output;
        Assert.True(output.IsError);
        Assert.Same(toolError, output.Error);
    }
}
