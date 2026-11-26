// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.GenerateObject;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject.GenerateObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; output = "object" &gt; result.toJsonResponse</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectResultToJsonResponseTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > output = \"object\" > result.toJsonResponse::";

    [Fact]
    [UpstreamTest(Id + "should return JSON response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_JSON_response()
    {
        var result = await Client()
            .GenerateObjectAsync(ObjectOptions(new GenerateObjectTestModel()));

        var response = result.ToJsonResponse();

        Assert.Equal(200, response.Status);
        Assert.Equal("application/json; charset=utf-8", response.Headers.Get("Content-Type"));
        var chunks = (await response.Body.ToArrayAsync()).Select(Encoding.UTF8.GetString);
        Assert.Equal(["{\"content\":\"Hello, world!\"}"], chunks);
    }
}
