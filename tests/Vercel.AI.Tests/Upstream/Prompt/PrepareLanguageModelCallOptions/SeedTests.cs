// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Prompt.PrepareLanguageModelCallOptions;

/// <summary>Port of <c>prepare-language-model-call-options.test.ts</c> &gt; <c>prepareLanguageModelCallOptions</c> &gt; <c>invalid inputs</c> &gt; <c>seed</c>.</summary>
public sealed class SeedTests
{
    private const string Prefix =
        "packages/ai/src/prompt/prepare-language-model-call-options.test.ts::prepareLanguageModelCallOptions > invalid inputs > seed::";

    [Fact]
    [UpstreamTest(
        Prefix + "should throw InvalidArgumentError if seed is not an integer",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_InvalidArgumentError_if_seed_is_not_an_integer()
    {
        var exception = Assert.Throws<InvalidArgumentError>(() =>
            LanguageModelCallPreparation.PrepareLanguageModelCallOptions(
                new LanguageModelCallOptionsInput { Seed = 10.5 }
            )
        );

        Assert.Equal("seed", exception.Parameter);
        Assert.Equal(10.5, exception.Value);
        Assert.Equal(
            "Invalid argument for parameter seed: seed must be an integer",
            exception.Message
        );
    }
}
