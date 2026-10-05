// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vercel.AI.FishAudio;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests;

/// <summary>Fish Audio behavior matched to the upstream catalog.</summary>
public sealed class FishAudioParityTests
{
    private const string Speech = "packages/fish-audio/src/fish-audio-speech-model.test.ts::FishAudioSpeechModel > doGenerate::";
    private const string Transcription = "packages/fish-audio/src/fish-audio-transcription-model.test.ts::FishAudioTranscriptionModel::";
    private const string TranscriptionJson = "{\"language\":\"English\",\"language_code\":\"en\",\"text\":\"Hello, world!\",\"duration\":2.5,\"segments\":[{\"text\":\"Hello,\",\"start\":0,\"end\":1.2},{\"text\":\"world!\",\"start\":1.2,\"end\":2.5}]}";
    private static readonly byte[] AudioData = { 0, 1, 2, 3, 4 };

    [Fact]
    [UpstreamTest("packages/fish-audio/src/fish-audio-error.test.ts::fishAudioErrorDataSchema::should parse a Fish Audio error", Coverage = UpstreamCoverage.Covered)]
    public void Error_schema_parses_status_and_message()
    {
        const string error = "{\"status\":401,\"message\":\"No permission -- see authorization schemes\"}";
        var result = JsonParsing.SafeParse(error, FishAudioProvider.ErrorSchema);
        Assert.True(result.Success);
        JsonAssert.Equal(result.Value!.Value, error);
        JsonAssert.Equal(result.RawValue!.Value, error);
    }

    [Fact]
    [UpstreamTest("packages/fish-audio/src/fish-audio-error.test.ts::fishAudioErrorDataSchema::should tolerate a payload without a message", Coverage = UpstreamCoverage.Covered)]
    public void Error_schema_accepts_a_missing_message()
    {
        Assert.True(JsonParsing.SafeParse("{\"status\":402}", FishAudioProvider.ErrorSchema).Success);
    }

