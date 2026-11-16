// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayTranscriptionModel.GatewayTranscriptionModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayTranscriptionModel;

/// <summary>Port of <c>gateway-transcription-model.test.ts</c> &gt; <c>GatewayTranscriptionModel &gt; doGenerate</c>.</summary>
public sealed class GatewayTranscriptionModelDoGenerateTests
{
    private const string Prefix = "packages/gateway/src/gateway-transcription-model.test.ts::GatewayTranscriptionModel > doGenerate::";
    private const string Commit = "41465fd1b634dfe8311885230589e19c9b4f431e";

    [Fact]
    [UpstreamTest(Prefix + "should pass headers correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers_correctly()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(CreateTestModel(handler), headers: new Dictionary<string, string> { ["Custom-Header"] = "test-value" });

        Assert.Equal("Bearer test-token", handler.Headers["authorization"]);
        Assert.Equal("test-value", handler.Headers["custom-header"]);
        Assert.Equal("4", handler.Headers["ai-transcription-model-specification-version"]);
        Assert.Equal("openai/gpt-4o-transcribe", handler.Headers["ai-model-id"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include o11y headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_o11y_headers()
    {
        var handler = PrepareJsonResponse();
        var o11yHeaders = new Dictionary<string, string>
        {
            ["ai-o11y-deployment-id"] = "deployment-1",
            ["ai-o11y-environment"] = "production",
            ["ai-o11y-region"] = "iad1",
        };

        await DoGenerate(CreateTestModel(handler, o11yHeaders: o11yHeaders));

        foreach (var pair in o11yHeaders)
        {
            Assert.Equal(pair.Value, handler.Headers[pair.Key]);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "should base64 encode byte audio in request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_base64_encode_byte_audio_in_request_body()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(CreateTestModel(handler), providerOptions: """{"openai":{"language":"en"}}""");

        Assert.True(Equal("""{"audio":"AQID","mediaType":"audio/wav","providerOptions":{"openai":{"language":"en"}}}""", Body(handler)));
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass string audio through in request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_string_audio_through_in_request_body()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(CreateTestModel(handler), audioBase64: "base64-audio", mediaType: "audio/mpeg");

        Assert.True(Equal("""{"audio":"base64-audio","mediaType":"audio/mpeg"}""", Body(handler)));
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn about shared timestamp granularities without changing the request body", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_warn_about_shared_timestamp_granularities_without_changing_the_request_body()
    {
        var handler = new UpstreamRecordingHandler("""
            {"text":"Hello world","words":[{"text":"Hello","startSecond":0,"endSecond":0.5}],"warnings":[{"type":"other","message":"test warning"}]}
            """);
        const string providerOptions = """{"openai":{"timestampGranularities":["segment"]},"gateway":{"order":["openai"]}}""";

        var result = await DoGenerate(CreateTestModel(handler), providerOptions: providerOptions, timestampGranularities: new[] { "word", "segment" });

        Assert.True(Equal("{\"audio\":\"AQID\",\"mediaType\":\"audio/wav\",\"providerOptions\":" + providerOptions + "}", Body(handler)));
        Assert.Equal(2, result.Warnings.Count);
        Assert.Equal("other", result.Warnings[0].Type);
        Assert.Equal("test warning", result.Warnings[0].Message);
        Assert.Equal("unsupported", result.Warnings[1].Type);
        Assert.Equal("timestampGranularities", result.Warnings[1].Feature);
        Assert.Equal("The AI Gateway adapter does not forward the shared timestampGranularities option.", result.Warnings[1].Details);
    }

    [Theory]
    [InlineData("omitted")]
    [InlineData("empty")]
    [UpstreamTest(Prefix + "should not warn about $name shared timestamp granularities", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_not_warn_about_shared_timestamp_granularities(string name)
    {
        var handler = PrepareJsonResponse();

        var result = await DoGenerate(CreateTestModel(handler), timestampGranularities: name == "empty" ? Array.Empty<string>() : null);

        Assert.Empty(result.Warnings);
        Assert.True(Equal("""{"audio":"AQID","mediaType":"audio/wav"}""", Body(handler)));
    }

    [Theory]
    [InlineData("speech")]
    [InlineData("openai")]
    [UpstreamTest(Prefix + "passes the Azure %s API override unchanged", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_the_Azure_API_override_unchanged(string api)
    {
        var handler = PrepareJsonResponse();
        var providerOptions = "{\"azure\":{\"api\":\"" + api + "\",\"timestamps\":\"word\",\"diarization\":{\"enabled\":true}}}";

        await DoGenerate(CreateTestModel(handler), providerOptions: providerOptions);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(providerOptions), Body(handler)!["providerOptions"]));
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract transcript fields and metadata from response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_transcript_fields_and_metadata_from_response()
    {
        var handler = new UpstreamRecordingHandler("""
            {"text":"Hello world","segments":[{"text":"Hello","startSecond":0,"endSecond":0.5},{"text":"world","startSecond":0.5,"endSecond":1}],"language":"en","durationInSeconds":1,"warnings":[{"type":"other","message":"test warning"}],"usage":{"inputTokens":11},"providerMetadata":{"gateway":{"cost":"0.002"}}}
            """);
        handler.ResponseHeaders["x-request-id"] = "req-123";

        var result = await DoGenerate(CreateTestModel(handler));

        Assert.Equal("Hello world", result.Text);
        Assert.Equal(
            new[] { ("Hello", 0.0, 0.5), ("world", 0.5, 1.0) },
            result.Segments.Select(segment => (segment.Text, segment.StartSecond, segment.EndSecond)));
        Assert.Equal("en", result.Language);
        Assert.Equal(1, result.DurationInSeconds);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("test warning", warning.Message);
        Assert.True(Equal("""{"inputTokens":11}""", JsonNode.Parse(result.Usage!.Value.GetRawText())));
        Assert.True(Equal("""{"gateway":{"cost":"0.002"}}""", JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())));
        Assert.Equal("req-123", result.Response.Headers!["x-request-id"]);
        Assert.Equal("openai/gpt-4o-transcribe", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(Prefix + "should default optional transcript fields", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_default_optional_transcript_fields()
    {
        var result = await DoGenerate(CreateTestModel(PrepareJsonResponse()));

        Assert.Empty(result.Segments);
        Assert.Null(result.Language);
        Assert.Null(result.DurationInSeconds);
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("missing", null)]
    [InlineData("null", "null")]
    [InlineData("empty", "[]")]
    [InlineData("populated", """[{"text":"Hello","startSecond":0,"endSecond":0.5},{"text":"world","startSecond":0.5,"endSecond":1}]""")]
    [UpstreamTest(Prefix + "should handle $name word timings", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_handle_word_timings(string name, string? words)
    {
        Assert.NotEmpty(name);
        const string segments = """[{"text":"Hello world","startSecond":0,"endSecond":1}]""";
        var body = "{\"text\":\"Hello world\",\"segments\":" + segments + (words is null ? string.Empty : ",\"words\":" + words) + "}";
        var handler = new UpstreamRecordingHandler(body);

        var result = await DoGenerate(CreateTestModel(handler));

        if (words is null or "null")
        {
            Assert.Null(result.Words);
        }
        else
        {
            Assert.True(Equal(words, new JsonArray(result.Words!.Select(word => (JsonNode)new JsonObject
            {
                ["text"] = word.Text,
                ["startSecond"] = word.StartSecond,
                ["endSecond"] = word.EndSecond,
            }).ToArray())));
        }

        Assert.Equal("Hello world", Assert.Single(result.Segments).Text);
        Assert.True(Equal(body, JsonNode.Parse(result.Response.Body!.Value.GetRawText())));
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("xai")]
    [InlineData("spacexai")]
    [UpstreamTest(Prefix + "should warn when xAI diarization from the %s option namespace is unavailable", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_warn_when_xAI_diarization_from_the_option_namespace_is_unavailable(string provider)
    {
        var handler = PrepareJsonResponse();

        var result = await DoGenerate(CreateTestModel(handler, "spacexai/grok-stt"), providerOptions: "{\"" + provider + "\":{\"diarize\":true}}");

        var warning = Assert.Single(result.Warnings, warning => warning.Feature == "providerOptions.xai.diarize");
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("AI Gateway does not currently expose xAI speaker diarization metadata.", warning.Details);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not warn when xAI diarization metadata is available", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_warn_when_xAI_diarization_metadata_is_available()
    {
        var handler = new UpstreamRecordingHandler("""
            {"text":"Hello world","providerMetadata":{"xai":{"words":[{"text":"Hello","start":0,"end":0.5,"speaker":0}]}}}
            """);

        var result = await DoGenerate(CreateTestModel(handler, "spacexai/grok-stt"), providerOptions: """{"xai":{"diarize":true}}""");

        Assert.Empty(result.Warnings);
        Assert.Equal(0, result.ProviderMetadata!.Value.GetProperty("xai").GetProperty("words")[0].GetProperty("speaker").GetInt32());
    }
}
