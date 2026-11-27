// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;

namespace Vercel.AI.Tests.Upstream.Prompt.PrepareLanguageModelCallOptions;

/// <summary>Port of <c>prepare-language-model-call-options.test.ts</c> &gt; <c>timeout helpers (from request-options)</c> &gt; <c>getTotalTimeoutMs</c>.</summary>
public sealed class GetTotalTimeoutMsTests
{
    private const string Prefix =
        "packages/ai/src/prompt/prepare-language-model-call-options.test.ts::timeout helpers (from request-options) > getTotalTimeoutMs::";

    [Fact]
    [UpstreamTest(
        Prefix + "should return the number directly when timeout is a number",
        Coverage = UpstreamCoverage.Covered,
        Note = "The number form is a TimeSpan, which converts to a configuration with the total timeout only."
    )]
    public void Should_return_the_number_directly_when_timeout_is_a_number()
    {
        TimeoutConfiguration timeout = TimeSpan.FromMilliseconds(5000);

        Assert.Equal(5000, timeout.TotalMs);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return totalMs from an object",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream's getTotalTimeoutMs is the TotalMs property here."
    )]
    public void Should_return_totalMs_from_an_object()
    {
        var timeout = new TimeoutConfiguration { TotalMs = 10000 };

        Assert.Equal(10000, timeout.TotalMs);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return undefined when totalMs is not set",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream's getTotalTimeoutMs is the TotalMs property here."
    )]
    public void Should_return_undefined_when_totalMs_is_not_set()
    {
        var timeout = new TimeoutConfiguration { StepMs = 5000 };

        Assert.Null(timeout.TotalMs);
    }
}
