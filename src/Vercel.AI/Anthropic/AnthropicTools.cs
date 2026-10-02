// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Anthropic;

/// <summary>Options for <see cref="AnthropicTools.Prepare"/>.</summary>
public sealed class AnthropicToolPrepareOptions
{
    /// <summary>Tool choice JSON: auto, none, required, or tool.</summary>
    public JsonElement? ToolChoice { get; set; }

    /// <summary>When true, Anthropic <c>disable_parallel_tool_use</c> is set.</summary>
    public bool? DisableParallelToolUse { get; set; }

    /// <summary>Shared cache-breakpoint counter.</summary>
    public AnthropicCacheControlValidator? CacheControl { get; set; }

    /// <summary>Whether the model supports native structured outputs.</summary>
    public bool SupportsStructuredOutput { get; set; }

    /// <summary>Whether the model accepts <c>strict</c> on tool definitions.</summary>
    public bool SupportsStrictTools { get; set; }

    /// <summary>Default <c>eager_input_streaming</c> when a function tool does not set it.</summary>
    public bool DefaultEagerInputStreaming { get; set; }

    /// <summary>When true, forced tool choice falls back to auto.</summary>
    public bool RejectsForcedToolUse { get; set; }
}

/// <summary>Tools, tool choice, warnings, and betas for one Messages request.</summary>
public sealed class AnthropicPreparedTools
{
    /// <summary>Creates a prepare result.</summary>
    public AnthropicPreparedTools(JsonArray? tools, JsonObject? toolChoice, IReadOnlyList<AnthropicWarning> warnings, IReadOnlyList<string> betas)
    {
        Tools = tools;
        ToolChoice = toolChoice;
        Warnings = warnings ?? Array.Empty<AnthropicWarning>();
        Betas = betas ?? Array.Empty<string>();
    }

    /// <summary>Anthropic <c>tools</c> array. Null when no tools are sent.</summary>
    public JsonArray? Tools { get; }

    /// <summary>Anthropic <c>tool_choice</c>. Null when it is omitted.</summary>
    public JsonObject? ToolChoice { get; }

    /// <summary>Warnings produced while preparing tools.</summary>
    public IReadOnlyList<AnthropicWarning> Warnings { get; }

    /// <summary>Beta headers required by the tools.</summary>
    public IReadOnlyList<string> Betas { get; }
}

/// <summary>Converts V4 tools into Anthropic tool definitions.</summary>
public static class AnthropicTools
{
    /// <summary>Prepares <paramref name="tools"/>, a JSON array of function and provider tools.</summary>
    public static AnthropicPreparedTools Prepare(JsonElement? tools, AnthropicToolPrepareOptions? options = null)
    {
        var settings = options ?? new AnthropicToolPrepareOptions();
        var warnings = new List<AnthropicWarning>();
        var betas = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var validator = settings.CacheControl ?? new AnthropicCacheControlValidator();
        if (tools == null || tools.Value.ValueKind != JsonValueKind.Array || tools.Value.GetArrayLength() == 0)
        {
            return new AnthropicPreparedTools(null, null, warnings, betas);
        }

        var prepared = new JsonArray();
        foreach (var tool in tools.Value.EnumerateArray())
        {
            var type = AnthropicJson.String(tool, "type");
            if (type == "function")
            {
                PrepareFunction(tool, settings, validator, prepared, warnings, betas, seen);
            }
            else if (type == "provider")
            {
                PrepareProvider(tool, prepared, warnings, betas, seen);
            }
            else
            {
                warnings.Add(new AnthropicWarning("unsupported", "tool " + type, null));
            }
        }

        return Finish(prepared, settings, warnings, betas);
    }

