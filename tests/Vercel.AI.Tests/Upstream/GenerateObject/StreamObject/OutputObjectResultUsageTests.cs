// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "object" &gt; result.usage</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectResultUsageTests
{
    private const string Id = Prefix + "output = \"object\" > result.usage::";

    [Fact]
    [UpstreamTest(Id + "should resolve with token usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_resolve_with_token_usage()
    {
        var result = Client()
            .StreamObjectAsync(
                ObjectOptions(
                    CreateTestModel([
                        new TextStartStreamPart("1"),
                        Delta("{ \"content\": \"Hello, world!\" }"),
                        new TextEndStreamPart("1"),
                        Finish(),
                    ])
                )
            );

        // consume stream (runs in parallel)
        _ = Collect(result.PartialObjectStream());

        var usage = await result.Usage;
        Assert.Null(usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(3, usage.NoCacheInputTokens);
        Assert.Equal(3, usage.InputTokens);
        Assert.Null(usage.ReasoningTokens);
        Assert.Equal(10, usage.TextTokens);
        Assert.Equal(10, usage.OutputTokens);
        Assert.Null(usage.Raw);
        Assert.Equal(13, usage.TotalTokens);
    }
}
