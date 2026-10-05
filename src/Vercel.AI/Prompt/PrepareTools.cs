// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Prompt;

/// <summary>Context passed to a function-valued tool description.</summary>
public sealed class ToolDescriptionContext
{
    /// <summary>Creates context.</summary>
    public ToolDescriptionContext(object? context, string? sandboxDescription)
    {
        Context = context;
        SandboxDescription = sandboxDescription;
    }

    /// <summary>Per-tool context value.</summary>
    public object? Context { get; }

    /// <summary>Sandbox description, when a sandbox is active.</summary>
    public string? SandboxDescription { get; }
}

/// <summary>A tool passed to <see cref="PromptTools.PrepareTools"/>.</summary>
public sealed class PromptTool
{
    /// <summary>Creates a function, dynamic, or provider tool.</summary>
    public PromptTool(string? type = null)
    {
        Type = type;
    }

    /// <summary>Tool type. Null, <c>function</c>, and <c>dynamic</c> are function tools. <c>provider</c> is provider-defined.</summary>
    public string? Type { get; }

    /// <summary>String description.</summary>
    public string? Description { get; set; }

    /// <summary>Description resolved from context and the sandbox.</summary>
    public Func<ToolDescriptionContext, string>? DescriptionFactory { get; set; }

    /// <summary>JSON Schema for function tools.</summary>
    public JsonElement InputSchema { get; set; }

    /// <summary>Strict mode.</summary>
    public bool? Strict { get; set; }

    /// <summary>Provider options.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Input examples.</summary>
    public JsonElement? InputExamples { get; set; }

    /// <summary>Provider tool id.</summary>
    public string? Id { get; set; }

    /// <summary>Provider tool arguments.</summary>
    public JsonElement? Args { get; set; }
}

/// <summary>A tool after <see cref="PromptTools.PrepareTools"/>.</summary>
public sealed class PreparedTool
{
    /// <summary>Creates a prepared tool.</summary>
    public PreparedTool(string type, string name)
    {
        Type = type;
        Name = name;
    }

    /// <summary><c>function</c> or <c>provider</c>.</summary>
    public string Type { get; }

    /// <summary>Tool name.</summary>
    public string Name { get; }

    /// <summary>Resolved description.</summary>
    public string? Description { get; set; }

    /// <summary>Input schema for function tools.</summary>
    public JsonElement? InputSchema { get; set; }

    /// <summary>Strict mode.</summary>
    public bool? Strict { get; set; }

    /// <summary>Provider options.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Input examples.</summary>
    public JsonElement? InputExamples { get; set; }

    /// <summary>Provider tool id.</summary>
    public string? Id { get; set; }

    /// <summary>Provider tool arguments.</summary>
    public JsonElement? Args { get; set; }
}

/// <summary>Tool preparation. Maps to <c>prepareTools</c>.</summary>
public static class PromptTools
{
    /// <summary>
    /// Converts tools into the language-model list. Returns null when <paramref name="tools"/> is null or empty.
    /// <paramref name="toolOrder"/> names come first. The rest are sorted by name.
    /// </summary>
    public static IReadOnlyList<PreparedTool>? PrepareTools(
        IReadOnlyDictionary<string, PromptTool>? tools,
        IReadOnlyList<string>? toolOrder = null,
        IReadOnlyDictionary<string, object?>? toolsContext = null,
        string? sandboxDescription = null)
    {
        if (tools is null || tools.Count == 0)
        {
            return null;
        }

        var prepared = new List<PreparedTool>();
        foreach (var pair in Order(tools, toolOrder))
        {
            var tool = pair.Value;
            if (string.Equals(tool.Type, "provider", StringComparison.Ordinal))
            {
                prepared.Add(new PreparedTool("provider", pair.Key)
                {
                    Id = tool.Id,
                    Args = tool.Args,
                });
                continue;
            }

            if (tool.Type != null && !string.Equals(tool.Type, "function", StringComparison.Ordinal) && !string.Equals(tool.Type, "dynamic", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Unsupported tool type: " + tool.Type);
            }

            object? context = null;
            if (toolsContext != null)
            {
                toolsContext.TryGetValue(pair.Key, out context);
            }

            string? description = tool.Description;
            if (tool.DescriptionFactory != null)
            {
                description = tool.DescriptionFactory(new ToolDescriptionContext(context, sandboxDescription));
            }

            prepared.Add(new PreparedTool("function", pair.Key)
            {
                Description = description,
                InputSchema = tool.InputSchema,
                Strict = tool.Strict,
                ProviderOptions = tool.ProviderOptions,
                InputExamples = tool.InputExamples,
            });
        }

        return prepared;
    }

    private static List<KeyValuePair<string, PromptTool>> Order(IReadOnlyDictionary<string, PromptTool> tools, IReadOnlyList<string>? toolOrder)
    {
        var entries = new List<KeyValuePair<string, PromptTool>>();
        foreach (var pair in tools)
        {
            entries.Add(pair);
        }

        if (toolOrder is null)
        {
            return entries;
        }

        var ordered = new List<KeyValuePair<string, PromptTool>>();
        foreach (var pair in entries)
        {
            if (IndexOf(toolOrder, pair.Key) >= 0)
            {
                ordered.Add(pair);
            }
        }

        ordered.Sort((left, right) => IndexOf(toolOrder, left.Key).CompareTo(IndexOf(toolOrder, right.Key)));
        var rest = new List<KeyValuePair<string, PromptTool>>();
        foreach (var pair in entries)
        {
            if (IndexOf(toolOrder, pair.Key) < 0)
            {
                rest.Add(pair);
            }
        }

        rest.Sort((left, right) => string.CompareOrdinal(left.Key, right.Key));
        ordered.AddRange(rest);
        return ordered;
    }

    private static int IndexOf(IReadOnlyList<string> names, string name)
    {
        for (var i = 0; i < names.Count; i++)
        {
            if (string.Equals(names[i], name, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }
}
