// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.Steering;
using static Vercel.AI.Tests.Upstream.GenerateTextSteeringInterrupt.SteeringInterruptTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateTextSteeringInterrupt;

/// <summary>Port of <c>generate-text-steering-interrupt.test.ts</c> &gt; <c>generateText experimental_steering interrupt</c>.</summary>
public sealed class GenerateTextExperimentalSteeringInterruptTests
{
    private const string Commit = "e4039889ba58bbddf965ce61bf8fe6626e9f8800";
    private const string Prefix =
        "packages/ai/src/generate-text/generate-text-steering-interrupt.test.ts::generateText experimental_steering interrupt::";

    private static Task<GenerateTextResult> GenerateAsync(
        GenerateTextOptions options,
        CancellationToken cancellationToken = default
    ) => Client().GenerateTextAsync(options, cancellationToken);

    [Fact]
    [UpstreamTest(
        Prefix
            + "should cancel a model call without output and redo it in the same step with the message",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_cancel_a_model_call_without_output_and_redo_it_in_the_same_step_with_the_message()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var model = InterruptTestModel.HangingFirstCall(
            _ =>
                outcome = steering.SteerAsync(
                    "Use Paris, Texas instead.",
                    new SteerOptions { Interrupt = true }
                ),
            _ => TextResponse("It is sunny in Paris, Texas.")
        );

        var result = await GenerateAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = PromptText,
                ExperimentalSteering = steering,
            }
        );

