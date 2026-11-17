// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Mapper = Vercel.AI.OpenAI.OpenAIRealtimeEventMapper;
using SessionOptions = Vercel.AI.OpenAI.OpenAIRealtimeSessionOptions;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIRealtimeEventMapper;

/// <summary>
/// Port of <c>openai-realtime-event-mapper.test.ts</c> &gt; <c>serializeOpenAIRealtimeClientEvent</c>. Upstream has two
/// describes with this name; this class holds the <c>it</c> of both.
/// </summary>
public sealed class SerializeOpenAIRealtimeClientEventTests
{
    private const string Prefix =
        RealtimeMapperSupport.File + "serializeOpenAIRealtimeClientEvent::";

    [Theory]
    [InlineData("client-event")]
    [InlineData("")]
    [InlineData((string?)null)]
    [UpstreamTest(
        Prefix + "preserves session-update eventId %j on the wire",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Preserves_session_update_eventId_on_the_wire(string? eventId)
    {
        var result = Mapper.SerializeOpenAIRealtimeClientEvent(
            new RealtimeModelV4ClientEvent("session-update")
            {
                Config = new RealtimeModelV4SessionConfig { Instructions = "Be concise." },
                EventId = eventId,
            },
            "gpt-realtime"
        );

        var expected =
            "{\"type\":\"session.update\",\"session\":{\"type\":\"realtime\",\"model\":\"gpt-realtime\",\"instructions\":\"Be concise.\"}"
            + (eventId != null ? ",\"event_id\":\"" + eventId + "\"" : string.Empty)
            + "}";
        OpenAIUpstream.Equal(result, expected);
    }

    [Theory]
    [InlineData("client-event")]
    [InlineData("")]
    [InlineData((string?)null)]
    [UpstreamTest(
        Prefix + "preserves input-audio-append eventId %j on the wire",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Preserves_input_audio_append_eventId_on_the_wire(string? eventId)
    {
        var result = Mapper.SerializeOpenAIRealtimeClientEvent(
            new RealtimeModelV4ClientEvent("input-audio-append")
            {
                Audio = "AA==",
                EventId = eventId,
            },
            "gpt-realtime"
        );

        var expected =
            "{\"type\":\"input_audio_buffer.append\",\"audio\":\"AA==\""
            + (eventId != null ? ",\"event_id\":\"" + eventId + "\"" : string.Empty)
            + "}";
        OpenAIUpstream.Equal(result, expected);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes the session intent through to session-update serialization",
        Coverage = UpstreamCoverage.Covered,
        Commit = RealtimeMapperSupport.Commit
    )]
    public void Passes_the_session_intent_through_to_session_update_serialization()
    {
        var result = Mapper.SerializeOpenAIRealtimeClientEvent(
            new RealtimeModelV4ClientEvent("session-update")
            {
                Config = new RealtimeModelV4SessionConfig(),
            },
            "gpt-realtime-whisper",
            new SessionOptions { Intent = RealtimeModelV4SessionIntent.Transcription }
        );

        OpenAIUpstream.Equal(
            result,
            "{\"type\":\"session.update\",\"session\":{\"type\":\"transcription\",\"audio\":{\"input\":{\"transcription\":{\"model\":\"gpt-realtime-whisper\"}}}}}"
        );
    }
}
