// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>automatic tool approval denial (static tool)</c>.</summary>
public sealed class AutomaticToolApprovalDenialStaticToolTests
{
    private const string Prefix = "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(Prefix + "automatic tool approval denial (static tool)::should keep automatic approval metadata through denial", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_keep_automatic_approval_metadata_through_denial()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start"},{"type":"start-step"},{"input":{"value":"value"},"toolCallId":"call-1","toolName":"tool1","type":"tool-input-available"},{"approvalId":"id-1","isAutomatic":true,"toolCallId":"call-1","type":"tool-approval-request"},{"approvalId":"id-1","approved":false,"reason":"Policy denied execution","type":"tool-approval-response"},{"toolCallId":"call-1","type":"tool-output-denied"},{"type":"finish-step"},{"type":"finish"}]
            """);

        ProcessUIMessageStreamHarness.AssertJson("""
            [
              {
                "input": {
                  "value": "value"
                },
                "state": "input-available",
                "toolCallId": "call-1",
                "type": "tool-tool1"
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
                "type": "tool-tool1"
              },
              {
                "approval": {
                  "approved": false,
                  "id": "id-1",
                  "isAutomatic": true,
                  "reason": "Policy denied execution"
                },
                "input": {
                  "value": "value"
                },
                "state": "approval-responded",
                "toolCallId": "call-1",
                "type": "tool-tool1"
              },
              {
                "approval": {
                  "approved": false,
                  "id": "id-1",
                  "isAutomatic": true,
                  "reason": "Policy denied execution"
                },
                "input": {
                  "value": "value"
                },
                "state": "output-denied",
                "toolCallId": "call-1",
                "type": "tool-tool1"
              }
            ]
            """, harness.WriteCallParts(1));
        ProcessUIMessageStreamHarness.AssertJson("""
            {
              "approval": {
                "approved": false,
                "id": "id-1",
                "isAutomatic": true,
                "reason": "Policy denied execution"
              },
              "input": {
                "value": "value"
              },
              "state": "output-denied",
              "toolCallId": "call-1",
              "type": "tool-tool1"
            }
            """, harness.Parts[1]);
    }
}
