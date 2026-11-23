// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.ElevenLabs;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.ElevenLabsSpeechModel;

public sealed class DoGenerateTests
{
    private const string Prefix =
        "packages/elevenlabs/src/elevenlabs-speech-model.test.ts::ElevenLabsSpeechModel > doGenerate::";

    [Theory]
    [InlineData(
        "test-voice-id",
        "https://api.elevenlabs.io/v1/text-to-speech/test-voice-id?output_format=mp3_44100_128"
    )]
    [InlineData(
        null,
        "https://api.elevenlabs.io/v1/text-to-speech/21m00Tcm4TlvDq8ikWAM?output_format=mp3_44100_128"
    )]
    [UpstreamTest(
        Prefix + "should send the full text-to-speech request for voice $voice",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7062ab6818c90e135d068433ce99b11ac0dfa081"
    )]
    public async Task Should_send_the_full_text_to_speech_request_for_voice(
        string? voice,
        string url
    )
    {
        var handler = new ParityHandler(_ => ParityHandler.Bytes(new byte[100], "audio/mp3"));
        var provider = ElevenLabsProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            handler
        );
        var model = (ElevenLabsProvider.ElevenLabsSpeechModel)
            provider.SpeechModel("eleven_multilingual_v2");

        await model.GenerateAsync(
            new ElevenLabsSpeechRequest("Hello, world!") { Voice = voice },
            CancellationToken.None
        );

