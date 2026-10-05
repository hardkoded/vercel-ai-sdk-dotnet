// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class GoogleToolsUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should return undefined tools and tool_choice when tools are null", Coverage = UpstreamCoverage.Covered)]
    public void Returns_nothing_when_tools_are_null()
    {
        var prepared = GoogleTools.Prepare(null, null, null, "gemini-2.5-flash", false);
        Assert.Null(prepared.Tools);
        Assert.Null(prepared.ToolConfig);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should return undefined tools and tool_choice when tools are empty", Coverage = UpstreamCoverage.Covered)]
    public void Returns_nothing_when_tools_are_empty()
    {
        var prepared = GoogleTools.Prepare(Array.Empty<LanguageModelTool>(), Array.Empty<GoogleProviderTool>(), null, "gemini-2.5-flash", false);
        Assert.Null(prepared.Tools);
        Assert.Null(prepared.ToolConfig);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should correctly prepare function tools", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_function_declarations_as_json_schema()
    {
        var prepared = GoogleTools.Prepare(new[] { GoogleUpstream.Function("testFunction", "{\"type\":\"object\",\"properties\":{}}", "A test function") }, null, null, "gemini-2.5-flash", false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"functionDeclarations\":[{\"name\":\"testFunction\",\"description\":\"A test function\",\"parametersJsonSchema\":{\"type\":\"object\",\"properties\":{}}}]}]");
        Assert.Null(prepared.ToolConfig);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should preserve recursive function tool schemas as JSON Schema", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_recursive_tool_schemas()
    {
        const string schema = "{\"type\":\"object\",\"properties\":{\"condition\":{\"$ref\":\"#/$defs/Condition\"}},\"required\":[\"condition\"],\"$defs\":{\"Condition\":{\"type\":\"object\",\"properties\":{\"children\":{\"type\":\"array\",\"items\":{\"$ref\":\"#/$defs/Condition\"}}}}}}";
        var prepared = GoogleTools.Prepare(new[] { GoogleUpstream.Function("search", schema, "Search with a condition tree") }, null, null, "gemini-2.5-flash", false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"functionDeclarations\":[{\"name\":\"search\",\"description\":\"Search with a condition tree\",\"parametersJsonSchema\":" + schema + "}]}]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should correctly prepare provider-defined tools as array", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_several_provider_tools()
    {
        var prepared = GoogleTools.Prepare(null, new[]
        {
            GoogleUpstream.Tool("google.google_search", "{}"),
            GoogleUpstream.Tool("google.url_context", "{}"),
            GoogleUpstream.Tool("google.file_search", "{\"fileSearchStoreNames\":[\"projects/foo/fileSearchStores/bar\"]}"),
        }, null, "gemini-2.5-flash", false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"googleSearch\":{}},{\"urlContext\":{}},{\"fileSearch\":{\"fileSearchStoreNames\":[\"projects/foo/fileSearchStores/bar\"]}}]");
        Assert.Null(prepared.ToolConfig);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should correctly prepare single provider-defined tool", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_one_provider_tool()
    {
        var prepared = GoogleTools.Prepare(null, new[] { GoogleUpstream.Tool("google.google_search", "{}") }, null, "gemini-2.5-flash", false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"googleSearch\":{}}]");
        Assert.Null(prepared.ToolConfig);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should add warnings for unsupported tools", Coverage = UpstreamCoverage.Covered)]
    public void Warns_for_an_unknown_provider_tool()
    {
        var prepared = GoogleTools.Prepare(null, new[] { GoogleUpstream.Tool("unsupported.tool", "{}") }, null, "gemini-2.5-flash", false);
        Assert.Null(prepared.Tools);
        Assert.Null(prepared.ToolConfig);
        Assert.Equal("unsupported", prepared.Warnings[0].Type);
        Assert.Equal("provider-defined tool unsupported.tool", prepared.Warnings[0].Feature);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should add warnings for file search on unsupported models", Coverage = UpstreamCoverage.Covered)]
    public void Warns_when_file_search_is_unsupported()
    {
        var prepared = GoogleTools.Prepare(null, new[] { GoogleUpstream.Tool("google.file_search", "{\"fileSearchStoreNames\":[\"projects/foo/fileSearchStores/bar\"]}") }, null, "gemini-1.5-flash-8b", false);
        Assert.Null(prepared.Tools);
        Assert.Equal("provider-defined tool google.file_search", prepared.Warnings[0].Feature);
        Assert.Equal("The file search tool is only supported with Gemini 2.5 models and Gemini 3 models.", prepared.Warnings[0].Details);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should correctly prepare file search tool for gemini-2.5 models", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_file_search_for_gemini_2_5()
    {
        var prepared = GoogleTools.Prepare(null, new[] { GoogleUpstream.Tool("google.file_search", "{\"fileSearchStoreNames\":[\"projects/foo/fileSearchStores/bar\"],\"metadataFilter\":\"author=Robert Graves\",\"topK\":5}") }, null, "gemini-2.5-pro", false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"fileSearch\":{\"fileSearchStoreNames\":[\"projects/foo/fileSearchStores/bar\"],\"metadataFilter\":\"author=Robert Graves\",\"topK\":5}}]");
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should correctly prepare file search tool for gemini-3 models", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_file_search_for_gemini_3()
    {
        var prepared = GoogleTools.Prepare(null, new[] { GoogleUpstream.Tool("google.file_search", "{\"fileSearchStoreNames\":[\"projects/foo/fileSearchStores/bar\"],\"metadataFilter\":\"author=Robert Graves\",\"topK\":5}") }, null, "gemini-3.1-pro-preview", false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"fileSearch\":{\"fileSearchStoreNames\":[\"projects/foo/fileSearchStores/bar\"],\"metadataFilter\":\"author=Robert Graves\",\"topK\":5}}]");
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should use newest tool support for an unknown future Gemini model", Coverage = UpstreamCoverage.Covered)]
    public void Combines_tools_for_an_unknown_future_model()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleUpstream.Function("getWeather", "{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"string\"}}}", "Get the weather") },
            new[]
            {
                GoogleUpstream.Tool("google.google_search", "{}"),
                GoogleUpstream.Tool("google.enterprise_web_search", "{}"),
                GoogleUpstream.Tool("google.url_context", "{}"),
                GoogleUpstream.Tool("google.code_execution", "{}"),
                GoogleUpstream.Tool("google.file_search", "{\"fileSearchStoreNames\":[\"fileSearchStores/example-store\"]}"),
            },
            null,
            "gemini-99-pro-preview",
            false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"googleSearch\":{}},{\"enterpriseWebSearch\":{}},{\"urlContext\":{}},{\"codeExecution\":{}},{\"fileSearch\":{\"fileSearchStoreNames\":[\"fileSearchStores/example-store\"]}},{\"functionDeclarations\":[{\"name\":\"getWeather\",\"description\":\"Get the weather\",\"parametersJsonSchema\":{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"string\"}}}}]}]");
        GoogleUpstream.JsonEqual(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"VALIDATED\"},\"includeServerSideToolInvocations\":true}");
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should handle tool choice \"auto\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_auto_tool_choice_to_auto_mode()
    {
        var prepared = GoogleTools.Prepare(new[] { GoogleUpstream.Function("testFunction", "{}", "Test") }, null, ToolChoice.Auto, "gemini-2.5-flash", false);
        GoogleUpstream.JsonEqual(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"AUTO\"}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should handle tool choice \"required\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_required_tool_choice_to_any_mode()
    {
        var prepared = GoogleTools.Prepare(new[] { GoogleUpstream.Function("testFunction", "{}", "Test") }, null, ToolChoice.Required, "gemini-2.5-flash", false);
        GoogleUpstream.JsonEqual(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"ANY\"}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should handle tool choice \"none\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_none_tool_choice_and_keeps_declarations()
    {
        var prepared = GoogleTools.Prepare(new[] { GoogleUpstream.Function("testFunction", "{}", "Test") }, null, ToolChoice.None, "gemini-2.5-flash", false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"functionDeclarations\":[{\"name\":\"testFunction\",\"description\":\"Test\",\"parametersJsonSchema\":{}}]}]");
        GoogleUpstream.JsonEqual(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"NONE\"}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should handle tool choice \"tool\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_a_named_tool_choice()
    {
        var prepared = GoogleTools.Prepare(new[] { GoogleUpstream.Function("testFunction", "{}", "Test") }, null, ToolChoice.Tool("testFunction"), "gemini-2.5-flash", false);
        GoogleUpstream.JsonEqual(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"ANY\",\"allowedFunctionNames\":[\"testFunction\"]}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should warn when mixing function and provider-defined tools", Coverage = UpstreamCoverage.Covered)]
    public void Warns_and_keeps_only_provider_tools_before_gemini_3()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleUpstream.Function("testFunction", "{\"type\":\"object\",\"properties\":{}}", "A test function") },
            new[] { GoogleUpstream.Tool("google.google_search", "{}") },
            null, "gemini-2.5-flash", false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"googleSearch\":{}}]");
        Assert.Null(prepared.ToolConfig);
        Assert.Equal("combination of function and provider-defined tools", prepared.Warnings[0].Feature);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should handle tool choice with mixed tools (provider-defined tools only)", Coverage = UpstreamCoverage.Covered)]
    public void Ignores_tool_choice_when_the_mix_is_unsupported()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleUpstream.Function("testFunction", "{\"type\":\"object\",\"properties\":{}}", "A test function") },
            new[] { GoogleUpstream.Tool("google.google_search", "{}") },
            ToolChoice.Auto, "gemini-2.5-flash", false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"googleSearch\":{}}]");
        Assert.Null(prepared.ToolConfig);
        Assert.Equal("unsupported", prepared.Warnings[0].Type);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should combine function and provider-defined tools on Gemini 3 models", Coverage = UpstreamCoverage.Covered)]
    public void Combines_function_and_provider_tools_on_gemini_3()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleUpstream.Function("testFunction", "{\"type\":\"object\",\"properties\":{}}", "A test function") },
            new[] { GoogleUpstream.Tool("google.google_search", "{}") },
            null, "gemini-3.1-flash-lite-preview", false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"googleSearch\":{}},{\"functionDeclarations\":[{\"name\":\"testFunction\",\"description\":\"A test function\",\"parametersJsonSchema\":{\"type\":\"object\",\"properties\":{}}}]}]");
        GoogleUpstream.JsonEqual(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"VALIDATED\"},\"includeServerSideToolInvocations\":true}");
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should omit server-side tool invocation flag for Vertex Gemini 3", Coverage = UpstreamCoverage.Covered)]
    public void Omits_server_side_invocations_on_vertex()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleUpstream.Function("testFunction", "{\"type\":\"object\",\"properties\":{}}", "A test function") },
            new[] { GoogleUpstream.Tool("google.google_search", "{}") },
            null, "gemini-3-flash-preview", true);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"googleSearch\":{}},{\"functionDeclarations\":[{\"name\":\"testFunction\",\"description\":\"A test function\",\"parametersJsonSchema\":{\"type\":\"object\",\"properties\":{}}}]}]");
        GoogleUpstream.JsonEqual(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"VALIDATED\"}}");
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should combine multiple provider tools with function tools on Gemini 3", Coverage = UpstreamCoverage.Covered)]
    public void Combines_several_provider_tools_with_functions()
    {
        var prepared = GoogleTools.Prepare(
            new[]
            {
                GoogleUpstream.Function("getWeather", "{\"type\":\"object\",\"properties\":{}}", "Get weather"),
                GoogleUpstream.Function("bookVenue", "{\"type\":\"object\",\"properties\":{}}", "Book a venue"),
            },
            new[] { GoogleUpstream.Tool("google.google_search", "{}"), GoogleUpstream.Tool("google.google_maps", "{}") },
            null, "gemini-3-flash-preview", false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"googleSearch\":{}},{\"googleMaps\":{}},{\"functionDeclarations\":[{\"name\":\"getWeather\",\"description\":\"Get weather\",\"parametersJsonSchema\":{\"type\":\"object\",\"properties\":{}}},{\"name\":\"bookVenue\",\"description\":\"Book a venue\",\"parametersJsonSchema\":{\"type\":\"object\",\"properties\":{}}}]}]");
        GoogleUpstream.JsonEqual(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"VALIDATED\"},\"includeServerSideToolInvocations\":true}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should use VALIDATED mode for combined tools with toolChoice auto on Gemini 3", Coverage = UpstreamCoverage.Covered)]
    public void Uses_validated_mode_for_combined_auto()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleUpstream.Function("testFunction", "{\"type\":\"object\",\"properties\":{}}", "A test function") },
            new[] { GoogleUpstream.Tool("google.google_search", "{}") },
            ToolChoice.Auto, "gemini-3-flash-preview", false);
        GoogleUpstream.JsonEqual(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"VALIDATED\"},\"includeServerSideToolInvocations\":true}");
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should use ANY mode for combined tools with toolChoice required on Gemini 3", Coverage = UpstreamCoverage.Covered)]
    public void Uses_any_mode_for_combined_required()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleUpstream.Function("testFunction", "{\"type\":\"object\",\"properties\":{}}", "A test function") },
            new[] { GoogleUpstream.Tool("google.google_search", "{}") },
            ToolChoice.Required, "gemini-3-flash-preview", false);
        GoogleUpstream.JsonEqual(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"ANY\"},\"includeServerSideToolInvocations\":true}");
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should use NONE mode for combined tools with toolChoice none on Gemini 3", Coverage = UpstreamCoverage.Covered)]
    public void Uses_none_mode_for_combined_none()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleUpstream.Function("testFunction", "{\"type\":\"object\",\"properties\":{}}", "A test function") },
            new[] { GoogleUpstream.Tool("google.google_search", "{}") },
            ToolChoice.None, "gemini-3-flash-preview", false);
        GoogleUpstream.JsonEqual(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"NONE\"},\"includeServerSideToolInvocations\":true}");
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should use ANY mode with allowedFunctionNames for combined tools with specific tool choice on Gemini 3", Coverage = UpstreamCoverage.Covered)]
    public void Uses_any_mode_for_a_named_combined_choice()
    {
        var prepared = GoogleTools.Prepare(
            new[] { GoogleUpstream.Function("testFunction", "{\"type\":\"object\",\"properties\":{}}", "A test function") },
            new[] { GoogleUpstream.Tool("google.google_search", "{}") },
            ToolChoice.Tool("testFunction"), "gemini-3-flash-preview", false);
        GoogleUpstream.JsonEqual(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"ANY\",\"allowedFunctionNames\":[\"testFunction\"]},\"includeServerSideToolInvocations\":true}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should handle latest modelId for provider-defined tools correctly", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_search_on_gemini_flash_latest()
    {
        var prepared = GoogleTools.Prepare(null, new[] { GoogleUpstream.Tool("google.google_search", "{}") }, null, "gemini-flash-latest", false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"googleSearch\":{}}]");
        Assert.Null(prepared.ToolConfig);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should handle gemini-3 modelId for provider-defined tools correctly", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_search_on_gemini_3()
    {
        var prepared = GoogleTools.Prepare(null, new[] { GoogleUpstream.Tool("google.google_search", "{}") }, null, "gemini-3.1-pro-preview", false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"googleSearch\":{}}]");
        Assert.Null(prepared.ToolConfig);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should handle code execution tool", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_code_execution()
    {
        var prepared = GoogleTools.Prepare(null, new[] { GoogleUpstream.Tool("google.code_execution", "{}") }, null, "gemini-2.5-flash", false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"codeExecution\":{}}]");
        Assert.Null(prepared.ToolConfig);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should handle url context tool alone", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_url_context_alone()
    {
        var prepared = GoogleTools.Prepare(null, new[] { GoogleUpstream.Tool("google.url_context", "{}") }, null, "gemini-2.5-flash", false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"urlContext\":{}}]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should handle google maps tool", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_google_maps()
    {
        var prepared = GoogleTools.Prepare(null, new[] { GoogleUpstream.Tool("google.google_maps", "{}") }, null, "gemini-2.5-flash", false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"googleMaps\":{}}]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should pass searchTypes args through for google search", Coverage = UpstreamCoverage.Covered)]
    public void Passes_search_types_through()
    {
        var prepared = GoogleTools.Prepare(null, new[] { GoogleUpstream.Tool("google.google_search", "{\"searchTypes\":{\"webSearch\":{},\"imageSearch\":{}}}") }, null, "gemini-3.1-flash-image-preview", false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"googleSearch\":{\"searchTypes\":{\"webSearch\":{},\"imageSearch\":{}}}}]");
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should pass timeRangeFilter args through for google search", Coverage = UpstreamCoverage.Covered)]
    public void Passes_the_time_range_filter_through()
    {
        var prepared = GoogleTools.Prepare(null, new[] { GoogleUpstream.Tool("google.google_search", "{\"timeRangeFilter\":{\"startTime\":\"2025-01-01T00:00:00Z\",\"endTime\":\"2025-12-31T23:59:59Z\"}}") }, null, "gemini-2.5-flash", false);
        GoogleUpstream.JsonEqual(prepared.Tools, "[{\"googleSearch\":{\"timeRangeFilter\":{\"startTime\":\"2025-01-01T00:00:00Z\",\"endTime\":\"2025-12-31T23:59:59Z\"}}}]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should add warnings for google search on unsupported models", Coverage = UpstreamCoverage.Covered)]
    public void Warns_when_search_is_unsupported()
    {
        var prepared = GoogleTools.Prepare(null, new[] { GoogleUpstream.Tool("google.google_search", "{}") }, null, "gemini-1.5-flash", false);
        Assert.Null(prepared.Tools);
        Assert.Equal("Google Search requires Gemini 2.0 or newer.", prepared.Warnings[0].Details);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should add warnings for google maps on unsupported models", Coverage = UpstreamCoverage.Covered)]
    public void Warns_when_maps_is_unsupported()
    {
        var prepared = GoogleTools.Prepare(null, new[] { GoogleUpstream.Tool("google.google_maps", "{}") }, null, "gemini-1.5-flash", false);
        Assert.Null(prepared.Tools);
        Assert.Equal("The Google Maps grounding tool is not supported with Gemini models other than Gemini 2 or newer.", prepared.Warnings[0].Details);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should use VALIDATED mode when any function tool has strict: true", Coverage = UpstreamCoverage.Covered)]
    public void Uses_validated_mode_when_a_tool_is_strict()
    {
        var prepared = GoogleTools.Prepare(new[] { GoogleUpstream.Function("createMeeting", "{\"type\":\"object\",\"properties\":{\"title\":{\"type\":\"string\"}},\"required\":[\"title\"],\"additionalProperties\":false}", "Create a meeting", true) }, null, null, "gemini-3-flash-preview", false);
        GoogleUpstream.JsonEqual(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"VALIDATED\"}}");
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should use VALIDATED mode with toolChoice auto when strict: true", Coverage = UpstreamCoverage.Covered)]
    public void Uses_validated_mode_for_strict_auto()
    {
        var prepared = GoogleTools.Prepare(new[] { GoogleUpstream.Function("getWeather", "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"],\"additionalProperties\":false}", "Get weather", true) }, null, ToolChoice.Auto, "gemini-3-flash-preview", false);
        GoogleUpstream.JsonEqual(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"VALIDATED\"}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should use ANY mode with toolChoice required when strict: true", Coverage = UpstreamCoverage.Covered)]
    public void Uses_any_mode_for_strict_required()
    {
        var prepared = GoogleTools.Prepare(new[] { GoogleUpstream.Function("getWeather", "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"],\"additionalProperties\":false}", "Get weather", true) }, null, ToolChoice.Required, "gemini-3-flash-preview", false);
        GoogleUpstream.JsonEqual(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"ANY\"}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should use ANY mode with named toolChoice when another tool has strict: true", Coverage = UpstreamCoverage.Covered)]
    public void Uses_any_mode_for_a_named_choice_when_another_tool_is_strict()
    {
        var prepared = GoogleTools.Prepare(new[]
        {
            GoogleUpstream.Function("createMeeting", "{\"type\":\"object\",\"properties\":{\"title\":{\"type\":\"string\"}},\"required\":[\"title\"],\"additionalProperties\":false}", "Create meeting"),
            GoogleUpstream.Function("getWeather", "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"],\"additionalProperties\":false}", "Get weather", true),
        }, null, ToolChoice.Tool("createMeeting"), "gemini-3-flash-preview", false);
        GoogleUpstream.JsonEqual(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"ANY\",\"allowedFunctionNames\":[\"createMeeting\"]}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-prepare-tools.test.ts::should use AUTO mode when no tools have strict: true", Coverage = UpstreamCoverage.Covered)]
    public void Uses_auto_mode_when_no_tool_is_strict()
    {
        var prepared = GoogleTools.Prepare(new[] { GoogleUpstream.Function("getWeather", "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"],\"additionalProperties\":false}", "Get weather") }, null, ToolChoice.Auto, "gemini-3-flash-preview", false);
        GoogleUpstream.JsonEqual(prepared.ToolConfig, "{\"functionCallingConfig\":{\"mode\":\"AUTO\"}}");
    }
}
