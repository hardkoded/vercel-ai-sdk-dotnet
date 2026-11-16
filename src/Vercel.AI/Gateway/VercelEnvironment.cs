// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Gateway;

/// <summary>
/// Vercel runtime lookups. Maps to <c>vercel-environment.ts</c>, which re-exports <c>getVercelOidcToken</c> from
/// <c>@vercel/oidc</c>.
/// </summary>
internal static class VercelEnvironment
{
    private static readonly AsyncLocal<Func<CancellationToken, Task<string>>?> s_oidcTokenOverride =
        new();

    /// <summary>Environment variable that carries the Vercel OIDC token.</summary>
    public const string OidcTokenEnvironmentVariable = "VERCEL_OIDC_TOKEN";

    /// <summary>Test seam that replaces <see cref="GetVercelOidcTokenAsync"/>. Null uses the environment variable.</summary>
    internal static Func<CancellationToken, Task<string>>? OidcTokenOverride
    {
        get => s_oidcTokenOverride.Value;
        set => s_oidcTokenOverride.Value = value;
    }

    /// <summary>Returns the Vercel OIDC token, or throws when none is available.</summary>
    public static Task<string> GetVercelOidcTokenAsync(CancellationToken cancellationToken)
    {
        var overrideProvider = OidcTokenOverride;
        if (overrideProvider is not null)
        {
            return overrideProvider(cancellationToken);
        }

        var token = Environment.GetEnvironmentVariable(OidcTokenEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new AiSdkException(
                "The '" + OidcTokenEnvironmentVariable + "' environment variable is not set."
            );
        }

        return Task.FromResult(token!);
    }
}
