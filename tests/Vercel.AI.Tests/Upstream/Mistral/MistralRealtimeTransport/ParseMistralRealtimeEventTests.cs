// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Mistral;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralRealtimeTransport;

/// <summary>Port of <c>mistral-realtime-transport.test.ts</c> &gt; <c>parseMistralRealtimeEvent</c>.</summary>
public sealed class ParseMistralRealtimeEventTests
{
    private const string Id =
        "packages/mistral/src/realtime/mistral-realtime-transport.test.ts::parseMistralRealtimeEvent::";

    [Fact]
    [UpstreamTest(
        Id + "preserves unknown provider events without inventing response commands",
        Coverage = UpstreamCoverage.Covered,
        Commit = "0912e0b1b937418b438344ff2098a57961a83787"
    )]
    public void Preserves_unknown_provider_events_without_inventing_response_commands()
    {
        var raw = JsonDocument
            .Parse("{\"type\":\"tool_call.ended\",\"id\":\"call-1\",\"result\":\"done\"}")
            .RootElement;

        var events = MistralRealtimeTransportFunctions.ParseMistralRealtimeEvent(raw);

        var parsed = Assert.Single(events);
        Assert.Equal("custom", parsed.Type);
        Assert.Equal("tool_call.ended", parsed.RawType);
        Assert.Equal(raw.GetRawText(), parsed.Raw!.Value.GetRawText());
    }
}
