// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.GenerateText.DiceGameFixture;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; programmatic tool calling &gt; 5 steps: code_execution triggers client tool across multiple turns (dice game fixture) &gt; prepareStep calls</c>.</summary>
public sealed class DiceGamePrepareStepCallsTests
{
    private const string Prefix = "packages/ai/src/generate-text/generate-text.test.ts::generateText > programmatic tool calling > 5 steps: code_execution triggers client tool across multiple turns (dice game fixture) > prepareStep calls::";

    [Fact]
    [UpstreamTest(Prefix + "should call prepareStep for each step with correct stepNumber", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_call_prepareStep_for_each_step_with_correct_stepNumber()
    {
        var run = await RunGenerateAsync();
        Assert.Equal(5, run.PrepareCalls.Count);
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, run.PrepareCalls.Select(call => call.StepNumber).ToArray());
        Assert.All(run.PrepareCalls, call => Assert.Equal("claude-sonnet-4-5-20250929", call.ModelId));
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass empty steps array for first prepareStep call", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_empty_steps_array_for_first_prepareStep_call()
    {
        var run = await RunGenerateAsync();
        Assert.Empty(run.PrepareCalls[0].Steps);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass accumulated steps to subsequent prepareStep calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_accumulated_steps_to_subsequent_prepareStep_calls()
    {
        var run = await RunGenerateAsync();
        Assert.Equal(new[] { 1, 2, 3, 4 }, run.PrepareCalls.Skip(1).Select(call => call.Steps.Count).ToArray());
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass accumulated messages to prepareStep", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_accumulated_messages_to_prepareStep()
    {
        var run = await RunGenerateAsync();

        // Step 0: just the initial user message.
        // Step 1: user message + assistant message + tool message.
        // Steps 2 to 4: each adds one assistant message and one tool message.
        Assert.Equal(new[] { 1, 3, 5, 7, 9 }, run.PrepareCalls.Select(call => Render(call.Messages).Length).ToArray());
    }
}
