// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages &gt; when converting tool approval request responses</c>.</summary>
[Collection("LogWarnings")]
public sealed class WhenConvertingToolApprovalRequestResponsesTests : WarningsOff
{
    private const string Suite =
        Prefix + "convertToModelMessages > when converting tool approval request responses::";

    [UpstreamTest(
        Suite + "should convert an approved tool approval request (static tool)",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_an_approved_tool_approval_request_static_tool()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [
                  {
                    "role": "user",
                    "parts": [
                      {
                        "type": "text",
                        "text": "What is the weather in Tokyo?"
                      }
                    ]
                  },
                  {
                    "role": "assistant",
                    "parts": [
                      {
                        "type": "step-start"
                      },
                      {
                        "type": "tool-weather",
                        "toolCallId": "call-1",
                        "state": "approval-responded",
                        "input": {
                          "city": "Tokyo"
                        },
                        "approval": {
                          "approved": true,
                          "id": "approval-1"
                        }
                      }
                    ]
                  }
                ]
                """
            )
        );

        AssertEqual(
            """
            [
              {
                "role": "user",
                "content": [
                  {
                    "type": "text",
                    "text": "What is the weather in Tokyo?"
                  }
                ]
              },
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "tool-call",
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "input": {
                      "city": "Tokyo"
                    }
                  },
                  {
                    "type": "tool-approval-request",
                    "approvalId": "approval-1",
                    "toolCallId": "call-1"
                  }
                ]
              },
              {
                "role": "tool",
                "content": [
                  {
                    "type": "tool-approval-response",
                    "approvalId": "approval-1",
                    "approved": true
                  }
                ]
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should convert an approved tool approval request (dynamic tool)",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_an_approved_tool_approval_request_dynamic_tool()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [
                  {
                    "role": "user",
                    "parts": [
                      {
                        "type": "text",
                        "text": "What is the weather in Tokyo?"
                      }
                    ]
                  },
                  {
                    "role": "assistant",
                    "parts": [
                      {
                        "type": "step-start"
                      },
                      {
                        "type": "dynamic-tool",
                        "toolCallId": "call-1",
                        "state": "approval-responded",
                        "input": {
                          "city": "Tokyo"
                        },
                        "toolName": "weather",
                        "approval": {
                          "approved": true,
                          "id": "approval-1"
                        }
                      }
                    ]
                  }
                ]
                """
            )
        );

        AssertEqual(
            """
            [
              {
                "role": "user",
                "content": [
                  {
                    "type": "text",
                    "text": "What is the weather in Tokyo?"
                  }
                ]
              },
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "tool-call",
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "input": {
                      "city": "Tokyo"
                    }
                  },
                  {
                    "type": "tool-approval-request",
                    "approvalId": "approval-1",
                    "toolCallId": "call-1"
                  }
                ]
              },
              {
                "role": "tool",
                "content": [
                  {
                    "type": "tool-approval-response",
                    "approvalId": "approval-1",
                    "approved": true
                  }
                ]
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should preserve automatic approval metadata for approved tool results",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_preserve_automatic_approval_metadata_for_approved_tool_results()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [
                  {
                    "role": "user",
                    "parts": [
                      {
                        "type": "text",
                        "text": "What is the weather in Tokyo?"
                      }
                    ]
                  },
                  {
                    "role": "assistant",
                    "parts": [
                      {
                        "type": "step-start"
                      },
                      {
                        "type": "tool-weather",
                        "toolCallId": "call-1",
                        "state": "output-available",
                        "input": {
                          "city": "Tokyo"
                        },
                        "approval": {
                          "approved": true,
                          "id": "approval-1",
                          "isAutomatic": true,
                          "reason": "trusted internal tool"
                        },
                        "output": {
                          "weather": "Sunny"
                        }
                      }
                    ]
                  }
                ]
                """
            )
        );

        AssertEqual(
            """
            [
              {
                "role": "user",
                "content": [
                  {
                    "type": "text",
                    "text": "What is the weather in Tokyo?"
                  }
                ]
              },
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "tool-call",
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "input": {
                      "city": "Tokyo"
                    }
                  },
                  {
                    "type": "tool-approval-request",
                    "approvalId": "approval-1",
                    "toolCallId": "call-1",
                    "isAutomatic": true
                  }
                ]
              },
              {
                "role": "tool",
                "content": [
                  {
                    "type": "tool-approval-response",
                    "approvalId": "approval-1",
                    "approved": true,
                    "reason": "trusted internal tool"
                  },
                  {
                    "type": "tool-result",
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "output": {
                      "type": "json",
                      "value": {
                        "weather": "Sunny"
                      }
                    }
                  }
                ]
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should convert a denied tool approval request and follow up text (static tool)",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_a_denied_tool_approval_request_and_follow_up_text_static_tool()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [
                  {
                    "role": "user",
                    "parts": [
                      {
                        "type": "text",
                        "text": "What is the weather in Tokyo?"
                      }
                    ]
                  },
                  {
                    "role": "assistant",
                    "parts": [
                      {
                        "type": "step-start"
                      },
                      {
                        "type": "tool-weather",
                        "toolCallId": "call-1",
                        "state": "approval-responded",
                        "input": {
                          "city": "Tokyo"
                        },
                        "approval": {
                          "approved": false,
                          "id": "approval-1",
                          "reason": "I don't want to approve this"
                        }
                      },
                      {
                        "type": "step-start"
                      },
                      {
                        "type": "text",
                        "text": "I was not able to retrieve the weather.",
                        "state": "done"
                      }
                    ]
                  }
                ]
                """
            )
        );

        AssertEqual(
            """
            [
              {
                "role": "user",
                "content": [
                  {
                    "type": "text",
                    "text": "What is the weather in Tokyo?"
                  }
                ]
              },
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "tool-call",
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "input": {
                      "city": "Tokyo"
                    }
                  },
                  {
                    "type": "tool-approval-request",
                    "approvalId": "approval-1",
                    "toolCallId": "call-1"
                  }
                ]
              },
              {
                "role": "tool",
                "content": [
                  {
                    "type": "tool-approval-response",
                    "approvalId": "approval-1",
                    "approved": false,
                    "reason": "I don't want to approve this"
                  },
                  {
                    "type": "tool-result",
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "output": {
                      "type": "execution-denied",
                      "reason": "I don't want to approve this"
                    }
                  }
                ]
              },
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "text",
                    "text": "I was not able to retrieve the weather."
                  }
                ]
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should convert a denied tool approval request and follow up text (dynamic tool)",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_a_denied_tool_approval_request_and_follow_up_text_dynamic_tool()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [
                  {
                    "role": "user",
                    "parts": [
                      {
                        "type": "text",
                        "text": "What is the weather in Tokyo?"
                      }
                    ]
                  },
                  {
                    "role": "assistant",
                    "parts": [
                      {
                        "type": "step-start"
                      },
                      {
                        "type": "dynamic-tool",
                        "toolCallId": "call-1",
                        "state": "approval-responded",
                        "input": {
                          "city": "Tokyo"
                        },
                        "toolName": "weather",
                        "approval": {
                          "approved": false,
                          "id": "approval-1",
                          "reason": "I don't want to approve this"
                        }
                      },
                      {
                        "type": "step-start"
                      },
                      {
                        "type": "text",
                        "text": "I was not able to retrieve the weather.",
                        "state": "done"
                      }
                    ]
                  }
                ]
                """
            )
        );

        AssertEqual(
            """
            [
              {
                "role": "user",
                "content": [
                  {
                    "type": "text",
                    "text": "What is the weather in Tokyo?"
                  }
                ]
              },
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "tool-call",
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "input": {
                      "city": "Tokyo"
                    }
                  },
                  {
                    "type": "tool-approval-request",
                    "approvalId": "approval-1",
                    "toolCallId": "call-1"
                  }
                ]
              },
              {
                "role": "tool",
                "content": [
                  {
                    "type": "tool-approval-response",
                    "approvalId": "approval-1",
                    "approved": false,
                    "reason": "I don't want to approve this"
                  },
                  {
                    "type": "tool-result",
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "output": {
                      "type": "execution-denied",
                      "reason": "I don't want to approve this"
                    }
                  }
                ]
              },
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "text",
                    "text": "I was not able to retrieve the weather."
                  }
                ]
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should convert tool output denied (static tool)",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_tool_output_denied_static_tool()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [
                  {
                    "role": "user",
                    "parts": [
                      {
                        "type": "text",
                        "text": "What is the weather in Tokyo?"
                      }
                    ]
                  },
                  {
                    "role": "assistant",
                    "parts": [
                      {
                        "type": "step-start"
                      },
                      {
                        "type": "tool-weather",
                        "toolCallId": "call-1",
                        "state": "output-denied",
                        "input": {
                          "city": "Tokyo"
                        },
                        "approval": {
                          "approved": false,
                          "id": "approval-1",
                          "reason": "I don't want to approve this"
                        }
                      }
                    ]
                  }
                ]
                """
            )
        );

        AssertEqual(
            """
            [
              {
                "role": "user",
                "content": [
                  {
                    "type": "text",
                    "text": "What is the weather in Tokyo?"
                  }
                ]
              },
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "tool-call",
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "input": {
                      "city": "Tokyo"
                    }
                  },
                  {
                    "type": "tool-approval-request",
                    "approvalId": "approval-1",
                    "toolCallId": "call-1"
                  }
                ]
              },
              {
                "role": "tool",
                "content": [
                  {
                    "type": "tool-approval-response",
                    "approvalId": "approval-1",
                    "approved": false,
                    "reason": "I don't want to approve this"
                  },
                  {
                    "type": "tool-result",
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "output": {
                      "type": "error-text",
                      "value": "I don't want to approve this"
                    }
                  }
                ]
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should convert tool output denied without approval (static tool)",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_tool_output_denied_without_approval_static_tool()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [
                  {
                    "role": "user",
                    "parts": [
                      {
                        "type": "text",
                        "text": "What is the weather in Tokyo?"
                      }
                    ]
                  },
                  {
                    "role": "assistant",
                    "parts": [
                      {
                        "type": "tool-weather",
                        "toolCallId": "call-1",
                        "state": "output-denied",
                        "input": {
                          "city": "Tokyo"
                        }
                      }
                    ]
                  }
                ]
                """
            )
        );

        AssertEqual(
            """
            [
              {
                "role": "user",
                "content": [
                  {
                    "type": "text",
                    "text": "What is the weather in Tokyo?"
                  }
                ]
              },
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "tool-call",
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "input": {
                      "city": "Tokyo"
                    }
                  }
                ]
              },
              {
                "role": "tool",
                "content": [
                  {
                    "type": "tool-result",
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "output": {
                      "type": "error-text",
                      "value": "Tool call execution denied."
                    }
                  }
                ]
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should convert tool output denied (dynamic tool)",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_tool_output_denied_dynamic_tool()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [
                  {
                    "role": "user",
                    "parts": [
                      {
                        "type": "text",
                        "text": "What is the weather in Tokyo?"
                      }
                    ]
                  },
                  {
                    "role": "assistant",
                    "parts": [
                      {
                        "type": "step-start"
                      },
                      {
                        "type": "dynamic-tool",
                        "toolCallId": "call-1",
                        "state": "output-denied",
                        "input": {
                          "city": "Tokyo"
                        },
                        "toolName": "weather",
                        "approval": {
                          "approved": false,
                          "id": "approval-1",
                          "reason": "I don't want to approve this"
                        }
                      }
                    ]
                  }
                ]
                """
            )
        );

        AssertEqual(
            """
            [
              {
                "role": "user",
                "content": [
                  {
                    "type": "text",
                    "text": "What is the weather in Tokyo?"
                  }
                ]
              },
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "tool-call",
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "input": {
                      "city": "Tokyo"
                    }
                  },
                  {
                    "type": "tool-approval-request",
                    "approvalId": "approval-1",
                    "toolCallId": "call-1"
                  }
                ]
              },
              {
                "role": "tool",
                "content": [
                  {
                    "type": "tool-approval-response",
                    "approvalId": "approval-1",
                    "approved": false,
                    "reason": "I don't want to approve this"
                  },
                  {
                    "type": "tool-result",
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "output": {
                      "type": "error-text",
                      "value": "I don't want to approve this"
                    }
                  }
                ]
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should convert tool output result with approval and follow up text (static tool)",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_tool_output_result_with_approval_and_follow_up_text_static_tool()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [
                  {
                    "role": "user",
                    "parts": [
                      {
                        "type": "text",
                        "text": "What is the weather in Tokyo?"
                      }
                    ]
                  },
                  {
                    "role": "assistant",
                    "parts": [
                      {
                        "type": "step-start"
                      },
                      {
                        "type": "tool-weather",
                        "toolCallId": "call-1",
                        "state": "output-available",
                        "input": {
                          "city": "Tokyo"
                        },
                        "approval": {
                          "approved": true,
                          "id": "approval-1"
                        },
                        "output": {
                          "weather": "Sunny",
                          "temperature": "20°C"
                        }
                      },
                      {
                        "type": "step-start"
                      },
                      {
                        "type": "text",
                        "text": "The weather in Tokyo is sunny.",
                        "state": "done"
                      }
                    ]
                  }
                ]
                """
            )
        );

        AssertEqual(
            """
            [
              {
                "role": "user",
                "content": [
                  {
                    "type": "text",
                    "text": "What is the weather in Tokyo?"
                  }
                ]
              },
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "tool-call",
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "input": {
                      "city": "Tokyo"
                    }
                  },
                  {
                    "type": "tool-approval-request",
                    "approvalId": "approval-1",
                    "toolCallId": "call-1"
                  }
                ]
              },
              {
                "role": "tool",
                "content": [
                  {
                    "type": "tool-approval-response",
                    "approvalId": "approval-1",
                    "approved": true
                  },
                  {
                    "type": "tool-result",
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "output": {
                      "type": "json",
                      "value": {
                        "weather": "Sunny",
                        "temperature": "20°C"
                      }
                    }
                  }
                ]
              },
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "text",
                    "text": "The weather in Tokyo is sunny."
                  }
                ]
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should propagate reason from a pending approval request",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_propagate_reason_from_a_pending_approval_request()
    {
        var result = Convert(
            new[]
            {
                Assistant(
                    new ToolUIPart("weather", "call-1", ToolUIPartState.ApprovalRequested)
                    {
                        Approval = new ToolUIApproval("a1")
                        {
                            RequestReason = "requires operator review",
                        },
                        Input = Json("{\"city\":\"Tokyo\"}"),
                    }
                ),
            }
        );

        var assistantMessage = Render(result)
            .AsArray()
            .First(message => (string?)message!["role"] == "assistant")!;
        Assert.Contains(assistantMessage["content"]!.AsArray(), part => JsonNode.DeepEquals(
                part,
                JsonNode.Parse(
                    """
                    {
                      "type": "tool-approval-request",
                      "approvalId": "a1",
                      "toolCallId": "call-1",
                      "reason": "requires operator review"
                    }
                    """
                )
            ));
    }

    [UpstreamTest(
        Suite + "should keep request and response reasons separate after approval",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_keep_request_and_response_reasons_separate_after_approval()
    {
        var result = Convert(
            new[]
            {
                Assistant(
                    new ToolUIPart("weather", "call-1", ToolUIPartState.ApprovalResponded)
                    {
                        Approval = new ToolUIApproval("a1")
                        {
                            Approved = true,
                            RequestReason = "requires operator review",
                            Reason = "approved by on-call operator",
                        },
                        Input = Json("{\"city\":\"Tokyo\"}"),
                    }
                ),
            }
        );

        var rendered = Render(result).AsArray();
        var assistantMessage = rendered.First(message => (string?)message!["role"] == "assistant")!;
        Assert.Contains(assistantMessage["content"]!.AsArray(), part => JsonNode.DeepEquals(
                part,
                JsonNode.Parse(
                    """
                    {
                      "type": "tool-approval-request",
                      "approvalId": "a1",
                      "toolCallId": "call-1",
                      "reason": "requires operator review"
                    }
                    """
                )
            ));

        var toolMessage = rendered.First(message => (string?)message!["role"] == "tool")!;
        Assert.Contains(toolMessage["content"]!.AsArray(), part => JsonNode.DeepEquals(
                part,
                JsonNode.Parse(
                    """
                    {
                      "type": "tool-approval-response",
                      "approvalId": "a1",
                      "approved": true,
                      "reason": "approved by on-call operator"
                    }
                    """
                )
            ));
    }

    [UpstreamTest(
        Suite + "should propagate signature from approval to tool-approval-request part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_propagate_signature_from_approval_to_tool_approval_request_part()
    {
        var result = Convert(
            new[]
            {
                User(new TextUIPart("What is the weather in Tokyo?")),
                Assistant(
                    new StepStartUIPart(),
                    new ToolUIPart("weather", "call-1", ToolUIPartState.ApprovalResponded)
                    {
                        Approval = new ToolUIApproval("a1")
                        {
                            Approved = true,
                            Signature = "test-sig",
                        },
                        Input = Json("{\"city\":\"Tokyo\"}"),
                    }
                ),
            }
        );

        AssertEqual(
            """
            [
              {
                "content": [
                  { "text": "What is the weather in Tokyo?", "type": "text" }
                ],
                "role": "user"
              },
              {
                "content": [
                  {
                    "input": { "city": "Tokyo" },
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "type": "tool-call"
                  },
                  {
                    "approvalId": "a1",
                    "signature": "test-sig",
                    "toolCallId": "call-1",
                    "type": "tool-approval-request"
                  }
                ],
                "role": "assistant"
              },
              {
                "content": [
                  {
                    "approvalId": "a1",
                    "approved": true,
                    "type": "tool-approval-response"
                  }
                ],
                "role": "tool"
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite
            + "should not include signature in tool-approval-request when approval has no signature",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_not_include_signature_in_tool_approval_request_when_approval_has_no_signature()
    {
        var result = Convert(
            new[]
            {
                User(new TextUIPart("What is the weather in Tokyo?")),
                Assistant(
                    new StepStartUIPart(),
                    new ToolUIPart("weather", "call-1", ToolUIPartState.ApprovalResponded)
                    {
                        Approval = new ToolUIApproval("a1") { Approved = true },
                        Input = Json("{\"city\":\"Tokyo\"}"),
                    }
                ),
            }
        );

        var assistantMessage = result.OfType<Provider.AssistantModelMessage>().Single();
        var approvalRequest = assistantMessage
            .Parts.OfType<Provider.AssistantToolApprovalRequestPart>()
            .SingleOrDefault();
        Assert.NotNull(approvalRequest);
        Assert.Null(approvalRequest.Signature);
        Assert.DoesNotContain(
            "signature",
            Render(result)[1]!["content"]![1]!.AsObject().Select(property => property.Key)
        );
    }

    [UpstreamTest(
        Suite + "should convert tool error result with approval and follow up text (static tool)",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_tool_error_result_with_approval_and_follow_up_text_static_tool()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [
                  {
                    "role": "user",
                    "parts": [
                      {
                        "type": "text",
                        "text": "What is the weather in Tokyo?"
                      }
                    ]
                  },
                  {
                    "role": "assistant",
                    "parts": [
                      {
                        "type": "step-start"
                      },
                      {
                        "type": "tool-weather",
                        "toolCallId": "call-1",
                        "state": "output-error",
                        "input": {
                          "city": "Tokyo"
                        },
                        "approval": {
                          "approved": true,
                          "id": "approval-1"
                        },
                        "errorText": "Error: Fetching weather data failed"
                      },
                      {
                        "type": "step-start"
                      },
                      {
                        "type": "text",
                        "text": "The weather in Tokyo is sunny.",
                        "state": "done"
                      }
                    ]
                  }
                ]
                """
            )
        );

        AssertEqual(
            """
            [
              {
                "role": "user",
                "content": [
                  {
                    "type": "text",
                    "text": "What is the weather in Tokyo?"
                  }
                ]
              },
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "tool-call",
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "input": {
                      "city": "Tokyo"
                    }
                  },
                  {
                    "type": "tool-approval-request",
                    "approvalId": "approval-1",
                    "toolCallId": "call-1"
                  }
                ]
              },
              {
                "role": "tool",
                "content": [
                  {
                    "type": "tool-approval-response",
                    "approvalId": "approval-1",
                    "approved": true
                  },
                  {
                    "type": "tool-result",
                    "toolCallId": "call-1",
                    "toolName": "weather",
                    "output": {
                      "type": "error-text",
                      "value": "Error: Fetching weather data failed"
                    }
                  }
                ]
              },
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "text",
                    "text": "The weather in Tokyo is sunny."
                  }
                ]
              }
            ]
            """,
            result
        );
    }
}
