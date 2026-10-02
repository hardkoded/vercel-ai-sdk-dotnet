// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class StopConditionTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stop-condition.test.ts::stop conditions > isStepCount::should return false when the step count does not match exactly",
        Coverage = UpstreamCoverage.Covered)]
    public void Is_step_count_is_true_only_for_an_exact_match()
    {
        var stop = StopWhen.IsStepCount(2);
        Assert.False(stop.ShouldStop(new[] { Step() }));
        Assert.True(stop.ShouldStop(new[] { Step(), Step() }));
        Assert.False(stop.ShouldStop(new[] { Step(), Step(), Step() }));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stop-condition.test.ts::stop conditions > isLoopFinished::should always return false",
        Coverage = UpstreamCoverage.Covered)]
    public void Is_loop_finished_never_stops_by_itself()
    {
        var stop = StopWhen.IsLoopFinished();
        Assert.False(stop.ShouldStop(Array.Empty<StepResult>()));
        Assert.False(stop.ShouldStop(new[] { Step() }));
        Assert.False(stop.ShouldStop(new[] { Step("finalAnswer") }));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stop-condition.test.ts::stop conditions > hasToolCall::should return true when the last step contains the specified tool call",
        Coverage = UpstreamCoverage.Covered)]
    public void Has_tool_call_matches_the_latest_step()
    {
        var stop = StopWhen.HasToolCall("finalAnswer");
        Assert.True(stop.ShouldStop(new[] { Step(), Step("finalAnswer") }));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stop-condition.test.ts::stop conditions > hasToolCall::should return false when the specified tool call only appears in earlier steps",
        Coverage = UpstreamCoverage.Covered)]
    public void Has_tool_call_ignores_earlier_steps()
    {
        var stop = StopWhen.HasToolCall("finalAnswer");
        Assert.False(stop.ShouldStop(new[] { Step("finalAnswer"), Step() }));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stop-condition.test.ts::stop conditions > hasToolCall::should return true when the last step contains any tool call from the provided tool names",
        Coverage = UpstreamCoverage.Covered)]
    public void Has_tool_call_matches_any_listed_name()
    {
        var stop = StopWhen.HasToolCall("search", "finalAnswer");
        Assert.True(stop.ShouldStop(new[] { Step(), Step("finalAnswer") }));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stop-condition.test.ts::stop conditions > hasToolCall::should return false when the last step does not contain any tool call from the provided tool names",
        Coverage = UpstreamCoverage.Covered)]
    public void Has_tool_call_rejects_other_names()
    {
        var stop = StopWhen.HasToolCall("search", "finalAnswer");
        Assert.False(stop.ShouldStop(new[] { Step(), Step("weather") }));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stop-condition.test.ts::stop conditions > hasToolCall::should return false when there are no steps",
        Coverage = UpstreamCoverage.Covered)]
    public void Has_tool_call_is_false_without_steps()
    {
        Assert.False(StopWhen.HasToolCall("finalAnswer").ShouldStop(Array.Empty<StepResult>()));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stop-condition.test.ts::stop conditions > isStopConditionMet::should return true when any stop condition returns true",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Is_met_when_any_condition_returns_true()
    {
        var met = await StopWhen.IsMetAsync(
            new[]
            {
                StopWhen.Custom(_ => false),
                StopWhen.Custom(_ => true),
                StopWhen.Custom(_ => false),
            },
            new[] { Step() });
        Assert.True(met);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stop-condition.test.ts::stop conditions > isStopConditionMet::should return false when all stop conditions return false",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Is_met_is_false_when_every_condition_is_false()
    {
        var met = await StopWhen.IsMetAsync(
            new[]
            {
                StopWhen.Custom(_ => false),
                StopWhen.Custom(_ => false),
            },
            new[] { Step() });
        Assert.False(met);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stop-condition.test.ts::stop conditions > isStopConditionMet::should support asynchronous stop conditions",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Is_met_awaits_asynchronous_conditions()
    {
        var met = await StopWhen.IsMetAsync(
            new[]
            {
                StopWhen.Custom((_, _) => Task.FromResult(false)),
                StopWhen.Custom((steps, _) => Task.FromResult(steps.Count == 2)),
            },
            new[] { Step(), Step() });
        Assert.True(met);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stop-condition.test.ts::stop conditions > isStopConditionMet::should reject when a stop condition rejects",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Is_met_propagates_a_failing_condition()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => StopWhen.IsMetAsync(
            new[]
            {
                StopWhen.Custom(_ => false),
                StopWhen.Custom((_, _) => Task.FromException<bool>(new InvalidOperationException("stop condition failed"))),
            },
            new[] { Step() }));
        Assert.Equal("stop condition failed", exception.Message);
    }

    private static StepResult Step(params string[] toolNames)
    {
        var calls = new List<GeneratedToolCall>();
        foreach (var name in toolNames)
        {
            calls.Add(new GeneratedToolCall("call", name, "{}"));
        }

        return new StepResult(
            string.Empty,
            null,
            calls,
            Array.Empty<ExecutedTool>(),
            toolNames.Length == 0 ? FinishReason.Stop : FinishReason.ToolCalls,
            LanguageModelUsage.Empty,
            Array.Empty<GeneratedSource>());
    }
}
