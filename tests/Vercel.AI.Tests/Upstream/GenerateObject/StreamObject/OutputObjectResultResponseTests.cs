// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "object" &gt; result.response</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectResultResponseTests
{
    private const string Id = Prefix + "output = \"object\" > result.response::";

    [Fact]
    [UpstreamTest(
        Id + "should resolve with response information",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_resolve_with_response_information()
    {
        var result = Client()
            .StreamObjectAsync(
                ObjectOptions(
                    CreateTestModel(
                        [
                            ResponseMetadata(),
                            new TextStartStreamPart("1"),
                            Delta("{\"content\": \"Hello, world!\"}"),
                            new TextEndStreamPart("1"),
                            Finish(),
                        ],
                        response: new LanguageModelStreamResponse(
                            new Dictionary<string, string> { ["call"] = "2" }
                        )
                    )
                )
            );

        // consume stream (runs in parallel)
        _ = Collect(result.PartialObjectStream());

        var response = await result.Response;
        Assert.Equal("id-0", response.Id);
        Assert.Equal("mock-model-id", response.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(0), response.Timestamp);
        Assert.Equal(new Dictionary<string, string> { ["call"] = "2" }, response.Headers);
    }
}
