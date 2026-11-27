// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Prompt.PrepareLanguageModelCallOptions;

/// <summary>Port of <c>prepare-language-model-call-options.test.ts</c> &gt; <c>prepareLanguageModelCallOptions</c> &gt; <c>invalid inputs</c> &gt; <c>topP</c>.</summary>
public sealed class TopPTests
{
    private const string Prefix =
        "packages/ai/src/prompt/prepare-language-model-call-options.test.ts::prepareLanguageModelCallOptions > invalid inputs > topP::";

    [Fact]
    [UpstreamTest(
        Prefix + "should throw InvalidArgumentError if topP is not a number",
        Coverage = UpstreamCoverage.Covered,
        Note = "A C# double? cannot hold a non-number, so NaN stands for it."
    )]
    public void Should_throw_InvalidArgumentError_if_topP_is_not_a_number()
    {
        var exception = Assert.Throws<InvalidArgumentError>(() =>
            LanguageModelCallPreparation.PrepareLanguageModelCallOptions(
                new LanguageModelCallOptionsInput { TopP = double.NaN }
            )
        );

        Assert.Equal("topP", exception.Parameter);
        Assert.Equal(double.NaN, exception.Value);
        Assert.Equal(
            "Invalid argument for parameter topP: topP must be a number",
            exception.Message
        );
    }
}