        Assert.Equal(2, model.DoGenerateCalls.Count);
        Assert.True(model.DoGenerateCalls[0].AbortSignal?.Aborted);
        Assert.Equal("Use Paris, Texas instead.", LastUserText(model.DoGenerateCalls[1]));
        Assert.Single(result.Steps);
        Assert.Equal("It is sunny in Paris, Texas.", result.Text);
        Assert.Equal(
            "Use Paris, Texas instead.",
            Text(Assert.Single(result.Steps[0].ExperimentalSteeringMessages!))
        );
        // the cancelled call is left out of the usage:
        Assert.Equal(10, result.Usage.OutputTokens);
        SteeringAssert.Delivered(await outcome!, 0);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep steering messages that the step already delivered in the redo",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_keep_steering_messages_that_the_step_already_delivered_in_the_redo()
    {
        var steering = CreateSteering();
        var earlier = steering.SteerAsync("Answer in Celsius.");
        Task<SteeringOutcome>? interrupt = null;
        var model = InterruptTestModel.HangingFirstCall(
            _ =>
                interrupt = steering.SteerAsync(
                    "Use Paris, Texas instead.",
                    new SteerOptions { Interrupt = true }
                ),
            _ => TextResponse("It is 22 degrees in Paris, Texas.")
        );

        var result = await GenerateAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = PromptText,
                ExperimentalSteering = steering,
            }
        );

        var userTexts = model
            .DoGenerateCalls[1]
            .Prompt.OfType<UserModelMessage>()
            .Select(Text)
            .ToArray();
        Assert.Equal(
            ["What is the weather in Paris?", "Answer in Celsius.", "Use Paris, Texas instead."],
            userTexts
        );
        Assert.Equal(
            ["Answer in Celsius.", "Use Paris, Texas instead."],
            result.Steps[0].ExperimentalSteeringMessages!.Select(Text)
        );
        SteeringAssert.Delivered(await earlier, 0);
        SteeringAssert.Delivered(await interrupt!, 0);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should redo the step for every interrupt and keep the messages in order",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_redo_the_step_for_every_interrupt_and_keep_the_messages_in_order()
    {
        var steering = CreateSteering();
        var outcomes = new List<Task<SteeringOutcome>>();
        InterruptTestModel? model = null;
        model = new InterruptTestModel
        {
            Generate = (options, token) =>
            {
                if (model!.DoGenerateCalls.Count <= 2)
                {
                    outcomes.Add(
                        steering.SteerAsync(
                            $"Correction {model.DoGenerateCalls.Count}.",
                            new SteerOptions { Interrupt = true }
                        )
                    );
                    return WaitForAbort<LanguageModelGenerateResult>(options, token);
                }

                return Task.FromResult(TextResponse("It is sunny in Paris, Texas."));
            },
        };

        var result = await GenerateAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = PromptText,
                ExperimentalSteering = steering,
            }
        );

        Assert.Equal(3, model.DoGenerateCalls.Count);
        Assert.Single(result.Steps);
        Assert.Equal(
            ["Correction 1.", "Correction 2."],
            result.Steps[0].ExperimentalSteeringMessages!.Select(Text)
        );
        foreach (var outcome in await Task.WhenAll(outcomes))
        {
            SteeringAssert.Delivered(outcome, 0);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should redo a later step with the tool results of the step before it",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_redo_a_later_step_with_the_tool_results_of_the_step_before_it()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var executions = 0;
        InterruptTestModel? model = null;
        model = new InterruptTestModel
        {
            Generate = (options, token) =>
            {
                switch (model!.DoGenerateCalls.Count)
                {
                    case 1:
                        return Task.FromResult(ToolCallResponse());
                    case 2:
                        outcome = steering.SteerAsync(
                            "Answer in Celsius.",
                            new SteerOptions { Interrupt = true }
                        );
                        return WaitForAbort<LanguageModelGenerateResult>(options, token);
                    default:
                        return Task.FromResult(TextResponse("It is 22 degrees in Paris."));
                }
            },
        };

        var result = await GenerateAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = PromptText,
                Tools =
                [
                    Weather(() =>
                    {
                        executions++;
                        return Task.FromResult("{\"temperature\":72}");
                    }),
                ],
                StopWhen = StopWhen.IsStepCount(3),
                ExperimentalSteering = steering,
            }
        );

        Assert.Equal(3, model.DoGenerateCalls.Count);
        Assert.Equal(1, executions);
        Assert.Equal(
            ["user", "assistant", "tool", "user"],
            model.DoGenerateCalls[2].Prompt.Select(message => message.Role)
        );
        Assert.Equal("Answer in Celsius.", LastUserText(model.DoGenerateCalls[2]));
        Assert.Equal(2, result.Steps.Count);
        SteeringAssert.Delivered(await outcome!, 1);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should run prepareStep again for the redo, notify onStepStart once, and start two model calls",
        Coverage = UpstreamCoverage.Partial,
        Note = "GenerateTextOptions has no OnStepStart, so the once-per-step notification is not checked.",
        Commit = Commit
    )]
    public async Task Should_run_prepareStep_again_for_the_redo_and_start_two_model_calls()
    {
        var steering = CreateSteering();
        var prepareStepContexts = new List<PrepareStepContext>();
        var callStarts = 0;
        var callEnds = 0;
        var model = InterruptTestModel.HangingFirstCall(
            options =>
                _ = steering.SteerAsync(
                    "Use Paris, Texas instead.",
                    new SteerOptions { Interrupt = true }
                ),
            _ => TextResponse("It is sunny in Paris, Texas.")
        );

        await GenerateAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = PromptText,
                ExperimentalSteering = steering,
                PrepareStep = context =>
                {
                    prepareStepContexts.Add(context);
                    return null;
                },
                OnLanguageModelCallStart = (_, _) =>
                {
                    callStarts++;
                    return Task.CompletedTask;
                },
                OnLanguageModelCallEnd = (_, _) =>
                {
                    callEnds++;
                    return Task.CompletedTask;
                },
            }
        );

        Assert.Equal(2, prepareStepContexts.Count);
        Assert.Equal(0, prepareStepContexts[1].StepNumber);
        Assert.Equal(
            "Use Paris, Texas instead.",
            Text(Assert.Single(prepareStepContexts[1].ExperimentalSteeringMessages!))
        );
        Assert.Equal(2, callStarts);
        Assert.Equal(1, callEnds);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should run prepareStep for the redo with the instructions and contexts that the step started with",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_run_prepareStep_for_the_redo_with_the_instructions_and_contexts_that_the_step_started_with()
    {
        var steering = CreateSteering();
        var prepareStepInputs = new List<(string? Instructions, string Runtime, string Tools)>();
        var model = InterruptTestModel.HangingFirstCall(
            options =>
                _ = steering.SteerAsync(
                    "Use Paris, Texas instead.",
                    new SteerOptions { Interrupt = true }
                ),
            _ => TextResponse("It is sunny in Paris, Texas.")
        );

        await GenerateAsync(
            new GenerateTextOptions
            {
                Model = model,
                Instructions = Instructions.From("Answer briefly."),
                Prompt = PromptText,
                Tools = [UnitWeather()],
                RuntimeContext = new Dictionary<string, object?> { ["prepareStepCalls"] = 0 },
                ToolsContext = UnitContext("fahrenheit"),
                ExperimentalSteering = steering,
                PrepareStep = context =>
                {
                    prepareStepInputs.Add(
                        (
                            context.Instructions?.Text,
                            JsonSerializer.Serialize(context.RuntimeContext),
                            JsonSerializer.Serialize(context.ToolsContext)
                        )
                    );
                    return new PrepareStepUpdate
                    {
                        Instructions = Instructions.From(
                            context.Instructions?.Text + " Use Celsius."
                        ),
                        RuntimeContext = new Dictionary<string, object?>
                        {
                            ["prepareStepCalls"] = (int)context.RuntimeContext["prepareStepCalls"]! + 1,
                        },
                        ToolsContext = UnitContext("celsius"),
                    };
                },
            }
        );

        var stepStartState = (
            "Answer briefly.",
            "{\"prepareStepCalls\":0}",
            "{\"weather\":{\"unit\":\"fahrenheit\"}}"
        );
        Assert.Equal([stepStartState, stepStartState], prepareStepInputs);
        var system = Assert.IsType<SystemModelMessage>(model.DoGenerateCalls[1].Prompt[0]);
        Assert.Equal("Answer briefly. Use Celsius.", system.Content);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not deliver an interrupt message that prepareStep removes on the redo",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_deliver_an_interrupt_message_that_prepareStep_removes_on_the_redo()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var model = InterruptTestModel.HangingFirstCall(
            _ =>
                outcome = steering.SteerAsync(
                    "Use Paris, Texas instead.",
                    new SteerOptions { Interrupt = true }
                ),
            _ => TextResponse("It is sunny in Paris.")
        );

        await GenerateAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = PromptText,
                ExperimentalSteering = steering,
                PrepareStep = context => new PrepareStepUpdate
                {
                    Messages = context
                        .Messages.Where(message =>
                            message is not UserModelMessage user
                            || Text(user) != "Use Paris, Texas instead."
                        )
                        .ToArray(),
                },
            }
        );

        Assert.Equal(2, model.DoGenerateCalls.Count);
        Assert.Equal(PromptText, LastUserText(model.DoGenerateCalls[1]));
        SteeringAssert.NotDelivered(
            await outcome!,
            SteeringNotDeliveredReason.RemovedByPrepareStep
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should interrupt for a message sent during prepareStep",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_interrupt_for_a_message_sent_during_prepareStep()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var model = InterruptTestModel.HangingFirstCall(
            _ => { },
            _ => TextResponse("It is sunny in Paris, Texas.")
        );

        await GenerateAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = PromptText,
                ExperimentalSteering = steering,
                PrepareStep = _ =>
                {
                    outcome ??= steering.SteerAsync(
                        "Use Paris, Texas instead.",
                        new SteerOptions { Interrupt = true }
                    );
                    return null;
                },
            }
        );

        Assert.Equal(2, model.DoGenerateCalls.Count);
        Assert.Equal("Use Paris, Texas instead.", LastUserText(model.DoGenerateCalls[1]));
        SteeringAssert.Delivered(await outcome!, 0);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should deliver an interrupt message sent before the run in the first step without an extra call",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_deliver_an_interrupt_message_sent_before_the_run_in_the_first_step_without_an_extra_call()
    {
        var steering = CreateSteering();
        var outcome = steering.SteerAsync(
            "Use Paris, Texas instead.",
            new SteerOptions { Interrupt = true }
        );
        var model = new InterruptTestModel
        {
            Generate = (_, _) => Task.FromResult(TextResponse("Sunny.")),
        };

        await GenerateAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = PromptText,
                ExperimentalSteering = steering,
            }
        );

        Assert.Single(model.DoGenerateCalls);
        SteeringAssert.Delivered(await outcome, 0);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should fall back to the next step and let tools finish when the model call already returned",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_fall_back_to_the_next_step_and_let_tools_finish_when_the_model_call_already_returned()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var executions = 0;
        InterruptTestModel? model = null;
        model = new InterruptTestModel
        {
            Generate = (options, _) =>
                Task.FromResult(
                    model!.DoGenerateCalls.Count == 1
                        ? ToolCallResponse()
                        : TextResponse($"answer for {LastUserText(options)}")
                ),
        };

        var result = await GenerateAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = PromptText,
                Tools =
                [
                    Weather(async () =>
                    {
                        executions++;
                        outcome = steering.SteerAsync(
                            "Use Paris, Texas instead.",
                            new SteerOptions { Interrupt = true }
                        );
                        await Task.Delay(5);
                        return "{\"temperature\":72}";
                    }),
                ],
                StopWhen = StopWhen.IsStepCount(3),
                ExperimentalSteering = steering,
            }
        );

        Assert.Equal(1, executions);
        Assert.Equal("{\"temperature\":72}", result.Steps[0].ToolResults[0].OutputJson);
        Assert.Equal(2, model.DoGenerateCalls.Count);
        SteeringAssert.Delivered(await outcome!, 1);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not cancel the model call for a steer without interrupt",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_cancel_the_model_call_for_a_steer_without_interrupt()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var cancelledWhileWaiting = false;
        InterruptTestModel? model = null;
        model = new InterruptTestModel
        {
            Generate = async (options, token) =>
            {
                if (model!.DoGenerateCalls.Count == 1)
                {
                    outcome = steering.SteerAsync("Use Paris, Texas instead.");
                    await Task.Delay(5);
                    cancelledWhileWaiting =
                        options.AbortSignal?.Aborted == true || token.IsCancellationRequested;
                    return TextResponse("It is sunny in Paris.");
                }

                return TextResponse("It is sunny in Paris, Texas.");
            },
        };

        var result = await GenerateAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = PromptText,
                StopWhen = StopWhen.IsStepCount(3),
                ExperimentalSteering = steering,
            }
        );

        Assert.False(cancelledWhileWaiting);
        Assert.Equal(2, result.Steps.Count);
        SteeringAssert.Delivered(await outcome!, 1);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should treat a caller abort as an abort and not as an interrupt",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_treat_a_caller_abort_as_an_abort_and_not_as_an_interrupt()
    {
        var steering = CreateSteering();
        using var controller = new CancellationTokenSource();
        Task<SteeringOutcome>? outcome = null;
        var model = new InterruptTestModel
        {
            Generate = (options, token) =>
            {
                outcome = steering.SteerAsync(
                    "Use Paris, Texas instead.",
                    new SteerOptions { Interrupt = true }
                );
                controller.Cancel();
                return WaitForAbort<LanguageModelGenerateResult>(options, token);
            },
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            GenerateAsync(
                new GenerateTextOptions
                {
                    Model = model,
                    Prompt = PromptText,
                    ExperimentalSteering = steering,
                },
                controller.Token
            )
        );

        Assert.Single(model.DoGenerateCalls);
        SteeringAssert.NotDelivered(await outcome!, SteeringNotDeliveredReason.Aborted);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should only interrupt once for a message sent again with the same id",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_only_interrupt_once_for_a_message_sent_again_with_the_same_id()
    {
        var steering = CreateSteering();
        var model = InterruptTestModel.HangingFirstCall(
            options =>
                _ = steering.SteerAsync(
                    "Use Paris, Texas instead.",
                    new SteerOptions { Id = "steer-1", Interrupt = true }
                ),
            options =>
            {
                _ = steering.SteerAsync(
                    "Use Paris, Texas instead.",
                    new SteerOptions { Id = "steer-1", Interrupt = true }
                );
                return TextResponse("It is sunny in Paris, Texas.");
            }
        );

        var result = await GenerateAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = PromptText,
                ExperimentalSteering = steering,
            }
        );

        Assert.Equal(2, model.DoGenerateCalls.Count);
        Assert.Single(result.Steps);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep the step timeout running across the redo",
        Coverage = UpstreamCoverage.Covered,
        Note = "Real timers replace the fake timers: a timeout that started again with the redo would end 600 ms later.",
        Commit = Commit
    )]
    public async Task Should_keep_the_step_timeout_running_across_the_redo()
    {
        var steering = CreateSteering();
        var model = new InterruptTestModel
        {
            Generate = (options, token) =>
                WaitForAbort<LanguageModelGenerateResult>(options, token),
        };
        var stopwatch = Stopwatch.StartNew();

        var run = GenerateAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = PromptText,
                Timeout = new TimeoutConfiguration { StepMs = 1000 },
                ExperimentalSteering = steering,
            }
        );

        await Task.Delay(600);
        _ = steering.SteerAsync("Use Paris, Texas instead.", new SteerOptions { Interrupt = true });

        await Assert.ThrowsAnyAsync<Exception>(() => run);
        stopwatch.Stop();

        Assert.Equal(2, model.DoGenerateCalls.Count);
        // a timeout that started again with the redo would end after about 1600 ms:
        Assert.True(
            stopwatch.ElapsedMilliseconds < 1450,
            $"The step timeout ended after {stopwatch.ElapsedMilliseconds} ms."
        );
    }
}
