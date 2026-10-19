// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GatewayImageModel;

public sealed class DoGenerateFileEncodingTests
{
    private const string Prefix = "packages/gateway/src/gateway-image-model.test.ts::GatewayImageModel > doGenerate > file encoding::";

    [Fact]
    [UpstreamTest(Prefix + "should encode Uint8Array files to base64 strings", Coverage = UpstreamCoverage.Covered)]
    public async Task Encodes_Uint8Array_files_to_base64_strings()
    {
        var handler = await GatewayImageModelFixture.DoGenerateAsync(new ImageCallOptions("Edit this image")
        {
            Files = new[] { ImageModelFile.FromFile(new byte[] { 72, 101, 108, 108, 111 }, "image/png") },
        });
        var files = GatewayImageModelFixture.RequestBody(handler)["files"]!.AsArray();
        Assert.Single(files);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"file\",\"mediaType\":\"image/png\",\"data\":\"SGVsbG8=\"}"), files[0]));
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass through URL-type files unchanged", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_through_URL_type_files_unchanged()
    {
        var handler = await GatewayImageModelFixture.DoGenerateAsync(new ImageCallOptions("Edit this image")
        {
            Files = new[] { ImageModelFile.FromUrl("https://example.com/image.png") },
        });
        var files = GatewayImageModelFixture.RequestBody(handler)["files"]!.AsArray();
        Assert.Single(files);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"url\",\"url\":\"https://example.com/image.png\"}"), files[0]));
    }

    [Fact]
    [UpstreamTest(Prefix + "should encode Uint8Array mask to base64 string", Coverage = UpstreamCoverage.Covered)]
    public async Task Encodes_Uint8Array_mask_to_base64_string()
    {
        var handler = await GatewayImageModelFixture.DoGenerateAsync(new ImageCallOptions("Inpaint this area")
        {
            Mask = ImageModelFile.FromFile(new byte[] { 255, 0, 255, 0 }, "image/png"),
        });
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("{\"type\":\"file\",\"mediaType\":\"image/png\",\"data\":\"/wD/AA==\"}"),
            GatewayImageModelFixture.RequestBody(handler)["mask"]));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle mixed file types with encoding",
        Coverage = UpstreamCoverage.Partial,
        Note = "ImageModelFile holds bytes or a URL, not a base64 string. The file with string data is left out.")]
    public async Task Handles_mixed_file_types_with_encoding()
    {
        var handler = await GatewayImageModelFixture.DoGenerateAsync(new ImageCallOptions("Edit these images")
        {
            Files = new[]
            {
                ImageModelFile.FromFile(new byte[] { 1, 2, 3 }, "image/png"),
                ImageModelFile.FromUrl("https://example.com/image.png"),
            },
            Mask = ImageModelFile.FromFile(new byte[] { 4, 5, 6 }, "image/png"),
        });
        var body = GatewayImageModelFixture.RequestBody(handler);
        var files = body["files"]!.AsArray();
        Assert.Equal(2, files.Count);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"file\",\"mediaType\":\"image/png\",\"data\":\"AQID\"}"), files[0]));
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"url\",\"url\":\"https://example.com/image.png\"}"), files[1]));
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"file\",\"mediaType\":\"image/png\",\"data\":\"BAUG\"}"), body["mask"]));
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve providerOptions on files during encoding", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_provider_options_on_files_during_encoding()
    {
        var file = ImageModelFile.FromFile(new byte[] { 72, 101, 108, 108, 111 }, "image/png");
        file.ProviderOptions = new Dictionary<string, JsonElement> { ["openai"] = GatewayImageModelFixture.Json("{\"quality\":\"hd\"}") };
        var handler = await GatewayImageModelFixture.DoGenerateAsync(new ImageCallOptions("Edit this image") { Files = new[] { file } });
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("{\"type\":\"file\",\"mediaType\":\"image/png\",\"data\":\"SGVsbG8=\",\"providerOptions\":{\"openai\":{\"quality\":\"hd\"}}}"),
            GatewayImageModelFixture.RequestBody(handler)["files"]![0]));
    }
}
