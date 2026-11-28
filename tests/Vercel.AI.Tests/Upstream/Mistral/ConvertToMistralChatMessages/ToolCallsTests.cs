// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Converter = Vercel.AI.Mistral.ConvertToMistralChatMessages;

namespace Vercel.AI.Tests.Upstream.Mistral.ConvertToMistralChatMessages;

/// <summary>Port of <c>convert-to-mistral-chat-messages.test.ts</c> &gt; <c>tool calls</c>.</summary>
public sealed class ToolCallsTests
{
    private const string Prefix =
        "packages/mistral/src/convert-to-mistral-chat-messages.test.ts::tool calls::";

    [Fact]
    [UpstreamTest(
        Prefix + "should stringify arguments to tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_stringify_arguments_to_tool_calls()
    {
        const string input = "{\"key\":\"arg-value\"}";
        var result = Converter.Convert(
            new ModelMessage[]
            {
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantToolCallPart(
                            new GeneratedToolCall("tool-call-id-1", "tool-1", input)
                        ),
                    }
                ),
                new ToolModelMessage(
                    "tool-call-id-1",
                    "tool-1",
                    "{\"key\":\"result-value\"}",
                    false
                ),
            }
        );

        var assistant = result[0]!;
        Assert.Equal("assistant", assistant["role"]!.GetValue<string>());
        Assert.Equal("", assistant["content"]!.GetValue<string>());
        var call = assistant["tool_calls"]![0]!;
        Assert.Equal("tool-call-id-1", call["id"]!.GetValue<string>());
        Assert.Equal("function", call["type"]!.GetValue<string>());
        Assert.Equal("tool-1", call["function"]!["name"]!.GetValue<string>());
        Assert.Equal(input, call["function"]!["arguments"]!.GetValue<string>());
        var tool = result[1]!;
        Assert.Equal("tool", tool["role"]!.GetValue<string>());
        Assert.Equal("tool-1", tool["name"]!.GetValue<string>());
        Assert.Equal("tool-call-id-1", tool["tool_call_id"]!.GetValue<string>());
        Assert.Equal("{\"key\":\"result-value\"}", tool["content"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle text output format", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_text_output_format()
    {
        const string input = "{\"query\":\"test\"}";
        var result = Converter.Convert(
            new ModelMessage[]
            {
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantToolCallPart(
                            new GeneratedToolCall("tool-call-id-2", "text-tool", input)
                        ),
                    }
                ),
                new ToolModelMessage(
                    "tool-call-id-2",
                    "text-tool",
                    "This is a text response",
                    false,
                    "text"
                ),
            }
        );

        var assistant = result[0]!;
        Assert.Equal("assistant", assistant["role"]!.GetValue<string>());
        Assert.Equal("", assistant["content"]!.GetValue<string>());
        var call = assistant["tool_calls"]![0]!;
        Assert.Equal("tool-call-id-2", call["id"]!.GetValue<string>());
        Assert.Equal("function", call["type"]!.GetValue<string>());
        Assert.Equal("text-tool", call["function"]!["name"]!.GetValue<string>());
        Assert.Equal(input, call["function"]!["arguments"]!.GetValue<string>());
        var tool = result[1]!;
        Assert.Equal("tool", tool["role"]!.GetValue<string>());
        Assert.Equal("text-tool", tool["name"]!.GetValue<string>());
        Assert.Equal("tool-call-id-2", tool["tool_call_id"]!.GetValue<string>());
        Assert.Equal("This is a text response", tool["content"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle content output format",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port's content output has its own wire form, so the test checks the text of the output and not the TypeScript object graph."
    )]
    public void Should_handle_content_output_format()
    {
        const string input = "{\"query\":\"generate image\"}";
        var result = Converter.Convert(
            new ModelMessage[]
            {
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantToolCallPart(
                            new GeneratedToolCall("tool-call-id-3", "image-tool", input)
                        ),
                    }
                ),
                new ToolModelMessage(
                    "tool-call-id-3",
                    "image-tool",
                    new UserContentPart[]
                    {
                        new TextContentPart("Here is the result:"),
                        new FileContentPart("image/png", null, new byte[] { 1, 2, 3 }, null),
                    }
                ),
            }
        );

        var assistant = result[0]!;
        Assert.Equal("assistant", assistant["role"]!.GetValue<string>());
        Assert.Equal("", assistant["content"]!.GetValue<string>());
        var call = assistant["tool_calls"]![0]!;
        Assert.Equal("tool-call-id-3", call["id"]!.GetValue<string>());
        Assert.Equal("function", call["type"]!.GetValue<string>());
        Assert.Equal("image-tool", call["function"]!["name"]!.GetValue<string>());
        Assert.Equal(input, call["function"]!["arguments"]!.GetValue<string>());
        var tool = result[1]!;
        Assert.Equal("tool", tool["role"]!.GetValue<string>());
        Assert.Equal("image-tool", tool["name"]!.GetValue<string>());
        Assert.Equal("tool-call-id-3", tool["tool_call_id"]!.GetValue<string>());
        Assert.Contains("Here is the result:", tool["content"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle error output format",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_error_output_format()
    {
        const string input = "{\"query\":\"test\"}";
        var result = Converter.Convert(
            new ModelMessage[]
            {
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantToolCallPart(
                            new GeneratedToolCall("tool-call-id-4", "error-tool", input)
                        ),
                    }
                ),
                new ToolModelMessage(
                    "tool-call-id-4",
                    "error-tool",
                    "Invalid input provided",
                    true,
                    "error-text"
                ),
            }
        );

        var assistant = result[0]!;
        Assert.Equal("assistant", assistant["role"]!.GetValue<string>());
        Assert.Equal("", assistant["content"]!.GetValue<string>());
        var call = assistant["tool_calls"]![0]!;
        Assert.Equal("tool-call-id-4", call["id"]!.GetValue<string>());
        Assert.Equal("function", call["type"]!.GetValue<string>());
        Assert.Equal("error-tool", call["function"]!["name"]!.GetValue<string>());
        Assert.Equal(input, call["function"]!["arguments"]!.GetValue<string>());
        var tool = result[1]!;
        Assert.Equal("tool", tool["role"]!.GetValue<string>());
        Assert.Equal("error-tool", tool["name"]!.GetValue<string>());
        Assert.Equal("tool-call-id-4", tool["tool_call_id"]!.GetValue<string>());
        Assert.Equal("Invalid input provided", tool["content"]!.GetValue<string>());
    }
}
