// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.PruneMessages.PruneMessagesSupport;

namespace Vercel.AI.Tests.PruneMessages;

/// <summary>Port of <c>prune-messages.test.ts</c> &gt; <c>pruneMessages &gt; toolCalls &gt; before-last-message</c>.</summary>
public sealed class ToolCallsBeforeLastMessageTests
{
    [UpstreamTest(Prefix + "pruneMessages > toolCalls > before-last-message::should prune tool calls before last message", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_prune_tool_calls_before_last_message()
    {
        var result = Prune(MessagesFixture2, PruneReasoning.None, PruneToolCallsRule.BeforeLastMessage());

        AssertSnapshot(
            """
            [
              {
                "content": [
                  {
                    "text": "Weather in Tokyo and Busan?",
                    "type": "text"
                  }
                ],
                "role": "user"
              },
              {
                "content": [
                  {
                    "text": "I need to get the weather in Tokyo and Busan.",
                    "type": "reasoning"
                  },
                  {
                    "input": {
                      "city": "Tokyo"
                    },
                    "toolCallId": "call-1",
                    "toolName": "get-weather-tool-1",
                    "type": "tool-call"
                  },
                  {
                    "input": {
                      "city": "Busan"
                    },
                    "toolCallId": "call-2",
                    "toolName": "get-weather-tool-2",
                    "type": "tool-call"
                  },
                  {
                    "approvalId": "approval-1",
                    "toolCallId": "call-1",
                    "type": "tool-approval-request"
                  }
                ],
                "role": "assistant"
              }
            ]
            """,
            result);
    }

    [UpstreamTest(Prefix + "pruneMessages > toolCalls > before-last-message::should prune tool calls and results from multi-turn conversation when last message has no tool calls", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_prune_tool_calls_and_results_from_multi_turn_conversation_when_last_message_has_no_tool_calls()
    {
        var result = Prune(MultiTurnToolCallMessagesFixture, PruneReasoning.None, PruneToolCallsRule.BeforeLastMessage());

        AssertSnapshot(
            """
            [
              {
                "content": [
                  {
                    "text": "ask me a question",
                    "type": "text"
                  }
                ],
                "role": "user"
              },
              {
                "content": [
                  {
                    "text": "What can i help you with",
                    "type": "text"
                  }
                ],
                "role": "assistant"
              },
              {
                "content": [
                  {
                    "text": "What would you like to discuss or work on?",
                    "type": "text"
                  }
                ],
                "role": "assistant"
              },
              {
                "content": [
                  {
                    "text": "never mind. lets end this conversation",
                    "type": "text"
                  }
                ],
                "role": "user"
              },
              {
                "content": [
                  {
                    "text": "ok, have a nice day",
                    "type": "text"
                  }
                ],
                "role": "assistant"
              },
              {
                "content": [
                  {
                    "text": "thank you",
                    "type": "text"
                  }
                ],
                "role": "user"
              }
            ]
            """,
            result);
    }

    [UpstreamTest(Prefix + "pruneMessages > toolCalls > before-last-message::should retain the originating tool call for a pending approval response", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_retain_the_originating_tool_call_for_a_pending_approval_response()
    {
        var expected = Render(PendingApprovalMessagesFixture);

        Assert.True(System.Text.Json.Nodes.JsonNode.DeepEquals(
            expected,
            Render(Prune(PendingApprovalMessagesFixture, PruneReasoning.None, PruneToolCallsRule.BeforeLastMessage()))));

        Assert.True(System.Text.Json.Nodes.JsonNode.DeepEquals(
            expected,
            Render(Prune(PendingApprovalMessagesFixture, PruneReasoning.None, PruneToolCallsRule.BeforeLastMessage(new[] { "echo" })))));
    }
}
