// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ElevenLabs;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Util;
using static Vercel.AI.Tests.ElevenLabsSpeechModel.SpeechModelTestHelpers;

namespace Vercel.AI.Tests.ElevenLabsSpeechModel;

public sealed class DialogueTests
{
    private const string Prefix =
        "packages/elevenlabs/src/elevenlabs-speech-model.test.ts::ElevenLabsSpeechModel > dialogue::";

    private const string Commit = "48ab95d62a08b409311c110852e8083910ba438d";

    private const string Inputs =
        "[{\"text\":\"Knock knock\",\"voiceId\":\"voice-a\"},{\"text\":\"Who is there?\",\"voiceId\":\"voice-b\"}]";

    [Fact]
    [UpstreamTest(
        Prefix + "should generate dialogue from inputs",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_generate_dialogue_from_inputs()
    {
        var (model, handler) = CreateModel("eleven_v3");

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("Knock knock")
            {
                Language = "en",
                OutputFormat = "mp3_22050_32",
                ProviderOptions = ElevenLabsOptions(
                    "{\"inputs\":"
                        + Inputs
                        + ",\"voiceSettings\":{\"stability\":0.4,\"similarityBoost\":0.8},\"nextText\":\"Lettuce.\",\"seed\":3}"
                ),
            },
            CancellationToken.None
        );

        Assert.Equal(
            "https://api.elevenlabs.io/v1/text-to-dialogue?output_format=mp3_22050_32",
            handler.Calls[0].Uri.AbsoluteUri
        );
        AssertJsonEqual(
            "{\"inputs\":[{\"text\":\"Knock knock\",\"voice_id\":\"voice-a\"},{\"text\":\"Who is there?\",\"voice_id\":\"voice-b\"}],\"model_id\":\"eleven_v3\",\"language_code\":\"en\",\"settings\":{\"stability\":0.4,\"similarity\":0.8},\"future_text\":\"Lettuce.\",\"seed\":3}",
            handler.Calls[0].Text
        );
        Assert.Equal(Audio, result.Audio);
        var warning = Assert.IsType<OtherWarning>(Assert.Single(result.Warnings));
        Assert.Equal(
            "ElevenLabs dialogue uses providerOptions.elevenlabs.inputs; the text prompt was ignored.",
            warning.Message
        );
        AssertMetadata("{\"elevenlabs\":{\"outputFormat\":\"mp3_22050_32\"}}", result);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not warn about an empty text prompt",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_warn_about_an_empty_text_prompt()
    {
        var (model, _) = CreateModel("eleven_v3");

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("")
            {
                ProviderOptions = ElevenLabsOptions("{\"inputs\":" + Inputs + "}"),
            },
            CancellationToken.None
        );

        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not warn about an empty text prompt beside a composition plan",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_warn_about_an_empty_text_prompt_beside_a_composition_plan()
    {
        var (model, handler) = CreateModel("music_v2_5");

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("")
            {
                ProviderOptions = ElevenLabsOptions("{\"compositionPlan\":{\"chunks\":[]}}"),
            },
            CancellationToken.None
        );

        Assert.Empty(result.Warnings);
        AssertJsonEqual(
            "{\"composition_plan\":{\"chunks\":[]},\"model_id\":\"music_v2_5\"}",
            handler.Calls[0].Text
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn about and ignore options dialogue does not take",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_warn_about_and_ignore_options_dialogue_does_not_take()
    {
        var (model, handler) = CreateModel("eleven_v3");

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("x")
            {
                Voice = "voice-a",
                Speed = 1.1,
                ProviderOptions = ElevenLabsOptions(
                    "{\"inputs\":"
                        + Inputs
                        + ",\"voiceSettings\":{\"style\":0.2},\"durationSeconds\":2}"
                ),
            },
            CancellationToken.None
        );

        using var body = System.Text.Json.JsonDocument.Parse(handler.Calls[0].Text);
        Assert.False(body.RootElement.TryGetProperty("settings", out _));
        Assert.Equal(
            new string?[]
            {
                "providerOptions.elevenlabs.durationSeconds",
                null,
                "speed",
                "voice",
                "providerOptions.elevenlabs.voiceSettings.style",
            },
            Features(result.Warnings)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should require a dialogue model",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_require_a_dialogue_model()
    {
        var (model, handler) = CreateModel("eleven_multilingual_v2");

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            model.GenerateAsync(
                new ElevenLabsSpeechRequest("x")
                {
                    ProviderOptions = ElevenLabsOptions("{\"inputs\":" + Inputs + "}"),
                },
                CancellationToken.None
            )
        );

        Assert.Contains(
            "ElevenLabs dialogue requires an eleven_v3 or eleven_v4 model; eleven_multilingual_v2 is not one.",
            error.Message
        );
        Assert.Empty(handler.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn when inputs are sent to a music model",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_warn_when_inputs_are_sent_to_a_music_model()
    {
        var (model, _) = CreateModel("music_v2");

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("Lo-fi beat")
            {
                ProviderOptions = ElevenLabsOptions("{\"inputs\":" + Inputs + "}"),
            },
            CancellationToken.None
        );

        Assert.Contains(result.Warnings, warning =>
            warning is UnsupportedWarning { Feature: "providerOptions.elevenlabs.inputs" }
        );
    }
}
