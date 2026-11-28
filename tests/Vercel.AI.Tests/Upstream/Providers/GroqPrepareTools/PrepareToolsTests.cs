// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Groq;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GroqPrepareTools;

/// <summary>Port of <c>groq-prepare-tools.test.ts</c> &gt; <c>prepareTools</c>.</summary>
public sealed class PrepareToolsTests
{
    private const string File = "packages/groq/src/groq-prepare-tools.test.ts";
    private const string Prefix = File + "::prepareTools::";
    private const string StrictPrefix = File + "::prepareTools > strict mode for function tools::";
    private const string BrowserPrefix = File + "::prepareTools > browser search tool::";

    private static JsonElement Schema(string json = "{\"type\":\"object\",\"properties\":{}}")
    {
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    private static LanguageModelTool Function(
        string name,
        string? description,
        JsonElement? schema = null,
        bool? strict = null
    )
    {
        return new LanguageModelTool(name, description, schema ?? Schema(), strict);
    }

    private static LanguageModelTool Provider(string id, string name)
    {
        return LanguageModelTool.ProviderDefined(id, name, Schema("{}"));
    }

    private static void AssertJson(string expected, JsonNode? actual)
    {
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expected), actual),
            $"Expected {expected} but was {actual?.ToJsonString()}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return undefined tools and toolChoice when tools are null",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_undefined_tools_and_toolChoice_when_tools_are_null()
    {
        var result = global::Vercel.AI.Groq.GroqPrepareTools.PrepareTools("gemma2-9b-it", null);

        Assert.Null(result.Tools);
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return undefined tools and toolChoice when tools are empty",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_undefined_tools_and_toolChoice_when_tools_are_empty()
    {
        var result = global::Vercel.AI.Groq.GroqPrepareTools.PrepareTools("gemma2-9b-it", []);

        Assert.Null(result.Tools);
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare function tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_function_tools()
    {
        var result = global::Vercel.AI.Groq.GroqPrepareTools.PrepareTools(
            "gemma2-9b-it",
            [Function("testFunction", "A test function")]
        );

        AssertJson(
            """[{"type":"function","function":{"name":"testFunction","description":"A test function","parameters":{"type":"object","properties":{}}}}]""",
            result.Tools
        );
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should add warnings for unsupported provider-defined tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_add_warnings_for_unsupported_provider_defined_tools()
    {
        var result = global::Vercel.AI.Groq.GroqPrepareTools.PrepareTools(
            "gemma2-9b-it",
            [Provider("some.unsupported_tool", "unsupported_tool")]
        );

        AssertJson("[]", result.Tools);
        Assert.Null(result.ToolChoice);
        var warning = Assert.Single(result.ToolWarnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("provider-defined tool some.unsupported_tool", warning.Feature);
        Assert.Equal("provider-defined tool some.unsupported_tool", warning.Message);
    }

    [Theory]
    [InlineData("auto", "\"auto\"")]
    [InlineData("required", "\"required\"")]
    [InlineData("none", "\"none\"")]
    [InlineData("tool", "{\"type\":\"function\",\"function\":{\"name\":\"testFunction\"}}")]
    [UpstreamTest(
        Prefix + "should handle tool choice \"auto\"",
        Coverage = UpstreamCoverage.Covered
    )]
    [UpstreamTest(
        Prefix + "should handle tool choice \"required\"",
        Coverage = UpstreamCoverage.Covered
    )]
    [UpstreamTest(
        Prefix + "should handle tool choice \"none\"",
        Coverage = UpstreamCoverage.Covered
    )]
    [UpstreamTest(
        Prefix + "should handle tool choice \"tool\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_tool_choice(string type, string expected)
    {
        var toolChoice = type switch
        {
            "auto" => ToolChoice.Auto,
            "required" => ToolChoice.Required,
            "none" => ToolChoice.None,
            _ => ToolChoice.Tool("testFunction"),
        };

        var result = global::Vercel.AI.Groq.GroqPrepareTools.PrepareTools(
            "gemma2-9b-it",
            [Function("testFunction", "Test", Schema("{}"))],
            toolChoice
        );

        AssertJson(expected, result.ToolChoice);
    }

    [Fact]
    [UpstreamTest(
        StrictPrefix + "should pass through strict mode when strict is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_strict_mode_when_strict_is_true()
    {
        var result = global::Vercel.AI.Groq.GroqPrepareTools.PrepareTools(
            "gemma2-9b-it",
            [Function("testFunction", "A test function", strict: true)]
        );

        AssertJson(
            """[{"type":"function","function":{"name":"testFunction","description":"A test function","parameters":{"type":"object","properties":{}},"strict":true}}]""",
            result.Tools
        );
    }

    [Fact]
    [UpstreamTest(
        StrictPrefix + "should pass through strict mode when strict is false",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_strict_mode_when_strict_is_false()
    {
        var result = global::Vercel.AI.Groq.GroqPrepareTools.PrepareTools(
            "gemma2-9b-it",
            [Function("testFunction", "A test function", strict: false)]
        );

        AssertJson(
            """[{"type":"function","function":{"name":"testFunction","description":"A test function","parameters":{"type":"object","properties":{}},"strict":false}}]""",
            result.Tools
        );
    }

    [Fact]
    [UpstreamTest(
        StrictPrefix + "should not include strict when strict is undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_include_strict_when_strict_is_undefined()
    {
        var result = global::Vercel.AI.Groq.GroqPrepareTools.PrepareTools(
            "gemma2-9b-it",
            [Function("testFunction", "A test function")]
        );

        AssertJson(
            """[{"type":"function","function":{"name":"testFunction","description":"A test function","parameters":{"type":"object","properties":{}}}}]""",
            result.Tools
        );
    }

    [Fact]
    [UpstreamTest(
        StrictPrefix
            + "should pass through strict mode for multiple tools with different strict settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_strict_mode_for_multiple_tools_with_different_strict_settings()
    {
        var result = global::Vercel.AI.Groq.GroqPrepareTools.PrepareTools(
            "gemma2-9b-it",
            [
                Function("strictTool", "A strict tool", strict: true),
                Function("nonStrictTool", "A non-strict tool", strict: false),
                Function("defaultTool", "A tool without strict setting"),
            ]
        );

        AssertJson(
            """
            [
              {"type":"function","function":{"name":"strictTool","description":"A strict tool","parameters":{"type":"object","properties":{}},"strict":true}},
              {"type":"function","function":{"name":"nonStrictTool","description":"A non-strict tool","parameters":{"type":"object","properties":{}},"strict":false}},
              {"type":"function","function":{"name":"defaultTool","description":"A tool without strict setting","parameters":{"type":"object","properties":{}}}}
            ]
            """,
            result.Tools
        );
    }

    [Fact]
    [UpstreamTest(
        BrowserPrefix + "should handle browser search tool with supported model",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_browser_search_tool_with_supported_model()
    {
        var result = global::Vercel.AI.Groq.GroqPrepareTools.PrepareTools(
            "openai/gpt-oss-120b",
            [Provider("groq.browser_search", "browser_search")]
        );

        AssertJson("""[{"type":"browser_search"}]""", result.Tools);
        Assert.Empty(result.ToolWarnings);
    }

    [Fact]
    [UpstreamTest(
        BrowserPrefix + "should warn when browser search is used with unsupported model",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_warn_when_browser_search_is_used_with_unsupported_model()
    {
        var result = global::Vercel.AI.Groq.GroqPrepareTools.PrepareTools(
            "gemma2-9b-it",
            [Provider("groq.browser_search", "browser_search")]
        );

        AssertJson("[]", result.Tools);
        var warning = Assert.Single(result.ToolWarnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("provider-defined tool groq.browser_search", warning.Feature);
        Assert.Equal("Browser search is not supported on model gemma2-9b-it.", warning.Message);
    }

    [Fact]
    [UpstreamTest(
        BrowserPrefix + "should handle mixed tools with model validation",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_mixed_tools_with_model_validation()
    {
        var result = global::Vercel.AI.Groq.GroqPrepareTools.PrepareTools(
            "openai/gpt-oss-20b",
            [
                Function("test-tool", "A test tool"),
                Provider("groq.browser_search", "browser_search"),
            ]
        );

        AssertJson(
            """[{"type":"function","function":{"name":"test-tool","description":"A test tool","parameters":{"type":"object","properties":{}}}},{"type":"browser_search"}]""",
            result.Tools
        );
        Assert.Empty(result.ToolWarnings);
    }

    [Theory]
    [InlineData("openai/gpt-oss-240b")]
    [InlineData("future/browser-model")]
    [InlineData("new-model")]
    [UpstreamTest(
        BrowserPrefix + "keeps browser search and function tools for unknown model %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Keeps_browser_search_and_function_tools_for_unknown_model(string modelId)
    {
        var result = global::Vercel.AI.Groq.GroqPrepareTools.PrepareTools(
            modelId,
            [Provider("groq.browser_search", "search"), Function("lookup", null)],
            ToolChoice.Required
        );

        AssertJson(
            """[{"type":"browser_search"},{"type":"function","function":{"name":"lookup","parameters":{"type":"object","properties":{}}}}]""",
            result.Tools
        );
        Assert.Equal("required", result.ToolChoice!.GetValue<string>());
        Assert.Empty(result.ToolWarnings);
    }

    [Fact]
    [UpstreamTest(
        BrowserPrefix + "should validate all browser search supported models",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_validate_all_browser_search_supported_models()
    {
        foreach (var modelId in new[] { "openai/gpt-oss-20b", "openai/gpt-oss-120b" })
        {
            var result = global::Vercel.AI.Groq.GroqPrepareTools.PrepareTools(
                modelId,
                [Provider("groq.browser_search", "browser_search")]
            );

            AssertJson("""[{"type":"browser_search"}]""", result.Tools);
            Assert.Empty(result.ToolWarnings);
        }
    }

    [Fact]
    [UpstreamTest(
        BrowserPrefix + "should handle browser search with tool choice",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_browser_search_with_tool_choice()
    {
        var result = global::Vercel.AI.Groq.GroqPrepareTools.PrepareTools(
            "openai/gpt-oss-120b",
            [Provider("groq.browser_search", "browser_search")],
            ToolChoice.Required
        );

        AssertJson("""[{"type":"browser_search"}]""", result.Tools);
        Assert.Equal("required", result.ToolChoice!.GetValue<string>());
        Assert.Empty(result.ToolWarnings);
    }
}
