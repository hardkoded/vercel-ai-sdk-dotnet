// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModelLive.GatewayRealtimeModelLiveSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModelLive;

/// <summary>Port of <c>gateway-realtime-model-live.test.ts</c> &gt; <c>GatewayRealtimeModelLive</c> &gt; <c>parseServerEvent</c>.</summary>
public sealed class ParseServerEventTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-realtime-model-live.test.ts::GatewayRealtimeModelLive > parseServerEvent::";

    [Fact]
    [UpstreamTest(
        Prefix + "passes normalized events through",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Passes_normalized_events_through()
    {
        var @event = new RealtimeModelV4ServerEvent("session-closed")
        {
            SessionId = "live_1",
            Usage = new RealtimeModelV4Usage { Seconds = 14 },
            Reason = "close_requested",
            Raw = Element("{\"type\":\"session.closed\"}"),
        };

        Assert.Same(@event, CreateTestModel().ParseServerEvent(@event));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps Gateway error frames as raw",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Keeps_Gateway_error_frames_as_raw()
    {
        const string frame =
            "{\"type\":\"error\",\"code\":\"invalid_request_error\",\"message\":\"Live requires a canonical model slug\"}";

        var result = CreateTestModel().ParseServerEvent(Element(frame));

        Assert.Equal("error", result.Type);
        Assert.Equal("invalid_request_error", result.Code);
        Assert.Equal("Live requires a canonical model slug", result.Message);
        Assert.Equal(frame, result.Raw!.Value.GetRawText());
    }

    public static TheoryData<object?> InvalidEvents =>
        new() { null, "text", Element("[]"), Element("{}"), Element("{\"type\":1}") };

    [Theory]
    [MemberData(nameof(InvalidEvents))]
    [UpstreamTest(
        Prefix + "reports %o as an invalid server event",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Reports_value_as_an_invalid_server_event(object? raw)
    {
        var result = CreateTestModel().ParseServerEvent(raw);

        Assert.Equal("error", result.Type);
        Assert.Equal("invalid_server_event", result.Code);
        Assert.Equal("Invalid AI Gateway Live server event.", result.Message);
        Assert.Equal(
            raw is JsonElement element ? element.GetRawText() : JsonSerializer.Serialize(raw),
            result.Raw!.Value.GetRawText()
        );
    }
}
