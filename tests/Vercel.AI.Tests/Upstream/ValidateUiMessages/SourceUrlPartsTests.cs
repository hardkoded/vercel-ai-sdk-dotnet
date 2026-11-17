// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.ConvertToModelMessages;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ValidateUiMessages.ValidateUiMessagesSupport;

namespace Vercel.AI.Tests.ValidateUiMessages;

/// <summary>Port of <c>validate-ui-messages.test.ts</c> &gt; <c>validateUIMessages &gt; source url parts</c>.</summary>
[Collection("LogWarnings")]
public sealed class SourceUrlPartsTests : WarningsOff
{
    private const string Suite = Prefix + "validateUIMessages > source url parts::";

    [UpstreamTest(
        Suite + "should validate an assistant message with a source url part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_an_assistant_message_with_a_source_url_part()
    {
        const string json = """
            [{"id":"1","role":"assistant","parts":[{"type":"source-url","sourceId":"1","url":"https://example.com","title":"Example"}]}]
            """;

        AssertMessages(json, await Validate(json));
    }
}
