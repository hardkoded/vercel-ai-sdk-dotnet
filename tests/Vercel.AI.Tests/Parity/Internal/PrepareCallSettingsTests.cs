// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests;

public sealed class PrepareCallSettingsTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/internal/index.test.ts::prepareCallSettings (deprecated alias)::should behave identically to prepareLanguageModelCallOptions",
        Coverage = UpstreamCoverage.Covered)]
    public void Prepare_call_settings_matches_prepare_language_model_call_options()
    {
        var input = new LanguageModelCallSettings
        {
            MaxOutputTokens = 100,
            Temperature = 0.7,
        };

        var alias = CallSettings.PrepareCallSettings(input);
        var direct = CallSettings.PrepareLanguageModelCallOptions(input);

        Assert.Equal(direct.MaxOutputTokens, alias.MaxOutputTokens);
        Assert.Equal(direct.Temperature, alias.Temperature);
        Assert.Equal(100d, alias.MaxOutputTokens);
        Assert.Equal(0.7d, alias.Temperature);
        Assert.NotSame(input, alias);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/internal/index.test.ts::prepareCallSettings (deprecated alias)::should throw the same errors as prepareLanguageModelCallOptions",
        Coverage = UpstreamCoverage.Covered)]
    public void Prepare_call_settings_rejects_a_fractional_max_output_tokens()
    {
        var alias = Assert.Throws<InvalidArgumentException>(() =>
            CallSettings.PrepareCallSettings(new LanguageModelCallSettings { MaxOutputTokens = 10.5 }));
        var direct = Assert.Throws<InvalidArgumentException>(() =>
            CallSettings.PrepareLanguageModelCallOptions(new LanguageModelCallSettings { MaxOutputTokens = 10.5 }));

        Assert.Equal("maxOutputTokens", alias.Parameter);
        Assert.Equal(10.5d, alias.Value);
        Assert.Equal("maxOutputTokens must be an integer", alias.Message);
        Assert.Equal(direct.Parameter, alias.Parameter);
        Assert.Equal(direct.Message, alias.Message);
    }
}
