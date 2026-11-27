// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Cohere;

/// <summary>Maps call tools and the tool choice to the Cohere request. Maps to <c>prepareTools</c> in <c>cohere-prepare-tools.ts</c>.</summary>
internal static class CoherePrepareTools
{
    /// <summary>The prepared <c>tools</c>, <c>tool_choice</c>, and the warnings the tools raised.</summary>
    internal sealed class Prepared
    {
        public Prepared(JsonArray? tools, string? toolChoice, List<CallWarning> toolWarnings)
        {
            Tools = tools;
            ToolChoice = toolChoice;
            ToolWarnings = toolWarnings;
        }

        /// <summary>The <c>tools</c> array, or null when there are no tools.</summary>
        public JsonArray? Tools { get; }

        /// <summary>The <c>tool_choice</c> value (<c>NONE</c> or <c>REQUIRED</c>), or null when none is sent.</summary>
        public string? ToolChoice { get; }

        /// <summary>Warnings about tools Cohere does not support.</summary>
        public List<CallWarning> ToolWarnings { get; }
    }

    public static Prepared Prepare(IReadOnlyList<LanguageModelTool>? tools, ToolChoice? toolChoice)
    {
        var toolWarnings = new List<CallWarning>();

        // When the tools array is empty, it becomes undefined to prevent errors.
        if (tools == null || tools.Count == 0)
        {
            return new Prepared(null, null, toolWarnings);
        }

        var cohereTools = new List<(string Name, JsonObject Tool)>();
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
            cohereTools.Add(
                (tool.Name, new JsonObject { ["type"] = "function", ["function"] = function })
            );
        }

        JsonArray ToArray(Func<string, bool> keep)
        {
            return new JsonArray(
                cohereTools
                    .Where(item => keep(item.Name))
                    .Select(item => (JsonNode?)item.Tool)
                    .ToArray()
            );
        }

        switch (toolChoice)
        {
            case null:
            case { Type: "auto" }:
                return new Prepared(ToArray(_ => true), null, toolWarnings);
            case { Type: "none" }:
                return new Prepared(ToArray(_ => true), "NONE", toolWarnings);
            case { Type: "required" }:
                return new Prepared(ToArray(_ => true), "REQUIRED", toolWarnings);
            case ToolChoice.NamedChoice named:
                return new Prepared(
                    ToArray(name => name == named.ToolName),
                    "REQUIRED",
                    toolWarnings
                );
            default:
                throw OpenAICompatibleChat.Unsupported("tool choice type: " + toolChoice.Type);
        }
    }
}
