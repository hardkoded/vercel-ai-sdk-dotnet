// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>Port of <c>gateway-provider.test.ts</c> &gt; <c>GatewayProvider &gt; Authentication Comprehensive Tests &gt; Environment variable edge cases</c>.</summary>
[Collection("GatewayAuthEnvironment")]
public sealed class GatewayProviderEnvironmentVariableEdgeCasesTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-provider.test.ts::GatewayProvider > Authentication Comprehensive Tests > Environment variable edge cases::";

    [Fact]
    [UpstreamTest(
        Prefix + "should handle empty string environment variables as undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_empty_string_environment_variables_as_undefined()
    {
        using var scope = new GatewayProviderTestScope();
        scope.SetEnvironment("VERCEL_OIDC_TOKEN", "");
        scope.SetEnvironment(GatewayProvider.ApiKeyEnvironmentVariable, "");
        var oidcError = new GatewayAuthenticationError("OIDC token not available", 401);
        scope.MockOidcFailure(oidcError);
        var provider = GatewayProvider.Create(new GatewayOptions());

        var error = await Assert.ThrowsAsync<GatewayAuthenticationError>(() =>
            provider.GetGatewayAuthTokenAsync(CancellationToken.None)
        );

        Assert.Same(oidcError, error);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle whitespace-only environment variables",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_whitespace_only_environment_variables()
    {
        using var scope = new GatewayProviderTestScope();
        scope.SetEnvironment("VERCEL_OIDC_TOKEN", "   ");
        scope.SetEnvironment(GatewayProvider.ApiKeyEnvironmentVariable, "\t\n ");
        var provider = GatewayProvider.Create(new GatewayOptions());

        // The whitespace API key should still be used (it's treated as a valid value)
        var result = await provider.GetGatewayAuthTokenAsync(CancellationToken.None);

        Assert.Equal("api-key", result.AuthMethod);
        Assert.Equal("\t\n ", result.Token);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prioritize options.apiKey over all environment variables",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prioritize_options_apiKey_over_all_environment_variables()
    {
        using var scope = new GatewayProviderTestScope();
        scope.SetEnvironment("VERCEL_OIDC_TOKEN", "env-oidc-token");
        scope.SetEnvironment(GatewayProvider.ApiKeyEnvironmentVariable, "env-api-key");
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "options-api-key" });

        var result = await provider.GetGatewayAuthTokenAsync(CancellationToken.None);

        Assert.Equal("api-key", result.AuthMethod);
        Assert.Equal("options-api-key", result.Token);
        Assert.Equal(0, scope.OidcCalls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should surface OIDC error as cause when authentication fails",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads config.headers of the GatewayLanguageModel constructor call. The port reads the provider's headers (getHeaders)."
    )]
    public async Task Should_surface_OIDC_error_as_cause_when_authentication_fails()
    {
        using var scope = new GatewayProviderTestScope();
        scope.SetEnvironment("VERCEL_OIDC_TOKEN", "");
        scope.SetEnvironment(GatewayProvider.ApiKeyEnvironmentVariable, null);
        var oidcError = new InvalidOperationException(
            "OIDC token generation failed: project not linked"
        );
        scope.MockOidcFailure(oidcError);
        var provider = GatewayProvider.Create(new GatewayOptions());
        provider.LanguageModel("test-model");

        var error = await Assert.ThrowsAsync<GatewayAuthenticationError>(() =>
            provider.GetHeadersAsync(CancellationToken.None)
        );

        Assert.Same(oidcError, error.Cause);
        Assert.Contains("No authentication provided", error.Message);
    }
}