    private static void PrepareFunction(
        JsonElement tool,
        AnthropicToolPrepareOptions settings,
        AnthropicCacheControlValidator validator,
        JsonArray prepared,
        List<AnthropicWarning> warnings,
        List<string> betas,
        HashSet<string> seen)
    {
        var name = AnthropicJson.String(tool, "name") ?? string.Empty;
        var anthropic = AnthropicJson.Anthropic(AnthropicJson.ProviderOptionsOf(tool));
        var eager = anthropic.ValueKind == JsonValueKind.Object && anthropic.TryGetProperty("eagerInputStreaming", out var eagerValue)
            ? eagerValue.ValueKind == JsonValueKind.True
            : settings.DefaultEagerInputStreaming;
        bool? strict = null;
        if (tool.TryGetProperty("strict", out var strictValue) && (strictValue.ValueKind == JsonValueKind.True || strictValue.ValueKind == JsonValueKind.False))
        {
            strict = strictValue.ValueKind == JsonValueKind.True;
        }

        if (!settings.SupportsStrictTools && strict != null)
        {
            warnings.Add(new AnthropicWarning(
                "unsupported",
                "strict",
                "Tool '" + name + "' has strict: " + (strict.Value ? "true" : "false") + ", but strict mode is not supported by this provider. The strict property will be ignored."));
        }

        var node = new JsonObject { ["name"] = name };
        AnthropicJson.Set(node, "description", AnthropicJson.String(tool, "description") is { } description ? JsonValue.Create(description) : null);
        if (tool.TryGetProperty("inputSchema", out var schema))
        {
            node["input_schema"] = AnthropicJson.Node(schema);
        }

        AnthropicJson.Set(node, "cache_control", validator.GetCacheControl(AnthropicJson.ProviderOptionsOf(tool), "tool definition", true));
        if (eager)
        {
            node["eager_input_streaming"] = true;
        }

        if (settings.SupportsStrictTools && strict != null)
        {
            node["strict"] = strict.Value;
        }

        if (anthropic.ValueKind == JsonValueKind.Object && anthropic.TryGetProperty("deferLoading", out var defer) && (defer.ValueKind == JsonValueKind.True || defer.ValueKind == JsonValueKind.False))
        {
            node["defer_loading"] = defer.ValueKind == JsonValueKind.True;
        }

        if (anthropic.ValueKind == JsonValueKind.Object && anthropic.TryGetProperty("allowedCallers", out var callers) && callers.ValueKind == JsonValueKind.Array)
        {
            node["allowed_callers"] = AnthropicJson.Node(callers);
            AddBeta(betas, seen, "advanced-tool-use-2025-11-20");
        }

        if (tool.TryGetProperty("inputExamples", out var examples) && examples.ValueKind == JsonValueKind.Array)
        {
            var mapped = new JsonArray();
            foreach (var example in examples.EnumerateArray())
            {
                if (example.TryGetProperty("input", out var input))
                {
                    mapped.Add(AnthropicJson.Node(input));
                }
            }

            node["input_examples"] = mapped;
            AddBeta(betas, seen, "advanced-tool-use-2025-11-20");
        }

        if (settings.SupportsStructuredOutput)
        {
            AddBeta(betas, seen, "structured-outputs-2025-11-13");
        }

        prepared.Add(node);
    }