    [Fact]
    [UpstreamTest(Speech + "should generate speech with required parameters", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_sends_text_and_the_default_format()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(SpeechCall(), CancellationToken.None);
        Assert.Equal("https://api.fish.audio/v1/tts", handler.Calls[0].Uri.AbsoluteUri);
        JsonAssert.Equal(Body(handler), "{\"text\":\"Hello, world!\",\"format\":\"mp3\"}");
    }

    [Fact]
    [UpstreamTest(Speech + "should send the model id as the `model` header", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_sends_the_model_header()
    {
        var (model, handler) = SpeechModel("s2.1-pro");
        await model.DoGenerateAsync(SpeechCall(), CancellationToken.None);
        Assert.Equal("s2.1-pro", handler.Calls[0].Header("model"));
    }

    [Fact]
    [UpstreamTest(Speech + "should pass the api key as a bearer token", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_sends_a_bearer_token()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(SpeechCall(), CancellationToken.None);
        Assert.Equal("Bearer test-api-key", handler.Calls[0].Header("Authorization"));
    }

    [Fact]
    [UpstreamTest(Speech + "should map voice to reference_id", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_sends_the_voice_as_reference_id()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(SpeechCall(voice: "test-reference-id"), CancellationToken.None);
        Assert.Equal("test-reference-id", Body(handler).GetProperty("reference_id").GetString());
    }

    [Fact]
    [UpstreamTest(Speech + "should let providerOptions.referenceId override voice", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_prefers_the_provider_reference_ids()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(SpeechCall(voice: "ignored-voice", providerOptions: "{\"fishAudio\":{\"referenceId\":[\"speaker-a\",\"speaker-b\"]}}"), CancellationToken.None);
        JsonAssert.Equal(Body(handler).GetProperty("reference_id"), "[\"speaker-a\",\"speaker-b\"]");
    }

    [Fact]
    [UpstreamTest(Speech + "should map supported output formats", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_sends_each_supported_format()
    {
        var (model, handler) = SpeechModel();
        foreach (var format in new[] { "wav", "pcm", "mp3", "opus" })
        {
            var result = await model.DoGenerateAsync(SpeechCall(outputFormat: format), CancellationToken.None);
            Assert.Equal(format, Json(handler.Calls[^1].Text).GetProperty("format").GetString());
            Assert.Empty(result.Warnings);
        }
    }

    [Fact]
    [UpstreamTest(Speech + "should warn and fall back to mp3 for an unsupported output format", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_falls_back_to_mp3()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(SpeechCall(outputFormat: "flac"), CancellationToken.None);
        Assert.Equal("mp3", Body(handler).GetProperty("format").GetString());
        AssertWarnings(result.Warnings, ("outputFormat", "Fish Audio does not support the output format \"flac\". Falling back to mp3. Supported formats are wav, pcm, mp3, opus."));
    }

    [Fact]
    [UpstreamTest(Speech + "should map speed to prosody.speed", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_sends_speed_in_prosody()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(SpeechCall(speed: 1.5), CancellationToken.None);
        JsonAssert.Equal(Body(handler).GetProperty("prosody"), "{\"speed\":1.5}");
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Speech + "should warn for an out-of-range speed", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_ignores_an_out_of_range_speed()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(SpeechCall(speed: 3), CancellationToken.None);
        Assert.False(Body(handler).TryGetProperty("prosody", out _));
        AssertWarnings(result.Warnings, ("speed", "Fish Audio speed must be between 0.5 and 2. The speed option was ignored."));
    }

    [Fact]
    [UpstreamTest(Speech + "should merge volume and normalizeLoudness into prosody", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_merges_volume_and_loudness_into_prosody()
    {
        var (model, handler) = SpeechModel("s2-pro");
        var result = await model.DoGenerateAsync(SpeechCall(speed: 1.2, providerOptions: "{\"fishAudio\":{\"volume\":-3,\"normalizeLoudness\":false}}"), CancellationToken.None);
        JsonAssert.Equal(Body(handler).GetProperty("prosody"), "{\"speed\":1.2,\"volume\":-3,\"normalize_loudness\":false}");
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Speech + "should send normalizeLoudness for the s2.1-pro model", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_sends_loudness_normalization_for_s2()
    {
        var (model, handler) = SpeechModel("s2.1-pro");
        var result = await model.DoGenerateAsync(SpeechCall(providerOptions: "{\"fishAudio\":{\"normalizeLoudness\":true}}"), CancellationToken.None);
        JsonAssert.Equal(Body(handler).GetProperty("prosody"), "{\"normalize_loudness\":true}");
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Speech + "should warn and drop normalizeLoudness on s1, which ignores it", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_drops_loudness_normalization_on_s1()
    {
        var (model, handler) = SpeechModel("s1");
        var result = await model.DoGenerateAsync(SpeechCall(providerOptions: "{\"fishAudio\":{\"normalizeLoudness\":true}}"), CancellationToken.None);
        Assert.False(Body(handler).TryGetProperty("prosody", out _));
        AssertWarnings(result.Warnings, ("providerOptions.fishAudio.normalizeLoudness", "Fish Audio ignores normalizeLoudness on s1. It is supported by the S2 family (s2-pro, s2.1-pro)."));
    }

    [Fact]
    [UpstreamTest(Speech + "should warn for language and instructions", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_warns_about_language_and_instructions()
    {
        var (model, _) = SpeechModel();
        var result = await model.DoGenerateAsync(SpeechCall(language: "en", instructions: "Speak slowly"), CancellationToken.None);
        AssertWarnings(
            result.Warnings,
            ("language", "Fish Audio infers the language from the input text and the selected voice, and has no language parameter. The language option was ignored."),
            ("instructions", "Fish Audio does not support instructions. The instructions option was ignored."));
    }

    [Fact]
    [UpstreamTest(Speech + "should pass through provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_passes_provider_options_through()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(
            SpeechCall(providerOptions: "{\"fishAudio\":{\"sampleRate\":44100,\"mp3Bitrate\":192,\"latency\":\"balanced\",\"temperature\":0.5,\"topP\":0.9,\"chunkLength\":200,\"minChunkLength\":20,\"normalize\":false,\"maxNewTokens\":2048,\"repetitionPenalty\":1.5,\"conditionOnPreviousChunks\":false,\"earlyStopThreshold\":0.8,\"features\":[\"quality-guard\"]}}"),
            CancellationToken.None);
        JsonAssert.Equal(
            Body(handler),
            "{\"text\":\"Hello, world!\",\"format\":\"mp3\",\"sample_rate\":44100,\"mp3_bitrate\":192,\"latency\":\"balanced\",\"temperature\":0.5,\"top_p\":0.9,\"chunk_length\":200,\"min_chunk_length\":20,\"normalize\":false,\"max_new_tokens\":2048,\"repetition_penalty\":1.5,\"condition_on_previous_chunks\":false,\"early_stop_threshold\":0.8,\"features\":[\"quality-guard\"]}");
    }

    [Fact]
    [UpstreamTest(Speech + "should warn when mp3Bitrate is used with a non-mp3 format", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_ignores_mp3_bitrate_for_opus()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(SpeechCall(outputFormat: "opus", providerOptions: "{\"fishAudio\":{\"mp3Bitrate\":192}}"), CancellationToken.None);
        Assert.False(Body(handler).TryGetProperty("mp3_bitrate", out _));
        AssertWarnings(result.Warnings, ("providerOptions.fishAudio.mp3Bitrate", "mp3Bitrate only applies to mp3 output. The option was ignored for opus output."));
    }

    [Fact]
    [UpstreamTest(Speech + "should warn when opusBitrate is used with a non-opus format", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_ignores_opus_bitrate_for_mp3()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(SpeechCall(providerOptions: "{\"fishAudio\":{\"opusBitrate\":48000}}"), CancellationToken.None);
        Assert.False(Body(handler).TryGetProperty("opus_bitrate", out _));
        AssertWarnings(result.Warnings, ("providerOptions.fishAudio.opusBitrate", "opusBitrate only applies to opus output. The option was ignored for mp3 output."));
    }

    [Fact]
    [UpstreamTest(Speech + "should send opus_bitrate for opus output", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_sends_the_opus_bitrate()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(SpeechCall(outputFormat: "opus", providerOptions: "{\"fishAudio\":{\"opusBitrate\":-1000}}"), CancellationToken.None);
        var body = Body(handler);
        Assert.Equal("opus", body.GetProperty("format").GetString());
        Assert.Equal(-1000, body.GetProperty("opus_bitrate").GetInt32());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Speech + "should return the audio and response metadata",
        Coverage = UpstreamCoverage.Partial,
        Note = "Audio, timestamp, model id, and headers match. SpeechModelResult has no request body to compare.")]
    public async Task Speech_returns_audio_and_response_metadata()
    {
        var handler = new ParityHandler(_ => ParityHandler.Bytes(new byte[100], "audio/mp3", ("x-request-id", "test-request-id")));
        var provider = FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler);
        var model = new FishAudioProvider.FishSpeechModel(provider, "s1", () => new DateTime(2024, 1, 1));
        var result = await model.DoGenerateAsync(SpeechCall(), CancellationToken.None);
        Assert.Equal(new byte[100], result.Audio);
        Assert.Equal(new DateTime(2024, 1, 1), result.Response.Timestamp);
        Assert.Equal("s1", result.Response.ModelId);
        Assert.Equal("test-request-id", result.Response.Headers!["x-request-id"]);
    }

    [Fact]
    [UpstreamTest(Speech + "should include a user agent suffix", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_sends_the_sdk_user_agent()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(SpeechCall(), CancellationToken.None);
        Assert.Contains("ai-sdk/fish-audio/0.0.0-test", handler.Calls[0].Header("User-Agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Speech + "should surface API errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_surfaces_the_api_error_message()
    {
        var handler = new ParityHandler(_ => Error(HttpStatusCode.PaymentRequired, "{\"status\":402,\"message\":\"No payment -- see charging schemes\"}"));
        var model = new FishAudioProvider.FishSpeechModel(FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler), "s1");
        var error = await Assert.ThrowsAsync<ApiException>(() => model.DoGenerateAsync(SpeechCall(), CancellationToken.None));
        Assert.Contains("No payment -- see charging schemes", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Transcription + "should send Uint8Array audio as a multipart file named `audio`", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_posts_the_audio_file()
    {
        var (model, handler) = TranscriptionModel();
        await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        var call = handler.Calls[0];
        Assert.Equal(HttpMethod.Post, call.Method);
        Assert.Equal("https://api.fish.audio/v1/asr", call.Uri.AbsoluteUri);
        Assert.Matches("Content-Type: audio/mpeg\r\nContent-Disposition: form-data; name=audio; filename=audio.mp3", call.Text);
    }

    [Fact]
    [UpstreamTest(Transcription + "should pass the api key as a bearer token", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_sends_a_bearer_token()
    {
        var (model, handler) = TranscriptionModel();
        await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Equal("Bearer test-api-key", handler.Calls[0].Header("Authorization"));
    }

    [Fact]
    [UpstreamTest(Transcription + "should request timestamps by default", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_requests_timestamps_by_default()
    {
        var (model, handler) = TranscriptionModel();
        await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Equal("false", FormValue(handler.Calls[0], "ignore_timestamps"));
    }

    [Fact]
    [UpstreamTest(Transcription + "should allow opting out of timestamps", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_can_ignore_timestamps()
    {
        var (model, handler) = TranscriptionModel();
        await model.DoGenerateAsync(TranscriptionCall("{\"fishAudio\":{\"ignoreTimestamps\":true}}"), CancellationToken.None);
        Assert.Equal("true", FormValue(handler.Calls[0], "ignore_timestamps"));
    }

    [Fact]
    [UpstreamTest(Transcription + "should send the language when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_sends_the_language()
    {
        var (model, handler) = TranscriptionModel();
        await model.DoGenerateAsync(TranscriptionCall("{\"fishAudio\":{\"language\":\"en\"}}"), CancellationToken.None);
        Assert.Equal("en", FormValue(handler.Calls[0], "language"));
    }

    [Fact]
    [UpstreamTest(Transcription + "should omit the language when not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_omits_the_language()
    {
        var (model, handler) = TranscriptionModel();
        await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Null(FormValue(handler.Calls[0], "language"));
    }

    [Fact]
    [UpstreamTest(Transcription + "should map the transcript, segments and duration", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_returns_text_segments_and_duration()
    {
        var (model, _) = TranscriptionModel();
        var result = await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Equal("Hello, world!", result.Text);
        Assert.Equal(2.5, result.DurationInSeconds);
        Assert.Collection(
            result.Segments,
            segment => Assert.Equal(("Hello,", 0d, 1.2), (segment.Text, segment.StartSecond, segment.EndSecond)),
            segment => Assert.Equal(("world!", 1.2, 2.5), (segment.Text, segment.StartSecond, segment.EndSecond)));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Transcription + "should report the detected language code", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_reports_the_language_code()
    {
        var (model, _) = TranscriptionModel();
        var result = await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Equal("en", result.Language);
    }

    [Fact]
    [UpstreamTest(Transcription + "should prefer the detected language over the requested one", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_prefers_the_detected_language()
    {
        var (model, _) = TranscriptionModel();
        var result = await model.DoGenerateAsync(TranscriptionCall("{\"fishAudio\":{\"language\":\"ja\"}}"), CancellationToken.None);
        Assert.Equal("en", result.Language);
    }

    [Fact]
    [UpstreamTest(Transcription + "should expose the human-readable language as provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_puts_the_language_name_in_provider_metadata()
    {
        var (model, _) = TranscriptionModel();
        var result = await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"fishAudio\":{\"language\":\"English\"}}");
    }

    [Fact]
    [UpstreamTest(Transcription + "should report an undefined language when the response omits it", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_leaves_the_language_unset_when_missing()
    {
        var (model, _) = TranscriptionModel("{\"text\":\"Hello, world!\",\"duration\":1}");
        var result = await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Null(result.Language);
        Assert.Null(result.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(Transcription + "should fall back to empty segments when the response omits them", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_returns_no_segments_when_missing()
    {
        var (model, _) = TranscriptionModel("{\"text\":\"Hello, world!\",\"duration\":1}");
        var result = await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Empty(result.Segments);
        Assert.Equal(1, result.DurationInSeconds);
    }

    [Fact]
    [UpstreamTest(Transcription + "should tolerate a missing duration", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_leaves_the_duration_unset_when_missing()
    {
        var (model, _) = TranscriptionModel("{\"text\":\"Hello, world!\",\"segments\":[]}");
        var result = await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Null(result.DurationInSeconds);
    }

    [Fact]
    [UpstreamTest(Transcription + "should return response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_returns_response_metadata()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(TranscriptionJson, ("x-request-id", "test-request-id")));
        var provider = FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler);
        var model = new FishAudioProvider.FishTranscriptionModel(provider, "transcribe-1", () => new DateTime(2024, 1, 1));
        var result = await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Equal(new DateTime(2024, 1, 1), result.Response.Timestamp);
        Assert.Equal("transcribe-1", result.Response.ModelId);
        Assert.Equal("test-request-id", result.Response.Headers!["x-request-id"]);
    }

    [Fact]
    [UpstreamTest(Transcription + "should include a user agent suffix", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_sends_the_sdk_user_agent()
    {
        var (model, handler) = TranscriptionModel();
        await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Contains("ai-sdk/fish-audio/0.0.0-test", handler.Calls[0].Header("User-Agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Transcription + "should surface API errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_surfaces_the_api_error_message()
    {
        var handler = new ParityHandler(_ => Error(HttpStatusCode.Unauthorized, "{\"status\":401,\"message\":\"No permission -- see authorization schemes\"}"));
        var model = new FishAudioProvider.FishTranscriptionModel(FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler), "transcribe-1");
        var error = await Assert.ThrowsAsync<AuthenticationException>(() => model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None));
        Assert.Contains("No permission -- see authorization schemes", error.Message, StringComparison.Ordinal);
    }
    [Fact]
    [UpstreamTest("packages/fish-audio/src/fish-audio-provider.test.ts::createFishAudio::should create speech models", Coverage = UpstreamCoverage.Covered)]
    public void Speech_and_speech_model_return_speech_models()
    {
        var provider = FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" });
        Assert.IsType<FishAudioProvider.FishSpeechModel>(provider.Speech("s1"));
        Assert.IsType<FishAudioProvider.FishSpeechModel>(provider.SpeechModel("s1"));
    }

    [Fact]
    [UpstreamTest("packages/fish-audio/src/fish-audio-provider.test.ts::createFishAudio::should create transcription models", Coverage = UpstreamCoverage.Covered)]
    public void Transcription_factories_return_transcription_models()
    {
        var provider = FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" });
        Assert.IsType<FishAudioProvider.FishTranscriptionModel>(provider.Transcription());
        Assert.IsType<FishAudioProvider.FishTranscriptionModel>(provider.TranscriptionModel("transcribe-1"));
    }

    [Fact]
    [UpstreamTest("packages/fish-audio/src/fish-audio-provider.test.ts::createFishAudio::should default the transcription model id to `transcribe-1`", Coverage = UpstreamCoverage.Covered)]
    public void Transcription_defaults_the_model_id()
    {
        var provider = FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" });
        Assert.Equal("transcribe-1", provider.Transcription().ModelId);
    }

    [Fact]
    [UpstreamTest("packages/fish-audio/src/fish-audio-provider.test.ts::createFishAudio::should expose the speech model id and provider", Coverage = UpstreamCoverage.Covered)]
    public void Speech_model_exposes_its_id_provider_and_version()
    {
        var model = (FishAudioProvider.FishSpeechModel)FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }).Speech("s2-pro");
        Assert.Equal("s2-pro", model.ModelId);
        Assert.Equal("fish-audio.speech", model.Provider);
        Assert.Equal("v4", model.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest("packages/fish-audio/src/fish-audio-provider.test.ts::createFishAudio::should expose the transcription model provider", Coverage = UpstreamCoverage.Covered)]
    public void Transcription_model_exposes_its_provider_and_version()
    {
        var model = (FishAudioProvider.FishTranscriptionModel)FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }).Transcription();
        Assert.Equal("fish-audio.transcription", model.Provider);
        Assert.Equal("v4", model.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest("packages/fish-audio/src/fish-audio-provider.test.ts::createFishAudio::should throw for unsupported model types", Coverage = UpstreamCoverage.Covered)]
    public void Unsupported_model_types_throw()
    {
        var provider = FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" });
        Assert.Throws<AiSdkException>(() => provider.LanguageModel("s1"));
        Assert.Throws<AiSdkException>(() => provider.EmbeddingModel("s1"));
        Assert.Throws<AiSdkException>(() => provider.ImageModel("s1"));
    }

    [Fact]
    [UpstreamTest("packages/fish-audio/src/fish-audio-provider.test.ts::createFishAudio::should report specification version v4", Coverage = UpstreamCoverage.Covered)]
    public void Provider_reports_specification_version_v4()
    {
        Assert.Equal("v4", FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }).SpecificationVersion);
    }

    private static (FishAudioProvider.FishSpeechModel Model, ParityHandler Handler) SpeechModel(string modelId = "s1")
    {
        var handler = new ParityHandler(_ => ParityHandler.Bytes(new byte[100], "audio/mp3"));
        var provider = FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler);
        return (new FishAudioProvider.FishSpeechModel(provider, modelId), handler);
    }

    private static (FishAudioProvider.FishTranscriptionModel Model, ParityHandler Handler) TranscriptionModel(string response = TranscriptionJson)
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(response));
        var provider = FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler);
        return (new FishAudioProvider.FishTranscriptionModel(provider, "transcribe-1"), handler);
    }

    private static SpeechModelCall SpeechCall(
        string? voice = null,
        string? outputFormat = null,
        string? instructions = null,
        double? speed = null,
        string? language = null,
        string providerOptions = "{}")
    {
        return new SpeechModelCall("Hello, world!", voice, outputFormat, instructions, speed, language, Json(providerOptions), new Dictionary<string, string>(), CancellationToken.None);
    }

    private static TranscriptionModelCall TranscriptionCall(string providerOptions = "{}")
    {
        return new TranscriptionModelCall(AudioData, "audio/mpeg", Json(providerOptions), new Dictionary<string, string>(), CancellationToken.None);
    }

    private static HttpResponseMessage Error(HttpStatusCode status, string body)
    {
        return new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static JsonElement Body(ParityHandler handler)
    {
        return Json(handler.Calls[0].Text);
    }

    private static string? FormValue(ParityCall call, string name)
    {
        var match = Regex.Match(call.Text, "name=" + Regex.Escape(name) + "\r\n(?:[^\r\n]+\r\n)*\r\n([^\r\n]*)\r\n");
        return match.Success ? match.Groups[1].Value : null;
    }

    private static void AssertWarnings(IReadOnlyList<OperationWarning> warnings, params (string Feature, string Details)[] expected)
    {
        Assert.Equal(expected.Length, warnings.Count);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal("unsupported", warnings[i].Type);
            Assert.Equal(expected[i].Feature, warnings[i].Feature);
            Assert.Equal(expected[i].Details, warnings[i].Details);
        }
    }
}
