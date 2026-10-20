// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.PruneMessages.PruneMessagesSupport;

namespace Vercel.AI.Tests.PruneMessages;

/// <summary>Port of <c>prune-messages.test.ts</c> &gt; <c>pruneMessages &gt; toolCalls &gt; consecutive reasoning parts &gt; before-last-message</c>.</summary>
public sealed class ToolCallsConsecutiveReasoningPartsBeforeLastMessageTests
{
    [UpstreamTest(Prefix + "pruneMessages > toolCalls > consecutive reasoning parts > before-last-message::should prune all %i reasoning summaries", Coverage = UpstreamCoverage.Covered)]
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void Should_prune_all_i_reasoning_summaries(int count)
    {
        var parts = new List<AssistantContentPart>();
        for (var index = 0; index < count; index++)
        {
            parts.Add(new AssistantReasoningPart($"summary {index}", ItemId("rs_1")));
        }

        parts.Add(Call("call-1", "remove-me", "{}"));

        var messages = new ModelMessage[]
        {
            new AssistantModelMessage(parts),
            new ToolModelMessage("call-1", "remove-me", "Tool result.", false, "text"),
            new UserModelMessage("Continue."),
            new AssistantModelMessage(new AssistantContentPart[] { new AssistantTextPart("Final answer.") }),
        };

        AssertEqual(messages.Skip(2).ToArray(), Prune(messages, PruneReasoning.None, PruneToolCallsRule.BeforeLastMessage()));
    }
}
