// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenAI;

/// <summary>A non-function tool passed to Chat Completions preparation.</summary>
public sealed class OpenAIUnsupportedTool
{
    /// <summary>Creates an unsupported tool description.</summary>
    public OpenAIUnsupportedTool(string type, string name)
    {
        Type = type;
        Name = name;
    }

    /// <summary>Tool type recorded in the warning.</summary>
    public string Type { get; }

    /// <summary>Tool name.</summary>
    public string Name { get; }
}

/// <summary>Chat Completions tools and tool choice.</summary>
public sealed class OpenAIChatToolPreparation
{
    internal OpenAIChatToolPreparation(JsonArray? tools, JsonNode? toolChoice, IReadOnlyList<OpenAICallWarning> warnings)
    {
        Tools = tools;
        ToolChoice = toolChoice;
        Warnings = warnings;
    }

    /// <summary>Tool objects. Null when the caller passed no tools.</summary>
    public JsonArray? Tools { get; }

    /// <summary>OpenAI <c>tool_choice</c>. Null when the caller did not set one.</summary>
    public JsonNode? ToolChoice { get; }

    /// <summary>Warnings produced while preparing tools.</summary>
    public IReadOnlyList<OpenAICallWarning> Warnings { get; }
}

/// <summary>Prepares Chat Completions function tools.</summary>
public static class OpenAIChatTools
{
    /// <summary>Prepares function tools. Strict is omitted when <see cref="LanguageModelTool.Strict"/> is null.</summary>
    public static OpenAIChatToolPreparation Prepare(IReadOnlyList<LanguageModelTool>? tools, ToolChoice? toolChoice)
    {
        return Prepare(tools, null, toolChoice);
    }

    /// <summary>Prepares function tools and records unsupported tool types.</summary>
    public static OpenAIChatToolPreparation Prepare(
        IReadOnlyList<LanguageModelTool>? tools,
        IReadOnlyList<OpenAIUnsupportedTool>? unsupported,
        ToolChoice? toolChoice)
    {
        var functionCount = tools?.Count ?? 0;
        var otherCount = unsupported?.Count ?? 0;
        if (functionCount == 0 && otherCount == 0)
        {
            return new OpenAIChatToolPreparation(null, null, Array.Empty<OpenAICallWarning>());
        }

        var warnings = new List<OpenAICallWarning>();
        var prepared = new JsonArray();
        if (tools != null)
        {
            foreach (var tool in tools)
            {
                var normalized = OpenAIJsonSchema.Normalize(tool.InputSchema);
                warnings.AddRange(normalized.Warnings);
                var function = new JsonObject
                {
                    ["name"] = tool.Name,
                    ["parameters"] = normalized.Schema,
                };
                if (tool.Description != null)
                {
                    function["description"] = tool.Description;
                }

                if (tool.Strict != null)
                {
                    function["strict"] = tool.Strict.Value;
                }

                prepared.Add(new JsonObject { ["type"] = "function", ["function"] = function });
            }
        }

        if (unsupported != null)
        {
            foreach (var tool in unsupported)
            {
                warnings.Add(new OpenAICallWarning("unsupported", "tool type: " + tool.Type, null));
            }
        }

        return new OpenAIChatToolPreparation(prepared, MapChoice(toolChoice), warnings);
    }

    private static JsonNode? MapChoice(ToolChoice? toolChoice)
    {
        if (toolChoice == null)
        {
            return null;
        }

        if (toolChoice.Type == "auto" || toolChoice.Type == "none" || toolChoice.Type == "required")
        {
            return JsonValue.Create(toolChoice.Type);
        }

        if (toolChoice is ToolChoice.NamedChoice named)
        {
            return new JsonObject
            {
                ["type"] = "function",
                ["function"] = new JsonObject { ["name"] = named.ToolName },
            };
        }

        throw new AiSdkException("tool choice type: " + toolChoice.Type);
    }
}
