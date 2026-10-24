// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; options.onStepStart</c>.</summary>
public sealed class OptionsOnStepStartTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.onStepStart::should pass initialInstructions, initialMessages, and responseMessages to prepareStep",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_initialInstructions_initialMessages_and_responseMessages_to_prepareStep()
    {
        var calls = new List<PrepareStepContext>();
        var responseCount = 0;
        var model = new StreamTextAbortSupport.ScriptedStreamModel(
            () => responseCount++ == 0 ? ToolCallStream(("call-1", "{ \"value\": \"test\" }")) : TextStream());
        var initialMessage = new UserModelMessage("test-input");

        await ConsumeAsync(Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Tools = new[] { Tool1((input, _) => Task.FromResult("\"test-result\"")) },
            Instructions = "test instructions",
            Messages = new ModelMessage[] { initialMessage },
            StopWhen = StopWhen.IsStepCount(3),
            PrepareStep = context =>
            {
                calls.Add(context);
                return context.StepNumber == 0 ? new PrepareStepUpdate { Instructions = "prepared instructions" } : null;
            },
        }));

        Assert.Equal(2, calls.Count);
        Assert.Equal("test instructions", calls[0].Instructions!.Text);
        Assert.Equal("test instructions", calls[0].InitialInstructions!.Text);
        Assert.Equal(new ModelMessage[] { initialMessage }, calls[0].InitialMessages);
        Assert.Empty(calls[0].ResponseMessages);
        Assert.Equal(new ModelMessage[] { initialMessage }, calls[0].Messages);

        Assert.Equal("prepared instructions", calls[1].Instructions!.Text);
        Assert.Equal("test instructions", calls[1].InitialInstructions!.Text);
        Assert.Equal(new ModelMessage[] { initialMessage }, calls[1].InitialMessages);
        Assert.Equal(new[] { "assistant", "tool" }, calls[1].ResponseMessages.Select(m => m.Role));
        var toolMessage = Assert.IsType<ToolModelMessage>(calls[1].ResponseMessages[1]);
        Assert.Equal("call-1", toolMessage.ToolCallId);
        Assert.Equal("tool1", toolMessage.ToolName);
        Assert.Equal("test-result", toolMessage.OutputJson.Trim('"'));
        Assert.Equal(calls[1].InitialMessages.Concat(calls[1].ResponseMessages), calls[1].Messages);
    }
}
