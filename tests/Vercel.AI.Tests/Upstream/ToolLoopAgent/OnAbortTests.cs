// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateText;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>onAbort</c>.</summary>
public sealed class OnAbortTests
{
    private const string Prefix =
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onAbort::";

    // Maps to `hangingModel`: the call waits until it is aborted.
    private static StepErrorModel HangingModel(Action? beforeHang = null) =>
        new(
            doGenerate: (_, token) =>
            {
                beforeHang?.Invoke();
                return StepErrorModel.HangUntilAborted<LanguageModelGenerateResult>(token);
            },
            doStream: (_, token) =>
            {
                beforeHang?.Invoke();
                return StepErrorModel.HangUntilAborted<LanguageModelStreamResult>(token);
            }
        );

    [Fact]
    [UpstreamTest(
        Prefix + "should call onAbort when stream() is aborted",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldCallOnAbortWhenStreamIsAborted()
    {
        var aborts = new List<StreamAbortContext>();
        var abortController = new AbortController();
        var agent = new Agent(
            new AgentOptions
            {
                Model = HangingModel(() => abortController.Abort()),
                OnAbort = (context, _) =>
                {
                    aborts.Add(context);
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );

        var result = agent.Stream("Hello, world!", abortSignal: abortController.Signal);
        await foreach (var _ in result.Stream()) { }

        Assert.Empty(Assert.Single(aborts).Steps);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call onAbort when stream() times out",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldCallOnAbortWhenStreamTimesOut()
    {
        var aborts = 0;
        var agent = new Agent(
            new AgentOptions
            {
                Model = HangingModel(),
                OnAbort = (_, _) =>
                {
                    aborts++;
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );

        var result = agent.Stream(
            "Hello, world!",
            timeout: new TimeoutConfiguration { TotalMs = 10 }
        );
        await foreach (var _ in result.Stream()) { }

        Assert.Equal(1, aborts);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not call onAbort when generate() is aborted",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldNotCallOnAbortWhenGenerateIsAborted()
    {
        var aborts = 0;
        var stepErrors = 0;
        using var cancellation = new CancellationTokenSource();
        var agent = new Agent(
            new AgentOptions
            {
                Model = HangingModel(() => cancellation.Cancel()),
                OnAbort = (_, _) =>
                {
                    aborts++;
                    return Task.CompletedTask;
                },
                OnStepError = (_, _) =>
                {
                    stepErrors++;
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );

        await Assert.ThrowsAnyAsync<Exception>(() =>
            agent.GenerateAsync("Hello, world!", cancellation.Token)
        );

        Assert.Equal(0, aborts);
        Assert.Equal(1, stepErrors);
    }
}
