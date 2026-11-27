// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Telemetry.CreateTelemetryDispatcher;

/// <summary>
/// Port of <c>create-telemetry-dispatcher.test.ts</c> &gt; <c>createTelemetryDispatcher &gt; global vs local integration resolution</c>.
/// </summary>
[Collection("TelemetryRegistry")]
public sealed class CreateTelemetryDispatcherGlobalVsLocalIntegrationResolutionTests : IDisposable
{
    private const string Id =
        "packages/ai/src/telemetry/create-telemetry-dispatcher.test.ts::createTelemetryDispatcher > global vs local integration resolution::";

    public CreateTelemetryDispatcherGlobalVsLocalIntegrationResolutionTests()
    {
        Vercel.AI.TelemetryRegistry.Integrations = null;
    }

    public void Dispose()
    {
        Vercel.AI.TelemetryRegistry.Integrations = null;
    }

    private static Task Dispatch(Vercel.AI.TelemetryDispatcher telemetry) =>
        telemetry.OnStart!(new GenerateTextStartEvent(), CancellationToken.None);

    [Fact]
    [UpstreamTest(
        Id + "uses globally registered integrations when no local integrations are provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Uses_globally_registered_integrations_when_no_local_integrations_are_provided()
    {
        var calls = new List<GenerateTextStartEvent>();
        Vercel.AI.TelemetryRegistry.RegisterTelemetry(
            CreateTelemetryDispatcherTests.OnStartCapture(calls)
        );

        await Dispatch(Vercel.AI.TelemetryDispatcher.Create(new TelemetryOptions()));

        Assert.Equal(Vercel.AI.Operations.AiSdkVersion.Version, Assert.Single(calls).AiVersion);
    }

    [Fact]
    [UpstreamTest(
        Id + "uses only local integrations when provided, ignoring global",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Uses_only_local_integrations_when_provided_ignoring_global()
    {
        var global = new List<GenerateTextStartEvent>();
        var local = new List<GenerateTextStartEvent>();
        Vercel.AI.TelemetryRegistry.RegisterTelemetry(
            CreateTelemetryDispatcherTests.OnStartCapture(global)
        );

        await Dispatch(
            Vercel.AI.TelemetryDispatcher.Create(
                CreateTelemetryDispatcherTests.Options(
                    CreateTelemetryDispatcherTests.OnStartCapture(local)
                )
            )
        );

        Assert.Empty(global);
        Assert.Equal(Vercel.AI.Operations.AiSdkVersion.Version, Assert.Single(local).AiVersion);
    }

    [Fact]
    [UpstreamTest(
        Id + "uses only local integration array when provided, ignoring global",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Uses_only_local_integration_array_when_provided_ignoring_global()
    {
        var global = new List<GenerateTextStartEvent>();
        var first = new List<GenerateTextStartEvent>();
        var second = new List<GenerateTextStartEvent>();
        Vercel.AI.TelemetryRegistry.RegisterTelemetry(
            CreateTelemetryDispatcherTests.OnStartCapture(global)
        );

        await Dispatch(
            Vercel.AI.TelemetryDispatcher.Create(
                CreateTelemetryDispatcherTests.Options(
                    CreateTelemetryDispatcherTests.OnStartCapture(first),
                    CreateTelemetryDispatcherTests.OnStartCapture(second)
                )
            )
        );

        Assert.Empty(global);
        Assert.Single(first);
        Assert.Single(second);
    }

    [Fact]
    [UpstreamTest(
        Id + "global integrations still work for calls without local integrations",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Global_integrations_still_work_for_calls_without_local_integrations()
    {
        var global = new List<GenerateTextStartEvent>();
        var local = new List<GenerateTextStartEvent>();
        Vercel.AI.TelemetryRegistry.RegisterTelemetry(
            CreateTelemetryDispatcherTests.OnStartCapture(global)
        );

        var withLocal = Vercel.AI.TelemetryDispatcher.Create(
            CreateTelemetryDispatcherTests.Options(
                CreateTelemetryDispatcherTests.OnStartCapture(local)
            )
        );
        var withoutLocal = Vercel.AI.TelemetryDispatcher.Create(new TelemetryOptions());

        await Dispatch(withLocal);
        await Dispatch(withoutLocal);

        Assert.Single(local);
        Assert.Single(global);
    }

    [Fact]
    [UpstreamTest(
        Id + "auto-binds class-based global integrations",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Auto_binds_class_based_global_integrations()
    {
        var integration = new ClassGlobalIntegration();
        Vercel.AI.TelemetryRegistry.RegisterTelemetry(integration);

        var telemetry = Vercel.AI.TelemetryDispatcher.Create(new TelemetryOptions());
        await Dispatch(telemetry);

        Assert.Equal(1, integration.Calls);
    }

    // Stands in for upstream's class with an onStart method, whose this context the dispatcher must keep.
    private sealed class ClassGlobalIntegration : TestTelemetry
    {
        public ClassGlobalIntegration()
        {
            OnStart = (_, _) =>
            {
                Calls++;
                return Task.CompletedTask;
            };
        }

        public int Calls { get; private set; }
    }
}
