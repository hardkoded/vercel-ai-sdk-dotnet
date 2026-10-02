// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.RevAI;
using Vercel.AI.Tests.Parity.Media;

namespace Vercel.AI.Tests;

public sealed class RevAITranscriptionTests
{
    private const string Transcript = "{\"monologues\":[{\"speaker\":0,\"elements\":[{\"type\":\"text\",\"value\":\"Hello\",\"ts\":0.075,\"end_ts\":0.425,\"confidence\":0.96},{\"type\":\"punct\",\"value\":\" \"},{\"type\":\"text\",\"value\":\"from\",\"ts\":0.425,\"end_ts\":0.665,\"confidence\":0.98},{\"type\":\"punct\",\"value\":\" \"},{\"type\":\"text\",\"value\":\"the\",\"ts\":0.665,\"end_ts\":0.785,\"confidence\":0.98},{\"type\":\"punct\",\"value\":\" \"},{\"type\":\"text\",\"value\":\"Sal\",\"ts\":0.945,\"end_ts\":1.105,\"confidence\":0.64},{\"type\":\"punct\",\"value\":\",\"},{\"type\":\"punct\",\"value\":\" \"},{\"type\":\"text\",\"value\":\"A-I-S-D-K\",\"ts\":1.185,\"end_ts\":2.145,\"confidence\":0.96},{\"type\":\"punct\",\"value\":\".\"}]}]}";

    private const string ErrorBody = "\n{\"error\":{\"message\":\"{\\n  \\\"error\\\": {\\n    \\\"code\\\": 429,\\n    \\\"message\\\": \\\"Resource has been exhausted (e.g. check quota).\\\",\\n    \\\"status\\\": \\\"RESOURCE_EXHAUSTED\\\"\\n  }\\n}\\n\",\"code\":429}}\n";

