// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateText;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;

#pragma warning disable CS0618 // OnStepFinish and OnFinish are the deprecated aliases the upstream tests exercise

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>onStepFinish > stream</c>.</summary>
public sealed class OnStepFinishStreamTests
{
    private const string Prefix =
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onStepFinish > stream::";

    [Fact]
    [UpstreamTest(
        Prefix + "should call onStepFinish from constructor",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldCallOnStepFinishFromConstructor()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Stream(() => HelloWorldStream(true)),
                OnStepFinish = (_, _) =>
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
        Prefix + "should call onStepFinish from stream method",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldCallOnStepFinishFromStreamMethod()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions { Model = SteeringTestModel.Stream(() => HelloWorldStream(true)) },
            ToolLoopAgentInstructions.Client()
        );
        await ToolExecutionCallbackSupport.ConsumeAsync(
            agent.Stream("Hello, world!", onStepFinish: (_, _) =>
            {
                calls.Add("method");
                return Task.CompletedTask;
            })
        );
        Assert.Equal(["method"], calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call both constructor and method onStepFinish in correct order",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldCallBothConstructorAndMethodOnStepFinishInCorrectOrder()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Stream(() => HelloWorldStream(true)),
                OnStepFinish = (_, _) =>
                {
                    calls.Add("constructor");
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );
        await ToolExecutionCallbackSupport.ConsumeAsync(
            agent.Stream("Hello, world!", onStepFinish: (_, _) =>
            {
                calls.Add("method");
                return Task.CompletedTask;
            })
        );
        Assert.Equal(["constructor", "method"], calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass stepResult to onStepFinish callback",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassStepResultToOnStepFinishCallback()
    {
        StepResult? captured = null;
        var agent = new Agent(
            new AgentOptions { Model = SteeringTestModel.Stream(() => HelloWorldStream(true)) },
            ToolLoopAgentInstructions.Client()
        );
        await ToolExecutionCallbackSupport.ConsumeAsync(
            agent.Stream("Hello, world!", onStepFinish: (r, _) =>
            {
                captured = r;
                return Task.CompletedTask;
            })
        );
        Assert.Equal(FinishReason.Stop, captured!.FinishReason);
        Assert.Equal(0, captured.StepNumber);
        Assert.Equal("Hello, world!", captured.Text);
        Assert.Equal(
            "testValue",
            captured
                .ProviderMetadata!.Value.GetProperty("testProvider")
                .GetProperty("testKey")
                .GetString()
        );
    }

    private static IReadOnlyList<LanguageModelStreamPart> HelloWorldStream(bool metadata) =>
        new LanguageModelStreamPart[]
        {
            new StreamStartStreamPart(Array.Empty<CallWarning>()),
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart("1", "Hello"),
            new TextDeltaStreamPart("1", ", "),
            new TextDeltaStreamPart("1", "world!"),
            new TextEndStreamPart("1"),
            new FinishStreamPart(
                FinishReason.Stop,
                SteeringTestSupport.Usage,
                "stop",
                metadata
                    ? JsonDocument
                        .Parse("{\"testProvider\":{\"testKey\":\"testValue\"}}")
                        .RootElement.Clone()
                    : null
            ),
        };
}
