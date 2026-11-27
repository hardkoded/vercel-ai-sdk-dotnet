// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;
using Vercel.AI.Util;
using static Vercel.AI.Tests.FishAudioTranscriptionModel.FishAudioTranscriptionModelTestSupport;
using FishAudioProviderType = Vercel.AI.FishAudio.FishAudioProvider;

namespace Vercel.AI.Tests.FishAudioTranscriptionModel;

public sealed class FishAudioTranscriptionModelTests
{
    private const string Prefix =
        "packages/fish-audio/src/fish-audio-transcription-model.test.ts::FishAudioTranscriptionModel::";

    [Fact]
    [UpstreamTest(
        Prefix + "should send Uint8Array audio as a multipart file named `audio`",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_Uint8Array_audio_as_a_multipart_file_named_audio()
    {
        var (model, handler) = TranscriptionModel();
        await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        var call = handler.Calls[0];
        Assert.Equal(HttpMethod.Post, call.Method);
        Assert.Equal("https://api.fish.audio/v1/asr", call.Uri.AbsoluteUri);
        Assert.Matches(
            "Content-Type: audio/mpeg\r\nContent-Disposition: form-data; name=audio; filename=audio.mp3",
            call.Text
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should accept base64 audio", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_accept_base64_audio()
    {
        var (model, handler) = TranscriptionModel();

        await model.DoGenerateAsync(
            new TranscriptionModelCall(
                Convert.ToBase64String(AudioData),
                "audio/mpeg",
                Json("{}"),
                new Dictionary<string, string>(),
                CancellationToken.None
            ),
            CancellationToken.None
        );

        var text = Encoding.Latin1.GetString(handler.Calls[0].Body);
        Assert.Contains("name=audio; filename=audio.mp3", text, StringComparison.Ordinal);
        var start =
            text.IndexOf(
                "\r\n\r\n",
                text.IndexOf("name=audio", StringComparison.Ordinal),
                StringComparison.Ordinal
            ) + 4;
        var end = text.IndexOf("\r\n--", start, StringComparison.Ordinal);
        Assert.Equal(AudioData, Encoding.Latin1.GetBytes(text.Substring(start, end - start)));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass the api key as a bearer token",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_the_api_key_as_a_bearer_token()
    {
        var (model, handler) = TranscriptionModel();
        await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Equal("Bearer test-api-key", handler.Calls[0].Header("Authorization"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should request timestamps by default",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_request_timestamps_by_default()
    {
        var (model, handler) = TranscriptionModel();
        await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Equal("false", FormValue(handler.Calls[0], "ignore_timestamps"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should allow opting out of timestamps",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_allow_opting_out_of_timestamps()
    {
        var (model, handler) = TranscriptionModel();
        await model.DoGenerateAsync(
            TranscriptionCall("{\"fishAudio\":{\"ignoreTimestamps\":true}}"),
            CancellationToken.None
        );
        Assert.Equal("true", FormValue(handler.Calls[0], "ignore_timestamps"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send the language when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_the_language_when_provided()
    {
        var (model, handler) = TranscriptionModel();
        await model.DoGenerateAsync(
            TranscriptionCall("{\"fishAudio\":{\"language\":\"en\"}}"),
            CancellationToken.None
        );
        Assert.Equal("en", FormValue(handler.Calls[0], "language"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit the language when not provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_the_language_when_not_provided()
    {
        var (model, handler) = TranscriptionModel();
        await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Null(FormValue(handler.Calls[0], "language"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map the transcript, segments and duration",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_the_transcript_segments_and_duration()
    {
        var (model, _) = TranscriptionModel();
        var result = await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Equal("Hello, world!", result.Text);
        Assert.Equal(2.5, result.DurationInSeconds);
        Assert.Collection(
            result.Segments,
            segment =>
                Assert.Equal(
                    ("Hello,", 0d, 1.2),
                    (segment.Text, segment.StartSecond, segment.EndSecond)
                ),
            segment =>
                Assert.Equal(
                    ("world!", 1.2, 2.5),
                    (segment.Text, segment.StartSecond, segment.EndSecond)
                )
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should report the detected language code",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_report_the_detected_language_code()
    {
        var (model, _) = TranscriptionModel();
        var result = await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Equal("en", result.Language);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer the detected language over the requested one",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_the_detected_language_over_the_requested_one()
    {
        var (model, _) = TranscriptionModel();
        var result = await model.DoGenerateAsync(
            TranscriptionCall("{\"fishAudio\":{\"language\":\"ja\"}}"),
            CancellationToken.None
        );
        Assert.Equal("en", result.Language);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should expose the human-readable language as provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_expose_the_human_readable_language_as_provider_metadata()
    {
        var (model, _) = TranscriptionModel();
        var result = await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        JsonAssert.Equal(
            result.ProviderMetadata!.Value,
            "{\"fishAudio\":{\"language\":\"English\"}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should report an undefined language when the response omits it",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_report_an_undefined_language_when_the_response_omits_it()
    {
        var (model, _) = TranscriptionModel("{\"text\":\"Hello, world!\",\"duration\":1}");
        var result = await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Null(result.Language);
        Assert.Null(result.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should fall back to empty segments when the response omits them",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_fall_back_to_empty_segments_when_the_response_omits_them()
    {
        var (model, _) = TranscriptionModel("{\"text\":\"Hello, world!\",\"duration\":1}");
        var result = await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Empty(result.Segments);
        Assert.Equal(1, result.DurationInSeconds);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should tolerate a missing duration",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_tolerate_a_missing_duration()
    {
        var (model, _) = TranscriptionModel("{\"text\":\"Hello, world!\",\"segments\":[]}");
        var result = await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Null(result.DurationInSeconds);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_response_metadata()
    {
        var handler = new ParityHandler(_ =>
            ParityHandler.Json(TranscriptionJson, ("x-request-id", "test-request-id"))
        );
        var provider = FishAudioProviderType.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            handler
        );
        var model = new FishAudioProviderType.FishTranscriptionModel(provider, "transcribe-1", () =>
            new DateTime(2024, 1, 1)
        );
        var result = await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Equal(new DateTime(2024, 1, 1), result.Response.Timestamp);
        Assert.Equal("transcribe-1", result.Response.ModelId);
        Assert.Equal("test-request-id", result.Response.Headers!["x-request-id"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include a user agent suffix",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_a_user_agent_suffix()
    {
        var (model, handler) = TranscriptionModel();
        await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
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
                HttpStatusCode.Unauthorized,
                "{\"status\":401,\"message\":\"No permission -- see authorization schemes\"}"
            )
        );
        var model = new FishAudioProviderType.FishTranscriptionModel(
            FishAudioProviderType.Create(
                new OpenAICompatibleOptions { ApiKey = "test-api-key" },
                handler
            ),
            "transcribe-1"
        );
        var error = await Assert.ThrowsAsync<AuthenticationException>(() =>
            model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None)
        );
        Assert.Contains(
            "No permission -- see authorization schemes",
            error.Message,
            StringComparison.Ordinal
        );
    }
}
