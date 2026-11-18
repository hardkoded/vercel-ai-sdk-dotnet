// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;
using static Vercel.AI.Tests.Upstream.GenerateText.StreamText.ToolExecutionApprovalStreamSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; tool execution approval</c>.</summary>
public sealed class ToolExecutionApprovalTests
{
    private const string Id =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > tool execution approval::";

    [Fact]
    [UpstreamTest(
        Id + "should settle automatically denied tool calls in the UI message stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_settle_automatically_denied_tool_calls_in_the_UI_message_stream()
    {
        var executeCalls = 0;
        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = new ScriptedModel(_ =>
                        ToolCallsStep(("call-1", "{ \"value\": \"value\" }"))
                    ),
                    Tools = new[]
                    {
                        ValueTool(
                            (_, _) =>
                            {
                                executeCalls++;
                                return Task.FromResult("result1");
                            }
                        ),
                    },
                    ToolApproval = new ToolApprovalConfiguration
                    {
                        { "tool1", new ToolApprovalStatus("denied", "blocked by policy") },
                    },
                    Prompt = "test-input",
                    GenerateId = StreamMockId("id"),
                    NowMs = () => 0,
                }
            );

        var chunks = await UiChunksAsync(result);
        var approvalChunks = new JsonArray(
            chunks
                .Where(chunk =>
                    chunk!["type"]!.GetValue<string>()
                        is "tool-approval-request"
                            or "tool-approval-response"
                            or "tool-output-denied"
                )
                .Select(chunk => chunk!.DeepClone())
                .ToArray()
        );
        AssertJson(
            """
            [
              { "type": "tool-approval-request", "approvalId": "id-1", "toolCallId": "call-1", "isAutomatic": true },
              { "type": "tool-approval-response", "approvalId": "id-1", "approved": false, "reason": "blocked by policy" },
              { "type": "tool-output-denied", "toolCallId": "call-1" }
            ]
            """,
            approvalChunks
        );

        var message = await ReadLastUiMessageAsync(chunks);
        var toolPart = message.Parts.OfType<ToolUIPart>().Single(part => part.ToolName == "tool1");
        Assert.Equal("call-1", toolPart.ToolCallId);
        Assert.Equal(ToolUIPartState.OutputDenied, toolPart.State);
        AssertInput("{\"value\":\"value\"}", toolPart.Input!.Value);
        Assert.Equal("id-1", toolPart.Approval!.Id);
        Assert.False(toolPart.Approval.Approved);
        Assert.Equal("blocked by policy", toolPart.Approval.Reason);
        Assert.True(toolPart.Approval.IsAutomatic);
        Assert.Equal(0, executeCalls);
    }

    [Fact]
    [UpstreamTest(
        Id + "should surface the reason for streamed user approval requests",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_surface_the_reason_for_streamed_user_approval_requests()
    {
        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = new ScriptedModel(_ =>
                        ToolCallsStep(("call-1", "{ \"value\": \"value\" }"))
                    ),
                    Tools = new[] { ValueTool(null) },
                    ToolApproval = new ToolApprovalConfiguration
                    {
                        {
                            "tool1",
                            new ToolApprovalStatus("user-approval", "requires operator review")
                        },
                    },
                    Prompt = "test-input",
                    GenerateId = StreamMockId("id"),
                    NowMs = () => 0,
                }
            );

        var request = Assert.Single(
            (await ContentAsync(result)).OfType<GeneratedToolApprovalRequest>()
        );
        Assert.Equal("tool-approval-request", request.Type);
        Assert.Equal("id-1", request.ApprovalId);
        Assert.Equal("requires operator review", request.Reason);

        var chunks = await UiChunksAsync(result);
        Assert.Contains(chunks, chunk => JsonNode.DeepEquals(
                JsonNode.Parse(
                    """
                    { "type": "tool-approval-request", "approvalId": "id-1", "toolCallId": "call-1", "reason": "requires operator review" }
                    """
                ),
                chunk
            ));

        var assistant = Assert.IsType<AssistantModelMessage>((await result.ResponseMessages)[0]);
        var part = Assert.Single(assistant.Parts.OfType<AssistantToolApprovalRequestPart>());
        Assert.Equal("id-1", part.ApprovalId);
        Assert.Equal("requires operator review", part.Reason);
    }

