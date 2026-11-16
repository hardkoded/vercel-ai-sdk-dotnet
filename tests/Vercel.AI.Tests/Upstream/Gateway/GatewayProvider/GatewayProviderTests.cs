// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>Port of the <c>getAvailableModels</c> tests of <c>gateway-provider.test.ts</c> &gt; <c>GatewayProvider &gt; createGateway</c>.</summary>
public sealed class GatewayProviderTests
{
    private const string Prefix = "packages/gateway/src/gateway-provider.test.ts::GatewayProvider > createGateway";
    private const string OneModel = "{\"models\":[{\"id\":\"test-model\",\"name\":\"Test\",\"specification\":{\"specificationVersion\":\"v4\",\"provider\":\"p\",\"modelId\":\"test-model\"}}]}";

    [Fact]
    [UpstreamTest(Prefix + "::should fetch available models", Coverage = UpstreamCoverage.Partial, Note = "Checks the base URL and the call through the HTTP handler. The upstream test mocks the GatewayFetchMetadata constructor.")]
    public async Task Should_fetch_available_models()
    {
        var server = new GatewayTestServer("{\"models\":[]}");
        var provider = GatewayProvider.Create(new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-api-key" }, server);

        await provider.GetAvailableModelsAsync();

        Assert.Equal("https://api.example.com/config", server.Calls[0].RequestUri!.ToString());
    }

    [Fact]
    [UpstreamTest(Prefix + " > metadata caching::should cache metadata for the specified refresh interval", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_cache_metadata_for_the_specified_refresh_interval()
    {
        var server = new GatewayTestServer(OneModel);
        var currentTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                BaseUrl = "https://api.example.com",
                ApiKey = "test-api-key",
                MetadataCacheRefreshMillis = 10000, // 10 seconds
                CurrentDate = () => currentTime,
            },
            server);

        // First call should fetch metadata
        await provider.GetAvailableModelsAsync();
        Assert.Single(server.Calls);

        // Second immediate call should use cache
        await provider.GetAvailableModelsAsync();
        Assert.Single(server.Calls);

        // Advance time by 9 seconds (should still use cache)
        currentTime = currentTime.AddMilliseconds(9000);
        await provider.GetAvailableModelsAsync();
        Assert.Single(server.Calls);

        // Advance time past 10 seconds (should refresh)
        currentTime = currentTime.AddMilliseconds(2000);
        await provider.GetAvailableModelsAsync();
        Assert.Equal(2, server.Calls.Count);
    }

    [Fact]
    [UpstreamTest(Prefix + " > metadata caching::should use default 5 minute refresh interval when not specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_default_5_minute_refresh_interval_when_not_specified()
    {
        var server = new GatewayTestServer(OneModel);
        var currentTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-api-key", CurrentDate = () => currentTime },
            server);

        // First call should fetch metadata
        await provider.GetAvailableModelsAsync();
        Assert.Single(server.Calls);

        // Advance time by 4 minutes (should still use cache)
        currentTime = currentTime.AddMinutes(4);
        await provider.GetAvailableModelsAsync();
        Assert.Single(server.Calls);

        // Advance time past 5 minutes (should refresh)
        currentTime = currentTime.AddMinutes(2);
        await provider.GetAvailableModelsAsync();
        Assert.Equal(2, server.Calls.Count);
    }
}
