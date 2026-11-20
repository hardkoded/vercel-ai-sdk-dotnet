// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;
using static Vercel.AI.Tests.Upstream.StreamText.StreamTextTransformTestSupport;

namespace Vercel.AI.Tests.Upstream.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText</c> &gt; <c>options.transform</c> &gt; <c>with base transformation</c>.</summary>
public sealed class StreamTextTransformBaseTransformationTests
{
    private const string Base =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.transform > with base transformation::";

    private const string ValueSchema =
        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    // Maps to upperCaseTransform.
    private static StreamTextTransform UpperCase =>
        _ =>
            Map(chunk =>
                chunk switch
                {
                    TextDeltaPart delta =>
                    [
                        new TextDeltaPart(
                            delta.Id,
                            delta.Text.ToUpperInvariant(),
                            delta.ProviderMetadata
                        ),
                    ],
                    ReasoningDeltaPart delta =>
                    [
                        new ReasoningDeltaPart(
                            delta.Id,
                            delta.Text.ToUpperInvariant(),
                            delta.ProviderMetadata
                        ),
                    ],
                    ToolInputDeltaPart delta =>
                    [
                        new ToolInputDeltaPart(
                            delta.Id,
                            delta.Delta.ToUpperInvariant(),
                            delta.ProviderMetadata
                        ),
                    ],
                    ToolCallPart { ToolCall.Dynamic: false } call =>
                    [
                        new ToolCallPart(
                            new GeneratedToolCall(
                                call.ToolCall.ToolCallId,
                                call.ToolCall.ToolName,
                                UpperValue(call.ToolCall.ArgumentsJson),
                                call.ToolCall.ProviderMetadata
                            )
                        ),
                    ],
                    ToolResultPart { Result.Dynamic: false } result
                        when result.Result.Preliminary != true =>
                    [
                        new ToolResultPart(
                            new ExecutedTool(
                                result.Result.ToolCallId,
                                result.Result.ToolName,
                                result.Result.OutputJson.ToUpperInvariant(),
                                false
                            )
                            {
                                InputJson = UpperValue(result.Result.InputJson ?? "{}"),
                            }
                        ),
                    ],
                    StepFinishPart finish =>
                    [
                        new StepFinishPart(
                            WithProviderMetadata(finish.Step, UpperProviderMetadata(finish.Step)),
                            finish.Response,
                            finish.RawFinishReason
                        ),
                    ],
                    _ => [chunk],
                }
            );

    private static string UpperValue(string json)
    {
        var value = JsonDocument.Parse(json).RootElement.GetProperty("value").GetString()!;
        return JsonSerializer.Serialize(new { value = value.ToUpperInvariant() });
    }

    private static JsonElement? UpperProviderMetadata(StepResult step) =>
        step.ProviderMetadata == null
            ? null
            : JsonSerializer.SerializeToElement(new { testProvider = new { testKey = "TEST VALUE" } });

    private static StepResult WithProviderMetadata(StepResult step, JsonElement? providerMetadata) =>
        new(
            step.Text,
            step.ReasoningText,
            step.ToolCalls,
            step.ToolResults,
            step.FinishReason,
            step.Usage,
            step.Sources,
            providerMetadata,
            step.Warnings,
            step.Files,
            step.Provider,
            step.ModelId,
            step.Content,
            step.Performance,
            step.Request,
            step.Response,
            step.RawFinishReason
        )
        {
            CallId = step.CallId,
            StepNumber = step.StepNumber,
        };

    private static Tool Tool1() =>
        Tool.Function(
            "tool1",
            null,
            ValueSchema,
            (input, _) => Task.FromResult(input.GetProperty("value").GetString()! + "-result")
        );

    private static readonly JsonElement TestMetadata = JsonSerializer.SerializeToElement(
        new { testProvider = new { testKey = "testValue" } }
    );

