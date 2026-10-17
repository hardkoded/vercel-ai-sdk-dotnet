// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.PruneMessages.PruneMessagesSupport;

namespace Vercel.AI.Tests.PruneMessages;

/// <summary>Port of <c>prune-messages.test.ts</c> &gt; <c>pruneMessages &gt; toolCalls &gt; two tool settings</c>.</summary>
public sealed class ToolCallsTwoToolSettingsTests
{
    [UpstreamTest(Prefix + "pruneMessages > toolCalls > two tool settings::should prune all tool calls, results, errors, and approvals", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_prune_all_tool_calls_results_errors_and_approvals()
    {
        var result = Prune(
            MessagesFixture1,
            PruneReasoning.None,
            PruneToolCallsRule.All(new[] { "get-weather-tool-1" }),
            PruneToolCallsRule.BeforeLastMessages(2, new[] { "get-weather-tool-2" }));

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
                      "city": "Busan"
                    },
                    "toolCallId": "call-2",
                    "toolName": "get-weather-tool-2",
                    "type": "tool-call"
                  },
                  {
                    "approvalId": "approval-1",
                    "toolCallId": "call-2",
                    "type": "tool-approval-request"
                  }
                ],
                "role": "assistant"
              },
              {
                "content": [
                  {
                    "approvalId": "approval-1",
                    "approved": true,
                    "type": "tool-approval-response"
                  },
                  {
                    "output": {
                      "type": "error-text",
                      "value": "Error: Fetching weather data failed"
                    },
                    "toolCallId": "call-2",
                    "toolName": "get-weather-tool-2",
                    "type": "tool-result"
                  }
                ],
                "role": "tool"
              },
              {
                "content": [
                  {
                    "text": "I have got the weather in Tokyo and Busan.",
                    "type": "reasoning"
                  },
                  {
                    "text": "The weather in Tokyo is sunny. I could not get the weather in Busan.",
                    "type": "text"
                  }
                ],
                "role": "assistant"
              }
            ]
            """,
            result);
    }
}
