// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "object" &gt; options.providerOptions</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectOptionsProviderOptionsTests
{
    private const string Id = Prefix + "output = \"object\" > options.providerOptions::";

    [Fact]
    [UpstreamTest(
        Id + "should pass provider options to model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_provider_options_to_model()
    {
        var model = Model(TextStream("{ \"content\": \"provider metadata test\" }"));
        var options = ObjectOptions(model);
        options.ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["aProvider"] = JsonSerializer.SerializeToElement(new { someKey = "someValue" }),
        };

        var result = Client().StreamObjectAsync(options);

        Assert.Equal(
            ["{\"content\":\"provider metadata test\"}"],
            await CollectRaw(result.PartialObjectStream())
        );
        var providerOptions = model.Calls[0].ProviderOptions!;
        Assert.Equal("aProvider", Assert.Single(providerOptions).Key);
        Assert.Equal("{\"someKey\":\"someValue\"}", providerOptions["aProvider"].GetRawText());
    }
}
