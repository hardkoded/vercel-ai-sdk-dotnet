// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Channel = Vercel.AI.RealtimeEventChannel;
using Vercel.AI.Tests.Upstream.RealtimeFixtures;

namespace Vercel.AI.Tests.Upstream.RealtimeEventChannel;

/// <summary>Port of <c>realtime-event-channel.test.ts</c> &gt; <c>RealtimeEventChannel</c>.</summary>
public sealed class RealtimeEventChannelTests
{
    private const string Prefix =
        "packages/ai/src/realtime/realtime-event-channel.test.ts::RealtimeEventChannel::";

    private const string Usage1 = "{\"type\":\"session-usage\",\"usage\":{\"seconds\":1},\"raw\":{}}";
    private const string Usage2 = "{\"type\":\"session-usage\",\"usage\":{\"seconds\":2},\"raw\":{}}";
    private const string Usage3 = "{\"type\":\"session-usage\",\"usage\":{\"seconds\":3},\"raw\":{}}";

    private sealed class BlockedBlob(Task<string> text) : RealtimeBlob
    {
        public override Task<string> TextAsync() => text;
    }

    private static RealtimeEventChannelOptions Options(
        FakeRealtimeModel model,
        Action<JsonNode?>? send = null,
        Func<RealtimeModelV4ServerEvent, Task>? onEvent = null,
        Action<Exception>? onError = null,
        Action<Exception>? onFatalError = null,
        int? maxPending = null,
        int? maxPendingBytes = null
    ) =>
        new(model, send ?? (_ => { }), onEvent ?? (_ => Task.CompletedTask), onError ?? (_ => { }))
        {
            OnFatalError = onFatalError,
            MaxPending = maxPending,
            MaxPendingBytes = maxPendingBytes,
        };

