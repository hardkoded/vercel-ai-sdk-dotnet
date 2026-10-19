// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>options.onToolExecutionEnd</c>.</summary>
public sealed class OptionsOnToolExecutionEndTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.onToolExecutionEnd::should be called with correct data on success",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port has no callId or toolContext, so the event is checked without them. See #623.")]
    public async Task Should_be_called_with_correct_data_on_success()
    {
        var events = new List<ToolExecutionEndEvent>();
        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = GenerateModel(("call-1", "{ \"value\": \"test-arg\" }")),
            Prompt = "test-input",
            Tools = new[] { Tool1() },
            OnToolExecutionEnd = (e, _) =>
            {
                events.Add(e);
                return Task.CompletedTask;
            },
        });

        var end = Assert.Single(events);
        Assert.Equal("call-1", end.ToolCall.ToolCallId);
        Assert.Equal("tool1", end.ToolCall.ToolName);
        Assert.Equal("test-input", Assert.IsType<TextContentPart>(Assert.Single(Assert.IsType<UserModelMessage>(Assert.Single(end.Messages)).Content)).Text);
        Assert.False(end.ToolOutput.IsError);
        Assert.Equal("call-1", end.ToolOutput.ToolCallId);
        Assert.Equal("test-arg-result", end.ToolOutput.OutputJson);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.onToolExecutionEnd::should be called with error data when tool execution fails",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port has no callId or toolContext, so the event is checked without them. See #623.")]
    public async Task Should_be_called_with_error_data_when_tool_execution_fails()
    {
        var events = new List<ToolExecutionEndEvent>();
        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = GenerateModel(("call-1", "{ \"value\": \"test\" }")),
            Prompt = "test-input",
            Tools = new[] { Tool1((_, _) => throw new InvalidOperationException("Tool execution failed")) },
            OnToolExecutionEnd = (e, _) =>
            {
                events.Add(e);
                return Task.CompletedTask;
            },
        });

        var end = Assert.Single(events);
        Assert.Equal("call-1", end.ToolCall.ToolCallId);
        Assert.True(end.ToolOutput.IsError);
        Assert.Contains("Tool execution failed", end.ToolOutput.OutputJson);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > options.onToolExecutionEnd::should have a positive toolExecutionMs", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_a_positive_toolExecutionMs()
    {
        var events = new List<ToolExecutionEndEvent>();
        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = GenerateModel(("call-1", "{ \"value\": \"test\" }")),
            Prompt = "test-input",
            Tools = new[] { Tool1(async (_, token) => { await Task.Delay(20, token); return "result"; }) },
            OnToolExecutionEnd = (e, _) =>
            {
                events.Add(e);
                return Task.CompletedTask;
            },
        });

        Assert.True(Assert.Single(events).ToolExecutionTime > TimeSpan.Zero);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > options.onToolExecutionEnd::should not break generation when callback throws", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_break_generation_when_callback_throws()
    {
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = GenerateModel(("call-1", "{ \"value\": \"test\" }")),
            Prompt = "test-input",
            Tools = new[] { Tool1() },
            OnToolExecutionEnd = (_, _) => throw new InvalidOperationException("callback error"),
        });

        Assert.Single(result.ToolResults);
        Assert.Equal("test-result", result.ToolResults[0].OutputJson);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > options.onToolExecutionEnd::should not fire for tools without execute", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_fire_for_tools_without_execute()
    {
        var events = new List<ToolExecutionEndEvent>();
        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = GenerateModel(("call-1", "{ \"value\": \"test\" }")),
            Prompt = "test-input",
            Tools = new[] { Tool1(withExecute: false) },
            OnToolExecutionEnd = (e, _) =>
            {
                events.Add(e);
                return Task.CompletedTask;
            },
        });

        Assert.Empty(events);
    }
}
