// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>Port of <c>gateway-provider.test.ts</c> &gt; <c>GatewayProvider &gt; Authentication Comprehensive Tests &gt; createGateway authentication</c>.</summary>
[Collection("GatewayAuthEnvironment")]
public sealed class GatewayProviderCreateGatewayAuthenticationTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-provider.test.ts::GatewayProvider > Authentication Comprehensive Tests > createGateway authentication::";

    [Theory]
    [MemberData(
        nameof(GatewayProviderAuthTestCases.All),
        MemberType = typeof(GatewayProviderAuthTestCases)
    )]
    [UpstreamTest(
        Prefix + "should handle provider creation with ${testCase.name}",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads config.headers of the GatewayLanguageModel constructor call. The port reads the provider's headers (getHeaders)."
    )]
    public async Task Should_handle_provider_creation_with_test_case(
        GatewayProviderAuthTestCase testCase
    )
    {
        using var scope = new GatewayProviderTestScope();
        testCase.Apply(scope);
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                BaseUrl = "https://test-gateway.example.com",
                ApiKey = testCase.OptionsApiKey,
                // Force no caching to ensure headers are called each time
                MetadataCacheRefreshMillis = 0,
            }
        );

        provider.LanguageModel("test-model");

        if (testCase.ExpectSuccess)
        {
            var headers = await provider.GetHeadersAsync(CancellationToken.None);

            Assert.Equal(testCase.ExpectedAuthMethod, headers["ai-gateway-auth-method"]);

            // For OIDC tests, we need to verify the auth token function was called
            if (testCase.ExpectedAuthMethod == "oidc")
            {
                Assert.True(scope.OidcCalls > 0);
            }
            else if (testCase.ExpectedAuthMethod == "api-key" && testCase.OptionsApiKey is not null)
            {
                // If we used options API key, OIDC should not be called
                Assert.Equal(0, scope.OidcCalls);
            }
        }
        else
        {
            var error = await Assert.ThrowsAnyAsync<Exception>(() =>
                provider.GetHeadersAsync(CancellationToken.None)
            );
            Assert.Matches("(?i)authentication|token", error.Message);
        }
    }
}
