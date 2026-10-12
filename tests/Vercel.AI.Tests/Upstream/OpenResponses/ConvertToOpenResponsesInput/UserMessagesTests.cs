// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenResponses.ConvertToOpenResponsesInput.ConvertToOpenResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.OpenResponses.ConvertToOpenResponsesInput;

/// <summary>Port of <c>convert-to-open-responses-input.test.ts</c> &gt; <c>convertToOpenResponsesInput &gt; user messages</c>.</summary>
public sealed class UserMessagesTests
{
    private const string Prefix = "packages/open-responses/src/responses/convert-to-open-responses-input.test.ts::convertToOpenResponsesInput > user messages::";

    [Fact]
    [UpstreamTest(Prefix + "should convert messages with only a text part to a string content", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_messages_with_only_a_text_part_to_a_string_content()
    {
        var result = Convert(User(new TextContentPart("Hello")));

        JsonAssert.Equal(result.Input, """[{"content":[{"text":"Hello","type":"input_text"}],"role":"user","type":"message"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert image file parts with base64 data to input_image", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_image_file_parts_with_base64_data_to_input_image()
    {
        var result = Convert(User(File("image/png", "ZmFrZS1kYXRh")));

        JsonAssert.Equal(result.Input, """
            [{"content":[{"detail":"auto","image_url":"data:image/png;base64,ZmFrZS1kYXRh","type":"input_image"}],"role":"user","type":"message"}]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert image file parts with URL data to input_image", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_image_file_parts_with_URL_data_to_input_image()
    {
        var result = Convert(User(UrlFile("image/png", "https://example.com/image.png")));

        JsonAssert.Equal(result.Input, """
            [{"content":[{"detail":"auto","image_url":"https://example.com/image.png","type":"input_image"}],"role":"user","type":"message"}]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve image detail provider options", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_image_detail_provider_options()
    {
        var result = Convert(
            "test-provider",
            User(
                File("image/png", "ZmFrZS1kYXRh", providerOptions: """{"imageDetail":"low"}"""),
                UrlFile("image/png", "https://example.com/image.png", """{"imageDetail":"high"}""")));

        JsonAssert.Equal(result.Input, """
            [
              {
                "type":"message",
                "role":"user",
                "content":[
                  {"type":"input_image","image_url":"data:image/png;base64,ZmFrZS1kYXRh","detail":"low"},
                  {"type":"input_image","image_url":"https://example.com/image.png","detail":"high"}
                ]
              }
            ]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert PDF file parts with base64 data to input_file", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_PDF_file_parts_with_base64_data_to_input_file()
    {
        var result = Convert(User(new TextContentPart("What does this PDF say?"), File("application/pdf", "UERGREFUQQ==")));

        JsonAssert.Equal(result.Input, """
            [
              {
                "content":[
                  {"text":"What does this PDF say?","type":"input_text"},
                  {"file_data":"data:application/pdf;base64,UERGREFUQQ==","filename":"data","type":"input_file"}
                ],
                "role":"user",
                "type":"message"
              }
            ]
            """);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert PDF file parts with URL data to input_file", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_PDF_file_parts_with_URL_data_to_input_file()
    {
        var result = Convert(User(UrlFile("application/pdf", "https://example.com/document.pdf")));

        JsonAssert.Equal(result.Input, """
            [{"content":[{"file_url":"https://example.com/document.pdf","type":"input_file"}],"role":"user","type":"message"}]
            """);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use provided filename for non-image file parts", Coverage = UpstreamCoverage.Covered)]
    public void Should_use_provided_filename_for_non_image_file_parts()
    {
        var result = Convert(User(File("application/pdf", "UERGREFUQQ==", "report.pdf")));

        JsonAssert.Equal(result.Input, """
            [{"content":[{"file_data":"data:application/pdf;base64,UERGREFUQQ==","filename":"report.pdf","type":"input_file"}],"role":"user","type":"message"}]
            """);
    }
}
