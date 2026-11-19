// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using SteeringFactory = Vercel.AI.GenerateText.Steering;

namespace Vercel.AI.Tests.Upstream.Steering;

/// <summary>Port of <c>steering.test.ts</c> &gt; <c>createSteering</c>.</summary>
public sealed class CreateSteeringTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/steering.test.ts::createSteering::";
    private const string Commit = "b8c01eb49d82fd32efe8a2a1e4cd02fbb2b7ae97";
    private const string AlreadyUsed =
        "Invalid argument for parameter experimental_steering: The steering object was already used by a generation. Create a new one with experimental_createSteering() for each call.";

    private static ISteering Create() => SteeringFactory.ExperimentalCreateSteering();

    private static SteeringRun Start(ISteering steering) =>
        SteeringFactory.StartSteeringRun(steering)!;

    private static string Text(UserModelMessage message) =>
        Assert.IsType<TextContentPart>(Assert.Single(message.Content)).Text;

    [Fact]
    [UpstreamTest(
        Prefix + "should deliver messages sent before the run starts in order",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_deliver_messages_sent_before_the_run_starts_in_order()
    {
        var steering = Create();

        var first = steering.SteerAsync("first", new SteerOptions { Id = "steer-1" });
        var second = steering.SteerAsync(
            new UserModelMessage([new TextContentPart("second")]),
            new SteerOptions { Id = "steer-2" }
        );

        var run = Start(steering);

        Assert.True(run.HasPendingMessages());
        var taken = run.TakeMessages();
        Assert.Equal(2, taken.Count);
        Assert.Equal("first", Text(taken[0]));
        Assert.Equal("second", Text(taken[1]));
        Assert.False(run.HasPendingMessages());

        var delivered = run.ResolveTakenMessages(0, taken);
        Assert.Equal(["steer-1", "steer-2"], delivered.Select(message => message.Id));
        Assert.Same(taken[0], delivered[0].Message);
        Assert.Same(taken[1], delivered[1].Message);

        var outcomes = await Task.WhenAll(first, second);
        var firstOutcome = Assert.IsType<DeliveredSteeringOutcome>(outcomes[0]);
        Assert.Equal("steer-1", firstOutcome.Id);
        Assert.Equal("first", Text(firstOutcome.Message));
        Assert.Equal(0, firstOutcome.StepNumber);
        var secondOutcome = Assert.IsType<DeliveredSteeringOutcome>(outcomes[1]);
        Assert.Equal("steer-2", secondOutcome.Id);
        Assert.Equal("second", Text(secondOutcome.Message));
        Assert.Equal(0, secondOutcome.StepNumber);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not deliver messages that are not sent to the model",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_deliver_messages_that_are_not_sent_to_the_model()
    {
        var steering = Create();
        var kept = steering.SteerAsync("kept", new SteerOptions { Id = "steer-1" });
        var removed = steering.SteerAsync("removed", new SteerOptions { Id = "steer-2" });

        var run = Start(steering);
        var keptMessage = run.TakeMessages()[0];

        var delivered = run.ResolveTakenMessages(1, [keptMessage]);
        var only = Assert.Single(delivered);
        Assert.Equal("steer-1", only.Id);
        Assert.Same(keptMessage, only.Message);
        SteeringAssert.Delivered(await kept, 1);
        var removedOutcome = SteeringAssert.NotDelivered(
            await removed,
            SteeringNotDeliveredReason.RemovedByPrepareStep
        );
        Assert.Equal("steer-2", removedOutcome.Id);
        Assert.Equal("removed", Text(removedOutcome.Message));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should deliver messages that are sent as new objects with the same content",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_deliver_messages_that_are_sent_as_new_objects_with_the_same_content()
    {
        var steering = Create();
        var outcome = steering.SteerAsync("hello");

        var run = Start(steering);
        var message = run.TakeMessages()[0];

        run.ResolveTakenMessages(
            0,
            [
                new UserModelMessage(message.Content)
                {
                    ProviderOptions = new Dictionary<string, JsonElement>
                    {
                        ["anthropic"] = JsonSerializer.SerializeToElement(
                            new { cacheControl = new { type = "ephemeral" } }
                        ),
                    },
                },
            ]
        );

        SteeringAssert.Delivered(await outcome, 0);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not deliver pending messages when the run ends",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_deliver_pending_messages_when_the_run_ends()
    {
        var steering = Create();
        var run = Start(steering);

        var outcome = steering.SteerAsync("too late", new SteerOptions { Id = "steer-1" });
        run.End(SteeringNotDeliveredReason.StopCondition);

        var notDelivered = SteeringAssert.NotDelivered(
            await outcome,
            SteeringNotDeliveredReason.StopCondition
        );
        Assert.Equal("steer-1", notDelivered.Id);
        Assert.Equal("too late", Text(notDelivered.Message));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not deliver taken messages when the run ends before they are sent",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_deliver_taken_messages_when_the_run_ends_before_they_are_sent()
    {
        var steering = Create();
        var outcome = steering.SteerAsync("taken");

        var run = Start(steering);
        run.TakeMessages();
        run.End(SteeringNotDeliveredReason.Error);

        SteeringAssert.NotDelivered(await outcome, SteeringNotDeliveredReason.Error);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not deliver messages sent after the run ended",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_deliver_messages_sent_after_the_run_ended()
    {
        var steering = Create();
        Start(steering).End(SteeringNotDeliveredReason.Finished);

        var notDelivered = SteeringAssert.NotDelivered(
            await steering.SteerAsync("after the run", new SteerOptions { Id = "steer-1" }),
            SteeringNotDeliveredReason.Finished
        );
        Assert.Equal("steer-1", notDelivered.Id);
        Assert.Equal("after the run", Text(notDelivered.Message));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return the first outcome for a repeated id",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_return_the_first_outcome_for_a_repeated_id()
    {
        var steering = Create();

        var first = steering.SteerAsync("first", new SteerOptions { Id = "steer-1" });
        var repeated = steering.SteerAsync("retry", new SteerOptions { Id = "steer-1" });

        Assert.Same(first, repeated);

        var run = Start(steering);
        var taken = run.TakeMessages();
        Assert.Single(taken);

        run.ResolveTakenMessages(1, taken);
        var delivered = SteeringAssert.Delivered(await repeated, 1);
        Assert.Equal("first", Text(delivered.Message));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should generate ids when no id is provided",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_generate_ids_when_no_id_is_provided()
    {
        var steering = Create();

        var outcome = steering.SteerAsync("hello");
        var run = Start(steering);
        run.ResolveTakenMessages(0, run.TakeMessages());

        Assert.Matches(@"^steer-", (await outcome).Id);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should hold queued messages until they are promoted",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_hold_queued_messages_until_they_are_promoted()
    {
        var steering = Create();
        var run = Start(steering);

        _ = steering.QueueAsync("first", new QueueOptions { Id = "queued-1" });
        _ = steering.QueueAsync("second", new QueueOptions { Id = "queued-2" });

        Assert.False(run.HasPendingMessages());
        Assert.Empty(run.TakeMessages());

        Assert.True(run.PromoteQueuedMessage());
        Assert.Equal("first", Text(Assert.Single(run.TakeMessages())));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return false when no message is queued",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_return_false_when_no_message_is_queued()
    {
        var run = Start(Create());

        Assert.False(run.PromoteQueuedMessage());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not deliver queued messages when the run ends",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_deliver_queued_messages_when_the_run_ends()
    {
        var steering = Create();
        var run = Start(steering);

        var outcome = steering.QueueAsync("too late", new QueueOptions { Id = "queued-1" });
        run.End(SteeringNotDeliveredReason.StopCondition);

        var notDelivered = SteeringAssert.NotDelivered(
            await outcome,
            SteeringNotDeliveredReason.StopCondition
        );
        Assert.Equal("queued-1", notDelivered.Id);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should share ids between steered and queued messages",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_share_ids_between_steered_and_queued_messages()
    {
        var steering = Create();

        var steered = steering.SteerAsync("first", new SteerOptions { Id = "message-1" });

        Assert.Same(steered, steering.QueueAsync("retry", new QueueOptions { Id = "message-1" }));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw when the steering object is used by two runs at once",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_throw_when_the_steering_object_is_used_by_two_runs_at_once()
    {
        var steering = Create();
        Start(steering);

        var error = Assert.Throws<InvalidArgumentError>(() =>
            SteeringFactory.StartSteeringRun(steering)
        );
        Assert.Equal(AlreadyUsed, error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw when the steering object is reused after its run ended",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_throw_when_the_steering_object_is_reused_after_its_run_ended()
    {
        var steering = Create();
        Start(steering).End(SteeringNotDeliveredReason.Finished);

        var error = Assert.Throws<InvalidArgumentError>(() =>
            SteeringFactory.StartSteeringRun(steering)
        );
        Assert.Contains(
            "Create a new one with experimental_createSteering() for each call.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw for steering objects that were not created with createSteering",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_throw_for_steering_objects_that_were_not_created_with_createSteering()
    {
        var error = Assert.Throws<InvalidArgumentError>(() =>
            SteeringFactory.StartSteeringRun(new CustomSteering())
        );
        Assert.Equal(
            "Invalid argument for parameter experimental_steering: The steering object must be created with experimental_createSteering().",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return undefined without a steering object",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_return_undefined_without_a_steering_object()
    {
        Assert.Null(SteeringFactory.StartSteeringRun(null));
    }

    private sealed class CustomSteering : ISteering
    {
        public Task<SteeringOutcome> SteerAsync(string message, SteerOptions? options = null) =>
            throw new NotSupportedException();

        public Task<SteeringOutcome> SteerAsync(
            UserModelMessage message,
            SteerOptions? options = null
        ) => throw new NotSupportedException();

        public Task<SteeringOutcome> QueueAsync(string message, QueueOptions? options = null) =>
            throw new NotSupportedException();

        public Task<SteeringOutcome> QueueAsync(
            UserModelMessage message,
            QueueOptions? options = null
        ) => throw new NotSupportedException();
    }
}

internal static class SteeringAssert
{
    public static DeliveredSteeringOutcome Delivered(SteeringOutcome? outcome, int stepNumber)
    {
        var delivered = Assert.IsType<DeliveredSteeringOutcome>(outcome);
        Assert.Equal("delivered", delivered.Type);
        Assert.Equal(stepNumber, delivered.StepNumber);
        return delivered;
    }

    public static NotDeliveredSteeringOutcome NotDelivered(
        SteeringOutcome? outcome,
        SteeringNotDeliveredReason reason
    )
    {
        var notDelivered = Assert.IsType<NotDeliveredSteeringOutcome>(outcome);
        Assert.Equal("not-delivered", notDelivered.Type);
        Assert.Equal(reason, notDelivered.Reason);
        return notDelivered;
    }
}
