// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.ElevenLabs;
using Vercel.AI.Operations;
using Vercel.AI.Tests.ElevenLabsSpeechStream;

namespace Vercel.AI.Tests.ElevenLabsSpeechModel;

public sealed class ElevenLabsSpeechModelTests
{
    [Theory]
    [InlineData("music_v2_5")]
    [InlineData("eleven_text_to_sound_v2")]
    [UpstreamTest(
        "packages/elevenlabs/src/elevenlabs-speech-model.test.ts::ElevenLabsSpeechModel::should reject streaming with %s and cancel the text stream",
        Coverage = UpstreamCoverage.Covered,
        Commit = "109a4fe2d1a511cd05a608a11321b7e87bb20cd6"
    )]
    public async Task Should_reject_streaming_with_modelId_and_cancel_the_text_stream(
        string modelId
    )
    {
        var text = new TrackedText([], holdOpen: true);
        var model = SpeechModelTestHelpers.CreateModel(modelId).Model;

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            model.DoStreamAsync(
                new SpeechModelStreamCall(
                    new SpeechTextStream(text),
                    null,
                    null,
                    null,
                    null,
                    null,
                    JsonDocument.Parse("{}").RootElement.Clone(),
                    new Dictionary<string, string>(),
                    null,
                    CancellationToken.None
                ),
                CancellationToken.None
            )
        );

        Assert.Contains(
            $"ElevenLabs {modelId} does not support streaming; use generateSpeech instead.",
            error.Message
        );
        Assert.True(text.Disposed);
    }
}
