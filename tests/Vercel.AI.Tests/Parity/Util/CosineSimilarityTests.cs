// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class CosineSimilarityTests
{
    [Fact]
    public void Empty_vectors_have_similarity_zero()
    {
        Assert.Equal(0d, Ai.CosineSimilarity(Array.Empty<float>(), Array.Empty<float>()));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/cosine-similarity.test.ts::should calculate negative cosine similarity correctly",
        Coverage = UpstreamCoverage.Covered)]
    public void Calculates_negative_cosine_similarity()
    {
        var result = Ai.CosineSimilarity(new[] { 1f, 0f }, new[] { -1f, 0f });
        Assert.Equal(-1d, result, 5);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/cosine-similarity.test.ts::should throw an error when vectors have different lengths",
        Coverage = UpstreamCoverage.Covered)]
    public void Throws_when_vector_lengths_differ()
    {
        var error = Assert.Throws<InvalidArgumentException>(
            () => Ai.CosineSimilarity(new[] { 1f, 2f, 3f }, new[] { 4f, 5f }));
        Assert.IsAssignableFrom<ArgumentException>(error);
        Assert.Equal("vector1,vector2", error.Parameter);
        Assert.Equal("Invalid argument for parameter vector1,vector2: Vectors must have the same length", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/cosine-similarity.test.ts::should give 0 when one of the vectors is a zero vector",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_zero_for_a_zero_vector()
    {
        var vector1 = new[] { 0f, 1f, 2f };
        var vector2 = new[] { 0f, 0f, 0f };
        Assert.Equal(0d, Ai.CosineSimilarity(vector1, vector2));
        Assert.Equal(0d, Ai.CosineSimilarity(vector2, vector1));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/cosine-similarity.test.ts::should handle vectors with very small magnitudes",
        Coverage = UpstreamCoverage.Covered)]
    public void Handles_very_small_magnitudes()
    {
        Assert.Equal(1d, Ai.CosineSimilarity(new[] { 1e-10f, 0f, 0f }, new[] { 2e-10f, 0f, 0f }));
        Assert.Equal(-1d, Ai.CosineSimilarity(new[] { 1e-10f, 0f, 0f }, new[] { -1e-10f, 0f, 0f }));
    }
}
