// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>steering-message</c>.</summary>
public sealed class SteeringMessageTests
{
    private const string Prefix = "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(Prefix + "steering-message::adds steering messages before the step that received them and keeps them when the step is reset", Coverage = UpstreamCoverage.Covered, Commit = "8ba3940d3dd1f3cd3c4365bbaec505c6ef0eb1e3")]
    public async Task Adds_steering_messages_before_the_step_that_received_them_and_keeps_them_when_the_step_is_reset()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start-step"},{"type":"text-start","id":"text-1"},{"type":"text-delta","id":"text-1","delta":"It is sunny."},{"type":"text-end","id":"text-1"},{"type":"finish-step"},{"type":"steering-message","id":"steer-1","parts":[{"type":"text","text":"I meant Paris, Texas."}]},{"type":"start-step"},{"type":"text-start","id":"stale-text"},{"type":"text-delta","id":"stale-text","delta":"It is"},{"type":"reset-step"},{"type":"text-start","id":"text-2"},{"type":"text-delta","id":"text-2","delta":"It is hot in Texas."},{"type":"text-end","id":"text-2"},{"type":"finish-step"}]
            """);

        ProcessUIMessageStreamHarness.AssertJson("""
            [
              {
                "type": "step-start"
              },
              {
                "state": "done",
                "text": "It is sunny.",
                "type": "text"
              },
              {
                "id": "steer-1",
                "parts": [
                  {
                    "text": "I meant Paris, Texas.",
                    "type": "text"
                  }
                ],
                "type": "steering-message"
              },
              {
                "type": "step-start"
              },
              {
                "state": "done",
                "text": "It is hot in Texas.",
                "type": "text"
              }
            ]
            """, harness.Parts);
    }
}
