// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "object" &gt; custom schema</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectCustomSchemaTests
{
    private const string Id = Prefix + "output = \"object\" > custom schema::";

    private const string CustomSchema =
        "{\"type\":\"object\",\"properties\":{\"content\":{\"type\":\"string\"}},\"required\":[\"content\"],\"additionalProperties\":false}";

    [Fact]
    [UpstreamTest(Id + "should send object deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_object_deltas()
    {
        var model = CreateTestModel();
        var options = ObjectOptions(model);
        options.Schema = CustomSchema;

        var result = Client().StreamObjectAsync(options);

        Assert.Equal(
            [
                "{}",
                "{\"content\":\"Hello, \"}",
                "{\"content\":\"Hello, world\"}",
                "{\"content\":\"Hello, world!\"}",
            ],
            await CollectRaw(result.PartialObjectStream())
        );
        Assert.Equal(CustomSchema, model.Calls[0].JsonSchema!.Value.GetRawText());
        Assert.Null(model.Calls[0].JsonSchemaName);
        Assert.Null(model.Calls[0].JsonSchemaDescription);
    }
}
