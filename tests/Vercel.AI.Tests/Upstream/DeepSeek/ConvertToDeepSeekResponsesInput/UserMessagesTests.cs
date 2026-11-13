// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekResponsesInput.ConvertToDeepSeekResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekResponsesInput;

/// <summary>Port of <c>convert-to-deepseek-responses-input.test.ts</c> &gt; <c>convertToDeepSeekResponsesInput &gt; user messages</c>.</summary>
public sealed class UserMessagesTests
{
    private const string Prefix = TestFile + " > user messages::";

    private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };

    [Fact]
    [UpstreamTest(Prefix + "should convert a single text part", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_convert_a_single_text_part()
    {
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, User(new TextContentPart("Hello")));

        JsonAssert.Equal(input, """[{"content":[{"text":"Hello","type":"input_text"}],"role":"user"}]""");
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve multiple text parts including empty text", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_preserve_multiple_text_parts_including_empty_text()
    {
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, User(new TextContentPart("Hello "), new TextContentPart(""), new TextContentPart("world")));

        JsonAssert.Equal(input, """[{"content":[{"text":"Hello ","type":"input_text"},{"text":"","type":"input_text"},{"text":"world","type":"input_text"}],"role":"user"}]""");
        Assert.Empty(warnings);
    }

    [Theory]
    [InlineData("image/png")]
    [InlineData("image")]
    [InlineData("image/*")]
    [UpstreamTest(Prefix + "should preserve text and image URL order for %s", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_preserve_text_and_image_URL_order_for_mediaType(string mediaType)
    {
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, User(
            new TextContentPart("Describe this image."),
            ImageUrl(mediaType, "https://example.com/image.png"),
            new TextContentPart("Be brief.")));

        JsonAssert.Equal(input, """
            [{"content":[{"text":"Describe this image.","type":"input_text"},{"image_url":"https://example.com/image.png","type":"input_image"},{"text":"Be brief.","type":"input_text"}],"role":"user"}]
            """);
        Assert.Empty(warnings);
    }

    [Theory]
    [InlineData("base64")]
    [InlineData("Uint8Array")]
    [UpstreamTest(Prefix + "should convert an inline image from $name", Coverage = UpstreamCoverage.Partial, Commit = Commit,
        Note = "The .NET file part holds bytes only, so the base64 and Uint8Array rows send the same bytes.")]
    public void Should_convert_an_inline_image_from_name(string name)
    {
        var data = name == "base64" ? System.Convert.FromBase64String("AQID") : new byte[] { 1, 2, 3 };
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, User(ImageData("image/png", data)));

        JsonAssert.Equal(input, """[{"content":[{"image_url":"data:image/png;base64,AQID","type":"input_image"}],"role":"user"}]""");
        Assert.Empty(warnings);
    }

    [Theory]
    [InlineData("image/jpeg", "data:image/jpeg;base64,AQID")]
    [InlineData("image/jpg", "data:image/jpeg;base64,AQID")]
    [InlineData("image/png", "data:image/png;base64,AQID")]
    [InlineData("image/gif", "data:image/gif;base64,AQID")]
    [InlineData("image/webp", "data:image/webp;base64,AQID")]
    [UpstreamTest(Prefix + "should support %s inline images", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_support_mediaType_inline_images(string mediaType, string imageUrl)
    {
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, User(ImageData(mediaType, Bytes123)));

        JsonAssert.Equal(input, "[{\"role\":\"user\",\"content\":[{\"type\":\"input_image\",\"image_url\":\"" + imageUrl + "\"}]}]");
        Assert.Empty(warnings);
    }

    [Theory]
    [InlineData("image")]
    [InlineData("image/*")]
    [UpstreamTest(Prefix + "should detect the image subtype for %s inline data", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_detect_the_image_subtype_for_mediaType_inline_data(string mediaType)
    {
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, User(ImageData(mediaType, PngSignature)));

        JsonAssert.Equal(input, "[{\"role\":\"user\",\"content\":[{\"type\":\"input_image\",\"image_url\":\"data:image/png;base64,iVBORw0KGgo=\"}]}]");
        Assert.Empty(warnings);
    }

    [Theory]
    [InlineData("image")]
    [InlineData("image/*")]
    [UpstreamTest(Prefix + "should reject %s inline data whose subtype cannot be detected", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_reject_mediaType_inline_data_whose_subtype_cannot_be_detected(string mediaType)
    {
        Assert.Throws<UnsupportedFunctionalityException>(() => Convert(User(ImageData(mediaType, Bytes123))));
    }

    [Theory]
    [InlineData("image/svg+xml")]
    [InlineData("image/tiff")]
    [UpstreamTest(Prefix + "should reject unsupported inline image format %s", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_reject_unsupported_inline_image_format_mediaType(string mediaType)
    {
        var exception = Assert.Throws<UnsupportedFunctionalityException>(() => Convert(User(ImageData(mediaType, Bytes123))));

        Assert.Contains("DeepSeek supports JPEG, PNG, GIF, and WebP image inputs.", exception.Message);
    }

    [Theory]
    [InlineData("deepseek")]
    [InlineData("custom")]
    [UpstreamTest(Prefix + "should resolve DeepSeek image references with providerOptionsName %s", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_resolve_DeepSeek_image_references_with_providerOptionsName(string providerOptionsName)
    {
        var warnings = new List<CallWarning>();
        var reference = new Dictionary<string, string> { ["deepseek"] = "file-image", ["openai"] = "file-other" };

        var input = Convert(warnings, providerOptionsName, User(FileContentPart.FromProviderReference("image/png", reference)));

        JsonAssert.Equal(input, """[{"content":[{"file_id":"file-image","type":"input_image"}],"role":"user"}]""");
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject image references without a DeepSeek identifier", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_reject_image_references_without_a_DeepSeek_identifier()
    {
        var reference = new Dictionary<string, string> { ["openai"] = "file-other" };

        Assert.Throws<NoSuchProviderReferenceError>(() => Convert(User(FileContentPart.FromProviderReference("image/png", reference))));
    }

    [Theory]
    [InlineData("low")]
    [InlineData("high")]
    [InlineData("original")]
    [InlineData("auto")]
    [UpstreamTest(Prefix + "should forward imageDetail %s for URL and inline images", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_forward_imageDetail_for_URL_and_inline_images(string imageDetail)
    {
        var options = Opts(("deepseek", "{\"imageDetail\":\"" + imageDetail + "\"}"));
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, User(
            ImageUrl("image/png", "https://example.com/image.png", options),
            ImageData("image/png", Bytes123, options)));

        JsonAssert.Equal(input, "[{\"role\":\"user\",\"content\":[{\"type\":\"input_image\",\"image_url\":\"https://example.com/image.png\",\"detail\":\"" + imageDetail
            + "\"},{\"type\":\"input_image\",\"image_url\":\"data:image/png;base64,AQID\",\"detail\":\"" + imageDetail + "\"}]}]");
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should read image options from the configured provider namespace", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_read_image_options_from_the_configured_provider_namespace()
    {
        var options = Opts(("custom", """{"imageDetail":"low"}"""), ("deepseek", """{"imageDetail":"high"}"""));
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, "custom", User(ImageData("image/png", Bytes123, options)));

        JsonAssert.Equal(input, """[{"content":[{"detail":"low","image_url":"data:image/png;base64,AQID","type":"input_image"}],"role":"user"}]""");
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject invalid image provider options", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_reject_invalid_image_provider_options()
    {
        var options = Opts(("deepseek", """{"imageDetail":"invalid"}"""));

        Assert.Throws<InvalidArgumentError>(() => Convert(User(ImageData("image/png", Bytes123, options))));
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject fileData image parts", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_reject_fileData_image_parts()
    {
        var options = Opts(("deepseek", """{"fileData":true}"""));

        var exception = Assert.Throws<UnsupportedFunctionalityException>(() => Convert(User(ImageData("image/png", Bytes123, options))));

        Assert.Contains("DeepSeek `fileData` image parts with the Responses API", exception.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should accept image URLs at the 8192 character limit", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_accept_image_URLs_at_the_8192_character_limit()
    {
        var url = "https://example.com/" + new string('a', 8192 - 20);
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, User(ImageUrl("image/png", url)));

        Assert.Equal(8192, url.Length);
        JsonAssert.Equal(input, "[{\"role\":\"user\",\"content\":[{\"type\":\"input_image\",\"image_url\":\"" + url + "\"}]}]");
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject image URLs longer than 8192 characters", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_reject_image_URLs_longer_than_8192_characters()
    {
        var url = "https://example.com/" + new string('a', 8193 - 20);

        var exception = Assert.Throws<InvalidPromptException>(() => Convert(User(ImageUrl("image/png", url))));

        Assert.Equal("DeepSeek image URLs must not exceed 8192 characters.", exception.Message);
    }

    [Theory]
    [InlineData("application/pdf", "url")]
    [InlineData("audio/wav", "data")]
    [InlineData("text/plain", "text")]
    [InlineData("image/png", "text")]
    [UpstreamTest(Prefix + "should warn and skip unsupported $mediaType $data.type file parts", Coverage = UpstreamCoverage.Partial, Commit = Commit,
        Note = "The .NET file part has no text data type. The text rows use a file part without data, which the port also skips with the same warning.")]
    public void Should_warn_and_skip_unsupported_mediaType_data_type_file_parts(string mediaType, string dataType)
    {
        var file = dataType switch
        {
            "url" => ImageUrl(mediaType, "https://example.com/document.pdf"),
            "data" => ImageData(mediaType, Bytes123),
            _ => new FileContentPart(mediaType, null, null, null),
        };
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, User(new TextContentPart("Before"), file, new TextContentPart("After")));

        JsonAssert.Equal(input, """[{"content":[{"text":"Before","type":"input_text"},{"text":"After","type":"input_text"}],"role":"user"}]""");
        var warning = Assert.Single(warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("user message part type: file", warning.Feature);
    }
}
