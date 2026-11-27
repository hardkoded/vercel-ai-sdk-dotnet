// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.Anthropic;
using static Vercel.AI.Tests.ConvertToAnthropicPrompt.PromptRunner;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>user messages</c>.</summary>
public sealed class UserMessagesTests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages::";

    private static AnthropicPromptResult RunFile(string filePart) =>
        Run("[{\"role\":\"user\",\"content\":[" + filePart + "]}]", sendReasoning: true);

    private static string File(string data, string mediaType, string extra = "") =>
        "{\"type\":\"file\",\"data\":"
        + data
        + ",\"mediaType\":\""
        + mediaType
        + "\""
        + extra
        + "}";

    private static string Data(string data) => "{\"type\":\"data\",\"data\":\"" + data + "\"}";

    private static string Url(string url) => "{\"type\":\"url\",\"url\":\"" + url + "\"}";

    private static string Reference(string provider, string id) =>
        "{\"type\":\"reference\",\"reference\":{\"" + provider + "\":\"" + id + "\"}}";

    private static void AssertResult(
        string expectedContent,
        AnthropicPromptResult result,
        params string[] betas
    )
    {
        AssertJson("[{\"role\":\"user\",\"content\":" + expectedContent + "}]", result.Messages);
        Assert.Null(result.System);
        Assert.Equal(betas.OrderBy(b => b), result.Betas.OrderBy(b => b));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should add image parts for UInt8Array images",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_add_image_parts_for_UInt8Array_images()
    {
        var result = RunFile(File(Data("AAECAw=="), "image/png"));

        AssertResult(
            "[{\"type\":\"image\",\"source\":{\"data\":\"AAECAw==\",\"media_type\":\"image/png\",\"type\":\"base64\"}}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should treat URL strings in image file data as URLs, not base64)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_treat_URL_strings_in_image_file_data_as_URLs_not_base64()
    {
        var result = RunFile(File(Url("https://example.com/image.png"), "image/png"));

        AssertResult(
            "[{\"type\":\"image\",\"source\":{\"type\":\"url\",\"url\":\"https://example.com/image.png\"}}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should treat URL strings in PDF file data as URLs, not base64)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_treat_URL_strings_in_PDF_file_data_as_URLs_not_base64()
    {
        var result = RunFile(File(Url("https://example.com/document.pdf"), "application/pdf"));

        AssertResult(
            "[{\"type\":\"document\",\"source\":{\"type\":\"url\",\"url\":\"https://example.com/document.pdf\"}}]",
            result,
            "pdfs-2024-09-25"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should add PDF file parts for base64 PDFs",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_add_PDF_file_parts_for_base64_PDFs()
    {
        var result = RunFile(File(Data("base64PDFdata"), "application/pdf"));

        AssertResult(
            "[{\"type\":\"document\",\"source\":{\"type\":\"base64\",\"media_type\":\"application/pdf\",\"data\":\"base64PDFdata\"}}]",
            result,
            "pdfs-2024-09-25"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should add PDF file parts for URL PDFs",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_add_PDF_file_parts_for_URL_PDFs()
    {
        var result = RunFile(File(Url("https://example.com/document.pdf"), "application/pdf"));

        AssertResult(
            "[{\"type\":\"document\",\"source\":{\"type\":\"url\",\"url\":\"https://example.com/document.pdf\"}}]",
            result,
            "pdfs-2024-09-25"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should add text file parts for text/plain documents",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_add_text_file_parts_for_text_plain_documents()
    {
        var data = System.Convert.ToBase64String(Encoding.UTF8.GetBytes("sample text content"));

        var result = RunFile(File(Data(data), "text/plain", ",\"filename\":\"sample.txt\""));

        AssertResult(
            "[{\"type\":\"document\",\"source\":{\"type\":\"text\",\"media_type\":\"text/plain\",\"data\":\"sample text content\"},\"title\":\"sample.txt\"}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map inline text file parts to inline text document source",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_map_inline_text_file_parts_to_inline_text_document_source()
    {
        var result = RunFile(
            File(
                "{\"type\":\"text\",\"text\":\"inline text content\"}",
                "text/plain",
                ",\"filename\":\"inline.txt\""
            )
        );

        AssertResult(
            "[{\"type\":\"document\",\"source\":{\"type\":\"text\",\"media_type\":\"text/plain\",\"data\":\"inline text content\"},\"title\":\"inline.txt\"}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw error for unsupported file types",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_error_for_unsupported_file_types()
    {
        var error = Assert.ThrowsAny<Exception>(() =>
            RunFile(File(Data("base64data"), "video/mp4"))
        );

        Assert.Contains("media type: video/mp4", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert messages with image file parts using provider reference",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_messages_with_image_file_parts_using_provider_reference()
    {
        var result = RunFile(File(Reference("anthropic", "file-img-12345"), "image/png"));

        AssertResult(
            "[{\"type\":\"image\",\"source\":{\"type\":\"file\",\"file_id\":\"file-img-12345\"}}]",
            result,
            "files-api-2025-04-14"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert messages with PDF file parts using provider reference",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_messages_with_PDF_file_parts_using_provider_reference()
    {
        var result = RunFile(File(Reference("anthropic", "file-pdf-12345"), "application/pdf"));

        AssertResult(
            "[{\"type\":\"document\",\"source\":{\"type\":\"file\",\"file_id\":\"file-pdf-12345\"}}]",
            result,
            "files-api-2025-04-14"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert messages with text/plain file parts using provider reference",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_messages_with_text_plain_file_parts_using_provider_reference()
    {
        var result = RunFile(File(Reference("anthropic", "file-txt-12345"), "text/plain"));

        AssertResult(
            "[{\"type\":\"document\",\"source\":{\"type\":\"file\",\"file_id\":\"file-txt-12345\"}}]",
            result,
            "files-api-2025-04-14"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should convert provider referenced file parts to container uploads when requested",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_provider_referenced_file_parts_to_container_uploads_when_requested()
    {
        var result = Run(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Analyze this data.\"},"
                + File(
                    Reference("anthropic", "file-csv-12345"),
                    "text/csv",
                    ",\"providerOptions\":{\"anthropic\":{\"containerUpload\":true}}"
                )
                + "]}]"
        );

        AssertResult(
            "[{\"type\":\"text\",\"text\":\"Analyze this data.\"},{\"type\":\"container_upload\",\"file_id\":\"file-csv-12345\"}]",
            result,
            "files-api-2025-04-14"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw when provider reference does not contain anthropic key",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_when_provider_reference_does_not_contain_anthropic_key()
    {
        var error = Assert.ThrowsAny<Exception>(() =>
            RunFile(File(Reference("openai", "file-xyz"), "application/pdf"))
        );

        Assert.Contains(
            "No provider reference found for provider 'anthropic'. Available providers: openai",
            error.Message
        );
    }
}
