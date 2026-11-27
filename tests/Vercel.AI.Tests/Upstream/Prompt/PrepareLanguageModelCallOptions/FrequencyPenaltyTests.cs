// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Prompt.PrepareLanguageModelCallOptions;

/// <summary>Port of <c>prepare-language-model-call-options.test.ts</c> &gt; <c>prepareLanguageModelCallOptions</c> &gt; <c>invalid inputs</c> &gt; <c>frequencyPenalty</c>.</summary>
public sealed class FrequencyPenaltyTests
{
    private const string Prefix =
        "packages/ai/src/prompt/prepare-language-model-call-options.test.ts::prepareLanguageModelCallOptions > invalid inputs > frequencyPenalty::";

    [Fact]
    [UpstreamTest(
        Prefix + "should throw InvalidArgumentError if frequencyPenalty is not a number",
        Coverage = UpstreamCoverage.Partial,
        Note = "A C# double? cannot hold a non-number, so NaN stands for upstream's string input; the typeof check upstream is not reproduced."
    )]
    public void Partial_Should_throw_InvalidArgumentError_if_frequencyPenalty_is_not_a_number()
    {
        var exception = Assert.Throws<InvalidArgumentError>(() =>
            LanguageModelCallPreparation.PrepareLanguageModelCallOptions(
                new LanguageModelCallOptionsInput { FrequencyPenalty = double.NaN }
            )
        );

        Assert.Equal("frequencyPenalty", exception.Parameter);
        Assert.Equal(double.NaN, exception.Value);
        Assert.Equal(
            "Invalid argument for parameter frequencyPenalty: frequencyPenalty must be a number",
            exception.Message
        );
    }
}
