// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Xai;

/// <summary>The names of the xAI provider-defined tools in a request. Provider-executed calls are reported under these names.</summary>
internal sealed class XaiToolNames
{
    private static readonly string[] WebSearchSubTools = { "web_search", "web_search_with_snippets", "browse_page" };
    private static readonly string[] XSearchSubTools = { "x_user_search", "x_keyword_search", "x_semantic_search", "x_thread_fetch" };

    private readonly string? _webSearch;
    private readonly string? _xSearch;
    private readonly string? _codeExecution;
    private readonly string? _mcp;
    private readonly string? _fileSearch;
    private readonly string? _imageGeneration;

    public XaiToolNames(IReadOnlyList<LanguageModelTool>? tools)
    {
        _webSearch = XaiResponsesTools.ProviderToolName(tools, "xai.web_search");
        _xSearch = XaiResponsesTools.ProviderToolName(tools, "xai.x_search");
        _codeExecution = XaiResponsesTools.ProviderToolName(tools, "xai.code_execution");
        _mcp = XaiResponsesTools.ProviderToolName(tools, "xai.mcp");
        _fileSearch = XaiResponsesTools.ProviderToolName(tools, "xai.file_search");
        _imageGeneration = XaiResponsesTools.ProviderToolName(tools, "xai.image_generation");
    }

    /// <summary>
    /// The tool name of a provider-executed item. xAI reports server-side sub-tools (such as <c>x_keyword_search</c>) or an empty
    /// name, so the item type and the known sub-tool names map back to the tool the caller declared.
    /// </summary>
    public string Resolve(string itemType, string? name)
    {
        if (itemType == "file_search_call")
        {
            return _fileSearch ?? "file_search";
        }

        if (itemType == "image_generation_call")
        {
            return _imageGeneration ?? "image_generation";
        }

        if (Array.IndexOf(WebSearchSubTools, name ?? string.Empty) >= 0 || itemType == "web_search_call")
        {
            return _webSearch ?? "web_search";
        }

        if (Array.IndexOf(XSearchSubTools, name ?? string.Empty) >= 0 || itemType == "x_search_call")
        {
            return _xSearch ?? "x_search";
        }

        if (name == "code_execution" || itemType == "code_interpreter_call" || itemType == "code_execution_call")
        {
            return _codeExecution ?? "code_execution";
        }

        if (itemType == "mcp_call")
        {
            return _mcp ?? name ?? "mcp";
        }

        return name ?? string.Empty;
    }
}

/// <summary>Maps the output items of xAI provider-executed tools to tool inputs and results.</summary>
internal static class XaiResponsesOutput
{
    private static readonly string[] ProviderToolItems =
    {
        "web_search_call",
        "x_search_call",
        "code_interpreter_call",
        "code_execution_call",
        "view_image_call",
        "view_x_video_call",
        "custom_tool_call",
        "mcp_call",
        "file_search_call",
        "image_generation_call",
    };

    /// <summary>True for an output item of a provider-executed tool.</summary>
    public static bool IsProviderToolItem(string? type)
    {
        return type != null && Array.IndexOf(ProviderToolItems, type) >= 0;
    }

    /// <summary>The tool input of a provider-executed item, as the raw argument string.</summary>
    public static string ToolInput(string type, JsonElement item)
    {
        switch (type)
        {
            case "file_search_call":
                return string.Empty;
            case "image_generation_call":
                return "{}";
            case "custom_tool_call":
                return OpenAIJson.String(item, "input") ?? string.Empty;
            default:
                return OpenAIJson.String(item, "arguments") ?? string.Empty;
        }
    }

    /// <summary>The result of a <c>file_search_call</c> item: its queries and results, with results null when xAI sent none.</summary>
    public static JsonObject FileSearchResult(JsonElement item)
    {
        var queries = OpenAIJson.Child(item, "queries") is { ValueKind: JsonValueKind.Array } queryList
            ? OpenAIJson.Node(queryList)!
            : new JsonArray();
        JsonArray? results = null;
        if (OpenAIJson.Child(item, "results") is { ValueKind: JsonValueKind.Array } resultList)
        {
            results = new JsonArray();
            foreach (var result in resultList.EnumerateArray())
            {
                results.Add(new JsonObject
                {
                    ["fileId"] = OpenAIJson.Node(OpenAIJson.Child(result, "file_id")),
                    ["filename"] = OpenAIJson.Node(OpenAIJson.Child(result, "filename")),
                    ["score"] = OpenAIJson.Node(OpenAIJson.Child(result, "score")),
                    ["text"] = OpenAIJson.Node(OpenAIJson.Child(result, "text")),
                });
            }
        }

        return new JsonObject { ["queries"] = queries, ["results"] = results };
    }

