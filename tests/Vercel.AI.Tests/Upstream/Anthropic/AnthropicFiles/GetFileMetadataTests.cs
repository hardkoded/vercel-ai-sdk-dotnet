// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.Anthropic;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.Anthropic.AnthropicFiles;

/// <summary>Anthropic <c>getFileMetadata</c> requests.</summary>
public sealed class GetFileMetadataTests
{
    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > getFileMetadata::should retrieve file metadata via GET", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Retrieves_file_metadata_via_GET()
    {
        var (files, handler) = FilesFixture.Json(FilesFixture.Metadata);
        var result = await files.GetFileMetadataAsync(FilesFixture.Ref("file-abc123"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(HttpMethod.Get, handler.Calls[0].Method);
        Assert.Equal("file-abc123", result.ProviderReference["anthropic"]);
        Assert.Equal("test.pdf", result.FileName);
        Assert.Equal("application/pdf", result.MediaType);
        Assert.Equal(12345, result.ByteSize);
        Assert.Equal(new DateTimeOffset(2025, 4, 14, 12, 0, 0, TimeSpan.Zero), result.CreatedAt);
        Assert.Equal("test.pdf", result.ProviderMetadata["filename"]);
        Assert.Equal("application/pdf", result.ProviderMetadata["mimeType"]);
        Assert.Equal(12345L, result.ProviderMetadata["sizeBytes"]);
        Assert.Equal("2025-04-14T12:00:00Z", result.ProviderMetadata["createdAt"]);
        Assert.Equal(true, result.ProviderMetadata["downloadable"]);
        Assert.Equal(5, result.ProviderMetadata.Count);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > getFileMetadata::should handle metadata without downloadable field", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Handles_metadata_without_a_downloadable_field()
    {
        var (files, _) = FilesFixture.Json("{\"id\":\"file-abc123\",\"type\":\"file\",\"filename\":\"test.pdf\",\"mime_type\":\"application/pdf\",\"size_bytes\":12345,\"created_at\":\"2025-04-14T12:00:00Z\"}");
        var result = await files.GetFileMetadataAsync(FilesFixture.Ref("file-abc123"), CancellationToken.None).ConfigureAwait(false);
        Assert.False(result.ProviderMetadata.ContainsKey("downloadable"));
    }

    [Theory]
    [InlineData("{\"id\":\"file-abc123\"}")]
    [InlineData("{\"id\":\"file-abc123\",\"type\":null,\"filename\":null,\"mime_type\":null,\"size_bytes\":null,\"created_at\":null,\"downloadable\":null}")]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > getFileMetadata::handles omitted or null metadata fields: %j", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Handles_omitted_or_null_metadata_fields(string json)
    {
        var (files, _) = FilesFixture.Json(json);
        var result = await files.GetFileMetadataAsync(FilesFixture.Ref("file-abc123"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("file-abc123", result.ProviderReference["anthropic"]);
        Assert.Null(result.FileName);
        Assert.Null(result.ByteSize);
        Assert.Null(result.CreatedAt);
        Assert.Empty(result.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > getFileMetadata::should reject a blank anthropic file id", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Rejects_a_blank_file_id()
    {
        var (files, _) = FilesFixture.Json("{}");
        var error = await Assert.ThrowsAsync<ArgumentException>(() => files.GetFileMetadataAsync(FilesFixture.Ref(""), CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("file reference is missing an 'anthropic' file id.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > getFileMetadata::should reject a whitespace-only anthropic file id", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Rejects_a_whitespace_only_file_id()
    {
        var (files, _) = FilesFixture.Json("{}");
        var error = await Assert.ThrowsAsync<ArgumentException>(() => files.GetFileMetadataAsync(FilesFixture.Ref("   "), CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("file reference is missing an 'anthropic' file id.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > getFileMetadata::should reject a reference without an anthropic file id", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Rejects_a_reference_without_an_anthropic_file_id()
    {
        var (files, _) = FilesFixture.Json("{}");
        var error = await Assert.ThrowsAsync<ArgumentException>(() => files.GetFileMetadataAsync(new Dictionary<string, string?> { ["other"] = "file-abc123" }, CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("file reference is missing an 'anthropic' file id.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > getFileMetadata::should send anthropic-beta header", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Sends_the_anthropic_beta_header()
    {
        var (files, handler) = FilesFixture.Json(FilesFixture.Metadata);
        await files.GetFileMetadataAsync(FilesFixture.Ref("file-abc123"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("files-api-2025-04-14", handler.Calls[0].Header("anthropic-beta"));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > getFileMetadata::should support abortSignal", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Supports_cancellation()
    {
        var (files, _) = FilesFixture.Json(FilesFixture.Metadata);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => files.GetFileMetadataAsync(FilesFixture.Ref("file-abc123"), cts.Token)).ConfigureAwait(false);
    }

    [Theory]
    [InlineData(".", "%252E")]
    [InlineData("..", "%252E%252E")]
    [InlineData("file/a?#%", "file%2Fa%3F%23%25")]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > getFileMetadata::should encode file id $fileId as one URL path segment", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Encodes_the_file_id_as_one_url_path_segment(string fileId, string encodedFileId)
    {
        var (files, handler) = FilesFixture.Json(FilesFixture.Metadata);
        await files.GetFileMetadataAsync(FilesFixture.Ref(fileId), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("https://api.anthropic.com/v1/files/" + encodedFileId, handler.Calls[0].Uri.OriginalString);
    }
}
