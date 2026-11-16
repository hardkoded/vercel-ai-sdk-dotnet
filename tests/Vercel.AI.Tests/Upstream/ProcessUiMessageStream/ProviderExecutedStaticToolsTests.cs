// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>provider-executed static tools</c>.</summary>
public sealed class ProviderExecutedStaticToolsTests
{
    private const string Prefix = "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(Prefix + "provider-executed static tools::should not call onToolCall", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_call_onToolCall()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        var onToolCallInvoked = false;
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-start","toolCallId":"tool-call-1","toolName":"tool-name","providerExecuted":true},{"type":"tool-input-delta","toolCallId":"tool-call-1","inputTextDelta":"{ \"query\": \"test\" }"},{"type":"tool-input-available","toolCallId":"tool-call-1","toolName":"tool-name","input":{"query":"test"},"providerExecuted":true},{"type":"tool-output-available","toolCallId":"tool-call-1","output":{"result":"provider-result"},"providerExecuted":true},{"type":"tool-input-available","toolCallId":"tool-call-2","toolName":"tool-name","input":{"query":"test"},"providerExecuted":true},{"type":"tool-output-error","toolCallId":"tool-call-2","errorText":"error-text","providerExecuted":true},{"type":"finish-step"},{"type":"finish"}]
            """, new ProcessUIMessageStreamOptions(harness.RunUpdateMessageJob, error => throw error) { OnToolCall = _ => { onToolCallInvoked = true; return Task.CompletedTask; } });

        Assert.False(onToolCallInvoked);
    }

    [Fact]
    [UpstreamTest(Prefix + "provider-executed static tools::should call the update function with the correct arguments", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_call_the_update_function_with_the_correct_arguments()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-start","toolCallId":"tool-call-1","toolName":"tool-name","providerExecuted":true},{"type":"tool-input-delta","toolCallId":"tool-call-1","inputTextDelta":"{ \"query\": \"test\" }"},{"type":"tool-input-available","toolCallId":"tool-call-1","toolName":"tool-name","input":{"query":"test"},"providerExecuted":true},{"type":"tool-output-available","toolCallId":"tool-call-1","output":{"result":"provider-result"},"providerExecuted":true},{"type":"tool-input-available","toolCallId":"tool-call-2","toolName":"tool-name","input":{"query":"test"},"providerExecuted":true},{"type":"tool-output-error","toolCallId":"tool-call-2","errorText":"error-text","providerExecuted":true},{"type":"finish-step"},{"type":"finish"}]
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
                      "providerExecuted": true,
                      "state": "input-streaming",
                      "toolCallId": "tool-call-1",
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
                      "type": "step-start"
                    },
                    {
                      "input": {
                        "query": "test"
                      },
                      "providerExecuted": true,
                      "rawInput": "{ \"query\": \"test\" }",
                      "state": "input-streaming",
                      "toolCallId": "tool-call-1",
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
                      "type": "step-start"
                    },
                    {
                      "input": {
                        "query": "test"
                      },
                      "providerExecuted": true,
                      "state": "input-available",
                      "toolCallId": "tool-call-1",
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
                      "type": "step-start"
                    },
                    {
                      "input": {
                        "query": "test"
                      },
                      "output": {
                        "result": "provider-result"
                      },
                      "providerExecuted": true,
                      "state": "output-available",
                      "toolCallId": "tool-call-1",
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
                      "type": "step-start"
                    },
                    {
                      "input": {
                        "query": "test"
                      },
                      "output": {
                        "result": "provider-result"
                      },
                      "providerExecuted": true,
                      "state": "output-available",
                      "toolCallId": "tool-call-1",
                      "type": "tool-tool-name"
                    },
                    {
                      "input": {
                        "query": "test"
                      },
                      "providerExecuted": true,
                      "state": "input-available",
                      "toolCallId": "tool-call-2",
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
                      "type": "step-start"
                    },
                    {
                      "input": {
                        "query": "test"
                      },
                      "output": {
                        "result": "provider-result"
                      },
                      "providerExecuted": true,
                      "state": "output-available",
                      "toolCallId": "tool-call-1",
                      "type": "tool-tool-name"
                    },
                    {
                      "errorText": "error-text",
                      "input": {
                        "query": "test"
                      },
                      "providerExecuted": true,
                      "state": "output-error",
                      "toolCallId": "tool-call-2",
                      "type": "tool-tool-name"
                    }
                  ],
                  "role": "assistant"
                }
              }
            ]
            """, harness.WriteCalls);
    }

    [Fact]
    [UpstreamTest(Prefix + "provider-executed static tools::should have the correct final message state", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_the_correct_final_message_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-start","toolCallId":"tool-call-1","toolName":"tool-name","providerExecuted":true},{"type":"tool-input-delta","toolCallId":"tool-call-1","inputTextDelta":"{ \"query\": \"test\" }"},{"type":"tool-input-available","toolCallId":"tool-call-1","toolName":"tool-name","input":{"query":"test"},"providerExecuted":true},{"type":"tool-output-available","toolCallId":"tool-call-1","output":{"result":"provider-result"},"providerExecuted":true},{"type":"tool-input-available","toolCallId":"tool-call-2","toolName":"tool-name","input":{"query":"test"},"providerExecuted":true},{"type":"tool-output-error","toolCallId":"tool-call-2","errorText":"error-text","providerExecuted":true},{"type":"finish-step"},{"type":"finish"}]
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
                "providerExecuted": true,
                "state": "output-available",
                "toolCallId": "tool-call-1",
                "type": "tool-tool-name"
              },
              {
                "errorText": "error-text",
                "input": {
                  "query": "test"
                },
                "providerExecuted": true,
                "state": "output-error",
                "toolCallId": "tool-call-2",
                "type": "tool-tool-name"
              }
            ]
            """, harness.Parts);
    }

    [Fact]
    [UpstreamTest(Prefix + "provider-executed static tools::should preserve separate call and result provider metadata for static tools", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_separate_call_and_result_provider_metadata_for_static_tools()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-available","toolCallId":"tool-call-1","toolName":"tool-name","input":{"query":"test"},"providerExecuted":true,"providerMetadata":{"testProvider":{"itemId":"call-item"}}},{"type":"tool-output-available","toolCallId":"tool-call-1","output":{"result":"provider-result"},"providerExecuted":true,"providerMetadata":{"testProvider":{"itemId":"result-item"}}},{"type":"finish-step"},{"type":"finish"}]
            """);

        var toolPart = harness.FindToolPart("tool-call-1")!;
        ProcessUIMessageStreamHarness.AssertJson("""
            {"testProvider":{"itemId":"call-item"}}
            """, toolPart["callProviderMetadata"]);
        ProcessUIMessageStreamHarness.AssertJson("""
            {"testProvider":{"itemId":"result-item"}}
            """, toolPart["resultProviderMetadata"]);
    }
}
