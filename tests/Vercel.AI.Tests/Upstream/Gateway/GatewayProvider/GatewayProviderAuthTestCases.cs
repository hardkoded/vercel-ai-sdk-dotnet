// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>One entry of <c>authTestCases</c> in <c>gateway-provider.test.ts</c>.</summary>
public sealed record GatewayProviderAuthTestCase(
    string Name,
    string? EnvOidcToken,
    string? EnvApiKey,
    string? OptionsApiKey,
    string? OidcTokenMock,
    bool ExpectSuccess,
    string? ExpectedAuthMethod
)
{
    public override string ToString() => Name;

    internal void Apply(GatewayProviderTestScope scope)
    {
        scope.SetEnvironment(VercelEnvironment.OidcTokenEnvironmentVariable, EnvOidcToken);
        scope.SetEnvironment(GatewayProvider.ApiKeyEnvironmentVariable, EnvApiKey);
        if (OidcTokenMock is not null)
        {
            scope.MockOidc(OidcTokenMock);
        }
        else
        {
            scope.MockOidcFailure(new GatewayAuthenticationError("OIDC token not available", 401));
        }
    }
}

/// <summary>Maps to <c>authTestCases</c> of <c>gateway-provider.test.ts</c>.</summary>
public static class GatewayProviderAuthTestCases
{
    public static TheoryData<GatewayProviderAuthTestCase> All =>
        new()
        {
            new("no auth at all", null, null, null, null, false, null),
            new(
                "valid oidc, invalid api key",
                "valid-oidc-token-12345",
                null,
                "invalid-api-key",
                "valid-oidc-token-12345",
                true,
                "api-key"
            ),
            new(
                "invalid oidc, valid api key",
                "invalid-oidc-token",
                null,
                "gw_valid_api_key_12345",
                null,
                true,
                "api-key"
            ),
            new("no oidc, invalid api key", null, "invalid-api-key", null, null, true, "api-key"),
            new(
                "no oidc, valid api key",
                null,
                "gw_valid_api_key_12345",
                null,
                null,
                true,
                "api-key"
            ),
            new(
                "valid oidc, no api key",
                "valid-oidc-token-12345",
                null,
                null,
                "valid-oidc-token-12345",
                true,
                "oidc"
            ),
            new(
                "valid oidc, valid api key",
                "valid-oidc-token-12345",
                "gw_valid_api_key_12345",
                null,
                "valid-oidc-token-12345",
                true,
                "api-key"
            ),
            new(
                "valid oidc, valid options api key",
                "valid-oidc-token-12345",
                null,
                "gw_valid_options_api_key_12345",
                "valid-oidc-token-12345",
                true,
                "api-key"
            ),
            new("invalid oidc, no api key", "invalid-oidc-token", null, null, null, false, null),
            new(
                "invalid oidc, invalid api key",
                "invalid-oidc-token",
                "invalid-api-key",
                null,
                null,
                true,
                "api-key"
            ),
        };
}
