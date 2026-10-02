// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Groq;

/// <summary>A tool passed to <see cref="GroqTools.Prepare"/>.</summary>
public abstract class GroqToolDefinition
{
    private GroqToolDefinition()
    {
    }

    /// <summary>Creates a function tool.</summary>
    public static GroqToolDefinition Function(string name, string? description, JsonElement parameters, bool? strict = null)
    {
        return new FunctionTool(name, description, parameters, strict);
    }

    /// <summary>Creates a provider-defined tool such as <c>groq.browser_search</c>.</summary>
    public static GroqToolDefinition Provider(string id)
    {
        return new ProviderTool(id);
    }

    private sealed class FunctionTool : GroqToolDefinition
    {
        public FunctionTool(string name, string? description, JsonElement parameters, bool? strict)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            Parameters = parameters;
            Strict = strict;
        }

        public string Name { get; }

        public string? Description { get; }

        public JsonElement Parameters { get; }

        public bool? Strict { get; }
    }

    private sealed class ProviderTool : GroqToolDefinition
    {
        public ProviderTool(string id)
        {
            Id = id ?? string.Empty;
        }

        public string Id { get; }
    }

    internal bool TryFunction(out string name, out string? description, out JsonElement parameters, out bool? strict)
    {
        if (this is FunctionTool function)
        {
            name = function.Name;
            description = function.Description;
            parameters = function.Parameters;
            strict = function.Strict;
            return true;
        }

        name = string.Empty;
        description = null;
        parameters = default;
        strict = null;
        return false;
    }

    internal bool TryProvider(out string id)
    {
        if (this is ProviderTool provider)
        {
            id = provider.Id;
            return true;
        }

        id = string.Empty;
        return false;
    }
}

/// <summary>Tools and tool choice prepared for a Groq chat request.</summary>
public sealed class GroqPreparedTools
{
    internal GroqPreparedTools(JsonArray? tools, JsonNode? toolChoice, IReadOnlyList<CallWarning> warnings)
    {
        Tools = tools;
        ToolChoice = toolChoice;
        Warnings = warnings ?? Array.Empty<CallWarning>();
    }

    /// <summary>Wire tools. Null when the caller passed no tools.</summary>
    public JsonArray? Tools { get; }

    /// <summary>Wire tool choice. A string or an object. Null when omitted.</summary>
    public JsonNode? ToolChoice { get; }

    /// <summary>Warnings produced while preparing tools.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }
}

/// <summary>Maps AI SDK tools onto Groq chat tool definitions.</summary>
public static class GroqTools
{
    /// <summary>Browser search is accepted on these model ids.</summary>
    public static readonly string[] BrowserSearchModels = { "openai/gpt-oss-20b", "openai/gpt-oss-120b" };

    /// <summary>Prepares tools for <paramref name="modelId"/>.</summary>
    public static GroqPreparedTools Prepare(IReadOnlyList<GroqToolDefinition>? tools, ToolChoice? toolChoice, string modelId)
    {
        if (tools == null || tools.Count == 0)
        {
            return new GroqPreparedTools(null, null, Array.Empty<CallWarning>());
        }

        var warnings = new List<CallWarning>();
        var prepared = new JsonArray();
        foreach (var tool in tools)
        {
            if (tool.TryProvider(out var id))
            {
                if (string.Equals(id, "groq.browser_search", StringComparison.Ordinal))
                {
                    if (!SupportsBrowserSearch(modelId))
                    {
                        warnings.Add(GroqWarnings.Unsupported(
                            "provider-defined tool " + id,
                            "Browser search is only supported on the following models: openai/gpt-oss-20b, openai/gpt-oss-120b. Current model: " + modelId));
                    }
                    else
                    {
                        prepared.Add(new JsonObject { ["type"] = "browser_search" });
                    }
                }
                else
                {
                    warnings.Add(GroqWarnings.Unsupported("provider-defined tool " + id));
                }

                continue;
            }

            if (!tool.TryFunction(out var name, out var description, out var parameters, out var strict))
            {
                continue;
            }

            var function = new JsonObject { ["name"] = name };
            if (description != null)
            {
                function["description"] = description;
            }

            function["parameters"] = parameters.ValueKind == JsonValueKind.Undefined
                ? new JsonObject()
                : JsonNode.Parse(parameters.GetRawText()) ?? new JsonObject();

            if (strict != null)
            {
                function["strict"] = strict.Value;
            }

            prepared.Add(new JsonObject
            {
                ["type"] = "function",
                ["function"] = function,
            });
        }

        if (toolChoice == null)
        {
            return new GroqPreparedTools(prepared, null, warnings);
        }

        if (toolChoice is ToolChoice.NamedChoice named)
        {
            return new GroqPreparedTools(
                prepared,
                new JsonObject
                {
                    ["type"] = "function",
                    ["function"] = new JsonObject { ["name"] = named.ToolName },
                },
                warnings);
        }

        return new GroqPreparedTools(prepared, JsonValue.Create(toolChoice.Type), warnings);
    }

    /// <summary>True for the GPT-OSS models that accept browser search.</summary>
    public static bool SupportsBrowserSearch(string modelId)
    {
        for (var i = 0; i < BrowserSearchModels.Length; i++)
        {
            if (string.Equals(BrowserSearchModels[i], modelId, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
