// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateText;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>onStepError</c>.</summary>
public sealed class OnStepErrorTests
{
    private const string Prefix =
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onStepError::";

    [Fact]
    [UpstreamTest(
        Prefix + "should call onStepError when a generate step fails",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public async Task ShouldCallOnStepErrorWhenAGenerateStepFails()
    {
        var stepStarts = new List<GenerateTextStepStartEvent>();
        var stepErrors = new List<GenerateTextStepErrorEvent>();
        var stepEnds = 0;
        var providerError = new InvalidOperationException("provider failure");
        var agent = new Agent(
            new AgentOptions
            {
                Model = new StepErrorModel(doGenerate: (_, _) => throw providerError),
                MaxRetries = 0,
                OnStepStart = (e, _) =>
                {
                    stepStarts.Add(e);
                    return Task.CompletedTask;
                },
                OnStepError = (e, _) =>
                {
                    stepErrors.Add(e);
                    return Task.CompletedTask;
                },
                OnStepEnd = (_, _) =>
                {
                    stepEnds++;
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            agent.GenerateAsync("Hello, world!")
        );

        Assert.Same(providerError, thrown);
        Assert.Equal(0, stepEnds);
        var stepError = Assert.Single(stepErrors);
        var stepStart = Assert.Single(stepStarts);
        Assert.Equal(stepStart.CallId, stepError.CallId);
        Assert.Equal(stepStart.StepNumber, stepError.StepNumber);
        Assert.Equal(stepStart.Provider, stepError.Provider);
        Assert.Equal(stepStart.ModelId, stepError.ModelId);
        Assert.Equal(stepStart.Instructions, stepError.Instructions);
        Assert.Equal(stepStart.Messages.Count, stepError.Messages.Count);
        Assert.Same(providerError, stepError.Error);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call onStepError when a stream step ends without output",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public async Task ShouldCallOnStepErrorWhenAStreamStepEndsWithoutOutput()
    {
        var stepErrors = new List<GenerateTextStepErrorEvent>();
        var stepEnds = 0;
        var agent = new Agent(
            new AgentOptions
            {
                Model = new Vercel.AI.Tests.StreamText.StreamTextAbortSupport.ScriptedStreamModel(
                    StartOnly
                ),
                OnStepError = (e, _) =>
                {
                    stepErrors.Add(e);
                    return Task.CompletedTask;
                },
                OnStepEnd = (_, _) =>
                {
                    stepEnds++;
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );

        var result = agent.Stream("Hello, world!");
        await foreach (var _ in result.Stream()) { }
        await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.Text);

        Assert.Equal(0, stepEnds);
        var stepError = Assert.Single(stepErrors);
        Assert.Equal(0, stepError.StepNumber);
        Assert.IsType<NoOutputGeneratedException>(stepError.Error);
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> StartOnly()
    {
        yield return new StreamStartStreamPart(null);
        await Task.CompletedTask;
    }
}
