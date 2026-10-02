// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class ConvertToOpenAIChatMessagesTests
{
    private const string File = "packages/openai/src/chat/convert-to-openai-chat-messages.test.ts";

    [Fact]
    [UpstreamTest(File + "::system messages::should forward system messages", Coverage = UpstreamCoverage.Covered)]
    public void Forwards_system_messages()
    {
        var result = Convert(new OpenAISystemChatMessage("You are a helpful assistant."));
        AssertMessages(result, """[{"role":"system","content":"You are a helpful assistant."}]""");
    }

    [Fact]
    [UpstreamTest(File + "::system messages::should convert system messages to developer messages when requested", Coverage = UpstreamCoverage.Covered)]
    public void Converts_system_messages_to_developer_messages()
    {
        var result = OpenAIChatMessages.ConvertToOpenAIChatMessages(
            new OpenAIChatPromptMessage[] { new OpenAISystemChatMessage("You are a helpful assistant.") },
            "developer");
        AssertMessages(result, """[{"role":"developer","content":"You are a helpful assistant."}]""");
    }

    [Fact]
    [UpstreamTest(File + "::system messages::should add a prompt cache breakpoint to a system message", Coverage = UpstreamCoverage.Covered)]
    public void Adds_a_prompt_cache_breakpoint_to_a_system_message()
    {
        var result = Convert(new OpenAISystemChatMessage(
            "You are a helpful assistant.",
            Options("""{"promptCacheBreakpoint":{"mode":"explicit"}}""")));
        AssertMessages(result, """
            [{"role":"system","content":[{"type":"text","text":"You are a helpful assistant.","prompt_cache_breakpoint":{"mode":"explicit"}}]}]
            """);
    }

    [Fact]
    [UpstreamTest(File + "::system messages::should remove system messages when requested", Coverage = UpstreamCoverage.Covered)]
    public void Removes_system_messages_when_requested()
    {
        var result = OpenAIChatMessages.ConvertToOpenAIChatMessages(
            new OpenAIChatPromptMessage[] { new OpenAISystemChatMessage("You are a helpful assistant.") },
            "remove");
        Assert.Empty(result.Messages);
        OpenAIParity.AssertWarnings(result.Warnings, ("other", null, null, "system messages are removed for this model"));
    }

    [Fact]
    [UpstreamTest(File + "::user messages::should convert messages with only a text part to a string content", Coverage = UpstreamCoverage.Covered)]
    public void Converts_a_single_text_part_to_a_string()
    {
        var result = Convert(new OpenAIUserChatMessage(new OpenAIChatUserPart[] { new OpenAIChatTextPart("Hello") }));
        AssertMessages(result, """[{"role":"user","content":"Hello"}]""");
    }

    [Fact]
    [UpstreamTest(File + "::user messages::should add prompt cache breakpoints to supported content blocks", Coverage = UpstreamCoverage.Covered)]
    public void Adds_prompt_cache_breakpoints_to_supported_blocks()
    {
        var breakpoint = Options("""{"promptCacheBreakpoint":{"mode":"explicit"}}""");
        var result = Convert(new OpenAIUserChatMessage(new OpenAIChatUserPart[]
        {
            new OpenAIChatTextPart("Hello", breakpoint),
            new OpenAIChatFilePart("image/png", OpenAIChatFileData.FromUrl("https://example.com/image.png"), providerOptions: breakpoint),
            new OpenAIChatFilePart("audio/wav", OpenAIChatFileData.FromBase64("AAECAw=="), providerOptions: breakpoint),
            new OpenAIChatFilePart(
                "application/pdf",
                OpenAIChatFileData.FromReference(new Dictionary<string, string> { ["openai"] = "file-pdf-123" }),
                providerOptions: breakpoint),
        }));
        AssertMessages(result, """
            [{"role":"user","content":[
              {"type":"text","text":"Hello","prompt_cache_breakpoint":{"mode":"explicit"}},
              {"type":"image_url","image_url":{"url":"https://example.com/image.png"},"prompt_cache_breakpoint":{"mode":"explicit"}},
              {"type":"input_audio","input_audio":{"data":"AAECAw==","format":"wav"},"prompt_cache_breakpoint":{"mode":"explicit"}},
              {"type":"file","file":{"file_id":"file-pdf-123"},"prompt_cache_breakpoint":{"mode":"explicit"}}
            ]}]
            """);
    }

    [Fact]
    [UpstreamTest(File + "::user messages::should convert messages with image parts", Coverage = UpstreamCoverage.Covered)]
    public void Converts_image_parts()
    {
        var result = Convert(new OpenAIUserChatMessage(new OpenAIChatUserPart[]
        {
            new OpenAIChatTextPart("Hello"),
            new OpenAIChatFilePart("image/png", OpenAIChatFileData.FromBase64("AAECAw==")),
        }));
        AssertMessages(result, """
            [{"role":"user","content":[
              {"type":"text","text":"Hello"},
              {"type":"image_url","image_url":{"url":"data:image/png;base64,AAECAw=="}}
            ]}]
            """);
    }

    [Fact]
    [UpstreamTest(File + "::user messages::should convert messages with Uint8Array image parts to data URLs", Coverage = UpstreamCoverage.Covered)]
    public void Converts_image_bytes_to_data_urls()
    {
        var result = Convert(new OpenAIUserChatMessage(new OpenAIChatUserPart[]
        {
            new OpenAIChatFilePart("image/jpeg", OpenAIChatFileData.FromBytes(new byte[] { 0xff, 0xd8, 0xff, 0xe0 })),
        }));
        AssertMessages(result, """
            [{"role":"user","content":[{"type":"image_url","image_url":{"url":"data:image/jpeg;base64,/9j/4A=="}}]}]
            """);
    }

    [Fact]
    [UpstreamTest(File + "::user messages::should add image detail when specified through extension", Coverage = UpstreamCoverage.Covered)]
    public void Adds_image_detail()
    {
        var result = Convert(new OpenAIUserChatMessage(new OpenAIChatUserPart[]
        {
            new OpenAIChatFilePart(
                "image/png",
                OpenAIChatFileData.FromBase64("AAECAw=="),
                providerOptions: Options("""{"imageDetail":"low"}""")),
        }));
        AssertMessages(result, """
            [{"role":"user","content":[{"type":"image_url","image_url":{"url":"data:image/png;base64,AAECAw==","detail":"low"}}]}]
            """);
    }

    [Fact]
    [UpstreamTest(File + "::user messages > file parts::should throw for unsupported mime types", Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_unsupported_mime_types()
    {
        var error = Assert.Throws<AiSdkException>(() => Convert(FilePart("application/something", OpenAIChatFileData.FromBase64("AAECAw=="))));
        Assert.Contains("file part media type application/something", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "::user messages > file parts::should throw for URL data", Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_audio_urls()
    {
        var error = Assert.Throws<AiSdkException>(() => Convert(FilePart("audio/wav", OpenAIChatFileData.FromUrl("https://example.com/foo.wav"))));
        Assert.Contains("audio file parts with URLs", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "::user messages > file parts::should add audio content for audio/wav file parts", Coverage = UpstreamCoverage.Covered)]
    public void Adds_wav_audio()
    {
        AssertAudio("audio/wav", "wav");
    }

    [Fact]
    [UpstreamTest(File + "::user messages > file parts::should add audio content for audio/mpeg file parts", Coverage = UpstreamCoverage.Covered)]
    public void Adds_mpeg_audio()
    {
        AssertAudio("audio/mpeg", "mp3");
    }

    [Fact]
    [UpstreamTest(File + "::user messages > file parts::should add audio content for audio/mp3 file parts", Coverage = UpstreamCoverage.Covered)]
    public void Adds_mp3_audio()
    {
        AssertAudio("audio/mp3", "mp3");
    }

    [Fact]
    [UpstreamTest(File + "::user messages > file parts::should convert messages with PDF file parts", Coverage = UpstreamCoverage.Covered)]
    public void Converts_pdf_file_parts()
    {
        var result = Convert(new OpenAIUserChatMessage(new OpenAIChatUserPart[]
        {
            new OpenAIChatFilePart("application/pdf", OpenAIChatFileData.FromBase64("AQIDBAU="), "document.pdf"),
        }));
        AssertPdf(result, "document.pdf", "AQIDBAU=");
    }

    [Fact]
    [UpstreamTest(File + "::user messages > file parts::should convert messages with binary PDF file parts", Coverage = UpstreamCoverage.Covered)]
    public void Converts_binary_pdf_file_parts()
    {
        var result = Convert(new OpenAIUserChatMessage(new OpenAIChatUserPart[]
        {
            new OpenAIChatFilePart("application/pdf", OpenAIChatFileData.FromBytes(new byte[] { 1, 2, 3, 4, 5 }), "document.pdf"),
        }));
        AssertPdf(result, "document.pdf", "AQIDBAU=");
    }

    [Fact]
    [UpstreamTest(File + "::user messages > file parts::should convert messages with PDF file parts using provider reference", Coverage = UpstreamCoverage.Covered)]
    public void Converts_pdf_provider_references()
    {
        var result = Convert(FilePart(
            "application/pdf",
            OpenAIChatFileData.FromReference(new Dictionary<string, string> { ["openai"] = "file-pdf-12345" })));
        AssertMessages(result, """[{"role":"user","content":[{"type":"file","file":{"file_id":"file-pdf-12345"}}]}]""");
    }

    [Fact]
    [UpstreamTest(File + "::user messages > file parts::should convert messages with image parts using provider reference", Coverage = UpstreamCoverage.Covered)]
    public void Converts_image_provider_references()
    {
        var result = Convert(FilePart(
            "image/png",
            OpenAIChatFileData.FromReference(new Dictionary<string, string> { ["openai"] = "file-img-12345" })));
        AssertMessages(result, """[{"role":"user","content":[{"type":"file","file":{"file_id":"file-img-12345"}}]}]""");
    }

    [Fact]
    [UpstreamTest(File + "::user messages > file parts::should throw when provider reference does not contain openai", Coverage = UpstreamCoverage.Covered)]
    public void Throws_when_the_provider_reference_is_not_openai()
    {
        var error = Assert.Throws<AiSdkException>(() => Convert(FilePart(
            "application/pdf",
            OpenAIChatFileData.FromReference(new Dictionary<string, string> { ["anthropic"] = "file-xyz" }))));
        Assert.Equal("No provider reference found for provider 'openai'. Available providers: anthropic", error.Message);
    }

    [Fact]
    [UpstreamTest(File + "::user messages > file parts::should use default filename for PDF file parts when not provided", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_default_pdf_filename()
    {
        var result = Convert(FilePart("application/pdf", OpenAIChatFileData.FromBase64("AQIDBAU=")));
        AssertPdf(result, "part-0.pdf", "AQIDBAU=");
    }

    [Fact]
    [UpstreamTest(File + "::user messages > file parts::should throw error for unsupported file types", Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_text_plain_files()
    {
        var error = Assert.Throws<AiSdkException>(() => Convert(FilePart("text/plain", OpenAIChatFileData.FromBase64("AQIDBAU="))));
        Assert.Contains("file part media type text/plain", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "::user messages > file parts::should throw error for file URLs", Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_pdf_urls()
    {
        var error = Assert.Throws<AiSdkException>(() => Convert(FilePart("application/pdf", OpenAIChatFileData.FromUrl("https://example.com/document.pdf"))));
        Assert.Contains("PDF file parts with URLs", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "::user messages > file parts > top-level-only media type resolution::detects image subtype from inline bytes for top-level \"image\"", Coverage = UpstreamCoverage.Covered)]
    public void Detects_an_image_subtype_from_inline_bytes()
    {
        AssertPng("image");
    }

    [Fact]
    [UpstreamTest(File + "::user messages > file parts > top-level-only media type resolution::normalizes image/* wildcard via detection", Coverage = UpstreamCoverage.Covered)]
    public void Normalizes_an_image_wildcard()
    {
        AssertPng("image/*");
    }

    [Fact]
    [UpstreamTest(File + "::user messages > file parts > top-level-only media type resolution::passes through URL source for top-level-only image (provider accepts raw URL)", Coverage = UpstreamCoverage.Covered)]
    public void Passes_through_a_top_level_image_url()
    {
        var result = Convert(FilePart("image", OpenAIChatFileData.FromUrl("https://example.com/x.png")));
        AssertMessages(result, """[{"role":"user","content":[{"type":"image_url","image_url":{"url":"https://example.com/x.png"}}]}]""");
    }

    [Fact]
    [UpstreamTest(File + "::user messages > file parts > top-level-only media type resolution::throws for top-level-only application (PDF requires full resolution) with URL source", Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_a_top_level_application_url()
    {
        var error = Assert.Throws<AiSdkException>(() => Convert(FilePart("application", OpenAIChatFileData.FromUrl("https://example.com/x.pdf"))));
        Assert.Contains("file of media type \"application\" must specify subtype since it is not passed as inline bytes", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "::user messages > file parts > top-level-only media type resolution::preserves full image/png pass-through", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_a_full_image_png_type()
    {
        AssertPng("image/png");
    }

    [Fact]
    [UpstreamTest(File + "::tool calls::should add a prompt cache breakpoint to assistant text content", Coverage = UpstreamCoverage.Covered)]
    public void Adds_a_prompt_cache_breakpoint_to_assistant_text()
    {
        var result = Convert(new OpenAIAssistantChatMessage(new OpenAIAssistantChatPart[]
        {
            new OpenAIAssistantTextPart("Cached assistant content", Options("""{"promptCacheBreakpoint":{"mode":"explicit"}}""")),
        }));
        AssertMessages(result, """
            [{"role":"assistant","content":[{"type":"text","text":"Cached assistant content","prompt_cache_breakpoint":{"mode":"explicit"}}]}]
            """);
        Assert.False(result.Messages[0]!.AsObject().ContainsKey("tool_calls"));
    }

    [Fact]
    [UpstreamTest(File + "::tool calls::should add a prompt cache breakpoint to tool text content", Coverage = UpstreamCoverage.Covered)]
    public void Adds_a_prompt_cache_breakpoint_to_tool_text()
    {
        var result = Convert(new OpenAIToolChatMessage(new[]
        {
            new OpenAIToolResultChatPart(
                "tool-result",
                "cached-tool",
                "cached-tool",
                OpenAIToolOutput.FromText("Cached tool content", Options("""{"promptCacheBreakpoint":{"mode":"explicit"}}"""))),
        }));
        AssertMessages(result, """
            [{"role":"tool","tool_call_id":"cached-tool","content":[{"type":"text","text":"Cached tool content","prompt_cache_breakpoint":{"mode":"explicit"}}]}]
            """);
    }

    [Fact]
    [UpstreamTest(File + "::tool calls::should stringify arguments to tool calls", Coverage = UpstreamCoverage.Covered)]
    public void Stringifies_tool_call_arguments()
    {
        var result = OpenAIChatMessages.ConvertToOpenAIChatMessages(new OpenAIChatPromptMessage[]
        {
            new OpenAIAssistantChatMessage(new OpenAIAssistantChatPart[]
            {
                new OpenAIAssistantToolCallPart("quux", "thwomp", JsonNode.Parse("""{"foo":"bar123"}""")),
            }),
            new OpenAIToolChatMessage(new[]
            {
                new OpenAIToolResultChatPart("tool-result", "quux", "thwomp", OpenAIToolOutput.FromJson(JsonNode.Parse("""{"oof":"321rab"}"""))),
            }),
        });
        AssertMessages(result, """
            [
              {"role":"assistant","content":null,"tool_calls":[{"id":"quux","type":"function","function":{"name":"thwomp","arguments":"{\"foo\":\"bar123\"}"}}]},
              {"role":"tool","tool_call_id":"quux","content":"{\"oof\":\"321rab\"}"}
            ]
            """);
    }

    [Fact]
    [UpstreamTest(File + "::tool calls::should send empty string content for assistant messages with no tool calls", Coverage = UpstreamCoverage.Covered)]
    public void Sends_empty_string_content_without_tool_calls()
    {
        var result = Convert(new OpenAIAssistantChatMessage(new OpenAIAssistantChatPart[] { new OpenAIAssistantTextPart(string.Empty) }));
        AssertMessages(result, """[{"role":"assistant","content":""}]""");
        Assert.False(result.Messages[0]!.AsObject().ContainsKey("tool_calls"));
    }

    [Fact]
    [UpstreamTest(File + "::tool calls::should default missing tool call input to an empty object", Coverage = UpstreamCoverage.Covered)]
    public void Defaults_missing_tool_input_to_an_empty_object()
    {
        var result = Convert(new OpenAIAssistantChatMessage(new OpenAIAssistantChatPart[]
        {
            new OpenAIAssistantToolCallPart("quux", "thwomp", null),
        }));
        AssertMessages(result, """
            [{"role":"assistant","content":null,"tool_calls":[{"id":"quux","type":"function","function":{"name":"thwomp","arguments":"{}"}}]}]
            """);
    }

    [Fact]
    [UpstreamTest(File + "::tool calls::should normalize malformed tool call input and preserve the tool error", Coverage = UpstreamCoverage.Covered)]
    public void Normalizes_malformed_tool_input()
    {
        var result = OpenAIChatMessages.ConvertToOpenAIChatMessages(new OpenAIChatPromptMessage[]
        {
            new OpenAIAssistantChatMessage(new OpenAIAssistantChatPart[]
            {
                new OpenAIAssistantToolCallPart("quux", "thwomp", JsonValue.Create("{\"foo\":\"bar\"")),
            }),
            new OpenAIToolChatMessage(new[]
            {
                new OpenAIToolResultChatPart("tool-result", "quux", "thwomp", OpenAIToolOutput.FromErrorText("Invalid input: JSON parsing failed")),
            }),
        });
        AssertMessages(result, """
            [
              {"role":"assistant","content":null,"tool_calls":[{"id":"quux","type":"function","function":{"name":"thwomp","arguments":"{}"}}]},
              {"role":"tool","tool_call_id":"quux","content":"Invalid input: JSON parsing failed"}
            ]
            """);
    }

    [Fact]
    [UpstreamTest(File + "::tool calls::should handle different tool output types", Coverage = UpstreamCoverage.Covered)]
    public void Handles_text_and_error_text_outputs()
    {
        var result = Convert(new OpenAIToolChatMessage(new[]
        {
            new OpenAIToolResultChatPart("tool-result", "text-tool", "text-tool", OpenAIToolOutput.FromText("Hello world")),
            new OpenAIToolResultChatPart("tool-result", "error-tool", "error-tool", OpenAIToolOutput.FromErrorText("Something went wrong")),
        }));
        AssertMessages(result, """
            [
              {"role":"tool","tool_call_id":"text-tool","content":"Hello world"},
              {"role":"tool","tool_call_id":"error-tool","content":"Something went wrong"}
            ]
            """);
    }

    private static void AssertAudio(string mediaType, string format)
    {
        var result = Convert(FilePart(mediaType, OpenAIChatFileData.FromBase64("AAECAw==")));
        AssertMessages(result, """[{"role":"user","content":[{"type":"input_audio","input_audio":{"data":"AAECAw==","format":"FORMAT"}}]}]""".Replace("FORMAT", format));
    }

    private static void AssertPdf(OpenAIChatMessageConversion result, string filename, string payload)
    {
        AssertMessages(result, """
            [{"role":"user","content":[{"type":"file","file":{"filename":"NAME","file_data":"data:application/pdf;base64,DATA"}}]}]
            """.Replace("NAME", filename).Replace("DATA", payload));
    }

    private static void AssertPng(string mediaType)
    {
        var result = Convert(FilePart(mediaType, OpenAIChatFileData.FromBase64("iVBORw0KGgo=")));
        AssertMessages(result, """[{"role":"user","content":[{"type":"image_url","image_url":{"url":"data:image/png;base64,iVBORw0KGgo="}}]}]""");
    }

    private static OpenAIUserChatMessage FilePart(string mediaType, OpenAIChatFileData data)
    {
        return new OpenAIUserChatMessage(new OpenAIChatUserPart[] { new OpenAIChatFilePart(mediaType, data) });
    }

    private static OpenAIChatMessageConversion Convert(OpenAIChatPromptMessage message)
    {
        return OpenAIChatMessages.ConvertToOpenAIChatMessages(new[] { message });
    }

    private static void AssertMessages(OpenAIChatMessageConversion result, string expected)
    {
        OpenAIParity.AssertJson(result.Messages.ToJsonString(), expected);
    }

    private static JsonElement Options(string openaiObject)
    {
        return OpenAIParity.Element("{\"openai\":" + openaiObject + "}");
    }
}
