// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModelLive.GatewayRealtimeModelLiveSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModelLive;

/// <summary>Port of <c>gateway-realtime-model-live.test.ts</c> &gt; <c>GatewayRealtimeModelLive</c>.</summary>
public sealed class GatewayRealtimeModelLiveTests
{
    [Fact]
    [UpstreamTest(
        "packages/gateway/src/gateway-realtime-model-live.test.ts::GatewayRealtimeModelLive::declares continuous Live capabilities",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Declares_continuous_Live_capabilities()
    {
        var model = CreateTestModel();

        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal("gateway.live", model.Provider);
        Assert.Equal("openai/gpt-live-1.1", model.ModelId);
        Assert.Equal("continuous", model.Capabilities.Conversation);
        Assert.Equal(["websocket"], model.Capabilities.Transports);
        Assert.Equal(
            ["client-secret-websocket", "server-websocket"],
            model.Capabilities.Connections
        );
        Assert.Equal("session-start", model.Capabilities.Startup);
        Assert.Equal("session-close", model.Capabilities.Finalization);
    }

    [Fact]
    [UpstreamTest(
        "packages/gateway/src/gateway-realtime-model-live.test.ts::GatewayRealtimeModelLive::carries the token and team scope in WebSocket subprotocols",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Carries_the_token_and_team_scope_in_WebSocket_subprotocols()
    {
        var config = CreateTestModel(teamIdOrSlug: "my-team").GetWebSocketConfig("vcst_x", LiveUrl);

        Assert.Equal(LiveUrl, config.Url);
        Assert.Equal(
            [
                Vercel.AI.Gateway.GatewayRealtimeAuth.GatewayRealtimeSubprotocol,
                Vercel.AI.Gateway.GatewayRealtimeAuth.GatewayAuthSubprotocolPrefix + "vcst_x",
            ],
            config.Protocols!.Take(2)
        );
        Assert.Equal(
            "my-team",
            Vercel.AI.Gateway.GatewayRealtimeAuth.GetGatewayRealtimeTeamIdOrSlug(
                string.Join(", ", config.Protocols!)
            )
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/gateway/src/gateway-realtime-model-live.test.ts::GatewayRealtimeModelLive::returns the live-model url and defined headers for server connections",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Returns_the_live_model_url_and_defined_headers_for_server_connections()
    {
        var descriptor = await CreateTestModel()
            .GetServerWebSocketConfigAsync(null, CancellationToken.None);

        Assert.Equal(LiveUrl, descriptor.Url);
        Assert.Equal(
            new Dictionary<string, string> { ["Authorization"] = "Bearer vck_test-token" },
            descriptor.Headers
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/gateway/src/gateway-realtime-model-live.test.ts::GatewayRealtimeModelLive::rejects non-conversation intents",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Rejects_non_conversation_intents()
    {
        var model = CreateTestModel();
        var intent = RealtimeModelV4SessionIntent.Transcription;

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            model.GetServerWebSocketConfigAsync(intent, CancellationToken.None)
        );
        Assert.Contains("does not support the 'transcription' session intent", thrown.Message);
        Assert.Contains(
            "does not support",
            Assert
                .Throws<InvalidOperationException>(() =>
                    model.SerializeClientEvent(
                        new RealtimeModelV4ClientEvent("session-close"),
                        intent
                    )
                )
                .Message
        );
        Assert.Contains(
            "does not support",
            Assert
                .Throws<InvalidOperationException>(() =>
                    model.BuildSessionConfig(new RealtimeModelV4SessionConfig(), intent)
                )
                .Message
        );
    }
}