    [Fact]
    [UpstreamTest(
        "packages/revai/src/revai-error.test.ts::revaiErrorDataSchema::should parse Rev.ai resource exhausted error",
        Coverage = UpstreamCoverage.Covered)]
    public void Parses_a_revai_resource_exhausted_error()
    {
        Assert.True(RevAIError.TryParse(ErrorBody, out var error));
        Assert.NotNull(error);
        Assert.Equal(429, error!.Code);
        Assert.Contains("Resource has been exhausted (e.g. check quota).", error.Message, StringComparison.Ordinal);
        Assert.Contains("RESOURCE_EXHAUSTED", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/revai/src/revai-transcription-model.test.ts::doGenerate > transcription::should pass the model",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_the_transcriber_in_the_multipart_config()
    {
        var handler = Jobs();
        var model = Model(handler, "test-api-key");
        await model.TranscribeAsync(new RevAITranscriptionRequest(new byte[] { 1, 2, 3 }, "audio/wav"), CancellationToken.None);

        Assert.Equal("POST", handler.Calls[0].Method);
        Assert.Equal("https://api.rev.ai/speechtotext/v1/jobs", handler.Calls[0].Url);
        Assert.StartsWith("multipart/form-data", handler.Calls[0].Header("content-type"), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("name=\"media\"", handler.Calls[0].Body, StringComparison.Ordinal);
        Assert.Contains("filename=\"audio.wav\"", handler.Calls[0].Body, StringComparison.Ordinal);
        Assert.Contains("name=\"config\"", handler.Calls[0].Body, StringComparison.Ordinal);
        Assert.Contains("{\"transcriber\":\"machine\"}", handler.Calls[0].Body, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/revai/src/revai-transcription-model.test.ts::doGenerate > transcription::should pass headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_provider_and_request_headers()
    {
        var handler = Jobs();
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var model = Model(handler, options);
        await model.TranscribeAsync(
            new RevAITranscriptionRequest(new byte[] { 1, 2, 3 }, "audio/wav")
            {
                Headers = new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" },
            },
            CancellationToken.None);

        Assert.Equal("Bearer test-api-key", handler.Calls[0].Header("authorization"));
        Assert.Equal("provider-header-value", handler.Calls[0].Header("custom-provider-header"));
        Assert.Equal("request-header-value", handler.Calls[0].Header("custom-request-header"));
        Assert.Contains("ai-sdk/revai/0.0.0-test", handler.Calls[0].Header("user-agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/revai/src/revai-transcription-model.test.ts::doGenerate > transcription::should extract the transcription text",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_the_transcription_text()
    {
        var model = Model(Jobs(), "test-api-key");
        var result = await model.TranscribeAsync(new RevAITranscriptionRequest(new byte[] { 1, 2, 3 }, "audio/wav"), CancellationToken.None);

        Assert.Equal("Hello from the Sal, A-I-S-D-K.", result.Text);
        Assert.Equal(2.145d, result.DurationSeconds);
        Assert.Equal("Hello", result.Segments[0].Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/revai/src/revai-transcription-model.test.ts::doGenerate > response headers::should include response data with timestamp, modelId and headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_timestamp_model_id_and_response_headers()
    {
        var handler = Jobs(new Dictionary<string, string>
        {
            ["x-request-id"] = "test-request-id",
            ["x-ratelimit-remaining"] = "123",
        });
        var model = Model(handler, "test-api-key");
        model.Clock = () => DateTimeOffset.UnixEpoch;
        var result = await model.TranscribeAsync(new RevAITranscriptionRequest(new byte[] { 1, 2, 3 }, "audio/wav"), CancellationToken.None);

        Assert.Equal(DateTimeOffset.UnixEpoch, result.Timestamp);
        Assert.Equal("machine", result.ModelId);
        Assert.Equal("application/json", result.ResponseHeaders["content-type"]);
        Assert.Equal("596", result.ResponseHeaders["content-length"]);
        Assert.Equal("test-request-id", result.ResponseHeaders["x-request-id"]);
        Assert.Equal("123", result.ResponseHeaders["x-ratelimit-remaining"]);
        Assert.Equal("Hello", MediaJson.Parse(result.ResponseBody)["monologues"]![0]!["elements"]![0]!["value"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/revai/src/revai-transcription-model.test.ts::doGenerate > response metadata::should use real date when no custom date provider is specified",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_injected_clock_and_model_id()
    {
        var model = Model(Jobs(), "test-api-key");
        model.Clock = () => DateTimeOffset.UnixEpoch;
        var result = await model.TranscribeAsync(new RevAITranscriptionRequest(new byte[] { 1, 2, 3 }, "audio/wav"), CancellationToken.None);

        Assert.Equal(DateTimeOffset.UnixEpoch, result.Timestamp);
        Assert.Equal("machine", result.ModelId);
    }

    private static RevAITranscriptionModel Model(MediaHandler handler, string apiKey)
    {
        return Model(handler, new OpenAICompatibleOptions { ApiKey = apiKey });
    }

    private static RevAITranscriptionModel Model(MediaHandler handler, OpenAICompatibleOptions options)
    {
        var provider = RevAIProvider.Create(options, handler);
        var model = (RevAITranscriptionModel)provider.TranscriptionModel("machine");
        model.PollInterval = TimeSpan.Zero;
        return model;
    }

    private static MediaHandler Jobs(IDictionary<string, string>? headers = null)
    {
        var handler = new MediaHandler();
        handler.Json("https://api.rev.ai/speechtotext/v1/jobs", "{\"id\":\"test-id\",\"status\":\"in_progress\",\"language\":\"en\"}", headers);
        handler.Json("https://api.rev.ai/speechtotext/v1/jobs/test-id", "{\"id\":\"test-id\",\"status\":\"transcribed\",\"language\":\"en\"}", headers);
        handler.Json("https://api.rev.ai/speechtotext/v1/jobs/test-id/transcript", Transcript, headers);
        return handler;
    }
}
