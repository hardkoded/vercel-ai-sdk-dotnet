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
}
