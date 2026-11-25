// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateText;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;

#pragma warning disable CS0618 // OnStepFinish and OnFinish are the deprecated aliases the upstream tests exercise

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>onStepFinish > generate</c>.</summary>
public sealed class OnStepFinishGenerateTests
{
    private const string Prefix =
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onStepFinish > generate::";

    [Fact]
    [UpstreamTest(Prefix + "should call onStepEnd from constructor and generate method in order", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldCallOnStepEndFromConstructorAndGenerateMethodInOrder()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Generate(_ => SteeringTestSupport.TextResponse("reply")),
                OnStepEnd = (_, _) =>
                {
                    calls.Add("constructor");
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );
        await agent.GenerateAsync("Hello, world!", onStepEnd: (_, _) =>
            {
                calls.Add("method");
                return Task.CompletedTask;
            });
        Assert.Equal(["constructor", "method"], calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "should prefer onStepEnd over deprecated onStepFinish", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPreferOnStepEndOverDeprecatedOnStepFinish()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Generate(_ => SteeringTestSupport.TextResponse("reply")),
                OnStepEnd = (_, _) =>
                {
                    calls.Add("constructor-onStepEnd");
                    return Task.CompletedTask;
                },
                OnStepFinish = (_, _) =>
                {
                    calls.Add("constructor-onStepFinish");
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );
        await agent.GenerateAsync(
            "Hello, world!",
            onStepEnd: (_, _) =>
            {
                calls.Add("method-onStepEnd");
                return Task.CompletedTask;
            },
            onStepFinish: (_, _) =>
            {
                calls.Add("method-onStepFinish");
                return Task.CompletedTask;
            }
        );
        Assert.Equal(["constructor-onStepEnd", "method-onStepEnd"], calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "should call onStepFinish from constructor", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldCallOnStepFinishFromConstructor()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Generate(_ => SteeringTestSupport.TextResponse("reply")),
                OnStepFinish = (_, _) =>
                {
                    calls.Add("constructor");
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );
        await agent.GenerateAsync("Hello, world!");
        Assert.Equal(["constructor"], calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "should call onStepFinish from generate method", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldCallOnStepFinishFromGenerateMethod()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Generate(_ => SteeringTestSupport.TextResponse("reply")),
            },
            ToolLoopAgentInstructions.Client()
        );
        await agent.GenerateAsync("Hello, world!", onStepFinish: (_, _) =>
            {
                calls.Add("method");
                return Task.CompletedTask;
            });
        Assert.Equal(["method"], calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "should call both constructor and method onStepFinish in correct order", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldCallBothConstructorAndMethodOnStepFinishInCorrectOrder()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Generate(_ => SteeringTestSupport.TextResponse("reply")),
                OnStepFinish = (_, _) =>
                {
                    calls.Add("constructor");
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );
        await agent.GenerateAsync("Hello, world!", onStepFinish: (_, _) =>
            {
                calls.Add("method");
                return Task.CompletedTask;
            });
        Assert.Equal(["constructor", "method"], calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass stepResult to onStepFinish callback", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassStepResultToOnStepFinishCallback()
    {
        StepResult? captured = null;
        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Generate(_ => SteeringTestSupport.TextResponse("reply")),
            },
            ToolLoopAgentInstructions.Client()
        );
        await agent.GenerateAsync("Hello, world!", onStepFinish: (r, _) =>
        {
            captured = r;
            return Task.CompletedTask;
        });
        Assert.Equal(FinishReason.Stop, captured!.FinishReason);
        Assert.Equal(0, captured.StepNumber);
        Assert.Equal("reply", captured.Text);
    }
}
