// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.Upstream.GenerateText;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>onToolExecutionEnd > generate</c>.</summary>
public sealed class OnToolExecutionEndGenerateTests
{
    private const string Prefix =
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onToolExecutionEnd > generate::";

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
                Model = ToolLoopAgentToolCalls.GenerateModel(out _),
                Tools = [ToolLoopAgentToolCalls.TestTool((value, _) => value + "-result")],
                OnToolExecutionEnd = (_, _) =>
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
        Prefix + "should call onToolExecutionEnd from generate method",
        Coverage = UpstreamCoverage.Partial,
        Note = "The call-level callback is set through PrepareCall, because the agent methods take no per-call callbacks."
    )]
    public async Task ShouldCallOnToolExecutionEndFromGenerateMethod()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = ToolLoopAgentToolCalls.GenerateModel(out _),
                Tools = [ToolLoopAgentToolCalls.TestTool((value, _) => value + "-result")],
                PrepareCall = call =>
                {
                    call.OnToolExecutionEnd = (_, _) =>
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
                OnToolExecutionEnd = (_, _) =>
                {
                    calls.Add("constructor");
                    return Task.CompletedTask;
                },
                PrepareCall = call =>
                {
                    call.OnToolExecutionEnd = (_, _) =>
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
        Prefix + "should pass correct event information on success",
        Coverage = UpstreamCoverage.Partial,
        Note = "The test does not compare callId, which is generated per call in the port, nor toolExecutionMs."
    )]
    public async Task ShouldPassCorrectEventInformationOnSuccess()
    {
        ToolExecutionEndEvent? e = null;
        var agent = new Agent(
            new AgentOptions
            {
                Model = ToolLoopAgentToolCalls.GenerateModel(out _),
                Tools = [ToolLoopAgentToolCalls.TestTool((value, _) => value + "-result")],
                OnToolExecutionEnd = (ev, _) =>
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
        Assert.Equal("call-1", e.ToolOutput.ToolCallId);
        Assert.Equal("\"test-result\"", e.ToolOutput.OutputJson);
    }
}
