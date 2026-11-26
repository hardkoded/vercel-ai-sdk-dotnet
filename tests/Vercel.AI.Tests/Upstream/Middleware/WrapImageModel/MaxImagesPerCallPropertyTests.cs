// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Middleware.WrapImageModel;

/// <summary>Port of <c>wrap-image-model.test.ts</c> &gt; <c>wrapImageModel &gt; maxImagesPerCall property</c>.</summary>
public sealed class MaxImagesPerCallPropertyTests
{
    private const string Id =
        "packages/ai/src/middleware/wrap-image-model.test.ts::wrapImageModel > maxImagesPerCall property::";

    [Fact]
    [UpstreamTest(Id + "should pass through by default", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_through_by_default()
    {
        var wrappedModel = new MockImageModel { MaxImagesPerCall = 2 }.WrapImageModel(
            new LambdaImageMiddleware()
        );

        Assert.Equal(2, wrappedModel.MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest(
        Id + "should use middleware overrideMaxImagesPerCall if provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_middleware_overrideMaxImagesPerCall_if_provided()
    {
        var wrappedModel = new MockImageModel { MaxImagesPerCall = 2 }.WrapImageModel(
            new LambdaImageMiddleware { OnOverrideMaxImagesPerCall = _ => 3 }
        );

        Assert.Equal(3, wrappedModel.MaxImagesPerCall);
    }
}
