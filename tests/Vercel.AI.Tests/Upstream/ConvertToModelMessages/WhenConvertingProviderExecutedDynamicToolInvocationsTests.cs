// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Ui;
using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages &gt; when converting provider-executed dynamic tool invocations</c>.</summary>
[Collection("LogWarnings")]
public sealed class WhenConvertingProviderExecutedDynamicToolInvocationsTests : WarningsOff
{
    private const string Suite =
        Prefix
        + "convertToModelMessages > when converting provider-executed dynamic tool invocations::";

    [UpstreamTest(
        Suite + "should convert a provider-executed dynamic tool invocation",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_a_provider_executed_dynamic_tool_invocation()
    {
        var result = Convert(
            UiMessages(
                """
                [
                  {
                    "role": "assistant",
                    "parts": [
                      { "type": "step-start" },
                      {
                        "type": "dynamic-tool",
                        "toolName": "screenshot",
                        "state": "output-available",
                        "toolCallId": "call-1",
                        "input": { "value": "value-1" },
                        "output": "result-1",
                        "providerExecuted": true,
                        "callProviderMetadata": { "test-provider": { "key-a": "test-value-1", "key-b": "test-value-2" } }
                      }
                    ]
                  },
                  { "role": "user", "parts": [{ "type": "text", "text": "Thanks!" }] }
                ]
                """
            ),
            new ConvertToModelMessagesOptions { IgnoreIncompleteToolCalls = true }
        );

        AssertEqual(
            """
            [
              {
                "content": [
                  {
                    "input": { "value": "value-1" },
                    "providerExecuted": true,
                    "providerOptions": { "test-provider": { "key-a": "test-value-1", "key-b": "test-value-2" } },
                    "toolCallId": "call-1",
                    "toolName": "screenshot",
                    "type": "tool-call"
                  },
                  {
                    "output": { "type": "text", "value": "result-1" },
                    "providerOptions": { "test-provider": { "key-a": "test-value-1", "key-b": "test-value-2" } },
                    "toolCallId": "call-1",
                    "toolName": "screenshot",
                    "type": "tool-result"
                  }
                ],
                "role": "assistant"
              },
              { "content": [{ "text": "Thanks!", "type": "text" }], "role": "user" }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite
            + "should convert a denied provider-executed tool approval request with an execution-denied result",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_a_denied_provider_executed_tool_approval_request_with_an_execution_denied_result()
    {
        var result = Convert(
            UiMessages(
                """
                [
                  {
                    "role": "assistant",
                    "parts": [
                      { "type": "step-start" },
                      {
                        "type": "dynamic-tool",
                        "toolName": "screenshot",
                        "state": "approval-responded",
                        "toolCallId": "call-1",
                        "input": { "value": "value-1" },
                        "providerExecuted": true,
                        "callProviderMetadata": { "test-provider": { "key-a": "test-value-1" } },
                        "approval": { "id": "approval-1", "approved": false, "reason": "User denied the request" }
                      }
                    ]
                  },
                  { "role": "user", "parts": [{ "type": "text", "text": "Thanks!" }] }
                ]
                """
            ),
            new ConvertToModelMessagesOptions { IgnoreIncompleteToolCalls = true }
        );

        AssertEqual(
            """
            [
              {
                "content": [
                  {
                    "input": { "value": "value-1" },
                    "providerExecuted": true,
                    "providerOptions": { "test-provider": { "key-a": "test-value-1" } },
                    "toolCallId": "call-1",
                    "toolName": "screenshot",
                    "type": "tool-call"
                  },
                  { "approvalId": "approval-1", "toolCallId": "call-1", "type": "tool-approval-request" }
                ],
                "role": "assistant"
              },
              {
                "content": [
                  { "approvalId": "approval-1", "approved": false, "providerExecuted": true, "reason": "User denied the request", "type": "tool-approval-response" },
                  {
                    "output": { "reason": "User denied the request", "type": "execution-denied" },
                    "providerOptions": { "test-provider": { "key-a": "test-value-1" } },
                    "toolCallId": "call-1",
                    "toolName": "screenshot",
                    "type": "tool-result"
                  }
                ],
                "role": "tool"
              },
              { "content": [{ "text": "Thanks!", "type": "text" }], "role": "user" }
            ]
            """,
            result
        );
    }
}
