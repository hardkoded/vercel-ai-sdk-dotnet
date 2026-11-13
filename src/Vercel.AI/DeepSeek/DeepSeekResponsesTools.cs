// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.DeepSeek;

/// <summary>Function tools, the web search tool, and tool choice of a DeepSeek Responses request. Maps to <c>prepareDeepSeekResponsesTools</c>.</summary>
internal static class DeepSeekResponsesTools
{
    internal sealed class Prepared
    {
        public Prepared(JsonArray? tools, JsonNode? toolChoice, List<CallWarning> warnings)
        {
            Tools = tools;
            ToolChoice = toolChoice;
            Warnings = warnings;
        }

        /// <summary>The <c>tools</c> array, or null when there are no tools.</summary>
        public JsonArray? Tools { get; }

        /// <summary>The <c>tool_choice</c> value, or null when none is sent.</summary>
        public JsonNode? ToolChoice { get; }

        public List<CallWarning> Warnings { get; }
    }

    public static Prepared Prepare(IReadOnlyList<LanguageModelTool>? tools, ToolChoice? toolChoice)
    {
        var warnings = new List<CallWarning>();
        if (tools == null || tools.Count == 0)
        {
            return new Prepared(null, null, warnings);
        }

        var prepared = new JsonArray();
        foreach (var tool in tools)
        {
            if (tool.IsProviderDefined)
            {
                if (tool.ProviderToolId == DeepSeekTools.WebSearchToolId)
                {
                    prepared.Add(new JsonObject { ["type"] = "web_search" });
                }
                else
                {
                    warnings.Add(new CallWarning("unsupported", "provider-defined tool " + tool.ProviderToolId, null));
                }

                continue;
            }

            if (tool.Strict != null)
            {
                warnings.Add(new CallWarning("unsupported", "strict tool calls", "The DeepSeek Responses API does not support strict mode."));
            }

            var function = new JsonObject { ["type"] = "function", ["name"] = tool.Name };
            if (tool.Description != null)
            {
                function["description"] = tool.Description;
            }

            function["parameters"] = tool.InputSchema.ValueKind == JsonValueKind.Undefined
                ? new JsonObject()
                : JsonNode.Parse(tool.InputSchema.GetRawText());
            prepared.Add(function);
        }

        return new Prepared(prepared, MapToolChoice(toolChoice, tools, warnings), warnings);
    }

    private static JsonNode? MapToolChoice(ToolChoice? toolChoice, IReadOnlyList<LanguageModelTool> tools, List<CallWarning> warnings)
    {
        switch (toolChoice)
        {
            case null:
                return null;
            case ToolChoice.NamedChoice named:
                var selected = tools.FirstOrDefault(tool => tool.Name == named.ToolName);
                if (selected is { IsProviderDefined: true })
                {
                    warnings.Add(new CallWarning("unsupported", "toolChoice for provider-defined tool " + selected.ProviderToolId, null));
                    return null;
                }

                return new JsonObject { ["type"] = "function", ["name"] = named.ToolName };
            default:
                return JsonValue.Create(toolChoice.Type);
        }
    }
}
