// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Groq.ConvertToGroqChatMessages.ConvertToGroqChatMessagesSupport;

namespace Vercel.AI.Tests.Upstream.Groq.ConvertToGroqChatMessages;

/// <summary>Port of <c>convert-to-groq-chat-messages.test.ts</c> &gt; <c>user messages</c>.</summary>
public sealed class UserMessagesTests
{
    private const string Prefix = TestFile + "::user messages::";

    private const string ImageExpected = """
        [{"content":[{"text":"HELLO","type":"text"},
          {"image_url":{"url":"data:image/png;base64,AAECAw=="},"type":"image_url"}],
          "role":"user"}]
        """;

    [Fact]
    [UpstreamTest(
        Prefix + "should convert messages with image parts",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66",
        Note = "The port has no base64 string file data, so the same bytes are passed as inline data."
    )]
    public void Should_convert_messages_with_image_parts()
    {
        var result = Run(User(new TextContentPart("Hello"), ImageData()));

        JsonAssert.Equal(result, ImageExpected.Replace("HELLO", "Hello"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert messages with image parts from Uint8Array",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Should_convert_messages_with_image_parts_from_Uint8Array()
    {
        var result = Run(User(new TextContentPart("Hi"), ImageData()));

        JsonAssert.Equal(result, ImageExpected.Replace("HELLO", "Hi"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert messages with only a text part to a string content",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Should_convert_messages_with_only_a_text_part_to_a_string_content()
    {
        var result = Run(User(new TextContentPart("Hello")));

        JsonAssert.Equal(result, """[{"content":"Hello","role":"user"}]""");
    }

    private static FileContentPart ImageData() => new("image/png", null, Bytes0123, null);
}
