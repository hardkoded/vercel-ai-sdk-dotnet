// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.ConvertToMoonshotAIChatMessages.Helpers;

namespace Vercel.AI.Tests.ConvertToMoonshotAIChatMessages;

/// <summary>Port of <c>convert-to-moonshotai-chat-messages.test.ts</c> &gt; <c>user messages</c>.</summary>
public sealed class UserMessagesTests
{
    private const string Prefix = Source + "user messages::";

    private static readonly byte[] Bytes = [0, 1, 2, 3];

    public static TheoryData<string> SupportedImageMediaTypes =>
        [
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp",
            "image/bmp",
            "image/heic",
            "image/heif",
        ];

    public static TheoryData<string> SupportedVideoMediaTypes =>
        [
            "video/mp4",
            "video/mpeg",
            "video/mov",
            "video/avi",
            "video/x-flv",
            "video/mpg",
            "video/webm",
            "video/wmv",
            "video/3gpp",
        ];

    private static UserModelMessage User(params UserContentPart[] parts)
    {
        return new UserModelMessage(parts);
    }

    private static UserModelMessage Data(string mediaType)
    {
        return User(new FileContentPart(mediaType, null, Bytes, null));
    }

    private static void AssertUnsupported(string functionality, Action convert)
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(convert);
        Assert.Contains("'" + functionality + "' functionality not supported", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert a text-only user message to string content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_a_text_only_user_message_to_string_content()
    {
        AssertJson(
            "[{\"role\":\"user\",\"content\":\"Hello\"}]",
            ConvertMessages(User(new TextContentPart("Hello")))
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert image data parts to image_url data URIs",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_image_data_parts_to_image_url_data_URIs()
    {
        var result = ConvertMessages(
            User(new TextContentPart("Hi"), new FileContentPart("image/png", null, Bytes, null))
        );

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Hi\"},"
                + "{\"type\":\"image_url\",\"image_url\":{\"url\":\"data:image/png;base64,AAECAw==\"}}]}]",
            result
        );
    }

    [Theory]
    [MemberData(nameof(SupportedImageMediaTypes))]
    [UpstreamTest(
        Prefix + "should accept supported image media type %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_accept_supported_image_media_type(string mediaType)
    {
        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"image_url\",\"image_url\":{\"url\":\"data:"
                + mediaType
                + ";base64,AAECAw==\"}}]}]",
            ConvertMessages(Data(mediaType))
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass through image URLs", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_through_image_URLs()
    {
        var result = ConvertMessages(
            User(new FileContentPart("image/*", "https://example.com/image.jpg", null, null))
        );

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"image_url\",\"image_url\":{\"url\":\"https://example.com/image.jpg\"}}]}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert video data parts to video_url data URIs",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_video_data_parts_to_video_url_data_URIs()
    {
        var result = ConvertMessages(
            User(
                new TextContentPart("Describe this video"),
                new FileContentPart("video/mp4", null, Bytes, null)
            )
        );

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Describe this video\"},"
                + "{\"type\":\"video_url\",\"video_url\":{\"url\":\"data:video/mp4;base64,AAECAw==\"}}]}]",
            result
        );
    }

    [Theory]
    [MemberData(nameof(SupportedVideoMediaTypes))]
    [UpstreamTest(
        Prefix + "should accept supported video media type %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_accept_supported_video_media_type(string mediaType)
    {
        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"video_url\",\"video_url\":{\"url\":\"data:"
                + mediaType
                + ";base64,AAECAw==\"}}]}]",
            ConvertMessages(Data(mediaType))
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass through video URLs", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_through_video_URLs()
    {
        var result = ConvertMessages(
            User(new FileContentPart("video/*", "https://example.com/video.mp4", null, null))
        );

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"video_url\",\"video_url\":{\"url\":\"https://example.com/video.mp4\"}}]}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass through ms:// file references from the Files API",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_ms_file_references_from_the_Files_API()
    {
        var result = ConvertMessages(
            User(new FileContentPart("video/mp4", "ms://file-abc123", null, null))
        );

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"video_url\",\"video_url\":{\"url\":\"ms://file-abc123\"}}]}]",
            result
        );
    }

    [Theory]
    [InlineData("image", "image_url")]
    [InlineData("video", "video_url")]
    [UpstreamTest(
        Prefix + "should pass through ms:// file references with top-level $mediaType media types",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_ms_file_references_with_top_level_media_types(
        string mediaType,
        string partType
    )
    {
        var result = ConvertMessages(
            User(new FileContentPart(mediaType, "ms://file-" + mediaType, null, null))
        );

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\""
                + partType
                + "\",\""
                + partType
                + "\":{\"url\":\"ms://file-"
                + mediaType
                + "\"}}]}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should decode text/* file parts into text parts",
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

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"hello markdown\"}]}]",
            result
        );
    }

    [Theory]
    [InlineData("image/svg+xml")]
    [InlineData("image/tiff")]
    [InlineData("video/quicktime")]
    [UpstreamTest(
        Prefix + "should throw for unsupported multimodal media type %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_for_unsupported_multimodal_media_type(string mediaType)
    {
        AssertUnsupported("file part media type " + mediaType, () =>
            ConvertMessages(Data(mediaType))
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw for audio file parts (rejected by the API)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_for_audio_file_parts_rejected_by_the_API()
    {
        AssertUnsupported("file part media type audio/mp3", () =>
            ConvertMessages(Data("audio/mp3"))
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw for PDF file parts (rejected by the API)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_for_PDF_file_parts_rejected_by_the_API()
    {
        AssertUnsupported("file part media type application/pdf", () =>
            ConvertMessages(Data("application/pdf"))
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw for unsupported file types",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_for_unsupported_file_types()
    {
        AssertUnsupported("file part media type application/zip", () =>
            ConvertMessages(Data("application/zip"))
        );
    }

    [Theory]
    [InlineData("image/png", "ms://image-file-123", "image_url")]
    [InlineData("video/mp4", "ms://video-file-123", "video_url")]
    [UpstreamTest(
        Prefix + "should convert $mediaType Moonshot provider references",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_media_type_Moonshot_provider_references(
        string mediaType,
        string reference,
        string partType
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

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\""
                + partType
                + "\",\""
                + partType
                + "\":{\"url\":\""
                + reference
                + "\"}}]}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw for foreign provider references",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_for_foreign_provider_references()
    {
        var error = Assert.Throws<NoSuchProviderReferenceError>(() =>
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
        Prefix + "should throw for Moonshot provider references without an ms:// URL",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_for_Moonshot_provider_references_without_an_ms_URL()
    {
        AssertUnsupported("Moonshot file provider references without an ms:// URL", () =>
            ConvertMessages(
                User(
                    FileContentPart.FromProviderReference(
                        "image/png",
                        new Dictionary<string, string>
                        {
                            ["moonshotai"] = "https://example.com/image.png",
                        }
                    )
                )
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw for Moonshot provider references with unsupported media types",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_for_Moonshot_provider_references_with_unsupported_media_types()
    {
        AssertUnsupported("file part media type application/pdf", () =>
            ConvertMessages(
                User(
                    FileContentPart.FromProviderReference(
                        "application/pdf",
                        new Dictionary<string, string> { ["moonshotai"] = "ms://document-file-123" }
                    )
                )
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert text file data to text parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_text_file_data_to_text_parts()
    {
        var result = ConvertMessages(User(FileContentPart.FromText("text/plain", "hello")));

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"hello\"}]}]",
            result
        );
    }
}
