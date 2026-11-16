// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>static tool name changes</c>.</summary>
public sealed class StaticToolNameChangesTests
{
    private const string Prefix = "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(Prefix + "static tool name changes::uses the completed input tool name for an existing streamed part", Coverage = UpstreamCoverage.Covered, Commit = "8ba3940d3dd1f3cd3c4365bbaec505c6ef0eb1e3")]
    public async Task Uses_the_completed_input_tool_name_for_an_existing_streamed_part()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start-step"},{"type":"tool-input-start","toolCallId":"tool-call-0","toolName":"originalTool"},{"type":"tool-input-delta","toolCallId":"tool-call-0","inputTextDelta":"{\"value\":\"test\"}"},{"type":"tool-input-available","toolCallId":"tool-call-0","toolName":"renamedTool","input":{"value":"test"}},{"type":"tool-approval-request","toolCallId":"tool-call-0","approvalId":"approval-0"}]
            """);

        ProcessUIMessageStreamHarness.AssertJson("""
            {"type":"tool-renamedTool","toolCallId":"tool-call-0","state":"approval-requested","input":{"value":"test"},"approval":{"id":"approval-0"}}
            """, ProcessUIMessageStreamHarness.PickKeys(harness.Parts[1]!.AsObject(), "type", "toolCallId", "state", "input", "approval"));
    }
}
