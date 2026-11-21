// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI;

/// <summary>
/// Globally registered telemetry integrations. Maps to <c>telemetry-registry.ts</c> and
/// <c>globalThis.AI_SDK_TELEMETRY_INTEGRATIONS</c>.
/// </summary>
public static class TelemetryRegistry
{
    private static readonly object Gate = new();
    private static IReadOnlyList<ITelemetry>? _integrations;

    /// <summary>
    /// The globally registered integrations (<c>AI_SDK_TELEMETRY_INTEGRATIONS</c>). Set it to <c>null</c> to clear them.
    /// Prefer <see cref="RegisterTelemetry"/>.
    /// </summary>
    public static IReadOnlyList<ITelemetry>? Integrations
    {
        get
        {
            lock (Gate)
            {
                return _integrations;
            }
        }
        set
        {
            lock (Gate)
            {
                _integrations = value;
            }
        }
    }

    /// <summary>Registers one or more telemetry integrations globally. Maps to <c>registerTelemetry</c>.</summary>
    public static void RegisterTelemetry(params ITelemetry[] integrations)
    {
        if (integrations is null)
        {
            throw new ArgumentNullException(nameof(integrations));
        }

        lock (Gate)
        {
            _integrations = [.. _integrations ?? [], .. integrations];
        }
    }

    /// <summary>Returns the globally registered integrations. Maps to <c>getGlobalTelemetryIntegrations</c>.</summary>
    public static IReadOnlyList<ITelemetry> GetGlobalTelemetryIntegrations()
    {
        return Integrations ?? [];
    }
}
