// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;

namespace Vercel.AI.Tests.Upstream.Telemetry.CreateTelemetryDispatcher;

/// <summary>Port of <c>create-telemetry-dispatcher.test.ts</c> &gt; <c>createTelemetryDispatcher</c>. The port dispatches <c>onLanguageModelCallStart</c> where upstream's test uses <c>onStart</c>, which the port has not ported.</summary>
[Collection("LogWarnings")]
public sealed class CreateTelemetryDispatcherTests
{
    private const string Prefix =
        "packages/ai/src/telemetry/create-telemetry-dispatcher.test.ts::createTelemetryDispatcher::";

    [Fact]
    [UpstreamTest(
        Prefix + "adds the emitting SDK version without modifying the original event",
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Adds_the_emitting_SDK_version_without_modifying_the_original_event()
    {
        LanguageModelCallStartEvent? delivered = null;
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            new TelemetryOptions
            {
                Integrations =
                [
                    new TestTelemetry
                    {
                        OnLanguageModelCallStart = (e, _) =>
                        {
                            delivered = e;
                            return Task.CompletedTask;
                        },
                    },
                ],
            }
        );
        var original = new LanguageModelCallStartEvent { AiVersion = "other-version" };

        await telemetry.OnLanguageModelCallStart!(original, CancellationToken.None);

        Assert.Equal(Vercel.AI.Operations.AiSdkVersion.Version, delivered!.AiVersion);
        Assert.Null(delivered.RecordInputs);
        Assert.Null(delivered.RecordOutputs);
        Assert.Null(delivered.FunctionId);
        Assert.Equal("other-version", original.AiVersion);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "omits listeners when no integrations are configured",
        Note = "Passes an empty integrations list instead of no options, so a globally registered integration cannot reach the test."
    )]
    public async Task Omits_listeners_when_no_integrations_are_configured()
    {
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            new TelemetryOptions { Integrations = [] }
        );

        Assert.Null(telemetry.OnStart);
        Assert.Null(telemetry.OnStepStart);
        Assert.Null(telemetry.OnLanguageModelCallStart);
        Assert.Null(telemetry.OnLanguageModelCallEnd);
        Assert.Null(telemetry.OnToolExecutionStart);
        Assert.Null(telemetry.OnToolExecutionEnd);
        Assert.Null(telemetry.OnStepEnd);
        Assert.Null(telemetry.OnObjectStepStart);
        Assert.Null(telemetry.OnObjectStepEnd);
        Assert.Null(telemetry.OnEmbedStart);
        Assert.Null(telemetry.OnEmbedEnd);
        Assert.Null(telemetry.OnRerankStart);
        Assert.Null(telemetry.OnRerankEnd);
        Assert.Null(telemetry.OnDecideStart);
        Assert.Null(telemetry.OnDecisionModelCallStart);
        Assert.Null(telemetry.OnDecisionModelCallEnd);
        Assert.Null(telemetry.OnDecideEnd);
        Assert.Null(telemetry.OnExtractStart);
        Assert.Null(telemetry.OnExtractionModelCallStart);
        Assert.Null(telemetry.OnExtractionModelCallEnd);
        Assert.Null(telemetry.OnExtractEnd);
        Assert.Null(telemetry.OnStreamTranscriptionStart);
        Assert.Null(telemetry.OnStreamTranscriptionEnd);
        Assert.Null(telemetry.OnEnd);
        Assert.Null(telemetry.OnAbort);
        Assert.Null(telemetry.OnError);

