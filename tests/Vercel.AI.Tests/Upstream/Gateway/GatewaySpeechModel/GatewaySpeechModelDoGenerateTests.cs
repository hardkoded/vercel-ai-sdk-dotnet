// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.Gateway.GatewaySpeechModel.GatewaySpeechModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewaySpeechModel;

/// <summary>Port of <c>gateway-speech-model.test.ts</c> &gt; <c>GatewaySpeechModel &gt; doGenerate</c>.</summary>
public sealed class GatewaySpeechModelDoGenerateTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-speech-model.test.ts::GatewaySpeechModel > doGenerate::";

    [Fact]
    [UpstreamTest(Prefix + "should pass headers correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers_correctly()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(
            CreateTestModel(handler),
            headers: new Dictionary<string, string> { ["Custom-Header"] = "test-value" }
        );

        Assert.Equal("Bearer test-token", handler.Headers["authorization"]);
        Assert.Equal("test-value", handler.Headers["custom-header"]);
        Assert.Equal("4", handler.Headers["ai-speech-model-specification-version"]);
        Assert.Equal("openai/tts-1", handler.Headers["ai-model-id"]);
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

        await DoGenerate(CreateTestModel(handler, o11yHeaders));

        foreach (var pair in o11yHeaders)
        {
            Assert.Equal(pair.Value, handler.Headers[pair.Key]);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send speech options in request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_speech_options_in_request_body()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(
            CreateTestModel(handler),
            voice: "alloy",
            outputFormat: "mp3",
            instructions: "Speak clearly",
            speed: 1.25,
            language: "en",
            providerOptions: "{\"openai\":{\"style\":\"friendly\"}}"
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"text\":\"Hello world\",\"voice\":\"alloy\",\"outputFormat\":\"mp3\",\"instructions\":\"Speak clearly\",\"speed\":1.25,\"language\":\"en\",\"providerOptions\":{\"openai\":{\"style\":\"friendly\"}}}"
                ),
                JsonNode.Parse(handler.Body)
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit optional speech options when not provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_optional_speech_options_when_not_provided()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(CreateTestModel(handler));

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"text\":\"Hello world\"}"),
                JsonNode.Parse(handler.Body)
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract audio and metadata from response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_audio_and_metadata_from_response()
    {
        var handler = new UpstreamRecordingHandler(
            "{\"audio\":\"base64-audio\",\"warnings\":[{\"type\":\"other\",\"message\":\"test warning\"}],\"usage\":{\"characters\":11},\"providerMetadata\":{\"gateway\":{\"cost\":\"0.002\"}}}"
        );
        handler.ResponseHeaders["x-request-id"] = "req-123";

        var result = await DoGenerate(CreateTestModel(handler));

        Assert.Equal(ByteEncoding.FromBase64("base64-audio"), result.Audio);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("test warning", warning.Message);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"characters\":11}"),
                JsonNode.Parse(result.Usage!.Value.GetRawText())
            )
        );
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"gateway\":{\"cost\":\"0.002\"}}"),
                JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())
            )
        );
        Assert.Equal("req-123", result.Response.Headers!["x-request-id"]);
        Assert.Equal("openai/tts-1", result.Response.ModelId);
    }
}
