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

/// <summary>Port of <c>execute-tool-call.test.ts</c> &gt; <c>executeToolCall</c> &gt; <c>array callbacks</c>.</summary>
public sealed class ArrayCallbacksTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/execute-tool-call.test.ts::executeToolCall > array callbacks::";

    [Fact]
    [UpstreamTest(
        Prefix + "should call all onToolExecutionStart listeners in an array",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_all_onToolExecutionStart_listeners_in_an_array()
    {
        var calls = new List<string>();

        await Execute(
            CreateToolCall(),
            [ResultTool()],
            onToolExecutionStart:
            [
                (_, _) =>
                {
                    calls.Add("first");
                    return Task.CompletedTask;
                },
                (_, _) =>
                {
                    calls.Add("second");
                    return Task.CompletedTask;
                },
            ]
        );

        Assert.Equal(["first", "second"], calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call all onToolExecutionEnd listeners in an array",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_all_onToolExecutionEnd_listeners_in_an_array()
    {
        var calls = new List<string>();

        await Execute(
            CreateToolCall(),
            [ResultTool()],
            onToolExecutionEnd:
            [
                (_, _) =>
                {
                    calls.Add("first");
                    return Task.CompletedTask;
                },
                (_, _) =>
                {
                    calls.Add("second");
                    return Task.CompletedTask;
                },
            ]
        );

        Assert.Equal(["first", "second"], calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not break when one listener in the array throws",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_break_when_one_listener_in_the_array_throws()
    {
        var calls = new List<string>();

        var result = await Execute(
            CreateToolCall(),
            [ResultTool()],
            onToolExecutionStart:
            [
                (_, _) => throw new InvalidOperationException("listener error"),
                (_, _) =>
                {
                    calls.Add("second-start");
                    return Task.CompletedTask;
                },
            ],
            onToolExecutionEnd:
            [
                (_, _) => throw new InvalidOperationException("listener error"),
                (_, _) =>
                {
                    calls.Add("second-finish");
                    return Task.CompletedTask;
                },
            ]
        );

        var output = result!.Value.Output;
        Assert.False(output.IsError);
        Assert.Equal("\"test-result\"", output.OutputJson);
        Assert.Equal(["second-start", "second-finish"], calls);
    }
}
