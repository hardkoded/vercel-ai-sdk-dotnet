// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Mapper = Vercel.AI.Mistral.MapMistralFinishReason;

namespace Vercel.AI.Tests.Upstream.Mistral.MapMistralFinishReason;

/// <summary>Port of <c>map-mistral-finish-reason.test.ts</c> &gt; <c>mapMistralFinishReason</c>.</summary>
public sealed class MapMistralFinishReasonTests
{
    private const string Prefix =
        "packages/mistral/src/map-mistral-finish-reason.test.ts::mapMistralFinishReason::";

    [Fact]
    [UpstreamTest(Prefix + "maps error to error", Coverage = UpstreamCoverage.Covered)]
    public void Maps_error_to_error()
    {
        Assert.Equal(FinishReason.Error, Mapper.Map("error"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps unknown finish reasons to other",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_unknown_finish_reasons_to_other()
    {
        Assert.Equal(FinishReason.Other, Mapper.Map("unknown"));
    }
}
