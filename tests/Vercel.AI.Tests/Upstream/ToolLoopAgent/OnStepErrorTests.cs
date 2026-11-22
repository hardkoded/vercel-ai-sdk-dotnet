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
        Coverage = UpstreamCoverage.Partial,
        Note = "AgentOptions has no OnStepStart, so the test does not check that the error event carries the step start event."
    )]
    public async Task ShouldCallOnStepErrorWhenAGenerateStepFails()
    {
        var stepErrors = new List<GenerateTextStepErrorEvent>();
        var stepEnds = 0;
        var providerError = new InvalidOperationException("provider failure");
        var agent = new Agent(
            new AgentOptions
            {
                Model = new StepErrorModel(doGenerate: (_, _) => throw providerError),
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
        Assert.Same(providerError, stepError.Error);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call onStepError when a stream step ends without output",
        Coverage = UpstreamCoverage.Covered
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
