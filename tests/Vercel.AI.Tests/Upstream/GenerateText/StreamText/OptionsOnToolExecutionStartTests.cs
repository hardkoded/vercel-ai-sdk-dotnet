// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>options.onToolExecutionStart</c>.</summary>
public sealed class OptionsOnToolExecutionStartTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.onToolExecutionStart::should be called with correct tool name, id, and input",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port has no callId or toolContext, so the event is checked without them. See #623.")]
    public async Task Should_be_called_with_correct_tool_name_id_and_input()
    {
        var events = new List<ToolExecutionStartEvent>();
        await ConsumeAsync(Client().StreamTextAsync(new StreamTextOptions
        {
            Model = StreamModel(("call-1", "{ \"value\": \"test-arg\" }")),
            Prompt = "test-input",
            Tools = new[] { Tool1() },
            OnToolExecutionStart = (e, _) =>
            {
                events.Add(e);
                return Task.CompletedTask;
            },
        }));

        var start = Assert.Single(events);
        Assert.Equal("call-1", start.ToolCall.ToolCallId);
        Assert.Equal("tool1", start.ToolCall.ToolName);
        Assert.Equal("test-arg", JsonDocument.Parse(start.ToolCall.ArgumentsJson).RootElement.GetProperty("value").GetString());
        Assert.False(start.ToolCall.ProviderExecuted);
        Assert.Null(start.ToolCall.ProviderMetadata);
        Assert.Equal("test-input", Assert.IsType<TextContentPart>(Assert.Single(Assert.IsType<UserModelMessage>(Assert.Single(start.Messages)).Content)).Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.onToolExecutionStart::should be called once per tool call in a multi-tool step",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port has no callId or toolContext, so the event is checked without them. See #623.")]
    public async Task Should_be_called_once_per_tool_call_in_a_multi_tool_step()
    {
        var events = new List<ToolExecutionStartEvent>();
        await ConsumeAsync(Client().StreamTextAsync(new StreamTextOptions
        {
            Model = StreamModel(("call-1", "{ \"value\": \"a\" }"), ("call-2", "{ \"value\": \"b\" }")),
            Prompt = "test-input",
            Tools = new[] { Tool1() },
            OnToolExecutionStart = (e, _) =>
            {
                lock (events) {{ events.Add(e); }}
                return Task.CompletedTask;
            },
        }));

        Assert.Equal(new[] { "call-1", "call-2" }, events.Select(e => e.ToolCall.ToolCallId).OrderBy(id => id).ToArray());
        Assert.All(events, e => Assert.Equal("test-input", Assert.IsType<TextContentPart>(Assert.Single(Assert.IsType<UserModelMessage>(Assert.Single(e.Messages)).Content)).Text));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/stream-text.test.ts::streamText > options.onToolExecutionStart::should be called before tool execution", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_be_called_before_tool_execution()
    {
        var callOrder = new List<string>();
        await ConsumeAsync(Client().StreamTextAsync(new StreamTextOptions
        {
            Model = StreamModel(("call-1", "{ \"value\": \"test\" }")),
            Prompt = "test-input",
            Tools = new[] { Tool1((_, _) => { callOrder.Add("execute"); return Task.FromResult("result"); }) },
            OnToolExecutionStart = (e, _) =>
            {
                callOrder.Add("onToolExecutionStart");
                return Task.CompletedTask;
            },
        }));

        Assert.True(callOrder.IndexOf("onToolExecutionStart") < callOrder.IndexOf("execute"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/stream-text.test.ts::streamText > options.onToolExecutionStart::should not break generation when callback throws", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_break_generation_when_callback_throws()
    {
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = StreamModel(("call-1", "{ \"value\": \"test\" }")),
            Prompt = "test-input",
            Tools = new[] { Tool1() },
            OnToolExecutionStart = (_, _) => throw new InvalidOperationException("callback error"),
        });
        await ConsumeAsync(result);
        var steps = await result.Steps;

        var toolResult = Assert.Single(steps[0].ToolResults);
        Assert.Equal("test-result", toolResult.OutputJson);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/stream-text.test.ts::streamText > options.onToolExecutionStart::should not fire for tools without execute", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_fire_for_tools_without_execute()
    {
        var events = new List<ToolExecutionStartEvent>();
        await ConsumeAsync(Client().StreamTextAsync(new StreamTextOptions
        {
            Model = StreamModel(("call-1", "{ \"value\": \"test\" }")),
            Prompt = "test-input",
            Tools = new[] { Tool1(withExecute: false) },
            OnToolExecutionStart = (e, _) =>
            {
                events.Add(e);
                return Task.CompletedTask;
            },
        }));

        Assert.Empty(events);
    }
}
