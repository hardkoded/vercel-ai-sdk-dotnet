// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.Steering;
using static Vercel.AI.Tests.Upstream.GenerateTextSteering.SteeringTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateTextSteering;

/// <summary>Port of <c>generate-text-steering.test.ts</c> &gt; <c>generateText experimental_steering</c> &gt; <c>queue</c>.</summary>
public sealed class GenerateTextExperimentalSteeringQueueTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/generate-text-steering.test.ts::generateText experimental_steering > queue::";
    private const string Commit = "b8c01eb49d82fd32efe8a2a1e4cd02fbb2b7ae97";
    private const string Prompt = "What is the weather in Paris?";

    [Fact]
    [UpstreamTest(
        Prefix + "should wait until the current task is done to deliver a queued message",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_wait_until_the_current_task_is_done_to_deliver_a_queued_message()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;

        var model = SteeringTestModel.Generate(
            _ => ToolCallResponse(),
            _ => TextResponse("It is sunny in Paris."),
            _ => TextResponse("The readme is updated.")
        );

        var result = await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = model,
                    Prompt = Prompt,
                    Tools =
                    [
                        Weather(() =>
                        {
                            outcome = steering.QueueAsync("Then update the readme.");
                            return "sunny";
                        }),
                    ],
                    StopWhen = StopWhen.IsStepCount(5),
                    ExperimentalSteering = steering,
                }
            );

        Assert.Equal(3, model.DoGenerateCalls.Count);
        Assert.Null(LastUserText(model.DoGenerateCalls[1]));
        Assert.Equal("Then update the readme.", LastUserText(model.DoGenerateCalls[2]));
        SteeringAssert.Delivered(await outcome!, 2);
        Assert.Equal("The readme is updated.", result.Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should deliver one queued message per task in order",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_deliver_one_queued_message_per_task_in_order()
    {
        var steering = CreateSteering();
        var first = steering.QueueAsync("Then update the readme.");
        var second = steering.QueueAsync("Then add a changelog entry.");

        var model = SteeringTestModel.Generate(
            _ => TextResponse("It is sunny in Paris."),
            _ => TextResponse("The readme is updated."),
            _ => TextResponse("The changelog is updated.")
        );

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = model,
                    Prompt = Prompt,
                    StopWhen = StopWhen.IsStepCount(5),
                    ExperimentalSteering = steering,
                }
            );

        Assert.Equal(
            [
                "What is the weather in Paris?",
                "Then update the readme.",
                "Then add a changelog entry.",
            ],
            model.DoGenerateCalls.Select(LastUserText)
        );
        SteeringAssert.Delivered(await first, 1);
        SteeringAssert.Delivered(await second, 2);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should deliver steering messages before queued messages",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_deliver_steering_messages_before_queued_messages()
    {
        var steering = CreateSteering();
        var queued = steering.QueueAsync("Then update the readme.");

        var model = SteeringTestModel.Generate(
            _ =>
            {
                Ignore(steering.SteerAsync("Use Celsius."));
                return TextResponse("It is sunny in Paris.");
            },
            _ => TextResponse("It is 20 degrees."),
            _ => TextResponse("The readme is updated.")
        );

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = model,
                    Prompt = Prompt,
                    StopWhen = StopWhen.IsStepCount(5),
                    ExperimentalSteering = steering,
                }
            );

        Assert.Equal(
            ["What is the weather in Paris?", "Use Celsius.", "Then update the readme."],
            model.DoGenerateCalls.Select(LastUserText)
        );
        SteeringAssert.Delivered(await queued, 2);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not deliver a queued message when a stop condition ends the run",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_deliver_a_queued_message_when_a_stop_condition_ends_the_run()
    {
        var steering = CreateSteering();
        var outcome = steering.QueueAsync(
            "Then update the readme.",
            new QueueOptions { Id = "queued-1" }
        );

        var model = SteeringTestModel.Generate(_ => TextResponse("It is sunny in Paris."));

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = model,
                    Prompt = Prompt,
                    ExperimentalSteering = steering,
                }
            );

        Assert.Single(model.DoGenerateCalls);
        var notDelivered = SteeringAssert.NotDelivered(
            await outcome,
            SteeringNotDeliveredReason.StopCondition
        );
        Assert.Equal("queued-1", notDelivered.Id);
        Assert.Equal("Then update the readme.", Text(notDelivered.Message));
    }

    [Theory]
    [InlineData(FinishReason.Length, "length")]
    [InlineData(FinishReason.ContentFilter, "content-filter")]
    [UpstreamTest(
        Prefix + "should not deliver a queued message when the response finished with %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_deliver_a_queued_message_when_the_response_finished_with(
        FinishReason finishReason,
        string raw
    )
    {
        var steering = CreateSteering();
        var outcome = steering.QueueAsync("Then update the readme.");

        var model = SteeringTestModel.Generate(_ => TextResponse("It is sunny", finishReason, raw));

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = model,
                    Prompt = Prompt,
                    StopWhen = StopWhen.IsStepCount(5),
                    ExperimentalSteering = steering,
                }
            );

        Assert.Single(model.DoGenerateCalls);
        SteeringAssert.NotDelivered(await outcome, SteeringNotDeliveredReason.Finished);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not deliver a queued message after the run finished",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_deliver_a_queued_message_after_the_run_finished()
    {
        var steering = CreateSteering();
        var model = SteeringTestModel.Generate(_ => TextResponse("It is sunny in Paris."));

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = model,
                    Prompt = Prompt,
                    ExperimentalSteering = steering,
                }
            );

        SteeringAssert.NotDelivered(
            await steering.QueueAsync("Then update the readme.", new QueueOptions { Id = "late" }),
            SteeringNotDeliveredReason.Finished
        );
    }
}
