// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.DeepSeek;

/// <summary>Maps the call tools and the tool choice to the DeepSeek request. Maps to <c>deepseek-prepare-tools.ts</c>.</summary>
internal static class DeepSeekPrepareTools
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

        /// <summary>Warnings about tools DeepSeek does not support.</summary>
        public List<CallWarning> ToolWarnings { get; }
    }

    /// <summary>Maps to <c>prepareTools</c>.</summary>
    /// <exception cref="UnsupportedFunctionalityException">Strict tool calls are not supported by the endpoint or are mixed with non-strict ones.</exception>
    public static Prepared PrepareTools(
        IReadOnlyList<LanguageModelTool>? tools,
        ToolChoice? toolChoice,
        bool? supportsStrictToolCalls
    )
    {
        var toolWarnings = new List<CallWarning>();
        if (tools == null || tools.Count == 0)
        {
            return new Prepared(null, null, toolWarnings);
        }

        var functionTools = tools.Where(tool => !tool.IsProviderDefined).ToList();
        var hasStrictTool = functionTools.Any(tool => tool.Strict == true);
        if (hasStrictTool && supportsStrictToolCalls == false)
        {
            throw new UnsupportedFunctionalityException(
                "DeepSeek strict tool calls",
                "DeepSeek strict tool calls require a beta base URL ending in `/beta`."
            );
        }

        if (
            hasStrictTool
            && supportsStrictToolCalls == true
            && functionTools.Any(tool => tool.Strict != true)
        )
        {
            throw new UnsupportedFunctionalityException(
                "mixed DeepSeek strict and non-strict tool calls",
                "DeepSeek strict mode requires every function tool in the request to set `strict: true`."
            );
        }

        var deepseekTools = new JsonArray();
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
            if (tool.Strict is { } strict)
            {
                function["strict"] = strict;
            }

            deepseekTools.Add(new JsonObject { ["type"] = "function", ["function"] = function });
        }

        if (toolChoice == null)
        {
            return new Prepared(deepseekTools, null, toolWarnings);
        }

        switch (toolChoice)
        {
            case ToolChoice.NamedChoice named:
                return new Prepared(
                    deepseekTools,
                    new JsonObject
                    {
                        ["type"] = "function",
                        ["function"] = new JsonObject { ["name"] = named.ToolName },
                    },
                    toolWarnings
                );
            default:
                if (toolChoice.Type is "auto" or "none" or "required")
                {
                    return new Prepared(deepseekTools, toolChoice.Type, toolWarnings);
                }

                toolWarnings.Add(
                    new CallWarning("unsupported", "tool choice type: " + toolChoice.Type, null)
                );
                return new Prepared(deepseekTools, null, toolWarnings);
        }
    }
}
