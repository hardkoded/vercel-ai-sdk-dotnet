// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>initial tool execution after approval (static tool)</c>.</summary>
public sealed class InitialToolExecutionAfterApprovalStaticToolTests
{
    private const string Prefix = "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(Prefix + "initial tool execution after approval (static tool)::should call the update function with the correct arguments", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_call_the_update_function_with_the_correct_arguments()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", """
            {"role":"assistant","id":"original-id","parts":[{"type":"step-start"},{"approval":{"id":"id-1","approved":true},"input":{"value":"value"},"state":"approval-responded","toolCallId":"call-1","type":"tool-tool1"}]}
            """);
        await harness.ProcessAsync("""
            [{"type":"start"},{"output":"result1","toolCallId":"call-1","type":"tool-output-available"},{"type":"start-step"},{"id":"1","type":"text-start"},{"delta":"Hello, world!","id":"1","type":"text-delta"},{"id":"1","type":"text-end"},{"type":"finish-step"},{"type":"finish"}]
            """);

        ProcessUIMessageStreamHarness.AssertJson("""
            [
              {
                "message": {
                  "id": "original-id",
                  "parts": [
                    {
                      "type": "step-start"
                    },
                    {
                      "approval": {
                        "approved": true,
                        "id": "id-1"
                      },
                      "input": {
                        "value": "value"
                      },
                      "output": "result1",
                      "state": "output-available",
                      "toolCallId": "call-1",
                      "type": "tool-tool1"
                    }
                  ],
                  "role": "assistant"
                }
              },
              {
                "message": {
                  "id": "original-id",
                  "parts": [
                    {
                      "type": "step-start"
                    },
                    {
                      "approval": {
                        "approved": true,
                        "id": "id-1"
                      },
                      "input": {
                        "value": "value"
                      },
                      "output": "result1",
                      "state": "output-available",
                      "toolCallId": "call-1",
                      "type": "tool-tool1"
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
                  "id": "original-id",
                  "parts": [
                    {
                      "type": "step-start"
                    },
                    {
                      "approval": {
                        "approved": true,
                        "id": "id-1"
                      },
                      "input": {
                        "value": "value"
                      },
                      "output": "result1",
                      "state": "output-available",
                      "toolCallId": "call-1",
                      "type": "tool-tool1"
                    },
                    {
                      "type": "step-start"
                    },
                    {
                      "state": "streaming",
                      "text": "Hello, world!",
                      "type": "text"
                    }
                  ],
                  "role": "assistant"
                }
              },
              {
                "message": {
                  "id": "original-id",
                  "parts": [
                    {
                      "type": "step-start"
                    },
                    {
                      "approval": {
                        "approved": true,
                        "id": "id-1"
                      },
                      "input": {
                        "value": "value"
                      },
                      "output": "result1",
                      "state": "output-available",
                      "toolCallId": "call-1",
                      "type": "tool-tool1"
                    },
                    {
                      "type": "step-start"
                    },
                    {
                      "state": "done",
                      "text": "Hello, world!",
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
    [UpstreamTest(Prefix + "initial tool execution after approval (static tool)::should have the correct final message state", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_the_correct_final_message_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", """
            {"role":"assistant","id":"original-id","parts":[{"type":"step-start"},{"approval":{"id":"id-1","approved":true},"input":{"value":"value"},"state":"approval-responded","toolCallId":"call-1","type":"tool-tool1"}]}
            """);
        await harness.ProcessAsync("""
            [{"type":"start"},{"output":"result1","toolCallId":"call-1","type":"tool-output-available"},{"type":"start-step"},{"id":"1","type":"text-start"},{"delta":"Hello, world!","id":"1","type":"text-delta"},{"id":"1","type":"text-end"},{"type":"finish-step"},{"type":"finish"}]
            """);

        ProcessUIMessageStreamHarness.AssertJson("""
            [
              {
                "type": "step-start"
              },
              {
                "approval": {
                  "approved": true,
                  "id": "id-1"
                },
                "input": {
                  "value": "value"
                },
                "output": "result1",
                "state": "output-available",
                "toolCallId": "call-1",
                "type": "tool-tool1"
              },
              {
                "type": "step-start"
              },
              {
                "state": "done",
                "text": "Hello, world!",
                "type": "text"
              }
            ]
            """, harness.Parts);
    }
}
