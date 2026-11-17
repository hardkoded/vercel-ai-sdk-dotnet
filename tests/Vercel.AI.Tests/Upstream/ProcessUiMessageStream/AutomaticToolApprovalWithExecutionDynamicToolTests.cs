// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>automatic tool approval with execution (dynamic tool)</c>.</summary>
public sealed class AutomaticToolApprovalWithExecutionDynamicToolTests
{
    private const string Prefix =
        "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(
        Prefix
            + "automatic tool approval with execution (dynamic tool)::should keep automatic approval metadata through execution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_keep_automatic_approval_metadata_through_execution()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync(
            """
            [{"type":"start"},{"type":"start-step"},{"dynamic":true,"input":{"value":"value"},"toolCallId":"call-1","toolName":"tool1","type":"tool-input-available"},{"approvalId":"id-1","isAutomatic":true,"toolCallId":"call-1","type":"tool-approval-request"},{"approvalId":"id-1","approved":true,"reason":"trusted internal tool","type":"tool-approval-response"},{"output":"result1","toolCallId":"call-1","type":"tool-output-available"},{"type":"finish-step"},{"type":"finish"}]
            """
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            [
              {
                "input": {
                  "value": "value"
                },
                "state": "input-available",
                "toolCallId": "call-1",
                "toolName": "tool1",
                "type": "dynamic-tool"
              },
              {
                "approval": {
                  "id": "id-1",
                  "isAutomatic": true
                },
                "input": {
                  "value": "value"
                },
                "state": "approval-requested",
                "toolCallId": "call-1",
                "toolName": "tool1",
                "type": "dynamic-tool"
              },
              {
                "approval": {
                  "approved": true,
                  "id": "id-1",
                  "isAutomatic": true,
                  "reason": "trusted internal tool"
                },
                "input": {
                  "value": "value"
                },
                "state": "approval-responded",
                "toolCallId": "call-1",
                "toolName": "tool1",
                "type": "dynamic-tool"
              },
              {
                "approval": {
                  "approved": true,
                  "id": "id-1",
                  "isAutomatic": true,
                  "reason": "trusted internal tool"
                },
                "input": {
                  "value": "value"
                },
                "output": "result1",
                "state": "output-available",
                "toolCallId": "call-1",
                "toolName": "tool1",
                "type": "dynamic-tool"
              }
            ]
            """,
            harness.WriteCallParts(1)
        );
        ProcessUIMessageStreamHarness.AssertJson(
            """
            {
              "approval": {
                "approved": true,
                "id": "id-1",
                "isAutomatic": true,
                "reason": "trusted internal tool"
              },
              "input": {
                "value": "value"
              },
              "output": "result1",
              "state": "output-available",
              "toolCallId": "call-1",
              "toolName": "tool1",
              "type": "dynamic-tool"
            }
            """,
            harness.Parts[1]
        );
    }
}