    private static void PrepareProvider(JsonElement tool, JsonArray prepared, List<AnthropicWarning> warnings, List<string> betas, HashSet<string> seen)
    {
        var id = AnthropicJson.String(tool, "id") ?? string.Empty;
        var args = tool.TryGetProperty("args", out var raw) && raw.ValueKind == JsonValueKind.Object ? raw : default;
        switch (id)
        {
            case "anthropic.code_execution_20250522":
                AddBeta(betas, seen, "code-execution-2025-05-22");
                prepared.Add(Typed("code_execution_20250522", "code_execution"));
                break;
            case "anthropic.code_execution_20250825":
                AddBeta(betas, seen, "code-execution-2025-08-25");
                prepared.Add(Typed("code_execution_20250825", "code_execution"));
                break;
            case "anthropic.code_execution_20260120":
                prepared.Add(Typed("code_execution_20260120", "code_execution"));
                break;
            case "anthropic.computer_20241022":
                AddBeta(betas, seen, "computer-use-2024-10-22");
                prepared.Add(Computer("computer_20241022", args, false));
                break;
            case "anthropic.computer_20250124":
                AddBeta(betas, seen, "computer-use-2025-01-24");
                prepared.Add(Computer("computer_20250124", args, false));
                break;
            case "anthropic.computer_20251124":
                AddBeta(betas, seen, "computer-use-2025-11-24");
                prepared.Add(Computer("computer_20251124", args, true));
                break;
            case "anthropic.computer_toolset_20260801":
                prepared.Add(ComputerToolset(args));
                break;
            case "anthropic.text_editor_20241022":
                AddBeta(betas, seen, "computer-use-2024-10-22");
                prepared.Add(Typed("text_editor_20241022", "str_replace_editor"));
                break;
            case "anthropic.text_editor_20250124":
                AddBeta(betas, seen, "computer-use-2025-01-24");
                prepared.Add(Typed("text_editor_20250124", "str_replace_editor"));
                break;
            case "anthropic.text_editor_20250429":
                AddBeta(betas, seen, "computer-use-2025-01-24");
                prepared.Add(Typed("text_editor_20250429", "str_replace_based_edit_tool"));
                break;
            case "anthropic.text_editor_20250728":
                var editor = Typed("text_editor_20250728", "str_replace_based_edit_tool");
                AnthropicJson.CopyIfPresent(editor, "max_characters", args, "maxCharacters");
                prepared.Add(editor);
                break;
            case "anthropic.bash_20241022":
                AddBeta(betas, seen, "computer-use-2024-10-22");
                prepared.Add(Typed("bash_20241022", "bash"));
                break;
            case "anthropic.bash_20250124":
                AddBeta(betas, seen, "computer-use-2025-01-24");
                prepared.Add(Typed("bash_20250124", "bash"));
                break;
            case "anthropic.memory_20250818":
                AddBeta(betas, seen, "context-management-2025-06-27");
                prepared.Add(Typed("memory_20250818", "memory"));
                break;
            case "anthropic.web_search_20250305":
                prepared.Add(WebSearch("web_search_20250305", args, false));
                break;
            case "anthropic.web_search_20260209":
                AddBeta(betas, seen, "code-execution-web-tools-2026-02-09");
                prepared.Add(WebSearch("web_search_20260209", args, false));
                break;
            case "anthropic.web_search_20260318":
                prepared.Add(WebSearch("web_search_20260318", args, true));
                break;
            case "anthropic.web_fetch_20250910":
                AddBeta(betas, seen, "web-fetch-2025-09-10");
                prepared.Add(WebFetch("web_fetch_20250910", args, false));
                break;
            case "anthropic.web_fetch_20260209":
                AddBeta(betas, seen, "code-execution-web-tools-2026-02-09");
                prepared.Add(WebFetch("web_fetch_20260209", args, false));
                break;
            case "anthropic.web_fetch_20260318":
                prepared.Add(WebFetch("web_fetch_20260318", args, true));
                break;
            case "anthropic.tool_search_regex_20251119":
                prepared.Add(Typed("tool_search_tool_regex_20251119", "tool_search_tool_regex"));
                break;
            case "anthropic.tool_search_bm25_20251119":
                prepared.Add(Typed("tool_search_tool_bm25_20251119", "tool_search_tool_bm25"));
                break;
            case "anthropic.advisor_20260301":
                AddBeta(betas, seen, "advisor-tool-2026-03-01");
                prepared.Add(Advisor(args));
                break;
            default:
                warnings.Add(new AnthropicWarning("unsupported", "provider-defined tool " + id, null));
                break;
        }
    }

    private static JsonObject Typed(string type, string name)
    {
        return new JsonObject { ["name"] = name, ["type"] = type };
    }

