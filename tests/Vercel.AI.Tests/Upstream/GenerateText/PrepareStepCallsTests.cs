// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

public sealed class PrepareStepCallsTests
{
    [Fact]
    public async Task PrepareStep_sees_the_results_of_one_step_as_a_single_tool_message()
    {
        var model = new TestLanguageModel();
        model.OnGenerate = _ => model.Calls.Count == 1
            ? new LanguageModelGenerateResult(
                new GeneratedContent[]
                {
                    new GeneratedToolCall("call-1", "roll", "{}"),
                    new GeneratedToolCall("call-2", "roll", "{}"),
                },
                FinishReason.ToolCalls,
                new LanguageModelUsage(1, 1, 2),
                null)
            : TestLanguageModel.Text("done");
        var seen = new List<IReadOnlyList<ModelMessage>>();

        await new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            StopWhen = StopWhen.IsStepCount(2),
            PrepareStep = context =>
            {
                seen.Add(context.Messages);
                return null;
            },
            Tools = new[] { Tool.Function("roll", null, "{\"type\":\"object\"}", (_, _) => Task.FromResult("4")) },
        });

        Assert.Equal(new[] { "user" }, seen[0].Select(m => m.Role));
        Assert.Equal(new[] { "user", "assistant", "tool" }, seen[1].Select(m => m.Role));
        var tool = Assert.IsType<ToolModelMessage>(seen[1][2]);
        Assert.Equal(new[] { "call-1", "call-2" }, tool.Results.Select(r => r.ToolCallId));
        Assert.Equal(new[] { "user", "assistant", "tool" }, model.Calls[1].Prompt.Take(3).Select(m => m.Role));
    }
}
