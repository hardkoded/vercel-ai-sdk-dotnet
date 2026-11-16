// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;
using Vercel.AI.Tests.Upstream.Steering;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateTextSteering.SteeringTestSupport;

namespace Vercel.AI.Tests.Upstream.StreamTextSteering;

/// <summary>Port of <c>stream-text-steering.test.ts</c> &gt; <c>streamText experimental_steering</c>.</summary>
public sealed class StreamTextExperimentalSteeringTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text-steering.test.ts::streamText experimental_steering::";
    private const string Commit = "b8c01eb49d82fd32efe8a2a1e4cd02fbb2b7ae97";
    private const string Prompt = "What is the weather in Paris?";

    private static async Task ConsumeAsync(StreamTextResult result)
    {
        try
        {
            await result.ConsumeStreamAsync();
        }
        catch (Exception)
        {
            // Like consumeStream({ onError: () => {} }), a failing stream is reported by the steering outcome of the test.
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should deliver a message sent during tool execution at the next step",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_deliver_a_message_sent_during_tool_execution_at_the_next_step()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var model = SteeringTestModel.Stream(ToolCallStream, () =>
            TextStream("It is sunny in Paris, Texas.")
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
                            outcome = steering.SteerAsync(
                                "I meant Paris, Texas.",
                                new SteerOptions { Id = "steer-1" }
                            );
                            return "sunny";
                        }),
                    ],
                    StopWhen = StopWhen.IsStepCount(5),
                    ExperimentalSteering = steering,
                }
            );

        Assert.Equal("It is sunny in Paris, Texas.", await result.Text);
        var delivered = SteeringAssert.Delivered(await outcome!, 1);
        Assert.Equal("steer-1", delivered.Id);
        Assert.Equal("I meant Paris, Texas.", Text(delivered.Message));
        var lastTwo = model.DoStreamCalls[1].Prompt.TakeLast(2).ToArray();
        var toolResult = Assert.IsType<ToolModelMessage>(lastTwo[0]);
        Assert.Equal("call-1", toolResult.ToolCallId);
        Assert.Equal("weather", toolResult.ToolName);
        Assert.Equal("I meant Paris, Texas.", Text(Assert.IsType<UserModelMessage>(lastTwo[1])));
        var steps = await result.Steps;
        Assert.Null(steps[0].ExperimentalSteeringMessages);
        Assert.Equal(
            "I meant Paris, Texas.",
            Text(Assert.Single(steps[1].ExperimentalSteeringMessages!))
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should continue for another step when a message arrives during the final answer",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_continue_for_another_step_when_a_message_arrives_during_the_final_answer()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var model = SteeringTestModel.Stream(
            () =>
            {
                outcome = steering.SteerAsync("Also tell me about Tokyo.");
                return TextStream("It is sunny in Paris. ");
            },
            () => TextStream("Tokyo is rainy.")
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

        Assert.Equal("Tokyo is rainy.", await result.Text);
        Assert.Equal(2, model.DoStreamCalls.Count);
        Assert.Equal("Also tell me about Tokyo.", LastUserText(model.DoStreamCalls[1]));
        SteeringAssert.Delivered(await outcome!, 1);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should deliver messages sent before the stream starts in the first step",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_deliver_messages_sent_before_the_stream_starts_in_the_first_step()
    {
        var steering = CreateSteering();
        var outcome = steering.SteerAsync("Answer in French.");
        var model = SteeringTestModel.Stream(() => TextStream("Il fait beau."));

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

        Assert.Equal("Answer in French.", LastUserText(model.DoStreamCalls[0]));
        SteeringAssert.Delivered(await outcome, 0);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not deliver messages when the default stop condition ends the stream",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_deliver_messages_when_the_default_stop_condition_ends_the_stream()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var model = SteeringTestModel.Stream(() =>
        {
            outcome = steering.SteerAsync(
                "Also tell me about Tokyo.",
                new SteerOptions { Id = "steer-1" }
            );
            return TextStream("It is sunny in Paris.");
        });

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
        var notDelivered = SteeringAssert.NotDelivered(
            await outcome!,
            SteeringNotDeliveredReason.StopCondition
        );
        Assert.Equal("steer-1", notDelivered.Id);
        Assert.Equal("Also tell me about Tokyo.", Text(notDelivered.Message));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not deliver messages while a tool approval is pending",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_deliver_messages_while_a_tool_approval_is_pending()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var model = SteeringTestModel.Stream(() =>
        {
            outcome = steering.SteerAsync("Hurry up.");
            return ToolCallStream();
        });

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = Prompt,
                    Tools = [Weather(() => "sunny")],
                    ToolApproval = new ToolApprovalConfiguration { { "weather", "user-approval" } },
                    StopWhen = StopWhen.IsStepCount(5),
                    ExperimentalSteering = steering,
                }
            );

        await result.ConsumeStreamAsync();

        Assert.Single(model.DoStreamCalls);
        SteeringAssert.NotDelivered(
            await outcome!,
            SteeringNotDeliveredReason.WaitingForToolResults
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not deliver messages when the stream is aborted",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_deliver_messages_when_the_stream_is_aborted()
    {
        var steering = CreateSteering();
        var abortController = new AbortController();
        Task<SteeringOutcome>? outcome = null;
        var model = SteeringTestModel.Stream(ToolCallStream);

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
                            outcome = steering.SteerAsync("I meant Paris, Texas.");
                            abortController.Abort();
                            return "sunny";
                        }),
                    ],
                    StopWhen = StopWhen.IsStepCount(5),
                    AbortSignal = abortController.Signal,
                    ExperimentalSteering = steering,
                }
            );

        await ConsumeAsync(result);

        Assert.Single(model.DoStreamCalls);
        SteeringAssert.NotDelivered(await outcome!, SteeringNotDeliveredReason.Aborted);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not deliver messages when the stream fails",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_deliver_messages_when_the_stream_fails()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var model = new SteeringTestModel(
            stream:
            [
                _ =>
                {
                    outcome = steering.SteerAsync("during the failure");
                    return FailingStream();
                },
            ]
        );

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = "hello",
                    StopWhen = StopWhen.IsStepCount(5),
                    OnError = (_, _) => Task.CompletedTask,
                    ExperimentalSteering = steering,
                }
            );

        await ConsumeAsync(result);

        SteeringAssert.NotDelivered(await outcome!, SteeringNotDeliveredReason.Error);
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> FailingStream()
    {
        yield return new StreamStartStreamPart(Array.Empty<CallWarning>());
        await Task.Yield();
        throw new InvalidOperationException("provider stream failure");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not deliver messages sent after the stream finished",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_deliver_messages_sent_after_the_stream_finished()
    {
        var steering = CreateSteering();
        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = SteeringTestModel.Stream(() => TextStream("done")),
                    Prompt = "hello",
                    ExperimentalSteering = steering,
                }
            );

        await result.ConsumeStreamAsync();

        SteeringAssert.NotDelivered(
            await steering.SteerAsync("too late"),
            SteeringNotDeliveredReason.Finished
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep steering messages out of response messages",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_keep_steering_messages_out_of_response_messages()
    {
        var steering = CreateSteering();
        var model = SteeringTestModel.Stream(
            () =>
            {
                Ignore(steering.SteerAsync("Also tell me about Tokyo."));
                return TextStream("It is sunny in Paris.");
            },
            () => TextStream("Tokyo is rainy.")
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

        Assert.Equal(
            ["assistant", "assistant"],
            (await result.ResponseMessages).Select(message => message.Role)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should pass steering messages to prepareStep so rebuilt messages keep them in later steps",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_pass_steering_messages_to_prepareStep_so_rebuilt_messages_keep_them_in_later_steps()
    {
        var steering = CreateSteering();
        var prepareStepSteeringMessages = new List<IReadOnlyList<UserModelMessage>?>();
        var steered = false;
        var model = SteeringTestModel.Stream(ToolCallStream, ToolCallStream, () =>
            TextStream("It is sunny in Paris, Texas.")
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
                            if (!steered)
                            {
                                steered = true;
                                Ignore(steering.SteerAsync("I meant Paris, Texas."));
                            }

                            return "sunny";
                        }),
                    ],
                    StopWhen = StopWhen.IsStepCount(5),
                    ExperimentalSteering = steering,
                    PrepareStep = context =>
                    {
                        prepareStepSteeringMessages.Add(context.ExperimentalSteeringMessages);
                        return new PrepareStepUpdate
                        {
                            Messages =
                            [
                                .. context.InitialMessages,
                                .. context.Steps.SelectMany(step =>
                                    (step.ExperimentalSteeringMessages ?? [])
                                        .Cast<ModelMessage>()
                                        .Concat(step.Response.Messages)
                                ),
                                .. context.ExperimentalSteeringMessages ?? [],
                            ],
                        };
                    },
                }
            );

        await result.ConsumeStreamAsync();

        Assert.Equal(3, prepareStepSteeringMessages.Count);
        Assert.Null(prepareStepSteeringMessages[0]);
        Assert.Equal("I meant Paris, Texas.", Text(Assert.Single(prepareStepSteeringMessages[1]!)));
        Assert.Null(prepareStepSteeringMessages[2]);
        foreach (var call in new[] { 1, 2 })
        {
            Assert.Contains(model.DoStreamCalls[call].Prompt, message =>
                message is UserModelMessage user
                && user.Content.Any(part =>
                    part is TextContentPart { Text: "I meant Paris, Texas." }
                )
            );
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not continue for a steering message after a provider error",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_continue_for_a_steering_message_after_a_provider_error()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var model = SteeringTestModel.Stream(
            () =>
            {
                outcome = steering.SteerAsync("Also tell me about Tokyo.");
                return new LanguageModelStreamPart[]
                {
                    new StreamStartStreamPart(Array.Empty<CallWarning>()),
                    new TextStartStreamPart("1"),
                    new TextDeltaStreamPart("1", "It is"),
                    new ErrorStreamPart("provider failure"),
                    new FinishStreamPart(FinishReason.Error, SteeringTestSupport.Usage, "error"),
                };
            },
            () => TextStream("Tokyo is rainy.")
        );

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = Prompt,
                    StopWhen = StopWhen.IsStepCount(5),
                    OnError = (_, _) => Task.CompletedTask,
                    ExperimentalSteering = steering,
                }
            );

        await ConsumeAsync(result);

        Assert.Single(model.DoStreamCalls);
        Assert.Equal(FinishReason.Error, await result.FinishReason);
        SteeringAssert.NotDelivered(await outcome!, SteeringNotDeliveredReason.Error);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not deliver messages when the prompt is invalid",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_deliver_messages_when_the_prompt_is_invalid()
    {
        var steering = CreateSteering();
        var outcome = steering.SteerAsync("hello");

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = SteeringTestModel.Stream(() => TextStream("unused")),
                    Messages = [],
                    OnError = (_, _) => Task.CompletedTask,
                    ExperimentalSteering = steering,
                }
            );

        await ConsumeAsync(result);

        SteeringAssert.NotDelivered(await outcome, SteeringNotDeliveredReason.Error);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw when the steering object is reused after a stream finished",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_throw_when_the_steering_object_is_reused_after_a_stream_finished()
    {
        var steering = CreateSteering();
        var model = SteeringTestModel.Stream(() => TextStream("first answer"));

        await Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = "hello",
                    ExperimentalSteering = steering,
                }
            )
            .ConsumeStreamAsync();

        var error = Assert.Throws<InvalidArgumentError>(() =>
            Client()
                .StreamTextAsync(
                    new StreamTextOptions
                    {
                        Model = model,
                        Prompt = "hello",
                        ExperimentalSteering = steering,
                    }
                )
        );
        Assert.Contains(
            "Create a new one with experimental_createSteering() for each call.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not deliver messages that prepareStep removes",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_deliver_messages_that_prepareStep_removes()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var model = SteeringTestModel.Stream(ToolCallStream, () =>
            TextStream("It is sunny in Paris.")
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
                            outcome = steering.SteerAsync("I meant Paris, Texas.");
                            return "sunny";
                        }),
                    ],
                    StopWhen = StopWhen.IsStepCount(5),
                    ExperimentalSteering = steering,
                    PrepareStep = context => new PrepareStepUpdate
                    {
                        Messages = [.. context.InitialMessages, .. context.ResponseMessages],
                    },
                }
            );

        var steps = await result.Steps;

        SteeringAssert.NotDelivered(
            await outcome!,
            SteeringNotDeliveredReason.RemovedByPrepareStep
        );
        Assert.Null(steps[1].ExperimentalSteeringMessages);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw when the steering object is used by two streams at once",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_throw_when_the_steering_object_is_used_by_two_streams_at_once()
    {
        var steering = CreateSteering();
        var model = SteeringTestModel.Stream(() => TextStream("first"), () => TextStream("second"));

        var first = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = "hello",
                    ExperimentalSteering = steering,
                }
            );

        var error = Assert.Throws<InvalidArgumentError>(() =>
            Client()
                .StreamTextAsync(
                    new StreamTextOptions
                    {
                        Model = model,
                        Prompt = "hello",
                        ExperimentalSteering = steering,
                    }
                )
        );
        Assert.Equal(
            "Invalid argument for parameter experimental_steering: The steering object was already used by a generation. Create a new one with experimental_createSteering() for each call.",
            error.Message
        );

        await first.ConsumeStreamAsync();
    }
}
