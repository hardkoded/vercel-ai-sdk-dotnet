// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>preliminary tool results</c>.</summary>
public sealed class PreliminaryToolResultsTests
{
    private const string Prefix =
        "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(
        Prefix
            + "preliminary tool results::should call the update function with the correct arguments",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_the_update_function_with_the_correct_arguments()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync(
            """
            [{"type":"start"},{"type":"start-step"},{"input":{"city":"San Francisco"},"toolCallId":"call-1","toolName":"cityAttractions","type":"tool-input-available"},{"output":{"status":"loading","text":"Getting weather for San Francisco"},"preliminary":true,"toolCallId":"call-1","type":"tool-output-available"},{"output":{"status":"success","temperature":72,"text":"The weather in San Francisco is 72°F"},"preliminary":true,"toolCallId":"call-1","type":"tool-output-available"},{"output":{"status":"success","temperature":72,"text":"The weather in San Francisco is 72°F"},"toolCallId":"call-1","type":"tool-output-available"},{"type":"finish-step"},{"type":"finish"}]
            """
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            [
              {
                "message": {
                  "id": "msg-123",
                  "parts": [
                    {
                      "type": "step-start"
                    },
                    {
                      "input": {
                        "city": "San Francisco"
                      },
                      "state": "input-available",
                      "toolCallId": "call-1",
                      "type": "tool-cityAttractions"
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
                        "city": "San Francisco"
                      },
                      "output": {
                        "status": "loading",
                        "text": "Getting weather for San Francisco"
                      },
                      "preliminary": true,
                      "state": "output-available",
                      "toolCallId": "call-1",
                      "type": "tool-cityAttractions"
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
                        "city": "San Francisco"
                      },
                      "output": {
                        "status": "success",
                        "temperature": 72,
                        "text": "The weather in San Francisco is 72°F"
                      },
                      "preliminary": true,
                      "state": "output-available",
                      "toolCallId": "call-1",
                      "type": "tool-cityAttractions"
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
                        "city": "San Francisco"
                      },
                      "output": {
                        "status": "success",
                        "temperature": 72,
                        "text": "The weather in San Francisco is 72°F"
                      },
                      "state": "output-available",
                      "toolCallId": "call-1",
                      "type": "tool-cityAttractions"
                    }
                  ],
                  "role": "assistant"
                }
              }
            ]
            """,
            harness.WriteCalls
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preliminary tool results::should have the correct final message state",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_have_the_correct_final_message_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync(
            """
            [{"type":"start"},{"type":"start-step"},{"input":{"city":"San Francisco"},"toolCallId":"call-1","toolName":"cityAttractions","type":"tool-input-available"},{"output":{"status":"loading","text":"Getting weather for San Francisco"},"preliminary":true,"toolCallId":"call-1","type":"tool-output-available"},{"output":{"status":"success","temperature":72,"text":"The weather in San Francisco is 72°F"},"preliminary":true,"toolCallId":"call-1","type":"tool-output-available"},{"output":{"status":"success","temperature":72,"text":"The weather in San Francisco is 72°F"},"toolCallId":"call-1","type":"tool-output-available"},{"type":"finish-step"},{"type":"finish"}]
            """
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            [
              {
                "type": "step-start"
              },
              {
                "input": {
                  "city": "San Francisco"
                },
                "output": {
                  "status": "success",
                  "temperature": 72,
                  "text": "The weather in San Francisco is 72°F"
                },
                "state": "output-available",
                "toolCallId": "call-1",
                "type": "tool-cityAttractions"
              }
            ]
            """,
            harness.Parts
        );
    }
}
