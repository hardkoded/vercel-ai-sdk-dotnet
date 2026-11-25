// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateText;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;

#pragma warning disable CS0618 // OnStepFinish and OnFinish are the deprecated aliases the upstream tests exercise

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>onEnd > stream</c>.</summary>
public sealed class OnEndStreamTests
{
    private const string Prefix =
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onEnd > stream::";

    [Fact]
    [UpstreamTest(
        Prefix + "should call onFinish from constructor",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldCallOnFinishFromConstructor()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Stream(() => HelloWorldStream(true)),
                OnFinish = (_, _) =>
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
        Prefix + "should call onFinish from stream method",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldCallOnFinishFromStreamMethod()
    {
        var calls = new List<string>();
        var agent = new Agent(
            new AgentOptions { Model = SteeringTestModel.Stream(() => HelloWorldStream(true)) },
            ToolLoopAgentInstructions.Client()
        );
        await ToolExecutionCallbackSupport.ConsumeAsync(
            agent.Stream("Hello, world!", onFinish: (_, _) =>
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
                Model = SteeringTestModel.Stream(() => HelloWorldStream(true)),
                OnFinish = (_, _) =>
                {
                    calls.Add("constructor");
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );
        await ToolExecutionCallbackSupport.ConsumeAsync(
            agent.Stream("Hello, world!", onFinish: (_, _) =>
            {
                calls.Add("method");
                return Task.CompletedTask;
            })
        );
        Assert.Equal(["constructor", "method"], calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass correct event information",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassCorrectEventInformation()
    {
        GenerateTextResult? captured = null;
        var agent = new Agent(
            new AgentOptions { Model = SteeringTestModel.Stream(() => HelloWorldStream(true)) },
            ToolLoopAgentInstructions.Client()
        );
        await ToolExecutionCallbackSupport.ConsumeAsync(agent.Stream("test", onFinish: (r, _) =>
            {
                captured = r;
                return Task.CompletedTask;
            }));
        Assert.Equal("Hello, world!", captured!.Text);
        Assert.Equal(FinishReason.Stop, captured.FinishReason);
        Assert.Single(captured.Steps);
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
