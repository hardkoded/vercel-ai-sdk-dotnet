// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModelLive.GatewayRealtimeModelLiveSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModelLive;

/// <summary>Port of <c>gateway-realtime-model-live.test.ts</c> &gt; <c>GatewayRealtimeModelLive</c> &gt; <c>doCreateClientSecret</c>.</summary>
public sealed class DoCreateClientSecretTests
{
    [Fact]
    [UpstreamTest(
        "packages/gateway/src/gateway-realtime-model-live.test.ts::GatewayRealtimeModelLive > doCreateClientSecret::mints a Live-bound secret and returns the live-model url",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Mints_a_Live_bound_secret_and_returns_the_live_model_url()
    {
        var calls = new List<(string ModelId, int? Expires, string RouteKind)>();
        var result = await CreateTestModel(
                createClientSecret: (modelId, expires, routeKind, _) =>
                {
                    calls.Add((modelId, expires, routeKind));
                    return Task.FromResult(
                        new GatewayMintedClientSecret("vcst_live", 1_700_000_060)
                    );
                }
            )
            .DoCreateClientSecretAsync(120, CancellationToken.None);

        Assert.Equal(("openai/gpt-live-1.1", 120, "live"), Assert.Single(calls));
        Assert.Equal("vcst_live", result.Token);
        Assert.Equal(LiveUrl, result.Url);
        Assert.Equal(1_700_000_060, result.ExpiresAt);
    }

    [Fact]
    [UpstreamTest(
        "packages/gateway/src/gateway-realtime-model-live.test.ts::GatewayRealtimeModelLive > doCreateClientSecret::upgrades an http base url to ws",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Upgrades_an_http_base_url_to_ws()
    {
        var result = await CreateTestModel(baseUrl: "http://localhost:3000/v4/ai")
            .DoCreateClientSecretAsync(null, CancellationToken.None);

        Assert.Equal(
            "ws://localhost:3000/v4/ai/live-model?ai-model-id=openai%2Fgpt-live-1.1",
            result.Url
        );
    }
}
