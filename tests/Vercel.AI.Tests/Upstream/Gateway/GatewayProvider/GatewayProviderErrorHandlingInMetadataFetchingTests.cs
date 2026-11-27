// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>Port of <c>gateway-provider.test.ts</c> &gt; <c>GatewayProvider &gt; Error handling in metadata fetching</c> .</summary>
[Collection("GatewayAuthEnvironment")]
public sealed class GatewayProviderErrorHandlingInMetadataFetchingTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-provider.test.ts::GatewayProvider > Error handling in metadata fetching::";

    private sealed class ThrowingHandler(Exception exception) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) => throw exception;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert metadata fetch errors to Gateway errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_convert_metadata_fetch_errors_to_Gateway_errors()
    {
        var server = new GatewayTestServer(
            "{\"error\":{\"message\":\"Database connection failed\",\"type\":\"internal_server_error\"}}",
            HttpStatusCode.InternalServerError
        );
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-key" },
            server
        );

        var error = await Assert.ThrowsAsync<GatewayInternalServerError>(() =>
            provider.GetAvailableModelsAsync()
        );

        Assert.Equal("Database connection failed", error.Message);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not double-wrap Gateway errors from metadata fetch",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_double_wrap_Gateway_errors_from_metadata_fetch()
    {
        var originalError = new GatewayAuthenticationError("Invalid token", 401);
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-key" },
            new ThrowingHandler(originalError)
        );

        var error = await Assert.ThrowsAsync<GatewayAuthenticationError>(() =>
            provider.GetAvailableModelsAsync()
        );

        Assert.Same(originalError, error);
        Assert.Equal("Invalid token", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle model specification errors",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream checks the GatewayLanguageModel constructor call. The port checks the provider id and the provider's base URL."
    )]
    public async Task Should_handle_model_specification_errors()
    {
        using var scope = new GatewayProviderTestScope();
        // Mock successful metadata fetch with a model
        var server = new GatewayTestServer(
            "{\"models\":[{\"id\":\"test-model\",\"name\":\"Test\",\"specification\":{\"provider\":\"test\",\"specificationVersion\":\"v2\",\"modelId\":\"test-model\"}}]}"
        );
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-key" },
            server
        );

        // Create a language model that should work
        var model = provider.LanguageModel("test-model");

        Assert.NotNull(model);
        Assert.Equal("test-model", model.ModelId);
        Assert.Equal("gateway", model.Provider);
        Assert.Equal("https://api.example.com", provider.CreateConfig().BaseUrl);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should create language model for any modelId",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream checks the GatewayLanguageModel constructor call. The port checks the provider id and the provider's base URL."
    )]
    public void Should_create_language_model_for_any_modelId()
    {
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-key" },
            new GatewayTestServer(
                "{\"models\":[{\"id\":\"model-1\",\"name\":\"Model 1\",\"specification\":{\"provider\":\"test\",\"specificationVersion\":\"v2\",\"modelId\":\"model-1\"}},{\"id\":\"model-2\",\"name\":\"Model 2\",\"specification\":{\"provider\":\"test\",\"specificationVersion\":\"v2\",\"modelId\":\"model-2\"}}]}"
            )
        );

        // Create a language model for any model ID
        var model = provider.LanguageModel("any-model-id");

        // The model should be created successfully
        Assert.NotNull(model);
        Assert.Equal("any-model-id", model.ModelId);
        Assert.Equal("gateway", model.Provider);
        Assert.Equal("https://api.example.com", provider.CreateConfig().BaseUrl);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle non-existent model requests",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream checks the GatewayLanguageModel constructor call. The port checks the provider id and the provider's base URL."
    )]
    public void Should_handle_non_existent_model_requests()
    {
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-key" }
        );

        // The model should be created successfully (validation happens at API call time)
        var model = provider.LanguageModel("non-existent-model");

        Assert.NotNull(model);
        Assert.Equal("non-existent-model", model.ModelId);
        Assert.Equal("gateway", model.Provider);
        Assert.Equal("https://api.example.com", provider.CreateConfig().BaseUrl);
    }
}
