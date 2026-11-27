// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Telemetry.TelemetryRegistry;

/// <summary>Port of <c>telemetry-registry.test.ts</c> &gt; <c>registerTelemetry</c>.</summary>
[Collection("TelemetryRegistry")]
public sealed class RegisterTelemetryTests : IDisposable
{
    private const string Prefix =
        "packages/ai/src/telemetry/telemetry-registry.test.ts::registerTelemetry::";

    public RegisterTelemetryTests()
    {
        Vercel.AI.TelemetryRegistry.Integrations = null;
    }

    public void Dispose()
    {
        Vercel.AI.TelemetryRegistry.Integrations = null;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "adds an integration to the global registry",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Adds_an_integration_to_the_global_registry()
    {
        var integration = new TestTelemetry();

        Vercel.AI.TelemetryRegistry.RegisterTelemetry(integration);

        Assert.Equal(
            new ITelemetry[] { integration },
            Vercel.AI.TelemetryRegistry.GetGlobalTelemetryIntegrations()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "adds multiple integrations in registration order",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Adds_multiple_integrations_in_registration_order()
    {
        var integration1 = new TestTelemetry();
        var integration2 = new TestTelemetry();

        Vercel.AI.TelemetryRegistry.RegisterTelemetry(integration1);
        Vercel.AI.TelemetryRegistry.RegisterTelemetry(integration2);

        Assert.Equal(
            new ITelemetry[] { integration1, integration2 },
            Vercel.AI.TelemetryRegistry.GetGlobalTelemetryIntegrations()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "adds multiple integrations passed in a single call",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Adds_multiple_integrations_passed_in_a_single_call()
    {
        var integration1 = new TestTelemetry();
        var integration2 = new TestTelemetry();
        var integration3 = new TestTelemetry();

        Vercel.AI.TelemetryRegistry.RegisterTelemetry(integration1, integration2, integration3);

        Assert.Equal(
            new ITelemetry[] { integration1, integration2, integration3 },
            Vercel.AI.TelemetryRegistry.GetGlobalTelemetryIntegrations()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "is a no-op when called with no integrations",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Is_a_no_op_when_called_with_no_integrations()
    {
        Vercel.AI.TelemetryRegistry.RegisterTelemetry();

        Assert.Empty(Vercel.AI.TelemetryRegistry.GetGlobalTelemetryIntegrations());
    }
}
