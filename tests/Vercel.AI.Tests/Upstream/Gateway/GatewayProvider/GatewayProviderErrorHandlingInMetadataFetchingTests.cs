// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>Port of <c>gateway-provider.test.ts</c> &gt; <c>GatewayProvider &gt; Error handling in metadata fetching</c> (the metadata fetch tests).</summary>
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
}
