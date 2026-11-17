// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using DeepSeekPrepareToolsType = Vercel.AI.DeepSeek.DeepSeekPrepareTools;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekPrepareTools;

/// <summary>Port of <c>deepseek-prepare-tools.test.ts</c> &gt; <c>prepareTools</c>.</summary>
public sealed class PrepareToolsTests
{
    private const string Prefix =
        "packages/deepseek/src/chat/deepseek-prepare-tools.test.ts::prepareTools::";

    private static LanguageModelTool Tool(
        string name,
        string? description = null,
        bool? strict = null
    )
    {
        return new LanguageModelTool(
            name,
            description,
            UpstreamChat.Json("""{ "type": "object", "properties": {} }"""),
            strict
        );
    }

    private static void AssertSnapshot(DeepSeekPrepareToolsType.Prepared result, string tools)
    {
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
        JsonAssert.Equal(result.Tools, tools);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass through strict mode when strict is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_strict_mode_when_strict_is_true()
    {
        var result = DeepSeekPrepareToolsType.PrepareTools(
            new[] { Tool("testFunction", "A test function", strict: true) },
            null,
            null
        );

        AssertSnapshot(
            result,
            """
            [
              {
                "function": {
                  "description": "A test function",
                  "name": "testFunction",
                  "parameters": { "properties": {}, "type": "object" },
                  "strict": true
                },
                "type": "function"
              }
            ]
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass through strict mode when strict is false",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_strict_mode_when_strict_is_false()
    {
        var result = DeepSeekPrepareToolsType.PrepareTools(
            new[] { Tool("testFunction", "A test function", strict: false) },
            null,
            null
        );

        AssertSnapshot(
            result,
            """
            [
              {
                "function": {
                  "description": "A test function",
                  "name": "testFunction",
                  "parameters": { "properties": {}, "type": "object" },
                  "strict": false
                },
                "type": "function"
              }
            ]
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not include strict mode when strict is undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_include_strict_mode_when_strict_is_undefined()
    {
        var result = DeepSeekPrepareToolsType.PrepareTools(
            new[] { Tool("testFunction", "A test function") },
            null,
            null
        );

        AssertSnapshot(
            result,
            """
            [
              {
                "function": {
                  "description": "A test function",
                  "name": "testFunction",
                  "parameters": { "properties": {}, "type": "object" }
                },
                "type": "function"
              }
            ]
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should pass through strict mode for multiple tools with different strict settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_strict_mode_for_multiple_tools_with_different_strict_settings()
    {
        var result = DeepSeekPrepareToolsType.PrepareTools(
            new[]
            {
                Tool("strictTool", "A strict tool", strict: true),
                Tool("nonStrictTool", "A non-strict tool", strict: false),
                Tool("defaultTool", "A tool without strict setting"),
            },
            null,
            null
        );

        AssertSnapshot(
            result,
            """
            [
              {
                "function": {
                  "description": "A strict tool",
                  "name": "strictTool",
                  "parameters": { "properties": {}, "type": "object" },
                  "strict": true
                },
                "type": "function"
              },
              {
                "function": {
                  "description": "A non-strict tool",
                  "name": "nonStrictTool",
                  "parameters": { "properties": {}, "type": "object" },
                  "strict": false
                },
                "type": "function"
              },
              {
                "function": {
                  "description": "A tool without strict setting",
                  "name": "defaultTool",
                  "parameters": { "properties": {}, "type": "object" }
                },
                "type": "function"
              }
            ]
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject strict tools when the endpoint does not support them",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_strict_tools_when_the_endpoint_does_not_support_them()
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(() =>
            DeepSeekPrepareToolsType.PrepareTools(
                new[] { Tool("strictTool", strict: true) },
                null,
                false
            )
        );

        Assert.Contains(
            "DeepSeek strict tool calls require a beta base URL ending in `/beta`.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject mixed strict and non-strict tools on the beta endpoint",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_mixed_strict_and_non_strict_tools_on_the_beta_endpoint()
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(() =>
            DeepSeekPrepareToolsType.PrepareTools(
                new[] { Tool("strictTool", strict: true), Tool("nonStrictTool") },
                null,
                true
            )
        );

        Assert.Contains(
            "DeepSeek strict mode requires every function tool in the request to set `strict: true`.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept all-strict tools on the beta endpoint",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_accept_all_strict_tools_on_the_beta_endpoint()
    {
        var result = DeepSeekPrepareToolsType.PrepareTools(
            new[] { Tool("firstStrictTool", strict: true), Tool("secondStrictTool", strict: true) },
            null,
            true
        );

        Assert.True(result.Tools!.All(tool => (bool?)tool!["function"]!["strict"] == true));
    }
}
