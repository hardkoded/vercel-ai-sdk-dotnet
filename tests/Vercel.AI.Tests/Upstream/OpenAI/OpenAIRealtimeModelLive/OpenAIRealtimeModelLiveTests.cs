// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Live = Vercel.AI.OpenAI.OpenAIRealtimeModelLive;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIRealtimeModelLive;

/// <summary>Port of <c>openai-realtime-model-live.test.ts</c> &gt; <c>OpenAIRealtimeModelLive</c>.</summary>
public sealed class OpenAIRealtimeModelLiveTests
{
    private const string Prefix = "packages/openai/src/live/openai-realtime-model-live.test.ts::OpenAIRealtimeModelLive::";

    private static readonly JsonObject s_options = new OpenAIRealtimeModelLiveOptions { ClientDelegation = true }.ToJsonObject();

    internal static Live Create(Action<OpenAIOptions>? configure = null, OpenAICapture? capture = null)
    {
        var provider = OpenAIUpstream.Provider(capture ?? new OpenAICapture(), options =>
        {
            options.ApiKey = "test-key";
            configure?.Invoke(options);
        });
        return (Live)provider.RealtimeModel("gpt-live-1");
    }

    private static JsonObject OpenAIProviderOptions(JsonNode openai)
    {
        return new JsonObject { ["openai"] = openai };
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects a transcription intent across connection and serialization helpers", Coverage = UpstreamCoverage.Covered, Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782")]
    public async Task Rejects_a_transcription_intent_across_connection_and_serialization_helpers()
    {
        var model = Create();
        var intent = RealtimeModelV4SessionIntent.Transcription;

        var connection = await Assert.ThrowsAsync<InvalidOperationException>(() => model.GetServerWebSocketConfigAsync(intent, CancellationToken.None));
        var serialization = Assert.Throws<InvalidOperationException>(() => model.SerializeClientEvent(new RealtimeModelV4ClientEvent("response-create"), intent));
        var configuration = Assert.Throws<InvalidOperationException>(() => model.BuildSessionConfig(new RealtimeModelV4SessionConfig(), intent));

        Assert.Matches("not supported", connection.Message);
        Assert.Matches("not supported", serialization.Message);
        Assert.Matches("not supported", configuration.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "implements the existing v4 spec and declares continuous transports", Coverage = UpstreamCoverage.Covered)]
    public void Implements_the_existing_v4_spec_and_declares_continuous_transports()
    {
        var model = Create();

        IRealtimeModel realtime = model;
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal("openai.live", realtime.Provider);
        Assert.Equal("gpt-live-1", realtime.ModelId);
        Assert.Equal("continuous", model.Capabilities.Conversation);
        Assert.Equal(new[] { "websocket", "webrtc" }, model.Capabilities.Transports);
        Assert.Equal(new[] { "server-websocket", "webrtc" }, model.Capabilities.Connections);
        Assert.Equal("session-start", model.Startup);
        Assert.Equal("session-close", model.Finalization);
        Assert.Equal("oai-events", model.GetWebRTCConfig().DataChannelLabel);
        Assert.Equal("openai.realtime", OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test-key" }).RealtimeModel("gpt-realtime").Provider);
    }

    [Fact]
    [UpstreamTest(Prefix + "returns authenticated server WS settings without model query parameters", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_authenticated_server_WS_settings_without_model_query_parameters()
    {
        var config = await Create().GetServerWebSocketConfigAsync(null, CancellationToken.None);

        Assert.Equal("wss://api.openai.com/v1/live/sessions", config.Url);
        Assert.Equal("Bearer test-key", config.Headers["authorization"]);
        Assert.All(config.Headers.Values, value => Assert.NotNull(value));
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves a custom base path, provider name, organization, project, and headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_a_custom_base_path_provider_name_organization_project_and_headers()
    {
        var custom = Create(options =>
        {
            options.BaseUrl = "https://example.com/proxy/v1/";
            options.ProviderName = "custom";
            options.Organization = "org-test";
            options.Project = "proj-test";
            options.Headers["X-Custom"] = "value";
        });

        var config = await custom.GetServerWebSocketConfigAsync(null, CancellationToken.None);

        Assert.Equal("custom.live", custom.Provider);
        Assert.Equal("wss://example.com/proxy/v1/live/sessions", config.Url);
        Assert.Equal("Bearer test-key", config.Headers["authorization"]);
        Assert.Equal("org-test", config.Headers["openai-organization"]);
        Assert.Equal("proj-test", config.Headers["openai-project"]);
        Assert.Equal("value", config.Headers["x-custom"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "maps an HTTP development base URL to ws", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_an_HTTP_development_base_URL_to_ws()
    {
        var model = Create(options => options.BaseUrl = "http://localhost:3000/v1");

        Assert.Equal("ws://localhost:3000/v1/live/sessions", (await model.GetServerWebSocketConfigAsync(null, CancellationToken.None)).Url);
    }

    [Fact]
    [UpstreamTest(Prefix + "builds the documented WS startup payload", Coverage = UpstreamCoverage.Covered)]
    public void Builds_the_documented_WS_startup_payload()
    {
        var result = Create().SerializeClientEvent(new RealtimeModelV4ClientEvent("session-start")
        {
            EventId = "start-1",
            Config = new RealtimeModelV4SessionConfig
            {
                Instructions = "Be concise.",
                Voice = "marin",
                ProviderOptions = OpenAIProviderOptions(s_options.DeepClone()),
            },
        });

        OpenAIUpstream.Equal(
            result,
            "{\"type\":\"session.start\",\"event_id\":\"start-1\",\"session\":{\"model\":\"gpt-live-1\",\"instructions\":\"Be concise.\",\"audio\":{\"format\":{\"type\":\"audio/pcm\",\"rate\":24000},\"output\":{\"voice\":\"marin\"}},\"delegation\":{\"type\":\"client\"}}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "supports startup history, client delegation, storage, and custom voices", Coverage = UpstreamCoverage.Covered)]
    public void Supports_startup_history_client_delegation_storage_and_custom_voices()
    {
        const string Input = "[{\"type\":\"message\",\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Hello\"}]},{\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"output_text\",\"text\":\"Hi\"}]}]";
        var model = Create();

        var result = model.BuildSessionConfig(new RealtimeModelV4SessionConfig
        {
            ProviderOptions = OpenAIProviderOptions(JsonNode.Parse(
                "{\"delegation\":{\"type\":\"client\"},\"input\":" + Input + ",\"store\":true,\"voice\":{\"id\":\"voice-test\"}}")!),
        });

        OpenAIUpstream.Equal(
            result,
            "{\"model\":\"gpt-live-1\",\"audio\":{\"format\":{\"type\":\"audio/pcm\",\"rate\":24000},\"output\":{\"voice\":{\"id\":\"voice-test\"}}},\"delegation\":{\"type\":\"client\"},\"input\":" + Input + ",\"store\":true}");

        var disabled = model.BuildSessionConfig(new RealtimeModelV4SessionConfig
        {
            ProviderOptions = OpenAIProviderOptions(JsonNode.Parse("{\"delegation\":null,\"store\":false}")!),
        });
        Assert.True(disabled.ContainsKey("delegation"));
        Assert.Null(disabled["delegation"]);
        Assert.False(disabled["store"]!.GetValue<bool>());
    }

    [Theory]
    [InlineData("audio/pcm", 16000)]
    [InlineData("audio/pcm", 24000)]
    [InlineData("audio/pcmu", 8000)]
    [InlineData("audio/pcma", 8000)]
    [UpstreamTest(Prefix + "supports the shared audio format %j", Coverage = UpstreamCoverage.Covered)]
    public void Supports_the_shared_audio_format(string type, int rate)
    {
        var model = Create();
        var expected = "{\"type\":\"" + type + "\",\"rate\":" + rate + "}";

        var both = model.BuildSessionConfig(new RealtimeModelV4SessionConfig
        {
            InputAudioFormatType = type,
            InputAudioFormatRate = rate,
            OutputAudioFormatType = type,
            OutputAudioFormatRate = rate,
        });
        var outputOnly = model.BuildSessionConfig(new RealtimeModelV4SessionConfig { OutputAudioFormatType = type, OutputAudioFormatRate = rate });

        OpenAIUpstream.Equal(both["audio"]!["format"], expected);
        OpenAIUpstream.Equal(outputOnly["audio"]!["format"], expected);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects different input and output formats and ambiguous voices", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_different_input_and_output_formats_and_ambiguous_voices()
    {
        var model = Create();

        Assert.Throws<InvalidArgumentException>(() => model.BuildSessionConfig(new RealtimeModelV4SessionConfig
        {
            InputAudioFormatType = "audio/pcm",
            InputAudioFormatRate = 24000,
            OutputAudioFormatType = "audio/pcm",
            OutputAudioFormatRate = 16000,
        }));
        Assert.Throws<InvalidArgumentException>(() => model.BuildSessionConfig(new RealtimeModelV4SessionConfig
        {
            Voice = "marin",
            ProviderOptions = OpenAIProviderOptions(JsonNode.Parse("{\"voice\":{\"id\":\"voice-test\"}}")!),
        }));
    }

    [Theory]
    [InlineData("{\"inputAudioFormat\":{\"type\":\"audio/pcm\",\"rate\":48000}}")]
    [InlineData("{\"inputAudioFormat\":{\"type\":\"audio/mp3\"}}")]
    [InlineData("{\"providerOptions\":{\"openai\":{\"input\":[{\"type\":\"message\",\"role\":\"system\",\"content\":[{\"type\":\"input_text\",\"text\":\"Hello\"}]}]}}}")]
    [InlineData("{\"providerOptions\":{\"openai\":{\"store\":\"yes\"}}}")]
    [InlineData("{\"providerOptions\":{\"openai\":{\"unknown\":true}}}")]
    [UpstreamTest(Prefix + "validates startup config %j", Coverage = UpstreamCoverage.Covered)]
    public void Validates_startup_config(string configJson)
    {
        var json = JsonNode.Parse(configJson)!.AsObject();
        var config = new RealtimeModelV4SessionConfig();
        if (json["inputAudioFormat"] is JsonObject format)
        {
            config.InputAudioFormatType = format["type"]?.GetValue<string>();
            config.InputAudioFormatRate = format["rate"]?.GetValue<int>();
        }

        if (json["providerOptions"] is JsonObject providerOptions)
        {
            config.ProviderOptions = (JsonObject)providerOptions.DeepClone();
        }

        Assert.ThrowsAny<Exception>(() => Create().BuildSessionConfig(config));
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects unsupported voice-turn settings", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_unsupported_voice_turn_settings()
    {
        var model = Create();

        Assert.Throws<UnsupportedFunctionalityException>(() => model.BuildSessionConfig(new RealtimeModelV4SessionConfig
        {
            TurnDetection = new RealtimeModelV4TurnDetection { Type = "disabled" },
        }));
        Assert.Throws<UnsupportedFunctionalityException>(() => model.BuildSessionConfig(new RealtimeModelV4SessionConfig { Tools = [] }));
    }

    [Theory]
    [InlineData("session-close", "close-1", null, "{\"type\":\"session.close\",\"event_id\":\"close-1\"}")]
    [InlineData("input-audio-append", null, "AAAA", "{\"type\":\"session.input_audio.append\",\"audio\":\"AAAA\"}")]
    [InlineData("input-audio-mute", "mute-1", null, "{\"type\":\"session.input_audio.mute\",\"event_id\":\"mute-1\"}")]
    [InlineData("input-audio-unmute", null, null, "{\"type\":\"session.input_audio.unmute\"}")]
    [UpstreamTest(Prefix + "serializes %j", Coverage = UpstreamCoverage.Covered)]
    public void Serializes(string type, string? eventId, string? audio, string expected)
    {
        var result = Create().SerializeClientEvent(new RealtimeModelV4ClientEvent(type) { EventId = eventId, Audio = audio });

        OpenAIUpstream.Equal(result, expected);
    }

    [Theory]
    [InlineData("instructions")]
    [InlineData("thinking")]
    [InlineData("commentary")]
    [UpstreamTest(Prefix + "serializes %s context with null or opaque delegation IDs", Coverage = UpstreamCoverage.Covered)]
    public void Serializes_context_with_null_or_opaque_delegation_IDs(string channel)
    {
        foreach (var delegationId in new string?[] { null, "opaque-delegation" })
        {
            var result = Create().SerializeClientEvent(new RealtimeModelV4ClientEvent("context-append")
            {
                ProviderOptions = OpenAIProviderOptions(new JsonObject { ["channel"] = channel }),
                Content = "Context",
                DelegationId = delegationId,
                EventId = "append-1",
            });

            var expectedDelegation = delegationId == null ? "null" : "\"" + delegationId + "\"";
            OpenAIUpstream.Equal(
                result,
                "{\"type\":\"session." + channel + ".append\",\"content\":\"Context\",\"delegation_id\":" + expectedDelegation + ",\"event_id\":\"append-1\"}");
        }
    }

    [Theory]
    [InlineData("input-audio-commit")]
    [InlineData("input-audio-clear")]
    [InlineData("response-create")]
    [InlineData("response-cancel")]
    [InlineData("conversation-item-truncate")]
    [UpstreamTest(Prefix + "rejects legacy command %j", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_legacy_command(string type)
    {
        var clientEvent = new RealtimeModelV4ClientEvent(type);
        if (type == "conversation-item-truncate")
        {
            clientEvent.ItemId = "item-1";
            clientEvent.ContentIndex = 0;
            clientEvent.AudioEndMs = 100;
        }

        Assert.Throws<UnsupportedFunctionalityException>(() => Create().SerializeClientEvent(clientEvent));
    }

    [Fact]
    [UpstreamTest(Prefix + "omits unsupported ephemeral and browser WebSocket capabilities", Coverage = UpstreamCoverage.Covered)]
    public void Omits_unsupported_ephemeral_and_browser_WebSocket_capabilities()
    {
        Assert.Null(typeof(Live).GetMethod("CreateClientSecretAsync"));
        Assert.Null(typeof(Live).GetMethod("GetWebSocketConfig"));
        Assert.Null(typeof(OpenAIProvider).GetMember("Live").FirstOrDefault());
    }
}
