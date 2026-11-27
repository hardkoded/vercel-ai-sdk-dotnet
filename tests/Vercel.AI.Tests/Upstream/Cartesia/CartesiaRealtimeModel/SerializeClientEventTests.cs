// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cartesia.CartesiaRealtimeModel.CartesiaRealtimeModelTestSupport;

namespace Vercel.AI.Tests.Upstream.Cartesia.CartesiaRealtimeModel;

/// <summary>Port of <c>cartesia-realtime-model.test.ts</c> &gt; <c>CartesiaRealtimeModel &gt; serializeClientEvent</c>.</summary>
public sealed class SerializeClientEventTests
{
    private const string Id =
        "packages/cartesia/src/cartesia-realtime-model.test.ts::CartesiaRealtimeModel > serializeClientEvent::";

    [Fact]
    [UpstreamTest(
        Id + "sends audio as binary and lets auto-finalize detect turns",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Sends_audio_as_binary_and_lets_auto_finalize_detect_turns()
    {
        var model = CreateModel();
        model.GetWebSocketConfig("token", "wss://api.cartesia.ai/stt/turns/websocket?model=ink-2");

        var audio = model.SerializeClientEvent(
            new RealtimeModelV4ClientEvent("input-audio-append") { Audio = "AQID" }
        );

        Assert.Equal(new byte[] { 1, 2, 3 }, Assert.IsType<byte[]>(audio));
        Assert.Null(
            model.SerializeClientEvent(new RealtimeModelV4ClientEvent("input-audio-commit"))
        );
        Assert.Null(
            model.SerializeClientEvent(
                new RealtimeModelV4ClientEvent("session-update")
                {
                    Config = new RealtimeModelV4SessionConfig(),
                }
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "sends finalize for manual turn detection",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Sends_finalize_for_manual_turn_detection()
    {
        var model = CreateModel();
        model.GetWebSocketConfig("token", "wss://api.cartesia.ai/stt/websocket?model=ink-2");

        Assert.Equal(
            "finalize",
            model.SerializeClientEvent(new RealtimeModelV4ClientEvent("input-audio-commit"))
        );
    }
}
