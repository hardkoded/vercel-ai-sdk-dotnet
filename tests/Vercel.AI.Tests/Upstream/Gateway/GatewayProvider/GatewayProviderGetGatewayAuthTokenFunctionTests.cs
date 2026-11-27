// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>Port of <c>gateway-provider.test.ts</c> &gt; <c>GatewayProvider &gt; Authentication Comprehensive Tests &gt; getGatewayAuthToken function</c>.</summary>
[Collection("GatewayAuthEnvironment")]
public sealed class GatewayProviderGetGatewayAuthTokenFunctionTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-provider.test.ts::GatewayProvider > Authentication Comprehensive Tests > getGatewayAuthToken function::";

    [Theory]
    [MemberData(
        nameof(GatewayProviderAuthTestCases.All),
        MemberType = typeof(GatewayProviderAuthTestCases)
    )]
    [UpstreamTest(Prefix + "should handle ${testCase.name}", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_test_case(GatewayProviderAuthTestCase testCase)
    {
        using var scope = new GatewayProviderTestScope();
        testCase.Apply(scope);
        var provider = GatewayProvider.Create(
            new GatewayOptions { ApiKey = testCase.OptionsApiKey }
        );

        if (testCase.ExpectSuccess)
        {
            var result = await provider.GetGatewayAuthTokenAsync(CancellationToken.None);

            Assert.Equal(testCase.ExpectedAuthMethod, result.AuthMethod);
            if (testCase.ExpectedAuthMethod == "api-key")
            {
                Assert.Equal(testCase.OptionsApiKey ?? testCase.EnvApiKey, result.Token);
                if (testCase.OptionsApiKey is not null)
                {
                    Assert.Equal(0, scope.OidcCalls);
                }
            }
            else if (testCase.ExpectedAuthMethod == "oidc")
            {
                Assert.Equal(testCase.OidcTokenMock, result.Token);
                Assert.True(scope.OidcCalls > 0);
            }
        }
        else
        {
            await Assert.ThrowsAnyAsync<Exception>(() =>
                provider.GetGatewayAuthTokenAsync(CancellationToken.None)
            );
        }
    }
}
