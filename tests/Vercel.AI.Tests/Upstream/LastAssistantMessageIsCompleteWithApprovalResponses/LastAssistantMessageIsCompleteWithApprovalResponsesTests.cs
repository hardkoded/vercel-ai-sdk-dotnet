// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Ui;

namespace Vercel.AI.Tests.LastAssistantMessageIsCompleteWithApprovalResponses;

/// <summary>Port of <c>last-assistant-message-is-complete-with-approval-responses.test.ts</c> &gt; <c>lastAssistantMessageIsCompleteWithApprovalResponses</c>.</summary>
public sealed class LastAssistantMessageIsCompleteWithApprovalResponsesTests
{
    private const string Suite =
        "packages/ai/src/ui/last-assistant-message-is-complete-with-approval-responses.test.ts::lastAssistantMessageIsCompleteWithApprovalResponses::";

    private static bool Check(string messagesJson) =>
        UIMessageCompletion.LastAssistantMessageIsCompleteWithApprovalResponses(
            Vercel.AI.Tests.ValidateUiMessages.ValidateUiMessagesSupport.Messages(messagesJson)
        );

    [UpstreamTest(
        Suite + "should return false if messages is empty",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_false_if_messages_is_empty()
    {
        var result = Check(
            """
            []
            """
        );

        Assert.False(result);
    }

    [UpstreamTest(
        Suite + "should return false if last message is a user message",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_false_if_last_message_is_a_user_message()
    {
        var result = Check(
            """
            [
              {
                "id": "1",
                "role": "user",
                "parts": []
              }
            ]
            """
        );

        Assert.False(result);
    }

    [UpstreamTest(
        Suite + "should return false if there are no tool invocations in the last step",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_false_if_there_are_no_tool_invocations_in_the_last_step()
    {
        var result = Check(
            """
            [
              {
                "id": "1",
                "role": "assistant",
                "parts": [
                  {
                    "type": "step-start"
                  },
                  {
                    "type": "text",
                    "text": "Hello",
                    "state": "done"
                  }
                ]
              }
            ]
            """
        );

        Assert.False(result);
    }

    [UpstreamTest(
        Suite + "should return false if no tool has approval-responded state",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_false_if_no_tool_has_approval_responded_state()
    {
        var result = Check(
            """
            [
              {
                "id": "1",
                "role": "assistant",
                "parts": [
                  {
                    "type": "step-start"
                  },
                  {
                    "type": "tool-getWeather",
                    "toolCallId": "call_1",
                    "state": "approval-requested",
                    "input": {
                      "city": "Tokyo"
                    },
                    "approval": {
                      "id": "approval_1"
                    }
                  }
                ]
              }
            ]
            """
        );

        Assert.False(result);
    }

    [UpstreamTest(
        Suite + "should return false if some tools still have approval-requested state",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_false_if_some_tools_still_have_approval_requested_state()
    {
        var result = Check(
            """
            [
              {
                "id": "1",
                "role": "assistant",
                "parts": [
                  {
                    "type": "step-start"
                  },
                  {
                    "type": "tool-getWeather",
                    "toolCallId": "call_1",
                    "state": "approval-responded",
                    "input": {
                      "city": "Tokyo"
                    },
                    "approval": {
                      "id": "approval_1",
                      "approved": true
                    }
                  },
                  {
                    "type": "tool-getWeather",
                    "toolCallId": "call_2",
                    "state": "approval-requested",
                    "input": {
                      "city": "Paris"
                    },
                    "approval": {
                      "id": "approval_2"
                    }
                  }
                ]
              }
            ]
            """
        );

        Assert.False(result);
    }

    [UpstreamTest(
        Suite + "should return true when a non-provider-executed tool has approval-responded",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_true_when_a_non_provider_executed_tool_has_approval_responded()
    {
        var result = Check(
            """
            [
              {
                "id": "1",
                "role": "assistant",
                "parts": [
                  {
                    "type": "step-start"
                  },
                  {
                    "type": "tool-getWeather",
                    "toolCallId": "call_1",
                    "state": "approval-responded",
                    "input": {
                      "city": "Tokyo"
                    },
                    "approval": {
                      "id": "approval_1",
                      "approved": true
                    }
                  }
                ]
              }
            ]
            """
        );

        Assert.True(result);
    }

    [UpstreamTest(
        Suite + "should return true when a provider-executed tool has approval-responded",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_true_when_a_provider_executed_tool_has_approval_responded()
    {
        var result = Check(
            """
            [
              {
                "id": "1",
                "role": "assistant",
                "parts": [
                  {
                    "type": "step-start"
                  },
                  {
                    "type": "dynamic-tool",
                    "toolName": "mcp.shorten_url",
                    "toolCallId": "call_1",
                    "state": "approval-responded",
                    "input": {
                      "url": "https://ai-sdk.dev/"
                    },
                    "approval": {
                      "id": "approval_1",
                      "approved": true
                    },
                    "providerExecuted": true
                  }
                ]
              }
            ]
            """
        );

        Assert.True(result);
    }

    [UpstreamTest(
        Suite
            + "should return true when all tools have a terminal state and at least one is approval-responded",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_true_when_all_tools_have_a_terminal_state_and_at_least_one_is_approval_responded()
    {
        var result = Check(
            """
            [
              {
                "id": "1",
                "role": "assistant",
                "parts": [
                  {
                    "type": "step-start"
                  },
                  {
                    "type": "tool-getWeather",
                    "toolCallId": "call_1",
                    "state": "approval-responded",
                    "input": {
                      "city": "Tokyo"
                    },
                    "approval": {
                      "id": "approval_1",
                      "approved": true
                    }
                  },
                  {
                    "type": "tool-getWeather",
                    "toolCallId": "call_2",
                    "state": "output-available",
                    "input": {
                      "city": "Paris"
                    },
                    "output": {
                      "temperature": 20,
                      "weather": "cloudy"
                    }
                  }
                ]
              }
            ]
            """
        );

        Assert.True(result);
    }

    [UpstreamTest(
        Suite + "should return false when a tool output is preliminary",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_false_when_a_tool_output_is_preliminary()
    {
        var result = Check(
            """
            [
              {
                "id": "1",
                "role": "assistant",
                "parts": [
                  {
                    "type": "step-start"
                  },
                  {
                    "type": "tool-getWeather",
                    "toolCallId": "call_1",
                    "state": "approval-responded",
                    "input": {
                      "city": "Tokyo"
                    },
                    "approval": {
                      "id": "approval_1",
                      "approved": true
                    }
                  },
                  {
                    "type": "tool-getWeather",
                    "toolCallId": "call_2",
                    "state": "output-available",
                    "input": {
                      "city": "Paris"
                    },
                    "output": {
                      "progress": 50
                    },
                    "preliminary": true
                  }
                ]
              }
            ]
            """
        );

        Assert.False(result);
    }

    [UpstreamTest(
        Suite + "should return false when a dynamic tool output is preliminary",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_false_when_a_dynamic_tool_output_is_preliminary()
    {
        var result = Check(
            """
            [
              {
                "id": "1",
                "role": "assistant",
                "parts": [
                  {
                    "type": "step-start"
                  },
                  {
                    "type": "tool-getWeather",
                    "toolCallId": "call_1",
                    "state": "approval-responded",
                    "input": {
                      "city": "Tokyo"
                    },
                    "approval": {
                      "id": "approval_1",
                      "approved": true
                    }
                  },
                  {
                    "type": "dynamic-tool",
                    "toolName": "getDynamicWeather",
                    "toolCallId": "call_2",
                    "state": "output-available",
                    "input": {
                      "city": "Paris"
                    },
                    "output": {
                      "progress": 50
                    },
                    "preliminary": true
                  }
                ]
              }
            ]
            """
        );

        Assert.False(result);
    }

    [UpstreamTest(
        Suite
            + "should return true when a tool output is denied and another approval has responded",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_true_when_a_tool_output_is_denied_and_another_approval_has_responded()
    {
        var result = Check(
            """
            [
              {
                "id": "1",
                "role": "assistant",
                "parts": [
                  {
                    "type": "step-start"
                  },
                  {
                    "type": "tool-getWeather",
                    "toolCallId": "call_1",
                    "state": "approval-responded",
                    "input": {
                      "city": "Tokyo"
                    },
                    "approval": {
                      "id": "approval_1",
                      "approved": true
                    }
                  },
                  {
                    "type": "tool-deleteCalendarEvent",
                    "toolCallId": "call_2",
                    "state": "output-denied",
                    "input": {
                      "eventId": "event_1"
                    },
                    "approval": {
                      "id": "approval_2",
                      "approved": false
                    }
                  }
                ]
              }
            ]
            """
        );

        Assert.True(result);
    }

    [UpstreamTest(
        Suite
            + "should return true mixing provider-executed (approval-responded) and regular (output-available)",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_true_mixing_provider_executed_approval_responded_and_regular_output_available()
    {
        var result = Check(
            """
            [
              {
                "id": "1",
                "role": "assistant",
                "parts": [
                  {
                    "type": "step-start"
                  },
                  {
                    "type": "dynamic-tool",
                    "toolName": "mcp.shorten_url",
                    "toolCallId": "call_1",
                    "state": "approval-responded",
                    "input": {
                      "url": "https://ai-sdk.dev/"
                    },
                    "approval": {
                      "id": "approval_1",
                      "approved": true
                    },
                    "providerExecuted": true
                  },
                  {
                    "type": "tool-getWeather",
                    "toolCallId": "call_2",
                    "state": "output-available",
                    "input": {
                      "city": "Tokyo"
                    },
                    "output": {
                      "temperature": 25,
                      "weather": "sunny"
                    }
                  }
                ]
              }
            ]
            """
        );

        Assert.True(result);
    }

    [UpstreamTest(
        Suite
            + "should return false when provider-executed tool is approval-responded but regular tool is still approval-requested",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_false_when_provider_executed_tool_is_approval_responded_but_regular_tool_is_still_approval_requested()
    {
        var result = Check(
            """
            [
              {
                "id": "1",
                "role": "assistant",
                "parts": [
                  {
                    "type": "step-start"
                  },
                  {
                    "type": "dynamic-tool",
                    "toolName": "mcp.shorten_url",
                    "toolCallId": "call_1",
                    "state": "approval-responded",
                    "input": {
                      "url": "https://ai-sdk.dev/"
                    },
                    "approval": {
                      "id": "approval_1",
                      "approved": true
                    },
                    "providerExecuted": true
                  },
                  {
                    "type": "tool-getWeather",
                    "toolCallId": "call_2",
                    "state": "approval-requested",
                    "input": {
                      "city": "Tokyo"
                    },
                    "approval": {
                      "id": "approval_2"
                    }
                  }
                ]
              }
            ]
            """
        );

        Assert.False(result);
    }

    [UpstreamTest(
        Suite + "should only consider the last step in a multi-step message",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_only_consider_the_last_step_in_a_multi_step_message()
    {
        var result = Check(
            """
            [
              {
                "id": "1",
                "role": "assistant",
                "parts": [
                  {
                    "type": "step-start"
                  },
                  {
                    "type": "tool-getWeather",
                    "toolCallId": "call_1",
                    "state": "approval-responded",
                    "input": {
                      "city": "Tokyo"
                    },
                    "approval": {
                      "id": "approval_1",
                      "approved": true
                    }
                  },
                  {
                    "type": "step-start"
                  },
                  {
                    "type": "text",
                    "text": "Done.",
                    "state": "done"
                  }
                ]
              }
            ]
            """
        );

        Assert.False(result);
    }
}
