// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Converter = Vercel.AI.Mistral.ConvertToMistralChatMessages;

namespace Vercel.AI.Tests.Upstream.Mistral.ConvertToMistralChatMessages;

/// <summary>Port of <c>convert-to-mistral-chat-messages.test.ts</c> &gt; <c>user messages</c>.</summary>
public sealed class UserMessagesTests
{
    private const string Prefix =
        "packages/mistral/src/convert-to-mistral-chat-messages.test.ts::user messages::";

    private static void AssertJson(JsonNode? actual, string expected) =>
        Assert.True(JsonNode.DeepEquals(actual, JsonNode.Parse(expected)), actual?.ToJsonString());

    [Fact]
    [UpstreamTest(
        Prefix + "should convert messages with image parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_messages_with_image_parts()
    {
        var result = Converter.Convert(
            new ModelMessage[]
            {
                new UserModelMessage(
                    new UserContentPart[]
                    {
                        new TextContentPart("Hello"),
                        new FileContentPart("image/png", null, new byte[] { 0, 1, 2, 3 }, null),
                    }
                ),
            }
        );

        AssertJson(
            result,
            "[{\"content\":[{\"text\":\"Hello\",\"type\":\"text\"},{\"image_url\":\"data:image/png;base64,AAECAw==\",\"type\":\"image_url\"}],\"role\":\"user\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert messages with image parts from Uint8Array",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_messages_with_image_parts_from_Uint8Array()
    {
        var result = Converter.Convert(
            new ModelMessage[]
            {
                new UserModelMessage(
                    new UserContentPart[]
                    {
                        new TextContentPart("Hi"),
                        new FileContentPart("image/png", null, new byte[] { 0, 1, 2, 3 }, null),
                    }
                ),
            }
        );

        AssertJson(
            result,
            "[{\"content\":[{\"text\":\"Hi\",\"type\":\"text\"},{\"image_url\":\"data:image/png;base64,AAECAw==\",\"type\":\"image_url\"}],\"role\":\"user\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert messages with PDF file parts using URL",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_messages_with_PDF_file_parts_using_URL()
    {
        var result = Converter.Convert(
            new ModelMessage[]
            {
                new UserModelMessage(
                    new UserContentPart[]
                    {
                        new TextContentPart("Please analyze this document"),
                        new FileContentPart(
                            "application/pdf",
                            "https://example.com/document.pdf",
                            null,
                            null
                        ),
                    }
                ),
            }
        );

        AssertJson(
            result,
            "[{\"content\":[{\"text\":\"Please analyze this document\",\"type\":\"text\"},{\"document_url\":\"https://example.com/document.pdf\",\"type\":\"document_url\"}],\"role\":\"user\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert messages with PDF file parts from Uint8Array",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_messages_with_PDF_file_parts_from_Uint8Array()
    {
        var result = Converter.Convert(
            new ModelMessage[]
            {
                new UserModelMessage(
                    new UserContentPart[]
                    {
                        new TextContentPart("Analyze this PDF"),
                        new FileContentPart(
                            "application/pdf",
                            null,
                            new byte[] { 0, 1, 2, 3 },
                            null
                        ),
                    }
                ),
            }
        );

        AssertJson(
            result,
            "[{\"content\":[{\"text\":\"Analyze this PDF\",\"type\":\"text\"},{\"document_url\":\"data:application/pdf;base64,AAECAw==\",\"type\":\"document_url\"}],\"role\":\"user\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert messages with reasoning content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_messages_with_reasoning_content()
    {
        var result = Converter.Convert(
            new ModelMessage[]
            {
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantReasoningPart("Let me think about this..."),
                        new AssistantTextPart("The answer is 42."),
                    }
                ),
            }
        );

        AssertJson(
            result,
            "[{\"content\":[{\"closed\":true,\"thinking\":[{\"text\":\"Let me think about this...\",\"type\":\"text\"}],\"type\":\"thinking\"},{\"text\":\"The answer is 42.\",\"type\":\"text\"}],\"prefix\":true,\"role\":\"assistant\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve the ordering of interleaved reasoning and text content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_the_ordering_of_interleaved_reasoning_and_text_content()
    {
        var result = Converter.Convert(
            new ModelMessage[]
            {
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantReasoningPart("First thought."),
                        new AssistantTextPart("Partial answer."),
                        new AssistantReasoningPart("Second thought."),
                        new AssistantTextPart("Final answer."),
                    }
                ),
                new UserModelMessage("Continue."),
            }
        );

        AssertJson(
            result[0],
            "{\"role\":\"assistant\",\"content\":[{\"type\":\"thinking\",\"thinking\":[{\"type\":\"text\",\"text\":\"First thought.\"}],\"closed\":true},{\"type\":\"text\",\"text\":\"Partial answer.\"},{\"type\":\"thinking\",\"thinking\":[{\"type\":\"text\",\"text\":\"Second thought.\"}],\"closed\":true},{\"type\":\"text\",\"text\":\"Final answer.\"}]}"
        );
    }
}
