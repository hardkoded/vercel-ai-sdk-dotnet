// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Prompt.PrepareLanguageModelCallOptions;

/// <summary>Port of <c>prepare-language-model-call-options.test.ts</c> &gt; <c>prepareLanguageModelCallOptions</c> &gt; <c>invalid inputs</c> &gt; <c>topK</c>.</summary>
public sealed class TopKTests
{
    private const string Prefix =
        "packages/ai/src/prompt/prepare-language-model-call-options.test.ts::prepareLanguageModelCallOptions > invalid inputs > topK::";

    [Fact]
    [UpstreamTest(
        Prefix + "should throw InvalidArgumentError if topK is not a number",
        Coverage = UpstreamCoverage.Partial,
        Note = "A C# double? cannot hold a non-number, so NaN stands for upstream's string input; the typeof check upstream is not reproduced."
    )]
    public void Partial_Should_throw_InvalidArgumentError_if_topK_is_not_a_number()
    {
        var exception = Assert.Throws<InvalidArgumentError>(() =>
            LanguageModelCallPreparation.PrepareLanguageModelCallOptions(
                new LanguageModelCallOptionsInput { TopK = double.NaN }
            )
        );

        Assert.Equal("topK", exception.Parameter);
        Assert.Equal(double.NaN, exception.Value);
        Assert.Equal(
            "Invalid argument for parameter topK: topK must be a number",
            exception.Message
        );
    }
}
