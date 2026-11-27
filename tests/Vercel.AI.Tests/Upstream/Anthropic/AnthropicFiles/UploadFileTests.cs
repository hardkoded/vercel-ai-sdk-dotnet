// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using Vercel.AI.Operations;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.Anthropic.AnthropicFiles;

/// <summary>Anthropic <c>uploadFile</c> requests.</summary>
public sealed class UploadFileTests
{
    private const string Prefix =
        "packages/anthropic/src/files/anthropic-files.test.ts::AnthropicFiles > uploadFile::";
    private const string CatalogPrefix =
        "packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::";
    private const string Commit = "8b11ff6eb6302564cf4c2cabd67420f0f04cda05";
    private static readonly byte[] Bytes = [1, 2, 3];

    [Fact]
    [UpstreamTest(
        Prefix + "sends POST to /v1/files with correct beta header",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    [UpstreamTest(
        CatalogPrefix + "sends POST to /v1/files with correct beta header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Sends_POST_to_v1_files_with_correct_beta_header()
    {
        var (files, handler) = FilesFixture.Json(FilesFixture.Metadata);
        await files
            .UploadAsync("blob", Bytes, "application/octet-stream", null, CancellationToken.None)
            .ConfigureAwait(false);
        Assert.Single(handler.Calls);
        Assert.Equal(HttpMethod.Post, handler.Calls[0].Method);
        Assert.Equal("https://api.anthropic.com/v1/files", handler.Calls[0].Uri.AbsoluteUri);
        Assert.Equal("files-api-2025-04-14", handler.Calls[0].Header("anthropic-beta"));
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::rejects stream data at runtime and cancels the stream",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Rejects_stream_data_at_runtime_and_cancels_the_stream()
    {
        var (files, handler) = FilesFixture.Json(FilesFixture.Metadata);
        var stream = new UploadStream();
        await Assert
            .ThrowsAsync<UnsupportedFunctionalityException>(() =>
                files.UploadFileAsync(
                    new UploadFileCall(
                        new UploadData("stream", stream),
                        "application/octet-stream",
                        null,
                        CancellationToken.None,
                        null,
                        null
                    ),
                    CancellationToken.None
                )
            )
            .ConfigureAwait(false);
        Assert.True(stream.Cancelled);
        Assert.Empty(handler.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "threads per-call headers and abortSignal",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    [UpstreamTest(
        CatalogPrefix + "threads per-call headers and abortSignal",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Threads_per_call_headers_and_abortSignal()
    {
        var (files, handler) = FilesFixture.Json(FilesFixture.Metadata);
        await files
            .UploadAsync(
                "blob",
                Bytes,
                "application/octet-stream",
                null,
                CancellationToken.None,
                new Dictionary<string, string?> { ["x-request-id"] = "req-1" }
            )
            .ConfigureAwait(false);
        Assert.Equal("req-1", handler.Calls[0].Header("x-request-id"));

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert
            .ThrowsAnyAsync<OperationCanceledException>(() =>
                files.UploadAsync(
                    "blob",
                    Bytes,
                    "application/octet-stream",
                    null,
                    cancellation.Token
                )
            )
            .ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "sends x-api-key header",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    [UpstreamTest(CatalogPrefix + "sends x-api-key header", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_x_api_key_header()
    {
        var (files, handler) = FilesFixture.Json(FilesFixture.Metadata);
        await files
            .UploadAsync("blob", Bytes, "application/octet-stream", null, CancellationToken.None)
            .ConfigureAwait(false);
        Assert.Equal("test-api-key", handler.Calls[0].Header("x-api-key"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "resolves auth tokens for each upload without changing beta header precedence",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Resolves_auth_tokens_for_each_upload_without_changing_beta_header_precedence()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(FilesFixture.Metadata));
        var tokens = new Queue<string>(["first-token", "refreshed-token"]);
        var resolved = 0;
        var options = new AnthropicOptions
        {
            BaseUrl = "https://api.anthropic.com/v1",
            AuthTokenProvider = _ =>
            {
                resolved++;
                return Task.FromResult(tokens.Dequeue());
            },
        };
        options.Headers["anthropic-beta"] = "provider-beta";
        var files = AnthropicProvider.Create(options, handler).Files();

        for (var i = 0; i < 2; i++)
        {
            await files
                .UploadAsync(
                    "blob",
                    Bytes,
                    "application/octet-stream",
                    null,
                    CancellationToken.None,
                    new Dictionary<string, string?>
                    {
                        ["anthropic-beta"] = "request-beta,files-api-2025-04-14",
                    }
                )
                .ConfigureAwait(false);
        }

        Assert.Equal(2, resolved);
        Assert.Equal(
            ["Bearer first-token", "Bearer refreshed-token"],
            handler.Calls.Select(call => call.Header("Authorization"))
        );
        foreach (var call in handler.Calls)
        {
            Assert.Null(call.Header("x-api-key"));
            Assert.Equal("request-beta,files-api-2025-04-14", call.Header("anthropic-beta"));
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "sends multipart form data with file",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    [UpstreamTest(
        CatalogPrefix + "sends multipart form data with file",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Sends_multipart_form_data_with_file()
    {
        var (files, handler) = FilesFixture.Json(FilesFixture.Metadata);
        await files
            .UploadAsync("blob", Bytes, "application/octet-stream", null, CancellationToken.None)
            .ConfigureAwait(false);
        Assert.StartsWith("multipart/form-data", handler.Calls[0].Header("Content-Type"));
        Assert.Contains("name=file", handler.Calls[0].Text.Replace("\"", string.Empty));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "uses default filename \"blob\" when not specified",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    [UpstreamTest(
        CatalogPrefix + "uses default filename \"blob\" when not specified",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Uses_default_filename_blob_when_not_specified()
    {
        var (files, handler) = FilesFixture.Json(FilesFixture.Metadata);
        await files
            .UploadFileAsync(
                string.Empty,
                Bytes,
                "application/octet-stream",
                CancellationToken.None
            )
            .ConfigureAwait(false);
        Assert.Contains("filename=blob", handler.Calls[0].Text.Replace("\"", string.Empty));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "uses custom filename from spec options",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    [UpstreamTest(
        CatalogPrefix + "uses custom filename from spec options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Uses_custom_filename_from_spec_options()
    {
        var (files, handler) = FilesFixture.Json(FilesFixture.Metadata);
        await files
            .UploadAsync(
                "custom-name.pdf",
                Bytes,
                "application/octet-stream",
                null,
                CancellationToken.None
            )
            .ConfigureAwait(false);
        Assert.Contains(
            "filename=custom-name.pdf",
            handler.Calls[0].Text.Replace("\"", string.Empty)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "uses mediaType from spec options",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    [UpstreamTest(
        CatalogPrefix + "uses mediaType from spec options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Uses_mediaType_from_spec_options()
    {
        var (files, handler) = FilesFixture.Json(FilesFixture.Metadata);
        await files
            .UploadAsync("blob", Bytes, "application/pdf", null, CancellationToken.None)
            .ConfigureAwait(false);
        Assert.Contains("Content-Type: application/pdf", handler.Calls[0].Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "returns providerReference with anthropic key set to file ID",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    [UpstreamTest(
        CatalogPrefix + "returns providerReference with anthropic key set to file ID",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Returns_providerReference_with_anthropic_key_set_to_file_ID()
    {
        var (files, _) = FilesFixture.Json(FilesFixture.Metadata);
        var result = await files
            .UploadAsync("blob", Bytes, "application/octet-stream", null, CancellationToken.None)
            .ConfigureAwait(false);
        Assert.Equal(
            new Dictionary<string, string> { ["anthropic"] = "file-abc123" },
            result.ProviderReference
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "returns providerMetadata with response data",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    [UpstreamTest(
        CatalogPrefix + "returns providerMetadata with response data",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Returns_providerMetadata_with_response_data()
    {
        var (files, _) = FilesFixture.Json(FilesFixture.Metadata);
        var result = await files
            .UploadAsync("blob", Bytes, "application/octet-stream", null, CancellationToken.None)
            .ConfigureAwait(false);
        var metadata = Assert.Single(result.ProviderMetadata).Value;
        Assert.Equal("anthropic", Assert.Single(result.ProviderMetadata).Key);
        Assert.Equal("test.pdf", metadata["filename"]);
        Assert.Equal("application/pdf", metadata["mimeType"]);
        Assert.Equal(12345L, metadata["sizeBytes"]);
        Assert.Equal("2025-04-14T12:00:00Z", metadata["createdAt"]);
        Assert.Equal(true, metadata["downloadable"]);
        Assert.Equal(5, metadata.Count);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "omits downloadable from providerMetadata when null",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    [UpstreamTest(
        CatalogPrefix + "omits downloadable from providerMetadata when null",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Omits_downloadable_from_providerMetadata_when_null()
    {
        var (files, _) = FilesFixture.Json(
            FilesFixture.Metadata.Replace("\"downloadable\":true", "\"downloadable\":null")
        );
        var result = await files
            .UploadAsync("blob", Bytes, "application/octet-stream", null, CancellationToken.None)
            .ConfigureAwait(false);
        Assert.DoesNotContain("downloadable", result.ProviderMetadata["anthropic"].Keys);
    }

    [Fact]
    [UpstreamTest(
        CatalogPrefix + "handles base64 string data",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Handles_base64_string_data()
    {
        var (files, _) = FilesFixture.Json(FilesFixture.Metadata);
        var result = await files
            .UploadFileAsync(
                new UploadFileCall(
                    new UploadData("data", "AQID"),
                    "application/octet-stream",
                    null,
                    CancellationToken.None,
                    null,
                    null
                ),
                CancellationToken.None
            )
            .ConfigureAwait(false);
        Assert.Equal("file-abc123", result.ProviderReference.Id);
        Assert.Equal("anthropic", result.ProviderReference.Provider);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "has specificationVersion v4",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    [UpstreamTest(
        CatalogPrefix + "has specificationVersion v4",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Has_specificationVersion_v4()
    {
        var (files, _) = FilesFixture.Json(FilesFixture.Metadata);
        Assert.Equal("v4", files.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "has correct provider name",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    [UpstreamTest(CatalogPrefix + "has correct provider name", Coverage = UpstreamCoverage.Covered)]
    public void Has_correct_provider_name()
    {
        var (files, _) = FilesFixture.Json(FilesFixture.Metadata);
        Assert.Equal("anthropic.files", files.Provider);
    }
}
