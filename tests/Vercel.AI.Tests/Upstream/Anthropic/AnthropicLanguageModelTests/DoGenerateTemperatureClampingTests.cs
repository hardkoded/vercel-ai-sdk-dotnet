// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; temperature clamping</c>.</summary>
public sealed class DoGenerateTemperatureClampingTests
{
    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > temperature clamping::should clamp temperature above 1 to 1 and add warning",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_clamp_temperature_above_1_to_1_and_add_warning()
    {
        var handler = JsonFixture("anthropic-text");
        var options = Options();
        options.Temperature = 1.5;

        var result = await DoGenerate(handler, options);

        Assert.Equal(1, (double)RequestBody(handler)["temperature"]!);
        AssertJson(
            """[{"details":"1.5 exceeds anthropic maximum of 1.0. clamped to 1.0","feature":"temperature","type":"unsupported"}]""",
            WarningsJson(result.Warnings)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > temperature clamping::should clamp temperature below 0 to 0 and add warning",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_clamp_temperature_below_0_to_0_and_add_warning()
    {
        var handler = JsonFixture("anthropic-text");
        var options = Options();
        options.Temperature = -0.5;

        var result = await DoGenerate(handler, options);

        Assert.Equal(0, (double)RequestBody(handler)["temperature"]!);
        AssertJson(
            """[{"details":"-0.5 is below anthropic minimum of 0. clamped to 0","feature":"temperature","type":"unsupported"}]""",
            WarningsJson(result.Warnings)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > temperature clamping::should not clamp valid temperature between 0 and 1",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_clamp_valid_temperature_between_0_and_1()
    {
        var handler = JsonFixture("anthropic-text");
        var options = Options();
        options.Temperature = 0.7;

        var result = await DoGenerate(handler, options);

        Assert.Equal(0.7, (double)RequestBody(handler)["temperature"]!);
        AssertJson("[]", WarningsJson(result.Warnings));
    }
}
