// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>provider metadata</c>.</summary>
public sealed class ProviderMetadataTests
{
    private const string Prefix = "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(Prefix + "provider metadata::should call the update function with the correct arguments", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_call_the_update_function_with_the_correct_arguments()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"text-start","id":"1","providerMetadata":{"testProvider":{"signature":"1"}}},{"type":"text-delta","id":"1","delta":"Hello"},{"type":"text-delta","id":"1","delta":", "},{"type":"text-delta","id":"1","delta":"world!"},{"type":"text-end","id":"1"},{"type":"tool-input-available","toolCallId":"tool-call-id","toolName":"tool-name","input":{"query":"test"},"providerMetadata":{"testProvider":{"signature":"2"}}},{"type":"finish-step"},{"type":"finish"}]
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
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "1"
                        }
                      },
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
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "1"
                        }
                      },
                      "state": "streaming",
                      "text": "Hello",
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
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "1"
                        }
                      },
                      "state": "streaming",
                      "text": "Hello, ",
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
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "1"
                        }
                      },
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
                  "id": "msg-123",
                  "parts": [
                    {
                      "type": "step-start"
                    },
                    {
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "1"
                        }
                      },
                      "state": "done",
                      "text": "Hello, world!",
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
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "1"
                        }
                      },
                      "state": "done",
                      "text": "Hello, world!",
                      "type": "text"
                    },
                    {
                      "callProviderMetadata": {
                        "testProvider": {
                          "signature": "2"
                        }
                      },
                      "input": {
                        "query": "test"
                      },
                      "state": "input-available",
                      "toolCallId": "tool-call-id",
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
    [UpstreamTest(Prefix + "provider metadata::should have the correct final message state", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_the_correct_final_message_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"text-start","id":"1","providerMetadata":{"testProvider":{"signature":"1"}}},{"type":"text-delta","id":"1","delta":"Hello"},{"type":"text-delta","id":"1","delta":", "},{"type":"text-delta","id":"1","delta":"world!"},{"type":"text-end","id":"1"},{"type":"tool-input-available","toolCallId":"tool-call-id","toolName":"tool-name","input":{"query":"test"},"providerMetadata":{"testProvider":{"signature":"2"}}},{"type":"finish-step"},{"type":"finish"}]
            """);

        ProcessUIMessageStreamHarness.AssertJson("""
            [
              {
                "type": "step-start"
              },
              {
                "providerMetadata": {
                  "testProvider": {
                    "signature": "1"
                  }
                },
                "state": "done",
                "text": "Hello, world!",
                "type": "text"
              },
              {
                "callProviderMetadata": {
                  "testProvider": {
                    "signature": "2"
                  }
                },
                "input": {
                  "query": "test"
                },
                "state": "input-available",
                "toolCallId": "tool-call-id",
                "type": "tool-tool-name"
              }
            ]
            """, harness.Parts);
    }
}
