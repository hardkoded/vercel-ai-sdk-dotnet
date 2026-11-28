// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Mistral;
using Vercel.AI.Provider;
using Tools = Vercel.AI.Mistral.MistralPrepareTools;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralPrepareTools;

/// <summary>Port of <c>mistral-prepare-tools.test.ts</c> &gt; <c>prepareTools</c>.</summary>
public sealed class PrepareToolsTests
{
    private const string Prefix =
        "packages/mistral/src/mistral-prepare-tools.test.ts::prepareTools::";

    private static LanguageModelTool Tool(string name, string description, bool? strict = null) =>
        new(
            name,
            description,
            JsonDocument.Parse("{\"type\":\"object\",\"properties\":{}}").RootElement.Clone(),
            strict
        );

    private static string Expected(string name, string description, string? strict) =>
        "{\"function\":{\"description\":\""
        + description
        + "\",\"name\":\""
        + name
        + "\",\"parameters\":{\"properties\":{},\"type\":\"object\"}"
        + (strict == null ? string.Empty : ",\"strict\":" + strict)
        + "},\"type\":\"function\"}";

    private static void AssertResult(MistralPreparedTools result, params string[] expectedTools)
    {
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
        Assert.True(
            JsonNode.DeepEquals(
                result.Tools,
                JsonNode.Parse("[" + string.Join(",", expectedTools) + "]")
            ),
            result.Tools?.ToJsonString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass through strict mode when strict is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_strict_mode_when_strict_is_true()
    {
        var result = Tools.PrepareTools(new[] { Tool("testFunction", "A test function", true) });

        AssertResult(result, Expected("testFunction", "A test function", "true"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass through strict mode when strict is false",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_strict_mode_when_strict_is_false()
    {
        var result = Tools.PrepareTools(new[] { Tool("testFunction", "A test function", false) });

        AssertResult(result, Expected("testFunction", "A test function", "false"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not include strict mode when strict is undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_include_strict_mode_when_strict_is_undefined()
    {
        var result = Tools.PrepareTools(new[] { Tool("testFunction", "A test function") });

        AssertResult(result, Expected("testFunction", "A test function", null));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should pass through strict mode for multiple tools with different strict settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_strict_mode_for_multiple_tools_with_different_strict_settings()
    {
        var result = Tools.PrepareTools(
            new[]
            {
                Tool("strictTool", "A strict tool", true),
                Tool("nonStrictTool", "A non-strict tool", false),
                Tool("defaultTool", "A tool without strict setting"),
            }
        );

        AssertResult(
            result,
            Expected("strictTool", "A strict tool", "true"),
            Expected("nonStrictTool", "A non-strict tool", "false"),
            Expected("defaultTool", "A tool without strict setting", null)
        );
    }
}
