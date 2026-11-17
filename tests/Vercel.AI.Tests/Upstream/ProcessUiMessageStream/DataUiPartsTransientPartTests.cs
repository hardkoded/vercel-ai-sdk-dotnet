// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>data ui parts (transient part)</c>.</summary>
public sealed class DataUiPartsTransientPartTests
{
    private const string Prefix =
        "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(
        Prefix
            + "data ui parts (transient part)::should not call the update function with the transient part",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_call_the_update_function_with_the_transient_part()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        var dataCalls = new JsonArray();
        await harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"data-test","data":"example-data-can-be-anything","transient":true},{"type":"finish-step"},{"type":"finish"}]
            """,
            new ProcessUIMessageStreamOptions(harness.RunUpdateMessageJob, error => throw error)
            {
                OnData = data => dataCalls.Add(data.DeepClone()),
            }
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            [
              {
                "message": {
                  "id": "msg-123",
                  "parts": [],
                  "role": "assistant"
                }
              }
            ]
            """,
            harness.WriteCalls
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "data ui parts (transient part)::should not have the transient part in the final message state",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_have_the_transient_part_in_the_final_message_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        var dataCalls = new JsonArray();
        await harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"data-test","data":"example-data-can-be-anything","transient":true},{"type":"finish-step"},{"type":"finish"}]
            """,
            new ProcessUIMessageStreamOptions(harness.RunUpdateMessageJob, error => throw error)
            {
                OnData = data => dataCalls.Add(data.DeepClone()),
            }
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            {
              "id": "msg-123",
              "parts": [
                {
                  "type": "step-start"
                }
              ],
              "role": "assistant"
            }
            """,
            harness.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "data ui parts (transient part)::should call the onData callback with the transient part",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_the_onData_callback_with_the_transient_part()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        var dataCalls = new JsonArray();
        await harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"data-test","data":"example-data-can-be-anything","transient":true},{"type":"finish-step"},{"type":"finish"}]
            """,
            new ProcessUIMessageStreamOptions(harness.RunUpdateMessageJob, error => throw error)
            {
                OnData = data => dataCalls.Add(data.DeepClone()),
            }
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            [
              {
                "data": "example-data-can-be-anything",
                "transient": true,
                "type": "data-test"
              }
            ]
            """,
            dataCalls
        );
    }
}
