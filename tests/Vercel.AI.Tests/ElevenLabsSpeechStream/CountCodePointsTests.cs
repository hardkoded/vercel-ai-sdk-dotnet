// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ElevenLabs;
using Speech = Vercel.AI.ElevenLabs.ElevenLabsSpeechStream;

namespace Vercel.AI.Tests.ElevenLabsSpeechStream;

public sealed class CountCodePointsTests
{
    [Fact]
    [UpstreamTest(
        "packages/elevenlabs/src/elevenlabs-speech-stream.test.ts::countCodePoints::should count astral characters once",
        Coverage = UpstreamCoverage.Covered,
        Commit = "109a4fe2d1a511cd05a608a11321b7e87bb20cd6"
    )]
    public void Should_count_astral_characters_once()
    {
        Assert.Equal(3, Speech.CountCodePoints("a👋é"));
        Assert.Equal(4, "a👋é".Length);
    }
}
