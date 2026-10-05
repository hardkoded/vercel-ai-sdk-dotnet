// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.DeepSeek;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class DeepSeekUpstreamTests
{
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
        var capture = new UpstreamCapture { ResponseBody = "{}" };
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Store(capture).UploadFileAsync("photo.png", Png, "image/png", CancellationToken.None));
        Assert.Equal("DeepSeek file response did not contain an id.", error.Message);
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

    private static UpstreamCapture Ok(string id, string filename)
    {
        return new UpstreamCapture { ResponseBody = "{\"id\":\"" + id + "\",\"filename\":\"" + filename + "\"}" };
    }
}
