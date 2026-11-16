// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayVideoModel.GatewayVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayVideoModel;

/// <summary>Port of <c>gateway-video-model.test.ts</c> &gt; <c>GatewayVideoModel &gt; doGenerate &gt; image file encoding for image-to-video</c>.</summary>
public sealed class GatewayVideoModelDoGenerateImageFileEncodingForImageToVideoTests
{
    private const string Prefix = "packages/gateway/src/gateway-video-model.test.ts::GatewayVideoModel > doGenerate > image file encoding for image-to-video::";

    [Fact]
    [UpstreamTest(Prefix + "should encode Uint8Array image to base64 string", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_encode_Uint8Array_image_to_base64_string()
    {
        var handler = PrepareJsonResponse();
        var binaryData = new byte[] { 72, 101, 108, 108, 111 }; // "Hello"

        await DoGenerate(CreateTestModel(handler), Call(prompt: "Animate this image", image: VideoModelFile.FromFile(binaryData, "image/png")));

        Assert.True(Equal("{\"type\":\"file\",\"mediaType\":\"image/png\",\"data\":\"SGVsbG8=\"}", Body(handler)!["image"]));
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass through image with string data unchanged", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_through_image_with_string_data_unchanged()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(CreateTestModel(handler), Call(prompt: "Animate this image", image: VideoModelFile.FromBase64("already-base64-encoded", "image/png")));

        Assert.True(Equal("{\"type\":\"file\",\"mediaType\":\"image/png\",\"data\":\"already-base64-encoded\"}", Body(handler)!["image"]));
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass through URL-type image unchanged", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_through_URL_type_image_unchanged()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(CreateTestModel(handler), Call(prompt: "Animate this image", image: VideoModelFile.FromUrl("https://example.com/image.png")));

        Assert.True(Equal("{\"type\":\"url\",\"url\":\"https://example.com/image.png\"}", Body(handler)!["image"]));
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve providerOptions on image during encoding", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_providerOptions_on_image_during_encoding()
    {
        var handler = PrepareJsonResponse();
        var image = VideoModelFile.FromFile(new byte[] { 72, 101, 108, 108, 111 }, "image/png");
        image.ProviderOptions = new Dictionary<string, JsonElement> { ["fal"] = JsonDocument.Parse("{\"enhanceImage\":true}").RootElement.Clone() };

        await DoGenerate(CreateTestModel(handler), Call(prompt: "Animate this image", image: image));

        Assert.True(Equal("{\"type\":\"file\",\"mediaType\":\"image/png\",\"data\":\"SGVsbG8=\",\"providerOptions\":{\"fal\":{\"enhanceImage\":true}}}", Body(handler)!["image"]));
    }
}
