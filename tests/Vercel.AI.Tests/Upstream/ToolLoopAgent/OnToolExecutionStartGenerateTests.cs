// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.Upstream.GenerateText;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>onToolExecutionStart > generate</c>.</summary>
public sealed class OnToolExecutionStartGenerateTests
{
    private const string Prefix =
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onToolExecutionStart > generate::";

    [Fact]
    [UpstreamTest(
        Prefix + "should call onToolExecutionStart from constructor",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldCallOnToolExecutionStartFromConstructor()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = ToolLoopAgentToolCalls.GenerateModel(out _),
                Tools = [ToolLoopAgentToolCalls.TestTool((value, _) => value + "-result")],
                OnToolExecutionStart = (_, _) =>
                {
                    calls.Add("constructor");
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );
        await agent.GenerateAsync("test");
        Assert.Equal(["constructor"], calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call onToolExecutionStart from generate method",
        Coverage = UpstreamCoverage.Partial,
        Note = "The call-level callback is set through PrepareCall, because the agent methods take no per-call callbacks."
    )]
    public async Task ShouldCallOnToolExecutionStartFromGenerateMethod()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = ToolLoopAgentToolCalls.GenerateModel(out _),
                Tools = [ToolLoopAgentToolCalls.TestTool((value, _) => value + "-result")],
                PrepareCall = call =>
                {
                    call.OnToolExecutionStart = (_, _) =>
                    {
                        calls.Add("method");
                        return Task.CompletedTask;
                    };
                    return call;
                },
            },
            ToolLoopAgentInstructions.Client()
        );
        await agent.GenerateAsync("test");
        Assert.Equal(["method"], calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call both constructor and method in correct order",
        Coverage = UpstreamCoverage.Partial,
        Note = "The call-level callback is set through PrepareCall, because the agent methods take no per-call callbacks."
    )]
    public async Task ShouldCallBothConstructorAndMethodInCorrectOrder()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = ToolLoopAgentToolCalls.GenerateModel(out _),
                Tools = [ToolLoopAgentToolCalls.TestTool((value, _) => value + "-result")],
                OnToolExecutionStart = (_, _) =>
                {
                    calls.Add("constructor");
                    return Task.CompletedTask;
                },
                PrepareCall = call =>
                {
                    call.OnToolExecutionStart = (_, _) =>
                    {
                        calls.Add("method");
                        return Task.CompletedTask;
                    };
                    return call;
                },
            },
            ToolLoopAgentInstructions.Client()
        );
        await agent.GenerateAsync("test");
        Assert.Equal(["constructor", "method"], calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass correct event information",
        Coverage = UpstreamCoverage.Partial,
        Note = "The test does not compare callId, which is generated per call in the port."
    )]
    public async Task ShouldPassCorrectEventInformation()
    {
        ToolExecutionStartEvent? e = null;
        var agent = new Agent(
            new AgentOptions
            {
                Model = ToolLoopAgentToolCalls.GenerateModel(out _),
                Tools = [ToolLoopAgentToolCalls.TestTool((value, _) => value + "-result")],
                OnToolExecutionStart = (ev, _) =>
                {
                    e = ev;
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );
        await agent.GenerateAsync("test");
        Assert.Equal("call-1", e!.ToolCall.ToolCallId);
        Assert.Equal("testTool", e.ToolCall.ToolName);
        Assert.Contains("test", e.ToolCall.ArgumentsJson);
        Assert.Single(e.Messages);
    }
}
