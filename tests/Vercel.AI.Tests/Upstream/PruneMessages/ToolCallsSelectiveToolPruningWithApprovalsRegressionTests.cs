// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.PruneMessages.PruneMessagesSupport;

namespace Vercel.AI.Tests.PruneMessages;

/// <summary>Port of <c>prune-messages.test.ts</c> &gt; <c>pruneMessages &gt; toolCalls &gt; selective tool pruning with approvals (regression)</c>.</summary>
public sealed class ToolCallsSelectiveToolPruningWithApprovalsRegressionTests
{
    // Not ported: 'should prune the approval response together with its request and tool-call' builds its
    // messages with convertToModelMessages, which the port does not have.

    [UpstreamTest(Prefix + "pruneMessages > toolCalls > selective tool pruning with approvals (regression)::should drop unresolved approval responses during selective pruning", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_drop_unresolved_approval_responses_during_selective_pruning()
    {
        var messages = new ModelMessage[]
        {
            new UserModelMessage("Weather in Tokyo and Busan?"),
            new AssistantModelMessage(new AssistantContentPart[] { Call("call-1", "get-weather-tool-1", "{\"city\":\"Tokyo\"}") }),
            new ToolApprovalResponseModelMessage("unknown-approval", true),
            new ToolModelMessage("call-1", "get-weather-tool-1", "sunny", false, "text"),
        };

        var result = Prune(messages, PruneReasoning.None, PruneToolCallsRule.All(new[] { "get-weather-tool-2" }));

        // The unresolved approval response must not survive as authority
        // context; only get-weather-tool-1 parts remain.
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
                    "input": {
                      "city": "Tokyo"
                    },
                    "toolCallId": "call-1",
                    "toolName": "get-weather-tool-1",
                    "type": "tool-call"
                  }
                ],
                "role": "assistant"
              },
              {
                "content": [
                  {
                    "output": {
                      "type": "text",
                      "value": "sunny"
                    },
                    "toolCallId": "call-1",
                    "toolName": "get-weather-tool-1",
                    "type": "tool-result"
                  }
                ],
                "role": "tool"
              }
            ]
            """,
            result);
    }
}
