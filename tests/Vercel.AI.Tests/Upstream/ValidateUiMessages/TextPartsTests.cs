// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.ConvertToModelMessages;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ValidateUiMessages.ValidateUiMessagesSupport;

namespace Vercel.AI.Tests.ValidateUiMessages;

/// <summary>Port of <c>validate-ui-messages.test.ts</c> &gt; <c>validateUIMessages &gt; text parts</c>.</summary>
[Collection("LogWarnings")]
public sealed class TextPartsTests : WarningsOff
{
    private const string Suite = Prefix + "validateUIMessages > text parts::";

    [UpstreamTest(
        Suite + "should validate a user message with a text part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_a_user_message_with_a_text_part()
    {
        const string json = """
            [{"id":"1","role":"user","parts":[{"type":"text","text":"Hello, world!"}]}]
            """;

        AssertMessages(json, await Validate(json));
    }
}
