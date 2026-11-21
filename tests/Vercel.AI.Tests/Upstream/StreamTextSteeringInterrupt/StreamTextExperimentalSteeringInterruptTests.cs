// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateTextSteeringInterrupt;
using Vercel.AI.Tests.Upstream.Steering;
using Vercel.AI.Ui;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateTextSteeringInterrupt.SteeringInterruptTestSupport;

namespace Vercel.AI.Tests.Upstream.StreamTextSteeringInterrupt;

/// <summary>Port of <c>stream-text-steering-interrupt.test.ts</c> &gt; <c>streamText experimental_steering interrupt</c>.</summary>
public sealed class StreamTextExperimentalSteeringInterruptTests
{
    private const string Commit = "e4039889ba58bbddf965ce61bf8fe6626e9f8800";
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text-steering-interrupt.test.ts::streamText experimental_steering interrupt::";

    private static readonly LanguageModelStreamPart[] StartOnly = [new StreamStartStreamPart([])];

    /// <summary>Maps to <c>createModel</c>: the first call gets <paramref name="first"/>, the next calls get <paramref name="rest"/>.</summary>
    private static InterruptTestModel CreateModel(
        Func<LanguageModelCallOptions, CancellationToken, LanguageModelStreamResult> first,
        Action? onFirstCall = null,
        params Func<IReadOnlyList<LanguageModelStreamPart>>[] rest
    )
    {
        InterruptTestModel? model = null;
        model = new InterruptTestModel
        {
            Stream = (options, token) =>
            {
                if (model!.DoStreamCalls.Count == 1)
                {
                    var stream = first(options, token);
                    onFirstCall?.Invoke();
                    return Task.FromResult(stream);
                }

                return Task.FromResult(ToResult(rest[model.DoStreamCalls.Count - 2]()));
            },
        };
        return model;
    }

    private static LanguageModelStreamResult Hanging(
        LanguageModelCallOptions options,
        CancellationToken token,
        IReadOnlyList<LanguageModelStreamPart>? parts = null,
        Task<IReadOnlyList<LanguageModelStreamPart>>? release = null
    ) => HangingStream(options, token, parts ?? StartOnly, release);

    private static Task<SteeringOutcome> Interrupt(ISteering steering, string? id = null) =>
        steering.SteerAsync(
            "Use Paris, Texas instead.",
            new SteerOptions { Id = id, Interrupt = true }
        );

    private static StreamTextResult Stream(StreamTextOptions options) =>
        Client().StreamTextAsync(options);

    private static async Task<List<TextStreamPart>> CollectAsync(StreamTextResult result)
    {
        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        return parts;
    }

    // Like consumeStream, a failing stream is reported by the steering outcome of the test.
    private static async Task ConsumeAsync(StreamTextResult result)
    {
        try
        {
            await result.ConsumeStreamAsync();
        }
        catch (Exception) { }
    }

