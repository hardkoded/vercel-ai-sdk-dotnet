// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gladia;
using Vercel.AI.Hume;
using Vercel.AI.Provider;
using Vercel.AI.RevAI;

namespace Vercel.AI.Tests;

public sealed class SpeechToTextParityTests
{
    private const string ExhaustedMessage = "{\n  \"error\": {\n    \"code\": 429,\n    \"message\": \"Resource has been exhausted (e.g. check quota).\",\n    \"status\": \"RESOURCE_EXHAUSTED\"\n  }\n}\n";

    private const string RevTranscript = "{\"monologues\":[{\"speaker\":0,\"elements\":[{\"type\":\"text\",\"value\":\"Hello\",\"ts\":0.075,\"end_ts\":0.425},{\"type\":\"punct\",\"value\":\" \"},{\"type\":\"text\",\"value\":\"from\",\"ts\":0.425,\"end_ts\":0.665},{\"type\":\"punct\",\"value\":\" \"},{\"type\":\"text\",\"value\":\"the\",\"ts\":0.665,\"end_ts\":0.785},{\"type\":\"punct\",\"value\":\" \"},{\"type\":\"text\",\"value\":\"Sal\",\"ts\":0.945,\"end_ts\":1.105},{\"type\":\"punct\",\"value\":\",\"},{\"type\":\"punct\",\"value\":\" \"},{\"type\":\"text\",\"value\":\"A-I-S-D-K\",\"ts\":1.185,\"end_ts\":2.145},{\"type\":\"punct\",\"value\":\".\"}]}]}";

    private static string ExhaustedJson()
    {
        return new JsonObject
        {
            ["error"] = new JsonObject { ["message"] = ExhaustedMessage, ["code"] = 429 },
        }.ToJsonString();
    }

    [Fact]
    [UpstreamTest("packages/revai/src/revai-error.test.ts::revaiErrorDataSchema::should parse Rev.ai resource exhausted error", Coverage = UpstreamCoverage.Covered)]
    public void Revai_parses_the_resource_exhausted_error()
    {
        var result = RevAIError.Parse(ExhaustedJson());
        Assert.True(result.Success);
        Assert.Equal(ExhaustedMessage, result.Value!.Message);
        Assert.Equal(429, result.Value.Code);
        Assert.Equal(result.Value.Message, result.RawValue!.Message);
        Assert.Equal(result.Value.Code, result.RawValue.Code);
    }

