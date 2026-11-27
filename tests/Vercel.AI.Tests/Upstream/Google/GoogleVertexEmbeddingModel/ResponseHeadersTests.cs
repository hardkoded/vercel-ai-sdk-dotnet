// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Google.GoogleVertexEmbeddingModel.GoogleVertexEmbeddingModelFixture;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexEmbeddingModel;

/// <summary>Port of <c>google-vertex-embedding-model.test.ts</c> &gt; <c>GoogleVertexEmbeddingModel</c> &gt; <c>response headers</c>.</summary>
public sealed class ResponseHeadersTests
{
    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-embedding-model.test.ts::GoogleVertexEmbeddingModel > response headers::should expose response headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_expose_response_headers()
    {
        var handler = Server();
        handler.ResponseHeaders["test-header"] = "test-value";

        var result = await Model(handler)
            .DoEmbedAsync(
                Call(TestValues, Options("google", MockProviderOptions)),
                CancellationToken.None
            );

        var headers = result.Response!.Headers!;
        Assert.StartsWith("application/json", headers["content-type"]);
        Assert.Equal("test-value", headers["test-header"]);
    }
}
