// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Telemetry.CreateTelemetryDispatcher;

/// <summary>Port of <c>create-telemetry-dispatcher.test.ts</c> &gt; <c>createTelemetryDispatcher &gt; isEnabled filter</c>.</summary>
[Collection("TelemetryRegistry")]
public sealed class CreateTelemetryDispatcherIsEnabledFilterTests
{
    private const string Id =
        "packages/ai/src/telemetry/create-telemetry-dispatcher.test.ts::createTelemetryDispatcher > isEnabled filter::";

    [Fact]
    [UpstreamTest(Id + "does not invoke any integration callbacks when isEnabled is false")]
    public async Task Does_not_invoke_any_integration_callbacks_when_isEnabled_is_false()
    {
        var wrapper = new PassThroughWrapper();
        var integration = new TestTelemetry
        {
            OnStart = (_, _) => Task.CompletedTask,
            OnStepStart = (_, _) => Task.CompletedTask,
            OnToolExecutionStart = (_, _) => Task.CompletedTask,
            OnToolExecutionEnd = (_, _) => Task.CompletedTask,
            OnStepEnd = (_, _) => Task.CompletedTask,
            OnObjectStepStart = (_, _) => Task.CompletedTask,
            OnObjectStepEnd = (_, _) => Task.CompletedTask,
            OnEmbedStart = (_, _) => Task.CompletedTask,
            OnEmbedEnd = (_, _) => Task.CompletedTask,
            OnRerankStart = (_, _) => Task.CompletedTask,
            OnRerankEnd = (_, _) => Task.CompletedTask,
            OnDecideStart = (_, _) => Task.CompletedTask,
            OnDecisionModelCallStart = (_, _) => Task.CompletedTask,
            OnDecisionModelCallEnd = (_, _) => Task.CompletedTask,
            OnDecideEnd = (_, _) => Task.CompletedTask,
            OnExtractStart = (_, _) => Task.CompletedTask,
            OnExtractionModelCallStart = (_, _) => Task.CompletedTask,
            OnExtractionModelCallEnd = (_, _) => Task.CompletedTask,
            OnExtractEnd = (_, _) => Task.CompletedTask,
            OnStreamTranscriptionStart = (_, _) => Task.CompletedTask,
            OnStreamTranscriptionEnd = (_, _) => Task.CompletedTask,
            OnEnd = (_, _) => Task.CompletedTask,
            OnAbort = (_, _) => Task.CompletedTask,
            OnError = (_, _) => Task.CompletedTask,
            ExecuteLanguageModelCall = wrapper,
            ExecuteTool = wrapper,
        };

        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            new TelemetryOptions { IsEnabled = false, Integrations = [integration] }
        );

        Assert.Null(telemetry.OnStart);
        Assert.Null(telemetry.OnStepStart);
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
        Assert.Empty(telemetry.ExecuteLanguageModelCallWrappers);
        Assert.Empty(telemetry.ExecuteToolWrappers);

        await AssertObjectCallbacksNotInvoked();
    }

    // The generateObject and streamObject dispatch of onStart, onObjectStepStart and onObjectStepEnd.
    private static async Task AssertObjectCallbacksNotInvoked()
    {
        var events = new List<string>();
        var integration = CreateTelemetryDispatcherTests.RecordingIntegration(events);

        var generateOptions = GenerateObjectTestSupportFacade.GenerateOptions();
        generateOptions.Telemetry = new TelemetryOptions
        {
            IsEnabled = false,
            Integrations = [integration],
        };
        await GenerateObjectTestSupportFacade.Client().GenerateObjectAsync(generateOptions);

        var streamOptions = GenerateObjectTestSupportFacade.StreamOptions();
        streamOptions.Telemetry = new TelemetryOptions
        {
            IsEnabled = false,
            Integrations = [integration],
        };
        await GenerateObjectTestSupportFacade.CollectAsync(streamOptions);

        Assert.Empty(events);
    }

    [Fact]
    [UpstreamTest(Id + "ignores globally registered integrations when isEnabled is false")]
    public void Ignores_globally_registered_integrations_when_isEnabled_is_false()
    {
        var wrapper = new PassThroughWrapper();
        Vercel.AI.TelemetryRegistry.Integrations = null;
        try
        {
            Vercel.AI.TelemetryRegistry.RegisterTelemetry(
                new TestTelemetry
                {
                    OnStart = (_, _) => Task.CompletedTask,
                    ExecuteLanguageModelCall = wrapper,
                    ExecuteTool = wrapper,
                }
            );

            var telemetry = Vercel.AI.TelemetryDispatcher.Create(
                new TelemetryOptions { IsEnabled = false }
            );

            Assert.Null(telemetry.OnStart);
            Assert.Empty(telemetry.ExecuteLanguageModelCallWrappers);
            Assert.Empty(telemetry.ExecuteToolWrappers);
        }
        finally
        {
            Vercel.AI.TelemetryRegistry.Integrations = null;
        }
    }

    [Fact]
    [UpstreamTest(
        Id + "invokes integrations when isEnabled is true",
        Note = "Dispatches onLanguageModelCallStart, which carries the augmented fields, where upstream's test uses onStart."
    )]
    public async Task Invokes_integrations_when_isEnabled_is_true()
    {
        var calls = new List<LanguageModelCallStartEvent>();
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            new TelemetryOptions
            {
                IsEnabled = true,
                Integrations = [CreateTelemetryDispatcherTests.OnCallStart(calls)],
            }
        );

        await telemetry.OnLanguageModelCallStart!(
            DispatcherEvents.LanguageModelCallStart(),
            CancellationToken.None
        );

        Assert.Equal(Vercel.AI.Operations.AiSdkVersion.Version, Assert.Single(calls).AiVersion);
    }

    [Fact]
    [UpstreamTest(
        Id + "invokes integrations when isEnabled is undefined (default enabled)",
        Note = "Dispatches onLanguageModelCallStart, which carries the augmented fields, where upstream's test uses onStart."
    )]
    public async Task Invokes_integrations_when_isEnabled_is_undefined_default_enabled()
    {
        var calls = new List<LanguageModelCallStartEvent>();
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            new TelemetryOptions
            {
                Integrations = [CreateTelemetryDispatcherTests.OnCallStart(calls)],
            }
        );

        await telemetry.OnLanguageModelCallStart!(
            DispatcherEvents.LanguageModelCallStart(),
            CancellationToken.None
        );

        Assert.Equal(Vercel.AI.Operations.AiSdkVersion.Version, Assert.Single(calls).AiVersion);
    }

    [Fact]
    [UpstreamTest(
        Id + "does not augment events with isEnabled",
        Note = "The events have no IsEnabled property, so the test checks the type has none."
    )]
    public void Does_not_augment_events_with_isEnabled()
    {
        Assert.Null(typeof(LanguageModelCallStartEvent).GetProperty("IsEnabled"));
    }
}

/// <summary>An <c>executeLanguageModelCall</c> and <c>executeTool</c> wrapper that only runs the function.</summary>
internal sealed class PassThroughWrapper : IExecuteLanguageModelCall, IExecuteTool
{
    public Task<T> ExecuteAsync<T>(
        ExecuteLanguageModelCallOptions<T> options,
        CancellationToken cancellationToken
    ) => options.Execute();

    public Task<T> ExecuteAsync<T>(
        ExecuteToolOptions<T> options,
        CancellationToken cancellationToken
    ) => options.Execute();
}
