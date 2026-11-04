// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages &gt; when converting tool approval request responses</c>.</summary>
[Collection("LogWarnings")]
public sealed class WhenConvertingToolApprovalRequestResponsesTests : WarningsOff
{
    private const string Suite = Prefix + "convertToModelMessages > when converting tool approval request responses::";

    [UpstreamTest(Suite + "should propagate reason from a pending approval request", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_propagate_reason_from_a_pending_approval_request()
    {
        var result = Convert(new[]
        {
            Assistant(new ToolUIPart("weather", "call-1", ToolUIPartState.ApprovalRequested)
            {
                Approval = new ToolUIApproval("a1") { RequestReason = "requires operator review" },
                Input = Json("{\"city\":\"Tokyo\"}"),
            }),
        });

        var assistantMessage = Render(result).AsArray().First(message => (string?)message!["role"] == "assistant")!;
        Assert.Contains(
            assistantMessage["content"]!.AsArray(),
            part => JsonNode.DeepEquals(
                part,
                JsonNode.Parse(
                    """
                    {
                      "type": "tool-approval-request",
                      "approvalId": "a1",
                      "toolCallId": "call-1",
                      "reason": "requires operator review"
                    }
                    """)));
    }

    [UpstreamTest(Suite + "should keep request and response reasons separate after approval", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_keep_request_and_response_reasons_separate_after_approval()
    {
        var result = Convert(new[]
        {
            Assistant(new ToolUIPart("weather", "call-1", ToolUIPartState.ApprovalResponded)
            {
                Approval = new ToolUIApproval("a1")
                {
                    Approved = true,
                    RequestReason = "requires operator review",
                    Reason = "approved by on-call operator",
                },
                Input = Json("{\"city\":\"Tokyo\"}"),
            }),
        });

        var rendered = Render(result).AsArray();
        var assistantMessage = rendered.First(message => (string?)message!["role"] == "assistant")!;
        Assert.Contains(
            assistantMessage["content"]!.AsArray(),
            part => JsonNode.DeepEquals(
                part,
                JsonNode.Parse(
                    """
                    {
                      "type": "tool-approval-request",
                      "approvalId": "a1",
                      "toolCallId": "call-1",
                      "reason": "requires operator review"
                    }
                    """)));

        var toolMessage = rendered.First(message => (string?)message!["role"] == "tool")!;
        Assert.Contains(
            toolMessage["content"]!.AsArray(),
            part => JsonNode.DeepEquals(
                part,
                JsonNode.Parse(
                    """
                    {
                      "type": "tool-approval-response",
                      "approvalId": "a1",
                      "approved": true,
                      "reason": "approved by on-call operator"
                    }
                    """)));
    }

    [UpstreamTest(Suite + "should propagate signature from approval to tool-approval-request part", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_propagate_signature_from_approval_to_tool_approval_request_part()
    {
        var result = Convert(new[]
        {
            User(new TextUIPart("What is the weather in Tokyo?")),
            Assistant(
                new StepStartUIPart(),
                new ToolUIPart("weather", "call-1", ToolUIPartState.ApprovalResponded)
                {
                    Approval = new ToolUIApproval("a1") { Approved = true, Signature = "test-sig" },
                    Input = Json("{\"city\":\"Tokyo\"}"),
                }),
        });

        AssertEqual(
            """
            [
              {
                "content": [
                  { "text": "What is the weather in Tokyo?", "type": "text" }
                ],
                "role": "user"
              },
              {
                "content": [
                  {
                    "input": { "city": "Tokyo" },
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "type": "tool-call"
                  },
                  {
                    "approvalId": "a1",
                    "signature": "test-sig",
                    "toolCallId": "call-1",
                    "type": "tool-approval-request"
                  }
                ],
                "role": "assistant"
              },
              {
                "content": [
                  {
                    "approvalId": "a1",
                    "approved": true,
                    "type": "tool-approval-response"
                  }
                ],
                "role": "tool"
              }
            ]
            """,
            result);
    }

    [UpstreamTest(Suite + "should not include signature in tool-approval-request when approval has no signature", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_not_include_signature_in_tool_approval_request_when_approval_has_no_signature()
    {
        var result = Convert(new[]
        {
            User(new TextUIPart("What is the weather in Tokyo?")),
            Assistant(
                new StepStartUIPart(),
                new ToolUIPart("weather", "call-1", ToolUIPartState.ApprovalResponded)
                {
                    Approval = new ToolUIApproval("a1") { Approved = true },
                    Input = Json("{\"city\":\"Tokyo\"}"),
                }),
        });

        var assistantMessage = result.OfType<Provider.AssistantModelMessage>().Single();
        var approvalRequest = assistantMessage.Parts.OfType<Provider.AssistantToolApprovalRequestPart>().SingleOrDefault();
        Assert.NotNull(approvalRequest);
        Assert.Null(approvalRequest.Signature);
        Assert.DoesNotContain("signature", Render(result)[1]!["content"]![1]!.AsObject().Select(property => property.Key));
    }
}
