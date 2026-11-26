// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Middleware.WrapEmbeddingModel;

/// <summary>Port of <c>wrap-embedding-model.test.ts</c> &gt; <c>wrapEmbeddingModel &gt; maxEmbeddingsPerCall property</c>.</summary>
public sealed class MaxEmbeddingsPerCallPropertyTests
{
    private const string Id =
        "packages/ai/src/middleware/wrap-embedding-model.test.ts::wrapEmbeddingModel > maxEmbeddingsPerCall property::";

    [Fact]
    [UpstreamTest(
        Id + "should pass through by default",
        Coverage = UpstreamCoverage.Partial,
        Note = "MaxEmbeddingsPerCall is a plain int?, so the awaited promise-or-value form upstream is not represented."
    )]
    public void Should_pass_through_by_default()
    {
        var wrappedModel = new MockEmbeddingModel { MaxEmbeddingsPerCall = 2 }.WrapEmbeddingModel(
            new LambdaEmbeddingMiddleware()
        );

        Assert.Equal(2, wrappedModel.MaxEmbeddingsPerCall);
    }

    [Fact]
    [UpstreamTest(
        Id + "should use middleware overrideSupportedUrls if provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_middleware_overrideSupportedUrls_if_provided()
    {
        var wrappedModel = new MockEmbeddingModel { MaxEmbeddingsPerCall = 2 }.WrapEmbeddingModel(
            new LambdaEmbeddingMiddleware { OnOverrideMaxEmbeddingsPerCall = _ => 3 }
        );

        Assert.Equal(3, wrappedModel.MaxEmbeddingsPerCall);
    }
}
