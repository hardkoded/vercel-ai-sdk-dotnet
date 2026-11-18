// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.ToolLoopAgent;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;
using static Vercel.AI.Tests.Upstream.GenerateText.StreamText.ToolExecutionApprovalStreamSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; tool execution approval &gt; when a call from a single tool that needs approval is approved</c>.</summary>
public sealed class WhenACallFromASingleToolThatNeedsApprovalIsApprovedTests
{
    private const string Id =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > tool execution approval > when a call from a single tool that needs approval is approved::";

    [Fact]
    [UpstreamTest(
        Id + "should execute the tool",
        Coverage = UpstreamCoverage.Partial,
        Note = "Upstream expects abortSignal to be undefined. The port always passes the cancellation token of the stream, which can be canceled, so abortSignal is not asserted."
    )]
    public async Task Should_execute_the_tool()
    {
        var run = await RunAsync();

        var call = Assert.Single(run.ExecuteCalls);
        AssertInput("{\"value\":\"value\"}", call.Input);
        Assert.Equal("call-1", call.Options.ToolCallId);
        Assert.NotNull(call.Options.Messages);
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass sandbox to approved tool execution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_sandbox_to_approved_tool_execution()
    {
        var run = await RunAsync();

        Assert.Same(run.Sandbox, Assert.Single(run.ExecuteCalls).Options.ExperimentalSandbox);
    }

    [Fact]
    [UpstreamTest(
        Id + "should call the model with a prompt that includes the tool result",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_the_model_with_a_prompt_that_includes_the_tool_result()
    {
        var run = await RunAsync();

        var prompt = Assert.Single(run.Model.Prompts);
        Assert.Equal(3, prompt.Count);
        Assert.Equal("test-input", PromptText(prompt));
        AssertToolCallPart(
            Assert.Single(Assert.IsType<AssistantModelMessage>(prompt[1]).Parts),
            "call-1",
            "tool1",
            "{\"value\":\"value\"}"
        );
        AssertToolResult(
            Assert.IsType<ToolModelMessage>(prompt[2]),
            "call-1",
            "tool1",
            "text",
            "result1"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should include the tool result in the response messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_the_tool_result_in_the_response_messages()
    {
        var run = await RunAsync();

        var messages = await run.Result.ResponseMessages;
        Assert.Equal(2, messages.Count);
        AssertToolResult(
            Assert.IsType<ToolModelMessage>(messages[0]),
            "call-1",
            "tool1",
            "text",
            "result1"
        );
        AssertText(messages[1], "Hello, world!");
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass the initial tool result in responseMessages to prepareStep",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_the_initial_tool_result_in_responseMessages_to_prepareStep()
    {
        var prepareStepResponseMessages = new List<IReadOnlyList<ModelMessage>>();

        await Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = new ScriptedModel(_ => TextStep()),
                    Tools = new[] { ValueTool((_, _) => Task.FromResult("result1")) },
                    ToolApproval = new ToolApprovalConfiguration { { "tool1", "user-approval" } },
                    StopWhen = StopWhen.IsStepCount(3),
                    Messages = ApprovedHistory(),
                    PrepareStep = context =>
                    {
                        prepareStepResponseMessages.Add(context.ResponseMessages.ToArray());
                        return null;
                    },
                }
            )
            .ConsumeStreamAsync();

        var responseMessages = Assert.Single(prepareStepResponseMessages);
        AssertToolResult(
            Assert.IsType<ToolModelMessage>(Assert.Single(responseMessages)),
            "call-1",
            "tool1",
            "text",
            "result1"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should include the tool result in the full stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_the_tool_result_in_the_full_stream()
    {
        var run = await RunAsync();

        var parts = await ReadAsync(run.Result);

        Assert.Equal(
            new[]
            {
                "start",
                "tool-result",
                "start-step",
                "text-start",
                "text-delta",
                "text-end",
                "finish-step",
                "finish",
            },
            Types(parts)
        );
        var toolResult = Assert.IsType<ToolResultPart>(parts[1]).Result;
        Assert.False(toolResult.Dynamic);
        AssertInput("{\"value\":\"value\"}", JsonDocument.Parse(toolResult.InputJson!).RootElement);
        Assert.Equal("result1", toolResult.OutputJson);
        Assert.Equal("call-1", toolResult.ToolCallId);
        Assert.Equal("tool1", toolResult.ToolName);
        Assert.Null(toolResult.Preliminary);
        AssertStartStep(parts[2]);
        Assert.Equal("1", Assert.IsType<TextStartPart>(parts[3]).Id);
        var delta = Assert.IsType<TextDeltaPart>(parts[4]);
        Assert.Equal("1", delta.Id);
        Assert.Equal("Hello, world!", delta.Text);
        Assert.Equal("1", Assert.IsType<TextEndPart>(parts[5]).Id);
        AssertStepAndFinish(parts, 6, FinishReason.Stop, "stop");
    }

    [Fact]
    [UpstreamTest(
        Id + "should include the tool result in the ui message stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_the_tool_result_in_the_ui_message_stream()
    {
        var run = await RunAsync();

        AssertJson(
            """
            [
              { "type": "start" },
              { "output": "result1", "toolCallId": "call-1", "type": "tool-output-available" },
              { "type": "start-step" },
              { "id": "1", "type": "text-start" },
              { "delta": "Hello, world!", "id": "1", "type": "text-delta" },
              { "id": "1", "type": "text-end" },
              { "type": "finish-step" },
              { "finishReason": "stop", "type": "finish" }
            ]
            """,
            await UiChunksAsync(run.Result)
        );
    }

    internal static List<ModelMessage> ApprovedHistory() =>
        History(
            "tool1",
            new[] { ("call-1", "{\"value\":\"value\"}", "id-1") },
            new ToolApprovalResponseModelMessage("id-1", true)
        );

    private static async Task<(
        StreamTextResult Result,
        ScriptedModel Model,
        List<(JsonElement Input, ToolExecutionOptions Options)> ExecuteCalls,
        TestSandboxSession Sandbox
    )> RunAsync()
    {
        var executeCalls = new List<(JsonElement Input, ToolExecutionOptions Options)>();
        var model = new ScriptedModel(_ => TextStep());
        var sandbox = new TestSandboxSession();
        var tool = ValueTool(null);
        tool.ExecuteWithOptions = (input, options) =>
        {
            executeCalls.Add((input.Clone(), options));
            return Task.FromResult("result1");
        };
        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Tools = new[] { tool },
                    ToolApproval = new ToolApprovalConfiguration { { "tool1", "user-approval" } },
                    ExperimentalSandbox = sandbox,
                    StopWhen = StopWhen.IsStepCount(3),
                    GenerateId = StreamMockId("id"),
                    NowMs = () => 0,
                    Messages = ApprovedHistory(),
                }
            );
        await result.ConsumeStreamAsync();
        return (result, model, executeCalls, sandbox);
    }
}
