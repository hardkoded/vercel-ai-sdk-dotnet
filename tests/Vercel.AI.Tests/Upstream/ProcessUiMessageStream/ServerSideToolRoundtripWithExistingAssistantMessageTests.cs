// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>server-side tool roundtrip with existing assistant message</c>.</summary>
public sealed class ServerSideToolRoundtripWithExistingAssistantMessageTests
{
    private const string Prefix = "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(Prefix + "server-side tool roundtrip with existing assistant message::should call the update function with the correct arguments", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_call_the_update_function_with_the_correct_arguments()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", """
            {"role":"assistant","id":"original-id","parts":[{"type":"tool-tool-name-original","toolCallId":"tool-call-id-original","state":"output-available","input":{},"output":{"location":"Berlin"}}]}
            """);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-available","toolCallId":"tool-call-id","toolName":"tool-name","input":{"city":"London"}},{"type":"tool-output-available","toolCallId":"tool-call-id","output":{"weather":"sunny"}},{"type":"finish-step"},{"type":"start-step"},{"type":"text-start","id":"text-1"},{"type":"text-delta","id":"text-1","delta":"The weather in London is sunny."},{"type":"text-end","id":"text-1"},{"type":"finish-step"},{"type":"finish"}]
            """);

        ProcessUIMessageStreamHarness.AssertJson("""
            [
              {
                "message": {
                  "id": "msg-123",
                  "parts": [
                    {
                      "input": {},
                      "output": {
                        "location": "Berlin"
                      },
                      "state": "output-available",
                      "toolCallId": "tool-call-id-original",
                      "type": "tool-tool-name-original"
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
                      "input": {},
                      "output": {
                        "location": "Berlin"
                      },
                      "state": "output-available",
                      "toolCallId": "tool-call-id-original",
                      "type": "tool-tool-name-original"
                    },
                    {
                      "type": "step-start"
                    },
                    {
                      "input": {
                        "city": "London"
                      },
                      "state": "input-available",
                      "toolCallId": "tool-call-id",
                      "type": "tool-tool-name"
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
                      "input": {},
                      "output": {
                        "location": "Berlin"
                      },
                      "state": "output-available",
                      "toolCallId": "tool-call-id-original",
                      "type": "tool-tool-name-original"
                    },
                    {
                      "type": "step-start"
                    },
                    {
                      "input": {
                        "city": "London"
                      },
                      "output": {
                        "weather": "sunny"
                      },
                      "state": "output-available",
                      "toolCallId": "tool-call-id",
                      "type": "tool-tool-name"
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
                      "input": {},
                      "output": {
                        "location": "Berlin"
                      },
                      "state": "output-available",
                      "toolCallId": "tool-call-id-original",
                      "type": "tool-tool-name-original"
                    },
                    {
                      "type": "step-start"
                    },
                    {
                      "input": {
                        "city": "London"
                      },
                      "output": {
                        "weather": "sunny"
                      },
                      "state": "output-available",
                      "toolCallId": "tool-call-id",
                      "type": "tool-tool-name"
                    },
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
                      "input": {},
                      "output": {
                        "location": "Berlin"
                      },
                      "state": "output-available",
                      "toolCallId": "tool-call-id-original",
                      "type": "tool-tool-name-original"
                    },
                    {
                      "type": "step-start"
                    },
                    {
                      "input": {
                        "city": "London"
                      },
                      "output": {
                        "weather": "sunny"
                      },
                      "state": "output-available",
                      "toolCallId": "tool-call-id",
                      "type": "tool-tool-name"
                    },
                    {
                      "type": "step-start"
                    },
                    {
                      "state": "streaming",
                      "text": "The weather in London is sunny.",
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
                      "input": {},
                      "output": {
                        "location": "Berlin"
                      },
                      "state": "output-available",
                      "toolCallId": "tool-call-id-original",
                      "type": "tool-tool-name-original"
                    },
                    {
                      "type": "step-start"
                    },
                    {
                      "input": {
                        "city": "London"
                      },
                      "output": {
                        "weather": "sunny"
                      },
                      "state": "output-available",
                      "toolCallId": "tool-call-id",
                      "type": "tool-tool-name"
                    },
                    {
                      "type": "step-start"
                    },
                    {
                      "state": "done",
                      "text": "The weather in London is sunny.",
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
    [UpstreamTest(Prefix + "server-side tool roundtrip with existing assistant message::should have the correct final message state", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_the_correct_final_message_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", """
            {"role":"assistant","id":"original-id","parts":[{"type":"tool-tool-name-original","toolCallId":"tool-call-id-original","state":"output-available","input":{},"output":{"location":"Berlin"}}]}
            """);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-available","toolCallId":"tool-call-id","toolName":"tool-name","input":{"city":"London"}},{"type":"tool-output-available","toolCallId":"tool-call-id","output":{"weather":"sunny"}},{"type":"finish-step"},{"type":"start-step"},{"type":"text-start","id":"text-1"},{"type":"text-delta","id":"text-1","delta":"The weather in London is sunny."},{"type":"text-end","id":"text-1"},{"type":"finish-step"},{"type":"finish"}]
            """);

        ProcessUIMessageStreamHarness.AssertJson("""
            {
              "id": "msg-123",
              "parts": [
                {
                  "input": {},
                  "output": {
                    "location": "Berlin"
                  },
                  "state": "output-available",
                  "toolCallId": "tool-call-id-original",
                  "type": "tool-tool-name-original"
                },
                {
                  "type": "step-start"
                },
                {
                  "input": {
                    "city": "London"
                  },
                  "output": {
                    "weather": "sunny"
                  },
                  "state": "output-available",
                  "toolCallId": "tool-call-id",
                  "type": "tool-tool-name"
                },
                {
                  "type": "step-start"
                },
                {
                  "state": "done",
                  "text": "The weather in London is sunny.",
                  "type": "text"
                }
              ],
              "role": "assistant"
            }
            """, harness.Message);
    }
}
