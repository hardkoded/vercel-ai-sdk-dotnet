// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.ProviderUtils;

/// <summary>Small JSON helpers shared by providers.</summary>
public static class JsonValues
{
    /// <summary>Serializer options used for provider bodies.</summary>
    public static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Parses a JSON object or returns an empty object.</summary>
    public static JsonElement ParseOrEmpty(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return EmptyObject();
        }

        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    /// <summary>An empty JSON object.</summary>
    public static JsonElement EmptyObject()
    {
        using var document = JsonDocument.Parse("{}");
        return document.RootElement.Clone();
    }

    /// <summary>Reads a string property, or null.</summary>
    public static string? GetString(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    /// <summary>Reads an integer property, or null.</summary>
    public static int? GetInt(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
        {
            return number;
        }

        return null;
    }

    /// <summary>Best-effort error message from a provider JSON body.</summary>
    public static string ExtractErrorMessage(string? body, int statusCode)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return "Provider request failed with status " + statusCode + ".";
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("error", out var error))
                {
                    if (error.ValueKind == JsonValueKind.String)
                    {
                        return error.GetString() ?? body;
                    }

                    if (error.ValueKind == JsonValueKind.Object)
                    {
                        var message = GetString(error, "message");
                        if (!string.IsNullOrEmpty(message))
                        {
                            var param = GetString(error, "param");
                            return string.IsNullOrEmpty(param) ? message! : message + " (" + param + ")";
                        }
                    }
                }

                var top = GetString(root, "message");
                if (!string.IsNullOrEmpty(top))
                {
                    return top!;
                }
            }
        }
        catch (JsonException)
        {
        }

        return body!;
    }

    /// <summary>Creates a new id. Maps to <c>generateId</c>.</summary>
    public static string GenerateId(string prefix)
    {
        return prefix + Guid.NewGuid().ToString("N");
    }
}
