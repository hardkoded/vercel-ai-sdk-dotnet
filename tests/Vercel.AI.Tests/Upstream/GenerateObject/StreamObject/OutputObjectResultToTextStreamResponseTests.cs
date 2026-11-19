// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "object" &gt; result.toTextStreamResponse</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectResultToTextStreamResponseTests
{
    private const string Id = Prefix + "output = \"object\" > result.toTextStreamResponse::";

    [Fact]
    [UpstreamTest(
        Id + "should create a Response with a text stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_create_a_Response_with_a_text_stream()
    {
        var result = Client().StreamObjectAsync(ObjectOptions(CreateTestModel()));

        var response = result.ToTextStreamResponse();

        Assert.Equal(200, response.Status);
        Assert.Equal("text/plain; charset=utf-8", response.Headers.Get("Content-Type"));
        var chunks = (await response.Body.ToArrayAsync()).Select(Encoding.UTF8.GetString);
        Assert.Equal(["{ ", "\"content\": \"Hello, ", "world", "!\"", " }"], chunks);
    }
}
