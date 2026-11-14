// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Anthropic;

/// <summary>Configuration of one member of the computer toolset. Maps to a <c>configs</c> entry of <c>computerToolset_20260801</c>.</summary>
public sealed class AnthropicComputerToolsetMemberConfig
{
    /// <summary>Whether Claude can use this member tool. Default: true.</summary>
    public bool? Enabled { get; set; }

    /// <summary>Whether this member tool is only loaded once discovered through tool search. Default: false.</summary>
    public bool? DeferLoading { get; set; }
}

/// <summary>Factories of the Anthropic provider-defined tools. Maps to <c>anthropic.tools</c>.</summary>
public static class AnthropicTools
{
    /// <summary>
    /// Member tools of the computer toolset. Each member is returned by the API as its own <c>tool_use</c> block;
    /// the AI SDK exposes it as the <c>action</c> of a single computer tool call. Maps to <c>computerToolset_20260801Members</c>.
    /// </summary>
    public static IReadOnlyList<string> ComputerToolset20260801Members => AnthropicToolset.Members;

    /// <summary>
    /// Creates the computer toolset tool (<c>anthropic.computer_toolset_20260801</c>). Its local input schema drops
    /// the members that <paramref name="configs"/> disables from the <c>action</c> enum.
    /// Members you omit keep their defaults (all members enabled, including <c>zoom</c>). Maps to <c>computerToolset_20260801</c>.
    /// </summary>
    /// <param name="configs">Per-member configuration, keyed by a name of <see cref="ComputerToolset20260801Members"/>.</param>
    /// <param name="name">Tool name. Default: <c>computer</c>.</param>
    public static Tool ComputerToolset20260801(
        IReadOnlyDictionary<string, AnthropicComputerToolsetMemberConfig>? configs = null,
        string name = "computer")
    {
        var args = new JsonObject();
        var disabled = new HashSet<string>(StringComparer.Ordinal);
        if (configs != null)
        {
            var mapped = new JsonObject();
            foreach (var pair in configs)
            {
                if (Array.IndexOf(AnthropicToolset.Members, pair.Key) < 0)
                {
                    throw new ArgumentException("Unknown computer toolset member: " + pair.Key + ".", nameof(configs));
                }

                var config = new JsonObject();
                if (pair.Value?.Enabled is { } enabled)
                {
                    config["enabled"] = enabled;
                    if (!enabled)
                    {
                        disabled.Add(pair.Key);
                    }
                }

                if (pair.Value?.DeferLoading is { } defer)
                {
                    config["deferLoading"] = defer;
                }

                mapped[pair.Key] = config;
            }

            args["configs"] = mapped;
        }

        return Tool.ProviderDefined(
            "anthropic.computer_toolset_20260801",
            name,
            JsonSerializer.SerializeToElement(args),
            JsonSerializer.SerializeToElement(ComputerToolsetInputSchema(disabled)));
    }

    private static JsonObject ComputerToolsetInputSchema(HashSet<string> disabled)
    {
        var actions = new JsonArray();
        foreach (var member in AnthropicToolset.Members)
        {
            if (!disabled.Contains(member))
            {
                actions.Add(member);
            }
        }

        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["action"] = new JsonObject { ["type"] = "string", ["enum"] = actions },
                ["coordinate"] = Tuple(2),
                ["duration"] = new JsonObject { ["type"] = "number" },
                ["region"] = Tuple(4),
                ["repeat"] = new JsonObject { ["type"] = "integer" },
                ["scroll_amount"] = new JsonObject { ["type"] = "number" },
                ["scroll_direction"] = new JsonObject
                {
                    ["type"] = "string",
                    ["enum"] = new JsonArray("up", "down", "left", "right"),
                },
                ["start_coordinate"] = Tuple(2),
                ["text"] = new JsonObject { ["type"] = "string" },
            },
            ["required"] = new JsonArray("action"),
        };
    }

    private static JsonObject Tuple(int length)
    {
        return new JsonObject
        {
            ["type"] = "array",
            ["items"] = new JsonObject { ["type"] = "integer" },
            ["minItems"] = length,
            ["maxItems"] = length,
        };
    }
}
