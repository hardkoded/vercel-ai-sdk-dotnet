// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.ConvertToModelMessages;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ValidateUiMessages.ValidateUiMessagesSupport;

namespace Vercel.AI.Tests.ValidateUiMessages;

/// <summary>Port of <c>validate-ui-messages.test.ts</c> &gt; <c>validateUIMessages &gt; custom parts</c>.</summary>
[Collection("LogWarnings")]
public sealed class CustomPartsTests : WarningsOff
{
    private const string Suite = Prefix + "validateUIMessages > custom parts::";

    [UpstreamTest(
        Suite + "should validate an assistant message with a custom part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_an_assistant_message_with_a_custom_part()
    {
        const string json = """
            [{"id":"1","role":"assistant","parts":[{"type":"custom","kind":"test-provider.compaction","providerMetadata":{"openai":{"itemId":"cmp_123"}}}]}]
            """;

        AssertMessages(json, await Validate(json));
    }

    [UpstreamTest(
        Suite + "should validate an assistant message with a custom part without providerMetadata",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_an_assistant_message_with_a_custom_part_without_providerMetadata()
    {
        const string json = """
            [{"id":"1","role":"assistant","parts":[{"type":"custom","kind":"openai.compaction"}]}]
            """;

        AssertMessages(json, await Validate(json));
    }
}
