// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GatewayImageModel;

public sealed class DoGenerateTests
{
    private const string Prefix = "packages/gateway/src/gateway-image-model.test.ts::GatewayImageModel > doGenerate::";

    [Fact]
    [UpstreamTest(Prefix + "should send correct request headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_correct_request_headers()
    {
        var handler = await GatewayImageModelFixture.DoGenerateAsync(new ImageCallOptions("A beautiful sunset over mountains"));
        Assert.Equal("Bearer test-token", handler.Headers["Authorization"]);
        Assert.Equal("4", handler.Headers["ai-image-model-specification-version"]);
        Assert.Equal(GatewayImageModelFixture.ModelId, handler.Headers["ai-model-id"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send correct request body with all parameters", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_correct_request_body_with_all_parameters()
    {
        var handler = await GatewayImageModelFixture.DoGenerateAsync(new ImageCallOptions("A cat playing piano")
        {
            Count = 2,
            Size = "1024x1024",
            AspectRatio = "16:9",
            Seed = 42,
            ProviderOptions = new Dictionary<string, JsonElement> { ["vertex"] = GatewayImageModelFixture.Json("{\"safetySettings\":\"block_none\"}") },
        });
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("{\"prompt\":\"A cat playing piano\",\"n\":2,\"size\":\"1024x1024\",\"aspectRatio\":\"16:9\",\"seed\":42,\"providerOptions\":{\"vertex\":{\"safetySettings\":\"block_none\"}}}"),
            GatewayImageModelFixture.RequestBody(handler)));
    }

    [Fact]
    [UpstreamTest(Prefix + "should omit optional parameters when not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_optional_parameters_when_not_provided()
    {
        var handler = await GatewayImageModelFixture.DoGenerateAsync(new ImageCallOptions("A simple prompt") { ProviderOptions = new Dictionary<string, JsonElement>() });
        var body = GatewayImageModelFixture.RequestBody(handler);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"prompt\":\"A simple prompt\",\"n\":1,\"providerOptions\":{}}"), body));
        Assert.False(body.ContainsKey("size"));
        Assert.False(body.ContainsKey("aspectRatio"));
        Assert.False(body.ContainsKey("seed"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return images array correctly",
        Coverage = UpstreamCoverage.Partial,
        Note = "Base64 image strings are decoded to bytes. The upstream result keeps the original strings.")]
    public async Task Returns_images_array_correctly()
    {
        var handler = new UpstreamRecordingHandler("{\"images\":[\"AQID\",\"BAUG\"]}");
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" }, handler);
        var result = await provider.ImageModel("image").DoGenerateAsync(new ImageCallOptions("Test prompt") { Count = 2 }, CancellationToken.None);
        Assert.Equal(new byte[] { 1, 2, 3 }, result.Images[0].Data);
        Assert.Equal(new byte[] { 4, 5, 6 }, result.Images[1].Data);
    }

    [Fact]
    [UpstreamTest(Prefix + "should merge custom headers with config headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Merges_custom_headers_with_config_headers()
    {
        var handler = await GatewayImageModelFixture.DoGenerateAsync(new ImageCallOptions("Test prompt")
        {
            Headers = new Dictionary<string, string?> { ["X-Custom-Header"] = "custom-value" },
        });
        Assert.Equal("Bearer test-token", handler.Headers["Authorization"]);
        Assert.Equal("custom-value", handler.Headers["X-Custom-Header"]);
        Assert.Equal("4", handler.Headers["ai-image-model-specification-version"]);
        Assert.Equal(GatewayImageModelFixture.ModelId, handler.Headers["ai-model-id"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle API errors correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_api_errors_correctly()
    {
        var handler = new UpstreamRecordingHandler("{\"error\":{\"message\":\"Invalid request\",\"code\":\"invalid_request\"}}", HttpStatusCode.BadRequest);
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" }, handler);
        await Assert.ThrowsAnyAsync<GatewayError>(() => provider.ImageModel("image").DoGenerateAsync(new ImageCallOptions("Test prompt"), CancellationToken.None));
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle authentication errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_authentication_errors()
    {
        var handler = new UpstreamRecordingHandler("{\"error\":{\"message\":\"Unauthorized\",\"code\":\"unauthorized\"}}", HttpStatusCode.Unauthorized);
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" }, handler);
        await Assert.ThrowsAnyAsync<GatewayError>(() => provider.ImageModel("image").DoGenerateAsync(new ImageCallOptions("Test prompt"), CancellationToken.None));
    }

    [Fact]
    [UpstreamTest(Prefix + "should include providerOptions object in request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_provider_options_object_in_request_body()
    {
        var handler = await GatewayImageModelFixture.DoGenerateAsync(new ImageCallOptions("Test prompt")
        {
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["vertex"] = GatewayImageModelFixture.Json("{\"safetySettings\":\"block_none\"}"),
                ["openai"] = GatewayImageModelFixture.Json("{\"style\":\"vivid\"}"),
            },
        });
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("{\"prompt\":\"Test prompt\",\"n\":1,\"providerOptions\":{\"vertex\":{\"safetySettings\":\"block_none\"},\"openai\":{\"style\":\"vivid\"}}}"),
            GatewayImageModelFixture.RequestBody(handler)));
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle empty provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_empty_provider_options()
    {
        var handler = await GatewayImageModelFixture.DoGenerateAsync(new ImageCallOptions("Test prompt") { ProviderOptions = new Dictionary<string, JsonElement>() });
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("{\"prompt\":\"Test prompt\",\"n\":1,\"providerOptions\":{}}"),
            GatewayImageModelFixture.RequestBody(handler)));
    }
}
