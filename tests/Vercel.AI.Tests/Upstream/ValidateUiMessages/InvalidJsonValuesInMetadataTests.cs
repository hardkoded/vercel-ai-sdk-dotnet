// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.ConvertToModelMessages;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ValidateUiMessages.ValidateUiMessagesSupport;

namespace Vercel.AI.Tests.ValidateUiMessages;

/// <summary>Port of <c>validate-ui-messages.test.ts</c> &gt; <c>validateUIMessages &gt; invalid JSON values in metadata</c>.</summary>
[Collection("LogWarnings")]
public sealed class InvalidJsonValuesInMetadataTests : WarningsOff
{
    private const string Suite = Prefix + "validateUIMessages > invalid JSON values in metadata::";

    // JSON.parse returns Infinity for numbers that are out of range:
    private const string NonFiniteNumber = "1e999";

    [UpstreamTest(
        Suite + "should report an invalid JSON value in provider metadata",
        Coverage = UpstreamCoverage.Partial,
        Commit = "9307549d51938172036744037d359735d214fe36"
    )]
    [Fact]
    public async Task Should_report_an_invalid_JSON_value_in_provider_metadata()
    {
        var result = await SafeValidate(
            Assistant(
                """[{"type":"text","text":"Hello","providerMetadata":{"provider":{"key":[1e999]}}}]"""
            )
        );

        Assert.False(result.Success);
        var error = Assert.IsType<TypeValidationException>(result.Error);
        Assert.Equal(["0/parts/0/providerMetadata/provider/key/0"], GetIssuePaths(error));
        var issue = ((UIMessagesSchemaException)error.InnerException!).Issues.Single();
        Assert.Equal("custom", issue.Code);
        Assert.Equal(
            "Invalid JSON value. Expected null, a string, a finite number, a boolean, an array, or an object.",
            issue.Message
        );
    }

    [UpstreamTest(
        Suite + "should report a single issue for a deeply nested invalid JSON value",
        Coverage = UpstreamCoverage.Covered,
        Commit = "9307549d51938172036744037d359735d214fe36"
    )]
    [Fact]
    public async Task Should_report_a_single_issue_for_a_deeply_nested_invalid_JSON_value()
    {
        var result = await SafeValidate(
            Assistant(
                """[{"type":"text","text":"Hello","providerMetadata":{"provider":{"key":"""
                    + NestedArrays(300, NonFiniteNumber)
                    + "}}}]"
            )
        );

        Assert.False(result.Success);
        Assert.Equal(
            [
                "0/parts/0/providerMetadata/provider/key/"
                    + string.Join("/", Enumerable.Repeat("0", 300)),
            ],
            GetIssuePaths(result.Error)
        );
        Assert.True(result.Error!.Message.Length < 10_000);
    }

    [UpstreamTest(
        Suite + "should report a single issue for many invalid JSON values",
        Coverage = UpstreamCoverage.Covered,
        Commit = "9307549d51938172036744037d359735d214fe36"
    )]
    [Fact]
    public async Task Should_report_a_single_issue_for_many_invalid_JSON_values()
    {
        var messages = Assistant(
            """[{"type":"tool-foo","toolCallId":"1","state":"input-available","input":{},"callProviderMetadata":{"provider":{"key":"""
                + NestedArrays(100, string.Join(",", Enumerable.Repeat(NonFiniteNumber, 1000)))
                + "}}}]"
        );

        var result = await SafeValidate(messages);

        Assert.False(result.Success);
        Assert.Single(GetIssuePaths(result.Error));
        Assert.True(result.Error!.Message.Length < messages.Length + 5000);
    }

    [UpstreamTest(
        Suite + "should report an invalid JSON value in tool metadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = "9307549d51938172036744037d359735d214fe36"
    )]
    [Fact]
    public async Task Should_report_an_invalid_JSON_value_in_tool_metadata()
    {
        var result = await SafeValidate(
            Assistant(
                """[{"type":"dynamic-tool","toolName":"foo","toolCallId":"1","state":"input-available","input":{},"toolMetadata":{"key":[[1e999]]}}]"""
            )
        );

        Assert.False(result.Success);
        Assert.Equal(["0/parts/0/toolMetadata/key/0/0"], GetIssuePaths(result.Error));
    }

    [UpstreamTest(
        Suite + "should report an invalid JSON value in citation provider metadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = "9307549d51938172036744037d359735d214fe36"
    )]
    [Fact]
    public async Task Should_report_an_invalid_JSON_value_in_citation_provider_metadata()
    {
        var result = await SafeValidate(
            Assistant(
                """[{"type":"text","text":"Hello","citations":[{"source":{"type":"source","sourceType":"url","id":"source-1","url":"https://example.com","providerMetadata":{"provider":{"key":[1e999]}}}}]}]"""
            )
        );

        Assert.False(result.Success);
        Assert.Equal(
            ["0/parts/0/citations/0/source/providerMetadata/provider/key/0"],
            GetIssuePaths(result.Error)
        );
    }

    [UpstreamTest(
        Suite + "should keep ignoring unknown properties with invalid JSON values",
        Coverage = UpstreamCoverage.Covered,
        Commit = "9307549d51938172036744037d359735d214fe36"
    )]
    [Fact]
    public async Task Should_keep_ignoring_unknown_properties_with_invalid_JSON_values()
    {
        var messages = await Validate(
            Assistant(
                """[{"type":"text","text":"Hello","unknown":1e999,"citations":[{"source":{"type":"source","sourceType":"url","id":"source-1","url":"https://example.com"},"unknown":1e999}]}]"""
            )
        );

        AssertMessages(
            Assistant(
                """[{"type":"text","text":"Hello","citations":[{"source":{"type":"source","sourceType":"url","id":"source-1","url":"https://example.com"}}]}]"""
            ),
            messages
        );
    }
}
