// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; legacy Claude model defaults</c>.</summary>
public sealed class DoGenerateLegacyClaudeModelDefaultsTests
{
    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > legacy Claude model defaults::should retain sampling parameters and the JSON tool fallback",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_retain_sampling_parameters_and_the_JSON_tool_fallback()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-json-tool.1");
        var options = AnthropicParity.Hello();
        options.MaxOutputTokens = 100;
        options.Temperature = 0.5;
        options.TopK = 10;
        options.JsonResponse = true;
        options.JsonSchema = AnthropicParity.Json(
            "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"],\"additionalProperties\":false}"
        );

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-5-sonnet-20241022",
            options
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Equal(0.5, requestBody["temperature"]!.GetValue<double>());
        Assert.Equal(10, requestBody["top_k"]!.GetValue<int>());
        Assert.Null(requestBody["output_config"]);
        Assert.Single(requestBody["tools"]!.AsArray());
        Assert.Equal("json", requestBody["tools"]![0]!["name"]!.GetValue<string>());
    }
}
