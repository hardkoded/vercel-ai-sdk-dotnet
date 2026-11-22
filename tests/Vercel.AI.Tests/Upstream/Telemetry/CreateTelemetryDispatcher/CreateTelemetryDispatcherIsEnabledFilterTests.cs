// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;

namespace Vercel.AI.Tests.Upstream.Telemetry.CreateTelemetryDispatcher;

/// <summary>Port of <c>create-telemetry-dispatcher.test.ts</c> &gt; <c>createTelemetryDispatcher &gt; isEnabled filter</c>.</summary>
[Collection("LogWarnings")]
public sealed class CreateTelemetryDispatcherIsEnabledFilterTests
{
    private const string Id =
        "packages/ai/src/telemetry/create-telemetry-dispatcher.test.ts::createTelemetryDispatcher > isEnabled filter::";

    [Fact]
    [UpstreamTest(
        Id + "does not invoke any integration callbacks when isEnabled is false",
        Coverage = UpstreamCoverage.Partial,
        Note = "Covers onStart, onObjectStepStart, onObjectStepEnd, onEnd and onError as generateObject and streamObject dispatch them."
    )]
    public async Task Does_not_invoke_object_integration_callbacks_when_isEnabled_is_false()
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

        var failingGenerate = GenerateObjectTestSupportFacade.GenerateOptions();
        ((GenerateObjectTestModel)failingGenerate.Model!).OnGenerate = () =>
            throw new InvalidOperationException("boom");
        failingGenerate.Telemetry = new TelemetryOptions
        {
            IsEnabled = false,
            Integrations = [integration],
        };
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            GenerateObjectTestSupportFacade.Client().GenerateObjectAsync(failingGenerate)
        );

        var failingStream = GenerateObjectTestSupportFacade.FailingStreamOptions(
            new InvalidOperationException("boom")
        );
        failingStream.Telemetry = new TelemetryOptions
        {
            IsEnabled = false,
            Integrations = [integration],
        };
        await GenerateObjectTestSupportFacade.CollectAsync(failingStream);

        Assert.Empty(events);
    }
}