        Assert.Equal(
            "result",
            await telemetry.ExecuteLanguageModelCallAsync(
                DispatcherEvents.LanguageModelCallStart(),
                () => Task.FromResult("result"),
                CancellationToken.None
            )
        );
        Assert.Equal(
            "result",
            await telemetry.ExecuteToolAsync(
                new ToolExecutionStartEvent(DispatcherEvents.ToolCall(), []) { CallId = "call-1" },
                () => Task.FromResult("result"),
                CancellationToken.None
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "accepts a single integration",
        Note = "Dispatches onLanguageModelCallStart, which carries the augmented fields, where upstream's test uses onStart."
    )]
    public async Task Accepts_a_single_integration()
    {
        var calls = new List<LanguageModelCallStartEvent>();
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(Options(OnCallStart(calls)));

        await telemetry.OnLanguageModelCallStart!(
            DispatcherEvents.LanguageModelCallStart(),
            CancellationToken.None
        );

        var delivered = Assert.Single(calls);
        Assert.Equal(Vercel.AI.Operations.AiSdkVersion.Version, delivered.AiVersion);
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts an array of integrations")]
    public void Accepts_an_array_of_integrations()
    {
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            Options(
                new TestTelemetry { OnStart = (_, _) => Task.CompletedTask },
                new TestTelemetry { OnEnd = (_, _) => Task.CompletedTask }
            )
        );

        Assert.NotNull(telemetry.OnStart);
        Assert.NotNull(telemetry.OnEnd);
    }

    [Fact]
    [UpstreamTest(Prefix + "omits listeners for methods that no integration implements")]
    public void Omits_listeners_for_methods_that_no_integration_implements()
    {
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            Options(new TestTelemetry { OnStart = (_, _) => Task.CompletedTask })
        );