    private static async IAsyncEnumerable<JsonObject> ToAsync(IEnumerable<JsonObject> chunks)
    {
        foreach (var chunk in chunks)
        {
            yield return chunk;
            await Task.Yield();
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should cancel a model stream without output and redo it in the same step with the message",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_cancel_a_model_stream_without_output_and_redo_it_in_the_same_step_with_the_message()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var model = CreateModel(
            (options, token) => Hanging(options, token),
            () => outcome = Interrupt(steering),
            () => TextStream("It is sunny in Paris, Texas.")
        );

        var result = Stream(
            new StreamTextOptions
            {
                Model = model,
                Prompt = PromptText,
                ExperimentalSteering = steering,
            }
        );
        var parts = await CollectAsync(result);

        Assert.Equal(2, model.DoStreamCalls.Count);
        Assert.True(model.DoStreamCalls[0].AbortSignal?.Aborted);
        Assert.Equal("Use Paris, Texas instead.", LastUserText(model.DoStreamCalls[1]));
        Assert.Single(parts, part => part.Type == "start-step");
        Assert.Single(parts, part => part.Type == "finish-step");
        Assert.DoesNotContain(parts, part => part.Type == "error");
        var steeringPart = Assert.IsType<SteeringMessagePart>(
            Assert.Single(parts, part => part.Type == "steering-message")
        );
        Assert.Equal("Use Paris, Texas instead.", Text(steeringPart.Message));
        Assert.Equal(0, steeringPart.StepNumber);
        Assert.Equal("It is sunny in Paris, Texas.", await result.Text);
        Assert.Single(await result.Steps);
        // the cancelled stream is left out of the usage:
        Assert.Equal(10, (await result.Usage).OutputTokens);
        SteeringAssert.Delivered(await outcome!, 0);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should close text and reasoning parts the ui saw before the interrupt and keep them as empty ui message parts",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_close_text_and_reasoning_parts_the_ui_saw_before_the_interrupt_and_keep_them_as_empty_ui_message_parts()
    {
        var steering = CreateSteering();
        var model = CreateModel(
            (options, token) =>
                Hanging(
                    options,
                    token,
                    [
                        new StreamStartStreamPart([]),
                        new ReasoningStartStreamPart("0"),
                        new TextStartStreamPart("1"),
                    ]
                ),
            null,
            () => TextStream("It is sunny in Paris, Texas.")
        );

        var result = Stream(
            new StreamTextOptions
            {
                Model = model,
                Prompt = PromptText,
                ExperimentalSteering = steering,
            }
        );
        var chunks = new List<JsonObject>();
        await foreach (var chunk in UIMessageStreamConverter.ToUIMessageStream(result.Stream()))
        {
            chunks.Add(chunk);
            if ((string?)chunk["type"] == "text-start" && model.DoStreamCalls.Count == 1)
            {
                _ = Interrupt(steering, "steer-1");
            }
        }

        Assert.Equal(
            [
                "start",
                "start-step",
                "reasoning-start",
                "text-start",
                "text-end",
                "reasoning-end",
                "steering-message",
                "text-start",
                "text-delta",
                "text-end",
                "finish-step",
                "finish",
            ],
            chunks.Select(chunk => (string?)chunk["type"])
        );

        UIMessage? message = null;
        await foreach (
            var uiMessage in UIMessageStreamConverter.ReadUIMessageStream(
                new ReadUIMessageStreamOptions(ToAsync(chunks))
            )
        )
        {
            message = uiMessage;
        }

        var parts = message!.Parts;
        Assert.Equal(5, parts.Count);
        Assert.IsType<StepStartUIPart>(parts[0]);
        var reasoning = Assert.IsType<ReasoningUIPart>(parts[1]);
        Assert.Equal("", reasoning.Text);
        Assert.Equal("done", reasoning.State);
        var emptyText = Assert.IsType<TextUIPart>(parts[2]);
        Assert.Equal("", emptyText.Text);
        Assert.Equal("done", emptyText.State);
        var steeringPart = Assert.IsType<SteeringMessageUIPart>(parts[3]);
        Assert.Equal("steer-1", steeringPart.Id);
        Assert.Equal(
            "Use Paris, Texas instead.",
            Assert.IsType<TextUIPart>(Assert.Single(steeringPart.Parts)).Text
        );
        var text = Assert.IsType<TextUIPart>(parts[4]);
        Assert.Equal("It is sunny in Paris, Texas.", text.Text);
        Assert.Equal("done", text.State);
        var step = Assert.Single(await result.Steps);
        Assert.Equal(
            "It is sunny in Paris, Texas.",
            Assert.Single(step.Content.OfType<GeneratedText>()).Text
        );
        Assert.Single(step.Content);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should send one start-step when the interrupt arrives after the ui saw the step start",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_one_start_step_when_the_interrupt_arrives_after_the_ui_saw_the_step_start()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var model = CreateModel(
            (options, token) =>
                Hanging(
                    options,
                    token,
                    [new StreamStartStreamPart([]), new TextStartStreamPart("1")]
                ),
            null,
            () => TextStream("It is sunny in Paris, Texas.")
        );

        var result = Stream(
            new StreamTextOptions
            {
                Model = model,
                Prompt = PromptText,
                ExperimentalSteering = steering,
            }
        );
        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
            if (part.Type == "text-start" && outcome == null)
            {
                outcome = Interrupt(steering);
            }
        }

