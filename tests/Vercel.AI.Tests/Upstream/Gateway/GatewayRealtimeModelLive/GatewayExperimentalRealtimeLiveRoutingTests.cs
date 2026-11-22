// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModel;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModelLive.GatewayRealtimeModelLiveSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModelLive;

/// <summary>Port of <c>gateway-realtime-model-live.test.ts</c> &gt; <c>gateway.experimental_realtime Live routing</c>.</summary>
public sealed class GatewayExperimentalRealtimeLiveRoutingTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-realtime-model-live.test.ts::gateway.experimental_realtime Live routing::";

    private static GatewayProvider CreateGateway(HttpMessageHandler? handler = null) =>
        GatewayProvider.Create(new GatewayOptions { ApiKey = "vck_test-token" }, handler);

    [Theory]
    [InlineData("openai/gpt-live-1")]
    [InlineData("openai/gpt-live-1.1")]
    [UpstreamTest(
        Prefix + "creates a Live model for %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Creates_a_Live_model_for_model_id(string modelId)
    {
        var model = CreateGateway().ExperimentalRealtime(modelId, null);

        Assert.IsType<Vercel.AI.Gateway.GatewayRealtimeModelLive>(model);
        Assert.Equal("gateway.live", model.Provider);
    }

    [Theory]
    [InlineData("openai/gpt-realtime-2")]
    [InlineData("google/gemini-3.8-live")]
    [InlineData("spacexai/grok-voice-3.0")]
    [UpstreamTest(
        Prefix + "keeps %s on the realtime route",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Keeps_model_id_on_the_realtime_route(string modelId)
    {
        var model = CreateGateway().ExperimentalRealtime(modelId, null);

        Assert.IsType<Vercel.AI.Gateway.GatewayRealtimeModel>(model);
        Assert.Equal("gateway.realtime", model.Provider);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "honors an explicit api option",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Honors_an_explicit_api_option()
    {
        var gateway = CreateGateway();

        Assert.IsType<Vercel.AI.Gateway.GatewayRealtimeModelLive>(
            gateway.ExperimentalRealtime(
                "openai/gpt-live-2",
                new GatewayRealtimeOptions { Api = GatewayRealtimeApi.Live }
            )
        );
        Assert.IsType<Vercel.AI.Gateway.GatewayRealtimeModel>(
            gateway.ExperimentalRealtime(
                "openai/gpt-live-1.1",
                new GatewayRealtimeOptions { Api = GatewayRealtimeApi.Realtime }
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects an unknown api option",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Rejects_an_unknown_api_option()
    {
        Assert.Throws<InvalidArgumentException>(() =>
            CreateGateway()
                .ExperimentalRealtime(
                    "openai/gpt-live-1.1",
                    new GatewayRealtimeOptions { Api = (GatewayRealtimeApi)99 }
                )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "mints a Live token with routeKind live and returns the live-model url",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Mints_a_Live_token_with_routeKind_live_and_returns_the_live_model_url()
    {
        var capture = new RealtimeMintCapture("{\"token\":\"vcst_live\",\"expiresAt\":1700000060}");
        var gateway = CreateGateway(capture);

        var result = await gateway.ExperimentalRealtimeGetTokenAsync("openai/gpt-live-1.1", 60);

        Assert.Equal("vcst_live", result.Token);
        Assert.Equal(LiveUrl, result.Url);
        Assert.Equal(1_700_000_060, result.ExpiresAt);
        var request = Assert.Single(capture.Requests);
        Assert.Equal(
            "https://ai-gateway.vercel.sh/v1/realtime/client-secrets",
            request.RequestUri!.AbsoluteUri
        );
        AssertJson(
            "{\"model\":\"openai/gpt-live-1.1\",\"routeKind\":\"live\",\"expiresIn\":60}",
            JsonNode.Parse(capture.Bodies[0])
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "routes getToken by the api option and leaves realtime mints unchanged",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Routes_getToken_by_the_api_option_and_leaves_realtime_mints_unchanged()
    {
        var capture = new RealtimeMintCapture("{\"token\":\"vcst_x\"}");
        var gateway = CreateGateway(capture);

        var live = await gateway.ExperimentalRealtimeGetTokenAsync(
            "openai/gpt-live-2",
            options: new GatewayRealtimeOptions { Api = GatewayRealtimeApi.Live }
        );
        var realtime = await gateway.ExperimentalRealtimeGetTokenAsync(
            "openai/gpt-live-1.1",
            options: new GatewayRealtimeOptions { Api = GatewayRealtimeApi.Realtime }
        );

        Assert.Contains("/v4/ai/live-model?", live.Url);
        Assert.Contains("/v4/ai/realtime-model?", realtime.Url);
        AssertJson(
            "{\"model\":\"openai/gpt-live-2\",\"routeKind\":\"live\"}",
            JsonNode.Parse(capture.Bodies[0])
        );
        AssertJson("{\"model\":\"openai/gpt-live-1.1\"}", JsonNode.Parse(capture.Bodies[1]));
    }
}
