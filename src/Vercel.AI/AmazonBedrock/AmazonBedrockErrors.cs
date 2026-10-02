// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Formats a failed Bedrock JSON error body.</summary>
public static class AmazonBedrockErrors
{
    /// <summary>
    /// Returns the provider message. When <c>type</c> is present the result is <c>type: message</c>.
    /// </summary>
    public static string Format(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return "Unknown error";
        }

        try
        {
            using var document = JsonDocument.Parse(body!);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
            {
                var text = message.GetString() ?? string.Empty;
                if (root.TryGetProperty("type", out var type) && type.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(type.GetString()))
                {
                    return type.GetString() + ": " + text;
                }

                return text;
            }
        }
        catch (JsonException)
        {
        }

        return body!;
    }
}
