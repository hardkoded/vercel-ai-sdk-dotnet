// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Upstream Gemini thinking-level and thinking-budget mapping.</summary>
public sealed class GoogleThinkingTests
{
    private const string Level = "packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 3 models (thinkingLevel)::";
    private const string Budget = "packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 2.5 models (thinkingBudget)::";

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should pass thinkingLevel in provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_thinking_level_from_provider_options()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.ProviderOptions = GoogleParity.GoogleOptions("{\"thinkingConfig\":{\"thinkingLevel\":\"low\"}}");
        await GoogleParity.Generate(capture, "gemini-2.0-flash", options);
        Assert.Equal("low", GoogleParity.Request(capture)["generationConfig"]!["thinkingConfig"]!["thinkingLevel"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should pass thinkingLevel \"minimal\" in provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_minimal_thinking_level()
    {
        await AssertLevel("gemini-2.0-flash", "{\"thinkingConfig\":{\"thinkingLevel\":\"minimal\"}}", "minimal");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should pass thinkingLevel \"medium\" in provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_medium_thinking_level()
    {
        await AssertLevel("gemini-2.0-flash", "{\"thinkingConfig\":{\"thinkingLevel\":\"medium\"}}", "medium");
    }

    [Fact]
    [UpstreamTest(Level + "should map reasoning \"minimal\" to thinkingLevel \"minimal\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_minimal_reasoning_to_minimal()
    {
        await AssertReasoning("gemini-3-flash", "minimal", "minimal");
    }

    [Fact]
    [UpstreamTest(Level + "should map reasoning \"low\" to thinkingLevel \"low\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_low_reasoning_to_low()
    {
        await AssertReasoning("gemini-3-flash", "low", "low");
    }

    [Fact]
    [UpstreamTest(Level + "should map reasoning \"medium\" to thinkingLevel \"medium\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_medium_reasoning_to_medium()
    {
        await AssertReasoning("gemini-3-flash", "medium", "medium");
    }

    [Fact]
    [UpstreamTest(Level + "should map reasoning \"high\" to thinkingLevel \"high\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_high_reasoning_to_high()
    {
        await AssertReasoning("gemini-3-flash", "high", "high");
    }

    [Fact]
    [UpstreamTest(Level + "should map reasoning \"none\" to thinkingLevel \"minimal\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_none_reasoning_to_minimal()
    {
        await AssertReasoning("gemini-3-pro", "none", "minimal");
    }

    [Fact]
    [UpstreamTest(Level + "should coerce reasoning \"xhigh\" to \"high\" with compatibility warning", Coverage = UpstreamCoverage.Covered)]
    public async Task Coerces_xhigh_reasoning_to_high()
    {
        var result = await Generate("gemini-3-flash", "xhigh");
        Assert.Equal("high", result.Level);
        Assert.Contains(result.Result.Warnings, warning => warning.Type == "compatibility");
    }

    [Fact]
    [UpstreamTest(Level + "should coerce reasoning \"minimal\" to thinkingLevel \"low\" for Gemini 3.7 Flash", Coverage = UpstreamCoverage.Covered)]
    public async Task Coerces_minimal_to_low_for_Gemini_3_7_flash()
    {
        var result = await Generate("gemini-3.7-flash", "minimal");
        Assert.Equal("low", result.Level);
        Assert.Contains(result.Result.Warnings, warning => warning.Type == "compatibility");
    }

    [Fact]
    [UpstreamTest(Level + "should coerce reasoning \"none\" to thinkingLevel \"low\" for Gemini 3.7 Flash", Coverage = UpstreamCoverage.Covered)]
    public async Task Coerces_none_to_low_for_Gemini_3_7_flash()
    {
        await AssertReasoning("gemini-3.7-flash", "none", "low");
    }

    [Fact]
    [UpstreamTest(Level + "should also detect gemini-3.1 models as Gemini 3", Coverage = UpstreamCoverage.Covered)]
    public async Task Detects_Gemini_3_1_as_Gemini_3()
    {
        await AssertReasoning("gemini-3.1-pro-preview", "low", "low");
    }

    [Fact]
    [UpstreamTest(Budget + "should map reasoning \"none\" to thinkingBudget 0", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_none_to_a_zero_budget()
    {
        Assert.Equal(0, await BudgetOf("gemini-2.5-pro", "none"));
    }

    [Fact]
    [UpstreamTest(Budget + "should map reasoning \"minimal\" to ~2% of maxOutputTokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_minimal_to_two_percent()
    {
        Assert.Equal(1311, await BudgetOf("gemini-2.5-pro", "minimal"));
    }

    [Fact]
    [UpstreamTest(Budget + "should map reasoning \"low\" to ~10% of maxOutputTokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_low_to_ten_percent()
    {
        Assert.Equal(6554, await BudgetOf("gemini-2.5-pro", "low"));
    }

    [Fact]
    [UpstreamTest(Budget + "should map reasoning \"medium\" to ~30% of maxOutputTokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_medium_to_thirty_percent()
    {
        Assert.Equal(19661, await BudgetOf("gemini-2.5-pro", "medium"));
    }

    [Fact]
    [UpstreamTest(Budget + "should map reasoning \"high\" to ~60% of maxOutputTokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_high_to_the_pro_cap()
    {
        Assert.Equal(32768, await BudgetOf("gemini-2.5-pro", "high"));
    }

    [Fact]
    [UpstreamTest(Budget + "should map reasoning \"xhigh\" to ~90% of maxOutputTokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_xhigh_to_the_pro_cap()
    {
        Assert.Equal(32768, await BudgetOf("gemini-2.5-pro", "xhigh"));
    }

    [Fact]
    [UpstreamTest(Budget + "should use lower maxOutputTokens for flash-lite models", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_flash_budget_for_flash_lite()
    {
        Assert.Equal(19661, await BudgetOf("gemini-2.5-flash-lite", "medium"));
    }

    private static async Task AssertLevel(string modelId, string options, string level)
    {
        var capture = new GoogleCapture();
        var call = GoogleParity.Prompt();
        call.ProviderOptions = GoogleParity.GoogleOptions(options);
        await GoogleParity.Generate(capture, modelId, call);
        Assert.Equal(level, GoogleParity.Request(capture)["generationConfig"]!["thinkingConfig"]!["thinkingLevel"]!.GetValue<string>());
    }

    private static async Task AssertReasoning(string modelId, string reasoning, string level)
    {
        var generated = await Generate(modelId, reasoning);
        Assert.Equal(level, generated.Level);
    }

    private static async Task<(string? Level, LanguageModelGenerateResult Result)> Generate(string modelId, string reasoning)
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.Reasoning = reasoning;
        var result = await GoogleParity.Generate(capture, modelId, options);
        return (GoogleParity.Request(capture)["generationConfig"]!["thinkingConfig"]!["thinkingLevel"]?.GetValue<string>(), result);
    }

    private static async Task<int> BudgetOf(string modelId, string reasoning)
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.Reasoning = reasoning;
        await GoogleParity.Generate(capture, modelId, options);
        return GoogleParity.Request(capture)["generationConfig"]!["thinkingConfig"]!["thinkingBudget"]!.GetValue<int>();
    }
}
