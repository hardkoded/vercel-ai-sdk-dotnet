// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Auth = Vercel.AI.Gateway.GatewayRealtimeAuth;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModel.GatewayRealtimeModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModel;

/// <summary>
/// Port of <c>gateway-realtime-model.test.ts</c> &gt; <c>gateway.experimental_realtime</c>. The two browser tests
/// (<c>allows building the realtime codec model in browsers</c> and <c>rejects minting (getToken) in browsers</c>) are not
/// ported: .NET has no <c>window</c> global to simulate.
/// </summary>
public sealed class GatewayExperimentalRealtimeTests
{
    private const string Prefix = "packages/gateway/src/gateway-realtime-model.test.ts::gateway.experimental_realtime::";

    [Fact]
    [UpstreamTest(Prefix + "creates a realtime model from a model id", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Creates_a_realtime_model_from_a_model_id()
    {
        var gateway = GatewayProvider.Create(new GatewayOptions { ApiKey = "vck_test-token" });

        var model = (Vercel.AI.Gateway.GatewayRealtimeModel)gateway.RealtimeModel("openai/gpt-realtime");

        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal("openai/gpt-realtime", model.ModelId);
        Assert.Equal("gateway.realtime", model.Provider);
    }

    [Fact]
    [UpstreamTest(Prefix + "mints a vcst_ client secret via /v1/realtime/client-secrets and returns it with the wss url", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Mints_a_vcst_client_secret_via_v1_realtime_client_secrets_and_returns_it_with_the_wss_url()
    {
        var capture = new RealtimeMintCapture("{\"token\":\"vcst_minted\",\"expiresAt\":1700000060}");
        var gateway = GatewayProvider.Create(new GatewayOptions { ApiKey = "vck_test-token" }, capture);

        var result = await gateway.ExperimentalRealtimeGetTokenAsync("openai/gpt-realtime", 120);

        // Minted token (not the raw key), and the mint hit the v1 route on the gateway origin, not the realtime baseURL
        // path (/v4/ai).
        Assert.Equal("vcst_minted", result.Token);
        Assert.Equal(1_700_000_060, result.ExpiresAt);
        Assert.Equal("wss://ai-gateway.vercel.sh/v4/ai/realtime-model?ai-model-id=openai%2Fgpt-realtime", result.Url);
        var request = Assert.Single(capture.Requests);
        Assert.Equal("https://ai-gateway.vercel.sh/v1/realtime/client-secrets", request.RequestUri!.AbsoluteUri);

        // The request body forwarded the model and the TTL. `routeKind` is omitted for realtime mints (the gateway
        // default) so older gateway deployments keep accepting them.
        var sentBody = JsonNode.Parse(capture.Bodies[0])!.AsObject();
        Assert.Equal("openai/gpt-realtime", (string?)sentBody["model"]);
        Assert.Equal(120, (int?)sentBody["expiresIn"]);
        Assert.False(sentBody.ContainsKey("routeKind"));
        // Authenticated with the long-lived key.
        Assert.Equal("Bearer vck_test-token", request.Headers.GetValues("authorization").Single());
    }

    [Fact]
    [UpstreamTest(Prefix + "tolerates a null expiresAt from the mint endpoint and omits it from the result", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Tolerates_a_null_expiresAt_from_the_mint_endpoint_and_omits_it_from_the_result()
    {
        var capture = new RealtimeMintCapture("{\"token\":\"vcst_minted\",\"expiresAt\":null}");
        var gateway = GatewayProvider.Create(new GatewayOptions { ApiKey = "vck_test-token" }, capture);

        var result = await gateway.ExperimentalRealtimeGetTokenAsync("openai/gpt-realtime");

        Assert.Equal("vcst_minted", result.Token);
        Assert.Null(result.ExpiresAt);
    }

    [Fact]
    [UpstreamTest(Prefix + "carries teamIdOrSlug through realtime WebSocket protocols", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Carries_teamIdOrSlug_through_realtime_WebSocket_protocols()
    {
        var scopedGateway = GatewayProvider.Create(new GatewayOptions { ApiKey = "vck_test-token", TeamIdOrSlug = "my-team" });

        var model = (Vercel.AI.Gateway.GatewayRealtimeModel)scopedGateway.RealtimeModel("openai/gpt-realtime");
        var config = model.GetWebSocketConfig(
            "vcst_test-secret",
            "wss://ai-gateway.vercel.sh/v4/ai/realtime-model?ai-model-id=openai%2Fgpt-realtime");

        Assert.Equal("my-team", Auth.GetGatewayRealtimeTeamIdOrSlug(string.Join(", ", config.Protocols!)));
    }
}
