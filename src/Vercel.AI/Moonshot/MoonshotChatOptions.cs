// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Moonshot;

/// <summary>
/// The message provider options of Moonshot AI. Maps to <c>moonshotaiAllMessageProviderOptions</c> in
/// <c>moonshotai-chat-options.ts</c>.
/// </summary>
internal sealed class MoonshotAIMessageProviderOptions
{
    /// <summary>The name of the participant represented by the message.</summary>
    public string? Name { get; init; }

    /// <summary>Whether the assistant message is a partial response that Moonshot continues.</summary>
    public bool? Partial { get; init; }

    /// <summary>Function tools to load at this point in a Kimi K3 conversation, or null when not set.</summary>
    public IReadOnlyList<LanguageModelTool>? Tools { get; init; }
}

/// <summary>Moonshot AI chat options. Maps to <c>moonshotai-chat-options.ts</c>.</summary>
internal static class MoonshotChatOptions
{
    /// <summary>
    /// Parses and validates <paramref name="providerOptions"/> under <paramref name="providerOptionsName"/> with the
    /// <c>moonshotaiAllMessageProviderOptions</c> schema. Maps to <c>parseProviderOptions</c>.
    /// </summary>
    /// <exception cref="InvalidArgumentException">The options do not match the schema.</exception>
    public static MoonshotAIMessageProviderOptions? ParseMessageProviderOptions(
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        string providerOptionsName
    )
    {
        if (
            providerOptions == null
            || !providerOptions.TryGetValue(providerOptionsName, out var bag)
            || bag.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
        )
        {
            return null;
        }

        var invalid = new InvalidArgumentException(
            "providerOptions",
            bag.GetRawText(),
            $"invalid {providerOptionsName} provider options"
        );
        if (bag.ValueKind != JsonValueKind.Object)
        {
            throw invalid;
        }

        string? name = null;
        bool? partial = null;
        List<LanguageModelTool>? tools = null;
        foreach (var property in bag.EnumerateObject())
        {
            switch (property.Name)
            {
                case "name":
                    name =
                        property.Value.ValueKind == JsonValueKind.String
                            ? property.Value.GetString()
                            : throw invalid;
                    break;
                case "partial":
                    partial = property.Value.ValueKind == JsonValueKind.True ? true : throw invalid;
                    break;
                case "tools":
                    tools = ParseTools(property.Value, invalid);
                    break;
            }
        }

        return new MoonshotAIMessageProviderOptions
        {
            Name = name,
            Partial = partial,
            Tools = tools,
        };
    }

    /// <summary>Maps to <c>moonshotaiDynamicToolSchema</c>.</summary>
    private static List<LanguageModelTool> ParseTools(
        JsonElement value,
        InvalidArgumentException invalid
    )
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw invalid;
        }

        var tools = new List<LanguageModelTool>();
        foreach (var tool in value.EnumerateArray())
        {
            if (
                tool.ValueKind != JsonValueKind.Object
                || !tool.TryGetProperty("type", out var type)
                || type.ValueKind != JsonValueKind.String
                || type.GetString() != "function"
                || !tool.TryGetProperty("name", out var name)
                || name.ValueKind != JsonValueKind.String
                || !tool.TryGetProperty("inputSchema", out var inputSchema)
                || inputSchema.ValueKind != JsonValueKind.Object
            )
            {
                throw invalid;
            }

            string? description = null;
            if (tool.TryGetProperty("description", out var descriptionElement))
            {
                description =
                    descriptionElement.ValueKind == JsonValueKind.String
                        ? descriptionElement.GetString()
                        : throw invalid;
            }

            bool? strict = null;
            if (tool.TryGetProperty("strict", out var strictElement))
            {
                strict = strictElement.ValueKind switch
                {
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => throw invalid,
                };
            }

            tools.Add(
                new LanguageModelTool(name.GetString()!, description, inputSchema.Clone(), strict)
            );
        }

        return tools;
    }
}
