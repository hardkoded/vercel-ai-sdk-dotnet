// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.PruneMessages.PruneMessagesSupport;

namespace Vercel.AI.Tests.PruneMessages;

/// <summary>Port of <c>prune-messages.test.ts</c> &gt; <c>pruneMessages &gt; toolCalls &gt; consecutive reasoning parts</c>.</summary>
public sealed class ToolCallsConsecutiveReasoningPartsTests
{
    [UpstreamTest(Prefix + "pruneMessages > toolCalls > consecutive reasoning parts::should prune consecutive reasoning text and files with emptyMessages: %s", Coverage = UpstreamCoverage.Covered)]
    [Theory]
    [InlineData(PruneEmptyMessages.Remove)]
    [InlineData(PruneEmptyMessages.Keep)]
    public void Should_prune_consecutive_reasoning_text_and_files_with_emptyMessages(PruneEmptyMessages emptyMessages)
    {
        var messages = new ModelMessage[]
        {
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("Call the tool."),
                new AssistantReasoningFilePart("image/png", null, Utf8("reasoning-image")),
                Call("call-1", "remove-me", "{}"),
            }),
        };

        var result = Ai.PruneMessages(new PruneMessagesOptions
        {
            Messages = messages,
            ToolCalls = new[] { PruneToolCallsRule.All() },
            EmptyMessages = emptyMessages,
        });

        AssertEqual(
            emptyMessages == PruneEmptyMessages.Keep
                ? new ModelMessage[] { new AssistantModelMessage(Array.Empty<AssistantContentPart>()) }
                : Array.Empty<ModelMessage>(),
            result);
    }

    [UpstreamTest(Prefix + "pruneMessages > toolCalls > consecutive reasoning parts::should keep the reasoning group for a retained tool during selective pruning", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_keep_the_reasoning_group_for_a_retained_tool_during_selective_pruning()
    {
        var removedReasoning = new AssistantContentPart[]
        {
            new AssistantReasoningPart("Removed summary 0.", ItemId("rs_removed")),
            new AssistantReasoningPart("Removed summary 1.", ItemId("rs_removed")),
        };
        var retainedContent = new AssistantContentPart[]
        {
            new AssistantReasoningPart("Retained summary 0.", ItemId("rs_kept")),
            new AssistantReasoningPart("Retained summary 1.", ItemId("rs_kept")),
            Call("call-kept", "keep-me", "{}"),
        };

        var result = Prune(
            new ModelMessage[]
            {
                new AssistantModelMessage(removedReasoning
                    .Append(Call("call-removed", "remove-me", "{}"))
                    .Concat(retainedContent)
                    .ToArray()),
            },
            PruneReasoning.None,
            PruneToolCallsRule.All(new[] { "remove-me" }));

        AssertEqual(new ModelMessage[] { new AssistantModelMessage(retainedContent) }, result);
    }

    [UpstreamTest(Prefix + "pruneMessages > toolCalls > consecutive reasoning parts::should retain the entire reasoning group when text follows a pruned tool call", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_retain_the_entire_reasoning_group_when_text_follows_a_pruned_tool_call()
    {
        var reasoning = new AssistantContentPart[]
        {
            new AssistantReasoningPart("First summary."),
            new AssistantReasoningFilePart("image/png", null, Utf8("reasoning-image")),
        };
        var text = new AssistantTextPart("Retained answer.");

        var result = Prune(
            new ModelMessage[]
            {
                new AssistantModelMessage(reasoning
                    .Append(Call("call-1", "remove-me", "{}"))
                    .Append(text)
                    .ToArray()),
            },
            PruneReasoning.None,
            PruneToolCallsRule.All());

        AssertEqual(new ModelMessage[] { new AssistantModelMessage(reasoning.Append(text).ToArray()) }, result);
    }

    [UpstreamTest(Prefix + "pruneMessages > toolCalls > consecutive reasoning parts::should retain a reasoning group without following content", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_retain_a_reasoning_group_without_following_content()
    {
        var messages = new ModelMessage[]
        {
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("First summary."),
                new AssistantReasoningPart("Second summary."),
            }),
        };

        AssertEqual(messages, Prune(messages, PruneReasoning.None, PruneToolCallsRule.All()));
    }
}
