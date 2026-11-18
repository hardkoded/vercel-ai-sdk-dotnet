// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.ConvertToModelMessages;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ValidateUiMessages.ValidateUiMessagesSupport;

namespace Vercel.AI.Tests.ValidateUiMessages;

/// <summary>Port of <c>validate-ui-messages.test.ts</c> &gt; <c>validateUIMessages &gt; invalid messages and parts</c>.</summary>
[Collection("LogWarnings")]
public sealed class InvalidMessagesAndPartsTests : WarningsOff
{
    private const string Suite = Prefix + "validateUIMessages > invalid messages and parts::";

    [UpstreamTest(
        Suite + "should only report the issues of the schema for the part type",
        Coverage = UpstreamCoverage.Partial,
        Commit = "9307549d51938172036744037d359735d214fe36"
    )]
    [Fact]
    public async Task Should_only_report_the_issues_of_the_schema_for_the_part_type()
    {
        var result = await SafeValidate("""[{"id":"1","role":"user","parts":[{"type":"text"}]}]""");

        Assert.False(result.Success);
        var error = Assert.IsType<TypeValidationException>(result.Error);
        Assert.StartsWith("Type validation failed: Value: ", error.Message);
        Assert.Equal(["0/parts/0/text"], GetIssuePaths(error));
        Assert.Contains("Invalid input: expected string, received undefined", error.Message);
    }

    [UpstreamTest(
        Suite + "should report a part with an unknown type",
        Coverage = UpstreamCoverage.Partial,
        Commit = "9307549d51938172036744037d359735d214fe36"
    )]
    [Fact]
    public async Task Should_report_a_part_with_an_unknown_type()
    {
        var result = await SafeValidate(
            """[{"id":"1","role":"user","parts":[{"type":"unknown"}]}]"""
        );

        Assert.False(result.Success);
        var error = Assert.IsType<TypeValidationException>(result.Error);
        Assert.Equal(["0/parts/0/type"], GetIssuePaths(error));
        Assert.Contains(
            "Invalid UI message part type. Expected a known part type (for example \"text\" or \"dynamic-tool\") or a type starting with \"data-\" or \"tool-\".",
            ((UIMessagesSchemaException)error.InnerException!).Issues[0].Message
        );
    }

    [UpstreamTest(
        Suite + "should report a tool part with an unknown state",
        Coverage = UpstreamCoverage.Partial,
        Commit = "9307549d51938172036744037d359735d214fe36"
    )]
    [Fact]
    public async Task Should_report_a_tool_part_with_an_unknown_state()
    {
        var result = await SafeValidate(
            """[{"id":"1","role":"assistant","parts":[{"type":"tool-foo","toolCallId":"1","state":"unknown"}]}]"""
        );

        Assert.False(result.Success);
        var error = Assert.IsType<TypeValidationException>(result.Error);
        Assert.Equal(["0/parts/0/state"], GetIssuePaths(error));
        Assert.Contains(
            "Invalid discriminator value. Expected 'input-streaming' | 'input-available' | 'approval-requested' | 'approval-responded' | 'output-available' | 'output-error' | 'output-denied'",
            error.InnerException!.Message
        );
    }

    [UpstreamTest(
        Suite + "should stop at the first invalid message",
        Coverage = UpstreamCoverage.Covered,
        Commit = "9307549d51938172036744037d359735d214fe36"
    )]
    [Fact]
    public async Task Should_stop_at_the_first_invalid_message()
    {
        var result = await SafeValidate(
            """[{"id":"1","role":"user","parts":[{"type":"text","text":"valid"}]},{},{}]"""
        );

        Assert.False(result.Success);
        Assert.Equal(["1/id", "1/role", "1/parts"], GetIssuePaths(result.Error));
    }

    [UpstreamTest(
        Suite + "should stop at the first invalid part",
        Coverage = UpstreamCoverage.Covered,
        Commit = "9307549d51938172036744037d359735d214fe36"
    )]
    [Fact]
    public async Task Should_stop_at_the_first_invalid_part()
    {
        var result = await SafeValidate(
            """[{"id":"1","role":"user","parts":[{"type":"text","text":"valid"},{},{}]}]"""
        );

        Assert.False(result.Success);
        Assert.Equal(["0/parts/1/type"], GetIssuePaths(result.Error));
    }

    [UpstreamTest(
        Suite + "should stop at the first invalid steering message part",
        Coverage = UpstreamCoverage.Covered,
        Commit = "9307549d51938172036744037d359735d214fe36"
    )]
    [Fact]
    public async Task Should_stop_at_the_first_invalid_steering_message_part()
    {
        var result = await SafeValidate(
            """[{"id":"1","role":"assistant","parts":[{"type":"steering-message","id":"steer-1","parts":[{"type":"text","text":"valid"},{},{}]}]}]"""
        );

        Assert.False(result.Success);
        Assert.Equal(["0/parts/0/parts/1/type"], GetIssuePaths(result.Error));
    }

    [UpstreamTest(
        Suite + "should stop at the first invalid citation",
        Coverage = UpstreamCoverage.Covered,
        Commit = "9307549d51938172036744037d359735d214fe36"
    )]
    [Fact]
    public async Task Should_stop_at_the_first_invalid_citation()
    {
        var result = await SafeValidate(
            """[{"id":"1","role":"assistant","parts":[{"type":"text","text":"valid","citations":[{},{}]}]}]"""
        );

        Assert.False(result.Success);
        Assert.Equal(["0/parts/0/citations/0/source"], GetIssuePaths(result.Error));
    }

    [UpstreamTest(
        Suite + "should not grow the error with the number of invalid parts",
        Coverage = UpstreamCoverage.Covered,
        Commit = "9307549d51938172036744037d359735d214fe36"
    )]
    [Fact]
    public async Task Should_not_grow_the_error_with_the_number_of_invalid_parts()
    {
        var messages =
            """[{"id":"1","role":"user","parts":["""
            + string.Join(",", Enumerable.Repeat("{}", 2500))
            + "]}]";

        var result = await SafeValidate(messages);

        Assert.False(result.Success);
        Assert.Equal(["0/parts/0/type"], GetIssuePaths(result.Error));
        // the error message contains the validated value once:
        Assert.True(result.Error!.Message.Length < messages.Length + 1000);
    }
}
