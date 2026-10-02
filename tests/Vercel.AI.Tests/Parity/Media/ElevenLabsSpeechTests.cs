// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.ElevenLabs;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Tests.Parity.Media;

namespace Vercel.AI.Tests;

public sealed class ElevenLabsSpeechTests
{
    private const string ErrorBody = "\n{\"error\":{\"message\":\"{\\n  \\\"error\\\": {\\n    \\\"code\\\": 429,\\n    \\\"message\\\": \\\"Resource has been exhausted (e.g. check quota).\\\",\\n    \\\"status\\\": \\\"RESOURCE_EXHAUSTED\\\"\\n  }\\n}\\n\",\"code\":429}}\n";

    private static readonly byte[] Audio = new byte[100];

    [Fact]
    [UpstreamTest(
        "packages/elevenlabs/src/elevenlabs-error.test.ts::elevenlabsErrorDataSchema::should parse ElevenLabs resource exhausted error",
        Coverage = UpstreamCoverage.Covered)]
    public void Parses_a_resource_exhausted_error()
    {
        Assert.True(ElevenLabsError.TryParse(ErrorBody, out var error));
        Assert.NotNull(error);
        Assert.Equal(429, error!.Code);
        Assert.Contains("Resource has been exhausted (e.g. check quota).", error.Message, StringComparison.Ordinal);
        Assert.Contains("RESOURCE_EXHAUSTED", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/elevenlabs/src/elevenlabs-speech-model.test.ts::ElevenLabsSpeechModel > doGenerate::should generate speech with required parameters",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_text_model_and_the_default_mp3_format()
    {
        var handler = Server("test-voice-id", "mp3_44100_128");
        await Generate(handler, new ElevenLabsSpeechRequest("Hello, world!") { Voice = "test-voice-id" });
        var body = MediaJson.Parse(handler.Calls[0].Body);

        Assert.Equal("Hello, world!", body["text"]!.GetValue<string>());
        Assert.Equal("eleven_multilingual_v2", body["model_id"]!.GetValue<string>());
        Assert.Contains("output_format=mp3_44100_128", handler.Calls[0].Url, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/elevenlabs/src/elevenlabs-speech-model.test.ts::ElevenLabsSpeechModel > doGenerate::should handle custom output format",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_a_pcm_output_format_query()
    {
        var handler = Server("test-voice-id", "pcm_44100");
        await Generate(handler, new ElevenLabsSpeechRequest("Hello, world!") { Voice = "test-voice-id", OutputFormat = "pcm_44100" });
        var body = MediaJson.Parse(handler.Calls[0].Body);

        Assert.Equal("Hello, world!", body["text"]!.GetValue<string>());
        Assert.Equal("eleven_multilingual_v2", body["model_id"]!.GetValue<string>());
        Assert.Contains("output_format=pcm_44100", handler.Calls[0].Url, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/elevenlabs/src/elevenlabs-speech-model.test.ts::ElevenLabsSpeechModel > doGenerate::should handle language parameter",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_language_code()
    {
        var handler = Server("test-voice-id", "mp3_44100_128");
        await Generate(handler, new ElevenLabsSpeechRequest("Hola, mundo!") { Voice = "test-voice-id", Language = "es" });
        var body = MediaJson.Parse(handler.Calls[0].Body);

        Assert.Equal("Hola, mundo!", body["text"]!.GetValue<string>());
        Assert.Equal("eleven_multilingual_v2", body["model_id"]!.GetValue<string>());
        Assert.Equal("es", body["language_code"]!.GetValue<string>());
        Assert.Contains("output_format=mp3_44100_128", handler.Calls[0].Url, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/elevenlabs/src/elevenlabs-speech-model.test.ts::ElevenLabsSpeechModel > doGenerate::should handle speed parameter in voice settings",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_speed_inside_voice_settings()
    {
        var handler = Server("test-voice-id", "mp3_44100_128");
        await Generate(handler, new ElevenLabsSpeechRequest("Hello, world!") { Voice = "test-voice-id", Speed = 1.5 });
        var body = MediaJson.Parse(handler.Calls[0].Body);

        Assert.Equal("Hello, world!", body["text"]!.GetValue<string>());
        Assert.Equal("eleven_multilingual_v2", body["model_id"]!.GetValue<string>());
        Assert.Equal(1.5, body["voice_settings"]!["speed"]!.GetValue<double>());
    }

    [Fact]
    [UpstreamTest(
        "packages/elevenlabs/src/elevenlabs-speech-model.test.ts::ElevenLabsSpeechModel > doGenerate::should warn about unsupported instructions parameter",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_when_instructions_are_set()
    {
        var handler = Server("test-voice-id", "mp3_44100_128");
        var result = await Generate(handler, new ElevenLabsSpeechRequest("Hello, world!") { Voice = "test-voice-id", Instructions = "Speak slowly" });
        var warning = Assert.Single(result.Warnings);

        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("instructions", warning.Feature);
        Assert.Equal("ElevenLabs speech models do not support instructions. Instructions parameter was ignored.", warning.Details);
    }

    [Fact]
    [UpstreamTest(
        "packages/elevenlabs/src/elevenlabs-speech-model.test.ts::ElevenLabsSpeechModel > doGenerate::should pass provider-specific options",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_voice_settings_and_seed()
    {
        var handler = Server("test-voice-id", "mp3_44100_128");
        await Generate(
            handler,
            new ElevenLabsSpeechRequest("Hello, world!")
            {
                Voice = "test-voice-id",
                ProviderOptions = Options("{\"voiceSettings\":{\"stability\":0.5,\"similarityBoost\":0.75},\"seed\":123}"),
            });
        var body = MediaJson.Parse(handler.Calls[0].Body);

        Assert.Equal("Hello, world!", body["text"]!.GetValue<string>());
        Assert.Equal("eleven_multilingual_v2", body["model_id"]!.GetValue<string>());
        Assert.Equal(0.5, body["voice_settings"]!["stability"]!.GetValue<double>());
        Assert.Equal(0.75, body["voice_settings"]!["similarity_boost"]!.GetValue<double>());
        Assert.Equal(123, body["seed"]!.GetValue<int>());
        Assert.Contains("output_format=mp3_44100_128", handler.Calls[0].Url, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/elevenlabs/src/elevenlabs-speech-model.test.ts::ElevenLabsSpeechModel > doGenerate::should include user-agent header",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_elevenlabs_user_agent()
    {
        var handler = Server("test-voice-id", "mp3_44100_128");
        await Generate(handler, new ElevenLabsSpeechRequest("Hello, world!") { Voice = "test-voice-id" });

        Assert.Contains("ai-sdk/elevenlabs/0.0.0-test", handler.Calls[0].Header("user-agent"), StringComparison.Ordinal);
        Assert.Equal("test-api-key", handler.Calls[0].Header("xi-api-key"));
    }

    private static Task<ElevenLabsSpeechGeneration> Generate(MediaHandler handler, ElevenLabsSpeechRequest request)
    {
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        var model = (ElevenLabsSpeechModel)ElevenLabsProvider.Create(options, handler).SpeechModel("eleven_multilingual_v2");
        return model.GenerateAsync(request, CancellationToken.None);
    }

    private static MediaHandler Server(string voice, string format)
    {
        var handler = new MediaHandler();
        handler.Bytes(
            "https://api.elevenlabs.io/v1/text-to-speech/" + voice + "?output_format=" + format,
            Audio,
            "audio/mp3");
        return handler;
    }

    private static JsonElement Options(string json)
    {
        using (var document = JsonDocument.Parse(json))
        {
            return document.RootElement.Clone();
        }
    }
}
