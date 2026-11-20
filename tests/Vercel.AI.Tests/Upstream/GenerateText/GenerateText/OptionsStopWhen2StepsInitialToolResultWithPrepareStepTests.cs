// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; options.stopWhen &gt; 2 steps: initial, tool-result with prepareStep</c>.</summary>
public sealed class OptionsStopWhen2StepsInitialToolResultWithPrepareStepTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen > 2 steps: initial, tool-result with prepareStep::";

    private sealed record PrepareStepCall(
        string ModelId,
        int StepNumber,
        IReadOnlyList<StepResult> Steps,
        IReadOnlyList<ModelMessage> Messages,
        IReadOnlyDictionary<string, object?> RuntimeContext
    );

    private sealed class Run
    {
        public GenerateTextResult Result { get; set; } = null!;
        public List<StepResult> OnStepFinishResults { get; } = [];
        public List<LanguageModelCallOptions> DoGenerateCalls { get; } = [];
        public List<PrepareStepCall> PrepareStepCalls { get; } = [];
    }

    private static async Task<Run> RunAsync()
    {
        var run = new Run();
        var responseCount = 0;
        var trueModel = new TestLanguageModel("test-response-model-id");
        trueModel.OnGenerate = options =>
        {
            run.DoGenerateCalls.Add(options);
            switch (responseCount++)
            {
                case 0:
                    return new LanguageModelGenerateResult(
                        [new GeneratedToolCall("call-1", "tool1", "{ \"value\": \"value\" }")],
                        FinishReason.ToolCalls,
                        new LanguageModelUsage(10, 5, 15),
                        null,
                        responseId: "test-id-1-from-model",
                        responseModelId: "test-response-model-id",
                        responseTimestamp: DateTimeOffset.FromUnixTimeMilliseconds(0)
                    );
                case 1:
                    return new LanguageModelGenerateResult(
                        [new GeneratedText("Hello, world!")],
                        FinishReason.Stop,
                        new LanguageModelUsage(3, 10, 13),
                        "stop",
                        responseId: "test-id-2-from-model",
                        responseModelId: "test-response-model-id",
                        responseTimestamp: DateTimeOffset.FromUnixTimeMilliseconds(10000),
                        responseHeaders: new Dictionary<string, string>
                        {
                            ["custom-response-header"] = "response-header-value",
                        }
                    );
                default:
                    throw new InvalidOperationException(
                        $"Unexpected response count: {responseCount}"
                    );
            }
        };

        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
        var result = await client.GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = new TestLanguageModel("mock-model-id"),
                Tools =
                [
                    Tool.Function(
                        "tool1",
                        null,
                        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}",
                        (_, _) => Task.FromResult("result1")
                    ),
                ],
                RuntimeContext = new Dictionary<string, object?> { ["context"] = "state1" },
                Prompt = "test-input",
                GenerateCallId = () => "test-telemetry-call-id",
                NowMs = () => 0,
                StopWhen = StopWhen.IsStepCount(3),
                OnStepEnd = (step, _) =>
                {
                    run.OnStepFinishResults.Add(step);
                    return Task.CompletedTask;
                },
                PrepareStep = context =>
                {
                    run.PrepareStepCalls.Add(
                        new PrepareStepCall(
                            context.Model!.ModelId,
                            context.StepNumber,
                            context.Steps.ToArray(),
                            context.Messages,
                            context.RuntimeContext
                        )
                    );

                    if (context.StepNumber == 0)
                    {
                        Assert.Empty(context.Steps);
                        return new PrepareStepUpdate
                        {
                            Model = trueModel,
                            ToolChoice = ToolChoice.Tool("tool1"),
                            Instructions = "system-message-0",
                            Messages = [new UserModelMessage("new input from prepareStep")],
                            RuntimeContext = new Dictionary<string, object?>
                            {
                                ["context"] = "state2",
                            },
                        };
                    }

                    if (context.StepNumber == 1)
                    {
                        Assert.Single(context.Steps);
                        return new PrepareStepUpdate
                        {
                            Model = trueModel,
                            ActiveTools = [],
                            Instructions = "system-message-1",
                            RuntimeContext = new Dictionary<string, object?>
                            {
                                ["context"] = "state3",
                            },
                        };
                    }

                    return null;
                },
            }
        );
        run.Result = result;
        return run;
    }

    [Fact]
    [UpstreamTest(Prefix + "should contain all prepareStep calls", Coverage = UpstreamCoverage.Partial)]
    public async Task Should_contain_all_prepareStep_calls()
    {
        var run = await RunAsync();
        var calls = run.PrepareStepCalls;
        Assert.Equal(2, calls.Count);

        Assert.All(calls, call => Assert.Equal("mock-model-id", call.ModelId));
        Assert.Equal(0, calls[0].StepNumber);
        Assert.Empty(calls[0].Steps);
        var first = Assert.IsType<UserModelMessage>(Assert.Single(calls[0].Messages));
        Assert.Equal("test-input", Assert.IsType<TextContentPart>(Assert.Single(first.Content)).Text);
        Assert.Equal("state1", calls[0].RuntimeContext["context"]);

        Assert.Equal(1, calls[1].StepNumber);
        Assert.Equal("state2", calls[1].RuntimeContext["context"]);
        var step = Assert.Single(calls[1].Steps);
        Assert.Equal("test-telemetry-call-id", step.CallId);
        Assert.Equal("test-id-1-from-model", step.Response.Id);
        Assert.Equal("state2", step.RuntimeContext["context"]);
        Assert.Equal(FinishReason.ToolCalls, step.FinishReason);
        Assert.Equal(3, calls[1].Messages.Count);
        Assert.Equal("user", calls[1].Messages[0].Role);
        Assert.Equal("assistant", calls[1].Messages[1].Role);
        Assert.Equal("tool", calls[1].Messages[2].Role);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "doGenerate should be called with the correct arguments",
        Coverage = UpstreamCoverage.Partial
    )]
    public async Task DoGenerate_should_be_called_with_the_correct_arguments()
    {
        var run = await RunAsync();
        var calls = run.DoGenerateCalls;
        Assert.Equal(2, calls.Count);

        Assert.Equal("tool1", Assert.Single(calls[0].Tools!).Name);
        Assert.Equal("tool", calls[0].ToolChoice!.Type);
        Assert.Equal("system", calls[0].Prompt[0].Role);
        Assert.Equal("system-message-0", ((SystemModelMessage)calls[0].Prompt[0]).Content);
        Assert.Equal("user", calls[0].Prompt[1].Role);
        Assert.Equal(2, calls[0].Prompt.Count);

        Assert.True(calls[1].Tools is null || calls[1].Tools!.Count == 0);
        Assert.Equal("system-message-1", ((SystemModelMessage)calls[1].Prompt[0]).Content);
        Assert.Equal(
            new[] { "system", "user", "assistant", "tool" },
            calls[1].Prompt.Select(message => message.Role).ToArray()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "result.text should return text from last step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Result_text_should_return_text_from_last_step()
    {
        var run = await RunAsync();
        Assert.Equal("Hello, world!", run.Result.Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "result.toolCalls should contain tool calls from all steps",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Result_toolCalls_should_contain_tool_calls_from_all_steps()
    {
        var run = await RunAsync();
        Assert.Equal(["call-1"], run.Result.ToolCalls.Select(call => call.ToolCallId).ToArray());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "result.finalStep.toolCalls should return empty tool calls from last step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Result_finalStep_toolCalls_should_return_empty_tool_calls_from_last_step()
    {
        var run = await RunAsync();
        Assert.Empty(run.Result.FinalStep.ToolCalls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "result.toolResults should contain tool results from all steps",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Result_toolResults_should_contain_tool_results_from_all_steps()
    {
        var run = await RunAsync();
        Assert.Equal(
            ["call-1"],
            run.Result.ToolResults.Select(toolResult => toolResult.ToolCallId).ToArray()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "result.finalStep.toolResults should return empty tool results from last step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Result_finalStep_toolResults_should_return_empty_tool_results_from_last_step()
    {
        var run = await RunAsync();
        Assert.Empty(run.Result.FinalStep.ToolResults);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "result.responseMessages should contain response messages from all steps",
        Coverage = UpstreamCoverage.Partial
    )]
    public async Task Result_responseMessages_should_contain_response_messages_from_all_steps()
    {
        var run = await RunAsync();
        var messages = run.Result.ResponseMessages;
        Assert.Equal(
            new[] { "assistant", "tool", "assistant" },
            messages.Select(message => message.Role).ToArray()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "result.totalUsage should sum token usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Result_totalUsage_should_sum_token_usage()
    {
        var run = await RunAsync();
        Assert.Equal(13, run.Result.TotalUsage.InputTokens);
        Assert.Equal(15, run.Result.TotalUsage.OutputTokens);
        Assert.Equal(28, run.Result.TotalUsage.TotalTokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "result.usage should sum token usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Result_usage_should_sum_token_usage()
    {
        var run = await RunAsync();
        Assert.Equal(13, run.Result.Usage.InputTokens);
        Assert.Equal(15, run.Result.Usage.OutputTokens);
        Assert.Equal(28, run.Result.Usage.TotalTokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "result.finalStep.usage should contain token usage from final step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Result_finalStep_usage_should_contain_token_usage_from_final_step()
    {
        var run = await RunAsync();
        Assert.Equal(3, run.Result.FinalStep.Usage.InputTokens);
        Assert.Equal(10, run.Result.FinalStep.Usage.OutputTokens);
        Assert.Equal(13, run.Result.FinalStep.Usage.TotalTokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "result.steps should contain all steps", Coverage = UpstreamCoverage.Partial)]
    public async Task Result_steps_should_contain_all_steps()
    {
        var run = await RunAsync();
        var steps = run.Result.Steps;
        Assert.Equal(2, steps.Count);
        Assert.All(steps, step => Assert.Equal("test-telemetry-call-id", step.CallId));
        Assert.Equal(
            new[] { "test-id-1-from-model", "test-id-2-from-model" },
            steps.Select(step => step.Response.Id).ToArray()
        );
        Assert.Equal(
            new[] { 0, 1 },
            steps.Select(step => step.StepNumber).ToArray()
        );
        Assert.Equal("state2", steps[0].RuntimeContext["context"]);
        Assert.Equal("state3", steps[1].RuntimeContext["context"]);
        Assert.Equal(
            "response-header-value",
            steps[1].Response.Headers!["custom-response-header"]
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "onStepFinish should be called for each step",
        Coverage = UpstreamCoverage.Partial
    )]
    public async Task OnStepFinish_should_be_called_for_each_step()
    {
        var run = await RunAsync();
        Assert.Equal(
            run.Result.Steps.Select(step => step.Response.Id).ToArray(),
            run.OnStepFinishResults.Select(step => step.Response.Id).ToArray()
        );
        Assert.Equal(2, run.OnStepFinishResults.Count);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "result.content should contain content from all steps",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Result_content_should_contain_content_from_all_steps()
    {
        var run = await RunAsync();
        var content = run.Result.Content;
        Assert.Equal(3, content.Count);
        var call = Assert.IsType<GeneratedToolCall>(content[0]);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("tool1", call.ToolName);
        var toolResult = Assert.IsType<GeneratedToolResult>(content[1]);
        Assert.Equal("call-1", toolResult.ToolCallId);
        Assert.Equal("result1", toolResult.ResultJson);
        Assert.Equal("Hello, world!", Assert.IsType<GeneratedText>(content[2]).Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "result.finalStep.content should contain content from the final step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Result_finalStep_content_should_contain_content_from_the_final_step()
    {
        var run = await RunAsync();
        Assert.Equal(
            "Hello, world!",
            Assert.IsType<GeneratedText>(Assert.Single(run.Result.FinalStep.Content)).Text
        );
    }
}
