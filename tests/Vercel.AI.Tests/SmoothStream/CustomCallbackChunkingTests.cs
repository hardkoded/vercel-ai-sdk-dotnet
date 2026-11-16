// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Xunit;

namespace Vercel.AI.Tests.SmoothStream;

public sealed class CustomCallbackChunkingTests : SmoothStreamTestBase
{
    private static TextStartPart Start => new("1");

    private static TextEndPart End => new("1");

    [Fact]
    public async Task ShouldSupportCustomChunkingCallback()
    {
        var pattern = new Regex("[^_]*_");
        await Run(
            [Start, D("He_llo, "), D("w_orld!"), End],
            new SmoothStreamOptions
            {
                ChunkDetector = buffer =>
                {
                    var match = pattern.Match(buffer);
                    return match.Success ? match.Value : null;
                },
            });
        Expect(Start, D("He_"), Delay, D("llo, w_"), Delay, D("orld!"), End);
    }

    [Fact]
    public async Task ThrowsEmptyMatchError()
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => Run(
            [Start, D("Hello, world!"), End],
            new SmoothStreamOptions { ChunkDetector = _ => string.Empty }));
        Assert.Equal("Chunking function must return a non-empty string.", error.Message);
    }

    [Fact]
    public async Task ThrowsMatchPrefixError()
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => Run(
            [Start, D("Hello, world!"), End],
            new SmoothStreamOptions { ChunkDetector = _ => "world" }));
        Assert.Equal(
            "Chunking function must return a match that is a prefix of the buffer. Received: \"world\" which is not a prefix of buffer \"Hello, world!\"",
            error.Message);
    }
}
