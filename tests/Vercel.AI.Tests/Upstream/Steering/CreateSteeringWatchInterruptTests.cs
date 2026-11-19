// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using SteeringFactory = Vercel.AI.GenerateText.Steering;

namespace Vercel.AI.Tests.Upstream.Steering;

/// <summary>Port of <c>steering.test.ts</c> &gt; <c>createSteering</c> &gt; <c>watchInterrupt</c>.</summary>
public sealed class CreateSteeringWatchInterruptTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/steering.test.ts::createSteering > watchInterrupt::";
    private const string Commit = "e4039889ba58bbddf965ce61bf8fe6626e9f8800";

    private static ISteering Create() => SteeringFactory.ExperimentalCreateSteering();

    private static SteeringRun Start(ISteering steering) =>
        SteeringFactory.StartSteeringRun(steering)!;

    [Fact]
    [UpstreamTest(
        Prefix + "should call the watcher when an interrupting message arrives",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_call_the_watcher_when_an_interrupting_message_arrives()
    {
        var steering = Create();
        var run = Start(steering);
        var calls = 0;

        run.WatchInterrupt(() => calls++);
        _ = steering.SteerAsync("stop", new SteerOptions { Interrupt = true });

        Assert.Equal(1, calls);
        Assert.True(run.HasPendingMessages());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not call the watcher for a steer without interrupt or a queued message",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_not_call_the_watcher_for_a_steer_without_interrupt_or_a_queued_message()
    {
        var steering = Create();
        var run = Start(steering);
        var calls = 0;

        run.WatchInterrupt(() => calls++);
        _ = steering.SteerAsync("later");
        _ = steering.QueueAsync("after the task", new QueueOptions { Id = "queued" });

        Assert.Equal(0, calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should call a new watcher right away for an interrupt that arrived while nothing watched",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_call_a_new_watcher_right_away_for_an_interrupt_that_arrived_while_nothing_watched()
    {
        var steering = Create();
        var run = Start(steering);
        var calls = 0;

        _ = steering.SteerAsync("stop", new SteerOptions { Interrupt = true });
        run.WatchInterrupt(() => calls++);

        Assert.Equal(1, calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should forget a waiting interrupt once its message is taken at a step boundary",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_forget_a_waiting_interrupt_once_its_message_is_taken_at_a_step_boundary()
    {
        var steering = Create();
        var run = Start(steering);
        var calls = 0;

        _ = steering.SteerAsync("stop", new SteerOptions { Interrupt = true });
        run.TakeMessages();
        run.WatchInterrupt(() => calls++);

        Assert.Equal(0, calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not call a watcher that stopped watching",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_not_call_a_watcher_that_stopped_watching()
    {
        var steering = Create();
        var run = Start(steering);
        var calls = 0;

        var stopWatching = run.WatchInterrupt(() => calls++);
        stopWatching();
        _ = steering.SteerAsync("stop", new SteerOptions { Interrupt = true });

        Assert.Equal(0, calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call each watcher at most once",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_call_each_watcher_at_most_once()
    {
        var steering = Create();
        var run = Start(steering);
        var calls = 0;

        run.WatchInterrupt(() => calls++);
        _ = steering.SteerAsync("stop", new SteerOptions { Interrupt = true });
        _ = steering.SteerAsync("stop again", new SteerOptions { Interrupt = true });

        Assert.Equal(1, calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not interrupt for a message sent again with the same id",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_not_interrupt_for_a_message_sent_again_with_the_same_id()
    {
        var steering = Create();
        var run = Start(steering);
        _ = steering.SteerAsync("stop", new SteerOptions { Id = "steer-1" });
        var calls = 0;

        run.WatchInterrupt(() => calls++);
        _ = steering.SteerAsync("stop", new SteerOptions { Id = "steer-1", Interrupt = true });

        Assert.Equal(0, calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not interrupt after the run ended",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_interrupt_after_the_run_ended()
    {
        var steering = Create();
        var run = Start(steering);
        var calls = 0;

        run.WatchInterrupt(() => calls++);
        run.End(SteeringNotDeliveredReason.Finished);
        var outcome = await steering.SteerAsync("stop", new SteerOptions { Interrupt = true });

        Assert.Equal(0, calls);
        SteeringAssert.NotDelivered(outcome, SteeringNotDeliveredReason.Finished);
    }
}
