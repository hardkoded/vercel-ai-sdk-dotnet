// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Live = Vercel.AI.OpenAI.OpenAIRealtimeModelLive;
using SessionConfig = Vercel.AI.OpenAI.OpenAILiveSessionConfig;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAILiveSessionConfig;

/// <summary>Port of <c>openai-live-session-config.test.ts</c> &gt; <c>Live client delegation startup options</c>.</summary>
public sealed class LiveClientDelegationStartupOptionsTests
{
    private const string Prefix = "packages/openai/src/live/openai-live-session-config.test.ts::Live client delegation startup options::";

    private static readonly Live s_model = (Live)OpenAIUpstream.Provider(new OpenAICapture()).RealtimeModel("gpt-live-1");

    private static RealtimeModelV4SessionConfig Config(string openaiJson)
    {
        return new RealtimeModelV4SessionConfig { ProviderOptions = new JsonObject { ["openai"] = JsonNode.Parse(openaiJson) } };
    }

    [Fact]
    [UpstreamTest(Prefix + "converts native RTC permissions without enabling a managed response backend", Coverage = UpstreamCoverage.Covered)]
    public void Converts_native_RTC_permissions_without_enabling_a_managed_response_backend()
    {
        var config = Config("{\"delegation\":{\"type\":\"client\"},\"client\":{\"dataChannel\":{\"allowedClientEvents\":[\"session.close\",\"session.thinking.append\"],\"allowedServerEvents\":[{\"type\":\"session.started\"},{\"type\":\"session.closed\"},{\"type\":\"response.event\",\"responseEvent\":\"response.completed\"}]}}}");

        var session = SessionConfig.BuildOpenAILiveSessionConfig(config, "gpt-live-1", "webrtc");

        OpenAIUpstream.Equal(
            session,
            "{\"model\":\"gpt-live-1\",\"audio\":{\"output\":{\"voice\":\"marin\"}},\"delegation\":{\"type\":\"client\"},\"client\":{\"data_channel\":{\"allowed_client_events\":[\"session.close\",\"session.thinking.append\"],\"allowed_server_events\":[{\"type\":\"session.started\"},{\"type\":\"session.closed\"},{\"type\":\"response.event\",\"response_event\":\"response.completed\"}]}}}");
        Assert.Throws<UnsupportedFunctionalityException>(() => s_model.BuildSessionConfig(config));
    }

