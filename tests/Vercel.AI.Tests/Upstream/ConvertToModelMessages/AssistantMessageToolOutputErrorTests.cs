// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Ui;
using Vercel.AI.Util;
using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages &gt; assistant message &gt; tool output error</c>.</summary>
[Collection("LogWarnings")]
public sealed class AssistantMessageToolOutputErrorTests : WarningsOff
{
    private const string Suite = Prefix + "convertToModelMessages > assistant message > tool output error::";

    [UpstreamTest(Suite + "should preserve result provider metadata on a failed tool call when call metadata is unavailable", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_preserve_result_provider_metadata_on_a_failed_tool_call_when_call_metadata_is_unavailable()
    {
        var result = Convert(new[]
        {
            Assistant(new ToolUIPart("createWidget", "call1", ToolUIPartState.OutputError)
            {
                Input = null,
                RawInput = Json("{}"),
                ErrorText = "Invalid input",
                ResultProviderMetadata = new Dictionary<string, JsonElement>
                {
                    ["openai"] = Json("{\"namespace\":\"widget_tools\"}"),
                },
            }),
        });

        AssertEqual(
            """
            [
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "tool-call",
                    "toolCallId": "call1",
                    "toolName": "createWidget",
                    "input": {},
                    "providerOptions": { "openai": { "namespace": "widget_tools" } }
                  }
                ]
              },
              {
                "role": "tool",
                "content": [
                  {
                    "type": "tool-result",
                    "toolCallId": "call1",
                    "toolName": "createWidget",
                    "output": { "type": "error-text", "value": "Invalid input" }
                  }
                ]
              }
            ]
            """,
            result);
    }

    [UpstreamTest(Suite + "should handle assistant message with tool output error that has raw input", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_handle_assistant_message_with_tool_output_error_that_has_raw_input()
    {
        var warnings = new List<LogWarningsOptions>();
        LogWarnings.Logger = new Action<LogWarningsOptions>(warnings.Add);

        var result = Convert(new[]
        {
            Assistant(
                new StepStartUIPart(),
                new TextUIPart("Let me calculate that for you."),
                new ToolUIPart("calculator", "call1", ToolUIPartState.OutputError)
                {
                    ErrorText = "Error: Invalid input",
                    Input = null,
                    RawInput = Json("\"{\\\"operation\\\":\\\"add\\\",\\\"numbers\\\":[1,2]\""),
                }),
        });

        AssertEqual(
            """
            [
              {
                "content": [
                  { "text": "Let me calculate that for you.", "type": "text" },
                  {
                    "input": "{\"operation\":\"add\",\"numbers\":[1,2]",
                    "toolCallId": "call1",
                    "toolName": "calculator",
                    "type": "tool-call"
                  }
                ],
                "role": "assistant"
              },
              {
                "content": [
                  {
                    "output": { "type": "error-text", "value": "Error: Invalid input" },
                    "toolCallId": "call1",
                    "toolName": "calculator",
                    "type": "tool-result"
                  }
                ],
                "role": "tool"
              }
            ]
            """,
            result);

        var logged = Assert.Single(warnings);
        var warning = Assert.IsType<DeprecatedWarning>(Assert.Single(logged.Warnings));
        Assert.Equal("rawInput in output-error UI message parts", warning.Setting);
        Assert.Equal("Use the \"input\" field instead. The \"rawInput\" field will be removed in the next major version.", warning.Message);
    }

    [UpstreamTest(Suite + "should preserve the deprecated rawInput fallback when input is null", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_preserve_the_deprecated_rawInput_fallback_when_input_is_null()
    {
        var result = Convert(new[]
        {
            Assistant(new ToolUIPart("calculator", "call1", ToolUIPartState.OutputError)
            {
                ErrorText = "Error: Invalid input",
                Input = Json("null"),
                RawInput = Json("\"legacy input\""),
            }),
        });

        var rendered = Render(result);
        Assert.Equal("assistant", (string?)rendered[0]!["role"]);
        Assert.Equal("tool-call", (string?)rendered[0]!["content"]![0]!["type"]);
        Assert.Equal("legacy input", (string?)rendered[0]!["content"]![0]!["input"]);
    }

    [UpstreamTest(Suite + "should handle assistant message with tool output error that has no raw input", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_handle_assistant_message_with_tool_output_error_that_has_no_raw_input()
    {
        var result = Convert(new[]
        {
            Assistant(
                new StepStartUIPart(),
                new TextUIPart("Let me calculate that for you."),
                new ToolUIPart("calculator", "call1", ToolUIPartState.OutputError)
                {
                    Input = Json("{\"operation\":\"add\",\"numbers\":[1,2]}"),
                    ErrorText = "Error: Invalid input",
                }),
        });

        AssertEqual(
            """
            [
              {
                "content": [
                  { "text": "Let me calculate that for you.", "type": "text" },
                  {
                    "input": { "numbers": [1, 2], "operation": "add" },
                    "toolCallId": "call1",
                    "toolName": "calculator",
                    "type": "tool-call"
                  }
                ],
                "role": "assistant"
              },
              {
                "content": [
                  {
                    "output": { "type": "error-text", "value": "Error: Invalid input" },
                    "toolCallId": "call1",
                    "toolName": "calculator",
                    "type": "tool-result"
                  }
                ],
                "role": "tool"
              }
            ]
            """,
            result);
    }
}