    private static JsonObject Computer(string type, JsonElement args, bool zoom)
    {
        var node = Typed(type, "computer");
        AnthropicJson.CopyIfPresent(node, "display_width_px", args, "displayWidthPx");
        AnthropicJson.CopyIfPresent(node, "display_height_px", args, "displayHeightPx");
        AnthropicJson.CopyIfPresent(node, "display_number", args, "displayNumber");
        if (zoom)
        {
            AnthropicJson.CopyIfPresent(node, "enable_zoom", args, "enableZoom");
        }

        return node;
    }

    private static JsonObject ComputerToolset(JsonElement args)
    {
        var node = new JsonObject { ["type"] = "computer_toolset_20260801" };
        if (args.ValueKind == JsonValueKind.Object && args.TryGetProperty("configs", out var configs) && configs.ValueKind == JsonValueKind.Object)
        {
            var mapped = new JsonObject();
            foreach (var member in configs.EnumerateObject())
            {
                var config = new JsonObject();
                if (member.Value.ValueKind == JsonValueKind.Object)
                {
                    AnthropicJson.CopyIfPresent(config, "enabled", member.Value, "enabled");
                    AnthropicJson.CopyIfPresent(config, "defer_loading", member.Value, "deferLoading");
                }

                mapped[member.Name] = config;
            }

            node["configs"] = mapped;
        }

        return node;
    }

    private static JsonObject WebSearch(string type, JsonElement args, bool responseInclusion)
    {
        var node = Typed(type, "web_search");
        CopyWeb(node, args, responseInclusion, false);
        return node;
    }

    private static JsonObject WebFetch(string type, JsonElement args, bool extended)
    {
        var node = Typed(type, "web_fetch");
        CopyWeb(node, args, extended, true);
        if (extended)
        {
            AnthropicJson.CopyIfPresent(node, "use_cache", args, "useCache");
        }

        return node;
    }

    private static void CopyWeb(JsonObject node, JsonElement args, bool responseInclusion, bool fetch)
    {
        AnthropicJson.CopyIfPresent(node, "max_uses", args, "maxUses");
        AnthropicJson.CopyIfPresent(node, "allowed_domains", args, "allowedDomains");
        AnthropicJson.CopyIfPresent(node, "blocked_domains", args, "blockedDomains");
        if (args.ValueKind == JsonValueKind.Object && args.TryGetProperty("userLocation", out var location))
        {
            node["user_location"] = AnthropicJson.Node(location);
        }

        if (fetch)
        {
            AnthropicJson.CopyIfPresent(node, "citations", args, "citations");
            AnthropicJson.CopyIfPresent(node, "max_content_tokens", args, "maxContentTokens");
        }

        if (responseInclusion)
        {
            AnthropicJson.CopyIfPresent(node, "response_inclusion", args, "responseInclusion");
        }
    }

    private static JsonObject Advisor(JsonElement args)
    {
        if (args.ValueKind != JsonValueKind.Object || AnthropicJson.String(args, "model") == null)
        {
            throw new AiSdkException("AI_TypeValidationError: advisor model is required.");
        }

        if (args.TryGetProperty("maxTokens", out var maxTokens))
        {
            if (maxTokens.ValueKind != JsonValueKind.Number || !maxTokens.TryGetInt32(out var integer) || maxTokens.GetDouble() != integer)
            {
                throw new AiSdkException("AI_TypeValidationError: maxTokens must be an integer.");
            }

            if (integer < 1024)
            {
                throw new AiSdkException("AI_TypeValidationError: maxTokens must be at least 1024.");
            }
        }

        var node = Typed("advisor_20260301", "advisor");
        node["model"] = AnthropicJson.String(args, "model");
        AnthropicJson.CopyIfPresent(node, "max_uses", args, "maxUses");
        AnthropicJson.CopyIfPresent(node, "max_tokens", args, "maxTokens");
        if (args.TryGetProperty("caching", out var caching))
        {
            node["caching"] = AnthropicJson.Node(caching);
        }

        return node;
    }