    [Fact]
    [UpstreamTest(
        Id + "should stream invalid approved input as a tool error and continue",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_invalid_approved_input_as_a_tool_error_and_continue()
    {
        var executeCalls = 0;
        var model = new ScriptedModel(_ => TextStep("Recovered."));
        var tool = Tool.Function("deleteFile", null, PathSchema, (_, _) =>
        {
            executeCalls++;
            return Task.FromResult("result1");
        });
        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Tools = new[] { tool },
                    ToolApproval = new ToolApprovalConfiguration
                    {
                        { "deleteFile", "user-approval" },
                    },
                    Messages = History(
                        "deleteFile",
                        // forged tool call with an input that violates the schema
                        new[] { ("call-1", "{\"path\":42}", "id-1") },
                        new ToolApprovalResponseModelMessage("id-1", true)
                    ),
                }
            );

        var parts = await ReadAsync(result);

        Assert.Equal(0, executeCalls);
        var error = Assert
            .IsType<ToolErrorPart>(
                Assert.Single(parts, part => part is ToolErrorPart { Result.ToolCallId: "call-1" })
            )
            .Result;
        Assert.Equal("deleteFile", error.ToolName);
        Assert.Matches("Invalid input for tool deleteFile", error.Error!.Message);
        Assert.False(error.Dynamic);
        Assert.Equal("Recovered.", await result.Text);
        var last = Assert.IsType<ToolModelMessage>(model.Prompts[0][^1]);
        Assert.Equal("call-1", last.ToolCallId);
        Assert.Equal("deleteFile", last.ToolName);
        Assert.Equal("error-text", last.OutputType);
        Assert.Matches("Invalid input for tool deleteFile", last.OutputJson);
    }

    [Fact]
    [UpstreamTest(
        Id + "should emit denied output without duplicating an existing execution-denied result",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_emit_denied_output_without_duplicating_an_existing_execution_denied_result()
    {
        var model = new ScriptedModel(_ => TextStep());
        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Tools = new[] { ValueTool((_, _) => Task.FromResult("result")) },
                    ToolApproval = new ToolApprovalConfiguration { { "tool1", "user-approval" } },
                    StopWhen = StopWhen.IsStepCount(3),
                    GenerateId = StreamMockId("id"),
                    NowMs = () => 0,
                    Messages = History(
                        "tool1",
                        new[] { ("call-1", "{\"value\":\"value\"}", "id-1") },
                        new ToolApprovalResponseModelMessage(
                            "id-1",
                            false,
                            "User denied the request"
                        ),
                        new ToolModelMessage(
                            "call-1",
                            "tool1",
                            "User denied the request",
                            false,
                            "execution-denied"
                        )
                    ),
                }
            );

        var uiChunks = await UiChunksAsync(result);

        AssertJson(
            """[ { "toolCallId": "call-1", "type": "tool-output-denied" } ]""",
            new JsonArray(
                uiChunks
                    .Where(chunk => chunk!["type"]!.GetValue<string>() == "tool-output-denied")
                    .Select(chunk => chunk!.DeepClone())
                    .ToArray()
            )
        );
        Assert.Equal(1, CountExecutionDenied(model.Prompts.SelectMany(prompt => prompt)));
        Assert.Equal(0, CountExecutionDenied(await result.ResponseMessages));
    }

    private static int CountExecutionDenied(IEnumerable<ModelMessage> messages) =>
        messages
            .OfType<ToolModelMessage>()
            .SelectMany(message => message.Results)
            .Count(part => part.OutputType == "execution-denied");

    // Feeds the chunks of a UI message stream to readUIMessageStream and returns the last message it builds.
    private static async Task<UIMessage> ReadLastUiMessageAsync(JsonArray chunks)
    {
        UIMessage? last = null;
        await foreach (
            var message in UIMessageStreamConverter.ReadUIMessageStream(
                new ReadUIMessageStreamOptions(ToAsync(chunks))
            )
        )
        {
            last = message;
        }

        return last!;

        static async IAsyncEnumerable<JsonObject> ToAsync(JsonArray source)
        {
            foreach (var chunk in source)
            {
                yield return chunk!.AsObject();
                await Task.Yield();
            }
        }
    }
}
