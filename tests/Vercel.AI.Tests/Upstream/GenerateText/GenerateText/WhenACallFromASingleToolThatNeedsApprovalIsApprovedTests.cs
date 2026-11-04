// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; tool execution approval &gt; when a call from a single tool that needs approval is approved</c>.</summary>
public sealed class WhenACallFromASingleToolThatNeedsApprovalIsApprovedTests
{
    private const string Id = "packages/ai/src/generate-text/generate-text.test.ts::generateText > tool execution approval > when a call from a single tool that needs approval is approved::";

    [Fact]
    [UpstreamTest(Id + "should execute the tool", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_execute_the_tool()
    {
        var run = await RunAsync();

        var call = Assert.Single(run.ExecuteCalls);
        Assert.Equal("{\"value\":\"value\"}", Compact(call.Input.GetRawText()));
        Assert.False(call.Options.AbortSignal.CanBeCanceled);
        Assert.Equal("call-1", call.Options.ToolCallId);
        Assert.NotNull(call.Options.Messages);
    }

    [Fact]
    [UpstreamTest(Id + "should call the model with a prompt that includes the tool result", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_call_the_model_with_a_prompt_that_includes_the_tool_result()
    {
        var run = await RunAsync();

        var prompt = Assert.Single(run.Model.Calls).Prompt;
        Assert.Equal(3, prompt.Count);
        Assert.Equal("test-input", PromptText(prompt));
        var assistant = Assert.IsType<AssistantModelMessage>(prompt[1]);
        AssertToolCallPart(Assert.Single(assistant.Parts), "call-1", "tool1", "{\"value\":\"value\"}");
        AssertToolResult(Assert.IsType<ToolModelMessage>(prompt[2]), "call-1", "tool1", "text", "result1");
    }

    [Fact]
    [UpstreamTest(Id + "should include the tool result in the response messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_the_tool_result_in_the_response_messages()
    {
        var run = await RunAsync();

        Assert.Equal(2, run.Result.ResponseMessages.Count);
        AssertToolResult(Assert.IsType<ToolModelMessage>(run.Result.ResponseMessages[0]), "call-1", "tool1", "text", "result1");
        AssertText(run.Result.ResponseMessages[1], "Hello, world!");
    }

    [Fact]
    [UpstreamTest(Id + "should pass the initial tool result in responseMessages to prepareStep", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_the_initial_tool_result_in_responseMessages_to_prepareStep()
    {
        var prepareStepResponseMessages = new List<IReadOnlyList<ModelMessage>>();

        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel { OnGenerate = _ => Text("Hello, world!") },
            Tools = new[] { ValueTool((_, _) => Task.FromResult("result1")) },
            ToolApproval = new ToolApprovalConfiguration { { "tool1", "user-approval" } },
            StopWhen = StopWhen.IsStepCount(3),
            Messages = ApprovedHistory(),
            PrepareStep = context =>
            {
                prepareStepResponseMessages.Add(context.ResponseMessages.ToArray());
                return null;
            },
        });

        var responseMessages = Assert.Single(prepareStepResponseMessages);
        AssertToolResult(Assert.IsType<ToolModelMessage>(Assert.Single(responseMessages)), "call-1", "tool1", "text", "result1");
    }

    internal static List<ModelMessage> ApprovedHistory() =>
        History(
            "tool1",
            new[] { ("call-1", "{\"value\":\"value\"}", "id-1") },
            new ToolApprovalResponseModelMessage("id-1", true));

    private static async Task<(GenerateTextResult Result, TestLanguageModel Model, List<(JsonElement Input, ToolExecutionOptions Options)> ExecuteCalls)> RunAsync()
    {
        var executeCalls = new List<(JsonElement Input, ToolExecutionOptions Options)>();
        var model = new TestLanguageModel { OnGenerate = _ => Text("Hello, world!") };
        var tool = ValueTool(null);
        tool.ExecuteWithOptions = (input, options) =>
        {
            executeCalls.Add((input, options));
            return Task.FromResult("result1");
        };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Tools = new[] { tool },
            ToolApproval = new ToolApprovalConfiguration { { "tool1", "user-approval" } },
            StopWhen = StopWhen.IsStepCount(3),
            GenerateId = MockId("id"),
            Messages = ApprovedHistory(),
        });
        return (result, model, executeCalls);
    }
}
