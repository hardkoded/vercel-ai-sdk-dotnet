// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Auth = Vercel.AI.Gateway.GatewayRealtimeAuth;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeAuth;

/// <summary>Port of <c>gateway-realtime-auth.test.ts</c> &gt; <c>getGatewayRealtimeProtocols</c>.</summary>
public sealed class GetGatewayRealtimeProtocolsTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-realtime-auth.test.ts::getGatewayRealtimeProtocols::";

    [Fact]
    [UpstreamTest(
        Prefix + "offers the marker subprotocol and the prefixed auth token",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Offers_the_marker_subprotocol_and_the_prefixed_auth_token()
    {
        Assert.Equal(
            new[]
            {
                Auth.GatewayRealtimeSubprotocol,
                Auth.GatewayAuthSubprotocolPrefix + "vck_test-token",
            },
            Auth.GetGatewayRealtimeProtocols("vck_test-token")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "offers encoded team scoping when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Offers_encoded_team_scoping_when_provided()
    {
        var protocols = Auth.GetGatewayRealtimeProtocols("vck_test-token", "team/with/slashes");

        Assert.Equal(3, protocols.Count);
        var teamProtocol = protocols[2];
        Assert.StartsWith(
            Auth.GatewayTeamSubprotocolPrefix,
            teamProtocol,
            StringComparison.Ordinal
        );
        Assert.Matches(
            new Regex("^[A-Za-z0-9_-]+$"),
            teamProtocol.Substring(Auth.GatewayTeamSubprotocolPrefix.Length)
        );
        Assert.Equal(
            "team/with/slashes",
            Auth.GetGatewayRealtimeTeamIdOrSlug(string.Join(", ", protocols))
        );
    }
}
