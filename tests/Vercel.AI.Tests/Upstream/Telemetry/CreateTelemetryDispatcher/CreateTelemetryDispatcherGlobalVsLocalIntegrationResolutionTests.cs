// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Telemetry.CreateTelemetryDispatcher;

/// <summary>
/// Port of <c>create-telemetry-dispatcher.test.ts</c> &gt; <c>createTelemetryDispatcher &gt; global vs local integration resolution</c>.
/// The port dispatches <c>onLanguageModelCallStart</c>, which carries the augmented fields, where upstream's tests use <c>onStart</c>.
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
        telemetry.OnLanguageModelCallStart!(
            DispatcherEvents.LanguageModelCallStart(),
            CancellationToken.None
        );

    [Fact]
    [UpstreamTest(
        Id + "uses globally registered integrations when no local integrations are provided"
    )]
    public async Task Uses_globally_registered_integrations_when_no_local_integrations_are_provided()
    {
        var calls = new List<LanguageModelCallStartEvent>();
        Vercel.AI.TelemetryRegistry.RegisterTelemetry(
            CreateTelemetryDispatcherTests.OnCallStart(calls)
        );

        await Dispatch(Vercel.AI.TelemetryDispatcher.Create(new TelemetryOptions()));

        Assert.Equal(Vercel.AI.Operations.AiSdkVersion.Version, Assert.Single(calls).AiVersion);
    }

    [Fact]
    [UpstreamTest(Id + "uses only local integrations when provided, ignoring global")]
    public async Task Uses_only_local_integrations_when_provided_ignoring_global()
    {
        var global = new List<LanguageModelCallStartEvent>();
        var local = new List<LanguageModelCallStartEvent>();
        Vercel.AI.TelemetryRegistry.RegisterTelemetry(
            CreateTelemetryDispatcherTests.OnCallStart(global)
        );

        await Dispatch(
            Vercel.AI.TelemetryDispatcher.Create(
                CreateTelemetryDispatcherTests.Options(
                    CreateTelemetryDispatcherTests.OnCallStart(local)
                )
            )
        );

        Assert.Empty(global);
        Assert.Equal(Vercel.AI.Operations.AiSdkVersion.Version, Assert.Single(local).AiVersion);
    }

    [Fact]
    [UpstreamTest(Id + "uses only local integration array when provided, ignoring global")]
    public async Task Uses_only_local_integration_array_when_provided_ignoring_global()
    {
        var global = new List<LanguageModelCallStartEvent>();
        var first = new List<LanguageModelCallStartEvent>();
        var second = new List<LanguageModelCallStartEvent>();
        Vercel.AI.TelemetryRegistry.RegisterTelemetry(
            CreateTelemetryDispatcherTests.OnCallStart(global)
        );

        await Dispatch(
            Vercel.AI.TelemetryDispatcher.Create(
                CreateTelemetryDispatcherTests.Options(
                    CreateTelemetryDispatcherTests.OnCallStart(first),
                    CreateTelemetryDispatcherTests.OnCallStart(second)
                )
            )
        );

        Assert.Empty(global);
        Assert.Single(first);
        Assert.Single(second);
    }

    [Fact]
    [UpstreamTest(Id + "global integrations still work for calls without local integrations")]
    public async Task Global_integrations_still_work_for_calls_without_local_integrations()
    {
        var global = new List<LanguageModelCallStartEvent>();
        var local = new List<LanguageModelCallStartEvent>();
        Vercel.AI.TelemetryRegistry.RegisterTelemetry(
            CreateTelemetryDispatcherTests.OnCallStart(global)
        );

        var withLocal = Vercel.AI.TelemetryDispatcher.Create(
            CreateTelemetryDispatcherTests.Options(
                CreateTelemetryDispatcherTests.OnCallStart(local)
            )
        );
        var withoutLocal = Vercel.AI.TelemetryDispatcher.Create(new TelemetryOptions());

        await Dispatch(withLocal);
        await Dispatch(withoutLocal);

        Assert.Single(local);
        Assert.Single(global);
    }
}
