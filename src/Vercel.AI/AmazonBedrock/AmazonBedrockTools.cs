// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Builds a Converse <c>toolConfig</c> from V4 function tools.</summary>
public static class AmazonBedrockTools
{
    private static readonly string[] ModelsWithoutStrictTools =
    {
        "claude-opus-4-7",
        "claude-opus-4-8",
        "claude-opus-5",
        "claude-fable-5",
        "claude-sonnet-5",
        "claude-haiku-5-5",
    };

    /// <summary>
    /// Prepares function tools. An empty or missing list returns an empty object.
    /// Tool choice <c>none</c> clears the tools. A named choice keeps only that tool.
    /// </summary>
    public static JsonObject Prepare(string modelId, IReadOnlyList<LanguageModelTool>? tools, ToolChoice? toolChoice, out List<CallWarning> warnings)
    {
        warnings = new List<CallWarning>();
        if (tools == null || tools.Count == 0 || toolChoice is { Type: "none" })
        {
            return new JsonObject();
        }

        var selected = new List<LanguageModelTool>();
        foreach (var tool in tools)
        {
            if (toolChoice is ToolChoice.NamedChoice named && !string.Equals(tool.Name, named.ToolName, StringComparison.Ordinal))
            {
                continue;
            }

            selected.Add(tool);
        }

        if (selected.Count == 0)
        {
            return new JsonObject();
        }

        var supportsStrict = SupportsStrictTools(modelId);
        var prepared = new JsonArray();
        foreach (var tool in selected)
        {
            var includeStrict = false;
            if (tool.Strict != null)
            {
                if (!supportsStrict)
                {
                    if (tool.Strict.Value)
                    {
                        warnings.Add(new CallWarning(
                            "unsupported",
                            "Tool '" + tool.Name + "' has strict: true, but strict mode is not supported by this model on Amazon Bedrock. The strict property will be ignored."));
                    }
                }
                else if (tool.Strict.Value && !IsStrictSchemaCompatible(tool.InputSchema))
                {
                    warnings.Add(new CallWarning(
                        "unsupported",
                        "Tool '" + tool.Name + "' has strict: true, but Amazon Bedrock requires every object in a strict tool schema to set additionalProperties: false. The strict property will be ignored."));
                }
                else
                {
                    includeStrict = true;
                }
            }

            var spec = new JsonObject { ["name"] = tool.Name };
            if (!string.IsNullOrWhiteSpace(tool.Description))
            {
                spec["description"] = tool.Description;
            }

            if (includeStrict)
            {
                spec["strict"] = tool.Strict!.Value;
            }

            spec["inputSchema"] = new JsonObject { ["json"] = JsonNode.Parse(tool.InputSchema.GetRawText()) };
            prepared.Add(new JsonObject { ["toolSpec"] = spec });
        }

        var config = new JsonObject { ["tools"] = prepared };
        if (toolChoice != null)
        {
            config["toolChoice"] = ToolChoiceObject(toolChoice);
        }

        return config;
    }

    private static bool SupportsStrictTools(string modelId)
    {
        if (string.IsNullOrEmpty(modelId))
        {
            return true;
        }

        foreach (var blocked in ModelsWithoutStrictTools)
        {
            if (modelId.IndexOf(blocked, StringComparison.Ordinal) >= 0)
            {
                return false;
            }
        }

        return true;
    }

    private static JsonObject ToolChoiceObject(ToolChoice toolChoice)
    {
        switch (toolChoice.Type)
        {
            case "required":
                return new JsonObject { ["any"] = new JsonObject() };
            case "tool":
                var named = (ToolChoice.NamedChoice)toolChoice;
                return new JsonObject { ["tool"] = new JsonObject { ["name"] = named.ToolName } };
            default:
                return new JsonObject { ["auto"] = new JsonObject() };
        }
    }

    private static bool IsStrictSchemaCompatible(JsonElement schema)
    {
        if (schema.ValueKind != JsonValueKind.Object)
        {
            return true;
        }

        if (IsObjectType(schema)
            && (!schema.TryGetProperty("additionalProperties", out var additional) || additional.ValueKind != JsonValueKind.False))
        {
            return false;
        }

        foreach (var name in new[] { "properties", "patternProperties", "definitions", "$defs" })
        {
            if (schema.TryGetProperty(name, out var map) && map.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in map.EnumerateObject())
                {
                    if (!IsStrictSchemaCompatible(property.Value))
                    {
                        return false;
                    }
                }
            }
        }

        if (schema.TryGetProperty("items", out var items))
        {
            if (items.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in items.EnumerateArray())
                {
                    if (!IsStrictSchemaCompatible(item))
                    {
                        return false;
                    }
                }
            }
            else if (!IsStrictSchemaCompatible(items))
            {
                return false;
            }
        }

        foreach (var name in new[] { "anyOf", "allOf", "oneOf" })
        {
            if (schema.TryGetProperty(name, out var alternatives) && alternatives.ValueKind == JsonValueKind.Array)
            {
                foreach (var alternative in alternatives.EnumerateArray())
                {
                    if (!IsStrictSchemaCompatible(alternative))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private static bool IsObjectType(JsonElement schema)
    {
        if (!schema.TryGetProperty("type", out var type))
        {
            return false;
        }

        if (type.ValueKind == JsonValueKind.String)
        {
            return type.GetString() == "object";
        }

        if (type.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in type.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String && item.GetString() == "object")
                {
                    return true;
                }
            }
        }

        return false;
    }
}
