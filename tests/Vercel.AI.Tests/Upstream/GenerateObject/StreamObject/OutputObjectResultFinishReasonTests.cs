// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "object" &gt; result.finishReason</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectResultFinishReasonTests
{
    private const string Id = Prefix + "output = \"object\" > result.finishReason::";

    [Fact]
    [UpstreamTest(Id + "should resolve with finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_resolve_with_finish_reason()
    {
        var result = Client()
            .StreamObjectAsync(
                ObjectOptions(
                    Model(TextStream("{ ", "\"content\": ", "\"Hello, ", "world", "!\"", " }"))
                )
            );

        // consume stream (runs in parallel)
        _ = Collect(result.PartialObjectStream());

        Assert.Equal(FinishReason.Stop, await result.FinishReason);
    }
}
