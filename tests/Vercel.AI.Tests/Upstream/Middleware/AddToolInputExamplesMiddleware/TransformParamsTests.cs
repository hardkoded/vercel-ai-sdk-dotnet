// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Middleware.AddToolInputExamplesMiddleware.Support;
using Examples = global::Vercel.AI.AddToolInputExamplesMiddleware;

namespace Vercel.AI.Tests.Upstream.Middleware.AddToolInputExamplesMiddleware;

/// <summary>Port of <c>add-tool-input-examples-middleware.test.ts</c> &gt; <c>addToolInputExamplesMiddleware &gt; transformParams</c>.</summary>
public sealed class TransformParamsTests
{
    private const string Id = FileId + "transformParams::";

    [Fact]
    [UpstreamTest(
        Id + "should append examples to tool description",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_append_examples_to_tool_description()
    {
        var schema = LocationSchema;
        var result = await Transform(
            new Examples(),
            new[]
            {
                Tool(
                    "weather",
                    "Get the weather in a location",
                    schema,
                    Example("{\"location\":\"San Francisco\"}"),
                    Example("{\"location\":\"London\"}")
                ),
            }
        );

        var tool = Assert.Single(result.Tools!);
        Assert.Equal(
            "Get the weather in a location\n\nInput Examples:\n{\"location\":\"San Francisco\"}\n{\"location\":\"London\"}",
            tool.Description
        );
        Assert.Null(tool.InputExamples);
        Assert.Equal(schema.GetRawText(), tool.InputSchema.GetRawText());
        Assert.Equal("weather", tool.Name);
        Assert.False(tool.IsProviderDefined);
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle tool without existing description",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_tool_without_existing_description()
    {
        var schema = LocationSchema;
        var result = await Transform(
            new Examples(),
            new[] { Tool("weather", null, schema, Example("{\"location\":\"Berlin\"}")) }
        );

        var tool = Assert.Single(result.Tools!);
        Assert.Equal("Input Examples:\n{\"location\":\"Berlin\"}", tool.Description);
        Assert.Null(tool.InputExamples);
        Assert.Equal(schema.GetRawText(), tool.InputSchema.GetRawText());
        Assert.Equal("weather", tool.Name);
        Assert.False(tool.IsProviderDefined);
    }
}
