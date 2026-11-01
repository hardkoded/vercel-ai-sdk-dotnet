// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.ElevenLabs;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.ElevenLabsSpeechModel;

public sealed class DoGenerateTests
{
    private const string Prefix = "packages/elevenlabs/src/elevenlabs-speech-model.test.ts::ElevenLabsSpeechModel > doGenerate::";

    [Theory]
    [InlineData("test-voice-id", "https://api.elevenlabs.io/v1/text-to-speech/test-voice-id?output_format=mp3_44100_128")]
    [InlineData(null, "https://api.elevenlabs.io/v1/text-to-speech/21m00Tcm4TlvDq8ikWAM?output_format=mp3_44100_128")]
    [UpstreamTest(
        Prefix + "should send the full text-to-speech request for voice $voice",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7062ab6818c90e135d068433ce99b11ac0dfa081")]
    public async Task Should_send_the_full_text_to_speech_request_for_voice(string? voice, string url)
    {
        var handler = new ParityHandler(_ => ParityHandler.Bytes(new byte[100], "audio/mp3"));
        var provider = ElevenLabsProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler);
        var model = (ElevenLabsProvider.ElevenLabsSpeechModel)provider.SpeechModel("eleven_multilingual_v2");

        await model.GenerateAsync(new ElevenLabsSpeechRequest("Hello, world!") { Voice = voice }, CancellationToken.None);

        Assert.Equal(url, handler.Calls[0].Uri.AbsoluteUri);
        using var body = JsonDocument.Parse(handler.Calls[0].Text);
        Assert.Equal("Hello, world!", body.RootElement.GetProperty("text").GetString());
        Assert.Equal("eleven_multilingual_v2", body.RootElement.GetProperty("model_id").GetString());
        Assert.Equal(2, body.RootElement.EnumerateObject().Count());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should surface the ElevenLabs error message and code",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7062ab6818c90e135d068433ce99b11ac0dfa081")]
    public async Task Should_surface_the_ElevenLabs_error_message_and_code()
    {
        const string body = """
            {"detail":{"type":"not_found","code":"voice_not_found","message":"A voice with voice_id 'missing-voice' was not found.","status":"voice_not_found","request_id":"req_test_speech"}}
            """;
        var handler = new ParityHandler(_ => Error(HttpStatusCode.NotFound, body));
        var provider = ElevenLabsProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler);
        var model = (ElevenLabsProvider.ElevenLabsSpeechModel)provider.SpeechModel("eleven_multilingual_v2");

        var error = Assert.IsAssignableFrom<ApiException>(await Record.ExceptionAsync(() =>
            model.GenerateAsync(new ElevenLabsSpeechRequest("Hello, world!") { Voice = "missing-voice" }, CancellationToken.None)));

        Assert.Equal("A voice with voice_id 'missing-voice' was not found. (voice_not_found)", error.Message);
        Assert.Equal(404, error.StatusCode);
        var data = Assert.IsType<ElevenLabsErrorData>(error.ErrorData);
        Assert.Equal("req_test_speech", data.Detail!.RequestId);
    }

    private static HttpResponseMessage Error(HttpStatusCode status, string body)
    {
        return new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
    }
}
