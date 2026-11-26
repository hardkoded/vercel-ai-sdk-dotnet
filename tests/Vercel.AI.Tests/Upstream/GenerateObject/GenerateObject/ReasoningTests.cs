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

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; reasoning</c>.</summary>
[Collection("LogWarnings")]
public sealed class ReasoningTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > reasoning::";

    [Fact]
    [UpstreamTest(
        Id + "should include reasoning in the result",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_reasoning_in_the_result()
    {
        var model = new GenerateObjectTestModel
        {
            Generate = _ =>
                Result(
                    "{ \"content\": \"Hello, world!\" }",
                    new GeneratedReasoning("This is a test reasoning."),
                    new GeneratedReasoning("This is another test reasoning.")
                ),
        };

        var result = await Client().GenerateObjectAsync(ObjectOptions(model));

        Assert.Equal(
            "This is a test reasoning.\nThis is another test reasoning.",
            result.Reasoning
        );
        Assert.Equal("Hello, world!", result.Object.Content);
    }
}
