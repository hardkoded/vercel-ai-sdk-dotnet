// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Groq;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class GroqChatMessagesTests
{
    [Fact]
    [UpstreamTest("packages/groq/src/convert-to-groq-chat-messages.test.ts::user messages::should convert messages with image parts", Coverage = UpstreamCoverage.Covered)]
    public void Converts_inline_image_parts()
    {
        var message = GroqChatMessages.ConvertUserParts(new[]
        {
            GroqUserPart.Text("Hello"),
            GroqUserPart.File("image/png", GroqFileKind.Data, new byte[] { 0, 1, 2, 3 }, null),
        });
        Assert.Equal("user", message["role"]!.GetValue<string>());
        var content = message["content"]!.AsArray();
        Assert.Equal("Hello", content[0]!["text"]!.GetValue<string>());
        Assert.Equal("data:image/png;base64,AAECAw==", content[1]!["image_url"]!["url"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-to-groq-chat-messages.test.ts::user messages::should convert messages with image parts from Uint8Array", Coverage = UpstreamCoverage.Covered)]
    public void Converts_image_bytes()
    {
        var message = GroqChatMessages.ConvertUserParts(new[]
        {
            GroqUserPart.Text("Hi"),
            GroqUserPart.File("image/png", GroqFileKind.Data, new byte[] { 0, 1, 2, 3 }, null),
        });
        Assert.Equal("data:image/png;base64,AAECAw==", message["content"]![1]!["image_url"]!["url"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-to-groq-chat-messages.test.ts::user messages::should convert messages with only a text part to a string content", Coverage = UpstreamCoverage.Covered)]
    public void Single_text_part_is_a_string()
    {
        var message = GroqChatMessages.ConvertUserParts(new[] { GroqUserPart.Text("Hello") });
        Assert.Equal("Hello", message["content"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-to-groq-chat-messages.test.ts::tool calls::should stringify arguments to tool calls", Coverage = UpstreamCoverage.Covered)]
    public void Stringifies_tool_call_arguments()
    {
        var assistant = GroqChatMessages.ConvertAssistantTurn(
            string.Empty,
            null,
            new[] { new GeneratedToolCall("quux", "thwomp", "{\"foo\":\"bar123\"}") });
        var tool = GroqChatMessages.ConvertToolResult("quux", "json", null, JsonNode.Parse("{\"oof\":\"321rab\"}"), null);
        Assert.Equal(string.Empty, assistant["content"]!.GetValue<string>());
        Assert.Equal("{\"foo\":\"bar123\"}", assistant["tool_calls"]![0]!["function"]!["arguments"]!.GetValue<string>());
        Assert.Equal("thwomp", assistant["tool_calls"]![0]!["function"]!["name"]!.GetValue<string>());
        Assert.Equal("{\"oof\":\"321rab\"}", tool["content"]!.GetValue<string>());
        Assert.Equal("quux", tool["tool_call_id"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-to-groq-chat-messages.test.ts::tool calls::should send reasoning if present", Coverage = UpstreamCoverage.Covered)]
    public void Sends_assistant_reasoning()
    {
        var assistant = GroqChatMessages.ConvertAssistantTurn(
            string.Empty,
            "I think the tool will return the correct value.",
            new[] { new GeneratedToolCall("quux", "thwomp", "{\"foo\":\"bar123\"}") });
        Assert.Equal("I think the tool will return the correct value.", assistant["reasoning"]!.GetValue<string>());
        Assert.NotNull(assistant["tool_calls"]);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-to-groq-chat-messages.test.ts::tool calls::should not include reasoning field when no reasoning content is present", Coverage = UpstreamCoverage.Covered)]
    public void Omits_empty_reasoning()
    {
        var assistant = GroqChatMessages.ConvertAssistantTurn("Hello, how can I help you?", null, null);
        Assert.Equal("Hello, how can I help you?", assistant["content"]!.GetValue<string>());
        Assert.Null(assistant["reasoning"]);
        Assert.Null(assistant["tool_calls"]);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-to-groq-chat-messages.test.ts::tool calls::should throw for file parts with provider references", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_provider_file_references()
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(() =>
            GroqChatMessages.ConvertUserParts(new[] { GroqUserPart.File("image/png", GroqFileKind.Reference, null, null) }));
        Assert.Equal("file parts with provider references", error.Functionality);
        Assert.Contains("file parts with provider references", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-to-groq-chat-messages.test.ts::top-level-only media type resolution::passes full image/png through unchanged for inline data", Coverage = UpstreamCoverage.Covered)]
    public void Passes_image_png_through()
    {
        var png = System.Convert.FromBase64String("iVBORw0KGgo=");
        var message = GroqChatMessages.ConvertUserParts(new[] { GroqUserPart.File("image/png", GroqFileKind.Data, png, null) });
        Assert.Equal("data:image/png;base64,iVBORw0KGgo=", message["content"]![0]!["image_url"]!["url"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-to-groq-chat-messages.test.ts::top-level-only media type resolution::detects image subtype from inline bytes for top-level \"image\"", Coverage = UpstreamCoverage.Covered)]
    public void Detects_png_from_image_media_type()
    {
        var png = System.Convert.FromBase64String("iVBORw0KGgo=");
        var message = GroqChatMessages.ConvertUserParts(new[] { GroqUserPart.File("image", GroqFileKind.Data, png, null) });
        Assert.Equal("data:image/png;base64,iVBORw0KGgo=", message["content"]![0]!["image_url"]!["url"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-to-groq-chat-messages.test.ts::top-level-only media type resolution::passes through URL source for top-level-only image", Coverage = UpstreamCoverage.Covered)]
    public void Passes_image_urls_through()
    {
        var message = GroqChatMessages.ConvertUserParts(new[] { GroqUserPart.File("image", GroqFileKind.Url, null, "https://example.com/x.png") });
        Assert.Equal("https://example.com/x.png", message["content"]![0]!["image_url"]!["url"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/convert-to-groq-chat-messages.test.ts::top-level-only media type resolution::normalizes image/* wildcard via detection", Coverage = UpstreamCoverage.Covered)]
    public void Detects_png_from_image_wildcard()
    {
        var png = System.Convert.FromBase64String("iVBORw0KGgo=");
        var message = GroqChatMessages.ConvertUserParts(new[] { GroqUserPart.File("image/*", GroqFileKind.Data, png, null) });
        Assert.Equal("data:image/png;base64,iVBORw0KGgo=", message["content"]![0]!["image_url"]!["url"]!.GetValue<string>());
    }
}
