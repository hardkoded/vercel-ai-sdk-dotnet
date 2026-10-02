// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.AmazonBedrock;

/// <summary>A function or provider tool passed to Converse tool preparation.</summary>
public sealed class AmazonBedrockToolInput
{
    /// <summary>Creates a tool definition.</summary>
    public AmazonBedrockToolInput(string kind, string name)
    {
        Kind = kind ?? "function";
        Name = name ?? string.Empty;
    }

    /// <summary><c>function</c> or <c>provider</c>.</summary>
    public string Kind { get; }

    /// <summary>Tool name sent to Converse.</summary>
    public string Name { get; }

    /// <summary>Provider tool id, such as <c>anthropic.web_search_20250305</c>.</summary>
    public string? ProviderToolId { get; set; }

    /// <summary>Tool description. Empty and whitespace-only descriptions are omitted.</summary>
    public string? Description { get; set; }

    /// <summary>JSON Schema for the arguments.</summary>
    public JsonNode? InputSchema { get; set; }

    /// <summary>Strict mode. Null means unset.</summary>
    public bool? Strict { get; set; }

    /// <summary>Creates a function tool.</summary>
    public static AmazonBedrockToolInput Function(string name, string? description, JsonNode? inputSchema, bool? strict = null)
    {
        return new AmazonBedrockToolInput("function", name)
        {
            Description = description,
            InputSchema = inputSchema ?? new JsonObject(),
            Strict = strict,
        };
    }

    /// <summary>Creates a provider-defined tool.</summary>
    public static AmazonBedrockToolInput ProviderTool(string id, string name)
    {
        return new AmazonBedrockToolInput("provider", name)
        {
            ProviderToolId = id,
        };
    }
}

/// <summary>Tool configuration and warnings for one Converse call.</summary>
public sealed class AmazonBedrockPreparedTools
{
    /// <summary>Creates a prepared tool set.</summary>
    public AmazonBedrockPreparedTools(JsonObject toolConfig, JsonObject? additionalTools, IReadOnlyList<string> betas, IReadOnlyList<AmazonBedrockWarning> warnings)
    {
        ToolConfig = toolConfig ?? new JsonObject();
        AdditionalTools = additionalTools;
        Betas = betas ?? Array.Empty<string>();
        Warnings = warnings ?? Array.Empty<AmazonBedrockWarning>();
    }

    /// <summary>Converse <c>toolConfig</c>. Empty when no tools are sent.</summary>
    public JsonObject ToolConfig { get; }

    /// <summary>Fields merged into <c>additionalModelRequestFields</c>, such as Anthropic <c>tool_choice</c>.</summary>
    public JsonObject? AdditionalTools { get; }

    /// <summary>Anthropic beta flags required by the tools.</summary>
    public IReadOnlyList<string> Betas { get; }

    /// <summary>Warnings produced while preparing tools.</summary>
    public IReadOnlyList<AmazonBedrockWarning> Warnings { get; }
}

/// <summary>Builds the Converse <c>toolConfig</c> object.</summary>
public static class AmazonBedrockPrepareTools
{
    private static readonly string[] UnsupportedWebTools =
    {
        "anthropic.web_search_20250305",
        "anthropic.web_search_20260318",
        "anthropic.web_fetch_20260318",
    };

