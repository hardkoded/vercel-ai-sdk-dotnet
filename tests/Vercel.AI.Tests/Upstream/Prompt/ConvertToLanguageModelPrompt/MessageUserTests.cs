// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt;

/// <summary>Port of <c>convert-to-language-model-prompt.test.ts</c> &gt; <c>convertToLanguageModelMessage &gt; user message</c> (text, image and file parts).</summary>
public sealed class MessageUserTests
{
    private const string Id =
        "packages/ai/src/prompt/convert-to-language-model-prompt.test.ts::convertToLanguageModelMessage > user message > ";

    private static readonly IReadOnlyDictionary<string, DownloadResult> NoAssets =
        new Dictionary<string, DownloadResult>();

    private static IReadOnlyList<UserContentPart> Convert(params UserContentPart[] content) =>
        Assert
            .IsType<UserModelMessage>(
                LanguageModelPrompts.ConvertToLanguageModelMessage(
                    new UserModelMessage(content),
                    NoAssets
                )
            )
            .Content;

    [Fact]
    [UpstreamTest(
        Id + "text parts::should filter out empty text parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_filter_out_empty_text_parts()
    {
        Assert.Empty(Convert(new TextContentPart(string.Empty)));
    }

    [Fact]
    [UpstreamTest(
        Id + "text parts::should pass through non-empty text parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_non_empty_text_parts()
    {
        var text = Assert.IsType<TextContentPart>(
            Assert.Single(Convert(new TextContentPart("hello, world!")))
        );
        Assert.Equal("hello, world!", text.Text);
    }

    [Fact]
    [UpstreamTest(
        Id + "image parts::should convert image string https url to URL object",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_image_string_https_url_to_URL_object()
    {
        var file = Assert.IsType<FileContentPart>(
            Assert.Single(
                Convert(new FileContentPart("image", "https://example.com/image.jpg", null, null))
            )
        );
        Assert.Equal("https://example.com/image.jpg", file.Url);
        Assert.Equal("image", file.MediaType);
        Assert.Null(file.Data);
    }

    [Fact]
    [UpstreamTest(
        Id + "image parts::should convert image string data url to base64 content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_image_string_data_url_to_base64_content()
    {
        var file = Assert.IsType<FileContentPart>(
            Assert.Single(
                Convert(new FileContentPart("image", "data:image/jpg;base64,/9j/3Q==", null, null))
            )
        );
        Assert.Equal(System.Convert.FromBase64String("/9j/3Q=="), file.Data);
        Assert.Equal("image/jpeg", file.MediaType);
        Assert.Null(file.Url);
    }

    [Fact]
    [UpstreamTest(
        Id + "image parts::should prefer detected mediaType",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_prefer_detected_mediaType()
    {
        // incorrect mediaType in the data URL:
        var file = Assert.IsType<FileContentPart>(
            Assert.Single(
                Convert(new FileContentPart("image", "data:image/png;base64,/9j/3Q==", null, null))
            )
        );
        Assert.Equal(System.Convert.FromBase64String("/9j/3Q=="), file.Data);
        Assert.Equal("image/jpeg", file.MediaType);
    }

    [Fact]
    [UpstreamTest(
        Id + "file parts::should convert file string https url to URL object",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_file_string_https_url_to_URL_object()
    {
        var file = Assert.IsType<FileContentPart>(
            Assert.Single(
                Convert(
                    new FileContentPart("image/jpg", "https://example.com/image.jpg", null, null)
                )
            )
        );
        Assert.Equal("https://example.com/image.jpg", file.Url);
        Assert.Equal("image/jpg", file.MediaType);
    }

    [Fact]
    [UpstreamTest(
        Id + "file parts::should convert file string data url to base64 content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_file_string_data_url_to_base64_content()
    {
        var file = Assert.IsType<FileContentPart>(
            Assert.Single(
                Convert(
                    new FileContentPart("image/jpg", "data:image/jpg;base64,dGVzdA==", null, null)
                )
            )
        );
        Assert.Equal(System.Convert.FromBase64String("dGVzdA=="), file.Data);
        Assert.Equal("image/jpg", file.MediaType);
    }
}
