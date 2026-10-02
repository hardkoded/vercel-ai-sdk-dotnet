// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class PrepareResponsesToolsTests
{
    [Fact]
    public void ShouldPassThroughStrictModeWhenStrictIsTrue()
    {
        var result = OpenAIResponsesLanguageModel.PrepareResponsesTools(
            new[] { FunctionTool("testFunction", "A test function", true) },
            toolChoice: null);

        AssertPrepared(result);
        var tool = Assert.Single(result.Tools!);
        AssertFunctionTool(tool, "testFunction", "A test function", true);
    }

    [Fact]
    public void ShouldPassThroughStrictModeWhenStrictIsFalse()
    {
        var result = OpenAIResponsesLanguageModel.PrepareResponsesTools(
            new[] { FunctionTool("testFunction", "A test function", false) },
            toolChoice: null);

        AssertPrepared(result);
        var tool = Assert.Single(result.Tools!);
        AssertFunctionTool(tool, "testFunction", "A test function", false);
    }

    [Fact]
    public void ShouldDefaultStrictModeToFalseWhenStrictIsUndefined()
    {
        var result = OpenAIResponsesLanguageModel.PrepareResponsesTools(
            new[] { FunctionTool("testFunction", "A test function", null) },
            toolChoice: null);

        AssertPrepared(result);
        var tool = Assert.Single(result.Tools!);
        AssertFunctionTool(tool, "testFunction", "A test function", false);
    }

    [Fact]
    public void ShouldPassThroughStrictModeForMultipleToolsWithDifferentStrictSettings()
    {
        var result = OpenAIResponsesLanguageModel.PrepareResponsesTools(
            new[]
            {
                FunctionTool("strictTool", "A strict tool", true),
                FunctionTool("nonStrictTool", "A non-strict tool", false),
                FunctionTool("defaultTool", "A tool without strict setting", null),
            },
            toolChoice: null);

        AssertPrepared(result);
        Assert.Equal(3, result.Tools!.Count);
        AssertFunctionTool(result.Tools[0], "strictTool", "A strict tool", true);
        AssertFunctionTool(result.Tools[1], "nonStrictTool", "A non-strict tool", false);
        AssertFunctionTool(result.Tools[2], "defaultTool", "A tool without strict setting", false);
    }

    private static void AssertPrepared(PreparedResponsesTools result)
    {
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
    }

    private static void AssertFunctionTool(JsonNode? node, string name, string description, bool strict)
    {
        var tool = Assert.IsType<JsonObject>(node);
        Assert.Equal(5, tool.Count);
        Assert.Equal("function", tool["type"]!.GetValue<string>());
        Assert.Equal(name, tool["name"]!.GetValue<string>());
        Assert.Equal(description, tool["description"]!.GetValue<string>());
        Assert.Equal(strict, tool["strict"]!.GetValue<bool>());
        var parameters = Assert.IsType<JsonObject>(tool["parameters"]);
        Assert.Equal(2, parameters.Count);
        Assert.Equal("object", parameters["type"]!.GetValue<string>());
        var properties = Assert.IsType<JsonObject>(parameters["properties"]);
        Assert.Empty(properties);
    }

    private static LanguageModelTool FunctionTool(string name, string description, bool? strict)
    {
        using var document = JsonDocument.Parse("{\"type\":\"object\",\"properties\":{}}");
        return new LanguageModelTool(name, description, document.RootElement.Clone(), strict);
    }
}
