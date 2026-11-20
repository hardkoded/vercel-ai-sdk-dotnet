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
                            Finish(FinishReason.ToolCalls, "tool-calls"),
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
                            new FinishStreamPart(
                                FinishReason.Stop,
                                new LanguageModelUsage(3, 10, 13),
                                "stop"
                            ),
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
                    GenerateCallId = () => "test-telemetry-call-id",
                }
            );
        return new Run(result, steps, finishes);
    }

    [Fact]
    [UpstreamTest(
        Suite + "::should contain assistant response message and tool message from all steps",
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts the parts and the values of the stream, not the performance numbers of finish-step."
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
        var toolResult = Assert.IsType<ToolResultPart>(parts[6]).Result;
        Assert.Equal("RESULT1", toolResult.OutputJson);
        Assert.Equal(
            "VALUE",
            JsonDocument.Parse(toolResult.InputJson!).RootElement.GetProperty("value").GetString()
        );
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
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts the fields of the end event as the port exposes them, not the full snapshot."
    )]
    public async Task OnFinish_should_send_correct_information()
    {
        var run = Start();
        await run.Result.ConsumeStreamAsync();

        var finish = Assert.Single(run.Finishes);

        Assert.Equal("Hello, world!", finish.Text);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal(2, finish.Steps.Count);
        Assert.Equal("RESULT1", Assert.Single(finish.Steps[0].ToolResults).OutputJson);
        Assert.Equal(
            ["reasoning", "tool-call", "tool-result"],
            finish.Steps[0].Content.Select(part => part.Type)
        );
        Assert.Equal(
            "RESULT1",
            Assert.IsType<GeneratedToolResult>(finish.Steps[0].Content[2]).ResultJson
        );
        Assert.Equal(3, finish.ResponseMessages.Count);
        Assert.Equal(6, finish.Usage.InputTokens);
        Assert.Equal(20, finish.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(
        Suite + " > callbacks::onStepFinish should send correct information",
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts the fields of each step as the port exposes them, not the full snapshot or the performance numbers."
    )]
    public async Task OnStepFinish_should_send_correct_information()
    {
        var run = Start();
        await run.Result.ConsumeStreamAsync();

        Assert.Equal(2, run.StepResults.Count);
        var first = run.StepResults[0];
        Assert.Equal(0, first.StepNumber);
        Assert.Equal("test-telemetry-call-id", first.CallId);
        Assert.Equal(FinishReason.ToolCalls, first.FinishReason);
        Assert.Equal("thinking", first.ReasoningText);
        Assert.Equal("RESULT1", Assert.Single(first.ToolResults).OutputJson);
        Assert.Equal("id-0", first.Response.Id);
        Assert.Equal("1", first.Response.Headers!["call"]);
        Assert.Equal(2, first.Response.Messages.Count);
        var second = run.StepResults[1];
        Assert.Equal(1, second.StepNumber);
        Assert.Equal("Hello, world!", second.Text);
        Assert.Equal(FinishReason.Stop, second.FinishReason);
        Assert.Equal("id-1", second.Response.Id);
        Assert.Equal("2", second.Response.Headers!["call"]);
    }
}
