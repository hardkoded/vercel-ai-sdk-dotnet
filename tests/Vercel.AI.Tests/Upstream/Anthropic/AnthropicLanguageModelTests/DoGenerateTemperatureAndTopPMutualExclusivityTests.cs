// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; temperature and topP mutual exclusivity</c>.</summary>
public sealed class DoGenerateTemperatureAndTopPMutualExclusivityTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > temperature and topP mutual exclusivity::";

    [Fact]
    [UpstreamTest(
        Prefix + "should only send temperature when both temperature and topP are provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_only_send_temperature_when_both_temperature_and_topP_are_provided()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = AnthropicParity.Hello();
        options.Temperature = 0.7;
        options.TopP = 0.9;

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            options
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Equal(0.7, requestBody["temperature"]!.GetValue<double>());
        Assert.Null(requestBody["top_p"]);
        LanguageModelTestSupport.AssertContainsWarning(
            result.Warnings,
            "unsupported",
            "topP",
            "topP is not supported when temperature is set. topP is ignored."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send temperature when only temperature is provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_temperature_when_only_temperature_is_provided()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = AnthropicParity.Hello();
        options.Temperature = 0.7;

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            options
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Equal(0.7, requestBody["temperature"]!.GetValue<double>());
        Assert.Null(requestBody["top_p"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send topP when only topP is provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_topP_when_only_topP_is_provided()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = AnthropicParity.Hello();
        options.TopP = 0.9;

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            options
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Null(requestBody["temperature"]);
        Assert.Equal(0.9, requestBody["top_p"]!.GetValue<double>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not send temperature or topP when neither is provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_send_temperature_or_topP_when_neither_is_provided()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            AnthropicParity.Hello()
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Null(requestBody["temperature"]);
        Assert.Null(requestBody["top_p"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send both temperature and topP for non-Anthropic models",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_both_temperature_and_topP_for_non_Anthropic_models()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = AnthropicParity.Hello();
        options.Temperature = 0.7;
        options.TopP = 0.9;

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "MiniMax-M2.7",
            options
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Equal(0.7, requestBody["temperature"]!.GetValue<double>());
        Assert.Equal(0.9, requestBody["top_p"]!.GetValue<double>());
        Assert.DoesNotContain(result.Warnings, warning => warning.Feature == "topP");
    }
}
