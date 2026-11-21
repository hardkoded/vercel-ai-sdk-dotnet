// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Telemetry.TelemetryRegistry;

/// <summary>Port of <c>telemetry-registry.test.ts</c> &gt; <c>getGlobalTelemetryIntegrations</c>.</summary>
[Collection("TelemetryRegistry")]
public sealed class GetGlobalTelemetryIntegrationsTests : IDisposable
{
    private const string Prefix =
        "packages/ai/src/telemetry/telemetry-registry.test.ts::getGlobalTelemetryIntegrations::";

    public GetGlobalTelemetryIntegrationsTests()
    {
        Vercel.AI.TelemetryRegistry.Integrations = null;
    }

    public void Dispose()
    {
        Vercel.AI.TelemetryRegistry.Integrations = null;
    }

    [Fact]
    [UpstreamTest(Prefix + "returns an empty array when no integrations are registered")]
    public void Returns_an_empty_array_when_no_integrations_are_registered()
    {
        Assert.Empty(Vercel.AI.TelemetryRegistry.GetGlobalTelemetryIntegrations());
    }
}
