// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.DeepSeek;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests;

public sealed class DeepSeekUpstreamTests
{
    private const string FileTests = "packages/deepseek/src/files/deepseek-files.test.ts::DeepSeek Files - uploadFile::";

    private static readonly byte[] Png = { 0x89, 0x50, 0x4E, 0x47 };

    [Fact]
    [UpstreamTest("packages/deepseek/src/files/deepseek-files.test.ts::DeepSeek Files - uploadFile::should upload an image with the user_data purpose", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_sends_user_data()
    {
        var capture = Ok("file_1", "photo.png");
        var file = await Store(capture).UploadFileAsync("photo.png", Png, "image/png", CancellationToken.None);
        Assert.Equal("file_1", file.Id);
        Assert.Equal("photo.png", file.FileName);
        Assert.Equal("https://api.deepseek.com/files", capture.Requests[0].Uri!.AbsoluteUri);
        Assert.Contains("user_data", capture.Requests[0].Body);
        Assert.Contains("photo.png", capture.Requests[0].Body);
    }

    [Fact]
    [UpstreamTest("packages/deepseek/src/files/deepseek-files.test.ts::DeepSeek Files - uploadFile::should reject a response without a file id", Coverage = UpstreamCoverage.Covered)]
    public async Task A_response_without_an_id_is_rejected()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"object\":\"file\",\"bytes\":1024,\"created_at\":1700000000,\"filename\":\"comic-cat.png\",\"purpose\":\"user_data\"}" };
        await AssertInvalidResponseField(Store(capture).UploadFileAsync("photo.png", Png, "image/png", CancellationToken.None), "id");
    }

