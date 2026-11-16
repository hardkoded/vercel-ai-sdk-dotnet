// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>dynamic tools</c>.</summary>
public sealed class DynamicToolsTests
{
    private const string Prefix = "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(Prefix + "dynamic tools::should invoke onToolCall for dynamic tools", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_invoke_onToolCall_for_dynamic_tools()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        var onToolCallInvoked = false;
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-start","toolCallId":"tool-call-1","toolName":"t1","dynamic":true},{"type":"tool-input-delta","toolCallId":"tool-call-1","inputTextDelta":"{ \"query\": \"test\" }"},{"type":"tool-input-available","toolCallId":"tool-call-1","toolName":"t1","input":{"query":"test"},"dynamic":true},{"type":"tool-input-available","toolCallId":"tool-call-2","toolName":"t1","input":{"query":"test"},"dynamic":true},{"type":"tool-output-available","toolCallId":"tool-call-1","output":{"result":"provider-result"},"dynamic":true},{"type":"tool-output-error","toolCallId":"tool-call-2","errorText":"error-text","dynamic":true},{"type":"finish-step"},{"type":"finish"}]
            """, new ProcessUIMessageStreamOptions(harness.RunUpdateMessageJob, error => throw error) { OnToolCall = _ => { onToolCallInvoked = true; return Task.CompletedTask; } });

        Assert.True(onToolCallInvoked);
    }

    [Fact]
    [UpstreamTest(Prefix + "dynamic tools::should call the update function with the correct arguments", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_call_the_update_function_with_the_correct_arguments()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-start","toolCallId":"tool-call-1","toolName":"t1","dynamic":true},{"type":"tool-input-delta","toolCallId":"tool-call-1","inputTextDelta":"{ \"query\": \"test\" }"},{"type":"tool-input-available","toolCallId":"tool-call-1","toolName":"t1","input":{"query":"test"},"dynamic":true},{"type":"tool-input-available","toolCallId":"tool-call-2","toolName":"t1","input":{"query":"test"},"dynamic":true},{"type":"tool-output-available","toolCallId":"tool-call-1","output":{"result":"provider-result"},"dynamic":true},{"type":"tool-output-error","toolCallId":"tool-call-2","errorText":"error-text","dynamic":true},{"type":"finish-step"},{"type":"finish"}]
            """, new ProcessUIMessageStreamOptions(harness.RunUpdateMessageJob, error => throw error) { OnToolCall = _ => Task.CompletedTask });

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
                      "toolCallId": "tool-call-1",
                      "toolName": "t1",
                      "type": "dynamic-tool"
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
                        "query": "test"
                      },
                      "rawInput": "{ \"query\": \"test\" }",
                      "state": "input-streaming",
                      "toolCallId": "tool-call-1",
                      "toolName": "t1",
                      "type": "dynamic-tool"
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
                        "query": "test"
                      },
                      "state": "input-available",
                      "toolCallId": "tool-call-1",
                      "toolName": "t1",
                      "type": "dynamic-tool"
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
                        "query": "test"
                      },
                      "state": "input-available",
                      "toolCallId": "tool-call-1",
                      "toolName": "t1",
                      "type": "dynamic-tool"
                    },
                    {
                      "input": {
                        "query": "test"
                      },
                      "state": "input-available",
                      "toolCallId": "tool-call-2",
                      "toolName": "t1",
                      "type": "dynamic-tool"
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
                        "query": "test"
                      },
                      "output": {
                        "result": "provider-result"
                      },
                      "state": "output-available",
                      "toolCallId": "tool-call-1",
                      "toolName": "t1",
                      "type": "dynamic-tool"
                    },
                    {
                      "input": {
                        "query": "test"
                      },
                      "state": "input-available",
                      "toolCallId": "tool-call-2",
                      "toolName": "t1",
                      "type": "dynamic-tool"
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
                        "query": "test"
                      },
                      "output": {
                        "result": "provider-result"
                      },
                      "state": "output-available",
                      "toolCallId": "tool-call-1",
                      "toolName": "t1",
                      "type": "dynamic-tool"
                    },
                    {
                      "errorText": "error-text",
                      "input": {
                        "query": "test"
                      },
                      "state": "output-error",
                      "toolCallId": "tool-call-2",
                      "toolName": "t1",
                      "type": "dynamic-tool"
                    }
                  ],
                  "role": "assistant"
                }
              }
            ]
            """, harness.WriteCalls);
    }

    [Fact]
    [UpstreamTest(Prefix + "dynamic tools::should have the correct final message state", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_the_correct_final_message_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-start","toolCallId":"tool-call-1","toolName":"t1","dynamic":true},{"type":"tool-input-delta","toolCallId":"tool-call-1","inputTextDelta":"{ \"query\": \"test\" }"},{"type":"tool-input-available","toolCallId":"tool-call-1","toolName":"t1","input":{"query":"test"},"dynamic":true},{"type":"tool-input-available","toolCallId":"tool-call-2","toolName":"t1","input":{"query":"test"},"dynamic":true},{"type":"tool-output-available","toolCallId":"tool-call-1","output":{"result":"provider-result"},"dynamic":true},{"type":"tool-output-error","toolCallId":"tool-call-2","errorText":"error-text","dynamic":true},{"type":"finish-step"},{"type":"finish"}]
            """, new ProcessUIMessageStreamOptions(harness.RunUpdateMessageJob, error => throw error) { OnToolCall = _ => Task.CompletedTask });

        ProcessUIMessageStreamHarness.AssertJson("""
            [
              {
                "type": "step-start"
              },
              {
                "input": {
                  "query": "test"
                },
                "output": {
                  "result": "provider-result"
                },
                "state": "output-available",
                "toolCallId": "tool-call-1",
                "toolName": "t1",
                "type": "dynamic-tool"
              },
              {
                "errorText": "error-text",
                "input": {
                  "query": "test"
                },
                "state": "output-error",
                "toolCallId": "tool-call-2",
                "toolName": "t1",
                "type": "dynamic-tool"
              }
            ]
            """, harness.Parts);
    }
}
