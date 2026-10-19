// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>options.onToolExecutionStart and onToolExecutionEnd ordering</c>.</summary>
public sealed class OptionsOnToolExecutionStartAndOnToolExecutionEndOrderingTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/stream-text.test.ts::streamText > options.onToolExecutionStart and onToolExecutionEnd ordering::should call start before finish", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_call_start_before_finish()
    {
        var callOrder = new List<string>();
        await ConsumeAsync(Client().StreamTextAsync(new StreamTextOptions
        {
            Model = StreamModel(("call-1", "{ \"value\": \"test\" }")),
            Prompt = "test-input",
            Tools = new[] { Tool1() },
            OnToolExecutionStart = (e, _) =>
            {
                callOrder.Add("start");
                return Task.CompletedTask;
            },
            OnToolExecutionEnd = (e, _) =>
            {
                callOrder.Add("end");
                return Task.CompletedTask;
            },
        }));

        Assert.Equal(new[] { "start", "end" }, callOrder);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/stream-text.test.ts::streamText > options.onToolExecutionStart and onToolExecutionEnd ordering::should fire tool call callbacks for each tool in a multi-step loop", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_fire_tool_call_callbacks_for_each_tool_in_a_multi_step_loop()
    {
        var callOrder = new List<string>();
        await ConsumeAsync(Client().StreamTextAsync(new StreamTextOptions
        {
            Model = TwoStepStreamModel(),
            Prompt = "test-input",
            Tools = new[] { Tool1() },
            StopWhen = StopWhen.IsStepCount(3),
            OnToolExecutionStart = (e, _) =>
            {
                callOrder.Add("start");
                return Task.CompletedTask;
            },
            OnToolExecutionEnd = (e, _) =>
            {
                callOrder.Add("end");
                return Task.CompletedTask;
            },
        }));

        Assert.Equal(new[] { "start", "end" }, callOrder);
    }
}
