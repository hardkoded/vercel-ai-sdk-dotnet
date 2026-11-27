// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; tool search tool &gt; bm25 variant</c>.</summary>
public sealed class DoGenerateToolSearchToolBm25VariantTests
{
    private static async Task<(CaptureHandler Handler, LanguageModelGenerateResult Result)> Run()
    {
        var handler = JsonFixture("anthropic-tool-search-bm25.1");
        var result = await DoGenerate(
            handler,
            Options(
                text: "What is the weather in San Francisco?",
                tools:
                [
                    ProviderTool("anthropic.tool_search_bm25_20251119", "tool_search"),
                    FunctionTool(
                        "get_weather",
                        "Get the current weather at a specific location",
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
            + "doGenerate > tool search tool > bm25 variant::should send request body with tool search bm25 tool",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_request_body_with_tool_search_bm25_tool()
    {
        var (handler, _) = await Run();

        AssertJson(
            """
            {
              "max_tokens": 64000,
              "messages": [{"content":[{"text":"What is the weather in San Francisco?","type":"text"}],"role":"user"}],
              "model": "claude-sonnet-4-5",
              "tools": [
                {"name":"tool_search_tool_bm25","type":"tool_search_tool_bm25_20251119"},
                {
                  "defer_loading": true,
                  "description": "Get the current weather at a specific location",
                  "input_schema": {
                    "properties": {
                      "location": {"type":"string"},
                      "unit": {"enum":["celsius","fahrenheit"],"type":"string"}
                    },
                    "type": "object"
                  },
                  "name": "get_weather"
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
            + "doGenerate > tool search tool > bm25 variant::should include advanced-tool-use beta header",
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
            + "doGenerate > tool search tool > bm25 variant::should include tool search tool call and result in content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_tool_search_tool_call_and_result_in_content()
    {
        var (_, result) = await Run();

        AssertJson(
            Snapshot(
                "doGenerate > tool search tool > bm25 variant > should include tool search tool call and result in content"
            ),
            ContentJson(result.Content)
        );
    }
}
