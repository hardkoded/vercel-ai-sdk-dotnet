// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>message metadata</c>.</summary>
public sealed class MessageMetadataTests
{
    private const string Prefix = "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(Prefix + "message metadata::should call the update function with the correct arguments", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_call_the_update_function_with_the_correct_arguments()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123","messageMetadata":{"start":"start-1","shared":{"key1":"value-1a","key2":"value-2a"}}},{"type":"start-step"},{"type":"text-start","id":"text-1"},{"type":"text-delta","id":"text-1","delta":"t1"},{"type":"message-metadata","messageMetadata":{"metadata":"metadata-1"}},{"type":"text-delta","id":"text-1","delta":"t2"},{"type":"text-end","id":"text-1"},{"type":"finish-step"},{"type":"finish","messageMetadata":{"finish":"finish-1","shared":{"key1":"value-1e","key6":"value-6e"}}}]
            """);

        ProcessUIMessageStreamHarness.AssertJson("""
            [
              {
                "message": {
                  "id": "msg-123",
                  "metadata": {
                    "shared": {
                      "key1": "value-1a",
                      "key2": "value-2a"
                    },
                    "start": "start-1"
                  },
                  "parts": [],
                  "role": "assistant"
                }
              },
              {
                "message": {
                  "id": "msg-123",
                  "metadata": {
                    "shared": {
                      "key1": "value-1a",
                      "key2": "value-2a"
                    },
                    "start": "start-1"
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
                    "shared": {
                      "key1": "value-1a",
                      "key2": "value-2a"
                    },
                    "start": "start-1"
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
                    "metadata": "metadata-1",
                    "shared": {
                      "key1": "value-1a",
                      "key2": "value-2a"
                    },
                    "start": "start-1"
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
                    "metadata": "metadata-1",
                    "shared": {
                      "key1": "value-1a",
                      "key2": "value-2a"
                    },
                    "start": "start-1"
                  },
                  "parts": [
                    {
                      "type": "step-start"
                    },
                    {
                      "state": "streaming",
                      "text": "t1t2",
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
                    "metadata": "metadata-1",
                    "shared": {
                      "key1": "value-1a",
                      "key2": "value-2a"
                    },
                    "start": "start-1"
                  },
                  "parts": [
                    {
                      "type": "step-start"
                    },
                    {
                      "state": "done",
                      "text": "t1t2",
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
                    "finish": "finish-1",
                    "metadata": "metadata-1",
                    "shared": {
                      "key1": "value-1e",
                      "key2": "value-2a",
                      "key6": "value-6e"
                    },
                    "start": "start-1"
                  },
                  "parts": [
                    {
                      "type": "step-start"
                    },
                    {
                      "state": "done",
                      "text": "t1t2",
                      "type": "text"
                    }
                  ],
                  "role": "assistant"
                }
              }
            ]
            """, harness.WriteCalls);
    }

    [Fact]
    [UpstreamTest(Prefix + "message metadata::should have the correct final message state", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_the_correct_final_message_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123","messageMetadata":{"start":"start-1","shared":{"key1":"value-1a","key2":"value-2a"}}},{"type":"start-step"},{"type":"text-start","id":"text-1"},{"type":"text-delta","id":"text-1","delta":"t1"},{"type":"message-metadata","messageMetadata":{"metadata":"metadata-1"}},{"type":"text-delta","id":"text-1","delta":"t2"},{"type":"text-end","id":"text-1"},{"type":"finish-step"},{"type":"finish","messageMetadata":{"finish":"finish-1","shared":{"key1":"value-1e","key6":"value-6e"}}}]
            """);

        ProcessUIMessageStreamHarness.AssertJson("""
            {
              "id": "msg-123",
              "metadata": {
                "finish": "finish-1",
                "metadata": "metadata-1",
                "shared": {
                  "key1": "value-1e",
                  "key2": "value-2a",
                  "key6": "value-6e"
                },
                "start": "start-1"
              },
              "parts": [
                {
                  "type": "step-start"
                },
                {
                  "state": "done",
                  "text": "t1t2",
                  "type": "text"
                }
              ],
              "role": "assistant"
            }
            """, harness.Message);
    }
}
