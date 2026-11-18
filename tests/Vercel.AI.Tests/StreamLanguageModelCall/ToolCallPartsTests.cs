// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.StreamLanguageModelCall.StreamLanguageModelCallFixture;

namespace Vercel.AI.Tests.StreamLanguageModelCall;

/// <summary>Port of <c>stream-language-model-call.test.ts</c> &gt; <c>streamLanguageModelCall &gt; tool-call parts</c>.</summary>
public sealed class ToolCallPartsTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-language-model-call.test.ts::streamLanguageModelCall > tool-call parts::";

    [Fact]
    [UpstreamTest(
        Prefix
            + "should not synthesize a client tool error for an invalid provider-executed tool call",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_synthesize_a_client_tool_error_for_an_invalid_provider_executed_tool_call()
    {
        var result = await StreamLanguageModelCallResult(
            [
                new ToolCallStreamPart("call-1", "web_search", "{}") { ProviderExecuted = true },
                new ToolResultStreamPart(
                    "call-1",
                    "web_search",
                    Json(
                        "{\"type\":\"web_search_tool_result_error\",\"errorCode\":\"invalid_tool_input\"}"
                    )
                )
                {
                    IsError = true,
                },
                Finish(FinishReason.ToolCalls, "tool_use"),
            ],
            tools:
            [
                Tool.ProviderDefined(
                    "test.web_search",
                    "web_search",
                    Json("{}"),
                    Json(
                        "{\"type\":\"object\",\"properties\":{\"query\":{\"type\":\"string\"}},\"required\":[\"query\"]}"
                    )
                ),
            ]
        );

        Assert.Equal(3, result.Count);
        var call = Assert.IsType<ToolCallPart>(result[0]).ToolCall;
        Assert.True(call.Dynamic);
        Assert.NotNull(call.Error);
        Assert.True(InvalidToolInputError.IsInstance(call.Error));
        Assert.Equal("{}", call.ArgumentsJson);
        Assert.True(call.Invalid);
        Assert.True(call.ProviderExecuted);
        Assert.Null(call.ProviderMetadata);
        Assert.Null(call.Title);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("web_search", call.ToolName);
        var error = Assert.IsType<ToolErrorPart>(result[1]).Result;
        Assert.Equal(
            "{\"type\":\"web_search_tool_result_error\",\"errorCode\":\"invalid_tool_input\"}",
            error.OutputJson
        );
        Assert.Equal("{}", error.InputJson);
        Assert.True(error.ProviderExecuted);
        Assert.Equal("call-1", error.ToolCallId);
        Assert.Equal("web_search", error.ToolName);
        var end = ModelCallEnd(result[2]);
        Assert.Equal(FinishReason.ToolCalls, end.FinishReason);
        Assert.Equal(0, end.Performance.ResponseTimeMs);
        Assert.Equal(0, end.Performance.TimeToFirstOutputMs);
        Assert.Null(end.Performance.TimeBetweenOutputChunksMs);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should measure time to first output from tool-call parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_measure_time_to_first_output_from_tool_call_parts()
    {
        var result = await StreamLanguageModelCallResult(
            [
                new ToolCallStreamPart("call-1", "testTool", "{ \"value\": \"test\" }"),
                Finish(FinishReason.ToolCalls, "tool-calls"),
            ],
            tools: [TestTool()],
            now: Now(1000, 1250, 1600)
        );

        AssertTimeToFirstOutput(ModelCallEnd(result[^1]));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should refine tool call input before emitting tool-call parts and model-call end content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_refine_tool_call_input_before_emitting_tool_call_parts_and_model_call_end_content()
    {
        var endEvents = new List<LanguageModelCallEndEvent>();

        var stream = await LanguageModelCallStreaming.StreamLanguageModelCall(
            new StreamLanguageModelCallOptions
            {
                Model = Model(
                    new ToolCallStreamPart("call-1", "testTool", "{ \"value\": \" raw \" }"),
                    Finish(FinishReason.ToolCalls, null)
                ),
                Tools = [TestTool()],
                Prompt = "test prompt",
                NowMs = () => 0,
                RefineToolInput = new Dictionary<
                    string,
                    Func<JsonElement, CancellationToken, Task<JsonElement>>
                >
                {
                    ["testTool"] = (input, _) =>
                        Task.FromResult(
                            Json(
                                "{\"value\":"
                                    + JsonSerializer.Serialize(
                                        input.GetProperty("value").GetString()!.Trim()
                                    )
                                    + "}"
                            )
                        ),
                },
                OnLanguageModelCallEnd = (@event, _) =>
                {
                    endEvents.Add(@event);
                    return Task.CompletedTask;
                },
            }
        );

        var result = await Collect(stream);

        var call = Assert.IsType<ToolCallPart>(result[0]).ToolCall;
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("testTool", call.ToolName);
        Assert.Equal("{\"value\":\"raw\"}", call.ArgumentsJson);
        var content = Assert.IsType<GeneratedToolCall>(endEvents[0].Content[0]);
        Assert.Equal("call-1", content.ToolCallId);
        Assert.Equal("testTool", content.ToolName);
        Assert.Equal("{\"value\":\"raw\"}", content.ArgumentsJson);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should try to repair tool call when the tool name is not found",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_try_to_repair_tool_call_when_the_tool_name_is_not_found()
    {
        var repairs = 0;

        var result = await StreamLanguageModelCallResult(
            [new ToolCallStreamPart("call-1", "unknownTool", "{ \"value\": \"test\" }"), Finish()],
            tools:
            [
                Tool.Function(
                    "correctTool",
                    null,
                    "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}",
                    (_, _) => Task.FromResult("\"result\"")
                ),
            ],
            repairToolCall: context =>
            {
                repairs++;
                Assert.True(NoSuchToolError.IsInstance(context.Error));
                Assert.Equal("call-1", context.ToolCall.ToolCallId);
                Assert.Equal("unknownTool", context.ToolCall.ToolName);
                Assert.Equal("{ \"value\": \"test\" }", context.ToolCall.ArgumentsJson);

                return Task.FromResult<GeneratedToolCall?>(
                    new GeneratedToolCall("call-1", "correctTool", context.ToolCall.ArgumentsJson)
                );
            }
        );

        Assert.Equal(1, repairs);
        Assert.Equal(2, result.Count);
        var call = Assert.IsType<ToolCallPart>(result[0]).ToolCall;
        Assert.True(
            System.Text.Json.Nodes.JsonNode.DeepEquals(
                System.Text.Json.Nodes.JsonNode.Parse("{\"value\":\"test\"}"),
                System.Text.Json.Nodes.JsonNode.Parse(call.ArgumentsJson)
            )
        );
        Assert.False(call.ProviderExecuted);
        Assert.Null(call.ProviderMetadata);
        Assert.Null(call.Title);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("correctTool", call.ToolName);
        var end = ModelCallEnd(result[1]);
        Assert.Equal(FinishReason.Stop, end.FinishReason);
        Assert.Equal(0, end.Performance.ResponseTimeMs);
        Assert.Equal(0, end.Performance.TimeToFirstOutputMs);
        Assert.Null(end.Performance.TimeBetweenOutputChunksMs);
    }
}
