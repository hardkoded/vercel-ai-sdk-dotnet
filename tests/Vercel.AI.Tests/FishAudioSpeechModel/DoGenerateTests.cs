// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;
using Vercel.AI.Util;
using static Vercel.AI.Tests.FishAudioSpeechModel.FishAudioSpeechModelTestSupport;
using FishAudioProviderType = Vercel.AI.FishAudio.FishAudioProvider;

namespace Vercel.AI.Tests.FishAudioSpeechModel;

public sealed class DoGenerateTests
{
    private const string Prefix =
        "packages/fish-audio/src/fish-audio-speech-model.test.ts::FishAudioSpeechModel > doGenerate::";

    [Fact]
    [UpstreamTest(
        Prefix + "should generate speech with required parameters",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_generate_speech_with_required_parameters()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(SpeechCall(), CancellationToken.None);
        Assert.Equal("https://api.fish.audio/v1/tts", handler.Calls[0].Uri.AbsoluteUri);
        JsonAssert.Equal(Body(handler), "{\"text\":\"Hello, world!\",\"format\":\"mp3\"}");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send the model id as the `model` header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_the_model_id_as_the_model_header()
    {
        var (model, handler) = SpeechModel("s2.1-pro");
        await model.DoGenerateAsync(SpeechCall(), CancellationToken.None);
        Assert.Equal("s2.1-pro", handler.Calls[0].Header("model"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass the api key as a bearer token",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_the_api_key_as_a_bearer_token()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(SpeechCall(), CancellationToken.None);
        Assert.Equal("Bearer test-api-key", handler.Calls[0].Header("Authorization"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should map voice to reference_id", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_voice_to_reference_id()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(SpeechCall(voice: "test-reference-id"), CancellationToken.None);
        Assert.Equal("test-reference-id", Body(handler).GetProperty("reference_id").GetString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should let providerOptions.referenceId override voice",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_let_providerOptions_referenceId_override_voice()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(
            SpeechCall(
                voice: "ignored-voice",
                providerOptions: "{\"fishAudio\":{\"referenceId\":[\"speaker-a\",\"speaker-b\"]}}"
            ),
            CancellationToken.None
        );
        JsonAssert.Equal(
            Body(handler).GetProperty("reference_id"),
            "[\"speaker-a\",\"speaker-b\"]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map supported output formats",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_supported_output_formats()
    {
        var (model, handler) = SpeechModel();
        foreach (var format in new[] { "wav", "pcm", "mp3", "opus" })
        {
            var result = await model.DoGenerateAsync(
                SpeechCall(outputFormat: format),
                CancellationToken.None
            );
            Assert.Equal(format, Json(handler.Calls[^1].Text).GetProperty("format").GetString());
            Assert.Empty(result.Warnings);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn and fall back to mp3 for an unsupported output format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_and_fall_back_to_mp3_for_an_unsupported_output_format()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(
            SpeechCall(outputFormat: "flac"),
            CancellationToken.None
        );
        Assert.Equal("mp3", Body(handler).GetProperty("format").GetString());
        AssertWarnings(
            result.Warnings,
            (
                "outputFormat",
                "Fish Audio does not support the output format \"flac\". Falling back to mp3. Supported formats are wav, pcm, mp3, opus."
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map speed to prosody.speed",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_speed_to_prosody_speed()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(SpeechCall(speed: 1.5), CancellationToken.None);
        JsonAssert.Equal(Body(handler).GetProperty("prosody"), "{\"speed\":1.5}");
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn for an out-of-range speed",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_for_an_out_of_range_speed()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(SpeechCall(speed: 3), CancellationToken.None);
        Assert.False(Body(handler).TryGetProperty("prosody", out _));
        AssertWarnings(
            result.Warnings,
            ("speed", "Fish Audio speed must be between 0.5 and 2. The speed option was ignored.")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should merge volume and normalizeLoudness into prosody",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_merge_volume_and_normalizeLoudness_into_prosody()
    {
        var (model, handler) = SpeechModel("s2-pro");
        var result = await model.DoGenerateAsync(
            SpeechCall(
                speed: 1.2,
                providerOptions: "{\"fishAudio\":{\"volume\":-3,\"normalizeLoudness\":false}}"
            ),
            CancellationToken.None
        );
        JsonAssert.Equal(
            Body(handler).GetProperty("prosody"),
            "{\"speed\":1.2,\"volume\":-3,\"normalize_loudness\":false}"
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send normalizeLoudness for the s2.1-pro model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_normalizeLoudness_for_the_s2_1_pro_model()
    {
        var (model, handler) = SpeechModel("s2.1-pro");
        var result = await model.DoGenerateAsync(
            SpeechCall(providerOptions: "{\"fishAudio\":{\"normalizeLoudness\":true}}"),
            CancellationToken.None
        );
        JsonAssert.Equal(Body(handler).GetProperty("prosody"), "{\"normalize_loudness\":true}");
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn and drop normalizeLoudness on s1, which ignores it",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_and_drop_normalizeLoudness_on_s1_which_ignores_it()
    {
        var (model, handler) = SpeechModel("s1");
        var result = await model.DoGenerateAsync(
            SpeechCall(providerOptions: "{\"fishAudio\":{\"normalizeLoudness\":true}}"),
            CancellationToken.None
        );
        Assert.False(Body(handler).TryGetProperty("prosody", out _));
        AssertWarnings(
            result.Warnings,
            (
                "providerOptions.fishAudio.normalizeLoudness",
                "Fish Audio ignores normalizeLoudness on s1. It is supported by the S2 family (s2-pro, s2.1-pro)."
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn for language and instructions",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_for_language_and_instructions()
    {
        var (model, _) = SpeechModel();
        var result = await model.DoGenerateAsync(
            SpeechCall(language: "en", instructions: "Speak slowly"),
            CancellationToken.None
        );
        AssertWarnings(
            result.Warnings,
            (
                "language",
                "Fish Audio infers the language from the input text and the selected voice, and has no language parameter. The language option was ignored."
            ),
            (
                "instructions",
                "Fish Audio does not support instructions. The instructions option was ignored."
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass through provider options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_through_provider_options()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(
            SpeechCall(
                providerOptions: "{\"fishAudio\":{\"sampleRate\":44100,\"mp3Bitrate\":192,\"latency\":\"balanced\",\"temperature\":0.5,\"topP\":0.9,\"chunkLength\":200,\"minChunkLength\":20,\"normalize\":false,\"maxNewTokens\":2048,\"repetitionPenalty\":1.5,\"conditionOnPreviousChunks\":false,\"earlyStopThreshold\":0.8,\"features\":[\"quality-guard\"]}}"
            ),
            CancellationToken.None
        );
        JsonAssert.Equal(
            Body(handler),
            "{\"text\":\"Hello, world!\",\"format\":\"mp3\",\"sample_rate\":44100,\"mp3_bitrate\":192,\"latency\":\"balanced\",\"temperature\":0.5,\"top_p\":0.9,\"chunk_length\":200,\"min_chunk_length\":20,\"normalize\":false,\"max_new_tokens\":2048,\"repetition_penalty\":1.5,\"condition_on_previous_chunks\":false,\"early_stop_threshold\":0.8,\"features\":[\"quality-guard\"]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn when mp3Bitrate is used with a non-mp3 format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_when_mp3Bitrate_is_used_with_a_non_mp3_format()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(
            SpeechCall(
                outputFormat: "opus",
                providerOptions: "{\"fishAudio\":{\"mp3Bitrate\":192}}"
            ),
            CancellationToken.None
        );
        Assert.False(Body(handler).TryGetProperty("mp3_bitrate", out _));
        AssertWarnings(
            result.Warnings,
            (
                "providerOptions.fishAudio.mp3Bitrate",
                "mp3Bitrate only applies to mp3 output. The option was ignored for opus output."
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn when opusBitrate is used with a non-opus format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_when_opusBitrate_is_used_with_a_non_opus_format()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(
            SpeechCall(providerOptions: "{\"fishAudio\":{\"opusBitrate\":48000}}"),
            CancellationToken.None
        );
        Assert.False(Body(handler).TryGetProperty("opus_bitrate", out _));
        AssertWarnings(
            result.Warnings,
            (
                "providerOptions.fishAudio.opusBitrate",
                "opusBitrate only applies to opus output. The option was ignored for mp3 output."
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send opus_bitrate for opus output",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_opus_bitrate_for_opus_output()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(
            SpeechCall(
                outputFormat: "opus",
                providerOptions: "{\"fishAudio\":{\"opusBitrate\":-1000}}"
            ),
            CancellationToken.None
        );
        var body = Body(handler);
        Assert.Equal("opus", body.GetProperty("format").GetString());
        Assert.Equal(-1000, body.GetProperty("opus_bitrate").GetInt32());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return the audio and response metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_the_audio_and_response_metadata()
    {
        var handler = new ParityHandler(_ =>
            ParityHandler.Bytes(new byte[100], "audio/mp3", ("x-request-id", "test-request-id"))
        );
        var provider = FishAudioProviderType.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            handler
        );
        var model = new FishAudioProviderType.FishSpeechModel(provider, "s1", () =>
            new DateTime(2024, 1, 1)
        );
        var result = await model.DoGenerateAsync(SpeechCall(), CancellationToken.None);
        Assert.Equal(new byte[100], result.Audio);
        Assert.Equal(new DateTime(2024, 1, 1), result.Response.Timestamp);
        Assert.Equal("s1", result.Response.ModelId);
        Assert.Equal("test-request-id", result.Response.Headers!["x-request-id"]);
        Assert.Equal("{\"text\":\"Hello, world!\",\"format\":\"mp3\"}", result.Request?.Body);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include a user agent suffix",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_a_user_agent_suffix()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(SpeechCall(), CancellationToken.None);
        Assert.Contains(
            "ai-sdk/fish-audio/" + AiSdkVersion.Version,
            handler.Calls[0].Header("User-Agent"),
            StringComparison.Ordinal
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should surface API errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_surface_API_errors()
    {
        var handler = new ParityHandler(_ =>
            Error(
                HttpStatusCode.PaymentRequired,
                "{\"status\":402,\"message\":\"No payment -- see charging schemes\"}"
            )
        );
        var model = new FishAudioProviderType.FishSpeechModel(
            FishAudioProviderType.Create(
                new OpenAICompatibleOptions { ApiKey = "test-api-key" },
                handler
            ),
            "s1"
        );
        var error = await Assert.ThrowsAsync<ApiException>(() =>
            model.DoGenerateAsync(SpeechCall(), CancellationToken.None)
        );
        Assert.Contains(
            "No payment -- see charging schemes",
            error.Message,
            StringComparison.Ordinal
        );
    }
}
