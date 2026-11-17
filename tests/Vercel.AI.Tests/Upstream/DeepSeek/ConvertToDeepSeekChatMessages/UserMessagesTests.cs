// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekChatMessages.ConvertToDeepSeekChatMessagesSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekChatMessages;

/// <summary>Port of <c>convert-to-deepseek-chat-messages.test.ts</c> &gt; <c>convertToDeepSeekChatMessages &gt; user messages</c>.</summary>
public sealed class UserMessagesTests
{
    private const string Prefix = TestFile + " > user messages::";

    [Fact]
    [UpstreamTest(
        Prefix + "should convert messages with only a text part to a string content",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public void Should_convert_messages_with_only_a_text_part_to_a_string_content()
    {
        var result = Run("deepseek-chat", [User(new TextContentPart("Hello"))]);

        JsonAssert.Equal(result.Messages, """[{"content":"Hello","role":"user"}]""");
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert image data to an image URL content part",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public void Should_convert_image_data_to_an_image_URL_content_part()
    {
        var result = Run(
            "deepseek-chat",
            [User(new TextContentPart("Hello"), ImageData("image/png", Bytes0123))]
        );

        JsonAssert.Equal(
            result.Messages,
            """
            [{"content":[
               {"text":"Hello","type":"text"},
               {"image_url":{"url":"data:image/png;base64,AAECAw=="},"type":"image_url"}],
              "role":"user"}]
            """
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert an image URL to an image URL content part",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public void Should_convert_an_image_URL_to_an_image_URL_content_part()
    {
        var result = Run(
            "deepseek-chat",
            [
                User(
                    new TextContentPart("Hello"),
                    ImageUrl("image/png", "https://example.com/image.png")
                ),
            ]
        );

        JsonAssert.Equal(
            result.Messages,
            """
            [{"content":[
               {"text":"Hello","type":"text"},
               {"image_url":{"url":"https://example.com/image.png"},"type":"image_url"}],
              "role":"user"}]
            """
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass imageDetail to image URL content parts",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public void Should_pass_imageDetail_to_image_URL_content_parts()
    {
        var result = Run(
            "deepseek-v4-flash-vision-exp",
            [
                User(
                    ImageUrl(
                        "image/webp",
                        "https://example.com/image.webp",
                        Opts(("deepseek", """{"imageDetail":"low"}"""))
                    )
                ),
            ]
        );

        JsonAssert.Equal(
            result.Messages,
            """
            [{"role":"user","content":[
               {"type":"image_url","image_url":{"url":"https://example.com/image.webp","detail":"low"}}]}]
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert inline image data to file_data and preserve its filename",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public void Should_convert_inline_image_data_to_file_data_and_preserve_its_filename()
    {
        var result = Run(
            "deepseek-v4-flash-vision-exp",
            [
                User(
                    ImageData(
                        "image/jpg",
                        Bytes0123,
                        "sample.jpg",
                        Opts(("deepseek", """{"fileData":true}"""))
                    )
                ),
            ]
        );

        JsonAssert.Equal(
            result.Messages,
            """
            [{"role":"user","content":[
               {"type":"file","file_data":"data:image/jpeg;base64,AAECAw==","filename":"sample.jpg"}]}]
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject imageDetail together with fileData",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public void Should_reject_imageDetail_together_with_fileData()
    {
        var error = Assert.Throws<InvalidPromptException>(() =>
            Run(
                "deepseek-v4-flash-vision-exp",
                [
                    User(
                        ImageData(
                            "image/png",
                            Bytes0123,
                            null,
                            Opts(("deepseek", """{"fileData":true,"imageDetail":"high"}"""))
                        )
                    ),
                ]
            )
        );

        Assert.Contains(
            "DeepSeek `imageDetail` cannot be combined with `fileData`.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject image URLs longer than 8192 characters",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public void Should_reject_image_URLs_longer_than_8192_characters()
    {
        var error = Assert.Throws<InvalidPromptException>(() =>
            Run(
                "deepseek-v4-flash-vision-exp",
                [User(ImageUrl("image/png", "https://example.com/" + new string('a', 8192)))]
            )
        );

        Assert.Contains("DeepSeek image URLs must not exceed 8192 characters.", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject unsupported image formats",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public void Should_reject_unsupported_image_formats()
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(() =>
            Run("deepseek-v4-flash-vision-exp", [User(ImageData("image/svg+xml", Bytes0123))])
        );

        Assert.Contains("DeepSeek supports JPEG, PNG, GIF, and WebP image inputs.", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert an image provider reference to a file content part",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public void Should_convert_an_image_provider_reference_to_a_file_content_part()
    {
        var reference = new Dictionary<string, string>
        {
            ["deepseek"] = "file-api-deepseek",
            ["openai"] = "file-openai",
        };

        var result = Run(
            "deepseek-v4-flash-vision-exp",
            [
                User(
                    new TextContentPart("Hello"),
                    FileContentPart.FromProviderReference("image/png", reference)
                ),
            ]
        );

        JsonAssert.Equal(
            result.Messages,
            """
            [{"content":[
               {"text":"Hello","type":"text"},
               {"file_id":"file-api-deepseek","type":"file"}],
              "role":"user"}]
            """
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw when an image reference has no DeepSeek identifier",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public void Should_throw_when_an_image_reference_has_no_DeepSeek_identifier()
    {
        var reference = new Dictionary<string, string> { ["openai"] = "file-openai" };

        Assert.Throws<NoSuchProviderReferenceError>(() =>
            Run(
                "deepseek-v4-flash-vision-exp",
                [User(FileContentPart.FromProviderReference("image/png", reference))]
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn about unsupported non-image file parts",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public void Should_warn_about_unsupported_non_image_file_parts()
    {
        var result = Run(
            "deepseek-chat",
            [User(new TextContentPart("Hello"), ImageData("application/pdf", Bytes0123))]
        );

        JsonAssert.Equal(result.Messages, """[{"content":"Hello","role":"user"}]""");
        Assert.Equal(
            """[{"type":"unsupported","feature":"user message part type: file"}]""",
            WarningsJson(result)
        );
    }
}
