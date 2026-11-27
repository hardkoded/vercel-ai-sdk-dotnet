// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Google.GoogleVertexImageModel.GoogleVertexImageModelTestSupport;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexImageModel;

/// <summary>Port of <c>google-vertex-image-model.test.ts</c> &gt; <c>GoogleVertexImageModel</c>.</summary>
public sealed class GoogleVertexImageModelTests
{
    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-image-model.test.ts::GoogleVertexImageModel::should return 1 for maxImagesPerCall",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_1_for_maxImagesPerCall()
    {
        Assert.Equal(1, Model(new RecordingHandler()).MaxImagesPerCall);
    }
}
