// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.DiceGameFixture;

namespace Vercel.AI.Tests.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; programmatic tool calling &gt; 5 steps: code_execution triggers client tool across multiple turns (dice game fixture) &gt; onStepFinish callback</c>.</summary>
public sealed class DiceGameOnStepFinishCallbackTests
{
    private const string Prefix = "packages/ai/src/generate-text/stream-text.test.ts::streamText > programmatic tool calling > 5 steps: code_execution triggers client tool across multiple turns (dice game fixture) > onStepFinish callback::";

    [Fact]
    [UpstreamTest(Prefix + "should be called for each step", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_be_called_for_each_step()
    {
        var run = await RunStreamAsync();
        Assert.Equal(5, run.StepResults.Count);
    }

    [Fact]
    [UpstreamTest(Prefix + "should contain correct finishReason for each step", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_contain_correct_finishReason_for_each_step()
    {
        var run = await RunStreamAsync();
        Assert.Equal(
            new[] { FinishReason.ToolCalls, FinishReason.ToolCalls, FinishReason.ToolCalls, FinishReason.ToolCalls, FinishReason.Stop },
            run.StepResults.Select(step => step.FinishReason).ToArray());
    }

    [Fact]
    [UpstreamTest(Prefix + "should contain provider metadata with container ID for steps 1 and 2", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_contain_provider_metadata_with_container_ID_for_steps_1_and_2()
    {
        var run = await RunStreamAsync();
        AssertContainerMetadata(run.StepResults[0], StreamContainerId);
        AssertContainerMetadata(run.StepResults[1], StreamContainerId);
    }
}
