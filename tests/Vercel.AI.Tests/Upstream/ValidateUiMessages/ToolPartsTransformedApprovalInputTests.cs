// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.ConvertToModelMessages;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ValidateUiMessages.ToolPartsTests;
using static Vercel.AI.Tests.ValidateUiMessages.ValidateUiMessagesSupport;

namespace Vercel.AI.Tests.ValidateUiMessages;

/// <summary>Port of <c>validate-ui-messages.test.ts</c> &gt; <c>validateUIMessages &gt; tool parts &gt; transformed approval input in %s state</c>.</summary>
[Collection("LogWarnings")]
public sealed class ToolPartsTransformedApprovalInputTests : WarningsOff
{
    private const string Suite =
        Prefix + "validateUIMessages > tool parts > transformed approval input in %s state::";

    public static TheoryData<string> States =>
        [
            "approval-requested",
            "approval-responded",
            "output-available",
            "output-denied",
            "output-error",
        ];

    public static TheoryData<string, string> MismatchedInputs
    {
        get
        {
            var data = new TheoryData<string, string>();
            foreach (
                var state in new[]
                {
                    "approval-requested",
                    "approval-responded",
                    "output-available",
                    "output-denied",
                    "output-error",
                }
            )
            {
                data.Add(state, "{\"count\":4}");
                data.Add(state, "{\"count\":\"not-a-number\"}");
            }

            return data;
        }
    }

    private static string CreateMessages(string state, string input)
    {
        var extra =
            (state == "output-available" ? ",\"output\":\"ok\"" : "")
            + (state == "output-error" ? ",\"errorText\":\"failed\"" : "");
        var approved =
            state == "approval-requested"
                ? ""
                : $",\"approved\":{(state != "output-denied" ? "true" : "false")}";
        return Assistant(
            $"[{{\"type\":\"tool-count\",\"toolCallId\":\"1\",\"state\":\"{state}\",\"input\":{input}{extra},\"approval\":{{\"id\":\"approval-1\",\"inputSchemaInput\":{{\"count\":\"3\"}}{approved}}}}}]"
        );
    }

    [UpstreamTest(
        Suite + "should preserve input matching the reconstructed schema output",
        Note = "The input schema is JSON Schema, so ExperimentalRefineToolInput stands in for z.string().transform(Number)."
    )]
    [Theory]
    [MemberData(nameof(States))]
    public async Task Should_preserve_input_matching_the_reconstructed_schema_output(string state)
    {
        var json = CreateMessages(state, "{\"count\":3}");

        AssertMessages(json, await Validate(json, CountTools(), refine: CountRefine()));
    }

    [UpstreamTest(
        Suite + "should not expose mismatched input %j as a validated static tool part",
        Note = "The input schema is JSON Schema, so ExperimentalRefineToolInput stands in for z.string().transform(Number)."
    )]
    [Theory]
    [MemberData(nameof(MismatchedInputs))]
    public async Task Should_not_expose_mismatched_input_as_a_validated_static_tool_part(
        string state,
        string input
    )
    {
        var json = CreateMessages(state, input);
        var messages = Messages(json);

        if (state == "output-error")
        {
            var result = await Validate(json, CountTools(), refine: CountRefine());
            var part = Assert.IsType<ToolUIPart>(result[0].Parts[0]);
            Assert.True(part.IsDynamic);
            Assert.Equal("count", part.ToolName);
            Assert.Equal(input, part.Input!.Value.GetRawText());
        }
        else
        {
            var error = await Assert.ThrowsAsync<TypeValidationException>(() =>
                Validate(json, CountTools(), refine: CountRefine())
            );
            Assert.Contains("does not match the output reconstructed", error.Message);
        }

        Assert.Equal(input, ((ToolUIPart)messages[0].Parts[0]).Input!.Value.GetRawText());
    }
}
