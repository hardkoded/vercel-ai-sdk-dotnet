// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.ConvertToModelMessages;
using Vercel.AI.Ui;
using Vercel.AI.Util;
using static Vercel.AI.Tests.ValidateUiMessages.ValidateUiMessagesSupport;

namespace Vercel.AI.Tests.ValidateUiMessages;

/// <summary>Port of <c>validate-ui-messages.test.ts</c> &gt; <c>validateUIMessages &gt; tool parts</c>.</summary>
[Collection("LogWarnings")]
public sealed class ToolPartsTests : WarningsOff
{
    private const string Suite = Prefix + "validateUIMessages > tool parts::";

    private const string RawInputWarningMessage =
        "Use the \"input\" field instead. The \"rawInput\" field will be removed in the next major version.";

    private static Task<IReadOnlyList<UIMessage>> ValidateForAgent(
        string json,
        Dictionary<string, Tool>? tools = null
    ) =>
        UIMessageValidation.ValidateUIMessagesForAgentAsync(
            new ValidateUIMessagesOptions { Messages = Messages(json), Tools = tools }
        );

    private static async Task<string> InputError(
        string json,
        Dictionary<string, Tool>? tools = null
    ) => (await Assert.ThrowsAsync<TypeValidationException>(() => Validate(json, tools))).Message;

