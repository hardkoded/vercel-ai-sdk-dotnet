// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.ConvertToModelMessages;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ValidateUiMessages.ValidateUiMessagesSupport;

namespace Vercel.AI.Tests.ValidateUiMessages;

/// <summary>Port of <c>validate-ui-messages.test.ts</c> &gt; <c>validateUIMessages &gt; step start parts</c>.</summary>
[Collection("LogWarnings")]
public sealed class StepStartPartsTests : WarningsOff
{
    private const string Suite = Prefix + "validateUIMessages > step start parts::";

    [UpstreamTest(
        Suite + "should validate an assistant message with a step start part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_an_assistant_message_with_a_step_start_part()
    {
        const string json = """
            [{"id":"1","role":"assistant","parts":[{"type":"step-start"}]}]
            """;

        AssertMessages(json, await Validate(json));
    }
}
