// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.ElevenLabs;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class ElevenLabsParityTests
{
    private const string Speech = "packages/elevenlabs/src/elevenlabs-speech-model.test.ts::ElevenLabsSpeechModel > doGenerate::";
    private const string Transcription = "packages/elevenlabs/src/elevenlabs-transcription-model.test.ts::doGenerate";
    private const string ExhaustedMessage = "{\n  \"error\": {\n    \"code\": 429,\n    \"message\": \"Resource has been exhausted (e.g. check quota).\",\n    \"status\": \"RESOURCE_EXHAUSTED\"\n  }\n}\n";
    private const string TranscriptJson = "{\"language_code\":\"eng\",\"language_probability\":0.8905608654022217,\"text\":\"Hello from the Vercel AI SDK.\",\"words\":["
        + "{\"text\":\"Hello\",\"start\":0.199,\"end\":0.479,\"type\":\"word\",\"speaker_id\":\"speaker_0\",\"logprob\":0},"
        + "{\"text\":\" \",\"start\":0.479,\"end\":0.499,\"type\":\"spacing\",\"speaker_id\":\"speaker_0\",\"logprob\":0},"
        + "{\"text\":\"from\",\"start\":0.5,\"end\":0.639,\"type\":\"word\",\"speaker_id\":\"speaker_0\",\"logprob\":0},"
        + "{\"text\":\" \",\"start\":0.639,\"end\":0.66,\"type\":\"spacing\",\"speaker_id\":\"speaker_0\",\"logprob\":0},"
        + "{\"text\":\"the\",\"start\":0.66,\"end\":0.759,\"type\":\"word\",\"speaker_id\":\"speaker_0\",\"logprob\":0},"
        + "{\"text\":\" \",\"start\":0.759,\"end\":0.759,\"type\":\"spacing\",\"speaker_id\":\"speaker_0\",\"logprob\":0},"
        + "{\"text\":\"Vercel\",\"start\":0.759,\"end\":1.12,\"type\":\"word\",\"speaker_id\":\"speaker_0\",\"logprob\":0},"
        + "{\"text\":\" \",\"start\":1.12,\"end\":1.2,\"type\":\"spacing\",\"speaker_id\":\"speaker_0\",\"logprob\":0},"
        + "{\"text\":\"AI\",\"start\":1.2,\"end\":1.519,\"type\":\"word\",\"speaker_id\":\"speaker_0\",\"logprob\":0},"
        + "{\"text\":\" \",\"start\":1.519,\"end\":1.58,\"type\":\"spacing\",\"speaker_id\":\"speaker_0\",\"logprob\":0},"
        + "{\"text\":\"SDK.\",\"start\":1.58,\"end\":2.479,\"type\":\"word\",\"speaker_id\":\"speaker_0\",\"logprob\":0}"
        + "],\"transcription_id\":\"dRgXxwt3SzzliHA3nDAQ\"}";

    private static readonly Regex FormField = new Regex("name=\"?(?<name>[^\";\\r\\n]+)\"?[^\\r\\n]*\\r\\n(?:[^\\r\\n]+\\r\\n)*\\r\\n(?<value>[^\\r\\n]*)\\r\\n", RegexOptions.Compiled);

    [Fact]
    [UpstreamTest("packages/elevenlabs/src/elevenlabs-error.test.ts::elevenlabsErrorDataSchema::should parse ElevenLabs resource exhausted error", Coverage = UpstreamCoverage.Covered)]
    public void Error_parser_reads_the_resource_exhausted_error()
    {
        var json = new JsonObject { ["error"] = new JsonObject { ["message"] = ExhaustedMessage, ["code"] = 429 } }.ToJsonString();
        var result = ElevenLabsError.Parse(json);
        Assert.True(result.Success);
        Assert.Equal(ExhaustedMessage, result.Value!.Message);
        Assert.Equal(429, result.Value.Code);
        Assert.Equal(result.Value.Message, result.RawValue!.Message);
        Assert.Equal(result.Value.Code, result.RawValue.Code);
    }

    [Fact]
    [UpstreamTest(Speech + "should generate speech with required parameters", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_sends_text_model_and_default_output_format()
    {
        var handler = SpeechHandler();
        await SpeechModel(handler).GenerateAsync(new ElevenLabsSpeechRequest("Hello, world!") { Voice = "test-voice-id" }, CancellationToken.None);
        var body = JsonNode.Parse(handler.Calls[0].Text)!;
        Assert.Equal("Hello, world!", body["text"]!.GetValue<string>());
        Assert.Equal("eleven_multilingual_v2", body["model_id"]!.GetValue<string>());
        Assert.Equal("https://api.elevenlabs.io/v1/text-to-speech/test-voice-id?output_format=mp3_44100_128", handler.Calls[0].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(Speech + "should handle custom output format", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_sends_a_custom_output_format()
    {
        var handler = SpeechHandler();
        await SpeechModel(handler).GenerateAsync(new ElevenLabsSpeechRequest("Hello, world!") { Voice = "test-voice-id", OutputFormat = "pcm_44100" }, CancellationToken.None);
        var body = JsonNode.Parse(handler.Calls[0].Text)!;
        Assert.Equal("Hello, world!", body["text"]!.GetValue<string>());
        Assert.Equal("eleven_multilingual_v2", body["model_id"]!.GetValue<string>());
        Assert.Contains("output_format=pcm_44100", handler.Calls[0].Uri.Query, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Speech + "should handle language parameter", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_sends_the_language_code()
    {
        var handler = SpeechHandler();
        await SpeechModel(handler).GenerateAsync(new ElevenLabsSpeechRequest("Hola, mundo!") { Voice = "test-voice-id", Language = "es" }, CancellationToken.None);
        var body = JsonNode.Parse(handler.Calls[0].Text)!;
        Assert.Equal("Hola, mundo!", body["text"]!.GetValue<string>());
        Assert.Equal("eleven_multilingual_v2", body["model_id"]!.GetValue<string>());
        Assert.Equal("es", body["language_code"]!.GetValue<string>());
        Assert.Contains("output_format=mp3_44100_128", handler.Calls[0].Uri.Query, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Speech + "should handle speed parameter in voice settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_sends_speed_in_voice_settings()
    {
        var handler = SpeechHandler();
        await SpeechModel(handler).GenerateAsync(new ElevenLabsSpeechRequest("Hello, world!") { Voice = "test-voice-id", Speed = 1.5 }, CancellationToken.None);
        var body = JsonNode.Parse(handler.Calls[0].Text)!;
        Assert.Equal("Hello, world!", body["text"]!.GetValue<string>());
        Assert.Equal(1.5, body["voice_settings"]!["speed"]!.GetValue<double>());
    }

    [Fact]
    [UpstreamTest(Speech + "should warn about unsupported instructions parameter", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_warns_about_instructions()
    {
        var result = await SpeechModel(SpeechHandler()).GenerateAsync(new ElevenLabsSpeechRequest("Hello, world!") { Voice = "test-voice-id", Instructions = "Speak slowly" }, CancellationToken.None);
        var warning = Assert.IsType<UnsupportedWarning>(Assert.Single(result.Warnings));
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("instructions", warning.Feature);
        Assert.Equal("ElevenLabs speech models do not support instructions. Instructions parameter was ignored.", warning.Details);
    }

    [Fact]
    [UpstreamTest(Speech + "should pass provider-specific options", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_maps_provider_options_to_snake_case()
    {
        var handler = SpeechHandler();
        var request = new ElevenLabsSpeechRequest("Hello, world!")
        {
            Voice = "test-voice-id",
            ProviderOptions = Options("{\"voiceSettings\":{\"stability\":0.5,\"similarityBoost\":0.75},\"seed\":123}"),
        };
        await SpeechModel(handler).GenerateAsync(request, CancellationToken.None);
        var body = JsonNode.Parse(handler.Calls[0].Text)!;
        Assert.Equal("Hello, world!", body["text"]!.GetValue<string>());
        Assert.Equal("eleven_multilingual_v2", body["model_id"]!.GetValue<string>());
        Assert.Equal(0.5, body["voice_settings"]!["stability"]!.GetValue<double>());
        Assert.Equal(0.75, body["voice_settings"]!["similarity_boost"]!.GetValue<double>());
        Assert.Equal(123, body["seed"]!.GetValue<int>());
        Assert.Contains("output_format=mp3_44100_128", handler.Calls[0].Uri.Query, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Speech + "should include user-agent header", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_sends_the_user_agent()
    {
        var handler = SpeechHandler();
        await SpeechModel(handler).GenerateAsync(new ElevenLabsSpeechRequest("Hello, world!") { Voice = "test-voice-id" }, CancellationToken.None);
        Assert.Contains("ai-sdk/elevenlabs/" + AiSdkVersion.Version, handler.Calls[0].Header("User-Agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Transcription + "::should reject scribe_v2_realtime for non-streaming transcription", Coverage = UpstreamCoverage.Covered)]
    public async Task Realtime_model_rejects_batch_transcription()
    {
        var model = TranscriptionModel(TranscriptHandler(), "scribe_v2_realtime");
        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => model.TranscribeAsync(Audio(), null, CancellationToken.None));
    }

    [Fact]
    [UpstreamTest(Transcription + "::warns when streaming options are passed to batch transcription", Coverage = UpstreamCoverage.Covered)]
    public async Task Streaming_options_warn_on_batch_transcription()
    {
        var request = new ElevenLabsTranscriptionRequest { ProviderOptions = Options("{\"streaming\":{\"includeTimestamps\":true}}") };
        var result = await TranscriptionModel(TranscriptHandler()).TranscribeAsync(Audio(), request, CancellationToken.None);
        var warning = Assert.IsType<UnsupportedWarning>(Assert.Single(result.Warnings));
        Assert.Equal("providerOptions.elevenlabs.streaming", warning.Feature);
        Assert.Equal("ElevenLabs batch transcription does not support streaming options.", warning.Details);
    }

    [Fact]
    [UpstreamTest(Transcription + " > transcription::should pass the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_sends_the_model_id()
    {
        var handler = TranscriptHandler();
        await TranscriptionModel(handler).TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Contains(("model_id", "scribe_v1"), Fields(handler.Calls[0].Text));
    }

    [Fact]
    [UpstreamTest(Transcription + " > transcription::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_sends_key_multipart_custom_headers_and_user_agent()
    {
        var handler = TranscriptHandler();
        var options = new ElevenLabsOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var model = (ElevenLabsProvider.ElevenLabsTranscriptionModel)ElevenLabsProvider.Create(options, handler).TranscriptionModel("scribe_v1");
        var request = new ElevenLabsTranscriptionRequest { Headers = new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" } };
        await model.TranscribeAsync(Audio(), request, CancellationToken.None);
        var call = handler.Calls[0];
        Assert.Equal("test-api-key", call.Header("xi-api-key"));
        Assert.StartsWith("multipart/form-data; boundary=", call.Header("Content-Type"), StringComparison.Ordinal);
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Contains("ai-sdk/elevenlabs/" + AiSdkVersion.Version, call.Header("User-Agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Transcription + " > transcription::should extract the transcription text", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_returns_the_text()
    {
        var result = await TranscriptionModel(TranscriptHandler()).TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal("Hello from the Vercel AI SDK.", result.Text);
    }

    [Fact]
    [UpstreamTest(Transcription + " > transcription::should pass provider options correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_sends_provider_options_as_form_fields()
    {
        var handler = TranscriptHandler();
        var request = new ElevenLabsTranscriptionRequest
        {
            ProviderOptions = Options("{\"languageCode\":\"en\",\"fileFormat\":\"pcm_s16le_16\",\"tagAudioEvents\":false,\"numSpeakers\":2,\"timestampsGranularity\":\"character\",\"diarize\":true}"),
        };
        await TranscriptionModel(handler).TranscribeAsync(Audio(), request, CancellationToken.None);
        var fields = Fields(handler.Calls[0].Text);
        Assert.Contains(fields, field => field.Name == "file");
        var rest = fields.Where(field => field.Name != "file").Distinct().OrderBy(field => field.Name, StringComparer.Ordinal).ToArray();
        Assert.Equal(
            new[]
            {
                ("diarize", "true"),
                ("file_format", "pcm_s16le_16"),
                ("language_code", "en"),
                ("model_id", "scribe_v1"),
                ("num_speakers", "2"),
                ("tag_audio_events", "false"),
                ("timestamps_granularity", "character"),
            },
            rest);
    }

    [Fact]
    [UpstreamTest(Transcription + " > response headers::should include response data with timestamp, modelId and headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_response_has_timestamp_model_headers_and_body()
    {
        var result = await TranscriptionModel(TranscriptHandler(("x-request-id", "test-request-id"), ("x-ratelimit-remaining", "123"))).TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("scribe_v1", result.Response.ModelId);
        Assert.Equal("test-request-id", result.Response.Headers["x-request-id"]);
        Assert.Equal("123", result.Response.Headers["x-ratelimit-remaining"]);
        Assert.Equal("application/json", result.Response.Headers["Content-Type"]);
        using var expected = JsonDocument.Parse(TranscriptJson);
        Assert.True(JsonElement.DeepEquals(expected.RootElement, result.Response.Body!.Value));
    }

    [Fact]
    [UpstreamTest(Transcription + " > response metadata::should use real date when no custom date provider is specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_uses_the_configured_clock()
    {
        var result = await TranscriptionModel(TranscriptHandler()).TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("scribe_v1", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(Transcription + " > no additional formats::should work when no additional formats are returned", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_maps_words_language_and_duration()
    {
        var result = await TranscriptionModel(TranscriptHandler()).TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal("Hello from the Vercel AI SDK.", result.Text);
        Assert.Equal("eng", result.Language);
        Assert.Equal(2.479, result.DurationInSeconds);
        Assert.Empty(result.Warnings);
        Assert.Equal(11, result.Segments.Count);
        Assert.Equal(("Hello", 0.199, 0.479), (result.Segments[0].Text, result.Segments[0].StartSecond, result.Segments[0].EndSecond));
        Assert.Equal((" ", 0.759, 0.759), (result.Segments[5].Text, result.Segments[5].StartSecond, result.Segments[5].EndSecond));
        Assert.Equal(("SDK.", 1.58, 2.479), (result.Segments[10].Text, result.Segments[10].StartSecond, result.Segments[10].EndSecond));
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("scribe_v1", result.Response.ModelId);
        Assert.Equal("dRgXxwt3SzzliHA3nDAQ", result.Response.Body!.Value.GetProperty("transcription_id").GetString());
    }

    private static ElevenLabsProvider.ElevenLabsSpeechModel SpeechModel(ParityHandler handler)
    {
        return (ElevenLabsProvider.ElevenLabsSpeechModel)ElevenLabsProvider.Create(new ElevenLabsOptions { ApiKey = "test-api-key" }, handler).SpeechModel("eleven_multilingual_v2");
    }

    private static ElevenLabsProvider.ElevenLabsTranscriptionModel TranscriptionModel(ParityHandler handler, string modelId = "scribe_v1")
    {
        var options = new ElevenLabsOptions { ApiKey = "test-api-key", Clock = () => DateTimeOffset.UnixEpoch };
        return (ElevenLabsProvider.ElevenLabsTranscriptionModel)ElevenLabsProvider.Create(options, handler).TranscriptionModel(modelId);
    }

    private static ParityHandler SpeechHandler()
    {
        return new ParityHandler(_ => ParityHandler.Bytes(new byte[100], "audio/mp3"));
    }

    private static ParityHandler TranscriptHandler(params (string Name, string Value)[] headers)
    {
        return new ParityHandler(call => call.Uri.AbsoluteUri == "https://api.elevenlabs.io/v1/speech-to-text"
            ? ParityHandler.Json(TranscriptJson, headers)
            : new HttpResponseMessage(HttpStatusCode.NotFound));
    }

    private static AudioInput Audio()
    {
        return new AudioInput(new byte[] { 1, 2, 3 }, "audio/wav", null);
    }

    private static JsonElement Options(string elevenlabs)
    {
        return JsonDocument.Parse("{\"elevenlabs\":" + elevenlabs + "}").RootElement.Clone();
    }

    private static List<(string Name, string Value)> Fields(string multipart)
    {
        return FormField.Matches(multipart).Select(match => (match.Groups["name"].Value, match.Groups["value"].Value)).ToList();
    }
}
