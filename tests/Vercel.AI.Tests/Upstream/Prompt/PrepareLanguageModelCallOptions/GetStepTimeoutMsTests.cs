// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;

namespace Vercel.AI.Tests.Upstream.Prompt.PrepareLanguageModelCallOptions;

/// <summary>Port of <c>prepare-language-model-call-options.test.ts</c> &gt; <c>timeout helpers (from request-options)</c> &gt; <c>getStepTimeoutMs</c>.</summary>
public sealed class GetStepTimeoutMsTests
{
    private const string Prefix =
        "packages/ai/src/prompt/prepare-language-model-call-options.test.ts::timeout helpers (from request-options) > getStepTimeoutMs::";

    [Fact]
    [UpstreamTest(
        Prefix + "should return stepMs from an object",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream's getStepTimeoutMs is the StepMs property here."
    )]
    public void Should_return_stepMs_from_an_object()
    {
        var timeout = new TimeoutConfiguration { StepMs = 3000 };

        Assert.Equal(3000, timeout.StepMs);
    }
}
