// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Prompt.PrepareLanguageModelCallOptions;

/// <summary>Port of <c>prepare-language-model-call-options.test.ts</c> &gt; <c>prepareLanguageModelCallOptions</c> &gt; <c>valid inputs</c>.</summary>
public sealed class ValidInputsTests
{
    private const string Prefix =
        "packages/ai/src/prompt/prepare-language-model-call-options.test.ts::prepareLanguageModelCallOptions > valid inputs::";

    [Fact]
    [UpstreamTest(
        Prefix + "should not throw an error for valid settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_throw_an_error_for_valid_settings()
    {
        var validSettings = new LanguageModelCallOptionsInput
        {
            MaxOutputTokens = 100,
            Temperature = 0.7,
            TopP = 0.9,
            TopK = 50,
            PresencePenalty = 0.5,
            FrequencyPenalty = 0.3,
            Seed = 42,
        };

        var exception = Record.Exception(() =>
            LanguageModelCallPreparation.PrepareLanguageModelCallOptions(validSettings)
        );

        Assert.Null(exception);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should allow undefined values for optional settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_allow_undefined_values_for_optional_settings()
    {
        var exception = Record.Exception(() =>
            LanguageModelCallPreparation.PrepareLanguageModelCallOptions(
                new LanguageModelCallOptionsInput()
            )
        );

        Assert.Null(exception);
    }
}
