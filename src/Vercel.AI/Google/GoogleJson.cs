// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Google;

internal static class GoogleJson
{
    public static JsonNode Null()
    {
        return JsonNode.Parse("null")!;
    }

    public static JsonNode Clone(JsonNode node)
    {
        return JsonNode.Parse(node.ToJsonString())!;
    }

    public static JsonNode Clone(JsonElement element)
    {
        return JsonNode.Parse(element.GetRawText())!;
    }

    public static JsonElement Element(JsonNode node)
    {
        using var document = JsonDocument.Parse(node.ToJsonString());
        return document.RootElement.Clone();
    }

    public static JsonNode ParseValue(string json)
    {
        return JsonNode.Parse(string.IsNullOrWhiteSpace(json) ? "null" : json) ?? Null();
    }

    public static bool TryParse(string? json, out JsonNode node)
    {
        node = Null();
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            node = JsonNode.Parse(json) ?? Null();
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
