// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>file parts</c>.</summary>
public sealed class FilePartsTests
{
    private const string Prefix = "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(Prefix + "file parts::should call the update function with the correct arguments", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_call_the_update_function_with_the_correct_arguments()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"text-start","id":"text-1"},{"type":"text-delta","id":"text-1","delta":"Here is a file:"},{"type":"text-end","id":"text-1"},{"type":"file","url":"data:text/plain;base64,SGVsbG8gV29ybGQ=","mediaType":"text/plain"},{"type":"text-start","id":"text-2"},{"type":"text-delta","id":"text-2","delta":"And another one:"},{"type":"text-end","id":"text-2"},{"type":"file","url":"data:application/json;base64,eyJrZXkiOiJ2YWx1ZSJ9","mediaType":"application/json"},{"type":"finish-step"},{"type":"finish"}]
            """);

        ProcessUIMessageStreamHarness.AssertJson("""
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
                  "parts": [
                    {
                      "type": "step-start"
                    },
                    {
                      "state": "streaming",
                      "text": "Here is a file:",
                      "type": "text"
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
                      "state": "done",
                      "text": "Here is a file:",
                      "type": "text"
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
                      "state": "done",
                      "text": "Here is a file:",
                      "type": "text"
                    },
                    {
                      "mediaType": "text/plain",
                      "type": "file",
                      "url": "data:text/plain;base64,SGVsbG8gV29ybGQ="
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
                      "state": "done",
                      "text": "Here is a file:",
                      "type": "text"
                    },
                    {
                      "mediaType": "text/plain",
                      "type": "file",
                      "url": "data:text/plain;base64,SGVsbG8gV29ybGQ="
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
                  "parts": [
                    {
                      "type": "step-start"
                    },
                    {
                      "state": "done",
                      "text": "Here is a file:",
                      "type": "text"
                    },
                    {
                      "mediaType": "text/plain",
                      "type": "file",
                      "url": "data:text/plain;base64,SGVsbG8gV29ybGQ="
                    },
                    {
                      "state": "streaming",
                      "text": "And another one:",
                      "type": "text"
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
                      "state": "done",
                      "text": "Here is a file:",
                      "type": "text"
                    },
                    {
                      "mediaType": "text/plain",
                      "type": "file",
                      "url": "data:text/plain;base64,SGVsbG8gV29ybGQ="
                    },
                    {
                      "state": "done",
                      "text": "And another one:",
                      "type": "text"
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
                      "state": "done",
                      "text": "Here is a file:",
                      "type": "text"
                    },
                    {
                      "mediaType": "text/plain",
                      "type": "file",
                      "url": "data:text/plain;base64,SGVsbG8gV29ybGQ="
                    },
                    {
                      "state": "done",
                      "text": "And another one:",
                      "type": "text"
                    },
                    {
                      "mediaType": "application/json",
                      "type": "file",
                      "url": "data:application/json;base64,eyJrZXkiOiJ2YWx1ZSJ9"
                    }
                  ],
                  "role": "assistant"
                }
              }
            ]
            """, harness.WriteCalls);
    }

    [Fact]
    [UpstreamTest(Prefix + "file parts::should have the correct final message state", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_the_correct_final_message_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"text-start","id":"text-1"},{"type":"text-delta","id":"text-1","delta":"Here is a file:"},{"type":"text-end","id":"text-1"},{"type":"file","url":"data:text/plain;base64,SGVsbG8gV29ybGQ=","mediaType":"text/plain"},{"type":"text-start","id":"text-2"},{"type":"text-delta","id":"text-2","delta":"And another one:"},{"type":"text-end","id":"text-2"},{"type":"file","url":"data:application/json;base64,eyJrZXkiOiJ2YWx1ZSJ9","mediaType":"application/json"},{"type":"finish-step"},{"type":"finish"}]
            """);

        ProcessUIMessageStreamHarness.AssertJson("""
            {
              "id": "msg-123",
              "parts": [
                {
                  "type": "step-start"
                },
                {
                  "state": "done",
                  "text": "Here is a file:",
                  "type": "text"
                },
                {
                  "mediaType": "text/plain",
                  "type": "file",
                  "url": "data:text/plain;base64,SGVsbG8gV29ybGQ="
                },
                {
                  "state": "done",
                  "text": "And another one:",
                  "type": "text"
                },
                {
                  "mediaType": "application/json",
                  "type": "file",
                  "url": "data:application/json;base64,eyJrZXkiOiJ2YWx1ZSJ9"
                }
              ],
              "role": "assistant"
            }
            """, harness.Message);
    }
}
