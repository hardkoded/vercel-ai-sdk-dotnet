// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Alibaba;

/// <summary>Maps call tools and the tool choice to the Alibaba request. Maps to <c>prepareTools</c> in <c>alibaba-prepare-tools.ts</c>.</summary>
internal static class AlibabaPrepareTools
{
    /// <summary>The prepared <c>tools</c>, <c>tool_choice</c>, and the warnings the tools raised.</summary>
    internal sealed class Prepared
    {
        public Prepared(JsonArray? tools, JsonNode? toolChoice, List<CallWarning> toolWarnings)
        {
            Tools = tools;
            ToolChoice = toolChoice;
            ToolWarnings = toolWarnings;
        }

        /// <summary>The <c>tools</c> array, or null when there are no tools.</summary>
        public JsonArray? Tools { get; }

        /// <summary>The <c>tool_choice</c> value, or null when none is sent.</summary>
        public JsonNode? ToolChoice { get; }

        /// <summary>Warnings about tools Alibaba does not support.</summary>
        public List<CallWarning> ToolWarnings { get; }
    }

    public static Prepared Prepare(IReadOnlyList<LanguageModelTool>? tools, ToolChoice? toolChoice)
    {
        var toolWarnings = new List<CallWarning>();
        if (tools == null || tools.Count == 0)
        {
            return new Prepared(null, null, toolWarnings);
        }

        var alibabaTools = new JsonArray();
        foreach (var tool in tools)
        {
            if (tool.IsProviderDefined)
            {
                toolWarnings.Add(new CallWarning("unsupported", "provider-defined tool " + tool.ProviderToolId, null));
                continue;
            }

            var function = new JsonObject { ["name"] = tool.Name };
            if (tool.Description != null)
            {
                function["description"] = tool.Description;
            }

            function["parameters"] = JsonNode.Parse(tool.InputSchema.GetRawText());
            if (tool.Strict is { } strict)
            {
                function["strict"] = strict;
            }

            alibabaTools.Add(new JsonObject { ["type"] = "function", ["function"] = function });
        }

        return new Prepared(alibabaTools, toolChoice == null ? null : MapToolChoice(toolChoice), toolWarnings);
    }

    private static JsonNode MapToolChoice(ToolChoice toolChoice)
    {
        switch (toolChoice)
        {
            case ToolChoice.NamedChoice named:
                return new JsonObject
                {
                    ["type"] = "function",
                    ["function"] = new JsonObject { ["name"] = named.ToolName },
                };
            case { Type: "auto" or "none" or "required" }:
                return toolChoice.Type;
            default:
                throw OpenAICompatibleChat.Unsupported("tool choice type: " + toolChoice.Type);
        }
    }
}
