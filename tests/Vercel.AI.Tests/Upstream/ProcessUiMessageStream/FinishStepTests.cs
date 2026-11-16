// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>finish-step</c>.</summary>
public sealed class FinishStepTests
{
    private const string Prefix = "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(Prefix + "finish-step::preserves active text and reasoning parts across interleaved step boundaries", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_active_text_and_reasoning_parts_across_interleaved_step_boundaries()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"text-start","id":"text-1"},{"type":"text-delta","id":"text-1","delta":"first "},{"type":"reasoning-start","id":"reasoning-1"},{"type":"reasoning-delta","id":"reasoning-1","delta":"thinking "},{"type":"start-step"},{"type":"finish-step"},{"type":"text-delta","id":"text-1","delta":"second"},{"type":"reasoning-delta","id":"reasoning-1","delta":"continued"},{"type":"text-end","id":"text-1"},{"type":"reasoning-end","id":"reasoning-1"}]
            """);

        ProcessUIMessageStreamHarness.AssertJson("""
            [
              {
                "type": "text",
                "text": "first second",
                "state": "done"
              },
              {
                "type": "reasoning",
                "id": "reasoning-1",
                "text": "thinking continued",
                "state": "done"
              },
              {
                "type": "step-start"
              }
            ]
            """, harness.Parts);
        ProcessUIMessageStreamHarness.AssertJson("""
            {}
            """, harness.ActiveTextParts);
        ProcessUIMessageStreamHarness.AssertJson("""
            {}
            """, harness.ActiveReasoningParts);
    }
}
