// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; forward-compatible defaults for unknown Claude models</c>.</summary>
public sealed class DoGenerateForwardCompatibleDefaultsForUnknownClaudeModelsTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > forward-compatible defaults for unknown Claude models::";

    [Fact]
    [UpstreamTest(
        Prefix + "should map xhigh reasoning to adaptive thinking with xhigh effort",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_xhigh_reasoning_to_adaptive_thinking_with_xhigh_effort()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = AnthropicParity.Hello();
        options.MaxOutputTokens = 100;
        options.Reasoning = "xhigh";

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-future-9",
            options
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        LanguageModelTestSupport.AssertJson(
            "{\"type\":\"adaptive\",\"display\":\"summarized\"}",
            requestBody["thinking"]
        );
        Assert.Null(requestBody["thinking"]!["budget_tokens"]);
        LanguageModelTestSupport.AssertJson("{\"effort\":\"xhigh\"}", requestBody["output_config"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should strip unsupported sampling parameters with warnings",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_strip_unsupported_sampling_parameters_with_warnings()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = AnthropicParity.Hello();
        options.MaxOutputTokens = 100;
        options.Temperature = 0.5;
        options.TopP = 0.7;
        options.TopK = 10;

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-future-9",
            options
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Null(requestBody["temperature"]);
        Assert.Null(requestBody["top_p"]);
        Assert.Null(requestBody["top_k"]);
        Assert.Equal(3, result.Warnings.Count);
        LanguageModelTestSupport.AssertWarning(
            result.Warnings[0],
            "unsupported",
            "temperature",
            "temperature is not supported by claude-future-9 and will be ignored"
        );
        LanguageModelTestSupport.AssertWarning(
            result.Warnings[1],
            "unsupported",
            "topK",
            "topK is not supported by claude-future-9 and will be ignored"
        );
        LanguageModelTestSupport.AssertWarning(
            result.Warnings[2],
            "unsupported",
            "topP",
            "topP is not supported by claude-future-9 and will be ignored"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use native structured output without a JSON tool fallback",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_native_structured_output_without_a_JSON_tool_fallback()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-json-output-format.1");
        var options = AnthropicParity.Hello();
        options.MaxOutputTokens = 100;
        options.JsonResponse = true;
        options.JsonSchema = AnthropicParity.Json(
            "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"],\"additionalProperties\":false}"
        );

        await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-future-9", options);

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        LanguageModelTestSupport.AssertJson(
            "{\"format\":{\"schema\":{\"additionalProperties\":false,\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"],\"type\":\"object\"},\"type\":\"json_schema\"}}",
            requestBody["output_config"]
        );
        Assert.Null(requestBody["tools"]);
        Assert.Null(requestBody["tool_choice"]);
    }
}
