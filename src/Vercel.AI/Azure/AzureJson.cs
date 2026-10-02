// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Azure;

/// <summary>JSON helpers shared by the Azure models.</summary>
internal static class AzureJson
{
    /// <summary>Parses a node into an independent element.</summary>
    public static JsonElement Element(JsonNode node)
    {
        using var document = JsonDocument.Parse(node.ToJsonString());
        return document.RootElement.Clone();
    }

    /// <summary>Copies an element so it outlives its document.</summary>
    public static JsonElement Clone(JsonElement element)
    {
        return element.Clone();
    }

    /// <summary>Reads a string property.</summary>
    public static string? String(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return value.GetString();
    }

    /// <summary>Reads an integer property.</summary>
    public static int? Int(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        return value.TryGetInt32(out var number) ? number : null;
    }

    /// <summary>Reads a nested object.</summary>
    public static bool Object(JsonElement element, string name, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out value) && value.ValueKind == JsonValueKind.Object)
        {
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>Serializes a node with compact JSON.</summary>
    public static string Compact(JsonNode node)
    {
        return node.ToJsonString();
    }

    /// <summary>Unix seconds as a UTC timestamp.</summary>
    public static DateTimeOffset? Timestamp(long? seconds)
    {
        if (seconds is null)
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeSeconds(seconds.Value);
    }

    /// <summary>Reads provider options for one provider name.</summary>
    public static bool ProviderOptions(IReadOnlyDictionary<string, JsonElement>? options, string provider, out JsonElement value)
    {
        if (options != null && options.TryGetValue(provider, out value) && value.ValueKind == JsonValueKind.Object)
        {
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>Adds a property when the value is non-null.</summary>
    public static void Set(JsonObject body, string name, string? value)
    {
        if (value != null)
        {
            body[name] = value;
        }
    }

    /// <summary>Adds a property when the value is non-null.</summary>
    public static void Set(JsonObject body, string name, int? value)
    {
        if (value is { } number)
        {
            body[name] = number;
        }
    }

    /// <summary>Adds a property when the value is non-null.</summary>
    public static void Set(JsonObject body, string name, double? value)
    {
        if (value is { } number)
        {
            body[name] = number;
        }
    }

    /// <summary>Adds a property when the value is non-null.</summary>
    public static void Set(JsonObject body, string name, bool? value)
    {
        if (value is { } flag)
        {
            body[name] = flag;
        }
    }

    /// <summary>JSON null that survives <see cref="JsonObject"/> assignment.</summary>
    public static JsonNode Null()
    {
        return JsonValue.Create((string?)null)!;
    }

    /// <summary>Formats a number the way compact JSON does.</summary>
    public static string Number(double value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}
