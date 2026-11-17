// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Xunit;

namespace Vercel.AI.Tests.SmoothStream;

public sealed class LineChunkingTests : SmoothStreamTestBase
{
    private static TextStartPart Start => new("1");

    private static TextEndPart End => new("1");

    [Fact]
    public async Task ShouldSplitTextByLinesWhenUsingLineChunkingMode()
    {
        await Run(
            [
                Start,
                D("First line\nSecond line\nThird line with more text\n"),
                D("Partial line"),
                D(" continues\nFinal line\n"),
                End,
            ],
            new SmoothStreamOptions { DelayInMs = 10, Chunking = SmoothStreamChunking.Line }
        );
        Expect(
            Start,
            D("First line\n"),
            Delay,
            D("Second line\n"),
            Delay,
            D("Third line with more text\n"),
            Delay,
            D("Partial line continues\n"),
            Delay,
            D("Final line\n"),
            Delay,
            End
        );
    }

    [Fact]
    public async Task ShouldHandleTextWithoutLineEndingsInLineChunkingMode()
    {
        await Run(
            [Start, D("Text without"), D(" any line"), D(" breaks"), End],
            new SmoothStreamOptions { Chunking = SmoothStreamChunking.Line }
        );
        Expect(Start, D("Text without any line breaks"), End);
    }
}
