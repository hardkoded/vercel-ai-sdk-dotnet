// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>data ui parts (single part with id and merge update)</c>.</summary>
public sealed class DataUiPartsSinglePartWithIdAndMergeUpdateTests
{
    private const string Prefix =
        "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(
        Prefix
            + "data ui parts (single part with id and merge update)::should call the update function with the correct arguments",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_the_update_function_with_the_correct_arguments()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"data-test","id":"data-part-id","data":{"a":"a1","b":"b1"}},{"type":"data-test","id":"data-part-id","data":{"b":"b2","c":"c2"}},{"type":"finish-step"},{"type":"finish"}]
            """
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
              },
              {
                "message": {
                  "id": "msg-123",
                  "parts": [
                    {
                      "type": "step-start"
                    },
                    {
                      "data": {
                        "a": "a1",
                        "b": "b1"
                      },
                      "id": "data-part-id",
                      "type": "data-test"
                    }
                  ],
                  "role": "assistant"
                }
              },
              {
                "message": {
                  "id": "msg-123",
                  "parts": [
                    {
                      "type": "step-start"
                    },
                    {
                      "data": {
                        "b": "b2",
                        "c": "c2"
                      },
                      "id": "data-part-id",
                      "type": "data-test"
                    }
                  ],
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
            + "data ui parts (single part with id and merge update)::should have the correct final message state",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_have_the_correct_final_message_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"data-test","id":"data-part-id","data":{"a":"a1","b":"b1"}},{"type":"data-test","id":"data-part-id","data":{"b":"b2","c":"c2"}},{"type":"finish-step"},{"type":"finish"}]
            """
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            {
              "id": "msg-123",
              "parts": [
                {
                  "type": "step-start"
                },
                {
                  "data": {
                    "b": "b2",
                    "c": "c2"
                  },
                  "id": "data-part-id",
                  "type": "data-test"
                }
              ],
              "role": "assistant"
            }
            """,
            harness.Message
        );
    }
}
