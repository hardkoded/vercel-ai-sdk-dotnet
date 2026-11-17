// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Xai.XaiRealtimeModel;

/// <summary>Port of <c>xai-realtime-model.test.ts</c> &gt; <c>XaiRealtimeModel &gt; serializeClientEvent</c>.</summary>
public sealed class SerializeClientEventTests
{
    private const string Id =
        "packages/xai/src/realtime/xai-realtime-model.test.ts::XaiRealtimeModel > serializeClientEvent::";

    [Fact]
    [UpstreamTest(
        Id + "drops conversation-item-truncate (unsupported over WebSocket)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Drops_conversation_item_truncate()
    {
        var result = XaiRealtimeTestSupport
            .CreateModel()
            .SerializeClientEvent(
                new RealtimeModelV4ClientEvent("conversation-item-truncate")
                {
                    ItemId = "item-1",
                    ContentIndex = 0,
                    AudioEndMs = 100,
                }
            );

        Assert.Null(result);
    }
}
