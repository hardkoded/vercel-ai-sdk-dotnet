// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Tests.Upstream.Telemetry;
using static Vercel.AI.Tests.Upstream.GenerateText.ResultToolCallsTests;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText</c> &gt; <c>options.onStepStart</c>.</summary>
public sealed class OptionsOnStepStartTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.onStepStart::";

    [Fact]
    [UpstreamTest(
        Prefix + "should be called with correct data for a single step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_called_with_correct_data_for_a_single_step()
    {
        GenerateTextStepStartEvent? stepStartEvent = null;

        await ToolExecutionCallbackSupport
            .Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => TestLanguageModel.Text("Hello!"),
                    },
                    Prompt = "test-input",
                    ToolOrder = ["b", "a"],
                    OnStepStart = (@event, _) =>
                    {
                        stepStartEvent = @event;
                        return Task.CompletedTask;
                    },
                }
            );

        Assert.Equal(0, stepStartEvent!.StepNumber);
        Assert.Empty(stepStartEvent.Steps);
        Assert.Equal("test", stepStartEvent.Provider);
        Assert.Equal("test", stepStartEvent.ModelId);
        var message = Assert.IsType<UserModelMessage>(Assert.Single(stepStartEvent.Messages));
        Assert.Equal(
            "test-input",
            Assert.IsType<TextContentPart>(Assert.Single(message.Content)).Text
        );
        Assert.Equal(new[] { "b", "a" }, stepStartEvent.ToolOrder);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should be called once per step in a multi-step tool loop",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_called_once_per_step_in_a_multi_step_tool_loop()
    {
        var events = new List<GenerateTextStepStartEvent>();
        var responseCount = 0;

        await ToolExecutionCallbackSupport
            .Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ =>
                            responseCount++ == 0
                                ? ToolCallResponse("{ \"value\": \"test\" }")
                                : TestLanguageModel.Text("Final answer."),
                    },
                    Tools = new[] { Tool1() },
                    Prompt = "test-input",
                    StopWhen = StopWhen.IsStepCount(3),
                    OnStepStart = (@event, _) =>
                    {
                        events.Add(@event);
                        return Task.CompletedTask;
                    },
                }
            );

        Assert.Equal(2, events.Count);
        Assert.Equal(0, events[0].StepNumber);
        Assert.Empty(events[0].Steps);
        Assert.Equal(1, events[1].StepNumber);
        Assert.Single(events[1].Steps);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should be called before doGenerate on each step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_called_before_doGenerate_on_each_step()
    {
        var callOrder = new List<string>();

        await ToolExecutionCallbackSupport
            .Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ =>
                        {
                            callOrder.Add("doGenerate");
                            return TestLanguageModel.Text("Hello!");
                        },
                    },
                    Prompt = "test-input",
                    OnStepStart = (_, _) =>
                    {
                        callOrder.Add("onStepStart");
                        return Task.CompletedTask;
                    },
                }
            );

        Assert.Equal(new[] { "onStepStart", "doGenerate" }, callOrder);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not break generation when callback throws",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_break_generation_when_callback_throws()
    {
        var result = await ToolExecutionCallbackSupport
            .Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => TestLanguageModel.Text("Hello, World!"),
                    },
                    Prompt = "test-input",
                    OnStepStart = (_, _) => throw new InvalidOperationException("callback error"),
                }
            );

        Assert.Equal("Hello, World!", result.Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reflect model changes from prepareStep",
        Coverage = UpstreamCoverage.Partial,
        Note = "TestLanguageModel always reports the provider test, so only the model id change is checked."
    )]
    public async Task Should_reflect_model_changes_from_prepareStep()
    {
        var events = new List<GenerateTextStepStartEvent>();
        var alternate = new TestLanguageModel("alternate-model-id")
        {
            OnGenerate = _ => TestLanguageModel.Text("Final answer."),
        };

        await ToolExecutionCallbackSupport
            .Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => ToolCallResponse("{ \"value\": \"test\" }"),
                    },
                    Tools = new[] { Tool1() },
                    Prompt = "test-input",
                    StopWhen = StopWhen.IsStepCount(3),
                    PrepareStep = context =>
                        context.StepNumber == 1
                            ? new PrepareStepUpdate { Model = alternate }
                            : null,
                    OnStepStart = (@event, _) =>
                    {
                        events.Add(@event);
                        return Task.CompletedTask;
                    },
                }
            );

        Assert.Equal("test", events[0].ModelId);
        Assert.Equal("alternate-model-id", events[1].ModelId);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should apply prepareStep model call settings only to the current step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_apply_prepareStep_model_call_settings_only_to_the_current_step()
    {
        var modelCallOptions = new List<LanguageModelCallOptions>();
        var modelCallStartEvents = new List<LanguageModelCallStartEvent>();
        var telemetryModelCallStartEvents = new List<LanguageModelCallStartEvent>();
        var responseCount = 0;

        await ToolExecutionCallbackSupport
            .Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = options =>
                        {
                            modelCallOptions.Add(options);
                            return responseCount++ < 2
                                ? ToolCallResponse("{ \"value\": \"test\" }")
                                : TestLanguageModel.Text("Final answer.");
                        },
                    },
                    Tools = new[] { Tool1() },
                    Prompt = "test-input",
                    StopWhen = StopWhen.IsStepCount(3),
                    MaxOutputTokens = 100,
                    Temperature = 1,
                    TopP = 0.9,
                    TopK = 40,
                    PresencePenalty = 0.4,
                    FrequencyPenalty = 0.3,
                    StopSequences = ["outer"],
                    Seed = 123,
                    Reasoning = "high",
                    PrepareStep = context =>
                        context.StepNumber == 1
                            ? new PrepareStepUpdate
                            {
                                MaxOutputTokens = 50,
                                Temperature = 0,
                                TopP = 0.5,
                                TopK = 10,
                                PresencePenalty = 0,
                                FrequencyPenalty = -0.2,
                                StopSequences = [],
                                Seed = 0,
                                Reasoning = "provider-default",
                            }
                        : context.StepNumber == 2 ? new PrepareStepUpdate { Temperature = null }
                        : null,
                    OnLanguageModelCallStart = (@event, _) =>
                    {
                        modelCallStartEvents.Add(@event);
                        return Task.CompletedTask;
                    },
                    Telemetry = new TelemetryOptions
                    {
                        IsEnabled = true,
                        Integrations =
                        [
                            new TestTelemetry
                            {
                                OnLanguageModelCallStart = (@event, _) =>
                                {
                                    telemetryModelCallStartEvents.Add(@event);
                                    return Task.CompletedTask;
                                },
                            },
                        ],
                    },
                }
            );

        static object?[] Settings(LanguageModelCallOptions options) =>
            [
                options.MaxOutputTokens,
                options.Temperature,
                options.TopP,
                options.TopK,
                options.PresencePenalty,
                options.FrequencyPenalty,
                string.Join(",", options.StopSequences ?? []),
                options.Seed,
                options.Reasoning,
            ];

        object?[] outerSettings = [100, 1.0, 0.9, 40, 0.4, 0.3, "outer", 123, "high"];
        object?[] stepSettings = [50, 0.0, 0.5, 10, 0.0, -0.2, "", 0, "provider-default"];
        object?[][] expected = [outerSettings, stepSettings, outerSettings];

        Assert.Equal(expected, modelCallOptions.Select(Settings));
        Assert.Equal(expected, modelCallStartEvents.Select(e => Settings(e.CallOptions)));
        Assert.Equal(expected, telemetryModelCallStartEvents.Select(e => Settings(e.CallOptions)));
    }
}
