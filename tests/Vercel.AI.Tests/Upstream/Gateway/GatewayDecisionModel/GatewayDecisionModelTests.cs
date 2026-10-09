// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Gateway.GatewayDecisionModel.GatewayDecisionModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayDecisionModel;

/// <summary>Port of <c>gateway-decision-model.test.ts</c> &gt; <c>GatewayDecisionModel</c>.</summary>
public sealed class GatewayDecisionModelTests
{
    private const string Prefix = "packages/gateway/src/gateway-decision-model.test.ts::GatewayDecisionModel::";

    [Fact]
    [UpstreamTest(Prefix + "should report all question types as supported", Coverage = UpstreamCoverage.Covered)]
    public void Should_report_all_question_types_as_supported()
    {
        Assert.Equal(new[] { "choice", "score", "boolean" }, Setup().Model.SupportedQuestionTypes);
    }
}
