// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.PruneMessages.PruneMessagesSupport;

namespace Vercel.AI.Tests.PruneMessages;

/// <summary>Port of <c>prune-messages.test.ts</c> &gt; <c>pruneMessages &gt; toolCalls &gt; before-last-0-messages</c>.</summary>
public sealed class ToolCallsBeforeLast0MessagesTests
{
    [UpstreamTest(Prefix + "pruneMessages > toolCalls > before-last-0-messages::should prune all tool calls, results, errors, and approvals", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_prune_all_tool_calls_results_errors_and_approvals()
    {
        var actual = Prune(MessagesFixture1, PruneReasoning.None, PruneToolCallsRule.BeforeLastMessages(0));
        var expected = Prune(MessagesFixture1, PruneReasoning.None, PruneToolCallsRule.All());

        Assert.True(System.Text.Json.Nodes.JsonNode.DeepEquals(Render(expected), Render(actual)));
    }
}
