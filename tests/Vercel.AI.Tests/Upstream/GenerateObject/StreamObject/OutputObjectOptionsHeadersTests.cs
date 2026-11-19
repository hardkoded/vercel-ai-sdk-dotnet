// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "object" &gt; options.headers</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectOptionsHeadersTests
{
    private const string Id = Prefix + "output = \"object\" > options.headers::";

    [Fact]
    [UpstreamTest(Id + "should pass headers to model", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers_to_model()
    {
        var model = Model(TextStream("{ \"content\": \"headers test\" }"));
        var options = ObjectOptions(model);
        options.Headers = new Dictionary<string, string?>
        {
            ["custom-request-header"] = "request-header-value",
        };

        var result = Client().StreamObjectAsync(options);

        Assert.Equal(
            ["{\"content\":\"headers test\"}"],
            await CollectRaw(result.PartialObjectStream())
        );
        Assert.Equal(
            new Dictionary<string, string?> { ["custom-request-header"] = "request-header-value" },
            model.Calls[0].Headers
        );
    }
}
