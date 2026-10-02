// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Perplexity;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class PerplexityMessageTests
{
    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/convert-to-perplexity-messages.test.ts::convertToPerplexityMessages > system messages::should convert a system message with text content",
        Coverage = UpstreamCoverage.Covered)]
    public void Converts_a_system_message()
    {
        var messages = PerplexityMessages.Convert(new ModelMessage[] { new SystemModelMessage("System initialization") });

        Assert.Equal("system", messages[0]!["role"]!.GetValue<string>());
        Assert.Equal("System initialization", messages[0]!["content"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/convert-to-perplexity-messages.test.ts::convertToPerplexityMessages > user messages::should convert a user message with text parts",
        Coverage = UpstreamCoverage.Covered)]
    public void Joins_user_text_parts()
    {
        var messages = PerplexityMessages.Convert(new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[]
            {
                new TextContentPart("Hello "),
                new TextContentPart("World"),
            }),
        });

        Assert.Equal("user", messages[0]!["role"]!.GetValue<string>());
        Assert.Equal("Hello World", messages[0]!["content"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/convert-to-perplexity-messages.test.ts::convertToPerplexityMessages > user messages::should convert a user message with image parts",
        Coverage = UpstreamCoverage.Covered)]
    public void Converts_inline_image_bytes()
    {
        var messages = PerplexityMessages.Convert(new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[]
            {
                new TextContentPart("Hello "),
                new FileContentPart("image/png", null, new byte[] { 0, 1, 2, 3 }, null),
            }),
        });
        var content = messages[0]!["content"]!.AsArray();

        Assert.Equal("text", content[0]!["type"]!.GetValue<string>());
        Assert.Equal("Hello ", content[0]!["text"]!.GetValue<string>());
        Assert.Equal("image_url", content[1]!["type"]!.GetValue<string>());
        Assert.Equal("data:image/png;base64,AAECAw==", content[1]!["image_url"]!["url"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/convert-to-perplexity-messages.test.ts::convertToPerplexityMessages > assistant messages::should convert an assistant message with text content",
        Coverage = UpstreamCoverage.Covered)]
    public void Converts_assistant_text()
    {
        var messages = PerplexityMessages.Convert(new ModelMessage[]
        {
            new AssistantModelMessage("Assistant reply", null, null),
        });

        Assert.Equal("assistant", messages[0]!["role"]!.GetValue<string>());
        Assert.Equal("Assistant reply", messages[0]!["content"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/convert-to-perplexity-messages.test.ts::convertToPerplexityMessages > tool messages::should throw an error for tool messages",
        Coverage = UpstreamCoverage.Covered)]
    public void Rejects_tool_messages()
    {
        var exception = Assert.Throws<AiSdkException>(() => PerplexityMessages.Convert(new ModelMessage[]
        {
            new ToolModelMessage("dummy-tool-call-id", "dummy-tool-name", "\"This should fail\"", false),
        }));

        Assert.Contains("Tool messages", exception.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/convert-to-perplexity-messages.test.ts::convertToPerplexityMessages > tool messages::should throw for file parts with provider references",
        Coverage = UpstreamCoverage.Covered)]
    public void Rejects_provider_file_references()
    {
        var exception = Assert.Throws<AiSdkException>(() => PerplexityMessages.Convert(new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[]
            {
                new PerplexityFilePart("image/png", null, null, null, null, isReference: true),
            }),
        }));

        Assert.Contains("provider references", exception.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/convert-to-perplexity-messages.test.ts::convertToPerplexityMessages > top-level-only media type resolution::passes full image/png through unchanged for inline data",
        Coverage = UpstreamCoverage.Covered)]
    public void Keeps_a_full_image_media_type()
    {
        const string png = "iVBORw0KGgo=";
        var part = Image(PerplexityMessages.Convert(UserFile("image/png", png)));

        Assert.Equal("image_url", part.GetProperty("type").GetString());
        Assert.Equal("data:image/png;base64," + png, part.GetProperty("image_url").GetProperty("url").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/convert-to-perplexity-messages.test.ts::convertToPerplexityMessages > top-level-only media type resolution::detects image subtype from inline bytes for top-level \"image\"",
        Coverage = UpstreamCoverage.Covered)]
    public void Detects_png_from_a_top_level_image_type()
    {
        const string png = "iVBORw0KGgo=";
        var part = Image(PerplexityMessages.Convert(UserFile("image", png)));

        Assert.Equal("data:image/png;base64," + png, part.GetProperty("image_url").GetProperty("url").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/convert-to-perplexity-messages.test.ts::convertToPerplexityMessages > top-level-only media type resolution::passes through URL source for top-level-only image",
        Coverage = UpstreamCoverage.Covered)]
    public void Passes_through_an_image_url()
    {
        var messages = PerplexityMessages.Convert(new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[]
            {
                new FileContentPart("image", "https://example.com/x.png", null, null),
            }),
        });
        var part = Image(messages);

        Assert.Equal("image_url", part.GetProperty("type").GetString());
        Assert.Equal("https://example.com/x.png", part.GetProperty("image_url").GetProperty("url").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/convert-to-perplexity-messages.test.ts::convertToPerplexityMessages > top-level-only media type resolution::converts a top-level-only \"application\" PDF into a file_url part",
        Coverage = UpstreamCoverage.Covered)]
    public void Converts_a_detected_pdf_to_file_url()
    {
        const string pdf = "JVBERi0xLjQ=";
        var messages = PerplexityMessages.Convert(new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[]
            {
                new PerplexityFilePart("application", null, null, pdf, "doc.pdf"),
            }),
        });
        var part = Image(messages);

        Assert.Equal("file_url", part.GetProperty("type").GetString());
        Assert.Equal(pdf, part.GetProperty("file_url").GetProperty("url").GetString());
        Assert.Equal("doc.pdf", part.GetProperty("file_name").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/convert-to-perplexity-messages.test.ts::convertToPerplexityMessages > top-level-only media type resolution::throws for unsupported file media types instead of dropping them",
        Coverage = UpstreamCoverage.Covered)]
    public void Rejects_audio_files()
    {
        var exception = Assert.Throws<AiSdkException>(() => PerplexityMessages.Convert(new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[]
            {
                new PerplexityFilePart("audio/mpeg", null, null, "SUQzBAAAAAAAI1RTU0UAAAAPAAADTGF2ZjU4", "clip.mp3"),
            }),
        }));

        Assert.Contains("audio/mpeg", exception.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/convert-to-perplexity-messages.test.ts::convertToPerplexityMessages > top-level-only media type resolution::normalizes image/* wildcard via detection",
        Coverage = UpstreamCoverage.Covered)]
    public void Detects_png_for_an_image_wildcard()
    {
        const string png = "iVBORw0KGgo=";
        var part = Image(PerplexityMessages.Convert(UserFile("image/*", png)));

        Assert.Equal("data:image/png;base64," + png, part.GetProperty("image_url").GetProperty("url").GetString());
    }

    private static ModelMessage[] UserFile(string mediaType, string inline)
    {
        return new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[]
            {
                new PerplexityFilePart(mediaType, null, null, inline, null),
            }),
        };
    }

    private static JsonElement Image(System.Text.Json.Nodes.JsonArray messages)
    {
        using var document = JsonDocument.Parse(messages.ToJsonString());
        return document.RootElement[0].GetProperty("content")[0].Clone();
    }
}
