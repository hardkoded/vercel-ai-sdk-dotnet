// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.ConvertToXaiResponsesInput.ConvertToXaiResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.Xai.ConvertToXaiResponsesInput;

/// <summary>Port of <c>convert-to-xai-responses-input.test.ts</c> &gt; <c>convertToXaiResponsesInput &gt; user messages</c>.</summary>
public sealed class UserMessagesTests
{
    private const string Prefix = TestFile + " > user messages::";

    [Fact]
    [UpstreamTest(Prefix + "should convert single text part", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_single_text_part()
    {
        var input = Convert(User(new TextContentPart("hello")));

        JsonAssert.Equal(input, """[{"content":[{"text":"hello","type":"input_text"}],"role":"user"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should concatenate multiple text parts", Coverage = UpstreamCoverage.Covered)]
    public void Should_concatenate_multiple_text_parts()
    {
        var input = Convert(User(new TextContentPart("hello "), new TextContentPart("world")));

        JsonAssert.Equal(input, """[{"content":[{"text":"hello ","type":"input_text"},{"text":"world","type":"input_text"}],"role":"user"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert image file parts with URL", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_image_file_parts_with_URL()
    {
        var warnings = new List<CallWarning>();

        var input = Convert(
            warnings,
            User(new TextContentPart("what is in this image"), new FileContentPart("image/jpeg", "https://example.com/image.jpg", null, null)));

        JsonAssert.Equal(input, """
            [{"content":[{"text":"what is in this image","type":"input_text"},{"image_url":"https://example.com/image.jpg","type":"input_image"}],"role":"user"}]
            """);
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert image file parts with base64 data", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_image_file_parts_with_base64_data()
    {
        var input = Convert(User(new TextContentPart("describe this"), new FileContentPart("image/png", null, new byte[] { 1, 2, 3 }, null)));

        JsonAssert.Equal(input, """
            [{"content":[{"text":"describe this","type":"input_text"},{"image_url":"data:image/png;base64,AQID","type":"input_image"}],"role":"user"}]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert non-image file parts with URL to input_file with file_url", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_non_image_file_parts_with_URL_to_input_file_with_file_url()
    {
        var warnings = new List<CallWarning>();

        var input = Convert(
            warnings,
            User(new TextContentPart("summarize this PDF"), new FileContentPart("application/pdf", "https://example.com/document.pdf", null, null)));

        JsonAssert.Equal(input, """
            [{"content":[{"text":"summarize this PDF","type":"input_text"},{"file_url":"https://example.com/document.pdf","type":"input_file"}],"role":"user"}]
            """);
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert text/* file parts with URL to input_file with file_url", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_text_file_parts_with_URL_to_input_file_with_file_url()
    {
        var input = Convert(User(new TextContentPart("analyze this CSV"), new FileContentPart("text/csv", "https://example.com/data.csv", null, null)));

        JsonAssert.Equal(input, """
            [{"content":[{"text":"analyze this CSV","type":"input_text"},{"file_url":"https://example.com/data.csv","type":"input_file"}],"role":"user"}]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw for non-image file parts provided as inline bytes", Coverage = UpstreamCoverage.Covered)]
    public void Should_throw_for_non_image_file_parts_provided_as_inline_bytes()
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(() => Convert(
            User(new TextContentPart("check this file"), new FileContentPart("application/pdf", null, new byte[] { 1, 2, 3 }, null))));

        Assert.Contains("file part media type application/pdf", error.Message, StringComparison.Ordinal);
    }
}