    private static AnthropicPreparedTools Finish(JsonArray tools, AnthropicToolPrepareOptions settings, List<AnthropicWarning> warnings, List<string> betas)
    {
        var choice = settings.ToolChoice;
        if (choice == null || choice.Value.ValueKind != JsonValueKind.Object)
        {
            JsonObject? auto = null;
            if (settings.DisableParallelToolUse == true)
            {
                auto = Choice("auto", null, true);
            }

            return new AnthropicPreparedTools(tools, auto, warnings, betas);
        }

        var type = AnthropicJson.String(choice.Value, "type");
        if (type == "auto")
        {
            return new AnthropicPreparedTools(tools, Choice("auto", null, settings.DisableParallelToolUse), warnings, betas);
        }

        if (type == "required")
        {
            if (settings.RejectsForcedToolUse)
            {
                warnings.Add(new AnthropicWarning(
                    "unsupported",
                    "toolChoice",
                    "toolChoice 'required' is not supported by this model because it rejects forced tool use. Using 'auto' instead. Instruct the model to use a tool in the prompt and verify that a tool call was made."));
                return new AnthropicPreparedTools(tools, Choice("auto", null, settings.DisableParallelToolUse), warnings, betas);
            }

            return new AnthropicPreparedTools(tools, Choice("any", null, settings.DisableParallelToolUse), warnings, betas);
        }

        if (type == "none")
        {
            return new AnthropicPreparedTools(null, null, warnings, betas);
        }

        if (type == "tool")
        {
            var toolName = AnthropicJson.String(choice.Value, "toolName") ?? string.Empty;
            if (settings.RejectsForcedToolUse)
            {
                warnings.Add(new AnthropicWarning(
                    "unsupported",
                    "toolChoice",
                    "toolChoice 'tool' is not supported by this model because it rejects forced tool use. Only the '" + toolName + "' tool is sent with 'auto' tool choice. Instruct the model to use the tool in the prompt and verify that a tool call was made."));
                var filtered = new JsonArray();
                foreach (var tool in tools)
                {
                    if (tool is JsonObject obj && obj["name"]?.GetValue<string>() == toolName)
                    {
                        filtered.Add(tool.DeepClone());
                    }
                }

                return new AnthropicPreparedTools(filtered, Choice("auto", null, settings.DisableParallelToolUse), warnings, betas);
            }

            return new AnthropicPreparedTools(tools, Choice("tool", toolName, settings.DisableParallelToolUse), warnings, betas);
        }

        throw new AiSdkException("Unsupported functionality: tool choice type: " + type);
    }

    private static JsonObject Choice(string type, string? name, bool? disableParallel)
    {
        var choice = new JsonObject { ["type"] = type };
        AnthropicJson.Set(choice, "name", name == null ? null : JsonValue.Create(name));
        if (disableParallel == true)
        {
            choice["disable_parallel_tool_use"] = true;
        }

        return choice;
    }

    private static void AddBeta(List<string> betas, HashSet<string> seen, string beta)
    {
        if (seen.Add(beta))
        {
            betas.Add(beta);
        }
    }
}

/// <summary>Accepts Anthropic tool-result payloads that the upstream schemas allow, including null titles.</summary>
public static class AnthropicToolSchemas
{
    /// <summary>True when a web fetch tool result allows a null document title.</summary>
    public static bool AcceptWebFetchOutput(JsonElement value)
    {
        if (AnthropicJson.String(value, "type") != "web_fetch_result" || AnthropicJson.String(value, "url") == null || AnthropicJson.String(value, "retrievedAt") == null)
        {
            return false;
        }

        if (!value.TryGetProperty("content", out var content) || AnthropicJson.String(content, "type") != "document" || !content.TryGetProperty("title", out var title))
        {
            return false;
        }

        if (title.ValueKind != JsonValueKind.String && title.ValueKind != JsonValueKind.Null)
        {
            return false;
        }

        return content.TryGetProperty("source", out var source)
            && AnthropicJson.String(source, "type") != null
            && AnthropicJson.String(source, "mediaType") != null
            && source.TryGetProperty("data", out _);
    }

