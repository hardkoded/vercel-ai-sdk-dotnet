// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModel.GatewayRealtimeModelSupport;
using Auth = Vercel.AI.Gateway.GatewayRealtimeAuth;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModel;

/// <summary>Port of <c>gateway-realtime-model.test.ts</c> &gt; <c>GatewayRealtimeModel</c> &gt; <c>getWebSocketConfig</c>.</summary>
public sealed class GetWebSocketConfigTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-realtime-model.test.ts::GatewayRealtimeModel > getWebSocketConfig::";

    private const string Url =
        "wss://ai-gateway.vercel.sh/v4/ai/realtime-model?ai-model-id=openai%2Fgpt-realtime";

    [Fact]
    [UpstreamTest(
        Prefix
            + "smuggles the bearer token through the subprotocol (browser has no header channel)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Smuggles_the_bearer_token_through_the_subprotocol_browser_has_no_header_channel()
    {
        var config = CreateTestModel().GetWebSocketConfig("vck_test-token", Url);

        Assert.Equal(Url, config.Url);
        Assert.Equal(
            new[]
            {
                Auth.GatewayRealtimeSubprotocol,
                Auth.GatewayAuthSubprotocolPrefix + "vck_test-token",
            },
            config.Protocols
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "smuggles optional team scoping through the subprotocol",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Smuggles_optional_team_scoping_through_the_subprotocol()
    {
        var config = CreateTestModel(
                "openai/gpt-realtime",
                "https://ai-gateway.vercel.sh/v4/ai",
                "vck_test-token",
                "team_123"
            )
            .GetWebSocketConfig("vck_test-token", Url);

        Assert.Equal(
            "team_123",
            Auth.GetGatewayRealtimeTeamIdOrSlug(string.Join(", ", config.Protocols!))
        );
    }
}
