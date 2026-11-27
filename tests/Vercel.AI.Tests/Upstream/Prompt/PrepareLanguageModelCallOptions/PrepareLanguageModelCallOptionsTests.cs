// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Prompt.PrepareLanguageModelCallOptions;

/// <summary>Port of <c>prepare-language-model-call-options.test.ts</c> &gt; <c>prepareLanguageModelCallOptions</c>.</summary>
public sealed class PrepareLanguageModelCallOptionsTests
{
    private const string Prefix =
        "packages/ai/src/prompt/prepare-language-model-call-options.test.ts::prepareLanguageModelCallOptions::";

    [Fact]
    [UpstreamTest(
        Prefix + "should return a new object with limited values",
        Coverage = UpstreamCoverage.Partial,
        Note = "The typed input has no property for an unknown setting, so the upstream extra random property cannot be passed."
    )]
    public void Partial_Should_return_a_new_object_with_limited_values()
    {
        var input = new LanguageModelCallOptionsInput { MaxOutputTokens = 100, Temperature = 0.7 };

        var options = LanguageModelCallPreparation.PrepareLanguageModelCallOptions(input);

        Assert.NotSame(input, options);
        Assert.Equal(100, options.MaxOutputTokens);
        Assert.Equal(0.7, options.Temperature);
        Assert.Null(options.FrequencyPenalty);
        Assert.Null(options.PresencePenalty);
        Assert.Null(options.Reasoning);
        Assert.Null(options.Seed);
        Assert.Null(options.StopSequences);
        Assert.Null(options.TopK);
        Assert.Null(options.TopP);
    }
}
