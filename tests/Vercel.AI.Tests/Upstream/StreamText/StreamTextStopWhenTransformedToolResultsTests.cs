// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;
using static Vercel.AI.Tests.Upstream.StreamText.StreamTextTransformTestSupport;

namespace Vercel.AI.Tests.Upstream.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText</c> &gt; <c>options.stopWhen</c> &gt; <c>2 steps: initial, tool-result with transformed tool results</c>.</summary>
public sealed class StreamTextStopWhenTransformedToolResultsTests
{
    private const string Suite =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.stopWhen > 2 steps: initial, tool-result with transformed tool results";

    private const string ValueSchema =
        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    // Maps to upperCaseToolResultTransform.
    private static StreamTextTransform UpperCaseToolResult =>
        _ =>
            Map(chunk =>
                chunk is ToolResultPart { Result.Dynamic: false } result
                && result.Result.Preliminary != true
                    ?
                    [
                        new ToolResultPart(
                            new ExecutedTool(
                                result.Result.ToolCallId,
                                result.Result.ToolName,
                                result.Result.OutputJson.ToUpperInvariant(),
                                false
                            )
                            {
                                InputJson = JsonSerializer.Serialize(
                                    new
                                    {
                                        value = JsonDocument
                                            .Parse(result.Result.InputJson!)
                                            .RootElement.GetProperty("value")
                                            .GetString()!
                                            .ToUpperInvariant(),
                                    }
                                ),
                            }
                        ),
                    ]
                    : [chunk]
            );

    private sealed record Run(
        StreamTextResult Result,
        List<StepResult> StepResults,
        List<GenerateTextResult> Finishes
    );

