// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.ConvertToModelMessages;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ValidateUiMessages.ValidateUiMessagesSupport;

namespace Vercel.AI.Tests.ValidateUiMessages;

/// <summary>Port of <c>validate-ui-messages.test.ts</c> &gt; <c>validateUIMessages &gt; metadata</c>.</summary>
[Collection("LogWarnings")]
public sealed class MetadataTests : WarningsOff
{
    private const string Suite = Prefix + "validateUIMessages > metadata::";

    private static readonly JsonNode FooSchema = JsonNode.Parse(
        "{\"type\":\"object\",\"properties\":{\"foo\":{\"type\":\"string\"}},\"required\":[\"foo\"]}"
    )!;

    [UpstreamTest(
        Suite + "should validate a user message with metadata when no metadata schema is provided",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_a_user_message_with_metadata_when_no_metadata_schema_is_provided()
    {
        const string json = """
            [{"id":"1","role":"user","metadata":{"foo":"bar"},"parts":[{"type":"text","text":"Hello, world!"}]}]
            """;

        AssertMessages(json, await Validate(json));
    }

    [UpstreamTest(
        Suite + "should validate a user message with metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_a_user_message_with_metadata()
    {
        const string json = """
            [{"id":"1","role":"user","metadata":{"foo":"bar"},"parts":[{"type":"text","text":"Hello, world!"}]}]
            """;

        AssertMessages(json, await Validate(json, metadataSchema: FooSchema));
    }

    [UpstreamTest(
        Suite + "should throw type validation error when metadata is invalid",
        Note = "Upstream's message carries a zod issue list. The port's message carries the schema validator's text."
    )]
    [Fact]
    public async Task Should_throw_type_validation_error_when_metadata_is_invalid()
    {
        var error = await Assert.ThrowsAsync<TypeValidationException>(() =>
            Validate(
                """
                [{"id":"1","role":"user","metadata":{"foo":123},"parts":[{"type":"text","text":"Hello, world!"}]}]
                """,
                metadataSchema: FooSchema
            )
        );

        Assert.StartsWith(
            "Type validation failed for messages[0].metadata (id: \"1\"): Value: {\"foo\":123}.\nError message: ",
            error.Message
        );
    }

    [UpstreamTest(
        Suite + "should validate text part with provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_text_part_with_provider_metadata()
    {
        const string json = """
            [{"id":"1","role":"user","parts":[{"type":"text","text":"Hello, world!","providerMetadata":{"someProvider":{"custom":"metadata"}}}]}]
            """;

        AssertMessages(json, await Validate(json));
    }
}
