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
    private const string Suite =
        Prefix + "convertToModelMessages > when ignoring incomplete tool calls::";

    [UpstreamTest(
        Suite + "should ignore preliminary tool outputs",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_ignore_preliminary_tool_outputs()
    {
        var toModelOutputCalls = 0;
        var streamingTool = new Tool(
            "streamingTool",
            null,
            Json(
                "{\"type\":\"object\",\"properties\":{\"task\":{\"type\":\"string\"}},\"required\":[\"task\"]}"
            ),
            null
        )
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
                Assistant(
                    new ToolUIPart(
                        "streamingTool",
                        "call-preliminary",
                        ToolUIPartState.OutputAvailable
                    )
                    {
                        Input = Json("{\"task\":\"finish the work\"}"),
                        Output = Json("{\"complete\":false,\"progress\":\"half finished\"}"),
                        Preliminary = true,
                    }
                ),
                User(new TextUIPart("Continue.")),
            },
            new ConvertToModelMessagesOptions
            {
                IgnoreIncompleteToolCalls = true,
                Tools = new Dictionary<string, Tool> { ["streamingTool"] = streamingTool },
            }
        );

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
            result
        );
    }

    [UpstreamTest(
        Suite + "should preserve transformed approval input through a UI message round trip"
    )]
    [Fact]
    public async Task Should_preserve_transformed_approval_input_through_a_UI_message_round_trip()
    {
        var tools = new Dictionary<string, Tool>
        {
            ["count"] = new Tool(
                "count",
                null,
                Json(
                    """{"type":"object","properties":{"count":{"type":"string"}},"required":["count"]}"""
                ),
                null
            )
            {
                InputTransform = input =>
                    Json("{\"count\":" + input.GetProperty("count").GetString() + "}"),
            },
        };
        var assistantMessage = await RecordAssistantMessageFromChunks(
            """
            [
              { "type": "tool-input-available", "toolCallId": "count-call", "toolName": "count", "input": { "count": 3 } },
              { "type": "tool-approval-request", "approvalId": "approval-1", "toolCallId": "count-call", "inputSchemaInput": { "count": "3" } },
              { "type": "tool-approval-response", "approvalId": "approval-1", "approved": true }
            ]
            """
        );
        var persistedMessage = Reload(new[] { assistantMessage });

        var validatedMessages = await Ai.ValidateUIMessagesAsync(
            new ValidateUIMessagesOptions { Messages = persistedMessage, Tools = tools }
        );
        var result = Convert(
            validatedMessages,
            new ConvertToModelMessagesOptions { Tools = tools }
        );

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
            result
        );
    }

    [UpstreamTest(
        Suite + "should ignore tool calls that are awaiting approval or have no state",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_ignore_tool_calls_that_are_awaiting_approval_or_have_no_state()
    {
        var result = Convert(
            UiMessages(
                """
                [
                  {
                    "role": "assistant",
                    "parts": [
                      { "type": "text", "text": "Waiting for completed tool calls.", "state": "done" },
                      { "type": "tool-weather", "state": "approval-requested", "toolCallId": "call-awaiting-approval", "input": { "city": "Tokyo" }, "approval": { "id": "approval-1" } },
                      { "type": "tool-weather", "toolCallId": "call-without-state", "input": { "city": "Berlin" } }
                    ]
                  },
                  { "role": "user", "parts": [{ "type": "text", "text": "Continue." }] }
                ]
                """
            ),
            new ConvertToModelMessagesOptions { IgnoreIncompleteToolCalls = true }
        );

        AssertEqual(
            """
            [
              { "role": "assistant", "content": [{ "type": "text", "text": "Waiting for completed tool calls." }] },
              { "role": "user", "content": [{ "type": "text", "text": "Continue." }] }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should preserve tool calls with approval responses",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_preserve_tool_calls_with_approval_responses()
    {
        var result = Convert(
            UiMessages(
                """
                [
                  {
                    "role": "assistant",
                    "parts": [
                      { "type": "tool-weather", "state": "approval-responded", "toolCallId": "call-approved", "input": { "city": "Tokyo" }, "approval": { "id": "approval-1", "approved": true } }
                    ]
                  }
                ]
                """
            ),
            new ConvertToModelMessagesOptions { IgnoreIncompleteToolCalls = true }
        );

        AssertEqual(
            """
            [
              {
                "role": "assistant",
                "content": [
                  { "type": "tool-call", "toolCallId": "call-approved", "toolName": "weather", "input": { "city": "Tokyo" } },
                  { "type": "tool-approval-request", "approvalId": "approval-1", "toolCallId": "call-approved" }
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
            result
        );
    }

    [UpstreamTest(
        Suite
            + "should handle conversation with multiple tool invocations and user message at the end",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_handle_conversation_with_multiple_tool_invocations_and_user_message_at_the_end()
    {
        var result = Convert(
            UiMessages(
                """
                [
                  {
                    "role": "assistant",
                    "parts": [
                      { "type": "step-start" },
                      { "type": "tool-screenshot", "state": "output-available", "toolCallId": "call-1", "input": { "value": "value-1" }, "output": "result-1" },
                      { "type": "step-start" },
                      { "type": "tool-screenshot", "state": "input-streaming", "toolCallId": "call-2", "input": { "value": "value-2" } },
                      { "type": "tool-screenshot", "state": "input-available", "toolCallId": "call-3", "input": { "value": "value-3" } },
                      { "type": "dynamic-tool", "toolName": "tool-screenshot2", "state": "input-available", "toolCallId": "call-3", "input": { "value": "value-3" } },
                      { "type": "text", "text": "response", "state": "done" }
                    ]
                  },
                  { "role": "user", "parts": [{ "type": "text", "text": "Thanks!" }] }
                ]
                """
            ),
            new ConvertToModelMessagesOptions { IgnoreIncompleteToolCalls = true }
        );

        AssertEqual(
            """
            [
              { "content": [{ "input": { "value": "value-1" }, "toolCallId": "call-1", "toolName": "screenshot", "type": "tool-call" }], "role": "assistant" },
              { "content": [{ "output": { "type": "text", "value": "result-1" }, "toolCallId": "call-1", "toolName": "screenshot", "type": "tool-result" }], "role": "tool" },
              { "content": [{ "text": "response", "type": "text" }], "role": "assistant" },
              { "content": [{ "text": "Thanks!", "type": "text" }], "role": "user" }
            ]
            """,
            result
        );
    }
}