    /// <summary>Prepares <paramref name="tools"/> for <paramref name="modelId"/>.</summary>
    public static AmazonBedrockPreparedTools Prepare(
        IReadOnlyList<AmazonBedrockToolInput>? tools,
        ToolChoice? toolChoice,
        string modelId,
        string? modelFamily = null,
        int? reasoningBudgetTokens = null,
        bool disableParallelToolUse = false)
    {
        var warnings = new List<AmazonBedrockWarning>();
        if (tools == null || tools.Count == 0)
        {
            return Empty(warnings);
        }

        var supported = new List<AmazonBedrockToolInput>();
        foreach (var tool in tools)
        {
            if (tool.Kind == "provider" && IsUnsupportedWebTool(tool.ProviderToolId))
            {
                var toolType = StripPrefix(tool.ProviderToolId, "anthropic.");
                warnings.Add(new AmazonBedrockWarning(
                    "unsupported",
                    toolType + " tool",
                    "The " + toolType + " tool is not supported on Amazon Bedrock."));
                continue;
            }

            supported.Add(tool);
        }

        if (supported.Count == 0)
        {
            return Empty(warnings);
        }

        var isAnthropic = AmazonBedrockModelSupport.IsAnthropicModel(modelId, modelFamily, reasoningBudgetTokens);
        var providerTools = new List<AmazonBedrockToolInput>();
        var functionTools = new List<AmazonBedrockToolInput>();
        foreach (var tool in supported)
        {
            if (tool.Kind == "provider")
            {
                providerTools.Add(tool);
            }
            else
            {
                functionTools.Add(tool);
            }
        }

        var usingAnthropicTools = isAnthropic && providerTools.Count > 0;
        var specs = new JsonArray();
        JsonObject? additionalTools = null;

        if (usingAnthropicTools)
        {
            foreach (var tool in providerTools)
            {
                warnings.Add(new AmazonBedrockWarning("unsupported", "tool " + (tool.ProviderToolId ?? tool.Name), null));
            }
        }
        else
        {
            foreach (var tool in providerTools)
            {
                warnings.Add(new AmazonBedrockWarning("unsupported", "tool " + (tool.ProviderToolId ?? tool.Name), null));
            }
        }

        if (toolChoice is ToolChoice.NamedChoice named)
        {
            var filtered = new List<AmazonBedrockToolInput>();
            foreach (var tool in functionTools)
            {
                if (tool.Name == named.ToolName)
                {
                    filtered.Add(tool);
                }
            }

            functionTools = filtered;
        }

        var supportsStrict = AmazonBedrockModelSupport.SupportsStrictTools(modelId);
        foreach (var tool in functionTools)
        {
            var schemaCompatible = tool.Strict != true || IsStrictSchemaCompatible(tool.InputSchema);
            var supportsStrictForTool = supportsStrict && schemaCompatible;
            if (!supportsStrict && tool.Strict != null)
            {
                warnings.Add(new AmazonBedrockWarning(
                    "unsupported",
                    "strict",
                    "Tool '" + tool.Name + "' has strict: " + (tool.Strict.Value ? "true" : "false") + ", but strict mode is not supported by this model on Amazon Bedrock. The strict property will be ignored."));
            }
            else if (tool.Strict == true && !supportsStrictForTool)
            {
                warnings.Add(new AmazonBedrockWarning(
                    "unsupported",
                    "strict",
                    "Tool '" + tool.Name + "' has strict: true, but Amazon Bedrock requires every object in a strict tool schema to set additionalProperties: false. The strict property will be ignored."));
            }

            var spec = new JsonObject
            {
                ["name"] = tool.Name,
            };
            if (tool.Description != null && tool.Description.Trim().Length > 0)
            {
                spec["description"] = tool.Description;
            }

            if (tool.Strict != null && supportsStrictForTool)
            {
                spec["strict"] = tool.Strict.Value;
            }

            spec["inputSchema"] = new JsonObject
            {
                ["json"] = tool.InputSchema == null ? new JsonObject() : tool.InputSchema.DeepClone(),
            };
            specs.Add(new JsonObject { ["toolSpec"] = spec });
        }

        var choiceType = toolChoice == null ? null : toolChoice.Type;
        if (isAnthropic && !usingAnthropicTools && disableParallelToolUse && specs.Count > 0 && choiceType != "none")
        {
            JsonObject toolChoiceFields;
            if (choiceType == "required")
            {
                toolChoiceFields = new JsonObject
                {
                    ["type"] = "any",
                    ["disable_parallel_tool_use"] = true,
                };
            }
            else if (toolChoice is ToolChoice.NamedChoice namedChoice)
            {
                toolChoiceFields = new JsonObject
                {
                    ["type"] = "tool",
                    ["name"] = namedChoice.ToolName,
                    ["disable_parallel_tool_use"] = true,
                };
            }
            else
            {
                toolChoiceFields = new JsonObject
                {
                    ["type"] = "auto",
                    ["disable_parallel_tool_use"] = true,
                };
            }

            additionalTools = new JsonObject
            {
                ["tool_choice"] = toolChoiceFields,
            };
        }

        JsonNode? bedrockChoice = null;
        if (!usingAnthropicTools && (additionalTools == null || additionalTools["tool_choice"] == null) && specs.Count > 0 && toolChoice != null)
        {
            switch (toolChoice.Type)
            {
                case "auto":
                    bedrockChoice = new JsonObject { ["auto"] = new JsonObject() };
                    break;
                case "required":
                    bedrockChoice = new JsonObject { ["any"] = new JsonObject() };
                    break;
                case "none":
                    specs.Clear();
                    bedrockChoice = null;
                    break;
                case "tool":
                    bedrockChoice = new JsonObject
                    {
                        ["tool"] = new JsonObject { ["name"] = ((ToolChoice.NamedChoice)toolChoice).ToolName },
                    };
                    break;
                default:
                    throw new AmazonBedrockUnsupportedException("tool choice type: " + toolChoice.Type);
            }
        }

        if (specs.Count == 0)
        {
            return new AmazonBedrockPreparedTools(new JsonObject(), additionalTools, Array.Empty<string>(), warnings);
        }

        var toolConfig = new JsonObject
        {
            ["tools"] = specs,
        };
        if (bedrockChoice != null)
        {
            toolConfig["toolChoice"] = bedrockChoice;
        }

        return new AmazonBedrockPreparedTools(toolConfig, additionalTools, Array.Empty<string>(), warnings);
    }

