// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>A function or provider-defined tool prepared for Gemini.</summary>
public sealed class GoogleToolSpec
{
    /// <summary>Creates a function tool.</summary>
    public GoogleToolSpec(string name, string? description, JsonElement schema, bool? strict)
    {
        Kind = "function";
        Name = name ?? string.Empty;
        Id = name ?? string.Empty;
        Description = description;
        Schema = schema;
        Strict = strict;
    }

    /// <summary>Creates a provider-defined tool such as <c>google.google_search</c>.</summary>
    public GoogleToolSpec(string id, string? name, JsonElement args)
    {
        Kind = "provider";
        Id = id ?? string.Empty;
        Name = string.IsNullOrEmpty(name) ? id ?? string.Empty : name!;
        Args = args;
    }

    /// <summary><c>function</c> or <c>provider</c>.</summary>
    public string Kind { get; }

    /// <summary>Function name or custom provider tool name.</summary>
    public string Name { get; }

    /// <summary>Provider tool id.</summary>
    public string Id { get; }

    /// <summary>Function description.</summary>
    public string? Description { get; }

    /// <summary>Function JSON Schema.</summary>
    public JsonElement Schema { get; }

    /// <summary>Provider tool arguments.</summary>
    public JsonElement Args { get; }

    /// <summary>Strict function calling.</summary>
    public bool? Strict { get; }
}

/// <summary>Tools and <c>toolConfig</c> for a Gemini request.</summary>
public sealed class GooglePreparedTools
{
    internal GooglePreparedTools(JsonArray? tools, JsonObject? toolConfig, List<CallWarning> warnings, string codeExecutionName)
    {
        Tools = tools;
        ToolConfig = toolConfig;
        Warnings = warnings;
        CodeExecutionName = codeExecutionName;
    }

    /// <summary>Gemini tools array. Null when nothing is sent.</summary>
    public JsonArray? Tools { get; }

    /// <summary>Gemini tool config. Null when nothing is sent.</summary>
    public JsonObject? ToolConfig { get; }

    /// <summary>Warnings for unsupported tools.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }

    /// <summary>Name used when code execution results are mapped back.</summary>
    public string CodeExecutionName { get; }
}

/// <summary>Prepares Gemini function declarations and provider tools.</summary>
public static class GoogleTools
{
    /// <summary>Prepares <paramref name="tools"/> for <paramref name="modelId"/>.</summary>
    public static GooglePreparedTools Prepare(
        IReadOnlyList<GoogleToolSpec>? tools,
        ToolChoice? toolChoice,
        string modelId,
        bool vertex)
    {
        var warnings = new List<CallWarning>();
        var codeName = "code_execution";
        if (tools == null || tools.Count == 0)
        {
            return new GooglePreparedTools(null, null, warnings, codeName);
        }

        foreach (var tool in tools)
        {
            if (tool.Kind == "provider" && tool.Id == "google.code_execution" && !string.IsNullOrEmpty(tool.Name))
            {
                codeName = tool.Name;
            }
        }

        var capabilities = GoogleModelCapabilitiesMap.Get(modelId);
        var hasFunction = false;
        var hasProvider = false;
        foreach (var tool in tools)
        {
            if (tool.Kind == "provider")
            {
                hasProvider = true;
            }
            else
            {
                hasFunction = true;
            }
        }

        if (hasFunction && hasProvider && !capabilities.UsesGemini3Features)
        {
            warnings.Add(new CallWarning("unsupported", "combination of function and provider-defined tools"));
        }

        if (hasProvider)
        {
            var googleTools = new JsonArray();
            foreach (var tool in tools)
            {
                if (tool.Kind != "provider")
                {
                    continue;
                }

                var built = ProviderTool(tool, capabilities, warnings);
                if (built != null)
                {
                    googleTools.Add(built);
                }
            }

            if (hasFunction && capabilities.UsesGemini3Features && googleTools.Count > 0)
            {
                var declarations = Declarations(tools, warnings, warnOnOther: false);
                var config = new JsonObject
                {
                    ["functionCallingConfig"] = new JsonObject { ["mode"] = "VALIDATED" },
                };
                if (!vertex)
                {
                    config["includeServerSideToolInvocations"] = true;
                }

                ApplyChoice(config, toolChoice, strict: false, forceValidated: true);
                googleTools.Add(new JsonObject { ["functionDeclarations"] = declarations });
                return new GooglePreparedTools(googleTools, config, warnings, codeName);
            }

            return new GooglePreparedTools(googleTools.Count > 0 ? googleTools : null, null, warnings, codeName);
        }

        var functions = Declarations(tools, warnings, warnOnOther: true);
        var strict = false;
        foreach (var tool in tools)
        {
            if (tool.Strict == true)
            {
                strict = true;
            }
        }

        var toolArray = new JsonArray { new JsonObject { ["functionDeclarations"] = functions } };
        if (toolChoice == null)
        {
            JsonObject? config = strict
                ? new JsonObject { ["functionCallingConfig"] = new JsonObject { ["mode"] = "VALIDATED" } }
                : null;
            return new GooglePreparedTools(toolArray, config, warnings, codeName);
        }

        var chosen = new JsonObject();
        ApplyChoice(chosen, toolChoice, strict, forceValidated: false);
        return new GooglePreparedTools(toolArray, chosen, warnings, codeName);
    }

