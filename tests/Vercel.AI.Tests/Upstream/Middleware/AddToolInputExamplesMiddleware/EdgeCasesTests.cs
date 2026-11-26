// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Middleware.AddToolInputExamplesMiddleware.Support;
using Examples = global::Vercel.AI.AddToolInputExamplesMiddleware;

namespace Vercel.AI.Tests.Upstream.Middleware.AddToolInputExamplesMiddleware;

/// <summary>Port of <c>add-tool-input-examples-middleware.test.ts</c> &gt; <c>addToolInputExamplesMiddleware &gt; edge cases</c>.</summary>
public sealed class EdgeCasesTests
{
    private const string Id = FileId + "edge cases::";

    [Fact]
    [UpstreamTest(
        Id + "should pass through tools without inputExamples",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_through_tools_without_inputExamples()
    {
        var schema = LocationSchema;
        var result = await Transform(
            new Examples(),
            new[] { new LanguageModelTool("weather", "Get the weather", schema) }
        );

        var tool = Assert.Single(result.Tools!);
        Assert.Equal("Get the weather", tool.Description);
        Assert.Equal(schema.GetRawText(), tool.InputSchema.GetRawText());
        Assert.Equal("weather", tool.Name);
        Assert.Null(tool.InputExamples);
        Assert.False(tool.IsProviderDefined);
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass through tools with empty inputExamples array",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_through_tools_with_empty_inputExamples_array()
    {
        var result = await Transform(
            new Examples(),
            new[] { Tool("weather", "Get the weather", LocationSchema) }
        );

        Assert.Equal("Get the weather", result.Tools![0].Description);
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass through provider tools unchanged",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_through_provider_tools_unchanged()
    {
        var tool = LanguageModelTool.ProviderDefined(
            "anthropic.web_search_20250305",
            "web_search",
            Json("{\"maxUses\":5}")
        );

        var result = await Transform(new Examples(), new[] { tool });

        var passed = Assert.Single(result.Tools!);
        Assert.True(passed.IsProviderDefined);
        Assert.Equal("anthropic.web_search_20250305", passed.ProviderToolId);
        Assert.Equal("web_search", passed.Name);
        Assert.Equal("{\"maxUses\":5}", passed.Args.GetRawText());
        Assert.Null(passed.InputExamples);
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle multiple tools with mixed examples",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_multiple_tools_with_mixed_examples()
    {
        var timeSchema = Json(
            "{\"type\":\"object\",\"properties\":{\"timezone\":{\"type\":\"string\"}}}"
        );
        var result = await Transform(
            new Examples(),
            new[]
            {
                Tool(
                    "weather",
                    "Get the weather",
                    LocationSchema,
                    Example("{\"location\":\"NYC\"}")
                ),
                new LanguageModelTool("time", "Get the current time", timeSchema),
            }
        );

        Assert.Equal(2, result.Tools!.Count);
        var weather = result.Tools[0];
        Assert.Equal(
            "Get the weather\n\nInput Examples:\n{\"location\":\"NYC\"}",
            weather.Description
        );
        Assert.Null(weather.InputExamples);
        Assert.Equal("weather", weather.Name);
        var time = result.Tools[1];
        Assert.Equal("Get the current time", time.Description);
        Assert.Equal(timeSchema.GetRawText(), time.InputSchema.GetRawText());
        Assert.Equal("time", time.Name);
        Assert.Null(time.InputExamples);
    }

    [Fact]
    [UpstreamTest(Id + "should handle empty tools array", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_empty_tools_array()
    {
        var result = await Transform(new Examples(), Array.Empty<LanguageModelTool>());

        Assert.NotNull(result.Tools);
        Assert.Empty(result.Tools!);
    }

    [Fact]
    [UpstreamTest(Id + "should handle undefined tools", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_undefined_tools()
    {
        var result = await Transform(new Examples(), null);

        Assert.Null(result.Tools);
    }
}
