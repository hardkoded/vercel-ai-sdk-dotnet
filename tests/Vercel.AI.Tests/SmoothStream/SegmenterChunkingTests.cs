// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Xunit;

namespace Vercel.AI.Tests.SmoothStream;

/// <summary>Maps to the 'Intl.Segmenter chunking' describe. <see cref="IcuWordSegmenter"/> plays the role of <c>Intl.Segmenter</c>.</summary>
public sealed class SegmenterChunkingTests : SmoothStreamTestBase
{
    private static TextStartPart Start => new("1");

    private static TextEndPart End => new("1");

    private static SmoothStreamOptions Options(string locale) =>
        new() { Segmenter = new IcuWordSegmenter(locale), DelayInMs = 10 };

    private void ExpectSegments(params string[] segments)
    {
        var expected = new List<object> { Start };
        for (var i = 0; i < segments.Length; i++)
        {
            expected.Add(D(segments[i]));
            expected.Add(Delay);
        }

        expected.Add(End);
        Expect([.. expected]);
    }

    [Fact]
    public async Task ShouldSegmentEnglishTextUsingIntlSegmenter()
    {
        await Run([Start, D("Hello, world!"), End], Options("en"));
        ExpectSegments("Hello", ",", " ", "world", "!");
    }

    [Fact]
    public async Task ShouldSegmentJapaneseTextUsingIntlSegmenter()
    {
        await Run([Start, D("こんにちは世界"), End], Options("ja"));
        ExpectSegments("こんにちは", "世界");
    }

    [Fact]
    public async Task ShouldSegmentChineseTextUsingIntlSegmenter()
    {
        await Run([Start, D("你好世界"), End], Options("zh"));
        ExpectSegments("你好", "世界");
    }

    [Fact]
    public async Task ShouldHandleMixedCjkAndLatinContent()
    {
        await Run([Start, D("Hello こんにちは World"), End], Options("ja"));
        ExpectSegments("Hello", " ", "こんにちは", " ", "World");
    }

    [Fact]
    public async Task ShouldCombinePartialChunksWithIntlSegmenter()
    {
        await Run([Start, D("こんに"), D("ちは"), D("世界"), End], Options("ja"));

        // Intl.Segmenter segments hiragana character-by-character when the full word isn't available in the buffer.
        ExpectSegments("こん", "に", "ち", "は", "世界");
    }

    [Fact]
    public async Task ShouldSegmentLongerJapaneseSentenceWithMixedContent()
    {
        await Run(
            [
                Start,
                D(
                    "東京は日本の首都です。人口は約1400万人で、世界最大の都市圏の一つです。美しい桜の季節には多くの観光客が訪れます。"
                ),
                End,
            ],
            Options("ja")
        );
        ExpectSegments(
            "東京",
            "は",
            "日本",
            "の",
            "首都",
            "です",
            "。",
            "人口",
            "は",
            "約",
            "1400",
            "万人",
            "で",
            "、",
            "世界",
            "最大",
            "の",
            "都市",
            "圏",
            "の",
            "一つ",
            "です",
            "。",
            "美しい",
            "桜の",
            "季節",
            "に",
            "は",
            "多く",
            "の",
            "観光",
            "客",
            "が",
            "訪れ",
            "ます",
            "。"
        );
    }
}
