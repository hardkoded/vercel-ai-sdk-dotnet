// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Chat Completions message conversion.</summary>
public sealed class OpenAIChatMessageUpstreamTests
{
    private const string Prefix = "packages/openai/src/chat/convert-to-openai-chat-messages.test.ts::";

    [Fact]
    [UpstreamTest(Prefix + "system messages::should forward system messages", Coverage = UpstreamCoverage.Covered)]
    public void ForwardsSystemMessages()
    {
        var result = OpenAIChatMessages.Convert(new[] { System("You are a helpful assistant.") });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"system\",\"content\":\"You are a helpful assistant.\"}]");
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "system messages::should convert system messages to developer messages when requested", Coverage = UpstreamCoverage.Covered)]
    public void ConvertsSystemMessagesToDeveloper()
    {
        var result = OpenAIChatMessages.Convert(new[] { System("You are a helpful assistant.") }, "developer");
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"developer\",\"content\":\"You are a helpful assistant.\"}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "system messages::should add a prompt cache breakpoint to a system message", Coverage = UpstreamCoverage.Covered)]
    public void AddsSystemPromptCacheBreakpoint()
    {
        var message = System("You are a helpful assistant.");
        message.ProviderOptions = Breakpoint();
        var result = OpenAIChatMessages.Convert(new[] { message });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"system\",\"content\":[{\"type\":\"text\",\"text\":\"You are a helpful assistant.\",\"prompt_cache_breakpoint\":{\"mode\":\"explicit\"}}]}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "system messages::should remove system messages when requested", Coverage = UpstreamCoverage.Covered)]
    public void RemovesSystemMessages()
    {
        var result = OpenAIChatMessages.Convert(new[] { System("You are a helpful assistant.") }, "remove");
        Assert.Empty(result.Messages);
        Assert.Equal("other", result.Warnings[0].Type);
        Assert.Equal("system messages are removed for this model", result.Warnings[0].Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages::should convert messages with only a text part to a string content", Coverage = UpstreamCoverage.Covered)]
    public void ConvertsSingleTextToString()
    {
        var result = OpenAIChatMessages.Convert(new[] { User(Text("Hello")) });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"user\",\"content\":\"Hello\"}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages::should add prompt cache breakpoints to supported content blocks", Coverage = UpstreamCoverage.Covered)]
    public void AddsPromptCacheBreakpoints()
    {
        var result = OpenAIChatMessages.Convert(new[]
        {
            User(
                Text("Hello", Breakpoint()),
                File("image/png", url: "https://example.com/image.png", providerOptions: Breakpoint()),
                File("audio/wav", base64: "AAECAw==", providerOptions: Breakpoint()),
                new OpenAIChatPromptPart("file") { FileId = "file-pdf-123", ProviderOptions = Breakpoint() }),
        });
        OpenAIUpstream.Equal(
            result.Messages,
            "[{\"role\":\"user\",\"content\":["
            + "{\"type\":\"text\",\"text\":\"Hello\",\"prompt_cache_breakpoint\":{\"mode\":\"explicit\"}},"
            + "{\"type\":\"image_url\",\"image_url\":{\"url\":\"https://example.com/image.png\"},\"prompt_cache_breakpoint\":{\"mode\":\"explicit\"}},"
            + "{\"type\":\"input_audio\",\"input_audio\":{\"data\":\"AAECAw==\",\"format\":\"wav\"},\"prompt_cache_breakpoint\":{\"mode\":\"explicit\"}},"
            + "{\"type\":\"file\",\"file\":{\"file_id\":\"file-pdf-123\"},\"prompt_cache_breakpoint\":{\"mode\":\"explicit\"}}"
            + "]}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages::should convert messages with image parts", Coverage = UpstreamCoverage.Covered)]
    public void ConvertsImageParts()
    {
        var result = OpenAIChatMessages.Convert(new[] { User(Text("Hello"), File("image/png", base64: "AAECAw==")) });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"},{\"type\":\"image_url\",\"image_url\":{\"url\":\"data:image/png;base64,AAECAw==\"}}]}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages::should convert messages with Uint8Array image parts to data URLs", Coverage = UpstreamCoverage.Covered)]
    public void ConvertsImageBytesToDataUrls()
    {
        var result = OpenAIChatMessages.Convert(new[] { User(File("image/jpeg", data: new byte[] { 0xff, 0xd8, 0xff, 0xe0 })) });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"user\",\"content\":[{\"type\":\"image_url\",\"image_url\":{\"url\":\"data:image/jpeg;base64,/9j/4A==\"}}]}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages::should add image detail when specified through extension", Coverage = UpstreamCoverage.Covered)]
    public void AddsImageDetail()
    {
        var result = OpenAIChatMessages.Convert(new[] { User(File("image/png", base64: "AAECAw==", providerOptions: OpenAIUpstream.Json("{\"openai\":{\"imageDetail\":\"low\"}}"))) });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"user\",\"content\":[{\"type\":\"image_url\",\"image_url\":{\"url\":\"data:image/png;base64,AAECAw==\",\"detail\":\"low\"}}]}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages > file parts::should throw for unsupported mime types", Coverage = UpstreamCoverage.Covered)]
    public void ThrowsForUnsupportedMimeTypes()
    {
        var exception = Assert.Throws<AiSdkException>(() => OpenAIChatMessages.Convert(new[] { User(File("application/something", base64: "AAECAw==")) }));
        Assert.Equal("file part media type application/something", exception.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages > file parts::should throw for URL data", Coverage = UpstreamCoverage.Covered)]
    public void ThrowsForAudioUrls()
    {
        var exception = Assert.Throws<AiSdkException>(() => OpenAIChatMessages.Convert(new[] { User(File("audio/wav", url: "https://example.com/foo.wav")) }));
        Assert.Equal("audio file parts with URLs", exception.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages > file parts::should add audio content for audio/wav file parts", Coverage = UpstreamCoverage.Covered)]
    public void AddsWavAudio()
    {
        EqualAudio("audio/wav", "wav");
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages > file parts::should add audio content for audio/mpeg file parts", Coverage = UpstreamCoverage.Covered)]
    public void AddsMpegAudio()
    {
        EqualAudio("audio/mpeg", "mp3");
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages > file parts::should add audio content for audio/mp3 file parts", Coverage = UpstreamCoverage.Covered)]
    public void AddsMp3Audio()
    {
        EqualAudio("audio/mp3", "mp3");
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages > file parts::should convert messages with PDF file parts", Coverage = UpstreamCoverage.Covered)]
    public void ConvertsPdfFileParts()
    {
        var result = OpenAIChatMessages.Convert(new[] { User(File("application/pdf", base64: "AQIDBAU=", fileName: "document.pdf")) });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"user\",\"content\":[{\"type\":\"file\",\"file\":{\"filename\":\"document.pdf\",\"file_data\":\"data:application/pdf;base64,AQIDBAU=\"}}]}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages > file parts::should convert messages with binary PDF file parts", Coverage = UpstreamCoverage.Covered)]
    public void ConvertsBinaryPdfFileParts()
    {
        var result = OpenAIChatMessages.Convert(new[] { User(File("application/pdf", data: new byte[] { 1, 2, 3, 4, 5 }, fileName: "document.pdf")) });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"user\",\"content\":[{\"type\":\"file\",\"file\":{\"filename\":\"document.pdf\",\"file_data\":\"data:application/pdf;base64,AQIDBAU=\"}}]}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages > file parts::should convert messages with PDF file parts using provider reference", Coverage = UpstreamCoverage.Covered)]
    public void ConvertsPdfProviderReference()
    {
        var result = OpenAIChatMessages.Convert(new[] { User(new OpenAIChatPromptPart("file") { MediaType = "application/pdf", FileId = "file-pdf-12345" }) });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"user\",\"content\":[{\"type\":\"file\",\"file\":{\"file_id\":\"file-pdf-12345\"}}]}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages > file parts::should convert messages with image parts using provider reference", Coverage = UpstreamCoverage.Covered)]
    public void ConvertsImageProviderReference()
    {
        var result = OpenAIChatMessages.Convert(new[] { User(new OpenAIChatPromptPart("file") { MediaType = "image/png", FileId = "file-img-12345" }) });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"user\",\"content\":[{\"type\":\"file\",\"file\":{\"file_id\":\"file-img-12345\"}}]}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages > file parts::should use default filename for PDF file parts when not provided", Coverage = UpstreamCoverage.Covered)]
    public void UsesDefaultPdfFilename()
    {
        var result = OpenAIChatMessages.Convert(new[] { User(File("application/pdf", base64: "AQIDBAU=")) });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"user\",\"content\":[{\"type\":\"file\",\"file\":{\"filename\":\"part-0.pdf\",\"file_data\":\"data:application/pdf;base64,AQIDBAU=\"}}]}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages > file parts::should throw error for unsupported file types", Coverage = UpstreamCoverage.Covered)]
    public void ThrowsForPlainTextFiles()
    {
        var exception = Assert.Throws<AiSdkException>(() => OpenAIChatMessages.Convert(new[] { User(File("text/plain", base64: "AQIDBAU=")) }));
        Assert.Equal("file part media type text/plain", exception.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages > file parts::should throw error for file URLs", Coverage = UpstreamCoverage.Covered)]
    public void ThrowsForPdfUrls()
    {
        var exception = Assert.Throws<AiSdkException>(() => OpenAIChatMessages.Convert(new[] { User(File("application/pdf", url: "https://example.com/document.pdf")) }));
        Assert.Equal("PDF file parts with URLs", exception.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages > file parts > top-level-only media type resolution::detects image subtype from inline bytes for top-level \"image\"", Coverage = UpstreamCoverage.Covered)]
    public void DetectsImageSubtype()
    {
        EqualDetected("image");
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages > file parts > top-level-only media type resolution::normalizes image/* wildcard via detection", Coverage = UpstreamCoverage.Covered)]
    public void NormalizesImageWildcard()
    {
        EqualDetected("image/*");
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages > file parts > top-level-only media type resolution::passes through URL source for top-level-only image (provider accepts raw URL)", Coverage = UpstreamCoverage.Covered)]
    public void PassesImageUrl()
    {
        var result = OpenAIChatMessages.Convert(new[] { User(File("image", url: "https://example.com/x.png")) });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"user\",\"content\":[{\"type\":\"image_url\",\"image_url\":{\"url\":\"https://example.com/x.png\"}}]}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages > file parts > top-level-only media type resolution::throws for top-level-only application (PDF requires full resolution) with URL source", Coverage = UpstreamCoverage.Covered)]
    public void ThrowsForApplicationUrl()
    {
        var exception = Assert.Throws<UnsupportedFunctionalityException>(() => OpenAIChatMessages.Convert(new[] { User(File("application", url: "https://example.com/x.pdf")) }));
        Assert.Equal("file of media type \"application\" must specify subtype since it is not passed as inline bytes", exception.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "user messages > file parts > top-level-only media type resolution::preserves full image/png pass-through", Coverage = UpstreamCoverage.Covered)]
    public void PreservesPngMediaType()
    {
        EqualDetected("image/png");
    }

    [Fact]
    [UpstreamTest(Prefix + "tool calls::should add a prompt cache breakpoint to assistant text content", Coverage = UpstreamCoverage.Covered)]
    public void AddsAssistantPromptCacheBreakpoint()
    {
        var result = OpenAIChatMessages.Convert(new[]
        {
            new OpenAIChatPromptMessage("assistant")
            {
                Parts = new[] { Text("Cached assistant content", Breakpoint()) },
            },
        });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Cached assistant content\",\"prompt_cache_breakpoint\":{\"mode\":\"explicit\"}}]}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "tool calls::should add a prompt cache breakpoint to tool text content", Coverage = UpstreamCoverage.Covered)]
    public void AddsToolPromptCacheBreakpoint()
    {
        var result = OpenAIChatMessages.Convert(new[]
        {
            new OpenAIChatPromptMessage("tool")
            {
                ToolCallId = "cached-tool",
                ToolOutput = "Cached tool content",
                ProviderOptions = Breakpoint(),
            },
        });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"tool\",\"tool_call_id\":\"cached-tool\",\"content\":[{\"type\":\"text\",\"text\":\"Cached tool content\",\"prompt_cache_breakpoint\":{\"mode\":\"explicit\"}}]}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "tool calls::should stringify arguments to tool calls", Coverage = UpstreamCoverage.Covered)]
    public void StringifiesToolCallArguments()
    {
        var result = OpenAIChatMessages.Convert(new[]
        {
            new OpenAIChatPromptMessage("assistant")
            {
                ToolCalls = new[] { new GeneratedToolCall("quux", "thwomp", "{\"foo\":\"bar123\"}") },
            },
            new OpenAIChatPromptMessage("tool") { ToolCallId = "quux", ToolOutput = "{\"oof\":\"321rab\"}" },
        });
        OpenAIUpstream.Equal(
            result.Messages,
            "[{\"role\":\"assistant\",\"content\":null,\"tool_calls\":[{\"id\":\"quux\",\"type\":\"function\",\"function\":{\"name\":\"thwomp\",\"arguments\":\"{\\\"foo\\\":\\\"bar123\\\"}\"}}]},{\"role\":\"tool\",\"tool_call_id\":\"quux\",\"content\":\"{\\\"oof\\\":\\\"321rab\\\"}\"}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "tool calls::should send empty string content for assistant messages with no tool calls", Coverage = UpstreamCoverage.Covered)]
    public void SendsEmptyAssistantContent()
    {
        var result = OpenAIChatMessages.Convert(new[]
        {
            new OpenAIChatPromptMessage("assistant") { Parts = new[] { Text(string.Empty) } },
        });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"assistant\",\"content\":\"\"}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "tool calls::should default missing tool call input to an empty object", Coverage = UpstreamCoverage.Covered)]
    public void DefaultsMissingToolInput()
    {
        var result = OpenAIChatMessages.Convert(new[]
        {
            new OpenAIChatPromptMessage("assistant")
            {
                ToolCalls = new[] { new GeneratedToolCall("quux", "thwomp", string.Empty) },
            },
        });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"assistant\",\"content\":null,\"tool_calls\":[{\"id\":\"quux\",\"type\":\"function\",\"function\":{\"name\":\"thwomp\",\"arguments\":\"{}\"}}]}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "tool calls::should normalize malformed tool call input and preserve the tool error", Coverage = UpstreamCoverage.Covered)]
    public void NormalizesMalformedToolInput()
    {
        var result = OpenAIChatMessages.Convert(new[]
        {
            new OpenAIChatPromptMessage("assistant")
            {
                ToolCalls = new[] { new GeneratedToolCall("quux", "thwomp", "{\"foo\":\"bar\"") },
            },
            new OpenAIChatPromptMessage("tool") { ToolCallId = "quux", ToolOutput = "Invalid input: JSON parsing failed" },
        });
        OpenAIUpstream.Equal(
            result.Messages,
            "[{\"role\":\"assistant\",\"content\":null,\"tool_calls\":[{\"id\":\"quux\",\"type\":\"function\",\"function\":{\"name\":\"thwomp\",\"arguments\":\"{}\"}}]},{\"role\":\"tool\",\"tool_call_id\":\"quux\",\"content\":\"Invalid input: JSON parsing failed\"}]");
    }

    private static void EqualAudio(string mediaType, string format)
    {
        var result = OpenAIChatMessages.Convert(new[] { User(File(mediaType, base64: "AAECAw==")) });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"user\",\"content\":[{\"type\":\"input_audio\",\"input_audio\":{\"data\":\"AAECAw==\",\"format\":\"" + format + "\"}}]}]");
    }

    private static void EqualDetected(string mediaType)
    {
        var result = OpenAIChatMessages.Convert(new[] { User(File(mediaType, base64: "iVBORw0KGgo=")) });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"user\",\"content\":[{\"type\":\"image_url\",\"image_url\":{\"url\":\"data:image/png;base64,iVBORw0KGgo=\"}}]}]");
    }

    private static OpenAIChatPromptMessage System(string text)
    {
        return new OpenAIChatPromptMessage("system") { Text = text };
    }

    private static OpenAIChatPromptMessage User(params OpenAIChatPromptPart[] parts)
    {
        return new OpenAIChatPromptMessage("user") { Parts = parts };
    }

    private static OpenAIChatPromptPart Text(string text, JsonElement? providerOptions = null)
    {
        return new OpenAIChatPromptPart("text") { Text = text, ProviderOptions = providerOptions };
    }

    private static OpenAIChatPromptPart File(string mediaType, byte[]? data = null, string? base64 = null, string? url = null, string? fileName = null, JsonElement? providerOptions = null)
    {
        return new OpenAIChatPromptPart("file")
        {
            MediaType = mediaType,
            Data = data,
            Base64 = base64,
            Url = url,
            FileName = fileName,
            ProviderOptions = providerOptions,
        };
    }

    private static JsonElement Breakpoint()
    {
        return OpenAIUpstream.Json("{\"openai\":{\"promptCacheBreakpoint\":{\"mode\":\"explicit\"}}}");
    }
}
