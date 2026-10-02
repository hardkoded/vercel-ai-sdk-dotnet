// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Upstream <c>google-prepare-tools</c> outcomes.</summary>
public sealed class GooglePrepareToolsTests
{
    private const string File = "packages/google/src/google-prepare-tools.test.ts::";

    [Fact]
    [UpstreamTest(File + "should return undefined tools and tool_choice when tools are null", Coverage = UpstreamCoverage.Covered)]
    public void Returns_no_tools_when_tools_are_null()
    {
        var prepared = GoogleTools.Prepare(null, null, "gemini-2.5-flash", vertex: false);
        Assert.Null(prepared.Tools);
        Assert.Null(prepared.ToolConfig);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "should return undefined tools and tool_choice when tools are empty", Coverage = UpstreamCoverage.Covered)]
    public void Returns_no_tools_when_tools_are_empty()
    {
        var prepared = GoogleTools.Prepare(Array.Empty<GoogleToolSpec>(), null, "gemini-2.5-flash", vertex: false);
        Assert.Null(prepared.Tools);
        Assert.Null(prepared.ToolConfig);
    }

    [Fact]
    [UpstreamTest(File + "should correctly prepare function tools", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_function_tools_as_parameters_json_schema()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleParity.Function("testFunction", "A test function", "{\"type\":\"object\",\"properties\":{}}") },
            null,
            "gemini-2.5-flash",
            vertex: false);
        GoogleParity.Equal(prepared.Tools, """
            [{"functionDeclarations":[{"name":"testFunction","description":"A test function","parametersJsonSchema":{"type":"object","properties":{}}}]}]
            """);
        Assert.Null(prepared.ToolConfig);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "should preserve recursive function tool schemas as JSON Schema", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_recursive_function_schemas()
    {
        const string schema = """
            {"type":"object","properties":{"condition":{"$ref":"#/$defs/Condition"}},"required":["condition"],"$defs":{"Condition":{"type":"object","properties":{"children":{"type":"array","items":{"$ref":"#/$defs/Condition"}}}}}}
            """;
        var prepared = GoogleTools.Prepare(
            new[] { GoogleParity.Function("search", "Search with a condition tree", schema) },
            null,
            "gemini-2.5-flash",
            vertex: false);
        GoogleParity.Equal(prepared.Tools![0]!["functionDeclarations"]![0]!["parametersJsonSchema"]!, schema);
    }

    [Fact]
    [UpstreamTest(File + "should correctly prepare provider-defined tools as array", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_provider_tools_as_an_array()
    {
        var prepared = GoogleTools.Prepare(
            new[]
            {
                GoogleParity.ProviderTool("google.google_search", "google_search"),
                GoogleParity.ProviderTool("google.url_context", "url_context"),
                GoogleParity.ProviderTool("google.file_search", "file_search", "{\"fileSearchStoreNames\":[\"projects/foo/fileSearchStores/bar\"]}"),
            },
            null,
            "gemini-2.5-flash",
            vertex: false);
        GoogleParity.Equal(prepared.Tools, """
            [
              {"googleSearch":{}},
              {"urlContext":{}},
              {"fileSearch":{"fileSearchStoreNames":["projects/foo/fileSearchStores/bar"]}}
            ]
            """);
        Assert.Null(prepared.ToolConfig);
    }

    [Fact]
    [UpstreamTest(File + "should correctly prepare single provider-defined tool", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_a_single_provider_tool()
    {
        var prepared = Prepare(GoogleParity.ProviderTool("google.google_search", "google_search"));
        GoogleParity.Equal(prepared.Tools, "[{\"googleSearch\":{}}]");
        Assert.Null(prepared.ToolConfig);
    }

    [Fact]
    [UpstreamTest(File + "should add warnings for unsupported tools", Coverage = UpstreamCoverage.Covered)]
    public void Warns_for_unsupported_provider_tools()
    {
        var prepared = Prepare(GoogleParity.ProviderTool("unsupported.tool", "unsupported_tool"));
        Assert.Null(prepared.Tools);
        Assert.Equal("unsupported", prepared.Warnings[0].Type);
        Assert.Equal("provider-defined tool unsupported.tool", prepared.Warnings[0].Message);
    }

    [Fact]
    [UpstreamTest(File + "should add warnings for file search on unsupported models", Coverage = UpstreamCoverage.Covered)]
    public void Warns_when_file_search_is_used_on_an_older_model()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleParity.ProviderTool("google.file_search", "file_search", "{\"fileSearchStoreNames\":[\"projects/foo/fileSearchStores/bar\"]}") },
            null,
            "gemini-1.5-flash-8b",
            vertex: false);
        Assert.Null(prepared.Tools);
        Assert.Equal("unsupported", prepared.Warnings[0].Type);
        Assert.Contains("provider-defined tool google.file_search", prepared.Warnings[0].Message, StringComparison.Ordinal);
        Assert.Contains("Gemini 2.5 models and Gemini 3 models", prepared.Warnings[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "should correctly prepare file search tool for gemini-2.5 models", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_file_search_for_Gemini_2_5()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleParity.ProviderTool("google.file_search", "file_search", "{\"fileSearchStoreNames\":[\"projects/foo/fileSearchStores/bar\"],\"metadataFilter\":\"author=Robert Graves\",\"topK\":5}") },
            null,
            "gemini-2.5-flash",
            vertex: false);
        GoogleParity.Equal(prepared.Tools, """
            [{"fileSearch":{"fileSearchStoreNames":["projects/foo/fileSearchStores/bar"],"metadataFilter":"author=Robert Graves","topK":5}}]
            """);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "should correctly prepare file search tool for gemini-3 models", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_file_search_for_Gemini_3()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleParity.ProviderTool("google.file_search", "file_search", "{\"fileSearchStoreNames\":[\"projects/foo/fileSearchStores/bar\"]}") },
            null,
            "gemini-3-flash",
            vertex: false);
        GoogleParity.Equal(prepared.Tools![0]!, "{\"fileSearch\":{\"fileSearchStoreNames\":[\"projects/foo/fileSearchStores/bar\"]}}");
    }

    [Fact]
    [UpstreamTest(File + "should use newest tool support for an unknown future Gemini model", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_newest_tool_support_for_an_unknown_Gemini_model()
    {
        var prepared = GoogleTools.Prepare(
            new[]
            {
                GoogleParity.Function("getWeather", "Get the weather", "{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"string\"}}}"),
                GoogleParity.ProviderTool("google.google_search", "google_search"),
                GoogleParity.ProviderTool("google.enterprise_web_search", "enterprise_web_search"),
                GoogleParity.ProviderTool("google.url_context", "url_context"),
                GoogleParity.ProviderTool("google.code_execution", "code_execution"),
                GoogleParity.ProviderTool("google.file_search", "file_search", "{\"fileSearchStoreNames\":[\"fileSearchStores/example-store\"]}"),
            },
            null,
            "gemini-99-pro-preview",
            vertex: false);
        GoogleParity.Equal(prepared.Tools, """
            [
              {"googleSearch":{}},
              {"enterpriseWebSearch":{}},
              {"urlContext":{}},
              {"codeExecution":{}},
              {"fileSearch":{"fileSearchStoreNames":["fileSearchStores/example-store"]}},
              {"functionDeclarations":[{"name":"getWeather","description":"Get the weather","parametersJsonSchema":{"type":"object","properties":{"location":{"type":"string"}}}}]}
            ]
            """);
        GoogleParity.Equal(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"VALIDATED\"},\"includeServerSideToolInvocations\":true}");
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "should handle tool choice \"auto\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_auto_tool_choice_to_AUTO()
    {
        var prepared = Choice(ToolChoice.Auto);
        GoogleParity.Equal(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"AUTO\"}}");
    }

    [Fact]
    [UpstreamTest(File + "should handle tool choice \"required\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_required_tool_choice_to_ANY()
    {
        var prepared = Choice(ToolChoice.Required);
        GoogleParity.Equal(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"ANY\"}}");
    }

    [Fact]
    [UpstreamTest(File + "should handle tool choice \"none\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_none_tool_choice_to_NONE()
    {
        var prepared = Choice(ToolChoice.None);
        GoogleParity.Equal(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"NONE\"}}");
    }

    [Fact]
    [UpstreamTest(File + "should handle tool choice \"tool\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_a_named_tool_choice_to_ANY_with_allowed_names()
    {
        var prepared = Choice(ToolChoice.Tool("testFunction"));
        GoogleParity.Equal(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"ANY\",\"allowedFunctionNames\":[\"testFunction\"]}}");
    }

    [Fact]
    [UpstreamTest(File + "should warn when mixing function and provider-defined tools", Coverage = UpstreamCoverage.Covered)]
    public void Warns_when_function_and_provider_tools_are_mixed()
    {
        var prepared = Mixed("gemini-2.5-flash", null, vertex: false);
        GoogleParity.Equal(prepared.Tools, "[{\"googleSearch\":{}}]");
        Assert.Null(prepared.ToolConfig);
        Assert.Equal("combination of function and provider-defined tools", prepared.Warnings[0].Message);
    }

    [Fact]
    [UpstreamTest(File + "should handle tool choice with mixed tools (provider-defined tools only)", Coverage = UpstreamCoverage.Covered)]
    public void Drops_function_tools_when_they_are_mixed_on_older_models()
    {
        var prepared = Mixed("gemini-2.5-flash", ToolChoice.Auto, vertex: false);
        GoogleParity.Equal(prepared.Tools, "[{\"googleSearch\":{}}]");
        Assert.Null(prepared.ToolConfig);
    }

    [Fact]
    [UpstreamTest(File + "should combine function and provider-defined tools on Gemini 3 models", Coverage = UpstreamCoverage.Covered)]
    public void Combines_function_and_provider_tools_on_Gemini_3()
    {
        var prepared = Mixed("gemini-3.1-flash-lite-preview", null, vertex: false);
        GoogleParity.Equal(prepared.Tools![0]!, "{\"googleSearch\":{}}");
        Assert.NotNull(prepared.Tools[1]!["functionDeclarations"]);
        GoogleParity.Equal(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"VALIDATED\"},\"includeServerSideToolInvocations\":true}");
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "should omit server-side tool invocation flag for Vertex Gemini 3", Coverage = UpstreamCoverage.Covered)]
    public void Omits_the_server_side_invocation_flag_for_Vertex()
    {
        var prepared = Mixed("gemini-3-flash-preview", null, vertex: true);
        GoogleParity.Equal(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"VALIDATED\"}}");
        Assert.Null(prepared.ToolConfig!["includeServerSideToolInvocations"]);
    }

    [Fact]
    [UpstreamTest(File + "should combine multiple provider tools with function tools on Gemini 3", Coverage = UpstreamCoverage.Covered)]
    public void Combines_multiple_provider_tools_with_function_tools()
    {
        var prepared = GoogleTools.Prepare(
            new[]
            {
                GoogleParity.Function("testFunction", "A test function", "{\"type\":\"object\",\"properties\":{}}"),
                GoogleParity.ProviderTool("google.google_search", "google_search"),
                GoogleParity.ProviderTool("google.url_context", "url_context"),
            },
            null,
            "gemini-3-flash",
            vertex: false);
        Assert.Equal(3, prepared.Tools!.Count);
        Assert.NotNull(prepared.Tools[2]!["functionDeclarations"]);
    }

    [Fact]
    [UpstreamTest(File + "should use VALIDATED mode for combined tools with toolChoice auto on Gemini 3", Coverage = UpstreamCoverage.Covered)]
    public void Uses_VALIDATED_for_combined_tools_when_choice_is_auto()
    {
        var prepared = Mixed("gemini-3-flash", ToolChoice.Auto, vertex: false);
        Assert.Equal("VALIDATED", prepared.ToolConfig!["functionCallingConfig"]!["mode"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "should use ANY mode for combined tools with toolChoice required on Gemini 3", Coverage = UpstreamCoverage.Covered)]
    public void Uses_ANY_for_combined_tools_when_choice_is_required()
    {
        var prepared = Mixed("gemini-3-flash", ToolChoice.Required, vertex: false);
        GoogleParity.Equal(prepared.ToolConfig!["functionCallingConfig"]!, "{\"mode\":\"ANY\"}");
    }

    [Fact]
    [UpstreamTest(File + "should use NONE mode for combined tools with toolChoice none on Gemini 3", Coverage = UpstreamCoverage.Covered)]
    public void Uses_NONE_for_combined_tools_when_choice_is_none()
    {
        var prepared = Mixed("gemini-3-flash", ToolChoice.None, vertex: false);
        Assert.Equal("NONE", prepared.ToolConfig!["functionCallingConfig"]!["mode"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "should use ANY mode with allowedFunctionNames for combined tools with specific tool choice on Gemini 3", Coverage = UpstreamCoverage.Covered)]
    public void Uses_ANY_with_allowed_names_for_a_combined_named_choice()
    {
        var prepared = Mixed("gemini-3-flash", ToolChoice.Tool("testFunction"), vertex: false);
        GoogleParity.Equal(prepared.ToolConfig!["functionCallingConfig"]!, "{\"mode\":\"ANY\",\"allowedFunctionNames\":[\"testFunction\"]}");
        Assert.True(prepared.ToolConfig["includeServerSideToolInvocations"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest(File + "should handle latest modelId for provider-defined tools correctly", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_search_for_gemini_flash_latest()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleParity.ProviderTool("google.google_search", "google_search") },
            null,
            "gemini-flash-latest",
            vertex: false);
        GoogleParity.Equal(prepared.Tools, "[{\"googleSearch\":{}}]");
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "should handle gemini-3 modelId for provider-defined tools correctly", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_search_for_Gemini_3()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleParity.ProviderTool("google.google_search", "google_search") },
            null,
            "gemini-3.1-pro-preview",
            vertex: false);
        GoogleParity.Equal(prepared.Tools, "[{\"googleSearch\":{}}]");
    }

    [Fact]
    [UpstreamTest(File + "should handle code execution tool", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_code_execution()
    {
        var prepared = Prepare(GoogleParity.ProviderTool("google.code_execution", "code_execution"));
        GoogleParity.Equal(prepared.Tools, "[{\"codeExecution\":{}}]");
        Assert.Equal("code_execution", prepared.CodeExecutionName);
    }

    [Fact]
    [UpstreamTest(File + "should handle url context tool alone", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_url_context()
    {
        var prepared = Prepare(GoogleParity.ProviderTool("google.url_context", "url_context"));
        GoogleParity.Equal(prepared.Tools, "[{\"urlContext\":{}}]");
    }

    [Fact]
    [UpstreamTest(File + "should handle google maps tool", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_google_maps()
    {
        var prepared = Prepare(GoogleParity.ProviderTool("google.google_maps", "google_maps"));
        GoogleParity.Equal(prepared.Tools, "[{\"googleMaps\":{}}]");
    }

    [Fact]
    [UpstreamTest(File + "should pass searchTypes args through for google search", Coverage = UpstreamCoverage.Covered)]
    public void Passes_search_types_through()
    {
        var prepared = Prepare(GoogleParity.ProviderTool("google.google_search", "google_search", "{\"searchTypes\":{\"webSearch\":{},\"imageSearch\":{}}}"));
        GoogleParity.Equal(prepared.Tools![0]!, "{\"googleSearch\":{\"searchTypes\":{\"webSearch\":{},\"imageSearch\":{}}}}");
    }

    [Fact]
    [UpstreamTest(File + "should pass timeRangeFilter args through for google search", Coverage = UpstreamCoverage.Covered)]
    public void Passes_a_time_range_filter_through()
    {
        var prepared = Prepare(GoogleParity.ProviderTool("google.google_search", "google_search", "{\"timeRangeFilter\":{\"startTime\":\"2024-01-01T00:00:00Z\",\"endTime\":\"2024-12-31T23:59:59Z\"}}"));
        Assert.Equal("2024-01-01T00:00:00Z", prepared.Tools![0]!["googleSearch"]!["timeRangeFilter"]!["startTime"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "should add warnings for google search on unsupported models", Coverage = UpstreamCoverage.Covered)]
    public void Warns_when_search_is_used_on_Gemini_1()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleParity.ProviderTool("google.google_search", "google_search") },
            null,
            "gemini-1.5-pro",
            vertex: false);
        Assert.Null(prepared.Tools);
        Assert.Contains("Google Search requires Gemini 2.0 or newer.", prepared.Warnings[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "should add warnings for google maps on unsupported models", Coverage = UpstreamCoverage.Covered)]
    public void Warns_when_maps_is_used_on_Gemini_1()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleParity.ProviderTool("google.google_maps", "google_maps") },
            null,
            "gemini-pro",
            vertex: false);
        Assert.Contains("Google Maps grounding tool", prepared.Warnings[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "should use VALIDATED mode when any function tool has strict: true", Coverage = UpstreamCoverage.Covered)]
    public void Uses_VALIDATED_when_a_function_tool_is_strict()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleParity.Function("getWeather", "Get weather", "{\"type\":\"object\"}", strict: true) },
            null,
            "gemini-2.5-flash",
            vertex: false);
        GoogleParity.Equal(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"VALIDATED\"}}");
    }

    [Fact]
    [UpstreamTest(File + "should use VALIDATED mode with toolChoice auto when strict: true", Coverage = UpstreamCoverage.Covered)]
    public void Uses_VALIDATED_for_auto_when_strict()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleParity.Function("getWeather", "Get weather", "{\"type\":\"object\"}", strict: true) },
            ToolChoice.Auto,
            "gemini-2.5-flash",
            vertex: false);
        Assert.Equal("VALIDATED", prepared.ToolConfig!["functionCallingConfig"]!["mode"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "should use ANY mode with toolChoice required when strict: true", Coverage = UpstreamCoverage.Covered)]
    public void Uses_ANY_for_required_when_strict()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleParity.Function("getWeather", "Get weather", "{\"type\":\"object\"}", strict: true) },
            ToolChoice.Required,
            "gemini-3-flash-preview",
            vertex: false);
        Assert.Equal("ANY", prepared.ToolConfig!["functionCallingConfig"]!["mode"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "should use ANY mode with named toolChoice when another tool has strict: true", Coverage = UpstreamCoverage.Covered)]
    public void Uses_ANY_for_a_named_choice_when_another_tool_is_strict()
    {
        var prepared = GoogleTools.Prepare(
            new[]
            {
                GoogleParity.Function("createMeeting", "Create meeting", "{\"type\":\"object\",\"properties\":{\"title\":{\"type\":\"string\"}}}"),
                GoogleParity.Function("getWeather", "Get weather", "{\"type\":\"object\"}", strict: true),
            },
            ToolChoice.Tool("createMeeting"),
            "gemini-3-flash-preview",
            vertex: false);
        GoogleParity.Equal(prepared.ToolConfig!["functionCallingConfig"]!, "{\"mode\":\"ANY\",\"allowedFunctionNames\":[\"createMeeting\"]}");
    }

    [Fact]
    [UpstreamTest(File + "should use AUTO mode when no tools have strict: true", Coverage = UpstreamCoverage.Covered)]
    public void Uses_AUTO_when_no_tool_is_strict()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleParity.Function("getWeather", "Get weather", "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}}}") },
            ToolChoice.Auto,
            "gemini-3-flash-preview",
            vertex: false);
        GoogleParity.Equal(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"AUTO\"}}");
    }

    private static GooglePreparedTools Prepare(GoogleToolSpec tool)
    {
        return GoogleTools.Prepare(new[] { tool }, null, "gemini-2.5-flash", vertex: false);
    }

    private static GooglePreparedTools Choice(ToolChoice choice)
    {
        return GoogleTools.Prepare(
            new[] { GoogleParity.Function("testFunction", "Test", "{}") },
            choice,
            "gemini-2.5-flash",
            vertex: false);
    }

    private static GooglePreparedTools Mixed(string modelId, ToolChoice? choice, bool vertex)
    {
        return GoogleTools.Prepare(
            new[]
            {
                GoogleParity.Function("testFunction", "A test function", "{\"type\":\"object\",\"properties\":{}}"),
                GoogleParity.ProviderTool("google.google_search", "google_search"),
            },
            choice,
            modelId,
            vertex);
    }
}