    /// <summary>
    /// The result of an <c>image_generation_call</c> item: the base64 image and the prompt, or an error message when the item has no image.
    /// </summary>
    public static (JsonNode Result, bool IsError) ImageGenerationResult(JsonElement item)
    {
        if (OpenAIJson.String(item, "result") is { } image)
        {
            var result = new JsonObject { ["result"] = image };
            if (OpenAIJson.String(item, "prompt") is { } prompt)
            {
                result["prompt"] = prompt;
            }

            return (result, false);
        }

        return (JsonValue.Create("Image generation failed (status: " + OpenAIJson.String(item, "status") + ").")!, true);
    }

    /// <summary>
    /// The result of a <c>web_search_call</c> item: the action in camel case (<c>search</c>, <c>openPage</c>, or <c>findInPage</c>)
    /// and its URL sources. An action that is missing or not recognized gives an empty object. Maps to <c>mapWebSearchAction</c>.
    /// </summary>
    public static JsonObject WebSearchResult(JsonElement? action)
    {
        if (action is not { ValueKind: JsonValueKind.Object } value
            || !TryOptionalArray(value, "sources", out var sourceList))
        {
            return new JsonObject();
        }

        var result = new JsonObject();
        switch (OpenAIJson.String(value, "type"))
        {
            case "search":
                if (!TryOptionalString(value, "query", out var query)
                    || !TryOptionalArray(value, "queries", out var queries)
                    || (queries != null && queries.Value.EnumerateArray().Any(entry => entry.ValueKind != JsonValueKind.String)))
                {
                    return new JsonObject();
                }

                var search = new JsonObject { ["type"] = "search" };
                if (query is { ValueKind: JsonValueKind.String })
                {
                    search["query"] = query.Value.GetString();
                }

                if (queries != null)
                {
                    search["queries"] = OpenAIJson.Node(queries);
                }

                result["action"] = search;
                break;
            case "open_page":
                if (!TryOptionalString(value, "url", out var openUrl))
                {
                    return new JsonObject();
                }

                var openPage = new JsonObject { ["type"] = "openPage" };
                CopyPresent(openPage, "url", openUrl);
                result["action"] = openPage;
                break;
            case "find_in_page":
                if (!TryOptionalString(value, "url", out var findUrl) || !TryOptionalString(value, "pattern", out var pattern))
                {
                    return new JsonObject();
                }

                var findInPage = new JsonObject { ["type"] = "findInPage" };
                CopyPresent(findInPage, "url", findUrl);
                CopyPresent(findInPage, "pattern", pattern);
                result["action"] = findInPage;
                break;
            default:
                return new JsonObject();
        }

        var sources = new JsonArray();
        if (sourceList != null)
        {
            foreach (var source in sourceList.Value.EnumerateArray())
            {
                if (OpenAIJson.String(source, "type") == "url" && OpenAIJson.String(source, "url") is { } url)
                {
                    sources.Add(new JsonObject { ["type"] = "url", ["url"] = url });
                }
            }
        }

        if (sources.Count > 0)
        {
            result["sources"] = sources;
        }

        return result;
    }

    /// <summary>Reads a property that may be missing, null, or a string. False when it has another type.</summary>
    private static bool TryOptionalString(JsonElement element, string name, out JsonElement? value)
    {
        value = element.TryGetProperty(name, out var property) ? property : null;
        return value is not { } present || present.ValueKind is JsonValueKind.Null or JsonValueKind.String;
    }

    /// <summary>Reads a property that may be missing, null, or an array. Null and missing both give null. False for another type.</summary>
    private static bool TryOptionalArray(JsonElement element, string name, out JsonElement? value)
    {
        value = null;
        if (!element.TryGetProperty(name, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        value = property;
        return property.ValueKind == JsonValueKind.Array;
    }

    /// <summary>Copies a string or null value. A missing value stays missing.</summary>
    private static void CopyPresent(JsonObject target, string name, JsonElement? value)
    {
        if (value is { } present)
        {
            target[name] = present.ValueKind == JsonValueKind.String ? JsonValue.Create(present.GetString()) : null;
        }
    }
}
