// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>tool approval request without signature</c>.</summary>
public sealed class ToolApprovalRequestWithoutSignatureTests
{
    private const string Prefix = "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(Prefix + "tool approval request without signature::should not include signature in the approval object", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_include_signature_in_the_approval_object()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start"},{"type":"start-step"},{"input":{"value":"value"},"toolCallId":"call-1","toolName":"tool1","type":"tool-input-available"},{"approvalId":"id-1","toolCallId":"call-1","type":"tool-approval-request"},{"type":"finish-step"},{"type":"finish"}]
            """);

        var toolPart = ProcessUIMessageStreamHarness.Find(harness.Parts, part => (string?)part["type"] == "tool-tool1")!;
        Assert.Equal("approval-requested", (string?)toolPart["state"]);
        ProcessUIMessageStreamHarness.AssertJson("""
            {"id":"id-1"}
            """, toolPart["approval"]);
        Assert.False(((JsonObject)toolPart["approval"]!).ContainsKey("signature"));
    }
}
