// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>Port of <c>gateway-provider.test.ts</c> &gt; <c>GatewayProvider &gt; Authentication Comprehensive Tests &gt; Authentication precedence</c>.</summary>
[Collection("GatewayAuthEnvironment")]
public sealed class GatewayProviderAuthenticationPrecedenceTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-provider.test.ts::GatewayProvider > Authentication Comprehensive Tests > Authentication precedence::";

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer options.apiKey over AI_GATEWAY_API_KEY",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_options_apiKey_over_AI_GATEWAY_API_KEY()
    {
        using var scope = new GatewayProviderTestScope();
        scope.SetEnvironment(GatewayProvider.ApiKeyEnvironmentVariable, "env-api-key");
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "options-api-key" });

        var result = await provider.GetGatewayAuthTokenAsync(CancellationToken.None);

        Assert.Equal("api-key", result.AuthMethod);
        Assert.Equal("options-api-key", result.Token);
        Assert.Equal(0, scope.OidcCalls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer AI_GATEWAY_API_KEY over OIDC token",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_AI_GATEWAY_API_KEY_over_OIDC_token()
    {
        using var scope = new GatewayProviderTestScope();
        scope.SetEnvironment("VERCEL_OIDC_TOKEN", "oidc-token");
        scope.SetEnvironment(GatewayProvider.ApiKeyEnvironmentVariable, "env-api-key");
        var provider = GatewayProvider.Create(new GatewayOptions());

        var result = await provider.GetGatewayAuthTokenAsync(CancellationToken.None);

        Assert.Equal("api-key", result.AuthMethod);
        Assert.Equal("env-api-key", result.Token);
        Assert.Equal(0, scope.OidcCalls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should fall back to OIDC when no API keys are available",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_fall_back_to_OIDC_when_no_API_keys_are_available()
    {
        using var scope = new GatewayProviderTestScope();
        scope.SetEnvironment("VERCEL_OIDC_TOKEN", "oidc-token");
        scope.MockOidc("oidc-token");
        var provider = GatewayProvider.Create(new GatewayOptions());

        var result = await provider.GetGatewayAuthTokenAsync(CancellationToken.None);

        Assert.Equal("oidc", result.AuthMethod);
        Assert.Equal("oidc-token", result.Token);
        Assert.True(scope.OidcCalls > 0);
    }
}
