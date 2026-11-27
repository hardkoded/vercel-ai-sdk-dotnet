// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Google.GoogleVertexEmbeddingModel.GoogleVertexEmbeddingModelFixture;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexEmbeddingModel;

/// <summary>Port of <c>google-vertex-embedding-model.test.ts</c> &gt; <c>GoogleVertexEmbeddingModel</c> &gt; <c>response metadata</c>.</summary>
public sealed class ResponseMetadataTests
{
    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-embedding-model.test.ts::GoogleVertexEmbeddingModel > response metadata::should expose the raw response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_expose_the_raw_response()
    {
        var handler = Server();
        handler.ResponseHeaders["test-header"] = "test-value";

        var result = await Model(handler)
            .DoEmbedAsync(
                Call(TestValues, Options("google", MockProviderOptions)),
                CancellationToken.None
            );

        GoogleUpstream.JsonEqual(result.Response!.Body!.Value, Fixture);
        Assert.StartsWith("application/json", result.Response.Headers!["content-type"]);
        Assert.Equal("test-value", result.Response.Headers["test-header"]);
    }
}
