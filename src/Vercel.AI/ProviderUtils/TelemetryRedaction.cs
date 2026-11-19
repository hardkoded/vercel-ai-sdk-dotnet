// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.ProviderUtils;

/// <summary>
/// A JSON value with the provider-owned telemetry view attached. The view is not part of the value, so the original is untouched.
/// Maps to a value annotated by <c>withTelemetryRedaction</c> in <c>@ai-sdk/provider-utils</c>.
/// </summary>
public sealed class TelemetryRedactedValue
{
    internal TelemetryRedactedValue(JsonElement value, Func<JsonElement, JsonElement>? redact)
    {
        Value = value;
        Redact = redact;
    }

    /// <summary>The original value, which still carries any credentials.</summary>
    public JsonElement Value { get; }

    /// <summary>The provider's telemetry view, or null when the value needs no redaction.</summary>
    public Func<JsonElement, JsonElement>? Redact { get; }
}

/// <summary>Provider-owned telemetry redaction. Maps to <c>telemetry-redaction.ts</c> in <c>@ai-sdk/provider-utils</c>.</summary>
public static class TelemetryRedaction
{
    /// <summary>Attaches a provider-owned telemetry view to <paramref name="value"/> without changing it. Maps to <c>withTelemetryRedaction</c>.</summary>
    public static TelemetryRedactedValue WithTelemetryRedaction(
        JsonElement value,
        Func<JsonElement, JsonElement> redact
    )
    {
        if (redact == null)
        {
            throw new ArgumentNullException(nameof(redact));
        }

        return new TelemetryRedactedValue(value, redact);
    }

    /// <summary>Applies the provider's telemetry view, leaving an unannotated value unchanged. Maps to <c>redactTelemetryValue</c>.</summary>
    public static JsonElement? RedactTelemetryValue(TelemetryRedactedValue value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        return RedactTelemetryValue(value.Value, value.Redact);
    }

    /// <summary>
    /// Applies <paramref name="redact"/> to <paramref name="value"/>. Returns null when the redactor throws, so a broken provider
    /// redactor neither exposes credentials nor fails the call. Maps to <c>redactTelemetryValue</c>.
    /// </summary>
    public static JsonElement? RedactTelemetryValue(
        JsonElement value,
        Func<JsonElement, JsonElement>? redact
    )
    {
        if (redact == null)
        {
            return value;
        }

        try
        {
            return redact(value);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
