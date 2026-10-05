// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class GatewayEmbeddingParityTests
{
    private static readonly string[] Values = { "sunny day at the beach", "rainy afternoon in the city" };

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-embedding-model.test.ts::GatewayEmbeddingModel > doEmbed::should send value as array", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_values_as_an_array()
    {
        var handler = await Embed(null, null);
        using var document = JsonDocument.Parse(handler.Body);
        Assert.Equal(Values[0], document.RootElement.GetProperty("values")[0].GetString());
        Assert.Equal(Values[1], document.RootElement.GetProperty("values")[1].GetString());
        Assert.False(document.RootElement.TryGetProperty("providerOptions", out _));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-embedding-model.test.ts::GatewayEmbeddingModel > doEmbed::should pass providerOptions into request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_provider_options()
    {
        using var options = JsonDocument.Parse("{\"openai\":{\"dimensions\":64}}");
        var handler = await Embed(options.RootElement.Clone(), null);
        using var document = JsonDocument.Parse(handler.Body);
        Assert.Equal(64, document.RootElement.GetProperty("providerOptions").GetProperty("openai").GetProperty("dimensions").GetInt32());
        Assert.Equal(Values[0], document.RootElement.GetProperty("values")[0].GetString());
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-embedding-model.test.ts::GatewayEmbeddingModel > doEmbed::should not include providerOptions when not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_provider_options_when_they_are_absent()
    {
        var handler = await Embed(null, null);
        using var document = JsonDocument.Parse(handler.Body);
        Assert.False(document.RootElement.TryGetProperty("providerOptions", out _));
        Assert.Equal(2, document.RootElement.GetProperty("values").GetArrayLength());
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-embedding-model.test.ts::GatewayEmbeddingModel > doEmbed::should pass headers correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_embedding_headers()
    {
        var handler = await Embed(null, new Dictionary<string, string?> { ["Custom-Header"] = "test-value" });
        Assert.Equal("Bearer test-token", handler.Headers["Authorization"]);
        Assert.Equal("test-value", handler.Headers["Custom-Header"]);
        Assert.Equal("4", handler.Headers["ai-embedding-model-specification-version"]);
        Assert.Equal("openai/text-embedding-3-small", handler.Headers["ai-model-id"]);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-embedding-model.test.ts::GatewayEmbeddingModel > doEmbed::should convert gateway error responses", Coverage = UpstreamCoverage.Covered)]
    public async Task Converts_gateway_error_responses()
    {
        var invalid = await Assert.ThrowsAsync<GatewayInvalidRequestError>(() => EmbedError(HttpStatusCode.BadRequest, "{\"error\":{\"message\":\"Invalid input\",\"type\":\"invalid_request_error\"}}"));
        Assert.Equal(400, invalid.StatusCode);

        var server = await Assert.ThrowsAsync<GatewayInternalServerError>(() => EmbedError(HttpStatusCode.InternalServerError, "{\"error\":{\"message\":\"Server blew up\",\"type\":\"internal_server_error\"}}"));
        Assert.Equal(500, server.StatusCode);
    }

    private static async Task<UpstreamRecordingHandler> Embed(JsonElement? providerOptions, IReadOnlyDictionary<string, string?>? headers)
    {
        var handler = new UpstreamRecordingHandler("{\"embeddings\":[[0.1,0.2,0.3],[0.4,0.5,0.6]],\"usage\":{\"tokens\":8}}");
        var model = Model(handler);
        await model.DoEmbedAsync(Values, providerOptions, headers, CancellationToken.None);
        return handler;
    }

    private static async Task EmbedError(HttpStatusCode status, string body)
    {
        var handler = new UpstreamRecordingHandler(body, status);
        await Model(handler).DoEmbedAsync(Values, CancellationToken.None);
    }

    private static GatewayEmbeddingModel Model(HttpMessageHandler handler)
    {
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" }, handler);
        return (GatewayEmbeddingModel)provider.EmbeddingModel("openai/text-embedding-3-small");
    }
}

public sealed class GatewayImageParityTests
{
    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-image-model.test.ts::GatewayImageModel > doGenerate::should send correct request headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_image_headers()
    {
        var handler = new UpstreamRecordingHandler("{\"images\":[]}");
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" }, handler);
        await provider.ImageModel("openai/gpt-image-1").DoGenerateAsync(new ImageCallOptions("A beautiful sunset over mountains"), CancellationToken.None);
        Assert.Equal("Bearer test-token", handler.Headers["Authorization"]);
        Assert.Equal("4", handler.Headers["ai-image-model-specification-version"]);
        Assert.Equal("openai/gpt-image-1", handler.Headers["ai-model-id"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/gateway/src/gateway-image-model.test.ts::GatewayImageModel > doGenerate::should omit optional parameters when not provided",
        Coverage = UpstreamCoverage.Partial,
        Note = "Size, aspect ratio, and seed are omitted. Image call options have no providerOptions object to send.")]
    public async Task Omits_unset_image_parameters()
    {
        var handler = new UpstreamRecordingHandler("{\"images\":[]}");
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" }, handler);
        await provider.ImageModel("image").DoGenerateAsync(new ImageCallOptions("A simple prompt"), CancellationToken.None);
        using var document = JsonDocument.Parse(handler.Body);
        Assert.Equal("A simple prompt", document.RootElement.GetProperty("prompt").GetString());
        Assert.Equal(1, document.RootElement.GetProperty("n").GetInt32());
        Assert.False(document.RootElement.TryGetProperty("size", out _));
        Assert.False(document.RootElement.TryGetProperty("aspectRatio", out _));
        Assert.False(document.RootElement.TryGetProperty("seed", out _));
    }

    [Fact]
    [UpstreamTest(
        "packages/gateway/src/gateway-image-model.test.ts::GatewayImageModel > doGenerate::should return images array correctly",
        Coverage = UpstreamCoverage.Partial,
        Note = "Base64 image strings are decoded to bytes. The upstream result keeps the original strings.")]
    public async Task Decodes_returned_images()
    {
        var handler = new UpstreamRecordingHandler("{\"images\":[\"AQID\",\"BAUG\"]}");
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" }, handler);
        var result = await provider.ImageModel("image").DoGenerateAsync(new ImageCallOptions("Test prompt") { Count = 2 }, CancellationToken.None);
        Assert.Equal(new byte[] { 1, 2, 3 }, result.Images[0].Data);
        Assert.Equal(new byte[] { 4, 5, 6 }, result.Images[1].Data);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-image-model.test.ts::GatewayImageModel > doGenerate::should handle API errors correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_for_an_image_api_error()
    {
        var handler = new UpstreamRecordingHandler("{\"error\":{\"message\":\"Invalid request\",\"code\":\"invalid_request\"}}", HttpStatusCode.BadRequest);
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" }, handler);
        await Assert.ThrowsAnyAsync<GatewayError>(() => provider.ImageModel("image").DoGenerateAsync(new ImageCallOptions("Test prompt"), CancellationToken.None));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-image-model.test.ts::GatewayImageModel > doGenerate::should handle authentication errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_for_an_image_authentication_error()
    {
        var handler = new UpstreamRecordingHandler("{\"error\":{\"message\":\"Unauthorized\",\"code\":\"unauthorized\"}}", HttpStatusCode.Unauthorized);
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" }, handler);
        await Assert.ThrowsAnyAsync<GatewayError>(() => provider.ImageModel("image").DoGenerateAsync(new ImageCallOptions("Test prompt"), CancellationToken.None));
    }
}

public sealed class GatewayTakoSearchTests
{
    [Fact]
    [UpstreamTest("packages/gateway/src/tool/tako-search.test.ts::takoSearch::describes data surcharge controls in the input schema", Coverage = UpstreamCoverage.Covered)]
    public void Describes_data_surcharge_controls()
    {
        var data = GatewayTakoSearch.InputSchema.GetProperty("properties").GetProperty("sources").GetProperty("properties").GetProperty("data").GetProperty("properties");
        Assert.Contains("data surcharge", data.GetProperty("count").GetProperty("description").GetString());
        Assert.Contains("cards.content.export_pricing", data.GetProperty("include_contents").GetProperty("description").GetString());
        Assert.Contains("per 1,000 exported rows", data.GetProperty("max_rows").GetProperty("description").GetString());
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/tool/tako-search.test.ts::takoSearch::does not declare internal response fields in the output schema", Coverage = UpstreamCoverage.Covered)]
    public void Omits_internal_response_fields()
    {
        var serialized = GatewayTakoSearch.OutputSchema.GetRawText();
        Assert.DoesNotContain("relevance_score", serialized);
        Assert.DoesNotContain("citation_number", serialized);
    }
}
