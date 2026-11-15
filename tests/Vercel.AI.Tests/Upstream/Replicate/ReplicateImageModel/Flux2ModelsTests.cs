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

/// <summary>Port of <c>replicate-image-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>Flux-2 Models</c>.</summary>
public sealed class Flux2ModelsTests
{
    private const string Prefix = "packages/replicate/src/replicate-image-model.test.ts::doGenerate > Flux-2 Models::";

    [Fact]
    [UpstreamTest(Prefix + "should report maxImagesPerCall as 8 for Flux-2 models", Coverage = UpstreamCoverage.Covered)]
    public void Flux2_models_accept_eight_images_per_call()
    {
        Assert.Equal(8, Model(Server(), "black-forest-labs/flux-2-pro").MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest(Prefix + "should report maxImagesPerCall as 1 for non-Flux-2 models", Coverage = UpstreamCoverage.Covered)]
    public void Other_models_return_one_image_per_call()
    {
        Assert.Equal(1, Model(Server(), "black-forest-labs/flux-schnell").MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send single image as input_image for Flux-2 models", Coverage = UpstreamCoverage.Covered)]
    public async Task Flux2_sends_one_file_as_input_image()
    {
        var handler = Server();
        await Generate(handler, Call("Generate image in similar style", files: new[] { ImageModelFile.FromUrl("https://example.com/reference.jpg") }), modelId: "black-forest-labs/flux-2-pro");
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"input\":{\"input_image\":\"https://example.com/reference.jpg\",\"num_outputs\":1,\"prompt\":\"Generate image in similar style\"}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send multiple images as input_image, input_image_2, etc. for Flux-2 models", Coverage = UpstreamCoverage.Covered)]
    public async Task Flux2_numbers_extra_input_images()
    {
        var handler = Server();
        var files = new[] { "reference1", "reference2", "reference3" }.Select(name => ImageModelFile.FromUrl("https://example.com/" + name + ".jpg")).ToList();
        await Generate(handler, Call("Combine styles from reference images", files: files), modelId: "black-forest-labs/flux-2-pro");
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"input\":{\"input_image\":\"https://example.com/reference1.jpg\",\"input_image_2\":\"https://example.com/reference2.jpg\",\"input_image_3\":\"https://example.com/reference3.jpg\",\"num_outputs\":1,\"prompt\":\"Combine styles from reference images\"}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn when more than 8 images are provided for Flux-2 models", Coverage = UpstreamCoverage.Covered)]
    public async Task Flux2_warns_and_drops_images_past_eight()
    {
        var handler = Server();
        var files = Enumerable.Range(1, 9).Select(i => ImageModelFile.FromUrl("https://example.com/img" + i + ".jpg")).ToList();
        var result = await Generate(handler, Call("Too many images", files: files), modelId: "black-forest-labs/flux-2-pro");
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("Flux-2 models support up to 8 input images. Additional images are ignored.", warning.Message);
        var input = JsonNode.Parse(handler.Calls[0].Text)!["input"]!.AsObject();
        Assert.Equal("https://example.com/img1.jpg", input["input_image"]!.GetValue<string>());
        Assert.Equal("https://example.com/img8.jpg", input["input_image_8"]!.GetValue<string>());
        Assert.False(input.ContainsKey("input_image_9"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn and ignore mask for Flux-2 models", Coverage = UpstreamCoverage.Covered)]
    public async Task Flux2_warns_and_drops_the_mask()
    {
        var handler = Server();
        var result = await Generate(handler, Call("Edit with mask", files: new[] { ImageModelFile.FromUrl("https://example.com/input.jpg") }, mask: ImageModelFile.FromUrl("https://example.com/mask.png")), modelId: "black-forest-labs/flux-2-pro");
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("Flux-2 models do not support mask input. The mask will be ignored.", warning.Message);
        Assert.False(JsonNode.Parse(handler.Calls[0].Text)!["input"]!.AsObject().ContainsKey("mask"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should call correct URL for Flux-2 models", Coverage = UpstreamCoverage.Covered)]
    public async Task Flux2_posts_to_the_model_predictions_route()
    {
        var handler = Server();
        await Generate(handler, Call("Generate something"), modelId: "black-forest-labs/flux-2-pro");
        Assert.Equal("https://api.replicate.com/v1/models/black-forest-labs/flux-2-pro/predictions", handler.Calls[0].Uri.AbsoluteUri);
    }
}
