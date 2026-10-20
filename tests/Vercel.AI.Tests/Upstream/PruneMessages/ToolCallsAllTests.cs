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

    [UpstreamTest(Prefix + "pruneMessages > toolCalls > all::should prune reasoning whose following tool content was pruned", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_prune_reasoning_whose_following_tool_content_was_pruned()
    {
        var messages = new ModelMessage[]
        {
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("Call the first tool."),
                Call("call-1", "remove-me", "{}"),
                new AssistantReasoningFilePart("image/png", null, Utf8("reasoning-image")),
                Call("call-2", "remove-me", "{}"),
            }),
        };

        AssertEqual(Array.Empty<ModelMessage>(), Prune(messages, PruneReasoning.None, PruneToolCallsRule.All()));
        AssertEqual(
            new ModelMessage[] { new AssistantModelMessage(Array.Empty<AssistantContentPart>()) },
            Ai.PruneMessages(new PruneMessagesOptions
            {
                Messages = messages,
                ToolCalls = new[] { PruneToolCallsRule.All() },
                EmptyMessages = PruneEmptyMessages.Keep,
            }));
    }

    [UpstreamTest(Prefix + "pruneMessages > toolCalls > all::should retain reasoning followed by kept content", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_retain_reasoning_followed_by_kept_content()
    {
        var messages = new ModelMessage[]
        {
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("Only explains the removed tool."),
                Call("call-removed", "remove-me", "{}"),
                new AssistantReasoningPart("Explains the retained tool."),
                Call("call-kept", "keep-me", "{}"),
                new AssistantReasoningPart("Explains the retained text."),
                Call("call-before-text", "remove-me", "{}"),
                new AssistantTextPart("Retained answer."),
            }),
        };

        var expected = new ModelMessage[]
        {
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("Explains the retained tool."),
                Call("call-kept", "keep-me", "{}"),
                new AssistantReasoningPart("Explains the retained text."),
                new AssistantTextPart("Retained answer."),
            }),
        };

        AssertEqual(expected, Prune(messages, PruneReasoning.None, PruneToolCallsRule.All(new[] { "remove-me" })));
    }
}
