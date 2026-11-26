// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Gateway;

/// <summary>
/// Gateway request bodies can carry request-scoped BYOK credentials in <c>providerOptions.gateway.byok</c>. The credentials must reach the Gateway,
/// but telemetry-only copies must not expose them. Maps to <c>redact-gateway-request-credentials.ts</c>.
/// </summary>
internal static class RedactGatewayRequestCredentials
{
    private const string Redacted = "[redacted]";

    /// <summary>
    /// Annotates a Gateway request body with a telemetry view that replaces every value under <c>providerOptions.gateway.byok</c> with
    /// <c>[redacted]</c>. A body without BYOK credentials needs no redaction. Maps to <c>withGatewayRequestTelemetryRedaction</c>.
    /// </summary>
    internal static TelemetryRedactedValue WithGatewayRequestTelemetryRedaction(JsonElement value)
    {
        if (
            value.ValueKind != JsonValueKind.Object
            || (
                !HasByok(Property(value, "providerOptions"))
                && !HasRequestByok(Property(value, "requests"))
            )
        )
        {
            return new TelemetryRedactedValue(value, null);
        }

        return TelemetryRedaction.WithTelemetryRedaction(value, RedactBody);
    }

    /// <summary>The telemetry view of a Gateway request body. Maps to the redactor of <c>withGatewayRequestTelemetryRedaction</c>.</summary>
    internal static JsonElement RedactBody(JsonElement body)
    {
        if (
            body.ValueKind != JsonValueKind.Object
            || JsonNode.Parse(body.GetRawText()) is not JsonObject copy
        )
        {
            return body;
        }

        RedactProviderOptions(copy);
        if (copy["requests"] is JsonArray requests)
        {
            foreach (var request in requests)
            {
                // Batch bodies nest the same provider options in every request's options.
                if (request is JsonObject { } entry && entry["options"] is JsonObject options)
                {
                    RedactProviderOptions(options);
                }
            }
        }

        return JsonSerializer.SerializeToElement(copy);
    }

    private static void RedactProviderOptions(JsonObject owner)
    {
        if (
            owner["providerOptions"] is JsonObject providerOptions
            && providerOptions["gateway"] is JsonObject gateway
            && gateway["byok"] is { } byok
        )
        {
            gateway["byok"] = RedactByok(byok);
        }
    }

    /// <summary>Keeps provider names and credential shapes, and replaces every BYOK value.</summary>
    private static JsonNode? RedactByok(JsonNode? value)
    {
        switch (value)
        {
            case JsonArray array:
                var items = new JsonArray();
                foreach (var item in array)
                {
                    items.Add(RedactByok(item));
                }

                return items;
            case JsonObject obj:
                var copy = new JsonObject();
                foreach (var entry in obj)
                {
                    copy[entry.Key] = RedactByok(entry.Value);
                }

                return copy;
            default:
                return JsonValue.Create(Redacted);
        }
    }

    private static JsonElement? Property(JsonElement value, string name) =>
        value.TryGetProperty(name, out var property) ? property : null;

    private static bool HasByok(JsonElement? providerOptions) =>
        providerOptions is { ValueKind: JsonValueKind.Object } options
        && options.TryGetProperty("gateway", out var gateway)
        && gateway.ValueKind == JsonValueKind.Object
        && gateway.TryGetProperty("byok", out _);

    private static bool HasRequestByok(JsonElement? requests) =>
        requests is { ValueKind: JsonValueKind.Array } array
        && array
            .EnumerateArray()
            .Any(request =>
                request.ValueKind == JsonValueKind.Object
                && request.TryGetProperty("options", out var options)
                && options.ValueKind == JsonValueKind.Object
                && HasByok(Property(options, "providerOptions"))
            );
}
