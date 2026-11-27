// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Telemetry.CreateTelemetryDispatcher;

/// <summary>Port of <c>create-telemetry-dispatcher.test.ts</c> &gt; <c>createTelemetryDispatcher &gt; class-based integrations</c>.</summary>
[Collection("TelemetryRegistry")]
public sealed class CreateTelemetryDispatcherClassBasedIntegrationsTests : IDisposable
{
    private const string Id =
        "packages/ai/src/telemetry/create-telemetry-dispatcher.test.ts::createTelemetryDispatcher > class-based integrations::";

    public CreateTelemetryDispatcherClassBasedIntegrationsTests()
    {
        Vercel.AI.TelemetryRegistry.Integrations = null;
    }

    public void Dispose()
    {
        Vercel.AI.TelemetryRegistry.Integrations = null;
    }

    [Fact]
    [UpstreamTest(
        Id + "preserves this context for local integrations",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_this_context_for_local_integrations()
    {
        var instance = new MyIntegration();
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            new TelemetryOptions { Integrations = [instance] }
        );

        await telemetry.OnStart!(new GenerateTextStartEvent(), CancellationToken.None);

        Assert.Equal("called", instance.Value);
    }

    [Fact]
    [UpstreamTest(
        Id + "preserves this context across multiple methods",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_this_context_across_multiple_methods()
    {
        var instance = new DevToolsTelemetry();
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            new TelemetryOptions { Integrations = [instance] }
        );

        await telemetry.OnStart!(new GenerateTextStartEvent(), CancellationToken.None);
        await telemetry.OnEnd!(DispatcherEvents.TextResult(), CancellationToken.None);

        Assert.Equal(["start", "end"], instance.Calls);
    }

    // Stand in for upstream's classes whose methods use this.
    private sealed class MyIntegration : TestTelemetry
    {
        public MyIntegration()
        {
            OnStart = (_, _) =>
            {
                Value = "called";
                return Task.CompletedTask;
            };
        }

        public string Value { get; private set; } = "";
    }

    private sealed class DevToolsTelemetry : TestTelemetry
    {
        public DevToolsTelemetry()
        {
            OnStart = (_, _) =>
            {
                Calls.Add("start");
                return Task.CompletedTask;
            };
            OnEnd = (_, _) =>
            {
                Calls.Add("end");
                return Task.CompletedTask;
            };
        }

        public List<string> Calls { get; } = [];
    }
}