    [Fact]
    [UpstreamTest("packages/revai/src/revai-transcription-model.test.ts::doGenerate > transcription::should pass the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Revai_sends_the_transcriber_in_the_config_part()
    {
        var handler = RevaiHandler();
        var provider = RevAIProvider.Create(new RevAIOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);
        await ((RevAIProvider.RevAITranscriptionModel)provider.TranscriptionModel("machine")).TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Contains("name=\"media\"", handler.Calls[0].Text, StringComparison.Ordinal);
        Assert.Contains("{\"transcriber\":\"machine\"}", handler.Calls[0].Text, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/revai/src/revai-transcription-model.test.ts::doGenerate > transcription::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Revai_sends_authorization_custom_headers_and_user_agent()
    {
        var handler = RevaiHandler();
        var options = new RevAIOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = RevAIProvider.Create(options, handler);
        var request = new RevAITranscriptionRequest { Headers = new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" } };
        await ((RevAIProvider.RevAITranscriptionModel)provider.TranscriptionModel("machine")).TranscribeAsync(Audio(), request, CancellationToken.None);
        var call = handler.Calls[0];
        Assert.Equal("Bearer test-api-key", call.Header("Authorization"));
        Assert.StartsWith("multipart/form-data", call.Header("Content-Type"), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Contains("ai-sdk/revai/0.0.0-test", call.Header("User-Agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/revai/src/revai-transcription-model.test.ts::doGenerate > transcription::should extract the transcription text", Coverage = UpstreamCoverage.Covered)]
    public async Task Revai_joins_monologue_values_into_the_transcript()
    {
        var handler = RevaiHandler();
        var provider = RevAIProvider.Create(new RevAIOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);
        var result = await ((RevAIProvider.RevAITranscriptionModel)provider.TranscriptionModel("machine")).TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal("Hello from the Sal, A-I-S-D-K.", result.Text);
        Assert.Equal(2.145, result.DurationInSeconds, 3);
        Assert.Equal(5, result.Segments.Count);
        Assert.Equal("Hello", result.Segments[0].Text);
        Assert.Equal("A-I-S-D-K", result.Segments[4].Text);
    }

    [Fact]
    [UpstreamTest("packages/revai/src/revai-transcription-model.test.ts::doGenerate > response headers::should include response data with timestamp, modelId and headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Revai_returns_the_transcript_timestamp_model_and_headers()
    {
        var handler = RevaiHandler(("x-request-id", "test-request-id"), ("x-ratelimit-remaining", "123"));
        var provider = RevAIProvider.Create(new RevAIOptions { ApiKey = "test-api-key", Clock = () => DateTimeOffset.UnixEpoch, PollDelay = _ => Task.CompletedTask }, handler);
        var result = await ((RevAIProvider.RevAITranscriptionModel)provider.TranscriptionModel("machine")).TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("machine", result.Response.ModelId);
        Assert.Equal("test-request-id", Header(result.Response.Headers, "x-request-id"));
        Assert.Equal("123", Header(result.Response.Headers, "x-ratelimit-remaining"));
        Assert.Contains("application/json", Header(result.Response.Headers, "content-type"), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("A-I-S-D-K", result.Response.Body, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/revai/src/revai-transcription-model.test.ts::doGenerate > response metadata::should use real date when no custom date provider is specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Revai_uses_the_injected_clock_and_model_id()
    {
        var handler = RevaiHandler();
        var provider = RevAIProvider.Create(new RevAIOptions { ApiKey = "test-api-key", Clock = () => DateTimeOffset.UnixEpoch, PollDelay = _ => Task.CompletedTask }, handler);
        var result = await ((RevAIProvider.RevAITranscriptionModel)provider.TranscriptionModel("machine")).TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("machine", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-error.test.ts::gladiaErrorDataSchema::should parse Gladia resource exhausted error", Coverage = UpstreamCoverage.Covered)]
    public void Gladia_parses_the_resource_exhausted_error()
    {
        var result = GladiaError.Parse(ExhaustedJson());
        Assert.True(result.Success);
        Assert.Equal(ExhaustedMessage, result.Value!.Message);
        Assert.Equal(429, result.Value.Code);
        Assert.Equal(result.Value.Message, result.RawValue!.Message);
        Assert.Equal(result.Value.Code, result.RawValue.Code);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should pass audio_url to pre-recorded endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task Gladia_posts_the_uploaded_audio_url()
    {
        var handler = GladiaHandler();
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);
        await provider.Transcription().TranscribeAsync(Audio(), null, CancellationToken.None);
        using var body = JsonDocument.Parse(handler.Calls[1].Text);
        Assert.Equal("https://api.gladia.io/file/7403f025-be30-4335-ae29-20131e0adbd6", body.RootElement.GetProperty("audio_url").GetString());
        Assert.Equal("https://api.gladia.io/v2/pre-recorded", handler.Calls[1].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::does not send the API key when the result URL is on a foreign origin", Coverage = UpstreamCoverage.Covered)]
    public async Task Gladia_strips_the_api_key_for_a_foreign_result_url()
    {
        var handler = GladiaHandler(foreign: true);
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);
        await provider.Transcription().TranscribeAsync(Audio(), null, CancellationToken.None);
        var poll = handler.Calls.Single(call => call.Uri.Host == "cdn.evil.example");
        Assert.Null(poll.Header("x-gladia-key"));
        Assert.Null(poll.Header("Authorization"));
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Gladia_sends_the_api_key_custom_headers_and_user_agent()
    {
        var handler = GladiaHandler();
        var options = new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = GladiaProvider.Create(options, handler);
        var request = new GladiaTranscriptionRequest { Headers = new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" } };
        await provider.Transcription().TranscribeAsync(Audio(), request, CancellationToken.None);
        var call = handler.Calls[1];
        Assert.Equal("test-api-key", call.Header("x-gladia-key"));
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Contains("ai-sdk/gladia/0.0.0-test", handler.Calls[0].Header("User-Agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should extract the transcription text", Coverage = UpstreamCoverage.Covered)]
    public async Task Gladia_returns_the_full_transcript()
    {
        var handler = GladiaHandler();
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);
        var result = await provider.Transcription().TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal(GladiaText, result.Text);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should preserve utterance metadata in provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Gladia_keeps_utterance_metadata_on_the_raw_result()
    {
        var handler = GladiaHandler(metadata: true);
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);
        var result = await provider.Transcription().TranscribeAsync(Audio(), null, CancellationToken.None);
        var utterances = result.ProviderMetadata.GetProperty("gladia").GetProperty("result").GetProperty("transcription").GetProperty("utterances");
        Assert.Equal(0, utterances[0].GetProperty("speaker").GetInt32());
        Assert.Equal(0.91, utterances[0].GetProperty("confidence").GetDouble(), 2);
        Assert.Equal("en", utterances[0].GetProperty("language").GetString());
        Assert.Equal("Galileo", utterances[0].GetProperty("words")[0].GetProperty("word").GetString());
        Assert.Equal("speaker-1", utterances[1].GetProperty("speaker").GetString());
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should generate full response", Coverage = UpstreamCoverage.Covered)]
    public async Task Gladia_returns_the_full_transcription_response()
    {
        var handler = GladiaHandler();
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", Clock = () => DateTimeOffset.UnixEpoch, PollDelay = _ => Task.CompletedTask }, handler);
        var result = await provider.Transcription().TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal(GladiaText, result.Text);
        Assert.Equal("en", result.Language);
        Assert.Equal(36.74, result.DurationInSeconds!.Value, 2);
        Assert.Empty(result.Warnings);
        Assert.Equal(11, result.Segments.Count);
        Assert.Equal(0.14, result.Segments[0].StartSecond, 2);
        Assert.Equal(5.341, result.Segments[0].EndSecond, 3);
        Assert.StartsWith("Galileo was an American robotic space program", result.Segments[0].Text, StringComparison.Ordinal);
        Assert.Equal("and became the first spacecraft to orbit Jupiter.", result.Segments[10].Text);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("default", result.Response.ModelId);
        Assert.Contains("application/json", Header(result.Response.Headers, "content-type"), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(JsonValueKind.Object, result.ProviderMetadata.ValueKind);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > response headers::should include response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Gladia_includes_the_result_timestamp_model_and_content_type()
    {
        var handler = GladiaHandler();
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", Clock = () => DateTimeOffset.UnixEpoch, PollDelay = _ => Task.CompletedTask }, handler);
        var result = await provider.Transcription().TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("default", result.Response.ModelId);
        Assert.Contains("application/json", Header(result.Response.Headers, "content-type"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > response metadata::should include timestamp and modelId", Coverage = UpstreamCoverage.Covered)]
    public async Task Gladia_includes_the_timestamp_and_model_id()
    {
        var handler = GladiaHandler();
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", Clock = () => DateTimeOffset.UnixEpoch, PollDelay = _ => Task.CompletedTask }, handler);
        var result = await provider.Transcription().TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("default", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/hume/src/hume-error.test.ts::humeErrorDataSchema::should parse Hume resource exhausted error", Coverage = UpstreamCoverage.Covered)]
    public void Hume_parses_the_resource_exhausted_error()
    {
        var result = HumeError.Parse(ExhaustedJson());
        Assert.True(result.Success);
        Assert.Equal(ExhaustedMessage, result.Value!.Message);
        Assert.Equal(429, result.Value.Code);
        Assert.Equal(result.Value.Message, result.RawValue!.Message);
        Assert.Equal(result.Value.Code, result.RawValue.Code);
    }

    [Fact]
    [UpstreamTest("packages/hume/src/hume-speech-model.test.ts::doGenerate::should pass the model and text", Coverage = UpstreamCoverage.Covered)]
    public async Task Hume_sends_the_default_voice_and_mp3_format()
    {
        var handler = HumeHandler(new byte[100]);
        var provider = HumeProvider.Create(new HumeOptions { ApiKey = "test-api-key" }, handler);
        await provider.Speech().GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!"), CancellationToken.None);
        using var body = JsonDocument.Parse(handler.Calls[0].Text);
        var utterance = body.RootElement.GetProperty("utterances")[0];
        Assert.Equal("Hello from the AI SDK!", utterance.GetProperty("text").GetString());
        Assert.Equal(HumeSpeechRequest.DefaultVoiceId, utterance.GetProperty("voice").GetProperty("id").GetString());
        Assert.Equal("HUME_AI", utterance.GetProperty("voice").GetProperty("provider").GetString());
        Assert.Equal("mp3", body.RootElement.GetProperty("format").GetProperty("type").GetString());
        Assert.False(utterance.TryGetProperty("speed", out _));
    }

    [Fact]
    [UpstreamTest("packages/hume/src/hume-speech-model.test.ts::doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Hume_sends_the_api_key_custom_headers_and_user_agent()
    {
        var handler = HumeHandler(new byte[100]);
        var options = new HumeOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = HumeProvider.Create(options, handler);
        await provider.Speech().GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!") { Headers = new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" } }, CancellationToken.None);
        var call = handler.Calls[0];
        Assert.Equal("test-api-key", call.Header("X-Hume-Api-Key"));
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Contains("ai-sdk/hume/0.0.0-test", call.Header("User-Agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/hume/src/hume-speech-model.test.ts::doGenerate::should pass options", Coverage = UpstreamCoverage.Covered)]
    public async Task Hume_sends_voice_speed_and_format()
    {
        var handler = HumeHandler(new byte[100]);
        var provider = HumeProvider.Create(new HumeOptions { ApiKey = "test-api-key" }, handler);
        await provider.Speech().GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!") { Voice = "test-voice", OutputFormat = "mp3", Speed = 1.5 }, CancellationToken.None);
        using var body = JsonDocument.Parse(handler.Calls[0].Text);
        var utterance = body.RootElement.GetProperty("utterances")[0];
        Assert.Equal("test-voice", utterance.GetProperty("voice").GetProperty("id").GetString());
        Assert.Equal(1.5, utterance.GetProperty("speed").GetDouble(), 3);
        Assert.Equal("mp3", body.RootElement.GetProperty("format").GetProperty("type").GetString());
    }

    [Fact]
    [UpstreamTest("packages/hume/src/hume-speech-model.test.ts::doGenerate::should return audio data with correct content type", Coverage = UpstreamCoverage.Covered)]
    public async Task Hume_returns_the_audio_bytes()
    {
        var audio = new byte[100];
        var handler = HumeHandler(audio, ("x-request-id", "test-request-id"), ("x-ratelimit-remaining", "123"));
        var provider = HumeProvider.Create(new HumeOptions { ApiKey = "test-api-key" }, handler);
        var result = await provider.Speech().GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!") { OutputFormat = "mp3" }, CancellationToken.None);
        Assert.Equal(audio, result.Audio);
        Assert.Equal("audio/mp3", result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/hume/src/hume-speech-model.test.ts::doGenerate::should include response data with timestamp, modelId and headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Hume_returns_the_timestamp_and_response_headers()
    {
        var handler = HumeHandler(new byte[8], ("x-request-id", "test-request-id"), ("x-ratelimit-remaining", "123"));
        var provider = HumeProvider.Create(new HumeOptions { ApiKey = "test-api-key", Clock = () => DateTimeOffset.UnixEpoch }, handler);
        var result = await provider.Speech().GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!"), CancellationToken.None);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal(string.Empty, result.Response.ModelId);
        Assert.Equal("audio/mp3", Header(result.Response.Headers, "content-type"));
        Assert.Equal("test-request-id", Header(result.Response.Headers, "x-request-id"));
        Assert.Equal("123", Header(result.Response.Headers, "x-ratelimit-remaining"));
    }

    [Fact]
    [UpstreamTest("packages/hume/src/hume-speech-model.test.ts::doGenerate::should use real date when no custom date provider is specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Hume_uses_the_injected_clock_and_empty_model_id()
    {
        var handler = HumeHandler(new byte[4]);
        var provider = HumeProvider.Create(new HumeOptions { ApiKey = "test-api-key", Clock = () => DateTimeOffset.UnixEpoch }, handler);
        var result = await provider.Speech().GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!"), CancellationToken.None);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal(string.Empty, result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/hume/src/hume-speech-model.test.ts::doGenerate::should handle different audio formats", Coverage = UpstreamCoverage.Covered)]
    public async Task Hume_returns_each_audio_payload()
    {
        foreach (var format in new[] { "mp3", "pcm", "wav" })
        {
            var audio = new byte[] { (byte)format[0], 2, 3 };
            var handler = HumeHandler(audio, contentType: "audio/" + format);
            var provider = HumeProvider.Create(new HumeOptions { ApiKey = "test-api-key" }, handler);
            var result = await provider.Speech().GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!"), CancellationToken.None);
            Assert.Equal(audio, result.Audio);
        }
    }

    [Fact]
    [UpstreamTest("packages/hume/src/hume-speech-model.test.ts::doGenerate::should include warnings if any are generated", Coverage = UpstreamCoverage.Covered)]
    public async Task Hume_returns_no_warnings_for_a_plain_request()
    {
        var handler = HumeHandler(new byte[4]);
        var provider = HumeProvider.Create(new HumeOptions { ApiKey = "test-api-key" }, handler);
        var result = await provider.Speech().GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!"), CancellationToken.None);
        Assert.Empty(result.Warnings);
    }

    private static AudioInput Audio()
    {
        return new AudioInput(new byte[] { 1, 2, 3, 4 }, "audio/wav", "audio.wav");
    }

    private static RecordingHandler RevaiHandler(params (string Name, string Value)[] headers)
    {
        return new RecordingHandler(call =>
        {
            if (call.Uri.AbsolutePath.EndsWith("/transcript", StringComparison.Ordinal))
            {
                return RecordingHandler.Json(RevTranscript, headers);
            }

            if (call.Method == HttpMethod.Get)
            {
                return RecordingHandler.Json("{\"id\":\"test-id\",\"status\":\"transcribed\",\"language\":\"en\"}", headers);
            }

            return RecordingHandler.Json("{\"id\":\"test-id\",\"status\":\"in_progress\",\"language\":\"en\"}", headers);
        });
    }

    private const string GladiaText = "Galileo was an American robotic space program that studied the planet Jupiter and its moons, as well as several other solar system bodies. Named after the Italian astronomer Galileo Galilei, the Galileo spacecraft consisted of an orbiter and an atmospheric entry probe. It was delivered into Earth orbit on October 18, 1989, by Space Shuttle Atlantis on the STS-34 mission. and arrived at Jupiter on December 7, 1995, after gravity-assist flybys of Venus and Earth, and became the first spacecraft to orbit Jupiter.";

    private static string GladiaResult(bool metadata)
    {
        var speaker = metadata ? "\"speaker\":\"speaker-1\"" : "\"speaker\":1";
        return "{\"status\":\"done\",\"result\":{\"metadata\":{\"audio_duration\":36.74},\"transcription\":{\"full_transcript\":" + JsonSerializer.Serialize(GladiaText) + ",\"languages\":[\"en\"],\"utterances\":["
            + "{\"text\":\"Galileo was an American robotic space program that studied the planet Jupiter and its moons,\",\"start\":0.14,\"end\":5.341,\"speaker\":0,\"confidence\":0.91,\"language\":\"en\",\"words\":[{\"word\":\"Galileo\"}]},"
            + "{\"text\":\"as well as several other solar system bodies.\",\"start\":5.662,\"end\":8.099," + speaker + "},"
            + "{\"text\":\"Named after the Italian astronomer Galileo Galilei,\",\"start\":9.138,\"end\":12.122},"
            + "{\"text\":\"the Galileo spacecraft consisted of an orbiter and an atmospheric entry probe.\",\"start\":12.419,\"end\":17.248},"
            + "{\"text\":\"It was delivered into Earth orbit on October 18,\",\"start\":18.185,\"end\":20.919},"
            + "{\"text\":\"1989,\",\"start\":20.92,\"end\":22.498},"
            + "{\"text\":\"by Space Shuttle Atlantis on the STS-34 mission.\",\"start\":22.499,\"end\":25.966},"
            + "{\"text\":\"and arrived at Jupiter on December 7,\",\"start\":26.4,\"end\":28.525},"
            + "{\"text\":\"1995,\",\"start\":28.526,\"end\":29.847},"
            + "{\"text\":\"after gravity-assist flybys of Venus and Earth,\",\"start\":30.247,\"end\":32.851},"
            + "{\"text\":\"and became the first spacecraft to orbit Jupiter.\",\"start\":33.132,\"end\":35.816}"
            + "]}}}";
    }

    private static RecordingHandler GladiaHandler(bool foreign = false, bool metadata = false)
    {
        var resultUrl = foreign
            ? "https://cdn.evil.example/v2/pre-recorded/result"
            : "https://api.gladia.io/v2/pre-recorded/7b0137fd-5fd6-4d08-b9bf-4cdf9876797d";
        return new RecordingHandler(call =>
        {
            if (call.Uri.AbsolutePath.EndsWith("/upload", StringComparison.Ordinal))
            {
                return RecordingHandler.Json("{\"audio_url\":\"https://api.gladia.io/file/7403f025-be30-4335-ae29-20131e0adbd6\"}");
            }

            if (call.Method == HttpMethod.Post)
            {
                return RecordingHandler.Json("{\"id\":\"7b0137fd-5fd6-4d08-b9bf-4cdf9876797d\",\"result_url\":\"" + resultUrl + "\"}");
            }

            return RecordingHandler.Json(GladiaResult(metadata));
        });
    }

    private static RecordingHandler HumeHandler(byte[] audio, params (string Name, string Value)[] headers)
    {
        return HumeHandler(audio, "audio/mp3", headers);
    }

    private static RecordingHandler HumeHandler(byte[] audio, string contentType, params (string Name, string Value)[] headers)
    {
        return new RecordingHandler(_ => RecordingHandler.Bytes(audio, contentType, headers));
    }

    private static string? Header(IReadOnlyDictionary<string, string> headers, string name)
    {
        foreach (var pair in headers)
        {
            if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value;
            }
        }

        return null;
    }
}
