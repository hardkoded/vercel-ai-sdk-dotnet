// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.PruneMessages.PruneMessagesSupport;

namespace Vercel.AI.Tests.PruneMessages;

/// <summary>Port of <c>prune-messages.test.ts</c> &gt; <c>pruneMessages &gt; toolCalls &gt; selective tool pruning with approvals (regression)</c>.</summary>
public sealed class ToolCallsSelectiveToolPruningWithApprovalsRegressionTests
{
    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    [UpstreamTest(Prefix + "pruneMessages > toolCalls > selective tool pruning with approvals (regression)::should prune the approval response together with its request and tool-call", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_prune_the_approval_response_together_with_its_request_and_tool_call()
    {
        var messages = Ai.ConvertToModelMessages(new[]
        {
            new UIMessage(UIMessageRole.User, new UIMessagePart[] { new TextUIPart("Weather in Tokyo and Busan?") }),
            new UIMessage(UIMessageRole.Assistant, new UIMessagePart[]
            {
                new StepStartUIPart(),
                new ToolUIPart("get-weather-tool-1", "call-1", ToolUIPartState.OutputAvailable)
                {
                    Input = Json("{\"city\":\"Tokyo\"}"),
                    Output = Json("\"sunny\""),
                },
                new ToolUIPart("get-weather-tool-2", "call-2", ToolUIPartState.OutputAvailable)
                {
                    Input = Json("{\"city\":\"Busan\"}"),
                    Output = Json("\"rainy\""),
                    Approval = new ToolUIApproval("approval-1") { Approved = true },
                },
            }),
        });

        var result = Prune(messages, PruneReasoning.None, PruneToolCallsRule.All(new[] { "get-weather-tool-2" }));

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

        // No orphaned approval responses remain in the pruned output.
        var approvalIds = new HashSet<string>();
        var responseIds = new HashSet<string>();
        foreach (var message in result)
        {
            if (message is AssistantModelMessage assistant)
            {
                foreach (var part in assistant.Parts)
                {
                    if (part is AssistantToolApprovalRequestPart request)
                    {
                        approvalIds.Add(request.ApprovalId);
                    }
                }
            }
            else if (message is ToolApprovalResponseModelMessage response)
            {
                responseIds.Add(response.ApprovalId);
            }
        }

        foreach (var id in responseIds)
        {
            Assert.Contains(id, approvalIds);
        }
    }

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