    private static JsonArray Declarations(IReadOnlyList<GoogleToolSpec> tools, List<CallWarning> warnings, bool warnOnOther)
    {
        var declarations = new JsonArray();
        foreach (var tool in tools)
        {
            if (tool.Kind != "function")
            {
                if (warnOnOther)
                {
                    warnings.Add(new CallWarning("unsupported", "function tool " + tool.Name));
                }

                continue;
            }

            var schema = tool.Schema.ValueKind == JsonValueKind.Undefined || tool.Schema.ValueKind == JsonValueKind.Null
                ? new JsonObject()
                : GoogleJson.Clone(tool.Schema);
            declarations.Add(new JsonObject
            {
                ["name"] = tool.Name,
                ["description"] = tool.Description ?? string.Empty,
                ["parametersJsonSchema"] = schema,
            });
        }

        return declarations;
    }

    private static void ApplyChoice(JsonObject toolConfig, ToolChoice? toolChoice, bool strict, bool forceValidated)
    {
        if (toolChoice == null)
        {
            return;
        }

        var config = new JsonObject();
        if (toolChoice.Type == "auto")
        {
            config["mode"] = forceValidated || strict ? "VALIDATED" : "AUTO";
        }
        else if (toolChoice.Type == "none")
        {
            config["mode"] = "NONE";
        }
        else if (toolChoice.Type == "required")
        {
            config["mode"] = "ANY";
        }
        else if (toolChoice is ToolChoice.NamedChoice named)
        {
            config["mode"] = "ANY";
            config["allowedFunctionNames"] = new JsonArray(JsonValue.Create(named.ToolName));
        }
        else
        {
            throw new InvalidOperationException("tool choice type: " + toolChoice.Type);
        }

        toolConfig["functionCallingConfig"] = config;
    }

    private static JsonObject? ProviderTool(GoogleToolSpec tool, GoogleModelCapabilities capabilities, List<CallWarning> warnings)
    {
        switch (tool.Id)
        {
            case "google.google_search":
                return Gemini2(capabilities, tool, warnings, "Google Search requires Gemini 2.0 or newer.")
                    ? new JsonObject { ["googleSearch"] = Args(tool) }
                    : null;
            case "google.enterprise_web_search":
                return Gemini2(capabilities, tool, warnings, "Enterprise Web Search requires Gemini 2.0 or newer.")
                    ? new JsonObject { ["enterpriseWebSearch"] = new JsonObject() }
                    : null;
            case "google.url_context":
                return Gemini2(capabilities, tool, warnings, "The URL context tool is not supported with other Gemini models than Gemini 2.")
                    ? new JsonObject { ["urlContext"] = new JsonObject() }
                    : null;
            case "google.code_execution":
                return Gemini2(capabilities, tool, warnings, "The code execution tool is not supported with other Gemini models than Gemini 2.")
                    ? new JsonObject { ["codeExecution"] = new JsonObject() }
                    : null;
            case "google.file_search":
                if (capabilities.SupportsFileSearch)
                {
                    return new JsonObject { ["fileSearch"] = Args(tool) };
                }

                warnings.Add(new CallWarning(
                    "unsupported",
                    "provider-defined tool " + tool.Id + "\nThe file search tool is only supported with Gemini 2.5 models and Gemini 3 models."));
                return null;
            case "google.vertex_rag_store":
                if (capabilities.SupportsGemini2Tools)
                {
                    var store = new JsonObject
                    {
                        ["rag_resources"] = new JsonObject { ["rag_corpus"] = StringArg(tool.Args, "ragCorpus") },
                    };
                    if (tool.Args.ValueKind == JsonValueKind.Object && tool.Args.TryGetProperty("topK", out var topK) && topK.ValueKind == JsonValueKind.Number)
                    {
                        store["similarity_top_k"] = topK.GetInt32();
                    }

                    return new JsonObject { ["retrieval"] = new JsonObject { ["vertex_rag_store"] = store } };
                }

                warnings.Add(new CallWarning(
                    "unsupported",
                    "provider-defined tool " + tool.Id + "\nThe RAG store tool is not supported with other Gemini models than Gemini 2."));
                return null;
            case "google.google_maps":
                return Gemini2(capabilities, tool, warnings, "The Google Maps grounding tool is not supported with Gemini models other than Gemini 2 or newer.")
                    ? new JsonObject { ["googleMaps"] = new JsonObject() }
                    : null;
            default:
                warnings.Add(new CallWarning("unsupported", "provider-defined tool " + tool.Id));
                return null;
        }
    }

    private static bool Gemini2(GoogleModelCapabilities capabilities, GoogleToolSpec tool, List<CallWarning> warnings, string details)
    {
        if (capabilities.SupportsGemini2Tools)
        {
            return true;
        }

        warnings.Add(new CallWarning("unsupported", "provider-defined tool " + tool.Id + "\n" + details));
        return false;
    }

    private static JsonObject Args(GoogleToolSpec tool)
    {
        if (tool.Args.ValueKind != JsonValueKind.Object)
        {
            return new JsonObject();
        }

        return (JsonObject)GoogleJson.Clone(tool.Args);
    }

    private static string StringArg(JsonElement args, string name)
    {
        if (args.ValueKind == JsonValueKind.Object && args.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
        {
            return value.GetString() ?? string.Empty;
        }

        return string.Empty;
    }
}
