// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
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

    private static Tool CreateTool1()
    {
        var tool = Tool.Function(
            "tool1",
            null,
            "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}",
            null
        );
        tool.ExecuteWithOptions = (input, options) =>
        {
            Assert.Equal("""{"value":"value"}""", input.GetRawText().Replace(" ", ""));
            var message = Assert.IsType<UserModelMessage>(Assert.Single(options.Messages));
            Assert.Equal(
                "new input from prepareStep",
                Assert.IsType<TextContentPart>(Assert.Single(message.Content)).Text
            );
            return Task.FromResult("result1");
        };
        return tool;
    }

    private static void AssertToolCallMessage(ModelMessage message)
    {
        var assistant = Assert.IsType<AssistantModelMessage>(message);
        var part = Assert.IsType<AssistantToolCallPart>(Assert.Single(assistant.Parts));
        Assert.Equal("call-1", part.ToolCall.ToolCallId);
        Assert.Equal("tool1", part.ToolCall.ToolName);
        Assert.Equal("""{"value":"value"}""", part.ToolCall.ArgumentsJson.Replace(" ", ""));
        Assert.False(part.ToolCall.ProviderExecuted);
        Assert.Null(part.ProviderOptions);
        Assert.Null(assistant.ProviderOptions);
    }

    private static void AssertToolResultMessage(ModelMessage message)
    {
        var tool = Assert.IsType<ToolModelMessage>(message);
        var result = Assert.Single(tool.Results);
        Assert.Equal("call-1", result.ToolCallId);
        Assert.Equal("tool1", result.ToolName);
        Assert.Equal("text", result.OutputType);
        Assert.Equal("result1", result.OutputJson);
    }

    private static void AssertTextAssistantMessage(ModelMessage message)
    {
        var assistant = Assert.IsType<AssistantModelMessage>(message);
        var part = Assert.IsType<AssistantTextPart>(Assert.Single(assistant.Parts));
        Assert.Equal("Hello, world!", part.Text);
        Assert.Null(part.ProviderOptions);
    }

    private static void AssertStepPerformance(StepPerformance performance, bool withTool)
    {
        Assert.Equal(0, performance.EffectiveOutputTokensPerSecond);
        Assert.Equal(0, performance.EffectiveTotalTokensPerSecond);
        Assert.Null(performance.InputTokensPerSecond);
        Assert.Null(performance.OutputTokensPerSecond);
        Assert.Equal(0, performance.ResponseTimeMs);
        Assert.Equal(0, performance.StepTimeMs);
        Assert.Null(performance.TimeToFirstOutputMs);
        if (withTool)
        {
            var execution = Assert.Single(performance.ToolExecutionMs);
            Assert.Equal("call-1", execution.Key);
            Assert.Equal(0, execution.Value);
        }
        else
        {
            Assert.Empty(performance.ToolExecutionMs);
        }
    }

    private static void AssertUsage(LanguageModelUsage usage, int input, int output)
    {
        Assert.Equal(input, usage.InputTokens);
        Assert.Null(usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(input, usage.NoCacheInputTokens);
        Assert.Equal(output, usage.OutputTokens);
        Assert.Null(usage.ReasoningTokens);
        Assert.Equal(output, usage.TextTokens);
        Assert.Null(usage.Raw);
        Assert.Equal(input + output, usage.TotalTokens);
    }

    private static void AssertStep0(StepResult step)
    {
        Assert.Equal("test-telemetry-call-id", step.CallId);
        Assert.Equal(2, step.Content.Count);
        var call = Assert.IsType<GeneratedToolCall>(step.Content[0]);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("tool1", call.ToolName);
        Assert.Equal("""{"value":"value"}""", call.ArgumentsJson.Replace(" ", ""));
        Assert.False(call.ProviderExecuted);
        Assert.Null(call.ProviderMetadata);
        var toolResult = Assert.IsType<GeneratedToolResult>(step.Content[1]);
        Assert.Equal("call-1", toolResult.ToolCallId);
        Assert.Equal("tool1", toolResult.ToolName);
        Assert.Equal("result1", toolResult.ResultJson);
        Assert.False(toolResult.Dynamic);
        var executed = Assert.Single(step.ToolResults);
        Assert.Equal("""{"value":"value"}""", executed.InputJson!.Replace(" ", ""));
        Assert.Equal(FinishReason.ToolCalls, step.FinishReason);
        Assert.Equal("mock-model-id", step.Model.ModelId);
        Assert.Equal("test", step.Model.Provider);
        AssertStepPerformance(step.Performance, withTool: true);
        Assert.Null(step.ProviderMetadata);
        Assert.Null(step.RawFinishReason);
        Assert.Null(step.Request.Body);
        Assert.Null(step.Request.Messages);
        Assert.Null(step.Response.Body);
        Assert.Empty(step.Response.Headers!); // upstream: undefined; the port keeps an empty map
        Assert.Equal("test-id-1-from-model", step.Response.Id);
        Assert.Equal(2, step.Response.Messages.Count);
        AssertToolCallMessage(step.Response.Messages[0]);
        AssertToolResultMessage(step.Response.Messages[1]);
        Assert.Equal("test-response-model-id", step.Response.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(0), step.Response.Timestamp);
        Assert.Equal("state2", Assert.Single(step.RuntimeContext).Value);
        Assert.Equal(0, step.StepNumber);
        Assert.Empty(step.ToolsContext);
        AssertUsage(step.Usage, 10, 5);
        Assert.Empty(step.Warnings);
    }

    private static void AssertStep1(StepResult step)
    {
        Assert.Equal("test-telemetry-call-id", step.CallId);
        Assert.Equal(
            "Hello, world!",
            Assert.IsType<GeneratedText>(Assert.Single(step.Content)).Text
        );
        Assert.Equal(FinishReason.Stop, step.FinishReason);
        Assert.Equal("mock-model-id", step.Model.ModelId);
        Assert.Equal("test", step.Model.Provider);
        AssertStepPerformance(step.Performance, withTool: false);
        Assert.Null(step.ProviderMetadata);
        Assert.Equal("stop", step.RawFinishReason);
        Assert.Null(step.Request.Body);
        Assert.Null(step.Request.Messages);
        Assert.Null(step.Response.Body);
        var header = Assert.Single(step.Response.Headers!);
        Assert.Equal("custom-response-header", header.Key);
        Assert.Equal("response-header-value", header.Value);
        Assert.Equal("test-id-2-from-model", step.Response.Id);
        AssertTextAssistantMessage(Assert.Single(step.Response.Messages));
        Assert.Equal("test-response-model-id", step.Response.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(10000), step.Response.Timestamp);
        Assert.Equal("state3", Assert.Single(step.RuntimeContext).Value);
        Assert.Equal(1, step.StepNumber);
        Assert.Empty(step.ToolsContext);
        AssertUsage(step.Usage, 3, 10);
        Assert.Empty(step.Warnings);
    }

    private static void AssertUserMessage(ModelMessage message, string text)
    {
        var user = Assert.IsType<UserModelMessage>(message);
        Assert.Equal(text, Assert.IsType<TextContentPart>(Assert.Single(user.Content)).Text);
        Assert.Null(user.ProviderOptions);
    }

    private static void AssertToolDefinition(LanguageModelTool tool)
    {
        Assert.Equal("tool1", tool.Name);
        Assert.Null(tool.Description);
        var schema = tool.InputSchema;
        Assert.Equal("object", schema.GetProperty("type").GetString());
        Assert.Equal(
            "string",
            schema.GetProperty("properties").GetProperty("value").GetProperty("type").GetString()
        );
        Assert.Equal(
            "value",
            Assert.Single(schema.GetProperty("required").EnumerateArray()).GetString()
        );
    }

    private static async Task<Run> RunAsync()
    {
        var run = new Run();
        var responseCount = 0;
        var trueModel = new TestLanguageModel("mock-model-id");
        trueModel.OnGenerate = options =>
        {
            run.DoGenerateCalls.Add(options);
            switch (responseCount++)
            {
                case 0:
                    return new LanguageModelGenerateResult(
                        [new GeneratedToolCall("call-1", "tool1", "{ \"value\": \"value\" }")],
                        FinishReason.ToolCalls,
                        new LanguageModelUsage(10, 5, 15, noCacheInputTokens: 10, textTokens: 5),
                        null,
                        responseId: "test-id-1-from-model",
                        responseModelId: "test-response-model-id",
                        responseTimestamp: DateTimeOffset.FromUnixTimeMilliseconds(0)
                    );
                case 1:
                    return new LanguageModelGenerateResult(
                        [new GeneratedText("Hello, world!")],
                        FinishReason.Stop,
                        new LanguageModelUsage(3, 10, 13, noCacheInputTokens: 3, textTokens: 10),
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
                Tools = [CreateTool1()],
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
                            context.Steps,
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
    [UpstreamTest(
        Prefix + "should contain all prepareStep calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_contain_all_prepareStep_calls()
    {
        var run = await RunAsync();
        var calls = run.PrepareStepCalls;
        Assert.Equal(2, calls.Count);

        Assert.All(calls, call => Assert.Equal("mock-model-id", call.ModelId));

        Assert.Equal(0, calls[0].StepNumber);
        AssertUserMessage(Assert.Single(calls[0].Messages), "test-input");
        Assert.Equal("state1", Assert.Single(calls[0].RuntimeContext).Value);
        // Upstream records the live steps array, so both calls see both finished steps.
        Assert.Equal(2, calls[0].Steps.Count);
        AssertStep0(calls[0].Steps[0]);
        AssertStep1(calls[0].Steps[1]);

        Assert.Equal(1, calls[1].StepNumber);
        Assert.Equal(3, calls[1].Messages.Count);
        AssertUserMessage(calls[1].Messages[0], "new input from prepareStep");
        AssertToolCallMessage(calls[1].Messages[1]);
        AssertToolResultMessage(calls[1].Messages[2]);
        Assert.Equal("state2", Assert.Single(calls[1].RuntimeContext).Value);
        Assert.Equal(2, calls[1].Steps.Count);
        AssertStep0(calls[1].Steps[0]);
        AssertStep1(calls[1].Steps[1]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "doGenerate should be called with the correct arguments",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task DoGenerate_should_be_called_with_the_correct_arguments()
    {
        var run = await RunAsync();
        var calls = run.DoGenerateCalls;
        Assert.Equal(2, calls.Count);

        Assert.Equal(2, calls[0].Prompt.Count);
        Assert.Equal(
            "system-message-0",
            Assert.IsType<SystemModelMessage>(calls[0].Prompt[0]).Content
        );
        AssertUserMessage(calls[0].Prompt[1], "new input from prepareStep");
        var choice = Assert.IsType<ToolChoice.NamedChoice>(calls[0].ToolChoice);
        Assert.Equal("tool1", choice.ToolName);
        AssertToolDefinition(Assert.Single(calls[0].Tools!));

        Assert.Equal(4, calls[1].Prompt.Count);
        Assert.Equal(
            "system-message-1",
            Assert.IsType<SystemModelMessage>(calls[1].Prompt[0]).Content
        );
        AssertUserMessage(calls[1].Prompt[1], "new input from prepareStep");
        AssertToolCallMessage(calls[1].Prompt[2]);
        AssertToolResultMessage(calls[1].Prompt[3]);
        Assert.Equal("auto", calls[1].ToolChoice!.Type);
        Assert.True(calls[1].Tools is null);
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
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Result_responseMessages_should_contain_response_messages_from_all_steps()
    {
        var run = await RunAsync();
        var messages = run.Result.ResponseMessages;
        Assert.Equal(3, messages.Count);
        AssertToolCallMessage(messages[0]);
        AssertToolResultMessage(messages[1]);
        AssertTextAssistantMessage(messages[2]);
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
    [UpstreamTest(
        Prefix + "result.steps should contain all steps",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Result_steps_should_contain_all_steps()
    {
        var run = await RunAsync();
        var steps = run.Result.Steps;
        Assert.Equal(2, steps.Count);
        AssertStep0(steps[0]);
        AssertStep1(steps[1]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "onStepFinish should be called for each step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task OnStepFinish_should_be_called_for_each_step()
    {
        var run = await RunAsync();
        Assert.Equal(2, run.OnStepFinishResults.Count);
        AssertStep0(run.OnStepFinishResults[0]);
        AssertStep1(run.OnStepFinishResults[1]);
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
