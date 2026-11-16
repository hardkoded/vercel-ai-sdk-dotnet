// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Auth = Vercel.AI.Gateway.GatewayRealtimeAuth;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeAuth;

/// <summary>Port of <c>gateway-realtime-auth.test.ts</c> &gt; <c>getGatewayRealtimeTeamIdOrSlug</c>.</summary>
public sealed class GetGatewayRealtimeTeamIdOrSlugTests
{
    private const string Prefix = "packages/gateway/src/gateway-realtime-auth.test.ts::getGatewayRealtimeTeamIdOrSlug::";

    [Fact]
    [UpstreamTest(Prefix + "round-trips a team id produced by getGatewayRealtimeProtocols", Coverage = UpstreamCoverage.Covered)]
    public void Round_trips_a_team_id_produced_by_getGatewayRealtimeProtocols()
    {
        var header = string.Join(", ", Auth.GetGatewayRealtimeProtocols("vck_test-token", "team_123"));
        Assert.Equal("team_123", Auth.GetGatewayRealtimeTeamIdOrSlug(header));
    }

    [Fact]
    [UpstreamTest(Prefix + "tolerates arbitrary whitespace and subprotocol ordering", Coverage = UpstreamCoverage.Covered)]
    public void Tolerates_arbitrary_whitespace_and_subprotocol_ordering()
    {
        var teamProtocol = Auth.GetGatewayRealtimeProtocols("token", "my-team")[2];
        var header = "  " + teamProtocol + "  ,  " + Auth.GatewayRealtimeSubprotocol + "  ";
        Assert.Equal("my-team", Auth.GetGatewayRealtimeTeamIdOrSlug(header));
    }

    [Fact]
    [UpstreamTest(Prefix + "returns undefined when no team subprotocol is present", Coverage = UpstreamCoverage.Covered)]
    public void Returns_undefined_when_no_team_subprotocol_is_present()
    {
        Assert.Null(Auth.GetGatewayRealtimeTeamIdOrSlug(Auth.GatewayRealtimeSubprotocol));
    }

    [Fact]
    [UpstreamTest(Prefix + "returns undefined for malformed encoded team values", Coverage = UpstreamCoverage.Covered)]
    public void Returns_undefined_for_malformed_encoded_team_values()
    {
        Assert.Null(Auth.GetGatewayRealtimeTeamIdOrSlug(Auth.GatewayTeamSubprotocolPrefix + "%%%"));
    }
}
