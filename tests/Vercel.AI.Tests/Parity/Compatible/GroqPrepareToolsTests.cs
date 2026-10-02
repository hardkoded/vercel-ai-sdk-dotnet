// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Groq;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class GroqPrepareToolsTests
{
    [Fact]
    [UpstreamTest("packages/groq/src/groq-prepare-tools.test.ts::prepareTools::should return undefined tools and toolChoice when tools are null", Coverage = UpstreamCoverage.Covered)]
    public void Null_tools_are_omitted()
    {
        var prepared = GroqTools.Prepare(null, null, "gemma2-9b-it");
        Assert.Null(prepared.Tools);
        Assert.Null(prepared.ToolChoice);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-prepare-tools.test.ts::prepareTools::should return undefined tools and toolChoice when tools are empty", Coverage = UpstreamCoverage.Covered)]
    public void Empty_tools_are_omitted()
    {
        var prepared = GroqTools.Prepare(Array.Empty<GroqToolDefinition>(), null, "gemma2-9b-it");
        Assert.Null(prepared.Tools);
        Assert.Null(prepared.ToolChoice);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-prepare-tools.test.ts::prepareTools::should correctly prepare function tools", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_function_tools()
    {
        var prepared = GroqTools.Prepare(new[] { Function("testFunction", "A test function", "{\"type\":\"object\",\"properties\":{}}", null) }, null, "gemma2-9b-it");
        var function = prepared.Tools![0]!["function"]!;
        Assert.Equal("testFunction", function["name"]!.GetValue<string>());
        Assert.Equal("A test function", function["description"]!.GetValue<string>());
        Assert.Equal("object", function["parameters"]!["type"]!.GetValue<string>());
        Assert.Null(function["strict"]);
        Assert.Null(prepared.ToolChoice);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-prepare-tools.test.ts::prepareTools::should add warnings for unsupported provider-defined tools", Coverage = UpstreamCoverage.Covered)]
    public void Warns_for_unsupported_provider_tools()
    {
        var prepared = GroqTools.Prepare(new[] { GroqToolDefinition.Provider("some.unsupported_tool") }, null, "gemma2-9b-it");
        Assert.Empty(prepared.Tools!);
        Assert.Null(prepared.ToolChoice);
        Assert.True(GroqWarnings.Matches(prepared.Warnings[0], "unsupported", "provider-defined tool some.unsupported_tool"));
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-prepare-tools.test.ts::prepareTools::should handle tool choice \"auto\"", Coverage = UpstreamCoverage.Covered)]
    public void Tool_choice_auto()
    {
        var prepared = PrepareChoice(ToolChoice.Auto);
        Assert.Equal("auto", prepared.ToolChoice!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-prepare-tools.test.ts::prepareTools::should handle tool choice \"required\"", Coverage = UpstreamCoverage.Covered)]
    public void Tool_choice_required()
    {
        var prepared = PrepareChoice(ToolChoice.Required);
        Assert.Equal("required", prepared.ToolChoice!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-prepare-tools.test.ts::prepareTools::should handle tool choice \"none\"", Coverage = UpstreamCoverage.Covered)]
    public void Tool_choice_none()
    {
        var prepared = PrepareChoice(ToolChoice.None);
        Assert.Equal("none", prepared.ToolChoice!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-prepare-tools.test.ts::prepareTools::should handle tool choice \"tool\"", Coverage = UpstreamCoverage.Covered)]
    public void Tool_choice_named_tool()
    {
        var prepared = PrepareChoice(ToolChoice.Tool("testFunction"));
        Assert.Equal("function", prepared.ToolChoice!["type"]!.GetValue<string>());
        Assert.Equal("testFunction", prepared.ToolChoice["function"]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-prepare-tools.test.ts::prepareTools > strict mode for function tools::should pass through strict mode when strict is true", Coverage = UpstreamCoverage.Covered)]
    public void Passes_strict_true()
    {
        var prepared = GroqTools.Prepare(new[] { Function("testFunction", "A test function", "{\"type\":\"object\",\"properties\":{}}", true) }, null, "gemma2-9b-it");
        Assert.True(prepared.Tools![0]!["function"]!["strict"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-prepare-tools.test.ts::prepareTools > strict mode for function tools::should pass through strict mode when strict is false", Coverage = UpstreamCoverage.Covered)]
    public void Passes_strict_false()
    {
        var prepared = GroqTools.Prepare(new[] { Function("testFunction", "A test function", "{\"type\":\"object\",\"properties\":{}}", false) }, null, "gemma2-9b-it");
        Assert.False(prepared.Tools![0]!["function"]!["strict"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-prepare-tools.test.ts::prepareTools > strict mode for function tools::should not include strict when strict is undefined", Coverage = UpstreamCoverage.Covered)]
    public void Omits_unset_strict()
    {
        var prepared = GroqTools.Prepare(new[] { Function("testFunction", "A test function", "{\"type\":\"object\",\"properties\":{}}", null) }, null, "gemma2-9b-it");
        Assert.Null(prepared.Tools![0]!["function"]!["strict"]);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-prepare-tools.test.ts::prepareTools > strict mode for function tools::should pass through strict mode for multiple tools with different strict settings", Coverage = UpstreamCoverage.Covered)]
    public void Passes_mixed_strict_settings()
    {
        var prepared = GroqTools.Prepare(
            new[]
            {
                Function("strictTool", "A strict tool", "{\"type\":\"object\",\"properties\":{}}", true),
                Function("nonStrictTool", "A non-strict tool", "{\"type\":\"object\",\"properties\":{}}", false),
                Function("defaultTool", "A tool without strict setting", "{\"type\":\"object\",\"properties\":{}}", null),
            },
            null,
            "gemma2-9b-it");
        Assert.True(prepared.Tools![0]!["function"]!["strict"]!.GetValue<bool>());
        Assert.False(prepared.Tools[1]!["function"]!["strict"]!.GetValue<bool>());
        Assert.Null(prepared.Tools[2]!["function"]!["strict"]);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-prepare-tools.test.ts::prepareTools > browser search tool::should handle browser search tool with supported model", Coverage = UpstreamCoverage.Covered)]
    public void Browser_search_on_supported_model()
    {
        var prepared = GroqTools.Prepare(new[] { GroqToolDefinition.Provider("groq.browser_search") }, null, "openai/gpt-oss-120b");
        Assert.Equal("browser_search", prepared.Tools![0]!["type"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-prepare-tools.test.ts::prepareTools > browser search tool::should warn when browser search is used with unsupported model", Coverage = UpstreamCoverage.Covered)]
    public void Browser_search_warns_on_unsupported_model()
    {
        var prepared = GroqTools.Prepare(new[] { GroqToolDefinition.Provider("groq.browser_search") }, null, "gemma2-9b-it");
        Assert.Empty(prepared.Tools!);
        Assert.True(GroqWarnings.Matches(
            prepared.Warnings[0],
            "unsupported",
            "provider-defined tool groq.browser_search",
            "Browser search is only supported on the following models: openai/gpt-oss-20b, openai/gpt-oss-120b. Current model: gemma2-9b-it"));
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-prepare-tools.test.ts::prepareTools > browser search tool::should handle mixed tools with model validation", Coverage = UpstreamCoverage.Covered)]
    public void Mixes_function_and_browser_search()
    {
        var prepared = GroqTools.Prepare(
            new GroqToolDefinition[]
            {
                Function("test-tool", "A test tool", "{\"type\":\"object\",\"properties\":{}}", null),
                GroqToolDefinition.Provider("groq.browser_search"),
            },
            null,
            "openai/gpt-oss-20b");
        Assert.Equal("function", prepared.Tools![0]!["type"]!.GetValue<string>());
        Assert.Equal("browser_search", prepared.Tools[1]!["type"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-prepare-tools.test.ts::prepareTools > browser search tool::should validate all browser search supported models", Coverage = UpstreamCoverage.Covered)]
    public void Validates_every_browser_search_model()
    {
        foreach (var modelId in new[] { "openai/gpt-oss-20b", "openai/gpt-oss-120b" })
        {
            var prepared = GroqTools.Prepare(new[] { GroqToolDefinition.Provider("groq.browser_search") }, null, modelId);
            Assert.Equal("browser_search", prepared.Tools![0]!["type"]!.GetValue<string>());
            Assert.Empty(prepared.Warnings);
        }
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-prepare-tools.test.ts::prepareTools > browser search tool::should handle browser search with tool choice", Coverage = UpstreamCoverage.Covered)]
    public void Browser_search_keeps_required_tool_choice()
    {
        var prepared = GroqTools.Prepare(new[] { GroqToolDefinition.Provider("groq.browser_search") }, ToolChoice.Required, "openai/gpt-oss-120b");
        Assert.Equal("browser_search", prepared.Tools![0]!["type"]!.GetValue<string>());
        Assert.Equal("required", prepared.ToolChoice!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    private static GroqPreparedTools PrepareChoice(ToolChoice choice)
    {
        return GroqTools.Prepare(new[] { Function("testFunction", "Test", "{}", null) }, choice, "gemma2-9b-it");
    }

    private static GroqToolDefinition Function(string name, string description, string schema, bool? strict)
    {
        using var document = JsonDocument.Parse(schema);
        return GroqToolDefinition.Function(name, description, document.RootElement.Clone(), strict);
    }
}
