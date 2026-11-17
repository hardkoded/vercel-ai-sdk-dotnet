// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Ui;
using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages &gt; pending tool approvals</c>.</summary>
[Collection("LogWarnings")]
public sealed class PendingToolApprovalsTests : WarningsOff
{
    private const string Suite = Prefix + "convertToModelMessages > pending tool approvals::";

    [UpstreamTest(
        Suite + "should ignore a pending approval when followed by a user message",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_ignore_a_pending_approval_when_followed_by_a_user_message()
    {
        var result = Convert(
            UiMessages(
                """
                [
                  {
                    "role": "assistant",
                    "parts": [
                      { "type": "tool-weather", "state": "approval-requested", "toolCallId": "call-awaiting-approval", "input": { "city": "Tokyo" }, "approval": { "id": "approval-1" } }
                    ]
                  },
                  { "role": "user", "parts": [{ "type": "text", "text": "Use a different city instead." }] }
                ]
                """
            )
        );

        AssertEqual(
            """
            [
              { "role": "user", "content": [{ "type": "text", "text": "Use a different city instead." }] }
            ]
            """,
            result
        );
    }
}
