// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModel.GatewayRealtimeModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModel;

/// <summary>Port of <c>gateway-realtime-model.test.ts</c> &gt; <c>GatewayRealtimeModel</c> &gt; <c>doCreateClientSecret</c>.</summary>
public sealed class DoCreateClientSecretTests
{
    private const string Prefix = "packages/gateway/src/gateway-realtime-model.test.ts::GatewayRealtimeModel > doCreateClientSecret::";

    [Fact]
    [UpstreamTest(Prefix + "returns the minted client secret and a wss realtime url with the ai-model-id query", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_minted_client_secret_and_a_wss_realtime_url_with_the_ai_model_id_query()
    {
        var result = await CreateTestModel().DoCreateClientSecretAsync(null, CancellationToken.None);
        Assert.Equal("vcst_test-secret", result.Token);
        Assert.Equal("wss://ai-gateway.vercel.sh/v4/ai/realtime-model?ai-model-id=openai%2Fgpt-realtime", result.Url);
    }

    [Fact]
    [UpstreamTest(Prefix + "upgrades an http base url to ws (local development)", Coverage = UpstreamCoverage.Covered)]
    public async Task Upgrades_an_http_base_url_to_ws_local_development()
    {
        var result = await CreateTestModel("openai/gpt-realtime", "http://localhost:3000/v4/ai").DoCreateClientSecretAsync(null, CancellationToken.None);
        Assert.Equal("ws://localhost:3000/v4/ai/realtime-model?ai-model-id=openai%2Fgpt-realtime", result.Url);
    }

    [Fact]
    [UpstreamTest(Prefix + "passes the model id through verbatim (the gateway owns qualification)", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_the_model_id_through_verbatim_the_gateway_owns_qualification()
    {
        var result = await CreateTestModel("gpt-realtime-2").DoCreateClientSecretAsync(null, CancellationToken.None);
        // No client-side `openai/` prefixing, which mirrors the non-realtime routes.
        Assert.Equal("wss://ai-gateway.vercel.sh/v4/ai/realtime-model?ai-model-id=gpt-realtime-2", result.Url);
    }

    [Fact]
    [UpstreamTest(Prefix + "forwards modelId + expiresAfterSeconds to the mint hook and surfaces expiresAt", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Forwards_modelId_expiresAfterSeconds_to_the_mint_hook_and_surfaces_expiresAt()
    {
        var calls = new List<(string ModelId, int? ExpiresAfterSeconds)>();
        var model = new Vercel.AI.Gateway.GatewayRealtimeModel(
            "openai/gpt-realtime-2",
            new GatewayRealtimeModelConfig(
                "gateway.realtime",
                "https://ai-gateway.vercel.sh/v4/ai",
                _ => Task.FromResult<IReadOnlyDictionary<string, string?>>(new Dictionary<string, string?>()),
                (modelId, expiresAfterSeconds, _) =>
                {
                    calls.Add((modelId, expiresAfterSeconds));
                    return Task.FromResult(new GatewayMintedClientSecret("vcst_minted", 1_700_000_060));
                }));

        var result = await model.DoCreateClientSecretAsync(120, CancellationToken.None);

        var call = Assert.Single(calls);
        Assert.Equal("openai/gpt-realtime-2", call.ModelId);
        Assert.Equal(120, call.ExpiresAfterSeconds);
        Assert.Equal("vcst_minted", result.Token);
        Assert.Equal(1_700_000_060, result.ExpiresAt);
    }

    [Fact]
    [UpstreamTest(Prefix + "omits expiresAt when the mint hook does not return one", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_expiresAt_when_the_mint_hook_does_not_return_one()
    {
        var result = await CreateTestModel().DoCreateClientSecretAsync(null, CancellationToken.None);
        Assert.Null(result.ExpiresAt);
    }
}
