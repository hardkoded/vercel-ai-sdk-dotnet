// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModel.GatewayRealtimeModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModel;

/// <summary>Port of <c>gateway-realtime-model.test.ts</c> &gt; <c>gateway.experimental_realtime auth errors</c>.</summary>
[Collection("GatewayAuthEnvironment")]
public sealed class GatewayExperimentalRealtimeAuthErrorsTests : IDisposable
{
    private const string Prefix = "packages/gateway/src/gateway-realtime-model.test.ts::";

    private readonly string? _previousApiKey;

    public GatewayExperimentalRealtimeAuthErrorsTests()
    {
        _previousApiKey = Environment.GetEnvironmentVariable(GatewayProvider.ApiKeyEnvironmentVariable);
        Environment.SetEnvironmentVariable(GatewayProvider.ApiKeyEnvironmentVariable, null);
        VercelEnvironment.OidcTokenOverride = _ => throw new InvalidOperationException("no oidc token available");
    }

    public void Dispose()
    {
        VercelEnvironment.OidcTokenOverride = null;
        Environment.SetEnvironmentVariable(GatewayProvider.ApiKeyEnvironmentVariable, _previousApiKey);
    }

    [Fact]
    [UpstreamTest(Prefix + "reuses HTTP auth headers for OIDC server connections", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Reuses_HTTP_auth_headers_for_OIDC_server_connections()
    {
        VercelEnvironment.OidcTokenOverride = _ => Task.FromResult("oidc-test-token");
        var gateway = GatewayProvider.Create(new GatewayOptions
        {
            TeamIdOrSlug = "team-example",
            Headers = new Dictionary<string, string> { ["x-custom"] = "example" },
        });
        var model = (IServerWebSocketRealtimeModel)gateway.RealtimeModel("openai/gpt-realtime");

        var descriptor = await model.GetServerWebSocketConfigAsync(null, CancellationToken.None);

        Assert.Equal("Bearer oidc-test-token", descriptor.Headers["authorization"]);
        Assert.Equal("oidc", descriptor.Headers["ai-gateway-auth-method"]);
        Assert.Equal("0.0.1", descriptor.Headers["ai-gateway-protocol-version"]);
        Assert.Equal("team-example", descriptor.Headers["x-vercel-ai-gateway-team"]);
        Assert.Equal("example", descriptor.Headers["x-custom"]);
        Assert.Contains("ai-sdk-gateway/", descriptor.Headers["user-agent"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Prefix + "wraps getToken auth failures in a GatewayAuthenticationError", Coverage = UpstreamCoverage.Covered)]
    public async Task Wraps_getToken_auth_failures_in_a_GatewayAuthenticationError()
    {
        // No API key and no OIDC token: the realtime auth path must surface the same GatewayAuthenticationError the other
        // gateway models throw.
        var gateway = GatewayProvider.Create();

        await Assert.ThrowsAsync<GatewayAuthenticationError>(
            () => gateway.ExperimentalRealtimeGetTokenAsync("openai/gpt-realtime-2"));
    }
}
