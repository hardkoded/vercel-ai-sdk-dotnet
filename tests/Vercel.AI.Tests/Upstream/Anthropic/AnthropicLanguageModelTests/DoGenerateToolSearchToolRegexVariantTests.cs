// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; tool search tool &gt; regex variant</c>.</summary>
public sealed class DoGenerateToolSearchToolRegexVariantTests
{
    private static async Task<(CaptureHandler Handler, LanguageModelGenerateResult Result)> Run()
    {
        var handler = JsonFixture("anthropic-tool-search-regex.1");
        var result = await DoGenerate(
            handler,
            Options(
                text: "Find out weather data in SF",
                tools:
                [
                    ProviderTool("anthropic.tool_search_regex_20251119", "tool_search"),
                    FunctionTool(
                        "get_temp_data",
                        "For a location",
                        """{"type":"object","properties":{"location":{"type":"string"},"unit":{"type":"string","enum":["celsius","fahrenheit"]}}}""",
                        """{"anthropic":{"deferLoading":true}}"""
                    ),
                ]
            ),
            "claude-sonnet-4-5"
        );

        return (handler, result);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > tool search tool > regex variant::should send request body with tool search tool and deferred tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_request_body_with_tool_search_tool_and_deferred_tools()
    {
        var (handler, _) = await Run();

        AssertJson(
            """
            {
              "max_tokens": 64000,
              "messages": [{"content":[{"text":"Find out weather data in SF","type":"text"}],"role":"user"}],
              "model": "claude-sonnet-4-5",
              "tools": [
                {"name":"tool_search_tool_regex","type":"tool_search_tool_regex_20251119"},
                {
                  "defer_loading": true,
                  "description": "For a location",
                  "input_schema": {
                    "properties": {
                      "location": {"type":"string"},
                      "unit": {"enum":["celsius","fahrenheit"],"type":"string"}
                    },
                    "type": "object"
                  },
                  "name": "get_temp_data"
                }
              ]
            }
            """,
            RequestBody(handler)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > tool search tool > regex variant::should include advanced-tool-use beta header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_advanced_tool_use_beta_header()
    {
        var (handler, _) = await Run();

        AssertRequestHeaders(
            handler,
            ("anthropic-beta", "structured-outputs-2025-11-13"),
            ("anthropic-version", "2023-06-01"),
            ("content-type", "application/json"),
            ("x-api-key", "test-api-key")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > tool search tool > regex variant::should include tool search tool call and result in content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_tool_search_tool_call_and_result_in_content()
    {
        var (_, result) = await Run();

        AssertJson(
            Snapshot(
                "doGenerate > tool search tool > regex variant > should include tool search tool call and result in content"
            ),
            ContentJson(result.Content)
        );
    }
}
