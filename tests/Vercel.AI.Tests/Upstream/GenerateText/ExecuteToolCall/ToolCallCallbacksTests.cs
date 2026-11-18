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

/// <summary>Port of <c>execute-tool-call.test.ts</c> &gt; <c>executeToolCall</c> &gt; <c>tool call callbacks</c>.</summary>
public sealed class ToolCallCallbacksTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/execute-tool-call.test.ts::executeToolCall > tool call callbacks::";

    [Fact]
    [UpstreamTest(
        Prefix + "should notify start and finish callbacks with success payload",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_notify_start_and_finish_callbacks_with_success_payload()
    {
        var startEvents = new List<ToolExecutionStartEvent>();
        var finishEvents = new List<ToolExecutionEndEvent>();

        await Execute(
            CreateToolCall(toolCallId: "my-call-id"),
            [ResultTool(Key1StringSchema)],
            messages: [new UserModelMessage("hello")],
            toolsContext: Key1Context("value1"),
            onToolExecutionStart:
            [
                (@event, _) =>
                {
                    startEvents.Add(@event);
                    return Task.CompletedTask;
                },
            ],
            onToolExecutionEnd:
            [
                (@event, _) =>
                {
                    finishEvents.Add(@event);
                    return Task.CompletedTask;
                },
            ]
        );

        var start = Assert.Single(startEvents);
        Assert.Equal("test-telemetry-call-id", start.CallId);
        Assert.Equal("hello", MessageText(Assert.Single(start.Messages)));
        Assert.Equal("my-call-id", start.ToolCall.ToolCallId);
        Assert.Equal("testTool", start.ToolCall.ToolName);
        Assert.Equal("{\"value\":\"test\"}", start.ToolCall.ArgumentsJson);
        Assert.False(start.ToolCall.Dynamic);
        Assert.Equal("{\"key1\":\"value1\"}", start.ToolContext!.Value.GetRawText());

        var finish = Assert.Single(finishEvents);
        Assert.Equal("test-telemetry-call-id", finish.CallId);
        Assert.Equal("hello", MessageText(Assert.Single(finish.Messages)));
        Assert.Equal("my-call-id", finish.ToolCall.ToolCallId);
        Assert.Equal("{\"key1\":\"value1\"}", finish.ToolContext!.Value.GetRawText());
        Assert.Equal(TimeSpan.Zero, finish.ToolExecutionTime);
        Assert.False(finish.ToolOutput.IsError);
        Assert.False(finish.ToolOutput.Dynamic);
        Assert.Equal("{\"value\":\"test\"}", finish.ToolOutput.InputJson);
        Assert.Equal("\"test-result\"", finish.ToolOutput.OutputJson);
        Assert.Equal("my-call-id", finish.ToolOutput.ToolCallId);
        Assert.Equal("testTool", finish.ToolOutput.ToolName);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should notify finish callback with error payload",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_notify_finish_callback_with_error_payload()
    {
        var toolError = new InvalidOperationException("test error");
        var toolExecutionEndEvents = new List<ToolExecutionEndEvent>();

        await Execute(
            CreateToolCall(),
            [FailingTool(toolError)],
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
        Assert.Equal("{\"value\":\"test\"}", end.ToolCall.ArgumentsJson);
        Assert.False(end.ToolCall.Dynamic);
        Assert.Null(end.ToolContext);
        Assert.Equal(TimeSpan.Zero, end.ToolExecutionTime);
        Assert.True(end.ToolOutput.IsError);
        Assert.False(end.ToolOutput.Dynamic);
        Assert.Same(toolError, end.ToolOutput.Error);
        Assert.Equal("{\"value\":\"test\"}", end.ToolOutput.InputJson);
        Assert.Equal("call-1", end.ToolOutput.ToolCallId);
        Assert.Equal("testTool", end.ToolOutput.ToolName);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should execute the tool inside the executeToolInTelemetryContext wrapper when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_execute_the_tool_inside_the_executeToolInTelemetryContext_wrapper_when_provided()
    {
        var calls = new List<ExecuteToolInTelemetryContextParams>();

        await Execute(
            CreateToolCall(toolCallId: "my-call-id"),
            [ResultTool()],
            executeToolInTelemetryContext: parameters =>
            {
                calls.Add(parameters);
                return parameters.Execute();
            }
        );

        var call = Assert.Single(calls);
        Assert.Equal("test-telemetry-call-id", call.CallId);
        Assert.Equal("my-call-id", call.ToolCallId);
        Assert.Empty(call.Messages);
        Assert.Equal("my-call-id", call.ToolCall.ToolCallId);
        Assert.Equal("testTool", call.ToolCall.ToolName);
        Assert.Equal("{\"value\":\"test\"}", call.ToolCall.ArgumentsJson);
        Assert.False(call.ToolCall.Dynamic);
        Assert.Null(call.ToolContext);
        Assert.NotNull(call.Execute);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should measure only the inner execute duration when wrapped in telemetry context",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_measure_only_the_inner_execute_duration_when_wrapped_in_telemetry_context()
    {
        var toolExecutionEndEvents = new List<ToolExecutionEndEvent>();
        var now = MockNow(1000, 2000, 2300, 5000);

        await Execute(
            CreateToolCall(toolCallId: "my-call-id"),
            [ResultTool()],
            now: now,
            executeToolInTelemetryContext: async parameters =>
            {
                now(); // simulate wrapper overhead before the tool runs
                await parameters.Execute();
                now(); // simulate wrapper overhead after the tool runs
            },
            onToolExecutionEnd:
            [
                (@event, _) =>
                {
                    toolExecutionEndEvents.Add(@event);
                    return Task.CompletedTask;
                },
            ]
        );

        Assert.Equal(TimeSpan.FromMilliseconds(300), toolExecutionEndEvents[0].ToolExecutionTime);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should execute the tool directly when executeToolInTelemetryContext is not provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_execute_the_tool_directly_when_executeToolInTelemetryContext_is_not_provided()
    {
        var result = await Execute(CreateToolCall(toolCallId: "my-call-id"), [ResultTool()]);

        var output = result!.Value.Output;
        Assert.False(output.IsError);
        Assert.Equal("\"test-result\"", output.OutputJson);
    }
}
