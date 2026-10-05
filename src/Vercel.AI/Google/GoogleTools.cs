// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>A Gemini provider-defined tool such as Google Search.</summary>
public sealed class GoogleProviderTool
{
    /// <summary>Creates a provider tool.</summary>
    public GoogleProviderTool(string id, JsonElement? args = null, string? name = null)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Args = args;
        Name = name;
    }

    /// <summary>Tool id, such as <c>google.google_search</c>.</summary>
    public string Id { get; }

    /// <summary>Tool arguments.</summary>
    public JsonElement? Args { get; }

    /// <summary>Custom name used when the model echoes a built-in tool such as code execution.</summary>
    public string? Name { get; }
}

/// <summary>Tools and <c>toolConfig</c> prepared for a generateContent request.</summary>
public sealed class GoogleToolPreparation
{
    /// <summary>Creates a preparation result.</summary>
    public GoogleToolPreparation(JsonArray? tools, JsonObject? toolConfig, IReadOnlyList<GoogleWarning> warnings, IReadOnlyDictionary<string, string> names)
    {
        Tools = tools;
        ToolConfig = toolConfig;
        Warnings = warnings;
        Names = names;
    }

    /// <summary>Gemini tools array.</summary>
    public JsonArray? Tools { get; }

    /// <summary>Function-calling configuration.</summary>
    public JsonObject? ToolConfig { get; }

    /// <summary>Warnings for unsupported combinations.</summary>
    public IReadOnlyList<GoogleWarning> Warnings { get; }

    /// <summary>Maps a built-in wire name such as <c>code_execution</c> onto the caller's tool name.</summary>
    public IReadOnlyDictionary<string, string> Names { get; }

    /// <summary>Returns the caller name for a built-in tool, or the wire name.</summary>
    public string CustomName(string wireName)
    {
        return Names.TryGetValue(wireName, out var name) ? name : wireName;
    }
}

/// <summary>Prepares Gemini function and provider-defined tools.</summary>
public static class GoogleTools
{
    /// <summary>Prepares tools for <paramref name="modelId"/>.</summary>
    public static GoogleToolPreparation Prepare(
        IReadOnlyList<LanguageModelTool>? functionTools,
        IReadOnlyList<GoogleProviderTool>? providerTools,
        ToolChoice? toolChoice,
        string modelId,
        bool isVertexProvider)
    {
        var warnings = new List<GoogleWarning>();
        var names = new Dictionary<string, string>(StringComparer.Ordinal);
        var functions = functionTools ?? Array.Empty<LanguageModelTool>();
        var providers = providerTools ?? Array.Empty<GoogleProviderTool>();
        if (functions.Count == 0 && providers.Count == 0)
        {
            return new GoogleToolPreparation(null, null, warnings, names);
        }

        foreach (var tool in providers)
        {
            if (tool.Id == "google.code_execution" && !string.IsNullOrEmpty(tool.Name))
            {
                names["code_execution"] = tool.Name!;
            }
        }

        var capabilities = GoogleModelCapability.Get(modelId);
        if (functions.Count > 0 && providers.Count > 0 && !capabilities.UsesGemini3Features)
        {
            warnings.Add(GoogleWarning.Unsupported("combination of function and provider-defined tools"));
        }

        if (providers.Count > 0)
        {
            var googleTools = new JsonArray();
            foreach (var tool in providers)
            {
                var node = ProviderTool(tool, capabilities, warnings);
                if (node != null)
                {
                    googleTools.Add(node);
                }
            }

            if (functions.Count > 0 && capabilities.UsesGemini3Features && googleTools.Count > 0)
            {
                var config = new JsonObject
                {
                    ["functionCallingConfig"] = new JsonObject { ["mode"] = "VALIDATED" },
                };
                if (!isVertexProvider)
                {
                    config["includeServerSideToolInvocations"] = true;
                }

                ApplyChoice(config, toolChoice, false);
                return new GoogleToolPreparation(
                    Join(googleTools, Declarations(functions)),
                    config,
                    warnings,
                    names);
            }

            return new GoogleToolPreparation(googleTools.Count > 0 ? googleTools : null, null, warnings, names);
        }

        var strict = false;
        foreach (var tool in functions)
        {
            if (tool.Strict == true)
            {
                strict = true;
            }
        }

        var declarations = new JsonArray { new JsonObject { ["functionDeclarations"] = Declarations(functions) } };
        if (toolChoice == null)
        {
            JsonObject? config = strict ? Mode("VALIDATED", null) : null;
            return new GoogleToolPreparation(declarations, config, warnings, names);
        }

        var mode = toolChoice.Type switch
        {
            "none" => "NONE",
            "required" => "ANY",
            "tool" => "ANY",
            _ => strict ? "VALIDATED" : "AUTO",
        };
        IReadOnlyList<string>? allowed = toolChoice is ToolChoice.NamedChoice named ? new[] { named.ToolName } : null;
        return new GoogleToolPreparation(declarations, Mode(mode, allowed), warnings, names);
    }

