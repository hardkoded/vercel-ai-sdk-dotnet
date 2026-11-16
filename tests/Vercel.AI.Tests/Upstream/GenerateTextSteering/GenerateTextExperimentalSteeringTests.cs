// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.Steering;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateTextSteering.SteeringTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateTextSteering;

/// <summary>Port of <c>generate-text-steering.test.ts</c> &gt; <c>generateText experimental_steering</c>.</summary>
public sealed class GenerateTextExperimentalSteeringTests
{
    private const string Prefix = "packages/ai/src/generate-text/generate-text-steering.test.ts::generateText experimental_steering::";
    private const string Commit = "b8c01eb49d82fd32efe8a2a1e4cd02fbb2b7ae97";
    private const string Prompt = "What is the weather in Paris?";

    [Fact]
    [UpstreamTest(Prefix + "should deliver a message sent during tool execution at the next step", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_deliver_a_message_sent_during_tool_execution_at_the_next_step()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;

        var model = SteeringTestModel.Generate(
            _ => ToolCallResponse(),
            _ => TextResponse("It is sunny in Paris, Texas."));

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = Prompt,
            Tools =
            [
                Tool.Function("weather", null, WeatherSchema, (_, _) =>
                {
                    outcome = steering.SteerAsync("I meant Paris, Texas.", new SteerOptions { Id = "steer-1" });
                    return Task.FromResult("sunny in Paris");
                }),
            ],
            StopWhen = StopWhen.IsStepCount(5),
            ExperimentalSteering = steering,
        });

