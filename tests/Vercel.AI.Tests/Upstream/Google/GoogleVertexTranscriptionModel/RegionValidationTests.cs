// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Google.GoogleVertexTranscriptionModel.GoogleVertexTranscriptionModelFixture;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexTranscriptionModel;

/// <summary>Port of <c>google-vertex-transcription-model.test.ts</c> &gt; <c>region validation</c>.</summary>
public sealed class RegionValidationTests
{
    [Theory]
    [InlineData("googleVertex")]
    [InlineData("vertex")]
    [InlineData("google")]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-transcription-model.test.ts::region validation::rejects an injected per-request region under %s before fetching",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_an_injected_per_request_region_before_fetching(string ns)
    {
        var handler = Server();

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            Model(handler, "chirp_3")
                .DoGenerateAsync(
                    Call(providerOptions: "{\"" + ns + "\":{\"region\":\"user@internal:8080/#\"}}"),
                    CancellationToken.None
                )
        );

        Assert.Equal("region", error.Parameter);
        Assert.Equal(0, handler.Calls);
    }
}
