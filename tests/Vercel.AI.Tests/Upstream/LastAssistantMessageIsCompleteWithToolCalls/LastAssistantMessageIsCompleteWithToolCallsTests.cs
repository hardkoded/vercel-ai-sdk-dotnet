// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Ui;

namespace Vercel.AI.Tests.LastAssistantMessageIsCompleteWithToolCalls;

/// <summary>Port of <c>last-assistant-message-is-complete-with-tool-calls.test.ts</c> &gt; <c>lastAssistantMessageIsCompleteWithToolCalls</c>.</summary>
public sealed class LastAssistantMessageIsCompleteWithToolCallsTests
{
    private const string Suite =
        "packages/ai/src/ui/last-assistant-message-is-complete-with-tool-calls.test.ts::lastAssistantMessageIsCompleteWithToolCalls::";

    private static bool Check(string messagesJson) =>
        UIMessageCompletion.LastAssistantMessageIsCompleteWithToolCalls(
            Vercel.AI.Tests.ValidateUiMessages.ValidateUiMessagesSupport.Messages(messagesJson)
        );

    [UpstreamTest(
        Suite + "should return false if the last step of a multi-step sequence only has text",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_false_if_the_last_step_of_a_multi_step_sequence_only_has_text()
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
                    "type": "tool-getLocation",
                    "toolCallId": "call_CuEdmzpx4ZldCkg5SVr3ikLz",
                    "state": "output-available",
                    "input": {},
                    "output": "New York"
                  },
                  {
                    "type": "step-start"
                  },
                  {
                    "type": "text",
                    "text": "The current weather in New York is windy.",
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
        Suite
            + "should return true when completed text follows the last tool result in the last step",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_true_when_completed_text_follows_the_last_tool_result_in_the_last_step()
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
                    "type": "tool-getWeatherInformation",
                    "toolCallId": "call_6iy0GxZ9R4VDI5MKohXxV48y",
                    "state": "output-available",
                    "input": {
                      "city": "New York"
                    },
                    "output": "windy"
                  },
                  {
                    "type": "text",
                    "text": "The current weather in New York is windy.",
                    "state": "done"
                  }
                ]
              }
            ]
            """
        );

        Assert.True(result);
    }

    [UpstreamTest(
        Suite + "should return false when trailing text has no completed stream state",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_false_when_trailing_text_has_no_completed_stream_state()
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
                    "type": "tool-getWeatherInformation",
                    "toolCallId": "call_6iy0GxZ9R4VDI5MKohXxV48y",
                    "state": "output-available",
                    "input": {
                      "city": "New York"
                    },
                    "output": {
                      "success": true,
                      "queryResult": "large result"
                    }
                  },
                  {
                    "type": "text",
                    "text": "Prompt is too long"
                  }
                ]
              }
            ]
            """
        );

        Assert.False(result);
    }

    [UpstreamTest(
        Suite + "should return true when the tool has an output-error state",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_true_when_the_tool_has_an_output_error_state()
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
                    "type": "tool-getWeatherInformation",
                    "toolCallId": "call_6iy0GxZ9R4VDI5MKohXxV48y",
                    "state": "output-error",
                    "input": {
                      "city": "New York"
                    },
                    "errorText": "Unable to get weather information"
                  },
                  {
                    "type": "text",
                    "text": "The current weather in New York is windy.",
                    "state": "done"
                  }
                ]
              }
            ]
            """
        );

        Assert.True(result);
    }

    [UpstreamTest(
        Suite + "should return true when text precedes the last completed tool call",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_true_when_text_precedes_the_last_completed_tool_call()
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
                    "text": "I will check the weather.",
                    "state": "done"
                  },
                  {
                    "type": "tool-getWeatherInformation",
                    "toolCallId": "call_1",
                    "state": "output-available",
                    "input": {
                      "city": "New York"
                    },
                    "output": "windy"
                  }
                ]
              }
            ]
            """
        );

        Assert.True(result);
    }

    [UpstreamTest(
        Suite + "should return true when dynamic tool call is complete",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_true_when_dynamic_tool_call_is_complete()
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
                    "toolName": "getDynamicWeather",
                    "toolCallId": "call_dynamic_123",
                    "state": "output-available",
                    "input": {
                      "location": "San Francisco"
                    },
                    "output": "sunny"
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
                    "type": "tool-getWeatherInformation",
                    "toolCallId": "call_1",
                    "state": "output-available",
                    "input": {
                      "city": "New York"
                    },
                    "output": "checking weather station",
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
                    "type": "dynamic-tool",
                    "toolName": "getDynamicWeather",
                    "toolCallId": "call_1",
                    "state": "output-available",
                    "input": {
                      "location": "San Francisco"
                    },
                    "output": "checking weather station",
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
        Suite + "should return false when dynamic tool call is still streaming input",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_false_when_dynamic_tool_call_is_still_streaming_input()
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
                    "toolName": "getDynamicWeather",
                    "toolCallId": "call_dynamic_123",
                    "state": "input-streaming",
                    "input": {
                      "location": "San Francisco"
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
        Suite + "should return false when dynamic tool call has input but no output",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_false_when_dynamic_tool_call_has_input_but_no_output()
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
                    "toolName": "getDynamicWeather",
                    "toolCallId": "call_dynamic_123",
                    "state": "input-available",
                    "input": {
                      "location": "San Francisco"
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
        Suite + "should return true when dynamic tool call has an error",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_true_when_dynamic_tool_call_has_an_error()
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
                    "toolName": "getDynamicWeather",
                    "toolCallId": "call_dynamic_123",
                    "state": "output-error",
                    "input": {
                      "location": "San Francisco"
                    },
                    "errorText": "Failed to fetch weather data"
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
            + "should return true when mixing regular and dynamic tool calls and all are complete",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_true_when_mixing_regular_and_dynamic_tool_calls_and_all_are_complete()
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
                    "type": "tool-getWeatherInformation",
                    "toolCallId": "call_regular_123",
                    "state": "output-available",
                    "input": {
                      "city": "New York"
                    },
                    "output": "windy"
                  },
                  {
                    "type": "dynamic-tool",
                    "toolName": "getDynamicWeather",
                    "toolCallId": "call_dynamic_123",
                    "state": "output-available",
                    "input": {
                      "location": "San Francisco"
                    },
                    "output": "sunny"
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
            + "should return false when mixing regular and dynamic tool calls and some are incomplete",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_false_when_mixing_regular_and_dynamic_tool_calls_and_some_are_incomplete()
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
                    "type": "tool-getWeatherInformation",
                    "toolCallId": "call_regular_123",
                    "state": "output-available",
                    "input": {
                      "city": "New York"
                    },
                    "output": "windy"
                  },
                  {
                    "type": "dynamic-tool",
                    "toolName": "getDynamicWeather",
                    "toolCallId": "call_dynamic_123",
                    "state": "input-available",
                    "input": {
                      "location": "San Francisco"
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
        Suite
            + "should return true for multi-step sequence where last step has complete dynamic tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_true_for_multi_step_sequence_where_last_step_has_complete_dynamic_tool_calls()
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
                    "type": "tool-getLocation",
                    "toolCallId": "call_location_123",
                    "state": "output-available",
                    "input": {},
                    "output": "New York"
                  },
                  {
                    "type": "step-start"
                  },
                  {
                    "type": "dynamic-tool",
                    "toolName": "getDynamicWeather",
                    "toolCallId": "call_dynamic_456",
                    "state": "output-available",
                    "input": {
                      "location": "New York"
                    },
                    "output": "cloudy"
                  },
                  {
                    "type": "text",
                    "text": "The current weather in New York is cloudy.",
                    "state": "done"
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
            + "should return false for multi-step sequence where last step has incomplete dynamic tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_false_for_multi_step_sequence_where_last_step_has_incomplete_dynamic_tool_calls()
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
                    "type": "tool-getLocation",
                    "toolCallId": "call_location_123",
                    "state": "output-available",
                    "input": {},
                    "output": "New York"
                  },
                  {
                    "type": "step-start"
                  },
                  {
                    "type": "dynamic-tool",
                    "toolName": "getDynamicWeather",
                    "toolCallId": "call_dynamic_456",
                    "state": "input-streaming",
                    "input": {
                      "location": "New York"
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
        Suite + "should return false for complete provider executed tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_return_false_for_complete_provider_executed_tool_calls()
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
                    "type": "tool-web_search",
                    "input": {
                      "query": "New York weather"
                    },
                    "state": "output-available",
                    "output": [],
                    "toolCallId": "srvtoolu_01KSMqkKSbgKhCwGZHQDaV48",
                    "providerExecuted": true
                  },
                  {
                    "type": "text",
                    "text": "The current weather in New York is windy.",
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