    private static CallsModel ToolModel(bool metadata = false) =>
        CallsModel.Single(
            [
                new ResponseMetadataStreamPart("id-0", "mock-model-id", DateTimeOffset.UnixEpoch),
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "Hello"),
                new TextDeltaStreamPart("1", ", "),
                new ToolCallStreamPart("call-1", "tool1", "{ \"value\": \"value\" }"),
                new TextDeltaStreamPart("1", "world!"),
                new TextEndStreamPart("1"),
                Finish(providerMetadata: metadata ? TestMetadata : null),
            ],
            headers: new Dictionary<string, string> { ["call"] = "2" }
        );

    private static StreamTextOptions Options(
        ILanguageModel model,
        params StreamTextTransform[] transforms
    ) =>
        new()
        {
            Model = model,
            Prompt = "test-input",
            ExperimentalTransform = transforms,
        };

    [Fact]
    [UpstreamTest(Base + "should transform the stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_transform_the_stream()
    {
        var result = SteeringTestSupport
            .Client()
            .StreamTextAsync(Options(CreateTestModel(), UpperCase));

        Assert.Equal(["HELLO", ", ", "WORLD!"], await TextStreamAsync(result));
    }

    [Fact]
    [UpstreamTest(Base + "result.text should be transformed", Coverage = UpstreamCoverage.Covered)]
    public async Task Result_text_should_be_transformed()
    {
        var result = SteeringTestSupport
            .Client()
            .StreamTextAsync(Options(CreateTestModel(), UpperCase));

        Assert.Equal("HELLO, WORLD!", await result.Text);
    }

    [Fact]
    [UpstreamTest(
        Base + "result.responseMessages should be transformed",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Result_responseMessages_should_be_transformed()
    {
        var result = SteeringTestSupport
            .Client()
            .StreamTextAsync(Options(CreateTestModel(), UpperCase));

        var response = await result.Response;

        Assert.NotEmpty(response.Id);
        Assert.NotEmpty(response.ModelId);
        var message = Assert.IsType<AssistantModelMessage>(Assert.Single(response.Messages));
        var text = Assert.IsType<AssistantTextPart>(Assert.Single(message.Parts));
        Assert.Equal("HELLO, WORLD!", text.Text);
    }

    [Fact]
    [UpstreamTest(
        Base + "result.totalUsage should be transformed",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Result_totalUsage_should_be_transformed()
    {
        var usage = new LanguageModelUsage(200, 300, null);
        StreamTextTransform transform = _ =>
            Map(chunk => chunk is FinishPart finish ? [new FinishPart(finish.FinishReason, usage)] : [chunk]);
        var result = SteeringTestSupport
            .Client()
            .StreamTextAsync(
                Options(
                    CallsModel.Single(
                        [
                            new TextStartStreamPart("1"),
                            new TextDeltaStreamPart("1", "Hello"),
                            new TextEndStreamPart("1"),
                            Finish(),
                        ]
                    ),
                    transform
                )
            );

        var totalUsage = await result.Usage;

        Assert.Equal(200, totalUsage.InputTokens);
        Assert.Equal(300, totalUsage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(
        Base + "result.finishReason should be transformed",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Result_finishReason_should_be_transformed()
    {
        StreamTextTransform transform = _ =>
            Map(chunk => chunk is FinishPart finish ? [new FinishPart(FinishReason.Stop, finish.Usage)] : [chunk]);
        var result = SteeringTestSupport
            .Client()
            .StreamTextAsync(
                Options(
                    CallsModel.Single(
                        [
                            new TextStartStreamPart("1"),
                            new TextDeltaStreamPart("1", "Hello"),
                            new TextEndStreamPart("1"),
                            Finish(FinishReason.Length, "raw-length"),
                        ]
                    ),
                    transform
                )
            );

        Assert.Equal(FinishReason.Stop, await result.FinishReason);
    }

    [Fact]
    [UpstreamTest(
        Base + "result.toolCalls should be transformed",
        Coverage = UpstreamCoverage.Partial,
        Note = "StreamTextResult has no ToolCalls property, so it asserts the tool calls of the final step."
    )]
    public async Task Result_toolCalls_should_be_transformed()
    {
        var result = SteeringTestSupport
            .Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = CallsModel.Single(
                        [
                            new TextStartStreamPart("1"),
                            new TextDeltaStreamPart("1", "Hello, "),
                            new TextDeltaStreamPart("1", "world!"),
                            new TextEndStreamPart("1"),
                            new ToolCallStreamPart("call-1", "tool1", "{ \"value\": \"value\" }"),
                            Finish(),
                        ]
                    ),
                    Tools = [Tool1()],
                    ExperimentalTransform = [UpperCase],
                    Prompt = "test-input",
                }
            );

        var call = Assert.Single((await result.Steps).Last().ToolCalls);

        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("tool1", call.ToolName);
        Assert.Equal("VALUE", JsonDocument.Parse(call.ArgumentsJson).RootElement.GetProperty("value").GetString());
    }

    [Fact]
    [UpstreamTest(
        Base + "result.toolResults should be transformed",
        Coverage = UpstreamCoverage.Partial,
        Note = "StreamTextResult has no ToolResults property, so it asserts the tool results of the final step."
    )]
    public async Task Result_toolResults_should_be_transformed()
    {
        var result = SteeringTestSupport
            .Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = CallsModel.Single(
                        [
                            new TextStartStreamPart("1"),
                            new TextDeltaStreamPart("1", "Hello, "),
                            new TextDeltaStreamPart("1", "world!"),
                            new TextEndStreamPart("1"),
                            new ToolCallStreamPart("call-1", "tool1", "{ \"value\": \"value\" }"),
                            Finish(),
                        ]
                    ),
                    Tools =
                    [
                        Tool.Function("tool1", null, ValueSchema, (_, _) => Task.FromResult("result1")),
                    ],
                    ExperimentalTransform = [UpperCase],
                    Prompt = "test-input",
                }
            );

        var executed = Assert.Single((await result.Steps).Last().ToolResults);

        Assert.False(executed.Dynamic);
        Assert.Equal("call-1", executed.ToolCallId);
        Assert.Equal("tool1", executed.ToolName);
        Assert.Equal("RESULT1", executed.OutputJson);
        Assert.Equal(
            "VALUE",
            JsonDocument.Parse(executed.InputJson!).RootElement.GetProperty("value").GetString()
        );
    }

    [Fact]
    [UpstreamTest(
        Base + "result.steps should be transformed",
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts the content, text, tool calls and results, finish reason, response and messages of the step, not the performance numbers."
    )]
    public async Task Result_steps_should_be_transformed()
    {
        var result = SteeringTestSupport
            .Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = CallsModel.Single(
                        [
                            new ResponseMetadataStreamPart(
                                "id-0",
                                "mock-model-id",
                                DateTimeOffset.UnixEpoch
                            ),
                            new TextStartStreamPart("1"),
                            new TextDeltaStreamPart("1", "Hello, "),
                            new TextDeltaStreamPart("1", "world!"),
                            new TextEndStreamPart("1"),
                            new ToolCallStreamPart("call-1", "tool1", "{ \"value\": \"value\" }"),
                            Finish(),
                        ]
                    ),
                    Tools =
                    [
                        Tool.Function("tool1", null, ValueSchema, (_, _) => Task.FromResult("result1")),
                    ],
                    ExperimentalTransform = [UpperCase],
                    Prompt = "test-input",
                    GenerateCallId = () => "test-telemetry-call-id",
                }
            );

        var step = Assert.Single(await result.Steps);

        Assert.Equal("test-telemetry-call-id", step.CallId);
        Assert.Equal(0, step.StepNumber);
        Assert.Equal("HELLO, WORLD!", step.Text);
        Assert.Equal(
            ["text", "tool-call", "tool-result"],
            step.Content.Select(part => part.Type)
        );
        Assert.Equal("HELLO, WORLD!", Assert.IsType<GeneratedText>(step.Content[0]).Text);
        Assert.Equal(
            "VALUE",
            JsonDocument
                .Parse(Assert.IsType<GeneratedToolCall>(step.Content[1]).ArgumentsJson)
                .RootElement.GetProperty("value")
                .GetString()
        );
        Assert.Equal("RESULT1", Assert.IsType<GeneratedToolResult>(step.Content[2]).ResultJson);
        Assert.Equal(FinishReason.Stop, step.FinishReason);
        Assert.Equal("stop", step.RawFinishReason);
        Assert.Equal("mock-provider", step.Provider);
        Assert.Equal("mock-model-id", step.ModelId);
        Assert.Equal(3, step.Usage.InputTokens);
        Assert.Equal(10, step.Usage.OutputTokens);
        Assert.Equal("id-0", step.Response.Id);
        Assert.Equal(DateTimeOffset.UnixEpoch, step.Response.Timestamp);
        Assert.Equal(2, step.Response.Messages.Count);
        var assistant = Assert.IsType<AssistantModelMessage>(step.Response.Messages[0]);
        Assert.Equal("HELLO, WORLD!", Assert.IsType<AssistantTextPart>(assistant.Parts[0]).Text);
        Assert.IsType<AssistantToolCallPart>(assistant.Parts[1]);
        Assert.IsType<ToolModelMessage>(step.Response.Messages[1]);
    }

    [Fact]
    [UpstreamTest(
        Base + "result.providerMetadata should be transformed",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Result_providerMetadata_should_be_transformed()
    {
        var result = SteeringTestSupport
            .Client()
            .StreamTextAsync(
                Options(
                    CallsModel.Single(
                        [
                            new ResponseMetadataStreamPart(
                                "id-0",
                                "mock-model-id",
                                DateTimeOffset.UnixEpoch
                            ),
                            new TextStartStreamPart("1"),
                            new TextDeltaStreamPart("1", "Hello"),
                            new TextEndStreamPart("1"),
                            Finish(providerMetadata: TestMetadata),
                        ],
                        requestBody: "test body"
                    ),
                    UpperCase
                )
            );

        Assert.Equal(
            "{\"testProvider\":{\"testKey\":\"TEST VALUE\"}}",
            (await result.ProviderMetadata)!.Value.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Base + "options.onFinish should receive transformed data",
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts the fields of the end event as the port exposes them, not the full snapshot (no finalStep, files or tool call splits on the end result)."
    )]
    public async Task Options_onFinish_should_receive_transformed_data()
    {
        GenerateTextResult? received = null;
        var result = SteeringTestSupport
            .Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = ToolModel(metadata: true),
                    Tools = [Tool1()],
                    Prompt = "test-input",
                    OnEnd = (end, _) =>
                    {
                        received = end;
                        return Task.CompletedTask;
                    },
                    ExperimentalTransform = [UpperCase],
                }
            );

        await result.ConsumeStreamAsync();

        Assert.NotNull(received);
        Assert.Equal("HELLO, WORLD!", received!.Text);
        Assert.Equal(FinishReason.Stop, received.FinishReason);
        var step = Assert.Single(received.Steps);
        Assert.Equal(
            ["text", "tool-call", "tool-result"],
            step.Content.Select(part => part.Type)
        );
        Assert.Equal("VALUE-RESULT", Assert.IsType<GeneratedToolResult>(step.Content[2]).ResultJson);
        Assert.Equal("VALUE-RESULT", Assert.Single(step.ToolResults).OutputJson);
        Assert.Equal(
            "VALUE",
            JsonDocument
                .Parse(Assert.Single(step.ToolCalls).ArgumentsJson)
                .RootElement.GetProperty("value")
                .GetString()
        );
        Assert.Equal(
            "{\"testProvider\":{\"testKey\":\"TEST VALUE\"}}",
            received.ProviderMetadata!.Value.GetRawText()
        );
        Assert.Equal(2, received.ResponseMessages.Count);
        Assert.Equal(3, received.Usage.InputTokens);
        Assert.Equal("call-1", Assert.Single(received.Steps.SelectMany(s => s.ToolCalls)).ToolCallId);
    }

    [Fact]
    [UpstreamTest(
        Base + "options.onStepFinish should receive transformed data",
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts the fields of the step as the port exposes them, not the full snapshot."
    )]
    public async Task Options_onStepFinish_should_receive_transformed_data()
    {
        StepResult? received = null;
        var result = SteeringTestSupport
            .Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = ToolModel(metadata: true),
                    Tools = [Tool1()],
                    Prompt = "test-input",
                    OnStepEnd = (step, _) =>
                    {
                        received = step;
                        return Task.CompletedTask;
                    },
                    ExperimentalTransform = [UpperCase],
                }
            );

        await result.ConsumeStreamAsync();

        Assert.NotNull(received);
        Assert.Equal("HELLO, WORLD!", received!.Text);
        Assert.Equal("VALUE-RESULT", Assert.Single(received.ToolResults).OutputJson);
        Assert.Equal(
            "{\"testProvider\":{\"testKey\":\"TEST VALUE\"}}",
            received.ProviderMetadata!.Value.GetRawText()
        );
        Assert.Equal("id-0", received.Response.Id);
        Assert.Equal(
            "call",
            Assert.Single(received.Response.Headers ?? new Dictionary<string, string>()).Key
        );
    }

    [Fact]
    [UpstreamTest(
        Base + "it should send transformed chunks to onChunk",
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts the type and the text, tool input and tool result values of each chunk, not the performance numbers of finish-step."
    )]
    public async Task It_should_send_transformed_chunks_to_onChunk()
    {
        var chunks = new List<TextStreamPart>();
        var result = SteeringTestSupport
            .Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = CallsModel.Single(
                        [
                            new TextStartStreamPart("1"),
                            new TextDeltaStreamPart("1", "Hello"),
                            new ReasoningStartStreamPart("2"),
                            new ReasoningDeltaStreamPart("2", "Feeling clever"),
                            new ReasoningEndStreamPart("2"),
                            new ToolInputStartStreamPart("call-1", "tool1"),
                            new ToolInputDeltaStreamPart("call-1", "{\"value\": \""),
                            new ToolInputDeltaStreamPart("call-1", "test"),
                            new ToolInputDeltaStreamPart("call-1", "\"}"),
                            new ToolInputEndStreamPart("call-1"),
                            new ToolCallStreamPart("call-1", "tool1", "{ \"value\": \"test\" }"),
                            new TextDeltaStreamPart("1", " World"),
                            new TextEndStreamPart("1"),
                            Finish(),
                        ]
                    ),
                    Tools = [Tool1()],
                    Prompt = "test-input",
                    OnChunk = (chunk, _) =>
                    {
                        chunks.Add(chunk.Chunk);
                        return Task.CompletedTask;
                    },
                    ExperimentalTransform = [UpperCase],
                }
            );

        await result.ConsumeStreamAsync();

        Assert.Equal(
            [
                "start",
                "start-step",
                "text-start",
                "text-delta",
                "reasoning-start",
                "reasoning-delta",
                "reasoning-end",
                "tool-input-start",
                "tool-input-delta",
                "tool-input-delta",
                "tool-input-delta",
                "tool-input-end",
                "tool-call",
                "text-delta",
                "text-end",
                "tool-result",
                "finish-step",
                "finish",
            ],
            chunks.Select(chunk => chunk.Type)
        );
        Assert.Equal("HELLO", Assert.IsType<TextDeltaPart>(chunks[3]).Text);
        Assert.Equal("FEELING CLEVER", Assert.IsType<ReasoningDeltaPart>(chunks[5]).Text);
        Assert.Equal(
            ["{\"VALUE\": \"", "TEST", "\"}"],
            chunks.OfType<ToolInputDeltaPart>().Select(part => part.Delta)
        );
        Assert.Equal(
            "TEST",
            JsonDocument
                .Parse(chunks.OfType<ToolCallPart>().Single().ToolCall.ArgumentsJson)
                .RootElement.GetProperty("value")
                .GetString()
        );
        Assert.Equal(" WORLD", Assert.IsType<TextDeltaPart>(chunks[13]).Text);
        Assert.Equal("TEST-RESULT", chunks.OfType<ToolResultPart>().Single().Result.OutputJson);
    }
}