    /// <summary>True when web search results allow a null title.</summary>
    public static bool AcceptWebSearchOutput(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var item in value.EnumerateArray())
        {
            if (AnthropicJson.String(item, "url") == null || AnthropicJson.String(item, "type") != "web_search_result" || AnthropicJson.String(item, "encryptedContent") == null)
            {
                return false;
            }

            if (!item.TryGetProperty("title", out var title) || (title.ValueKind != JsonValueKind.String && title.ValueKind != JsonValueKind.Null))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>True when a Messages response contains a web fetch tool result with a document source.</summary>
    public static bool AcceptWebFetchResponse(JsonElement value)
    {
        if (AnthropicJson.String(value, "type") != "message" || !value.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var part in content.EnumerateArray())
        {
            if (AnthropicJson.String(part, "type") != "web_fetch_tool_result" || !part.TryGetProperty("content", out var body))
            {
                return false;
            }

            if (!body.TryGetProperty("content", out var document) || !document.TryGetProperty("source", out var source))
            {
                return false;
            }

            var sourceType = AnthropicJson.String(source, "type");
            if (sourceType != "base64" && sourceType != "text")
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>True when a streaming content block carries a web fetch result or an MCP tool result.</summary>
    public static bool AcceptContentBlock(JsonElement value)
    {
        var type = AnthropicJson.String(value, "type");
        if (type == "content_block_start" && value.TryGetProperty("content_block", out var block))
        {
            return AcceptBlock(block);
        }

        if (type == "web_fetch_tool_result" || type == "mcp_tool_result")
        {
            return AcceptBlock(value);
        }

        return value.ValueKind == JsonValueKind.Object && value.TryGetProperty("content_block", out var nested) && AcceptBlock(nested);
    }

    private static bool AcceptBlock(JsonElement block)
    {
        var type = AnthropicJson.String(block, "type");
        if (type == "web_fetch_tool_result")
        {
            return block.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Object;
        }

        if (type == "mcp_tool_result")
        {
            return block.TryGetProperty("content", out _);
        }

        return false;
    }
}

/// <summary>Removes JSON Schema keywords Anthropic rejects on <c>output_config.format.schema</c>.</summary>
public static class AnthropicJsonSchema
{
    private static readonly string[] SupportedFormats = new[]
    {
        "date-time", "time", "date", "duration", "email", "hostname", "uri", "ipv4", "ipv6", "uuid",
    };

    private static readonly string[] ConstraintKeys = new[]
    {
        "minimum", "maximum", "exclusiveMinimum", "exclusiveMaximum", "multipleOf", "minLength", "maxLength", "pattern",
        "minItems", "maxItems", "uniqueItems", "minProperties", "maxProperties", "not",
    };

    /// <summary>Returns a schema Anthropic's structured-output decoder accepts.</summary>
    public static JsonNode Sanitize(JsonElement schema)
    {
        return SanitizeElement(schema) ?? new JsonObject();
    }

    private static JsonNode? SanitizeElement(JsonElement schema)
    {
        if (schema.ValueKind == JsonValueKind.True || schema.ValueKind == JsonValueKind.False)
        {
            return AnthropicJson.Node(schema);
        }

        if (schema.ValueKind != JsonValueKind.Object)
        {
            return AnthropicJson.Node(schema);
        }

        if (schema.TryGetProperty("$ref", out var reference) && reference.ValueKind != JsonValueKind.Null)
        {
            return new JsonObject { ["$ref"] = AnthropicJson.Node(reference) };
        }

        var result = new JsonObject();
        Copy(result, "$schema", schema, "$schema");
        Copy(result, "$id", schema, "$id");
        Copy(result, "title", schema, "title");
        Copy(result, "description", schema, "description");
        Copy(result, "default", schema, "default");
        Copy(result, "const", schema, "const");
        Copy(result, "enum", schema, "enum");
        Copy(result, "type", schema, "type");
        if (schema.TryGetProperty("anyOf", out var anyOf) && anyOf.ValueKind == JsonValueKind.Array)
        {
            result["anyOf"] = MapArray(anyOf);
        }
        else if (schema.TryGetProperty("oneOf", out var oneOf) && oneOf.ValueKind == JsonValueKind.Array)
        {
            result["anyOf"] = MapArray(oneOf);
        }

        if (schema.TryGetProperty("allOf", out var allOf) && allOf.ValueKind == JsonValueKind.Array)
        {
            result["allOf"] = MapArray(allOf);
        }

        CopyDefinitions(result, "definitions", schema, "definitions");
        CopyDefinitions(result, "$defs", schema, "$defs");
        var type = AnthropicJson.String(schema, "type");
        if (type == "object" || schema.TryGetProperty("properties", out _))
        {
            if (schema.TryGetProperty("properties", out var properties) && properties.ValueKind == JsonValueKind.Object)
            {
                var mapped = new JsonObject();
                foreach (var property in properties.EnumerateObject())
                {
                    mapped[property.Name] = SanitizeElement(property.Value);
                }

                result["properties"] = mapped;
            }

            result["additionalProperties"] = false;
            Copy(result, "required", schema, "required");
        }

        if (schema.TryGetProperty("items", out var items))
        {
            if (items.ValueKind == JsonValueKind.Array)
            {
                result["items"] = MapArray(items);
            }
            else
            {
                result["items"] = SanitizeElement(items);
            }
        }

        var format = AnthropicJson.String(schema, "format");
        if (format != null && IsSupportedFormat(format))
        {
            result["format"] = format;
        }

        var constraint = ConstraintDescription(schema);
        if (constraint != null)
        {
            var description = result["description"]?.GetValue<string>();
            result["description"] = description == null ? constraint : description + "\n" + constraint;
        }

        return result;
    }

    private static JsonArray MapArray(JsonElement array)
    {
        var mapped = new JsonArray();
        foreach (var item in array.EnumerateArray())
        {
            mapped.Add(SanitizeElement(item));
        }

        return mapped;
    }

    private static void Copy(JsonObject target, string name, JsonElement source, string sourceName)
    {
        if (source.TryGetProperty(sourceName, out var value) && value.ValueKind != JsonValueKind.Null && value.ValueKind != JsonValueKind.Undefined)
        {
            target[name] = AnthropicJson.Node(value);
        }
    }

    private static void CopyDefinitions(JsonObject target, string name, JsonElement source, string sourceName)
    {
        if (!source.TryGetProperty(sourceName, out var definitions) || definitions.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        var mapped = new JsonObject();
        foreach (var definition in definitions.EnumerateObject())
        {
            mapped[definition.Name] = SanitizeElement(definition.Value);
        }

        target[name] = mapped;
    }

    private static string? ConstraintDescription(JsonElement schema)
    {
        var parts = new List<string>();
        foreach (var key in ConstraintKeys)
        {
            if (!schema.TryGetProperty(key, out var value) || value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.False)
            {
                continue;
            }

            parts.Add(FormatName(key) + ": " + (value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText()));
        }

        var format = AnthropicJson.String(schema, "format");
        if (format != null && !IsSupportedFormat(format))
        {
            parts.Add("format: " + format);
        }

        return parts.Count == 0 ? null : string.Join("; ", parts) + ".";
    }

    private static string FormatName(string key)
    {
        var builder = new System.Text.StringBuilder();
        foreach (var character in key)
        {
            if (char.IsUpper(character))
            {
                builder.Append(' ');
                builder.Append(char.ToLower(character, CultureInfo.InvariantCulture));
            }
            else
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    private static bool IsSupportedFormat(string format)
    {
        foreach (var supported in SupportedFormats)
        {
            if (supported == format)
            {
                return true;
            }
        }

        return false;
    }
}
