// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleRealtimeModel;

/// <summary>Port of <c>google-realtime-model.test.ts</c> &gt; <c>GoogleRealtimeModel &gt; doCreateClientSecret</c>.</summary>
public sealed class DoCreateClientSecretTests
{
    private const string Id = "packages/google/src/realtime/google-realtime-model.test.ts::GoogleRealtimeModel > doCreateClientSecret::";

    [Fact]
    [UpstreamTest(Id + "calls auth_tokens endpoint with correct payload", Coverage = UpstreamCoverage.Covered)]
    public async Task Calls_auth_tokens_endpoint_with_correct_payload()
    {
        var handler = new RecordingHandler { ResponseText = "{\"name\":\"projects/123/locations/us/accessTokens/abc\",\"expireTime\":\"2026-01-01T00:05:00.000Z\"}" };
        var secret = await GoogleRealtimeTestSupport.CreateModel(handler).CreateClientSecretAsync(new RealtimeModelV4SessionConfig { Instructions = "Be helpful", Voice = "Puck" }, null, CancellationToken.None);
        Assert.Equal("https://generativelanguage.googleapis.com/v1alpha/auth_tokens?key=test-key", Assert.Single(handler.Uris));
        Assert.Equal("POST", handler.Method);
        var body = JsonNode.Parse(handler.Body)!;
        Assert.Equal(0, (int)body["uses"]!);
        Assert.NotNull(body["expireTime"]);
        var setup = body["bidiGenerateContentSetup"]!;
        Assert.Equal("models/gemini-2.0-flash-live-001", (string?)setup["model"]);
        GoogleUpstream.JsonEqual(setup["systemInstruction"], "{\"parts\":[{\"text\":\"Be helpful\"}]}");
        Assert.Equal("Puck", (string?)setup["generationConfig"]!["speechConfig"]!["voiceConfig"]!["prebuiltVoiceConfig"]!["voiceName"]);
        Assert.Equal("projects/123/locations/us/accessTokens/abc", secret.Token);
        Assert.Contains("generativelanguage.googleapis.com", secret.Url, StringComparison.Ordinal);
        Assert.Equal(DateTimeOffset.Parse("2026-01-01T00:05:00Z", CultureInfo.InvariantCulture).ToUnixTimeSeconds(), secret.ExpiresAt);
    }

    [Fact]
    [UpstreamTest(Id + "sets newSessionExpireTime from expiresAfterSeconds", Coverage = UpstreamCoverage.Covered)]
    public async Task Sets_newSessionExpireTime_from_expiresAfterSeconds()
    {
        var handler = new RecordingHandler { ResponseText = "{\"name\":\"token\"}" };
        var now = DateTimeOffset.Parse("2026-01-01T00:00:00Z", CultureInfo.InvariantCulture);
        var provider = GoogleUpstream.Client(handler, new GoogleOptions { ApiKey = "test-key" });
        provider.Clock = () => now;
        await ((global::Vercel.AI.Google.GoogleRealtimeModel)provider.RealtimeModel("gemini-2.0-flash-live-001")).CreateClientSecretAsync(null, 120, CancellationToken.None);
        var body = JsonNode.Parse(handler.Body)!;

        // newSessionExpireTime controls the window to open a session.
        var newSession = DateTimeOffset.Parse((string)body["newSessionExpireTime"]!, CultureInfo.InvariantCulture);
        Assert.Equal(now.AddSeconds(120), newSession);

        // expireTime is the overall token lifetime and must outlast the open
        // window so the opened session has room to run.
        Assert.True(DateTimeOffset.Parse((string)body["expireTime"]!, CultureInfo.InvariantCulture) > newSession);
    }

