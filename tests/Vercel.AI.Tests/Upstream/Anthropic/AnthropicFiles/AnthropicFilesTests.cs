// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.Anthropic.AnthropicFiles;

/// <summary>Anthropic files tests that sit outside a <c>describe</c> in upstream.</summary>
public sealed class AnthropicFilesTests
{
    [Theory]
    [InlineData("getFileMetadata")]
    [InlineData("downloadFile")]
    [InlineData("deleteFile")]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles::preserves API errors and async request headers for %s", Coverage = UpstreamCoverage.Partial, Note = "Provider headers are a synchronous callback in the port, so 'called once' is checked without an async header resolver.", Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Preserves_API_errors_and_request_headers(string operation)
    {
        var (files, handler) = FilesFixture.Create(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{\"type\":\"error\",\"error\":{\"type\":\"not_found_error\",\"message\":\"File not found\"}}", Encoding.UTF8, "application/json"),
            };
            return response;
        }, () => new Dictionary<string, string?> { ["Authorization"] = "Bearer test-token" });
        var reference = FilesFixture.Ref("file-abc123");
        var call = new Dictionary<string, string?> { ["x-request-id"] = "request-1" };
        var error = await Assert.ThrowsAsync<NotFoundException>(() => operation switch
        {
            "downloadFile" => files.DownloadFileAsync(reference, CancellationToken.None, call),
            "deleteFile" => files.DeleteFileAsync(reference, CancellationToken.None, call),
            _ => files.GetFileMetadataAsync(reference, CancellationToken.None, call),
        }).ConfigureAwait(false);
        Assert.Equal(404, error.StatusCode);
        Assert.Contains("File not found", error.Message);
        Assert.Single(handler.Calls);
        Assert.Equal("Bearer test-token", handler.Calls[0].Header("Authorization"));
        Assert.Equal("request-1", handler.Calls[0].Header("x-request-id"));
        Assert.Equal("files-api-2025-04-14", handler.Calls[0].Header("anthropic-beta"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("null")]
    [InlineData("\"2027-01-01T00:00:00Z\"")]
    [UpstreamTest("packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles::maps upload and metadata fields consistently with expiry %s", Coverage = UpstreamCoverage.Covered, Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05")]
    public async Task Maps_upload_and_metadata_fields_consistently_with_expiry(string? expiresAt)
    {
        var json = FilesFixture.Metadata.TrimEnd('}') + (expiresAt == null ? string.Empty : ",\"expires_at\":" + expiresAt) + "}";
        var (files, _) = FilesFixture.Json(json);
        var uploaded = await files.UploadAsync("blob", Encoding.UTF8.GetBytes("hello"), "text/plain", null, CancellationToken.None).ConfigureAwait(false);
        var metadata = await files.GetFileMetadataAsync(uploaded.ProviderReference.ToDictionary(p => p.Key, p => (string?)p.Value), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(uploaded.ProviderReference, metadata.ProviderReference);
        Assert.Equal(uploaded.FileName, metadata.FileName);
        Assert.Equal(uploaded.ProviderMetadata, metadata.ProviderMetadata);
        Assert.Equal("application/pdf", metadata.MediaType);
        Assert.Equal(12345, metadata.ByteSize);
        Assert.Equal(new DateTimeOffset(2025, 4, 14, 12, 0, 0, TimeSpan.Zero), metadata.CreatedAt);
        Assert.Equal(uploaded.ExpiresAt, metadata.ExpiresAt);
        if (expiresAt == null || expiresAt == "null")
        {
            Assert.Null(metadata.ExpiresAt);
            Assert.False(metadata.ProviderMetadata["anthropic"].ContainsKey("expiresAt"));
        }
        else
        {
            Assert.Equal(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero), metadata.ExpiresAt);
            Assert.Equal("2027-01-01T00:00:00Z", metadata.ProviderMetadata["anthropic"]["expiresAt"]);
        }
    }
}
