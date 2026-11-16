// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Auth = Vercel.AI.Gateway.GatewayRealtimeAuth;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeAuth;

/// <summary>Port of <c>gateway-realtime-auth.test.ts</c> &gt; <c>getGatewayRealtimeAuthToken</c>.</summary>
public sealed class GetGatewayRealtimeAuthTokenTests
{
    private const string Prefix = "packages/gateway/src/gateway-realtime-auth.test.ts::getGatewayRealtimeAuthToken::";

    [Fact]
    [UpstreamTest(Prefix + "round-trips a token produced by getGatewayRealtimeProtocols", Coverage = UpstreamCoverage.Covered)]
    public void Round_trips_a_token_produced_by_getGatewayRealtimeProtocols()
    {
        // The server receives the offered subprotocols as a comma-joined header.
        var header = string.Join(", ", Auth.GetGatewayRealtimeProtocols("vck_test-token"));
        Assert.Equal("vck_test-token", Auth.GetGatewayRealtimeAuthToken(header));
    }

    [Fact]
    [UpstreamTest(Prefix + "tolerates arbitrary whitespace and subprotocol ordering", Coverage = UpstreamCoverage.Covered)]
    public void Tolerates_arbitrary_whitespace_and_subprotocol_ordering()
    {
        var header = "  " + Auth.GatewayAuthSubprotocolPrefix + "oidc-token  ,  " + Auth.GatewayRealtimeSubprotocol + "  ";
        Assert.Equal("oidc-token", Auth.GetGatewayRealtimeAuthToken(header));
    }

    [Fact]
    [UpstreamTest(Prefix + "returns undefined when no auth subprotocol is present", Coverage = UpstreamCoverage.Covered)]
    public void Returns_undefined_when_no_auth_subprotocol_is_present()
    {
        Assert.Null(Auth.GetGatewayRealtimeAuthToken(Auth.GatewayRealtimeSubprotocol));
    }

    [Fact]
    [UpstreamTest(Prefix + "returns undefined for an empty/whitespace token", Coverage = UpstreamCoverage.Covered)]
    public void Returns_undefined_for_an_empty_whitespace_token()
    {
        Assert.Null(Auth.GetGatewayRealtimeAuthToken(Auth.GatewayAuthSubprotocolPrefix + "   "));
    }

    [Fact]
    [UpstreamTest(Prefix + "returns undefined for missing header values", Coverage = UpstreamCoverage.Covered)]
    public void Returns_undefined_for_missing_header_values()
    {
        Assert.Null(Auth.GetGatewayRealtimeAuthToken(null));
        Assert.Null(Auth.GetGatewayRealtimeAuthToken(string.Empty));
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves tokens that contain dots (e.g. JWT/OIDC)", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_tokens_that_contain_dots()
    {
        const string jwt = "header.payload.signature";
        var header = string.Join(",", Auth.GetGatewayRealtimeProtocols(jwt));
        Assert.Equal(jwt, Auth.GetGatewayRealtimeAuthToken(header));
    }
}
