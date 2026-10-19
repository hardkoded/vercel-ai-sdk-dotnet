// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;

namespace Vercel.AI.Tests.Upstream.Google.GoogleImageModel;

/// <summary>Port of <c>google-image-model.test.ts</c> &gt; <c>GoogleImageModel &gt; maxImagesPerCall</c>.</summary>
public sealed class MaxImagesPerCallTests
{
    [Fact]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > maxImagesPerCall::should default to a supported per-call limit", Coverage = UpstreamCoverage.Covered)]
    public void Should_default_to_a_supported_per_call_limit()
    {
        var model = GoogleImageModelTestSupport.Model(new RecordingHandler());

        Assert.Equal(1, model.MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > maxImagesPerCall::should respect a custom setting", Coverage = UpstreamCoverage.Covered)]
    public void Should_respect_a_custom_setting()
    {
        var customModel = new Vercel.AI.Google.GoogleImageModel(GoogleImageModelTestSupport.CreateProvider(new RecordingHandler()), "gemini-2.5-flash-image", 5);

        Assert.Equal(5, customModel.MaxImagesPerCall);
    }
}
