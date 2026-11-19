// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolsFromStream.ExecuteToolsFromStreamSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolsFromStream;

/// <summary>Port of <c>execute-tools-from-stream.test.ts</c> &gt; <c>executeToolsFromStream</c> &gt; <c>onToolExecutionStart and onToolExecutionEnd callbacks</c>.</summary>
public sealed class OnToolExecutionStartAndOnToolExecutionEndCallbacksTests
{
    private const string Id =
        Prefix + "executeToolsFromStream > onToolExecutionStart and onToolExecutionEnd callbacks::";

    private static Tool ResultTool(
        string name = "testTool",
        Func<string, CancellationToken, Task<string>>? execute = null
    ) =>
        ValueTool(
            name,
            execute ?? ((value, _) => Task.FromResult(JsonSerializer.Serialize(value + "-result")))
        );

    private static GenerateTextOptions Recording(
        IReadOnlyList<Tool> tools,
        ConcurrentQueue<ToolExecutionStartEvent> starts,
        ConcurrentQueue<ToolExecutionEndEvent> ends,
        Action<GenerateTextOptions>? configure = null
    ) => Options(tools, options =>
        {
            options.OnToolExecutionStart = (@event, _) =>
            {
                starts.Enqueue(@event);
                return Task.CompletedTask;
            };
            options.OnToolExecutionEnd = (@event, _) =>
            {
                ends.Enqueue(@event);
                return Task.CompletedTask;
            };
            configure?.Invoke(options);
        });

