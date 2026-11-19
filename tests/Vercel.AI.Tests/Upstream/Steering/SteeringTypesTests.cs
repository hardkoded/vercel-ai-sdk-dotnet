// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using SteeringFactory = Vercel.AI.GenerateText.Steering;

namespace Vercel.AI.Tests.Upstream.Steering;

/// <summary>
/// Port of <c>steering.test-d.ts</c> &gt; <c>steering types</c>. The type tests become compile-time checks:
/// an assignment that does not compile stands for a <c>@ts-expect-error</c>.
/// </summary>
public sealed class SteeringTypesTests
{
    private const string Commit = "b8c01eb49d82fd32efe8a2a1e4cd02fbb2b7ae97";
    private const string Prefix =
        "packages/ai/src/generate-text/steering.test-d.ts::steering types::";

    [Fact]
    [UpstreamTest(
        Prefix + "should accept text and user messages",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_accept_text_and_user_messages()
    {
        var steering = SteeringFactory.ExperimentalCreateSteering();

        Task<SteeringOutcome> outcome = steering.SteerAsync("hello");
        Assert.NotNull(outcome);
        steering.SteerAsync(new UserModelMessage("hello"), new SteerOptions { Id = "steer-1" });
        steering.SteerAsync("stop", new SteerOptions { Interrupt = true });

        // interrupt is a boolean, and queued messages wait for the task and cannot interrupt:
        Assert.Equal(typeof(bool), typeof(SteerOptions).GetProperty("Interrupt")!.PropertyType);
        Assert.Null(typeof(QueueOptions).GetProperty("Interrupt"));

        // only user messages can be sent: there is no overload for another message type.
        Assert.All(
            typeof(ISteering).GetMethods().Where(m => m.Name == nameof(ISteering.SteerAsync)),
            m =>
                Assert.Contains(
                    m.GetParameters()[0].ParameterType,
                    new[] { typeof(string), typeof(UserModelMessage) }
                )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept text and user messages in the queue",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_accept_text_and_user_messages_in_the_queue()
    {
        var steering = SteeringFactory.ExperimentalCreateSteering();

        Task<SteeringOutcome> outcome = steering.QueueAsync("hello");
        Assert.NotNull(outcome);
        steering.QueueAsync(new UserModelMessage("hello"), new QueueOptions { Id = "queued-1" });

        // only user messages can be queued: there is no overload for another message type.
        Assert.All(
            typeof(ISteering).GetMethods().Where(m => m.Name == nameof(ISteering.QueueAsync)),
            m =>
                Assert.Contains(
                    m.GetParameters()[0].ParameterType,
                    new[] { typeof(string), typeof(UserModelMessage) }
                )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should narrow the outcome by type",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_narrow_the_outcome_by_type()
    {
        // a type test never runs the steering object, so the outcomes are built directly.
        var message = new UserModelMessage("hello");
        SteeringOutcome[] outcomes =
        [
            new DeliveredSteeringOutcome("a", message, 0),
            new NotDeliveredSteeringOutcome("b", message, SteeringNotDeliveredReason.Finished),
        ];

        foreach (var outcome in outcomes)
        {
            if (outcome is DeliveredSteeringOutcome delivered)
            {
                int stepNumber = delivered.StepNumber;
                Assert.Equal(0, stepNumber);
            }
            else
            {
                SteeringNotDeliveredReason reason = ((NotDeliveredSteeringOutcome)outcome).Reason;
                Assert.Equal(SteeringNotDeliveredReason.Finished, reason);
            }

            UserModelMessage outcomeMessage = outcome.Message;
            Assert.Same(message, outcomeMessage);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should expose steering messages on step results",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_expose_steering_messages_on_step_results()
    {
        Assert.Equal(
            typeof(IReadOnlyList<UserModelMessage>),
            typeof(StepResult)
                .GetProperty(nameof(StepResult.ExperimentalSteeringMessages))!
                .PropertyType
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass steering messages to prepareStep",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_pass_steering_messages_to_prepareStep()
    {
        Assert.Equal(
            typeof(IReadOnlyList<UserModelMessage>),
            typeof(PrepareStepContext)
                .GetProperty(nameof(PrepareStepContext.ExperimentalSteeringMessages))!
                .PropertyType
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept a steering object in generateText, streamText and agents",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_accept_a_steering_object_in_generateText_streamText_and_agents()
    {
        var steering = SteeringFactory.ExperimentalCreateSteering();

        var generate = new GenerateTextOptions { ExperimentalSteering = steering };
        var stream = new StreamTextOptions { ExperimentalSteering = steering };
        var agent = typeof(Agent);

        Assert.Same(steering, generate.ExperimentalSteering);
        Assert.Same(steering, stream.ExperimentalSteering);
        Assert.All(agent.GetMethods().Where(m => m.Name is "GenerateAsync" or "Stream"), m =>
            Assert.Contains(m.GetParameters(), p => p.ParameterType == typeof(ISteering))
        );
    }
}
