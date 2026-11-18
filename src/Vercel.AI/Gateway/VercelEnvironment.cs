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
    private static readonly AsyncLocal<IReadOnlyDictionary<string, string?>?> s_headers = new();

    internal static IReadOnlyDictionary<string, string?>? Headers => s_headers.Value;

    /// <summary>
    /// Makes <paramref name="headers"/> the request headers of the current async flow (for example <c>x-vercel-id</c> and
    /// <c>x-vercel-oidc-token</c>) until the returned scope is disposed. Header names are matched case-insensitively.
    /// </summary>
    public static IDisposable Use(IReadOnlyDictionary<string, string?>? headers)
    {
        var previous = s_headers.Value;
        s_headers.Value = headers is null
            ? null
            : new Dictionary<string, string?>(
                headers.ToDictionary(pair => pair.Key, pair => pair.Value),
                StringComparer.OrdinalIgnoreCase
            );
        return new Scope(previous);
    }

    private sealed class Scope(IReadOnlyDictionary<string, string?>? previous) : IDisposable
    {
        public void Dispose() => s_headers.Value = previous;
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

    /// <summary>Request header that carries the Vercel request id.</summary>
    public const string RequestIdHeader = "x-vercel-id";

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
        return GetContextHeader(RequestIdHeader);
    }

    /// <summary>Returns a header of the Vercel request context, or null. Maps to <c>getContext().headers?.[name]</c>.</summary>
    internal static string? GetContextHeader(string name)
    {
        var headers = VercelRequestContext.Headers;
        return headers is not null && headers.TryGetValue(name, out var value) ? value : null;
    }
}
