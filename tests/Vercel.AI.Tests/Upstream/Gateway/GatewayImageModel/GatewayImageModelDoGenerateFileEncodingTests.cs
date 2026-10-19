// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayImageModel;

/// <summary>Port of <c>gateway-image-model.test.ts</c> &gt; <c>GatewayImageModel &gt; doGenerate &gt; file encoding</c>.</summary>
public sealed class GatewayImageModelDoGenerateFileEncodingTests
{
    private const string Prefix = "packages/gateway/src/gateway-image-model.test.ts::GatewayImageModel > doGenerate > file encoding::";

    [Fact]
    [UpstreamTest(Prefix + "should encode Uint8Array files to base64 strings", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_encode_Uint8Array_files_to_base64_strings()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse();
        var binaryData = new byte[] { 72, 101, 108, 108, 111 }; // "Hello"

        await GatewayImageModelSupport.DoGenerate(
            GatewayImageModelSupport.CreateTestModel(handler),
            prompt: "Edit this image",
            files: new[] { ImageModelFile.FromFile(binaryData, "image/png") });

        var files = GatewayImageModelSupport.Body(handler)!["files"]!.AsArray();
        Assert.Single(files);
        Assert.True(GatewayImageModelSupport.Equal("{\"type\":\"file\",\"mediaType\":\"image/png\",\"data\":\"SGVsbG8=\"}", files[0]));
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass through files with string data unchanged", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_through_files_with_string_data_unchanged()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse();

        await GatewayImageModelSupport.DoGenerate(
            GatewayImageModelSupport.CreateTestModel(handler),
            prompt: "Edit this image",
            files: new[] { ImageModelFile.FromBase64("already-base64-encoded", "image/png") });

        var files = GatewayImageModelSupport.Body(handler)!["files"]!.AsArray();
        Assert.Single(files);
        Assert.True(GatewayImageModelSupport.Equal("{\"type\":\"file\",\"mediaType\":\"image/png\",\"data\":\"already-base64-encoded\"}", files[0]));
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass through URL-type files unchanged", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_through_URL_type_files_unchanged()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse();

        await GatewayImageModelSupport.DoGenerate(
            GatewayImageModelSupport.CreateTestModel(handler),
            prompt: "Edit this image",
            files: new[] { ImageModelFile.FromUrl("https://example.com/image.png") });

        var files = GatewayImageModelSupport.Body(handler)!["files"]!.AsArray();
        Assert.Single(files);
        Assert.True(GatewayImageModelSupport.Equal("{\"type\":\"url\",\"url\":\"https://example.com/image.png\"}", files[0]));
    }

    [Fact]
    [UpstreamTest(Prefix + "should encode Uint8Array mask to base64 string", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_encode_Uint8Array_mask_to_base64_string()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse();
        var maskData = new byte[] { 255, 0, 255, 0 }; // Simple mask

        await GatewayImageModelSupport.DoGenerate(
            GatewayImageModelSupport.CreateTestModel(handler),
            prompt: "Inpaint this area",
            mask: ImageModelFile.FromFile(maskData, "image/png"));

        Assert.True(GatewayImageModelSupport.Equal("{\"type\":\"file\",\"mediaType\":\"image/png\",\"data\":\"/wD/AA==\"}", GatewayImageModelSupport.Body(handler)!["mask"]));
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle mixed file types with encoding", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_mixed_file_types_with_encoding()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse();

        await GatewayImageModelSupport.DoGenerate(
            GatewayImageModelSupport.CreateTestModel(handler),
            prompt: "Edit these images",
            files: new[]
            {
                ImageModelFile.FromFile(new byte[] { 1, 2, 3 }, "image/png"),
                ImageModelFile.FromBase64("already-encoded", "image/jpeg"),
                ImageModelFile.FromUrl("https://example.com/image.png"),
            },
            mask: ImageModelFile.FromFile(new byte[] { 4, 5, 6 }, "image/png"));

        var requestBody = GatewayImageModelSupport.Body(handler)!;
        var files = requestBody["files"]!.AsArray();
        Assert.Equal(3, files.Count);
        Assert.True(GatewayImageModelSupport.Equal("{\"type\":\"file\",\"mediaType\":\"image/png\",\"data\":\"AQID\"}", files[0]));
        Assert.True(GatewayImageModelSupport.Equal("{\"type\":\"file\",\"mediaType\":\"image/jpeg\",\"data\":\"already-encoded\"}", files[1]));
        Assert.True(GatewayImageModelSupport.Equal("{\"type\":\"url\",\"url\":\"https://example.com/image.png\"}", files[2]));
        Assert.True(GatewayImageModelSupport.Equal("{\"type\":\"file\",\"mediaType\":\"image/png\",\"data\":\"BAUG\"}", requestBody["mask"]));
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve providerOptions on files during encoding", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_providerOptions_on_files_during_encoding()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse();
        var binaryData = new byte[] { 72, 101, 108, 108, 111 };
        var file = ImageModelFile.FromFile(binaryData, "image/png");
        file.ProviderOptions = new Dictionary<string, JsonElement> { ["openai"] = JsonDocument.Parse("{\"quality\":\"hd\"}").RootElement.Clone() };

        await GatewayImageModelSupport.DoGenerate(
            GatewayImageModelSupport.CreateTestModel(handler),
            prompt: "Edit this image",
            files: new[] { file });

        var files = GatewayImageModelSupport.Body(handler)!["files"]!.AsArray();
        Assert.True(GatewayImageModelSupport.Equal(
            "{\"type\":\"file\",\"mediaType\":\"image/png\",\"data\":\"SGVsbG8=\",\"providerOptions\":{\"openai\":{\"quality\":\"hd\"}}}",
            files[0]));
    }
}
