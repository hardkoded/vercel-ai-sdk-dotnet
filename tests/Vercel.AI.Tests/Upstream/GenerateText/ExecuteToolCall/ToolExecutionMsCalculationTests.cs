// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolCall.ExecuteToolCallSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolCall;

/// <summary>Port of <c>execute-tool-call.test.ts</c> &gt; <c>executeToolCall</c> &gt; <c>toolExecutionMs calculation</c>.</summary>
public sealed class ToolExecutionMsCalculationTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/execute-tool-call.test.ts::executeToolCall > toolExecutionMs calculation::";

    [Fact]
    [UpstreamTest(
        Prefix + "should calculate correct duration on success",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_calculate_correct_duration_on_success()
    {
        var toolExecutionEndEvents = new List<ToolExecutionEndEvent>();

        var result = await Execute(
            CreateToolCall(),
            [ResultTool()],
            now: MockNow(5000, 5250),
            onToolExecutionEnd:
            [
                (@event, _) =>
                {
                    toolExecutionEndEvents.Add(@event);
                    return Task.CompletedTask;
                },
            ]
        );

        Assert.Equal(TimeSpan.FromMilliseconds(250), toolExecutionEndEvents[0].ToolExecutionTime);
        Assert.Equal(250, result!.Value.ToolExecutionMs);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should calculate correct duration on error",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_calculate_correct_duration_on_error()
    {
        var toolExecutionEndEvents = new List<ToolExecutionEndEvent>();

        var result = await Execute(
            CreateToolCall(),
            [FailingTool(new InvalidOperationException("error"))],
            now: MockNow(1000, 1500),
            onToolExecutionEnd:
            [
                (@event, _) =>
                {
                    toolExecutionEndEvents.Add(@event);
                    return Task.CompletedTask;
                },
            ]
        );

        Assert.Equal(TimeSpan.FromMilliseconds(500), toolExecutionEndEvents[0].ToolExecutionTime);
        Assert.Equal(500, result!.Value.ToolExecutionMs);
    }
}
