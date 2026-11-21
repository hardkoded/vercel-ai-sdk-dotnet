// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI;

/// <summary>Telemetry configuration. Maps to <c>TelemetryOptions</c> in <c>telemetry/telemetry-options.ts</c>.</summary>
public sealed class TelemetryOptions
{
    /// <summary>
    /// Enable or disable telemetry. Enabled by default when a telemetry integration is registered. Set to
    /// <c>false</c> to opt out. Maps to <c>isEnabled</c>.
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>Enable or disable input recording. Enabled by default. Maps to <c>recordInputs</c>.</summary>
    public bool? RecordInputs { get; set; }

    /// <summary>Enable or disable output recording. Enabled by default. Maps to <c>recordOutputs</c>.</summary>
    public bool? RecordOutputs { get; set; }

    /// <summary>Identifier for this function. Used to group telemetry data by function. Maps to <c>functionId</c>.</summary>
    public string? FunctionId { get; set; }

    /// <summary>
    /// Per-call telemetry integrations that receive lifecycle events during generation. When provided, they take precedence over
    /// the globally registered integrations for this call. Maps to <c>integrations</c>.
    /// </summary>
    public IReadOnlyList<ITelemetry>? Integrations { get; set; }

    /// <summary>
    /// Top-level runtime context properties that should be included in telemetry. Runtime context properties are excluded
    /// unless they are explicitly set to <c>true</c>. Maps to <c>includeRuntimeContext</c>.
    /// </summary>
    public IReadOnlyDictionary<string, bool>? IncludeRuntimeContext { get; set; }

    /// <summary>
    /// Top-level tool context properties that should be included in telemetry, configured per tool name. Tool context properties
    /// are excluded unless they are explicitly set to <c>true</c>. Maps to <c>includeToolsContext</c>.
    /// </summary>
    public IReadOnlyDictionary<
        string,
        IReadOnlyDictionary<string, bool>
    >? IncludeToolsContext { get; set; }
}
