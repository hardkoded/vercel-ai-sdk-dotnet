// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Covers Anthropic tool preparation, schema acceptance, and forced-tool fallback.</summary>
public sealed class AnthropicToolsTests
{

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should return undefined tools and tool_choice when tools are null", Coverage = UpstreamCoverage.Covered)]
    public void Null_tools_are_omitted()
    {
        var result = AnthropicTools.Prepare(null, Structured());
        Assert.Null(result.Tools);
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.Warnings);
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should return undefined tools and tool_choice when tools are empty", Coverage = UpstreamCoverage.Covered)]
    public void Empty_tools_are_omitted()
    {
        var result = AnthropicParity.PrepareTools("[]", Structured());
        Assert.Null(result.Tools);
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.Warnings);
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should correctly prepare function tools", Coverage = UpstreamCoverage.Covered)]
    public void Function_tools_keep_schema_and_eager_streaming()
    {
        var result = AnthropicParity.PrepareTools(@"[{
            ""type"":""function"",""name"":""testFunction"",""description"":""A test function"",
            ""inputSchema"":{""type"":""object"",""properties"":{}},
            ""providerOptions"":{""anthropic"":{""eagerInputStreaming"":true}}}]", Structured());
        AnthropicParity.JsonEqual(result.Tools![0], @"{
            ""name"":""testFunction"",""description"":""A test function"",
            ""input_schema"":{""type"":""object"",""properties"":{}},
            ""eager_input_streaming"":true}");
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should correctly preserve tool input examples", Coverage = UpstreamCoverage.Covered)]
    public void Input_examples_add_the_advanced_tool_beta()
    {
        var result = AnthropicParity.PrepareTools(@"[{
            ""type"":""function"",""name"":""tool_with_examples"",""description"":""tool with examples"",
            ""inputSchema"":{""type"":""object"",""properties"":{""a"":{""type"":""number""}}},
            ""inputExamples"":[{""input"":{""a"":1}},{""input"":{""a"":2}}]}]", Structured());
        AnthropicParity.JsonEqual(result.Tools![0]!["input_examples"], @"[{""a"":1},{""a"":2}]");
        Assert.Contains("structured-outputs-2025-11-13", result.Betas);
        Assert.Contains("advanced-tool-use-2025-11-20", result.Betas);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > strict mode for function tools::should include strict and structured-outputs beta when supportsStructuredOutput is true and strict is true", Coverage = UpstreamCoverage.Covered)]
    public void Strict_true_is_sent_with_the_structured_output_beta()
    {
        var result = PrepareFunction("true", structured: true, strictTools: true);
        Assert.True(result.Tools![0]!["strict"]!.GetValue<bool>());
        Assert.Equal(new[] { "structured-outputs-2025-11-13" }, result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > strict mode for function tools::should include beta but not strict property when strict is undefined and supportsStructuredOutput is true", Coverage = UpstreamCoverage.Covered)]
    public void Missing_strict_still_adds_the_structured_output_beta()
    {
        var result = AnthropicParity.PrepareTools(FunctionTool(null), Structured());
        Assert.Null(result.Tools![0]!["strict"]);
        Assert.Equal(new[] { "structured-outputs-2025-11-13" }, result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > strict mode for function tools::should not include strict, emit warning, and not add beta when both supportsStructuredOutput and supportsStrictTools are false", Coverage = UpstreamCoverage.Covered)]
    public void Unsupported_strict_is_dropped_with_a_warning()
    {
        var result = PrepareFunction("true", structured: false, strictTools: false);
        Assert.Null(result.Tools![0]!["strict"]);
        Assert.Empty(result.Betas);
        Assert.Equal("Tool 'testFunction' has strict: true, but strict mode is not supported by this provider. The strict property will be ignored.", result.Warnings[0].Details);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > strict mode for function tools::should include strict but not beta when supportsStructuredOutput is false but supportsStrictTools is true", Coverage = UpstreamCoverage.Covered)]
    public void Strict_tools_without_structured_output_omit_the_beta()
    {
        var result = PrepareFunction("true", structured: false, strictTools: true);
        Assert.True(result.Tools![0]!["strict"]!.GetValue<bool>());
        Assert.Empty(result.Betas);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > strict mode for function tools::should include beta when strict is false and supportsStructuredOutput is true", Coverage = UpstreamCoverage.Covered)]
    public void Strict_false_is_sent_with_the_beta()
    {
        var result = PrepareFunction("false", structured: true, strictTools: true);
        Assert.False(result.Tools![0]!["strict"]!.GetValue<bool>());
        Assert.Contains("structured-outputs-2025-11-13", result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools > computer_20241022::should correctly prepare computer_20241022 tool", Coverage = UpstreamCoverage.Covered)]
    public void Computer_20241022_uses_snake_case_display_fields()
    {
        var result = ProviderTool("anthropic.computer_20241022", @"{""displayWidthPx"":800,""displayHeightPx"":600,""displayNumber"":1}");
        AnthropicParity.JsonEqual(result.Tools![0], @"{""name"":""computer"",""type"":""computer_20241022"",""display_width_px"":800,""display_height_px"":600,""display_number"":1}");
        Assert.Equal(new[] { "computer-use-2024-10-22" }, result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools > computer_20250124::should correctly prepare computer_20250124 tool", Coverage = UpstreamCoverage.Covered)]
    public void Computer_20250124_uses_its_beta()
    {
        var result = ProviderTool("anthropic.computer_20250124", @"{""displayWidthPx"":1024,""displayHeightPx"":768,""displayNumber"":1}");
        Assert.Equal("computer_20250124", result.Tools![0]!["type"]!.GetValue<string>());
        Assert.Equal(new[] { "computer-use-2025-01-24" }, result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools > computer_20251124::should correctly prepare computer_20251124 tool", Coverage = UpstreamCoverage.Covered)]
    public void Computer_20251124_omits_zoom_when_unset()
    {
        var result = ProviderTool("anthropic.computer_20251124", @"{""displayWidthPx"":1024,""displayHeightPx"":768}");
        Assert.Equal("computer_20251124", result.Tools![0]!["type"]!.GetValue<string>());
        Assert.Null(result.Tools![0]!["enable_zoom"]);
        Assert.Equal(new[] { "computer-use-2025-11-24" }, result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools > computer_20251124::should correctly prepare computer_20251124 tool with enableZoom", Coverage = UpstreamCoverage.Covered)]
    public void Computer_20251124_sends_enable_zoom()
    {
        var result = ProviderTool("anthropic.computer_20251124", @"{""displayWidthPx"":1,""displayHeightPx"":1,""enableZoom"":true}");
        Assert.True(result.Tools![0]!["enable_zoom"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools > computer_20251124::should correctly prepare computer_20251124 tool with enableZoom false", Coverage = UpstreamCoverage.Covered)]
    public void Computer_20251124_sends_enable_zoom_false()
    {
        var result = ProviderTool("anthropic.computer_20251124", @"{""displayWidthPx"":1,""displayHeightPx"":1,""enableZoom"":false}");
        Assert.False(result.Tools![0]!["enable_zoom"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools > text_editor_20241022::should correctly prepare text_editor_20241022 tool", Coverage = UpstreamCoverage.Covered)]
    public void Text_editor_20241022_is_str_replace_editor()
    {
        var result = ProviderTool("anthropic.text_editor_20241022", "{}");
        AnthropicParity.JsonEqual(result.Tools![0], @"{""name"":""str_replace_editor"",""type"":""text_editor_20241022""}");
        Assert.Equal(new[] { "computer-use-2024-10-22" }, result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::should correctly prepare bash_20241022 tool", Coverage = UpstreamCoverage.Covered)]
    public void Bash_20241022_uses_the_computer_use_beta()
    {
        var result = ProviderTool("anthropic.bash_20241022", "{}");
        AnthropicParity.JsonEqual(result.Tools![0], @"{""name"":""bash"",""type"":""bash_20241022""}");
        Assert.Equal(new[] { "computer-use-2024-10-22" }, result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::should correctly prepare text_editor_20250728 with max_characters", Coverage = UpstreamCoverage.Covered)]
    public void Text_editor_20250728_sends_max_characters()
    {
        var result = ProviderTool("anthropic.text_editor_20250728", @"{""maxCharacters"":1000}");
        Assert.Equal(1000, result.Tools![0]!["max_characters"]!.GetValue<int>());
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::should correctly prepare text_editor_20250728 without max_characters", Coverage = UpstreamCoverage.Covered)]
    public void Text_editor_20250728_omits_max_characters()
    {
        var result = ProviderTool("anthropic.text_editor_20250728", "{}");
        Assert.Null(result.Tools![0]!["max_characters"]);
        Assert.Equal("str_replace_based_edit_tool", result.Tools![0]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::should correctly prepare web_search_20250305", Coverage = UpstreamCoverage.Covered)]
    public void Web_search_20250305_has_no_beta()
    {
        var result = ProviderTool("anthropic.web_search_20250305", @"{""maxUses"":10,""allowedDomains"":[""https://www.google.com""],""userLocation"":{""type"":""approximate"",""city"":""New York""}}");
        AnthropicParity.JsonEqual(result.Tools![0], @"{
            ""name"":""web_search"",""type"":""web_search_20250305"",""max_uses"":10,
            ""allowed_domains"":[""https://www.google.com""],
            ""user_location"":{""type"":""approximate"",""city"":""New York""}}");
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::should correctly prepare web_search_20260209", Coverage = UpstreamCoverage.Covered)]
    public void Web_search_20260209_adds_the_web_tools_beta()
    {
        var result = ProviderTool("anthropic.web_search_20260209", @"{""maxUses"":10}");
        Assert.Equal("web_search_20260209", result.Tools![0]!["type"]!.GetValue<string>());
        Assert.Equal(new[] { "code-execution-web-tools-2026-02-09" }, result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::should correctly prepare web_search_20260318 without a beta header", Coverage = UpstreamCoverage.Covered)]
    public void Web_search_20260318_sends_response_inclusion()
    {
        var result = ProviderTool("anthropic.web_search_20260318", @"{""maxUses"":10,""responseInclusion"":""excluded""}");
        Assert.Equal("excluded", result.Tools![0]!["response_inclusion"]!.GetValue<string>());
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::should correctly prepare web_fetch_20250910", Coverage = UpstreamCoverage.Covered)]
    public void Web_fetch_20250910_sends_citations_and_max_content_tokens()
    {
        var result = ProviderTool("anthropic.web_fetch_20250910", @"{""maxUses"":10,""citations"":{""enabled"":true},""maxContentTokens"":1000}");
        Assert.Equal(1000, result.Tools![0]!["max_content_tokens"]!.GetValue<int>());
        Assert.True(result.Tools![0]!["citations"]!["enabled"]!.GetValue<bool>());
        Assert.Equal(new[] { "web-fetch-2025-09-10" }, result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::should correctly prepare web_fetch_20260209", Coverage = UpstreamCoverage.Covered)]
    public void Web_fetch_20260209_uses_the_web_tools_beta()
    {
        var result = ProviderTool("anthropic.web_fetch_20260209", @"{""maxUses"":1}");
        Assert.Equal("web_fetch_20260209", result.Tools![0]!["type"]!.GetValue<string>());
        Assert.Equal(new[] { "code-execution-web-tools-2026-02-09" }, result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::should correctly prepare web_fetch_20260318 without a beta header", Coverage = UpstreamCoverage.Covered)]
    public void Web_fetch_20260318_sends_use_cache()
    {
        var result = ProviderTool("anthropic.web_fetch_20260318", @"{""useCache"":true,""responseInclusion"":""excluded""}");
        Assert.True(result.Tools![0]!["use_cache"]!.GetValue<bool>());
        Assert.Equal("excluded", result.Tools![0]!["response_inclusion"]!.GetValue<string>());
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::should correctly prepare tool_search_regex_20251119", Coverage = UpstreamCoverage.Covered)]
    public void Tool_search_regex_has_no_beta()
    {
        var result = ProviderTool("anthropic.tool_search_regex_20251119", "{}");
        AnthropicParity.JsonEqual(result.Tools![0], @"{""name"":""tool_search_tool_regex"",""type"":""tool_search_tool_regex_20251119""}");
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::should correctly prepare code_execution_20260120 without beta header", Coverage = UpstreamCoverage.Covered)]
    public void Code_execution_20260120_has_no_beta()
    {
        var result = ProviderTool("anthropic.code_execution_20260120", "{}");
        AnthropicParity.JsonEqual(result.Tools![0], @"{""name"":""code_execution"",""type"":""code_execution_20260120""}");
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::should correctly prepare tool_search_bm25_20251119", Coverage = UpstreamCoverage.Covered)]
    public void Tool_search_bm25_has_no_beta()
    {
        var result = ProviderTool("anthropic.tool_search_bm25_20251119", "{}");
        Assert.Equal("tool_search_tool_bm25", result.Tools![0]!["name"]!.GetValue<string>());
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::should correctly prepare advisor_20260301 with only the required model", Coverage = UpstreamCoverage.Covered)]
    public void Advisor_requires_only_a_model()
    {
        var result = ProviderTool("anthropic.advisor_20260301", @"{""model"":""claude-opus-4-7""}");
        AnthropicParity.JsonEqual(result.Tools![0], @"{""name"":""advisor"",""type"":""advisor_20260301"",""model"":""claude-opus-4-7""}");
        Assert.Equal(new[] { "advisor-tool-2026-03-01" }, result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::should correctly prepare advisor_20260301 with all optional args", Coverage = UpstreamCoverage.Covered)]
    public void Advisor_optional_args_are_snake_case()
    {
        var result = ProviderTool("anthropic.advisor_20260301", @"{""model"":""claude-opus-4-7"",""maxUses"":5,""maxTokens"":2048,""caching"":{""type"":""ephemeral"",""ttl"":""1h""}}");
        AnthropicParity.JsonEqual(result.Tools![0], @"{
            ""name"":""advisor"",""type"":""advisor_20260301"",""model"":""claude-opus-4-7"",
            ""max_uses"":5,""max_tokens"":2048,""caching"":{""type"":""ephemeral"",""ttl"":""1h""}}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::should reject advisor_20260301 maxTokens below 1024", Coverage = UpstreamCoverage.Covered)]
    public void Advisor_max_tokens_below_1024_is_rejected()
    {
        var error = Assert.Throws<AiSdkException>(() => ProviderTool("anthropic.advisor_20260301", @"{""model"":""claude-opus-4-7"",""maxTokens"":100}"));
        Assert.StartsWith("AI_TypeValidationError:", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::should reject non-integer advisor_20260301 maxTokens", Coverage = UpstreamCoverage.Covered)]
    public void Advisor_max_tokens_must_be_an_integer()
    {
        var error = Assert.Throws<AiSdkException>(() => ProviderTool("anthropic.advisor_20260301", @"{""model"":""claude-opus-4-7"",""maxTokens"":1024.5}"));
        Assert.Contains("maxTokens", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > deferLoading for function tools::should include defer_loading when set to true", Coverage = UpstreamCoverage.Covered)]
    public void Defer_loading_true_is_sent()
    {
        var result = AnthropicParity.PrepareTools(FunctionWithAnthropic(@"{""deferLoading"":true}"), Structured());
        Assert.True(result.Tools![0]!["defer_loading"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > deferLoading for function tools::should include defer_loading when set to false", Coverage = UpstreamCoverage.Covered)]
    public void Defer_loading_false_is_sent()
    {
        var result = AnthropicParity.PrepareTools(FunctionWithAnthropic(@"{""deferLoading"":false}"), Structured());
        Assert.False(result.Tools![0]!["defer_loading"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > deferLoading for function tools::should not include defer_loading when not specified", Coverage = UpstreamCoverage.Covered)]
    public void Defer_loading_is_omitted_when_unset()
    {
        var result = AnthropicParity.PrepareTools(FunctionTool(null), Structured());
        Assert.Null(result.Tools![0]!["defer_loading"]);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > allowedCallers for function tools (programmatic tool calling)::should include allowed_callers and advanced-tool-use beta when allowedCallers is set", Coverage = UpstreamCoverage.Covered)]
    public void Allowed_callers_add_the_advanced_tool_beta()
    {
        var result = AnthropicParity.PrepareTools(FunctionWithAnthropic(@"{""allowedCallers"":[""code_execution_20250825""]}"), Structured());
        AnthropicParity.JsonEqual(result.Tools![0]!["allowed_callers"], @"[""code_execution_20250825""]");
        Assert.Contains("advanced-tool-use-2025-11-20", result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > allowedCallers for function tools (programmatic tool calling)::should not include allowed_callers when not specified", Coverage = UpstreamCoverage.Covered)]
    public void Allowed_callers_are_omitted_when_unset()
    {
        var result = AnthropicParity.PrepareTools(FunctionTool(null), Structured());
        Assert.Null(result.Tools![0]!["allowed_callers"]);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > allowedCallers for function tools (programmatic tool calling)::should include both deferLoading and allowedCallers when both are set", Coverage = UpstreamCoverage.Covered)]
    public void Defer_loading_and_allowed_callers_are_both_sent()
    {
        var result = AnthropicParity.PrepareTools(FunctionWithAnthropic(@"{""deferLoading"":true,""allowedCallers"":[""code_execution_20250825""]}"), Structured());
        Assert.True(result.Tools![0]!["defer_loading"]!.GetValue<bool>());
        Assert.NotNull(result.Tools![0]!["allowed_callers"]);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > allowedCallers for function tools (programmatic tool calling)::should include allowed_callers with code_execution_20260120", Coverage = UpstreamCoverage.Covered)]
    public void Allowed_callers_accept_code_execution_20260120()
    {
        var result = AnthropicParity.PrepareTools(FunctionWithAnthropic(@"{""allowedCallers"":[""code_execution_20260120""]}"), Structured());
        AnthropicParity.JsonEqual(result.Tools![0]!["allowed_callers"], @"[""code_execution_20260120""]");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should add warnings for unsupported tools", Coverage = UpstreamCoverage.Covered)]
    public void Unknown_provider_tools_warn_and_are_omitted()
    {
        var result = AnthropicParity.PrepareTools(@"[{""type"":""provider"",""id"":""unsupported.tool"",""name"":""unsupported_tool"",""args"":{}}]", Structured());
        Assert.Empty(result.Tools!);
        Assert.Null(result.ToolChoice);
        Assert.Equal("provider-defined tool unsupported.tool", result.Warnings[0].Feature);
        Assert.Equal("unsupported", result.Warnings[0].Type);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should handle tool choice \"auto\"", Coverage = UpstreamCoverage.Covered)]
    public void Tool_choice_auto_is_auto()
    {
        var result = AnthropicParity.PrepareTools(FunctionTool(null), Choice("auto"));
        AnthropicParity.JsonEqual(result.ToolChoice, @"{""type"":""auto""}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should handle tool choice \"required\"", Coverage = UpstreamCoverage.Covered)]
    public void Tool_choice_required_is_any()
    {
        var result = AnthropicParity.PrepareTools(FunctionTool(null), Choice("required"));
        AnthropicParity.JsonEqual(result.ToolChoice, @"{""type"":""any""}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should handle tool choice \"none\"", Coverage = UpstreamCoverage.Covered)]
    public void Tool_choice_none_omits_tools()
    {
        var result = AnthropicParity.PrepareTools(FunctionTool(null), Choice("none"));
        Assert.Null(result.Tools);
        Assert.Null(result.ToolChoice);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should handle tool choice \"tool\"", Coverage = UpstreamCoverage.Covered)]
    public void Tool_choice_tool_names_the_tool()
    {
        var options = Choice("tool");
        var result = AnthropicParity.PrepareTools(FunctionTool(null), options);
        AnthropicParity.JsonEqual(result.ToolChoice, @"{""type"":""tool"",""name"":""testFunction""}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should set cache control", Coverage = UpstreamCoverage.Covered)]
    public void Function_tool_cache_control_is_copied()
    {
        var result = AnthropicParity.PrepareTools(FunctionWithAnthropic(@"{""cacheControl"":{""type"":""ephemeral""}}"), Structured());
        AnthropicParity.JsonEqual(result.Tools![0]!["cache_control"], @"{""type"":""ephemeral""}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should limit cache breakpoints to 4", Coverage = UpstreamCoverage.Covered)]
    public void Fifth_tool_cache_breakpoint_is_dropped()
    {
        var tools = new System.Text.StringBuilder("[");
        for (var i = 1; i <= 5; i++)
        {
            if (i > 1)
            {
                tools.Append(',');
            }

            tools.Append(@"{""type"":""function"",""name"":""tool").Append(i).Append(@""",""description"":""Test"",""inputSchema"":{},""providerOptions"":{""anthropic"":{""cacheControl"":{""type"":""ephemeral""}}}}");
        }

        tools.Append(']');
        var validator = new AnthropicCacheControlValidator();
        var result = AnthropicParity.PrepareTools(tools.ToString(), new AnthropicToolPrepareOptions { CacheControl = validator });
        Assert.NotNull(result.Tools![3]!["cache_control"]);
        Assert.Null(result.Tools![4]!["cache_control"]);
        Assert.Contains(validator.Warnings, warning => warning.Details == "Maximum 4 cache breakpoints exceeded (found 5). This breakpoint will be ignored.");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::webFetch_20250910OutputSchema::should not fail validation when title is null", Coverage = UpstreamCoverage.Covered)]
    public void Web_fetch_output_allows_a_null_title()
    {
        Assert.True(AnthropicToolSchemas.AcceptWebFetchOutput(AnthropicParity.Element(WebFetchOutput("null"))));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::webFetch_20250910OutputSchema::should accept valid response with string title", Coverage = UpstreamCoverage.Covered)]
    public void Web_fetch_output_allows_a_string_title()
    {
        Assert.True(AnthropicToolSchemas.AcceptWebFetchOutput(AnthropicParity.Element(WebFetchOutput(@"""Example Title"""))));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::webFetch_20260209OutputSchema::should not fail validation when title is null", Coverage = UpstreamCoverage.Covered)]
    public void Web_fetch_20260209_output_allows_a_null_title()
    {
        Assert.True(AnthropicToolSchemas.AcceptWebFetchOutput(AnthropicParity.Element(WebFetchOutput("null"))));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::webFetch_20260318OutputSchema::should not fail validation when title is null", Coverage = UpstreamCoverage.Covered)]
    public void Web_fetch_20260318_output_allows_a_null_title()
    {
        Assert.True(AnthropicToolSchemas.AcceptWebFetchOutput(AnthropicParity.Element(WebFetchOutput("null", "2026-09-15T20:00:00Z"))));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::webSearch_20250305OutputSchema::should not fail validation when title is null", Coverage = UpstreamCoverage.Covered)]
    public void Web_search_output_allows_a_null_title()
    {
        Assert.True(AnthropicToolSchemas.AcceptWebSearchOutput(AnthropicParity.Element(WebSearchOutput("null"))));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::webSearch_20250305OutputSchema::should accept valid response with string title", Coverage = UpstreamCoverage.Covered)]
    public void Web_search_output_allows_a_string_title()
    {
        Assert.True(AnthropicToolSchemas.AcceptWebSearchOutput(AnthropicParity.Element(WebSearchOutput(@"""Example"""))));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::webSearch_20260209OutputSchema::should not fail validation when title is null", Coverage = UpstreamCoverage.Covered)]
    public void Web_search_20260209_output_allows_a_null_title()
    {
        Assert.True(AnthropicToolSchemas.AcceptWebSearchOutput(AnthropicParity.Element(WebSearchOutput("null"))));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::webSearch_20260318OutputSchema::should not fail validation when title is null", Coverage = UpstreamCoverage.Covered)]
    public void Web_search_20260318_output_allows_a_null_title()
    {
        Assert.True(AnthropicToolSchemas.AcceptWebSearchOutput(AnthropicParity.Element(WebSearchOutput("null"))));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::anthropicResponseSchema - web_fetch_tool_result::should accept PDF response with base64 source", Coverage = UpstreamCoverage.Covered)]
    public void Web_fetch_response_accepts_a_base64_pdf_source()
    {
        Assert.True(AnthropicToolSchemas.AcceptWebFetchResponse(AnthropicParity.Element(WebFetchResponse("base64"))));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::anthropicResponseSchema - web_fetch_tool_result::should accept text source in response", Coverage = UpstreamCoverage.Covered)]
    public void Web_fetch_response_accepts_a_text_source()
    {
        Assert.True(AnthropicToolSchemas.AcceptWebFetchResponse(AnthropicParity.Element(WebFetchResponse("text"))));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::anthropicChunkSchema - web_fetch_tool_result::should accept base64 PDF source in streaming response", Coverage = UpstreamCoverage.Covered)]
    public void Streaming_web_fetch_block_accepts_a_pdf_source()
    {
        Assert.True(AnthropicToolSchemas.AcceptContentBlock(AnthropicParity.Element(@"{""type"":""content_block_start"",""content_block"":{""type"":""web_fetch_tool_result"",""content"":{""type"":""web_fetch_result""}}}")));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::anthropicChunkSchema - web_fetch_tool_result::should accept text source in streaming response", Coverage = UpstreamCoverage.Covered)]
    public void Streaming_web_fetch_block_accepts_a_text_source()
    {
        Assert.True(AnthropicToolSchemas.AcceptContentBlock(AnthropicParity.Element(@"{""type"":""web_fetch_tool_result"",""content"":{""content"":{""source"":{""type"":""text""}}}}")));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::anthropicChunkSchema - shared batch content variants::accepts a string MCP tool result in a streaming content block", Coverage = UpstreamCoverage.Covered)]
    public void Streaming_mcp_tool_result_accepts_a_string()
    {
        Assert.True(AnthropicToolSchemas.AcceptContentBlock(AnthropicParity.Element(@"{
            ""type"":""content_block_start"",
            ""content_block"":{""type"":""mcp_tool_result"",""tool_use_id"":""mcp_123"",""is_error"":false,""content"":""tool output""}}")));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::anthropicChunkSchema - shared batch content variants::accepts opaque MCP tool result citations", Coverage = UpstreamCoverage.Covered)]
    public void Streaming_mcp_tool_result_accepts_opaque_citations()
    {
        Assert.True(AnthropicToolSchemas.AcceptContentBlock(AnthropicParity.Element(@"{
            ""type"":""content_block_start"",
            ""content_block"":{""type"":""mcp_tool_result"",""content"":[{""type"":""text"",""text"":""tool output"",""citations"":[{""type"":""future_citation_variant"",""reference"":""opaque-reference""}]}]}}")));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::computer_toolset_20260801::should prepare the toolset without a name or beta header", Coverage = UpstreamCoverage.Covered)]
    public void Computer_toolset_has_no_name_or_beta()
    {
        var result = ProviderTool("anthropic.computer_toolset_20260801", "{}");
        AnthropicParity.JsonEqual(result.Tools![0], @"{""type"":""computer_toolset_20260801""}");
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::computer_toolset_20260801::should map member configs to snake_case", Coverage = UpstreamCoverage.Covered)]
    public void Computer_toolset_configs_are_snake_case()
    {
        var result = ProviderTool("anthropic.computer_toolset_20260801", @"{""configs"":{""zoom"":{""enabled"":false},""wait"":{""enabled"":true,""deferLoading"":true}}}");
        AnthropicParity.JsonEqual(result.Tools![0], @"{""type"":""computer_toolset_20260801"",""configs"":{""zoom"":{""enabled"":false},""wait"":{""enabled"":true,""defer_loading"":true}}}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::rejectsForcedToolUse::should fall back to auto for tool choice \"required\"", Coverage = UpstreamCoverage.Covered)]
    public void Forced_required_choice_falls_back_to_auto()
    {
        var result = AnthropicParity.PrepareTools(TwoFunctions(), new AnthropicToolPrepareOptions
        {
            SupportsStructuredOutput = true,
            SupportsStrictTools = true,
            RejectsForcedToolUse = true,
            ToolChoice = AnthropicParity.Element(@"{""type"":""required""}"),
        });
        AnthropicParity.JsonEqual(result.ToolChoice, @"{""type"":""auto""}");
        Assert.Equal(2, result.Tools!.Count);
        Assert.Contains("rejects forced tool use", result.Warnings[0].Details);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::rejectsForcedToolUse::should only send the selected tool with auto for tool choice \"tool\"", Coverage = UpstreamCoverage.Covered)]
    public void Forced_named_choice_sends_only_that_tool()
    {
        var result = AnthropicParity.PrepareTools(TwoFunctions(), new AnthropicToolPrepareOptions
        {
            SupportsStructuredOutput = true,
            SupportsStrictTools = true,
            RejectsForcedToolUse = true,
            ToolChoice = AnthropicParity.Element(@"{""type"":""tool"",""toolName"":""otherFunction""}"),
        });
        AnthropicParity.JsonEqual(result.ToolChoice, @"{""type"":""auto""}");
        Assert.Equal("otherFunction", result.Tools![0]!["name"]!.GetValue<string>());
        Assert.Single(result.Tools);
        Assert.Single(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::rejectsForcedToolUse::should preserve disableParallelToolUse in the auto fallback", Coverage = UpstreamCoverage.Covered)]
    public void Forced_choice_keeps_disable_parallel_tool_use()
    {
        var result = AnthropicParity.PrepareTools(TwoFunctions(), new AnthropicToolPrepareOptions
        {
            SupportsStructuredOutput = true,
            SupportsStrictTools = true,
            RejectsForcedToolUse = true,
            DisableParallelToolUse = true,
            ToolChoice = AnthropicParity.Element(@"{""type"":""required""}"),
        });
        AnthropicParity.JsonEqual(result.ToolChoice, @"{""type"":""auto"",""disable_parallel_tool_use"":true}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::rejectsForcedToolUse::should not affect auto and none tool choices", Coverage = UpstreamCoverage.Covered)]
    public void Auto_and_none_are_unchanged_when_forced_tool_use_is_rejected()
    {
        var auto = AnthropicParity.PrepareTools(TwoFunctions(), new AnthropicToolPrepareOptions
        {
            SupportsStructuredOutput = true,
            SupportsStrictTools = true,
            RejectsForcedToolUse = true,
            ToolChoice = AnthropicParity.Element(@"{""type"":""auto""}"),
        });
        AnthropicParity.JsonEqual(auto.ToolChoice, @"{""type"":""auto""}");
        Assert.Empty(auto.Warnings);

        var none = AnthropicParity.PrepareTools(TwoFunctions(), new AnthropicToolPrepareOptions
        {
            SupportsStructuredOutput = true,
            SupportsStrictTools = true,
            RejectsForcedToolUse = true,
            ToolChoice = AnthropicParity.Element(@"{""type"":""none""}"),
        });
        Assert.Null(none.Tools);
        Assert.Null(none.ToolChoice);
        Assert.Empty(none.Warnings);
    }

    private static AnthropicToolPrepareOptions Structured()
    {
        return new AnthropicToolPrepareOptions { SupportsStructuredOutput = true, SupportsStrictTools = true };
    }

    private static AnthropicToolPrepareOptions Choice(string type)
    {
        var json = type == "tool"
            ? @"{""type"":""tool"",""toolName"":""testFunction""}"
            : @"{""type"":""" + type + @"""}";
        return new AnthropicToolPrepareOptions
        {
            SupportsStructuredOutput = true,
            SupportsStrictTools = true,
            ToolChoice = AnthropicParity.Element(json),
        };
    }

    private static AnthropicPreparedTools PrepareFunction(string strict, bool structured, bool strictTools)
    {
        return AnthropicParity.PrepareTools(FunctionTool(strict), new AnthropicToolPrepareOptions
        {
            SupportsStructuredOutput = structured,
            SupportsStrictTools = strictTools,
        });
    }

    private static AnthropicPreparedTools ProviderTool(string id, string args)
    {
        return AnthropicParity.PrepareTools(@"[{""type"":""provider"",""id"":""" + id + @""",""name"":""tool"",""args"":" + args + "}]", Structured());
    }

    private static string FunctionTool(string? strict)
    {
        var strictJson = strict == null ? string.Empty : @",""strict"":" + strict;
        return @"[{""type"":""function"",""name"":""testFunction"",""description"":""A test function"",""inputSchema"":{""type"":""object"",""properties"":{}}" + strictJson + "}]";
    }

    private static string FunctionWithAnthropic(string anthropic)
    {
        return @"[{""type"":""function"",""name"":""testFunction"",""description"":""A test function"",""inputSchema"":{""type"":""object"",""properties"":{}},""providerOptions"":{""anthropic"":" + anthropic + "}}]";
    }

    private static string TwoFunctions()
    {
        return @"[
            {""type"":""function"",""name"":""testFunction"",""description"":""Test"",""inputSchema"":{}},
            {""type"":""function"",""name"":""otherFunction"",""description"":""Other"",""inputSchema"":{}}]";
    }

    private static string WebFetchOutput(string title, string retrievedAt = "2025-12-08T20:46:31.114158")
    {
        return @"{""type"":""web_fetch_result"",""url"":""https://test.com"",""retrievedAt"":""" + retrievedAt + @""",""content"":{""type"":""document"",""title"":" + title + @",""source"":{""type"":""text"",""mediaType"":""text/plain"",""data"":""""}}}";
    }

    private static string WebSearchOutput(string title)
    {
        return @"[{""type"":""web_search_result"",""url"":""https://test.com"",""title"":" + title + @",""encryptedContent"":""abc""}]";
    }

    private static string WebFetchResponse(string sourceType)
    {
        return @"{""type"":""message"",""content"":[{""type"":""web_fetch_tool_result"",""content"":{""content"":{""source"":{""type"":""" + sourceType + @""",""data"":""abc""}}}}]}";
    }
}
