// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt.PromptTestSupport;

namespace Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt;

/// <summary>Port of <c>convert-to-language-model-prompt.test.ts</c> &gt; <c>convertToLanguageModelPrompt &gt; user message &gt; file parts</c>.</summary>
public sealed class UserMessageFilePartsTests
{
    private const string Id =
        "packages/ai/src/prompt/convert-to-language-model-prompt.test.ts::convertToLanguageModelPrompt > user message > file parts::";

    private const string PdfUrl = "https://example.com/document.pdf";

    private static readonly byte[] Bytes = [0, 1, 2, 3];

    private static UserModelMessage[] PdfMessage(string? fileName = null) =>
        [new UserModelMessage([new FileContentPart("application/pdf", PdfUrl, null, fileName)])];

    private static async Task AssertPdfIsDownloadedAsync(
        IReadOnlyDictionary<string, string> supportedUrls
    )
    {
        var result = await ConvertAsync(
            PdfMessage(),
            supportedUrls,
            DownloadOf(PdfUrl, Bytes, "application/pdf")
        );

        var file = OnlyFile(result);
        Assert.Equal("application/pdf", file.MediaType);
        Assert.Equal(Bytes, file.Data);
        Assert.Null(file.Url);
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass through URLs when the model supports a particular URL",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_through_URLs_when_the_model_supports_a_particular_URL()
    {
        var result = await ConvertAsync(
            PdfMessage(),
            new Dictionary<string, string> { ["*"] = "^https://.*$" },
            requests =>
            {
                Assert.True(Assert.Single(requests).IsUrlSupportedByModel);
                return Task.FromResult<IReadOnlyList<DownloadResult>>([null!]);
            }
        );

        var file = OnlyFile(result);
        Assert.Equal(PdfUrl, file.Url);
        Assert.Equal("application/pdf", file.MediaType);
        Assert.Null(file.Data);
    }

    [Fact]
    [UpstreamTest(
        Id + "should download the URL as an asset when the model does not support a URL",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_download_the_URL_as_an_asset_when_the_model_does_not_support_a_URL() =>
        AssertPdfIsDownloadedAsync(
            // PDF is not supported, but image/* is
            new Dictionary<string, string> { ["image/*"] = "^https://.*$" }
        );

    [Fact]
    [UpstreamTest(
        Id + "should handle file parts with base64 string data",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_file_parts_with_base64_string_data()
    {
        var result = await ConvertAsync(
            [
                new UserModelMessage([
                    new FileContentPart(
                        "text/plain",
                        null,
                        Convert.FromBase64String("SGVsbG8sIFdvcmxkIQ=="),
                        null
                    ),
                ]),
            ],
            new Dictionary<string, string> { ["image/*"] = "^https://.*$" }
        );

        var file = OnlyFile(result);
        Assert.Equal("text/plain", file.MediaType);
        Assert.Equal("Hello, World!", System.Text.Encoding.UTF8.GetString(file.Data!));
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle file parts with Uint8Array data",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_file_parts_with_Uint8Array_data()
    {
        var result = await ConvertAsync(
            [
                new UserModelMessage([
                    new FileContentPart("text/plain", null, [72, 101, 108, 108, 111], null),
                ]),
            ],
            new Dictionary<string, string> { ["image/*"] = "^https://.*$" }
        );

        var file = OnlyFile(result);
        Assert.Equal("text/plain", file.MediaType);
        Assert.Equal(new byte[] { 72, 101, 108, 108, 111 }, file.Data);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "should download files for user file parts with URL objects when model does not support downloads",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_download_files_for_user_file_parts_with_URL_objects_when_model_does_not_support_downloads() =>
        AssertPdfIsDownloadedAsync(NoUrls);

    [Fact]
    [UpstreamTest(
        Id
            + "should download files for user file parts with string URLs when model does not support downloads",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_download_files_for_user_file_parts_with_string_URLs_when_model_does_not_support_downloads() =>
        AssertPdfIsDownloadedAsync(NoUrls);

    [Fact]
    [UpstreamTest(
        Id
            + "should download files for user file parts with string URLs when model does not support the particular URL",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_download_files_for_user_file_parts_with_string_URLs_when_model_does_not_support_the_particular_URL() =>
        AssertPdfIsDownloadedAsync(
            new Dictionary<string, string>
            {
                // everything except https://example.com/document.pdf
                ["application/pdf"] = @"^(?!https://example\.com/document\.pdf$).*$",
            }
        );

    [Fact]
    [UpstreamTest(
        Id
            + "does not download URLs for user file parts for URL objects when model does support the URL",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Does_not_download_URLs_for_user_file_parts_for_URL_objects_when_model_does_support_the_URL()
    {
        var result = await ConvertAsync(
            PdfMessage(),
            new Dictionary<string, string>
            {
                // match exactly https://example.com/document.pdf
                ["application/pdf"] = @"^https://example\.com/document\.pdf$",
            },
            requests =>
            {
                Assert.True(Assert.Single(requests).IsUrlSupportedByModel);
                return Task.FromResult<IReadOnlyList<DownloadResult>>([null!]);
            }
        );

        var file = OnlyFile(result);
        Assert.Equal("application/pdf", file.MediaType);
        Assert.Equal(PdfUrl, file.Url);
        Assert.Null(file.Data);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "it should default to downloading the URL when the model does not provider a supportsUrl function",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task It_should_default_to_downloading_the_URL_when_the_model_does_not_provider_a_supportsUrl_function() =>
        AssertPdfIsDownloadedAsync(NoUrls);

    [Fact]
    [UpstreamTest(
        Id + "should handle file parts with filename",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_file_parts_with_filename()
    {
        var result = await ConvertAsync(
            [
                new UserModelMessage([
                    new FileContentPart(
                        "text/plain",
                        null,
                        Convert.FromBase64String("SGVsbG8sIFdvcmxkIQ=="),
                        "hello.txt"
                    ),
                ]),
            ],
            new Dictionary<string, string> { ["image/*"] = "^https://.*$" }
        );

        var file = OnlyFile(result);
        Assert.Equal("text/plain", file.MediaType);
        Assert.Equal("hello.txt", file.FileName);
        Assert.Equal("Hello, World!", System.Text.Encoding.UTF8.GetString(file.Data!));
    }

    [Fact]
    [UpstreamTest(
        Id + "should preserve filename when downloading file from URL",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_filename_when_downloading_file_from_URL()
    {
        var result = await ConvertAsync(
            PdfMessage("important-document.pdf"),
            NoUrls,
            DownloadOf(PdfUrl, Bytes, "application/pdf")
        );

        var file = OnlyFile(result);
        Assert.Equal("application/pdf", file.MediaType);
        Assert.Equal(Bytes, file.Data);
        Assert.Equal("important-document.pdf", file.FileName);
    }

    [Fact]
    [UpstreamTest(
        Id + "should prioritize user-provided mediaType over downloaded file mediaType",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prioritize_user_provided_mediaType_over_downloaded_file_mediaType()
    {
        var result = await ConvertAsync(
            [
                new UserModelMessage([
                    new FileContentPart("image/jpeg", "https://example.com/image.jpg", null, null),
                ]),
            ],
            NoUrls,
            DownloadOf("https://example.com/image.jpg", Bytes, "application/octet-stream")
        );

        var file = OnlyFile(result);
        Assert.Equal(Bytes, file.Data);
        Assert.Null(file.FileName);
        Assert.Equal("image/jpeg", file.MediaType);
        Assert.Null(file.ProviderOptions);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "should use downloaded file mediaType as fallback when user provides generic mediaType",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_downloaded_file_mediaType_as_fallback_when_user_provides_generic_mediaType()
    {
        var result = await ConvertAsync(
            [
                new UserModelMessage([
                    new FileContentPart(
                        "application/octet-stream",
                        "https://example.com/document.txt",
                        null,
                        null
                    ),
                ]),
            ],
            NoUrls,
            DownloadOf("https://example.com/document.txt", [72, 101, 108, 108, 111], "text/plain")
        );

        var file = OnlyFile(result);
        Assert.Equal(new byte[] { 72, 101, 108, 108, 111 }, file.Data);
        // The snapshot of upstream keeps the generic media type of the part.
        Assert.Equal("application/octet-stream", file.MediaType);
        Assert.Null(file.FileName);
        Assert.Null(file.ProviderOptions);
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass through provider reference for file parts without conversion",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_through_provider_reference_for_file_parts_without_conversion()
    {
        var reference = new Dictionary<string, string>
        {
            ["openai"] = "file-abc123",
            ["anthropic"] = "file-xyz789",
        };

        var result = await ConvertAsync([
            new UserModelMessage([
                FileContentPart.FromProviderReference("application/pdf", reference, "doc.pdf"),
            ]),
        ]);

        var file = OnlyFile(result);
        Assert.Equal("application/pdf", file.MediaType);
        Assert.Equal("doc.pdf", file.FileName);
        Assert.Equal(reference, file.ProviderReference);
        Assert.Null(file.Url);
        Assert.Null(file.Data);
    }
}
