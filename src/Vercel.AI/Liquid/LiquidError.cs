// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Liquid;

/// <summary>Maps to <c>liquidFailedResponseHandler</c>: reads Liquid's <c>{ error: { message, code } }</c> body.</summary>
internal static class LiquidError
{
    internal static string? ErrorMessage(string? body)
    {
        try
        {
            using var document = JsonDocument.Parse(body ?? string.Empty);
            if (
                document.RootElement.ValueKind != JsonValueKind.Object
                || !document.RootElement.TryGetProperty("error", out var error)
                || error.ValueKind != JsonValueKind.Object
                || Read(error, "message") is not { } message
            )
            {
                return null;
            }

            return message + (Read(error, "code") is { } code ? " (" + code + ")" : string.Empty);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? Read(JsonElement element, string name)
    {
        return
            element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }
}