        Assert.Equal(2, model.DoStreamCalls.Count);
        Assert.Equal(
            [
                "start",
                "start-step",
                "text-start",
                "text-end",
                "steering-message",
                "text-start",
                "text-delta",
                "text-end",
                "finish-step",
                "finish",
            ],
            parts.Select(part => part.Type)
        );
        SteeringAssert.Delivered(await outcome!, 0);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should use the request and warnings of the redo when the cancelled call already sent start-step",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_use_the_request_and_warnings_of_the_redo_when_the_cancelled_call_already_sent_start_step()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        InterruptTestModel? model = null;
        model = new InterruptTestModel
        {
            Stream = (options, token) =>
            {
                if (model!.DoStreamCalls.Count == 1)
                {
                    return Task.FromResult(
                        HangingStream(
                            options,
                            token,
                            [
                                new StreamStartStreamPart([
                                    new CallWarning("other", "cancelled call"),
                                ]),
                                new TextStartStreamPart("1"),
                            ],
                            request: LanguageModelStreamRequest.FromJson("\"cancelled request\"")
                        )
                    );
                }

                return Task.FromResult(
                    ToResult(
                        [
                            new StreamStartStreamPart([new CallWarning("other", "redo call")]),
                            .. TextStream("It is sunny in Paris, Texas.").Skip(1),
                        ],
                        LanguageModelStreamRequest.FromJson("\"redo request\"")
                    )
                );
            },
        };

        var result = Stream(
            new StreamTextOptions
            {
                Model = model,
                Prompt = PromptText,
                Include = new StreamTextInclude { RequestBody = true },
                ExperimentalSteering = steering,
            }
        );
        await foreach (var part in result.Stream())
        {
            if (part.Type == "text-start" && outcome == null)
            {
                outcome = Interrupt(steering);
            }
        }

