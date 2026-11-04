// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; tool execution approval &gt; when two calls from a single tool that needs approval are approved</c>.</summary>
public sealed class WhenTwoCallsFromASingleToolThatNeedsApprovalAreApprovedTests
{
    private const string Id = "packages/ai/src/generate-text/generate-text.test.ts::generateText > tool execution approval > when two calls from a single tool that needs approval are approved::";

    [Fact]
    [UpstreamTest(Id + "should call the model with a prompt that includes the tool results", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_call_the_model_with_a_prompt_that_includes_the_tool_results()
    {
        var (_, model) = await RunAsync();

        var prompt = Assert.Single(model.Calls).Prompt;
        Assert.Equal(3, prompt.Count);
        Assert.Equal("test-input", PromptText(prompt));
        var assistant = Assert.IsType<AssistantModelMessage>(prompt[1]);
        Assert.Equal(2, assistant.Parts.Count);
        AssertToolCallPart(assistant.Parts[0], "call-1", "tool1", "{\"value\":\"value1\"}");
        AssertToolCallPart(assistant.Parts[1], "call-2", "tool1", "{\"value\":\"value2\"}");
        var results = Assert.IsType<ToolModelMessage>(prompt[2]).Results;
        Assert.Equal(2, results.Count);
        AssertToolResult(results[0], "call-1", "tool1", "text", "result1");
        AssertToolResult(results[1], "call-2", "tool1", "text", "result1");
    }

    [Fact]
    [UpstreamTest(Id + "should include the tool results in the response messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_the_tool_results_in_the_response_messages()
    {
        var (result, _) = await RunAsync();

        Assert.Equal(2, result.ResponseMessages.Count);
        var results = Assert.IsType<ToolModelMessage>(result.ResponseMessages[0]).Results;
        Assert.Equal(2, results.Count);
        AssertToolResult(results[0], "call-1", "tool1", "text", "result1");
        AssertToolResult(results[1], "call-2", "tool1", "text", "result1");
        AssertText(result.ResponseMessages[1], "Hello, world!");
    }

    private static async Task<(GenerateTextResult Result, TestLanguageModel Model)> RunAsync()
    {
        var model = new TestLanguageModel { OnGenerate = _ => Text("Hello, world!", FinishReason.ToolCalls) };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Tools = new[] { ValueTool((_, _) => Task.FromResult("result1")) },
            ToolApproval = new ToolApprovalConfiguration { { "tool1", "user-approval" } },
            StopWhen = StopWhen.IsStepCount(3),
            GenerateId = MockId("id"),
            Messages = History(
                "tool1",
                new[] { ("call-1", "{\"value\":\"value1\"}", "id-1"), ("call-2", "{\"value\":\"value2\"}", "id-2") },
                new ToolApprovalResponseModelMessage("id-1", true),
                new ToolApprovalResponseModelMessage("id-2", true)),
        });
        return (result, model);
    }
}
