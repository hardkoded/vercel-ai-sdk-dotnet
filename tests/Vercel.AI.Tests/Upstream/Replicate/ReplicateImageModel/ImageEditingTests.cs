// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Replicate;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.Upstream.Replicate.ReplicateImageModel.ReplicateImageModelSupport;
using ImageModel = Vercel.AI.Replicate.ReplicateImageModel;

namespace Vercel.AI.Tests.Upstream.Replicate.ReplicateImageModel;

/// <summary>Port of <c>replicate-image-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>Image Editing</c>.</summary>
public sealed class ImageEditingTests
{
    private const string Prefix = "packages/replicate/src/replicate-image-model.test.ts::doGenerate > Image Editing::";

    [Fact]
    [UpstreamTest(Prefix + "should send image when URL file is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task A_url_file_is_sent_as_image()
    {
        var handler = Server();
        await Generate(handler, Call("Add a hat to the person", files: new[] { ImageModelFile.FromUrl("https://example.com/input.jpg") }));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"input\":{\"image\":\"https://example.com/input.jpg\",\"num_outputs\":1,\"prompt\":\"Add a hat to the person\"}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert Uint8Array file to data URI", Coverage = UpstreamCoverage.Covered)]
    public async Task A_byte_file_is_sent_as_a_data_uri()
    {
        var handler = Server();
        await Generate(handler, Call("Transform this image", files: new[] { ImageModelFile.FromFile(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, "image/png") }));
        var input = JsonNode.Parse(handler.Calls[0].Text)!["input"]!;
        Assert.StartsWith("data:image/png;base64,", input["image"]!.GetValue<string>(), StringComparison.Ordinal);
        Assert.Equal("Transform this image", input["prompt"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Prefix + "should send mask for inpainting", Coverage = UpstreamCoverage.Covered)]
    public async Task A_mask_is_sent()
    {
        var handler = Server();
        await Generate(handler, Call("Replace the masked area with a tree", files: new[] { ImageModelFile.FromUrl("https://example.com/input.jpg") }, mask: ImageModelFile.FromUrl("https://example.com/mask.png")));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"input\":{\"image\":\"https://example.com/input.jpg\",\"mask\":\"https://example.com/mask.png\",\"num_outputs\":1,\"prompt\":\"Replace the masked area with a tree\"}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn when multiple files are provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Extra_files_warn_and_only_the_first_is_sent()
    {
        var handler = Server();
        var result = await Generate(handler, Call("Edit multiple images", files: new[] { ImageModelFile.FromUrl("https://example.com/input1.jpg"), ImageModelFile.FromUrl("https://example.com/input2.jpg") }));
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("This Replicate model only supports a single input image. Additional images are ignored.", warning.Message);
        Assert.Equal("https://example.com/input1.jpg", JsonNode.Parse(handler.Calls[0].Text)!["input"]!["image"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass provider options with image editing", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_options_are_sent_with_an_edit()
    {
        var handler = Server();
        await Generate(handler, Call(
            "Inpaint this area",
            files: new[] { ImageModelFile.FromUrl("https://example.com/input.jpg") },
            mask: ImageModelFile.FromUrl("https://example.com/mask.png"),
            providerOptions: "{\"replicate\":{\"guidance_scale\":7.5,\"num_inference_steps\":30,\"negative_prompt\":\"blurry, low quality\"}}"));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"input\":{\"guidance_scale\":7.5,\"image\":\"https://example.com/input.jpg\",\"mask\":\"https://example.com/mask.png\",\"negative_prompt\":\"blurry, low quality\",\"num_inference_steps\":30,\"num_outputs\":1,\"prompt\":\"Inpaint this area\"}}");
    }
}
