// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Gladia;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Tests.Parity.Media;

namespace Vercel.AI.Tests;

public sealed class GladiaTranscriptionTests
{
    private const string UploadUrl = "https://api.gladia.io/v2/upload";
    private const string PreRecordedUrl = "https://api.gladia.io/v2/pre-recorded";
    private const string ResultUrl = "https://api.gladia.io/v2/pre-recorded/job";
    private const string AudioUrl = "https://api.gladia.io/file/audio";

    private const string ErrorBody = "\n{\"error\":{\"message\":\"{\\n  \\\"error\\\": {\\n    \\\"code\\\": 429,\\n    \\\"message\\\": \\\"Resource has been exhausted (e.g. check quota).\\\",\\n    \\\"status\\\": \\\"RESOURCE_EXHAUSTED\\\"\\n  }\\n}\\n\",\"code\":429}}\n";

    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-error.test.ts::gladiaErrorDataSchema::should parse Gladia resource exhausted error",
        Coverage = UpstreamCoverage.Covered)]
    public void Parses_a_gladia_resource_exhausted_error()
    {
        Assert.True(GladiaError.TryParse(ErrorBody, out var error));
        Assert.NotNull(error);
        Assert.Equal(429, error!.Code);
        Assert.Contains("Resource has been exhausted (e.g. check quota).", error.Message, StringComparison.Ordinal);
        Assert.Contains("RESOURCE_EXHAUSTED", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should pass audio_url to pre-recorded endpoint",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_the_uploaded_audio_url()
    {
        var handler = Ready();
        await Model(handler).TranscribeAsync(Audio(), CancellationToken.None);

        Assert.Equal("POST", handler.Calls[0].Method);
        Assert.Equal(UploadUrl, handler.Calls[0].Url);
        Assert.StartsWith("multipart/form-data", handler.Calls[0].Header("content-type"), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("name=\"audio\"", handler.Calls[0].Body, StringComparison.Ordinal);
        Assert.Contains("filename=\"audio.wav\"", handler.Calls[0].Body, StringComparison.Ordinal);
        var initiate = MediaJson.Parse(handler.Calls[1].Body);
        Assert.Equal(AudioUrl, initiate["audio_url"]!.GetValue<string>());
        Assert.Equal(PreRecordedUrl, handler.Calls[1].Url);
    }

    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::does not send the API key when the result URL is on a foreign origin",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_the_api_key_on_a_foreign_result_url()
    {
        var foreign = "https://cdn.evil.example/v2/pre-recorded/result";
        var handler = new MediaHandler();
        handler.Json(UploadUrl, "{\"audio_url\":\"" + AudioUrl + "\"}");
        handler.Json(PreRecordedUrl, "{\"id\":\"job\",\"result_url\":\"" + foreign + "\"}");
        handler.Json(foreign, ResultJson("Hello from Gladia."));
        await Model(handler).TranscribeAsync(Audio(), CancellationToken.None);

        var poll = handler.Calls.Single(call => call.Url == foreign);
        Assert.Null(poll.Header("x-gladia-key"));
        Assert.Null(poll.Header("authorization"));
        Assert.Equal("test-api-key", handler.Calls[1].Header("x-gladia-key"));
    }

    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should pass headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_provider_and_request_headers()
    {
        var handler = Ready();
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = GladiaProvider.Create(options, handler);
        var model = (GladiaTranscriptionModel)provider.TranscriptionModel("default");
        await model.TranscribeAsync(
            new GladiaTranscriptionRequest(new byte[] { 1, 2, 3 }, "audio/wav")
            {
                Headers = new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" },
            },
            CancellationToken.None);

        Assert.Equal("test-api-key", handler.Calls[1].Header("x-gladia-key"));
        Assert.Equal("application/json", handler.Calls[1].Header("content-type"));
        Assert.Equal("provider-header-value", handler.Calls[1].Header("custom-provider-header"));
        Assert.Equal("request-header-value", handler.Calls[1].Header("custom-request-header"));
        Assert.Contains("ai-sdk/gladia/0.0.0-test", handler.Calls[0].Header("user-agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should extract the transcription text",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_the_full_transcript()
    {
        var result = await Model(Ready()).TranscribeAsync(Audio(), CancellationToken.None);
        Assert.Equal("Hello from Gladia.", result.Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should preserve utterance metadata in provider metadata",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_utterance_metadata()
    {
        var handler = new MediaHandler();
        handler.Json(UploadUrl, "{\"audio_url\":\"" + AudioUrl + "\"}");
        handler.Json(PreRecordedUrl, "{\"result_url\":\"" + ResultUrl + "\"}");
        handler.Json(ResultUrl, "{\"status\":\"done\",\"result\":{\"metadata\":{\"audio_duration\":1},\"transcription\":{\"full_transcript\":\"Hi there\",\"languages\":[\"en\"],\"utterances\":[{\"text\":\"Hi\",\"start\":0,\"end\":1,\"speaker\":0,\"confidence\":0.98,\"language\":\"en\",\"words\":[{\"word\":\"Hi\",\"start\":0,\"end\":1}]},{\"text\":\"there\",\"start\":1,\"end\":2,\"speaker\":\"speaker-1\",\"confidence\":0.5,\"language\":\"en\"}]}}}");
        var result = await Model(handler).TranscribeAsync(Audio(), CancellationToken.None);
        var utterances = result.ProviderMetadata!["result"]!["transcription"]!["utterances"]!.AsArray();

        Assert.Equal(0, utterances[0]!["speaker"]!.GetValue<int>());
        Assert.Equal(0.98d, utterances[0]!["confidence"]!.GetValue<double>());
        Assert.Equal("en", utterances[0]!["language"]!.GetValue<string>());
        Assert.Equal("Hi", utterances[0]!["words"]![0]!["word"]!.GetValue<string>());
        Assert.Equal("speaker-1", utterances[1]!["speaker"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should generate full response",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_text_segments_language_and_timestamp()
    {
        var model = Model(Ready());
        model.Clock = () => DateTimeOffset.UnixEpoch;
        var result = await model.TranscribeAsync(Audio(), CancellationToken.None);

        Assert.Equal("Hello from Gladia.", result.Text);
        Assert.Equal(36.74d, result.DurationSeconds);
        Assert.Equal("en", result.Language);
        Assert.Equal("default", result.ModelId);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Timestamp);
        Assert.Empty(result.Warnings);
        Assert.Equal("Hello from Gladia.", result.Segments[0].Text);
        Assert.Equal(0.14d, result.Segments[0].StartSecond);
        Assert.Equal(5.341d, result.Segments[0].EndSecond);
        Assert.NotNull(result.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > response headers::should include response headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_response_headers()
    {
        var handler = Ready(new Dictionary<string, string>
        {
            ["x-request-id"] = "test-request-id",
            ["x-ratelimit-remaining"] = "123",
        });
        var model = Model(handler);
        model.Clock = () => DateTimeOffset.UnixEpoch;
        var result = await model.TranscribeAsync(Audio(), CancellationToken.None);

        Assert.Equal(DateTimeOffset.UnixEpoch, result.Timestamp);
        Assert.Equal("default", result.ModelId);
        Assert.Equal("application/json", result.ResponseHeaders["content-type"]);
        Assert.Equal("test-request-id", result.ResponseHeaders["x-request-id"]);
        Assert.Equal("123", result.ResponseHeaders["x-ratelimit-remaining"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > response metadata::should include timestamp and modelId",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_timestamp_and_model_id()
    {
        var model = Model(Ready());
        model.Clock = () => DateTimeOffset.UnixEpoch;
        var result = await model.TranscribeAsync(Audio(), CancellationToken.None);

        Assert.Equal(DateTimeOffset.UnixEpoch, result.Timestamp);
        Assert.Equal("default", result.ModelId);
    }

    private static GladiaTranscriptionRequest Audio()
    {
        return new GladiaTranscriptionRequest(new byte[] { 1, 2, 3 }, "audio/wav");
    }

    private static GladiaTranscriptionModel Model(MediaHandler handler)
    {
        var provider = GladiaProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler);
        var model = (GladiaTranscriptionModel)provider.TranscriptionModel("default");
        model.PollInterval = TimeSpan.Zero;
        return model;
    }

    private static MediaHandler Ready(IDictionary<string, string>? headers = null)
    {
        var handler = new MediaHandler();
        handler.Json(UploadUrl, "{\"audio_url\":\"" + AudioUrl + "\"}", headers);
        handler.Json(PreRecordedUrl, "{\"result_url\":\"" + ResultUrl + "\"}", headers);
        handler.Json(ResultUrl, ResultJson("Hello from Gladia."), headers);
        return handler;
    }

    private static string ResultJson(string text)
    {
        return "{\"status\":\"done\",\"result\":{\"metadata\":{\"audio_duration\":36.74},\"transcription\":{\"full_transcript\":\"" + text + "\",\"languages\":[\"en\"],\"utterances\":[{\"text\":\"" + text + "\",\"start\":0.14,\"end\":5.341}]}}}";
    }
}