    private static void ApplyChoice(JsonObject config, ToolChoice? toolChoice, bool strict)
    {
        if (toolChoice == null)
        {
            return;
        }

        switch (toolChoice.Type)
        {
            case "auto":
                break;
            case "none":
                config["functionCallingConfig"] = new JsonObject { ["mode"] = "NONE" };
                break;
            case "required":
                config["functionCallingConfig"] = new JsonObject { ["mode"] = "ANY" };
                break;
            case "tool" when toolChoice is ToolChoice.NamedChoice named:
                config["functionCallingConfig"] = new JsonObject
                {
                    ["mode"] = "ANY",
                    ["allowedFunctionNames"] = new JsonArray(JsonValue.Create(named.ToolName)),
                };
                break;
        }
    }

    private static JsonObject Mode(string mode, IReadOnlyList<string>? allowed)
    {
        var calling = new JsonObject { ["mode"] = mode };
        if (allowed != null)
        {
            var names = new JsonArray();
            for (var i = 0; i < allowed.Count; i++)
            {
                names.Add(allowed[i]);
            }

            calling["allowedFunctionNames"] = names;
        }

        return new JsonObject { ["functionCallingConfig"] = calling };
    }

    private static JsonArray Join(JsonArray providerTools, JsonArray declarations)
    {
        var tools = new JsonArray();
        foreach (var tool in providerTools)
        {
            tools.Add(tool?.DeepClone());
        }

        tools.Add(new JsonObject { ["functionDeclarations"] = declarations });
        return tools;
    }

    private static JsonArray Declarations(IReadOnlyList<LanguageModelTool> tools)
    {
        var declarations = new JsonArray();
        foreach (var tool in tools)
        {
            declarations.Add(new JsonObject
            {
                ["name"] = tool.Name,
                ["description"] = tool.Description ?? string.Empty,
                ["parametersJsonSchema"] = GoogleJson.Clone(tool.InputSchema) ?? new JsonObject(),
            });
        }

        return declarations;
    }

    private static JsonObject? ProviderTool(GoogleProviderTool tool, GoogleModelCapabilities capabilities, List<GoogleWarning> warnings)
    {
        switch (tool.Id)
        {
            case "google.google_search":
                return Gemini2(tool, capabilities, warnings, "Google Search requires Gemini 2.0 or newer.")
                    ? new JsonObject { ["googleSearch"] = Args(tool) }
                    : null;
            case "google.enterprise_web_search":
                return Gemini2(tool, capabilities, warnings, "Enterprise Web Search requires Gemini 2.0 or newer.")
                    ? new JsonObject { ["enterpriseWebSearch"] = new JsonObject() }
                    : null;
            case "google.url_context":
                return Gemini2(tool, capabilities, warnings, "The URL context tool is not supported with other Gemini models than Gemini 2.")
                    ? new JsonObject { ["urlContext"] = new JsonObject() }
                    : null;
            case "google.code_execution":
                return Gemini2(tool, capabilities, warnings, "The code execution tool is not supported with other Gemini models than Gemini 2.")
                    ? new JsonObject { ["codeExecution"] = new JsonObject() }
                    : null;
            case "google.file_search":
                if (capabilities.SupportsFileSearch)
                {
                    return new JsonObject { ["fileSearch"] = Args(tool) };
                }

                warnings.Add(GoogleWarning.Unsupported("provider-defined tool " + tool.Id, "The file search tool is only supported with Gemini 2.5 models and Gemini 3 models."));
                return null;
            case "google.vertex_rag_store":
                if (!capabilities.SupportsGemini2Tools)
                {
                    warnings.Add(GoogleWarning.Unsupported("provider-defined tool " + tool.Id, "The RAG store tool is not supported with other Gemini models than Gemini 2."));
                    return null;
                }

                var corpus = tool.Args is { } args && args.TryGetProperty("ragCorpus", out var value) ? value.GetString() : null;
                int? topK = tool.Args is { } top && top.TryGetProperty("topK", out var topValue) && topValue.TryGetInt32(out var parsed) ? parsed : null;
                var store = new JsonObject { ["rag_resources"] = new JsonObject { ["rag_corpus"] = corpus } };
                GoogleJson.Set(store, "similarity_top_k", topK);
                return new JsonObject { ["retrieval"] = new JsonObject { ["vertex_rag_store"] = store } };
            case "google.google_maps":
                return Gemini2(tool, capabilities, warnings, "The Google Maps grounding tool is not supported with Gemini models other than Gemini 2 or newer.")
                    ? new JsonObject { ["googleMaps"] = new JsonObject() }
                    : null;
            default:
                warnings.Add(GoogleWarning.Unsupported("provider-defined tool " + tool.Id));
                return null;
        }
    }

    private static bool Gemini2(GoogleProviderTool tool, GoogleModelCapabilities capabilities, List<GoogleWarning> warnings, string details)
    {
        if (capabilities.SupportsGemini2Tools)
        {
            return true;
        }

        warnings.Add(GoogleWarning.Unsupported("provider-defined tool " + tool.Id, details));
        return false;
    }

    private static JsonObject Args(GoogleProviderTool tool)
    {
        return GoogleJson.Clone(tool.Args) as JsonObject ?? new JsonObject();
    }
}
