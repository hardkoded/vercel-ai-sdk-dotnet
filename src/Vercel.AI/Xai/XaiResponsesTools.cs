// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Xai;

/// <summary>Function tools and tool choice for an xAI Responses request. Maps to <c>prepareResponsesTools</c>.</summary>
internal static class XaiResponsesTools
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
            var function = new JsonObject { ["type"] = "function", ["name"] = tool.Name };
            if (tool.Description != null)
            {
                function["description"] = tool.Description;
            }

            function["parameters"] = tool.InputSchema.ValueKind == JsonValueKind.Undefined
                ? new JsonObject()
                : JsonNode.Parse(tool.InputSchema.GetRawText());
            if (tool.Strict is { } strict)
            {
                function["strict"] = strict;
            }

            prepared.Add(function);
        }

        return new Prepared(prepared, MapToolChoice(toolChoice, tools), warnings);
    }

    private static JsonNode? MapToolChoice(ToolChoice? toolChoice, IReadOnlyList<LanguageModelTool> tools)
    {
        switch (toolChoice)
        {
            case null:
                return null;
            case ToolChoice.NamedChoice named:
                // A name that matches no tool sends no choice.
                return tools.Any(tool => tool.Name == named.ToolName)
                    ? new JsonObject { ["type"] = "function", ["name"] = named.ToolName }
                    : null;
            default:
                return JsonValue.Create(toolChoice.Type);
        }
    }
}
