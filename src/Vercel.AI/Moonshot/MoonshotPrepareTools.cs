// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Moonshot;

/// <summary>Maps call tools and the tool choice to the Moonshot AI request. Maps to <c>prepareTools</c> in <c>moonshotai-prepare-tools.ts</c>.</summary>
internal static class MoonshotPrepareTools
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

        /// <summary>Warnings about tools and tool choices Moonshot AI does not support.</summary>
        public List<CallWarning> ToolWarnings { get; }
    }

    /// <summary>Maps to <c>prepareTools</c>.</summary>
    public static Prepared PrepareTools(
        string modelId,
        IReadOnlyList<LanguageModelTool>? tools,
        ToolChoice? toolChoice = null
    )
    {
        var toolWarnings = new List<CallWarning>();
        if (tools == null || tools.Count == 0)
        {
            return new Prepared(null, null, toolWarnings);
        }

        var moonshotTools = new JsonArray();
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

            function["parameters"] = MoonshotJsonSchema.NormalizeJsonSchemaForMFJS(
                JsonNode.Parse(tool.InputSchema.GetRawText())
            );
            if (tool.Strict is { } strict)
            {
                function["strict"] = strict;
            }

            moonshotTools.Add(new JsonObject { ["type"] = "function", ["function"] = function });
        }

        if (toolChoice == null)
        {
            return new Prepared(moonshotTools, null, toolWarnings);
        }

        switch (toolChoice.Type)
        {
            case "auto":
            case "none":
                return new Prepared(moonshotTools, toolChoice.Type, toolWarnings);
            case "required":
                if (!MoonshotModelCatalog.SupportsRequiredToolChoice(modelId))
                {
                    toolWarnings.Add(
                        new CallWarning(
                            "unsupported",
                            $"tool choice \"required\" for model \"{modelId}\"",
                            "Moonshot AI rejects required tool choice for this model. The setting has been omitted; use \"auto\" or select a specific tool instead."
                        )
                    );
                    return new Prepared(moonshotTools, null, toolWarnings);
                }

                return new Prepared(moonshotTools, "required", toolWarnings);
            default:
                return new Prepared(
                    moonshotTools,
                    new JsonObject
                    {
                        ["type"] = "function",
                        ["function"] = new JsonObject
                        {
                            ["name"] = ((ToolChoice.NamedChoice)toolChoice).ToolName,
                        },
                    },
                    toolWarnings
                );
        }
    }
}
