// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;

namespace Vercel.AI.Tests.Upstream.Prompt.PrepareLanguageModelCallOptions;

/// <summary>Port of <c>prepare-language-model-call-options.test.ts</c> &gt; <c>timeout helpers (from request-options)</c> &gt; <c>getFirstChunkTimeoutMs</c>.</summary>
public sealed class GetFirstChunkTimeoutMsTests
{
    private const string Prefix =
        "packages/ai/src/prompt/prepare-language-model-call-options.test.ts::timeout helpers (from request-options) > getFirstChunkTimeoutMs::";

    [Fact]
    [UpstreamTest(
        Prefix + "should return firstChunkMs from an object",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream's getFirstChunkTimeoutMs is the FirstChunkMs property here."
    )]
    public void Should_return_firstChunkMs_from_an_object()
    {
        var timeout = new TimeoutConfiguration { FirstChunkMs = 1500 };

        Assert.Equal(1500, timeout.FirstChunkMs);
    }
}
