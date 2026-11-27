// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Cartesia.CartesiaRealtimeModel.CartesiaRealtimeModelTestSupport;

namespace Vercel.AI.Tests.Upstream.Cartesia.CartesiaRealtimeModel;

/// <summary>Port of the <c>it</c> outside any nested <c>describe</c> of <c>cartesia-realtime-model.test.ts</c>.</summary>
public sealed class CartesiaRealtimeModelTests
{
    [Fact]
    [UpstreamTest(
        "packages/cartesia/src/cartesia-realtime-model.test.ts::CartesiaRealtimeModel::adds the access token to the WebSocket URL",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Adds_the_access_token_to_the_WebSocket_URL()
    {
        var model = CreateModel();

        var config = model.GetWebSocketConfig(
            "access-token",
            "wss://api.cartesia.ai/stt/turns/websocket?model=ink-2"
        );

        Assert.Equal(
            "wss://api.cartesia.ai/stt/turns/websocket?model=ink-2&access_token=access-token",
            config.Url
        );
    }
}
