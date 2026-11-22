// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Gateway.GatewayRerankingModel.GatewayRerankingModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRerankingModel;

/// <summary>Port of <c>gateway-reranking-model.test.ts</c> &gt; <c>GatewayRerankingModel &gt; URL construction</c>.</summary>
public sealed class GatewayRerankingModelUrlConstructionTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-reranking-model.test.ts::GatewayRerankingModel > URL construction::";

    [Fact]
    [UpstreamTest(
        Prefix + "should post to /reranking-model endpoint",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_post_to_reranking_model_endpoint()
    {
        var (model, capture) = Setup();
        await model.DoRerankAsync(Call(), CancellationToken.None);
        Assert.Equal("POST", capture.Method);
        Assert.Equal("https://api.test.com/reranking-model", capture.Uri);
    }
}
