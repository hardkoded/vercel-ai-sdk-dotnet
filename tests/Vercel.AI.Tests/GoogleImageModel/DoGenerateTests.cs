// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleImageModel;

public sealed class DoGenerateTests
{
    private const string Prefix = "packages/google/src/google-image-model.test.ts::GoogleImageModel > doGenerate::";

    [Fact]
    [UpstreamTest(Prefix + "should reject non-Gemini model IDs before sending a request", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_non_Gemini_model_IDs_before_sending_a_request()
    {
        var handler = new RecordingHandler();
        var error = await Assert.ThrowsAsync<AiSdkException>(() => new Vercel.AI.Google.GoogleImageModel(GoogleProvider.Create(GoogleImageModelFixture.Key(), handler), "imagen-3.0-generate-002").DoGenerateAsync(new ImageCallOptions("cat"), CancellationToken.None));
        Assert.Contains("gemini-", error.Message);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use the language model endpoint and extract generated images", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_language_model_endpoint_and_extracts_generated_images()
    {
        var handler = new RecordingHandler { ResponseText = GoogleImageModelFixture.ImageResponse };
        var result = await GoogleImageModelFixture.Model(handler).DoGenerateAsync(new ImageCallOptions("cat"), CancellationToken.None);
        Assert.Contains(":generateContent", handler.Uris[0]);
        Assert.Equal(new byte[] { 1, 2, 3 }, result.Images[0].Data);
        Assert.Equal("image/png", result.Images[0].MediaType);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send response modalities, aspect ratio, seed, and headers",
        Coverage = UpstreamCoverage.Partial,
        Note = "The test checks that the provider and request headers arrive. It does not check the exact header set, because the port also sends its API key and user agent.")]
    public async Task Sends_response_modalities_aspect_ratio_seed_and_headers()
    {
        var handler = new RecordingHandler { ResponseText = GoogleImageModelFixture.ImageResponse };
        var options = GoogleImageModelFixture.Key();
        options.Headers["Custom-Provider-Header"] = "provider-header-value";

        await GoogleImageModelFixture.Model(handler, options).DoGenerateAsync(
            new ImageCallOptions("A beautiful sunset")
            {
                AspectRatio = "21:9",
                Seed = 12345,
                ProviderOptions = new Dictionary<string, JsonElement> { ["google"] = JsonDocument.Parse("{\"imageConfig\":{\"imageSize\":\"4K\"}}").RootElement.Clone() },
                Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" },
            },
            CancellationToken.None);

        Assert.Equal("application/json", handler.RequestHeaders["Content-Type"].Split(';')[0]);
        Assert.Equal("provider-header-value", handler.RequestHeaders["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", handler.RequestHeaders["Custom-Request-Header"]);
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("{\"contents\":[{\"parts\":[{\"text\":\"A beautiful sunset\"}],\"role\":\"user\"}],\"generationConfig\":{\"imageConfig\":{\"aspectRatio\":\"21:9\",\"imageSize\":\"4K\"},\"responseModalities\":[\"IMAGE\"],\"seed\":12345}}"),
            GoogleImageModelFixture.RequestBody(handler)));
    }

    [Fact]
    [UpstreamTest(Prefix + "should return a warning for the unsupported size option", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_a_warning_for_the_unsupported_size_option()
    {
        var handler = new RecordingHandler { ResponseText = "{\"candidates\":[]}" };
        var result = await GoogleImageModelFixture.Model(handler).DoGenerateAsync(new ImageCallOptions("cat") { Size = "1024x1024" }, CancellationToken.None);
        var image = Assert.IsType<GoogleImageResult>(result);
        Assert.Contains(image.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "size");
    }

    [Fact]
    [UpstreamTest(Prefix + "should include input images for editing", Coverage = UpstreamCoverage.Partial, Note = "The port has no base64-string file data, so the input image is passed as bytes.")]
    public async Task Includes_input_images_for_editing()
    {
        var handler = new RecordingHandler { ResponseText = GoogleImageModelFixture.ImageResponse };
        await GoogleImageModelFixture.Model(handler).DoGenerateAsync(
            new ImageCallOptions("Add a hat to this cat") { Files = new[] { ImageModelFile.FromFile(new byte[] { 1, 2, 3 }, "image/png") } },
            CancellationToken.None);

        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("[{\"role\":\"user\",\"parts\":[{\"text\":\"Add a hat to this cat\"},{\"inlineData\":{\"mimeType\":\"image/png\",\"data\":\"AQID\"}}]}]"),
            GoogleImageModelFixture.RequestBody(handler)["contents"]));
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject unsupported URL editing input and masks", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unsupported_URL_editing_input_and_masks()
    {
        var handler = new RecordingHandler { ResponseText = GoogleImageModelFixture.ImageResponse };
        var model = GoogleImageModelFixture.Model(handler);

        var url = await Assert.ThrowsAsync<AiSdkException>(() => model.DoGenerateAsync(
            new ImageCallOptions("Add a hat to this cat") { Files = new[] { ImageModelFile.FromUrl("https://example.com/cat.png") } },
            CancellationToken.None));
        Assert.Matches("media type \"image/\\*\".*not passed as inline bytes", url.Message);

        var mask = await Assert.ThrowsAsync<AiSdkException>(() => model.DoGenerateAsync(
            new ImageCallOptions("Edit this image") { Mask = ImageModelFile.FromFile(new byte[] { 1 }, "image/png") },
            CancellationToken.None));
        Assert.Equal("Gemini image models do not support mask-based image editing.", mask.Message);
        Assert.Equal(0, handler.Calls);
    }
}
