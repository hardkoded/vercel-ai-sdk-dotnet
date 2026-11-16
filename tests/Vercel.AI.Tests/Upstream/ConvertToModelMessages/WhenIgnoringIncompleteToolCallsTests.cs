// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages &gt; when ignoring incomplete tool calls</c>.</summary>
[Collection("LogWarnings")]
public sealed class WhenIgnoringIncompleteToolCallsTests : WarningsOff
{
    private const string Suite = Prefix + "convertToModelMessages > when ignoring incomplete tool calls::";

    [UpstreamTest(Suite + "should ignore preliminary tool outputs", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_ignore_preliminary_tool_outputs()
    {
        var toModelOutputCalls = 0;
        var streamingTool = new Tool("streamingTool", null, Json("{\"type\":\"object\",\"properties\":{\"task\":{\"type\":\"string\"}},\"required\":[\"task\"]}"), null)
        {
            ToModelOutput = call =>
            {
                toModelOutputCalls++;
                return new JsonToolModelOutput(Json((string)call.Output!));
            },
        };

        var result = Convert(
            new[]
            {
                Assistant(new ToolUIPart("streamingTool", "call-preliminary", ToolUIPartState.OutputAvailable)
                {
                    Input = Json("{\"task\":\"finish the work\"}"),
                    Output = Json("{\"complete\":false,\"progress\":\"half finished\"}"),
                    Preliminary = true,
                }),
                User(new TextUIPart("Continue.")),
            },
            new ConvertToModelMessagesOptions
            {
                IgnoreIncompleteToolCalls = true,
                Tools = new Dictionary<string, Tool> { ["streamingTool"] = streamingTool },
            });

        Assert.Equal(0, toModelOutputCalls);
        AssertEqual(
            """
            [
              {
                "role": "user",
                "content": [{ "type": "text", "text": "Continue." }]
              }
            ]
            """,
            result);
    }

    [UpstreamTest(
        Suite + "should preserve transformed approval input through a UI message round trip",
        Coverage = UpstreamCoverage.Partial,
        Note = "A JSON Schema has no transform, so the input schema of the tool is a string count and ExperimentalRefineToolInput turns it into a number, as upstream's z.string().transform(Number) does.")]
    [Fact]
    public async Task Should_preserve_transformed_approval_input_through_a_UI_message_round_trip()
    {
        var tools = new Dictionary<string, Tool>
        {
            ["count"] = new Tool(
                "count",
                null,
                Json("""{"type":"object","properties":{"count":{"type":"string"}},"required":["count"]}"""),
                null),
        };
        var assistantMessage = await RecordAssistantMessageFromChunks(
            """
            [
              { "type": "tool-input-available", "toolCallId": "count-call", "toolName": "count", "input": { "count": 3 } },
              { "type": "tool-approval-request", "approvalId": "approval-1", "toolCallId": "count-call", "inputSchemaInput": { "count": "3" } },
              { "type": "tool-approval-response", "approvalId": "approval-1", "approved": true }
            ]
            """);
        var persistedMessage = Reload(new[] { assistantMessage });

        var validatedMessages = await Ai.ValidateUIMessagesAsync(new ValidateUIMessagesOptions
        {
            Messages = persistedMessage,
            Tools = tools,
            ExperimentalRefineToolInput = new Dictionary<string, Func<JsonElement, Task<JsonElement>>>
            {
                ["count"] = input => Task.FromResult(Json("{\"count\":" + input.GetProperty("count").GetString() + "}")),
            },
        });
        var result = Convert(validatedMessages, new ConvertToModelMessagesOptions { Tools = tools });

        AssertEqual(
            """
            [
              {
                "role": "assistant",
                "content": [
                  { "type": "tool-call", "toolCallId": "count-call", "toolName": "count", "input": { "count": 3 } },
                  { "type": "tool-approval-request", "approvalId": "approval-1", "toolCallId": "count-call", "inputSchemaInput": { "count": "3" } }
                ]
              },
              {
                "role": "tool",
                "content": [
                  { "type": "tool-approval-response", "approvalId": "approval-1", "approved": true }
                ]
              }
            ]
            """,
            result);
    }
}
