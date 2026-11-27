// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cartesia.CartesiaRealtimeModel.CartesiaRealtimeModelTestSupport;

namespace Vercel.AI.Tests.Upstream.Cartesia.CartesiaRealtimeModel;

/// <summary>Port of <c>cartesia-realtime-model.test.ts</c> &gt; <c>CartesiaRealtimeModel &gt; parseServerEvent</c>.</summary>
public sealed class ParseServerEventTests
{
    private const string Id =
        "packages/cartesia/src/cartesia-realtime-model.test.ts::CartesiaRealtimeModel > parseServerEvent::";

    [Fact]
    [UpstreamTest(
        Id + "maps Ink 2 turn events to normalized realtime events",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_Ink_2_turn_events_to_normalized_realtime_events()
    {
        var model = CreateModel();
        var connected = Json("{\"type\":\"connected\",\"request_id\":\"request-1\"}");
        var turnStart = Json("{\"type\":\"turn.start\",\"request_id\":\"request-1\"}");
        var turnEnd = Json(
            "{\"type\":\"turn.end\",\"request_id\":\"request-1\",\"transcript\":\"Hello world\"}"
        );

        var created = Assert.Single(model.ParseServerEvent(connected));
        Assert.Equal("session-created", created.Type);
        Assert.Equal("request-1", created.SessionId);
        AssertRaw(connected, created);

        var started = Assert.Single(model.ParseServerEvent(turnStart));
        Assert.Equal("speech-started", started.Type);
        Assert.Equal("request-1", started.ItemId);
        AssertRaw(turnStart, started);

        var ended = model.ParseServerEvent(turnEnd);
        Assert.Equal(2, ended.Count);
        Assert.Equal("speech-stopped", ended[0].Type);
        Assert.Equal("request-1", ended[0].ItemId);
        AssertRaw(turnEnd, ended[0]);
        Assert.Equal("input-transcription-completed", ended[1].Type);
        Assert.Equal("request-1", ended[1].ItemId);
        Assert.Equal("Hello world", ended[1].Transcript);
        AssertRaw(turnEnd, ended[1]);
    }

    [Fact]
    [UpstreamTest(
        Id + "assembles manual transcript chunks when a flush completes",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Assembles_manual_transcript_chunks_when_a_flush_completes()
    {
        var model = CreateModel();
        var first = Json(
            "{\"type\":\"transcript\",\"request_id\":\"request-1\",\"text\":\"Hello \",\"is_final\":true,\"duration\":0.5}"
        );
        var second = Json(
            "{\"type\":\"transcript\",\"request_id\":\"request-1\",\"text\":\"world\",\"is_final\":true,\"duration\":0.4}"
        );
        var flush = Json("{\"type\":\"flush_done\",\"request_id\":\"request-1\"}");

        var firstEvents = model.ParseServerEvent(first);
        Assert.Equal(2, firstEvents.Count);
        Assert.Equal("session-created", firstEvents[0].Type);
        Assert.Equal("request-1", firstEvents[0].SessionId);
        AssertRaw(first, firstEvents[0]);
        Assert.Equal("custom", firstEvents[1].Type);
        Assert.Equal("transcript", firstEvents[1].RawType);
        AssertRaw(first, firstEvents[1]);

        model.ParseServerEvent(second);
        var flushed = model.ParseServerEvent(flush);

        Assert.Equal(2, flushed.Count);
        Assert.Equal("audio-committed", flushed[0].Type);
        Assert.Equal("request-1", flushed[0].ItemId);
        AssertRaw(flush, flushed[0]);
        Assert.Equal("input-transcription-completed", flushed[1].Type);
        Assert.Equal("request-1", flushed[1].ItemId);
        Assert.Equal("Hello world", flushed[1].Transcript);
        var raw = flushed[1].Raw!.Value;
        AssertRaw(flush, raw.GetProperty("event"));
        var transcriptEvents = raw.GetProperty("transcriptEvents");
        Assert.Equal(2, transcriptEvents.GetArrayLength());
        AssertRaw(first, transcriptEvents[0]);
        AssertRaw(second, transcriptEvents[1]);
        Assert.Equal(0.9, raw.GetProperty("duration").GetDouble());
    }

    [Fact]
    [UpstreamTest(
        Id + "emits one custom event when a manual stream closes without text",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Emits_one_custom_event_when_a_manual_stream_closes_without_text()
    {
        var model = CreateModel();
        var done = Json("{\"type\":\"done\",\"request_id\":\"request-1\"}");

        var events = model.ParseServerEvent(done);

        Assert.Equal(2, events.Count);
        Assert.Equal("session-created", events[0].Type);
        Assert.Equal("request-1", events[0].SessionId);
        AssertRaw(done, events[0]);
        Assert.Equal("custom", events[1].Type);
        Assert.Equal("done", events[1].RawType);
        AssertRaw(done, events[1]);
    }

    [Fact]
    [UpstreamTest(Id + "maps structured Cartesia errors", Coverage = UpstreamCoverage.Covered)]
    public void Maps_structured_Cartesia_errors()
    {
        var model = CreateModel();
        var error = Json(
            "{\"type\":\"error\",\"message\":\"Invalid model\",\"error_code\":\"model_not_found\",\"request_id\":\"request-1\"}"
        );

        var events = model.ParseServerEvent(error);

        Assert.Equal(2, events.Count);
        Assert.Equal("session-created", events[0].Type);
        Assert.Equal("request-1", events[0].SessionId);
        AssertRaw(error, events[0]);
        Assert.Equal("error", events[1].Type);
        Assert.Equal("Invalid model", events[1].Message);
        Assert.Equal("model_not_found", events[1].Code);
        AssertRaw(error, events[1]);
    }

    private static void AssertRaw(JsonElement expected, RealtimeModelV4ServerEvent actual)
    {
        AssertRaw(expected, actual.Raw!.Value);
    }

    private static void AssertRaw(JsonElement expected, JsonElement actual)
    {
        Assert.Equal(expected.GetRawText(), actual.GetRawText());
    }
}