    private static Func<JsonElement, IReadOnlyList<RealtimeModelV4ServerEvent>> CountingParser()
    {
        var count = 0;
        return raw =>
            [
                new RealtimeModelV4ServerEvent("session-usage")
                {
                    Usage = new RealtimeModelV4Usage { Seconds = ++count },
                    Raw = raw,
                },
            ];
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "fails closed on the byte budget and preserves accepted frames without accepting later input",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Fails_closed_on_the_byte_budget_and_preserves_accepted_frames_without_accepting_later_input()
    {
        var blocked = FakeRealtime.Deferred<string>();
        var first = new BlockedBlob(blocked.Task);
        var events = new List<RealtimeModelV4ServerEvent>();
        var fatalErrors = new List<Exception>();
        var codec = new Channel(
            Options(
                new FakeRealtimeModel(),
                onEvent: e =>
                {
                    events.Add(e);
                    return Task.CompletedTask;
                },
                onFatalError: fatalErrors.Add,
                maxPendingBytes: 128
            )
        );
        codec.Receive(first);
        codec.Receive(new string('x', 65));
        codec.Receive("{}");
        Assert.Single(fatalErrors);
        var error = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = codec.Send(new RealtimeModelV4ClientEvent("input-audio-mute"));
            }
        );
        Assert.Contains("closed", error.Message);
        blocked.SetResult(Usage1);
        await codec.FinishAsync();
        Assert.Single(events);
        Assert.Equal(1, events[0].Usage!.Seconds);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "creates isolated model parsers for every connection",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Creates_isolated_model_parsers_for_every_connection()
    {
        var created = 0;
        var model = new FakeRealtimeModel
        {
            CreateServerEventParserHandler = () =>
            {
                created++;
                return CountingParser();
            },
            ParseServerEventHandler = _ =>
                throw new InvalidOperationException("Shared parser must not be used"),
        };
        var firstSeconds = new List<double?>();
        var secondSeconds = new List<double?>();
        var first = new Channel(
            Options(
                model,
                onEvent: e =>
                {
                    firstSeconds.Add(e.Usage!.Seconds);
                    return Task.CompletedTask;
                }
            )
        );
        var second = new Channel(
            Options(
                model,
                onEvent: e =>
                {
                    secondSeconds.Add(e.Usage!.Seconds);
                    return Task.CompletedTask;
                }
            )
        );
        first.Receive("{}");
        first.Receive("{}");
        second.Receive("{}");
        await FakeRealtime.FlushEvents();
        Assert.Equal(2, created);
        Assert.Equal([1d, 2d], firstSeconds);
        Assert.Equal(1d, secondSeconds[0]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "serializes outgoing asynchronous codecs without reordering commands",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Serializes_outgoing_asynchronous_codecs_without_reordering_commands()
    {
        var first = FakeRealtime.Deferred<JsonNode?>();
        var serialized = 0;
        var model = new FakeRealtimeModel
        {
            SerializeClientEventHandler = _ =>
                Interlocked.Increment(ref serialized) == 1
                    ? first.Task
                    : Task.FromResult<JsonNode?>(new JsonObject { ["second"] = true }),
        };
        var sent = new List<string>();
        var codec = new Channel(Options(model, send: data => sent.Add(data!.ToJsonString())));
        _ = codec.Send(new RealtimeModelV4ClientEvent("input-audio-mute"));
        _ = codec.Send(new RealtimeModelV4ClientEvent("input-audio-unmute"));
        await FakeRealtime.FlushEvents();
        Assert.Equal(1, serialized);
        Assert.Empty(sent);
        first.SetResult(new JsonObject { ["first"] = true });
        await FakeRealtime.FlushEvents();
        Assert.Equal(["{\"first\":true}", "{\"second\":true}"], sent);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "discards outgoing work from disposed connections",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Discards_outgoing_work_from_disposed_connections()
    {
        var first = FakeRealtime.Deferred<JsonNode?>();
        var serialized = 0;
        var model = new FakeRealtimeModel
        {
            SerializeClientEventHandler = _ =>
            {
                Interlocked.Increment(ref serialized);
                return first.Task;
            },
        };
        var sent = 0;
        var codec = new Channel(Options(model, send: _ => sent++));
        var firstSend = codec.Send(new RealtimeModelV4ClientEvent("input-audio-mute"));
        var secondSend = codec.Send(new RealtimeModelV4ClientEvent("input-audio-unmute"));
        await FakeRealtime.FlushEvents();
        codec.Dispose();
        first.SetResult(new JsonObject { ["late"] = true });
        await FakeRealtime.FlushEvents();
        Assert.Equal(0, sent);
        Assert.Equal(1, serialized);
        await Assert.ThrowsAsync<InvalidOperationException>(() => firstSend);
        await Assert.ThrowsAsync<InvalidOperationException>(() => secondSend);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "bounds both queues while work is pending",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Bounds_both_queues_while_work_is_pending()
    {
        var pending = FakeRealtime.Deferred<JsonNode?>();
        var model = new FakeRealtimeModel { SerializeClientEventHandler = _ => pending.Task };
        var errors = new List<Exception>();
        var codec = new Channel(
            Options(
                model,
                maxPending: 1,
                onEvent: async _ => await pending.Task,
                onError: errors.Add
            )
        );
        _ = codec.Send(new RealtimeModelV4ClientEvent("input-audio-mute"));
        var full = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = codec.Send(new RealtimeModelV4ClientEvent("input-audio-unmute"));
            }
        );
        Assert.Contains("outgoing queue is full", full.Message);
        codec.Receive(Usage1);
        codec.Receive(Usage2);
        Assert.Equal(
            "Realtime incoming queue is full; input protocol continuity was lost",
            Assert.Single(errors).Message
        );
        codec.Dispose();
        pending.SetResult(null);
        await FakeRealtime.FlushEvents();
    }

    [Fact]
    [UpstreamTest(
        Prefix + "continues after an event callback fails, even when the error callback also throws",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Continues_after_an_event_callback_fails_even_when_the_error_callback_also_throws()
    {
        var calls = 0;
        var codec = new Channel(
            Options(
                new FakeRealtimeModel(),
                onEvent: _ =>
                    Interlocked.Increment(ref calls) == 1
                        ? throw new InvalidOperationException("application")
                        : Task.CompletedTask,
                onError: _ => throw new InvalidOperationException("application error handler")
            )
        );
        codec.Receive(Usage1);
        codec.Receive(Usage2);
        await FakeRealtime.FlushEvents();
        Assert.Equal(2, calls);
        codec.Receive(Usage3);
        await FakeRealtime.FlushEvents();
        Assert.Equal(3, calls);
    }
}
