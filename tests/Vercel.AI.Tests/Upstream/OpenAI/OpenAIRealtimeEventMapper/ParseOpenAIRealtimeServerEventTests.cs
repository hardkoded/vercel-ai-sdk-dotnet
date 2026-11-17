// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Mapper = Vercel.AI.OpenAI.OpenAIRealtimeEventMapper;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIRealtimeEventMapper;

/// <summary>Port of <c>openai-realtime-event-mapper.test.ts</c> &gt; <c>parseOpenAIRealtimeServerEvent</c>.</summary>
public sealed class ParseOpenAIRealtimeServerEventTests
{
    private const string Prefix = RealtimeMapperSupport.File + "parseOpenAIRealtimeServerEvent::";

    [Theory]
    [InlineData("\"client-event\"")]
    [InlineData("\"\"")]
    [InlineData("null")]
    [InlineData((string?)null)]
    [UpstreamTest(
        Prefix + "normalizes nested error.event_id %j as clientEventId",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Normalizes_nested_error_event_id_as_clientEventId(string? eventIdJson)
    {
        // A null argument is the upstream `undefined`: the property is omitted.
        var eventId = eventIdJson != null ? "\"event_id\":" + eventIdJson + "," : string.Empty;
        var raw = RealtimeMapperSupport.Json(
            "{\"type\":\"error\",\"event_id\":\"server-event\",\"error\":{\"message\":\"Invalid audio\",\"code\":\"invalid_audio\","
                + eventId
                + "\"client_event_id\":\"live-client-event\"}}"
        );

        var evt = Mapper.ParseOpenAIRealtimeServerEvent(raw);

        Assert.Equal("error", evt.Type);
        Assert.Equal("Invalid audio", evt.Message);
        Assert.Equal("invalid_audio", evt.Code);
        Assert.Equal(
            eventIdJson == "\"client-event\"" ? "client-event"
                : eventIdJson == "\"\"" ? string.Empty
                : null,
            evt.ClientEventId
        );
        Assert.True(JsonElement.DeepEquals(raw, evt.Raw!.Value));
    }

    [Theory]
    [InlineData((string?)null)]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"message\":null,\"code\":null,\"event_id\":null}")]
    [UpstreamTest(
        Prefix + "handles missing or null optional error fields (%j)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Handles_missing_or_null_optional_error_fields(string? errorJson)
    {
        // A null argument is the upstream `undefined`: the property is omitted.
        var error = errorJson != null ? ",\"error\":" + errorJson : string.Empty;
        var raw = RealtimeMapperSupport.Json(
            "{\"type\":\"error\",\"event_id\":\"server-event\"" + error + "}"
        );

        var evt = Mapper.ParseOpenAIRealtimeServerEvent(raw);

        Assert.Equal("error", evt.Type);
        Assert.Equal("Unknown error", evt.Message);
        Assert.Null(evt.Code);
        Assert.Null(evt.ClientEventId);
        Assert.True(JsonElement.DeepEquals(raw, evt.Raw!.Value));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves top-level message and code fallbacks",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Preserves_top_level_message_and_code_fallbacks()
    {
        var raw = RealtimeMapperSupport.Json(
            "{\"type\":\"error\",\"message\":\"Invalid request\",\"code\":\"invalid_request\",\"error\":{\"message\":null,\"code\":null}}"
        );

        var evt = Mapper.ParseOpenAIRealtimeServerEvent(raw);

        Assert.Equal("error", evt.Type);
        Assert.Equal("Invalid request", evt.Message);
        Assert.Equal("invalid_request", evt.Code);
        Assert.Null(evt.ClientEventId);
        Assert.True(JsonElement.DeepEquals(raw, evt.Raw!.Value));
    }
}
