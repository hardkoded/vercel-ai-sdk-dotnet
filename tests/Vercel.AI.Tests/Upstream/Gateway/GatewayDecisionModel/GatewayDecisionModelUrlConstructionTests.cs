// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Gateway.GatewayDecisionModel.GatewayDecisionModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayDecisionModel;

/// <summary>Port of <c>gateway-decision-model.test.ts</c> &gt; <c>GatewayDecisionModel &gt; URL construction</c>.</summary>
public sealed class GatewayDecisionModelUrlConstructionTests
{
    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-decision-model.test.ts::GatewayDecisionModel > URL construction::should post to /decision-model endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_post_to_decision_model_endpoint()
    {
        var (model, capture) = Setup();
        await model.DoDecideAsync(Call());
        Assert.Equal("https://api.test.com/decision-model", capture.Uri);
    }
}
