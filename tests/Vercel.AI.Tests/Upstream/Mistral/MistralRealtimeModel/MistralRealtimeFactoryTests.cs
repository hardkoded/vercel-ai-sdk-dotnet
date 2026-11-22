// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Mistral;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using static Vercel.AI.Tests.Upstream.Mistral.MistralRealtimeModel.MistralRealtimeModelTestSupport;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralRealtimeModel;

/// <summary>Port of <c>mistral-realtime-model.test.ts</c> &gt; <c>Mistral realtime factory</c>.</summary>
public sealed class MistralRealtimeFactoryTests
{
    private const string Id =
        "packages/mistral/src/realtime/mistral-realtime-model.test.ts::Mistral realtime factory::";

    [Fact]
    [UpstreamTest(
        Id + "creates a browser model without loading an API key",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Creates_a_browser_model_without_loading_an_API_key()
    {
        var provider = MistralProvider.Create(
            new OpenAICompatibleOptions
            {
                ApiKey = null,
                ApiKeyEnvironmentVariable = "MISTRAL_API_KEY_UNSET_FOR_REALTIME_TESTS",
            }
        );

        var model = provider.ExperimentalRealtime.Model("mistral-small-latest");

        Assert.IsType<Vercel.AI.Mistral.MistralRealtimeModel>(model);
        Assert.Equal("mistral-small-latest", model.ModelId);
        Assert.Equal("mistral.realtime", model.Provider);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(["webrtc"], model.Capabilities.Transports);
        Assert.Equal(["provider"], model.Capabilities.Connections);
        Assert.Equal("turn-based", model.Capabilities.Conversation);
    }

    [Fact]
    [UpstreamTest(
        Id + "creates a managed session and returns only its browser credentials",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Creates_a_managed_session_and_returns_only_its_browser_credentials()
    {
        var handler = SessionHandler();

        var session = await CreateProvider(handler)
            .ExperimentalRealtime.CreateSessionAsync("mistral-small-latest");

        Assert.Equal(SessionId, session.Id);
        Assert.Equal("participant-token", session.ParticipantToken);
        Assert.Equal("wss://voice.example.com", session.ServerUrl);
        var call = Assert.Single(handler.Calls);
        Assert.Equal(HttpMethod.Post, call.Method);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"llm\":{\"model\":\"mistral-small-latest\"}}"),
                JsonNode.Parse(call.Text)
            )
        );
        Assert.Equal("Bearer test-api-key", call.Header("authorization"));
        Assert.Contains("application/json", call.Header("content-type"));
        Assert.Contains("ai-sdk/mistral/" + AiSdkVersion.Version, call.Header("user-agent"));
    }

    [Fact]
    [UpstreamTest(
        Id + "maps normalized and provider-specific session settings",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Maps_normalized_and_provider_specific_session_settings()
    {
        var handler = SessionHandler();

        await CreateProvider(handler)
            .ExperimentalRealtime.CreateSessionAsync(
                "mistral-small-latest",
                new RealtimeModelV4SessionConfig
                {
                    Instructions = "Be concise.",
                    Voice = "voice_447",
                    OutputModalities = ["audio", "text"],
                    OutputAudioTranscription = new RealtimeModelV4AudioTranscription(),
                    InputAudioTranscription = new RealtimeModelV4AudioTranscription
                    {
                        Model = "voxtral-mini-transcribe-realtime-3",
                        Language = "en",
                    },
                    Tools =
                    [
                        new RealtimeModelV4ToolDefinition(
                            "weather",
                            null,
                            JsonDocument.Parse("{\"type\":\"object\"}").RootElement
                        ),
                    ],
                    ProviderOptions = (JsonObject)
                        JsonNode.Parse(
                            """
                            {"mistral":{
                              "stt":{"model":"overridden-model","language":"fr","contextBias":["Vercel"],"targetStreamingDelayMs":240,"turnDetection":{"minSilenceMs":500,"endThreshold":0.33}},
                              "tts":{"model":"voxtral-mini-tts-3","voiceId":"overridden-voice","phonemeHints":{"SDK":["s"]}},
                              "turn":{"allowInterruptions":false,"inactivity":{"timeoutMs":10000,"maxReminders":2,"messages":["Hello?"]}},
                              "welcomeMessage":"Hello!","agentId":"saved-agent","agentVersion":2}}
                            """
                        )!,
                }
            );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """
                    {"llm":{"model":"mistral-small-latest"},"instructions":"Be concise.",
                     "stt":{"model":"voxtral-mini-transcribe-realtime-3","language":"en","context_bias":["Vercel"],"target_streaming_delay_ms":240,"turn_detection":{"min_silence_ms":500,"end_threshold":0.33}},
                     "tts":{"model":"voxtral-mini-tts-3","voice_id":"voice_447","phoneme_hints":{"SDK":["s"]}},
                     "tools":[{"type":"function","function":{"name":"weather","parameters":{"type":"object"}}}],
                     "turn":{"allow_interruptions":false,"inactivity":{"timeout_ms":10000,"max_reminders":2,"messages":["Hello?"]}},
                     "welcome_message":"Hello!","agent_id":"saved-agent","agent_version":2}
                    """
                ),
                JsonNode.Parse(handler.Calls[0].Text)
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "uses custom URL, headers, fetch and abort signal for create and close",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Uses_custom_URL_headers_fetch_and_abort_signal_for_create_and_close()
    {
        var handler = SessionHandler();
        using var abort = new CancellationTokenSource();
        var custom = CreateProvider(handler, options =>
        {
            options.BaseUrl = "https://proxy.example.com/v1/";
            options.ApiKey = "custom-key";
            options.Headers["x-custom"] = "custom";
        });

        await custom.ExperimentalRealtime.CreateSessionAsync(
            "custom-model",
            cancellationToken: abort.Token
        );
        await custom.ExperimentalRealtime.CloseSessionAsync("session/1?query", abort.Token);

        Assert.Equal(
            "https://proxy.example.com/v1/audio/sessions",
            handler.Calls[0].Uri.AbsoluteUri
        );
        Assert.Equal(
            "https://proxy.example.com/v1/audio/sessions/session%2F1%3Fquery",
            handler.Calls[1].Uri.OriginalString
        );
        Assert.Equal(HttpMethod.Post, handler.Calls[0].Method);
        Assert.Equal(HttpMethod.Delete, handler.Calls[1].Method);
        foreach (var call in handler.Calls)
        {
            Assert.Equal("Bearer custom-key", call.Header("authorization"));
            Assert.Equal("custom", call.Header("x-custom"));
        }
    }

    [Fact]
    [UpstreamTest(
        Id + "accepts an empty successful close response",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Accepts_an_empty_successful_close_response()
    {
        var handler = SessionHandler();

        await CreateProvider(handler).ExperimentalRealtime.CloseSessionAsync(SessionId);

        Assert.Equal(HttpMethod.Delete, Assert.Single(handler.Calls).Method);
    }

    [Theory]
    [InlineData("create")]
    [InlineData("close")]
    [UpstreamTest(
        Id + "reports %s HTTP errors",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Reports_HTTP_errors(string operation)
    {
        var handler = new ParityHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(
                "{\"object\":\"error\",\"message\":\"Invalid key\",\"type\":\"authentication\",\"param\":null,\"code\":null}",
                Encoding.UTF8,
                "application/json"
            ),
        });
        var factory = CreateProvider(handler).ExperimentalRealtime;

        var error = await Assert.ThrowsAnyAsync<ApiException>(() =>
            operation == "create"
                ? factory.CreateSessionAsync("mistral-small-latest")
                : factory.CloseSessionAsync(SessionId)
        );

        Assert.Equal("Invalid key", error.Message);
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(
        Id + "rejects malformed session credentials",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Rejects_malformed_session_credentials()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json("{\"id\":\"" + SessionId + "\"}"));

        await Assert.ThrowsAsync<InvalidResponseDataException>(() =>
            CreateProvider(handler).ExperimentalRealtime.CreateSessionAsync("mistral-small-latest")
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData(".")]
    [InlineData("..")]
    [UpstreamTest(
        Id + "rejects invalid close session ID %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Rejects_invalid_close_session_ID(string sessionId)
    {
        var handler = SessionHandler();

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            CreateProvider(handler).ExperimentalRealtime.CloseSessionAsync(sessionId)
        );

        Assert.Contains("valid session ID", error.Message);
        Assert.Empty(handler.Calls);
    }
}
