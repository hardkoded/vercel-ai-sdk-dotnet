// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.ToResponseMessages.ResponseMessagesAssert;

namespace Vercel.AI.Tests.ToResponseMessages;

/// <summary>Port of <c>to-response-messages.test.ts</c> &gt; <c>toResponseMessages</c>.</summary>
public sealed class ToResponseMessagesTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/to-response-messages.test.ts::toResponseMessages::";

    [Fact]
    [UpstreamTest(
        Prefix + "should return an assistant message with text when no tool calls or results",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_an_assistant_message_with_text_when_no_tool_calls_or_results()
    {
        var result = ToResponse([new GeneratedText("Hello, world!")], [TestTool()]);

        var message = Assert.Single(result);
        var assistant = Assistant(result, 0);
        Assert.Same(assistant, message);
        Assert.Single(assistant.Parts);
        Text(assistant, 0, "Hello, world!");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include tool calls in the assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_tool_calls_in_the_assistant_message()
    {
        var result = ToResponse(
            [new GeneratedText("Using a tool"), new GeneratedToolCall("123", "testTool", "{}")],
            [TestTool()]
        );

        Assert.Single(result);
        var assistant = Assistant(result, 0);
        Assert.Equal(2, assistant.Parts.Count);
        Text(assistant, 0, "Using a tool");
        ToolCall(assistant, 1, "123", "testTool", "{}");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include tool calls with metadata in the assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_tool_calls_with_metadata_in_the_assistant_message()
    {
        var result = ToResponse(
            [
                new GeneratedText("Using a tool"),
                new GeneratedToolCall(
                    "123",
                    "testTool",
                    "{}",
                    Json("{\"testProvider\":{\"signature\":\"sig\"}}")
                ),
            ],
            [TestTool()]
        );

        Assert.Single(result);
        var assistant = Assistant(result, 0);
        Assert.Equal(2, assistant.Parts.Count);
        Text(assistant, 0, "Using a tool");
        ToolCall(
            assistant,
            1,
            "123",
            "testTool",
            "{}",
            providerOptionsJson: "{\"testProvider\":{\"signature\":\"sig\"}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include custom parts in the assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_custom_parts_in_the_assistant_message()
    {
        var result = ToResponse(
            [
                new GeneratedCustom(
                    "mock-provider.compaction",
                    Json("{\"openai\":{\"itemId\":\"cmp_123\"}}")
                ),
            ],
            null
        );

        Assert.Single(result);
        var assistant = Assistant(result, 0);
        var custom = Assert.IsType<AssistantCustomPart>(Assert.Single(assistant.Parts));
        Assert.Equal("mock-provider.compaction", custom.Kind);
        AssertOptions("{\"openai\":{\"itemId\":\"cmp_123\"}}", custom.ProviderOptions);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include tool results as a separate message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_tool_results_as_a_separate_message()
    {
        var result = ToResponse(
            [
                new GeneratedText("Tool used"),
                new GeneratedToolCall("123", "testTool", "{}"),
                ClientResult("123", "testTool", "Tool result"),
            ],
            [TestTool()]
        );

        Assert.Equal(2, result.Count);
        var assistant = Assistant(result, 0);
        Assert.Equal(2, assistant.Parts.Count);
        Text(assistant, 0, "Tool used");
        ToolCall(assistant, 1, "123", "testTool", "{}");
        AssertToolResult(ToolMessage(result, 1), "123", "testTool", "text", "Tool result");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include tool errors as a separate message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_tool_errors_as_a_separate_message()
    {
        var result = ToResponse(
            [
                new GeneratedText("Tool used"),
                new GeneratedToolCall("123", "testTool", "{}"),
                ClientResult("123", "testTool", "Tool error", isError: true),
            ],
            [TestTool()]
        );

        Assert.Equal(2, result.Count);
        var assistant = Assistant(result, 0);
        Assert.Equal(2, assistant.Parts.Count);
        Text(assistant, 0, "Tool used");
        ToolCall(assistant, 1, "123", "testTool", "{}");
        AssertToolResult(ToolMessage(result, 1), "123", "testTool", "error-text", "Tool error");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should serialize parallel tool results in tool call order",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_serialize_parallel_tool_results_in_tool_call_order()
    {
        var result = ToResponse(
            [
                new GeneratedText("Using tools"),
                new GeneratedToolCall("call-a", "toolA", "{}"),
                new GeneratedToolCall("call-b", "toolB", "{}"),
                // Simulates parallel execution where toolB resolved before toolA.
                ClientResult("call-b", "toolB", "B result"),
                ClientResult("call-a", "toolA", "A result"),
            ],
            [TestTool("toolA", "Tool A"), TestTool("toolB", "Tool B")]
        );

        var toolMessage = ToolMessage(result, 1);
        Assert.Equal("tool", toolMessage.Role);
        Assert.Equal(["call-a", "call-b"], toolMessage.Results.Select(part => part.ToolCallId));
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle undefined text", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_undefined_text()
    {
        var result = ToResponse(
            [
                new GeneratedReasoning(
                    "Thinking text",
                    Json("{\"testProvider\":{\"signature\":\"sig\"}}")
                ),
            ],
            []
        );

        Assert.Single(result);
        var assistant = Assistant(result, 0);
        var reasoning = Assert.IsType<AssistantReasoningPart>(Assert.Single(assistant.Parts));
        Assert.Equal("Thinking text", reasoning.Text);
        AssertOptions("{\"testProvider\":{\"signature\":\"sig\"}}", reasoning.ProviderOptions);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include reasoning array with redacted reasoning in the assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_reasoning_array_with_redacted_reasoning_in_the_assistant_message()
    {
        var result = ToResponse(
            [
                new GeneratedReasoning(
                    "redacted-data",
                    Json("{\"testProvider\":{\"isRedacted\":true}}")
                ),
                new GeneratedReasoning(
                    "Thinking text",
                    Json("{\"testProvider\":{\"signature\":\"sig\"}}")
                ),
                new GeneratedText("Final text"),
            ],
            []
        );

        Assert.Single(result);
        var assistant = Assistant(result, 0);
        Assert.Equal(3, assistant.Parts.Count);
        var redacted = Part<AssistantReasoningPart>(assistant, 0);
        Assert.Equal("redacted-data", redacted.Text);
        AssertOptions("{\"testProvider\":{\"isRedacted\":true}}", redacted.ProviderOptions);
        var thinking = Part<AssistantReasoningPart>(assistant, 1);
        Assert.Equal("Thinking text", thinking.Text);
        AssertOptions("{\"testProvider\":{\"signature\":\"sig\"}}", thinking.ProviderOptions);
        Text(assistant, 2, "Final text");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle multipart tool results",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_multipart_tool_results()
    {
        var tool = TestTool();
        tool.ToModelOutput = _ => new JsonToolModelOutput(
            Json("{\"proof\":\"that toModelOutput is called\"}")
        );

        var result = ToResponse(
            [
                new GeneratedText("multipart tool result"),
                new GeneratedToolCall("123", "testTool", "{}"),
                ClientResult("123", "testTool", "image-base64"),
            ],
            [tool]
        );

        Assert.Equal(2, result.Count);
        var assistant = Assistant(result, 0);
        Assert.Equal(2, assistant.Parts.Count);
        Text(assistant, 0, "multipart tool result");
        ToolCall(assistant, 1, "123", "testTool", "{}");
        AssertToolResult(
            ToolMessage(result, 1),
            "123",
            "testTool",
            "json",
            "{\"proof\":\"that toModelOutput is called\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include reasoning-file parts in the assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_reasoning_file_parts_in_the_assistant_message()
    {
        var result = ToResponse(
            [
                new GeneratedReasoningFile(
                    PngBytes,
                    "image/png",
                    Json("{\"testProvider\":{\"signature\":\"sig\"}}")
                ),
                new GeneratedText("Here is my analysis"),
            ],
            []
        );

        Assert.Single(result);
        var assistant = Assistant(result, 0);
        Assert.Equal(2, assistant.Parts.Count);
        var reasoningFile = Part<AssistantReasoningFilePart>(assistant, 0);
        Assert.Equal("image/png", reasoningFile.MediaType);
        Assert.Equal("iVBORw0KGgo=", Convert.ToBase64String(reasoningFile.Data!));
        AssertOptions("{\"testProvider\":{\"signature\":\"sig\"}}", reasoningFile.ProviderOptions);
        Text(assistant, 1, "Here is my analysis");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include images in the assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_images_in_the_assistant_message()
    {
        var result = ToResponse(
            [new GeneratedText("Here is an image"), new GeneratedFile(PngBytes, "image/png")],
            []
        );

        Assert.Single(result);
        var assistant = Assistant(result, 0);
        Assert.Equal(2, assistant.Parts.Count);
        Text(assistant, 0, "Here is an image");
        AssertFile(Part<AssistantFilePart>(assistant, 1), PngBytes, "image/png");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle multiple images in the assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_multiple_images_in_the_assistant_message()
    {
        byte[] jpegBytes = [255, 216, 255];

        var result = ToResponse(
            [
                new GeneratedText("Here are multiple images"),
                new GeneratedFile(PngBytes, "image/png"),
                new GeneratedFile(jpegBytes, "image/jpeg"),
            ],
            []
        );

        Assert.Single(result);
        var assistant = Assistant(result, 0);
        Assert.Equal(3, assistant.Parts.Count);
        Text(assistant, 0, "Here are multiple images");
        AssertFile(Part<AssistantFilePart>(assistant, 1), PngBytes, "image/png");
        AssertFile(Part<AssistantFilePart>(assistant, 2), jpegBytes, "image/jpeg");
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle Uint8Array images", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_Uint8Array_images()
    {
        var result = ToResponse(
            [new GeneratedText("Here is a binary image"), new GeneratedFile(PngBytes, "image/png")],
            []
        );

        Assert.Single(result);
        var assistant = Assistant(result, 0);
        Assert.Equal(2, assistant.Parts.Count);
        Text(assistant, 0, "Here is a binary image");
        AssertFile(Part<AssistantFilePart>(assistant, 1), PngBytes, "image/png");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include images, reasoning, and tool calls in the correct order",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_images_reasoning_and_tool_calls_in_the_correct_order()
    {
        var result = ToResponse(
            [
                new GeneratedReasoning(
                    "Thinking text",
                    Json("{\"testProvider\":{\"signature\":\"sig\"}}")
                ),
                new GeneratedFile(PngBytes, "image/png"),
                new GeneratedText("Combined response"),
                new GeneratedToolCall("123", "testTool", "{}"),
            ],
            [TestTool()]
        );

        Assert.Single(result);
        var assistant = Assistant(result, 0);
        Assert.Equal(4, assistant.Parts.Count);
        var reasoning = Part<AssistantReasoningPart>(assistant, 0);
        Assert.Equal("Thinking text", reasoning.Text);
        AssertOptions("{\"testProvider\":{\"signature\":\"sig\"}}", reasoning.ProviderOptions);
        AssertFile(Part<AssistantFilePart>(assistant, 1), PngBytes, "image/png");
        Text(assistant, 2, "Combined response");
        ToolCall(assistant, 3, "123", "testTool", "{}");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not append text parts if text is empty string",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_append_text_parts_if_text_is_empty_string()
    {
        var result = ToResponse(
            [new GeneratedText(""), new GeneratedToolCall("123", "testTool", "{}")],
            [TestTool()]
        );

        Assert.Single(result);
        var assistant = Assistant(result, 0);
        Assert.Single(assistant.Parts);
        ToolCall(assistant, 0, "123", "testTool", "{}");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not append assistant message if there is no content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_append_assistant_message_if_there_is_no_content()
    {
        var result = ToResponse([], []);

        Assert.Empty(result);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should sanitize invalid tool call with non-object input to empty object",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_sanitize_invalid_tool_call_with_non_object_input_to_empty_object()
    {
        var result = ToResponse(
            [
                new GeneratedToolCall("call-1", "weather", "{ city: San Francisco, }")
                {
                    Dynamic = true,
                    Invalid = true,
                    Error = new InvalidOperationException("JSON parsing failed"),
                },
                ClientResult(
                    "call-1",
                    "weather",
                    "Invalid input for tool weather: JSON parsing failed",
                    isError: true
                ),
            ],
            [TestTool("weather", "Get weather")]
        );

        Assert.Equal(2, result.Count);
        var assistant = Assistant(result, 0);
        Assert.Single(assistant.Parts);
        ToolCall(assistant, 0, "call-1", "weather", "{}");
        AssertToolResult(
            ToolMessage(result, 1),
            "call-1",
            "weather",
            "error-text",
            "Invalid input for tool weather: JSON parsing failed"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve valid object input on invalid tool call",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_valid_object_input_on_invalid_tool_call()
    {
        var result = ToResponse(
            [
                new GeneratedToolCall("call-1", "weather", "{\"cities\":\"San Francisco\"}")
                {
                    Dynamic = true,
                    Invalid = true,
                    Error = new InvalidOperationException("Type validation failed"),
                },
                ClientResult(
                    "call-1",
                    "weather",
                    "Invalid input for tool weather: Type validation failed",
                    isError: true
                ),
            ],
            [TestTool("weather", "Get weather")]
        );

        Assert.Equal(2, result.Count);
        var assistant = Assistant(result, 0);
        Assert.Single(assistant.Parts);
        ToolCall(assistant, 0, "call-1", "weather", "{\"cities\":\"San Francisco\"}");
        AssertToolResult(
            ToolMessage(result, 1),
            "call-1",
            "weather",
            "error-text",
            "Invalid input for tool weather: Type validation failed"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include provider metadata in the text parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_provider_metadata_in_the_text_parts()
    {
        var result = ToResponse(
            [
                new GeneratedText(
                    "Here is a text",
                    providerMetadata: Json("{\"testProvider\":{\"signature\":\"sig\"}}")
                ),
            ],
            []
        );

        Assert.Single(result);
        var assistant = Assistant(result, 0);
        Text(assistant, 0, "Here is a text", "{\"testProvider\":{\"signature\":\"sig\"}}");
    }

    private static void AssertFile(AssistantFilePart part, byte[] data, string mediaType)
    {
        Assert.Equal(mediaType, part.MediaType);
        Assert.Equal(Convert.ToBase64String(data), Convert.ToBase64String(part.Data!));
        Assert.Null(part.ProviderOptions);
    }
}
