// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Ui;
using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages &gt; multiple messages</c>.</summary>
[Collection("LogWarnings")]
public sealed class MultipleMessagesTests : WarningsOff
{
    private const string Suite = Prefix + "convertToModelMessages > multiple messages::";

    [UpstreamTest(
        Suite + "should handle a conversation with multiple messages",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_handle_a_conversation_with_multiple_messages()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [
                  { "role": "user", "parts": [{ "type": "text", "text": "What's the weather like?" }] },
                  { "role": "assistant", "parts": [{ "type": "text", "text": "I'll check that for you.", "state": "done" }] },
                  { "role": "user", "parts": [{ "type": "text", "text": "Thanks!" }] }
                ]
                """
            )
        );

        AssertEqual(
            """
            [
              { "content": [{ "text": "What's the weather like?", "type": "text" }], "role": "user" },
              { "content": [{ "text": "I'll check that for you.", "type": "text" }], "role": "assistant" },
              { "content": [{ "text": "Thanks!", "type": "text" }], "role": "user" }
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
            UIMessagesFromJson(
                """
                [
                  {
                    "role": "assistant",
                    "parts": [
                      { "type": "step-start" },
                      { "type": "tool-screenshot", "state": "output-available", "toolCallId": "call-1", "input": { "value": "value-1" }, "output": "result-1" },
                      { "type": "step-start" },
                      { "type": "tool-screenshot", "state": "output-available", "toolCallId": "call-2", "input": { "value": "value-2" }, "output": "result-2" },
                      { "type": "tool-screenshot", "state": "output-available", "toolCallId": "call-3", "input": { "value": "value-3" }, "output": "result-3" },
                      { "type": "step-start" },
                      { "type": "tool-screenshot", "state": "output-available", "toolCallId": "call-4", "input": { "value": "value-4" }, "output": "result-4" },
                      { "type": "step-start" },
                      { "type": "text", "text": "response", "state": "done" }
                    ]
                  },
                  { "role": "user", "parts": [{ "type": "text", "text": "Thanks!" }] }
                ]
                """
            )
        );

        AssertEqual(
            """
            [
              { "content": [{ "input": { "value": "value-1" }, "toolCallId": "call-1", "toolName": "screenshot", "type": "tool-call" }], "role": "assistant" },
              { "content": [{ "output": { "type": "text", "value": "result-1" }, "toolCallId": "call-1", "toolName": "screenshot", "type": "tool-result" }], "role": "tool" },
              {
                "content": [
                  { "input": { "value": "value-2" }, "toolCallId": "call-2", "toolName": "screenshot", "type": "tool-call" },
                  { "input": { "value": "value-3" }, "toolCallId": "call-3", "toolName": "screenshot", "type": "tool-call" }
                ],
                "role": "assistant"
              },
              {
                "content": [
                  { "output": { "type": "text", "value": "result-2" }, "toolCallId": "call-2", "toolName": "screenshot", "type": "tool-result" },
                  { "output": { "type": "text", "value": "result-3" }, "toolCallId": "call-3", "toolName": "screenshot", "type": "tool-result" }
                ],
                "role": "tool"
              },
              { "content": [{ "input": { "value": "value-4" }, "toolCallId": "call-4", "toolName": "screenshot", "type": "tool-call" }], "role": "assistant" },
              { "content": [{ "output": { "type": "text", "value": "result-4" }, "toolCallId": "call-4", "toolName": "screenshot", "type": "tool-result" }], "role": "tool" },
              { "content": [{ "text": "response", "type": "text" }], "role": "assistant" },
              { "content": [{ "text": "Thanks!", "type": "text" }], "role": "user" }
            ]
            """,
            result
        );
    }
}
