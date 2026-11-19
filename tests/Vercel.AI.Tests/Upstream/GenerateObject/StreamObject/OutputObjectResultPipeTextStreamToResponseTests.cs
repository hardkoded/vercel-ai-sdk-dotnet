// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "object" &gt; result.pipeTextStreamToResponse</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectResultPipeTextStreamToResponseTests
{
    private const string Id = Prefix + "output = \"object\" > result.pipeTextStreamToResponse::";

    [Fact]
    [UpstreamTest(
        Id + "should write text deltas to a Node.js response-like object",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_write_text_deltas_to_a_Node_js_response_like_object()
    {
        var response = new ServerResponse();
        var result = Client().StreamObjectAsync(ObjectOptions(CreateTestModel()));

        await result.PipeTextStreamToResponse(response);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", response.Headers["content-type"]);
        Assert.Single(response.Headers);
        Assert.Equal(
            ["{ ", "\"content\": \"Hello, ", "world", "!\"", " }"],
            response.GetDecodedChunks()
        );
    }
}
