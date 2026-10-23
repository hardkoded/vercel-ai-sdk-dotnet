// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.ResultToolCallsTests;

namespace Vercel.AI.Tests.Upstream.GenerateText;

public sealed class OptionsOnStepStartTests
{
    [Fact]
    public async Task Should_pass_initialInstructions_initialMessages_and_responseMessages_to_prepareStep()
    {
        var calls = new List<PrepareStepContext>();
        var model = new TestLanguageModel();
        model.OnGenerate = _ => model.Calls.Count == 1 ? ToolCallResponse("{ \"value\": \"test\" }") : TestLanguageModel.Text("Done.");
        var initialMessage = new UserModelMessage("test-input");

        await new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Tools = new[] { Tool.Function("tool1", null, "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}", (_, _) => Task.FromResult("\"test-result\"")) },
            Instructions = "test instructions",
            Messages = new ModelMessage[] { initialMessage },
            StopWhen = StopWhen.IsStepCount(3),
            PrepareStep = context =>
            {
                calls.Add(context);
                return context.StepNumber == 0 ? new PrepareStepUpdate { Instructions = "prepared instructions" } : null;
            },
        });

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
        var prompt = Assert.IsType<SystemModelMessage>(model.Calls[1].Prompt[0]);
        Assert.Equal("prepared instructions", prompt.Content);
    }
}
