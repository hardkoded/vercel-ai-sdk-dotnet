// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.Anthropic;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.Anthropic.AnthropicFiles;

/// <summary>Anthropic <c>deleteFile</c> requests.</summary>
public sealed class DeleteFileTests
{
    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > deleteFile::should delete a file via DELETE", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Deletes_a_file_via_DELETE()
    {
        var (files, handler) = FilesFixture.Json("{\"id\":\"file-abc123\",\"type\":\"file_deleted\"}");
        var result = await files.DeleteFileAsync(FilesFixture.Ref("file-abc123"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(HttpMethod.Delete, handler.Calls[0].Method);
        Assert.True(result.Deleted);
        Assert.Equal("file-abc123", result.ProviderReference["anthropic"]);
    }

    [Theory]
    [InlineData("{\"id\":\"file-abc123\"}")]
    [InlineData("{\"id\":\"file-abc123\",\"type\":null}")]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > deleteFile::reports successful deletion with an omitted/null type (%s)", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Reports_successful_deletion_with_an_omitted_or_null_type(string json)
    {
        var (files, _) = FilesFixture.Json(json);
        var result = await files.DeleteFileAsync(FilesFixture.Ref("file-abc123"), CancellationToken.None).ConfigureAwait(false);
        Assert.True(result.Deleted);
        Assert.Equal("file-abc123", result.ProviderReference["anthropic"]);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > deleteFile::should reject a blank anthropic file id", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Rejects_a_blank_file_id()
    {
        var (files, _) = FilesFixture.Json("{}");
        var error = await Assert.ThrowsAsync<ArgumentException>(() => files.DeleteFileAsync(FilesFixture.Ref(""), CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("file reference is missing an 'anthropic' file id.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > deleteFile::should reject a whitespace-only anthropic file id", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Rejects_a_whitespace_only_file_id()
    {
        var (files, _) = FilesFixture.Json("{}");
        var error = await Assert.ThrowsAsync<ArgumentException>(() => files.DeleteFileAsync(FilesFixture.Ref("   "), CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("file reference is missing an 'anthropic' file id.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > deleteFile::should reject a reference without an anthropic file id", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Rejects_a_reference_without_an_anthropic_file_id()
    {
        var (files, _) = FilesFixture.Json("{}");
        var error = await Assert.ThrowsAsync<ArgumentException>(() => files.DeleteFileAsync(new Dictionary<string, string?> { ["other"] = "file-abc123" }, CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("file reference is missing an 'anthropic' file id.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > deleteFile::should send anthropic-beta header", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Sends_the_anthropic_beta_header()
    {
        var (files, handler) = FilesFixture.Json("{\"id\":\"file-abc123\",\"type\":\"file_deleted\"}");
        await files.DeleteFileAsync(FilesFixture.Ref("file-abc123"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("files-api-2025-04-14", handler.Calls[0].Header("anthropic-beta"));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > deleteFile::should support abortSignal", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Supports_cancellation()
    {
        var (files, _) = FilesFixture.Json("{\"id\":\"file-abc123\",\"type\":\"file_deleted\"}");
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => files.DeleteFileAsync(FilesFixture.Ref("file-abc123"), cts.Token)).ConfigureAwait(false);
    }

    [Theory]
    [InlineData(".", "%252E")]
    [InlineData("..", "%252E%252E")]
    [InlineData("file/a?#%", "file%2Fa%3F%23%25")]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > deleteFile::should encode file id $fileId as one URL path segment", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Encodes_the_file_id_as_one_url_path_segment(string fileId, string encodedFileId)
    {
        var (files, handler) = FilesFixture.Json("{\"id\":\"file-abc123\",\"type\":\"file_deleted\"}");
        await files.DeleteFileAsync(FilesFixture.Ref(fileId), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("https://api.anthropic.com/v1/files/" + encodedFileId, handler.Calls[0].Uri.OriginalString);
    }
}

internal static class FilesFixture
{
    public const string Metadata = "{\"id\":\"file-abc123\",\"type\":\"file\",\"filename\":\"test.pdf\",\"mime_type\":\"application/pdf\",\"size_bytes\":12345,\"created_at\":\"2025-04-14T12:00:00Z\",\"downloadable\":true}";

    public static IReadOnlyDictionary<string, string?> Ref(string? id) => new Dictionary<string, string?> { ["anthropic"] = id };

    public static (global::Vercel.AI.Anthropic.AnthropicFiles Files, ParityHandler Handler) Create(Func<ParityCall, HttpResponseMessage> respond, Func<IReadOnlyDictionary<string, string?>>? headers = null)
    {
        var handler = new ParityHandler(respond);
        var files = new global::Vercel.AI.Anthropic.AnthropicFiles(
            new Vercel.AI.ProviderUtils.ProviderHttp(new HttpClient(handler, disposeHandler: false)),
            () => "https://api.anthropic.com/v1",
            headers ?? (() => new Dictionary<string, string?> { ["x-api-key"] = "test-api-key", ["anthropic-version"] = "2023-06-01" }),
            "anthropic.files");
        return (files, handler);
    }

    public static (global::Vercel.AI.Anthropic.AnthropicFiles Files, ParityHandler Handler) Json(string json) => Create(_ => ParityHandler.Json(json));
}
