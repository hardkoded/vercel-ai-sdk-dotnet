// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.BlackForestLabs;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.BlackForestLabsImageModel;

public sealed class DoGenerateTests
{
    private const string Prefix = "packages/black-forest-labs/src/black-forest-labs-image-model.test.ts::BlackForestLabsImageModel > doGenerate::";
    private const string PartialNote = "Checks the submit request only. The port does not poll the result or download the image.";

    [Fact]
    [UpstreamTest(Prefix + "passes the correct parameters including aspect ratio and providerOptions", Coverage = UpstreamCoverage.Partial, Note = PartialNote)]
    public async Task Passes_the_correct_parameters_including_aspect_ratio_and_providerOptions()
    {
        var handler = new UpstreamRecordingHandler("{\"id\":\"req-123\"}");
        await Model(handler, "flux-pro-1.1").DoGenerateAsync(
            new ImageCallOptions("A cute baby sea otter")
            {
                AspectRatio = "16:9",
                Seed = 42,
                ProviderOptions = new Dictionary<string, JsonElement> { ["blackForestLabs"] = JsonDocument.Parse("{\"steps\":20,\"guidance\":7.5,\"outputFormat\":\"jpeg\"}").RootElement.Clone() },
            },
            CancellationToken.None);

        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("{\"prompt\":\"A cute baby sea otter\",\"seed\":42,\"aspect_ratio\":\"16:9\",\"steps\":20,\"guidance\":7.5,\"output_format\":\"jpeg\"}"),
            JsonNode.Parse(handler.Body)));
    }

    [Fact]
    [UpstreamTest(Prefix + "uses image field for flux-pro-1.0-fill input images", Coverage = UpstreamCoverage.Partial, Note = PartialNote)]
    public async Task Uses_image_field_for_flux_pro_1_0_fill_input_images()
    {
        var handler = new UpstreamRecordingHandler("{\"id\":\"req-123\"}");
        await Model(handler, "flux-pro-1.0-fill").DoGenerateAsync(
            new ImageCallOptions("Fill the area")
            {
                Files = new[] { ImageModelFile.FromFile(new byte[] { 1, 2, 3 }, "image/png"), ImageModelFile.FromUrl("https://example.com/second.png") },
                Mask = ImageModelFile.FromFile(new byte[] { 4, 5, 6 }, "image/png"),
            },
            CancellationToken.None);

        var body = JsonNode.Parse(handler.Body)!.AsObject();
        Assert.Equal("AQID", (string?)body["image"]);
        Assert.Equal("https://example.com/second.png", (string?)body["image_2"]);
        Assert.Equal("BAUG", (string?)body["mask"]);
        Assert.False(body.ContainsKey("input_image"));
    }

    [Fact]
    [UpstreamTest(Prefix + "uses input_image field for Kontext input images", Coverage = UpstreamCoverage.Partial, Note = PartialNote)]
    public async Task Uses_input_image_field_for_Kontext_input_images()
    {
        var handler = new UpstreamRecordingHandler("{\"id\":\"req-123\"}");
        await Model(handler, "flux-kontext-pro").DoGenerateAsync(
            new ImageCallOptions("Edit the image")
            {
                Files = new[] { ImageModelFile.FromFile(new byte[] { 1, 2, 3 }, "image/png"), ImageModelFile.FromUrl("https://example.com/second.png") },
            },
            CancellationToken.None);

        var body = JsonNode.Parse(handler.Body)!.AsObject();
        Assert.Equal("AQID", (string?)body["input_image"]);
        Assert.Equal("https://example.com/second.png", (string?)body["input_image_2"]);
        Assert.False(body.ContainsKey("image"));
    }

    [Fact]
    [UpstreamTest(Prefix + "merges provider and request headers for submit call", Coverage = UpstreamCoverage.Partial, Note = PartialNote)]
    public async Task Merges_provider_and_request_headers_for_submit_call()
    {
        var handler = new UpstreamRecordingHandler("{\"id\":\"req-123\"}");
        var options = new OpenAICompatibleOptions { ApiKey = "test-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        await BlackForestLabsProvider.Create(options, handler).ImageModel("flux-pro-1.1").DoGenerateAsync(
            new ImageCallOptions("A cute baby sea otter")
            {
                Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" },
            },
            CancellationToken.None);

        Assert.Equal("test-key", handler.Headers["x-key"]);
        Assert.Equal("provider-header-value", handler.Headers["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", handler.Headers["Custom-Request-Header"]);
    }

    private static IImageModel Model(UpstreamRecordingHandler handler, string modelId) =>
        BlackForestLabsProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-key" }, handler).ImageModel(modelId);
}
