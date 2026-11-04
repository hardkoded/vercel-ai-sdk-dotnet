// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; tool execution approval &gt; when a call from a single tool that needs approval is denied</c>.</summary>
public sealed class WhenACallFromASingleToolThatNeedsApprovalIsDeniedTests
{
    private const string Id = "packages/ai/src/generate-text/generate-text.test.ts::generateText > tool execution approval > when a call from a single tool that needs approval is denied::";

    [Fact]
    [UpstreamTest(Id + "should not execute the tool", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_execute_the_tool()
    {
        var run = await RunAsync();

        Assert.Equal(0, run.ExecuteCalls);
    }

    [Fact]
    [UpstreamTest(Id + "should call the model with a prompt that includes the tool error", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_call_the_model_with_a_prompt_that_includes_the_tool_error()
    {
        var run = await RunAsync();

        var prompt = Assert.Single(run.Model.Calls).Prompt;
        Assert.Equal(3, prompt.Count);
        Assert.Equal("test-input", PromptText(prompt));
        var assistant = Assert.IsType<AssistantModelMessage>(prompt[1]);
        AssertToolCallPart(Assert.Single(assistant.Parts), "call-1", "tool1", "{\"value\":\"value\"}");
        var denied = Assert.IsType<ToolModelMessage>(prompt[2]);
        Assert.Equal("call-1", denied.ToolCallId);
        Assert.Equal("tool1", denied.ToolName);
        Assert.Equal("execution-denied", denied.OutputType);
        Assert.Equal(string.Empty, denied.OutputJson);
    }

    [Fact]
    [UpstreamTest(Id + "should include the tool error in the response messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_the_tool_error_in_the_response_messages()
    {
        var run = await RunAsync();

        Assert.Equal(2, run.Result.ResponseMessages.Count);
        var denied = Assert.IsType<ToolModelMessage>(run.Result.ResponseMessages[0]);
        Assert.Equal("call-1", denied.ToolCallId);
        Assert.Equal("tool1", denied.ToolName);
        Assert.Equal("execution-denied", denied.OutputType);
        Assert.Equal(string.Empty, denied.OutputJson);
        AssertText(run.Result.ResponseMessages[1], "Hello, world!");
    }

    private static async Task<(GenerateTextResult Result, TestLanguageModel Model, int ExecuteCalls)> RunAsync()
    {
        var executeCalls = 0;
        var model = new TestLanguageModel { OnGenerate = _ => Text("Hello, world!", FinishReason.ToolCalls) };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Tools = new[]
            {
                ValueTool((_, _) =>
                {
                    executeCalls++;
                    return Task.FromResult("result1");
                }),
            },
            ToolApproval = new ToolApprovalConfiguration { { "tool1", "user-approval" } },
            StopWhen = StopWhen.IsStepCount(3),
            GenerateId = MockId("id"),
            Messages = History(
                "tool1",
                new[] { ("call-1", "{\"value\":\"value\"}", "id-1") },
                new ToolApprovalResponseModelMessage("id-1", false)),
        });
        return (result, model, executeCalls);
    }
}
