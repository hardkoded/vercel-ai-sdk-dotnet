// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>A tool passed to a language model when building a name map.</summary>
public sealed class MappedTool
{
    /// <summary>Creates a tool description.</summary>
    public MappedTool(string type, string name, string? id = null)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Id = id;
    }

    /// <summary><c>provider</c> or <c>function</c>.</summary>
    public string Type { get; }

    /// <summary>Client tool name.</summary>
    public string Name { get; }

    /// <summary>Provider tool id, such as <c>anthropic.computer-use</c>.</summary>
    public string? Id { get; }
}

/// <summary>Maps client tool names to provider tool names. Maps to <c>ToolNameMapping</c>.</summary>
public sealed class ToolNameMapping
{
    private readonly Dictionary<string, string> _toProvider;
    private readonly Dictionary<string, string> _toCustom;

    internal ToolNameMapping(Dictionary<string, string> toProvider, Dictionary<string, string> toCustom)
    {
        _toProvider = toProvider;
        _toCustom = toCustom;
    }

    /// <summary>Maps a client name to the provider name, or returns the input when it is not mapped.</summary>
    public string ToProviderToolName(string customToolName)
    {
        string? mapped;
        return _toProvider.TryGetValue(customToolName, out mapped) ? mapped : customToolName;
    }

    /// <summary>Maps a provider name to the client name, or returns the input when it is not mapped.</summary>
    public string ToCustomToolName(string providerToolName)
    {
        string? mapped;
        return _toCustom.TryGetValue(providerToolName, out mapped) ? mapped : providerToolName;
    }
}

/// <summary>Builds tool name maps. Maps to <c>createToolNameMapping</c>.</summary>
public static class ToolNames
{
    /// <summary>Creates a mapping from provider-defined tools whose ids are listed in <paramref name="providerToolNames"/>.</summary>
    public static ToolNameMapping CreateToolNameMapping(
        IReadOnlyList<MappedTool>? tools,
        IReadOnlyDictionary<string, string> providerToolNames)
    {
        if (providerToolNames is null)
        {
            throw new ArgumentNullException(nameof(providerToolNames));
        }

        var toProvider = new Dictionary<string, string>();
        var toCustom = new Dictionary<string, string>();
        if (tools != null)
        {
            foreach (var tool in tools)
            {
                if (tool.Type == "provider" && tool.Id != null && providerToolNames.ContainsKey(tool.Id))
                {
                    var providerName = providerToolNames[tool.Id];
                    toProvider[tool.Name] = providerName;
                    toCustom[providerName] = tool.Name;
                }
            }
        }

        return new ToolNameMapping(toProvider, toCustom);
    }
}
