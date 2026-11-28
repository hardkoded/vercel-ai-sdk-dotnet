// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.MoonshotAI.ConvertToMoonshotAIChatMessages.MoonshotMessagesSupport;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.ConvertToMoonshotAIChatMessages;

/// <summary>Port of <c>convert-to-moonshotai-chat-messages.test.ts</c> &gt; <c>user messages</c>.</summary>
public sealed class UserMessagesTests
{
    private const string Suite = Prefix + "user messages::";

    private static void AssertUnsupported(string functionality, params ModelMessage[] prompt)
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(() => ConvertMessages(prompt));
        Assert.Contains($"'{functionality}' functionality not supported", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Suite + "should convert a text-only user message to string content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_a_text_only_user_message_to_string_content()
    {
        var result = ConvertMessages(User(new TextContentPart("Hello")));

        Equal(result, """[{"role":"user","content":"Hello"}]""");
    }

    [Fact]
    [UpstreamTest(
        Suite + "should convert image data parts to image_url data URIs",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_image_data_parts_to_image_url_data_URIs()
    {
        var result = ConvertMessages(User(new TextContentPart("Hi"), DataFile("image/png")));

        Equal(
            result,
            """[{"role":"user","content":[{"type":"text","text":"Hi"},{"type":"image_url","image_url":{"url":"data:image/png;base64,AAECAw=="}}]}]"""
        );
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/gif")]
    [InlineData("image/webp")]
    [InlineData("image/bmp")]
    [InlineData("image/heic")]
    [InlineData("image/heif")]
    [UpstreamTest(
        Suite + "should accept supported image media type %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_accept_supported_image_media_type(string mediaType)
    {
        var result = ConvertMessages(User(DataFile(mediaType)));

        Equal(
            result,
            $$$"""[{"role":"user","content":[{"type":"image_url","image_url":{"url":"data:{{{mediaType}}};base64,AAECAw=="}}]}]"""
        );
    }

    [Fact]
    [UpstreamTest(Suite + "should pass through image URLs", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_through_image_URLs()
    {
        var result = ConvertMessages(User(UrlFile("image/*", "https://example.com/image.jpg")));

        Equal(
            result,
            """[{"role":"user","content":[{"type":"image_url","image_url":{"url":"https://example.com/image.jpg"}}]}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should convert video data parts to video_url data URIs",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_video_data_parts_to_video_url_data_URIs()
    {
        var result = ConvertMessages(
            User(new TextContentPart("Describe this video"), DataFile("video/mp4"))
        );

        Equal(
            result,
            """[{"role":"user","content":[{"type":"text","text":"Describe this video"},{"type":"video_url","video_url":{"url":"data:video/mp4;base64,AAECAw=="}}]}]"""
        );
    }

    [Theory]
    [InlineData("video/mp4")]
    [InlineData("video/mpeg")]
    [InlineData("video/mov")]
    [InlineData("video/avi")]
    [InlineData("video/x-flv")]
    [InlineData("video/mpg")]
    [InlineData("video/webm")]
    [InlineData("video/wmv")]
    [InlineData("video/3gpp")]
    [UpstreamTest(
        Suite + "should accept supported video media type %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_accept_supported_video_media_type(string mediaType)
    {
        var result = ConvertMessages(User(DataFile(mediaType)));

        Equal(
            result,
            $$$"""[{"role":"user","content":[{"type":"video_url","video_url":{"url":"data:{{{mediaType}}};base64,AAECAw=="}}]}]"""
        );
    }

    [Fact]
    [UpstreamTest(Suite + "should pass through video URLs", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_through_video_URLs()
    {
        var result = ConvertMessages(User(UrlFile("video/*", "https://example.com/video.mp4")));

        Equal(
            result,
            """[{"role":"user","content":[{"type":"video_url","video_url":{"url":"https://example.com/video.mp4"}}]}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should pass through ms:// file references from the Files API",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_ms_file_references_from_the_Files_API()
    {
        var result = ConvertMessages(User(UrlFile("video/mp4", "ms://file-abc123")));

        Equal(
            result,
            """[{"role":"user","content":[{"type":"video_url","video_url":{"url":"ms://file-abc123"}}]}]"""
        );
    }

    [Theory]
    [InlineData("image")]
    [InlineData("video")]
    [UpstreamTest(
        Suite + "should pass through ms:// file references with top-level $mediaType media types",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_ms_file_references_with_top_level_mediaType_media_types(
        string mediaType
    )
    {
        var result = ConvertMessages(User(UrlFile(mediaType, $"ms://file-{mediaType}")));

        Equal(
            result,
            $$$"""[{"role":"user","content":[{"type":"{{{mediaType}}}_url","{{{mediaType}}}_url":{"url":"ms://file-{{{mediaType}}}"}}]}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should decode text/* file parts into text parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_decode_text_file_parts_into_text_parts()
    {
        var result = ConvertMessages(
            User(
                new FileContentPart(
                    "text/markdown",
                    null,
                    System.Text.Encoding.UTF8.GetBytes("hello markdown"),
                    null
                )
            )
        );

        Equal(result, """[{"role":"user","content":[{"type":"text","text":"hello markdown"}]}]""");
    }

    [Theory]
    [InlineData("image/svg+xml")]
    [InlineData("image/tiff")]
    [InlineData("video/quicktime")]
    [UpstreamTest(
        Suite + "should throw for unsupported multimodal media type %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_for_unsupported_multimodal_media_type(string mediaType)
    {
        AssertUnsupported($"file part media type {mediaType}", User(DataFile(mediaType)));
    }

    [Fact]
    [UpstreamTest(
        Suite + "should throw for audio file parts (rejected by the API)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_for_audio_file_parts_rejected_by_the_API()
    {
        AssertUnsupported("file part media type audio/mp3", User(DataFile("audio/mp3")));
    }

    [Fact]
    [UpstreamTest(
        Suite + "should throw for PDF file parts (rejected by the API)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_for_PDF_file_parts_rejected_by_the_API()
    {
        AssertUnsupported(
            "file part media type application/pdf",
            User(DataFile("application/pdf"))
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should throw for unsupported file types",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_for_unsupported_file_types()
    {
        AssertUnsupported(
            "file part media type application/zip",
            User(DataFile("application/zip"))
        );
    }

    [Theory]
    [InlineData("image/png", "ms://image-file-123", "image")]
    [InlineData("video/mp4", "ms://video-file-123", "video")]
    [UpstreamTest(
        Suite + "should convert $mediaType Moonshot provider references",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_mediaType_Moonshot_provider_references(
        string mediaType,
        string reference,
        string kind
    )
    {
        var result = ConvertMessages(
            User(
                FileContentPart.FromProviderReference(
                    mediaType,
                    new Dictionary<string, string> { ["moonshotai"] = reference }
                )
            )
        );

        Equal(
            result,
            $$$"""[{"role":"user","content":[{"type":"{{{kind}}}_url","{{{kind}}}_url":{"url":"{{{reference}}}"}}]}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should throw for foreign provider references",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_for_foreign_provider_references()
    {
        var error = Assert.ThrowsAny<Exception>(() =>
            ConvertMessages(
                User(
                    FileContentPart.FromProviderReference(
                        "image/png",
                        new Dictionary<string, string> { ["openai"] = "ms://image-file-123" }
                    )
                )
            )
        );

        Assert.Contains("No provider reference found for provider 'moonshotai'", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Suite + "should throw for Moonshot provider references without an ms:// URL",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_for_Moonshot_provider_references_without_an_ms_URL()
    {
        AssertUnsupported(
            "Moonshot file provider references without an ms:// URL",
            User(
                FileContentPart.FromProviderReference(
                    "image/png",
                    new Dictionary<string, string>
                    {
                        ["moonshotai"] = "https://example.com/image.png",
                    }
                )
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should throw for Moonshot provider references with unsupported media types",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_for_Moonshot_provider_references_with_unsupported_media_types()
    {
        AssertUnsupported(
            "file part media type application/pdf",
            User(
                FileContentPart.FromProviderReference(
                    "application/pdf",
                    new Dictionary<string, string> { ["moonshotai"] = "ms://document-file-123" }
                )
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should convert text file data to text parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_text_file_data_to_text_parts()
    {
        var result = ConvertMessages(User(FileContentPart.FromText("text/plain", "hello")));

        Equal(result, """[{"role":"user","content":[{"type":"text","text":"hello"}]}]""");
    }
}
