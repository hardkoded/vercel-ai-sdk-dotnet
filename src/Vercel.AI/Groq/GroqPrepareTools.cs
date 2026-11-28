// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Groq;

/// <summary>Maps call tools and the tool choice to the Groq request. Maps to <c>prepareTools</c> in <c>groq-prepare-tools.ts</c>.</summary>
internal static class GroqPrepareTools
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

        /// <summary>Warnings about tools Groq does not support.</summary>
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

        var groqTools = new JsonArray();
        foreach (var tool in tools)
        {
            if (tool.IsProviderDefined)
            {
                if (tool.ProviderToolId == "groq.browser_search")
                {
                    if (GroqModelCatalog.SupportsBrowserSearch(modelId) == false)
                    {
                        toolWarnings.Add(
                            new CallWarning(
                                "unsupported",
                                "provider-defined tool " + tool.ProviderToolId,
                                $"Browser search is not supported on model {modelId}."
                            )
                        );
                    }
                    else
                    {
                        groqTools.Add(new JsonObject { ["type"] = "browser_search" });
                    }
                }
                else
                {
                    toolWarnings.Add(
                        new CallWarning(
                            "unsupported",
                            "provider-defined tool " + tool.ProviderToolId,
                            null
                        )
                    );
                }

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

            groqTools.Add(new JsonObject { ["type"] = "function", ["function"] = function });
        }

        if (toolChoice == null)
        {
            return new Prepared(groqTools, null, toolWarnings);
        }

        switch (toolChoice.Type)
        {
            case "auto":
            case "none":
            case "required":
                return new Prepared(groqTools, toolChoice.Type, toolWarnings);
            default:
                return new Prepared(
                    groqTools,
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
