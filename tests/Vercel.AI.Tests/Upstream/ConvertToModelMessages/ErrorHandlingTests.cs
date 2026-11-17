// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Ui;
using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages &gt; error handling</c>.</summary>
[Collection("LogWarnings")]
public sealed class ErrorHandlingTests : WarningsOff
{
    private const string Suite = Prefix + "convertToModelMessages > error handling::";

    [UpstreamTest(
        Suite + "should throw an error for unhandled roles",
        Coverage = UpstreamCoverage.Partial,
        Note = "A role is an enum here, so the unknown role is an undefined enum value and the message names its number instead of 'unknown'."
    )]
    [Fact]
    public void Should_throw_an_error_for_unhandled_roles()
    {
        var exception = Assert.ThrowsAny<Exception>(() =>
            Convert(
                new[]
                {
                    new UIMessage(
                        (UIMessageRole)99,
                        new UIMessagePart[] { new TextUIPart("unknown role message") }
                    ),
                }
            )
        );

        Assert.Contains("Unsupported role: ", exception.Message);
    }
}