        Assert.Null(telemetry.OnToolExecutionStart);
        Assert.Null(telemetry.OnEmbedEnd);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "broadcasts an event to all integrations that implement the method",
        Note = "Dispatches onLanguageModelCallStart, which carries the augmented fields, where upstream's test uses onStart."
    )]
    public async Task Broadcasts_an_event_to_all_integrations_that_implement_the_method()
    {
        var first = new List<LanguageModelCallStartEvent>();
        var second = new List<LanguageModelCallStartEvent>();
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            Options(OnCallStart(first), OnCallStart(second))
        );

        await telemetry.OnLanguageModelCallStart!(
            DispatcherEvents.LanguageModelCallStart(),
            CancellationToken.None
        );

        Assert.Equal(Vercel.AI.Operations.AiSdkVersion.Version, Assert.Single(first).AiVersion);
        Assert.Equal(Vercel.AI.Operations.AiSdkVersion.Version, Assert.Single(second).AiVersion);
    }

    [Fact]
    [UpstreamTest(Prefix + "fans out step-end events to deprecated onStepFinish integrations")]
    public async Task Fans_out_step_end_events_to_deprecated_onStepFinish_integrations()
    {
        var stepEnd = 0;
        var stepFinish = 0;
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            Options(
                new TestTelemetry
                {
                    OnStepEnd = (_, _) =>
                    {
                        stepEnd++;
                        return Task.CompletedTask;
                    },
                },
                new TestTelemetry
                {
                    OnStepFinish = (_, _) =>
                    {
                        stepFinish++;
                        return Task.CompletedTask;
                    },
                }
            )
        );

        await telemetry.OnStepEnd!(DispatcherEvents.Step(), CancellationToken.None);

        Assert.Equal(1, stepEnd);
        Assert.Equal(1, stepFinish);
    }

    [Fact]
    [UpstreamTest(Prefix + "skips integrations that do not implement the method")]
    public async Task Skips_integrations_that_do_not_implement_the_method()
    {
        var calls = 0;
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            Options(
                new TestTelemetry
                {
                    OnStart = (_, _) =>
                    {
                        calls++;
                        return Task.CompletedTask;
                    },
                },
                new TestTelemetry()
            )
        );

        await telemetry.OnStart!(new GenerateTextStartEvent(), CancellationToken.None);

        Assert.Equal(1, calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "swallows async errors from individual integrations without affecting others"
    )]
    public async Task Swallows_async_errors_from_individual_integrations_without_affecting_others()
    {
        var first = 0;
        var second = 0;
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            Options(
                new TestTelemetry
                {
                    OnStart = async (_, _) =>
                    {
                        first++;
                        await Task.Yield();
                        throw new InvalidOperationException("boom");
                    },
                },
                new TestTelemetry
                {
                    OnStart = (_, _) =>
                    {
                        second++;
                        return Task.CompletedTask;
                    },
                }
            )
        );

        await telemetry.OnStart!(new GenerateTextStartEvent(), CancellationToken.None);

        Assert.Equal(1, first);
        Assert.Equal(1, second);
    }

    [Fact]
    [UpstreamTest(Prefix + "swallows sync errors thrown by integrations")]
    public async Task Swallows_sync_errors_thrown_by_integrations()
    {
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            Options(
                new TestTelemetry
                {
                    OnStart = (_, _) => throw new InvalidOperationException("sync boom"),
                }
            )
        );

        await telemetry.OnStart!(new GenerateTextStartEvent(), CancellationToken.None);
    }

    [Fact]
    [UpstreamTest(Prefix + "works with all lifecycle methods")]
    public async Task Works_with_all_lifecycle_methods()
    {
        var calls = new List<string>();
        Func<T, CancellationToken, Task> Record<T>(string name) =>
            (_, _) =>
            {
                calls.Add(name);
                return Task.CompletedTask;
            };

        var integration = new TestTelemetry
        {
            OnStart = Record<IOperationStartEvent>("onStart"),
            OnStepStart = Record<GenerateTextStepStartEvent>("onStepStart"),
            OnLanguageModelCallStart = Record<LanguageModelCallStartEvent>(
                "onLanguageModelCallStart"
            ),
            OnLanguageModelCallEnd = Record<LanguageModelCallEndEvent>("onLanguageModelCallEnd"),
            OnToolExecutionStart = Record<ToolExecutionStartEvent>("onToolExecutionStart"),
            OnToolExecutionEnd = Record<ToolExecutionEndEvent>("onToolExecutionEnd"),
            OnStepEnd = Record<StepResult>("onStepEnd"),
            OnObjectStepStart = Record<GenerateObjectStepStartEvent>("onObjectStepStart"),
            OnObjectStepEnd = Record<GenerateObjectStepEndEvent>("onObjectStepEnd"),
            OnEmbedStart = Record<EmbeddingModelCallStartEvent>("onEmbedStart"),
            OnEmbedEnd = Record<EmbeddingModelCallEndEvent>("onEmbedEnd"),
            OnRerankStart = Record<RerankingModelCallStartEvent>("onRerankStart"),
            OnRerankEnd = Record<RerankingModelCallEndEvent>("onRerankEnd"),
            OnDecideStart = Record<DecideEvent>("experimental_onDecideStart"),
            OnDecisionModelCallStart = Record<DecisionModelEvent>(
                "experimental_onDecisionModelCallStart"
            ),
            OnDecisionModelCallEnd = Record<DecisionModelEvent>(
                "experimental_onDecisionModelCallEnd"
            ),
            OnDecideEnd = Record<DecideEvent>("experimental_onDecideEnd"),
            OnExtractStart = Record<ExtractStartEvent>("experimental_onExtractStart"),
            OnExtractionModelCallStart = Record<ExtractionModelCallStartEvent>(
                "experimental_onExtractionModelCallStart"
            ),
            OnExtractionModelCallEnd = Record<ExtractionModelCallEndEvent>(
                "experimental_onExtractionModelCallEnd"
            ),
            OnExtractEnd = Record<ExtractEndEvent>("experimental_onExtractEnd"),
            OnStreamTranscriptionStart = Record<StreamTranscriptionStartEvent>(
                "experimental_onStreamTranscriptionStart"
            ),
            OnStreamTranscriptionEnd = Record<StreamTranscriptionEndEvent>(
                "experimental_onStreamTranscriptionEnd"
            ),
            OnEnd = Record<IOperationEndEvent>("onEnd"),
            OnAbort = Record<StreamAbortContext>("onAbort"),
            OnError = Record<TelemetryErrorEvent>("onError"),
            OnStepError = Record<GenerateTextStepErrorEvent>("onStepError"),
        };
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(Options(integration));
        var ct = CancellationToken.None;
        var error = new InvalidOperationException("failed");
        var toolCall = DispatcherEvents.ToolCall();
        var decide = DispatcherEvents.Decide();
        var extractStart = DispatcherEvents.ExtractStart();
        var extractionModelCallStart = DispatcherEvents.ExtractionModelCallStart();

        await telemetry.OnStart!(new GenerateTextStartEvent(), ct);
        await telemetry.OnStepStart!(new GenerateTextStepStartEvent(), ct);
        await telemetry.OnLanguageModelCallStart!(DispatcherEvents.LanguageModelCallStart(), ct);
        await telemetry.OnLanguageModelCallEnd!(
            new LanguageModelCallEndEvent(
                "provider",
                "model",
                FinishReason.Stop,
                LanguageModelUsage.Null,
                [],
                null,
                null,
                TimeSpan.Zero
            ),
            ct
        );
        await telemetry.OnToolExecutionStart!(new ToolExecutionStartEvent(toolCall, []), ct);
        await telemetry.OnToolExecutionEnd!(
            new ToolExecutionEndEvent(
                toolCall,
                [],
                new ExecutedTool("tool-1", "weather", "null", false),
                TimeSpan.Zero
            ),
            ct
        );
        await telemetry.OnStepEnd!(DispatcherEvents.Step(), ct);
        await telemetry.OnObjectStepStart!(
            new GenerateObjectStepStartEvent("call-1", "provider", "model"),
            ct
        );
        await telemetry.OnObjectStepEnd!(
            new GenerateObjectStepEndEvent("call-1", "provider", "model", DispatcherEvents.Step()),
            ct
        );
        await telemetry.OnEmbedStart!(DispatcherEvents.EmbedStart(), ct);
        await telemetry.OnEmbedEnd!(DispatcherEvents.EmbedEnd(), ct);
        await telemetry.OnRerankStart!(DispatcherEvents.RerankStart(), ct);
        await telemetry.OnRerankEnd!(DispatcherEvents.RerankEnd(), ct);
        await telemetry.OnDecideStart!(decide, ct);
        await telemetry.OnDecisionModelCallStart!(DispatcherEvents.DecisionModel(), ct);
        await telemetry.OnDecisionModelCallEnd!(DispatcherEvents.DecisionModel(), ct);
        await telemetry.OnDecideEnd!(decide, ct);
        await telemetry.OnExtractStart!(extractStart, ct);
        await telemetry.OnExtractionModelCallStart!(extractionModelCallStart, ct);
        await telemetry.OnExtractionModelCallEnd!(
            new ExtractionModelCallEndEvent(
                extractionModelCallStart,
                new Dictionary<string, ExtractionAnswer>(),
                null,
                [],
                null,
                null
            ),
            ct
        );
        await telemetry.OnExtractEnd!(
            new ExtractEndEvent(
                extractStart,
                new Dictionary<string, ExtractionAnswer>(),
                new OperationUsage(),
                [],
                null,
                DispatcherEvents.Response()
            ),
            ct
        );
        await telemetry.OnStreamTranscriptionStart!(
            DispatcherEvents.StreamTranscriptionStart(),
            ct
        );
        await telemetry.OnStreamTranscriptionEnd!(DispatcherEvents.StreamTranscriptionEnd(), ct);
        await telemetry.OnEnd!(DispatcherEvents.TextResult(), ct);
        await telemetry.OnAbort!(new StreamAbortContext([], error), ct);
        await telemetry.OnError!(new TelemetryErrorEvent("call-1", error), ct);
        await telemetry.OnStepError!(
            new GenerateTextStepErrorEvent(new GenerateTextStepStartEvent(), error),
            ct
        );

        Assert.Equal(
            [
                "onStart",
                "onStepStart",
                "onLanguageModelCallStart",
                "onLanguageModelCallEnd",
                "onToolExecutionStart",
                "onToolExecutionEnd",
                "onStepEnd",
                "onObjectStepStart",
                "onObjectStepEnd",
                "onEmbedStart",
                "onEmbedEnd",
                "onRerankStart",
                "onRerankEnd",
                "experimental_onDecideStart",
                "experimental_onDecisionModelCallStart",
                "experimental_onDecisionModelCallEnd",
                "experimental_onDecideEnd",
                "experimental_onExtractStart",
                "experimental_onExtractionModelCallStart",
                "experimental_onExtractionModelCallEnd",
                "experimental_onExtractEnd",
                "experimental_onStreamTranscriptionStart",
                "experimental_onStreamTranscriptionEnd",
                "onEnd",
                "onAbort",
                "onError",
                "onStepError",
            ],
            calls
        );

        await AssertObjectLifecycleDispatch();
    }

    // The generateObject and streamObject dispatch of onStart, onObjectStepStart and onObjectStepEnd.
    private static async Task AssertObjectLifecycleDispatch()
    {
        var events = new List<string>();
        var integration = RecordingIntegration(events);

        var generateOptions = GenerateObjectTestSupportFacade.GenerateOptions();
        generateOptions.Telemetry = new TelemetryOptions { Integrations = [integration] };
        await GenerateObjectTestSupportFacade.Client().GenerateObjectAsync(generateOptions);
        Assert.Equal(
            ["onStart:ai.generateObject", "onObjectStepStart", "onObjectStepEnd", "onEnd"],
            events
        );

        events.Clear();
        var streamOptions = GenerateObjectTestSupportFacade.StreamOptions();
        streamOptions.Telemetry = new TelemetryOptions { Integrations = [integration] };
        await GenerateObjectTestSupportFacade.CollectAsync(streamOptions);
        Assert.Equal(
            ["onStart:ai.streamObject", "onObjectStepStart", "onObjectStepEnd", "onEnd"],
            events
        );

        events.Clear();
        var failingGenerate = GenerateObjectTestSupportFacade.GenerateOptions();
        ((GenerateObjectTestModel)failingGenerate.Model!).OnGenerate = () =>
            throw new InvalidOperationException("boom");
        failingGenerate.Telemetry = new TelemetryOptions { Integrations = [integration] };
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            GenerateObjectTestSupportFacade.Client().GenerateObjectAsync(failingGenerate)
        );
        Assert.Equal(["onStart:ai.generateObject", "onObjectStepStart", "onError"], events);

        events.Clear();
        var failingStream = GenerateObjectTestSupportFacade.FailingStreamOptions(
            new InvalidOperationException("boom")
        );
        failingStream.Telemetry = new TelemetryOptions { Integrations = [integration] };
        await GenerateObjectTestSupportFacade.CollectAsync(failingStream);
        Assert.Contains("onError", events);
    }

    [Fact]
    [UpstreamTest(Prefix + "handles an empty array of integrations")]
    public void Handles_an_empty_array_of_integrations()
    {
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            new TelemetryOptions { Integrations = [] }
        );

        Assert.Null(telemetry.OnStart);
        Assert.Null(telemetry.OnEnd);
    }

    internal static TelemetryOptions Options(params ITelemetry[] integrations) =>
        new() { Integrations = integrations };

    internal static TestTelemetry OnCallStart(List<LanguageModelCallStartEvent> calls) =>
        new()
        {
            OnLanguageModelCallStart = (e, _) =>
            {
                calls.Add(e);
                return Task.CompletedTask;
            },
        };

    internal static TestTelemetry RecordingIntegration(List<string> events) =>
        new()
        {
            OnStart = (e, _) =>
            {
                events.Add("onStart:" + Assert.IsType<GenerateObjectStartEvent>(e).OperationId);
                return Task.CompletedTask;
            },
            OnObjectStepStart = (_, _) =>
            {
                events.Add("onObjectStepStart");
                return Task.CompletedTask;
            },
            OnObjectStepEnd = (_, _) =>
            {
                events.Add("onObjectStepEnd");
                return Task.CompletedTask;
            },
            OnEnd = (e, _) =>
            {
                Assert.StartsWith("GenerateObjectEndEvent", e.GetType().Name);
                events.Add("onEnd");
                return Task.CompletedTask;
            },
            OnError = (_, _) =>
            {
                events.Add("onError");
                return Task.CompletedTask;
            },
        };
}
