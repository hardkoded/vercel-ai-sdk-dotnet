// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModelLive.GatewayRealtimeModelLiveSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModelLive;

/// <summary>Port of <c>gateway-realtime-model-live.test.ts</c> &gt; <c>GatewayRealtimeModelLive</c> &gt; <c>serializeClientEvent</c>.</summary>
public sealed class SerializeClientEventTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-realtime-model-live.test.ts::GatewayRealtimeModelLive > serializeClientEvent::";

    [Fact]
    [UpstreamTest(
        Prefix + "adds the model id to session-start",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Adds_the_model_id_to_session_start()
    {
        var result = CreateTestModel()
            .SerializeClientEvent(
                new RealtimeModelV4ClientEvent("session-start")
                {
                    EventId = "start-1",
                    Config = new RealtimeModelV4SessionConfig
                    {
                        Instructions = "Be brief.",
                        Voice = "marin",
                        InputAudioFormatType = "audio/pcm",
                        InputAudioFormatRate = 24000,
                        OutputAudioFormatType = "audio/pcm",
                        OutputAudioFormatRate = 24000,
                        ProviderOptions = JsonNode
                            .Parse(
                                "{\"gateway\":{\"tags\":[\"demo\"]},\"openai\":{\"delegation\":{\"type\":\"client\"}}}"
                            )!
                            .AsObject(),
                    },
                }
            );

        AssertJson(
            """
            {
              "type": "session-start",
              "modelId": "openai/gpt-live-1.1",
              "eventId": "start-1",
              "config": {
                "instructions": "Be brief.",
                "voice": "marin",
                "inputAudioFormat": { "type": "audio/pcm", "rate": 24000 },
                "outputAudioFormat": { "type": "audio/pcm", "rate": 24000 },
                "providerOptions": {
                  "gateway": { "tags": ["demo"] },
                  "openai": { "delegation": { "type": "client" } }
                }
              }
            }
            """,
            result
        );
    }

    public static TheoryData<RealtimeModelV4ClientEvent, string> MapsCases =>
        new()
        {
            { new RealtimeModelV4ClientEvent("session-close"), "{\"type\":\"session-close\"}" },
            {
                new RealtimeModelV4ClientEvent("session-close") { EventId = "close-1" },
                "{\"type\":\"session-close\",\"eventId\":\"close-1\"}"
            },
            {
                new RealtimeModelV4ClientEvent("input-audio-mute"),
                "{\"type\":\"input-audio-mute\"}"
            },
            {
                new RealtimeModelV4ClientEvent("input-audio-unmute"),
                "{\"type\":\"input-audio-unmute\"}"
            },
            {
                new RealtimeModelV4ClientEvent("input-audio-append") { Audio = "AAAA" },
                "{\"type\":\"input-audio-append\",\"audio\":\"AAAA\"}"
            },
            {
                new RealtimeModelV4ClientEvent("context-append")
                {
                    Content = "Note",
                    DelegationId = null,
                },
                "{\"type\":\"context-append\",\"content\":\"Note\",\"delegationId\":null}"
            },
            {
                new RealtimeModelV4ClientEvent("context-append")
                {
                    Content = "Checking",
                    DelegationId = "d1",
                    EventId = "ctx-1",
                    ProviderOptions = JsonNode
                        .Parse("{\"openai\":{\"channel\":\"thinking\"}}")!
                        .AsObject(),
                },
                "{\"type\":\"context-append\",\"content\":\"Checking\",\"delegationId\":\"d1\",\"eventId\":\"ctx-1\",\"providerOptions\":{\"openai\":{\"channel\":\"thinking\"}}}"
            },
        };

    [Theory]
    [MemberData(nameof(MapsCases))]
    [UpstreamTest(
        Prefix + "maps %o to the normalized Live event",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Maps_event_to_the_normalized_Live_event(
        RealtimeModelV4ClientEvent @event,
        string expected
    )
    {
        AssertJson(expected, CreateTestModel().SerializeClientEvent(@event));
    }

    [Theory]
    [InlineData("{\"openai\":{\"channel\":\"shouting\"}}")]
    [InlineData("{\"openai\":{\"channel\":null}}")]
    [InlineData("{\"openai\":{\"priority\":\"high\"}}")]
    [InlineData("{\"google\":{\"channel\":\"thinking\"}}")]
    [UpstreamTest(
        Prefix + "rejects context-append provider options %o",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Rejects_context_append_provider_options(string providerOptions)
    {
        Assert.Throws<UnsupportedFunctionalityException>(() =>
            CreateTestModel()
                .SerializeClientEvent(
                    new RealtimeModelV4ClientEvent("context-append")
                    {
                        Content = "Note",
                        DelegationId = null,
                        ProviderOptions = JsonNode.Parse(providerOptions)!.AsObject(),
                    }
                )
        );
    }

    [Theory]
    [InlineData("session-update")]
    [InlineData("input-audio-commit")]
    [InlineData("input-audio-clear")]
    [InlineData("response-create")]
    [InlineData("response-cancel")]
    [UpstreamTest(
        Prefix + "rejects $type, which Live does not accept",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Rejects_type_which_Live_does_not_accept(string type)
    {
        var @event = new RealtimeModelV4ClientEvent(type);
        if (type == "session-update")
        {
            @event.Config = new RealtimeModelV4SessionConfig();
        }

        Assert.Throws<UnsupportedFunctionalityException>(() =>
            CreateTestModel().SerializeClientEvent(@event)
        );
    }
}
