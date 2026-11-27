// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.RealtimeFixtures;
using Tracker = Vercel.AI.RealtimeCommandTracker;

namespace Vercel.AI.Tests.Upstream.RealtimeCommandTracker;

/// <summary>Port of <c>realtime-command-tracker.test.ts</c> &gt; <c>normalized realtime command completion</c>.</summary>
public sealed class NormalizedRealtimeCommandCompletionTests
{
    private const string Prefix =
        "packages/ai/src/realtime/realtime-command-tracker.test.ts::normalized realtime command completion::";

    public static TheoryData<string> NoAckCommands =>
        ["input-audio-append", "session-start", "session-close"];

    public static TheoryData<string> AcknowledgedCommands =>
        ["session-update", "context-append", "input-audio-mute", "input-audio-unmute"];

    private static RealtimeModelV4ClientEvent Command(string type, string? eventId = null) =>
        new(type)
        {
            EventId = eventId,
            Audio = type == "input-audio-append" ? "AAAA" : null,
            Config = type is "session-start" or "session-update"
                ? new RealtimeModelV4SessionConfig()
                : null,
            Content = type == "context-append" ? "context" : null,
        };

    private static RealtimeModelV4ServerEvent Server(
        string type,
        string clientEventId,
        string? command = null
    ) =>
        new(type)
        {
            ClientEventId = clientEventId,
            Command = command,
            Message = type == "error" ? "rejected" : null,
        };

    private static Tracker Coordinator(Func<RealtimeModelV4ClientEvent, Task>? send = null) =>
        new(send ?? (_ => Task.CompletedTask));

    private static void AssertThrowsContaining(Action action, string text)
    {
        var error = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains(text, error.Message);
    }

    [Theory]
    [MemberData(nameof(NoAckCommands))]
    [UpstreamTest(
        Prefix + "turns over 4100 identified $type sends without an ACK and protects recent IDs",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Turns_over_4100_identified_type_sends_without_an_ACK_and_protects_recent_IDs(
        string type
    )
    {
        var sent = 0;
        var commands = Coordinator(_ =>
        {
            Interlocked.Increment(ref sent);
            return Task.CompletedTask;
        });
        for (var index = 0; index < 4100; index++)
        {
            await commands.Send(Command(type, $"id-{index}"));
        }

        Assert.Equal(4100, sent);
        AssertThrowsContaining(() => commands.Send(Command(type, "id-4099")), "fresh eventId");
        commands.Receive(Server("error", "id-4099"));
        AssertThrowsContaining(() => commands.Send(Command(type, "id-4099")), "fresh eventId");
        await commands.Send(Command(type, "corrected"));
        await commands.Send(Command(type, "id-0"));
        await commands.Send(
            new RealtimeModelV4ClientEvent("input-audio-mute") { EventId = "mute" }
        );
        await commands.Send(Command("session-update", "update"));
    }

    [Theory]
    [MemberData(nameof(AcknowledgedCommands))]
    [UpstreamTest(
        Prefix + "keeps successful $type pending until a real ACK or error",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Keeps_successful_type_pending_until_a_real_ACK_or_error(string type)
    {
        var commands = Coordinator();
        for (var index = 0; index < 512; index++)
        {
            await commands.Send(Command(type, $"id-{index}"));
        }

        AssertThrowsContaining(() => commands.Send(Command(type, "overflow")), "pending");
        commands.Receive(Server("command-acknowledged", "id-0", type));
        await commands.Send(Command(type, "after-ack"));
        AssertThrowsContaining(() => commands.Send(Command(type, "overflow")), "pending");
        commands.Receive(Server("error", "id-1"));
        await commands.Send(Command(type, "after-error"));
        AssertThrowsContaining(() => commands.Send(Command(type, "id-1")), "fresh eventId");
    }

    [Theory]
    [MemberData(nameof(NoAckCommands))]
    [UpstreamTest(
        Prefix
            + "retains failed $type IDs for correlation while admitting fresh retries without a lifetime quota",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Retains_failed_type_IDs_for_correlation_while_admitting_fresh_retries_without_a_lifetime_quota(
        string type
    )
    {
        var failNext = false;
        var commands = Coordinator(_ =>
        {
            if (!failNext)
            {
                return Task.CompletedTask;
            }

            failNext = false;
            return Task.FromException(new InvalidOperationException("send failed"));
        });
        for (var index = 0; index < 600; index++)
        {
            failNext = true;
            var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                commands.Send(Command(type, $"failed-{index}"))
            );
            Assert.Equal("send failed", error.Message);
            await commands.Send(Command(type, $"retry-{index}"));
        }

        commands.Receive(Server("error", "failed-599"));
        AssertThrowsContaining(() => commands.Send(Command(type, "failed-599")), "fresh eventId");
        AssertThrowsContaining(() => commands.Send(Command(type, "retry-599")), "fresh eventId");
        await commands.Send(Command(type, "fresh"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not evict unresolved no-ACK sends to admit more work",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Does_not_evict_unresolved_no_ACK_sends_to_admit_more_work()
    {
        var pending = FakeRealtime.Deferred<bool>();
        var commands = Coordinator(_ => pending.Task);
        var sends = Enumerable
            .Range(0, 512)
            .Select(index => commands.Send(Command("input-audio-append", $"pending-{index}")))
            .ToList();
        AssertThrowsContaining(
            () => commands.Send(Command("input-audio-append", "overflow")),
            "pending"
        );
        pending.SetResult(true);
        await Task.WhenAll(sends);
        await commands.Send(Command("input-audio-append", "admitted"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "releases a synchronously failed no-ACK send while retaining its ID",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Releases_a_synchronously_failed_no_ACK_send_while_retaining_its_ID()
    {
        var fail = true;
        var commands = Coordinator(_ =>
            fail
                ? throw new InvalidOperationException("synchronous send failure")
                : Task.CompletedTask
        );
        var error = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = commands.Send(Command("session-close", "failed"));
        });
        Assert.Contains("synchronous send failure", error.Message);
        fail = false;
        AssertThrowsContaining(
            () => commands.Send(Command("session-close", "failed")),
            "fresh eventId"
        );
        await commands.Send(Command("session-close", "retry"));
    }
}
