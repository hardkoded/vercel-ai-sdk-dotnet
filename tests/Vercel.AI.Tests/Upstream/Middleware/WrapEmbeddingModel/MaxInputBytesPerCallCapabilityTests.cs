// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Middleware.WrapEmbeddingModel;

/// <summary>Port of <c>wrap-embedding-model.test.ts</c> &gt; <c>wrapEmbeddingModel &gt; max input bytes per call capability</c>.</summary>
public sealed class MaxInputBytesPerCallCapabilityTests
{
    private const string Id =
        "packages/ai/src/middleware/wrap-embedding-model.test.ts::wrapEmbeddingModel > max input bytes per call capability::";

    [Fact]
    [UpstreamTest(
        Id + "should pass through by default",
        Coverage = UpstreamCoverage.Partial,
        Note = "The upstream capability is a Reflect.get on an experimental symbol property that may be a promise. The port reads IEmbeddingCaller.MaxInputBytesPerCall, a plain double?."
    )]
    public void Should_pass_through_by_default()
    {
        var wrappedModel = new MockEmbeddingModel { MaxInputBytesPerCall = 2 }.WrapEmbeddingModel(
            new LambdaEmbeddingMiddleware()
        );

        Assert.Equal(2, wrappedModel.MaxInputBytesPerCall);
    }
}
