// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>Port of <c>gateway-provider.test.ts</c> &gt; <c>GatewayProvider &gt; getCredits method</c>.</summary>
public sealed class GatewayProviderGetCreditsMethodTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-provider.test.ts::GatewayProvider > getCredits method::";
    private const string Credits = "{\"balance\":\"150.50\",\"total_used\":\"75.25\"}";

    [Fact]
    [UpstreamTest(
        Prefix + "should fetch credits successfully",
        Coverage = UpstreamCoverage.Covered,
        Note = "The upstream fetch: undefined is the provider's own Http, which the port always sets."
    )]
    public async Task Should_fetch_credits_successfully()
    {
        var server = new GatewayTestServer(Credits);
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-key" }, server);

        var credits = await provider.GetCreditsAsync();

        Assert.Equal("150.50", credits.Balance);
        Assert.Equal("75.25", credits.TotalUsed);
        Assert.Equal(
            "https://ai-gateway.vercel.sh/v1/credits",
            server.Calls[0].RequestUri!.ToString()
        );
        var config = provider.CreateConfig();
        Assert.Equal("https://ai-gateway.vercel.sh/v4/ai", config.BaseUrl);
        Assert.NotNull(config.Headers);
        Assert.Same(provider.Http, config.Http);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass teamIdOrSlug to the credits fetcher via the team header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_teamIdOrSlug_to_the_credits_fetcher_via_the_team_header()
    {
        var server = new GatewayTestServer(Credits);
        var provider = GatewayProvider.Create(
            new GatewayOptions { ApiKey = "vca_test-token", TeamIdOrSlug = "vercel" },
            server
        );

        await provider.GetCreditsAsync();

        var headers = server.RequestHeaders();
        Assert.Equal("Bearer vca_test-token", headers["authorization"]);
        Assert.Equal("vercel", headers["x-vercel-ai-gateway-team"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should work with custom baseURL", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_work_with_custom_baseURL()
    {
        var server = new GatewayTestServer(Credits);
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                ApiKey = "test-key",
                BaseUrl = "https://custom-gateway.example.com/v4/ai",
            },
            server
        );

        await provider.GetCreditsAsync();

        Assert.Equal(
            "https://custom-gateway.example.com/v1/credits",
            server.Calls[0].RequestUri!.ToString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should work with OIDC authentication in getCredits",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_work_with_OIDC_authentication_in_getCredits()
    {
        var oidcCalls = 0;
        VercelEnvironment.OidcTokenOverride = _ =>
        {
            oidcCalls++;
            return Task.FromResult("oidc-token");
        };
        try
        {
            var server = new GatewayTestServer(Credits);
            var provider = GatewayProvider.Create(
                new GatewayOptions { ApiKey = string.Empty },
                server
            );

            var credits = await provider.GetCreditsAsync();

            Assert.NotNull(credits);
            Assert.Equal(1, oidcCalls);
            Assert.Equal("Bearer oidc-token", server.RequestHeaders()["authorization"]);
        }
        finally
        {
            VercelEnvironment.OidcTokenOverride = null;
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle errors from the credits endpoint",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_errors_from_the_credits_endpoint()
    {
        var server = new GatewayTestServer(
            "{\"error\":{\"message\":\"Credits service unavailable\",\"type\":\"internal_server_error\"}}",
            HttpStatusCode.InternalServerError
        );
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-key" }, server);

        var error = await Assert.ThrowsAsync<GatewayInternalServerError>(() =>
            provider.GetCreditsAsync()
        );

        Assert.Contains("Credits service unavailable", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include proper headers for credits request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_proper_headers_for_credits_request()
    {
        var server = new GatewayTestServer(Credits);
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                ApiKey = "test-key",
                Headers = new Dictionary<string, string> { ["custom-header"] = "custom-value" },
            },
            server
        );

        await provider.GetCreditsAsync();

        var headers = server.RequestHeaders();
        Assert.Equal("Bearer test-key", headers["authorization"]);
        Assert.Equal("0.0.1", headers["ai-gateway-protocol-version"]);
        Assert.Equal("api-key", headers["ai-gateway-auth-method"]);
        Assert.Equal("custom-value", headers["custom-header"]);
        Assert.StartsWith("ai-sdk-gateway/", headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should be available on the provider interface",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_be_available_on_the_provider_interface()
    {
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-key" });

        Func<CancellationToken, Task<GatewayCreditsResponse>> getCredits = provider.GetCreditsAsync;

        Assert.NotNull(getCredits);
    }
}
