// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>tool input error</c>.</summary>
public sealed class ToolInputErrorTests
{
    private const string Prefix =
        "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(
        Prefix + "tool input error::should not warn when creating a static output-error part",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_warn_when_creating_a_static_output_error_part()
    {
        var warnings = new List<LogWarningsOptions>();
        var previous = LogWarnings.Logger;
        LogWarnings.Logger = new Action<LogWarningsOptions>(warnings.Add);
        try
        {
            var harness = new ProcessUIMessageStreamHarness();
            harness.CreateState("msg-123", null);
            await harness.ProcessAsync(
                """
                [{"type":"start"},{"type":"start-step"},{"toolCallId":"call-1","toolName":"cityAttractions","type":"tool-input-start"},{"inputTextDelta":"{ \"cities\": \"San Francisco\" }","toolCallId":"call-1","type":"tool-input-delta"},{"errorText":"Invalid input for tool cityAttractions","input":"{ \"cities\": \"San Francisco\" }","toolCallId":"call-1","toolName":"cityAttractions","type":"tool-input-error"},{"errorText":"Invalid input for tool cityAttractions","toolCallId":"call-1","type":"tool-output-error"},{"type":"finish-step"},{"type":"finish"}]
                """
            );

            Assert.Empty(warnings);
        }
        finally
        {
            LogWarnings.Logger = previous;
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "tool input error::should call the update function with the correct arguments",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_the_update_function_with_the_correct_arguments()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync(
            """
            [{"type":"start"},{"type":"start-step"},{"toolCallId":"call-1","toolName":"cityAttractions","type":"tool-input-start"},{"inputTextDelta":"{ \"cities\": \"San Francisco\" }","toolCallId":"call-1","type":"tool-input-delta"},{"errorText":"Invalid input for tool cityAttractions","input":"{ \"cities\": \"San Francisco\" }","toolCallId":"call-1","toolName":"cityAttractions","type":"tool-input-error"},{"errorText":"Invalid input for tool cityAttractions","toolCallId":"call-1","type":"tool-output-error"},{"type":"finish-step"},{"type":"finish"}]
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
                      "state": "input-streaming",
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
                        "cities": "San Francisco"
                      },
                      "rawInput": "{ \"cities\": \"San Francisco\" }",
                      "state": "input-streaming",
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
                      "errorText": "Invalid input for tool cityAttractions",
                      "input": "{ \"cities\": \"San Francisco\" }",
                      "state": "output-error",
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
                      "errorText": "Invalid input for tool cityAttractions",
                      "input": "{ \"cities\": \"San Francisco\" }",
                      "state": "output-error",
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
        Prefix + "tool input error::should have the correct final message state",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_have_the_correct_final_message_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync(
            """
            [{"type":"start"},{"type":"start-step"},{"toolCallId":"call-1","toolName":"cityAttractions","type":"tool-input-start"},{"inputTextDelta":"{ \"cities\": \"San Francisco\" }","toolCallId":"call-1","type":"tool-input-delta"},{"errorText":"Invalid input for tool cityAttractions","input":"{ \"cities\": \"San Francisco\" }","toolCallId":"call-1","toolName":"cityAttractions","type":"tool-input-error"},{"errorText":"Invalid input for tool cityAttractions","toolCallId":"call-1","type":"tool-output-error"},{"type":"finish-step"},{"type":"finish"}]
            """
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            [
              {
                "type": "step-start"
              },
              {
                "errorText": "Invalid input for tool cityAttractions",
                "input": "{ \"cities\": \"San Francisco\" }",
                "state": "output-error",
                "toolCallId": "call-1",
                "type": "tool-cityAttractions"
              }
            ]
            """,
            harness.Parts
        );
    }
}
