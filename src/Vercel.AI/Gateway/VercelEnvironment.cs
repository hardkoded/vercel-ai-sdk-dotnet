// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway.VercelOidc;

namespace Vercel.AI.Gateway;

/// <summary>
/// The ambient Vercel request context. Maps to the <c>@vercel/request-context</c> object that Vercel stores on
/// <c>globalThis[Symbol.for('@vercel/request-context')]</c> and that <c>getContext</c> in <c>@vercel/oidc</c> reads.
/// A .NET host supplies it with <see cref="Use"/>, for example from ASP.NET Core middleware.
/// </summary>
public static class VercelRequestContext
{
    private static readonly AsyncLocal<Scope?> s_current = new();

    internal static IReadOnlyDictionary<string, string?>? Headers => s_current.Value?.Headers;

    /// <summary>
    /// Makes <paramref name="headers"/> the request headers of the current async flow (for example <c>x-vercel-id</c> and
    /// <c>x-vercel-oidc-token</c>) until the returned scope is disposed. Header names are matched case-insensitively;
    /// names that differ only by case must carry the same value, otherwise <see cref="ArgumentException"/> is thrown.
    /// Disposing the scope also revokes the headers for work that captured them, and disposing twice does nothing.
    /// </summary>
    public static IDisposable Use(IReadOnlyDictionary<string, string?>? headers)
    {
        Dictionary<string, string?>? copy = null;
        if (headers is not null)
        {
            copy = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in headers)
            {
                if (copy.TryGetValue(pair.Key, out var existing) && existing != pair.Value)
                {
                    throw new ArgumentException(
                        $"The header '{pair.Key}' appears more than once with different values.",
                        nameof(headers)
                    );
                }

                copy[pair.Key] = pair.Value;
            }
        }

        var scope = new Scope(copy, s_current.Value);
        s_current.Value = scope;
        return scope;
    }

    private sealed class Scope(IReadOnlyDictionary<string, string?>? headers, Scope? previous)
        : IDisposable
    {
        private int _disposed;

        public IReadOnlyDictionary<string, string?>? Headers { get; private set; } = headers;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                return;
            }

            Headers = null;
            s_current.Value = previous;
        }
    }
}

/// <summary>
/// Vercel runtime lookups. Maps to <c>vercel-environment.ts</c>, which re-exports <c>getVercelOidcToken</c> from
/// <c>@vercel/oidc</c>, ported in <c>VercelOidc/</c>.
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

    /// <summary>
    /// Returns the Vercel OIDC token, refreshing it with the Vercel CLI credentials when it is missing or expired.
    /// Maps to <c>getVercelOidcToken</c>.
    /// </summary>
    public static Task<string> GetVercelOidcTokenAsync(CancellationToken cancellationToken)
    {
        var overrideProvider = OidcTokenOverride;
        if (overrideProvider is not null)
        {
            return overrideProvider(cancellationToken);
        }

        return VercelOidcToken.GetVercelOidcTokenAsync(null, cancellationToken);
    }

    /// <summary>
    /// Returns the <c>x-vercel-id</c> header of the Vercel request context, or null. Maps to <c>getVercelRequestId</c>.
    /// </summary>
    public static string? GetVercelRequestId()
    {
        return GetContextHeader("x-vercel-id");
    }

    /// <summary>Returns a header of the Vercel request context, or null. Maps to <c>getContext().headers?.[name]</c>.</summary>
    internal static string? GetContextHeader(string name)
    {
        var headers = VercelRequestContext.Headers;
        return headers is not null && headers.TryGetValue(name, out var value) ? value : null;
    }
}
