// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;

namespace Vercel.AI.Tests.Upstream.Telemetry.CreateTelemetryDispatcher;

/// <summary>Port of <c>create-telemetry-dispatcher.test.ts</c> &gt; <c>createTelemetryDispatcher</c>.</summary>
[Collection("LogWarnings")]
public sealed class CreateTelemetryDispatcherTests
{
    private const string Id =
        "packages/ai/src/telemetry/create-telemetry-dispatcher.test.ts::createTelemetryDispatcher::";

    [Fact]
    [UpstreamTest(
        Id + "works with all lifecycle methods",
        Coverage = UpstreamCoverage.Partial,
        Note = "Covers onStart, onObjectStepStart and onObjectStepEnd as generateObject and streamObject dispatch them."
    )]
    public async Task Works_with_all_lifecycle_methods_for_generateObject_and_streamObject()
    {
        var events = new List<string>();
        var integration = RecordingIntegration(events);

        var generateOptions = GenerateObjectTestSupportFacade.GenerateOptions();
        generateOptions.Telemetry = new TelemetryOptions { Integrations = [integration] };
        await GenerateObjectTestSupportFacade.Client().GenerateObjectAsync(generateOptions);
        Assert.Equal(["onStart:ai.generateObject", "onObjectStepStart", "onObjectStepEnd"], events);

        events.Clear();
        var streamOptions = GenerateObjectTestSupportFacade.StreamOptions();
        streamOptions.Telemetry = new TelemetryOptions { Integrations = [integration] };
        await GenerateObjectTestSupportFacade.CollectAsync(streamOptions);
        Assert.Equal(["onStart:ai.streamObject", "onObjectStepStart", "onObjectStepEnd"], events);
    }

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
        };
}
