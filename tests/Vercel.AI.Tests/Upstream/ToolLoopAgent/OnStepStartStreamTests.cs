// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.Upstream.GenerateText;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>onStepStart > stream</c>.</summary>
public sealed class OnStepStartStreamTests
{
    private const string Prefix =
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onStepStart > stream::";

    [Fact]
    [UpstreamTest(
        Prefix + "should call onStepStart from constructor",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldCallOnStepStartFromConstructor()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Stream(() => SteeringTestSupport.TextStream("Hello")),
                OnStepStart = (_, _) =>
                {
                    calls.Add("constructor");
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );
        await ToolExecutionCallbackSupport.ConsumeAsync(agent.Stream("Hello, world!"));
        Assert.Equal(["constructor"], calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call onStepStart from stream method",
        Coverage = UpstreamCoverage.Partial,
        Note = "The call-level callback is set through PrepareCall, because the agent methods take no per-call callbacks."
    )]
    public async Task ShouldCallOnStepStartFromStreamMethod()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Stream(() => SteeringTestSupport.TextStream("Hello")),
                PrepareCall = call =>
                {
                    call.OnStepStart = (_, _) =>
                    {
                        calls.Add("method");
                        return Task.CompletedTask;
                    };
                    return call;
                },
            },
            ToolLoopAgentInstructions.Client()
        );
        await ToolExecutionCallbackSupport.ConsumeAsync(agent.Stream("Hello, world!"));
        Assert.Equal(["method"], calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call both constructor and method onStepStart in correct order",
        Coverage = UpstreamCoverage.Partial,
        Note = "The call-level callback is set through PrepareCall, because the agent methods take no per-call callbacks."
    )]
    public async Task ShouldCallBothConstructorAndMethodOnStepStartInCorrectOrder()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Stream(() => SteeringTestSupport.TextStream("Hello")),
                OnStepStart = (_, _) =>
                {
                    calls.Add("constructor");
                    return Task.CompletedTask;
                },
                PrepareCall = call =>
                {
                    call.OnStepStart = (_, _) =>
                    {
                        calls.Add("method");
                        return Task.CompletedTask;
                    };
                    return call;
                },
            },
            ToolLoopAgentInstructions.Client()
        );
        await ToolExecutionCallbackSupport.ConsumeAsync(agent.Stream("Hello, world!"));
        Assert.Equal(["constructor", "method"], calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass correct event information",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassCorrectEventInformation()
    {
        GenerateTextStepStartEvent? stepStartEvent = null;
        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Stream(() => SteeringTestSupport.TextStream("Hello")),
                Instructions = "You are a helpful assistant",
                RuntimeContext = new Dictionary<string, object?> { ["userId"] = "test-user" },
                OnStepStart = (e, _) =>
                {
                    stepStartEvent = e;
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );
        await ToolExecutionCallbackSupport.ConsumeAsync(agent.Stream("Hello, world!"));
        Assert.Equal(0, stepStartEvent!.StepNumber);
        Assert.Equal("mock-provider", stepStartEvent.Provider);
        Assert.Equal("mock-model-id", stepStartEvent.ModelId);
        Assert.Equal("You are a helpful assistant", stepStartEvent.Instructions?.Text);
        Assert.Single(stepStartEvent.Messages);
        Assert.Empty(stepStartEvent.Steps);
        Assert.Equal("test-user", stepStartEvent.RuntimeContext!["userId"]);
    }
}
