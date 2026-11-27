// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Google.GoogleVertexEmbeddingModel.GoogleVertexEmbeddingModelFixture;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexEmbeddingModel;

/// <summary>Port of <c>google-vertex-embedding-model.test.ts</c> &gt; <c>GoogleVertexEmbeddingModel</c> &gt; <c>embedding</c> &gt; <c>maxEmbeddingsPerCall</c>.</summary>
public sealed class MaxEmbeddingsPerCallTests
{
    private const string Prefix =
        "packages/google-vertex/src/google-vertex-embedding-model.test.ts::GoogleVertexEmbeddingModel > embedding > maxEmbeddingsPerCall::";

    [Fact]
    [UpstreamTest(
        Prefix + "should limit predict endpoint models to 250 values per call",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_limit_predict_endpoint_models_to_250_values_per_call()
    {
        Assert.Equal(250, Model(Server(), "gemini-embedding-001").MaxEmbeddingsPerCall);
    }

    [Theory]
    [InlineData("gemini-embedding-2")]
    [InlineData("gemini-embedding-2-preview")]
    [UpstreamTest(
        Prefix + "should limit %s to one value per call",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_limit_model_to_one_value_per_call(string modelId)
    {
        Assert.Equal(1, Model(Server(), modelId).MaxEmbeddingsPerCall);
    }
}
