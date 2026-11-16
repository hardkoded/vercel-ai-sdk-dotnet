// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>Port of the <c>getAvailableModels</c> tests of <c>gateway-provider.test.ts</c> &gt; <c>Authentication Comprehensive Tests &gt; Real-world usage scenarios</c>.</summary>
public sealed class GatewayProviderRealWorldUsageScenariosTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-provider.test.ts::GatewayProvider > Authentication Comprehensive Tests > Real-world usage scenarios::";

    [Fact]
    [UpstreamTest(
        Prefix + "should work in local development with API key",
        Coverage = UpstreamCoverage.Partial,
        Note = "Passes the key through GatewayOptions.ApiKey. The upstream test sets the AI_GATEWAY_API_KEY environment variable, which the port does not mutate because tests run in parallel."
    )]
    public async Task Should_work_in_local_development_with_API_key()
    {
        var oidcCalls = 0;
        VercelEnvironment.OidcTokenOverride = _ =>
        {
            oidcCalls++;
            return Task.FromResult("unused");
        };
        try
        {
            var provider = GatewayProvider.Create(
                new GatewayOptions { ApiKey = "local-dev-api-key" },
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