        var delivered = SteeringAssert.Delivered(await outcome!, 1);
        Assert.Equal("steer-1", delivered.Id);
        Assert.Equal("I meant Paris, Texas.", Text(delivered.Message));
        var prompt = model.DoGenerateCalls[1].Prompt.Skip(1).ToArray();
        Assert.Equal(["assistant", "tool", "user"], prompt.Select(message => message.Role));
        var toolResult = Assert.IsType<ToolModelMessage>(prompt[1]);
        Assert.Equal("call-1", toolResult.ToolCallId);
        Assert.Equal("weather", toolResult.ToolName);
        Assert.Equal("I meant Paris, Texas.", Text(Assert.IsType<UserModelMessage>(prompt[2])));
        Assert.Null(result.Steps[0].ExperimentalSteeringMessages);
        Assert.Equal("I meant Paris, Texas.", Text(Assert.Single(result.Steps[1].ExperimentalSteeringMessages!)));
        Assert.Equal("It is sunny in Paris, Texas.", result.Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "should continue for another step when a message arrives during the final answer", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_continue_for_another_step_when_a_message_arrives_during_the_final_answer()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;

        var model = SteeringTestModel.Generate(
            _ =>
            {
                outcome = steering.SteerAsync("Also tell me about Tokyo.");
                return TextResponse("It is sunny in Paris.");
            },
            _ => TextResponse("Tokyo is rainy."));

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = Prompt,
            StopWhen = StopWhen.IsStepCount(5),
            ExperimentalSteering = steering,
        });

        Assert.Equal(2, model.DoGenerateCalls.Count);
        Assert.Equal("Also tell me about Tokyo.", LastUserText(model.DoGenerateCalls[1]));
        SteeringAssert.Delivered(await outcome!, 1);
        Assert.Equal("Tokyo is rainy.", result.Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "should deliver messages sent before the run in the first step", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_deliver_messages_sent_before_the_run_in_the_first_step()
    {
        var steering = CreateSteering();
        var outcome = steering.SteerAsync("Answer in French.");

        var model = SteeringTestModel.Generate(_ => TextResponse("Il fait beau."));

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = Prompt,
            ExperimentalSteering = steering,
        });

        Assert.Single(model.DoGenerateCalls);
        Assert.Equal("Answer in French.", LastUserText(model.DoGenerateCalls[0]));
        SteeringAssert.Delivered(await outcome, 0);
        Assert.Single(result.Steps[0].ExperimentalSteeringMessages!);
    }

    [Fact]
    [UpstreamTest(Prefix + "should add each message as its own user message in order", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_add_each_message_as_its_own_user_message_in_order()
    {
        var steering = CreateSteering();

        var model = SteeringTestModel.Generate(
            _ =>
            {
                Ignore(steering.SteerAsync("first"));
                Ignore(steering.SteerAsync("second"));
                return TextResponse("done");
            },
            _ => TextResponse("done again"));

        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "hello",
            StopWhen = StopWhen.IsStepCount(5),
            ExperimentalSteering = steering,
        });

        var lastTwo = model.DoGenerateCalls[1].Prompt.TakeLast(2).ToArray();
        Assert.Equal("first", Text(Assert.IsType<UserModelMessage>(lastTwo[0])));
        Assert.Equal("second", Text(Assert.IsType<UserModelMessage>(lastTwo[1])));
    }

    [Fact]
    [UpstreamTest(Prefix + "should not deliver messages when the default stop condition ends the run", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_not_deliver_messages_when_the_default_stop_condition_ends_the_run()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;

        var model = SteeringTestModel.Generate(_ =>
        {
            outcome = steering.SteerAsync("Also tell me about Tokyo.", new SteerOptions { Id = "steer-1" });
            return TextResponse("It is sunny in Paris.");
        });

        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = Prompt,
            ExperimentalSteering = steering,
        });

        Assert.Single(model.DoGenerateCalls);
        var notDelivered = SteeringAssert.NotDelivered(await outcome!, SteeringNotDeliveredReason.StopCondition);
        Assert.Equal("steer-1", notDelivered.Id);
        Assert.Equal("Also tell me about Tokyo.", Text(notDelivered.Message));
    }

    [Fact]
    [UpstreamTest(Prefix + "should not deliver messages when a stop condition ends a tool loop", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_not_deliver_messages_when_a_stop_condition_ends_a_tool_loop()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;

        var model = SteeringTestModel.Generate(_ => ToolCallResponse());

        await Client().GenerateTextAsync(new GenerateTextOptions
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
            StopWhen = StopWhen.IsStepCount(1),
            ExperimentalSteering = steering,
        });

        Assert.Single(model.DoGenerateCalls);
        SteeringAssert.NotDelivered(await outcome!, SteeringNotDeliveredReason.StopCondition);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not deliver messages while a tool approval is pending", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_not_deliver_messages_while_a_tool_approval_is_pending()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;

        var model = SteeringTestModel.Generate(_ =>
        {
            outcome = steering.SteerAsync("Hurry up.");
            return ToolCallResponse();
        });

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = Prompt,
            Tools = [Weather(() => "sunny")],
            ToolApproval = new ToolApprovalConfiguration { { "weather", "user-approval" } },
            StopWhen = StopWhen.IsStepCount(5),
            ExperimentalSteering = steering,
        });

        Assert.Single(model.DoGenerateCalls);
        Assert.Equal("tool-approval-request", result.Steps[^1].Content[^1].Type);
        SteeringAssert.NotDelivered(await outcome!, SteeringNotDeliveredReason.WaitingForToolResults);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not deliver messages when the run is aborted", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_not_deliver_messages_when_the_run_is_aborted()
    {
        var steering = CreateSteering();
        using var cancellation = new CancellationTokenSource();
        Task<SteeringOutcome>? outcome = null;

        var model = SteeringTestModel.Generate(_ => ToolCallResponse());

        await Assert.ThrowsAnyAsync<Exception>(() => Client().GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = Prompt,
                Tools =
                [
                    Weather(() =>
                    {
                        outcome = steering.SteerAsync("I meant Paris, Texas.");
                        cancellation.Cancel();
                        return "sunny";
                    }),
                ],
                StopWhen = StopWhen.IsStepCount(5),
                ExperimentalSteering = steering,
            },
            cancellation.Token));

        Assert.Single(model.DoGenerateCalls);
        SteeringAssert.NotDelivered(await outcome!, SteeringNotDeliveredReason.Aborted);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not deliver messages when the run fails", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_not_deliver_messages_when_the_run_fails()
    {
        var steering = CreateSteering();
        var outcome = steering.SteerAsync("hello");

        var model = SteeringTestModel.Generate(_ => throw new InvalidOperationException("provider failure"));

        var error = await Assert.ThrowsAnyAsync<Exception>(() => Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = Prompt,
            MaxRetries = 0,
            ExperimentalSteering = steering,
        }));

        Assert.Contains("provider failure", error.Message);
        SteeringAssert.Delivered(await outcome, 0);
        SteeringAssert.NotDelivered(await steering.SteerAsync("after the failure"), SteeringNotDeliveredReason.Finished);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not deliver messages sent after the run finished", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_not_deliver_messages_sent_after_the_run_finished()
    {
        var steering = CreateSteering();

        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = SteeringTestModel.Generate(_ => TextResponse("done")),
            Prompt = "hello",
            ExperimentalSteering = steering,
        });

        SteeringAssert.NotDelivered(await steering.SteerAsync("too late"), SteeringNotDeliveredReason.Finished);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass steering messages to prepareStep and keep them out of response messages", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_pass_steering_messages_to_prepareStep_and_keep_them_out_of_response_messages()
    {
        var steering = CreateSteering();
        var prepareStepMessages = new List<ModelMessage?>();

        var model = SteeringTestModel.Generate(
            _ =>
            {
                Ignore(steering.SteerAsync("Also tell me about Tokyo."));
                return TextResponse("It is sunny in Paris.");
            },
            _ => TextResponse("Tokyo is rainy."));

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = Prompt,
            StopWhen = StopWhen.IsStepCount(5),
            ExperimentalSteering = steering,
            PrepareStep = context =>
            {
                prepareStepMessages.Add(context.Messages.LastOrDefault());
                return null;
            },
        });

        Assert.Equal("Also tell me about Tokyo.", Text(Assert.IsType<UserModelMessage>(prepareStepMessages[1])));
        Assert.Equal(["assistant", "assistant"], result.ResponseMessages.Select(message => message.Role));
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass steering messages to prepareStep so rebuilt messages keep them in later steps", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_pass_steering_messages_to_prepareStep_so_rebuilt_messages_keep_them_in_later_steps()
    {
        var steering = CreateSteering();
        var prepareStepSteeringMessages = new List<IReadOnlyList<UserModelMessage>?>();
        var steered = false;

        var model = SteeringTestModel.Generate(
            _ => ToolCallResponse(),
            _ => ToolCallResponse(),
            _ => TextResponse("It is sunny in Paris, Texas."));

        await Client().GenerateTextAsync(new GenerateTextOptions
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
                        .. context.Steps.SelectMany(step => (step.ExperimentalSteeringMessages ?? []).Cast<ModelMessage>().Concat(step.Response.Messages)),
                        .. context.ExperimentalSteeringMessages ?? [],
                    ],
                };
            },
        });

        Assert.Equal(3, prepareStepSteeringMessages.Count);
        Assert.Null(prepareStepSteeringMessages[0]);
        Assert.Equal("I meant Paris, Texas.", Text(Assert.Single(prepareStepSteeringMessages[1]!)));
        Assert.Null(prepareStepSteeringMessages[2]);
        foreach (var call in new[] { 1, 2 })
        {
            Assert.Contains(
                model.DoGenerateCalls[call].Prompt,
                message => message is UserModelMessage user && user.Content.Any(part => part is TextContentPart { Text: "I meant Paris, Texas." }));
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "should not continue for a steering message after a step that finished with an error", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_not_continue_for_a_steering_message_after_a_step_that_finished_with_an_error()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;

        var model = SteeringTestModel.Generate(
            _ =>
            {
                outcome = steering.SteerAsync("Also tell me about Tokyo.");
                return TextResponse("It is", FinishReason.Error, "error");
            },
            _ => TextResponse("Tokyo is rainy."));

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = Prompt,
            StopWhen = StopWhen.IsStepCount(5),
            ExperimentalSteering = steering,
        });

        Assert.Single(model.DoGenerateCalls);
        Assert.Equal(FinishReason.Error, result.FinishReason);
        SteeringAssert.NotDelivered(await outcome!, SteeringNotDeliveredReason.Error);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not deliver messages when the prompt is invalid", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_not_deliver_messages_when_the_prompt_is_invalid()
    {
        var steering = CreateSteering();
        var outcome = steering.SteerAsync("hello");

        await Assert.ThrowsAnyAsync<Exception>(() => Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = SteeringTestModel.Generate(_ => TextResponse("unused")),
            Messages = [],
            ExperimentalSteering = steering,
        }));

        SteeringAssert.NotDelivered(await outcome, SteeringNotDeliveredReason.Error);
        SteeringAssert.NotDelivered(await steering.SteerAsync("after the failure"), SteeringNotDeliveredReason.Finished);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not deliver messages that prepareStep removes", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_not_deliver_messages_that_prepareStep_removes()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;

        var model = SteeringTestModel.Generate(
            _ => ToolCallResponse(),
            _ => TextResponse("It is sunny in Paris."));

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
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
            PrepareStep = context => new PrepareStepUpdate { Messages = [.. context.InitialMessages, .. context.ResponseMessages] },
        });

        SteeringAssert.NotDelivered(await outcome!, SteeringNotDeliveredReason.RemovedByPrepareStep);
        Assert.Null(result.Steps[1].ExperimentalSteeringMessages);
        Assert.Null(LastUserText(model.DoGenerateCalls[1]));
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw when the steering object is used by two runs at once", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_throw_when_the_steering_object_is_used_by_two_runs_at_once()
    {
        var steering = CreateSteering();
        var model = SteeringTestModel.Generate(
            _ => TextResponse("first"),
            _ => TextResponse("second"));

        var first = Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "hello", ExperimentalSteering = steering });

        var error = await Assert.ThrowsAsync<InvalidArgumentError>(() => Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "hello", ExperimentalSteering = steering }));
        Assert.Equal(
            "Invalid argument for parameter experimental_steering: The steering object was already used by a generation. Create a new one with experimental_createSteering() for each call.",
            error.Message);

        await first;
    }
}
