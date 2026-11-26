// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Middleware.WrapEmbeddingModel;

/// <summary>Port of <c>wrap-embedding-model.test.ts</c> &gt; <c>wrapEmbeddingModel &gt; provider property</c>.</summary>
public sealed class ProviderPropertyTests
{
    private const string Id =
        "packages/ai/src/middleware/wrap-embedding-model.test.ts::wrapEmbeddingModel > provider property::";

    [Fact]
    [UpstreamTest(Id + "should pass through by default", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_through_by_default()
    {
        var wrappedModel = new MockEmbeddingModel { Provider = "test-provider" }.WrapEmbeddingModel(
            new LambdaEmbeddingMiddleware()
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
        var wrappedModel = new MockEmbeddingModel { Provider = "test-provider" }.WrapEmbeddingModel(
            new LambdaEmbeddingMiddleware { OnOverrideProvider = _ => "override-provider" }
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
        var wrappedModel = new MockEmbeddingModel { Provider = "test-provider" }.WrapEmbeddingModel(
            new LambdaEmbeddingMiddleware(),
            providerId: "override-provider"
        );

        Assert.Equal("override-provider", wrappedModel.Provider);
    }
}
