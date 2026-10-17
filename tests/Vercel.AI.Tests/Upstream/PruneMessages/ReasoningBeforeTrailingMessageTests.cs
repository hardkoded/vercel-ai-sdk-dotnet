// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.PruneMessages.PruneMessagesSupport;

namespace Vercel.AI.Tests.PruneMessages;

/// <summary>Port of <c>prune-messages.test.ts</c> &gt; <c>pruneMessages &gt; reasoning &gt; before-trailing-message</c>.</summary>
public sealed class ReasoningBeforeTrailingMessageTests
{
    // Not ported: 'should prune reasoning files before the last message' needs reasoning-file parts,
    // which the port does not have.

    [UpstreamTest(Prefix + "pruneMessages > reasoning > before-trailing-message::should prune the trailing message", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_prune_the_trailing_message()
    {
        var result = Prune(MessagesFixture1, PruneReasoning.BeforeLastMessage);

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
