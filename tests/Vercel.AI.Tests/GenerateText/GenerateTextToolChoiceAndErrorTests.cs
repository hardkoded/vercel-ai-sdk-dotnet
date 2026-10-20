// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.GenerateText;

/// <summary>Port of the tool choice and tool error checks of <c>generate-text.test.ts</c> &gt; <c>generateText</c>.</summary>
public sealed class GenerateTextToolChoiceAndErrorTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > result.toolResults::should contain tool results",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks the default auto tool choice sent to the model.")]
    public async Task ShouldSendAutoToolChoiceByDefault()
    {
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("ok") };
        await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "hi" });

        Assert.Equal("auto", model.Calls[0].ToolChoice!.Type);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > tool execution errors::should send the error text and keep the error",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks the tool error text and the error kept in the content part.")]
    public async Task ShouldSendErrorTextAndKeepTheError()
    {
        var model = new TestLanguageModel();
        model.OnGenerate = _ => model.Calls.Count == 1
            ? new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedToolCall("c1", "tool1", "{}") },
                FinishReason.ToolCalls,
                LanguageModelUsage.Empty,
                "tool_calls")
            : TestLanguageModel.Text("done");

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "hi",
            StopWhen = StopWhen.IsStepCount(2),
            Tools = new[]
            {
                Tool.Function("tool1", "t", "{\"type\":\"object\"}", (_, _) => Task.FromException<string>(new JsError("test error"))),
            },
        });

        var toolResult = Assert.IsType<GeneratedToolResult>(Assert.Single(result.Steps[0].Content, c => c is GeneratedToolResult));
        Assert.True(toolResult.IsError);
        Assert.Equal("test error", toolResult.Error!.Message);
        var toolMessage = Assert.IsType<ToolModelMessage>(model.Calls[1].Prompt.Last());
        Assert.Contains("Error: test error", toolMessage.OutputJson, StringComparison.Ordinal);
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
