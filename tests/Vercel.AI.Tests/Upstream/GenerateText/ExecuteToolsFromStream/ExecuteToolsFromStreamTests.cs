// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolsFromStream.ExecuteToolsFromStreamSupport;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolCallConcurrency.ToolCallConcurrencySupport;
using Transformer = Vercel.AI.GenerateText.ExecuteToolsFromStream;

namespace Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolsFromStream;

/// <summary>Port of <c>execute-tools-from-stream.test.ts</c> &gt; <c>executeToolsFromStream</c>.</summary>
public sealed class ExecuteToolsFromStreamTests
{
    private const string Id = Prefix + "executeToolsFromStream::";

    // The parts after the tool call: model-call-end, tool-execution-end, and the result, as the upstream snapshots show them.
    private static void AssertExecuted(
        IReadOnlyList<TextStreamPart> result,
        string toolName,
        string input,
        string output
    )
    {
        Assert.Equal("tool-call,model-call-end,tool-execution-end,tool-result", Types(result));
        var end = Assert.IsType<Transformer.ToolExecutionEndPart>(result[2]);
        Assert.Equal("call-1", end.ToolCallId);
        Assert.Equal(0, end.ToolExecutionMs);
        var toolResult = Assert.IsType<ToolResultPart>(result[3]).Result;
        Assert.False(toolResult.Dynamic);
        Assert.Equal(input, toolResult.InputJson);
        Assert.Equal(output, toolResult.OutputJson);
        Assert.Equal("call-1", toolResult.ToolCallId);
        Assert.Equal(toolName, toolResult.ToolName);
    }

