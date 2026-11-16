// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>tool approval response with signature</c>.</summary>
public sealed class ToolApprovalResponseWithSignatureTests
{
    private const string Prefix =
        "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(
        Prefix
            + "tool approval response with signature::preserves request details separately from the response reason",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_request_details_separately_from_the_response_reason()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync(
            """
            [{"type":"start"},{"type":"start-step"},{"input":{"value":"value"},"toolCallId":"call-1","toolName":"tool1","type":"tool-input-available"},{"approvalId":"id-1","toolCallId":"call-1","type":"tool-approval-request","reason":"requires operator review","signature":"test-sig"},{"approvalId":"id-1","approved":true,"reason":"approved by operator","type":"tool-approval-response"},{"type":"finish-step"},{"type":"finish"}]
            """
        );

        var toolPart = ProcessUIMessageStreamHarness.Find(harness.Parts, part =>
            (string?)part["type"] == "tool-tool1"
        )!;
        Assert.Equal("approval-responded", (string?)toolPart["state"]);
        ProcessUIMessageStreamHarness.AssertJson(
            """
            {"id":"id-1","approved":true,"requestReason":"requires operator review","reason":"approved by operator","signature":"test-sig"}
            """,
            toolPart["approval"]
        );
    }
}
