// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.PruneMessages.PruneMessagesSupport;

namespace Vercel.AI.Tests.PruneMessages;

/// <summary>Port of <c>prune-messages.test.ts</c> &gt; <c>pruneMessages &gt; reasoning &gt; all</c>.</summary>
public sealed class ReasoningAllTests
{
    // Not ported: 'should prune reasoning files while preserving regular files and text' and
    // 'should remove messages containing only reasoning text and files' need reasoning-file and file parts
    // in assistant messages, which the port does not have.

    [UpstreamTest(Prefix + "pruneMessages > reasoning > all::should prune all reasoning parts", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_prune_all_reasoning_parts()
    {
        var result = Prune(MessagesFixture1, PruneReasoning.All);

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
                      "type": "text",
                      "value": "sunny"
                    },
                    "toolCallId": "call-1",
                    "toolName": "get-weather-tool-1",
                    "type": "tool-result"
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