    /// <summary>Returns whether every object in <paramref name="schema"/> sets <c>additionalProperties</c> to false.</summary>
    public static bool IsStrictSchemaCompatible(JsonNode? schema)
    {
        if (schema == null || schema is JsonValue)
        {
            return true;
        }

        if (schema is not JsonObject obj)
        {
            return true;
        }

        var type = obj["type"];
        var isObject = false;
        if (type is JsonValue typeValue && typeValue.TryGetValue<string>(out var typeName))
        {
            isObject = typeName == "object";
        }
        else if (type is JsonArray typeArray)
        {
            foreach (var item in typeArray)
            {
                if (item is JsonValue itemValue && itemValue.TryGetValue<string>(out var itemName) && itemName == "object")
                {
                    isObject = true;
                    break;
                }
            }
        }

        if (isObject && !IsFalse(obj["additionalProperties"]))
        {
            return false;
        }

        foreach (var mapName in new[] { "properties", "patternProperties", "definitions", "$defs" })
        {
            if (obj[mapName] is JsonObject map)
            {
                foreach (var pair in map)
                {
                    if (!IsStrictSchemaCompatible(pair.Value))
                    {
                        return false;
                    }
                }
            }
        }

        if (obj["dependencies"] is JsonObject dependencies)
        {
            foreach (var pair in dependencies)
            {
                if (pair.Value is JsonArray)
                {
                    continue;
                }

                if (!IsStrictSchemaCompatible(pair.Value))
                {
                    return false;
                }
            }
        }

        foreach (var nestedName in new[] { "propertyNames", "contains", "not", "if", "then", "else" })
        {
            if (obj[nestedName] != null && !IsStrictSchemaCompatible(obj[nestedName]))
            {
                return false;
            }
        }

        if (obj["items"] is JsonArray items)
        {
            foreach (var item in items)
            {
                if (!IsStrictSchemaCompatible(item))
                {
                    return false;
                }
            }
        }
        else if (obj["items"] != null && !IsStrictSchemaCompatible(obj["items"]))
        {
            return false;
        }

        foreach (var alternativesName in new[] { "anyOf", "allOf", "oneOf" })
        {
            if (obj[alternativesName] is JsonArray alternatives)
            {
                foreach (var alternative in alternatives)
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

    private static bool IsFalse(JsonNode? node)
    {
        return node is JsonValue value && value.TryGetValue<bool>(out var flag) && flag == false;
    }

    private static bool IsUnsupportedWebTool(string? id)
    {
        if (id == null)
        {
            return false;
        }

        foreach (var tool in UnsupportedWebTools)
        {
            if (tool == id)
            {
                return true;
            }
        }

        return false;
    }

    private static string StripPrefix(string? value, string prefix)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value.IndexOf(prefix, StringComparison.Ordinal) == 0)
        {
            return value.Substring(prefix.Length);
        }

        return value;
    }

    private static AmazonBedrockPreparedTools Empty(List<AmazonBedrockWarning> warnings)
    {
        return new AmazonBedrockPreparedTools(new JsonObject(), null, Array.Empty<string>(), warnings);
    }
}
