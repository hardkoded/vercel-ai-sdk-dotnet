// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Prepare = Vercel.AI.Cohere.CoherePrepareTools;

namespace Vercel.AI.Tests.Upstream.Cohere.CoherePrepareTools;

/// <summary>Port of <c>cohere-prepare-tools.test.ts</c>.</summary>
public sealed class CoherePrepareToolsTests
{
    private const string Prefix = "packages/cohere/src/cohere-prepare-tools.test.ts::";

    [Fact]
    [UpstreamTest(
        Prefix + "should return undefined tools when no tools are provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_undefined_tools_when_no_tools_are_provided()
    {
        var result = Prepare.Prepare(Array.Empty<LanguageModelTool>(), null);

        Assert.Null(result.Tools);
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should process function tools correctly",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_process_function_tools_correctly()
    {
        var result = Prepare.Prepare(new[] { BasicTool() }, null);

        JsonAssert.Equal(result.Tools, ExpectedTools);
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should add warnings for provider-defined tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_add_warnings_for_provider_defined_tools()
    {
        var result = Prepare.Prepare(
            new[]
            {
                LanguageModelTool.ProviderDefined(
                    "provider.tool",
                    "tool",
                    JsonSerializer.Deserialize<JsonElement>("{}")
                ),
            },
            null
        );

        JsonAssert.Equal(result.Tools, "[]");
        Assert.Null(result.ToolChoice);
        var warning = Assert.Single(result.ToolWarnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("provider-defined tool provider.tool", warning.Feature);
    }

    internal const string ExpectedTools =
        "[{\"type\":\"function\",\"function\":{\"name\":\"testFunction\",\"description\":\"test description\",\"parameters\":{\"type\":\"object\",\"properties\":{}}}}]";

    internal static LanguageModelTool BasicTool() =>
        new(
            "testFunction",
            "test description",
            JsonSerializer.Deserialize<JsonElement>("{\"type\":\"object\",\"properties\":{}}")
        );
}
