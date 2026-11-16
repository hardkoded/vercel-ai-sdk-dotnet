// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>tool call streaming</c>.</summary>
public sealed class ToolCallStreamingTests
{
    private const string Prefix = "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(Prefix + "tool call streaming::should call the update function with the correct arguments", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_call_the_update_function_with_the_correct_arguments()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-start","toolCallId":"tool-call-0","toolName":"test-tool"},{"type":"tool-input-delta","toolCallId":"tool-call-0","inputTextDelta":"{\"testArg\":\"t"},{"type":"tool-input-delta","toolCallId":"tool-call-0","inputTextDelta":"est-value\"}}"},{"type":"tool-input-available","toolCallId":"tool-call-0","toolName":"test-tool","input":{"testArg":"test-value"}},{"type":"tool-output-available","toolCallId":"tool-call-0","output":"test-result"},{"type":"finish-step"},{"type":"finish"}]
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
                      "state": "input-streaming",
                      "toolCallId": "tool-call-0",
                      "type": "tool-test-tool"
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
                      "input": {
                        "testArg": "t"
                      },
                      "rawInput": "{\"testArg\":\"t",
                      "state": "input-streaming",
                      "toolCallId": "tool-call-0",
                      "type": "tool-test-tool"
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
                      "input": {
                        "testArg": "test-value"
                      },
                      "rawInput": "{\"testArg\":\"test-value\"}}",
                      "state": "input-streaming",
                      "toolCallId": "tool-call-0",
                      "type": "tool-test-tool"
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
                      "input": {
                        "testArg": "test-value"
                      },
                      "state": "input-available",
                      "toolCallId": "tool-call-0",
                      "type": "tool-test-tool"
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
                      "input": {
                        "testArg": "test-value"
                      },
                      "output": "test-result",
                      "state": "output-available",
                      "toolCallId": "tool-call-0",
                      "type": "tool-test-tool"
                    }
                  ],
                  "role": "assistant"
                }
              }
            ]
            """, harness.WriteCalls);
    }

    [Fact]
    [UpstreamTest(Prefix + "tool call streaming::should have the correct final message state", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_the_correct_final_message_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-start","toolCallId":"tool-call-0","toolName":"test-tool"},{"type":"tool-input-delta","toolCallId":"tool-call-0","inputTextDelta":"{\"testArg\":\"t"},{"type":"tool-input-delta","toolCallId":"tool-call-0","inputTextDelta":"est-value\"}}"},{"type":"tool-input-available","toolCallId":"tool-call-0","toolName":"test-tool","input":{"testArg":"test-value"}},{"type":"tool-output-available","toolCallId":"tool-call-0","output":"test-result"},{"type":"finish-step"},{"type":"finish"}]
            """);

        ProcessUIMessageStreamHarness.AssertJson("""
            {
              "id": "msg-123",
              "parts": [
                {
                  "type": "step-start"
                },
                {
                  "input": {
                    "testArg": "test-value"
                  },
                  "output": "test-result",
                  "state": "output-available",
                  "toolCallId": "tool-call-0",
                  "type": "tool-test-tool"
                }
              ],
              "role": "assistant"
            }
            """, harness.Message);
    }
}
