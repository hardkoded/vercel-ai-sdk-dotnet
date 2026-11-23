// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ElevenLabs;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Util;
using static Vercel.AI.Tests.ElevenLabsSpeechModel.SpeechModelTestHelpers;

namespace Vercel.AI.Tests.ElevenLabsSpeechModel;

public sealed class MusicModelsTests
{
    private const string Prefix =
        "packages/elevenlabs/src/elevenlabs-speech-model.test.ts::ElevenLabsSpeechModel > music models::";

    private const string Commit = "48ab95d62a08b409311c110852e8083910ba438d";

    [Fact]
    [UpstreamTest(
        Prefix + "should compose music from the text prompt",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_compose_music_from_the_text_prompt()
    {
        var (model, handler) = CreateModel("music_v2_5");

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("An upbeat synthwave intro")
            {
                OutputFormat = "mp3_22050_32",
                ProviderOptions = ElevenLabsOptions(
                    "{\"musicLengthMs\":10000,\"forceInstrumental\":true,\"enableLogging\":false}"
                ),
            },
            CancellationToken.None
        );

        Assert.Equal(
            "https://api.elevenlabs.io/v1/music?output_format=mp3_22050_32&enable_logging=false",
            handler.Calls[0].Uri.AbsoluteUri
        );
        AssertJsonEqual(
            "{\"prompt\":\"An upbeat synthwave intro\",\"model_id\":\"music_v2_5\",\"music_length_ms\":10000,\"force_instrumental\":true}",
            handler.Calls[0].Text
        );
        Assert.Equal(Audio, result.Audio);
        Assert.Empty(result.Warnings);
        AssertMetadata(
            "{\"elevenlabs\":{\"outputFormat\":\"mp3_22050_32\",\"requestedDurationMs\":10000}}",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should default the output format and leave the length to the model",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_default_the_output_format_and_leave_the_length_to_the_model()
    {
        var (model, handler) = CreateModel("music_v2_5");

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("Lo-fi beat"),
            CancellationToken.None
        );

        Assert.Equal(
            "https://api.elevenlabs.io/v1/music?output_format=mp3_44100_128",
            handler.Calls[0].Uri.AbsoluteUri
        );
        AssertJsonEqual(
            "{\"prompt\":\"Lo-fi beat\",\"model_id\":\"music_v2_5\"}",
            handler.Calls[0].Text
        );
        AssertMetadata("{\"elevenlabs\":{\"outputFormat\":\"mp3_44100_128\"}}", result);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn about and ignore speech-only and sound effects options",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_warn_about_and_ignore_speech_only_and_sound_effects_options()
    {
        var (model, handler) = CreateModel("music_v2_5");

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("Lo-fi beat")
            {
                Voice = "test-voice-id",
                Speed = 1.2,
                Language = "en",
                Instructions = "Make it calm",
                ProviderOptions = ElevenLabsOptions(
                    "{\"voiceSettings\":{\"stability\":0.5},\"durationSeconds\":2,\"loop\":true}"
                ),
            },
            CancellationToken.None
        );

        AssertJsonEqual(
            "{\"prompt\":\"Lo-fi beat\",\"model_id\":\"music_v2_5\"}",
            handler.Calls[0].Text
        );
        Assert.Equal(
            new string?[]
            {
                "instructions",
                "language",
                "speed",
                "voice",
                "providerOptions.elevenlabs.voiceSettings",
                "providerOptions.elevenlabs.durationSeconds",
                "providerOptions.elevenlabs.loop",
            },
            Features(result.Warnings)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send a composition plan in place of the prompt",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_a_composition_plan_in_place_of_the_prompt()
    {
        var (model, handler) = CreateModel("music_v2_5");
        const string plan =
            "{\"chunks\":[{\"text\":\"[Intro]\",\"duration_ms\":4000,\"positive_styles\":[\"ambient\"]},{\"song_id\":\"song-1\",\"range\":{\"start_ms\":1000,\"end_ms\":3500}}]}";

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("ignored")
            {
                ProviderOptions = ElevenLabsOptions(
                    "{\"compositionPlan\":" + plan + ",\"seed\":7}"
                ),
            },
            CancellationToken.None
        );

        AssertJsonEqual(
            "{\"composition_plan\":" + plan + ",\"model_id\":\"music_v2_5\",\"seed\":7}",
            handler.Calls[0].Text
        );
        var warning = Assert.IsType<OtherWarning>(Assert.Single(result.Warnings));
        Assert.Equal(
            "ElevenLabs music uses providerOptions.elevenlabs.compositionPlan; the text prompt was ignored.",
            warning.Message
        );
        AssertMetadata(
            "{\"elevenlabs\":{\"outputFormat\":\"mp3_44100_128\",\"requestedDurationMs\":6500}}",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should sum music_v1 composition plan sections",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_sum_music_v1_composition_plan_sections()
    {
        var (model, handler) = CreateModel("music_v1");
        const string section =
            "\"section_name\":\"Verse\",\"positive_local_styles\":[\"driving\"],\"negative_local_styles\":[],\"lines\":[\"Hello\"]";
        var plan =
            "{\"positive_global_styles\":[\"rock\"],\"negative_global_styles\":[],\"sections\":[{"
            + section
            + ",\"duration_ms\":3000},{"
            + section
            + ",\"duration_ms\":5000}]}";

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("ignored")
            {
                ProviderOptions = ElevenLabsOptions("{\"compositionPlan\":" + plan + "}"),
            },
            CancellationToken.None
        );

        AssertJsonEqual(
            "{\"composition_plan\":" + plan + ",\"model_id\":\"music_v1\"}",
            handler.Calls[0].Text
        );
        Assert.Equal(
            8000,
            result
                .ProviderMetadata!.Value.GetProperty("elevenlabs")
                .GetProperty("requestedDurationMs")
                .GetDouble()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit the requested duration for an empty plan",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_omit_the_requested_duration_for_an_empty_plan()
    {
        var (model, _) = CreateModel("music_v2_5");

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("ignored")
            {
                ProviderOptions = ElevenLabsOptions("{\"compositionPlan\":{\"chunks\":[]}}"),
            },
            CancellationToken.None
        );

        AssertMetadata("{\"elevenlabs\":{\"outputFormat\":\"mp3_44100_128\"}}", result);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject a plan chunk without a length",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_reject_a_plan_chunk_without_a_length()
    {
        var (model, handler) = CreateModel("music_v2_5");

        await Assert.ThrowsAnyAsync<Exception>(() =>
            model.GenerateAsync(
                new ElevenLabsSpeechRequest("ignored")
                {
                    ProviderOptions = ElevenLabsOptions(
                        "{\"compositionPlan\":{\"chunks\":[{\"text\":\"[Intro]\"}]}}"
                    ),
                },
                CancellationToken.None
            )
        );

        Assert.Empty(handler.Calls);
    }

    [Theory]
    [InlineData("musicLengthMs", "5000")]
    [InlineData("forceInstrumental", "false")]
    [UpstreamTest(
        Prefix + "should reject $option: $value combined with a composition plan",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_reject_option_value_combined_with_a_composition_plan(
        string option,
        string value
    )
    {
        var (model, handler) = CreateModel("music_v2_5");

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            model.GenerateAsync(
                new ElevenLabsSpeechRequest("ignored")
                {
                    ProviderOptions = ElevenLabsOptions(
                        "{\"compositionPlan\":{\"chunks\":[]},\"" + option + "\":" + value + "}"
                    ),
                },
                CancellationToken.None
            )
        );

        Assert.Contains(
            $"ElevenLabs music {option} cannot be combined with compositionPlan.",
            error.Message
        );
        Assert.Empty(handler.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject an out-of-range music length",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_reject_an_out_of_range_music_length()
    {
        var (model, handler) = CreateModel("music_v2_5");

        await Assert.ThrowsAnyAsync<Exception>(() =>
            model.GenerateAsync(
                new ElevenLabsSpeechRequest("Lo-fi beat")
                {
                    ProviderOptions = ElevenLabsOptions("{\"musicLengthMs\":1000}"),
                },
                CancellationToken.None
            )
        );

        Assert.Empty(handler.Calls);
    }
}
