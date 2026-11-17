// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Ui;
using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages &gt; when converting dynamic tool invocations</c>.</summary>
[Collection("LogWarnings")]
public sealed class WhenConvertingDynamicToolInvocationsTests : WarningsOff
{
    private const string Suite =
        Prefix + "convertToModelMessages > when converting dynamic tool invocations::";

    [UpstreamTest(
        Suite + "should convert a dynamic tool invocation",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_a_dynamic_tool_invocation()
    {
        var result = Convert(
            UiMessages(
                """
                [
                  {
                    "role": "assistant",
                    "parts": [
                      { "type": "step-start" },
                      { "type": "dynamic-tool", "toolName": "screenshot", "state": "output-available", "toolCallId": "call-1", "input": { "value": "value-1" }, "output": "result-1" }
                    ]
                  },
                  { "role": "user", "parts": [{ "type": "text", "text": "Thanks!" }] }
                ]
                """
            ),
            new ConvertToModelMessagesOptions { IgnoreIncompleteToolCalls = true }
        );

        AssertEqual(
            """
            [
              { "content": [{ "input": { "value": "value-1" }, "toolCallId": "call-1", "toolName": "screenshot", "type": "tool-call" }], "role": "assistant" },
              { "content": [{ "output": { "type": "text", "value": "result-1" }, "toolCallId": "call-1", "toolName": "screenshot", "type": "tool-result" }], "role": "tool" },
              { "content": [{ "text": "Thanks!", "type": "text" }], "role": "user" }
            ]
            """,
            result
        );
    }
}
