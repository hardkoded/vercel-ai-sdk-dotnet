// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Mistral;

/// <summary>Tools and tool choice ready for a chat request.</summary>
internal sealed class MistralPreparedTools
{
    /// <summary>Creates a result.</summary>
    public MistralPreparedTools(
        JsonArray? tools,
        JsonNode? toolChoice,
        IReadOnlyList<CallWarning> toolWarnings
    )
    {
        Tools = tools;
        ToolChoice = toolChoice;
        ToolWarnings = toolWarnings;
    }

    /// <summary>The <c>tools</c> array. Null when there are no tools.</summary>
    public JsonArray? Tools { get; }

    /// <summary>The <c>tool_choice</c> value: <c>auto</c>, <c>none</c>, <c>any</c>, or a function selection.</summary>
    public JsonNode? ToolChoice { get; }

    /// <summary>Warnings for tools Mistral does not support.</summary>
    public IReadOnlyList<CallWarning> ToolWarnings { get; }
}

/// <summary>Prepares chat tools. Maps to <c>mistral-prepare-tools.ts</c>.</summary>
internal static class MistralPrepareTools
{
    /// <summary>Prepares the tools and tool choice. Maps to <c>prepareTools</c>.</summary>
    public static MistralPreparedTools PrepareTools(
        IReadOnlyList<LanguageModelTool>? tools,
        ToolChoice? toolChoice = null
    )
    {
        // When the tools array is empty, treat it as undefined to prevent errors.
        if (tools is not { Count: > 0 })
        {
            return new MistralPreparedTools(null, null, Array.Empty<CallWarning>());
        }

        var toolWarnings = new List<CallWarning>();
        var mistralTools = new JsonArray();
        foreach (var tool in tools)
        {
            if (tool.IsProviderDefined)
            {
                toolWarnings.Add(
                    new CallWarning(
                        "unsupported",
                        "provider-defined tool " + tool.ProviderToolId,
                        null
                    )
                );
                continue;
            }

            var function = new JsonObject { ["name"] = tool.Name };
            if (tool.Description != null)
            {
                function["description"] = tool.Description;
            }

            function["parameters"] = JsonNode.Parse(tool.InputSchema.GetRawText());
            if (tool.Strict != null)
            {
                function["strict"] = tool.Strict;
            }

            mistralTools.Add(new JsonObject { ["type"] = "function", ["function"] = function });
        }

        JsonNode? choice = null;
        if (toolChoice != null)
        {
            choice = toolChoice.Type switch
            {
                "auto" or "none" => toolChoice.Type,
                "required" => "any",
                "tool" => new JsonObject
                {
                    ["type"] = "function",
                    ["function"] = new JsonObject
                    {
                        ["name"] = ((ToolChoice.NamedChoice)toolChoice).ToolName,
                    },
                },
                _ => throw new UnsupportedFunctionalityException(
                    "tool choice type: " + toolChoice.Type,
                    "'tool choice type: " + toolChoice.Type + "' functionality not supported."
                ),
            };
        }

        return new MistralPreparedTools(mistralTools, choice, toolWarnings);
    }
}