    [Theory]
    [InlineData("{}", "{}")]
    [InlineData("{\"allowedClientEvents\":\"all\",\"allowedServerEvents\":\"all\"}", "{\"allowed_client_events\":\"all\",\"allowed_server_events\":\"all\"}")]
    [InlineData("{\"allowedClientEvents\":[],\"allowedServerEvents\":[]}", "{\"allowed_client_events\":[],\"allowed_server_events\":[]}")]
    [UpstreamTest(Prefix + "preserves omitted, all, and empty RTC permissions: %j", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_omitted_all_and_empty_RTC_permissions(string dataChannelJson, string expectedJson)
    {
        var session = SessionConfig.BuildOpenAILiveSessionConfig(
            Config("{\"client\":{\"dataChannel\":" + dataChannelJson + "}}"),
            "gpt-live-1",
            "webrtc");

        OpenAIUpstream.Equal(session["client"]!["data_channel"], expectedJson);
    }

    [Theory]
    [InlineData("{\"allowedServerEvents\":[\"session.started\"]}")]
    [InlineData("{\"allowedServerEvents\":[{\"type\":\"response.event\"}]}")]
    [InlineData("{\"allowedServerEvents\":[{\"type\":\"session.started\",\"responseEvent\":\"response.created\"}]}")]
    [InlineData("{\"allowedServerEvents\":[{\"type\":\"response.event\",\"response_event\":\"response.created\"}]}")]
    [InlineData("{\"allowedClientEvents\":true}")]
    [InlineData("{\"extra\":[]}")]
    [UpstreamTest(Prefix + "rejects malformed RTC selectors %j", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_malformed_RTC_selectors(string dataChannelJson)
    {
        Assert.ThrowsAny<Exception>(() => SessionConfig.BuildOpenAILiveSessionConfig(
            Config("{\"client\":{\"dataChannel\":" + dataChannelJson + "}}"),
            "gpt-live-1",
            "webrtc"));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"delegation\":null}")]
    [InlineData("{\"delegation\":{\"type\":\"client\"}}")]
    [UpstreamTest(Prefix + "accepts client startup options %j", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_client_startup_options(string optionsJson)
    {
        var options = (JsonObject)JsonNode.Parse(optionsJson)!;

        Assert.Equal(optionsJson, OpenAIRealtimeModelLiveOptions.Parse(options).ToJsonObject().ToJsonString());
        var serialized = s_model.SerializeClientEvent(new RealtimeModelV4ClientEvent("session-start") { Config = Config(optionsJson) })!;
        Assert.Equal("session.start", serialized["type"]!.GetValue<string>());
        foreach (var pair in options)
        {
            Assert.Equal(pair.Value?.ToJsonString(), serialized["session"]![pair.Key]?.ToJsonString());
        }
    }

    [Theory]
    [InlineData("{\"type\":\"responses\"}")]
    [InlineData("{\"type\":\"responses\",\"responses\":{\"model\":\"test-model\"}}")]
    [UpstreamTest(Prefix + "rejects Responses delegation before a socket send: %j", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_Responses_delegation_before_a_socket_send(string delegationJson)
    {
        var config = Config("{\"delegation\":" + delegationJson + "}");
        var sends = 0;

        Assert.Throws<UnsupportedFunctionalityException>(() => s_model.BuildSessionConfig(config));
        var exception = Assert.Throws<UnsupportedFunctionalityException>(() =>
        {
            _ = s_model.SerializeClientEvent(new RealtimeModelV4ClientEvent("session-start") { Config = config });
            sends++;
        });
        Assert.Matches("only client delegation is supported", exception.Message);
        Assert.Equal(0, sends);
    }

    [Theory]
    [InlineData("{\"client\":{\"dataChannel\":{}}}")]
    [InlineData("{\"delegation\":{\"type\":\"client\",\"client\":{}}}")]
    [InlineData("{\"delegation\":{\"type\":\"client\",\"responses\":{}}}")]
    [InlineData("{\"tools\":[]}")]
    [InlineData("{\"input\":[{\"type\":\"message\",\"role\":\"user\",\"content\":[{\"type\":\"input_image\",\"image_url\":\"https://example.com/image.png\"}]}]}")]
    [UpstreamTest(Prefix + "rejects unsupported startup fields: %j", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_unsupported_startup_fields(string optionsJson)
    {
        Assert.ThrowsAny<Exception>(() => s_model.BuildSessionConfig(Config(optionsJson)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("{\"instructions\":\"new instructions\"}")]
    [InlineData("{\"openai\":{\"delegation\":{\"type\":\"client\"}}}")]
    [InlineData("{\"openai\":{\"delegation\":null}}")]
    [InlineData("{\"openai\":{\"delegation\":{\"type\":\"responses\"}}}")]
    [UpstreamTest(Prefix + "rejects every Live session update before sending: %j", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_every_Live_session_update_before_sending(string? configJson)
    {
        var config = new RealtimeModelV4SessionConfig();
        if (configJson != null)
        {
            var node = (JsonObject)JsonNode.Parse(configJson)!;
            config.Instructions = node["instructions"]?.GetValue<string>();
            config.ProviderOptions = node["openai"] == null ? null : new JsonObject { ["openai"] = node["openai"]!.DeepClone() };
        }

        var sends = 0;

        var exception = Assert.Throws<UnsupportedFunctionalityException>(() =>
        {
            _ = s_model.SerializeClientEvent(new RealtimeModelV4ClientEvent("session-update") { Config = config });
            sends++;
        });
        Assert.Matches("startup settings are immutable; use context-append", exception.Message);
        Assert.Equal(0, sends);
    }

    [Fact]
    [UpstreamTest(Prefix + "defaults context to thinking and validates OpenAI channel options", Coverage = UpstreamCoverage.Covered)]
    public void Defaults_context_to_thinking_and_validates_OpenAI_channel_options()
    {
        var serialized = s_model.SerializeClientEvent(new RealtimeModelV4ClientEvent("context-append") { Content = string.Empty, DelegationId = null });

        OpenAIUpstream.Equal(serialized, "{\"type\":\"session.thinking.append\",\"content\":\"\",\"delegation_id\":null}");
        Assert.ThrowsAny<Exception>(() => s_model.SerializeClientEvent(new RealtimeModelV4ClientEvent("context-append")
        {
            Content = "context",
            DelegationId = null,
            ProviderOptions = new JsonObject { ["openai"] = new JsonObject { ["channel"] = "invalid" } },
        }));
    }
}
