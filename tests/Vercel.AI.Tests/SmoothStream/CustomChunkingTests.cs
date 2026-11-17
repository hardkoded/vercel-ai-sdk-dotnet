// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Xunit;

namespace Vercel.AI.Tests.SmoothStream;

public sealed class CustomChunkingTests : SmoothStreamTestBase
{
    private static TextStartPart Start => new("1");

    private static TextEndPart End => new("1");

    [Fact]
    public async Task ShouldReturnCorrectResultForRegexesThatDontMatchFromTheExactStartOnwards()
    {
        await Run(
            [Start, D("Hello_, world!"), End],
            new SmoothStreamOptions { ChunkingRegex = new Regex("_"), DelayInMs = 10 }
        );
        Expect(Start, D("Hello_"), Delay, D(", world!"), End);
    }

    [Fact]
    public async Task ShouldSupportCustomChunkingRegexpsCharacterLevel()
    {
        await Run(
            [Start, D("Hello, world!"), End],
            new SmoothStreamOptions { ChunkingRegex = new Regex("."), DelayInMs = 10 }
        );
        Expect(
            Start,
            D("H"),
            Delay,
            D("e"),
            Delay,
            D("l"),
            Delay,
            D("l"),
            Delay,
            D("o"),
            Delay,
            D(","),
            Delay,
            D(" "),
            Delay,
            D("w"),
            Delay,
            D("o"),
            Delay,
            D("r"),
            Delay,
            D("l"),
            Delay,
            D("d"),
            Delay,
            D("!"),
            Delay,
            End
        );
    }

    [Fact]
    public async Task ThrowsWhenARegexpProducesAnEmptyMatch()
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Run(
                [Start, D("hello"), End],
                new SmoothStreamOptions { ChunkingRegex = new Regex(@"\S*") }
            )
        );
        Assert.Equal("Chunking RegExp must not match an empty string.", error.Message);
    }
}
