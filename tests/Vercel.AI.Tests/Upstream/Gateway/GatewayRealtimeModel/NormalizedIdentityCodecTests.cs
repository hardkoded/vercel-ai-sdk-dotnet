// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModel.GatewayRealtimeModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModel;

/// <summary>Port of <c>gateway-realtime-model.test.ts</c> &gt; <c>GatewayRealtimeModel</c> &gt; <c>normalized identity codec</c>.</summary>
public sealed class NormalizedIdentityCodecTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-realtime-model.test.ts::GatewayRealtimeModel > normalized identity codec::";

    [Fact]
    [UpstreamTest(
        Prefix + "passes server events through unchanged",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Passes_server_events_through_unchanged()
    {
        var @event = new RealtimeModelV4ServerEvent("response-created")
        {
            ResponseId = "resp_1",
            Raw = JsonDocument.Parse("{}").RootElement.Clone(),
        };
        Assert.Same(@event, CreateTestModel().ParseServerEvent(@event));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes client events through unchanged",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Passes_client_events_through_unchanged()
    {
        var @event = new RealtimeModelV4ClientEvent("response-create");
        Assert.Same(@event, CreateTestModel().SerializeClientEvent(@event));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes session config through unchanged",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Passes_session_config_through_unchanged()
    {
        var config = new RealtimeModelV4SessionConfig
        {
            Instructions = "be concise",
            Voice = "alloy",
        };
        Assert.Same(config, CreateTestModel().BuildSessionConfig(config));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes session-update provider options through unchanged",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Passes_session_update_provider_options_through_unchanged()
    {
        // "Support all the things": gateway provider options (tags/user/byok/...) ride session.update exactly as they ride
        // the request body on the non-realtime routes. The identity codec must not strip them.
        var @event = new RealtimeModelV4ClientEvent("session-update")
        {
            Config = new RealtimeModelV4SessionConfig
            {
                Instructions = "be concise",
                ProviderOptions = new JsonObject
                {
                    ["gateway"] = new JsonObject
                    {
                        ["tags"] = new JsonArray("demo"),
                        ["user"] = "user-1",
                    },
                },
            },
        };
        Assert.Same(@event, CreateTestModel().SerializeClientEvent(@event));
    }
}
