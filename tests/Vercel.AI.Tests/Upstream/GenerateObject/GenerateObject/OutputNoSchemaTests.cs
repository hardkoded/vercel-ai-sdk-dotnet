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

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; output = "no-schema"</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputNoSchemaTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > output = \"no-schema\"::";

    [Fact]
    [UpstreamTest(Id + "should generate object", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_generate_object()
    {
        var model = new GenerateObjectTestModel();

        var result = await Client()
            .GenerateObjectAsync(
                new GenerateObjectOptions<JsonElement>
                {
                    Model = model,
                    Output = StreamObjectOutput.NoSchema,
                    Prompt = "prompt",
                }
            );

        Assert.Equal("Hello, world!", result.Object.GetProperty("content").GetString());
        var prompt = Assert.IsType<UserModelMessage>(Assert.Single(model.Calls[0].Prompt));
        Assert.Equal("prompt", Assert.IsType<TextContentPart>(Assert.Single(prompt.Content)).Text);
        Assert.True(model.Calls[0].JsonResponse);
        Assert.Null(model.Calls[0].JsonSchema);
        Assert.Null(model.Calls[0].JsonSchemaName);
        Assert.Null(model.Calls[0].JsonSchemaDescription);
    }
}
