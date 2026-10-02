// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class LanguageModelCallTests
{
    [UpstreamTest("packages/ai/internal/index.test.ts::prepareCallSettings (deprecated alias)::should behave identically to prepareLanguageModelCallOptions", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Prepare_call_settings_matches_the_current_helper()
    {
        var input = new LanguageModelCallOptionsInput { MaxOutputTokens = 100, Temperature = 0.7 };
        var alias = LanguageModelCallPreparation.PrepareCallSettings(input);
        var current = LanguageModelCallPreparation.PrepareLanguageModelCallOptions(input);
        Assert.Equal(current.MaxOutputTokens, alias.MaxOutputTokens);
        Assert.Equal(current.Temperature, alias.Temperature);
        Assert.Equal(current.TopP, alias.TopP);
        Assert.Equal(current.TopK, alias.TopK);
        Assert.Equal(current.PresencePenalty, alias.PresencePenalty);
        Assert.Equal(current.FrequencyPenalty, alias.FrequencyPenalty);
        Assert.Equal(current.Seed, alias.Seed);
        Assert.NotSame(input, alias);
    }

    [UpstreamTest("packages/ai/internal/index.test.ts::prepareCallSettings (deprecated alias)::should throw the same errors as prepareLanguageModelCallOptions", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Prepare_call_settings_rejects_a_fractional_token_limit()
    {
        var error = Assert.Throws<InvalidArgumentError>(delegate
        {
            LanguageModelCallPreparation.PrepareCallSettings(new LanguageModelCallOptionsInput { MaxOutputTokens = 10.5 });
        });
        Assert.Equal("maxOutputTokens", error.Parameter);
        Assert.Equal(10.5d, Assert.IsType<double>(error.Value!));
        Assert.Equal("Invalid argument for parameter maxOutputTokens: maxOutputTokens must be an integer", error.Message);
        Assert.Throws<InvalidArgumentError>(delegate
        {
            LanguageModelCallPreparation.PrepareLanguageModelCallOptions(new LanguageModelCallOptionsInput { MaxOutputTokens = 10.5 });
        });
    }
}
