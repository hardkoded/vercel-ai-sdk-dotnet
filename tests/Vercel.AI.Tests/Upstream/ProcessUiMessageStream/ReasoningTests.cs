// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>reasoning</c>.</summary>
public sealed class ReasoningTests
{
    private const string Prefix = "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(Prefix + "reasoning::should preserve reasoning part ids", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_reasoning_part_ids()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"reasoning-start","id":"reasoning-1"},{"type":"reasoning-delta","id":"reasoning-1","delta":"I will open the conversation"},{"type":"reasoning-delta","id":"reasoning-1","delta":" with witty banter. ","providerMetadata":{"testProvider":{"signature":"1234567890"}}},{"type":"reasoning-end","id":"reasoning-1"},{"type":"reasoning-start","id":"reasoning-2"},{"type":"reasoning-delta","id":"reasoning-2","delta":"redacted-data","providerMetadata":{"testProvider":{"isRedacted":true}}},{"type":"reasoning-end","id":"reasoning-2"},{"type":"reasoning-start","id":"reasoning-3"},{"type":"reasoning-delta","id":"reasoning-3","delta":"Once the user has relaxed,"},{"type":"reasoning-delta","id":"reasoning-3","delta":" I will pry for valuable information.","providerMetadata":{"testProvider":{"signature":"abc123"}}},{"type":"reasoning-end","id":"reasoning-3"},{"type":"text-start","id":"text-1"},{"type":"text-delta","id":"text-1","delta":"Hi there!"},{"type":"text-end","id":"text-1"},{"type":"finish-step"},{"type":"finish"}]
            """);

        ProcessUIMessageStreamHarness.AssertJson("""
            ["reasoning-1","reasoning-2","reasoning-3"]
            """, new JsonArray(harness.Parts.OfType<JsonObject>().Where(part => (string?)part["type"] == "reasoning").Select(part => (JsonNode?)part["id"]!.DeepClone()).ToArray()));
    }

    [Fact]
    [UpstreamTest(Prefix + "reasoning::should call the update function with the correct arguments", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_call_the_update_function_with_the_correct_arguments()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"reasoning-start","id":"reasoning-1"},{"type":"reasoning-delta","id":"reasoning-1","delta":"I will open the conversation"},{"type":"reasoning-delta","id":"reasoning-1","delta":" with witty banter. ","providerMetadata":{"testProvider":{"signature":"1234567890"}}},{"type":"reasoning-end","id":"reasoning-1"},{"type":"reasoning-start","id":"reasoning-2"},{"type":"reasoning-delta","id":"reasoning-2","delta":"redacted-data","providerMetadata":{"testProvider":{"isRedacted":true}}},{"type":"reasoning-end","id":"reasoning-2"},{"type":"reasoning-start","id":"reasoning-3"},{"type":"reasoning-delta","id":"reasoning-3","delta":"Once the user has relaxed,"},{"type":"reasoning-delta","id":"reasoning-3","delta":" I will pry for valuable information.","providerMetadata":{"testProvider":{"signature":"abc123"}}},{"type":"reasoning-end","id":"reasoning-3"},{"type":"text-start","id":"text-1"},{"type":"text-delta","id":"text-1","delta":"Hi there!"},{"type":"text-end","id":"text-1"},{"type":"finish-step"},{"type":"finish"}]
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
                      "id": "reasoning-1",
                      "state": "streaming",
                      "text": "",
                      "type": "reasoning"
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
                      "id": "reasoning-1",
                      "state": "streaming",
                      "text": "I will open the conversation",
                      "type": "reasoning"
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
                      "id": "reasoning-1",
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "1234567890"
                        }
                      },
                      "state": "streaming",
                      "text": "I will open the conversation with witty banter. ",
                      "type": "reasoning"
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
                      "id": "reasoning-1",
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "1234567890"
                        }
                      },
                      "state": "done",
                      "text": "I will open the conversation with witty banter. ",
                      "type": "reasoning"
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
                      "id": "reasoning-1",
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "1234567890"
                        }
                      },
                      "state": "done",
                      "text": "I will open the conversation with witty banter. ",
                      "type": "reasoning"
                    },
                    {
                      "id": "reasoning-2",
                      "state": "streaming",
                      "text": "",
                      "type": "reasoning"
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
                      "id": "reasoning-1",
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "1234567890"
                        }
                      },
                      "state": "done",
                      "text": "I will open the conversation with witty banter. ",
                      "type": "reasoning"
                    },
                    {
                      "id": "reasoning-2",
                      "providerMetadata": {
                        "testProvider": {
                          "isRedacted": true
                        }
                      },
                      "state": "streaming",
                      "text": "redacted-data",
                      "type": "reasoning"
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
                      "id": "reasoning-1",
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "1234567890"
                        }
                      },
                      "state": "done",
                      "text": "I will open the conversation with witty banter. ",
                      "type": "reasoning"
                    },
                    {
                      "id": "reasoning-2",
                      "providerMetadata": {
                        "testProvider": {
                          "isRedacted": true
                        }
                      },
                      "state": "done",
                      "text": "redacted-data",
                      "type": "reasoning"
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
                      "id": "reasoning-1",
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "1234567890"
                        }
                      },
                      "state": "done",
                      "text": "I will open the conversation with witty banter. ",
                      "type": "reasoning"
                    },
                    {
                      "id": "reasoning-2",
                      "providerMetadata": {
                        "testProvider": {
                          "isRedacted": true
                        }
                      },
                      "state": "done",
                      "text": "redacted-data",
                      "type": "reasoning"
                    },
                    {
                      "id": "reasoning-3",
                      "state": "streaming",
                      "text": "",
                      "type": "reasoning"
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
                      "id": "reasoning-1",
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "1234567890"
                        }
                      },
                      "state": "done",
                      "text": "I will open the conversation with witty banter. ",
                      "type": "reasoning"
                    },
                    {
                      "id": "reasoning-2",
                      "providerMetadata": {
                        "testProvider": {
                          "isRedacted": true
                        }
                      },
                      "state": "done",
                      "text": "redacted-data",
                      "type": "reasoning"
                    },
                    {
                      "id": "reasoning-3",
                      "state": "streaming",
                      "text": "Once the user has relaxed,",
                      "type": "reasoning"
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
                      "id": "reasoning-1",
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "1234567890"
                        }
                      },
                      "state": "done",
                      "text": "I will open the conversation with witty banter. ",
                      "type": "reasoning"
                    },
                    {
                      "id": "reasoning-2",
                      "providerMetadata": {
                        "testProvider": {
                          "isRedacted": true
                        }
                      },
                      "state": "done",
                      "text": "redacted-data",
                      "type": "reasoning"
                    },
                    {
                      "id": "reasoning-3",
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "abc123"
                        }
                      },
                      "state": "streaming",
                      "text": "Once the user has relaxed, I will pry for valuable information.",
                      "type": "reasoning"
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
                      "id": "reasoning-1",
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "1234567890"
                        }
                      },
                      "state": "done",
                      "text": "I will open the conversation with witty banter. ",
                      "type": "reasoning"
                    },
                    {
                      "id": "reasoning-2",
                      "providerMetadata": {
                        "testProvider": {
                          "isRedacted": true
                        }
                      },
                      "state": "done",
                      "text": "redacted-data",
                      "type": "reasoning"
                    },
                    {
                      "id": "reasoning-3",
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "abc123"
                        }
                      },
                      "state": "done",
                      "text": "Once the user has relaxed, I will pry for valuable information.",
                      "type": "reasoning"
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
                      "id": "reasoning-1",
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "1234567890"
                        }
                      },
                      "state": "done",
                      "text": "I will open the conversation with witty banter. ",
                      "type": "reasoning"
                    },
                    {
                      "id": "reasoning-2",
                      "providerMetadata": {
                        "testProvider": {
                          "isRedacted": true
                        }
                      },
                      "state": "done",
                      "text": "redacted-data",
                      "type": "reasoning"
                    },
                    {
                      "id": "reasoning-3",
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "abc123"
                        }
                      },
                      "state": "done",
                      "text": "Once the user has relaxed, I will pry for valuable information.",
                      "type": "reasoning"
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
                      "id": "reasoning-1",
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "1234567890"
                        }
                      },
                      "state": "done",
                      "text": "I will open the conversation with witty banter. ",
                      "type": "reasoning"
                    },
                    {
                      "id": "reasoning-2",
                      "providerMetadata": {
                        "testProvider": {
                          "isRedacted": true
                        }
                      },
                      "state": "done",
                      "text": "redacted-data",
                      "type": "reasoning"
                    },
                    {
                      "id": "reasoning-3",
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "abc123"
                        }
                      },
                      "state": "done",
                      "text": "Once the user has relaxed, I will pry for valuable information.",
                      "type": "reasoning"
                    },
                    {
                      "state": "streaming",
                      "text": "Hi there!",
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
                      "id": "reasoning-1",
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "1234567890"
                        }
                      },
                      "state": "done",
                      "text": "I will open the conversation with witty banter. ",
                      "type": "reasoning"
                    },
                    {
                      "id": "reasoning-2",
                      "providerMetadata": {
                        "testProvider": {
                          "isRedacted": true
                        }
                      },
                      "state": "done",
                      "text": "redacted-data",
                      "type": "reasoning"
                    },
                    {
                      "id": "reasoning-3",
                      "providerMetadata": {
                        "testProvider": {
                          "signature": "abc123"
                        }
                      },
                      "state": "done",
                      "text": "Once the user has relaxed, I will pry for valuable information.",
                      "type": "reasoning"
                    },
                    {
                      "state": "done",
                      "text": "Hi there!",
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
    [UpstreamTest(Prefix + "reasoning::should have the correct final message state", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_the_correct_final_message_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"reasoning-start","id":"reasoning-1"},{"type":"reasoning-delta","id":"reasoning-1","delta":"I will open the conversation"},{"type":"reasoning-delta","id":"reasoning-1","delta":" with witty banter. ","providerMetadata":{"testProvider":{"signature":"1234567890"}}},{"type":"reasoning-end","id":"reasoning-1"},{"type":"reasoning-start","id":"reasoning-2"},{"type":"reasoning-delta","id":"reasoning-2","delta":"redacted-data","providerMetadata":{"testProvider":{"isRedacted":true}}},{"type":"reasoning-end","id":"reasoning-2"},{"type":"reasoning-start","id":"reasoning-3"},{"type":"reasoning-delta","id":"reasoning-3","delta":"Once the user has relaxed,"},{"type":"reasoning-delta","id":"reasoning-3","delta":" I will pry for valuable information.","providerMetadata":{"testProvider":{"signature":"abc123"}}},{"type":"reasoning-end","id":"reasoning-3"},{"type":"text-start","id":"text-1"},{"type":"text-delta","id":"text-1","delta":"Hi there!"},{"type":"text-end","id":"text-1"},{"type":"finish-step"},{"type":"finish"}]
            """);

        ProcessUIMessageStreamHarness.AssertJson("""
            {
              "id": "msg-123",
              "parts": [
                {
                  "type": "step-start"
                },
                {
                  "id": "reasoning-1",
                  "providerMetadata": {
                    "testProvider": {
                      "signature": "1234567890"
                    }
                  },
                  "state": "done",
                  "text": "I will open the conversation with witty banter. ",
                  "type": "reasoning"
                },
                {
                  "id": "reasoning-2",
                  "providerMetadata": {
                    "testProvider": {
                      "isRedacted": true
                    }
                  },
                  "state": "done",
                  "text": "redacted-data",
                  "type": "reasoning"
                },
                {
                  "id": "reasoning-3",
                  "providerMetadata": {
                    "testProvider": {
                      "signature": "abc123"
                    }
                  },
                  "state": "done",
                  "text": "Once the user has relaxed, I will pry for valuable information.",
                  "type": "reasoning"
                },
                {
                  "state": "done",
                  "text": "Hi there!",
                  "type": "text"
                }
              ],
              "role": "assistant"
            }
            """, harness.Message);
    }
}