    [Fact]
    [UpstreamTest(
        Id + "should call onToolExecutionStart before tool execution and onToolExecutionEnd after",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onToolExecutionStart_before_tool_execution_and_onToolExecutionEnd_after()
    {
        var callOrder = new ConcurrentQueue<string>();
        var tools = new[]
        {
            ResultTool(
                execute: (value, _) =>
                {
                    callOrder.Enqueue("execute");
                    return Task.FromResult(JsonSerializer.Serialize(value + "-result"));
                }
            ),
        };

        await Run(Options(tools, options =>
            {
                options.OnToolExecutionStart = (_, _) =>
                {
                    callOrder.Enqueue("onToolExecutionStart");
                    return Task.CompletedTask;
                };
                options.OnToolExecutionEnd = (_, _) =>
                {
                    callOrder.Enqueue("onToolExecutionEnd");
                    return Task.CompletedTask;
                };
            }), Call("call-1", "testTool", "{\"value\":\"hello\"}"), FinishChunk());

        Assert.Equal(
            new[] { "onToolExecutionStart", "execute", "onToolExecutionEnd" },
            callOrder.ToArray()
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass stepNumber and model to callbacks",
        Coverage = UpstreamCoverage.Partial,
        Note = "ToolExecutionStartEvent has no StepNumber or Model. The test checks the other fields of the upstream snapshots."
    )]
    public async Task Should_pass_the_call_id_and_context_to_callbacks()
    {
        var starts = new ConcurrentQueue<ToolExecutionStartEvent>();
        var ends = new ConcurrentQueue<ToolExecutionEndEvent>();

        await Run(
            Recording(new[] { ResultTool() }, starts, ends, options =>
                options.ToolsContext = new Dictionary<string, JsonElement>
                {
                    ["testTool"] = JsonDocument.Parse("{\"value\":\"test\"}").RootElement.Clone(),
                }
            ),
            Call("call-1", "testTool"),
            FinishChunk()
        );

        var start = Assert.Single(starts);
        Assert.Equal("test-telemetry-call-id", start.CallId);
        Assert.Empty(start.Messages);
        Assert.Equal("call-1", start.ToolCall.ToolCallId);
        Assert.Equal("testTool", start.ToolCall.ToolName);
        Assert.Equal("{\"value\":\"test\"}", start.ToolCall.ArgumentsJson);
        Assert.Equal("{\"value\":\"test\"}", Raw(start.ToolContext));
        var end = Assert.Single(ends);
        Assert.Equal("test-telemetry-call-id", end.CallId);
        Assert.Empty(end.Messages);
        Assert.Equal("call-1", end.ToolCall.ToolCallId);
        Assert.Equal("{\"value\":\"test\"}", Raw(end.ToolContext));
        Assert.Equal(TimeSpan.Zero, end.ToolExecutionTime);
        Assert.Equal("\"test-result\"", end.ToolOutput.OutputJson);
    }

    [Fact]
    [UpstreamTest(
        Id + "should call onToolExecutionEnd with success data",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onToolExecutionEnd_with_success_data()
    {
        var starts = new ConcurrentQueue<ToolExecutionStartEvent>();
        var ends = new ConcurrentQueue<ToolExecutionEndEvent>();

        await Run(
            Recording(new[] { ResultTool() }, starts, ends),
            Call("call-1", "testTool", "{\"value\":\"abc\"}"),
            FinishChunk()
        );

        var end = Assert.Single(ends);
        Assert.Equal("test-telemetry-call-id", end.CallId);
        Assert.Empty(end.Messages);
        Assert.Equal("call-1", end.ToolCall.ToolCallId);
        Assert.Equal("undefined", Raw(end.ToolContext));
        Assert.Equal(TimeSpan.Zero, end.ToolExecutionTime);
        Assert.False(end.ToolOutput.IsError);
        Assert.False(end.ToolOutput.Dynamic);
        Assert.Equal("{\"value\":\"abc\"}", end.ToolOutput.InputJson);
        Assert.Equal("\"abc-result\"", end.ToolOutput.OutputJson);
        Assert.Equal("call-1", end.ToolOutput.ToolCallId);
        Assert.Equal("testTool", end.ToolOutput.ToolName);
    }

    [Fact]
    [UpstreamTest(
        Id + "should call onToolExecutionEnd with error data when tool fails",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onToolExecutionEnd_with_error_data_when_tool_fails()
    {
        var starts = new ConcurrentQueue<ToolExecutionStartEvent>();
        var ends = new ConcurrentQueue<ToolExecutionEndEvent>();
        var tools = new[]
        {
            ResultTool("failingTool", (_, _) => throw new InvalidOperationException("tool failed")),
        };

        await Run(Recording(tools, starts, ends), Call("call-1", "failingTool"), FinishChunk());

        var end = Assert.Single(ends);
        Assert.Equal("test-telemetry-call-id", end.CallId);
        Assert.Empty(end.Messages);
        Assert.Equal("call-1", end.ToolCall.ToolCallId);
        Assert.Equal("undefined", Raw(end.ToolContext));
        Assert.Equal(TimeSpan.Zero, end.ToolExecutionTime);
        Assert.True(end.ToolOutput.IsError);
        Assert.False(end.ToolOutput.Dynamic);
        Assert.Equal("tool failed", end.ToolOutput.Error!.Message);
        Assert.Equal("{\"value\":\"test\"}", end.ToolOutput.InputJson);
        Assert.Equal("call-1", end.ToolOutput.ToolCallId);
        Assert.Equal("failingTool", end.ToolOutput.ToolName);
    }

    [Fact]
    [UpstreamTest(
        Id + "should not call callbacks for tools without execute",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_call_callbacks_for_tools_without_execute()
    {
        var starts = new ConcurrentQueue<ToolExecutionStartEvent>();
        var ends = new ConcurrentQueue<ToolExecutionEndEvent>();

        await Run(
            Recording(new[] { ValueTool("noExecuteTool", null) }, starts, ends),
            Call("call-1", "noExecuteTool"),
            FinishChunk()
        );

        Assert.Empty(starts);
        Assert.Empty(ends);
    }

    [Fact]
    [UpstreamTest(
        Id + "should call callbacks for each tool in a multi-tool stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_callbacks_for_each_tool_in_a_multi_tool_stream()
    {
        var starts = new ConcurrentQueue<ToolExecutionStartEvent>();
        var ends = new ConcurrentQueue<ToolExecutionEndEvent>();

        await Run(
            Recording(new[] { ResultTool() }, starts, ends),
            Call("call-1", "testTool", "{\"value\":\"a\"}"),
            Call("call-2", "testTool", "{\"value\":\"b\"}"),
            FinishChunk()
        );

        Assert.Equal(new[] { "call-1", "call-2" }, starts.Select(e => e.ToolCall.ToolCallId));
        Assert.All(starts, start => Assert.Equal("test-telemetry-call-id", start.CallId));
        Assert.Equal(new[] { "call-1", "call-2" }, ends.Select(e => e.ToolCall.ToolCallId));
        Assert.Equal(
            new[] { "\"a-result\"", "\"b-result\"" },
            ends.Select(e => e.ToolOutput.OutputJson)
        );
        Assert.All(ends, end => Assert.Equal(TimeSpan.Zero, end.ToolExecutionTime));
    }

    [Fact]
    [UpstreamTest(
        Id + "should not call callbacks for provider-executed tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_call_callbacks_for_provider_executed_tools()
    {
        var starts = new ConcurrentQueue<ToolExecutionStartEvent>();
        var ends = new ConcurrentQueue<ToolExecutionEndEvent>();

        await Run(
            Recording(new[] { ValueTool("providerTool", null) }, starts, ends),
            Call("call-1", "providerTool", providerExecuted: true),
            new ToolResultPart(
                new ExecutedTool("call-1", "providerTool", "{\"result\":\"example\"}", false)
                {
                    ProviderExecuted = true,
                    InputJson = "{\"value\":\"test\"}",
                }
            ),
            FinishChunk()
        );

        Assert.Empty(starts);
        Assert.Empty(ends);
    }
}
