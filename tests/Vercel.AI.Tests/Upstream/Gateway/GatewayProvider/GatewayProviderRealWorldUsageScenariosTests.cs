// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>Port of the <c>getAvailableModels</c> tests of <c>gateway-provider.test.ts</c> &gt; <c>Authentication Comprehensive Tests &gt; Real-world usage scenarios</c>.</summary>
[Collection("GatewayAuthEnvironment")]
public sealed class GatewayProviderRealWorldUsageScenariosTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-provider.test.ts::GatewayProvider > Authentication Comprehensive Tests > Real-world usage scenarios::";

    [Fact]
    [UpstreamTest(
        Prefix + "should work in Vercel deployment with OIDC",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads config.headers of the GatewayLanguageModel constructor call. The port reads the provider's headers (getHeaders)."
    )]
    public async Task Should_work_in_Vercel_deployment_with_OIDC()
    {
        using var scope = new GatewayProviderTestScope();

        // Simulate Vercel deployment environment
        scope.SetEnvironment("VERCEL_OIDC_TOKEN", "vercel-deployment-oidc-token");
        scope.SetEnvironment("VERCEL_DEPLOYMENT_ID", "dpl_12345");
        scope.SetEnvironment("VERCEL_ENV", "production");
        scope.SetEnvironment("VERCEL_REGION", "iad1");

        // Explicitly remove AI_GATEWAY_API_KEY to force OIDC usage
        scope.SetEnvironment(GatewayProvider.ApiKeyEnvironmentVariable, null);
        scope.MockOidc("vercel-deployment-oidc-token");
        var provider = GatewayProvider.Create(new GatewayOptions());
        provider.LanguageModel("test-model");

        var headers = await provider.GetHeadersAsync(CancellationToken.None);

        Assert.Equal("Bearer vercel-deployment-oidc-token", headers["authorization"]);
        Assert.Equal("oidc", headers["ai-gateway-auth-method"]);
        Assert.True(scope.OidcCalls > 0);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should work in local development with API key",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_work_in_local_development_with_API_key()
    {
        var previous = Environment.GetEnvironmentVariable(
            GatewayProvider.ApiKeyEnvironmentVariable
        );
        Environment.SetEnvironmentVariable(
            GatewayProvider.ApiKeyEnvironmentVariable,
            "local-dev-api-key"
        );
        var oidcCalls = 0;
        VercelEnvironment.OidcTokenOverride = _ =>
        {
            oidcCalls++;
            return Task.FromResult("unused");
        };
        try
        {
            var provider = GatewayProvider.Create(
                new GatewayOptions(),
                new GatewayTestServer("{\"models\":[]}")
            );

            var models = await provider.GetAvailableModelsAsync();

            Assert.NotNull(models);
            Assert.Equal(0, oidcCalls);
        }
        finally
        {
            Environment.SetEnvironmentVariable(GatewayProvider.ApiKeyEnvironmentVariable, previous);
            VercelEnvironment.OidcTokenOverride = null;
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should work with explicit API key override",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_work_with_explicit_API_key_override()
    {
        var oidcCalls = 0;
        VercelEnvironment.OidcTokenOverride = _ =>
        {
            oidcCalls++;
            return Task.FromResult("should-not-be-used");
        };
        try
        {
            var provider = GatewayProvider.Create(
                new GatewayOptions { ApiKey = "explicit-user-api-key" },
                new GatewayTestServer("{\"models\":[]}")
            );

            var models = await provider.GetAvailableModelsAsync();

            Assert.NotNull(models);
            Assert.Equal(0, oidcCalls);
        }
        finally
        {
            VercelEnvironment.OidcTokenOverride = null;
        }
    }
}