    [Fact]
    [UpstreamTest(Id + "uses the configured base URL for realtime endpoints", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_configured_base_URL_for_realtime_endpoints()
    {
        var handler = new RecordingHandler { ResponseText = "{\"name\":\"token\"}" };
        var secret = await GoogleRealtimeTestSupport.CreateModel(handler, "https://proxy.example.com/google/v1beta").CreateClientSecretAsync(null, null, CancellationToken.None);
        Assert.Equal("https://proxy.example.com/google/v1alpha/auth_tokens?key=test-key", Assert.Single(handler.Uris));
        Assert.Equal("wss://proxy.example.com/google/ws/google.ai.generativelanguage.v1alpha.GenerativeService.BidiGenerateContentConstrained", secret.Url);
    }

    [Fact]
    [UpstreamTest(Id + "uses ws protocol for http base URLs", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_ws_protocol_for_http_base_URLs()
    {
        var handler = new RecordingHandler { ResponseText = "{\"name\":\"token\"}" };
        var secret = await GoogleRealtimeTestSupport.CreateModel(handler, "http://localhost:8787/v1beta").CreateClientSecretAsync(null, null, CancellationToken.None);
        Assert.Equal("ws://localhost:8787/ws/google.ai.generativelanguage.v1alpha.GenerativeService.BidiGenerateContentConstrained", secret.Url);
    }

    [Fact]
    [UpstreamTest(Id + "enables output audio transcription when configured", Coverage = UpstreamCoverage.Covered)]
    public async Task Enables_output_audio_transcription_when_configured()
    {
        var handler = new RecordingHandler { ResponseText = "{\"name\":\"token\"}" };
        await GoogleRealtimeTestSupport.CreateModel(handler).CreateClientSecretAsync(new RealtimeModelV4SessionConfig { OutputAudioTranscription = new RealtimeModelV4AudioTranscription() }, null, CancellationToken.None);
        GoogleUpstream.JsonEqual(JsonNode.Parse(handler.Body)!["bidiGenerateContentSetup"]!["outputAudioTranscription"], "{}");
    }

    [Fact]
    [UpstreamTest(Id + "embeds Google Live Translation config in auth token setup", Coverage = UpstreamCoverage.Covered)]
    public async Task Embeds_Google_Live_Translation_config_in_auth_token_setup()
    {
        var handler = new RecordingHandler { ResponseText = "{\"name\":\"token\"}" };
        await GoogleRealtimeTestSupport.CreateModel(handler, modelId: "gemini-3.5-live-translate-preview").CreateClientSecretAsync(
            new RealtimeModelV4SessionConfig { ProviderOptions = JsonNode.Parse("{\"google\":{\"translationConfig\":{\"targetLanguageCode\":\"pl\",\"echoTargetLanguage\":true}}}")!.AsObject() },
            null,
            CancellationToken.None);
        var setup = JsonNode.Parse(handler.Body)!["bidiGenerateContentSetup"]!;
        GoogleUpstream.JsonEqual(setup["generationConfig"]!["translationConfig"], "{\"targetLanguageCode\":\"pl\",\"echoTargetLanguage\":true}");
        Assert.False(setup.AsObject().ContainsKey("google"));
    }

    [Fact]
    [UpstreamTest(Id + "throws on missing API key", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_on_missing_API_key()
    {
        var handler = new RecordingHandler();
        var provider = GoogleProvider.Create(new GoogleOptions { ApiKeyEnvironmentVariable = "VERCEL_AI_TEST_UNSET_GOOGLE_KEY" }, handler);
        var model = (global::Vercel.AI.Google.GoogleRealtimeModel)provider.RealtimeModel("gemini-2.0-flash-live-001");
        var error = await Assert.ThrowsAsync<AiSdkException>(() => model.CreateClientSecretAsync(null, null, CancellationToken.None));
        Assert.Contains("API key is required", error.Message, StringComparison.Ordinal);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [UpstreamTest(Id + "throws on failed request", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_on_failed_request()
    {
        var handler = new RecordingHandler { Status = HttpStatusCode.Forbidden, ResponseText = "Forbidden", MediaType = "text/plain" };
        var error = await Assert.ThrowsAsync<PermissionDeniedException>(() => GoogleRealtimeTestSupport.CreateModel(handler).CreateClientSecretAsync(null, null, CancellationToken.None));
        Assert.Equal(403, error.StatusCode);
        Assert.Equal("Forbidden", error.ResponseBody);
    }
}
