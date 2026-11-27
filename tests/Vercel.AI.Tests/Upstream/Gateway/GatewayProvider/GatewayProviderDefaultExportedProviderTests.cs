// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>Port of <c>gateway-provider.test.ts</c> &gt; <c>GatewayProvider &gt; default exported provider</c>. The default <c>gateway</c> export maps to <c>GatewayProvider.Create()</c>.</summary>
[Collection("GatewayAuthEnvironment")]
public sealed class GatewayProviderDefaultExportedProviderTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-provider.test.ts::GatewayProvider > default exported provider::";

    private const string DefaultBaseUrl = "https://ai-gateway.vercel.sh/v4/ai";

    [Fact]
    [UpstreamTest(
        Prefix + "should export a default provider instance",
        Coverage = UpstreamCoverage.Covered,
        Note = "The default gateway export maps to GatewayProvider.Create(). The typeof function checks map to the LanguageModel and GetAvailableModelsAsync members."
    )]
    public void Should_export_a_default_provider_instance()
    {
        var gateway = GatewayProvider.Create();

        Assert.NotNull(gateway);
        Assert.NotNull(gateway.LanguageModel("openai/gpt-5"));
        Func<CancellationToken, Task<GatewayFetchMetadataResponse>> getAvailableModels =
            gateway.GetAvailableModelsAsync;
        Assert.NotNull(getAvailableModels);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use the default baseURL when none is provided",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream checks the GatewayFetchMetadata constructor call. The port checks the URL of the metadata request."
    )]
    public async Task Should_use_the_default_baseURL_when_none_is_provided()
    {
        using var scope = new GatewayProviderTestScope();
        var server = new GatewayTestServer("{\"models\":[]}");
        // Provide API key to avoid OIDC token lookup
        var testProvider = GatewayProvider.Create(
            new GatewayOptions { ApiKey = "test-key" },
            server
        );

        await testProvider.GetAvailableModelsAsync();

        Assert.Equal(DefaultBaseUrl + "/config", server.Calls[0].RequestUri!.ToString());
        Assert.Equal(DefaultBaseUrl, testProvider.CreateConfig().BaseUrl);
    }

    [Fact]
    [UpstreamTest(Prefix + "should accept empty options", Coverage = UpstreamCoverage.Covered)]
    public void Should_accept_empty_options()
    {
        // This should not throw an error
        var provider = GatewayProvider.Create();

        Assert.NotNull(provider);
        Assert.NotNull(provider.LanguageModel("openai/gpt-5"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should expose imageModel on the default provider and construct model",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads provider and baseURL from the internal model config. The port asserts the provider id and the URL of the request the model sends."
    )]
    public async Task Should_expose_imageModel_on_the_default_provider_and_construct_model()
    {
        var handler = new UpstreamRecordingHandler("{\"images\":[\"base64-image-1\"]}");
        var gateway = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-key" }, handler);

        var model = Assert.IsType<Vercel.AI.Gateway.GatewayImageModel>(
            gateway.ImageModel("openai/gpt-image-1")
        );
        await GatewayProviderTests.SendImageRequestAsync(model);

        Assert.Equal("gateway", model.Provider);
        Assert.Equal(DefaultBaseUrl + "/image-model", handler.Uri);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should expose videoModel on the default provider and construct model",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads provider and baseURL from the internal model config. The port asserts the provider id and the URL of the request the model sends."
    )]
    public async Task Should_expose_videoModel_on_the_default_provider_and_construct_model()
    {
        var handler = new UpstreamRecordingHandler(
            "data: {\"type\":\"result\",\"videos\":[{\"type\":\"base64\",\"data\":\"base64-video-1\",\"mediaType\":\"video/mp4\"}]}\n\n"
        );
        var gateway = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-key" }, handler);

        var model = Assert.IsType<Vercel.AI.Gateway.GatewayVideoModel>(
            gateway.VideoModel("google/veo-2.0-generate-001")
        );
        await GatewayProviderTests.SendVideoRequestAsync(model);

        Assert.Equal("gateway", model.Provider);
        Assert.Equal(DefaultBaseUrl + "/video-model", handler.Uri);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should expose speechModel on the default provider and construct model",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads provider and baseURL from the internal model config. The port asserts the provider id and the provider's base URL."
    )]
    public void Should_expose_speechModel_on_the_default_provider_and_construct_model()
    {
        var gateway = GatewayProvider.Create();

        var model = Assert.IsType<Vercel.AI.Gateway.GatewaySpeechModel>(
            gateway.SpeechModel("openai/tts-1")
        );

        Assert.Equal("gateway", model.Provider);
        Assert.Equal(DefaultBaseUrl, gateway.CreateConfig().BaseUrl);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should expose transcriptionModel on the default provider and construct model",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads provider and baseURL from the internal model config. The port asserts the provider id and the provider's base URL."
    )]
    public void Should_expose_transcriptionModel_on_the_default_provider_and_construct_model()
    {
        var gateway = GatewayProvider.Create();

        var model = Assert.IsType<Vercel.AI.Gateway.GatewayTranscriptionModel>(
            gateway.TranscriptionModel("openai/gpt-4o-transcribe")
        );

        Assert.Equal("gateway", model.Provider);
        Assert.Equal(DefaultBaseUrl, gateway.CreateConfig().BaseUrl);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should override default baseURL when provided",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream checks the GatewayFetchMetadata constructor call. The port checks the URL of the metadata request."
    )]
    public async Task Should_override_default_baseURL_when_provided()
    {
        using var scope = new GatewayProviderTestScope();
        var server = new GatewayTestServer("{\"models\":[]}");
        var testProvider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://custom-api.example.com", ApiKey = "test-key" },
            server
        );

        await testProvider.GetAvailableModelsAsync();

        Assert.Equal(
            "https://custom-api.example.com/config",
            server.Calls[0].RequestUri!.ToString()
        );
        Assert.Equal("https://custom-api.example.com", testProvider.CreateConfig().BaseUrl);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use apiKey over OIDC token when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_apiKey_over_OIDC_token_when_provided()
    {
        VercelEnvironment.OidcTokenOverride = _ =>
            throw new InvalidOperationException("Should not be called");
        try
        {
            var server = new GatewayTestServer("{\"models\":[]}");
            var provider = GatewayProvider.Create(
                new GatewayOptions { ApiKey = "test-api-key-123" },
                server
            );

            await provider.GetAvailableModelsAsync();

            var headers = server.RequestHeaders();
            Assert.Equal("Bearer test-api-key-123", headers["authorization"]);
            Assert.Equal("api-key", headers["ai-gateway-auth-method"]);
            Assert.StartsWith("ai-sdk-gateway/", headers["user-agent"]);
        }
        finally
        {
            VercelEnvironment.OidcTokenOverride = null;
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use apiKey and teamIdOrSlug for getAvailableModels",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_apiKey_and_teamIdOrSlug_for_getAvailableModels()
    {
        VercelEnvironment.OidcTokenOverride = _ =>
            throw new InvalidOperationException("Should not be called");
        try
        {
            var server = new GatewayTestServer("{\"models\":[]}");
            var provider = GatewayProvider.Create(
                new GatewayOptions { ApiKey = "vca_test-token", TeamIdOrSlug = "vercel" },
                server
            );

            await provider.GetAvailableModelsAsync();

            var headers = server.RequestHeaders();
            Assert.Equal("Bearer vca_test-token", headers["authorization"]);
            Assert.Equal("vercel", headers["x-vercel-ai-gateway-team"]);
        }
        finally
        {
            VercelEnvironment.OidcTokenOverride = null;
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should use OIDC and teamIdOrSlug for getAvailableModels when no apiKey is configured",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_OIDC_and_teamIdOrSlug_for_getAvailableModels_when_no_apiKey_is_configured()
    {
        var oidcCalls = 0;
        VercelEnvironment.OidcTokenOverride = _ =>
        {
            oidcCalls++;
            return Task.FromResult("oidc-token");
        };
        try
        {
            var server = new GatewayTestServer("{\"models\":[]}");
            var provider = GatewayProvider.Create(
                new GatewayOptions { ApiKey = string.Empty, TeamIdOrSlug = "vercel" },
                server
            );

            await provider.GetAvailableModelsAsync();

            var headers = server.RequestHeaders();
            Assert.Equal("Bearer oidc-token", headers["authorization"]);
            Assert.Equal("oidc", headers["ai-gateway-auth-method"]);
            Assert.Equal("vercel", headers["x-vercel-ai-gateway-team"]);
            Assert.True(oidcCalls > 0);
        }
        finally
        {
            VercelEnvironment.OidcTokenOverride = null;
        }
    }
}
