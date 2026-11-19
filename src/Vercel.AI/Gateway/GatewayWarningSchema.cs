// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Gateway;

/// <summary>Reads warnings from Gateway responses. Maps to <c>gatewayWarningSchema</c>.</summary>
internal static class GatewayWarningSchema
{
    /// <summary>
    /// Parses a known warning. Warnings are advisory, so one this client does not know is kept as an <c>other</c> warning
    /// instead of failing a successful response.
    /// </summary>
    public static OperationWarning Parse(JsonElement warning)
    {
        if (
            warning.ValueKind == JsonValueKind.Object
            && Text(warning, "type") is { } type
            && Known(warning, type) is { } known
        )
        {
            return known;
        }

        return new OperationWarning(
            "other",
            message: "Unknown warning: " + JsonSerializer.Serialize(warning)
        );
    }

    private static OperationWarning? Known(JsonElement warning, string type)
    {
        switch (type)
        {
            case "unsupported":
            case "compatibility":
                return Text(warning, "feature") is { } feature
                    ? new OperationWarning(
                        type,
                        feature: feature,
                        details: Text(warning, "details")
                    )
                    : null;
            case "deprecated":
                return
                    Text(warning, "setting") is { } setting
                    && Text(warning, "message") is { } deprecatedMessage
                    ? new OperationWarning(type, message: deprecatedMessage, setting: setting)
                    : null;
            case "other":
                return Text(warning, "message") is { } message
                    ? new OperationWarning(type, message: message)
                    : null;
            default:
                return null;
        }
    }

    private static string? Text(JsonElement element, string name)
    {
        return
            element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }
}
