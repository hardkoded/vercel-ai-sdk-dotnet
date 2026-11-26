// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ElevenLabs;
using Speech = Vercel.AI.ElevenLabs.ElevenLabsSpeechStream;

namespace Vercel.AI.Tests.ElevenLabsSpeechStream;

public sealed class GetElevenLabsAudioFormatTests
{
    [Theory]
    [InlineData("mp3_44100_128", "audio/mpeg", 44100)]
    [InlineData("pcm_16000", "audio/pcm", 16000)]
    [InlineData("ulaw_8000", "audio/pcmu", 8000)]
    [InlineData("alaw_8000", "audio/pcma", 8000)]
    [InlineData("opus_48000_64", "audio/ogg", 48000)]
    [InlineData("unknown", "application/octet-stream", null)]
    [UpstreamTest(
        "packages/elevenlabs/src/elevenlabs-speech-stream.test.ts::getElevenLabsAudioFormat::should map %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "109a4fe2d1a511cd05a608a11321b7e87bb20cd6"
    )]
    public void Should_map_outputFormat(string outputFormat, string type, int? rate)
    {
        var format = Speech.GetElevenLabsAudioFormat(outputFormat);

        Assert.Equal(type, format.Type);
        Assert.Equal(rate, format.Rate);
    }
}
