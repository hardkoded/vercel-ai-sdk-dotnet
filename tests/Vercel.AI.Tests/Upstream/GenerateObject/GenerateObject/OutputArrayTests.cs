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

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; output = "array"</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputArrayTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > output = \"array\"::";

    private const string ElementSchema =
        "{\"type\":\"object\",\"properties\":{\"content\":{\"type\":\"string\"}},\"required\":[\"content\"],\"additionalProperties\":false}";

    private sealed class ContentResult
    {
        public string Content { get; set; } = string.Empty;
    }

    [Fact]
    [UpstreamTest(
        Id + "should generate an array with 3 elements",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_generate_an_array_with_3_elements()
    {
        var model = new GenerateObjectTestModel
        {
            Generate = _ =>
                Result(
                    "{\"elements\":[{\"content\":\"element 1\"},{\"content\":\"element 2\"},{\"content\":\"element 3\"}]}"
                ),
        };

        var result = await Client()
            .GenerateObjectAsync(
                new GenerateObjectOptions<List<ContentResult>>
                {
                    Model = model,
                    Schema = ElementSchema,
                    Output = StreamObjectOutput.Array,
                    Prompt = "prompt",
                    SerializerOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    },
                }
            );

        Assert.Equal(
            ["element 1", "element 2", "element 3"],
            result.Object.Select(element => element.Content)
        );
        var prompt = Assert.IsType<UserModelMessage>(Assert.Single(model.Calls[0].Prompt));
        Assert.Equal("prompt", Assert.IsType<TextContentPart>(Assert.Single(prompt.Content)).Text);
        Assert.True(
            System.Text.Json.Nodes.JsonNode.DeepEquals(
                System.Text.Json.Nodes.JsonNode.Parse(
                    "{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"type\":\"object\",\"properties\":{\"elements\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"content\":{\"type\":\"string\"}},\"required\":[\"content\"],\"additionalProperties\":false}}},\"required\":[\"elements\"],\"additionalProperties\":false}"
                ),
                System.Text.Json.Nodes.JsonNode.Parse(model.Calls[0].JsonSchema!.Value.GetRawText())
            )
        );
    }
}
