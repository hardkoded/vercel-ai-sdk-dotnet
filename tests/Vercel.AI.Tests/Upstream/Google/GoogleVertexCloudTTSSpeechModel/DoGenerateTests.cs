// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using Vercel.AI.Google;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexCloudTTSSpeechModel;

/// <summary>Port of <c>google-vertex-cloud-tts-speech-model.test.ts</c> &gt; <c>doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Prefix =
        "packages/google-vertex/src/google-vertex-cloud-tts-speech-model.test.ts::doGenerate::";
    private const string Url = "https://texttospeech.googleapis.com/v1/text:synthesize";
    private const string AudioBase64 = "AQIDBAUGBwg=";

    private static readonly byte[] AudioBytes = [1, 2, 3, 4, 5, 6, 7, 8];

    private static RecordingHandler Server(string? body = null)
    {
        return new RecordingHandler
        {
            ResponseText = body ?? "{\"audioContent\":\"" + AudioBase64 + "\"}",
        };
    }

    private static GoogleVertexProvider Provider(
        RecordingHandler handler,
        Action<VertexOptions>? configure = null
    )
    {
        var options = new VertexOptions
        {
            Project = "test-project",
            Region = "us-central1",
            ApiKey = "k",
        };
        configure?.Invoke(options);
        return GoogleVertexProvider.Create(options, handler);
    }

    private static ISpeechCaller Model(
        RecordingHandler handler,
        Action<VertexOptions>? configure = null,
        DateTimeOffset? now = null
    )
    {
        var provider = Provider(handler, configure);
        if (now is { } clock)
        {
            provider.Clock = () => clock;
        }

        return (ISpeechCaller)provider.SpeechModel("chirp-3-hd");
    }

    private static SpeechModelCall Call(
        string text,
        string? voice = null,
        string? outputFormat = null,
        string? instructions = null,
        double? speed = null,
        string? language = null,
        Dictionary<string, string>? headers = null
    )
    {
        return new SpeechModelCall(
            text,
            voice,
            outputFormat,
            instructions,
            speed,
            language,
            JsonDocument.Parse("{}").RootElement,
            headers ?? new Dictionary<string, string>(),
            CancellationToken.None
        );
    }

    private static JsonElement Body(RecordingHandler handler)
    {
        return JsonDocument.Parse(handler.Body).RootElement;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should target the Cloud Text-to-Speech synthesize endpoint",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_target_the_Cloud_Text_to_Speech_synthesize_endpoint()
    {
        var handler = Server();
        await Model(handler)
            .DoGenerateAsync(Call("Hello from the AI SDK!"), CancellationToken.None);

        Assert.Equal(Url, handler.Uris[0]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send text, a default voice and LINEAR16 audio config",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_text_a_default_voice_and_LINEAR16_audio_config()
    {
        var handler = Server();
        await Model(handler)
            .DoGenerateAsync(Call("Hello from the AI SDK!"), CancellationToken.None);

        GoogleUpstream.JsonEqual(
            JsonDocument.Parse(handler.Body).RootElement,
            "{\"input\":{\"text\":\"Hello from the AI SDK!\"},\"voice\":{\"languageCode\":\"en-US\",\"name\":\"en-US-Chirp3-HD-Kore\"},\"audioConfig\":{\"audioEncoding\":\"LINEAR16\"}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should compose the voice name from language and voice",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_compose_the_voice_name_from_language_and_voice()
    {
        var handler = Server();
        await Model(handler)
            .DoGenerateAsync(
                Call("Hallo!", voice: "Aoede", language: "de-DE"),
                CancellationToken.None
            );

        GoogleUpstream.JsonEqual(
            Body(handler).GetProperty("voice"),
            "{\"languageCode\":\"de-DE\",\"name\":\"de-DE-Chirp3-HD-Aoede\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass a fully-qualified Chirp 3: HD voice name through verbatim",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_a_fully_qualified_Chirp_3_HD_voice_name_through_verbatim()
    {
        var handler = Server();
        await Model(handler)
            .DoGenerateAsync(
                Call("Bonjour !", voice: "fr-FR-Chirp3-HD-Charon"),
                CancellationToken.None
            );

        GoogleUpstream.JsonEqual(
            Body(handler).GetProperty("voice"),
            "{\"languageCode\":\"fr-FR\",\"name\":\"fr-FR-Chirp3-HD-Charon\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should default the language for a Chirp 3: HD voice name without a locale prefix",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_default_the_language_for_a_Chirp_3_HD_voice_name_without_a_locale_prefix()
    {
        var handler = Server();
        await Model(handler)
            .DoGenerateAsync(Call("Hello!", voice: "Chirp3-HD-Kore"), CancellationToken.None);

        GoogleUpstream.JsonEqual(
            Body(handler).GetProperty("voice"),
            "{\"languageCode\":\"en-US\",\"name\":\"Chirp3-HD-Kore\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer an explicit language over the voice-name locale",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_an_explicit_language_over_the_voice_name_locale()
    {
        var handler = Server();
        await Model(handler)
            .DoGenerateAsync(
                Call("Hello!", voice: "en-AU-Chirp3-HD-Kore", language: "en-GB"),
                CancellationToken.None
            );

        GoogleUpstream.JsonEqual(
            Body(handler).GetProperty("voice"),
            "{\"languageCode\":\"en-GB\",\"name\":\"en-AU-Chirp3-HD-Kore\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should decode the base64 audio content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_decode_the_base64_audio_content()
    {
        var result = await Model(Server()).DoGenerateAsync(Call("Hello!"), CancellationToken.None);

        Assert.Equal(AudioBytes, result.Audio);
        GoogleUpstream.JsonEqual(
            result.ProviderMetadata!.Value,
            "{\"google\":{\"mimeType\":\"audio/wav\"}}"
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should map speed to speakingRate", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_speed_to_speakingRate()
    {
        var handler = Server();
        await Model(handler).DoGenerateAsync(Call("Hello!", speed: 1.5), CancellationToken.None);

        GoogleUpstream.JsonEqual(
            Body(handler).GetProperty("audioConfig"),
            "{\"audioEncoding\":\"LINEAR16\",\"speakingRate\":1.5}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn about unsupported instructions",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_about_unsupported_instructions()
    {
        var result = await Model(Server())
            .DoGenerateAsync(Call("Hello!", instructions: "Speak slowly."), CancellationToken.None);

        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("instructions", warning.Feature);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn about unsupported output formats",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_about_unsupported_output_formats()
    {
        var result = await Model(Server())
            .DoGenerateAsync(Call("Hello!", outputFormat: "mp3"), CancellationToken.None);

        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("outputFormat", warning.Feature);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not warn for the wav output format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_warn_for_the_wav_output_format()
    {
        var result = await Model(Server())
            .DoGenerateAsync(Call("Hello!", outputFormat: "wav"), CancellationToken.None);

        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return empty audio when the response has no audio content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_empty_audio_when_the_response_has_no_audio_content()
    {
        var result = await Model(Server("{}"))
            .DoGenerateAsync(Call("Hello!"), CancellationToken.None);

        Assert.Empty(result.Audio);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass headers and user agent",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_headers_and_user_agent()
    {
        var handler = Server();
        await Model(handler, options =>
                options.Headers["Custom-Provider-Header"] = "provider-header-value"
            )
            .DoGenerateAsync(
                Call(
                    "Hello!",
                    headers: new Dictionary<string, string>
                    {
                        ["Custom-Request-Header"] = "request-header-value",
                    }
                ),
                CancellationToken.None
            );

        Assert.Equal("provider-header-value", handler.RequestHeaders["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", handler.RequestHeaders["Custom-Request-Header"]);
        Assert.Contains("ai-sdk/google", handler.RequestHeaders["User-Agent"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include response data with timestamp, modelId, headers and raw body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_response_data_with_timestamp_modelId_headers_and_raw_body()
    {
        var testDate = DateTimeOffset.FromUnixTimeMilliseconds(0);
        var handler = Server();
        handler.ResponseHeaders["x-request-id"] = "test-request-id";

        var result = await Model(handler, now: testDate)
            .DoGenerateAsync(Call("Hello!"), CancellationToken.None);

        Assert.Equal(testDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("chirp-3-hd", result.Response.ModelId);
        Assert.Equal("test-request-id", result.Response.Headers!["x-request-id"]);
        GoogleUpstream.JsonEqual(
            result.Response.Body!.Value,
            "{\"audioContent\":\"" + AudioBase64 + "\"}"
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should include the request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_the_request_body()
    {
        var result = await Model(Server()).DoGenerateAsync(Call("Hello!"), CancellationToken.None);

        Assert.Equal(
            "{\"input\":{\"text\":\"Hello!\"},\"voice\":{\"languageCode\":\"en-US\",\"name\":\"en-US-Chirp3-HD-Kore\"},\"audioConfig\":{\"audioEncoding\":\"LINEAR16\"}}",
            result.Request!.Body
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw an API call error for error responses",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_an_API_call_error_for_error_responses()
    {
        var handler = Server(
            "{\"error\":{\"code\":400,\"message\":\"Voice not found.\",\"status\":\"INVALID_ARGUMENT\"}}"
        );
        handler.Status = HttpStatusCode.BadRequest;

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            Model(handler).DoGenerateAsync(Call("Hello!"), CancellationToken.None)
        );

        Assert.Contains("Voice not found.", error.Message);
    }
}
