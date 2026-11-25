// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateText;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>onStart > stream</c>.</summary>
public sealed class OnStartStreamTests
{
    private const string Prefix =
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onStart > stream::";

    [Fact]
    [UpstreamTest(Prefix + "should call onStart from constructor", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldCallOnStartFromConstructor()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Stream(() => SteeringTestSupport.TextStream("Hello")),
                OnStart = (_, _) =>
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
    [UpstreamTest(Prefix + "should call onStart from stream method", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldCallOnStartFromStreamMethod()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Stream(() => SteeringTestSupport.TextStream("Hello")),
            },
            ToolLoopAgentInstructions.Client()
        );
        await ToolExecutionCallbackSupport.ConsumeAsync(agent.Stream("Hello, world!", onStart: (_, _) =>
            {
                calls.Add("method");
                return Task.CompletedTask;
            }));
        Assert.Equal(["method"], calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "should call both constructor and method onStart in correct order", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldCallBothConstructorAndMethodOnStartInCorrectOrder()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Stream(() => SteeringTestSupport.TextStream("Hello")),
                OnStart = (_, _) =>
                {
                    calls.Add("constructor");
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );
        await ToolExecutionCallbackSupport.ConsumeAsync(agent.Stream("Hello, world!", onStart: (_, _) =>
            {
                calls.Add("method");
                return Task.CompletedTask;
            }));
        Assert.Equal(["constructor", "method"], calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass correct event information", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassCorrectEventInformation()
    {
        GenerateTextStartEvent? startEvent = null;
        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Stream(() => SteeringTestSupport.TextStream("Hello")),
                Instructions = "You are a helpful assistant",
                Temperature = 0.7,
                MaxOutputTokens = 500,
                RuntimeContext = new Dictionary<string, object?> { ["userId"] = "test-user" },
            },
            ToolLoopAgentInstructions.Client()
        );
        await ToolExecutionCallbackSupport.ConsumeAsync(
            agent.Stream("Hello, world!", onStart: (e, _) =>
            {
                startEvent = e;
                return Task.CompletedTask;
            })
        );
        Assert.Equal("mock-provider", startEvent!.Provider);
        Assert.Equal("mock-model-id", startEvent.ModelId);
        Assert.Equal("You are a helpful assistant", startEvent.Instructions?.Text);
        var message = Assert.IsType<UserModelMessage>(Assert.Single(startEvent.Messages));
        Assert.Equal("Hello, world!", Assert.IsType<TextContentPart>(Assert.Single(message.Content)).Text);
        Assert.Equal(0.7, startEvent.Temperature);
        Assert.Equal(500, startEvent.MaxOutputTokens);
        Assert.Equal("test-user", startEvent.RuntimeContext!["userId"]);
    }
}
