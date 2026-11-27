// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages &gt; user message</c>.</summary>
[Collection("LogWarnings")]
public sealed class UserMessageTests : WarningsOff
{
    private const string Suite = Prefix + "convertToModelMessages > user message::";

    [UpstreamTest(
        Suite + "should convert a simple user message",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_a_simple_user_message()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"user","parts":[{"text":"Hello, AI!","type":"text"}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"user","content":[{"text":"Hello, AI!","type":"text"}]}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should convert a simple user message with provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_a_simple_user_message_with_provider_metadata()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"user","parts":[{"text":"Hello, AI!","type":"text","providerMetadata":{"testProvider":{"signature":"1234567890"}}}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"user","content":[{"providerOptions":{"testProvider":{"signature":"1234567890"}},"text":"Hello, AI!","type":"text"}]}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should handle user message file parts",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_handle_user_message_file_parts()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"user","parts":[{"type":"file","mediaType":"image/jpeg","url":"https://example.com/image.jpg"},{"type":"text","text":"Check this image"}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"user","content":[{"type":"file","mediaType":"image/jpeg","data":{"type":"url","url":"https://example.com/image.jpg"}},{"type":"text","text":"Check this image"}]}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should handle user message file parts with provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_handle_user_message_file_parts_with_provider_metadata()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"user","parts":[{"type":"file","mediaType":"image/jpeg","url":"https://example.com/image.jpg","providerMetadata":{"testProvider":{"signature":"1234567890"}}},{"type":"text","text":"Check this image"}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"user","content":[{"type":"file","mediaType":"image/jpeg","data":{"type":"url","url":"https://example.com/image.jpg"},"providerOptions":{"testProvider":{"signature":"1234567890"}}},{"type":"text","text":"Check this image"}]}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should include filename for user file parts when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_include_filename_for_user_file_parts_when_provided()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"user","parts":[{"type":"file","mediaType":"image/jpeg","url":"https://example.com/image.jpg","filename":"image.jpg"}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"user","content":[{"type":"file","mediaType":"image/jpeg","data":{"type":"url","url":"https://example.com/image.jpg"},"filename":"image.jpg"}]}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should use providerReference as data for user file parts",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_use_providerReference_as_data_for_user_file_parts()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"user","parts":[{"type":"file","mediaType":"application/pdf","filename":"doc.pdf","url":"data:application/pdf;base64,abc","providerReference":{"openai":"file-abc123"}},{"type":"text","text":"Summarize this"}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"user","content":[{"type":"file","mediaType":"application/pdf","filename":"doc.pdf","data":{"type":"reference","reference":{"openai":"file-abc123"}}},{"type":"text","text":"Summarize this"}]}]
            """,
            result
        );
    }
}
