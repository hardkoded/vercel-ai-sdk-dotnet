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

    [UpstreamTest(Prefix + "pruneMessages > reasoning > all::should prune reasoning files while preserving regular files and text", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_prune_reasoning_files_while_preserving_regular_files_and_text()
    {
        var messages = new ModelMessage[]
        {
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("Intermediate reasoning."),
                new AssistantReasoningFilePart("image/png", null, Utf8("reasoning-image")),
                new AssistantFilePart("image/png", null, Utf8("output-image")),
                new AssistantTextPart("Final answer."),
            }),
        };
        var originalCount = ((AssistantModelMessage)messages[0]).Parts.Count;

        var result = Prune(messages, PruneReasoning.All);

        AssertSnapshot(
            """
            [
              {
                "role": "assistant",
                "content": [
                  { "type": "file", "mediaType": "image/png", "data": "output-image" },
                  { "type": "text", "text": "Final answer." }
                ]
              }
            ]
            """,
            result);
        Assert.Equal(originalCount, ((AssistantModelMessage)messages[0]).Parts.Count);
    }

    [UpstreamTest(Prefix + "pruneMessages > reasoning > all::should remove messages containing only reasoning text and files", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_remove_messages_containing_only_reasoning_text_and_files()
    {
        var result = Prune(
            new ModelMessage[]
            {
                new AssistantModelMessage(new AssistantContentPart[]
                {
                    new AssistantReasoningPart("Intermediate reasoning."),
                    new AssistantReasoningFilePart("image/png", null, Utf8("reasoning-image")),
                }),
            },
            PruneReasoning.All);

        Assert.Empty(result);
    }
}
