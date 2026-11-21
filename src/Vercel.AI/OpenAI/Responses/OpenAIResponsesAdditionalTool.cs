// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.OpenAI;

/// <summary>
/// A single OpenAI-format tool definition that can be loaded at a position in the conversation. History is
/// replayable, so the provider treats these as references to tools registered for the current request and sends its
/// own definition for the registered tool. Maps to <c>OpenAIResponsesAdditionalTool</c>.
/// </summary>
public abstract class OpenAIResponsesAdditionalTool
{
    private protected OpenAIResponsesAdditionalTool() { }

    /// <summary>The tool name.</summary>
    public string Name { get; set; } = "";

    internal abstract JsonNode ToJson();
}

/// <summary>A function tool definition (<c>type: 'function'</c>).</summary>
public sealed class OpenAIResponsesFunctionToolDefinition : OpenAIResponsesAdditionalTool
{
    /// <summary>The tool description.</summary>
    public string? Description { get; set; }

    /// <summary>The JSON Schema of the tool input, an object.</summary>
    public JsonElement Parameters { get; set; }

    /// <summary>Strict mode.</summary>
    public bool? Strict { get; set; }

    /// <summary>Defers loading the tool (<c>defer_loading</c>).</summary>
    public bool? DeferLoading { get; set; }

    internal override JsonNode ToJson()
    {
        var json = new JsonObject { ["type"] = "function", ["name"] = Name };
        if (Description != null)
        {
            json["description"] = Description;
        }

        json["parameters"] = JsonNode.Parse(Parameters.GetRawText());
        if (Strict != null)
        {
            json["strict"] = Strict;
        }

        if (DeferLoading != null)
        {
            json["defer_loading"] = DeferLoading;
        }

        return json;
    }
}

/// <summary>A custom tool definition (<c>type: 'custom'</c>).</summary>
public sealed class OpenAIResponsesCustomToolDefinition : OpenAIResponsesAdditionalTool
{
    /// <summary>The tool description.</summary>
    public string? Description { get; set; }

    /// <summary>The input format. <c>null</c> leaves it to the provider.</summary>
    public OpenAIResponsesCustomToolFormat? Format { get; set; }

    internal override JsonNode ToJson()
    {
        var json = new JsonObject { ["type"] = "custom", ["name"] = Name };
        if (Description != null)
        {
            json["description"] = Description;
        }

        if (Format != null)
        {
            json["format"] = Format.ToJson();
        }

        return json;
    }
}

/// <summary>The input format of a custom tool: free text, or a <c>regex</c> / <c>lark</c> grammar.</summary>
public sealed class OpenAIResponsesCustomToolFormat
{
    /// <summary>Free-text input (<c>type: 'text'</c>).</summary>
    public static OpenAIResponsesCustomToolFormat Text() => new();

    /// <summary>A grammar (<c>type: 'grammar'</c>). <paramref name="syntax"/> is <c>regex</c> or <c>lark</c>.</summary>
    public static OpenAIResponsesCustomToolFormat Grammar(string syntax, string definition) =>
        new() { Syntax = syntax, Definition = definition };

    private string? Syntax { get; init; }

    private string? Definition { get; init; }

    internal JsonNode ToJson() =>
        Syntax == null
            ? new JsonObject { ["type"] = "text" }
            : new JsonObject
            {
                ["type"] = "grammar",
                ["syntax"] = Syntax,
                ["definition"] = Definition,
            };
}

/// <summary>A namespace of function tools (<c>type: 'namespace'</c>).</summary>
public sealed class OpenAIResponsesNamespaceToolDefinition : OpenAIResponsesAdditionalTool
{
    /// <summary>The namespace description.</summary>
    public string Description { get; set; } = "";

    /// <summary>The function tools of the namespace. At least one.</summary>
    public IReadOnlyList<OpenAIResponsesFunctionToolDefinition> Tools { get; set; } = [];

    internal override JsonNode ToJson()
    {
        var tools = new JsonArray();
        foreach (var tool in Tools)
        {
            tools.Add(tool.ToJson());
        }

        return new JsonObject
        {
            ["type"] = "namespace",
            ["name"] = Name,
            ["description"] = Description,
            ["tools"] = tools,
        };
    }
}
