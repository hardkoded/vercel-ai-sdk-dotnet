// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>onToolCall is executed</c>.</summary>
public sealed class OnToolCallIsExecutedTests
{
    private const string Prefix = "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(Prefix + "onToolCall is executed::should call the update function twice with the correct arguments", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_call_the_update_function_twice_with_the_correct_arguments()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-available","toolCallId":"tool-call-id","toolName":"tool-name","input":{"city":"London"}},{"type":"finish-step"},{"type":"finish"}]
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
                      "input": {
                        "city": "London"
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
    [UpstreamTest(Prefix + "onToolCall is executed::should have the correct final message state", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_the_correct_final_message_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-available","toolCallId":"tool-call-id","toolName":"tool-name","input":{"city":"London"}},{"type":"finish-step"},{"type":"finish"}]
            """, new ProcessUIMessageStreamOptions(harness.RunUpdateMessageJob, error => throw error) { OnToolCall = _ => Task.CompletedTask });

        ProcessUIMessageStreamHarness.AssertJson("""
            {
              "id": "msg-123",
              "parts": [
                {
                  "type": "step-start"
                },
                {
                  "input": {
                    "city": "London"
                  },
                  "state": "input-available",
                  "toolCallId": "tool-call-id",
                  "type": "tool-tool-name"
                }
              ],
              "role": "assistant"
            }
            """, harness.Message);
    }
}
