// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "no-schema"</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputNoSchemaTests
{
    private const string Id = Prefix + "output = \"no-schema\"::";

    [Fact]
    [UpstreamTest(Id + "should send object deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_object_deltas()
    {
        var model = CreateTestModel(
            TextStream("{ ", "\"content\": ", "\"Hello, ", "world", "!\"", " }")
        );

        var result = Client()
            .StreamObjectAsync(
                new StreamObjectOptions<JsonElement>
                {
                    Model = model,
                    Output = StreamObjectOutput.NoSchema,
                    Prompt = "prompt",
                }
            );

        Assert.Equal(
            [
                "{}",
                "{\"content\":\"Hello, \"}",
                "{\"content\":\"Hello, world\"}",
                "{\"content\":\"Hello, world!\"}",
            ],
            await CollectRaw(result.PartialObjectStream())
        );
        Assert.True(model.Calls[0].JsonResponse);
        Assert.Null(model.Calls[0].JsonSchema);
        Assert.Null(model.Calls[0].JsonSchemaName);
        Assert.Null(model.Calls[0].JsonSchemaDescription);
    }
}
