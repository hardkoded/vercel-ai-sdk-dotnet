// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Xunit;

namespace Vercel.AI.Tests.SmoothStream;

/// <summary>
/// Maps to the 'Intl.Segmenter chunking' describe. .NET has no word segmenter, so these tests use a small
/// Latin-only <see cref="ISegmenter"/>. The five CJK tests need an ICU-grade segmenter and are not ported.
/// </summary>
public sealed class SegmenterChunkingTests : SmoothStreamTestBase
{
    private static TextStartPart Start => new("1");

    private static TextEndPart End => new("1");

    [Fact]
    public async Task ShouldSegmentEnglishTextUsingASegmenter()
    {
        await Run(
            [Start, D("Hello, world!"), End],
            new SmoothStreamOptions { Segmenter = new LatinWordSegmenter(), DelayInMs = 10 }
        );
        Expect(
            Start,
            D("Hello"),
            Delay,
            D(","),
            Delay,
            D(" "),
            Delay,
            D("world"),
            Delay,
            D("!"),
            Delay,
            End
        );
    }

    private sealed class LatinWordSegmenter : ISegmenter
    {
        private static readonly Regex s_segment = new(
            @"[\p{L}\p{N}']+|\s|.",
            RegexOptions.Singleline,
            TimeSpan.FromSeconds(1)
        );

        public IEnumerable<string> Segment(string input) =>
            s_segment.Matches(input).Cast<Match>().Select(m => m.Value);
    }
}
