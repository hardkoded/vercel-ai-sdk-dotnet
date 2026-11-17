// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Gateway.VercelOidc;

/// <summary>Options for <see cref="VercelOidcToken.GetVercelOidcTokenAsync"/>. Maps to the <c>options</c> of <c>getVercelOidcToken</c>.</summary>
internal sealed class VercelOidcTokenOptions
{
    /// <summary>Treat the token as expired this many milliseconds before it is.</summary>
    public double? ExpirationBufferMs { get; set; }

    /// <summary>Project id. Read from <c>.vercel/project.json</c> when unset.</summary>
    public string? Project { get; set; }

    /// <summary>Team id. Read from <c>.vercel/project.json</c> when unset.</summary>
    public string? Team { get; set; }
}

/// <summary>Maps to <c>get-vercel-oidc-token.ts</c> in <c>@vercel/oidc</c>.</summary>
internal static class VercelOidcToken
{
    /// <summary>Returns the OIDC token, refreshing it through the Vercel CLI credentials when it is missing or expired.</summary>
    public static async Task<string> GetVercelOidcTokenAsync(
        VercelOidcTokenOptions? options,
        CancellationToken cancellationToken
    )
    {
        var token = "";
        Exception? err = null;
        try
        {
            token = GetVercelOidcTokenSync();
        }
        catch (InvalidOperationException error)
        {
            err = error;
        }

        try
        {
            if (
                token.Length == 0
                || TokenUtil.IsExpired(
                    TokenUtil.GetTokenPayload(token),
                    options?.ExpirationBufferMs ?? 0
                )
            )
            {
                await Token.RefreshTokenAsync(options, cancellationToken).ConfigureAwait(false);
                token = GetVercelOidcTokenSync();
            }
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            throw new VercelOidcTokenError((err?.Message ?? "") + "\n" + error.Message);
        }

        return token;
    }

    /// <summary>
    /// Reads the OIDC token from <c>VERCEL_OIDC_TOKEN</c>. Upstream first reads the <c>x-vercel-oidc-token</c> request header
    /// from the Vercel request context, which has no .NET counterpart.
    /// </summary>
    public static string GetVercelOidcTokenSync()
    {
        var token = Environment.GetEnvironmentVariable(
            VercelEnvironment.OidcTokenEnvironmentVariable
        );
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException(
                "The 'x-vercel-oidc-token' header is missing from the request. Do you have the OIDC option enabled in the Vercel project settings?"
            );
        }

        return token!;
    }
}
