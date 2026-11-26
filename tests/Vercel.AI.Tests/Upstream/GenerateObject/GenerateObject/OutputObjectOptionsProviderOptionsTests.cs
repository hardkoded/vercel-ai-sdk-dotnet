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

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; output = "object" &gt; options.providerOptions</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectOptionsProviderOptionsTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > output = \"object\" > options.providerOptions::";

    [Fact]
    [UpstreamTest(
        Id + "should pass provider options to model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_provider_options_to_model()
    {
        var providerOptions = new Dictionary<string, JsonElement>
        {
            ["aProvider"] = JsonSerializer.SerializeToElement(new { someKey = "someValue" }),
        };
        var model = new GenerateObjectTestModel
        {
            Generate = call =>
            {
                Assert.Equal(
                    providerOptions["aProvider"].GetRawText(),
                    call.ProviderOptions!["aProvider"].GetRawText()
                );
                return Result("{ \"content\": \"provider metadata test\" }");
            },
        };
        var options = ObjectOptions(model);
        options.ProviderOptions = providerOptions;

        var result = await Client().GenerateObjectAsync(options);

        Assert.Equal("provider metadata test", result.Object.Content);
    }
}