        Assert.Equal(url, handler.Calls[0].Uri.AbsoluteUri);
        using var body = JsonDocument.Parse(handler.Calls[0].Text);
        Assert.Equal("Hello, world!", body.RootElement.GetProperty("text").GetString());
        Assert.Equal(
            "eleven_multilingual_v2",
            body.RootElement.GetProperty("model_id").GetString()
        );
        Assert.Equal(2, body.RootElement.EnumerateObject().Count());
    }

    [Theory]
    [InlineData("wav", "wav_44100")]
    [InlineData("wav_24000", "wav_24000")]
    [UpstreamTest(
        Prefix + "should map HTTP output format %s to %s",
        Coverage = UpstreamCoverage.Partial,
        Commit = "3e5a12334b64c987a86b9ce54e369e56a5f2f863"
    )]
    public async Task Should_map_HTTP_output_format(string outputFormat, string nativeFormat)
    {
        var audio = new byte[] { 1, 2, 3, 4 };
        var handler = new ParityHandler(_ => ParityHandler.Bytes(audio, "audio/wav"));
        var provider = ElevenLabsProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            handler
        );
        var model = (ElevenLabsProvider.ElevenLabsSpeechModel)
            provider.SpeechModel("eleven_multilingual_v2");

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("Hello, world!") { OutputFormat = outputFormat },
            CancellationToken.None
        );

        Assert.Contains($"output_format={nativeFormat}", handler.Calls[0].Uri.Query);
        Assert.Equal(audio, result.Audio);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should report the output format in provider metadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = "48ab95d62a08b409311c110852e8083910ba438d"
    )]
    public async Task Should_report_the_output_format_in_provider_metadata()
    {
        var handler = new ParityHandler(_ => ParityHandler.Bytes(new byte[100], "audio/mp3"));
        var provider = ElevenLabsProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            handler
        );
        var model = (ISpeechCaller)provider.SpeechModel("eleven_multilingual_v2");

        var result = await model.DoGenerateAsync(
            new SpeechModelCall(
                "Hello, world!",
                null,
                "pcm_24000",
                null,
                null,
                null,
                JsonDocument.Parse("{}").RootElement,
                new Dictionary<string, string>(),
                CancellationToken.None
            ),
            CancellationToken.None
        );

        Assert.Equal(
            "{\"elevenlabs\":{\"outputFormat\":\"pcm_24000\"}}",
            result.ProviderMetadata!.Value.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should surface the ElevenLabs error message and code",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7062ab6818c90e135d068433ce99b11ac0dfa081"
    )]
    public async Task Should_surface_the_ElevenLabs_error_message_and_code()
    {
        const string body = """
            {"detail":{"type":"not_found","code":"voice_not_found","message":"A voice with voice_id 'missing-voice' was not found.","status":"voice_not_found","request_id":"req_test_speech"}}
            """;
        var handler = new ParityHandler(_ => Error(HttpStatusCode.NotFound, body));
        var provider = ElevenLabsProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            handler
        );
        var model = (ElevenLabsProvider.ElevenLabsSpeechModel)
            provider.SpeechModel("eleven_multilingual_v2");

        var error = Assert.IsAssignableFrom<ApiException>(
            await Record.ExceptionAsync(() =>
                model.GenerateAsync(
                    new ElevenLabsSpeechRequest("Hello, world!") { Voice = "missing-voice" },
                    CancellationToken.None
                )
            )
        );

        Assert.Equal(
            "A voice with voice_id 'missing-voice' was not found. (voice_not_found)",
            error.Message
        );
        Assert.Equal(404, error.StatusCode);
        var data = Assert.IsType<ElevenLabsErrorData>(error.ErrorData);
        Assert.Equal("req_test_speech", data.Detail!.RequestId);
    }

    private const string TimestampsCommit = "48ab95d62a08b409311c110852e8083910ba438d";

    private const string AlignmentJson =
        "{\"characters\":[\"H\",\"i\"],\"character_start_times_seconds\":[0,0.123456789],\"character_end_times_seconds\":[0.123456789,0.4]}";

    private const string AlignmentMetadataJson =
        "{\"characters\":[\"H\",\"i\"],\"characterStartTimesSeconds\":[0,0.123456789],\"characterEndTimesSeconds\":[0.123456789,0.4]}";

    private static ElevenLabsProvider.ElevenLabsSpeechModel SpeechModel(
        ParityHandler handler,
        string modelId = "eleven_multilingual_v2"
    ) =>
        (ElevenLabsProvider.ElevenLabsSpeechModel)
            ElevenLabsProvider
                .Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler)
                .SpeechModel(modelId);

    private static JsonElement Options(string json) => JsonDocument.Parse(json).RootElement.Clone();

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    [UpstreamTest(
        Prefix + "preserves binary audio when withTimestamps is %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = TimestampsCommit
    )]
    public async Task Preserves_binary_audio_when_withTimestamps_is(bool? withTimestamps)
    {
        var audio = new byte[100];
        var handler = new ParityHandler(_ => ParityHandler.Bytes(audio, "audio/mp3"));
        var model = SpeechModel(handler);

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("Hi")
            {
                ProviderOptions = Options(
                    withTimestamps is null
                        ? "{\"elevenlabs\":{}}"
                        : "{\"elevenlabs\":{\"withTimestamps\":false}}"
                ),
            },
            CancellationToken.None
        );

        Assert.Equal(audio, result.Audio);
        Assert.Equal(
            "{\"elevenlabs\":{\"outputFormat\":\"mp3_44100_128\"}}",
            result.ProviderMetadata!.Value.GetRawText()
        );
        Assert.Equal(
            "https://api.elevenlabs.io/v1/text-to-speech/21m00Tcm4TlvDq8ikWAM?output_format=mp3_44100_128",
            handler.Calls[0].Uri.AbsoluteUri
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "returns timestamped audio and metadata from Text to Speech",
        Coverage = UpstreamCoverage.Covered,
        Commit = TimestampsCommit
    )]
    public async Task Returns_timestamped_audio_and_metadata_from_Text_to_Speech()
    {
        var body =
            "{\"audio_base64\":\"AQID\",\"alignment\":"
            + AlignmentJson
            + ",\"normalized_alignment\":"
            + AlignmentJson
            + "}";
        var handler = new ParityHandler(_ =>
            ParityHandler.Json(body, ("request-id", "tts-request"))
        );
        var model = SpeechModel(handler);

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("Hi")
            {
                Voice = "test-voice",
                OutputFormat = "pcm_24000",
                Headers = new Dictionary<string, string> { ["custom-header"] = "custom-value" },
                ProviderOptions = Options(
                    "{\"elevenlabs\":{\"withTimestamps\":true,\"seed\":0,\"enableLogging\":false}}"
                ),
            },
            CancellationToken.None
        );

        Assert.Equal(
            "https://api.elevenlabs.io/v1/text-to-speech/test-voice/with-timestamps?output_format=pcm_24000&enable_logging=false",
            handler.Calls[0].Uri.AbsoluteUri
        );
        Assert.Equal(
            "{\"text\":\"Hi\",\"model_id\":\"eleven_multilingual_v2\",\"seed\":0}",
            handler.Calls[0].Text
        );
        Assert.Equal("custom-value", handler.Calls[0].Headers["custom-header"]);
        Assert.Equal(new byte[] { 1, 2, 3 }, result.Audio);
        Assert.Equal(
            "{\"elevenlabs\":{\"outputFormat\":\"pcm_24000\",\"alignment\":"
                + AlignmentMetadataJson
                + ",\"normalizedAlignment\":"
                + AlignmentMetadataJson
                + "}}",
            result.ProviderMetadata!.Value.GetRawText()
        );
        Assert.Equal("tts-request", result.Response.Headers["request-id"]);
        Assert.Equal(body, result.Response.Body!.Value.GetRawText());
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"alignment\":null,\"normalized_alignment\":null,\"voice_segments\":null}")]
    [UpstreamTest(
        Prefix + "accepts absent or null timestamp metadata: %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = TimestampsCommit
    )]
    public async Task Accepts_absent_or_null_timestamp_metadata(string metadata)
    {
        var extra = metadata == "{}" ? string.Empty : "," + metadata.Trim('{', '}');
        var handler = new ParityHandler(_ =>
            ParityHandler.Json("{\"audio_base64\":\"AQID\"" + extra + "}")
        );
        var model = SpeechModel(handler);

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("Hi")
            {
                ProviderOptions = Options("{\"elevenlabs\":{\"withTimestamps\":true}}"),
            },
            CancellationToken.None
        );

        Assert.Equal(new byte[] { 1, 2, 3 }, result.Audio);
        Assert.Equal(
            "{\"elevenlabs\":{\"outputFormat\":\"mp3_44100_128\"}}",
            result.ProviderMetadata!.Value.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves empty audio and alignment arrays",
        Coverage = UpstreamCoverage.Covered,
        Commit = TimestampsCommit
    )]
    public async Task Preserves_empty_audio_and_alignment_arrays()
    {
        var handler = new ParityHandler(_ =>
            ParityHandler.Json(
                "{\"audio_base64\":\"\",\"alignment\":{\"characters\":[],\"character_start_times_seconds\":[],\"character_end_times_seconds\":[]}}"
            )
        );
        var model = SpeechModel(handler);

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("Hi")
            {
                ProviderOptions = Options("{\"elevenlabs\":{\"withTimestamps\":true}}"),
            },
            CancellationToken.None
        );

        Assert.Empty(result.Audio);
        Assert.Equal(
            "{\"elevenlabs\":{\"outputFormat\":\"mp3_44100_128\",\"alignment\":{\"characters\":[],\"characterStartTimesSeconds\":[],\"characterEndTimesSeconds\":[]}}}",
            result.ProviderMetadata!.Value.GetRawText()
        );
    }

    private static HttpResponseMessage Error(HttpStatusCode status, string body)
    {
        return new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
    }
}
