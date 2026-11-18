// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.ConvertToModelMessages;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ValidateUiMessages.ValidateUiMessagesSupport;

namespace Vercel.AI.Tests.ValidateUiMessages;

/// <summary>Port of <c>validate-ui-messages.test.ts</c> &gt; <c>safeValidateUIMessages</c>.</summary>
[Collection("LogWarnings")]
public sealed class SafeValidateUIMessagesTests : WarningsOff
{
    private const string Suite = Prefix + "safeValidateUIMessages::";

    [UpstreamTest(
        Suite + "should return success result for valid messages",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_return_success_result_for_valid_messages()
    {
        var result = await SafeValidate(UserText());

        Assert.True(result.Success);
        AssertMessages(UserText(), result.Data!);
    }

    [UpstreamTest(
        Suite + "should return failure result when messages parameter is null",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_return_failure_result_when_messages_parameter_is_null()
    {
        var result = await Ai.SafeValidateUIMessagesAsync(
            new ValidateUIMessagesOptions { Messages = null }
        );

        Assert.False(result.Success);
        var error = Assert.IsType<InvalidArgumentException>(result.Error);
        Assert.Equal(
            "Invalid argument for parameter messages: messages parameter must be provided",
            error.Message
        );
    }

    [UpstreamTest(
        Suite + "should return failure result when messages array is empty",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_return_failure_result_when_messages_array_is_empty()
    {
        var result = await SafeValidate("[]");

        Assert.False(result.Success);
        var error = Assert.IsType<TypeValidationException>(result.Error);
        Assert.Contains("Type validation failed", error.Message);
        Assert.Contains("Messages array must not be empty", error.Message);
    }

    [UpstreamTest(
        Suite + "should return failure result when message has empty parts array",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_return_failure_result_when_message_has_empty_parts_array()
    {
        var result = await SafeValidate("""[{"id":"1","role":"user","parts":[]}]""");

        Assert.False(result.Success);
        var error = Assert.IsType<TypeValidationException>(result.Error);
        Assert.Contains("Type validation failed", error.Message);
    }

    [UpstreamTest(
        Suite
            + "should return success result for chat ending with assistant message with empty parts array",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_return_success_result_for_chat_ending_with_assistant_message_with_empty_parts_array()
    {
        const string json =
            """[{"id":"1","role":"user","parts":[{"type":"text","text":"Hello"}]},{"id":"2","role":"assistant","parts":[]}]""";

        var result = await SafeValidate(json);

        Assert.True(result.Success);
        AssertMessages(json, result.Data!);
    }

    [UpstreamTest(
        Suite + "should return failure result when metadata validation fails",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_return_failure_result_when_metadata_validation_fails()
    {
        var result = await SafeValidate(
            """[{"id":"1","role":"user","metadata":{"foo":123},"parts":[{"type":"text","text":"Hello, world!"}]}]""",
            metadataSchema: JsonNode.Parse(
                """{"type":"object","properties":{"foo":{"type":"string"}},"required":["foo"]}"""
            )
        );

        Assert.False(result.Success);
        var error = Assert.IsType<TypeValidationException>(result.Error);
        Assert.Contains("Type validation failed", error.Message);
    }

    [UpstreamTest(
        Suite + "should return failure result when tool input validation fails",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_return_failure_result_when_tool_input_validation_fails()
    {
        var result = await SafeValidate(
            Assistant(
                """[{"type":"tool-foo","toolCallId":"1","state":"input-available","input":{"foo":123},"providerExecuted":true}]"""
            ),
            TestTools()
        );

        Assert.False(result.Success);
        var error = Assert.IsType<TypeValidationException>(result.Error);
        Assert.Contains("Type validation failed", error.Message);
    }

    [UpstreamTest(
        Suite
            + "should return completed empty input as a dynamic tool part when it no longer matches the tool schema",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_return_completed_empty_input_as_a_dynamic_tool_part_when_it_no_longer_matches_the_tool_schema()
    {
        var result = await SafeValidate(
            Assistant(
                """[{"type":"tool-foo","toolCallId":"1","state":"output-available","input":{},"output":{"result":"success"}}]"""
            ),
            TestTools()
        );

        Assert.True(result.Success);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """{"type":"dynamic-tool","dynamic":false,"toolName":"foo","toolCallId":"1","state":"output-available","input":{},"output":{"result":"success"}}"""
                ),
                Render(result.Data!)[0]!["parts"]![0]
            )
        );
    }

    [UpstreamTest(
        Suite + "should return unavailable terminal tools as dynamic tool parts",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_return_unavailable_terminal_tools_as_dynamic_tool_parts()
    {
        var current = new Tool(
            "current",
            null,
            Json(
                """{"type":"object","properties":{"current":{"type":"string"}},"required":["current"]}"""
            ),
            null
        );

        var result = await SafeValidate(
            Assistant(
                """[{"type":"tool-removed","toolCallId":"1","state":"output-error","input":{"previous":"value"},"errorText":"Tool execution failed"}]"""
            ),
            new Dictionary<string, Tool> { ["current"] = current }
        );

        Assert.True(result.Success);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """{"type":"dynamic-tool","dynamic":false,"toolName":"removed","toolCallId":"1","state":"output-error","input":{"previous":"value"},"errorText":"Tool execution failed"}"""
                ),
                Render(result.Data!)[0]!["parts"]![0]
            )
        );
    }

    [UpstreamTest(
        Suite + "should return failure result when data schema is missing",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_return_failure_result_when_data_schema_is_missing()
    {
        var result = await SafeValidate(
            Assistant("""[{"type":"data-bar","data":{"foo":"bar"}}]"""),
            dataSchemas: new Dictionary<string, JsonNode>
            {
                ["foo"] = JsonNode.Parse(
                    """{"type":"object","properties":{"foo":{"type":"string"}},"required":["foo"]}"""
                )!,
            }
        );

        Assert.False(result.Success);
        var error = Assert.IsType<TypeValidationException>(result.Error);
        Assert.Contains("No data schema found for data part bar", error.Message);
    }

    [UpstreamTest(
        Suite + "should return failure result for invalid message structure",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_return_failure_result_for_invalid_message_structure()
    {
        var result = await SafeValidate("""[{"role":"user"}]""");

        Assert.False(result.Success);
        Assert.IsType<TypeValidationException>(result.Error);
    }
}
