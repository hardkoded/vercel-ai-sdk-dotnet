// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>
/// Stands in for the <c>beforeEach</c> of <c>gateway-provider.test.ts</c>: the OIDC token resolves to
/// <c>mock-oidc-token</c>, the request id is <c>mock-request-id</c>, and <c>AI_GATEWAY_API_KEY</c> is unset. Disposing
/// restores the environment. Use it from a class in the <c>GatewayAuthEnvironment</c> collection.
/// </summary>
internal sealed class GatewayProviderTestScope : IDisposable
{
    private readonly Dictionary<string, string?> _previous = [];
    private readonly IDisposable _requestContext;

    public GatewayProviderTestScope()
    {
        SetEnvironment(GatewayProvider.ApiKeyEnvironmentVariable, null);
        MockOidc("mock-oidc-token");
        _requestContext = VercelRequestContext.Use(
            new Dictionary<string, string?> { ["x-vercel-id"] = "mock-request-id" }
        );
    }

    /// <summary>How many times the OIDC token was requested. Maps to <c>getVercelOidcToken</c> mock calls.</summary>
    public int OidcCalls { get; private set; }

    /// <summary>Maps to <c>process.env[name] = value</c>. A null value deletes the variable.</summary>
    public void SetEnvironment(string name, string? value)
    {
        if (!_previous.ContainsKey(name))
        {
            _previous[name] = Environment.GetEnvironmentVariable(name);
        }

        Environment.SetEnvironmentVariable(name, value);
    }

    /// <summary>Maps to <c>vi.mocked(getVercelOidcToken).mockResolvedValue(token)</c>.</summary>
    public void MockOidc(string token)
    {
        VercelEnvironment.OidcTokenOverride = _ =>
        {
            OidcCalls++;
            return Task.FromResult(token);
        };
    }

    /// <summary>Maps to <c>vi.mocked(getVercelOidcToken).mockRejectedValue(error)</c>.</summary>
    public void MockOidcFailure(Exception error)
    {
        VercelEnvironment.OidcTokenOverride = _ =>
        {
            OidcCalls++;
            return Task.FromException<string>(error);
        };
    }

    public void Dispose()
    {
        _requestContext.Dispose();
        VercelEnvironment.OidcTokenOverride = null;
        foreach (var (name, value) in _previous)
        {
            Environment.SetEnvironmentVariable(name, value);
        }
    }
}
