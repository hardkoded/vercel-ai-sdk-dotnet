// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.ConvertToOpenResponsesInputTests.ConvertToOpenResponsesInputHelpers;

namespace Vercel.AI.Tests.ConvertToOpenResponsesInputTests;

/// <summary>Port of <c>convert-to-open-responses-input.test.ts</c> &gt; <c>convertToOpenResponsesInput &gt; user messages</c>.</summary>
public sealed class UserMessagesTests
{
    private const string Prefix = "packages/open-responses/src/responses/convert-to-open-responses-input.test.ts::convertToOpenResponsesInput > user messages::";

    [Fact]
    [UpstreamTest(Prefix + "should convert messages with only a text part to a string content", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_messages_with_only_a_text_part_to_a_string_content()
    {
        var result = Convert(User(new TextContentPart("Hello")));

        AssertInput("[{\"content\":[{\"text\":\"Hello\",\"type\":\"input_text\"}],\"role\":\"user\",\"type\":\"message\"}]", result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert image file parts with base64 data to input_image", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_image_file_parts_with_base64_data_to_input_image()
    {
        var result = Convert(User(new FileContentPart("image/png", null, System.Convert.FromBase64String("ZmFrZS1kYXRh"), null)));

        AssertInput(
            "[{\"content\":[{\"detail\":\"auto\",\"image_url\":\"data:image/png;base64,ZmFrZS1kYXRh\",\"type\":\"input_image\"}],\"role\":\"user\",\"type\":\"message\"}]",
            result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert image file parts with URL data to input_image", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_image_file_parts_with_URL_data_to_input_image()
    {
        var result = Convert(User(new FileContentPart("image/png", "https://example.com/image.png", null, null)));

        AssertInput(
            "[{\"content\":[{\"detail\":\"auto\",\"image_url\":\"https://example.com/image.png\",\"type\":\"input_image\"}],\"role\":\"user\",\"type\":\"message\"}]",
            result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve image detail provider options", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_image_detail_provider_options()
    {
        var result = Convert(
            "test-provider",
            false,
            User(
                new FileContentPart("image/png", null, System.Convert.FromBase64String("ZmFrZS1kYXRh"), null)
                {
                    ProviderOptions = UpstreamChat.Bag("test-provider", "{\"imageDetail\":\"low\"}"),
                },
                new FileContentPart("image/png", "https://example.com/image.png", null, null)
                {
                    ProviderOptions = UpstreamChat.Bag("test-provider", "{\"imageDetail\":\"high\"}"),
                }));

        AssertInput(
            "[{\"type\":\"message\",\"role\":\"user\",\"content\":["
            + "{\"type\":\"input_image\",\"image_url\":\"data:image/png;base64,ZmFrZS1kYXRh\",\"detail\":\"low\"},"
            + "{\"type\":\"input_image\",\"image_url\":\"https://example.com/image.png\",\"detail\":\"high\"}]}]",
            result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert PDF file parts with base64 data to input_file", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_PDF_file_parts_with_base64_data_to_input_file()
    {
        var result = Convert(User(
            new TextContentPart("What does this PDF say?"),
            new FileContentPart("application/pdf", null, System.Convert.FromBase64String("UERGREFUQQ=="), null)));

        AssertInput(
            "[{\"content\":[{\"text\":\"What does this PDF say?\",\"type\":\"input_text\"},"
            + "{\"file_data\":\"data:application/pdf;base64,UERGREFUQQ==\",\"filename\":\"data\",\"type\":\"input_file\"}],\"role\":\"user\",\"type\":\"message\"}]",
            result);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert PDF file parts with URL data to input_file", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_PDF_file_parts_with_URL_data_to_input_file()
    {
        var result = Convert(User(new FileContentPart("application/pdf", "https://example.com/document.pdf", null, null)));

        AssertInput(
            "[{\"content\":[{\"file_url\":\"https://example.com/document.pdf\",\"type\":\"input_file\"}],\"role\":\"user\",\"type\":\"message\"}]",
            result);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use provided filename for non-image file parts", Coverage = UpstreamCoverage.Covered)]
    public void Should_use_provided_filename_for_non_image_file_parts()
    {
        var result = Convert(User(new FileContentPart("application/pdf", null, System.Convert.FromBase64String("UERGREFUQQ=="), "report.pdf")));

        AssertInput(
            "[{\"content\":[{\"file_data\":\"data:application/pdf;base64,UERGREFUQQ==\",\"filename\":\"report.pdf\",\"type\":\"input_file\"}],\"role\":\"user\",\"type\":\"message\"}]",
            result);
    }
}
