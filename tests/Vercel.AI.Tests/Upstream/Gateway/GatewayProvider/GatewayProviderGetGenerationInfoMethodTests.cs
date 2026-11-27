// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>Port of <c>gateway-provider.test.ts</c> &gt; <c>GatewayProvider &gt; getGenerationInfo method</c>.</summary>
[Collection("GatewayAuthEnvironment")]
public sealed class GatewayProviderGetGenerationInfoMethodTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-provider.test.ts::GatewayProvider > getGenerationInfo method::";
    private const string Info =
        "{\"data\":{\"id\":\"gen_test\",\"total_cost\":0.00123,\"market_cost\":0.0011,\"surcharge_cost\":0.00013,\"gateway_cost\":0.00123,\"upstream_inference_cost\":0.0011,\"usage\":0.00123,\"created_at\":\"2024-01-01T00:00:00.000Z\",\"model\":\"gpt-4\",\"is_byok\":false,\"provider_name\":\"openai\",\"streamed\":true,\"finish_reason\":\"stop\",\"latency\":200,\"generation_time\":1500,\"native_tokens_prompt\":100,\"native_tokens_completion\":50,\"native_tokens_reasoning\":0,\"native_tokens_cached\":0,\"native_tokens_cache_creation\":0,\"billable_web_search_calls\":0}}";

    [Fact]
    [UpstreamTest(
        Prefix + "should fetch generation info successfully",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream mocks GatewayGenerationInfoFetcher. The port sends the request through the HTTP handler and checks the id in the query and the parsed result."
    )]
    public async Task Should_fetch_generation_info_successfully()
    {
        using var scope = new GatewayProviderTestScope();
        var server = new GatewayTestServer(Info);
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-key" }, server);

        var generationInfo = await provider.GetGenerationInfoAsync(
            new GatewayGenerationInfoParams { Id = "gen_test" }
        );

        Assert.Equal("gen_test", generationInfo.Id);
        Assert.Equal("?id=gen_test", server.Calls[0].RequestUri!.Query);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should work with OIDC authentication in generation info",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_work_with_OIDC_authentication_in_generation_info()
    {
        using var scope = new GatewayProviderTestScope();
        scope.MockOidc("oidc-token");
        var server = new GatewayTestServer(Info);
        var provider = GatewayProvider.Create(new GatewayOptions(), server);

        var generationInfo = await provider.GetGenerationInfoAsync(
            new GatewayGenerationInfoParams { Id = "gen_test" }
        );

        Assert.NotNull(generationInfo);
        Assert.True(scope.OidcCalls > 0);
        Assert.Equal("Bearer oidc-token", server.RequestHeaders()["authorization"]);
    }
}
