// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Prompt.PrepareLanguageModelCallOptions;

/// <summary>Port of <c>prepare-language-model-call-options.test.ts</c> &gt; <c>prepareLanguageModelCallOptions</c> &gt; <c>invalid inputs</c> &gt; <c>temperature</c>.</summary>
public sealed class TemperatureTests
{
    private const string Prefix =
        "packages/ai/src/prompt/prepare-language-model-call-options.test.ts::prepareLanguageModelCallOptions > invalid inputs > temperature::";

    [Fact]
    [UpstreamTest(
        Prefix + "should throw InvalidArgumentError if temperature is not a number",
        Coverage = UpstreamCoverage.Partial,
        Note = "A C# double? cannot hold a non-number, so NaN stands for upstream's string input; the typeof check upstream is not reproduced."
    )]
    public void Partial_Should_throw_InvalidArgumentError_if_temperature_is_not_a_number()
    {
        var exception = Assert.Throws<InvalidArgumentError>(() =>
            LanguageModelCallPreparation.PrepareLanguageModelCallOptions(
                new LanguageModelCallOptionsInput { Temperature = double.NaN }
            )
        );

        Assert.Equal("temperature", exception.Parameter);
        Assert.Equal(double.NaN, exception.Value);
        Assert.Equal(
            "Invalid argument for parameter temperature: temperature must be a number",
            exception.Message
        );
    }
}
