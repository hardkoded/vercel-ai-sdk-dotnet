// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>Result of <see cref="GoogleInteractionsTools.Prepare"/>: the <c>tools</c> and <c>tool_choice</c> request fields and the warnings.</summary>
public sealed class PrepareGoogleInteractionsToolsResult
{
    /// <summary>Creates a result.</summary>
    public PrepareGoogleInteractionsToolsResult(JsonArray? tools, JsonNode? toolChoice, IReadOnlyList<CallWarning> toolWarnings)
    {
        Tools = tools;
        ToolChoice = toolChoice;
        ToolWarnings = toolWarnings;
    }

    /// <summary>The Interactions <c>tools[]</c>, or null when there is none.</summary>
    public JsonArray? Tools { get; }

    /// <summary>The Interactions <c>tool_choice</c>, or null when it is omitted.</summary>
    public JsonNode? ToolChoice { get; }

    /// <summary>Warnings of dropped tools.</summary>
    public IReadOnlyList<CallWarning> ToolWarnings { get; }
}

/// <summary>
/// Maps tool definitions and <c>toolChoice</c> onto the Gemini Interactions <c>tools[]</c> and <c>tool_choice</c> request fields.
/// Port of <c>prepare-google-interactions-tools.ts</c>.
/// </summary>
public static class GoogleInteractionsTools
{
    /// <summary>Maps <paramref name="tools"/> and <paramref name="toolChoice"/>. <c>toolChoice: none</c> omits every tool.</summary>
    public static PrepareGoogleInteractionsToolsResult Prepare(IReadOnlyList<LanguageModelTool>? tools, ToolChoice? toolChoice = null)
    {
        var toolWarnings = new List<CallWarning>();

        // The API's tool_choice only controls function tools. Omit all tools to disable provider-executed tools as well.
        if (toolChoice?.Type == "none" || tools == null || tools.Count == 0)
        {
            return new PrepareGoogleInteractionsToolsResult(null, null, toolWarnings);
        }

        var interactionsTools = new JsonArray();
        var hasFunctionTool = false;
        foreach (var tool in tools)
        {
            if (!tool.IsProviderDefined)
            {
                hasFunctionTool = true;
                interactionsTools.Add(new JsonObject
                {
                    ["type"] = "function",
                    ["name"] = tool.Name,
                    ["description"] = tool.Description ?? string.Empty,
                    ["parameters"] = JsonNode.Parse(tool.InputSchema.GetRawText()),
                });
                continue;
            }

            var args = tool.Args.ValueKind == JsonValueKind.Object ? tool.Args : default;
            switch (tool.ProviderToolId)
            {
                case "google.google_search":
                    {
                        var entry = new JsonObject { ["type"] = "google_search" };
                        if (Arg(args, "searchTypes") is { ValueKind: JsonValueKind.Object } searchTypes)
                        {
                            var list = new JsonArray();
                            if (Arg(searchTypes, "webSearch") != null)
                            {
                                list.Add("web_search");
                            }

                            if (Arg(searchTypes, "imageSearch") != null)
                            {
                                list.Add("image_search");
                            }

                            if (list.Count > 0)
                            {
                                entry["search_types"] = list;
                            }
                        }

                        interactionsTools.Add(entry);
                        break;
                    }

                case "google.code_execution":
                    interactionsTools.Add(new JsonObject { ["type"] = "code_execution" });
                    break;
                case "google.url_context":
                    interactionsTools.Add(new JsonObject { ["type"] = "url_context" });
                    break;
                case "google.file_search":
                    {
                        var entry = new JsonObject { ["type"] = "file_search" };
                        Copy(entry, "file_search_store_names", args, "fileSearchStoreNames");
                        Copy(entry, "top_k", args, "topK");
                        Copy(entry, "metadata_filter", args, "metadataFilter");
                        interactionsTools.Add(entry);
                        break;
                    }

                case "google.google_maps":
                    {
                        var entry = new JsonObject { ["type"] = "google_maps" };
                        Copy(entry, "latitude", args, "latitude");
                        Copy(entry, "longitude", args, "longitude");
                        Copy(entry, "enable_widget", args, "enableWidget");
                        interactionsTools.Add(entry);
                        break;
                    }

                case "google.computer_use":
                    {
                        var entry = new JsonObject { ["type"] = "computer_use", ["environment"] = "browser" };
                        Copy(entry, "environment", args, "environment");
                        Copy(entry, "excludedPredefinedFunctions", args, "excludedPredefinedFunctions");
                        interactionsTools.Add(entry);
                        break;
                    }

                case "google.mcp_server":
                    {
                        var entry = new JsonObject { ["type"] = "mcp_server" };
                        Copy(entry, "name", args, "name");
                        Copy(entry, "url", args, "url");
                        Copy(entry, "headers", args, "headers");
                        Copy(entry, "allowed_tools", args, "allowedTools");
                        interactionsTools.Add(entry);
                        break;
                    }

                case "google.retrieval":
                    {
                        var entry = new JsonObject { ["type"] = "retrieval", ["retrieval_types"] = new JsonArray("vertex_ai_search") };
                        Copy(entry, "retrieval_types", args, "retrievalTypes");
                        Copy(entry, "vertex_ai_search_config", args, "vertexAiSearchConfig");
                        interactionsTools.Add(entry);
                        break;
                    }

                default:
                    toolWarnings.Add(new CallWarning("unsupported", "provider-defined tool " + tool.ProviderToolId, "provider-defined tool " + tool.ProviderToolId + " is not supported by google.interactions; tool dropped."));
                    break;
            }
        }

        // The API rejects tool_choice when no function tool is present, so it is dropped then.
        JsonNode? mappedToolChoice = null;
        if (toolChoice != null && hasFunctionTool)
        {
            mappedToolChoice = toolChoice switch
            {
                { Type: "auto" } => JsonValue.Create("auto"),
                { Type: "required" } => JsonValue.Create("any"),
                ToolChoice.NamedChoice named => new JsonObject
                {
                    ["allowed_tools"] = new JsonObject { ["mode"] = "validated", ["tools"] = new JsonArray(named.ToolName) },
                },
                _ => null,
            };
        }

        return new PrepareGoogleInteractionsToolsResult(interactionsTools.Count > 0 ? interactionsTools : null, mappedToolChoice, toolWarnings);
    }

    private static JsonElement? Arg(JsonElement args, string name)
    {
        return args.ValueKind == JsonValueKind.Object && args.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null ? value : null;
    }

    private static void Copy(JsonObject target, string key, JsonElement args, string name)
    {
        if (Arg(args, name) is { } value)
        {
            target[key] = JsonNode.Parse(value.GetRawText());
        }
    }
}
