// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.ConvertToModelMessages;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ValidateUiMessages.ValidateUiMessagesSupport;

namespace Vercel.AI.Tests.ValidateUiMessages;

/// <summary>Port of <c>validate-ui-messages.test.ts</c> &gt; <c>validateUIMessages &gt; data parts</c>.</summary>
[Collection("LogWarnings")]
public sealed class DataPartsTests : WarningsOff
{
    private const string Suite = Prefix + "validateUIMessages > data parts::";

    private static JsonNode Schema(string property, string type) =>
        JsonNode.Parse(
            "{\"type\":\"object\",\"properties\":{\""
                + property
                + "\":{\"type\":\""
                + type
                + "\"}},\"required\":[\""
                + property
                + "\"]}"
        )!;

    [UpstreamTest(
        Suite + "should validate an assistant message with two data parts",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_an_assistant_message_with_two_data_parts()
    {
        const string json = """
            [{"id":"1","role":"assistant","parts":[{"type":"data-foo","data":{"foo":"bar"}},{"type":"data-bar","data":{"bar":123}}]}]
            """;

        var messages = await Validate(
            json,
            dataSchemas: new Dictionary<string, JsonNode>
            {
                ["foo"] = Schema("foo", "string"),
                ["bar"] = Schema("bar", "number"),
            }
        );

        AssertMessages(json, messages);
    }

    [UpstreamTest(
        Suite + "should throw type validation error when data is invalid",
        Note = "Upstream's message carries a zod issue list. The port's message carries the schema validator's text."
    )]
    [Fact]
    public async Task Should_throw_type_validation_error_when_data_is_invalid()
    {
        var error = await Assert.ThrowsAsync<TypeValidationException>(() =>
            Validate(
                """
                [{"id":"1","role":"assistant","parts":[{"type":"data-foo","data":{"foo":123}}]}]
                """,
                dataSchemas: new Dictionary<string, JsonNode> { ["foo"] = Schema("foo", "string") }
            )
        );

        Assert.StartsWith(
            "Type validation failed for messages[0].parts[0].data (foo): Value: {\"foo\":123}.\nError message: ",
            error.Message
        );
    }

    [UpstreamTest(
        Suite + "should throw type validation error when there is no data schema for a data part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_throw_type_validation_error_when_there_is_no_data_schema_for_a_data_part()
    {
        var error = await Assert.ThrowsAsync<TypeValidationException>(() =>
            Validate(
                """
                [{"id":"1","role":"assistant","parts":[{"type":"data-bar","data":{"foo":"bar"}}]}]
                """,
                dataSchemas: new Dictionary<string, JsonNode> { ["foo"] = Schema("foo", "string") }
            )
        );

        Assert.Equal(
            "Type validation failed for messages[0].parts[0].data (bar): Value: {\"foo\":\"bar\"}.\nError message: No data schema found for data part bar",
            error.Message
        );
    }
}
