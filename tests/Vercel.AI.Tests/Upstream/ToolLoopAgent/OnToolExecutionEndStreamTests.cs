// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.Upstream.GenerateText;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>onToolExecutionEnd > stream</c>.</summary>
public sealed class OnToolExecutionEndStreamTests
{
    private const string Prefix =
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onToolExecutionEnd > stream::";

    [Fact]
    [UpstreamTest(
        Prefix + "should call onToolExecutionEnd from constructor",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldCallOnToolExecutionEndFromConstructor()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = ToolLoopAgentToolCalls.StreamModel(out _),
                Tools = [ToolLoopAgentToolCalls.TestTool((value, _) => value + "-result")],
                OnToolExecutionEnd = (_, _) =>
                {
                    calls.Add("constructor");
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );
        await ToolExecutionCallbackSupport.ConsumeAsync(agent.Stream("test"));
        Assert.Equal(["constructor"], calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call onToolExecutionEnd from stream method",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldCallOnToolExecutionEndFromStreamMethod()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = ToolLoopAgentToolCalls.StreamModel(out _),
                Tools = [ToolLoopAgentToolCalls.TestTool((value, _) => value + "-result")],
            },
            ToolLoopAgentInstructions.Client()
        );
        await ToolExecutionCallbackSupport.ConsumeAsync(
            agent.Stream("test", onToolExecutionEnd: (_, _) =>
            {
                calls.Add("method");
                return Task.CompletedTask;
            })
        );
        Assert.Equal(["method"], calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call both constructor and method in correct order",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldCallBothConstructorAndMethodInCorrectOrder()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = ToolLoopAgentToolCalls.StreamModel(out _),
                Tools = [ToolLoopAgentToolCalls.TestTool((value, _) => value + "-result")],
                OnToolExecutionEnd = (_, _) =>
                {
                    calls.Add("constructor");
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );
        await ToolExecutionCallbackSupport.ConsumeAsync(
            agent.Stream("test", onToolExecutionEnd: (_, _) =>
            {
                calls.Add("method");
                return Task.CompletedTask;
            })
        );
        Assert.Equal(["constructor", "method"], calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass correct event information on success",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassCorrectEventInformationOnSuccess()
    {
        ToolExecutionEndEvent? e = null;
        var agent = new Agent(
            new AgentOptions
            {
                Model = ToolLoopAgentToolCalls.StreamModel(out _),
                Tools = [ToolLoopAgentToolCalls.TestTool((value, _) => value + "-result")],
                GenerateCallId = () => "call-3",
                NowMs = () => 0,
            },
            ToolLoopAgentInstructions.Client()
        );
        await ToolExecutionCallbackSupport.ConsumeAsync(
            agent.Stream("test", onToolExecutionEnd: (ev, _) =>
            {
                e = ev;
                return Task.CompletedTask;
            })
        );
        Assert.Equal("call-3", e!.CallId);
        Assert.Equal("call-1", e.ToolCall.ToolCallId);
        Assert.Equal("testTool", e.ToolCall.ToolName);
        Assert.Contains("test", e.ToolCall.ArgumentsJson);
        Assert.Single(e.Messages);
        Assert.Equal("call-1", e.ToolOutput.ToolCallId);
        Assert.Equal("\"test-result\"", e.ToolOutput.OutputJson);
        Assert.Equal(TimeSpan.Zero, e.ToolExecutionTime);
    }
}
