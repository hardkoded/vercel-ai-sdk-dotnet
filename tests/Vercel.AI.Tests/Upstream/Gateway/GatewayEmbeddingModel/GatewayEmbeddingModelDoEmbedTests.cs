// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayEmbeddingModel;

/// <summary>Port of <c>gateway-embedding-model.test.ts</c> &gt; <c>GatewayEmbeddingModel &gt; doEmbed</c>.</summary>
public sealed class GatewayEmbeddingModelDoEmbedTests
{
    private const string Prefix = "packages/gateway/src/gateway-embedding-model.test.ts::GatewayEmbeddingModel > doEmbed::";

    private static readonly string[] Values = { "sunny day at the beach", "rainy afternoon in the city" };

    [Fact]
    [UpstreamTest(Prefix + "should forward dimensions alongside provider options for server-side resolution", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_dimensions_alongside_provider_options_for_server_side_resolution()
    {
        var (model, capture) = Setup();

        await model.DoEmbedAsync(Values, JsonSerializer.Deserialize<JsonElement>("{\"openai\":{\"dimensions\":512}}"), null, CancellationToken.None, 256);

        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("{\"values\":[\"sunny day at the beach\",\"rainy afternoon in the city\"],\"dimensions\":256,\"providerOptions\":{\"openai\":{\"dimensions\":512}}}"),
            capture.Body));
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward dimensions without provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_dimensions_without_provider_options()
    {
        var (model, capture) = Setup();

        await ((IEmbeddingModel)model).DoEmbedAsync(Values, null, CancellationToken.None, 256);

        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("{\"values\":[\"sunny day at the beach\",\"rainy afternoon in the city\"],\"dimensions\":256}"),
            capture.Body));
    }

    private static (Vercel.AI.Gateway.GatewayEmbeddingModel Model, EmbeddingCapture Capture) Setup()
    {
        var capture = new EmbeddingCapture("{\"embeddings\":[[0.1,0.2,0.3],[0.4,0.5,0.6]],\"usage\":{\"tokens\":8}}");
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" }, capture);
        return ((Vercel.AI.Gateway.GatewayEmbeddingModel)provider.EmbeddingModel("openai/text-embedding-3-small"), capture);
    }
}
