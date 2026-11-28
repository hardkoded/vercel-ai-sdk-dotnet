// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Groq;

/// <summary>Groq chat response shapes. Maps to the schemas in <c>groq-chat-language-model.ts</c>.</summary>
internal static class GroqChatApiTypes
{
    /// <summary>
    /// Limits a usage object to the fields the Groq chat schemas keep. Upstream parses it with a limited zod schema,
    /// which drops every other field, so <c>raw</c> holds only the known fields.
    /// </summary>
    public static JsonElement? ParseUsage(JsonElement usage)
    {
        if (usage.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var result = new JsonObject();
        foreach (var name in new[] { "prompt_tokens", "completion_tokens", "total_tokens" })
        {
            if (usage.TryGetProperty(name, out var value) && IsNumberOrNull(value))
            {
                result[name] = JsonNode.Parse(value.GetRawText());
            }
        }

        CopyDetails(usage, result, "prompt_tokens_details", "cached_tokens");
        CopyDetails(usage, result, "completion_tokens_details", "reasoning_tokens");
        return JsonDocument.Parse(result.ToJsonString()).RootElement.Clone();
    }

    private static void CopyDetails(JsonElement usage, JsonObject result, string name, string field)
    {
        if (!usage.TryGetProperty(name, out var details))
        {
            return;
        }

        if (details.ValueKind == JsonValueKind.Null)
        {
            result[name] = null;
            return;
        }

        if (details.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        var copy = new JsonObject();
        if (details.TryGetProperty(field, out var value) && IsNumberOrNull(value))
        {
            copy[field] = JsonNode.Parse(value.GetRawText());
        }

        result[name] = copy;
    }

    private static bool IsNumberOrNull(JsonElement value)
    {
        return value.ValueKind is JsonValueKind.Number or JsonValueKind.Null;
    }
}
