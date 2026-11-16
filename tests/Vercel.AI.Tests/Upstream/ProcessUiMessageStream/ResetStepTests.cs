// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>reset-step</c>.</summary>
public sealed class ResetStepTests
{
    private const string Prefix =
        "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(
        Prefix + "reset-step::removes parts from the current step and accepts retried parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Removes_parts_from_the_current_step_and_accepts_retried_parts()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync(
            """
            [{"type":"start-step"},{"type":"text-start","id":"completed-text"},{"type":"text-delta","id":"completed-text","delta":"Completed step"},{"type":"text-end","id":"completed-text"},{"type":"finish-step"},{"type":"start-step"},{"type":"tool-input-start","toolCallId":"stale-tool","toolName":"deleteFile"},{"type":"tool-input-delta","toolCallId":"stale-tool","inputTextDelta":"{\"path\":\"partial"},{"type":"reset-step"},{"type":"tool-input-start","toolCallId":"retried-tool","toolName":"deleteFile"},{"type":"tool-input-delta","toolCallId":"retried-tool","inputTextDelta":"{\"path\":\"target\"}"},{"type":"tool-input-available","toolCallId":"retried-tool","toolName":"deleteFile","input":{"path":"target"}}]
            """
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            [{"type":"step-start"},{"type":"text","text":"Completed step","state":"done"},{"type":"step-start"},{"type":"tool-deleteFile","toolCallId":"retried-tool","state":"input-available","input":{"path":"target"}}]
            """,
            harness.Parts
        );
        Assert.DoesNotContain(harness.Parts.OfType<JsonObject>(), part =>
            UIMessageJson.IsToolPart(part) && (string?)part["toolCallId"] == "stale-tool"
        );
    }
}
