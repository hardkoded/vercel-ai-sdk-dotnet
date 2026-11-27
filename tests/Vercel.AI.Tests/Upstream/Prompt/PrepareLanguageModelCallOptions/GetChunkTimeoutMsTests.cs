// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;

namespace Vercel.AI.Tests.Upstream.Prompt.PrepareLanguageModelCallOptions;

/// <summary>Port of <c>prepare-language-model-call-options.test.ts</c> &gt; <c>timeout helpers (from request-options)</c> &gt; <c>getChunkTimeoutMs</c>.</summary>
public sealed class GetChunkTimeoutMsTests
{
    private const string Prefix =
        "packages/ai/src/prompt/prepare-language-model-call-options.test.ts::timeout helpers (from request-options) > getChunkTimeoutMs::";

    [Fact]
    [UpstreamTest(
        Prefix + "should return chunkMs from an object",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream's getChunkTimeoutMs is the ChunkMs property here."
    )]
    public void Should_return_chunkMs_from_an_object()
    {
        var timeout = new TimeoutConfiguration { ChunkMs = 2000 };

        Assert.Equal(2000, timeout.ChunkMs);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return undefined when timeout is undefined",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream's getChunkTimeoutMs is the ChunkMs property here."
    )]
    public void Should_return_undefined_when_timeout_is_undefined()
    {
        TimeoutConfiguration? timeout = null;

        Assert.Null(timeout?.ChunkMs);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return undefined when timeout is a number",
        Coverage = UpstreamCoverage.Covered,
        Note = "The number form is a TimeSpan, which converts to a configuration with the total timeout only."
    )]
    public void Should_return_undefined_when_timeout_is_a_number()
    {
        TimeoutConfiguration timeout = TimeSpan.FromMilliseconds(5000);

        Assert.Null(timeout.ChunkMs);
    }
}
