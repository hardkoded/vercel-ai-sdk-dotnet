// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.ConvertToModelMessages;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ValidateUiMessages.ValidateUiMessagesSupport;

namespace Vercel.AI.Tests.ValidateUiMessages;

/// <summary>Port of <c>validate-ui-messages.test.ts</c> &gt; <c>validateUIMessages &gt; file parts</c>.</summary>
[Collection("LogWarnings")]
public sealed class FilePartsTests : WarningsOff
{
    private const string Suite = Prefix + "validateUIMessages > file parts::";

    [UpstreamTest(
        Suite + "should validate an assistant message with a file part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_an_assistant_message_with_a_file_part()
    {
        const string json = """
            [{"id":"1","role":"assistant","parts":[{"type":"file","mediaType":"text/plain","url":"https://example.com"}]}]
            """;

        AssertMessages(json, await Validate(json));
    }

    [UpstreamTest(
        Suite + "should preserve provider references on file parts",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_preserve_provider_references_on_file_parts()
    {
        const string json = """
            [{"id":"1","role":"user","parts":[{"type":"file","mediaType":"text/csv","filename":"sample.csv","url":"data:text/csv;base64,bW9udGgscmV2ZW51ZQ==","providerReference":{"anthropic":"file_abc123"},"providerMetadata":{"anthropic":{"containerUpload":true}}}]}]
            """;

        AssertMessages(json, await Validate(json));
    }
}