    [Fact]
    [UpstreamTest(Id + "should handle async tool execution", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_async_tool_execution()
    {
        var tools = new[]
        {
            ValueTool("syncTool", (value, _) =>
                Task.FromResult(JsonSerializer.Serialize(value + "-sync-result"))
            ),
        };

        var result = await Run(Options(tools), Call("call-1", "syncTool"), FinishChunk());

        AssertExecuted(result, "syncTool", "{\"value\":\"test\"}", "\"test-sync-result\"");
    }

    [Fact]
    [UpstreamTest(
        Id + "should execute tool calls sequentially in stream order with concurrency 1",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_execute_tool_calls_sequentially_in_stream_order_with_concurrency_1()
    {
        var firstStarted = new DelayedPromise<bool>();
        var secondStarted = new DelayedPromise<bool>();
        var releaseFirst = new DelayedPromise<bool>();
        var events = new ConcurrentQueue<string>();
        var tools = new[] { Tool.Function("first", null, "{\"type\":\"object\"}", async (_, _) =>
            {
                events.Enqueue("first:start");
                firstStarted.Resolve(true);
                await releaseFirst.Promise;
                events.Enqueue("first:end");
                return "null";
            }), Tool.Function("second", null, "{\"type\":\"object\"}", (_, _) =>
            {
                events.Enqueue("second:start");
                secondStarted.Resolve(true);
                events.Enqueue("second:end");
                return Task.FromResult("null");
            }) };

        var chunksTask = Run(
            Options(tools, options => options.ToolCallConcurrency = 1),
            Call("call-1", "first", "{}"),
            Call("call-2", "second", "{}"),
            FinishChunk()
        );

        await firstStarted.Promise.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(new[] { "first:start" }, events.ToArray());
        releaseFirst.Resolve(true);
        await secondStarted.Promise.WaitAsync(TimeSpan.FromSeconds(5));
        await WaitForEvents(events, "first:start", "first:end", "second:start", "second:end");
        await chunksTask;
    }

    [Fact]
    [UpstreamTest(Id + "should handle sync tool execution", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_sync_tool_execution()
    {
        var tools = new[]
        {
            ValueTool("syncTool", (value, _) =>
                Task.FromResult(JsonSerializer.Serialize(value + "-sync-result"))
            ),
        };

        var result = await Run(Options(tools), Call("call-1", "syncTool"), FinishChunk());

        AssertExecuted(result, "syncTool", "{\"value\":\"test\"}", "\"test-sync-result\"");
    }

    [Theory]
    [InlineData(FinishReason.Length)]
    [InlineData(FinishReason.Error)]
    [InlineData(FinishReason.ContentFilter)]
    [InlineData(FinishReason.Interrupted)]
    [InlineData(FinishReason.Other)]
    [UpstreamTest(
        Id + "should not execute tools when the finish reason is %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_execute_tools_when_the_finish_reason_is(FinishReason finishReason)
    {
        var executed = false;
        var tools = new[] { ValueTool("testTool", (_, _) =>
            {
                executed = true;
                return Task.FromResult("\"tool-result\"");
            }) };

        await Run(Options(tools), Call("call-1", "testTool"), FinishChunk(finishReason));

        Assert.False(executed);
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass sandbox to tool execution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_sandbox_to_tool_execution()
    {
        var sandbox = new Vercel.AI.Tests.Upstream.ToolLoopAgent.TestSandboxSession();
        ISandboxSession? receivedSandbox = null;
        var tool = Tool.Function("sandboxTool", null, ValueSchema, null);
        tool.ExecuteWithOptions = (input, options) =>
        {
            receivedSandbox = options.ExperimentalSandbox;
            return Task.FromResult(
                JsonSerializer.Serialize(input.GetProperty("value").GetString() + "-sandbox-result")
            );
        };

        await Run(
            Options(new[] { tool }, options => options.ExperimentalSandbox = sandbox),
            Call("call-1", "sandboxTool"),
            FinishChunk()
        );

        Assert.Same(sandbox, receivedSandbox);
    }

    [Fact]
    [UpstreamTest(
        Id + "should not call execute for provider-executed tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_call_execute_for_provider_executed_tool_calls()
    {
        var toolExecuted = false;
        var tools = new[] { ValueTool("providerTool", (_, _) =>
            {
                toolExecuted = true;
                return Task.FromResult("\"should-not-execute\"");
            }) };

        await Run(
            Options(tools),
            Call("call-1", "providerTool", providerExecuted: true),
            new ToolResultPart(
                new ExecutedTool("call-1", "providerTool", "\"example-result\"", false)
                {
                    ProviderExecuted = true,
                    InputJson = "{\"value\":\"test\"}",
                }
            ),
            FinishChunk()
        );

        Assert.False(toolExecuted);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "should emit approval request and approved response before executing auto-approved tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_emit_approval_request_and_approved_response_before_executing_auto_approved_tools()
    {
        var executions = 0;
        var tools = new[] { ValueTool("approvedTool", (value, _) =>
            {
                executions++;
                return Task.FromResult(JsonSerializer.Serialize(value + "-approved-result"));
            }) };

        var result = await Run(
            Options(tools, options =>
                options.ToolApproval = new ToolApprovalConfiguration
                {
                    { "approvedTool", new ToolApprovalStatus("approved", "trusted internal tool") },
                }
            ),
            Call("call-1", "approvedTool"),
            FinishChunk()
        );

        Assert.Equal(
            "tool-call,tool-approval-request,tool-approval-response,model-call-end,tool-execution-end,tool-result",
            Types(result)
        );
        var request = Assert.IsType<ToolApprovalRequestPart>(result[1]);
        Assert.Equal("id-0", request.ApprovalId);
        Assert.True(request.IsAutomatic);
        Assert.Equal("call-1", request.ToolCall.ToolCallId);
        Assert.Equal("approvedTool", request.ToolCall.ToolName);
        var response = Assert.IsType<ToolApprovalResponsePart>(result[2]);
        Assert.Equal("id-0", response.ApprovalId);
        Assert.True(response.Approved);
        Assert.False(response.ProviderExecuted);
        Assert.Equal("trusted internal tool", response.Reason);
        Assert.Equal("call-1", response.ToolCall.ToolCallId);
        var end = Assert.IsType<Transformer.ToolExecutionEndPart>(result[4]);
        Assert.Equal("call-1", end.ToolCallId);
        Assert.Equal(0, end.ToolExecutionMs);
        var toolResult = Assert.IsType<ToolResultPart>(result[5]).Result;
        Assert.Equal("{\"value\":\"test\"}", toolResult.InputJson);
        Assert.Equal("\"test-approved-result\"", toolResult.OutputJson);
        Assert.Equal(1, executions);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "should emit denied output after the approval response without executing auto-denied tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_emit_denied_output_after_the_approval_response_without_executing_auto_denied_tools()
    {
        var toolExecuted = false;
        var tools = new[] { ValueTool("deniedTool", (_, _) =>
            {
                toolExecuted = true;
                return Task.FromResult("\"denied-result\"");
            }) };

        var result = await Run(
            Options(tools, options =>
                options.ToolApproval = new ToolApprovalConfiguration
                {
                    { "deniedTool", new ToolApprovalStatus("denied", "blocked by policy") },
                }
            ),
            Call("call-1", "deniedTool"),
            FinishChunk()
        );

        Assert.Equal(
            "tool-call,tool-approval-request,tool-approval-response,tool-output-denied,model-call-end",
            Types(result)
        );
        var request = Assert.IsType<ToolApprovalRequestPart>(result[1]);
        Assert.Equal("id-0", request.ApprovalId);
        Assert.True(request.IsAutomatic);
        Assert.Equal("deniedTool", request.ToolCall.ToolName);
        var response = Assert.IsType<ToolApprovalResponsePart>(result[2]);
        Assert.Equal("id-0", response.ApprovalId);
        Assert.False(response.Approved);
        Assert.False(response.ProviderExecuted);
        Assert.Equal("blocked by policy", response.Reason);
        var denied = Assert.IsType<ToolOutputDeniedPart>(result[3]);
        Assert.Equal("call-1", denied.ToolCallId);
        Assert.Equal("deniedTool", denied.ToolName);
        Assert.False(toolExecuted);
    }
}