    private static Run Start()
    {
        var steps = new List<StepResult>();
        var finishes = new List<GenerateTextResult>();
        var model = new CallsModel(
            (call, _) =>
                call switch
                {
                    0 => new CallScript(
                        [
                            new ResponseMetadataStreamPart(
                                "id-0",
                                "mock-model-id",
                                DateTimeOffset.UnixEpoch
                            ),
                            new ReasoningStartStreamPart("id-0"),
                            new ReasoningDeltaStreamPart("id-0", "thinking"),
                            new ReasoningEndStreamPart("id-0"),
                            new ToolCallStreamPart("call-1", "tool1", "{ \"value\": \"value\" }"),
                            new FinishStreamPart(FinishReason.ToolCalls, TestUsage, null),
                        ],
                        Headers: new Dictionary<string, string> { ["call"] = "1" }
                    ),
                    1 => new CallScript(
                        [
                            new ResponseMetadataStreamPart(
                                "id-1",
                                "mock-model-id",
                                DateTimeOffset.FromUnixTimeMilliseconds(1000)
                            ),
                            new TextStartStreamPart("1"),
                            new TextDeltaStreamPart("1", "Hello, "),
                            new TextDeltaStreamPart("1", "world!"),
                            new TextEndStreamPart("1"),
                            new FinishStreamPart(FinishReason.Stop, TestUsage2, "stop"),
                        ],
                        Headers: new Dictionary<string, string> { ["call"] = "2" }
                    ),
                    _ => throw new InvalidOperationException(
                        $"Unexpected response count: {call + 1}"
                    ),
                }
        );
        var result = SteeringTestSupport
            .Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Tools =
                    [
                        Tool.Function("tool1", null, ValueSchema, (_, _) =>
                            Task.FromResult("result1")
                        ),
                    ],
                    ExperimentalTransform = [UpperCaseToolResult],
                    Prompt = "test-input",
                    OnEnd = (end, _) =>
                    {
                        finishes.Add(end);
                        return Task.CompletedTask;
                    },
                    OnStepEnd = (step, _) =>
                    {
                        steps.Add(step);
                        return Task.CompletedTask;
                    },
                    StopWhen = StopWhen.IsStepCount(3),
                    NowMs = MockValues(0, 0, 100, 500, 600, 1000),
                    GenerateId = MockId(),
                    GenerateCallId = () => "test-telemetry-call-id",
                }
            );
        return new Run(result, steps, finishes);
    }

    private static void AssertUsage(
        LanguageModelUsage usage,
        int input,
        int output,
        int? cacheRead,
        int? cacheWrite,
        int? reasoning,
        int text
    )
    {
        Assert.Equal(input, usage.InputTokens);
        Assert.Equal(input, usage.NoCacheInputTokens);
        Assert.Equal(cacheRead, usage.CacheReadTokens);
        Assert.Equal(cacheWrite, usage.CacheWriteTokens);
        Assert.Equal(output, usage.OutputTokens);
        Assert.Equal(reasoning, usage.ReasoningTokens);
        Assert.Equal(text, usage.TextTokens);
        Assert.Equal(input + output, usage.TotalTokens);
    }

    private static void AssertToolCall(GeneratedToolCall call)
    {
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("tool1", call.ToolName);
        Assert.True(
            JsonElement.DeepEquals(
                JsonDocument.Parse(call.ArgumentsJson).RootElement,
                JsonDocument.Parse("{\"value\":\"value\"}").RootElement
            )
        );
        Assert.False(call.Dynamic);
        Assert.False(call.ProviderExecuted);
        Assert.Null(call.ProviderMetadata);
        Assert.Null(call.Title);
    }

    private static void AssertToolResult(ExecutedTool result)
    {
        Assert.False(result.Dynamic);
        Assert.Equal("call-1", result.ToolCallId);
        Assert.Equal("tool1", result.ToolName);
        Assert.Equal("RESULT1", result.OutputJson);
        Assert.Equal(
            "VALUE",
            JsonDocument.Parse(result.InputJson!).RootElement.GetProperty("value").GetString()
        );
    }

    private static void AssertToolContent(IReadOnlyList<GeneratedContent> content, int offset)
    {
        var reasoning = Assert.IsType<GeneratedReasoning>(content[offset]);
        Assert.Equal("thinking", reasoning.Text);
        Assert.Null(reasoning.ProviderMetadata);
        AssertToolCall(Assert.IsType<GeneratedToolCall>(content[offset + 1]));
        var result = Assert.IsType<GeneratedToolResult>(content[offset + 2]);
        Assert.Equal("call-1", result.ToolCallId);
        Assert.Equal("tool1", result.ToolName);
        Assert.Equal("RESULT1", result.ResultJson);
    }

    // chunk: the finish-step chunk carries only the summary fields of the step, not its content, messages or tool lists.
    private static void AssertStep1(StepResult step, bool chunk = false)
    {
        if (!chunk)
        {
            Assert.Equal("test-telemetry-call-id", step.CallId);
            Assert.Equal(0, step.StepNumber);
            Assert.Equal(3, step.Content.Count);
            AssertToolContent(step.Content, 0);
        }

        Assert.Equal(FinishReason.ToolCalls, step.FinishReason);
        Assert.Null(step.RawFinishReason);
        Assert.Equal("mock-provider", step.Model.Provider);
        Assert.Equal("mock-model-id", step.Model.ModelId);
        var performance = step.Performance;
        Assert.Equal(16.666666666666668, performance.EffectiveOutputTokensPerSecond);
        Assert.Equal(21.666666666666668, performance.EffectiveTotalTokensPerSecond);
        Assert.Equal(30, performance.InputTokensPerSecond);
        Assert.Equal(20, performance.OutputTokensPerSecond);
        Assert.Equal(600, performance.ResponseTimeMs);
        Assert.Equal(1000, performance.StepTimeMs);
        var gaps = performance.TimeBetweenOutputChunksMs!;
        Assert.Equal(
            [400, 400, 400, 400, 400, 400],
            [gaps.Avg, gaps.Max, gaps.Median, gaps.Min, gaps.P10, gaps.P90]
        );
        Assert.Equal(100, performance.TimeToFirstOutputMs);
        var execution = Assert.Single(performance.ToolExecutionMs);
        Assert.Equal("call-1", execution.Key);
        Assert.Equal(0, execution.Value);
        Assert.Null(step.ProviderMetadata);
        Assert.Null(step.Request.Body);
        Assert.Null(step.Request.Messages);
        Assert.Equal("1", Assert.Single(step.Response.Headers!).Value);
        Assert.Equal("id-0", step.Response.Id);
        Assert.Equal("mock-model-id", step.Response.ModelId);
        Assert.Equal(DateTimeOffset.UnixEpoch, step.Response.Timestamp);
        AssertUsage(step.Usage, 3, 10, null, null, null, 10);
        Assert.Null(step.Usage.Raw);
        if (chunk)
        {
            return;
        }

        Assert.Equal(2, step.Response.Messages.Count);
        var assistant = Assert.IsType<AssistantModelMessage>(step.Response.Messages[0]);
        Assert.Equal(2, assistant.Parts.Count);
        Assert.Equal("thinking", Assert.IsType<AssistantReasoningPart>(assistant.Parts[0]).Text);
        AssertToolCall(Assert.IsType<AssistantToolCallPart>(assistant.Parts[1]).ToolCall);
        var tool = Assert.IsType<ToolModelMessage>(step.Response.Messages[1]);
        Assert.Equal("call-1", tool.ToolCallId);
        Assert.Equal("tool1", tool.ToolName);
        Assert.Equal("text", tool.OutputType);
        Assert.Equal("RESULT1", tool.OutputJson);
        Assert.Empty(step.RuntimeContext);
        Assert.Empty(step.ToolsContext);
        Assert.Empty(step.Warnings);
        AssertToolCall(Assert.Single(step.ToolCalls));
        AssertToolResult(Assert.Single(step.ToolResults));
    }

    private static void AssertStep2(StepResult step, bool chunk = false)
    {
        if (!chunk)
        {
            Assert.Equal("test-telemetry-call-id", step.CallId);
            Assert.Equal(1, step.StepNumber);
            var text = Assert.IsType<GeneratedText>(Assert.Single(step.Content));
            Assert.Equal("Hello, world!", text.Text);
            Assert.Null(text.ProviderMetadata);
        }

        Assert.Equal(FinishReason.Stop, step.FinishReason);
        Assert.Equal("stop", step.RawFinishReason);
        Assert.Equal("mock-provider", step.Model.Provider);
        Assert.Equal("mock-model-id", step.Model.ModelId);
        var performance = step.Performance;
        Assert.Equal(0, performance.EffectiveOutputTokensPerSecond);
        Assert.Equal(0, performance.EffectiveTotalTokensPerSecond);
        Assert.Equal(0, performance.InputTokensPerSecond);
        Assert.Equal(0, performance.OutputTokensPerSecond);
        Assert.Equal(0, performance.ResponseTimeMs);
        Assert.Equal(0, performance.StepTimeMs);
        var gaps = performance.TimeBetweenOutputChunksMs!;
        Assert.Equal(
            [0, 0, 0, 0, 0, 0],
            [gaps.Avg, gaps.Max, gaps.Median, gaps.Min, gaps.P10, gaps.P90]
        );
        Assert.Equal(0, performance.TimeToFirstOutputMs);
        Assert.Empty(performance.ToolExecutionMs);
        Assert.Null(step.ProviderMetadata);
        Assert.Null(step.Request.Body);
        Assert.Null(step.Request.Messages);
        Assert.Equal("2", Assert.Single(step.Response.Headers!).Value);
        Assert.Equal("id-1", step.Response.Id);
        Assert.Equal("mock-model-id", step.Response.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(1000), step.Response.Timestamp);
        AssertUsage(step.Usage, 3, 10, 0, 0, 10, 10);
        Assert.Null(step.Usage.Raw);
        if (chunk)
        {
            return;
        }

        var message = Assert.IsType<AssistantModelMessage>(Assert.Single(step.Response.Messages));
        Assert.Equal(
            "Hello, world!",
            Assert.IsType<AssistantTextPart>(Assert.Single(message.Parts)).Text
        );
        Assert.Empty(step.RuntimeContext);
        Assert.Empty(step.ToolsContext);
        Assert.Empty(step.Warnings);
        Assert.Empty(step.ToolCalls);
        Assert.Empty(step.ToolResults);
    }

    [Fact]
    [UpstreamTest(
        Suite + "::should contain assistant response message and tool message from all steps",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_contain_assistant_response_message_and_tool_message_from_all_steps()
    {
        var run = Start();

        var parts = await PartsAsync(run.Result);

        Assert.Equal(
            [
                "start",
                "start-step",
                "reasoning-start",
                "reasoning-delta",
                "reasoning-end",
                "tool-call",
                "tool-result",
                "finish-step",
                "start-step",
                "text-start",
                "text-delta",
                "text-delta",
                "text-end",
                "finish-step",
                "finish",
            ],
            parts.Select(part => part.Type)
        );
        foreach (var startStep in parts.OfType<StartStepPart>())
        {
            Assert.Null(startStep.Request.Body);
            Assert.Null(startStep.Request.Messages);
            Assert.Empty(startStep.Warnings);
        }

        Assert.Equal("id-0", Assert.IsType<ReasoningStartPart>(parts[2]).Id);
        var reasoningDelta = Assert.IsType<ReasoningDeltaPart>(parts[3]);
        Assert.Equal("id-0", reasoningDelta.Id);
        Assert.Equal("thinking", reasoningDelta.Text);
        Assert.Null(reasoningDelta.ProviderMetadata);
        Assert.Equal("id-0", Assert.IsType<ReasoningEndPart>(parts[4]).Id);
        AssertToolCall(Assert.IsType<ToolCallPart>(parts[5]).ToolCall);
        AssertToolResult(Assert.IsType<ToolResultPart>(parts[6]).Result);
        var firstFinishStep = Assert.IsType<StepFinishPart>(parts[7]);
        AssertStep1(firstFinishStep.Step, chunk: true);
        Assert.Equal(FinishReason.ToolCalls, firstFinishStep.Step.FinishReason);
        Assert.Equal("1", Assert.Single(firstFinishStep.Response.Headers!).Value);
        Assert.Equal("1", Assert.IsType<TextStartPart>(parts[9]).Id);
        foreach (var (index, text) in new[] { (10, "Hello, "), (11, "world!") })
        {
            var delta = Assert.IsType<TextDeltaPart>(parts[index]);
            Assert.Equal("1", delta.Id);
            Assert.Equal(text, delta.Text);
            Assert.Null(delta.ProviderMetadata);
        }

        Assert.Equal("1", Assert.IsType<TextEndPart>(parts[12]).Id);
        var secondFinishStep = Assert.IsType<StepFinishPart>(parts[13]);
        AssertStep2(secondFinishStep.Step, chunk: true);
        var streamFinish = Assert.IsType<FinishPart>(parts[14]);
        Assert.Equal(FinishReason.Stop, streamFinish.FinishReason);
        AssertUsage(streamFinish.Usage, 6, 20, 0, 0, 10, 20);
        var messages = await run.Result.ResponseMessages;
        Assert.Equal(3, messages.Count);
        var assistant = Assert.IsType<AssistantModelMessage>(messages[0]);
        Assert.Equal("thinking", Assert.IsType<AssistantReasoningPart>(assistant.Parts[0]).Text);
        Assert.IsType<AssistantToolCallPart>(assistant.Parts[1]);
        Assert.IsType<ToolModelMessage>(messages[1]);
        var final = Assert.IsType<AssistantModelMessage>(messages[2]);
        Assert.Equal(
            "Hello, world!",
            Assert.IsType<AssistantTextPart>(Assert.Single(final.Parts)).Text
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + " > callbacks::onFinish should send correct information",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task OnFinish_should_send_correct_information()
    {
        var run = Start();
        await run.Result.ConsumeStreamAsync();

        var finish = Assert.Single(run.Finishes);

        Assert.Equal("test-telemetry-call-id", finish.CallId);
        Assert.Equal(4, finish.Content.Count);
        AssertToolContent(finish.Content, 0);
        Assert.Equal("Hello, world!", Assert.IsType<GeneratedText>(finish.Content[3]).Text);
        Assert.Empty(finish.DynamicToolCalls);
        Assert.Empty(finish.DynamicToolResults);
        Assert.Empty(finish.Files);
        Assert.Equal(2, finish.Steps.Count);
        AssertStep1(finish.Steps[0]);
        AssertStep2(finish.Steps[1]);
        Assert.Same(finish.Steps[1], finish.FinalStep);
        AssertStep2(finish.FinalStep);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("stop", finish.RawFinishReason);
        Assert.Equal("mock-provider", finish.Model.Provider);
        Assert.Equal("mock-model-id", finish.Model.ModelId);
        Assert.Null(finish.ProviderMetadata);
        Assert.Empty(finish.Reasoning);
        Assert.Equal("thinking", finish.Steps[0].ReasoningText);
        Assert.Null(finish.Request.Body);
        Assert.Null(finish.Request.Messages);
        Assert.Same(finish.FinalStep.Response, finish.Response);
        Assert.Equal(3, finish.ResponseMessages.Count);
        Assert.IsType<AssistantModelMessage>(finish.ResponseMessages[0]);
        Assert.IsType<ToolModelMessage>(finish.ResponseMessages[1]);
        var final = Assert.IsType<AssistantModelMessage>(finish.ResponseMessages[2]);
        Assert.Equal(
            "Hello, world!",
            Assert.IsType<AssistantTextPart>(Assert.Single(final.Parts)).Text
        );
        Assert.Empty(finish.RuntimeContext);
        Assert.Empty(finish.Sources);
        Assert.Equal(1, finish.StepNumber);
        Assert.Equal("Hello, world!", finish.Text);
        AssertToolCall(Assert.Single(finish.ToolCalls));
        AssertToolCall(Assert.Single(finish.StaticToolCalls));
        AssertToolResult(Assert.Single(finish.ToolResults));
        AssertToolResult(Assert.Single(finish.StaticToolResults));
        Assert.Empty(finish.ToolsContext);
        AssertUsage(finish.TotalUsage, 6, 20, 0, 0, 10, 20);
        AssertUsage(finish.Usage, 6, 20, 0, 0, 10, 20);
        Assert.Empty(finish.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Suite + " > callbacks::onStepFinish should send correct information",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task OnStepFinish_should_send_correct_information()
    {
        var run = Start();
        await run.Result.ConsumeStreamAsync();

        Assert.Equal(2, run.StepResults.Count);
        AssertStep1(run.StepResults[0]);
        AssertStep2(run.StepResults[1]);
    }
}
