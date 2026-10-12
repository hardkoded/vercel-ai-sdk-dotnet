// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Xai;

/// <summary>Function tools, xAI provider-defined tools, and tool choice for an xAI Responses request. Maps to <c>prepareResponsesTools</c>.</summary>
internal static class XaiResponsesTools
{
    /// <summary>Arguments of <c>xai.web_search</c>. Maps to <c>webSearchArgsSchema</c>.</summary>
    internal static readonly JsonNode WebSearchArgsSchema = JsonNode.Parse("""
        {"type":"object","properties":{
          "allowedDomains":{"type":"array","items":{"type":"string"},"maxItems":5},
          "excludedDomains":{"type":"array","items":{"type":"string"},"maxItems":5},
          "enableImageSearch":{"type":"boolean"},
          "enableImageUnderstanding":{"type":"boolean"}}}
        """)!;

    /// <summary>Arguments of <c>xai.x_search</c>. Maps to <c>xSearchArgsSchema</c>.</summary>
    internal static readonly JsonNode XSearchArgsSchema = JsonNode.Parse("""
        {"type":"object","properties":{
          "allowedXHandles":{"type":"array","items":{"type":"string"},"maxItems":20},
          "excludedXHandles":{"type":"array","items":{"type":"string"},"maxItems":20},
          "fromDate":{"type":"string"},
          "toDate":{"type":"string"},
          "enableImageUnderstanding":{"type":"boolean"},
          "enableVideoUnderstanding":{"type":"boolean"}}}
        """)!;

    /// <summary>Arguments of <c>xai.image_generation</c>. Maps to <c>imageGenerationArgsSchema</c>.</summary>
    internal static readonly JsonNode ImageGenerationArgsSchema = JsonNode.Parse("""
        {"type":"object","properties":{"action":{"type":"string","enum":["auto","generate","edit"]}}}
        """)!;

    /// <summary>Arguments of <c>xai.file_search</c>. Maps to <c>fileSearchArgsSchema</c>.</summary>
    internal static readonly JsonNode FileSearchArgsSchema = JsonNode.Parse("""
        {"type":"object","properties":{
          "vectorStoreIds":{"type":"array","items":{"type":"string"}},
          "maxNumResults":{"type":"number"}},
         "required":["vectorStoreIds"]}
        """)!;

    /// <summary>Arguments of <c>xai.mcp</c>. Maps to <c>mcpServerArgsSchema</c>.</summary>
    internal static readonly JsonNode McpServerArgsSchema = JsonNode.Parse("""
        {"type":"object","properties":{
          "serverUrl":{"type":"string"},
          "serverLabel":{"type":"string"},
          "serverDescription":{"type":"string"},
          "allowedTools":{"type":"array","items":{"type":"string"}},
          "headers":{"type":"object","properties":{},"additionalProperties":{"type":"string"}},
          "authorization":{"type":"string"}},
         "required":["serverUrl"]}
        """)!;

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

    /// <exception cref="TypeValidationException">The arguments of an xAI provider-defined tool are invalid.</exception>
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
                if (ProviderTool(tool) is { } providerTool)
                {
                    prepared.Add(providerTool);
                }
                else
                {
                    warnings.Add(new CallWarning("unsupported", "provider-defined tool " + tool.Name, null));
                }

                continue;
            }

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

        return new Prepared(prepared, MapToolChoice(toolChoice, tools, warnings), warnings);
    }

    /// <summary>The name of the first provider-defined tool with <paramref name="id"/>, or null when there is none.</summary>
    public static string? ProviderToolName(IReadOnlyList<LanguageModelTool>? tools, string id)
    {
        return tools?.FirstOrDefault(tool => tool.ProviderToolId == id)?.Name;
    }

    private static JsonObject? ProviderTool(LanguageModelTool tool)
    {
        switch (tool.ProviderToolId)
        {
            case "xai.web_search":
                {
                    var args = Validate(tool, WebSearchArgsSchema);
                    return Wire(
                        "web_search",
                        ("allowed_domains", args["allowedDomains"]),
                        ("excluded_domains", args["excludedDomains"]),
                        ("enable_image_search", args["enableImageSearch"]),
                        ("enable_image_understanding", args["enableImageUnderstanding"]));
                }

            case "xai.x_search":
                {
                    var args = Validate(tool, XSearchArgsSchema);
                    return Wire(
                        "x_search",
                        ("allowed_x_handles", args["allowedXHandles"]),
                        ("excluded_x_handles", args["excludedXHandles"]),
                        ("from_date", args["fromDate"]),
                        ("to_date", args["toDate"]),
                        ("enable_image_understanding", args["enableImageUnderstanding"]),
                        ("enable_video_understanding", args["enableVideoUnderstanding"]));
                }

            case "xai.code_execution":
                return Wire("code_interpreter");
            case "xai.view_image":
                return Wire("view_image");
            case "xai.view_x_video":
                return Wire("view_x_video");
            case "xai.image_generation":
                return Wire("image_generation", ("action", Validate(tool, ImageGenerationArgsSchema)["action"]));
            case "xai.file_search":
                {
                    var args = Validate(tool, FileSearchArgsSchema);
                    return Wire(
                        "file_search",
                        ("vector_store_ids", args["vectorStoreIds"]),
                        ("max_num_results", args["maxNumResults"]));
                }

            case "xai.mcp":
                {
                    var args = Validate(tool, McpServerArgsSchema);
                    return Wire(
                        "mcp",
                        ("server_url", args["serverUrl"]),
                        ("server_label", args["serverLabel"]),
                        ("server_description", args["serverDescription"]),
                        ("allowed_tools", args["allowedTools"]),
                        ("headers", args["headers"]),
                        ("authorization", args["authorization"]));
                }

            default:
                return null;
        }
    }

    private static JsonObject Validate(LanguageModelTool tool, JsonNode schema)
    {
        var args = tool.Args.ValueKind == JsonValueKind.Undefined ? OpenAI.OpenAIJson.Parse("{}") : tool.Args;
        return (JsonObject)JsonNode.Parse(JsonParsing.ValidateTypes(args, schema).GetRawText())!;
    }

    /// <summary>A wire tool of <paramref name="type"/>. Fields whose value is missing are left out.</summary>
    private static JsonObject Wire(string type, params (string Name, JsonNode? Value)[] fields)
    {
        var wire = new JsonObject { ["type"] = type };
        foreach (var (name, value) in fields)
        {
            if (value != null)
            {
                wire[name] = value.DeepClone();
            }
        }

        return wire;
    }

    private static JsonNode? MapToolChoice(ToolChoice? toolChoice, IReadOnlyList<LanguageModelTool> tools, List<CallWarning> warnings)
    {
        switch (toolChoice)
        {
            case null:
                return null;
            case ToolChoice.NamedChoice named:
                // A name that matches no tool sends no choice. A later tool with the same name wins.
                var selected = tools.LastOrDefault(tool => tool.Name == named.ToolName);
                if (selected == null)
                {
                    return null;
                }

                if (selected.IsProviderDefined)
                {
                    // xAI can force only function tools.
                    warnings.Add(new CallWarning("unsupported", "toolChoice for server-side tool \"" + selected.Name + "\"", null));
                    return null;
                }

                return new JsonObject { ["type"] = "function", ["name"] = named.ToolName };
            default:
                return JsonValue.Create(toolChoice.Type);
        }
    }
}
