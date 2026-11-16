// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Xai;
using static Vercel.AI.Tests.Upstream.Xai.XaiTranscriptionModel.XaiTranscriptionTestSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiTranscriptionModel;

/// <summary>Port of <c>xai-transcription-model.test.ts</c> &gt; <c>doGenerate</c>.</summary>
public sealed class XaiTranscriptionModelDoGenerateTests
{
    private const string Prefix = "packages/xai/src/xai-transcription-model.test.ts::doGenerate::";
    private const string Commit = "41465fd1b634dfe8311885230589e19c9b4f431e";

    private const string ProviderOptionsJson = """
        {"xai":{"audioFormat":"pcm","sampleRate":16000,"language":"en","format":true,"multichannel":true,"channels":2,"diarize":true,"keyterm":["AI SDK","Grok"],"fillerWords":true}}
        """;

    [Fact]
    [UpstreamTest(Prefix + "should expose word timings separately from segments", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_expose_word_timings_separately_from_segments()
    {
        var model = Model(PrepareJsonResponse());

        var result = await DoGenerate(model, timestampGranularities: new[] { "word", "segment" });

        var expected = new[] { ("Hello", 0.0, 1.0), ("from the AI SDK!", 1.0, 2.5) };
        Assert.Equal(expected, result.Words!.Select(word => (word.Text, word.StartSecond, word.EndSecond)));
        Assert.Equal(expected, result.Segments.Select(word => (word.Text, word.StartSecond, word.EndSecond)));
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("timestampGranularities", warning.Feature);
        Assert.Equal("xAI transcription returns word timings and does not provide separate caption segments.", warning.Details);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send a multipart request with the audio file", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_a_multipart_request_with_the_audio_file()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(Model(handler));

        Assert.Contains("filename=audio.wav", handler.Body);
        Assert.Contains("Content-Type: audio/wav", handler.Body);
        Assert.Equal(Url, handler.Uri);
    }

    [Fact]
    [UpstreamTest(Prefix + "should map provider options onto xAI request fields", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_provider_options_onto_xAI_request_fields()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(Model(handler), "audio/pcm", ProviderOptionsJson);

        foreach (var (name, value) in new[]
        {
            ("audio_format", "pcm"),
            ("sample_rate", "16000"),
            ("language", "en"),
            ("format", "true"),
            ("multichannel", "true"),
            ("channels", "2"),
            ("diarize", "true"),
            ("keyterm", "AI SDK"),
            ("keyterm", "Grok"),
            ("filler_words", "true"),
        })
        {
            Assert.Matches("name=\"?" + name + "\"?\\r\\n\\r\\n" + value + "\\r\\n", handler.Body);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve diarized words in provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_diarized_words_in_provider_metadata()
    {
        const string words = """[{"text":"Hello","start":0,"end":1,"speaker":0},{"text":"from the AI SDK!","start":1,"end":2.5,"speaker":1}]""";
        var handler = new UpstreamRecordingHandler("{\"text\":\"Hello from the AI SDK!\",\"language\":\"en\",\"duration\":2.5,\"words\":" + words + "}");

        var result = await DoGenerate(Model(handler), providerOptions: """{"xai":{"diarize":true}}""");

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"xai\":{\"words\":" + words + "}}"), JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())));
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass the opus audio format through for batch requests", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_the_opus_audio_format_through_for_batch_requests()
    {
        var handler = new UpstreamRecordingHandler("{}", HttpStatusCode.BadRequest);

        await Assert.ThrowsAnyAsync<Exception>(() => DoGenerate(Model(handler), "audio/ogg", """{"xai":{"audioFormat":"opus"}}"""));

        Assert.Matches("name=\"?audio_format\"?\\r\\n\\r\\nopus\\r\\n", handler.Body);
        Assert.Contains("filename=audio.ogg", handler.Body);
    }

    [Fact]
    [UpstreamTest(Prefix + "should append file after all other multipart fields", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_append_file_after_all_other_multipart_fields()
    {
        var handler = new UpstreamRecordingHandler("""{"text":"Hello from the AI SDK!"}""");

        await DoGenerate(Model(handler), "audio/pcm", ProviderOptionsJson);

        Assert.Equal(
            new[] { "sample_rate", "language", "multichannel", "channels", "diarize", "filler_words", "keyterm", "keyterm", "audio_format", "format", "file" },
            FieldNames(handler.Body));
    }

    [Fact]
    [UpstreamTest(Prefix + "should send the model id and vad_threshold before the file field", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_the_model_id_and_vad_threshold_before_the_file_field()
    {
        var handler = new UpstreamRecordingHandler("""{"text":"Hello from the AI SDK!"}""");

        var result = await DoGenerate(Model(handler, "grok-voice-transcribe-2.0"), providerOptions: """{"xai":{"vadThreshold":0.3}}""");

        Assert.Equal(new[] { "model", "vad_threshold", "file" }, FieldNames(handler.Body));
        Assert.Matches("name=\"?model\"?\\r\\n\\r\\ngrok-voice-transcribe-2.0\\r\\n", handler.Body);
        Assert.Matches("name=\"?vad_threshold\"?\\r\\n\\r\\n0.3\\r\\n", handler.Body);
        Assert.Equal("grok-voice-transcribe-2.0", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send the model id selected via the provider", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_the_model_id_selected_via_the_provider()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate((ITranscriptionCaller)CreateProvider(handler).TranscriptionModel("grok-voice-transcribe-2.0"));

        Assert.Matches("name=\"?model\"?\\r\\n\\r\\ngrok-voice-transcribe-2.0\\r\\n", handler.Body);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not send a model when no model id is set", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_send_a_model_when_no_model_id_is_set()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(Model(handler));

        Assert.DoesNotContain("model", FieldNames(handler.Body));
    }

    [Fact]
    [UpstreamTest(Prefix + "should keep the model id across a workflow serialization round trip", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_keep_the_model_id_across_a_workflow_serialization_round_trip()
    {
        var handler = new UpstreamRecordingHandler("""{"text":"Hello from the AI SDK!"}""");
        var original = Model(null, "grok-voice-transcribe-2.0");

        var serialized = original.Serialize();
        Assert.Equal("grok-voice-transcribe-2.0", serialized.ModelId);

        var restored = Vercel.AI.Xai.XaiTranscriptionModel.Deserialize(
            new SerializedModelOptions(serialized.ModelId, JsonSerializer.Deserialize<Dictionary<string, object?>>(JsonSerializer.Serialize(serialized.Config))!),
            "test-api-key",
            handler);
        await DoGenerate(restored);

        Assert.Equal("grok-voice-transcribe-2.0", restored.ModelId);
        Assert.Matches("name=\"?model\"?\\r\\n\\r\\ngrok-voice-transcribe-2.0\\r\\n", handler.Body);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass headers and the xAI user agent", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers_and_the_xAI_user_agent()
    {
        var handler = PrepareJsonResponse();
        var provider = CreateProvider(handler, options => options.Headers["Custom-Provider-Header"] = "provider-header-value");

        await DoGenerate(
            provider.TranscriptionModel(),
            headers: new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" });

        Assert.Equal("Bearer test-api-key", handler.Headers["Authorization"]);
        Assert.Matches("^multipart/form-data; boundary=", handler.Headers["Content-Type"]);
        Assert.Equal("provider-header-value", handler.Headers["custom-provider-header"]);
        Assert.Equal("request-header-value", handler.Headers["custom-request-header"]);
        Assert.Contains("ai-sdk/xai/" + AiSdkVersion.Version, handler.Headers["User-Agent"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract text, segments, language, and duration", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_text_segments_language_and_duration()
    {
        var result = await DoGenerate(Model(PrepareJsonResponse()));

        Assert.Equal("Hello from the AI SDK!", result.Text);
        Assert.Equal("en", result.Language);
        Assert.Equal(2.5, result.DurationInSeconds);
        Assert.Equal(
            new[] { ("Hello", 0.0, 1.0), ("from the AI SDK!", 1.0, 2.5) },
            result.Segments.Select(segment => (segment.Text, segment.StartSecond, segment.EndSecond)));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include response timestamp, model id, and headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_response_timestamp_model_id_and_headers()
    {
        var handler = PrepareJsonResponse(("x-request-id", "test-request-id"), ("x-ratelimit-remaining", "123"));
        var testDate = new DateTimeOffset(DateTime.UnixEpoch, TimeSpan.Zero);

        var result = await DoGenerate(Model(handler, clock: () => testDate));

        Assert.Equal(testDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal(string.Empty, result.Response.ModelId);
        Assert.StartsWith("application/json", result.Response.Headers!["content-type"]);
        Assert.Equal("test-request-id", result.Response.Headers["x-request-id"]);
        Assert.Equal("123", result.Response.Headers["x-ratelimit-remaining"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle missing words, duration, and empty language", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_missing_words_duration_and_empty_language()
    {
        var handler = new UpstreamRecordingHandler("""{"text":"Hello from the AI SDK!","language":""}""");

        var result = await DoGenerate(Model(handler));

        Assert.Equal("Hello from the AI SDK!", result.Text);
        Assert.Null(result.Language);
        Assert.Null(result.DurationInSeconds);
        Assert.Empty(result.Segments);
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("undefined")]
    [InlineData("null")]
    [InlineData("[]")]
    [UpstreamTest(Prefix + "should warn when a nonempty response omits requested word timings ($words)", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_warn_when_a_nonempty_response_omits_requested_word_timings(string words)
    {
        var handler = new UpstreamRecordingHandler(words == "undefined" ? """{"text":"Hello"}""" : "{\"text\":\"Hello\",\"words\":" + words + "}");

        var result = await DoGenerate(Model(handler), timestampGranularities: new[] { "word" });

        Assert.Null(result.Words);
        Assert.Empty(result.Segments);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("The xAI transcription response omitted the requested word timings.", warning.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not warn about omitted word timings for an empty transcript", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_not_warn_about_omitted_word_timings_for_an_empty_transcript()
    {
        var handler = new UpstreamRecordingHandler("""{"text":"","words":[]}""");

        var result = await DoGenerate(Model(handler), timestampGranularities: new[] { "word" });

        Assert.Equal(string.Empty, result.Text);
        Assert.Null(result.Words);
        Assert.Empty(result.Warnings);
    }
}