    [Fact]
    [UpstreamTest(FileTests + "should reject stream data at runtime and cancel the stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_data_is_rejected_and_the_stream_is_cancelled()
    {
        var capture = Ok("file_1", "comic-cat.png");
        var stream = new UploadStream();
        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => UploadFile.UploadFileAsync(new UploadFileRequest { Api = Store(capture), Data = new UploadData("stream", stream), MediaType = "image/png", Filename = "comic-cat.png" }));
        Assert.True(stream.Cancelled);
        Assert.Empty(capture.Requests);
    }

    [Fact]
    [UpstreamTest(FileTests + "should thread per-call headers and abortSignal", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_sends_call_headers_and_honors_cancellation()
    {
        var capture = FileResponse();
        var store = Store(capture);
        await store.UploadFileAsync(Call(new byte[] { 1, 2, 3 }, "image/png", "comic-cat.png", new Dictionary<string, string> { ["x-request-id"] = "req-1" }), CancellationToken.None);
        Assert.Equal("req-1", capture.Requests[0].Headers["x-request-id"]);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        await Assert.ThrowsAsync<ApiUserAbortException>(() => store.UploadFileAsync(Call(new byte[] { 1, 2, 3 }, "image/png", "comic-cat.png"), cancelled.Token));
    }

    [Fact]
    [UpstreamTest(FileTests + "should return a DeepSeek provider reference and response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_returns_the_provider_reference_and_metadata()
    {
        var capture = FileResponse("{\"id\":\"file-api-xyz789\",\"object\":\"file\",\"bytes\":1024,\"created_at\":1700000000,\"filename\":\"comic-cat.png\",\"purpose\":\"user_data\",\"expires_at\":1700003600}");
        var result = await Store(capture).UploadFileAsync(Call(new byte[] { 1, 2, 3 }, "image/png", "comic-cat.png"), CancellationToken.None);
        Assert.Empty(result.Warnings);
        Assert.Equal("file-api-xyz789", result.ProviderReference.Id);
        Assert.Equal("deepseek", result.ProviderReference.Provider);
        Assert.Equal("comic-cat.png", result.Filename);
        Assert.Equal("image/png", result.MediaType);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"deepseek\":{\"object\":\"file\",\"filename\":\"comic-cat.png\",\"purpose\":\"user_data\",\"bytes\":1024,\"createdAt\":1700000000,\"expiresAt\":1700003600}}");
    }

    [Theory]
    [UpstreamTest(FileTests + "should tolerate $name optional response metadata and fall back to the request filename", Coverage = UpstreamCoverage.Covered)]
    [InlineData("{\"id\":\"file-api-incomplete\"}")]
    [InlineData("{\"id\":\"file-api-incomplete\",\"object\":null,\"bytes\":null,\"created_at\":null,\"filename\":null,\"purpose\":null,\"expires_at\":null}")]
    public async Task Missing_response_metadata_falls_back_to_the_request_filename(string body)
    {
        var result = await Store(FileResponse(body)).UploadFileAsync(Call(new byte[] { 1, 2, 3 }, "image/png", "request-filename.png"), CancellationToken.None);
        Assert.Empty(result.Warnings);
        Assert.Equal("file-api-incomplete", result.ProviderReference.Id);
        Assert.Equal("request-filename.png", result.Filename);
        Assert.Equal("image/png", result.MediaType);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"deepseek\":{}}");
    }

    [Theory]
    [UpstreamTest(FileTests + "should reject an invalid %s response field", Coverage = UpstreamCoverage.Covered)]
    [InlineData("object", "\"document\"")]
    [InlineData("purpose", "\"assistants\"")]
    [InlineData("bytes", "-1")]
    [InlineData("bytes", "1.5")]
    [InlineData("created_at", "-1")]
    [InlineData("created_at", "1.5")]
    [InlineData("expires_at", "-1")]
    [InlineData("expires_at", "1.5")]
    [InlineData("filename", "123")]
    public async Task Invalid_response_fields_are_rejected(string field, string value)
    {
        var body = JsonNode.Parse("{\"id\":\"file-api-invalid\",\"object\":\"file\",\"bytes\":1024,\"created_at\":1700000000,\"filename\":\"comic-cat.png\",\"purpose\":\"user_data\"}")!.AsObject();
        body[field] = JsonNode.Parse(value);
        await AssertInvalidResponseField(Store(FileResponse(body.ToJsonString())).UploadFileAsync(Call(new byte[] { 1, 2, 3 }, "image/png", null), CancellationToken.None), field);
    }

    [Fact]
    [UpstreamTest(FileTests + "should handle base64-encoded data", Coverage = UpstreamCoverage.Covered)]
    public async Task Base64_data_is_uploaded()
    {
        var capture = FileResponse();
        var result = await Store(capture).UploadFileAsync(Call(Convert.ToBase64String(Encoding.UTF8.GetBytes("image bytes")), "image/png", null), CancellationToken.None);
        Assert.Equal("file-api-abc123", result.ProviderReference.Id);
        Assert.Contains("image bytes", capture.Requests[0].Body);
    }

    [Theory]
    [UpstreamTest(FileTests + "should accept $format uploads", Coverage = UpstreamCoverage.Covered)]
    [InlineData(new byte[] { 0xff, 0xd8, 0xff }, "image/jpeg", "image.jpeg")]
    [InlineData(new byte[] { 0x89, 0x50, 0x4e, 0x47 }, "image/png", "image.png")]
    [InlineData(new byte[] { 0x47, 0x49, 0x46 }, "image/gif", "image.gif")]
    [InlineData(new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 }, "image/webp", "image.webp")]
    public async Task Supported_image_formats_are_uploaded(byte[] data, string mediaType, string filename)
    {
        var capture = FileResponse();
        await Store(capture).UploadFileAsync(Call(data, mediaType, filename), CancellationToken.None);
        Assert.Single(capture.Requests);
    }

    [Fact]
    [UpstreamTest(FileTests + "should accept supported base64-encoded file content", Coverage = UpstreamCoverage.Covered)]
    public async Task Base64_image_content_allows_a_generic_media_type()
    {
        var capture = FileResponse();
        await Store(capture).UploadFileAsync(Call(Convert.ToBase64String(Encoding.ASCII.GetBytes("GIF89a")), "application/octet-stream", null), CancellationToken.None);
        Assert.Single(capture.Requests);
    }

    [Fact]
    [UpstreamTest(FileTests + "should expose the v4 files interface", Coverage = UpstreamCoverage.Covered)]
    public void Files_expose_the_v4_interface()
    {
        var store = Store(FileResponse());
        Assert.Equal("v4", store.SpecificationVersion);
        Assert.Equal("deepseek.files", store.Provider);
    }

    [Fact]
    [UpstreamTest("packages/deepseek/src/files/deepseek-files.test.ts::DeepSeek Files - uploadFile::should pass expires_after as bracketed multipart fields", Coverage = UpstreamCoverage.Covered)]
    public async Task Expiry_is_sent_as_bracketed_fields()
    {
        var capture = Ok("file_1", "photo.png");
        var store = Store(capture);
        store.ExpiresAfterSeconds = 3600;
        await store.UploadFileAsync("photo.png", Png, "image/png", CancellationToken.None);
        Assert.Contains("created_at", capture.Requests[0].Body);
        Assert.Contains("3600", capture.Requests[0].Body);
        Assert.Contains("expires_after[anchor]", capture.Requests[0].Body);
        Assert.Contains("expires_after[seconds]", capture.Requests[0].Body);
    }

    [Fact]
    [UpstreamTest("packages/deepseek/src/files/deepseek-files.test.ts::DeepSeek Files - uploadFile::should omit expires_after fields when no expiry is requested", Coverage = UpstreamCoverage.Covered)]
    public async Task Expiry_fields_are_omitted_by_default()
    {
        var capture = Ok("file_1", "photo.png");
        await Store(capture).UploadFileAsync("photo.png", Png, "image/png", CancellationToken.None);
        Assert.DoesNotContain("expires_after", capture.Requests[0].Body);
    }

    [Fact]
    [UpstreamTest("packages/deepseek/src/files/deepseek-files.test.ts::DeepSeek Files - uploadFile::should reject an expiry outside the supported range", Coverage = UpstreamCoverage.Covered)]
    public async Task An_expiry_below_the_minimum_is_rejected_before_upload()
    {
        var capture = Ok("file_1", "photo.png");
        var store = Store(capture);
        store.ExpiresAfterSeconds = 10;
        var error = await Assert.ThrowsAsync<AiSdkException>(() => store.UploadFileAsync("photo.png", Png, "image/png", CancellationToken.None));
        Assert.Contains("3600", error.Message);
        Assert.Contains("2592000", error.Message);
        Assert.Empty(capture.Requests);
    }

    [Fact]
    [UpstreamTest("packages/deepseek/src/files/deepseek-files.test.ts::DeepSeek Files - uploadFile::should pass authentication and custom headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_sends_the_bearer_token_and_call_headers()
    {
        var capture = Ok("file_1", "photo.png");
        var store = Store(capture);
        store.Headers = new Dictionary<string, string?> { ["X-Custom"] = "yes" };
        await store.UploadFileAsync("photo.png", Png, "image/png", CancellationToken.None);
        Assert.Equal("Bearer secret", capture.Requests[0].Headers["Authorization"]);
        Assert.Equal("yes", capture.Requests[0].Headers["X-Custom"]);
    }

    [Fact]
    [UpstreamTest("packages/deepseek/src/files/deepseek-files.test.ts::DeepSeek Files - uploadFile::should accept the image/jpg media type alias", Coverage = UpstreamCoverage.Covered)]
    public void Image_jpg_is_accepted()
    {
        DeepSeekFileStore.Validate(3, "photo.jpg", "image/jpg", new byte[] { 1, 2, 3 });
    }

    [Fact]
    [UpstreamTest("packages/deepseek/src/files/deepseek-files.test.ts::DeepSeek Files - uploadFile::should accept a supported filename when the media type is generic", Coverage = UpstreamCoverage.Covered)]
    public void A_png_filename_allows_a_generic_media_type()
    {
        DeepSeekFileStore.Validate(3, "photo.png", "application/octet-stream", new byte[] { 1, 2, 3 });
    }

    [Fact]
    [UpstreamTest("packages/deepseek/src/files/deepseek-files.test.ts::DeepSeek Files - uploadFile::should accept a file at the 64 MiB size limit", Coverage = UpstreamCoverage.Covered)]
    public void The_size_limit_is_accepted()
    {
        DeepSeekFileStore.Validate(DeepSeekFileStore.MaxBytes, "image.png", "image/png", Png);
    }

    [Fact]
    [UpstreamTest("packages/deepseek/src/files/deepseek-files.test.ts::DeepSeek Files - uploadFile::should accept a filename at the 512-character limit", Coverage = UpstreamCoverage.Covered)]
    public async Task A_512_character_filename_is_uploaded()
    {
        var name = new string('a', 508) + ".png";
        var capture = Ok("file_1", name);
        var file = await Store(capture).UploadFileAsync(name, Png, "image/png", CancellationToken.None);
        Assert.Equal(512, name.Length);
        Assert.Equal("file_1", file.Id);
        Assert.Single(capture.Requests);
    }

    [Fact]
    [UpstreamTest("packages/deepseek/src/files/deepseek-files.test.ts::DeepSeek Files - uploadFile::should reject $name without making a fetch call", Coverage = UpstreamCoverage.Covered)]
    public async Task Invalid_uploads_are_rejected_before_a_request()
    {
        var capture = Ok("file_1", "photo.png");
        var store = Store(capture);
        var plain = await Assert.ThrowsAsync<AiSdkException>(() => store.UploadFileAsync("notes.txt", new byte[] { 1, 2, 3 }, "text/plain", CancellationToken.None));
        Assert.Contains("Received unsupported media type \"text/plain\".", plain.Message);
        var pdf = await Assert.ThrowsAsync<AiSdkException>(() => store.UploadFileAsync("image.png", new byte[] { 0x25, 0x50, 0x44, 0x46 }, "image/png", CancellationToken.None));
        Assert.Contains("Detected unsupported file content type \"application/pdf\".", pdf.Message);
        var longName = await Assert.ThrowsAsync<AiSdkException>(() => store.UploadFileAsync(new string('a', 509) + ".png", Png, "image/png", CancellationToken.None));
        Assert.Contains("Received 513 characters.", longName.Message);
        var generic = await Assert.ThrowsAsync<AiSdkException>(() => store.UploadFileAsync(null!, new byte[] { 1, 2, 3 }, "application/octet-stream", CancellationToken.None));
        Assert.Contains("Provide a supported media type or a filename ending in .jpg, .jpeg, .png, .gif, or .webp.", generic.Message);
        var oversized = Assert.Throws<AiSdkException>(() => DeepSeekFileStore.Validate(DeepSeekFileStore.MaxBytes + 1, "image.png", "image/png", Png));
        Assert.Contains("Received 67,108,865 bytes.", oversized.Message);
        Assert.Empty(capture.Requests);
    }

    private static DeepSeekFileStore Store(UpstreamCapture capture)
    {
        return (DeepSeekFileStore)DeepSeekProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).FileStore();
    }

    private static UpstreamCapture FileResponse(string? body = null)
    {
        return new UpstreamCapture { ResponseBody = body ?? "{\"id\":\"file-api-abc123\",\"object\":\"file\",\"bytes\":1024,\"created_at\":1700000000,\"filename\":\"image.png\",\"purpose\":\"user_data\",\"expires_at\":null}" };
    }

    private static UploadFileCall Call(object data, string mediaType, string? filename, IReadOnlyDictionary<string, string>? headers = null)
    {
        return new UploadFileCall(new UploadData("data", data), mediaType, filename, CancellationToken.None, headers, null);
    }

    private static async Task AssertInvalidResponseField(Task upload, string field)
    {
        var error = await Assert.ThrowsAsync<ApiException>(() => upload);
        Assert.Equal("Invalid JSON response", error.Message);
        var cause = Assert.IsType<TypeValidationException>(error.InnerException);
        Assert.Contains("\"" + field + "\"", cause.Message);
    }

    private static UpstreamCapture Ok(string id, string filename)
    {
        return new UpstreamCapture { ResponseBody = "{\"id\":\"" + id + "\",\"filename\":\"" + filename + "\"}" };
    }
}
