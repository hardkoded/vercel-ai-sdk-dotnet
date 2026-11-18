// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.ConvertToModelMessages;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ValidateUiMessages.ValidateUiMessagesSupport;

namespace Vercel.AI.Tests.ValidateUiMessages;

/// <summary>Port of <c>validate-ui-messages.test.ts</c> &gt; <c>validateUIMessages &gt; valid JSON values in metadata</c>.</summary>
[Collection("LogWarnings")]
public sealed class ValidJsonValuesInMetadataTests : WarningsOff
{
    private const string Suite = Prefix + "validateUIMessages > valid JSON values in metadata::";

    private static string NestedValue() =>
        "[" + string.Join(",", Enumerable.Repeat(NestedArrays(100), 20)) + "]";

    private static async Task<JsonNode> ValidatedPart(string partJson)
    {
        var messages = await Validate(Assistant("[" + partJson + "]"));
        return Render(messages)[0]!["parts"]![0]!;
    }

    // Values are JSON elements, so the port cannot return the same object. The test checks that the value is kept.
    [UpstreamTest(
        Suite + "should not copy JSON values in provider metadata",
        Coverage = UpstreamCoverage.Partial,
        Commit = "718824cd5959a15afe6d29765799659078e302c6"
    )]
    [Fact]
    public async Task Should_not_copy_JSON_values_in_provider_metadata()
    {
        var value = NestedValue();
        var part = await ValidatedPart(
            """{"type":"text","text":"Hello","providerMetadata":{"provider":{"key":"""
                + value
                + "}}}"
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """{"type":"text","text":"Hello","providerMetadata":{"provider":{"key":"""
                        + value
                        + "}}}",
                    null,
                    new System.Text.Json.JsonDocumentOptions { MaxDepth = 4096 }
                ),
                part
            )
        );
    }

    [UpstreamTest(
        Suite + "should not copy JSON values in tool part metadata",
        Coverage = UpstreamCoverage.Partial,
        Commit = "718824cd5959a15afe6d29765799659078e302c6"
    )]
    [Fact]
    public async Task Should_not_copy_JSON_values_in_tool_part_metadata()
    {
        var call = NestedValue();
        var result = NestedValue();
        var tool = NestedValue();
        var part = await ValidatedPart(
            """{"type":"tool-foo","toolCallId":"1","state":"output-available","input":{},"output":"result","callProviderMetadata":{"provider":{"key":"""
                + call
                + """}},"resultProviderMetadata":{"provider":{"key":"""
                + result
                + """}},"toolMetadata":{"key":"""
                + tool
                + "}}"
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(call, null, Deep),
                part["callProviderMetadata"]!["provider"]!["key"]
            )
        );
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(result, null, Deep),
                part["resultProviderMetadata"]!["provider"]!["key"]
            )
        );
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(tool, null, Deep), part["toolMetadata"]!["key"])
        );
    }

    [UpstreamTest(
        Suite + "should not copy JSON values in citation provider metadata",
        Coverage = UpstreamCoverage.Partial,
        Commit = "718824cd5959a15afe6d29765799659078e302c6"
    )]
    [Fact]
    public async Task Should_not_copy_JSON_values_in_citation_provider_metadata()
    {
        var value = NestedValue();
        var part = await ValidatedPart(
            """{"type":"text","text":"Hello","citations":[{"source":{"type":"source","sourceType":"url","id":"source-1","url":"https://example.com","providerMetadata":{"provider":{"key":"""
                + value
                + "}}}}]}"
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(value, null, Deep),
                part["citations"]![0]!["source"]!["providerMetadata"]!["provider"]!["key"]
            )
        );
    }

    private static readonly System.Text.Json.JsonDocumentOptions Deep = new() { MaxDepth = 4096 };

    [UpstreamTest(
        Suite + "should remove __proto__ keys from JSON values in provider metadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = "718824cd5959a15afe6d29765799659078e302c6"
    )]
    [Fact]
    public async Task Should_remove_proto_keys_from_JSON_values_in_provider_metadata()
    {
        var part = await ValidatedPart(
            """{"type":"text","text":"Hello","providerMetadata":{"provider":{"key":[{"__proto__":{"polluted":true},"a":1}]}}}"""
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("""[{"a":1}]"""),
                part["providerMetadata"]!["provider"]!["key"]
            )
        );
    }

    // The "an undefined provider" case has no JSON equivalent.
    [UpstreamTest(
        Suite + "should reject provider metadata that is %s",
        Coverage = UpstreamCoverage.Partial,
        Commit = "718824cd5959a15afe6d29765799659078e302c6"
    )]
    [Theory]
    [InlineData("[]")]
    [InlineData("""{"provider":"value"}""")]
    public async Task Should_reject_provider_metadata_that_is_not_a_record_of_records(
        string providerMetadata
    ) => await AssertRejected(providerMetadata);

    [UpstreamTest(
        Suite + "should reject tool metadata that is not an object",
        Coverage = UpstreamCoverage.Covered,
        Commit = "718824cd5959a15afe6d29765799659078e302c6"
    )]
    [Fact]
    public async Task Should_reject_tool_metadata_that_is_not_an_object()
    {
        var result = await SafeValidate(
            Assistant(
                """[{"type":"dynamic-tool","toolName":"foo","toolCallId":"1","state":"input-available","input":{},"toolMetadata":["value"]}]"""
            )
        );

        Assert.False(result.Success);
        Assert.Equal(["0/parts/0/toolMetadata"], GetIssuePaths(result.Error));
    }

    private static async Task AssertRejected(string providerMetadata)
    {
        var result = await SafeValidate(
            Assistant(
                """[{"type":"text","text":"Hello","providerMetadata":""" + providerMetadata + "}]"
            )
        );

        Assert.False(result.Success);
        var cause = Assert.IsType<UIMessagesSchemaException>(result.Error!.InnerException);
        Assert.Equal(["invalid_type"], cause.Issues.Select(issue => issue.Code));
    }
}