    [UpstreamTest(
        Suite + "should validate an assistant message with a tool part in input-streaming state",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_an_assistant_message_with_a_tool_part_in_input_streaming_state()
    {
        var json = Assistant(
            """[{"type":"tool-foo","toolCallId":"1","state":"input-streaming","input":{"foo":"bar"},"providerExecuted":true}]"""
        );

        AssertMessages(json, await Validate(json));
    }

    [UpstreamTest(
        Suite + "should validate an assistant message with a tool part in input-available state",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_an_assistant_message_with_a_tool_part_in_input_available_state()
    {
        var json = Assistant(
            """[{"type":"tool-foo","toolCallId":"1","state":"input-available","input":{"foo":"bar"},"providerExecuted":true}]"""
        );

        AssertMessages(json, await Validate(json));
    }

    [UpstreamTest(
        Suite + "should validate an assistant message with a tool part in output-available state",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_an_assistant_message_with_a_tool_part_in_output_available_state()
    {
        var json = Assistant(
            """[{"type":"tool-foo","toolCallId":"1","state":"output-available","input":{"foo":"bar"},"output":{"result":"success"},"providerExecuted":true}]"""
        );

        AssertMessages(json, await Validate(json));
    }

    [UpstreamTest(
        Suite + "should validate an assistant message with a tool part in output-error state",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_an_assistant_message_with_a_tool_part_in_output_error_state()
    {
        var json = Assistant(
            """[{"type":"tool-foo","toolCallId":"1","state":"output-error","input":{"foo":"bar"},"errorText":"Tool execution failed","providerExecuted":true}]"""
        );

        AssertMessages(json, await Validate(json));
    }

    [UpstreamTest(
        Suite + "should validate tool input when state is input-available",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_tool_input_when_state_is_input_available()
    {
        var json = Assistant(
            """[{"type":"tool-foo","toolCallId":"1","state":"input-available","input":{"foo":"bar"},"providerExecuted":true}]"""
        );

        AssertMessages(json, await Validate(json, TestTools()));
    }

    [UpstreamTest(
        Suite + "should validate tool input when state is output-available",
        Note = "Upstream's message carries a zod issue list. The port's message carries the schema validator's text."
    )]
    [Fact]
    public async Task Should_validate_tool_input_when_state_is_output_available()
    {
        var message = await InputError(
            Assistant(
                """[{"type":"tool-foo","toolCallId":"1","state":"output-available","input":{"foo":123},"output":{"result":"success"}}]"""
            ),
            TestTools()
        );

        Assert.StartsWith(
            "Type validation failed for messages[0].parts[0].input (foo, id: \"1\"): Value: {\"foo\":123}.\nError message: ",
            message
        );
    }

    [UpstreamTest(
        Suite + "should validate tool input when state is approval-requested",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_tool_input_when_state_is_approval_requested()
    {
        var message = await InputError(
            Assistant(
                """[{"type":"tool-foo","toolCallId":"1","state":"approval-requested","input":{"foo":123},"approval":{"id":"approval-1"}}]"""
            ),
            TestTools()
        );

        Assert.Contains("Type validation failed for messages[0].parts[0].input", message);
    }

    [UpstreamTest(
        Suite + "should validate tool input when state is approval-responded",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_tool_input_when_state_is_approval_responded()
    {
        var message = await InputError(
            Assistant(
                """[{"type":"tool-foo","toolCallId":"1","state":"approval-responded","input":{"foo":123},"approval":{"id":"approval-1","approved":true}}]"""
            ),
            TestTools()
        );

        Assert.Contains("Type validation failed for messages[0].parts[0].input", message);
    }

    [UpstreamTest(
        Suite + "should reject transformed approval input that does not match its schema input",
        Note = "The input schema is JSON Schema, so ExperimentalRefineToolInput stands in for z.string().transform(Number)."
    )]
    [Fact]
    public async Task Should_reject_transformed_approval_input_that_does_not_match_its_schema_input()
    {
        var error = await Assert.ThrowsAsync<TypeValidationException>(() =>
            Validate(
                Assistant(
                    """[{"type":"tool-count","toolCallId":"call-1","state":"approval-responded","input":{"count":4},"approval":{"id":"approval-1","approved":true,"inputSchemaInput":{"count":"3"}}}]"""
                ),
                CountTools(),
                refine: CountRefine()
            )
        );

        Assert.Contains(
            "Tool input does not match the output reconstructed from inputSchemaInput.",
            error.Message
        );
    }

    [UpstreamTest(
        Suite + "should compare transformed approval input after applying input refinement",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_compare_transformed_approval_input_after_applying_input_refinement()
    {
        var refinedTool = new Tool(
            "refined",
            null,
            Json(
                """{"type":"object","properties":{"value":{"type":"string"}},"required":["value"]}"""
            ),
            null
        );

        var messages = await Validate(
            Assistant(
                """[{"type":"tool-refined","toolCallId":"call-1","state":"approval-responded","input":{"value":"trimmed"},"approval":{"id":"approval-1","approved":true,"inputSchemaInput":{"value":" trimmed "}}}]"""
            ),
            new Dictionary<string, Tool> { ["refined"] = refinedTool },
            refine: new Dictionary<string, Func<JsonElement, Task<JsonElement>>>
            {
                ["refined"] = input =>
                    Task.FromResult(
                        Json(
                            "{\"value\":"
                                + JsonSerializer.Serialize(
                                    input.GetProperty("value").GetString()!.Trim()
                                )
                                + "}"
                        )
                    ),
            }
        );

        var part = Assert.IsType<ToolUIPart>(messages[0].Parts[0]);
        Assert.Equal("""{"value":"trimmed"}""", part.Input!.Value.GetRawText());
    }

    [UpstreamTest(
        Suite + "should validate tool input when state is output-denied",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_tool_input_when_state_is_output_denied()
    {
        var message = await InputError(
            Assistant(
                """[{"type":"tool-foo","toolCallId":"1","state":"output-denied","input":{"foo":123},"approval":{"id":"approval-1","approved":false}}]"""
            ),
            TestTools()
        );

        Assert.Contains("Type validation failed for messages[0].parts[0].input", message);
    }

    [UpstreamTest(
        Suite + "should normalize persisted aborted tool history from issue #18425",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_normalize_persisted_aborted_tool_history_from_issue_18425()
    {
        var createArtifactTool = new Tool(
            "create_artifact",
            null,
            Json(
                """{"type":"object","properties":{"identifier":{"type":"string"},"type":{"type":"string","enum":["application/vnd.react","text/html"]},"language":{"type":"string"},"code":{"type":"string"}},"required":["identifier","type","language","code"]}"""
            ),
            null
        );

        var messages = await Validate(
            """
            [
              {"id":"u1","role":"user","parts":[{"type":"text","text":"make me a chart"}]},
              {"id":"a1","role":"assistant","parts":[
                {"type":"step-start"},
                {"type":"tool-create_artifact","toolCallId":"toolu_demo_aborted","state":"output-available","input":{},"output":"{\"error\":\"Tool was aborted by the user.\"}"}
              ]},
              {"id":"u2","role":"user","parts":[{"type":"text","text":"are you working?"}]}
            ]
            """,
            new Dictionary<string, Tool> { ["create_artifact"] = createArtifactTool }
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """{"type":"dynamic-tool","dynamic":false,"toolName":"create_artifact","toolCallId":"toolu_demo_aborted","state":"output-available","input":{},"output":"{\"error\":\"Tool was aborted by the user.\"}"}"""
                ),
                Render(messages)[1]!["parts"]![1]
            )
        );
        AssertMessages(
            """[{"id":"u2","role":"user","parts":[{"type":"text","text":"are you working?"}]}]""",
            new[] { messages[2] }
        );
    }

    [UpstreamTest(
        Suite + "should validate output when an output-available tool call has empty input",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_output_when_an_output_available_tool_call_has_empty_input()
    {
        var noArgumentTool = new Tool(
            "noArgument",
            null,
            Json("""{"type":"object","properties":{}}"""),
            null
        )
        {
            OutputSchema = Json(
                """{"type":"object","properties":{"result":{"type":"string"}},"required":["result"]}"""
            ),
        };

        var message = await InputError(
            Assistant(
                """[{"type":"tool-noArgument","toolCallId":"1","state":"output-available","input":{},"output":{}}]"""
            ),
            new Dictionary<string, Tool> { ["noArgument"] = noArgumentTool }
        );

        Assert.Contains("Type validation failed for messages[0].parts[0].output", message);
    }

    [UpstreamTest(
        Suite + "should validate tool output when state is output-available",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_tool_output_when_state_is_output_available()
    {
        var message = await InputError(
            Assistant(
                """[{"type":"tool-foo","toolCallId":"1","state":"output-available","input":{"foo":"bar"},"output":{}}]"""
            ),
            TestTools()
        );

        Assert.Contains("Type validation failed for messages[0].parts[0].output", message);
    }

    [UpstreamTest(
        Suite + "should preserve result provider metadata when state is output-available",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_preserve_result_provider_metadata_when_state_is_output_available()
    {
        var messages = await Validate(
            Assistant(
                """[{"type":"tool-foo","toolCallId":"1","state":"output-available","input":{"foo":"bar"},"output":{"result":"success"},"resultProviderMetadata":{"testProvider":{"itemId":"result-item"}}}]"""
            ),
            TestTools()
        );

        var part = Assert.IsType<ToolUIPart>(messages[0].Parts[0]);
        Assert.Equal("foo", part.ToolName);
        Assert.Equal(ToolUIPartState.OutputAvailable, part.State);
        Assert.Equal(
            """{"itemId":"result-item"}""",
            part.ResultProviderMetadata!["testProvider"].GetRawText()
        );
    }

    [UpstreamTest(
        Suite + "should represent schema-incompatible output-error input as a dynamic tool part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_represent_schema_incompatible_output_error_input_as_a_dynamic_tool_part()
    {
        var messages = await Validate(
            Assistant(
                """[{"type":"tool-foo","toolCallId":"1","state":"output-error","input":{"foo":123},"errorText":"AI_InvalidToolInputError","providerExecuted":false}]"""
            ),
            TestTools()
        );

        AssertMessages(
            """[{"id":"1","role":"assistant","parts":[{"errorText":"AI_InvalidToolInputError","dynamic":false,"input":{"foo":123},"providerExecuted":false,"state":"output-error","toolCallId":"1","toolName":"foo","type":"dynamic-tool"}]}]""",
            messages
        );
    }

    [UpstreamTest(
        Suite + "should preserve result provider metadata when state is output-error",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_preserve_result_provider_metadata_when_state_is_output_error()
    {
        var messages = await Validate(
            Assistant(
                """[{"type":"tool-foo","toolCallId":"1","state":"output-error","input":{"foo":"bar"},"errorText":"Tool execution failed","providerExecuted":true,"resultProviderMetadata":{"testProvider":{"itemId":"result-item"}}}]"""
            ),
            TestTools()
        );

        var part = Assert.IsType<ToolUIPart>(messages[0].Parts[0]);
        Assert.Equal("foo", part.ToolName);
        Assert.Equal(ToolUIPartState.OutputError, part.State);
        Assert.Equal(
            """{"itemId":"result-item"}""",
            part.ResultProviderMetadata!["testProvider"].GetRawText()
        );
    }

    [UpstreamTest(
        Suite
            + "should skip tool input validation when state is output-error and there is no input",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_skip_tool_input_validation_when_state_is_output_error_and_there_is_no_input()
    {
        var json = Assistant(
            """[{"type":"tool-foo","toolCallId":"1","state":"output-error","errorText":"Tool input validation failed","providerExecuted":false}]"""
        );

        AssertMessages(json, await Validate(json, TestTools()));
    }

    [UpstreamTest(
        Suite + "should validate a tool part in output-error state when input key is absent",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_a_tool_part_in_output_error_state_when_input_key_is_absent()
    {
        var json = Assistant(
            """[{"type":"tool-foo","toolCallId":"1","state":"output-error","rawInput":{"foo":"bar"},"errorText":"Tool input validation failed"}]"""
        );

        AssertMessages(json, await Validate(json));
    }

    [UpstreamTest(
        Suite + "should preserve rawInput when state is output-error",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_preserve_rawInput_when_state_is_output_error()
    {
        var warnings = new List<LogWarningsOptions>();
        LogWarnings.Logger = new Action<LogWarningsOptions>(warnings.Add);
        var json = Assistant(
            """[{"type":"tool-foo","toolCallId":"1","state":"output-error","rawInput":{"foo":"bar"},"errorText":"Tool input validation failed","providerExecuted":false}]"""
        );

        var result = await Validate(json, TestTools());

        AssertMessages(json, result);
        var warning = Assert.IsType<DeprecatedWarning>(
            Assert.Single(Assert.Single(warnings).Warnings)
        );
        Assert.Equal("rawInput in output-error UI message parts", warning.Setting);
        Assert.Equal(RawInputWarningMessage, warning.Message);
    }

    [UpstreamTest(
        Suite + "should throw error when no tool schema is found",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_throw_error_when_no_tool_schema_is_found()
    {
        var message = await InputError(
            Assistant(
                """[{"type":"tool-bar","toolCallId":"1","state":"input-available","input":{"foo":"bar"},"providerExecuted":true}]"""
            ),
            TestTools()
        );

        Assert.Equal(
            "Type validation failed for messages[0].parts[0].input (bar, id: \"1\"): Value: {\"foo\":\"bar\"}.\nError message: No tool schema found for tool part bar",
            message
        );
    }

    [UpstreamTest(
        Suite + "should represent terminal calls from missing tools as dynamic tool parts",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_represent_terminal_calls_from_missing_tools_as_dynamic_tool_parts()
    {
        var messages = await Validate(
            Assistant(
                """
                [
                  {"type":"tool-bar","toolCallId":"1","state":"output-available","input":{"foo":"bar"},"output":{"result":"success"},"providerExecuted":true},
                  {"type":"tool-bar","toolCallId":"2","state":"output-error","input":{"foo":"bar"},"errorText":"Tool execution failed","providerExecuted":true},
                  {"type":"tool-bar","toolCallId":"3","state":"output-denied","input":{"foo":"bar"},"approval":{"id":"approval-1","approved":false}}
                ]
                """
            ),
            TestTools()
        );

        AssertMessages(
            Assistant(
                """
                [
                  {"type":"dynamic-tool","dynamic":false,"toolName":"bar","toolCallId":"1","state":"output-available","input":{"foo":"bar"},"output":{"result":"success"},"providerExecuted":true},
                  {"type":"dynamic-tool","dynamic":false,"toolName":"bar","toolCallId":"2","state":"output-error","input":{"foo":"bar"},"errorText":"Tool execution failed","providerExecuted":true},
                  {"type":"dynamic-tool","dynamic":false,"toolName":"bar","toolCallId":"3","state":"output-denied","input":{"foo":"bar"},"approval":{"id":"approval-1","approved":false}}
                ]
                """
            ),
            messages
        );
    }

    [UpstreamTest(
        Suite
            + "should represent terminal calls from unavailable agent tools as dynamic tool parts",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_represent_terminal_calls_from_unavailable_agent_tools_as_dynamic_tool_parts()
    {
        var messages = await ValidateForAgent(
            Assistant(
                """
                [
                  {"type":"tool-bar","toolCallId":"1","state":"output-available","input":{},"output":{"result":"success"}},
                  {"type":"tool-bar","toolCallId":"2","state":"output-error","errorText":"Tool execution failed"},
                  {"type":"tool-bar","toolCallId":"3","state":"output-denied","input":{},"approval":{"id":"approval-1","approved":false}}
                ]
                """
            ),
            TestTools()
        );

        AssertMessages(
            Assistant(
                """
                [
                  {"type":"dynamic-tool","dynamic":false,"toolName":"bar","toolCallId":"1","state":"output-available","input":{},"output":{"result":"success"}},
                  {"type":"dynamic-tool","dynamic":false,"toolName":"bar","toolCallId":"2","state":"output-error","errorText":"Tool execution failed"},
                  {"type":"dynamic-tool","dynamic":false,"toolName":"bar","toolCallId":"3","state":"output-denied","input":{},"approval":{"id":"approval-1","approved":false}}
                ]
                """
            ),
            messages
        );
    }

    [UpstreamTest(
        Suite
            + "should represent terminal calls as dynamic tool parts when agent tools are omitted",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_represent_terminal_calls_as_dynamic_tool_parts_when_agent_tools_are_omitted()
    {
        var messages = await ValidateForAgent(
            Assistant(
                """[{"type":"tool-bar","toolCallId":"1","state":"output-available","input":{"previous":"value"},"output":{"result":"success"}}]"""
            )
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """{"type":"dynamic-tool","dynamic":false,"toolName":"bar","toolCallId":"1","state":"output-available","input":{"previous":"value"},"output":{"result":"success"}}"""
                ),
                Render(messages)[0]!["parts"]![0]
            )
        );
    }

    [UpstreamTest(
        Suite + "should reject stale terminal input for an available agent tool",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_reject_stale_terminal_input_for_an_available_agent_tool()
    {
        var error = await Assert.ThrowsAsync<TypeValidationException>(() =>
            ValidateForAgent(
                Assistant(
                    """[{"type":"tool-foo","toolCallId":"1","state":"output-available","input":{"foo":123},"output":{"result":"success"}}]"""
                ),
                TestTools()
            )
        );

        Assert.Contains("Type validation failed for messages[0].parts[0].input", error.Message);
    }

    [UpstreamTest(
        Suite + "should validate automatic approval reasons on output parts",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_automatic_approval_reasons_on_output_parts()
    {
        var json = Assistant(
            """[{"type":"tool-foo","toolCallId":"1","state":"output-available","input":{"foo":"bar"},"output":{"result":"ok"},"approval":{"id":"approval-1","approved":true,"isAutomatic":true,"reason":"trusted internal tool"}}]"""
        );

        AssertMessages(json, await Validate(json, TestTools()));
    }

    [UpstreamTest(
        Suite + "should validate request and response reasons throughout approval",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_request_and_response_reasons_throughout_approval()
    {
        var json = Assistant(
            """
            [
              {"type":"tool-foo","toolCallId":"1","state":"approval-requested","input":{"foo":"bar"},"approval":{"id":"approval-1","requestReason":"requires operator review"}},
              {"type":"tool-foo","toolCallId":"2","state":"approval-responded","input":{"foo":"baz"},"approval":{"id":"approval-2","approved":true,"requestReason":"requires security review","reason":"approved by operator"}}
            ]
            """
        );

        AssertMessages(json, await Validate(json, TestTools()));
    }

    [UpstreamTest(
        Suite + "should preserve approval descriptors",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_preserve_approval_descriptors()
    {
        const string descriptor =
            """{"action":"deleteAccount","permissions":["account:delete"],"risk":"high"}""";
        var json = Assistant(
            """
            [
              {"type":"tool-foo","toolCallId":"1","state":"approval-requested","input":{"foo":"bar"},"approval":{"id":"approval-1","descriptor":DESCRIPTOR}},
              {"type":"tool-foo","toolCallId":"2","state":"approval-responded","input":{"foo":"baz"},"approval":{"id":"approval-2","approved":true,"descriptor":DESCRIPTOR}}
            ]
            """.Replace("DESCRIPTOR", descriptor)
        );

        AssertMessages(json, await Validate(json, TestTools()));
    }

    [UpstreamTest(
        Suite + "should throw error when tool input validation fails",
        Note = "Upstream's message carries a zod issue list. The port's message carries the schema validator's text."
    )]
    [Fact]
    public async Task Should_throw_error_when_tool_input_validation_fails()
    {
        var message = await InputError(
            Assistant(
                """[{"type":"tool-foo","toolCallId":"1","state":"input-available","input":{"foo":123},"providerExecuted":true}]"""
            ),
            TestTools()
        );

        Assert.StartsWith(
            "Type validation failed for messages[0].parts[0].input (foo, id: \"1\"): Value: {\"foo\":123}.\nError message: ",
            message
        );
    }

    [UpstreamTest(
        Suite + "should throw error when tool output validation fails",
        Note = "Upstream's message carries a zod issue list. The port's message carries the schema validator's text."
    )]
    [Fact]
    public async Task Should_throw_error_when_tool_output_validation_fails()
    {
        var message = await InputError(
            Assistant(
                """[{"type":"tool-foo","toolCallId":"1","state":"output-available","providerExecuted":true,"input":{"foo":"bar"},"output":{"result":123}}]"""
            ),
            TestTools()
        );

        Assert.StartsWith(
            "Type validation failed for messages[0].parts[0].output (foo, id: \"1\"): Value: {\"result\":123}.\nError message: ",
            message
        );
    }

    [UpstreamTest(
        Suite + "should not validate input in input-streaming state",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_not_validate_input_in_input_streaming_state()
    {
        var json = Assistant(
            """[{"type":"tool-foo","toolCallId":"1","state":"input-streaming","input":{"foo":123},"rawInput":"{\"foo\":123","providerExecuted":true}]"""
        );

        AssertMessages(json, await Validate(json, TestTools()));
    }

    /// <summary>The tools of the transformed approval input tests. <c>count</c> is a string that the refinement turns into a number.</summary>
    internal static Dictionary<string, Tool> CountTools() =>
        new()
        {
            ["count"] = new Tool(
                "count",
                null,
                Json(
                    """{"type":"object","properties":{"count":{"type":"string"}},"required":["count"]}"""
                ),
                null
            ),
        };

    internal static Dictionary<string, Func<JsonElement, Task<JsonElement>>> CountRefine() =>
        new()
        {
            ["count"] = input =>
                Task.FromResult(
                    Json(
                        "{\"count\":"
                            + double.Parse(
                                    input.GetProperty("count").GetString()!,
                                    System.Globalization.CultureInfo.InvariantCulture
                                )
                                .ToString(System.Globalization.CultureInfo.InvariantCulture)
                            + "}"
                    )
                ),
        };
}
