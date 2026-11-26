// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Middleware.WrapImageModel;

/// <summary>Port of <c>wrap-image-model.test.ts</c> &gt; <c>wrapImageModel &gt; provider property</c>.</summary>
public sealed class ProviderPropertyTests
{
    private const string Id =
        "packages/ai/src/middleware/wrap-image-model.test.ts::wrapImageModel > provider property::";

    [Fact]
    [UpstreamTest(Id + "should pass through by default", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_through_by_default()
    {
        var wrappedModel = new MockImageModel { Provider = "test-provider" }.WrapImageModel(
            new LambdaImageMiddleware()
        );

        Assert.Equal("test-provider", wrappedModel.Provider);
    }

    [Fact]
    [UpstreamTest(
        Id + "should use middleware overrideProvider if provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_middleware_overrideProvider_if_provided()
    {
        var wrappedModel = new MockImageModel { Provider = "test-provider" }.WrapImageModel(
            new LambdaImageMiddleware { OnOverrideProvider = _ => "override-provider" }
        );

        Assert.Equal("override-provider", wrappedModel.Provider);
    }

    [Fact]
    [UpstreamTest(
        Id + "should use providerId parameter if provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_providerId_parameter_if_provided()
    {
        var wrappedModel = new MockImageModel { Provider = "test-provider" }.WrapImageModel(
            new LambdaImageMiddleware(),
            providerId: "override-provider"
        );

        Assert.Equal("override-provider", wrappedModel.Provider);
    }
}
