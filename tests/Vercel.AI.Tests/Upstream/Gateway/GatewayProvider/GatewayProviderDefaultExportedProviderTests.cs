// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>Port of the <c>getAvailableModels</c> tests of <c>gateway-provider.test.ts</c> &gt; <c>GatewayProvider &gt; default exported provider</c>.</summary>
public sealed class GatewayProviderDefaultExportedProviderTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-provider.test.ts::GatewayProvider > default exported provider::";

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