        var step = Assert.Single(await result.Steps);
        Assert.Equal(2, model.DoStreamCalls.Count);
        Assert.Equal("redo request", step.Request.Body!.Value.GetString());
        var warning = Assert.Single(step.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("redo call", warning.Message);
        Assert.Equal(
            "It is sunny in Paris, Texas.",
            Assert.IsType<GeneratedText>(Assert.Single(step.Content)).Text
        );
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
        var model = CreateModel(
            (options, token) => Hanging(options, token),
            () => interrupt = Interrupt(steering),
            () => TextStream("It is 22 degrees in Paris, Texas.")
        );

        var result = Stream(
            new StreamTextOptions
            {
                Model = model,
                Prompt = PromptText,
                ExperimentalSteering = steering,
            }
        );
        await result.ConsumeStreamAsync();

        Assert.Equal(
            ["What is the weather in Paris?", "Answer in Celsius.", "Use Paris, Texas instead."],
            model.DoStreamCalls[1].Prompt.OfType<UserModelMessage>().Select(Text)
        );
        Assert.Equal(
            ["Answer in Celsius.", "Use Paris, Texas instead."],
            (await result.Steps)[0].ExperimentalSteeringMessages!.Select(Text)
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
            Stream = (options, token) =>
            {
                if (model!.DoStreamCalls.Count <= 2)
                {
                    var stream = Hanging(options, token);
                    outcomes.Add(
                        steering.SteerAsync(
                            $"Correction {model.DoStreamCalls.Count}.",
                            new SteerOptions { Interrupt = true }
                        )
                    );
                    return Task.FromResult(stream);
                }

                return Task.FromResult(ToResult(TextStream("It is sunny in Paris, Texas.")));
            },
        };

        var result = Stream(
            new StreamTextOptions
            {
                Model = model,
                Prompt = PromptText,
                ExperimentalSteering = steering,
            }
        );
        var parts = await CollectAsync(result);

        Assert.Equal(3, model.DoStreamCalls.Count);
        Assert.Single(parts, part => part.Type == "start-step");
        Assert.Single(parts, part => part.Type == "finish-step");
        Assert.Single(await result.Steps);
        Assert.Equal(
            ["Correction 1.", "Correction 2."],
            (await result.Steps)[0].ExperimentalSteeringMessages!.Select(Text)
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
            Stream = (options, token) =>
            {
                switch (model!.DoStreamCalls.Count)
                {
                    case 1:
                        return Task.FromResult(
                            ToResult([
                                new StreamStartStreamPart([]),
                                new ToolCallStreamPart(
                                    "call-1",
                                    "weather",
                                    "{ \"city\": \"Paris\" }"
                                ),
                                new FinishStreamPart(
                                    FinishReason.ToolCalls,
                                    UsageOf(5),
                                    "tool_calls"
                                ),
                            ])
                        );
                    case 2:
                        var stream = Hanging(options, token);
                        outcome = steering.SteerAsync(
                            "Answer in Celsius.",
                            new SteerOptions { Interrupt = true }
                        );
                        return Task.FromResult(stream);
                    default:
                        return Task.FromResult(ToResult(TextStream("It is 22 degrees in Paris.")));
                }
            },
        };

        var result = Stream(
            new StreamTextOptions
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
        await result.ConsumeStreamAsync();

        Assert.Equal(3, model.DoStreamCalls.Count);
        Assert.Equal(1, executions);
        Assert.Equal(
            ["user", "assistant", "tool", "user"],
            model.DoStreamCalls[2].Prompt.Select(message => message.Role)
        );
        Assert.Equal("Answer in Celsius.", LastUserText(model.DoStreamCalls[2]));
        Assert.Equal(2, (await result.Steps).Count);
        SteeringAssert.Delivered(await outcome!, 1);
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
        var model = CreateModel(
            (options, token) => Hanging(options, token),
            () => outcome = Interrupt(steering),
            () => TextStream("It is sunny in Paris.")
        );

        var result = Stream(
            new StreamTextOptions
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
        var parts = await CollectAsync(result);

        Assert.Equal(2, model.DoStreamCalls.Count);
        Assert.Equal(PromptText, LastUserText(model.DoStreamCalls[1]));
        Assert.DoesNotContain(parts, part => part.Type == "steering-message");
        SteeringAssert.NotDelivered(
            await outcome!,
            SteeringNotDeliveredReason.RemovedByPrepareStep
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should run prepareStep again for the redo, notify onStepStart once, and start two model calls",
        Coverage = UpstreamCoverage.Partial,
        Note = "StreamTextOptions has no OnStepStart, so the once-per-step notification is not checked.",
        Commit = Commit
    )]
    public async Task Should_run_prepareStep_again_for_the_redo_and_start_two_model_calls()
    {
        var steering = CreateSteering();
        var prepareStepContexts = new List<PrepareStepContext>();
        var callStarts = 0;
        var callEnds = 0;
        var model = CreateModel(
            (options, token) => Hanging(options, token),
            () => _ = Interrupt(steering),
            () => TextStream("It is sunny in Paris, Texas.")
        );

        var result = Stream(
            new StreamTextOptions
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
                    Interlocked.Increment(ref callStarts);
                    return Task.CompletedTask;
                },
                OnLanguageModelCallEnd = (_, _) =>
                {
                    Interlocked.Increment(ref callEnds);
                    return Task.CompletedTask;
                },
            }
        );
        await result.ConsumeStreamAsync();

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
        var model = CreateModel(
            (options, token) => Hanging(options, token),
            () => _ = Interrupt(steering),
            () => TextStream("It is sunny in Paris, Texas.")
        );

        var result = Stream(
            new StreamTextOptions
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
                            ["prepareStepCalls"] =
                                (int)context.RuntimeContext["prepareStepCalls"]! + 1,
                        },
                        ToolsContext = UnitContext("celsius"),
                    };
                },
            }
        );
        await result.ConsumeStreamAsync();

        var stepStartState = (
            "Answer briefly.",
            "{\"prepareStepCalls\":0}",
            "{\"weather\":{\"unit\":\"fahrenheit\"}}"
        );
        Assert.Equal([stepStartState, stepStartState], prepareStepInputs);
        var system = Assert.IsType<SystemModelMessage>(model.DoStreamCalls[1].Prompt[0]);
        Assert.Equal("Answer briefly. Use Celsius.", system.Content);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should fall back to the next step when output already started",
        Coverage = UpstreamCoverage.Covered,
        Note = "The port has no onChunk callback: the test reads the full stream and steers on the first text-delta part.",
        Commit = Commit
    )]
    public async Task Should_fall_back_to_the_next_step_when_output_already_started()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var release = new TaskCompletionSource<IReadOnlyList<LanguageModelStreamPart>>();
        var model = CreateModel(
            (options, token) =>
                Hanging(
                    options,
                    token,
                    [
                        new StreamStartStreamPart([]),
                        new TextStartStreamPart("1"),
                        new TextDeltaStreamPart("1", "It is "),
                    ],
                    release.Task
                ),
            null,
            () => TextStream("It is sunny in Paris, Texas.")
        );

        var result = Stream(
            new StreamTextOptions
            {
                Model = model,
                Prompt = PromptText,
                StopWhen = StopWhen.IsStepCount(3),
                ExperimentalSteering = steering,
            }
        );
        await foreach (var part in result.Stream())
        {
            if (part.Type == "text-delta" && outcome == null)
            {
                outcome = Interrupt(steering);
                release.SetResult([
                    new TextDeltaStreamPart("1", "sunny in Paris."),
                    new TextEndStreamPart("1"),
                    new FinishStreamPart(FinishReason.Stop, UsageOf(10), "stop"),
                ]);
            }
        }

        Assert.False(model.DoStreamCalls[0].AbortSignal?.Aborted ?? false);
        Assert.Equal(2, (await result.Steps).Count);
        SteeringAssert.Delivered(await outcome!, 1);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should fall back to the next step after a tool input started",
        Coverage = UpstreamCoverage.Covered,
        Note = "The port has no onChunk callback: the test reads the full stream and steers on the first tool-input-start part.",
        Commit = Commit
    )]
    public async Task Should_fall_back_to_the_next_step_after_a_tool_input_started()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var release = new TaskCompletionSource<IReadOnlyList<LanguageModelStreamPart>>();
        var model = CreateModel(
            (options, token) =>
                Hanging(
                    options,
                    token,
                    [
                        new StreamStartStreamPart([]),
                        new ToolInputStartStreamPart("call-1", "weather"),
                    ],
                    release.Task
                ),
            null,
            () => TextStream("It is sunny in Paris, Texas.")
        );

        var result = Stream(
            new StreamTextOptions
            {
                Model = model,
                Prompt = PromptText,
                StopWhen = StopWhen.IsStepCount(3),
                ExperimentalSteering = steering,
            }
        );
        await foreach (var part in result.Stream())
        {
            if (part.Type == "tool-input-start" && outcome == null)
            {
                outcome = Interrupt(steering);
                release.SetResult([
                    new ToolInputEndStreamPart("call-1"),
                    new FinishStreamPart(FinishReason.Stop, UsageOf(10), "stop"),
                ]);
            }
        }

        Assert.False(model.DoStreamCalls[0].AbortSignal?.Aborted ?? false);
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
        var controller = new AbortController();
        Task<SteeringOutcome>? outcome = null;
        var model = CreateModel(
            (options, token) => Hanging(options, token),
            () =>
            {
                outcome = Interrupt(steering);
                controller.Abort(new InvalidOperationException("stopped by user"));
            },
            () => TextStream("It is sunny in Paris, Texas.")
        );

        var result = Stream(
            new StreamTextOptions
            {
                Model = model,
                Prompt = PromptText,
                AbortSignal = controller.Signal,
                ExperimentalSteering = steering,
            }
        );
        await ConsumeAsync(result);

        Assert.Single(model.DoStreamCalls);
        SteeringAssert.NotDelivered(await outcome!, SteeringNotDeliveredReason.Aborted);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not cancel the model stream for a steer without interrupt",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_cancel_the_model_stream_for_a_steer_without_interrupt()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var release = new TaskCompletionSource<IReadOnlyList<LanguageModelStreamPart>>();
        var model = CreateModel(
            (options, token) => Hanging(options, token, StartOnly, release.Task),
            () =>
            {
                outcome = steering.SteerAsync("Use Paris, Texas instead.");
                _ = Task.Delay(5)
                    .ContinueWith(
                        _ =>
                            release.SetResult([
                                new TextStartStreamPart("1"),
                                new TextDeltaStreamPart("1", "Sunny."),
                                new TextEndStreamPart("1"),
                                new FinishStreamPart(FinishReason.Stop, UsageOf(10), "stop"),
                            ]),
                        TaskScheduler.Default
                    );
            },
            () => TextStream("It is sunny in Paris, Texas.")
        );

        var result = Stream(
            new StreamTextOptions
            {
                Model = model,
                Prompt = PromptText,
                StopWhen = StopWhen.IsStepCount(3),
                ExperimentalSteering = steering,
            }
        );
        await result.ConsumeStreamAsync();

        Assert.False(model.DoStreamCalls[0].AbortSignal?.Aborted ?? false);
        Assert.Equal(2, (await result.Steps).Count);
        SteeringAssert.Delivered(await outcome!, 1);
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
            Stream = (options, token) => Task.FromResult(Hanging(options, token)),
        };
        var stopwatch = Stopwatch.StartNew();

        var result = Stream(
            new StreamTextOptions
            {
                Model = model,
                Prompt = PromptText,
                Timeout = new TimeoutConfiguration { StepMs = 1000 },
                ExperimentalSteering = steering,
            }
        );
        var run = ConsumeAsync(result);

        await Task.Delay(600);
        _ = Interrupt(steering);

        await run;
        stopwatch.Stop();

        Assert.Equal(2, model.DoStreamCalls.Count);
        // a timeout that started again with the redo would end after about 1600 ms:
        Assert.True(
            stopwatch.ElapsedMilliseconds < 1450,
            $"The step timeout ended after {stopwatch.ElapsedMilliseconds} ms."
        );
    }
}
