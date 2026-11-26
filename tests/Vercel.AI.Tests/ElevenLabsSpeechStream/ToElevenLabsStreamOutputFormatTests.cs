// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ElevenLabs;
using Vercel.AI.Operations;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.ElevenLabsSpeechStream;

public sealed class ToElevenLabsStreamOutputFormatTests
{
    private const string Prefix =
        "packages/elevenlabs/src/elevenlabs-speech-stream.test.ts::toElevenLabsStreamOutputFormat::";

    [Theory]
    [InlineData(null, null, "mp3_44100_128")]
    [InlineData("audio/mpeg", null, "mp3_44100_128")]
    [InlineData("audio/mpeg", 22050, "mp3_22050_32")]
    [InlineData("audio/pcm", null, "pcm_24000")]
    [InlineData("audio/pcm", 16000, "pcm_16000")]
    [InlineData("audio/pcmu", null, "ulaw_8000")]
    [InlineData("audio/pcma", 8000, "alaw_8000")]
    [InlineData("audio/opus", null, "opus_48000_64")]
    [InlineData("Audio/L16; rate=16000", 16000, "pcm_16000")]
    [UpstreamTest(
        Prefix + "should map %j to %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "109a4fe2d1a511cd05a608a11321b7e87bb20cd6"
    )]
    public void Should_map_format_to_expected(string? type, int? rate, string expected)
    {
        var format = type == null ? null : new SpeechAudioFormat(type, rate);

        Assert.Equal(
            expected,
            ElevenLabsProvider.ElevenLabsSpeechModel.ToElevenLabsStreamOutputFormat(format)
        );
    }

    [Theory]
    [InlineData("audio/pcm", 11025)]
    [InlineData("audio/wav", null)]
    [InlineData("audio/mpeg", 48000)]
    [UpstreamTest(
        Prefix + "should reject %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = "109a4fe2d1a511cd05a608a11321b7e87bb20cd6"
    )]
    public void Should_reject_format(string type, int? rate)
    {
        Assert.Throws<InvalidArgumentError>(() =>
            ElevenLabsProvider.ElevenLabsSpeechModel.ToElevenLabsStreamOutputFormat(
                new SpeechAudioFormat(type, rate)
            )
        );
    }
}
