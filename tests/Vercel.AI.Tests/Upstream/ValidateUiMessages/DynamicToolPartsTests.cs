// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Tests.ConvertToModelMessages;
using Vercel.AI.Ui;
using Vercel.AI.Util;
using static Vercel.AI.Tests.ValidateUiMessages.ValidateUiMessagesSupport;

namespace Vercel.AI.Tests.ValidateUiMessages;

/// <summary>Port of <c>validate-ui-messages.test.ts</c> &gt; <c>validateUIMessages &gt; dynamic tool parts</c>.</summary>
[Collection("LogWarnings")]
public sealed class DynamicToolPartsTests : WarningsOff
{
    private const string Suite = Prefix + "validateUIMessages > dynamic tool parts::";

    [UpstreamTest(
        Suite + "should preserve titles on static and dynamic tool parts in every state",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_preserve_titles_on_static_and_dynamic_tool_parts_in_every_state()
    {
        var stateDetails = new (string State, string Details)[]
        {
            ("input-streaming", "\"input\":{\"value\":\"test\"}"),
            ("input-available", "\"input\":{\"value\":\"test\"}"),
            (
                "approval-requested",
                "\"input\":{\"value\":\"test\"},\"approval\":{\"id\":\"approval-requested\"}"
            ),
            (
                "approval-responded",
                "\"input\":{\"value\":\"test\"},\"approval\":{\"id\":\"approval-responded\",\"approved\":true}"
            ),
            (
                "output-available",
                "\"input\":{\"value\":\"test\"},\"output\":{\"result\":\"success\"}"
            ),
            (
                "output-error",
                "\"input\":{\"value\":\"test\"},\"errorText\":\"Tool execution failed\""
            ),
            (
                "output-denied",
                "\"input\":{\"value\":\"test\"},\"approval\":{\"id\":\"output-denied\",\"approved\":false}"
            ),
        };
        var parts = string.Join(
            ",",
            stateDetails.SelectMany(item =>
                new[]
                {
                    $"{{\"type\":\"tool-foo\",\"toolCallId\":\"static-{item.State}\",\"title\":\"Example tool\",\"state\":\"{item.State}\",{item.Details}}}",
                    $"{{\"type\":\"dynamic-tool\",\"toolName\":\"foo\",\"toolCallId\":\"dynamic-{item.State}\",\"title\":\"Example tool\",\"state\":\"{item.State}\",{item.Details}}}",
                }
            )
        );
        var json = Assistant("[" + parts + "]");

        AssertMessages(json, await Validate(json));
    }

    [UpstreamTest(
        Suite
            + "should validate an assistant message with a dynamic tool part in input-streaming state",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_an_assistant_message_with_a_dynamic_tool_part_in_input_streaming_state()
    {
        var json = Assistant(
            """[{"type":"dynamic-tool","toolName":"foo","toolCallId":"1","state":"input-streaming","input":{"foo":"bar"},"rawInput":"{\"foo\":\"bar"}]"""
        );

        AssertMessages(json, await Validate(json));
    }

    [UpstreamTest(
        Suite
            + "should validate an assistant message with a dynamic tool part in input-available state",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_an_assistant_message_with_a_dynamic_tool_part_in_input_available_state()
    {
        var json = Assistant(
            "[{\"type\":\"dynamic-tool\",\"toolName\":\"foo\",\"toolCallId\":\"1\",\"state\":\"input-available\",\"input\":{\"foo\":\"bar\"}}]"
        );

        AssertMessages(json, await Validate(json));
    }

    [UpstreamTest(
        Suite
            + "should validate an assistant message with a dynamic tool part in output-available state",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_an_assistant_message_with_a_dynamic_tool_part_in_output_available_state()
    {
        var json = Assistant(
            "[{\"type\":\"dynamic-tool\",\"toolName\":\"foo\",\"toolCallId\":\"1\",\"state\":\"output-available\",\"input\":{\"foo\":\"bar\"},\"output\":{\"result\":\"success\"}}]"
        );

        AssertMessages(json, await Validate(json));
    }

    [UpstreamTest(
        Suite
            + "should validate an assistant message with a dynamic tool part in output-error state",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_an_assistant_message_with_a_dynamic_tool_part_in_output_error_state()
    {
        var json = Assistant(
            "[{\"type\":\"dynamic-tool\",\"toolName\":\"foo\",\"toolCallId\":\"1\",\"state\":\"output-error\",\"input\":{\"foo\":\"bar\"},\"errorText\":\"Tool execution failed\"}]"
        );

        AssertMessages(json, await Validate(json));
    }

    [UpstreamTest(
        Suite
            + "should validate a dynamic tool part in output-error state when input key is absent",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_a_dynamic_tool_part_in_output_error_state_when_input_key_is_absent()
    {
        var warnings = new List<LogWarningsOptions>();
        LogWarnings.Logger = new Action<LogWarningsOptions>(warnings.Add);
        var json = Assistant(
            "[{\"type\":\"dynamic-tool\",\"toolName\":\"foo\",\"toolCallId\":\"1\",\"state\":\"output-error\",\"rawInput\":{\"foo\":\"bar\"},\"errorText\":\"Tool execution failed\"}]"
        );

        var result = await Validate(json);

        AssertMessages(json, result);
        var logged = Assert.Single(warnings);
        var warning = Assert.IsType<DeprecatedWarning>(Assert.Single(logged.Warnings));
        Assert.Equal("rawInput in output-error UI message parts", warning.Setting);
        Assert.Equal(
            "Use the \"input\" field instead. The \"rawInput\" field will be removed in the next major version.",
            warning.Message
        );
    }
}
