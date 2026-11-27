// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Prompt.PrepareLanguageModelCallOptions;

/// <summary>Port of <c>prepare-language-model-call-options.test.ts</c> &gt; <c>prepareLanguageModelCallOptions</c> &gt; <c>reasoning</c>.</summary>
public sealed class ReasoningTests
{
    private const string Prefix =
        "packages/ai/src/prompt/prepare-language-model-call-options.test.ts::prepareLanguageModelCallOptions > reasoning::";

    [Fact]
    [UpstreamTest(
        Prefix + "should pass through valid reasoning values",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_valid_reasoning_values()
    {
        foreach (var value in new[] { "none", "minimal", "low", "medium", "high", "xhigh", "max" })
        {
            var options = LanguageModelCallPreparation.PrepareLanguageModelCallOptions(
                new LanguageModelCallOptionsInput { Reasoning = value }
            );

            Assert.Equal(value, options.Reasoning);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass through undefined", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_through_undefined()
    {
        var options = LanguageModelCallPreparation.PrepareLanguageModelCallOptions(
            new LanguageModelCallOptionsInput()
        );

        Assert.Null(options.Reasoning);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass through provider-default",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_provider_default()
    {
        var options = LanguageModelCallPreparation.PrepareLanguageModelCallOptions(
            new LanguageModelCallOptionsInput { Reasoning = "provider-default" }
        );

        Assert.Equal("provider-default", options.Reasoning);
    }
}
