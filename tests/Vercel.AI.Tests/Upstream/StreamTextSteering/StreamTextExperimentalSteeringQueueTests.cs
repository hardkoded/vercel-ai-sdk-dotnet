// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;
using Vercel.AI.Tests.Upstream.Steering;
using static Vercel.AI.Tests.Upstream.GenerateTextSteering.SteeringTestSupport;

namespace Vercel.AI.Tests.Upstream.StreamTextSteering;

/// <summary>Port of <c>stream-text-steering.test.ts</c> &gt; <c>streamText experimental_steering</c> &gt; <c>queue</c>.</summary>
public sealed class StreamTextExperimentalSteeringQueueTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text-steering.test.ts::streamText experimental_steering > queue::";
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
        var model = SteeringTestModel.Stream(
            ToolCallStream,
            () => TextStream("It is sunny in Paris."),
            () => TextStream("The readme is updated.")
        );

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
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

        Assert.Equal("The readme is updated.", await result.Text);
        Assert.Equal(3, model.DoStreamCalls.Count);
        Assert.Null(LastUserText(model.DoStreamCalls[1]));
        Assert.Equal("Then update the readme.", LastUserText(model.DoStreamCalls[2]));
        SteeringAssert.Delivered(await outcome!, 2);
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
        var model = SteeringTestModel.Stream(
            () => TextStream("It is sunny in Paris."),
            () => TextStream("The readme is updated."),
            () => TextStream("The changelog is updated.")
        );

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = Prompt,
                    StopWhen = StopWhen.IsStepCount(5),
                    ExperimentalSteering = steering,
                }
            );

        await result.ConsumeStreamAsync();

        Assert.Equal(
            [
                "What is the weather in Paris?",
                "Then update the readme.",
                "Then add a changelog entry.",
            ],
            model.DoStreamCalls.Select(LastUserText)
        );
        SteeringAssert.Delivered(await first, 1);
        SteeringAssert.Delivered(await second, 2);
        Assert.Equal(
            [null, "Then update the readme.", "Then add a changelog entry."],
            (await result.Steps).Select(step =>
                step.ExperimentalSteeringMessages is { } messages ? Text(messages[0]) : null
            )
        );
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
        var model = SteeringTestModel.Stream(() => TextStream("It is sunny", finishReason, raw));

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = Prompt,
                    StopWhen = StopWhen.IsStepCount(5),
                    ExperimentalSteering = steering,
                }
            );

        await result.ConsumeStreamAsync();

        Assert.Single(model.DoStreamCalls);
        SteeringAssert.NotDelivered(await outcome, SteeringNotDeliveredReason.Finished);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should not deliver a queued message when the default stop condition ends the stream",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_deliver_a_queued_message_when_the_default_stop_condition_ends_the_stream()
    {
        var steering = CreateSteering();
        var outcome = steering.QueueAsync("Then update the readme.");
        var model = SteeringTestModel.Stream(() => TextStream("It is sunny in Paris."));

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = Prompt,
                    ExperimentalSteering = steering,
                }
            );

        await result.ConsumeStreamAsync();

        Assert.Single(model.DoStreamCalls);
        SteeringAssert.NotDelivered(await outcome, SteeringNotDeliveredReason.StopCondition);
    }
}
