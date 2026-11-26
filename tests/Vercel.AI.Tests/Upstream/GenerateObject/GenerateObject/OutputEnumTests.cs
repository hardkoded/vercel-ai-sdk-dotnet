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

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; output = "enum"</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputEnumTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > output = \"enum\"::";

    [Fact]
    [UpstreamTest(Id + "should generate an enum value", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_generate_an_enum_value()
    {
        var model = new GenerateObjectTestModel
        {
            Generate = _ => Result("{\"result\":\"sunny\"}"),
        };

        var result = await Client()
            .GenerateObjectAsync(
                new GenerateObjectOptions<string>
                {
                    Model = model,
                    Output = StreamObjectOutput.Enum,
                    EnumValues = ["sunny", "rainy", "snowy"],
                    Prompt = "prompt",
                }
            );

        Assert.Equal("sunny", result.Object);
        var prompt = Assert.IsType<UserModelMessage>(Assert.Single(model.Calls[0].Prompt));
        Assert.Equal("prompt", Assert.IsType<TextContentPart>(Assert.Single(prompt.Content)).Text);
        Assert.True(
            System.Text.Json.Nodes.JsonNode.DeepEquals(
                System.Text.Json.Nodes.JsonNode.Parse(
                    "{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"type\":\"object\",\"properties\":{\"result\":{\"type\":\"string\",\"enum\":[\"sunny\",\"rainy\",\"snowy\"]}},\"required\":[\"result\"],\"additionalProperties\":false}"
                ),
                System.Text.Json.Nodes.JsonNode.Parse(model.Calls[0].JsonSchema!.Value.GetRawText())
            )
        );
    }
}
