// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gladia;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.GladiaTranscriptionModel;

public sealed class DoGenerateTranscriptionTests
{
    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should pass audio_url to pre-recorded endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassAudioUrlToPreRecordedEndpoint()
    {
        var handler = GladiaFixtures.Handler();
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);
        await provider.Transcription().TranscribeAsync(GladiaFixtures.Audio(), null, CancellationToken.None);
        using var body = JsonDocument.Parse(handler.Calls[1].Text);
        Assert.Equal("https://api.gladia.io/file/7403f025-be30-4335-ae29-20131e0adbd6", body.RootElement.GetProperty("audio_url").GetString());
        Assert.Equal("https://api.gladia.io/v2/pre-recorded", handler.Calls[1].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::does not send the API key when the result URL is on a foreign origin", Coverage = UpstreamCoverage.Covered)]
    public async Task DoesNotSendTheApiKeyWhenTheResultUrlIsOnAForeignOrigin()
    {
        var handler = GladiaFixtures.Handler(foreign: true);
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);
        await provider.Transcription().TranscribeAsync(GladiaFixtures.Audio(), null, CancellationToken.None);
        var poll = handler.Calls.Single(call => call.Uri.Host == "cdn.evil.example");
        Assert.Null(poll.Header("x-gladia-key"));
        Assert.Null(poll.Header("Authorization"));
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassHeaders()
    {
        var handler = GladiaFixtures.Handler();
        var options = new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = GladiaProvider.Create(options, handler);
        var request = new GladiaTranscriptionRequest { Headers = new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" } };
        await provider.Transcription().TranscribeAsync(GladiaFixtures.Audio(), request, CancellationToken.None);
        var call = handler.Calls[1];
        Assert.Equal("test-api-key", call.Header("x-gladia-key"));
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Contains("ai-sdk/gladia/" + AiSdkVersion.Version, handler.Calls[0].Header("User-Agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should extract the transcription text", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldExtractTheTranscriptionText()
    {
        var handler = GladiaFixtures.Handler();
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);
        var result = await provider.Transcription().TranscribeAsync(GladiaFixtures.Audio(), null, CancellationToken.None);
        Assert.Equal(GladiaFixtures.Text, result.Text);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should preserve utterance metadata in provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPreserveUtteranceMetadataInProviderMetadata()
    {
        var handler = GladiaFixtures.Handler(metadata: true);
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);
        var result = await provider.Transcription().TranscribeAsync(GladiaFixtures.Audio(), null, CancellationToken.None);
        var utterances = result.ProviderMetadata.GetProperty("gladia").GetProperty("result").GetProperty("transcription").GetProperty("utterances");
        Assert.Equal(0, utterances[0].GetProperty("speaker").GetInt32());
        Assert.Equal(0.91, utterances[0].GetProperty("confidence").GetDouble(), 2);
        Assert.Equal("en", utterances[0].GetProperty("language").GetString());
        Assert.Equal("Galileo", utterances[0].GetProperty("words")[0].GetProperty("word").GetString());
        Assert.Equal("speaker-1", utterances[1].GetProperty("speaker").GetString());
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should preserve audio intelligence results in provider metadata", Coverage = UpstreamCoverage.Covered, Commit = "9207c2b1e040938ac6753d766da4ee9672127ae5")]
    public async Task ShouldPreserveAudioIntelligenceResultsInProviderMetadata()
    {
        var fixture = JsonNode.Parse("""
            {
              "metadata": { "number_of_distinct_channels": 1, "billing_time": 36.74, "transcription_time": 12.5 },
              "transcription": {
                "sentences": [{ "sentence": "Galileo was an American robotic space program.", "start": 0.14, "end": 5.341, "words": [] }],
                "subtitles": [{ "format": "srt", "subtitles": "1\n00:00:00.140 --> 00:00:05.341\nGalileo" }]
              },
              "translation": {
                "success": true, "is_empty": false, "exec_time": 2.4, "error": null,
                "results": [{
                  "languages": ["fr"],
                  "full_transcript": "Galilée était un programme spatial.",
                  "utterances": [{ "start": 0.14, "end": 5.341, "text": "Galilée était un programme spatial.", "language": "fr", "channel": 0, "confidence": 0.9, "words": [] }]
                }]
              },
              "summarization": { "success": true, "is_empty": false, "exec_time": 1.2, "error": null, "results": "A summary of the transcription." },
              "named_entity_recognition": {
                "success": true, "is_empty": false, "exec_time": 0.5, "error": null,
                "results": [{ "entity_type": "ORIGIN", "text": "American", "start": 1.02, "end": 1.48 }]
              },
              "sentiment_analysis": {
                "success": true, "is_empty": false, "exec_time": 0.6, "error": null,
                "results": [{ "text": "Galileo was an American robotic space program.", "sentiment": "neutral", "emotion": "interest", "start": 0.14, "end": 5.341, "channel": 0 }]
              },
              "audio_to_llm": {
                "success": true, "is_empty": false, "exec_time": 3.1, "error": null,
                "results": [{ "results": { "model": "openai/gpt-5.4-nano", "prompt": "What is this about?", "response": "A space program." } }]
              },
              "sentences": {
                "success": true, "is_empty": false, "exec_time": 0.4, "error": null,
                "results": [{ "sentence": "Galileo was an American robotic space program.", "start": 0.14, "end": 5.341, "words": [] }]
              }
            }
            """)!.AsObject();
        var response = GladiaFixtures.Result();
        var result = response["result"]!.AsObject();
        foreach (var pair in fixture.ToArray())
        {
            fixture.Remove(pair.Key);
            if (pair.Key is "metadata" or "transcription")
            {
                foreach (var inner in pair.Value!.AsObject().ToArray())
                {
                    pair.Value.AsObject().Remove(inner.Key);
                    result[pair.Key]![inner.Key] = inner.Value;
                }
            }
            else
            {
                result[pair.Key] = pair.Value;
            }
        }

        var handler = GladiaFixtures.Handler(result: response);
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);

        var transcription = await provider.Transcription().TranscribeAsync(GladiaFixtures.Audio(), null, CancellationToken.None);

        var gladia = transcription.ProviderMetadata.GetProperty("gladia").GetProperty("result");
        var metadata = gladia.GetProperty("metadata");
        Assert.Equal(36.74, metadata.GetProperty("audio_duration").GetDouble(), 2);
        Assert.Equal(1, metadata.GetProperty("number_of_distinct_channels").GetInt32());
        Assert.Equal(36.74, metadata.GetProperty("billing_time").GetDouble(), 2);
        Assert.Equal(12.5, metadata.GetProperty("transcription_time").GetDouble(), 2);
        var subtitles = gladia.GetProperty("transcription").GetProperty("subtitles")[0];
        Assert.Equal("srt", subtitles.GetProperty("format").GetString());
        Assert.Equal("1\n00:00:00.140 --> 00:00:05.341\nGalileo", subtitles.GetProperty("subtitles").GetString());
        Assert.Equal("Galileo was an American robotic space program.", gladia.GetProperty("transcription").GetProperty("sentences")[0].GetProperty("sentence").GetString());
        var translation = gladia.GetProperty("translation").GetProperty("results")[0];
        Assert.Equal("fr", translation.GetProperty("languages")[0].GetString());
        Assert.Equal("Galilée était un programme spatial.", translation.GetProperty("full_transcript").GetString());
        Assert.Equal("Galilée était un programme spatial.", translation.GetProperty("utterances")[0].GetProperty("text").GetString());
        Assert.Equal(0, translation.GetProperty("utterances")[0].GetProperty("channel").GetInt32());
        Assert.Equal("A summary of the transcription.", gladia.GetProperty("summarization").GetProperty("results").GetString());
        var entity = gladia.GetProperty("named_entity_recognition").GetProperty("results")[0];
        Assert.Equal("ORIGIN", entity.GetProperty("entity_type").GetString());
        Assert.Equal("American", entity.GetProperty("text").GetString());
        Assert.Equal(1.02, entity.GetProperty("start").GetDouble(), 2);
        Assert.Equal(1.48, entity.GetProperty("end").GetDouble(), 2);
        var sentiment = gladia.GetProperty("sentiment_analysis").GetProperty("results")[0];
        Assert.Equal("neutral", sentiment.GetProperty("sentiment").GetString());
        Assert.Equal("interest", sentiment.GetProperty("emotion").GetString());
        var llm = gladia.GetProperty("audio_to_llm").GetProperty("results")[0].GetProperty("results");
        Assert.Equal("openai/gpt-5.4-nano", llm.GetProperty("model").GetString());
        Assert.Equal("What is this about?", llm.GetProperty("prompt").GetString());
        Assert.Equal("A space program.", llm.GetProperty("response").GetString());
        Assert.Equal("Galileo was an American robotic space program.", gladia.GetProperty("sentences").GetProperty("results")[0].GetProperty("sentence").GetString());
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should pass through add-ons that do not match the documented shape", Coverage = UpstreamCoverage.Covered, Commit = "9207c2b1e040938ac6753d766da4ee9672127ae5")]
    public async Task ShouldPassThroughAddOnsThatDoNotMatchTheDocumentedShape()
    {
        var response = GladiaFixtures.Result();
        var result = response["result"]!.AsObject();
        result["named_entity_recognition"] = JsonNode.Parse("""
            {
              "success": true, "is_empty": false, "exec_time": 0.5,
              "error": { "status_code": 500, "message": "Upstream failure" },
              "results": [{ "entity_type": "ORIGIN", "start": 1.02, "end": 1.48 }]
            }
            """);
        result["sentences"] = JsonNode.Parse("""
            {
              "success": true, "is_empty": false, "exec_time": 0.4, "error": null,
              "results": ["Galileo was an American robotic space program.", "It studied the planet Jupiter."]
            }
            """);
        var handler = GladiaFixtures.Handler(result: response);
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);

        var transcription = await provider.Transcription().TranscribeAsync(GladiaFixtures.Audio(), null, CancellationToken.None);

        Assert.Equal(GladiaFixtures.Text, transcription.Text);
        var gladia = transcription.ProviderMetadata.GetProperty("gladia").GetProperty("result");
        var entities = gladia.GetProperty("named_entity_recognition");
        Assert.Equal(500, entities.GetProperty("error").GetProperty("status_code").GetInt32());
        Assert.Equal("Upstream failure", entities.GetProperty("error").GetProperty("message").GetString());
        var entity = entities.GetProperty("results")[0];
        Assert.Equal("ORIGIN", entity.GetProperty("entity_type").GetString());
        Assert.Equal(1.02, entity.GetProperty("start").GetDouble(), 2);
        Assert.Equal(1.48, entity.GetProperty("end").GetDouble(), 2);
        var sentences = gladia.GetProperty("sentences").GetProperty("results");
        Assert.Equal("Galileo was an American robotic space program.", sentences[0].GetString());
        Assert.Equal("It studied the planet Jupiter.", sentences[1].GetString());
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should preserve audio-to-LLM item fields in provider metadata", Coverage = UpstreamCoverage.Covered, Commit = "9207c2b1e040938ac6753d766da4ee9672127ae5")]
    public async Task ShouldPreserveAudioToLlmItemFieldsInProviderMetadata()
    {
        var response = GladiaFixtures.Result();
        response["result"]!["audio_to_llm"] = JsonNode.Parse("""
            {
              "success": true, "is_empty": false, "exec_time": 1.6, "error": null,
              "results": [
                { "prompt": "What is this about?", "response": "A space program.", "success": true, "is_empty": false, "exec_time": 1.4, "error": null },
                { "prompt": "Who is speaking?", "response": null, "success": false, "is_empty": true, "exec_time": 0.2,
                  "error": { "status_code": 400, "exception": "BadRequest", "message": "Prompt rejected" } }
              ]
            }
            """);
        var handler = GladiaFixtures.Handler(result: response);
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);

        var transcription = await provider.Transcription().TranscribeAsync(GladiaFixtures.Audio(), null, CancellationToken.None);

        var items = transcription.ProviderMetadata.GetProperty("gladia").GetProperty("result").GetProperty("audio_to_llm").GetProperty("results");
        Assert.Equal(2, items.GetArrayLength());
        Assert.Equal("What is this about?", items[0].GetProperty("prompt").GetString());
        Assert.Equal("A space program.", items[0].GetProperty("response").GetString());
        Assert.True(items[0].GetProperty("success").GetBoolean());
        Assert.False(items[0].GetProperty("is_empty").GetBoolean());
        Assert.Equal(1.4, items[0].GetProperty("exec_time").GetDouble(), 2);
        Assert.Equal(JsonValueKind.Null, items[0].GetProperty("error").ValueKind);
        Assert.Equal("Who is speaking?", items[1].GetProperty("prompt").GetString());
        Assert.Equal(JsonValueKind.Null, items[1].GetProperty("response").ValueKind);
        Assert.False(items[1].GetProperty("success").GetBoolean());
        Assert.True(items[1].GetProperty("is_empty").GetBoolean());
        Assert.Equal(0.2, items[1].GetProperty("exec_time").GetDouble(), 2);
        Assert.Equal(400, items[1].GetProperty("error").GetProperty("status_code").GetInt32());
        Assert.Equal("BadRequest", items[1].GetProperty("error").GetProperty("exception").GetString());
        Assert.Equal("Prompt rejected", items[1].GetProperty("error").GetProperty("message").GetString());
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should include the Gladia error code in the error message", Coverage = UpstreamCoverage.Covered, Commit = "9207c2b1e040938ac6753d766da4ee9672127ae5")]
    public async Task ShouldIncludeTheGladiaErrorCodeInTheErrorMessage()
    {
        var response = GladiaFixtures.Result();
        response["status"] = "error";
        response["error_code"] = 400;
        response["result"] = null;
        var handler = GladiaFixtures.Handler(result: response);
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);

        var error = await Assert.ThrowsAsync<AiSdkError>(() => provider.Transcription().TranscribeAsync(GladiaFixtures.Audio(), null, CancellationToken.None));

        Assert.Contains("Transcription job failed with error code 400", error.Message, StringComparison.Ordinal);
        Assert.Equal("TranscriptionJobFailed", error.ErrorName);
        var cause = Assert.IsType<GladiaTranscriptionJobResultException>(error.Cause);
        Assert.Equal(400, cause.ErrorCode);
        Assert.Equal("error", cause.Result["status"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should generate full response", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldExtractTheTranscriptionTextion_response()
    {
        var handler = GladiaFixtures.Handler();
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", Clock = () => DateTimeOffset.UnixEpoch, PollDelay = _ => Task.CompletedTask }, handler);
        var result = await provider.Transcription().TranscribeAsync(GladiaFixtures.Audio(), null, CancellationToken.None);
        Assert.Equal(GladiaFixtures.Text, result.Text);
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
        Assert.Contains("application/json", ContentType(result.Response.Headers), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(JsonValueKind.Object, result.ProviderMetadata.ValueKind);
    }

    private static string? ContentType(IReadOnlyDictionary<string, string> headers)
    {
        foreach (var pair in headers)
        {
            if (string.Equals(pair.Key, "content-type", StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value;
            }
        }

        return null;
    }
}
