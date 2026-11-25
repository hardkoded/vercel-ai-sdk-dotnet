// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ElevenLabs;
using Vercel.AI.Tests.MoreProviders;
using static Vercel.AI.Tests.ElevenLabsSpeechModel.SpeechModelTestHelpers;

namespace Vercel.AI.Tests.ElevenLabsSpeechModel;

public sealed class SoundEffectsModelsTests
{
    private const string Prefix =
        "packages/elevenlabs/src/elevenlabs-speech-model.test.ts::ElevenLabsSpeechModel > sound effects models::";

    private const string Commit = "48ab95d62a08b409311c110852e8083910ba438d";

    [Fact]
    [UpstreamTest(
        Prefix + "should generate a sound effect from the text",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_generate_a_sound_effect_from_the_text()
    {
        var (model, handler) = CreateModel("eleven_text_to_sound_v2");

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("Glass shattering")
            {
                OutputFormat = "pcm_24000",
                ProviderOptions = ElevenLabsOptions(
                    "{\"durationSeconds\":2,\"promptInfluence\":0.6,\"loop\":true}"
                ),
            },
            CancellationToken.None
        );

        Assert.Equal(
            "https://api.elevenlabs.io/v1/sound-generation?output_format=pcm_24000",
            handler.Calls[0].Uri.AbsoluteUri
        );
        AssertJsonEqual(
            "{\"text\":\"Glass shattering\",\"model_id\":\"eleven_text_to_sound_v2\",\"duration_seconds\":2,\"prompt_influence\":0.6,\"loop\":true}",
            handler.Calls[0].Text
        );
        Assert.Equal(Audio, result.Audio);
        Assert.Empty(result.Warnings);
        AssertMetadata(
            "{\"elevenlabs\":{\"outputFormat\":\"pcm_24000\",\"requestedDurationMs\":2000}}",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should leave the duration to the model when omitted",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_leave_the_duration_to_the_model_when_omitted()
    {
        var (model, handler) = CreateModel("eleven_text_to_sound_v2");

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("Door creak"),
            CancellationToken.None
        );

        AssertJsonEqual(
            "{\"text\":\"Door creak\",\"model_id\":\"eleven_text_to_sound_v2\"}",
            handler.Calls[0].Text
        );
        AssertMetadata("{\"elevenlabs\":{\"outputFormat\":\"mp3_44100_128\"}}", result);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn about and ignore music and speech-only options",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_warn_about_and_ignore_music_and_speech_only_options()
    {
        var (model, handler) = CreateModel("eleven_text_to_sound_v2");

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("Door creak")
            {
                ProviderOptions = ElevenLabsOptions(
                    "{\"musicLengthMs\":5000,\"seed\":1,\"previousText\":\"x\"}"
                ),
            },
            CancellationToken.None
        );

        AssertJsonEqual(
            "{\"text\":\"Door creak\",\"model_id\":\"eleven_text_to_sound_v2\"}",
            handler.Calls[0].Text
        );
        Assert.Equal(
            new string?[]
            {
                "providerOptions.elevenlabs.previousText",
                "providerOptions.elevenlabs.musicLengthMs",
                "providerOptions.elevenlabs.seed",
            },
            Features(result.Warnings)
        );
    }
}
