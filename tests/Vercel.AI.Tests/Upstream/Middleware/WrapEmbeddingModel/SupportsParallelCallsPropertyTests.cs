// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Middleware.WrapEmbeddingModel;

/// <summary>Port of <c>wrap-embedding-model.test.ts</c> &gt; <c>wrapEmbeddingModel &gt; supportsParallelCalls property</c>.</summary>
public sealed class SupportsParallelCallsPropertyTests
{
    private const string Id =
        "packages/ai/src/middleware/wrap-embedding-model.test.ts::wrapEmbeddingModel > supportsParallelCalls property::";

    [Fact]
    [UpstreamTest(
        Id + "should pass through by default",
        Coverage = UpstreamCoverage.Partial,
        Note = "SupportsParallelCalls is a plain bool, so the awaited promise-or-value form upstream is not represented."
    )]
    public void Should_pass_through_by_default()
    {
        var wrappedModel = new MockEmbeddingModel
        {
            SupportsParallelCalls = true,
        }.WrapEmbeddingModel(new LambdaEmbeddingMiddleware());

        Assert.True(wrappedModel.SupportsParallelCalls);
    }

    [Fact]
    [UpstreamTest(
        Id + "should use middleware overrideSupportsParallelCalls if provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_middleware_overrideSupportsParallelCalls_if_provided()
    {
        var wrappedModel = new MockEmbeddingModel
        {
            SupportsParallelCalls = false,
        }.WrapEmbeddingModel(
            new LambdaEmbeddingMiddleware { OnOverrideSupportsParallelCalls = _ => true }
        );

        Assert.True(wrappedModel.SupportsParallelCalls);
    }
}
