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

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; output = "object" &gt; options.headers</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectOptionsHeadersTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > output = \"object\" > options.headers::";

    [Fact]
    [UpstreamTest(Id + "should pass headers to model", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers_to_model()
    {
        var model = new GenerateObjectTestModel
        {
            Generate = call =>
            {
                Assert.Equal(
                    new Dictionary<string, string?>
                    {
                        ["custom-request-header"] = "request-header-value",
                        ["user-agent"] = AiSdkVersion.UserAgent,
                    },
                    call.Headers
                );
                return Result("{ \"content\": \"headers test\" }");
            },
        };
        var options = ObjectOptions(model);
        options.Headers = new Dictionary<string, string?>
        {
            ["custom-request-header"] = "request-header-value",
        };

        var result = await Client().GenerateObjectAsync(options);

        Assert.Equal("headers test", result.Object.Content);
    }
}
