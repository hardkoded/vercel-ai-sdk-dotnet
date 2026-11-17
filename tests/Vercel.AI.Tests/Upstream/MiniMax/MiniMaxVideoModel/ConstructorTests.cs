// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.MiniMaxVideoModel.MiniMaxVideoModelHarness;

namespace Vercel.AI.Tests.MiniMaxVideoModel;

/// <summary>Port of <c>minimax-video-model.test.ts</c> &gt; <c>MiniMaxVideoModel &gt; constructor</c>.</summary>
public sealed class ConstructorTests
{
    [Fact]
    [UpstreamTest(
        "packages/minimax/src/minimax-video-model.test.ts::MiniMaxVideoModel > constructor::should expose correct provider and model information",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_expose_correct_provider_and_model_information()
    {
        var model = CreateModel(Server());

        Assert.Equal("minimax.video", model.Provider);
        Assert.Equal("MiniMax-H3", model.ModelId);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(1, model.MaxVideosPerCall);
    }
}
