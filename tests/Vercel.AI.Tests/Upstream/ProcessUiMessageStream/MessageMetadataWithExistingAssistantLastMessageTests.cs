// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>message metadata with existing assistant lastMessage</c>.</summary>
public sealed class MessageMetadataWithExistingAssistantLastMessageTests
{
    private const string Prefix =
        "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(
        Prefix
            + "message metadata with existing assistant lastMessage::should call the update function with the correct arguments",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_the_update_function_with_the_correct_arguments()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState(
            "msg-123",
            """
            {"role":"assistant","id":"original-id","metadata":{"key1":"value-1a","key3":"value-3a"},"parts":[]}
            """
        );
        await harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123","messageMetadata":{"key1":"value-1b","key2":"value-2b"}},{"type":"start-step"},{"type":"text-start","id":"text-1"},{"type":"text-delta","id":"text-1","delta":"t1"},{"type":"text-end","id":"text-1"},{"type":"finish-step"},{"type":"finish"}]
            """
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            [
              {
                "message": {
                  "id": "msg-123",
                  "metadata": {
                    "key1": "value-1b",
                    "key2": "value-2b",
                    "key3": "value-3a"
                  },
                  "parts": [],
                  "role": "assistant"
                }
              },
              {
                "message": {
                  "id": "msg-123",
                  "metadata": {
                    "key1": "value-1b",
                    "key2": "value-2b",
                    "key3": "value-3a"
                  },
                  "parts": [
                    {
                      "type": "step-start"
                    },
                    {
                      "state": "streaming",
                      "text": "",
                      "type": "text"
                    }
                  ],
                  "role": "assistant"
                }
              },
              {
                "message": {
                  "id": "msg-123",
                  "metadata": {
                    "key1": "value-1b",
                    "key2": "value-2b",
                    "key3": "value-3a"
                  },
                  "parts": [
                    {
                      "type": "step-start"
                    },
                    {
                      "state": "streaming",
                      "text": "t1",
                      "type": "text"
                    }
                  ],
                  "role": "assistant"
                }
              },
              {
                "message": {
                  "id": "msg-123",
                  "metadata": {
                    "key1": "value-1b",
                    "key2": "value-2b",
                    "key3": "value-3a"
                  },
                  "parts": [
                    {
                      "type": "step-start"
                    },
                    {
                      "state": "done",
                      "text": "t1",
                      "type": "text"
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
            + "message metadata with existing assistant lastMessage::should have the correct final message state",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_have_the_correct_final_message_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState(
            "msg-123",
            """
            {"role":"assistant","id":"original-id","metadata":{"key1":"value-1a","key3":"value-3a"},"parts":[]}
            """
        );
        await harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123","messageMetadata":{"key1":"value-1b","key2":"value-2b"}},{"type":"start-step"},{"type":"text-start","id":"text-1"},{"type":"text-delta","id":"text-1","delta":"t1"},{"type":"text-end","id":"text-1"},{"type":"finish-step"},{"type":"finish"}]
            """
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            {
              "id": "msg-123",
              "metadata": {
                "key1": "value-1b",
                "key2": "value-2b",
                "key3": "value-3a"
              },
              "parts": [
                {
                  "type": "step-start"
                },
                {
                  "state": "done",
                  "text": "t1",
                  "type": "text"
                }
              ],
              "role": "assistant"
            }
            """,
            harness.Message
        );
    }
}
