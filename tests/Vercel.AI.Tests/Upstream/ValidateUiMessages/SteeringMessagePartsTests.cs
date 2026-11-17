// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.ConvertToModelMessages;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ValidateUiMessages.ValidateUiMessagesSupport;

namespace Vercel.AI.Tests.ValidateUiMessages;

/// <summary>Port of <c>validate-ui-messages.test.ts</c> &gt; <c>validateUIMessages &gt; steering message parts</c>.</summary>
[Collection("LogWarnings")]
public sealed class SteeringMessagePartsTests : WarningsOff
{
    private const string Suite = Prefix + "validateUIMessages > steering message parts::";

    [UpstreamTest(
        Suite + "should validate an assistant message with a steering message part",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    [Fact]
    public async Task Should_validate_an_assistant_message_with_a_steering_message_part()
    {
        const string json = """
            [{"id":"1","role":"assistant","parts":[{"type":"steering-message","id":"steer-1","parts":[{"type":"text","text":"I meant Paris, Texas."},{"type":"file","mediaType":"application/pdf","url":"","providerReference":{"openai":"file-1"}}]},{"type":"step-start"}]}]
            """;

        AssertMessages(json, await Validate(json));
    }
}
