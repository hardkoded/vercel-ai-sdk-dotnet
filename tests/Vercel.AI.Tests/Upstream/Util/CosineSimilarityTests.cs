// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI;

namespace Vercel.AI.Tests;

public sealed class CosineSimilarityTests
{
    [UpstreamTest("packages/ai/src/util/cosine-similarity.test.ts::should calculate cosine similarity correctly", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Calculates_close_similarity()
    {
        var result = Ai.CosineSimilarity(new float[] { 1f, 2f, 3f }, new float[] { 4f, 5f, 6f });
        Assert.Equal(0.9746318461970762, result, 5);
    }

    [UpstreamTest("packages/ai/src/util/cosine-similarity.test.ts::should calculate negative cosine similarity correctly", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Calculates_negative_similarity()
    {
        var result = Ai.CosineSimilarity(new float[] { 1f, 0f }, new float[] { -1f, 0f });
        Assert.Equal(-1d, result, 5);
    }

    [UpstreamTest("packages/ai/src/util/cosine-similarity.test.ts::should throw an error when vectors have different lengths", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Throws_when_lengths_differ()
    {
        Assert.ThrowsAny<Exception>(delegate
        {
            Ai.CosineSimilarity(new float[] { 1f, 2f, 3f }, new float[] { 4f, 5f });
        });
    }

    [UpstreamTest("packages/ai/src/util/cosine-similarity.test.ts::should give 0 when one of the vectors is a zero vector", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_zero_for_a_zero_vector()
    {
        Assert.Equal(0d, Ai.CosineSimilarity(new float[] { 0f, 1f, 2f }, new float[] { 0f, 0f, 0f }));
        Assert.Equal(0d, Ai.CosineSimilarity(new float[] { 0f, 0f, 0f }, new float[] { 0f, 1f, 2f }));
    }

    [UpstreamTest("packages/ai/src/util/cosine-similarity.test.ts::should handle vectors with very small magnitudes", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Handles_tiny_magnitudes()
    {
        Assert.Equal(1d, Ai.CosineSimilarity(new float[] { 1e-10f, 0f, 0f }, new float[] { 2e-10f, 0f, 0f }));
        Assert.Equal(-1d, Ai.CosineSimilarity(new float[] { 1e-10f, 0f, 0f }, new float[] { -1e-10f, 0f, 0f }));
    }
}
