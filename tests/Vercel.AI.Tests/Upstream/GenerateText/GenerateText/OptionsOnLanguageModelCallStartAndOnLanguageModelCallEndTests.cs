// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.ResultToolCallsTests;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; options.onLanguageModelCallStart and onLanguageModelCallEnd</c>.</summary>
public sealed class OptionsOnLanguageModelCallStartAndOnLanguageModelCallEndTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.onLanguageModelCallStart and onLanguageModelCallEnd::";

    [Fact]
    [UpstreamTest(
        Prefix + "should fire the model-call callbacks before tool execution and step finish",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port has no onStepStart, so it is not part of the call order. See #623."
    )]
    public async Task Should_fire_the_model_call_callbacks_before_tool_execution_and_step_finish()
    {
        var callOrder = new List<string>();
        LanguageModelCallStartEvent? modelCallStartEvent = null;

        await ToolExecutionCallbackSupport
            .Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => ToolCallResponse("{ \"value\": \"test\" }"),
                    },
                    Tools = new[] { Tool1() },
                    Instructions = Instructions.From("test-system"),
                    Prompt = "test-input",
                    MaxOutputTokens = 128,
                    Temperature = 0.7,
                    TopP = 0.9,
                    TopK = 40,
                    PresencePenalty = 0.2,
                    FrequencyPenalty = 0.1,
                    StopSequences = ["stop"],
                    Seed = 123,
                    Reasoning = "high",
                    OnLanguageModelCallStart = (@event, _) =>
                    {
                        callOrder.Add("onLanguageModelCallStart");
                        modelCallStartEvent = @event;
                        return Task.CompletedTask;
                    },
                    OnLanguageModelCallEnd = (_, _) =>
                    {
                        callOrder.Add("onLanguageModelCallEnd");
                        return Task.CompletedTask;
                    },
                    OnToolExecutionStart = (_, _) =>
                    {
                        callOrder.Add("onToolExecutionStart");
                        return Task.CompletedTask;
                    },
                    OnToolExecutionEnd = (_, _) =>
                    {
                        callOrder.Add("onToolExecutionEnd");
                        return Task.CompletedTask;
                    },
                    OnStepEnd = (_, _) =>
                    {
                        callOrder.Add("onStepFinish");
                        return Task.CompletedTask;
                    },
                    GenerateCallId = () => "test-telemetry-call-id",
                }
            );

        Assert.Equal(
            [
                "onLanguageModelCallStart",
                "onLanguageModelCallEnd",
                "onToolExecutionStart",
                "onToolExecutionEnd",
                "onStepFinish",
            ],
            callOrder
        );
        var start = Assert.IsType<LanguageModelCallStartEvent>(modelCallStartEvent);
        Assert.Equal("test-telemetry-call-id", start.CallId);
        Assert.Equal("test-system", start.Instructions!.Text);
        var message = Assert.IsType<UserModelMessage>(Assert.Single(start.Messages));
        Assert.Equal("test-input", Assert.IsType<TextContentPart>(message.Content[0]).Text);
        Assert.Equal("test", start.Provider);
        Assert.Equal(128, start.CallOptions.MaxOutputTokens);
        Assert.Equal(0.7, start.CallOptions.Temperature);
        Assert.Equal(0.9, start.CallOptions.TopP);
        Assert.Equal(40, start.CallOptions.TopK);
        Assert.Equal(0.2, start.CallOptions.PresencePenalty);
        Assert.Equal(0.1, start.CallOptions.FrequencyPenalty);
        Assert.Equal(["stop"], start.CallOptions.StopSequences);
        Assert.Equal(123, start.CallOptions.Seed);
        Assert.Equal("high", start.CallOptions.Reasoning);
        var tool = Assert.Single(start.Tools!);
        Assert.Equal("tool1", tool.Name);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should provide parsed tool calls on model-call end",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port's end event has no usage detail breakdown and its ResponseTime is a TimeSpan. See #623."
    )]
    public async Task Should_provide_parsed_tool_calls_on_model_call_end()
    {
        var modelCallEndEvents = new List<LanguageModelCallEndEvent>();

        await ToolExecutionCallbackSupport
            .Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => new LanguageModelGenerateResult(
                            new GeneratedContent[]
                            {
                                new GeneratedText("Before tool."),
                                new GeneratedToolCall(
                                    "call-1",
                                    "tool1",
                                    "{ \"value\": \"test-arg\" }"
                                ),
                            },
                            FinishReason.ToolCalls,
                            new LanguageModelUsage(3, 10, 13),
                            "tool-calls",
                            responseId: "response-1",
                            responseModelId: "mock-response-model"
                        ),
                    },
                    Tools = new[] { Tool1() },
                    OnLanguageModelCallEnd = (@event, _) =>
                    {
                        modelCallEndEvents.Add(@event);
                        return Task.CompletedTask;
                    },
                    GenerateCallId = () => "test-telemetry-call-id",
                    Prompt = "test-input",
                }
            );

        var end = Assert.Single(modelCallEndEvents);
        Assert.Equal("test-telemetry-call-id", end.CallId);
        Assert.Equal("Before tool.", Assert.IsType<GeneratedText>(end.Content[0]).Text);
        var toolCall = Assert.IsType<GeneratedToolCall>(end.Content[1]);
        Assert.Equal("call-1", toolCall.ToolCallId);
        Assert.Equal("tool1", toolCall.ToolName);
        Assert.Equal("{\"value\":\"test-arg\"}", Compact(toolCall.ArgumentsJson));
        Assert.Equal(FinishReason.ToolCalls, end.FinishReason);
        Assert.Equal("mock-response-model", end.ModelId);
        Assert.Equal("test", end.Provider);
        Assert.Equal("response-1", end.ResponseId);
        Assert.Equal(13, end.Usage.TotalTokens);
    }
}
