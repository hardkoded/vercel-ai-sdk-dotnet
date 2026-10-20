// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.PruneMessages.PruneMessagesSupport;

namespace Vercel.AI.Tests.PruneMessages;

/// <summary>Port of <c>prune-messages.test.ts</c> &gt; <c>pruneMessages &gt; toolCalls &gt; all</c>.</summary>
public sealed class ToolCallsAllTests
{
    [UpstreamTest(Prefix + "pruneMessages > toolCalls > all::should prune all tool calls, results, errors, and approvals", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_prune_all_tool_calls_results_errors_and_approvals()
    {
        var result = Prune(MessagesFixture1, PruneReasoning.None, PruneToolCallsRule.All());

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
