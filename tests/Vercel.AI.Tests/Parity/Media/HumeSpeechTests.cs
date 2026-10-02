// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Hume;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Tests.Parity.Media;

namespace Vercel.AI.Tests;

public sealed class HumeSpeechTests
{
    private const string SpeechUrl = "https://api.hume.ai/v0/tts/file";

    private const string ErrorBody = "\n{\"error\":{\"message\":\"{\\n  \\\"error\\\": {\\n    \\\"code\\\": 429,\\n    \\\"message\\\": \\\"Resource has been exhausted (e.g. check quota).\\\",\\n    \\\"status\\\": \\\"RESOURCE_EXHAUSTED\\\"\\n  }\\n}\\n\",\"code\":429}}\n";

    [Fact]
    [UpstreamTest(
        "packages/hume/src/hume-error.test.ts::humeErrorDataSchema::should parse Hume resource exhausted error",
        Coverage = UpstreamCoverage.Covered)]
    public void Parses_a_hume_resource_exhausted_error()
    {
        Assert.True(HumeError.TryParse(ErrorBody, out var error));
        Assert.NotNull(error);
        Assert.Equal(429, error!.Code);
        Assert.Contains("Resource has been exhausted (e.g. check quota).", error.Message, StringComparison.Ordinal);
        Assert.Contains("RESOURCE_EXHAUSTED", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/hume/src/hume-speech-model.test.ts::doGenerate::should pass the model and text",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_text_default_voice_and_mp3()
    {
        var handler = Audio();
        await Model(handler).GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!"), CancellationToken.None);
        var body = MediaJson.Parse(handler.Calls[0].Body);
        var utterance = body["utterances"]![0]!;

        Assert.Equal(SpeechUrl, handler.Calls[0].Url);
        Assert.Equal("Hello from the AI SDK!", utterance["text"]!.GetValue<string>());
        Assert.Equal(HumeSpeechRequest.DefaultVoiceId, utterance["voice"]!["id"]!.GetValue<string>());
        Assert.Equal("HUME_AI", utterance["voice"]!["provider"]!.GetValue<string>());
        Assert.Equal("mp3", body["format"]!["type"]!.GetValue<string>());
        Assert.Null(utterance["speed"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/hume/src/hume-speech-model.test.ts::doGenerate::should pass headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_provider_and_request_headers()
    {
        var handler = Audio();
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = HumeProvider.Create(options, handler);
        await provider.Speech().GenerateAsync(
            new HumeSpeechRequest("Hello from the AI SDK!")
            {
                Headers = new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" },
            },
            CancellationToken.None);

        Assert.Equal("test-api-key", handler.Calls[0].Header("x-hume-api-key"));
        Assert.Equal("application/json", handler.Calls[0].Header("content-type"));
        Assert.Equal("provider-header-value", handler.Calls[0].Header("custom-provider-header"));
        Assert.Equal("request-header-value", handler.Calls[0].Header("custom-request-header"));
        Assert.Contains("ai-sdk/hume/0.0.0-test", handler.Calls[0].Header("user-agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/hume/src/hume-speech-model.test.ts::doGenerate::should pass options",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_voice_format_and_speed()
    {
        var handler = Audio();
        await Model(handler).GenerateAsync(
            new HumeSpeechRequest("Hello from the AI SDK!")
            {
                Voice = "test-voice",
                OutputFormat = "mp3",
                Speed = 1.5,
            },
            CancellationToken.None);
        var utterance = MediaJson.Parse(handler.Calls[0].Body)["utterances"]![0]!;

        Assert.Equal("test-voice", utterance["voice"]!["id"]!.GetValue<string>());
        Assert.Equal("HUME_AI", utterance["voice"]!["provider"]!.GetValue<string>());
        Assert.Equal(1.5d, utterance["speed"]!.GetValue<double>());
        Assert.Equal("mp3", MediaJson.Parse(handler.Calls[0].Body)["format"]!["type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/hume/src/hume-speech-model.test.ts::doGenerate::should return audio data with correct content type",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_audio_bytes()
    {
        var audio = new byte[100];
        var handler = new MediaHandler();
        handler.Bytes(SpeechUrl, audio, "audio/mp3", new Dictionary<string, string>
        {
            ["x-request-id"] = "test-request-id",
            ["x-ratelimit-remaining"] = "123",
        });
        var result = await Model(handler).GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!") { OutputFormat = "mp3" }, CancellationToken.None);

        Assert.Equal(audio, result.Audio);
        Assert.Equal("audio/mp3", result.ResponseHeaders["content-type"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/hume/src/hume-speech-model.test.ts::doGenerate::should include response data with timestamp, modelId and headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_timestamp_and_response_headers()
    {
        var handler = new MediaHandler();
        handler.Bytes(SpeechUrl, new byte[100], "audio/mp3", new Dictionary<string, string>
        {
            ["x-request-id"] = "test-request-id",
            ["x-ratelimit-remaining"] = "123",
        });
        var model = Model(handler);
        model.Clock = () => DateTimeOffset.UnixEpoch;
        var result = await model.GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!"), CancellationToken.None);

        Assert.Equal(DateTimeOffset.UnixEpoch, result.Timestamp);
        Assert.Equal(string.Empty, result.ModelId);
        Assert.Equal("audio/mp3", result.ResponseHeaders["content-type"]);
        Assert.Equal("test-request-id", result.ResponseHeaders["x-request-id"]);
        Assert.Equal("123", result.ResponseHeaders["x-ratelimit-remaining"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/hume/src/hume-speech-model.test.ts::doGenerate::should use real date when no custom date provider is specified",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_injected_clock_and_empty_model_id()
    {
        var model = Model(Audio());
        model.Clock = () => DateTimeOffset.UnixEpoch;
        var result = await model.GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!"), CancellationToken.None);

        Assert.Equal(DateTimeOffset.UnixEpoch, result.Timestamp);
        Assert.Equal(string.Empty, result.ModelId);
    }

    [Fact]
    [UpstreamTest(
        "packages/hume/src/hume-speech-model.test.ts::doGenerate::should handle different audio formats",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_audio_for_mp3_pcm_and_wav_responses()
    {
        foreach (var format in new[] { "mp3", "pcm", "wav" })
        {
            var audio = new byte[100];
            var handler = new MediaHandler();
            handler.Bytes(SpeechUrl, audio, "audio/" + format);
            var result = await Model(handler).GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!"), CancellationToken.None);
            Assert.Equal(audio, result.Audio);
            Assert.Equal("audio/" + format, result.ResponseHeaders["content-type"]);
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/hume/src/hume-speech-model.test.ts::doGenerate::should include warnings if any are generated",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_no_warnings_for_a_normal_call()
    {
        var result = await Model(Audio()).GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!"), CancellationToken.None);
        Assert.Empty(result.Warnings);
    }

    private static MediaHandler Audio()
    {
        var handler = new MediaHandler();
        handler.Bytes(SpeechUrl, new byte[100], "audio/mp3");
        return handler;
    }

    private static HumeSpeechModel Model(MediaHandler handler)
    {
        return HumeProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler).Speech();
    }
}
