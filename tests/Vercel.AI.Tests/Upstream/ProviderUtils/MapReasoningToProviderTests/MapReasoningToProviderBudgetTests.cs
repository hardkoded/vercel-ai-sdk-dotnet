// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.MapReasoningToProviderTests;

/// <summary>Port of <c>map-reasoning-to-provider.test.ts</c> &gt; <c>mapReasoningToProviderBudget</c>.</summary>
public sealed class MapReasoningToProviderBudgetTests
{
    [Fact]
    [UpstreamTest("packages/provider-utils/src/map-reasoning-to-provider.test.ts::mapReasoningToProviderBudget::maps max to 95 percent of max output tokens", Coverage = UpstreamCoverage.Covered)]
    public void Maps_max_to_95_percent_of_max_output_tokens()
    {
        var warnings = new List<ModelWarning>();

        var result = ReasoningMap.MapReasoningToProviderBudget("max", 64000, 64000, warnings);

        Assert.Equal(60800, result);
        Assert.Empty(warnings);
    }
}
