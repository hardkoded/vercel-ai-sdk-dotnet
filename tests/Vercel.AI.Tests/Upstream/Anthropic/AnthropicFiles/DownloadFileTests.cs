// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.Anthropic;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.Anthropic.AnthropicFiles;

/// <summary>Anthropic <c>downloadFile</c> requests.</summary>
public sealed class DownloadFileTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("; charset=utf-8")]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > downloadFile::omits unknown download media types (%s)", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Omits_unknown_download_media_types(string? contentType)
    {
        var (files, _) = FilesFixture.Create(_ => ParityHandler.Bytes(Encoding.UTF8.GetBytes("file content"), null, contentType == null ? Array.Empty<(string, string)>() : new[] { ("Content-Type", contentType) }));
        var result = await files.DownloadFileAsync(FilesFixture.Ref("file-abc123"), CancellationToken.None).ConfigureAwait(false);
        Assert.Null(result.MediaType);
        using var reader = new StreamReader(result.Content);
        Assert.Equal("file content", await reader.ReadToEndAsync().ConfigureAwait(false));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > downloadFile::should download file content as a stream", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Downloads_file_content_as_a_stream()
    {
        var (files, handler) = FilesFixture.Create(_ => ParityHandler.Bytes(Encoding.UTF8.GetBytes("test file content"), "application/pdf"));
        var result = await files.DownloadFileAsync(FilesFixture.Ref("file-abc123"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(HttpMethod.Get, handler.Calls[0].Method);
        Assert.Equal("https://api.anthropic.com/v1/files/file-abc123/content", handler.Calls[0].Uri.AbsoluteUri);
        using var reader = new StreamReader(result.Content);
        Assert.Equal("test file content", await reader.ReadToEndAsync().ConfigureAwait(false));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > downloadFile::should expose the response content type as mediaType (parameters stripped)", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Exposes_the_response_content_type_as_media_type()
    {
        var (files, _) = FilesFixture.Create(_ => ParityHandler.Bytes(Encoding.UTF8.GetBytes("test file content"), "application/pdf; charset=utf-8"));
        var result = await files.DownloadFileAsync(FilesFixture.Ref("file-abc123"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("application/pdf", result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > downloadFile::should reject a blank anthropic file id", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Rejects_a_blank_file_id()
    {
        var (files, _) = FilesFixture.Json("{}");
        var error = await Assert.ThrowsAsync<ArgumentException>(() => files.DownloadFileAsync(FilesFixture.Ref(""), CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("file reference is missing an 'anthropic' file id.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > downloadFile::should reject a whitespace-only anthropic file id", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Rejects_a_whitespace_only_file_id()
    {
        var (files, _) = FilesFixture.Json("{}");
        var error = await Assert.ThrowsAsync<ArgumentException>(() => files.DownloadFileAsync(FilesFixture.Ref("   "), CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("file reference is missing an 'anthropic' file id.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > downloadFile::should reject a reference without an anthropic file id", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Rejects_a_reference_without_an_anthropic_file_id()
    {
        var (files, _) = FilesFixture.Json("{}");
        var error = await Assert.ThrowsAsync<ArgumentException>(() => files.DownloadFileAsync(new Dictionary<string, string?> { ["other"] = "file-abc123" }, CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("file reference is missing an 'anthropic' file id.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > downloadFile::should send anthropic-beta header", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Sends_the_anthropic_beta_header()
    {
        var (files, handler) = FilesFixture.Create(_ => ParityHandler.Bytes(Encoding.UTF8.GetBytes("test file content"), "application/pdf"));
        await files.DownloadFileAsync(FilesFixture.Ref("file-abc123"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("files-api-2025-04-14", handler.Calls[0].Header("anthropic-beta"));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > downloadFile::should support abortSignal", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Supports_cancellation()
    {
        var (files, _) = FilesFixture.Create(_ => ParityHandler.Bytes(Encoding.UTF8.GetBytes("test file content"), "application/pdf"));
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => files.DownloadFileAsync(FilesFixture.Ref("file-abc123"), cts.Token)).ConfigureAwait(false);
    }

    [Theory]
    [InlineData(".", "%252E")]
    [InlineData("..", "%252E%252E")]
    [InlineData("file/a?#%", "file%2Fa%3F%23%25")]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > downloadFile::should encode file id $fileId as one URL path segment", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Encodes_the_file_id_as_one_url_path_segment(string fileId, string encodedFileId)
    {
        var (files, handler) = FilesFixture.Create(_ => ParityHandler.Bytes(Encoding.UTF8.GetBytes("test file content"), "application/pdf"));
        await files.DownloadFileAsync(FilesFixture.Ref(fileId), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("https://api.anthropic.com/v1/files/" + encodedFileId + "/content", handler.Calls[0].Uri.OriginalString);
    }
}
