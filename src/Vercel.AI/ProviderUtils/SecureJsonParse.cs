// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.ProviderUtils;

/// <summary>Parses JSON and rejects prototype-pollution payloads. Maps to <c>secure-json-parse.ts</c>.</summary>
internal static class SecureJsonParse
{
    /// <summary>
    /// Parses <paramref name="text"/>. Throws <see cref="JsonException"/> when the JSON is malformed or any object
    /// carries a <c>__proto__</c> key, or a <c>constructor</c> key whose value has a <c>prototype</c> key.
    /// </summary>
    public static JsonElement Parse(string text)
    {
        using var document = JsonDocument.Parse(text);
        var root = document.RootElement;
        try
        {
            Scan(root);
        }
        catch (InvalidOperationException error)
        {
            // A string with an invalid escape (a lone surrogate, for example) only fails when it is decoded.
            throw new JsonException(error.Message, error);
        }

        return root.Clone();
    }

    private static void Scan(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                _ = value.GetString();
                break;
            case JsonValueKind.Array:
                foreach (var item in value.EnumerateArray())
                {
                    Scan(item);
                }

                break;
            case JsonValueKind.Object:
                foreach (var property in value.EnumerateObject())
                {
                    if (property.Name == "__proto__")
                    {
                        throw new JsonException("Object contains forbidden prototype property");
                    }

                    if (
                        property.Name == "constructor"
                        && property.Value.ValueKind == JsonValueKind.Object
                        && property.Value.TryGetProperty("prototype", out _)
                    )
                    {
                        throw new JsonException("Object contains forbidden prototype property");
                    }

                    Scan(property.Value);
                }

                break;
        }
    }
}
